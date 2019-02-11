

public class SpherePackFifo
{
    private int mCount; //현재 들어있는 데이터갯수 (flush 한 데이터도 포함한다)
    private int _tail; //Push queue index. 새로 넣을 위치점
    private int _head; //Pop queue index. 꺼낼 위치점 
    private int mFifoSize; //최대 fifo 사이즈 

    private SpherePack[] mFifo = null;

    public struct Out_Push
    {
        public int queueIndex;
        public SpherePackFifo packFifo;

        public void Init()
        {
            queueIndex = -1;
            packFifo = null;
        }

        public bool IsNull()
        {
            return packFifo == null;
        }

        public SpherePack Unlink()
        {
            if(null != packFifo)
            {
                return packFifo.Unlink(queueIndex);
            }
            return null;
        }
    }

    public SpherePackFifo(int fifosize)
    {
        mCount = 0;
        _tail = 0;
        _head = 0;
        mFifoSize = fifosize;

        mFifo = new SpherePack[mFifoSize];
    }

    // ~SpherePackFifo(void)
    // {
    //   delete mFifo;
    // delete [] mFifo; //chamto 20190115: 이렇게 해제 해야 하는거 아닌가? 
    // }



    //public SpherePack ** Push(SpherePack *sphere)
    public Out_Push Push(SpherePack sphere)
    {
        Out_Push out_Push;
        out_Push.packFifo = this;
        out_Push.queueIndex = _tail; //큐 위치를 반환한다 
                
        mCount++;
        //SpherePack **ret = &mFifo[mSP]; //chamto 20190115: (mFifo + mSP) == &mFifo[mSP] : 1차원 포인터'배열'공간의 주소값
        //SpherePack ret = mFifo[_SI_push];

        mFifo[_tail] = sphere;
        _tail++;
        if (_tail == mFifoSize) _tail = 0;

        return out_Push;
    }

    //public SpherePack * Pop(void)
    public SpherePack Pop()
    {
        while (_tail != _head) //데이터가 있다면 
        {
            mCount--;
            //SpherePack *ret = mFifo[mBottom];
            SpherePack ret = mFifo[_head];
            _head++;
            if (_head == mFifoSize) _head = 0;
            if (null != ret) return ret;
        }
        return null;
    }

    //Out_Push 객체에서 사용하는 전용함수 
    private SpherePack Unlink(int queueIndex)
    {
        SpherePack pack = mFifo[queueIndex];
        mFifo[queueIndex] = null;

        return pack;
    }

    //사용처가 없는 함수 
    //데이터의 갯수는 변경하지 않고, 데이터만 날린다 
    private bool Flush(SpherePack pack)
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