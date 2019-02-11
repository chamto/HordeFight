using UnityEngine;


public class SpherePackFactory : SpherePackCallback
{

    private SpherePack mRoot = null;     // 1024x1024 root node of all active spheres.
    private SpherePack mLeaf = null;     // 1024x1024 root node of all active spheres.
    private SpherePackCallback mCircleFactory_Callback = null;

    private Pool<SpherePack> mSpheres = null;  // all spheres possibly represented.

    private SpherePackFifo mIntegrate = null; // integration fifo
    private SpherePackFifo mRecompute = null; // recomputation fifo

    //#if DEMO
    //#define MAXCOLORS 12
    const int MAXCOLORS = 12;
    private int mColorCount;
    private uint[] mColors = null;
    //#endif

    private float mMaxRootSize;              // maximum size of a root node supersphere
    private float mMaxLeafSize;              // maximum size of the leaf node supersphere
    private float mSuperSphereGravy;         // binding distance gravy. //여분의 양. 여분은 객체들이 부모로 부터 너무 자주 떨어지지 않도록 경계구의 크기를 넉넉하게 만드는 역할을 한다


    public SpherePackFactory(int maxspheres, float rootsize, float leafsize, float gravy)
    {
        maxspheres *= 4; // include room for both trees, the root node and leaf node tree, and the supersheres
        mMaxRootSize = rootsize;
        mMaxLeafSize = leafsize;
        mSuperSphereGravy = gravy;

        mIntegrate = new SpherePackFifo(maxspheres);
        mRecompute = new SpherePackFifo(maxspheres);
        mSpheres = new Pool<SpherePack>();
        mSpheres.Init(maxspheres);       // init pool to hold all possible SpherePack instances.

        Vector3 p = Vector3.zero;

        mRoot = mSpheres.GetFreeLink(); // initially empty
        mRoot.Init(this, p, 65536);
        mRoot.SetSpherePackFlag((int)(SpherePack.Flag.SUPERSPHERE | SpherePack.Flag.ROOTNODE | SpherePack.Flag.ROOT_TREE));

        //#if DEMO
        mRoot.SetColor_Debug(0x00FFFFFF);
        //#endif

        mLeaf = mSpheres.GetFreeLink();  // initially empty
        mLeaf.Init(this, p, 16384);
        mLeaf.SetSpherePackFlag((int)(SpherePack.Flag.SUPERSPHERE | SpherePack.Flag.ROOTNODE | SpherePack.Flag.LEAF_TREE));

        //#if DEMO
        mColors = new uint[MAXCOLORS];
        mLeaf.SetColor_Debug(0x00FFFFFF);
        mColorCount = 0;

        mColors[0] = 0x00FF0000;
        mColors[1] = 0x0000FF00;
        mColors[2] = 0x000000FF;
        mColors[3] = 0x00FFFF00;
        mColors[4] = 0x00FF00FF;
        mColors[5] = 0x0000FFFF;
        mColors[6] = 0x00FF8080;
        mColors[7] = 0x0000FF80;
        mColors[8] = 0x000080FF;
        mColors[9] = 0x00FFFF80;
        mColors[10] = 0x00FF80FF;
        mColors[11] = 0x0080FFFF;

        //#endif
    }

    // ~SpherePackFactory(void)
    // {
    //   delete mIntegrate;  // free up integration fifo
    //   delete mRecompute;  // free up recomputation fifo.
    // }


    public void Process()
    {
        if (true)
        {
            // First recompute anybody that needs to be recomputed!!
            // When leaf node spheres exit their parent sphere, then the parent sphere needs to be rebalanced.  In fact,it may now be empty and
            // need to be removed.
            // This is the location where (n) number of spheres in the recomputation FIFO are allowed to be rebalanced in the tree.
            int maxrecompute = mRecompute.GetCount();
            for (int i = 0; i < maxrecompute; i++)
            {
                SpherePack pack = mRecompute.Pop();
                if (null == pack) break;
                //pack.SetFifo1(null); // no longer on the fifo!!
                pack.InitFifo1();
                bool kill = pack.Recompute(mSuperSphereGravy);
                if (kill) Remove(pack);
            }
        }

        if (true)
        {
            // Now, process the integration step.

            int maxintegrate = mIntegrate.GetCount();

            for (int i = 0; i < maxintegrate; i++)
            {
                SpherePack pack = mIntegrate.Pop();
                if (null == pack) break;
                //pack.SetFifo2(null);
                pack.InitFifo2();

                if (pack.HasSpherePackFlag(SpherePack.Flag.ROOT_TREE))
                    Integrate(pack, mRoot, mMaxRootSize); // integrate this one single dude against the root node.
                else
                    Integrate(pack, mLeaf, mMaxLeafSize); // integrate this one single dude against the root node.
            }
        }

    }



    public SpherePack AddSphere<USERDATA>(Vector3 pos, float radius, SpherePack.Flag flag) where USERDATA : IUserData
    {

        SpherePack pack = mSpheres.GetFreeLink();

        //assert(pack);

        if (null != pack)
        {

            pack.Init(this, pos, radius); //SetSpherePackFlag 함수 보다 먼저 호출되어야 한다. mFlag 정보를 초기화 하기 때문이다. 

            //if (SpherePack.Flag.NONE != (flags & SpherePack.Flag.ROOT_TREE)) //루트트리가 들어 있다면 
            if (flag == SpherePack.Flag.ROOT_TREE) //루트트리 라면 
            {
                pack.SetSpherePackFlag(SpherePack.Flag.ROOT_TREE); // member of the leaf node tree!
            }
            else
            {
                pack.SetSpherePackFlag(SpherePack.Flag.LEAF_TREE); // member of the leaf node tree!
            }

            AddIntegrate(pack); // add to integration list.
        }

        return pack;
    }

    public void AddIntegrate(SpherePack pack)          // add to the integration FIFO
    {

        if (pack.HasSpherePackFlag(SpherePack.Flag.ROOT_TREE))
            mRoot.AddChild(pack);
        else
            mLeaf.AddChild(pack);

        pack.SetSpherePackFlag(SpherePack.Flag.INTEGRATE); // still needs to be integrated!
        SpherePackFifo.Out_Push fifo = mIntegrate.Push(pack); // add it to the integration stack.
        pack.SetFifoOut2(fifo);
    }

    public void AddRecompute(SpherePack recompute)     // add to the recomputation (balancing) FIFO.
    {
        if (!recompute.HasSpherePackFlag(SpherePack.Flag.RECOMPUTE))
        {
            if (0 != recompute.GetChildCount())
            {
                recompute.SetSpherePackFlag(SpherePack.Flag.RECOMPUTE); // needs to be recalculated!
                SpherePackFifo.Out_Push fifo = mRecompute.Push(recompute);
                recompute.SetFifoOut1(fifo);
            }
            else
            {
                Remove(recompute);
            }
        }
    }

    public void Integrate(SpherePack src_pack, SpherePack supersphere, float node_size)
    {
        // ok..time to integrate this sphere with the tree
        // first find which supersphere we are closest to the center of

        SpherePack search = supersphere.GetChildren();

        SpherePack nearest1 = null;  // nearest supersphere we are completely
        float nearSqrDist1 = 1e9f;     // enclosed within. 10의9승. 1000000000.0

        SpherePack nearest2 = null; // supersphere we must grow the least to
        float nearDist2 = 1e9f;    // add ourselves to.

        //int scount = 1;

        while (null != search)
        {
            if (search.HasSpherePackFlag(SpherePack.Flag.SUPERSPHERE) &&
                !search.HasSpherePackFlag(SpherePack.Flag.ROOTNODE) && 0 != search.GetChildCount())
            {

                float sqrDist = src_pack.ToDistanceSquared(search);

                if (null != nearest1)
                {
                    if (sqrDist < nearSqrDist1)
                    {

                        float dist = Mathf.Sqrt(sqrDist) + src_pack.GetRadius();

                        if (dist <= search.GetRadius())
                        {
                            nearSqrDist1 = sqrDist;
                            nearest1 = search;
                        }
                    }
                }
                else
                {

                    float dist = (Mathf.Sqrt(sqrDist) + src_pack.GetRadius()) - search.GetRadius();

                    if (dist < nearDist2)
                    {
                        if (dist < 0)
                        {
                            nearSqrDist1 = sqrDist;
                            nearest1 = search;
                        }
                        else
                        {
                            nearDist2 = dist;
                            nearest2 = search;
                        }
                    }
                }
            }
            search = search.GetNextSibling();
        }

        // ok...now..on exit let's see what we got.
        if (null != nearest1)
        {
            // if we are inside an existing supersphere, we are all good!
            // we need to detach item from wherever it is, and then add it to
            // this supersphere as a child.
            src_pack.Unlink();
            nearest1.AddChild(src_pack);
            src_pack.ComputeBindingDistance(nearest1);
            nearest1.Recompute(mSuperSphereGravy);

            if (nearest1.HasSpherePackFlag(SpherePack.Flag.LEAF_TREE))
            {
                SpherePack link = nearest1.GetUserData<SpherePack>();
                link.NewPosRadius(nearest1.GetPos(), nearest1.GetRadius());
            }

        }
        else
        {
            bool newsphere = true;

            if (null != nearest2)
            {
                float newsize = nearDist2 + nearest2.GetRadius() + mSuperSphereGravy;

                if (newsize <= node_size)
                {
                    src_pack.Unlink();

                    nearest2.SetRadius(newsize);
                    nearest2.AddChild(src_pack);
                    nearest2.Recompute(mSuperSphereGravy);
                    src_pack.ComputeBindingDistance(nearest2);

                    if (nearest2.HasSpherePackFlag(SpherePack.Flag.LEAF_TREE))
                    {
                        SpherePack link = nearest2.GetUserData<SpherePack>();
                        link.NewPosRadius(nearest2.GetPos(), nearest2.GetRadius());
                    }

                    newsphere = false;

                }

            }

            if (newsphere)
            {
                //assert(supersphere->HasSpherePackFlag(SPF_ROOTNODE));
                // we are going to create a new superesphere around this guy!
                src_pack.Unlink();

                SpherePack parent = mSpheres.GetFreeLink();
                //assert(parent);
                parent.Init(this, src_pack.GetPos(), src_pack.GetRadius() + mSuperSphereGravy);

                if (supersphere.HasSpherePackFlag(SpherePack.Flag.ROOT_TREE))
                    parent.SetSpherePackFlag(SpherePack.Flag.ROOT_TREE);
                else
                    parent.SetSpherePackFlag(SpherePack.Flag.LEAF_TREE);

                parent.SetSpherePackFlag(SpherePack.Flag.SUPERSPHERE);
                //#if DEMO
                parent.SetColor_Debug(GetColor_Debug());
                //#endif
                parent.AddChild(src_pack);

                supersphere.AddChild(parent);

                parent.Recompute(mSuperSphereGravy);
                src_pack.ComputeBindingDistance(parent);

                if (parent.HasSpherePackFlag(SpherePack.Flag.LEAF_TREE))
                {
                    // need to create parent association!
                    SpherePack link = AddSphere<SpherePack>(parent.GetPos(), parent.GetRadius(), SpherePack.Flag.ROOT_TREE);
                    link.SetUserData<SpherePack>(parent);
                    parent.SetUserData<SpherePack>(link); // hook him up!!
                }

            }
        }

        src_pack.ClearSpherePackFlag((int)SpherePack.Flag.INTEGRATE); // we've been integrated!
    }


    public void Remove(SpherePack pack)
    {

        if (pack.HasSpherePackFlag(SpherePack.Flag.ROOTNODE)) return; // CAN NEVER REMOVE THE ROOT NODE EVER!!!

        if (pack.HasSpherePackFlag(SpherePack.Flag.SUPERSPHERE) && pack.HasSpherePackFlag(SpherePack.Flag.LEAF_TREE))
        {

            SpherePack link = pack.GetUserData<SpherePack>();

            Remove(link);
        }

        pack.Unlink();

        mSpheres.Release(pack);
    }

    public void ResetFlag()
    {
        mRoot.ResetFlag();
        mLeaf.ResetFlag();
    }

    public void FrustumTest(Frustum f, SpherePackCallback circleFactory_callback)
    {
        // test case here, just traverse children.
        mCircleFactory_Callback = circleFactory_callback;
        mRoot.VisibilityTest(ref f, this, Frustum.ViewState.PARTIAL);
    }

    //p1: source
    //p2: dest
    public void RayTrace(ref Vector3 p1, ref Vector3 p2, SpherePackCallback circleFactory_callback)
    {
        // test case here, just traverse children.
        Vector3 dir = p2 - p1;
        float dist = dir.magnitude;
        dir.Normalize();
        mCircleFactory_Callback = circleFactory_callback;
        mRoot.RayTrace(ref p1, ref dir, dist, this);
    }


    public void RangeTest(ref Vector3 center, float radius, SpherePackCallback circleFactory_callback)
    {
        mCircleFactory_Callback = circleFactory_callback;
        mRoot.RangeTest(ref center, radius, this, Frustum.ViewState.PARTIAL);
    }


    //========================================================
    //==================    재정의/구현 함수    ==================
    //========================================================

    //p1: source pos of ray
    //dir: direction of ray
    //distance: distance of ray
    //sect: intersection location
    public override void RayTraceCallback(ref Vector3 p1, ref Vector3 dir, float distance, ref Vector3 sect, SpherePack sphere)
    {
        SpherePack link = sphere.GetUserData<SpherePack>();
        if (null != link) link.RayTrace(ref p1, ref dir, distance, mCircleFactory_Callback);
    }

    public override void RangeTestCallback(ref Vector3 searchpos, float distance, SpherePack sphere, Frustum.ViewState state)
    {
        SpherePack link = sphere.GetUserData<SpherePack>();
        if (null != link) link.RangeTest(ref searchpos, distance, mCircleFactory_Callback, state);
    }


    public override void VisibilityCallback(Frustum f, SpherePack sphere, Frustum.ViewState state)
    {
        SpherePack link = sphere.GetUserData<SpherePack>();
        if (null != link) link.VisibilityTest(ref f, mCircleFactory_Callback, state);
    }


    //========================================================
    //==================    테스트 출력 함수    ==================
    //========================================================
    public void Render_Debug()
    {
        //#if DEMO
        mRoot.Render_Debug(mRoot.GetColor_Debug());
        mLeaf.Render_Debug(mLeaf.GetColor_Debug());
        //#endif
    }

    // see if any other spheres are contained within this one, if so
    // collapse them and inherit their children.
    //#if DEMO
    public uint GetColor_Debug()
    {
        uint ret = mColors[mColorCount];
        mColorCount++;
        if (mColorCount == MAXCOLORS) mColorCount = 0;
        return ret;
    }
    //#endif



}