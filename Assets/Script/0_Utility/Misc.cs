using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Text.RegularExpressions;

namespace UtilGS9
{
    //========================================================
    //==================      전역  상수      ==================
    //========================================================

    /// <summary>
    /// 데카르트좌표계 사분면을 8방향으로 나누어 숫자 지정
    /// </summary>
    public enum eDirection8 : int
    {
        none = 0,
        center = none,
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

    //열거형 이름 바꾸면 안됨. 애니파일 파싱할때 문자열로 변환하여 사용한다 
    public enum eAniBaseKind
    {
        idle = 0,
        move,
        attack,
        fallDown,
        MAX,
    }

    public partial class ConstV
    {
        public static readonly string STRING_EMPTY = string.Empty;
        //public static readonly string base_idle = "base_idle";
        //public static readonly string base_move = "base_move";
        //public static readonly string base_attack = "base_attack";
        //public static readonly string base_fallDown = "base_fallDown";
        //public static readonly string[] base_anis = new string[]
        //{
        //    "base_idle",
        //    "base_move",
        //    "base_attack",
        //    "base_fallDown",
        //};
        //public static readonly KeyValuePair<eAniBaseKind, string>[] base_anis = new KeyValuePair<eAniBaseKind, string>[]
        //{
        //    new KeyValuePair<eAniBaseKind, string>(eAniBaseKind.idle, "base_idle"),
        //    new KeyValuePair<eAniBaseKind, string>(eAniBaseKind.move, "base_move"),
        //    new KeyValuePair<eAniBaseKind, string>(eAniBaseKind.attack, "base_attack"),
        //    new KeyValuePair<eAniBaseKind, string>(eAniBaseKind.fallDown, "base_fallDown"),
        //};
        public static readonly string[] base_anis = new string[(int)eAniBaseKind.MAX];
        public static void InitConst_BaseAni()
        {
            base_anis[(int)eAniBaseKind.idle] = "base_idle";
            base_anis[(int)eAniBaseKind.move] = "base_move";
            base_anis[(int)eAniBaseKind.attack] = "base_attack";
            base_anis[(int)eAniBaseKind.fallDown] = "base_fallDown";
        }
        public static string GetAniBaseKind(eAniBaseKind kind)
        {
            return base_anis[(int)kind];
        }

        public static AnimationClip[] FindAniBaseClips(AnimationClip[] clips_all)
        {
            AnimationClip[] clips_base = new AnimationClip[(int)eAniBaseKind.MAX];
            foreach(AnimationClip cl in clips_all)
            {
                for (int i = 0; i < (int)eAniBaseKind.MAX; i++)
                {
                    if (cl.name == base_anis[i])
                    {
                        clips_base[i] = cl;
                    }    
                }
            }

            return clips_base;
        }
    }

    public partial class ConstV
    {
        static public readonly Vector3 v3_zero = Vector3.zero;
        static public readonly Vector3 v3_one = Vector3.one;
        static public readonly Vector3 v3_up = Vector3.up;
        static public readonly Vector3 v3_right = Vector3.right;
        static public readonly Vector3 v3_forward = Vector3.forward;

        static public readonly Vector3Int v3Int_zero = Vector3Int.zero;
        static public readonly Vector2Int v2Int_zero = Vector2Int.zero;
    }

}



namespace UtilGS9
{
    
    ///////////////////////////////////////////////////////////////////////
    /// <summary>
    /// 해쉬값과 문자열을 쌍으로 묶어 관리하는 객체
    /// </summary>
    ///////////////////////////////////////////////////////////////////////
    public class HashToStringMap
    {
        Dictionary<int, string> _hashStringMap = new Dictionary<int, string>();

        public void Add(int hash, string str)
        {
            if (false == _hashStringMap.ContainsKey(hash))
            {
                _hashStringMap.Add(hash, str);
            }
        }

        public void Add(string str)
        {
            int hash = str.GetHashCode();
            this.Add(hash, str);
        }

        public bool Contain(int hash)
        {
            return _hashStringMap.ContainsKey(hash);
        }

        public string GetString(int hash)
        {
            if (_hashStringMap.ContainsKey(hash))
            {
                return _hashStringMap[hash];
            }

            //throw new KeyNotFoundException();

            return ""; //존재하지 않는 키값
        }

        public string GetString_ForAssetFile(int hash)
        {
            string tempStr = this.GetString(hash);
            return Regex.Replace(tempStr, "[?<>:*|\"]", ""); //애셋파일 이름에 들어가면 안되는 특수문자 제거 ?, <, >, :, *, |, ".
        }
    }


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
    /// 벡터 함수 묶음 
    /// </summary>
    ///////////////////////////////////////////////////////////////////////
    public static class VOp
    {
        //Vector3 사칙연산 함수보다 약간 더 빠르다 
        static public Vector3 Plus(Vector3 va, Vector3 vb, Vector3 result = default(Vector3))
        {
            result.x = va.x + vb.x;
            result.y = va.y + vb.y;
            result.z = va.z + vb.z;
            return result;
        }


        static public Vector3 Minus(Vector3 va, Vector3 vb, Vector3 result = default(Vector3))
        {
            result.x = va.x - vb.x;
            result.y = va.y - vb.y;
            result.z = va.z - vb.z;
            return result;
        }

        static public Vector3 Multiply(Vector3 va, float b, Vector3 result = default(Vector3))
        {
            result.x = va.x * b;
            result.y = va.y * b;
            result.z = va.z * b;
            return result;
        }

        static public Vector3 Division(Vector3 va, float b, Vector3 result = default(Vector3))
        {
            result.x = va.x / b;
            result.y = va.y / b;
            result.z = va.z / b;
            return result;
        }


        //Vector3.normalize 보다 빠르다
        static public Vector3 Normalize(Vector3 vector3)
        {
            float len = vector3.magnitude;
            vector3.x /= len;
            vector3.y /= len;
            vector3.z /= len;

            return vector3; 

        }
    }

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

        //value 보다 target 값이 작으면 True 를 반환한다.
        static public bool Distance_LessThan(float srcDis, Vector3 dstV3)
        {
            //거듭제곱 함수를 써야하는데 지수함수를 잘못사용함. 최적화를 위해 함수를 사용하지 않고 직접 계산한다 
            //if(Mathf.Pow(Value , 2) >=  Vector3.SqrMagnitude( target ))
            //if(Mathf.Exp(Value * 0.5f) >=  Vector3.SqrMagnitude( target ))

            if (srcDis * srcDis >= Vector3.SqrMagnitude(dstV3))
            {
                return true;
            }

            return false;
        }

        //==================================================
        //** 수행횟수 5만번 기준 **
        //1ms Math.Sqrt
        ///1.4ms VOp.Multiply //벡터연산 곱 
        ///1.4ms VOp.Plus //벡터연산 합
        ///1.4ms VOp.Minus //벡터연산 차
        ///2ms VOp.Division //벡터연산 나눔
        //2ms Vector3.magnitude
        //2ms Math.Atan2 ~ Math.Cos ~ Math.Sin
        ///3.5ms AngleSigned_AxisY (월드축)
        ///4ms VOp.Normalize
        //6ms Vector3.Dot
        ///6ms AngleSigned_Normal_V0V1 (월드축제한 없음 : 인수벡터 2개 정규화)
        ///10ms AngleSigned_Normal_V0 (월드축제한 없음 : 인수벡터 1개 정규화)
        ///17ms AngleSigned (월드축제한 없음 : 인수벡터 0개 정규화)
        /// 
        //Vector3.SignedAngle 와 내부 알고리즘 동일. 속도가 조금더 빠르다 
        public static float AngleSigned(Vector3 v0, Vector3 v1, Vector3 axis)
        {
            //v0.Normalize();
            //v1.Normalize();
            v0 = VOp.Normalize(v0);
            v1 = VOp.Normalize(v1);
            float proj = Vector3.Dot(v0, v1);
            Vector3 vxw = Vector3.Cross(v0, v1);

            //스칼라삼중적을 이용하여 최단회전방향을 구한다 
            //float sign = 1f;
            if (Vector3.Dot(axis, vxw) < 0)
                return Mathf.Acos(proj) * Mathf.Rad2Deg * -1f;

            return Mathf.Acos(proj) * Mathf.Rad2Deg;
        }

        public static float AngleSigned_Normal_V0(Vector3 v0, Vector3 v1, Vector3 axis)
        {
            v1 = VOp.Normalize(v1);
            float proj = Vector3.Dot(v0, v1);
            Vector3 vxw = Vector3.Cross(v0, v1);

            //스칼라삼중적을 이용하여 최단회전방향을 구한다 
            //float sign = 1f;
            if (Vector3.Dot(axis, vxw) < 0)
                return Mathf.Acos(proj) * Mathf.Rad2Deg * -1f;

            return Mathf.Acos(proj) * Mathf.Rad2Deg;
        }

        public static float AngleSigned_Normal_V0V1(Vector3 v0, Vector3 v1, Vector3 axis)
        {
            float proj = Vector3.Dot(v0, v1);
            Vector3 vxw = Vector3.Cross(v0, v1);

            //스칼라삼중적을 이용하여 최단회전방향을 구한다 
            //float sign = 1f;
            if (Vector3.Dot(axis, vxw) < 0)
                return Mathf.Acos(proj) * Mathf.Rad2Deg * -1f;

            return Mathf.Acos(proj) * Mathf.Rad2Deg;
        }


        //속도가 가장 빠름. 월드축에서만 사용 할 수 있다 
        public static float AngleSigned_AxisY(Vector3 v0, Vector3 v1)
        {
            float at0 = Mathf.Atan2(v0.z, v0.x);
            float at1 = Mathf.Atan2(v1.z, v1.x);

            return (at0 - at1) * Mathf.Rad2Deg;
        }

        public static float Angle_AxisY(Vector3 v0, Vector3 v1)
        {
            float at0 = Mathf.Atan2(v0.z, v0.x);
            float at1 = Mathf.Atan2(v1.z, v1.x);
            float rad = at0 - at1;

            if (rad < 0) rad *= -1; //부호를 없앤다 

            return rad * Mathf.Rad2Deg;
        }

        //==================================================

        //ray_dir : 정규화된 값을 넣어야 한다 
        //intersection_firstPoint : 반직선이 원과 충돌한 첫번째 위치를 반환
        static public bool IntersectRay(Vector3 sphere_center, float sphere_radius, Vector3 ray_origin, Vector3 ray_dir, out Vector3 intersection_firstPoint)
        {

            Vector3 w = sphere_center - ray_origin;
            Vector3 v = ray_dir; //rayDirection 이 정규화 되어 있어야 올바르게 계산할 수 있다 
            float rsq = sphere_radius * sphere_radius;
            float wsq = Vector3.Dot(w, w); //w.x * w.x + w.y * w.y + w.z * w.z;

            // Bug Fix For Gem, if origin is *inside* the sphere, invert the
            // direction vector so that we get a valid intersection location.
            if (wsq < rsq) v *= -1; //반직선의 시작점이 원안에 있는 경우 - 방법1 

            float proj = Vector3.Dot(w, v);
            float ssq = (wsq - proj * proj);
            float dsq = rsq - ssq;

            intersection_firstPoint = Vector3.zero;
            if (dsq > 0.0f)
            {
                float d = Mathf.Sqrt(dsq);

                //반직선의 시작점이 원안에 있는 경우 - 방법2
                //float length = proj - d; //선분 시작점이 원 밖에 있는 경우
                //if(wsq < rsq) length = proj + d; //선분 시작점이 원 안에 있는 경우
                //intersect_firstPoint = rayOrigin + v * length;

                intersection_firstPoint = ray_origin + v * (proj - d);

                return true;
            }
            return false;
        }

        //segment_dir : 정규화된 값을 넣어야 한다 
        //intersection_firstPoint : 반직선이 원과 충돌한 첫번째 위치를 반환
        static public bool IntersectLineSegment(Vector3 sphere_center, float sphere_radius, Vector3 segment_origin, Vector3 segment_dir, float segment_distance, out Vector3 intersection_firstPoint)
        {
            Vector3 sect;
            bool hit = IntersectRay(sphere_center, sphere_radius, segment_origin, segment_dir, out sect);

            intersection_firstPoint = Vector3.zero;
            if (hit)
            {
                float d = (segment_origin - sect).sqrMagnitude;
                if (d > (segment_distance * segment_distance)) return false;
                intersection_firstPoint = sect;
                return true;
            }
            return false;
        }

        //ray_dir : 비정규화된 값을 넣어도 된다  
        static public bool IntersectRay(Vector3 sphere_center, float sphere_radius, Vector3 ray_origin, Vector3 ray_dir)
        {
            // compute intermediate values
            Vector3 w = sphere_center - ray_origin;
            float wsq = Vector3.Dot(w, w); //w.sqrMagnitude
            float proj = Vector3.Dot(w, ray_dir);
            float rsq = sphere_radius * sphere_radius;

            // if sphere behind ray, no intersection
            if (proj < 0.0f && wsq > rsq)
                return false;
            float vsq = Vector3.Dot(ray_dir, ray_dir);

            // test length of difference vs. radius
            return (vsq * wsq - proj * proj <= vsq * rsq);
        }



        //ref : http://nic-gamedev.blogspot.com/2011/11/using-vector-mathematics-and-bit-of_09.html
        static public bool IntersectLineSegment(Vector3 sphere_center, float sphere_radius, Vector3 segment_origin, Vector3 segment_last)
        {

            Vector3 v = segment_last - segment_origin;
            float vsq = v.sqrMagnitude;

            Vector3 w = sphere_center - segment_origin;
            float proj = Vector3.Dot(w, v);

            //ipt : 원의 중심점과 선분이 가장 가까운 선분위의 점. 원의 중점과 선분이 직각으로 만나는 점.
            //oi = ipt - segment_origin
            //cos||w||||v|| / vsq = cos||w|| / ||v|| => ||oi|| / ||v||
            //전체선분 길이에 대하여, 원의 중심점에서 ipt 까지의 길이의 비율을 구한다 
            float percAlongLine = proj / vsq; //0~1 사이의 비율값으로 변환한다

            if (percAlongLine < 0.0f)
            {
                percAlongLine = 0.0f;
            }
            else if (percAlongLine > 1.0f)
            {
                percAlongLine = 1.0f;
            }

            Vector3 ipt = (segment_origin + (percAlongLine * v)); //선분에 비율값을 곱한다 

            Vector3 s = ipt - sphere_center;
            float ssq = s.sqrMagnitude;
            float rsq = sphere_radius * sphere_radius;
            return (ssq <= rsq);
        }




    }//end geo


    ///////////////////////////////////////////////////////////////////////
    /// <summary>
    /// 보조 함수 묶음 
    /// </summary>
    ///////////////////////////////////////////////////////////////////////
    public class Misc
    {

        //!!!! 초기화
        static public void Init()
        {
            ConstV.InitConst_BaseAni();

            _dir64_normal3D_AxisY = Misc.Create_DirNormal(64);
        }

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
            //Assert.IsTrue((y >= x), "<RandInt>: y is less than x");

            return rand.Next() % (y - x + 1) + x;
        }


        //http://gabble-workers.tistory.com/6
        //실수 랜덤값 구하는 다른 방법
        //(rand() % 10000 ) * 0.0001f : 0~1.0 (1.0은 포함되지 않는다)
        //(rand() % 10000 + 1) * 0.0001f : 0~1.0 (1.0은 포함된다)


        //.NextDouble() 과 같은 함수이다.
        //returns a random double between zero and 1
        static public float RandFloat()
        {
            //https://docs.microsoft.com/ko-kr/dotnet/api/system.random.next?view=netframework-4.7.2#System_Random_Next
            //MaxValue 는 rand.Next 가 나올수 있는 값보다 1 큰 값이다. 
            //1 큰값으로 나누기 때문에 1.0 에 결코 도달하지 못한다 
            return ((float)rand.Next() / (float)(int.MaxValue));
        }

        static public float RandFloat(float x, float y)
        {
            return x + RandFloat() * (y - x);
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


        //미리구한 정규화된 8방향값 
        static private Vector3[] _dir8_normal3D_AxisY = new Vector3[]
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
        static private Vector3[] _dir8_normal3D_AxisMZ = new Vector3[]
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

        static private Vector3Int[] _dir8_normal2D = new Vector3Int[]
        {   new Vector3Int(0,0,0) ,                //    zero = 0, 
            new Vector3Int(1,0,0) ,     //    right = 1, 
            new Vector3Int(1,1,0) ,     //    rightUp = 2, 
            new Vector3Int(0,1,0) ,     //    up = 3,
            new Vector3Int(-1,1,0) ,    //    leftUp = 4,
            new Vector3Int(-1,0,0) ,    //    left = 5,
            new Vector3Int(-1,-1,0) ,   //    leftDown = 6,
            new Vector3Int(0,-1,0) ,    //    down = 7,
            new Vector3Int(1,-1,0) ,    //    rightDown = 8,
        };

        static private Vector3[] _dir64_normal3D_AxisY = null;
        static public Vector3[] Create_DirNormal(ushort equal_division)
        {
            Vector3[] dirN = new Vector3[equal_division+1];
            float angle = 360f / (float)equal_division;
            dirN[0] = Vector3.zero;
            for (ushort i = 0; i < equal_division;i++)
            {
                dirN[i+1] = Quaternion.AngleAxis(-angle * i, Vector3.up) * Vector3.right;
                dirN[i+1].Normalize();
                //DebugWide.LogBlue((i+1) + "  : " + dirN[i + 1] + "  : " + angle * i); //chamto test

            }
            dirN[equal_division] = dirN[0]; //360도에 0도 일때의 값을 넣는다 

            return dirN;
        }

        static public void DrawDirN()
        {
#if UNITY_EDITOR
            int count = 0;
            foreach (Vector3 xz in _dir64_normal3D_AxisY)
            {
                Debug.DrawLine(Vector3.zero, xz);
                UnityEditor.Handles.Label(xz, ":" + count++);
            }
#endif
        }

        /// <summary>
        ///  여덟 방향중 하나를 무작위로 반환한다. 
        ///  8방향 캐릭터의 방향을 무작위로 얻고 싶을때 사용 
        /// </summary>
        /// <returns> 정규벡터 방향값 </returns>
        static public Vector3 GetDir8_Random_AxisY()
        {
            //const float ANG_RAD = (360f / 8f) * Mathf.Deg2Rad;
            int rand = Misc.rand.Next(1, 9); //1~8
             //Vector3 dir = Vector3.zero;
             //rand -= 1;
             //dir.x = Mathf.Cos(ANG_RAD * rand);
             //dir.z = Mathf.Sin(ANG_RAD * rand);
             //return dir;

            return _dir8_normal3D_AxisY[rand];
        }

        static public eDirection8 GetDir8_Reverse_AxisY(eDirection8 eDirection)
        {
            if (eDirection8.none == eDirection) return eDirection8.none;

            int dir = (int)eDirection;
            dir--; //0~7로 변경
            dir += 4;
            dir %= 8;
            dir++; //1~8로 변경 
            return (eDirection8)dir;
        }

        static public Vector3 GetDir8_Normal3D_AxisY(eDirection8 eDirection)
        {
            return _dir8_normal3D_AxisY[(int)eDirection];
        }

        static public Vector3Int GetDir8_Normal2D(eDirection8 eDirection)
        {
            return _dir8_normal2D[(int)eDirection];
        }
        static public Vector3Int GetDir8_Normal2D(Vector3 dir)
        {
            int quad = (int)GetDir8_AxisY(dir);
            return _dir8_normal2D[quad];
        }
        static public Vector3 GetDir8_Normal3D(Vector3 dir)
        {
            int quad = (int)GetDir8_AxisY(dir);
            return _dir8_normal3D_AxisY[quad];
        }


        static public int RoundToInt(float value)
        {

            //반올림
            if (0 <= value)
            {
                //양수일때
                value += 0.5f;
            }
            else
            {
                //음수일때
                value -= 0.5f;
            }
            
            return (int)value;
        }

        static public bool IsZero(float value)
        {
            if (0 > value) value *= -1f;
            if (Vector3.kEpsilon < value)
                return false;
            
            return true;
        }

        static public bool IsZero(Vector3 v3)
        {
            float value = v3.x * v3.x + v3.y * v3.y + v3.z * v3.z;
            if (0 > value) value *= -1f;
            if (Vector3.kEpsilon < value)
                return false;

            //return Misc.IsZero(v3.x * v3.x + v3.y * v3.y + v3.z * v3.z);
            return true;
        }

        /// <summary>
        /// 방향값을 8방향 값으로 변환해 준다 
        /// </summary>
        static public eDirection8 GetDir8_AxisY(Vector3 dir)
        {
            //if (Misc.IsZero(dir.x*dir.x+dir.y*dir.y+dir.z*dir.z)) return eDirection8.none;
            if (Misc.IsZero(dir)) return eDirection8.none;

            float rad = Mathf.Atan2(dir.z, dir.x);
            float deg = Mathf.Rad2Deg * rad;

            //각도가 음수라면 360을 더한다 
            if (deg < 0) deg += 360f;

            //360 / 45 = 8
            //int quad = Mathf.RoundToInt(deg / 45f); //0~8
            //int quad = Misc.RoundToInt(deg / 45f); //0~8
            int quad = (int)((deg / 45f) + 0.5f);
            quad %= 8; //8을 0으로 변경 : 0~7,0 
            quad++; //값의 범위를 0~7 에서 1~8로 변경 

            return (eDirection8)quad;
        }


        static public int GetDirN_AxisY(ushort equal_division, Vector3 dir)
        {
            
            float rad = Mathf.Atan2(dir.z, dir.x);
            float deg = Mathf.Rad2Deg * rad;

            //각도가 음수라면 360을 더한다 
            if (deg < 0) deg += 360f;

            float angle_division = 360f / equal_division;
            int quad = (int)((deg / angle_division) + 0.5f); //0~32

            return quad;
        }

        //느림.  GetDir64_Normal3D > Vector3.normalized  .  GetDir64_Normal3D 함수 쓰기 
        //static public Vector3 TestGetDir_Normal3D_AxisY(Vector3 dir)
        //{
        //    float rad = Mathf.Atan2(dir.z, dir.x);
        //    return new Vector3(Mathf.Cos(rad),0,Mathf.Sin(rad));
        //}


        //==========================  equal_division 64 ==============================

        const float ANGLE_DIVISION_64 = 360f / 64;
        static public Vector3 GetDir64_Normal3D(Vector3 dir)
        {
            
            float rad = Mathf.Atan2(dir.z, dir.x);
            float deg = Mathf.Rad2Deg * rad;

            //각도가 음수라면 360을 더한다 
            if (deg < 0) deg += 360f;
            
            int quad = (int)((deg / ANGLE_DIVISION_64) + 0.5f); //0~64

            return _dir64_normal3D_AxisY[quad];
        }



        //static public Vector3 GetDir64_Normal3D(Vector3 dir)
        //{
        //    int quad = GetDirN_AxisY(64,dir);
        //    return _dir64_normal3D_AxisY[quad];
        //}

        //==========================  Axis munus Z ==============================

        static public Vector3 GetDir8_Normal3D_AxisMZ(eDirection8 eDirection)
        {
            return _dir8_normal3D_AxisMZ[(int)eDirection];
        }

        static public eDirection8 GetDir8_AxisMZ(Vector3 dir)
        {
            if (Misc.IsZero(dir)) return eDirection8.none;

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

        //ref : https://davidzych.com/converting-an-int-to-a-binary-string-in-c/
        public static string IntToBinaryString(int number)
        {
            if (0 == number) return "0";

            const int mask = 1; //첫번째 1비트만 걸러내는 마스크 
            string binary = string.Empty;
            while (number > 0)
            {
                // Logical AND the number and prepend it to the result string
                binary = (number & mask) + binary;
                number = number >> 1;
            }

            return binary;
        }

        //========================================================
        //==================       컬러연산        ==================
        //========================================================

        //ref : https://answers.unity.com/questions/1161444/convert-int-to-color.html
        public static Color32 Hex_ToColor32(uint aCol)
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

        //========================================================
        //==================       곡  선       ==================
        //========================================================
    
        //선형 베지어 곡선
        static public Vector3 BezierCurve(Vector3 p0, Vector3 p1, float t)
        {
            return (1f - t) * p0 + t * p1;
        }

        //2차 베지어 곡선 (Quadratic)
        static public Vector3 BezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            float o1Mt = 1f - t;

            return (o1Mt * o1Mt * p0) + (2 * t * o1Mt * p1) + (t * t * p2);

            //Vector3 p0p1 = Misc.BezierCurve(p0, p1, t);
            //Vector3 p1p2 = Misc.BezierCurve(p1, p2, t);
            //return Misc.BezierCurve(p0p1, p1p2, t);
        }

        //3차 베지어 곡선 (Cubic)
        static public Vector3 BezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            Vector3 p0p1p2 = Misc.BezierCurve(p0, p1, p2, t);
            Vector3 p1p2p3 = Misc.BezierCurve(p1, p2, p3, t);

            return Misc.BezierCurve(p0p1p2, p1p2p3, t);
        }

    }

}

