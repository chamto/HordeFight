using UnityEngine;

namespace ST_Test_006
{ 
    public class Frustum
    {
        public enum ViewState
        {
            INSIDE,   // completely inside the frustum.
            PARTIAL,  // partially inside and partially outside the frustum.
            OUTSIDE   // completely outside the frustum
        }

        //코헨 서덜랜드 알고리즘
        //ref : https://en.wikipedia.org/wiki/Cohen%E2%80%93Sutherland_algorithm
        public enum CohenSutherland
        {
            CS_LEFT = (1 << 0),
            CS_RIGHT = (1 << 1),
            CS_TOP = (1 << 2),
            CS_BOTTOM = (1 << 3)
        }

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

        public ViewState ViewVolumeTest(ref Vector3 center, float radius)
        {
            int acode = 0xFFFFFF;
            int ocode = 0;

            int x = (int)(center.x);
            int y = (int)(center.y);
            int r = (int)(radius);

            ViewCode(x - r, y - r, ref acode, ref ocode);
            ViewCode(x + r, y - r, ref acode, ref ocode);
            ViewCode(x + r, y + r, ref acode, ref ocode);
            ViewCode(x - r, y + r, ref acode, ref ocode);

            if (0 != acode) return ViewState.OUTSIDE;
            if (0 != ocode) return ViewState.PARTIAL;

            return ViewState.INSIDE;
        }

        private void ViewCode(int x, int y, ref int acode, ref int ocode)
        {
            int code = 0;
            if (x < mX1) code |= (int)CohenSutherland.CS_LEFT;
            if (x > mX2) code |= (int)CohenSutherland.CS_RIGHT;
            if (y < mY1) code |= (int)CohenSutherland.CS_TOP;
            if (y > mY2) code |= (int)CohenSutherland.CS_BOTTOM;
            ocode |= code;
            acode &= code;
        }
    }
}
