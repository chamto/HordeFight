using System.Collections.Generic;
using UnityEngine;
using UtilGS9;
using Buckland;

namespace SteeringBehavior
{
    public class Path
    {

        LinkedList<Vector2> m_WayPoints = new LinkedList<Vector2>();

        //points to the current waypoint
        LinkedListNode<Vector2> curWaypoint;

        //flag to indicate if the path should be looped
        //(The last waypoint connected to the first)
        bool m_bLooped;


        public Path() { m_bLooped = false; }

        //constructor for creating a path with initial random waypoints. MinX/Y
        //& MaxX/Y define the bounding box of the path.
        public Path(int NumWaypoints,
                    float MinX,
                    float MinY,
                    float MaxX,
                    float MaxY,
                    bool looped)
        {
            m_bLooped = looped;
            CreateRandomPath(NumWaypoints, MinX, MinY, MaxX, MaxY);

            curWaypoint = m_WayPoints.First;
        }


        //returns the current waypoint
        public Vector2 CurrentWaypoint()
        {
            //assert(curWaypoint != NULL); 
            return curWaypoint.Value;
        }

        //returns true if the end of the list has been reached
        public bool Finished()
        {
            //return !(curWaypoint != m_WayPoints.Last); 
            if (null == curWaypoint) return true;

            return (curWaypoint == m_WayPoints.Last);
        }

        //moves the iterator on to the next waypoint in the list
        public void SetNextWaypoint()
        {
            //assert(m_WayPoints.size() > 0);

            curWaypoint = curWaypoint.Next;
            if (curWaypoint == m_WayPoints.Last)
            {
                if (m_bLooped)
                {
                    curWaypoint = m_WayPoints.First;
                }
            }

        }

        //creates a random path which is bound by rectangle described by
        //the min/max values
        public LinkedList<Vector2> CreateRandomPath(int NumWaypoints,
                                         float MinX,
                                         float MinY,
                                         float MaxX,
                                         float MaxY)
        {
            m_WayPoints.Clear();

            float midX = (MaxX + MinX) / 2.0f;
            float midY = (MaxY + MinY) / 2.0f;

            float smaller = Util.MinOf(midX, midY);

            float spacing = Const.TwoPi / (float)NumWaypoints;

            for (int i = 0; i < NumWaypoints; ++i)
            {

                float RadialDist = Misc.RandFloat(smaller * 0.2f, smaller);

                Vector2 temp = new Vector2(RadialDist, 0.0f);

                Transformations.Vec2DRotateAroundOrigin(ref temp, i * spacing);

                temp.x += midX; temp.y += midY;

                m_WayPoints.AddLast(temp);

            }

            curWaypoint = m_WayPoints.First;

            return m_WayPoints;
        }


        public void LoopOn() { m_bLooped = true; }
        public void LoopOff() { m_bLooped = false; }

        //adds a waypoint to the end of the path
        //public void AddWayPoint(Vector2 new_point);

        //methods for setting the path with either another Path or a list of vectors
        public void Set(LinkedList<Vector2> new_path) { m_WayPoints = new_path; curWaypoint = m_WayPoints.First; }
        public void Set(Path path) { m_WayPoints = path.GetPath(); curWaypoint = m_WayPoints.First; }


        public void Clear() { m_WayPoints.Clear(); }

        public LinkedList<Vector2> GetPath() { return m_WayPoints; }

        //renders the path in orange
        public void Render()
        {

            LinkedListNode<Vector2> it = m_WayPoints.First;
            LinkedListNode<Vector2> wp = it;
            it = it.Next;
            while (it != m_WayPoints.Last) //마지막 노드는 못그리는 문제가 있을 것으로 예상 
            {
                DebugWide.DrawLine(wp.Value, it.Value, Color.magenta);
                wp = it;
                it = it.Next;
            }


            if (m_bLooped) DebugWide.DrawLine(m_WayPoints.Last.Value, m_WayPoints.First.Value, Color.magenta);
        }
    }
}

