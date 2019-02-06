/***********************************************************************/
/** POOL        : Template class to manage a fixed pool of items for   */
/**               extremely fast allocation and deallocation.          */
/**                                                                    */
/**               Written by John W. Ratcliff jratcliff@att.net        */
/***********************************************************************/

public interface IPoolConnector<Type> where Type : class, new()
{
    
    void SetPoolNext(Type obj);
    Type GetPoolNext();
    void SetPoolPrevious(Type obj);
    Type GetPoolPrevious();
}

public class Pool<Type>  where Type : class, IPoolConnector<Type>, new()
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


    public void Release()
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
        Release();

        mMaxItems = maxitems;
        mData = new Type[mMaxItems]; //같은코드 다른 의미. c++ : Type 객체로 된 배열을 생성. c# : Type 주소공간을 가지는 배열을 생성. c#은 포인터배열이 되므로 따로 객체를 생성해 주소를 넣어주어야 함.
        for (int i = 0; i < mMaxItems; i++)
        {
            //포인터배열에 객체를 생성해 넣는다  
            mData[i] = new Type();
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

    //해제처리가 아닌 연결을 끊는 처리이다 
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