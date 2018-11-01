using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;

namespace Utility
{
    /// <summary>
    /// 데카르트좌표계 사분면을 8방향으로 나누어 숫자 지정
    /// </summary>
    public enum eDirection8 : int
    {
        none = 0,
        right = 1,
        rightUp = 2,
        up = 3,
        leftUp = 4,
        left = 5,
        leftDown = 6,
        down = 7,
        rightDown = 8,
        max,


    }
}



namespace Utility
{
    ///////////////////////////////////////////////////////////////////////
    /// <summary>
    /// frame skip 시 해당프레임의 deltaTime을 최소 프레임시간으로 설정한다.
    /// </summary>
    ///////////////////////////////////////////////////////////////////////
    public class FrameTime
    {
        public const float DELTA_FPS_30 = 1f / 30f; 
        public const float DELTA_FPS_60 = 1f / 60f; 

        //기본값을 30프레임으로 설정한다
        static private float _deltaTime_mix = DELTA_FPS_30;
        static private float _deltaTime_max = DELTA_FPS_30 * 2f;

        static public void SetFixedFrame_FPS_30()
        {
            FrameTime.SetFixedFrame_FPS(30);
        }

        static public void SetFixedFrame_FPS(int fps)
        {
            //참고 : http://prosto.tistory.com/79
            //유니티 프레임 고정 , VSyn을 꺼야 적용된다함 , ProjectSettings => Quality => VSynCount
            Application.targetFrameRate = fps; 

            _deltaTime_mix = 1f / (float)fps;
            _deltaTime_max = _deltaTime_mix * 2f; //최소시간의 2배 한다. 
        }

        static public float DeltaTime_Mix()
        {
            return _deltaTime_mix;
        }

        static public float DeltaTime_Max()
        {
            return _deltaTime_max;
        }

        static public float DeltaTime()
        {
            //전프레임이 허용프레임 시간의 최대치를 넘었다면 최소시간을 반환한다.
            if (Time.deltaTime > _deltaTime_max)
            {
                //DebugWide.LogBlue ("FrameControl - frameSkip detected !!! - DeltaTime : "+Time.deltaTime);//chamto test
                return _deltaTime_mix;
            }


            return Time.deltaTime;
        }

    }//end class

    ///////////////////////////////////////////////////////////////////////
    /// <summary>
    /// 화면 해상도 조정
    /// </summary>
    ///////////////////////////////////////////////////////////////////////
    public static class ResolutionController
    {

        public const float WIDTH_STANDARD = 1024;
        public const float HEIGHT_STANDARD = 600;
        public const float ASPECT_RATIO = WIDTH_STANDARD / HEIGHT_STANDARD;
        public const float REVERSE_ASPECT_RATIO = HEIGHT_STANDARD / WIDTH_STANDARD;


        static private void InitViewportRect(Canvas root, Camera mainCamera)
        {
            //ui canvas
            //뷰포트렉 크기 기준으로 해상도에 상관없이 크기조정 설정
            if (null != root)
            {
                CanvasScaler cans = root.GetComponent<CanvasScaler>();
                cans.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                cans.referenceResolution = new Vector2(WIDTH_STANDARD, HEIGHT_STANDARD);
                cans.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand; //이 모드를 사용해야 에디터와 디바이스상의 위치값이 일치한다
            }


            //viewport
            mainCamera.aspect = ASPECT_RATIO;
            Rect pr = mainCamera.pixelRect;
            pr.x = 0f;
            pr.y = 0f;
            pr.width = WIDTH_STANDARD;
            pr.height = HEIGHT_STANDARD;
            mainCamera.pixelRect = pr;

            //DebugWide.LogBlue (_camera.pixelRect);
        }


        static public void CalcViewportRect(Canvas root, Camera mainCamera)
        {
            //DebugWide.LogBlue ("CalcResolution");
            ResolutionController.InitViewportRect(root, mainCamera);

            //==================================
            int iHeight = (int)(Screen.width * REVERSE_ASPECT_RATIO);
            int iWidth = (int)(Screen.height * ASPECT_RATIO);
            Rect pr = mainCamera.pixelRect;
            //==================================

            float fScreenRate = (float)Screen.height / (float)Screen.width;
            if (fScreenRate > REVERSE_ASPECT_RATIO)
            { //기준해상도 비율에 비해 , 모바일 기기의 화면세로비율이 커졌거나, 가로비율이 작아진 경우

                //==================================
                pr.height = iHeight;
                pr.width = Screen.width;
                pr.y = (Screen.height - iHeight) * 0.5f; //화면중앙으로 이동시킴
                mainCamera.pixelRect = pr;
                //==================================
                //DebugWide.LogBlue (mainCamera.pixelRect + "  camera aspect:" + GetCameraAspect() + "  reverse:" + GetCameraReverseAspect() );

            }
            else if (fScreenRate < REVERSE_ASPECT_RATIO)
            { //기준해상도 비율에 비해 , 모바일 기기의 화면가로비율이 커졌거나, 세로비율이 작아진 경우

                //==================================
                pr.height = Screen.height;
                pr.width = iWidth;
                pr.x = (Screen.width - iWidth) * 0.5f; //화면중앙으로 이동시킴
                mainCamera.pixelRect = pr;
                //==================================
                //DebugWide.LogBlue (_camera.pixelRect + "  camera aspect:" + GetCameraAspect() + "  reverse:" + GetCameraReverseAspect() );
            }


        }//end func


        static public void CalcScreenResolution()
        {

            //* 지정된 비율로 해상도를 재조정한다.
            //디바이스 상에서는 실제 화면비율에 따라 늘어나 보이게 된다
            //이 처리로는 에디터와 디바이스 화면이 다르게 보이는 것을 잡을수 없다

            //standard : h/w = 0.6
            //h/w > 0.6 : 기준보다 h값 비율이 크다. w값 비율이 작다.
            //h/w < 0.6 : 기준보다 h값 비율이 작다. w값 비율이 크다.

            float fScreenRate = (float)Screen.height / (float)Screen.width;
            if (fScreenRate > REVERSE_ASPECT_RATIO) //기준해상도 비율에 비해 , 모바일 기기의 화면세로비율이 커졌거나, 가로비율이 작아진 경우
            {
                int iHeight = (int)(Screen.width * REVERSE_ASPECT_RATIO);
                Screen.SetResolution(Screen.width, iHeight, false);
            }
            else if (fScreenRate < REVERSE_ASPECT_RATIO) //기준해상도 비율에 비해 , 모바일 기기의 화면가로비율이 커졌거나, 세로비율이 작아진 경우
            {
                int iWidth = (int)(Screen.height * ASPECT_RATIO);
                Screen.SetResolution(iWidth, Screen.height, false);
            }

        }//end func
    }//end class


    ///////////////////////////////////////////////////////////////////////
    /// <summary>
    /// 기하 함수 묶음
    /// </summary>
    ///////////////////////////////////////////////////////////////////////
    public class Geo
    {
        //========================================================
        //==================       기 하        ==================
        //========================================================

        /// <summary>
        /// Arc.
        /// </summary>
        public struct Arc
        {
            public Vector3 pos;             //호의 시작점  
            public Vector3 dir;             //정규화 되어야 한다
            public float degree;            //각도 
            public float radius_near;       //시작점에서 가까운 원의 반지름 
            public float radius_far;        //시작점에서 먼 원의 반지름
                                            //public float radius;

            public const float STANDARD_COLLIDER_RADIUS = 2f;
            public float radius_collider_standard;  //기준이 되는 충돌원의 반지름 

            public float factor
            {
                get
                {   //f = radius / sin
                    return radius_collider_standard / Mathf.Sin(Mathf.Deg2Rad * degree * 0.5f);
                }
            }

            //ratio : [-1 ~ 1]
            //호에 원이 완전히 포함 [1]
            //호에 원의 중점까지 포함 [0]
            //호에 원의 경계까지 포함 [-1 : 포함 범위 가장 넒음] 
            public const float Fully_Included = 1f;
            public const float Focus_Included = 0f;
            public const float Boundary_Included = -1f;
            public Vector3 GetPosition_Factor(float ratio = Focus_Included)
            {
                if (0 == ratio)
                    return pos;

                return pos + dir * (factor * ratio);
            }

            public Sphere sphere_near
            {
                get
                {
                    Sphere sph;
                    sph.pos = this.pos;
                    sph.radius = this.radius_near;
                    return sph;
                }

            }

            public Sphere sphere_far
            {
                get
                {
                    Sphere sph;
                    sph.pos = this.pos;
                    sph.radius = this.radius_far;
                    return sph;
                }

            }

            public override string ToString()
            {

                return "pos: " + pos + "  dir: " + dir + "  degree: " + degree
                + "  radius_near: " + radius_near + "  radius_far: " + radius_far + "  radius_collider_standard: " + radius_collider_standard + "  factor: " + factor;
            }
        }

        /// <summary>
        /// Sphere.
        /// </summary>
        public struct Sphere
        {
            public Vector3 pos;
            public float radius;

            public Sphere(Vector3 p, float r)
            {
                pos = p;
                radius = r;
            }

            static public Sphere Zero
            {
                get
                {
                    Sphere sphere = new Sphere();
                    sphere.pos = Vector3.zero;
                    sphere.radius = 0f;

                    return sphere;
                }
            }

            public override string ToString()
            {
                return "pos: " + pos + "  radius: " + radius;
            }
        }

        //코사인의 각도값을 비교 한다.
        //0 ~ 180도 사이만 비교 할수있다. (1,4사분면과 2,3사분면의 cos값이 같기 때문임)  
        //cosA > cosB : 1
        //cosA < cosB : 2
        //cosA == cosB : 0
        static public int Compare_CosAngle(float cos_1, float cos_2)
        {
            //각도가 클수록 cos값은 작아진다 (0~180도 에서만 해당)
            if (cos_1 < cos_2)
                return 1;
            if (cos_1 > cos_2)
                return 2;

            return 0;
        }


        //호와 원의 충돌 검사 (2D 한정)
        static public bool Collision_Arc_VS_Sphere(Geo.Arc arc, Geo.Sphere sph)
        {
            //DebugWide.LogBlue ("1  srcPos" + arc.sphere_far.pos + " r:" + arc.sphere_far.radius + " dstPos:" + sph.pos + " r:" + sph.radius); //chamto test
            if (true == Geo.Collision_Sphere(arc.sphere_far, sph, eSphere_Include_Status.Focus))
            {

                if (false == Geo.Collision_Sphere(arc.sphere_near, sph, eSphere_Include_Status.Focus))
                {
                    //각도를 반으로 줄여 넣는다. 1과 4분면을 구별 못하기 때문에 1사분면에서 검사하면 4사분면도 검사 결과에 포함된다. 즉 실제 검사 범위가 2배가 된다.
                    float angle_arc = Mathf.Cos(arc.degree * 0.5f * Mathf.Deg2Rad);

                    //DebugWide.LogBlue ( Mathf.Acos(angle_arc) * Mathf.Rad2Deg + " [arc] " + arc.ToString() + "   [sph] " + sph.ToString());//chamto test

                    Vector3 arc_sph_dir = sph.pos - arc.GetPosition_Factor(Geo.Arc.Focus_Included);
                    arc_sph_dir.Normalize(); //노멀값 구하지 않는 계산식을 찾지 못했다. 

                    float rate_cos = Vector3.Dot(arc.dir, arc_sph_dir);
                    if (rate_cos > angle_arc)
                    {
                        return true;
                    }
                }

            }

            return false;
        }


        public enum eSphere_Include_Status
        {
            Boundary = 1,   //두원의 닿는 경계까지 포함 
            Focus,          //작은원이 중점까지 포함
            Fully           //작은원이 완전포함 
        }
        //ratio : 충돌민감도 설정 , 기본 1f , 민감도올리기 1f 보다작은값 , 민감도낮추기 1f 보다큰값  
        static public bool Collision_Sphere(Geo.Sphere src, Geo.Sphere dst, eSphere_Include_Status eInclude, float ratio = 1f)
        {

            float min_radius, max_radius, sum_radius, sqr_standard_value;
            if (src.radius > dst.radius)
            {
                min_radius = dst.radius;
                max_radius = src.radius;
            }
            else
            {
                min_radius = src.radius;
                max_radius = dst.radius;
            }

            //(src.r - dst.r) < src.r  < (src.r + dst.r)
            //완전포함        < 중점포함  < 경계포함
            //Fully           < Focus   < Boundary
            const float Minimum_Error_Value = 0.01f; //최소오차값
            switch (eInclude)
            {
                case eSphere_Include_Status.Fully:
                    sum_radius = max_radius - min_radius;
                    if (Minimum_Error_Value > sum_radius) //반지름 합값이 너무 작으면 판정을 못하므로 임의의 "최소오차값"을 할당해 준다.
                        sum_radius = Minimum_Error_Value;
                    break;
                case eSphere_Include_Status.Focus:
                    sum_radius = max_radius;
                    break;
                case eSphere_Include_Status.Boundary:
                default:
                    //두원의 반지름을 더한후 제곱해 준다. 
                    sum_radius = max_radius + min_radius;
                    break;
            }

            sqr_standard_value = (sum_radius * sum_radius) * ratio;

            //두원의 중점 사이의 거리를 구한다. 피타고라스의 정리를 이용 , 제곱을 벗기지 않는다.
            float sqr_dis_between = Vector3.SqrMagnitude(src.pos - dst.pos);

            //DebugWide.LogBlue (" r+r: "+Mathf.Sqrt(sqr_standard_value) + " p-p: " + Mathf.Sqrt(sqr_dis_between));

            if (sqr_standard_value > sqr_dis_between)
            {
                //              DebugWide.LogGreen ("T-----  include: " + eInclude.ToString() + "  std: "+Mathf.Sqrt(sqr_standard_value) + "   dis: " + Mathf.Sqrt(sqr_dis_between)
                //                  + "  srcPos: "+src.pos + "   dstPos: "+ dst.pos); //chamto test
                return true; //두원이 겹쳐짐 
            }
            //if (sqr_standard_value == sqr_dis_between)
            if(float.Epsilon >= Mathf.Abs(sqr_standard_value - sqr_dis_between))
            {
                //              DebugWide.LogGreen ("T-----  include: " + eInclude.ToString() + "  std: "+Mathf.Sqrt(sqr_standard_value) + "   dis: " + Mathf.Sqrt(sqr_dis_between)
                //                  + "  srcPos: "+src.pos + "   dstPos: "+ dst.pos); //chamto test
                return true; //포함조건과 똑같음
            }
            if (sqr_standard_value < sqr_dis_between)
            {
                //              DebugWide.LogBlue ("F-----  include: " + eInclude.ToString() + "  std: "+Mathf.Sqrt(sqr_standard_value) + "   dis: " + Mathf.Sqrt(sqr_dis_between)
                //                  + "  srcPos: "+src.pos + "   dstPos: "+ dst.pos); //chamto test
                return false; //두원이 겹쳐 있지 않음
            }

            //          DebugWide.LogWhite ("***** unreachable !!! ******");
            return false;
        }

        static public bool Collision_Sphere(Vector3 src_pos, float src_radius, Vector3 des_pos, float des_radius, eSphere_Include_Status eInclude)
        {
            Geo.Sphere src, dst;
            src.pos = src_pos; src.radius = src_radius;
            dst.pos = des_pos; dst.radius = des_radius;
            return Geo.Collision_Sphere(src, dst, eInclude);
        }

        //!!test 필요
        //value 보다 target 값이 작으면 True 를 반환한다.
        static public bool Distance_LessThan(float Value, Vector3 target)
        {
            if (Mathf.Exp(Value * 0.5f) >= Vector3.SqrMagnitude(target))
            {
                return true;
            }

            return false;
        }
    }


    ///////////////////////////////////////////////////////////////////////
    /// <summary>
    /// 보조 함수 묶음 
    /// </summary>
    ///////////////////////////////////////////////////////////////////////
    public class Misc
    {
        //========================================================
        //==================       애셋 경로       ==================
        //========================================================

#if UNITY_EDITOR
        public const string CURRENT_PLATFORM = "UNITY_EDITOR";
        public static string ASSET_PATH = "file://" + UnityEngine.Application.dataPath + "/StreamingAssets/";
#elif UNITY_IPHONE
            public const string CURRENT_PLATFORM = "UNITY_IPHONE";
            public static string ASSET_PATH = "file://" + UnityEngine.Application.dataPath + "/Raw/";
#elif UNITY_ANDROID
            public const string CURRENT_PLATFORM = "UNITY_ANDROID";
            public static string ASSET_PATH = "jar:file://" + UnityEngine.Application.dataPath + "!/assets/";
#elif SERVER
            public const string CURRENT_PLATFORM = "SERVER";
            public static string ASSET_PATH = "Data_KOR\\";
#elif TOOL
            public const string CURRENT_PLATFORM = "TOOL";
            public static string ASSET_PATH = "Data_KOR\\";
#endif

        //========================================================
        //==================     문자열 처리     ==================
        //========================================================
        static public Vector3 StringToVector3(string s)
        {
            char[] delimiterChars = { ' ', ',', '(', ')' };
            string[] parts = s.Split(delimiterChars, System.StringSplitOptions.RemoveEmptyEntries);

            return new Vector3(
                float.Parse(parts[0]),
                float.Parse(parts[1]),
                float.Parse(parts[2]));
        }

        static public Vector2Int StringToVector2Int(string s)
        {
            char[] delimiterChars = { ' ', ',', '(', ')' };
            string[] parts = s.Split(delimiterChars, System.StringSplitOptions.RemoveEmptyEntries);

            return new Vector2Int(
                int.Parse(parts[0]),
                int.Parse(parts[1]));
        }


        //========================================================
        //==================       랜덤함수       ==================
        //========================================================

        //https://docs.microsoft.com/ko-kr/dotnet/api/system.random.sample?view=netframework-4.7.2#System_Random_Sample
        //.NextDouble() : 0 보다 크고 1.0 보다 작은 임의의 실수값 (1.0은 포함되지 않는다)
        //.Sample() : 0 과 1.0 사이의 실수값 (1.0은 포함된다)
        //.Next(int minValue, int maxValue) : minValue ~ maxValue 사이의 값을 반환한다. (maxValue 는 포함되지 않는다)
        private static System.Random _rand = new System.Random();
        static public System.Random rand
        {
            get { return _rand; }
        }

        //==============================================
        //ref : Mat Buckland - Programming Game AI by Example
        // 랜덤함수 참조 
        //==============================================

        //.Next(int minValue, int maxValue) 와 비슷한 함수이다.
        // maxValue 값이 포함된다는 차이점이 있다. maxValue 에 int.MaxValue 를 넣을시 오버플로우가 발생한다. 최대값 예외처리 없음 
        //returns a random integer between x and y
        static public int RandInt(int x, int y)
        {
            Assert.IsTrue((y >= x), "<RandInt>: y is less than x");

            return rand.Next() % (y - x + 1) + x;
        }


        //http://gabble-workers.tistory.com/6
        //실수 랜덤값 구하는 다른 방법
        //(rand() % 10000 ) * 0.0001f : 0~1.0 (1.0은 포함되지 않는다)
        //(rand() % 10000 + 1) * 0.0001f : 0~1.0 (1.0은 포함된다)


        //.NextDouble() 과 같은 함수이다.
        //returns a random double between zero and 1
        static public double RandFloat()
        {
            //https://docs.microsoft.com/ko-kr/dotnet/api/system.random.next?view=netframework-4.7.2#System_Random_Next
            //MaxValue 는 rand.Next 가 나올수 있는 값보다 1 큰 값이다. 
            //1 큰값으로 나누기 때문에 1.0 에 결코 도달하지 못한다 
            return (rand.Next() / (int.MaxValue));
        }

        //RandFloat를 사용하기 때문에 최대값은 포함 되지 않는다. 
        static public double RandInRange(double x, double y)
        {
            return x + RandFloat() * (y - x);
        }

        //returns a random bool
        static public bool RandBool()
        {
            if (RandFloat() > 0.5) return true;

            else return false;
        }

        //returns a random double in the range -1 < n < 1
        static public double RandomClamped() { return RandFloat() - RandFloat(); }


        //returns a random number with a normal distribution. See method at
        //http://www.taygeta.com/random/gaussian.html
        private static double __gaussian_y2 = 0;
        private static bool __gaussian_useLast = false;
        static public double RandGaussian(double mean = 0.0, double standard_deviation = 1.0)
        {
            double x1, x2, w, y1;

            if (__gaussian_useLast)               /* use value from previous call */
            {
                y1 = __gaussian_y2;
                __gaussian_useLast = false;
            }
            else
            {
                do
                {
                    x1 = 2.0 * RandFloat() - 1.0;
                    x2 = 2.0 * RandFloat() - 1.0;
                    w = x1 * x1 + x2 * x2;
                }
                while (w >= 1.0);

                w = Math.Sqrt((-2.0 * Math.Log(w)) / w);
                y1 = x1 * w;
                __gaussian_y2 = x2 * w;
                __gaussian_useLast = true;
            }

            return (mean + y1 * standard_deviation);
        }

        //========================================================
        //==================      8방향 함수     ==================
        //========================================================

        /// <summary>
        ///  여덟 방향중 하나를 무작위로 반환한다. 
        ///  8방향 캐릭터의 방향을 무작위로 얻고 싶을때 사용 
        /// </summary>
        /// <returns> 정규벡터 방향값 </returns>
        static public Vector3 RandomDir8_AxisY()
        {
            const float ANG_RAD = (360f / 8f) * Mathf.Deg2Rad;
            int rand = Misc.rand.Next(1, 9); //1~8
            Vector3 dir = Vector3.zero;
            rand -= 1;
            dir.x = Mathf.Cos(ANG_RAD * rand);
            dir.z = Mathf.Sin(ANG_RAD * rand);

            return dir;
        }


        //미리구한 정규화된 8방향값 
        static private Vector3[] _Dir8_AxisY = new Vector3[]
        {   new Vector3(0,0,0) ,                //    zero = 0, 
            new Vector3(1,0,0).normalized ,     //    right = 1, 
            new Vector3(1,0,1).normalized ,     //    rightUp = 2, 
            new Vector3(0,0,1).normalized ,     //    up = 3,
            new Vector3(-1,0,1).normalized ,    //    leftUp = 4,
            new Vector3(-1,0,0).normalized ,    //    left = 5,
            new Vector3(-1,0,-1).normalized ,   //    leftDown = 6,
            new Vector3(0,0,-1).normalized ,    //    down = 7,
            new Vector3(1,0,-1).normalized ,    //    rightDown = 8,
        };

        //-z축 기준 
        static private Vector3[] _Dir8_AxisMZ = new Vector3[]
        {   new Vector3(0,0,0) ,                //    zero = 0, 
            new Vector3(1,0,0).normalized ,     //    right = 1, 
            new Vector3(1,1,0).normalized ,     //    rightUp = 2, 
            new Vector3(0,1,0).normalized ,     //    up = 3,
            new Vector3(-1,1,0).normalized ,    //    leftUp = 4,
            new Vector3(-1,0,0).normalized ,    //    left = 5,
            new Vector3(-1,-1,0).normalized ,   //    leftDown = 6,
            new Vector3(0,-1,0).normalized ,    //    down = 7,
            new Vector3(1,-1,0).normalized ,    //    rightDown = 8,
        };

        static public Vector3 GetDir8Normal_AxisY(eDirection8 eDirection)
        {
            return _Dir8_AxisY[(int)eDirection];
        }

        static public Vector3 GetDir8Normal_AxisMZ(eDirection8 eDirection)
        {
            return _Dir8_AxisMZ[(int)eDirection];
        }

        /// <summary>
        /// 방향값을 8방향 값으로 변환해 준다 
        /// </summary>
        static public eDirection8 TransDirection8_AxisY(Vector3 dir)
        {
            if (Vector3.zero.Equals(dir)) return eDirection8.none;

            float rad = Mathf.Atan2(dir.z, dir.x);
            float deg = Mathf.Rad2Deg * rad;

            //각도가 음수라면 360을 더한다 
            if (deg < 0) deg += 360f;

            //360 / 45 = 8
            int quad = Mathf.RoundToInt(deg / 45f);
            quad %= 8; //8 => 0 , 8을 0으로 변경  
            quad++; //값의 범위를 0~7 에서 1~8로 변경 

            return (eDirection8)quad;
        }

        static public eDirection8 TransDirection8_AxisMZ(Vector3 dir)
        {
            if (Vector3.zero.Equals(dir)) return eDirection8.none;

            float rad = Mathf.Atan2(dir.y, dir.x);
            float deg = Mathf.Rad2Deg * rad;

            //각도가 음수라면 360을 더한다 
            if (deg < 0) deg += 360f;

            //360 / 45 = 8
            int quad = Mathf.RoundToInt(deg / 45f);
            quad %= 8; //8 => 0 , 8을 0으로 변경  
            quad++; //값의 범위를 0~7 에서 1~8로 변경 

            return (eDirection8)quad;
        }


        //none = 0,
        //right = 1,
        //rightUp = 2,
        //up = 3,
        //leftUp = 4,
        //left = 5,
        //leftDown = 6,
        //down = 7,
        //rightDown = 8,
        //max,
        static public eDirection8 ReverseDir8_AxisY(eDirection8 eDirection)
        {
            if (eDirection8.none == eDirection) return eDirection8.none;
            
            int dir = (int)eDirection;
            dir--; //0~7로 변경
            dir += 4;
            dir %= 8;
            dir++; //1~8로 변경 
            return (eDirection8)dir;
        }

        //========================================================
        //==================       비트연산        ==================
        //========================================================

        //ref : https://stackoverflow.com/questions/27237776/convert-int-bits-to-float-bits
        //int i = ...;
        //float f = BitConverter.ToSingle(BitConverter.GetBytes(i), 0);
        public static unsafe int SingleToInt32Bits(float value)
        {
            return *(int*)(&value);
        }
        public static unsafe float Int32BitsToSingle(int value)
        {
            return *(float*)(&value);
        }

        //========================================================
        //==================       수   학       ==================
        //========================================================

        //ref : https://ko.wikipedia.org/wiki/%EA%B3%A0%EC%86%8D_%EC%97%AD_%EC%A0%9C%EA%B3%B1%EA%B7%BC
        //함수해석 : https://zariski.wordpress.com/2014/10/29/%EC%A0%9C%EA%B3%B1%EA%B7%BC-%EC%97%AD%EC%88%98%EC%99%80-%EB%A7%88%EB%B2%95%EC%9D%98-%EC%88%98-0x5f3759df/
        //함수해석 : http://eastroot1590.tistory.com/entry/%EC%A0%9C%EA%B3%B1%EA%B7%BC%EC%9D%98-%EC%97%AD%EC%88%98-%EA%B3%84%EC%82%B0%EB%B2%95
        //뉴턴-랩슨법  : http://darkpgmr.tistory.com/58
        //sse 명령어 rsqrtss 보다 빠를수가 없다.
        //[ reciprocal square root ]
        // 제곱근의 역수이다. a 일때  역수는 1/a 이다.
        static public unsafe float RSqrt_Quick_2(float x)
        {

            const int SQRT_MAGIC_F = 0x5f3759df;
            const float threehalfs = 1.5F;
            float xhalf = 0.5f * x;

            float ux;
            int ui;

            ui = *(int*)&x;
            ui = SQRT_MAGIC_F - (ui >> 1);  // gives initial guess y0
            ux = *(float*)&ui;
            ux = ux * (threehalfs - xhalf * ux * ux);// Newton step, repeating increases accuracy 

            return ux;
        }


        //Algorithm: The Magic Number (Quake 3) - 유니티엔진에서 쓰는 방식 추정 
        static public unsafe float Sqrt_Quick_2(float x)
        {
            const int SQRT_MAGIC_F = 0x5f3759df;
            const float threehalfs = 1.5F;
            float xhalf = 0.5f * x;

            float ux;
            int ui;

            ui = *(int*)&x;
            ui = SQRT_MAGIC_F - (ui >> 1);  // gives initial guess y0
            ux = *(float*)&ui;
            ux = x * ux * (threehalfs - xhalf * ux * ux);// Newton step, repeating increases accuracy 

            return ux;
        }

        //어셈코드 다음으로 속도가 가장 빠름. 매직넘버 "0x5f3759df" 코드보다 빠르지만 정확도는 더 떨어진다
        //ref : https://www.codeproject.com/Articles/69941/Best-Square-Root-Method-Algorithm-Function-Precisi
        //Algorithm: Dependant on IEEE representation and only works for 32 bits 
        static public unsafe float Sqrt_Quick_7(float x)
        {

            uint i = *(uint*)&x;
            // adjust bias
            i += 127 << 23;
            // approximation of square root
            i >>= 1;
            return *(float*)&i;
        }


        static public Vector3 Norm_Quick(Vector3 v3)
        {
            //float r_length = Util.RSqrt_Quick_2 (v3.sqrMagnitude);
            float r_length = 1f / Misc.Sqrt_Quick_7(v3.sqrMagnitude);
            return v3 * r_length;
        }
    

    }

}

