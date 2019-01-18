using UnityEngine;
using UtilGS9;

public class Attractor
{
    private int mAcount;
    private int mX;
    private int mY;
    private int mDx;
    private int mDy;

    public int GetX() { return mX; }
    public int GetY() { return mY; }

    public Attractor()
    {
        mX = Misc.rand.Next() % DefineO.SWID;
        mY = Misc.rand.Next() % DefineO.SHIT;

        mDx = (Misc.rand.Next() & 7) - 4;
        mDy = (Misc.rand.Next() & 7) - 4;


        mAcount = (Misc.rand.Next() % 127) + 16;
    }

    public int Cap(int v)
    {
        if (v > 0)
        {
            if (v > 32) v = 32;
        }
        else
        {
            if (v < -32) v = -32;
        }
        return v;
    }

    public void AdjustXY(int x, int y, ref int dx, ref int dy)
    {
        if (mX < x) dx--;
        if (mX > x) dx++;
        if (mY < y) dy--;
        if (mY > x) dy++;

        dx = Cap(dx);
        dy = Cap(dy);
    }

    public void ResetTest()
    {
        mAcount--;

        if (mAcount < 0)
        {
            mX = Misc.rand.Next() % DefineO.SWID;
            mY = Misc.rand.Next() % DefineO.SHIT;
            mDx = (Misc.rand.Next() & 7) - 4;
            mDy = (Misc.rand.Next() & 7) - 4;
            mAcount = (Misc.rand.Next() % 127) + 16;
        }

        mX += mDx;
        mY += mDy;

        if (mX > DefineO.SWID)
        {
            mDx *= -1;
        }

        if (mX < 0)
        {
            mDx *= -1;
        }

        if (mY > DefineO.SHIT)
        {
            mDy *= -1;
        }

        if (mY < 0)
        {
            mDy *= -1;
        }

    }
}



public class CircleItem : IPoolConnector<CircleItem>
{
    private CircleItem mNext = null;
    private CircleItem mPrevious = null;
    private Circle mCircle = null;

    public void SetCircle(Circle circle) { mCircle = circle; }
    public Circle GetCircle() { return mCircle; }


    //=====================================================
    //interface 구현 
    //=====================================================
    public CircleItem GetNext() { return mNext; }
    public CircleItem GetPrevious() { return mPrevious; }

    public void SetNext(CircleItem item) { mNext = item; }
    public void SetPrevious(CircleItem item) { mPrevious = item; }
    //=====================================================
}

public class Circle : IUserData
{
    public enum CircleState
    {
        CS_SHOW_ALL,
        CS_SHOW_FRUSTUM,
        CS_SHOW_RAYTRACE,
        CS_SHOW_RANGE_TEST,
        CS_LAST
    }

    private int mJumpCounter;
    private int mLocX;
    private int mLocY;
    private int mRadius;

    private int mDeltaX;
    private int mDeltaY;
    private SpherePack mSphere = null;
    private Attractor mAttractor = null;
    private CircleItem mItem = null;
    private DefineO.ViewState mViewState;


    //public Circle(int x, int y, int radius, SpherePackFactory* factory, Attractor* attractor)
    public Circle(int x, int y, int radius, SpherePackFactory factory, Attractor attractor)
    {
        mItem = null;
        mLocX = x;
        mLocY = y;
        mRadius = radius;
        mAttractor = attractor;

        mJumpCounter = (Misc.rand.Next() % DefineO.JUMP_TIME) + DefineO.JUMP_TIME / 4;

        if (null != mAttractor)
        {
            mDeltaX = (Misc.rand.Next() & 7) - 3;
            mDeltaY = (Misc.rand.Next() & 7) - 3;
        }
        else
        {
            mDeltaX = 0;
            mDeltaY = 0;
        }


        Vector3 pos = new Vector3(x, y, 0);

        pos.x = (float)mLocX * (1.0f / DefineO.FIXED);
        pos.y = (float)mLocY * (1.0f / DefineO.FIXED);
        pos.z = 0;

        mSphere = factory.AddSphere<Circle>(pos, mRadius, this, (int)(SpherePack.SpherePackFlag.SPF_LEAF_TREE));
    }


    public int Process()
    {
        if (null != mAttractor)
        {

            mAttractor.AdjustXY(mLocX, mLocY, ref mDeltaX, ref mDeltaY);

            if ((mLocX + mDeltaX) > DefineO.SWID)
            {
                mDeltaX *= -1;
            }
            if ((mLocX + mDeltaX) < 0)
            {
                mDeltaX *= -1;
            }
            if ((mLocY + mDeltaY) > DefineO.SHIT)
            {
                mDeltaY *= -1;
            }
            if ((mLocY + mDeltaY) < 0)
            {
                mDeltaY *= -1;
            }


            mLocX += mDeltaX;
            mLocY += mDeltaY;


            Vector3 pos;

            pos.x = (float)(mLocX) * (1.0f / DefineO.FIXED);
            pos.y = (float)(mLocY) * (1.0f / DefineO.FIXED);
            pos.z = 0;

            mSphere.NewPos(ref pos);

        }


        mJumpCounter--;

        if (mJumpCounter <= 0)
        {
            Vector3 pos;

            if (null == mAttractor)
            {
                int wx = DefineO.SWID / 4;
                int wy = DefineO.SHIT / 4;
                mLocX = (Misc.rand.Next() % wx) - (wx / 2) + DefineO.gCenterX;
                mLocY = (Misc.rand.Next() % wy) - (wy / 2) + DefineO.gCenterY;

                if (mLocX < 0) mLocX = 0;
                if (mLocX > DefineO.SWID) mLocX = DefineO.SWID;
                if (mLocY < 0) mLocY = 0;
                if (mLocY > DefineO.SHIT) mLocY = DefineO.SHIT;

            }

            pos.x = (float)(mLocX) * (1.0f / DefineO.FIXED);
            pos.y = (float)(mLocY) * (1.0f / DefineO.FIXED);
            pos.z = 0;

            mJumpCounter = Misc.rand.Next() % DefineO.JUMP_TIME;

            //rand AND 31 (5bit) 연산하여 ( 0 ~ 31 ) 사이의 값을 구함. 구한 값에 16을 뺌. 최종범위 (-16 ~ 15)
            mDeltaX = (Misc.rand.Next() & 31) - 16; //1 + 2 + 4 + 8 + 16 = 31 : 5bit
            mDeltaY = (Misc.rand.Next() & 31) - 16;
            mSphere.NewPos(ref pos);
        }

        return 0;
    }

    public int Render()
    {
        DefineO.DrawCircle((float)mLocX, (float)mLocY, (float)mRadius, (uint)0x00ffffff);

        return 0;
    }

    public void SetCircleItem(CircleItem item) { mItem = item; }
    public CircleItem GetCircleItem() { return mItem; }

    public SpherePack GetSpherePack() { return mSphere; }

    public DefineO.ViewState GetViewState() { return mViewState; }
    public void SetViewState(DefineO.ViewState state) { mViewState = state; }

    //=====================================================
    //interface 구현 
    //=====================================================
    //public void Available_UserData() { }
    //=====================================================
}
