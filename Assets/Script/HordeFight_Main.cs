using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using Utility;

namespace HordeFight
{
    public class HordeFight_Main : MonoBehaviour
    {




        // Use this for initialization
        void Start()
        {
           
            gameObject.AddComponent<TouchProcess>();

            //===================

            Single.objectManager.Create_StageInfo();


        }

        // Update is called once per frame
        void Update()
        {

        }

        //private Transform _root_grid = null;
        //private Transform _root_unit = null;
        //private Animator  _hero_animator = null;
        //private void LoadComponent()
        //{
        //    const string ROOT_GRID = "0_grid";
        //    const string ROOT_UNIT = "0_unit";
        //    const string HERO_LOTHAR = "Lothar";
        //    foreach (Transform t in this.GetComponentsInChildren<Transform>(true))
        //    {
        //        if (ROOT_GRID == t.name)
        //        {
        //            _root_grid = t;
        //        }
        //        else if (ROOT_UNIT == t.name)
        //        {
        //            _root_unit = t;
        //        }
        //        else if (HERO_LOTHAR == t.name)
        //        {
        //            _hero_animator = t.GetComponent<Animator>();
        //        }
        //
        //        if (null != _root_grid && null != _root_unit && null != _hero_animator)
        //            break;
        //    }
        //    //Debug.Log("dddd " + _root_unit + _root_grid + _hero_animator); //chamto test
        //}
    }

}

//========================================================
//==================      전역  객체      ==================
//========================================================
namespace HordeFight
{
    public static class Single
    {

       
        private static System.Random _rand = new System.Random();
        static public System.Random rand
        {
            get { return _rand; }
        }

        public static TouchProcess touchProcess
        {
            get
            {
                return CSingletonMono<TouchProcess>.Instance;
            }
        }

        public static ObjectManager objectManager
        {
            get
            {
                return CSingleton<ObjectManager>.Instance;
            }
        }

        private static Camera _mainCamera = null;
        public static Camera mainCamera
        {
            get
            {
                if (null == _mainCamera)
                {

                    GameObject obj = GameObject.Find("Main Camera");
                    if (null != obj)
                    {
                        _mainCamera = obj.GetComponent<Camera>();
                    }

                }
                return _mainCamera;
            }
        }


        private static Canvas _canvasRoot = null;
        public static Canvas canvasRoot
        {
            get
            {
                if (null == _canvasRoot)
                {
                    GameObject obj = GameObject.Find("Canvas");
                    if (null != obj)
                    {
                        _canvasRoot = obj.GetComponent<Canvas>();
                    }

                }
                return _canvasRoot;
            }
        }


        private static Transform _gridRoot = null;
        public static Transform gridRoot
        {
            get
            {
                if (null == _gridRoot)
                {
                    GameObject obj = GameObject.Find("0_grid");
                    if (null != obj)
                    {
                        _gridRoot = obj.GetComponent<Transform>();
                    }

                }
                return _gridRoot;
            }
        }

        private static Transform _unitRoot = null;
        public static Transform unitRoot
        {
            get
            {
                if (null == _unitRoot)
                {
                    GameObject obj = GameObject.Find("0_unit");
                    if (null != obj)
                    {
                        _unitRoot = obj.GetComponent<Transform>();
                    }

                }
                return _unitRoot;
            }
        }


    }

}//end namespace



//========================================================
//==================      객체 관리기      ==================
//========================================================

namespace HordeFight
{

    public class ObjectManager
    {
        public List<Character> _characters = new List<Character>();


        public void ClearAll()
        {

            foreach (Character t in _characters)
            {
                GameObject.Destroy(t.gameObject);
            }

            _characters.Clear();

        }

        public Character GetCharacter(int id)
        {
            foreach (Character c in _characters)
            {
                if (c._id == id)
                {
                    return c;
                }
            }

            return null;
        }

        public Movable GetCharacterMove(int id)
        {
            foreach (Character c in _characters)
            {
                if (c._id == id)
                {
                    return c.GetComponent<Movable>();
                }
            }

            return null;
        }

        //최대 반경이내에서 가장 가까운 객체를 반환한다
        public Transform GetNearCharacter(Transform exceptChar, float maxRadius)
        {

            //todo : 추후구현하기

            foreach (Character t in _characters)
            {
                if (t.transform == exceptChar) continue;

                return t.transform;
            }

            return null;
        }


       

        //____________________________________________
        //                  객체 생성 
        //____________________________________________

        public GameObject CreatePrefab(string prefabPath, Transform parent, string name)
        {
            const string root = "Warcraft/Prefab/";
            GameObject obj = MonoBehaviour.Instantiate(Resources.Load(root + prefabPath)) as GameObject;
            obj.transform.SetParent(parent, false);
            obj.transform.name = name;


            return obj;
        }

       

        public Character Create_Character(Transform parent, Character.eKind eKind,int id, Vector3 pos)
        {
            GameObject obj = CreatePrefab(eKind.ToString(), parent, id.ToString("000") + "_" + eKind.ToString());
            Character cha = obj.AddComponent<Character>();
            obj.AddComponent<Movable>();
            cha._id = id;
            cha._eKind = eKind;
            cha.transform.localPosition = pos;
            cha.Init_Create();

            _characters.Add(cha);

            return cha;
        }

        public void Create_StageInfo()
        {
            Vector3 pos = new Vector3(0.5f,0.5f,0);
            Create_Character(Single.unitRoot, Character.eKind.lothar, 0, pos);
        }

    }

    public class Character : MonoBehaviour
    {
        
        private Animator                    _animator = null;
        private AnimatorOverrideController  _overCtr = null;
        private AnimationClip[]             _aniClips = null;

        private Movable _move     = null;
        private eDirection _eDir8 = eDirection.down;

        public int      _id = -1;
        public eKind    _eKind = eKind.none;
       

        public enum eKind
        {
            none = 0,
            footman,
            lothar,
            skeleton,
            garona,
            conjurer,
            raider,
            slime,
            spearman,
            grunt,
            brigand,
            knight,
            ogre,

        }

        public enum eState
        {
            None = 0,
            Idle = 1,
            Move = 2,
            Attack = 3,
            FallDown = 4,
        }


        //데카르트좌표계 사분면을 기준으로 숫자 지정
        public enum eDirection : int
        {
            none        = 0,
            right       = 1,
            rightUp     = 2,
            up          = 3,
            leftUp      = 4,
            left        = 5,
            leftDown    = 6,
            down        = 7,
            rightDown   = 8,

        }

		public void Init_Create()
		{
            _animator = GetComponent<Animator>();
            _move = GetComponent<Movable>();
            //Single.touchProcess.Attach_SendObject(this.gameObject);

            //오버라이드컨트롤러를 생성해서 추가하지 않고, 미리 생성된 것을 쓰면 객체하나의 애니정보가 바뀔때 다른 객체의 애니정보까지 모두 바뀌게 된다. 
            _overCtr = new AnimatorOverrideController(_animator.runtimeAnimatorController);
            _overCtr.name = "divide_character";
            _animator.runtimeAnimatorController = _overCtr;


            _aniClips = Resources.LoadAll<AnimationClip>("Warcraft/Animation"); //todo : 리소스관리기로 분리시키기 
		}

		//ref : https://docs.unity3d.com/ScriptReference/AnimatorOverrideController.html
		private void Start()
        {
            _animator.SetInteger("state", (int)eState.Idle);
        }


        private float __elapsedTime = 0f;
        private float __randTime = 0f;
		private void Update()
		{

            if((int)eState.Idle == _animator.GetInteger("state"))
            {
                __elapsedTime += Time.deltaTime;


                if(__randTime < __elapsedTime)
                {
                    _eDir8 = (eDirection)Single.rand.Next(1, 8);
                    Switch_AniMove("base_idle", _eKind.ToString() + "_idle_", _eDir8);

                    __elapsedTime = 0f;

                    //3~7초가 지났을 때 돌아감
                    __randTime = (float)Single.rand.Next(3, 7);
                }

            }


		}

        public AnimationClip GetClip(string name)
        {
            foreach(AnimationClip ani in _aniClips)
            {
                if(ani.name.Equals(name))
                {
                    return ani;
                }
            }

            return null;
        }

        public eDirection TransDirection(Vector3 dir)
        {
            float rad = Mathf.Atan2(dir.y, dir.x); 
            float deg = Mathf.Rad2Deg * rad;

            //각도가 음수라면 360을 더한다 
            if (deg < 0) deg += 360f;

            //360 / 45 = 8
            int quad = Mathf.RoundToInt(deg / 45f);
            quad %= 8; //8 => 0 , 8을 0으로 변경  
            quad++; //값의 범위를 0~7 에서 1~8로 변경 
            //DebugWide.LogRed(deg + "   " + quad);

            return (eDirection)quad;
        }


        public void Switch_AniMove(string aniKind, string aniName , eDirection dir)
        {
            
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            sr.flipX = false;
            string aniNameSum = "";
            switch(dir)
            {
                
                case eDirection.leftUp:
                    {
                        aniNameSum = aniName + eDirection.rightUp.ToString();
                        sr.flipX = true;
                    }
                    break;
                case eDirection.left:
                    {
                        aniNameSum = aniName + eDirection.right.ToString();
                        sr.flipX = true;
                    }
                    break;
                case eDirection.leftDown:
                    {
                        aniNameSum = aniName + eDirection.rightDown.ToString();
                        sr.flipX = true;
                    }
                    break;
                default:
                    {
                        aniNameSum = aniName + dir.ToString();
                        sr.flipX = false;
                    }
                    break;
                
            }

            _overCtr[aniKind] = GetClip(aniNameSum);
        }


        private Vector2 _startPos = Vector3.zero;
        private void TouchBegan() 
        {
            //DebugWide.LogBlue("TouchBegan " + Single.touchProcess.GetTouchPos());

            _animator.speed = 1f;


            RaycastHit2D hit = Single.touchProcess.GetHit2D();

            _startPos = hit.point;

            _animator.SetInteger("state", (int)eState.Move);

        }


        private void TouchMoved()
        {
            //DebugWide.LogBlue("TouchMoved " + Single.touchProcess.GetTouchPos());

            RaycastHit2D hit = Single.touchProcess.GetHit2D();

            Vector3 dir = (Vector3)hit.point - this.transform.position;
            //dir.Normalize();
            _move.Move_Forward(dir, 1f, 1f);

          


            _eDir8 = TransDirection(dir);
            Switch_AniMove("base_move",_eKind.ToString()+"_move_",_eDir8);
            //Switch_AniMove("base_move", "lothar_attack_", eDir);
            _animator.SetInteger("state", (int)eState.Move);


        }

        private void TouchEnded() 
        {
            //DebugWide.LogBlue("TouchEnded " + Single.touchProcess.GetTouchPos());

            Switch_AniMove("base_idle", _eKind.ToString()+"_idle_", _eDir8);
            _animator.SetInteger("state", (int)eState.Idle);
        }
    }



    public class Movable : MonoBehaviour
    {
        public enum eDirection
        {
            None    = 0,
            UP      = 1<<1,
            DOWN    = 1<<2,
            LEFT    = 1<<3,
            RIGHT   = 1<<4,
        }



        public void Move_Forward(Vector3 dir , float distance , float speed)
        {
            
            //보간, 이동 처리
            //float delta = Interpolation.easeInOutBack(0f, 0.2f, accumulate / MAX_SECOND);
            this.transform.Translate(dir * Time.deltaTime * speed * distance);

           
        }

        public void Move_Backward(Vector3 dir, float speed)
        {
            
            this.transform.Translate(-dir * Time.deltaTime * speed); 
        }


        public Vector3 GetDirect(Vector3 dstPos)
        {
            Vector3 dir = dstPos - this.transform.position;
            dir.Normalize();
            return dir;
        }

        //객체의 전진 방향을 반환한다.
        public Vector3 GetForwardDirect()
        {
            return Quaternion.Euler(this.transform.localEulerAngles) * Vector3.forward;
        }


        //내방향을 기준으로 목표위치가 어디쪽에 있는지 반환한다.  
        //public eDirection DirectionalInspection(Vector3 targetPos)
        //{

        //    Vector3 mainDir = GetForwardDirect();

        //    Vector3 targetTo = targetPos - this.transform.localPosition;

        //    //mainDir.Normalize();
        //    //targetTo.Normalize();

        //    Vector3 dir = Vector3.Cross(mainDir, targetTo);
        //    //dir.Normalize();
        //    //DebugWide.LogBlue("mainDir:" + mainDir + "  targetTo:" + targetTo + "   cross:" + dir.y);

        //    float angle = Vector3.Angle(mainDir, targetTo);
        //    angle = Mathf.Abs(angle);

        //    if (angle < 3f) return eDirection.CENTER; //사이각이 3도 보다 작다면 중앙으로 여긴다 

        //    //외적의 y축값이 음수는 왼쪽방향 , 양수는 오른쪽방향 
        //    if (dir.y < 0)
        //        return eDirection.LEFT;
        //    else if (dir.y > 0)
        //        return eDirection.RIGHT;

        //    return eDirection.CENTER;
        //}


        //회전할 각도 구하기
        public float CalcRotationAngle(Vector3 targetDir)
        {
            //atan2로 각도 구하는 것과 같음. -180 ~ 180 사이의 값을 반환
            return Vector3.SignedAngle(GetForwardDirect(), targetDir, Vector3.up);

        }
    }
}


//가속도계 참고할것  :  https://docs.unity3d.com/kr/530/Manual/MobileInput.html
//마우스 시뮬레이션  :  https://docs.unity3d.com/kr/530/ScriptReference/Input.html   마우스 => 터치로 변환
//========================================================
//==================      터치  처리      ==================
//========================================================
namespace HordeFight
{

    public class TouchProcess : MonoBehaviour
    {

        private GameObject _TouchedObject = null;
        private List<GameObject> _sendList = new List<GameObject>();

        private Vector2 _prevTouchMovedPos = Vector3.zero;
        public Vector2 prevTouchMovedPos
        {
            get
            {
                return _prevTouchMovedPos;
            }
        }



        void Awake()
        {

            Input.simulateMouseWithTouches = false;
            Input.multiTouchEnabled = false;

        }

        // Use this for initialization
        void Start()
        {


        }


        //void Update()
        void FixedUpdate()
        {
            //화면상 ui를 터치했을 경우 터치이벤트를 보내지 않는다 
            if (null != EventSystem.current && null != EventSystem.current.currentSelectedGameObject)
            {
                return;
            }

            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                SendTouchEvent_Device_Target();
                SendTouchEvent_Device_NonTarget();
            }
            else if (Application.platform == RuntimePlatform.OSXEditor)
            {
                SendMouseEvent_Editor_Target();
                SendMouseEvent_Editor_NonTarget();
            }
        }

        //==========================================
        //                 보조  함수
        //==========================================

        public void Attach_SendObject(GameObject obj)
        {
            _sendList.Add(obj);
        }

        public void Detach_SendObject(GameObject obj)
        {
            _sendList.Remove(obj);
        }

        public void DetachAll()
        {
            _sendList.Clear();
        }


        public Vector2 GetTouchPos()
        {
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return Input.GetTouch(0).position;
            }
            else if (Application.platform == RuntimePlatform.OSXEditor)
            {
                return Input.mousePosition;
            }

            return Vector2.zero;
        }

        public bool GetMouseButtonMove(int button)
        {
            if (Input.GetMouseButton(button) && Input.GetMouseButtonDown(button) == false)
            {
                return true;
            }

            return false;
        }

        //충돌체가 2d 면 Physics2D 함수로만 찾아낼 수 있다.  2d객체에 3d충돌체를 넣으면 Raycast(3D) 함수로 찾아낼 수 있다. 
        public RaycastHit2D GetHit2D()
        {
            Ray ray = Camera.main.ScreenPointToRay(this.GetTouchPos());

            return Physics2D.Raycast(ray.origin, ray.direction);
        }

        public RaycastHit GetHit3D()
        {
            Ray ray = Camera.main.ScreenPointToRay(this.GetTouchPos());

            RaycastHit hit3D = default(RaycastHit);
            Physics.Raycast(ray, out hit3D, Mathf.Infinity);

            return hit3D;
        }

        //==========================================
        //                 이벤트  함수
        //==========================================

        private void SendTouchEvent_Device_NonTarget()
        {
            foreach (GameObject o in _sendList)
            {
                if (Input.touchCount > 0)
                {
                    if (Input.GetTouch(0).phase == TouchPhase.Began)
                    {

                        o.SendMessage("TouchBegan", 0, SendMessageOptions.DontRequireReceiver);

                    }
                    else if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary)
                    {

                        o.SendMessage("TouchMoved", 0, SendMessageOptions.DontRequireReceiver);

                    }
                    else if (Input.GetTouch(0).phase == TouchPhase.Ended)
                    {

                        o.SendMessage("TouchEnded", 0, SendMessageOptions.DontRequireReceiver);

                    }
                    else
                    {
                        DebugWide.LogError("Update : Exception Input Event : " + Input.GetTouch(0).phase);
                    }
                }
            }

        }

        private bool __touchBegan = false;
        private void SendMouseEvent_Editor_NonTarget()
        {
            foreach (GameObject o in _sendList)
            {
                //=================================
                //    mouse Down
                //=================================
                if (Input.GetMouseButtonDown(0))
                {
                    //DebugWide.LogBlue("SendMouseEvent_Editor_NonTarget : TouchPhase.Began"); //chamto test
                    if (false == __touchBegan)
                    {
                        o.SendMessage("TouchBegan", 0, SendMessageOptions.DontRequireReceiver);

                        __touchBegan = true;

                    }
                }

                //=================================
                //    mouse Up
                //=================================
                if (Input.GetMouseButtonUp(0))
                {

                    if (true == __touchBegan)
                    {
                        __touchBegan = false;

                        o.SendMessage("TouchEnded", 0, SendMessageOptions.DontRequireReceiver);
                    }

                }


                //=================================
                //    mouse Move
                //=================================
                if (this.GetMouseButtonMove(0))
                {

                    //=================================
                    //     mouse Drag 
                    //=================================
                    if (true == __touchBegan)
                    {

                        o.SendMessage("TouchMoved", 0, SendMessageOptions.DontRequireReceiver);

                    }//if
                }//if
            }

        }

        private void SendTouchEvent_Device_Target()
        {
            if (Input.touchCount > 0)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    //DebugWide.LogError("Update : TouchPhase.Began"); //chamto test
                    _prevTouchMovedPos = this.GetTouchPos();
                    _TouchedObject = SendMessage_TouchObject("TouchBegan", Input.GetTouch(0).position);
                }
                else if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary)
                {
                    //DebugWide.LogError("Update : TouchPhase.Moved"); //chamto test

                    if (null != _TouchedObject)
                        _TouchedObject.SendMessage("TouchMoved", 0, SendMessageOptions.DontRequireReceiver);

                    _prevTouchMovedPos = this.GetTouchPos();

                }
                else if (Input.GetTouch(0).phase == TouchPhase.Ended)
                {
                    //DebugWide.LogError("Update : TouchPhase.Ended"); //chamto test
                    if (null != _TouchedObject)
                        _TouchedObject.SendMessage("TouchEnded", 0, SendMessageOptions.DontRequireReceiver);
                    _TouchedObject = null;
                }
                else
                {
                    DebugWide.LogError("Update : Exception Input Event : " + Input.GetTouch(0).phase);
                }
            }
        }


        private bool f_isEditorDraging = false;
        private void SendMouseEvent_Editor_Target()
        {

            //=================================
            //    mouse Down
            //=================================
            //Debug.Log("mousedown:" +Input.GetMouseButtonDown(0)+ "  mouseup:" + Input.GetMouseButtonUp(0) + " state:" +Input.GetMouseButton(0)); //chamto test
            if (Input.GetMouseButtonDown(0))
            {
                //Debug.Log ("______________ MouseBottonDown ______________" + m_TouchedObject); //chamto test
                if (false == f_isEditorDraging)
                {

                    _TouchedObject = SendMessage_TouchObject("TouchBegan", Input.mousePosition);
                    if (null != _TouchedObject)
                        f_isEditorDraging = true;
                }

            }

            //=================================
            //    mouse Up
            //=================================
            if (Input.GetMouseButtonUp(0))
            {

                //Debug.Log ("______________ MouseButtonUp ______________" + m_TouchedObject); //chamto test
                f_isEditorDraging = false;

                if (null != _TouchedObject)
                {
                    _TouchedObject.SendMessage("TouchEnded", 0, SendMessageOptions.DontRequireReceiver);
                }

                _TouchedObject = null;

            }


            //=================================
            //    mouse Move
            //=================================
            if (this.GetMouseButtonMove(0))
            {

                //=================================
                //     mouse Drag 
                //=================================
                if (f_isEditorDraging)
                {
                    //Debug.Log ("______________ MouseMoved ______________" + m_TouchedObject); //chamto test

                    if (null != _TouchedObject)
                        _TouchedObject.SendMessage("TouchMoved", 0, SendMessageOptions.DontRequireReceiver);


                }//if
            }//if
        }


        private GameObject SendMessage_TouchObject(string callbackMethod, Vector3 touchPos)
        {

            Ray ray = Camera.main.ScreenPointToRay(touchPos);

            //Debug.Log ("  -- currentSelectedGameObject : " + EventSystem.current.currentSelectedGameObject); //chamto test

            //2. game input event test
            RaycastHit2D hit2D = Physics2D.Raycast(ray.origin, ray.direction);
            if (null != hit2D.collider)
            {
                //DebugWide.Log(hit.transform.gameObject.name); //chamto test
                hit2D.transform.gameObject.SendMessage(callbackMethod, 0, SendMessageOptions.DontRequireReceiver);

                return hit2D.transform.gameObject;
            }



            RaycastHit hit3D = default(RaycastHit);
            if (true == Physics.Raycast(ray, out hit3D, Mathf.Infinity))
            {
                DebugWide.LogBlue("SendMessage_TouchObject 2");
                hit3D.transform.gameObject.SendMessage(callbackMethod, 0, SendMessageOptions.DontRequireReceiver);

                return hit3D.transform.gameObject;
            }


            return null;
        }

        ////콜백함수 원형 : 지정 객체 아래로 터치이벤트를 보낸다 
        //private void TouchBegan() { }
        //private void TouchMoved() { }
        //private void TouchEnded() { }

        ////콜백함수 원형 : 지정 객체에 터치이벤트를 보낸다 
        //private void TouchBegan() { }
        //private void TouchMoved() { }
        //private void TouchEnded() { }


    }

}//end namespace

