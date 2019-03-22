using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UtilGS9
{
    public class AABBCulling
    {

        public struct UnOrderedEdgeKey : IComparable<UnOrderedEdgeKey>
        {
            public int _V0, _V1;

            //4byte 해쉬키가 중복되지 않게 할려면 2byte 값까지만 넣어야 한다.
            public UnOrderedEdgeKey(int v0, int v1)
            {
                if (v0 < v1)
                {
                    _V0 = v0;
                    _V1 = v1;
                }
                else
                {
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

            static public int ToEdgeKey1D(UnOrderedEdgeKey edge2d)
            {
                return (edge2d._V0 << 16) + edge2d._V1;
            }

            static public UnOrderedEdgeKey ToEdgeKey2D(int edge1d)
            {
                UnOrderedEdgeKey edge2d = new UnOrderedEdgeKey();
                edge2d._V0 = edge1d >> 16;
                edge2d._V1 = edge1d - edge2d._V0;

                return edge2d;
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
                type = NONE; value = 0f; index = NONE;
            }

            public Endpoint(int ty, float va, int idx)
            {
                Set(ty, va, idx);
            }

            public void Set(int ty, float va, int idx)
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
                //return x.GetHashCode();
                //ref : https://code.i-harness.com/ko-kr/q/dff7e9
                //v0 과 v1 이 2byte 크기만 사용해야지 고유해시 범위에 들 수 있다. 즉 65535 이상의 인덱스값을 넣으면 안된다는 것임 
                return (x._V0 << 16) + x._V1; //0 ~ 2 ^ 16-1 (0 ~ 65535) 사이의 키에 대한 고유 해시를 반환
                //return UnOrderedEdgeKey.ToEdgeKey1D(x);
            }
        }

        private List<HordeFight.Being> mRectangles = null;
        private HashSet<UnOrderedEdgeKey> mOverlap = null;

        private List<Endpoint> mXEndpoints = null, mYEndpoints = null, mZEndpoints = null;
        private List<int> mXLookup = null, mYLookup = null, mZLookup = null;


        public AABBCulling()
        {
            //Initialize();
        }


        public void Initialize(List<HordeFight.Being> rectangles)
        {
            mRectangles = rectangles;

            // Get the rectangle endpoints.
            int intrSize = mRectangles.Count;
            int endpSize = (2 * intrSize);

            mXEndpoints = new List<Endpoint>(endpSize);
            mYEndpoints = new List<Endpoint>(endpSize);
            mZEndpoints = new List<Endpoint>(endpSize);
            mXLookup = new List<int>(endpSize);
            mYLookup = new List<int>(endpSize);
            mZLookup = new List<int>(endpSize);

            for (int i = 0; i < endpSize; i++)
            {
                mXEndpoints.Add(new Endpoint());
                mYEndpoints.Add(new Endpoint());
                mZEndpoints.Add(new Endpoint());
                mXLookup.Add(-1);
                mYLookup.Add(-1);
                mZLookup.Add(-1);
            }
            DebugWide.LogBlue("init  AABBCulling  endpoint size : " + endpSize);



            for (int i = 0, j = 0; i < intrSize; ++i)
            {
                //DebugWide.LogBlue(j); //chamto test

                //Bounds bb = mRectangles[i].GetBounds();
                HordeFight.Being bb = mRectangles[i];

                mXEndpoints[j].Set(Endpoint.BEGIN, bb._getBounds_min.x, i);
                mYEndpoints[j].Set(Endpoint.BEGIN, bb._getBounds_min.y, i);
                mZEndpoints[j].Set(Endpoint.BEGIN, bb._getBounds_min.z, i);
                ++j;


                mXEndpoints[j].Set(Endpoint.END, bb._getBounds_max.x, i);
                mYEndpoints[j].Set(Endpoint.END, bb._getBounds_max.y, i);
                mZEndpoints[j].Set(Endpoint.END, bb._getBounds_max.z, i);
                ++j;

            }

            //foreach(Endpoint x in mXEndpoints)
            //{
            //    DebugWide.LogBlue("before:  " + x);
            //}

            // Sort the rectangle endpoints.
            mXEndpoints.Sort();
            mYEndpoints.Sort();
            mZEndpoints.Sort();

            //foreach (Endpoint x in mXEndpoints)
            //{
            //    DebugWide.LogRed("after:  " + x);
            //}

            // Create the interval-to-endpoint lookup tables.

            for (int j = 0; j < endpSize; ++j)
            {
                mXLookup[2 * mXEndpoints[j].index + mXEndpoints[j].type] = j;
                mYLookup[2 * mYEndpoints[j].index + mYEndpoints[j].type] = j;
                mZLookup[2 * mZEndpoints[j].index + mZEndpoints[j].type] = j;
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
            mOverlap = new HashSet<UnOrderedEdgeKey>(new EdgeKeyComparer());


            // Sweep through the endpoints to determine overlapping x-intervals.
            for (int i = 0; i < endpSize; ++i)
            {
                Endpoint endpoint = mXEndpoints[i];
                int index = endpoint.index;
                if (endpoint.type == Endpoint.BEGIN)  // an interval 'begin' value
                {

                    foreach (int activeIndex in active)
                    {

                        //Bounds b0 = mRectangles[activeIndex].GetBounds();
                        //Bounds b1 = mRectangles[index].GetBounds();
                        //if (b0.Intersects(b1))
                        //if (b0.max.y >= b1.min.y && b0.min.y <= b1.max.y
                            //&& b0.max.z >= b1.min.z && b0.min.z <= b1.max.z)
                        if(mRectangles[activeIndex].Intersects(mRectangles[index]))
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

        //public void UpdateEndPoints()
        //{

        //    for (int i = 0, j = 0; i < mRectangles.Count; ++i)
        //    {

        //        Bounds bb = mRectangles[i].GetBounds();


        //        mXEndpoints[mXLookup[2 * i]].value = bb.min.x;
        //        mXEndpoints[mXLookup[2 * i + 1]].value = bb.max.x;
        //        mYEndpoints[mYLookup[2 * i]].value = bb.min.y;
        //        mYEndpoints[mYLookup[2 * i + 1]].value = bb.max.y;
        //        mZEndpoints[mZLookup[2 * i]].value = bb.min.z;
        //        mZEndpoints[mZLookup[2 * i + 1]].value = bb.max.z;

        //    }

        //}


        public void SetEndPoint(int i, HordeFight.Being box)
        {

            //DebugWide.LogBlue("a: " + mXEndpoints[mXLookup[2 * i]] + "  =>min: " + rectangle.min.x);

            mXEndpoints[mXLookup[2 * i]].value = box._getBounds_min.x;
            mXEndpoints[mXLookup[2 * i + 1]].value = box._getBounds_max.x;
            mYEndpoints[mYLookup[2 * i]].value = box._getBounds_min.y;
            mYEndpoints[mYLookup[2 * i + 1]].value = box._getBounds_max.y;
            mZEndpoints[mZLookup[2 * i]].value = box._getBounds_min.z;
            mZEndpoints[mZLookup[2 * i + 1]].value = box._getBounds_max.z;

            //DebugWide.LogGreen("b: "+mXEndpoints[mXLookup[2 * i]]);
        }

        //public void SetEndPoint(int i, Bounds box)
        //{

        //    //DebugWide.LogBlue("a: " + mXEndpoints[mXLookup[2 * i]] + "  =>min: " + rectangle.min.x);

        //    mXEndpoints[mXLookup[2 * i]].value = box.min.x;
        //    mXEndpoints[mXLookup[2 * i + 1]].value = box.max.x;
        //    mYEndpoints[mYLookup[2 * i]].value = box.min.y;
        //    mYEndpoints[mYLookup[2 * i + 1]].value = box.max.y;
        //    mZEndpoints[mZLookup[2 * i]].value = box.min.z;
        //    mZEndpoints[mZLookup[2 * i + 1]].value = box.max.z;

        //    //DebugWide.LogGreen("b: "+mXEndpoints[mXLookup[2 * i]]);
        //}

        //public Bounds GetRectangle(int i)
        //{
        //    return mRectangles[i];
        //}


        public void UpdateXZ()
        {
            if (null == mRectangles) return;

            InsertionSort(mXEndpoints, mXLookup);
            InsertionSort(mZEndpoints, mZLookup);
        }

        public void UpdateXYZ()
        {
            if (null == mRectangles) return;

            //UpdateEndPoints();

            InsertionSort(mXEndpoints, mXLookup);
            InsertionSort(mYEndpoints, mYLookup);
            InsertionSort(mZEndpoints, mZLookup);
        }


        public HashSet<UnOrderedEdgeKey> GetOverlap()
        {
            return mOverlap;
        }


        private void InsertionSort(List<Endpoint> endpoint, List<int> lookup)
        {

            int endpSize = endpoint.Count;
            for (int j = 1; j < endpSize; ++j)
            {
                Endpoint key = endpoint[j];
                int i = j - 1;
                //while (i >= 0 && key < endpoint[i])
                //while (i >= 0 && key.CompareTo(endpoint[i]) < 0)
                while (i >= 0 && key.value < endpoint[i].value)
                {
                    Endpoint e0 = endpoint[i];
                    Endpoint e1 = endpoint[i + 1];

                    // Update the overlap status.
                    if (e0.type == Endpoint.BEGIN)
                    {
                        if (e1.type == Endpoint.END)
                        {
                            mOverlap.Remove(new UnOrderedEdgeKey(e0.index, e1.index));
                        }
                    }
                    else
                    {
                        if (e1.type == Endpoint.BEGIN)
                        {
                            
                            //if (mRectangles[e0.index].GetBounds().Intersects(mRectangles[e1.index].GetBounds()))
                            if (mRectangles[e0.index].Intersects(mRectangles[e1.index]))
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
}


