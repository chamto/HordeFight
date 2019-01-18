using UnityEngine;

public class Frustum
{
    private int mX1;
    private int mX2;
    private int mY1;
    private int mY2;


    public void Set(int x1, int y1, int x2, int y2)
    {
        mX1 = x1;
        mY1 = y1;
        mX2 = x2;
        mY2 = y2;
    }

    public DefineO.ViewState ViewVolumeTest(ref Vector3 center, float radius)
    {
        int acode = 0xFFFFFF;
        int ocode = 0;

        int x = (int)(center.x);
        int y = (int)(center.y);
        int r = (int)(radius);

        ViewCode(x-r, y-r, ref acode, ref ocode);
        ViewCode(x+r, y-r, ref acode, ref ocode);
        ViewCode(x+r, y+r, ref acode, ref ocode);
        ViewCode(x-r, y+r, ref acode, ref ocode);

        if (0 != acode ) return DefineO.ViewState.VS_OUTSIDE;
        if (0 != ocode ) return DefineO.ViewState.VS_PARTIAL;

        return DefineO.ViewState.VS_INSIDE;
    }

    private void ViewCode(int x, int y, ref int acode, ref int ocode)
    {
        int code = 0;
        if (x < mX1) code |= (int)DefineO.CohenSutherland.CS_LEFT;
        if (x > mX2) code |= (int)DefineO.CohenSutherland.CS_RIGHT;
        if (y < mY1) code |= (int)DefineO.CohenSutherland.CS_TOP;
        if (y > mY2) code |= (int)DefineO.CohenSutherland.CS_BOTTOM;
        ocode |= code;
        acode &= code;
    }
}