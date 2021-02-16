using System;
using System.Collections.Generic;
using UnityEngine;

namespace Raven
{

    public struct Cell<T> //where T : BaseGameEntity
    {
        //all the entities inhabiting this cell
        public LinkedList<T> Members;

        //the cell's bounding box (it's inverted because the Window's default
        //co-ordinate system has a y axis that increases as it descends)
        public InvertedAABBox2D BBox;

        public Cell(Vector3 topleft, Vector3 botright)
        {
            Members = new LinkedList<T>();
            BBox = new InvertedAABBox2D(topleft, botright);
        }
    }

    public class CellSpacePartition<T> where T : Base_Pos
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
        int m_iNumCellsZ;

        float m_dCellSizeX;
        float m_dCellSizeZ;


        //given a position in the game space this method determines the           
        //relevant cell's index
        public int PositionToIndex( Vector3 pos) 
        {
            int idx = (int)(m_iNumCellsX * pos.x / m_dSpaceWidth) +
            ((int)((m_iNumCellsZ) * pos.z / m_dSpaceHeight) * m_iNumCellsX);

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
            m_iNumCellsZ = cellsY;
            m_Neighbors = new List<T>();
            m_Cells = new List<Cell<T>>();

            //calculate bounds of each cell
            m_dCellSizeX = width / cellsX;
            m_dCellSizeZ = height / cellsY;


            //create the cells

            Cell<T> cell;
            for (int z = 0; z < m_iNumCellsZ; ++z)
            {
                for (int x = 0; x < m_iNumCellsX; ++x)
                {
                    float left = x * m_dCellSizeX;
                    float right = left + m_dCellSizeX;
                    float top = z * m_dCellSizeZ;
                    float bot = top + m_dCellSizeZ;

                    cell = new Cell<T>(new Vector3(left, 0, top) , new Vector3(right, 0, bot));
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
        public void UpdateEntity(T ent, Vector3 OldPos)
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
        public void CalculateNeighbors(Vector3 TargetPos, float QueryRadius)
        {
            
            //create the query box that is the bounding box of the target's query
            //area
            InvertedAABBox2D QueryBox = new InvertedAABBox2D(TargetPos - new Vector3(QueryRadius, 0, QueryRadius),
                            TargetPos + new Vector3(QueryRadius, 0, QueryRadius));

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
                        Vector3 pos = ent.Pos();
                        if ((pos - TargetPos).sqrMagnitude < QueryRadius * QueryRadius)
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

