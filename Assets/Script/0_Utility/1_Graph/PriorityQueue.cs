using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;


public class Heap
{
    //----------------------- Swap -------------------------------------------
    //
    //  used to swap two values
    //------------------------------------------------------------------------

    static public void Swap<T>(ref T a, ref T b) 
    {
        T temp = a;
        a = b;
        b = temp;
    }

    static public void Swap<T>(List<T> heap, int src_index, int dst_index)
    {
        T temp = heap[src_index];
        heap[src_index] = heap[dst_index];
        heap[dst_index] = temp;
    }

    //-------------------- ReorderUpwards ------------------------------------
    //
    //  given a heap and a node in the heap, this function moves upwards
    //  through the heap swapping elements until the heap is ordered
    //------------------------------------------------------------------------

    static public void ReorderUpwards<T>(List<T> heap, int nd) where T : IEquatable<T> , IComparable<T>
    {
        //move up the heap swapping the elements until the heap is ordered
        //while ((nd > 1) && (heap[nd / 2] < heap[nd]))
        while ((nd > 1) && (heap[nd / 2].CompareTo(heap[nd]) < 0))
        {
            //T a = heap[nd / 2];
            //T b = heap[nd];
            //PriorityQueue.Swap<T>(ref a, ref b);
            //heap[nd / 2] = a;
            //heap[nd] = b;

            Heap.Swap<T>(heap, nd / 2, nd);

            nd /= 2;
        }
    }

    //--------------------- ReorderDownwards ---------------------------------
    //
    //  given a heap, the heapsize and a node in the heap, this function
    //  reorders the elements in a top down fashion by moving down the heap
    //  and swapping the current node with the greater of its two children
    //  (provided a child is larger than the current node)
    //------------------------------------------------------------------------

    static public void ReorderDownwards<T>(List<T> heap, int nd, int HeapSize) where T : IEquatable<T>, IComparable<T>
    {
        //move down the heap from node nd swapping the elements until
        //the heap is reordered
        while (2 * nd <= HeapSize)
        {
            int child = 2 * nd;

            //set child to largest of nd's two children
            if ((child < HeapSize) && (heap[child].CompareTo(heap[child + 1]) < 0  ))
            {
                ++child;
            }

            //if this nd is smaller than its child, swap
            //if (heap[nd] < heap[child])
            if (heap[nd].CompareTo(heap[child]) < 0)
            {
                //Swap(heap[child], heap[nd]);
                Heap.Swap<T>(heap, child, nd);

                //move the current node down the tree
                nd = child;
            }

            else
            {
                break;
            }
        }
    }
}







//--------------------- PriorityQ ----------------------------------------
//
//  basic heap based priority queue implementation
//------------------------------------------------------------------------
public class PriorityQ<T>  where T : IEquatable<T> , IComparable<T>
{
    private List<T> m_Heap;

    private int m_iSize;

    private int m_iMaxSize;

    //given a heap and a node in the heap, this function moves upwards
    //through the heap swapping elements until the heap is ordered
    private void ReorderUpwards(List<T> heap, int nd)
    {
        //move up the heap swapping the elements until the heap is ordered
        while ((nd > 1) && (heap[nd / 2].CompareTo(heap[nd])< 0))
        {
            //Swap(heap[nd / 2], heap[nd]);
            Heap.Swap(heap, nd / 2, nd);

            nd /= 2;
        }
    }

    //given a heap, the heapsize and a node in the heap, this function
    //reorders the elements in a top down fashion by moving down the heap
    //and swapping the current node with the greater of its two children
    //(provided a child is larger than the current node)
    private void ReorderDownwards(List<T> heap, int nd, int HeapSize)
    {
        //move down the heap from node nd swapping the elements until
        //the heap is reordered
        while (2 * nd <= HeapSize)
        {
            int child = 2 * nd;

            //set child to largest of nd's two children
            //if ((child < HeapSize) && (heap[child] < heap[child + 1]))
            if ((child < HeapSize) && (heap[child].CompareTo(heap[child + 1]) < 0))
            {
                ++child;
            }

            //if this nd is smaller than its child, swap
            //if (heap[nd] < heap[child])
            if (heap[nd].CompareTo(heap[child]) < 0 )
            {
                //Swap(heap[child], heap[nd]);
                Heap.Swap(heap, child, nd);

                //move the current node down the tree
                nd = child;
            }

            else
            {
                break;
            }
        }
    }



    public PriorityQ(int MaxSize)
    {
        m_iMaxSize = MaxSize;
        m_iSize = 0;
        //m_Heap.assign(MaxSize + 1, T());
        m_Heap = new List<T>(MaxSize + 1);
    }

    public bool empty() 
    {
        return (m_iSize==0);
    }

    //to insert an item into the queue it gets added to the end of the heap
    //and then the heap is reordered
    public void insert(T item)
    {
        //assert(m_iSize + 1 <= m_iMaxSize);
        Assert.IsTrue((m_iSize + 1 <= m_iMaxSize));

        ++m_iSize;

        m_Heap[m_iSize] = item;

        ReorderUpwards(m_Heap, m_iSize);
    }

    //to get the max item the first element is exchanged with the lowest
    //in the heap and then the heap is reordered from the top down. 
    public T pop()
    {
        //Swap(m_Heap[1], m_Heap[m_iSize]);
        Heap.Swap(m_Heap,1, m_iSize);

        ReorderDownwards(m_Heap, 1, m_iSize - 1);

        return m_Heap[m_iSize--];
    }

    //so we can take a peek at the first in line
    //public const T& Peek()const{return m_Heap[1];}
    public void SetPeek(T value) 
    {
        m_Heap[1] = value;
    }
    public T GetPeek()
    {
        return m_Heap[1];
    }
}

//--------------------- PriorityQLow -------------------------------------
//
//  basic 2-way heap based priority queue implementation. This time the priority
//  is given to the lowest valued key
//------------------------------------------------------------------------

public class PriorityQLow<T> where T : IEquatable<T>, IComparable<T>
{
    

    private List<T> m_Heap;

    private int m_iSize;

    private int m_iMaxSize;

    //given a heap and a node in the heap, this function moves upwards
    //through the heap swapping elements until the heap is ordered
    private void ReorderUpwards(List<T> heap, int nd)
    {
        //move up the heap swapping the elements until the heap is ordered
        //while ((nd > 1) && (heap[nd / 2] > heap[nd]))
        while ((nd > 1) && (heap[nd / 2].CompareTo(heap[nd]) > 0))
        {
            //Swap(heap[nd / 2], heap[nd]);
            Heap.Swap(heap,nd / 2, nd);

            nd /= 2;
        }
    }

    //given a heap, the heapsize and a node in the heap, this function
    //reorders the elements in a top down fashion by moving down the heap
    //and swapping the current node with the smaller of its two children
    //(provided a child is larger than the current node)
    private void ReorderDownwards(List<T> heap, int nd, int HeapSize)
    {
        //move down the heap from node nd swapping the elements until
        //the heap is reordered
        while (2 * nd <= HeapSize)
        {
            int child = 2 * nd;

            //set child to largest of nd's two children
            //if ((child < HeapSize) && (heap[child] > heap[child + 1]))
            if ((child < HeapSize) && (heap[child].CompareTo(heap[child + 1]) > 0))
            {
                ++child;
            }

            //if this nd is smaller than its child, swap
            //if (heap[nd] > heap[child])
            if (heap[nd].CompareTo(heap[child]) > 0)
            {
                //Swap(heap[child], heap[nd]);
                Heap.Swap(heap,child, nd);

                //move the current node down the tree
                nd = child;
            }

            else
            {
                break;
            }
        }
    }



    public PriorityQLow(int MaxSize)
    {
        m_iMaxSize = MaxSize;
        m_iSize = 0;
        //m_Heap.assign(MaxSize + 1, T());
        m_Heap = new List<T>(MaxSize + 1);
    }

    public bool empty() 
    {
        return (m_iSize==0);
    }

    //to insert an item into the queue it gets added to the end of the heap
    //and then the heap is reordered
    public void insert(T item)
    {
        //assert(m_iSize + 1 <= m_iMaxSize);
        Assert.IsTrue(m_iSize + 1 <= m_iMaxSize);

        ++m_iSize;

        m_Heap[m_iSize] = item;

        ReorderUpwards(m_Heap, m_iSize);
    }

    //to get the max item the first element is exchanged with the lowest
    //in the heap and then the heap is reordered from the top down. 
    public T pop()
    {
        //Swap(m_Heap[1], m_Heap[m_iSize]);
        Heap.Swap(m_Heap, 1,m_iSize);

        ReorderDownwards(m_Heap, 1, m_iSize - 1);

        return m_Heap[m_iSize--];
    }

    //so we can take a peek at the first in line
    //public const T& peek()const{return m_Heap[1];}
    public void SetPeek(T value)
    {
        m_Heap[1] = value;
    }
    public T GetPeek()
    {
        return m_Heap[1];
    }

}

//----------------------- IndexedPriorityQLow ---------------------------
//
//  Priority queue based on an index into a set of keys. The queue is
//  maintained as a 2-way heap.
//
//  The priority in this implementation is the lowest valued key
//------------------------------------------------------------------------

public class IndexedPriorityQLow<KeyType> where KeyType : IEquatable<KeyType>, IComparable<KeyType>
{
    private List<KeyType>  m_vecKeys;

    private List<int> m_Heap;

    private List<int> m_invHeap;

    int m_iSize;
    int m_iMaxSize;

    private void Swap(int a, int b)
    {
        int temp = m_Heap[a]; m_Heap[a] = m_Heap[b]; m_Heap[b] = temp;

        //change the handles too
        m_invHeap[m_Heap[a]] = a; m_invHeap[m_Heap[b]] = b;
    }

    private void ReorderUpwards(int nd)
    {
        //move up the heap swapping the elements until the heap is ordered
        //while ((nd > 1) && (m_vecKeys[m_Heap[nd / 2]] > m_vecKeys[m_Heap[nd]]))
        while ((nd > 1) && (m_vecKeys[m_Heap[nd / 2]].CompareTo(m_vecKeys[m_Heap[nd]]) > 0))
        {
            Swap(nd / 2, nd);

            nd /= 2;
        }
    }

    private void ReorderDownwards(int nd, int HeapSize)
    {
        //move down the heap from node nd swapping the elements until
        //the heap is reordered
        while (2 * nd <= HeapSize)
        {
            int child = 2 * nd;

            //set child to smaller of nd's two children
            //if ((child < HeapSize) && (m_vecKeys[m_Heap[child]] > m_vecKeys[m_Heap[child + 1]]))
            if ((child < HeapSize) && (m_vecKeys[m_Heap[child]].CompareTo(m_vecKeys[m_Heap[child + 1]]) > 0))
            {
                ++child;
            }

            //if this nd is larger than its child, swap
            //if (m_vecKeys[m_Heap[nd]] > m_vecKeys[m_Heap[child]])
            if (m_vecKeys[m_Heap[nd]].CompareTo(m_vecKeys[m_Heap[child]]) > 0)
            {
                Swap(child, nd);

                //move the current node down the tree
                nd = child;
            }

            else
            {
                break;
            }
        }
    }



  
      //you must pass the constructor a reference to the std::vector the PQ
      //will be indexing into and the maximum size of the queue.
    public  IndexedPriorityQLow(List<KeyType> keys, int MaxSize) 
    {
        m_vecKeys = keys;
        m_iMaxSize = MaxSize;
        m_iSize = 0;
        //m_Heap.assign(MaxSize + 1, 0);
        //m_invHeap.assign(MaxSize + 1, 0);
        m_Heap = new List<int>(MaxSize + 1);
        m_invHeap = new List<int>(MaxSize + 1);
            
        //미리할당후 0으로 초기화 
        for (int i = 0; i < MaxSize + 1; i++)
        {
            m_Heap.Add(0);
            m_invHeap.Add(0);
        }
    }

    public bool empty() 
    {
        return (m_iSize==0);
    }

    //to insert an item into the queue it gets added to the end of the heap
    //and then the heap is reordered from the bottom up.
    public void insert(int idx)
    {
        //assert(m_iSize + 1 <= m_iMaxSize);
        Assert.IsTrue(m_iSize + 1 <= m_iMaxSize);

        ++m_iSize;

        m_Heap[m_iSize] = idx;

        m_invHeap[idx] = m_iSize;

        ReorderUpwards(m_iSize);
    }

    //to get the min item the first element is exchanged with the lowest
    //in the heap and then the heap is reordered from the top down. 
    public int Pop()
    {
        Swap(1, m_iSize);

        ReorderDownwards(1, m_iSize - 1);

        return m_Heap[m_iSize--];
    }

    //if the value of one of the client key's changes then call this with 
    //the key's index to adjust the queue accordingly
    public void ChangePriority(int idx)
    {
        ReorderUpwards(m_invHeap[idx]);
    }
}

