

public class SpherePackFifo
{
    private int mCount;
    private int mSP; // stack pointer
    private int mBottom;
    private int mFifoSize;
    //private SpherePack** mFifo;
    private SpherePack[] mFifo = null;

    public struct Out_Push
    {
        public int stackIndex;
        public SpherePackFifo packFifo;

        public void Init()
        {
            stackIndex = -1;
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
                return packFifo.Unlink(stackIndex);
            }
            return null;
        }
    }

    public SpherePackFifo(int fifosize)
    {
        mCount = 0;
        mSP = 0;
        mBottom = 0;
        mFifoSize = fifosize;
        //mFifo = new SpherePack* [mFifoSize];
        mFifo = new SpherePack[mFifoSize];
    }

    // ~SpherePackFifo(void)
    // {
    //   delete mFifo;
    // delete [] mFifo; //chamto 20190115: 이렇게 해제 해야 하는거 아닌가? 
    // }


    public SpherePack Unlink(int stackIndex)
    {
        SpherePack pack = mFifo[stackIndex];
        mFifo[stackIndex] = null;

        return pack;
    }

    //public SpherePack ** Push(SpherePack *sphere)
    public Out_Push Push(SpherePack sphere)
    {
        Out_Push out_Push;
        out_Push.packFifo = this;
        out_Push.stackIndex = mSP;
                
        mCount++;
        //SpherePack **ret = &mFifo[mSP]; //chamto 20190115: (mFifo + mSP) == &mFifo[mSP] : 1차원 포인터'배열'공간의 주소값
        SpherePack ret = mFifo[mSP];
        //stackIndex = mSP; //스택 위치를 반환한다 
        mFifo[mSP] = sphere;
        mSP++;
        if (mSP == mFifoSize) mSP = 0;

        return out_Push;
    }

    //public SpherePack * Pop(void)
    public SpherePack Pop()
    {
        while (mSP != mBottom)
        {
            mCount--;
            //SpherePack *ret = mFifo[mBottom];
            SpherePack ret = mFifo[mBottom];
            mBottom++;
            if (mBottom == mFifoSize) mBottom = 0;
            if (null != ret) return ret;
        }
        return null;
    }

    //public bool Flush(SpherePack *pack)
    public bool Flush(SpherePack pack)
    {
        if (mSP == mBottom) return false;
        int i = mBottom;
        while (i != mSP)
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