using UnityEngine;
using UtilGS9;

//끌어당기는 것 
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
        InitRandom();
    }

    public void InitRandom()
    {
        mX = Misc.rand.Next() % DefineO.SW_ID;
        mY = Misc.rand.Next() % DefineO.SH_IT;

        mDx = (Misc.rand.Next() & 7) - 4; //7 => 0111 , (0 ~ 7)-4 => (-4 ~ 3) 
        mDy = (Misc.rand.Next() & 7) - 4;


        mAcount = (Misc.rand.Next() % 127) + 16; //(0 ~ 126)+16 => (16 ~ 142) 
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

    public void AdjustXY(int loc_x, int loc_y, ref int delta_x, ref int delta_y)
    {
        if (mX < loc_x) delta_x--;
        if (mX > loc_x) delta_x++;
        if (mY < loc_y) delta_y--;
        if (mY > loc_y) delta_y++;

        delta_x = Cap(delta_x);
        delta_y = Cap(delta_y);
    }

    public void ResetTest()
    {
        mAcount--;

        if (mAcount < 0)
        {
            InitRandom();
        }

        mX += mDx;
        mY += mDy;

        if (mX > DefineO.SW_ID)
        {
            mDx *= -1;
        }

        if (mX < 0)
        {
            mDx *= -1;
        }

        if (mY > DefineO.SH_IT)
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
    //------------------------------------------------------
    //                   메모리풀 전용 변수
    //------------------------------------------------------
    private int _id = -1;
    private bool _isUsed = false; 
    private CircleItem mNext = null;
    private CircleItem mPrevious = null;

    //------------------------------------------------------

    private Circle mCircle = null;

    public void SetCircle(Circle circle) { mCircle = circle; }
    public Circle GetCircle() { return mCircle; }


    //=====================================================
    //interface 구현 
    //=====================================================
    public void InitID(int id) { _id = id; }
    public int GetID() { return _id; }
    public void SetUsed(bool used) { _isUsed = used; }
    public bool IsUsed() { return _isUsed; }

    public CircleItem GetPoolNext() { return mNext; }
    public CircleItem GetPoolPrevious() { return mPrevious; }

    public void SetPoolNext(CircleItem item) { mNext = item; }
    public void SetPoolPrevious(CircleItem item) { mPrevious = item; }
    //=====================================================
}

public class Circle : IUserData
{
    public enum State
    {
        SHOW_ALL,
        SHOW_FRUSTUM,
        SHOW_RAYTRACE,
        SHOW_RANGE_TEST,

        MAX
    }


    public const int JUMP_TIME = 128;

    private int mJumpCounter;
    private float mLocX;
    private float mLocY;
    private float mRadius;

    private int mDeltaX;
    private int mDeltaY;
    private SpherePack mSphere = null;
    private Attractor _ref_Attractor = null;
    private Frustum.ViewState mViewState;

    private CircleItem mItem = null;


    //public Circle(int x, int y, int radius, SpherePackFactory* factory, Attractor* attractor)
    //public Circle(int x, int y, int radius, SpherePackFactory factory, Attractor attractor)
    public Circle(Vector3 pos, float radius, SpherePack spherePack, Attractor attractor)
    {
        mItem = null;
        mLocX = pos.x;
        mLocY = pos.y;
        mRadius = radius;
        _ref_Attractor = attractor;
        mJumpCounter = (Misc.rand.Next() % JUMP_TIME) + JUMP_TIME / 4;

        if (null != _ref_Attractor)
        {
            mDeltaX = (Misc.rand.Next() & 7) - 3; //7 => 0111 , (0 ~ 7) -3 => (-3 ~ 4)
            mDeltaY = (Misc.rand.Next() & 7) - 3;
        }
        else
        {
            mDeltaX = 0;
            mDeltaY = 0;
        }


        //Vector3 pos = new Vector3(x, y, 0);

        pos.x = (float)mLocX * (1.0f / DefineO.FIXED);
        pos.y = (float)mLocY * (1.0f / DefineO.FIXED);
        pos.z = 0;

        //mSphere = factory.AddSphere<Circle>(pos, mRadius, this, (int)(SpherePack.Flag.LEAF_TREE));
        mSphere = spherePack;
        mSphere.NewPos(ref pos); //위치값 다시 갱신
    }


    public int Process()
    {
        if (null != _ref_Attractor)
        {

            _ref_Attractor.AdjustXY((int)mLocX, (int)mLocY, ref mDeltaX, ref mDeltaY); 

            if ((mLocX + mDeltaX) > DefineO.SW_ID)
            {
                mDeltaX *= -1;
            }
            if ((mLocX + mDeltaX) < 0)
            {
                mDeltaX *= -1;
            }
            if ((mLocY + mDeltaY) > DefineO.SH_IT)
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

            pos.x = (float)(mLocX)* (1.0f / DefineO.FIXED);
            pos.y = (float)(mLocY)* (1.0f / DefineO.FIXED);
            pos.z = 0;

            mSphere.NewPos(ref pos);

        }


        mJumpCounter--;
        //mJumpCounter = 1; //chamto test

        if (mJumpCounter <= 0)
        {
            Vector3 pos;

            if (null == _ref_Attractor)
            {
                int wx = DefineO.SW_ID / 4;
                int wy = DefineO.SH_IT / 4;
                mLocX = (Misc.rand.Next() % wx) - (wx / 2) + DefineO.G_CENTER_X;
                mLocY = (Misc.rand.Next() % wy) - (wy / 2) + DefineO.G_CENTER_Y;

                if (mLocX < 0) mLocX = 0;
                if (mLocX > DefineO.SW_ID) mLocX = DefineO.SW_ID;
                if (mLocY < 0) mLocY = 0;
                if (mLocY > DefineO.SH_IT) mLocY = DefineO.SH_IT;

            }

            pos.x = (float)(mLocX)* (1.0f / DefineO.FIXED);
            pos.y = (float)(mLocY) * (1.0f / DefineO.FIXED);
            pos.z = 0;

            mJumpCounter = Misc.rand.Next() % JUMP_TIME;

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

    public Frustum.ViewState GetViewState() { return mViewState; }
    public void SetViewState(Frustum.ViewState state) { mViewState = state; }

    //=====================================================
    //interface 구현 
    //=====================================================
    //public void Available_UserData() { }
    //=====================================================
}
