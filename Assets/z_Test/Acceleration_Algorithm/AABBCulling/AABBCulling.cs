// David Eberly, Geometric Tools, Redmond WA 98052
// Copyright (c) 1998-2018
// Distributed under the Boost Software License, Version 1.0.
// http://www.boost.org/LICENSE_1_0.txt
// http://www.geometrictools.com/License/Boost/LICENSE_1_0.txt
// File Version: 3.0.0 (2016/06/19)

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AABBCulling
{

    public struct UnOrderedEdgeKey : IComparable<UnOrderedEdgeKey>
    {
        public int _V0, _V1;

        public UnOrderedEdgeKey(int v0, int v1)
        {
            if (v0 < v1)
            {
                // v0 is minimum
                _V0 = v0;
                _V1 = v1;
            }
            else
            {
                // v1 is minimum
                _V0 = v1;
                _V1 = v0;
            }
        }

        public void Init()
        {
            _V0 = _V1 = -1;
        }

        public int CompareTo(UnOrderedEdgeKey part)
        {

            if (_V1 < part._V1)
            {
                return -1; //this < part
            }

            if (_V1 > part._V1)
            {
                return 1; //part < this
            }

            if (_V0 < part._V0)
            {
                return -1; //this < part
            }

            if (_V0 > part._V0)
            {
                return 1; //part < this
            }

            //if (_V0 == part._V0 && _V1 == part._V1)
            return 0; //part == this
        }

    }

    public class Endpoint : IComparable<Endpoint>
    {
        public const int NONE = -1;
        public const int BEGIN = 0;
        public const int END = 1;

        public float value; // endpoint value
        public int type;   // '0' if interval min, '1' if interval max.
        public int index;  // index of interval containing this endpoint

        public Endpoint()
        {
            type = NONE; value = 0f; index = -1;
        }

        public Endpoint(int ty , float va , int idx)
        {
            Init(ty, va, idx);
        }

        public void Init(int ty, float va, int idx)
        {
            type = ty; value = va; index = idx;
        }


        public int CompareTo(Endpoint part)
        {
            return value.CompareTo(part.value);
        }

		public override string ToString()
		{
            return value + "   type: " + type + "   idx: " + index;
		}

	}

    public class EdgeKeyComparer : IEqualityComparer<UnOrderedEdgeKey>
    {
        public bool Equals(UnOrderedEdgeKey x, UnOrderedEdgeKey y)
        {
            return x._V0 == y._V0 && x._V1 == y._V1;
        }
        public int GetHashCode(UnOrderedEdgeKey x)
        {
            return x.GetHashCode();
        }
    }

    private List<Bounds> mRectangles = null;
    private List<Endpoint> mXEndpoints = null, mYEndpoints = null;
    private HashSet<UnOrderedEdgeKey> mOverlap = null;

    // The intervals are indexed 0 <= i < n.  The endpoint array has 2*n
    // entries.  The original 2*n interval values are ordered as b[0], e[0],
    // b[1], e[1], ..., b[n-1], e[n-1].  When the endpoint array is sorted,
    // the mapping between interval values and endpoints is lost.  In order
    // to modify interval values that are stored in the endpoint array, we
    // need to maintain the mapping.  This is done by the following lookup
    // table of 2*n entries.  The value mLookup[2*i] is the index of b[i]
    // in the endpoint array.  The value mLookup[2*i+1] is the index of
    // e[i] in the endpoint array.
    private List<int> mXLookup = null, mYLookup = null;


    // Construction.
    public AABBCulling(List<Bounds> rectangles)
    {
        mRectangles = rectangles;
        Initialize();
    }

    // This function is called by the constructor and does the sort-and-sweep
    // to initialize the update system.  However, if you add or remove items
    // from the array of rectangles after the constructor call, you will need
    // to call this function once before you start the multiple calls of the
    // update function.
    public void Initialize()
    {
        // Get the rectangle endpoints.
        int intrSize = mRectangles.Count;
        int endpSize = (2 * intrSize);
        //mXEndpoints.resize(endpSize);
        //mYEndpoints.resize(endpSize);
        mXEndpoints = new List<Endpoint>(endpSize);
        mYEndpoints = new List<Endpoint>(endpSize);
        mXLookup = new List<int>(endpSize);
        mYLookup = new List<int>(endpSize);

        for (int i = 0; i < endpSize;i++)
        {
            mXEndpoints.Add(new Endpoint());
            mYEndpoints.Add(new Endpoint());
            mXLookup.Add(-1);
            mYLookup.Add(-1);
        }
        DebugWide.LogBlue("  x_endpoint count : "+mXEndpoints.Count);



        for (int i = 0, j = 0; i < intrSize; ++i)
        {
            //DebugWide.LogBlue(j); //chamto test

            //mXEndpoints[j] = new Endpoint(Endpoint.BEGIN, mRectangles[i].min.x, i);
            //mYEndpoints[j] = new Endpoint(Endpoint.BEGIN, mRectangles[i].min.y, i);
            mXEndpoints[j].Init(Endpoint.BEGIN, mRectangles[i].min.x, i);
            mYEndpoints[j].Init(Endpoint.BEGIN, mRectangles[i].min.y, i);
            ++j;

            //mXEndpoints[j] = new Endpoint(Endpoint.END, mRectangles[i].max.x, i);
            //mYEndpoints[j] = new Endpoint(Endpoint.END, mRectangles[i].max.y, i);
            mXEndpoints[j].Init(Endpoint.END, mRectangles[i].max.x, i);
            mYEndpoints[j].Init(Endpoint.END, mRectangles[i].max.y, i);
            ++j;

        }

        //foreach(Endpoint x in mXEndpoints)
        //{
        //    DebugWide.LogBlue("before:  " + x);
        //}

        // Sort the rectangle endpoints.
        //std::sort(mXEndpoints.begin(), mXEndpoints.end());
        //std::sort(mYEndpoints.begin(), mYEndpoints.end());
        mXEndpoints.Sort();
        mYEndpoints.Sort();


        //foreach (Endpoint x in mXEndpoints)
        //{
        //    DebugWide.LogRed("after:  " + x);
        //}

        // Create the interval-to-endpoint lookup tables.
        //mXLookup.resize(endpSize);
        //mYLookup.resize(endpSize);


        for (int j = 0; j < endpSize; ++j)
        {
            mXLookup[2 * mXEndpoints[j].index + mXEndpoints[j].type] = j;
            mYLookup[2 * mYEndpoints[j].index + mYEndpoints[j].type] = j;
        }

        //foreach(int lk in mXLookup)
        //{
        //    DebugWide.LogGreen("  x_lookUp: " + lk);
        //}
        //foreach (int lk in mYLookup)
        //{
        //    DebugWide.LogGreen("  y_lookUp: " + lk);
        //}

        // Active set of rectangles (stored by index in array).
        HashSet<int> active = new HashSet<int>();

        // Set of overlapping rectangles (stored by pairs of indices in array).
        //mOverlap.clear();
        mOverlap = new HashSet<UnOrderedEdgeKey>(new EdgeKeyComparer());

        // Sweep through the endpoints to determine overlapping x-intervals.
        for (int i = 0; i < endpSize; ++i)
        {
            Endpoint endpoint = mXEndpoints[i];
            int index = endpoint.index;
            if (endpoint.type == Endpoint.BEGIN)  // an interval 'begin' value
            {
                // In the 1D problem, the current interval overlaps with all the
                // active intervals.  In 2D we also need to check for y-overlap.
                foreach (int activeIndex in active)
                {
                    // Rectangles activeIndex and index overlap in the
                    // x-dimension.  Test for overlap in the y-dimension.
                    Bounds r0 = mRectangles[activeIndex];
                    Bounds r1 = mRectangles[index];
                    if (r0.max.y >= r1.min.y && r0.min.y <= r1.max.y)
                    {
                        if (activeIndex < index)
                        {
                            mOverlap.Add(new UnOrderedEdgeKey(activeIndex, index));
                        }
                        else
                        {
                            mOverlap.Add(new UnOrderedEdgeKey(index, activeIndex));
                        }
                    }
                }
                active.Add(index);
            }
            else  // an interval 'end' value
            {
                active.Remove(index);
            }
        }
    }

    // After the system is initialized, you can move the rectangles using this
    // function.  It is not enough to modify the input array of rectangles
    // since the endpoint values stored internally by this class must also
    // change.  You can also retrieve the current rectangles information.
    public void SetRectangle(int i, Bounds rectangle)
    {
        mRectangles[i] = rectangle;


        //DebugWide.LogBlue("a: " + mXEndpoints[mXLookup[2 * i]] + "  =>min: " + rectangle.min.x);

        mXEndpoints[mXLookup[2 * i]].value = rectangle.min.x;
        mXEndpoints[mXLookup[2 * i + 1]].value = rectangle.max.x;
        mYEndpoints[mYLookup[2 * i]].value = rectangle.min.y;
        mYEndpoints[mYLookup[2 * i + 1]].value = rectangle.max.y;

        //DebugWide.LogGreen("b: "+mXEndpoints[mXLookup[2 * i]]);
    }

    public Bounds GetRectangle(int i)
    {
        return mRectangles[i];
    }

    // When you are finished moving rectangles, call this function to
    // determine the overlapping rectangles.  An incremental update is applied
    // to determine the new set of overlapping rectangles.
    public void Update()
    {
        InsertionSort(mXEndpoints, mXLookup);
        InsertionSort(mYEndpoints, mYLookup);
    }

    // If (i,j) is in the overlap set, then rectangle i and rectangle j are
    // overlapping.  The indices are those for the the input array.  The
    // set elements (i,j) are stored so that i < j.
    public HashSet<UnOrderedEdgeKey> GetOverlap()
    {
        return mOverlap;
    }


    private void InsertionSort(List<Endpoint> endpoint, List<int> lookup)
    {
        // Apply an insertion sort.  Under the assumption that the rectangles
        // have not changed much since the last call, the endpoints are nearly
        // sorted.  The insertion sort should be very fast in this case.

        int endpSize = endpoint.Count;
        for (int j = 1; j < endpSize; ++j)
        {
            Endpoint key = endpoint[j];
            int i = j - 1;
            //while (i >= 0 && key < endpoint[i])
            while (i >= 0 && key.CompareTo(endpoint[i]) < 0)
            {
                Endpoint e0 = endpoint[i];
                Endpoint e1 = endpoint[i + 1];

                // Update the overlap status.
                if (e0.type == Endpoint.BEGIN)
                {
                    if (e1.type == Endpoint.END)
                    {
                        // The 'b' of interval E0.index was smaller than the 'e'
                        // of interval E1.index, and the intervals *might have
                        // been* overlapping.  Now 'b' and 'e' are swapped, and
                        // the intervals cannot overlap.  Remove the pair from
                        // the overlap set.  The removal operation needs to find
                        // the pair and erase it if it exists.  Finding the pair
                        // is the expensive part of the operation, so there is no
                        // real time savings in testing for existence first, then
                        // deleting if it does.
                        mOverlap.Remove(new UnOrderedEdgeKey(e0.index, e1.index));
                    }
                }
                else
                {
                    if (e1.type == Endpoint.BEGIN)
                    {
                        // The 'b' of interval E1.index was larger than the 'e'
                        // of interval E0.index, and the intervals were not
                        // overlapping.  Now 'b' and 'e' are swapped, and the
                        // intervals *might be* overlapping.  Determine if they
                        // are overlapping and then insert.
                        if(mRectangles[e0.index].Intersects(mRectangles[e1.index]))
                        {
                            mOverlap.Add(new UnOrderedEdgeKey(e0.index, e1.index));
                        }
                    }
                }

                // Reorder the items to maintain the sorted list.
                endpoint[i] = e1;
                endpoint[i + 1] = e0;
                lookup[2 * e1.index + e1.type] = i;
                lookup[2 * e0.index + e0.type] = i + 1;
                --i;
            }
            endpoint[i + 1] = key;
            lookup[2 * key.index + key.type] = i + 1;
        }
    }

}

