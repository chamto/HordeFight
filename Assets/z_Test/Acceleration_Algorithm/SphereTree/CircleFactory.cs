using UnityEngine;
using System.Collections;
using UtilGS9;

public class DefineO
{

    public static int SCREEN_WIDTH = 1024;
    public static int SCREEN_HEIGHT = 768;
    public static int MAX_ATTRACTORS = 16;

    public static int JUMP_TIME = 128;
    public static int FIXED = 16;
    public static int SWID = (SCREEN_WIDTH * FIXED);
    public static int SHIT = (SCREEN_HEIGHT * FIXED);

    public static int gCenterX = SWID / 2;
    public static int gCenterY = SHIT / 2;



    public enum SpherePackFlag
    {

        SPF_SUPERSPHERE = (1 << 0), // this is a supersphere, allocated and deleted by us
        SPF_ROOT_TREE = (1 << 1), // member of the root tree
        SPF_LEAF_TREE = (1 << 2), // member of the leaf node tree
        SPF_ROOTNODE = (1 << 3), // this is the root node
        SPF_RECOMPUTE = (1 << 4), // needs recomputed bounding sphere
        SPF_INTEGRATE = (1 << 5), // needs to be reintegrated into tree
                                  // Frame-to-frame view frustum status.  Only does callbacks when a
                                  // state change occurs.
        SPF_HIDDEN = (1 << 6), // outside of view frustum
        SPF_PARTIAL = (1 << 7), // partially inside view frustum
        SPF_INSIDE = (1 << 8)  // completely inside view frustum
    }

    public enum ViewState
    {
        VS_INSIDE,   // completely inside the frustum.
        VS_PARTIAL,  // partially inside and partially outside the frustum.
        VS_OUTSIDE   // completely outside the frustum
    }


    public enum CohenSutherland
    {
        CS_LEFT = (1 << 0),
        CS_RIGHT = (1 << 1),
        CS_TOP = (1 << 2),
        CS_BOTTOM = (1 << 3)
    }


    //ref : https://answers.unity.com/questions/1161444/convert-int-to-color.html
    public static Color32 HexToColor(uint aCol)
    {
        Color32 c = new Color32();
        c.b = (byte)((aCol) & 0xFF);
        c.g = (byte)((aCol >> 8) & 0xFF);
        c.r = (byte)((aCol >> 16) & 0xFF);
        c.a = (byte)((aCol >> 24) & 0xFF);
        return c;
    }

    static public Color Color32_ToColor(Color32 c32)
    {
        return new Color(c32.r / 255.0f, c32.g / 255.0f, c32.b / 255.0f, 1f);
    }

    static public void DrawLine(float x1, float y1, float x2, float y2, uint color)
    {
        Color cc = DefineO.Color32_ToColor(DefineO.HexToColor(color));
        //DebugWide.LogBlue(cc);
        //cc.a = 1f;


        Gizmos.color = cc;
        Gizmos.DrawLine(new Vector3(x1, y1, 0), new Vector3(x2, y2, 0));
    }

    static public void DrawCircle(float locx, float locy, float radius, uint color)
    {
        Color cc = DefineO.Color32_ToColor(DefineO.HexToColor(color));
        //DebugWide.LogBlue(cc + "    " + DefineO.HexToColor(color) + "    " + color);
        //cc.a = 1f;

        //UnityEditor.Handles.color = Color.red;
        Gizmos.color = cc;
        Gizmos.DrawWireSphere(new Vector3(locx, locy, 0), radius);
    }

    static public void PrintText(float x, float y, uint color, string text)
    {
        UnityEditor.Handles.color = DefineO.Color32_ToColor(DefineO.HexToColor(color));

        UnityEditor.Handles.Label(new Vector3(x, y, 0), text);
    }
}



public class CircleFactory : SpherePackCallback
{

    private Circle.CircleState mState;
    private int mHitCount;

    private Circle[] mCircles = null;

    private SpherePackFactory mFactory = null;
    private Attractor[] mAttractors = null;
    private Pool<CircleItem> mVisible = null;
    private int mCircleCount;



    public CircleFactory(int circlecount)
    {
        mState = Circle.CircleState.CS_SHOW_ALL;
        mCircleCount = circlecount;
        mCircles = new Circle[mCircleCount];
        mAttractors = new Attractor[DefineO.MAX_ATTRACTORS];

        //포인트배열에 들어가는 객체 생성
        for (int i = 0; i < DefineO.MAX_ATTRACTORS; i++)
        {
            mAttractors[i] = new Attractor();
        }

        mVisible = new Pool<CircleItem>();
        mVisible.Set(mCircleCount); // visiblelist

        mFactory = new SpherePackFactory(mCircleCount, 256, 64, 8);

        for (int i = 0; i != mCircleCount; i++)
        {

            int a = Misc.rand.Next() % DefineO.MAX_ATTRACTORS;

            Attractor at = mAttractors[a];
            //todo : 아래 구문 0~100 까지 비트연산하여 출력해보기 - 정확한 의도를 모르겠음 
            if ((i & 3) == 0) at = null; // 1 of 4 are non moving. 

            mCircles[i] = new Circle(Misc.rand.Next() % DefineO.SWID,
                                     Misc.rand.Next() % DefineO.SHIT,
                                     (Misc.rand.Next() % 4) + 1,
                                     mFactory,
                                 at);
        }
    }

    //p1: source pos of ray
    //dir: dest pos of ray (normalized)
    //distance: length of ray
    public virtual void RayTraceCallback(ref Vector3 p1, ref Vector3 dir, float distance, ref Vector3 sect, SpherePack sphere)
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

    public virtual void VisibilityCallback(Frustum f, SpherePack sphere, DefineO.ViewState state)
    {
        Circle circle = sphere.GetUserData<Circle>();

        if (state == DefineO.ViewState.VS_OUTSIDE) // it is not visible!
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

    public void RangeTestCallback(ref Vector3 p, float distance, SpherePack sphere, DefineO.ViewState state)
    {
        Vector3 pos = sphere.GetPos();

        float radius = sphere.GetRadius();

        DefineO.DrawCircle(pos.x, pos.y, radius, 0x00FF00);


        mHitCount++;
    }


    public void SetState(Circle.CircleState s) { mState = s; }

    private int __gCount = 0;
    public int Process()
    {
        if (true)
        {
            //static gCount = 0;
            __gCount++;
            if (__gCount == 128)
            {
                __gCount = 0;
                DefineO.gCenterX = Misc.rand.Next() % DefineO.SWID;
                DefineO.gCenterY = Misc.rand.Next() % DefineO.SHIT;
            }
        }

        if (true)
        {
            for (int i = 0; i < DefineO.MAX_ATTRACTORS; i++) mAttractors[i].ResetTest();
        }

        // Perform 'physics' on all circles.
        for (int i = 0; i != mCircleCount; i++)
        {
            mCircles[i].Process();
        }

        Circle.CircleState gLastState = Circle.CircleState.CS_SHOW_ALL;

        if (mState != gLastState)
        {
            // Frustum culling in this example presumes frame to frame coherency.
            // If we change modes we need to reset the visibility status of the
            // sphere tree to a null state.
            gLastState = mState;
            mFactory.Reset(); // reset visibility state
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
        //static int fcount = 0;

        __fcount++;

        uint color1 = (uint)0xFFFFFF;
        uint color2 = (uint)0xFFFFFF;
        uint color3 = (uint)0xFFFFFF;
        uint color4 = (uint)0xFFFFFF;

        switch (mState)
        {
            case Circle.CircleState.CS_SHOW_ALL:
                mFactory.Render();
                color1 = (uint)0x00FFFF;
                break;
            case Circle.CircleState.CS_SHOW_FRUSTUM:
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
                        if (circle.GetViewState() == DefineO.ViewState.VS_PARTIAL) color = 0x00FFFF;
                        DefineO.DrawCircle(pos.x, pos.y, sphere.GetRadius(), (uint)color);
                    }

                    DefineO.PrintText(0f, 24f, 0xFFFFFF, count + " Spheres Intersected Frustum.");

                }
                break;
            case Circle.CircleState.CS_SHOW_RAYTRACE:
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
            case Circle.CircleState.CS_SHOW_RANGE_TEST:
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
        DefineO.PrintText(900, 80, 0xFFFFFF, "(SPACE) Pause");
        DefineO.PrintText(900, 100, 0xFFFFFF, "(ENTER) UnPause");

        return 0;
    }


    private void RayTest(int x1, int y1, int x2, int y2)
    {
        Vector3 p1 = new Vector3(x1, y1, 0);
        Vector3 p2 = new Vector3(x2, y2, 0);

        DefineO.DrawLine(x1, y1, x2, y2, 0xFFFFFF);

        mHitCount = 0;

        mFactory.RayTrace(ref p1, ref p2, this);

        DefineO.PrintText(0, 24, 0xFFFFFF, mHitCount+" Spheres Intersected Ray.");

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

        DefineO.PrintText(0, 24, 0xFFFFFF, mHitCount+" Spheres Inside Range.");
    }

    private void FrustumTest(Frustum f)
    {
        mFactory.FrustumTest(f, this);
    }




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