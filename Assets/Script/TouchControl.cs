using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

using UtilGS9;


//가속도계 참고할것  :  https://docs.unity3d.com/kr/530/Manual/MobileInput.html
//마우스 시뮬레이션  :  https://docs.unity3d.com/kr/530/ScriptReference/Input.html   마우스 => 터치로 변환
//========================================================
//==================      터치  처리      ==================
//========================================================
namespace HordeFight
{
    public class TouchControl : MonoBehaviour
    {
        public Being _selected = null;

        private void Start()
        {
            SingleO.touchEvent.Attach_SendObject(this.gameObject);
        }

        private void Update()
        {
            if (null == _selected) return;
            if (_selected.isDeath())
            {
                _selected = null;
            }

        }

        private Vector3 __startPos = ConstV.v3_zero;
        private void TouchBegan()
        {
            ChampUnit champ = null;
            RaycastHit hit = SingleO.touchEvent.GetHit3D();
            __startPos = hit.point;
            __startPos.y = 0f;

            //DebugWide.LogBlue(hit + "  " + hit.transform.name);
            Being getBeing = hit.transform.GetComponent<Being>();
            if (null != (object)getBeing)
            {
                //쓰러진 객체는 처리하지 않는다 
                if (true == getBeing.isDeath()) return;

                //전 객체 선택 해제 
                if (null != (object)_selected)
                {
                    champ = _selected as ChampUnit;
                    if (null != champ)
                    {
                        champ.GetComponent<AI>()._ai_running = true;
                        //SingleO.lineControl.SetActive(champ._UIID_circle_collider, false);
                        //champ._ui_circle.gameObject.SetActive(false);
                        //champ._ui_hp.gameObject.SetActive(false);
                    }


                }

                //새로운 객체 선택
                _selected = getBeing;

                champ = _selected as ChampUnit;
                if (null != (object)champ)
                {
                    _selected.GetComponent<AI>()._ai_running = false;
                    //SingleO.lineControl.SetActive(champ._UIID_circle_collider, true);
                    //champ._ui_circle.gameObject.SetActive(true);
                    //champ._ui_hp.gameObject.SetActive(true);
                }

                SingleO.cameraWalk.SetTarget(_selected._transform);
            }
            //else
            //{
            //    if (null != _selected)
            //    {
            //        SingleO.lineControl.SetActive(_selected._UIID_circle_collider, false);
            //    }
            //    _selected = null;
            //}


            //DebugWide.LogBlue(__startPos + "  " + _selected); //chamto test

            //===============================================

            if (null == (object)_selected) return;

            //챔프를 선택한 경우, 추가 처리 하지 않는다
            if ((object)getBeing == (object)_selected) return;

            //_selected.MoveToTarget(hit.point, 1f);


        }
        private void TouchMoved()
        {
            if (null == (object)_selected) return;

            RaycastHit hit = SingleO.touchEvent.GetHit3D();
            Vector3 touchDir = VOp.Minus(hit.point, _selected.GetPos3D());

            //_selected.Move_Forward(touchDir, 1f, true); //chamto test - 테스트 끝난후 주석풀기 

            ChampUnit champSelected = _selected as ChampUnit;
            champSelected.Attack(hit.point - _selected.transform.position); //테스트
            //champSelected.Block_Forward(hit.point - _selected.transform.position);
            if (null != (object)champSelected)
            {
                //임시처리 
                //최적화를 위해 주석처리 
                if (null != SingleO.objectManager)
                {
                    Being target = SingleO.objectManager.GetNearCharacter(champSelected, Camp.eRelation.Enemy,
                                                                      champSelected.attack_range_min, champSelected.attack_range_max);
                    if (null != target)
                    {
                        if (true == SingleO.objectManager.IsVisibleArea(champSelected, target.transform.position))
                        {
                            champSelected.Attack(target.GetPos3D() - _selected.GetPos3D(), target);
                        }

                        //_selected.Move_Forward(hit.point - _selected._getPos3D, 3f, true); 

                    }
                }


                //champSelected.Attack(champSelected._move._direction); //chamto test

            }


            //_selected._ani.Print_AnimatorState();
        }
        private void TouchEnded()
        {
            if (null == (object)_selected) return;

            //_selected.Idle();

        }

    }

    //==================      기본 터치 이벤트 처리      ==================

    public class TouchEvent : MonoBehaviour
    {

        //private GameObject _TouchedObject = null;
        private List<GameObject> _sendList = new List<GameObject>();

        private Vector2 _prevTouchMovedPos = ConstV.v3_zero;
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


        void Update()
        {
            //화면상 ui를 터치했을 경우 터치이벤트를 보내지 않는다 
            if (null != (object)EventSystem.current && null != (object)EventSystem.current.currentSelectedGameObject)
            {
                return;
            }

            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                //SendTouchEvent_Device_Target();
                SendTouchEvent_Device_NonTarget();
            }
            else //if (Application.platform == RuntimePlatform.OSXEditor )
            {
                //SendMouseEvent_Editor_Target();
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
            else //if (Application.platform == RuntimePlatform.OSXEditor)
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
            //foreach (GameObject o in _sendList)
            for (int i = 0; i < _sendList.Count; i++)
            {
                GameObject o = _sendList[i];
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
            //foreach (GameObject o in _sendList)
            for (int i = 0; i < _sendList.Count; i++)
            {
                GameObject o = _sendList[i];

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

        //==========================================

        //private void SendTouchEvent_Device_Target()
        //{
        //    if (Input.touchCount > 0)
        //    {
        //        if (Input.GetTouch(0).phase == TouchPhase.Began)
        //        {
        //            //DebugWide.LogError("Update : TouchPhase.Began"); //chamto test
        //            _prevTouchMovedPos = this.GetTouchPos();
        //            _TouchedObject = SendMessage_TouchObject("TouchBegan", Input.GetTouch(0).position);
        //        }
        //        else if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary)
        //        {
        //            //DebugWide.LogError("Update : TouchPhase.Moved"); //chamto test

        //            if (null != _TouchedObject)
        //                _TouchedObject.SendMessage("TouchMoved", 0, SendMessageOptions.DontRequireReceiver);

        //            _prevTouchMovedPos = this.GetTouchPos();

        //        }
        //        else if (Input.GetTouch(0).phase == TouchPhase.Ended)
        //        {
        //            //DebugWide.LogError("Update : TouchPhase.Ended"); //chamto test
        //            if (null != _TouchedObject)
        //                _TouchedObject.SendMessage("TouchEnded", 0, SendMessageOptions.DontRequireReceiver);
        //            _TouchedObject = null;
        //        }
        //        else
        //        {
        //            DebugWide.LogError("Update : Exception Input Event : " + Input.GetTouch(0).phase);
        //        }
        //    }
        //}


        //private bool f_isEditorDraging = false;
        //private void SendMouseEvent_Editor_Target()
        //{

        //    //=================================
        //    //    mouse Down
        //    //=================================
        //    //Debug.Log("mousedown:" +Input.GetMouseButtonDown(0)+ "  mouseup:" + Input.GetMouseButtonUp(0) + " state:" +Input.GetMouseButton(0)); //chamto test
        //    if (Input.GetMouseButtonDown(0))
        //    {
        //        //Debug.Log ("______________ MouseBottonDown ______________" + m_TouchedObject); //chamto test
        //        if (false == f_isEditorDraging)
        //        {

        //            _TouchedObject = SendMessage_TouchObject("TouchBegan", Input.mousePosition);
        //            if (null != _TouchedObject)
        //                f_isEditorDraging = true;
        //        }

        //    }

        //    //=================================
        //    //    mouse Up
        //    //=================================
        //    if (Input.GetMouseButtonUp(0))
        //    {

        //        //Debug.Log ("______________ MouseButtonUp ______________" + m_TouchedObject); //chamto test
        //        f_isEditorDraging = false;

        //        if (null != _TouchedObject)
        //        {
        //            _TouchedObject.SendMessage("TouchEnded", 0, SendMessageOptions.DontRequireReceiver);
        //        }

        //        _TouchedObject = null;

        //    }


        //    //=================================
        //    //    mouse Move
        //    //=================================
        //    if (this.GetMouseButtonMove(0))
        //    {

        //        //=================================
        //        //     mouse Drag 
        //        //=================================
        //        if (f_isEditorDraging)
        //        {
        //            //Debug.Log ("______________ MouseMoved ______________" + m_TouchedObject); //chamto test

        //            if (null != _TouchedObject)
        //                _TouchedObject.SendMessage("TouchMoved", 0, SendMessageOptions.DontRequireReceiver);


        //        }//if
        //    }//if
        //}

        //==========================================

        private GameObject SendMessage_TouchObject(string callbackMethod, Vector3 touchPos)
        {

            Ray ray = Camera.main.ScreenPointToRay(touchPos);

            //Debug.Log ("  -- currentSelectedGameObject : " + EventSystem.current.currentSelectedGameObject); //chamto test

            //2. game input event test
            RaycastHit2D hit2D = Physics2D.Raycast(ray.origin, ray.direction);
            if (null != hit2D.collider)
            {
                //DebugWide.Log(hit2D.transform.gameObject.name); //chamto test
                hit2D.transform.gameObject.SendMessage(callbackMethod, 0, SendMessageOptions.DontRequireReceiver);

                return hit2D.transform.gameObject;
            }



            RaycastHit hit3D = default(RaycastHit);
            if (true == Physics.Raycast(ray, out hit3D, Mathf.Infinity))
            {
                //DebugWide.LogBlue("SendMessage_TouchObject 2");
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
