using System;
using System.Collections.Generic;
using UnityEngine;

namespace Buckland
{
    public struct InvertedAABBox2D
    {
  
        public Vector2 m_vTopLeft;
        public Vector2 m_vBottomRight;
        public Vector2 m_vCenter;


        public InvertedAABBox2D(Vector2 tl, Vector2 br)
        {
            m_vTopLeft = tl;
            m_vBottomRight = br;
            m_vCenter = (tl + br) / 2.0f;
        }

        //returns true if the bbox described by other intersects with this one
        public bool isOverlappedWith(InvertedAABBox2D other)
        {
            return !((other.Top() > this.Bottom()) ||
                   (other.Bottom() < this.Top()) ||
                 (other.Left() > this.Right()) ||
                (other.Right() < this.Left()));
        }


        public Vector2 TopLeft() {return m_vTopLeft;}
        public Vector2 BottomRight() {return m_vBottomRight;}

        public float Top() {return m_vTopLeft.y;}
        public float Left() {return m_vTopLeft.x;}
        public float Bottom() {return m_vBottomRight.y;}
        public float Right() {return m_vBottomRight.x;}
        public Vector2 Center() {return m_vCenter;}

        public void Render(bool RenderCenter = false)
        {
            float size_x = Mathf.Abs(m_vTopLeft.x - m_vBottomRight.x);
            float size_y = Mathf.Abs(m_vTopLeft.y - m_vBottomRight.y);
            DebugWide.DrawCube(m_vCenter, new Vector2(size_x, size_y), Color.white);

            if (RenderCenter)
            {
                DebugWide.DrawCircle(m_vCenter, 5, Color.white);
            }
        }

    }



    public struct Cell<T> where T : BaseGameEntity
    {
        //all the entities inhabiting this cell
        public LinkedList<T> Members;

        //the cell's bounding box (it's inverted because the Window's default
        //co-ordinate system has a y axis that increases as it descends)
        public InvertedAABBox2D BBox;

        public Cell(Vector2 topleft, Vector2 botright)
        {
            Members = new LinkedList<T>();
            BBox = new InvertedAABBox2D(topleft, botright);
        }
    }

    public class CellSpacePartition<T> where T : BaseGameEntity
    { 
        //the required amount of cells in the space
        List<Cell<T>> m_Cells;

        //this is used to store any valid neighbors when an agent searches
        //its neighboring space
        List<T> m_Neighbors;

        //this iterator will be used by the methods next and begin to traverse
        //through the above vector of neighbors
        int   m_curNeighbor;

        //the width and height of the world space the entities inhabit
        float m_dSpaceWidth;
        float m_dSpaceHeight;

        //the number of cells the space is going to be divided up into
        int m_iNumCellsX;
        int m_iNumCellsY;

        float m_dCellSizeX;
        float m_dCellSizeY;


        //given a position in the game space this method determines the           
        //relevant cell's index
        public int PositionToIndex( Vector2 pos) 
        {
            int idx = (int)(m_iNumCellsX * pos.x / m_dSpaceWidth) +
            ((int)((m_iNumCellsY) * pos.y / m_dSpaceHeight) * m_iNumCellsX);

            //if the entity's position is equal to vector2d(m_dSpaceWidth, m_dSpaceHeight)
            //then the index will overshoot. We need to check for this and adjust
            if (idx > (int)m_Cells.Count - 1) idx = (int)m_Cells.Count - 1;

            return idx;
        }


        public CellSpacePartition(float width,        //width of the environment
                         float height,       //height ...
                         int cellsX,       //number of cells horizontally
                         int cellsY,       //number of cells vertically
                         int MaxEntitys)  //maximum number of entities to add
        {
            m_dSpaceWidth = width;
            m_dSpaceHeight = height;
            m_iNumCellsX = cellsX;
            m_iNumCellsY = cellsY;
            m_Neighbors = new List<T>();
            m_Cells = new List<Cell<T>>();

            //calculate bounds of each cell
            m_dCellSizeX = width / cellsX;
            m_dCellSizeY = height / cellsY;


            //create the cells

            Cell<T> cell;
            for (int y = 0; y < m_iNumCellsY; ++y)
            {
                for (int x = 0; x < m_iNumCellsX; ++x)
                {
                    float left = x * m_dCellSizeX;
                    float right = left + m_dCellSizeX;
                    float top = y * m_dCellSizeY;
                    float bot = top + m_dCellSizeY;

                    cell = new Cell<T>(new Vector2(left, top) , new Vector2(right, bot));
                    m_Cells.Add(cell);
                }
            }
        }

        //adds entities to the class by allocating them to the appropriate cell
        public void AddEntity(T ent)
        {
            int sz = m_Cells.Count;
            int idx = PositionToIndex(ent.Pos());

            m_Cells[idx].Members.AddLast(ent);
        }

        //update an entity's cell by calling this from your entity's Update method 
        public void UpdateEntity(T ent, Vector2 OldPos)
        {
            //if the index for the old pos and the new pos are not equal then
            //the entity has moved to another cell.
            int OldIdx = PositionToIndex(OldPos);
            int NewIdx = PositionToIndex(ent.Pos());

            if (NewIdx == OldIdx) return;

            //the entity has moved into another cell so delete from current cell
            //and add to new one
            m_Cells[OldIdx].Members.Remove(ent);
            m_Cells[NewIdx].Members.AddLast(ent);
        }

        //this method calculates all a target's neighbors and stores them in
        //the neighbor vector. After you have called this method use the begin, 
        //next and end methods to iterate through the vector.
        public void CalculateNeighbors(Vector2 TargetPos, float QueryRadius)
        {
            
            //create the query box that is the bounding box of the target's query
            //area
            InvertedAABBox2D QueryBox = new InvertedAABBox2D(TargetPos - new Vector2(QueryRadius, QueryRadius),
                            TargetPos + new Vector2(QueryRadius, QueryRadius));

            //iterate through each cell and test to see if its bounding box overlaps
            //with the query box. If it does and it also contains entities then
            //make further proximity tests.
            foreach (Cell<T> curCell in m_Cells)
            {
                //test to see if this cell contains members and if it overlaps the
                //query box
                if (curCell.BBox.isOverlappedWith(QueryBox) &&
                    0 != curCell.Members.Count)
                {
                    //add any entities found within query radius to the neighbor list
                    foreach (T ent in curCell.Members)
                    {
                        if (Util.Vec2DDistanceSq(ent.Pos(), TargetPos) < QueryRadius * QueryRadius)
                        {
                            m_Neighbors.Add(ent);
                        }    
                    }

                }
            }//next cell

            //mark the end of the list with a zero.
            //*curNbor = 0;
        }

        public List<T> GetNeighbors() { return m_Neighbors; }
        //returns a reference to the entity at the front of the neighbor vector
        //public T begin() { m_curNeighbor = 0; return m_Neighbors[m_curNeighbor]; }

        //this returns the next entity in the neighbor vector
        //public T next() { ++m_curNeighbor; return m_Neighbors[m_curNeighbor]; }

        //returns true if the end of the vector is found (a zero value marks the end)
        //public bool end() //{ return (m_curNeighbor == m_Neighbors.end()) || (*m_curNeighbor == 0); }
        //{
        //    return m_curNeighbor >= m_Neighbors.Count - 1;
        //}

        //empties the cells of entities
        public void EmptyCells()
        {
            foreach(Cell<T> cell in m_Cells)
            {
                cell.Members.Clear();
            }
        }

        //call this to use the gdi to render the cell edges
        public void RenderCells()
        {
            foreach (Cell<T> cell in m_Cells)
            {
                cell.BBox.Render(false);
            }

        }
    }
}

