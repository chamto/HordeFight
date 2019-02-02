using System;
using UnityEngine;


//UserData 대상이 되는 객체는 이 인터페이스를 상속받아야 한다 
public interface IUserData
{
    //void Available_UserData();
}

public class SpherePack : Sphere , IPoolConnector<SpherePack> , IUserData
{
    //enum을 비트연산의 대상으로 하고자 할때 [FlagsAttribute]를 넣어준다 
    //ref : https://docs.microsoft.com/en-us/dotnet/api/system.flagsattribute?redirectedfrom=MSDN&view=netframework-4.7.2
    [FlagsAttribute]
    public enum Flag
    {
        NONE = 0,
        
        SUPERSPHERE = (1 << 0), // this is a supersphere, allocated and deleted by us
        ROOT_TREE = (1 << 1), // member of the root tree
        LEAF_TREE = (1 << 2), // member of the leaf node tree
        ROOTNODE = (1 << 3), // this is the root node
        RECOMPUTE = (1 << 4), // needs recomputed bounding sphere
        INTEGRATE = (1 << 5), // needs to be reintegrated into tree
                                  // Frame-to-frame view frustum status.  Only does callbacks when a
                                  // state change occurs.
        HIDDEN = (1 << 6), // outside of view frustum
        PARTIAL = (1 << 7), // partially inside view frustum
        INSIDE = (1 << 8)  // completely inside view frustum
    }

    private SpherePack mNext = null; // linked list pointers
    private SpherePack mPrevious = null; // used by pool memory management linked list code

    private SpherePack mParent = null;
    private SpherePack mChildren = null;  // *my* children

    private SpherePack mNextSibling = null; // doubly linked list of my brothers // our brothers and sisters at this level.
    private SpherePack mPrevSibling = null; // and sisters

    private SpherePackFifo.Out_Push mFifo1; // address of location inside of fifo1
    private SpherePackFifo.Out_Push mFifo2; // address of location inside of fifo2

    private int mFlags; // my bit flags.
    private int mChildCount = 0; // number of children

    private float mBindingDistance;

    //private void* mUserData = null;
    private IUserData mUserData = null;

    private SpherePackFactory mFactory = null; // the factory we are a member of.
                                                //#if DEMO
    private uint mColor;

    //public SpherePack() : base()
    //{
        
    //    //base.SetRadius(0);          // default radius
    //    //base.mCenter = Vector3.zero;
    //}

    //factory: factory we belong to
    //pos: center of sphere
    //radius: radius of sphere
    //public void Init(SpherePackFactory factory, Vector3 pos, float radius, IUserData userdata)
    public void Init(SpherePackFactory factory, Vector3 pos, float radius)
    {
        mUserData = null;
        //mUserData = userdata;
        mParent = null;
        mNextSibling = null;
        mPrevSibling = null;
        mFlags = 0;
        mFifo1.Init();
        mFifo2.Init();
        mFactory = factory;
        mCenter = pos;
        SetRadius(radius);
    }

    // Access to SpherePack bit flags.
    public void SetSpherePackFlag(int flag) { mFlags |= flag; }
    public void SetSpherePackFlag(Flag flag) { mFlags |= (int)flag; }
    public void ClearSpherePackFlag(int flag) { mFlags &= ~flag; }
    public bool HasSpherePackFlag(Flag flag)
    {
        if (0 != (mFlags & (int)flag)) return true;
        return false;
    }


    public void SetParent(SpherePack pack) { mParent = pack; }
    public SpherePack GetParent() { return mParent; }

    // Sphere has a new position. //inline
    public void NewPos(ref Vector3 pos)
    {

        mCenter = pos;    // set our new center position.

        // is we have a parent (meaning we are a valid leaf node) and we have not already been flagged for re-integration, then.....

        if (null != mParent && !HasSpherePackFlag(Flag.INTEGRATE))
        {

            float dist = DistanceSquared(mParent);  // compute squared distance to our parent.

            if (dist >= mBindingDistance) // if that exceeds our binding distance...
            {

                if (!mParent.HasSpherePackFlag(Flag.RECOMPUTE))
                {
                    // If our parent, is not already marked to be recomputed (rebalance the sphere), then add him to the recomputation fifo.
                    mFactory.AddRecompute(mParent);
                }
                // Unlink ourselves from the parent sphere and place ourselves into the root node.
                Unlink();
                mFactory.AddIntegrate(this); // add ourselves to the re-integration fifo.
            }
        }

    }

    // Sphere has a new position and radius //inline
    public void NewPosRadius(Vector3 pos, float radius)
    {
        // New position and, possibly, a new radius.

        mCenter = pos;

        if (null != mParent && !HasSpherePackFlag(Flag.INTEGRATE))
        {

            //if (radius != GetRadius())
            if(Mathf.Epsilon < Mathf.Abs(radius - mRadius))
            {
                SetRadius(radius);
                ComputeBindingDistance(mParent);
            }

            float dist = DistanceSquared(mParent);

            if (dist >= mBindingDistance)
            {
                if (!mParent.HasSpherePackFlag(Flag.RECOMPUTE)) mFactory.AddRecompute(mParent);
                Unlink();
                mFactory.AddIntegrate(this);
            }
            else
            {
                if (!mParent.HasSpherePackFlag(Flag.RECOMPUTE)) mFactory.AddRecompute(mParent);
            }
        }
    }


    public void Unlink()
    {
        if (false == mFifo1.IsNull()) // if we belong to fifo1, null us out
        {
            //*mFifo1 = null;
            mFifo1.Unlink();
            mFifo1.Init();
        }

        if (false == mFifo2.IsNull()) // if we belong to fifo2, null us out
        {
            //*mFifo2 = null;
            mFifo2.Unlink();
            mFifo2.Init();
        }

        if (null != mParent) mParent.LostChild(this);

        //assert(!mChildren); // can't unlink guys with children!

        mParent = null; // got no father anymore
    }


    public void AddChild(SpherePack pack)
    {

        SpherePack my_child = mChildren;
        mChildren = pack; // new head of list

        pack.SetNextSibling(my_child); // his next is my old next
        pack.SetPrevSibling(null); // at head of list, no previous
        pack.SetParent(this);

        if (null != my_child) my_child.SetPrevSibling(pack); // previous now this..

        mChildCount++;

        float dist = DistanceSquared(pack);
        float radius = Mathf.Sqrt(dist) + pack.GetRadius();

        //assert(radius <= GetRadius());
    }

    public void SetNextSibling(SpherePack child) { mNextSibling = child; }
    public void SetPrevSibling(SpherePack child) { mPrevSibling = child; }

    public SpherePack GetNextSibling() { return mNextSibling; }
    public SpherePack GetPrevSibling() { return mPrevSibling; }
    public SpherePack GetChildren() { return mChildren; }


    //=====================================================
    //interface 구현 
    //=====================================================
    public SpherePack GetNext() { return mNext; }
    public SpherePack GetPrevious() { return mPrevious; }

    public void SetNext(SpherePack pack) { mNext = pack; }
    public void SetPrevious(SpherePack pack) { mPrevious = pack; }

    //public void Available_UserData(){}
    //=====================================================

    public T GetUserData<T>() where T : IUserData { return (T)mUserData; }
    public void SetUserData<T>(T data) where T : IUserData { mUserData = (IUserData)data; }
    //public IUserData GetUserData() { return mUserData; }
    //public void SetUserData(IUserData data) { mUserData = data; }
    //=====================================================

    public Vector3 GetPos() { return mCenter; }


    public float DistanceSquared(SpherePack pack) { return (mCenter - pack.mCenter).sqrMagnitude; }

    //inline
    public void LostChild(SpherePack t)
    {
        //assert( mChildCount );
        //assert( mChildren );

        //#ifdef _DEBUG  // debug validation code.

        SpherePack pack = mChildren;
        //bool found = false;
        while (null != pack)
        {
            if (pack == t)
            {
                //assert(!found);
                //found = true;
            }
            pack = pack.GetNextSibling();
        }
        //assert( found );

        //#endif

        // first patch old linked list.. his previous now points to his next
        SpherePack prev = t.GetPrevSibling();

        if (null != prev)
        {
            SpherePack next = t.GetNextSibling();
            prev.SetNextSibling(next); // my previous now points to my next
            if (null != next) next.SetPrevSibling(prev);
            // list is patched!
        }
        else
        {
            SpherePack next = t.GetNextSibling();
            mChildren = next;
            if (null != mChildren) mChildren.SetPrevSibling(null);
        }

        mChildCount--;

        //if (!mChildCount && HasSpherePackFlag(SpherePackFlag.SPF_SUPERSPHERE))
        if (0 == mChildCount && HasSpherePackFlag(Flag.SUPERSPHERE))
        {
            mFactory.Remove(this);
        }
    }


    //inline
    public bool Recompute(float gravy)
    {
        if (null == mChildren) return true; // kill it!
        if (HasSpherePackFlag(Flag.ROOTNODE)) return false; // don't recompute root nodes!

        //#if 1
        // recompute bounding sphere!
        Vector3 total = Vector3.zero;
        int count = 0;
        SpherePack pack = mChildren;
        while (null != pack)
        {
            total += pack.mCenter;
            count++;
            pack = pack.GetNextSibling();
        }

        if (0 != count)
        {
            float recip = 1.0f / (float)(count);
            total *= recip;

            Vector3 oldpos = mCenter;

            mCenter = total; // new origin!
            float maxradius = 0;

            pack = mChildren;

            while (null != pack)
            {
                float dist = DistanceSquared(pack);
                float radius = Mathf.Sqrt(dist) + pack.GetRadius();
                if (radius > maxradius)
                {
                    maxradius = radius;
                    if ((maxradius + gravy) >= GetRadius())
                    {
                        mCenter = oldpos;
                        ClearSpherePackFlag((int)Flag.RECOMPUTE);
                        return false;
                    }
                }
                pack = pack.GetNextSibling();
            }

            maxradius += gravy;

            SetRadius(maxradius);

            // now all children have to recompute binding distance!!
            pack = mChildren;

            while (null != pack)
            {
                pack.ComputeBindingDistance(this);
                pack = pack.GetNextSibling();
            }

        }

        //#endif

        ClearSpherePackFlag((int)Flag.RECOMPUTE);

        return false;
    }

    public int GetChildCount() { return mChildCount; }


    public void SetFifo1(SpherePackFifo.Out_Push fifo)
    {
        mFifo1 = fifo;
    }

    public void SetFifo2(SpherePackFifo.Out_Push fifo)
    {
        mFifo2 = fifo;
    }

    public void InitFifo1()
    {
        mFifo1.Init();
    }

    public void InitFifo2()
    {
        mFifo2.Init();
    }

    public void ComputeBindingDistance(SpherePack parent)
    {
        mBindingDistance = parent.GetRadius() - GetRadius();
        if (mBindingDistance <= 0)
            mBindingDistance = 0;
        else
            mBindingDistance *= mBindingDistance;
    }

    public void VisibilityTest(ref Frustum f, SpherePackCallback callback, Frustum.ViewState state)
    {
        if (state == Frustum.ViewState.PARTIAL)
        {
            state = f.ViewVolumeTest(ref mCenter, GetRadius());
            //#if DEMO
            if (state != Frustum.ViewState.OUTSIDE)
            {
                DefineO.DrawCircle(mCenter.x, mCenter.y, GetRadius(), 0x404040);
            }
            //#endif
        }

        if (HasSpherePackFlag(Flag.SUPERSPHERE))
        {


            if (state == Frustum.ViewState.OUTSIDE)
            {
                if (HasSpherePackFlag(Flag.HIDDEN)) return; // no state change
                ClearSpherePackFlag((int)(Flag.INSIDE | Flag.PARTIAL));
                SetSpherePackFlag(Flag.HIDDEN);
            }
            else
            {
                if (state == Frustum.ViewState.INSIDE)
                {
                    if (HasSpherePackFlag(Flag.INSIDE)) return; // no state change
                    ClearSpherePackFlag((int)(Flag.PARTIAL | Flag.HIDDEN));
                    SetSpherePackFlag(Flag.INSIDE);
                }
                else
                {
                    ClearSpherePackFlag((int)(Flag.HIDDEN | Flag.INSIDE));
                    SetSpherePackFlag(Flag.PARTIAL);
                }
            }

            SpherePack pack = mChildren;

            while (null != pack)
            {
                pack.VisibilityTest(ref f, callback, state);
                pack = pack.GetNextSibling();
            }

        }
        else
        {
            switch (state)
            {
                case Frustum.ViewState.INSIDE:
                    if (!HasSpherePackFlag(Flag.INSIDE))
                    {
                        ClearSpherePackFlag((int)(Flag.HIDDEN | Flag.PARTIAL));
                        SetSpherePackFlag(Flag.INSIDE);
                        callback.VisibilityCallback(f, this, state);
                    }
                    break;
                case Frustum.ViewState.OUTSIDE:
                    if (!HasSpherePackFlag(Flag.HIDDEN))
                    {
                        ClearSpherePackFlag((int)(Flag.INSIDE | Flag.PARTIAL));
                        SetSpherePackFlag(Flag.HIDDEN);
                        callback.VisibilityCallback(f, this, state);
                    }
                    break;
                case Frustum.ViewState.PARTIAL:
                    if (!HasSpherePackFlag(Flag.PARTIAL))
                    {
                        ClearSpherePackFlag((int)(Flag.INSIDE | Flag.HIDDEN));
                        SetSpherePackFlag(Flag.PARTIAL);
                        callback.VisibilityCallback(f, this, state);
                    }
                    break;
            }

        }
    }

    //p1: origin of Ray
    //dir1: direction of Ray
    //distance: length of ray.
    public void RayTrace(ref Vector3 p1, ref Vector3 dir, float distance, SpherePackCallback callback)
    {
        bool hit = false;
        Vector3 sect;

        if (HasSpherePackFlag(Flag.SUPERSPHERE))
        {

            hit = RayIntersectionInFront(ref p1, ref dir, out sect);
            if (hit)
            {
                //#if DEMO
                DefineO.DrawCircle(mCenter.x, mCenter.y, GetRadius(), 0x404040);
                //#endif
                SpherePack pack = mChildren;

                while (null != pack)
                {
                    pack.RayTrace(ref p1, ref dir, distance, callback);
                    pack = pack.GetNextSibling();
                }
            }

        }
        else
        {

            hit = RayIntersection(ref p1, ref dir, distance, out sect);
            if (hit)
            {
                callback.RayTraceCallback(ref p1, ref dir, distance, ref sect, this);
            }
        }
    }


    public void RangeTest(ref Vector3 p, float distance, SpherePackCallback callback, Frustum.ViewState state)
    {
        if (state == Frustum.ViewState.PARTIAL)
        {
            
            //float d = p.Distance(mCenter);
            float d = (p - mCenter).magnitude;
            if ((d - distance) > GetRadius()) return;
            if ((GetRadius() + d) < distance) state = Frustum.ViewState.INSIDE;
        }

        if (HasSpherePackFlag(Flag.SUPERSPHERE))
        {
            //#if DEMO
            if (state == Frustum.ViewState.PARTIAL)
            {
                DefineO.DrawCircle(mCenter.x, mCenter.y, GetRadius(), 0x404040);
            }
            //#endif
            SpherePack pack = mChildren;
            while (null != pack)
            {
                pack.RangeTest(ref p, distance, callback, state);
                pack = pack.GetNextSibling();
            }

        }
        else
        {
            callback.RangeTestCallback(ref p, distance, this, state);
        }
    }

    public void ResetFlag()
    {
        ClearSpherePackFlag((int)(Flag.HIDDEN | Flag.PARTIAL | Flag.INSIDE));

        SpherePack pack = mChildren;
        while (null != pack)
        {
            pack.ResetFlag();
            pack = pack.GetNextSibling();
        }
    }




    //========================================================
    //==================    테스트 출력 함수    ==================
    //========================================================

    //#if DEMO
    public void SetColor_Debug(uint color) { mColor = color; }
    public uint GetColor_Debug() { return mColor; }
    //#endif

    //inline
    public void Render_Debug(uint color)
    {
        //#if DEMO
        if (!HasSpherePackFlag(Flag.ROOTNODE))
        {

            if (HasSpherePackFlag(Flag.SUPERSPHERE))
            {
                color = mColor;
            }
            else
            {
                if (mParent.HasSpherePackFlag(Flag.ROOTNODE)) color = 0x00FFFFFF;
            }
            //#if DEMO
            DefineO.DrawCircle(mCenter.x, mCenter.y, GetRadius(), color);
            //#endif

            if (HasSpherePackFlag(Flag.SUPERSPHERE))
            {
                if (HasSpherePackFlag(Flag.LEAF_TREE))
                {

                    //#if DEMO
                    DefineO.DrawCircle(mCenter.x, mCenter.y, GetRadius(), color);
                    //#endif
                    SpherePack link = GetUserData<SpherePack>();

                    if (null != link)
                        link = link.GetParent();

                    if (null != link && !link.HasSpherePackFlag(Flag.ROOTNODE))
                    {
                        DefineO.DrawLine(mCenter.x, mCenter.y,
                                  link.mCenter.x, link.mCenter.y,
                                  link.GetColor_Debug());
                    }
                }
                else
                {
                    //#if DEMO
                    DefineO.DrawCircle(mCenter.x, mCenter.y, GetRadius() + 3, color);
                    DefineO.PrintText(mCenter.x, mCenter.y, 0x00FFFFFF, ((Flag)mFlags).ToString()); //chamto test
                    //#endif
                }

            }

        }

        if (null != mChildren)
        {
            SpherePack pack = mChildren;

            while (null != pack)
            {
                pack.Render_Debug(color);
                pack = pack.GetNextSibling();
            }
        }
        //#endif
    }


    //#endif
}