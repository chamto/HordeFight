using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ST_Test_003
{
    public interface IPoolConnector<Type> where Type : class, new()
    {
        //숨겨야되는 인터페이스 
        //=====================================================
        //아래 인터페이스는 Pool 객체에서만 사용하는 전용 메소드이다
        //상속 방식으로 인터페이스를 만들면 아래 인터페이스를 숨길수 있으나, 
        //확장성이 떨어져 공개된 인터페이스 방식을 사용한다 
        void InitID(int id);
        void SetUsed(bool used);

        void SetPoolNext(Type obj);
        Type GetPoolNext();
        void SetPoolPrevious(Type obj);
        Type GetPoolPrevious();


        //공개된 인터페이스
        //=====================================================
        int GetID();
        bool IsUsed();
    }

    public class Pool<Type> where Type : class, IPoolConnector<Type>, new()
    {

        private int mMaxItems = 0;
        private Type[] mData = null;

        private Type mCurrent = null; // current iteration location.
        private Type mHead = null; // head of used list.
        private Type mFree = null; // head of free list.
        private int mUsedCount = 0;
        private int mFreeCount = 0;

        public Pool() { }

        // ~Pool(void)
        // {
        //   delete mData;
        // }


        public void RemoveAll()
        {
            mHead = null;
            mFree = null;
            mCurrent = null;

            //delete mData; //todo : GC 대상에 넣기 
            mData = null;

            mMaxItems = 0;
            mUsedCount = 0;
            mFreeCount = 0;
        }


        public void Init(int maxitems)
        {
            //delete mData; // delete any previous incarnation. //todo : GC 대상에 넣기
            RemoveAll();

            mMaxItems = maxitems;
            mData = new Type[mMaxItems]; //같은코드 다른 의미. c++ : Type 객체로 된 배열을 생성. c# : Type 주소공간을 가지는 배열을 생성. c#은 포인터배열이 되므로 따로 객체를 생성해 주소를 넣어주어야 함.
            for (int i = 0; i < mMaxItems; i++)
            {
                //포인터배열에 객체를 생성해 넣는다  
                mData[i] = new Type();
                mData[i].InitID(i);
                mData[i].SetUsed(false);
            }

            mFree = mData[0];
            mHead = null;
            for (int i = 0; i < (mMaxItems - 1); i++)
            {
                mData[i].SetPoolNext(mData[i + 1]);
                if (i == 0)
                    mData[i].SetPoolPrevious(null);
                else
                    mData[i].SetPoolPrevious(mData[i - 1]);
            }
            //마지막 배열값 설정
            mData[mMaxItems - 1].SetPoolNext(null);
            mData[mMaxItems - 1].SetPoolPrevious(mData[mMaxItems - 2]);

            mCurrent = null; // there is no current, currently. <g>
            mFreeCount = maxitems;
            mUsedCount = 0;
        }

        public Type GetData(int id)
        {
            if (0 > id || id >= mMaxItems)
                return null; //잘못된 인덱스 

            if (false == mData[id].IsUsed())
                return null; //해제된 데이터

            return mData[id];
        }


        public Type GetNext(ref bool looped)
        {

            looped = false; //default value

            if (null == mHead) return null; // there is no data to process.
            Type ret;

            if (null == mCurrent)
            {
                ret = mHead;
                looped = true;
            }
            else
            {
                ret = mCurrent;
            }

            if (null != ret) mCurrent = ret.GetPoolNext();


            return ret;
        }

        public bool IsEmpty()
        {
            if (null == mHead) return true;
            return false;
        }

        public int GetUsedCount() { return mUsedCount; }
        public int GetFreeCount() { return mFreeCount; }


        //Begin() 사용법. GetNext()와 짝으로 사용된다 
        //int count = mVisible.Begin();
        //for (int i = 0; i<count; i++)
        //{
        //    CircleItem item = mVisible.GetNext();
        //}
        public int Begin()
        {
            mCurrent = mHead;
            return mUsedCount;
        }


        public Type GetNext()
        {
            if (null == mHead) return null; // there is no data to process.

            Type ret = null;

            if (null == mCurrent)
            {
                ret = mHead;
            }
            else
            {
                ret = mCurrent;
            }

            //현재것에 다음것을 넣음 
            if (null != ret) mCurrent = ret.GetPoolNext();


            return ret;
        }


        //사용하지 않는 데이터를 메모리풀로 반환한다
        public void Release(Type t)
        {

            if (t == mCurrent) mCurrent = t.GetPoolNext();

            // first patch old linked list.. his previous now points to his next
            Type prev = t.GetPoolPrevious();

            if (null != prev)
            {
                Type next = t.GetPoolNext();
                prev.SetPoolNext(next); // my previous now points to my next
                if (null != next) next.SetPoolPrevious(prev);
                // list is patched!
            }
            else
            {
                Type next = t.GetPoolNext();
                mHead = next;
                if (null != mHead) mHead.SetPoolPrevious(null);
            }

            Type temp = mFree; // old head of free list.
            mFree = t; // new head of linked list.
            t.SetPoolPrevious(null);
            t.SetPoolNext(temp);
            t.SetUsed(false); //사용안됨 설정 

            mUsedCount--;
            mFreeCount++;
        }

        public Type GetFreeNoLink() // get free, but don't link it to the used list!!
        {
            // Free allocated items are always added to the head of the list
            if (null == mFree) return null;
            Type ret = mFree;
            mFree = ret.GetPoolNext(); // new head of free list
            mUsedCount++;
            mFreeCount--;
            ret.SetPoolNext(null);
            ret.SetPoolPrevious(null);

            ret.SetUsed(true); //사용됨 설정 

            return ret;
        }

        public Type GetFreeLink()
        {
            // Free allocated items are always added to the head of the list
            if (null == mFree) return null;
            Type ret = mFree;
            mFree = ret.GetPoolNext(); // new head of free list
            Type temp = mHead; // current head of list
            mHead = ret;        // new head of list is this free one
            if (null != temp) temp.SetPoolPrevious(ret);
            mHead.SetPoolNext(temp);
            mHead.SetPoolPrevious(null);
            mUsedCount++;
            mFreeCount--;

            ret.SetUsed(true); //사용됨 설정 

            return ret;
        }

        public void AddAfter(Type e, Type item)
        {
            // Add 'item' after 'e'
            if (null != e)
            {
                //Type eprev = e.GetPrevious();
                Type enext = e.GetPoolNext();
                e.SetPoolNext(item);
                item.SetPoolNext(enext);
                item.SetPoolPrevious(e);
                if (null != enext) enext.SetPoolPrevious(item);
            }
            else
            {
                mHead = item;
                item.SetPoolPrevious(null);
                item.SetPoolNext(null);
            }

        }

        public void AddBefore(Type e, Type item)
        {
            // Add 'item' before 'e'
            Type eprev = e.GetPoolPrevious();
            //Type enext = e.GetNext();

            if (null == eprev)
                mHead = item;
            else
                eprev.SetPoolNext(item);

            item.SetPoolPrevious(eprev);
            item.SetPoolNext(e);

            e.SetPoolPrevious(item);

        }

    }

    //==========================================================
    //==========================================================
    //==========================================================
    //==========================================================

    public class QFifo<MODEL> where MODEL : class
    {
        private int mCount; //현재 들어있는 데이터갯수 (flush 한 데이터도 포함한다)
        private int _tail; //Push queue index. 새로 넣을 위치점
        private int _head; //Pop queue index. 꺼낼 위치점 
        private int mFifoSize; //최대 fifo 사이즈 

        private MODEL[] mFifo = null;

        public struct Out_Point
        {
            public int queueIndex;
            public QFifo<MODEL> packFifo;

            public void Init()
            {
                queueIndex = -1;
                packFifo = null;
            }

            public bool IsNull()
            {
                return packFifo == null;
            }

            public MODEL Unlink()
            {
                if (null != packFifo)
                {
                    return packFifo.Unlink(queueIndex);
                }
                return null;
            }
        }

        public QFifo(int fifosize)
        {
            mCount = 0;
            _tail = 0;
            _head = 0;
            mFifoSize = fifosize;

            mFifo = new MODEL[mFifoSize];
        }

        // ~SpherePackFifo(void)
        // {
        //   delete mFifo;
        // delete [] mFifo; //chamto 20190115: 이렇게 해제 해야 하는거 아닌가? 
        // }



        //public SpherePack ** Push(SpherePack *sphere)
        public Out_Point Push(MODEL sphere)
        {
            Out_Point out_point;
            out_point.packFifo = this;
            out_point.queueIndex = _tail; //큐 위치를 반환한다 

            mCount++;
            //SpherePack **ret = &mFifo[mSP]; //chamto 20190115: (mFifo + mSP) == &mFifo[mSP] : 1차원 포인터'배열'공간의 주소값
            // &*(mFifo + mSP) == &mFifo[mSP] == (mFifo + mSP)

            mFifo[_tail] = sphere;
            _tail++;
            if (_tail == mFifoSize) _tail = 0;

            return out_point;
        }

        // 큐 구조 : [ head o o o o tail x x x x ]
        //head 에서 값을 꺼내고 , tail 에 값을 넣는다 
        // 큐 구조 : [ head == tail x x x x ]
        //head 와 tail 이 같은 경우는 , 큐가 비어 있다는 의미이다  


        //Unlink함수에 의해 데이터가 null인 것들이 항목으로 포함되어 있다. 이런 항목을 _head 증가로 간단히 걸러낸다.
        public MODEL Pop()
        {
            while (_tail != _head) //데이터가 있다면 
            {
                mCount--;
                //SpherePack *ret = mFifo[mBottom];
                MODEL ret = mFifo[_head];
                _head++;
                if (_head == mFifoSize) _head = 0;
                if (null != ret) return ret; //큐에서 가져온 데이터가 null 일 경우, 다음 데이터를 가져온다. 
            }
            return null;
        }

        //데이터의 개수는 변경하지 않고 데이터만 날린다
        //Out_Push 객체에서 사용하는 전용함수 
        private MODEL Unlink(int queueIndex)
        {
            MODEL pack = mFifo[queueIndex];
            mFifo[queueIndex] = null;

            return pack;
        }

        //Unlink와 같은 기능을 하는 함수. 이름을 동일하게 해야 함 
        //사용처가 없는 함수 
        //데이터의 개수는 변경하지 않고, 데이터만 날린다 
        private bool Unlink(MODEL pack)
        {
            if (_tail == _head) return false; //push 와 pop 이 값다면 데이터가 없는 상태이다. 한번도 넣고 빼기를 안했거나, 같은 횟수로 넣거나 뺀경우이다. 
            int i = _head;
            while (_tail != i)
            {
                if (mFifo[i] == pack)
                {
                    mFifo[i] = null;
                    return true;
                }
                i++;
                if (i == mFifoSize) i = 0;
            }
            return false;
        }

        public int GetCount() { return mCount; }

    }


    //==========================================================
    //==========================================================
    //==========================================================
    //==========================================================


    public class Frustum
    {
        public enum ViewState
        {
            INSIDE,   // completely inside the frustum.
            PARTIAL,  // partially inside and partially outside the frustum.
            OUTSIDE   // completely outside the frustum
        }

        //코헨 서덜랜드 알고리즘
        //ref : https://en.wikipedia.org/wiki/Cohen%E2%80%93Sutherland_algorithm
        public enum CohenSutherland
        {
            CS_LEFT = (1 << 0),
            CS_RIGHT = (1 << 1),
            CS_TOP = (1 << 2),
            CS_BOTTOM = (1 << 3)
        }

        private int mX1;
        private int mX2;
        private int mY1;
        private int mY2;


        public void Set(int x1, int y1, int x2, int y2)
        {
            mX1 = x1;
            mY1 = y1;
            mX2 = x2;
            mY2 = y2;
        }

        public ViewState ViewVolumeTest(ref Vector3 center, float radius)
        {
            int acode = 0xFFFFFF;
            int ocode = 0;

            int x = (int)(center.x);
            int y = (int)(center.y);
            int r = (int)(radius);

            ViewCode(x - r, y - r, ref acode, ref ocode);
            ViewCode(x + r, y - r, ref acode, ref ocode);
            ViewCode(x + r, y + r, ref acode, ref ocode);
            ViewCode(x - r, y + r, ref acode, ref ocode);

            if (0 != acode) return ViewState.OUTSIDE;
            if (0 != ocode) return ViewState.PARTIAL;

            return ViewState.INSIDE;
        }

        private void ViewCode(int x, int y, ref int acode, ref int ocode)
        {
            int code = 0;
            if (x < mX1) code |= (int)CohenSutherland.CS_LEFT;
            if (x > mX2) code |= (int)CohenSutherland.CS_RIGHT;
            if (y < mY1) code |= (int)CohenSutherland.CS_TOP;
            if (y > mY2) code |= (int)CohenSutherland.CS_BOTTOM;
            ocode |= code;
            acode &= code;
        }
    }
}