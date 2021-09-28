

namespace ST_Test_006
{
    public class QFifo<MODEL> where MODEL : class
    {
        public int mCount; //현재 들어있는 데이터갯수 (flush 한 데이터도 포함한다)
        public int _tail; //Push queue index. 새로 넣을 위치점
        public int _head; //Pop queue index. 꺼낼 위치점 
        public int mFifoSize; //최대 fifo 사이즈 

        public MODEL[] mFifo = null;

        //public struct Out_Point
        //{
        //    public int queueIndex;
        //    public QFifo<MODEL> packFifo;

        //    public void Init()
        //    {
        //        queueIndex = -1;
        //        packFifo = null;
        //    }

        //    public bool IsNull()
        //    {
        //        return (null == packFifo);
        //    }

        //    //public MODEL Unlink()
        //    //{
        //    //    if (null != packFifo)
        //    //    {
        //    //        return packFifo.Unlink(queueIndex);
        //    //    }
        //    //    return null;
        //    //}
        //    public void Unlink()
        //    {
        //        if (null != packFifo)
        //        {
        //            packFifo.Unlink(queueIndex);
        //        }
        //    }
        //}

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



        //추가되기전 마지막 큐값을 반환 한다 
        //public SpherePack ** Push(SpherePack *sphere)
        //public Out_Point Push(MODEL sphere)
        //{
        //    Out_Point out_point;
        //    out_point.packFifo = this;
        //    out_point.queueIndex = _tail; //큐 위치를 반환한다 

        //    mCount++;
        //    //SpherePack **ret = &mFifo[mSP]; //chamto 20190115: (mFifo + mSP) == &mFifo[mSP] : 1차원 포인터'배열'공간의 주소값
        //    // &*(mFifo + mSP) == &mFifo[mSP] == (mFifo + mSP)

        //    mFifo[_tail] = sphere;
        //    _tail++;
        //    if (_tail == mFifoSize) _tail = 0;

        //    return out_point;
        //}

        public void Push(MODEL sphere)
        {

            mCount++;
            //SpherePack **ret = &mFifo[mSP]; //chamto 20190115: (mFifo + mSP) == &mFifo[mSP] : 1차원 포인터'배열'공간의 주소값
            // &*(mFifo + mSP) == &mFifo[mSP] == (mFifo + mSP)

            mFifo[_tail] = sphere;
            _tail++;
            if (_tail == mFifoSize) _tail = 0;

        }

        // 큐 구조 : [ head o o o o tail x x x x ]
        //head 에서 값을 꺼내고 , tail 에 값을 넣는다 
        // 큐 구조 : [ head == tail x x x x ]
        //head 와 tail 이 같은 경우는 , 큐가 비어 있다는 의미이다  


        //Unlink함수에 의해 데이터가 null인 것들이 항목으로 포함되어 있다. 이런 항목을 _head 증가로 간단히 걸러낸다.
        //public MODEL Pop()
        //{
        //    while (_tail != _head) //데이터가 있다면 
        //    {
        //        mCount--;
        //        //SpherePack *ret = mFifo[mBottom];
        //        MODEL ret = mFifo[_head];
        //        _head++;
        //        if (_head == mFifoSize) _head = 0;
        //        if (null != ret) return ret; //큐에서 가져온 데이터가 null 일 경우, 다음 데이터를 가져온다. 
        //    }
        //    return null;
        //}

        //null 값이어도 반환하게 한다. mCount값에 의해 루프시 중간에 값이 삽입되는 비정상 상황에서도 정해진 루프의 데이터만 pop하게 보장한다 
        public MODEL Pop()
        {
            if (_tail != _head) //데이터가 있다면 
            {
                mCount--;

                MODEL ret = mFifo[_head];
                mFifo[_head] = null; //데이터를 날린다 
                _head++;
                if (_head == mFifoSize) _head = 0;
                return ret; 
            }
            return null;
        }

        public void Clear()
        {
            _head = _tail;
            if (_head == mFifoSize)
            {
                _head = 0;
                _tail = 0;
            }

            mCount = 0;
        }

        //데이터의 개수는 변경하지 않고 데이터만 날린다
        //Out_Push 객체에서 사용하는 전용함수 
        //private MODEL Unlink(int queueIndex)
        //{
        //    MODEL pack = mFifo[queueIndex];
        //    mFifo[queueIndex] = null;

        //    return pack;
        //}

        //데이터의 개수는 변경하지 않고 데이터만 날린다
        private void Unlink(int queueIndex)
        {
            mFifo[queueIndex] = null;
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

        public bool Contain(MODEL pack)
        {
            if (_tail == _head) return false; //push 와 pop 이 값다면 데이터가 없는 상태이다. 한번도 넣고 빼기를 안했거나, 같은 횟수로 넣거나 뺀경우이다. 
            int i = _head;
            while (_tail != i)
            {
                if (mFifo[i] == pack)
                {
                    return true;
                }
                i++;
                if (i == mFifoSize) i = 0;
            }
            return false;
        }

        public int GetCount() { return mCount; }

    }
}
