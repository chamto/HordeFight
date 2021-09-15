
using System;
using UnityEngine;
using UtilGS9;

namespace ST_Test_003
{
    [FlagsAttribute]
    public enum eRender_Tree
    {
        NONE = 0,
        ROOT = 1 << 0,
        LEAF = 1 << 1,
        TEXT = 1 << 2,
        ALL1 = ROOT + LEAF,
        ALL2 = ROOT + LEAF + TEXT,
    }
    public class CircleFactory : SpherePackCallback
    {
        public const int MAX_ATTRACTORS = 16;

        private Circle.State mState;
        private int mHitCount;

        private Circle[] mCircles = null;
        private SpherePackFactory mFactory = null;
        private Attractor[] mAttractors = null;

        private Pool<CircleItem> mVisible = null;
        private int mCircleCount;

        private eRender_Tree _render_tree = eRender_Tree.ALL1;

        public CircleFactory(int circlecount)
        {
            mState = Circle.State.SHOW_ALL;
            mCircleCount = circlecount;
            mCircles = new Circle[mCircleCount];
            mAttractors = new Attractor[MAX_ATTRACTORS];
            mVisible = new Pool<CircleItem>();

            //==============================
            //   초기화 
            //==============================
            mVisible.Init(mCircleCount);

            //포인트배열에 들어가는 객체 생성
            for (int i = 0; i < MAX_ATTRACTORS; i++)
            {
                mAttractors[i] = new Attractor();
            }


            //rootsize, leafsize, gravy
            mFactory = new SpherePackFactory(mCircleCount, 256, 64, 8);//256, 64, 8);

            for (int i = 0; i != mCircleCount; i++)
            {

                int a = Misc.rand.Next() % MAX_ATTRACTORS; // [ 0 ~ 15 ]

                Attractor at = mAttractors[a];


                if ((i & 3) == 0) at = null; // 1 of 4are non moving. 
                                             //if ((i % 4) == 0) at = null; DebugWide.LogRed((i % 4)); //위와 같은 표현

                //비트에 대한 AND연산 결과
                //011(3),0111(7) 같은 밑에서 부터 채워진 비트 : 3 => 0 1 2 3 0 1 2 3 ...   , 7 => 0 ~ 7 0 ~ 7 .... 
                //100(4), 010(2) 같은 하나의 비트 : 4 => 0 0 0 0 4 4 4 4 .... , 2 => 0 0 2 2 0 0 2 2 .... 
                //int rBit = 3; 
                //DebugWide.LogBlue(i + " & " + rBit + " = " + (i & rBit) + "  ----  " + "bit: " + Misc.IntToBinaryString(i) + " & "+Misc.IntToBinaryString(rBit) + "   =  " + Misc.IntToBinaryString(i & rBit)); //chamto test


                Vector3 pos = new Vector3(Misc.rand.Next() % DefineO.SW_ID, Misc.rand.Next() % DefineO.SH_IT, 0);
                float radius = (Misc.rand.Next() % 4) + 1;
                SpherePack spherePack = mFactory.AddSphere(pos, radius, SpherePack.Flag.LEAF_TREE);
                mCircles[i] = new Circle(pos, radius, spherePack, at);
                spherePack.SetData_LeafTree<Circle>(mCircles[i]);

            }
        }


        public void SetRenderMode(eRender_Tree opt) { _render_tree = opt; }


        public void SetState(Circle.State s) { mState = s; }

        private int __gCount = 0;
        //private Circle.State __gLastState = Circle.State.SHOW_ALL;
        public int Process()
        {
            if (true)
            {
                //static gCount = 0;
                __gCount++;
                if (__gCount == 128)
                {
                    __gCount = 0;
                    DefineO.G_CENTER_X = Misc.rand.Next() % DefineO.SW_ID;
                    DefineO.G_CENTER_Y = Misc.rand.Next() % DefineO.SH_IT;
                }
            }

            if (true)
            {
                for (int i = 0; i < MAX_ATTRACTORS; i++) mAttractors[i].ResetTest();
            }

            // Perform 'physics' on all circles.
            for (int i = 0; i != mCircleCount; i++)
            {
                mCircles[i].Process(); //spherePack 의 newPos 호출
            }

            Circle.State __gLastState = Circle.State.SHOW_ALL;

            if (mState != __gLastState)
            {
                // Frustum culling in this example presumes frame to frame coherency.
                // If we change modes we need to reset the visibility status of the
                // sphere tree to a null state.
                __gLastState = mState;
                mFactory.ResetFlag(); // reset visibility state
            }

            mFactory.Process(); // controls how much CPU to give up to reintegration and recomputation fifos

            return 0;
        }



        private int __fcount = 0;
        private Frustum __f = new Frustum();

        private int SF_x1 = 300;
        private int SF_y1 = 250;
        private int SF_x2 = 500;
        private int SF_y2 = 400;

        private int SR_x1 = 300;
        private int SR_y1 = 250;
        private int SR_x2 = 500;
        private int SR_y2 = 400;

        private int SRT_dx = 0;
        private int SRT_dy = 0;
        private int SRT_radius = 128;

        public int Render()
        {

            __fcount++;

            uint color1 = (uint)0xFFFFFF;
            uint color2 = (uint)0xFFFFFF;
            uint color3 = (uint)0xFFFFFF;
            uint color4 = (uint)0xFFFFFF;

            switch (mState)
            {
                case Circle.State.SHOW_ALL:
                    mFactory.Render_Debug(_render_tree);
                    color1 = (uint)0x00FFFF;
                    break;
                case Circle.State.SHOW_FRUSTUM:
                    color2 = (uint)0x00FFFF;
                    if (true)
                    {
                        DefineO.PrintText(0, 12, 0xFFFFFF, "Show Spheres Intersecting A Frustum");

                        // static int x1 = 300;
                        // static int y1 = 250;
                        // static int x2 = 500;
                        // static int y2 = 400;


                        if ((__fcount & 255) == 0)
                        {
                            SF_x1 = Misc.rand.Next() % (DefineO.SCREEN_WIDTH - 200);
                            SF_y1 = Misc.rand.Next() % (DefineO.SCREEN_HEIGHT - 200);

                            int wid = Misc.rand.Next() % ((DefineO.SCREEN_WIDTH) / 2 - SF_x1) + 32;
                            int hit = Misc.rand.Next() % ((DefineO.SCREEN_HEIGHT) / 2 - SF_y1) + 32;

                            SF_x2 = SF_x1 + wid;
                            SF_y2 = SF_y1 + hit;
                            if (SF_x2 > (DefineO.SCREEN_WIDTH - 20)) SF_x2 = DefineO.SCREEN_WIDTH - 20;
                            if (SF_y2 > (DefineO.SCREEN_HEIGHT - 20)) SF_y2 = DefineO.SCREEN_HEIGHT - 20;

                        }

                        //Frustum f;

                        __f.Set(SF_x1, SF_y1, SF_x2, SF_y2);

                        DefineO.DrawLine(SF_x1, SF_y1, SF_x2, SF_y1, (uint)0xFFFFFF);
                        DefineO.DrawLine(SF_x1, SF_y2, SF_x2, SF_y2, (uint)0xFFFFFF);
                        DefineO.DrawLine(SF_x1, SF_y1, SF_x1, SF_y2, (uint)0xFFFFFF);
                        DefineO.DrawLine(SF_x2, SF_y1, SF_x2, SF_y2, (uint)0xFFFFFF);

                        this.FrustumTest(__f);

                        // now render everybody in the visible list.
                        int count = mVisible.Begin();

                        for (int i = 0; i < count; i++)
                        {
                            CircleItem item = mVisible.GetNext();
                            if (null == item) break;
                            Circle circle = item.GetCircle();
                            SpherePack sphere = circle.GetSpherePack();
                            Vector3 pos = sphere.GetCenter();
                            int color = 0x00FF00;
                            if (circle.GetViewState() == Frustum.ViewState.PARTIAL) color = 0x00FFFF;
                            DefineO.DrawCircle(pos.x, pos.y, sphere.GetRadius(), (uint)color);
                        }

                        DefineO.PrintText(0f, 24f, 0xFFFFFF, count + " Spheres Intersected Frustum.");

                    }
                    break;
                case Circle.State.SHOW_RAYTRACE:
                    color3 = 0x00FFFF;
                    if (true)
                    {
                        DefineO.PrintText(0, 12, 0xFFFFFF, "Show Spheres Intersecting A Line Segment");

                        //static int x1 = 300;
                        //static int y1 = 250;
                        //static int x2 = 500;
                        //static int y2 = 400;

                        if ((__fcount & 255) == 0)
                        {
                            SR_x1 = Misc.rand.Next() % DefineO.SCREEN_WIDTH;
                            SR_y1 = Misc.rand.Next() % DefineO.SCREEN_HEIGHT;
                            SR_x2 = Misc.rand.Next() % DefineO.SCREEN_WIDTH;
                            SR_y2 = Misc.rand.Next() % DefineO.SCREEN_HEIGHT;
                        }

                        RayTest(SR_x1, SR_y1, SR_x2, SR_y2);
                    }
                    break;
                case Circle.State.SHOW_RANGE_TEST:
                    color4 = 0x00FFFF;
                    if (true)
                    {
                        //static int dx = 0;
                        //static int dy = 0;
                        //static int radius = 128;

                        if ((__fcount & 255) == 0)
                        {
                            SRT_dx = (Misc.rand.Next() & 255) - 128;
                            SRT_dy = (Misc.rand.Next() & 255) - 128;
                            SRT_radius = (Misc.rand.Next() & 127) + 32;
                        }

                        DefineO.PrintText(0, 12, 0xFFFFFF, "Show Spheres Within A Certain Range");

                        RangeTest(DefineO.SCREEN_WIDTH / 2 + SRT_dx, DefineO.SCREEN_HEIGHT / 2 + SRT_dy, SRT_radius);

                    }
                    break;
            }


            DefineO.PrintText(900, 0, color1, "(A) Show All");
            DefineO.PrintText(900, 20, color2, "(F) Frustum Culling");
            DefineO.PrintText(900, 40, color3, "(T) Ray Tracing");
            DefineO.PrintText(900, 60, color4, "(R) Range Testing");
            DefineO.PrintText(900, 80, 0xFFFFFF, "(P) Pause");
            DefineO.PrintText(900, 100, 0xFFFFFF, "(1, 2, 3, 4) Render Tree Mode");


            return 0;
        }


        private void RayTest(int x1, int y1, int x2, int y2)
        {
            Vector3 p1 = new Vector3(x1, y1, 0);
            Vector3 p2 = new Vector3(x2, y2, 0);

            DefineO.DrawLine(x1, y1, x2, y2, 0xFFFFFF);

            mHitCount = 0;

            mFactory.RayTrace_Factory(ref p1, ref p2, this);

            DefineO.PrintText(0, 24, 0xFFFFFF, mHitCount + " Spheres Intersected Ray.");

        }

        private void RangeTest(int x1, int y1, int distance)
        {
            DefineO.DrawCircle(x1, y1, distance, 0x00FF);
            DefineO.DrawCircle(x1, y1, distance + 2, 0x00FF);
            DefineO.DrawCircle(x1, y1, distance + 4, 0x00FF);
            DefineO.DrawCircle(x1, y1, distance + 6, 0x00FF);

            mHitCount = 0;

            Vector3 p = new Vector3(x1, y1, 0);

            mFactory.RangeTest(ref p, distance, this);

            DefineO.PrintText(0, 24, 0xFFFFFF, mHitCount + " Spheres Inside Range.");
        }

        private void FrustumTest(Frustum f)
        {
            mFactory.FrustumTest(f, this);
        }


        //========================================================
        //==================    재정의/구현 함수    ==================
        //========================================================

        //p1: source pos of ray
        //dir: dest pos of ray (normalized)
        //distance: length of ray
        public override void RayTraceCallback(ref Vector3 p1, ref Vector3 dir, float distance, ref Vector3 sect, SpherePack sphere)
        {
            Vector3 pos = sphere.GetPos();

            float radius = sphere.GetRadius();

            DefineO.DrawCircle(pos.x, pos.y, radius, 0x00FF00);

            float sx = sect.x;
            float sy = sect.y;

            DefineO.DrawLine(sx - 10, sy, sx + 10, sy, 0x0000FF);
            DefineO.DrawLine(sx, sy - 10, sx, sy + 10, 0x0000FF);

            mHitCount++;

        }

        public override void VisibilityCallback(Frustum f, SpherePack sphere, Frustum.ViewState state)
        {
            Circle circle = sphere.GetData_LeafTree<Circle>();

            if (state == Frustum.ViewState.OUTSIDE) // it is not visible!
            {
                CircleItem item = circle.GetCircleItem();

                if (null != item)
                {
                    circle.SetCircleItem(null);
                    mVisible.Release(item);
                }
            }
            else
            {
                circle.SetViewState(state);

                CircleItem item = circle.GetCircleItem();

                if (null == item)
                {

                    item = mVisible.GetFreeLink();

                    if (null != item)
                    {
                        circle.SetCircleItem(item);
                        item.SetCircle(circle);
                    }

                }
            }
        }

        public override void RangeTestCallback(ref Vector3 searchpos, float distance, SpherePack sphere, Frustum.ViewState state)
        {
            Vector3 pos = sphere.GetPos();

            float radius = sphere.GetRadius();

            DefineO.DrawCircle(pos.x, pos.y, radius, 0x00FF00);


            mHitCount++;
        }

        //========================================================

        //======================================================================================
        // CircleFactory::~CircleFactory(void)
        // {
        //     for (int i = 0; i != mCircleCount; i++)
        //     {
        //         delete(mCircles[i]);
        //     }
        //   delete mFactory;
        // }
        //======================================================================================
    }
}

