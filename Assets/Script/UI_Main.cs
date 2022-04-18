using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.Assertions;




//========================================================
//==================   User Inteface   ==================
//========================================================
namespace HordeFight
{
    public class UI_Main : MonoBehaviour
    {
        public Text _fpsText = null;
        public Image _img_leader = null;

        public Text _gameText = null;
        public Button _reset = null;

        public void Init()
        {
            GameObject o = null;
            o = GameObject.Find("FPS");
            if (null != o)
            {
                _fpsText = o.GetComponentInChildren<Text>();
            }

            o = GameObject.Find("leader");
            if (null != o)
            {
                _img_leader = o.GetComponentInChildren<Image>();
            }

            o = GameObject.Find("GameText");
            if (null != o)
            {
                _gameText = o.GetComponentInChildren<Text>();
            }
            o = GameObject.Find("Reset");
            if (null != o)
            {
                _reset = o.GetComponentInChildren<Button>();
                _reset.onClick.AddListener(Reset_AllUnit_OnClick);
            }

        }

        //StringBuilder __sb = new StringBuilder(32);
        float __deltaTime = 0.0f;
        float __msec, __fps;
        
        int _fps2 = 0;
        float _deltaTime2 = 0;
        private void Update2()
        {

            __deltaTime += (Time.unscaledDeltaTime - __deltaTime) * 0.1f;

            __msec = __deltaTime * 1000.0f;
            __fps = 1.0f / __deltaTime;
            _fpsText.text = string.Format("{1:0.} fps ({0:0.0} ms)", __fps, __msec); //실제 프레임이 떨어지면 값이 더 크게나옴 , 엉터리 


            //StringBuilder 를 사용해도 GC가 발생함. 
            //__sb.Length = 0; //clear
            //_fpsText.text = __sb.AppendFormat("{0:0.0} ms ({1:0.} fps)", __msec, __fps).ToString();

            //_deltaTime2 += Time.deltaTime;
            //_fps2++;
            //if(1f < _deltaTime2)
            //{
            //    //DebugWide.LogBlue(_fps2);
            //    _deltaTime2 = 0;
            //    _fps2 = 0;
            //}
        }

        int __count = 0;
        public bool _loading = true;

        //ref : https://forum.unity.com/threads/fps-counter.505495/
        private float _hudRefreshRate = 1f;
        private float _timer;
        private void Update()
        {
            if (Time.unscaledTime > _timer)
            {
                int fps = (int)(1f / Time.unscaledDeltaTime);
                _fpsText.text = "FPS: " + fps;
                _timer = Time.unscaledTime + _hudRefreshRate;
            }

            if(true == _loading)
            {
                if (10 < __count)
                {
                    __count = 0;
                    _gameText.text = "loading ";
                }
                _gameText.text += ".";
                __count++;
            }


        }

        public void SelectLeader(string name)
        {
            if (null == _img_leader) return;

            Sprite spr = null;
            SingleO.resourceManager._sprIcons.TryGetValue(name.GetHashCode(), out spr);
            if (null == spr)
            {
                name = "None";
                spr = SingleO.resourceManager._sprIcons[name.GetHashCode()];
            }
            _img_leader.sprite = spr;

            //========================================

            foreach (RectTransform rt in _img_leader.GetComponentsInChildren<RectTransform>(true))
            {
                if ("back" == rt.name)
                {
                    rt.GetComponent<Image>().sprite = spr;
                }
                else if ("Text" == rt.name)
                {
                    rt.GetComponent<Text>().text = name;
                }

            }
        }

        public void Reset_AllUnit_OnClick()
        {
            SingleO.objectManager.Reset_AllUnit();
            DebugWide.LogRed("Reset_AllUnit!!"); 
        }

    }
}

