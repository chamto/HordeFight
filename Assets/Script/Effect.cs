using UnityEngine;

using UtilGS9;

namespace HordeFight
{
    public class Effect
    {
        public enum eKind
        {
            Aim,        //조준점
            Dir,        //방향
            Emotion,    //감정표현
            Circle,     //선택원
            Bar_Red,    //생명바
            Bar_Blue,   //체력바

            Max,
        }

        //전용effect
        private Transform[] _effect = new Transform[(int)eKind.Max];

        public SpriteRenderer _bar_red = null;
        public SpriteRenderer _bar_blue = null;

        public void Init(Transform parent)
        {

            // 전용 effect 설정 
            //_effect[(int)eEffectKind.Aim] = SingleO.hierarchy.GetTransformA(_owner._transform, "effect/aim");
            //_effect[(int)eEffectKind.Dir] = SingleO.hierarchy.GetTransformA(_owner._transform, "effect/dir");
            //_effect[(int)eEffectKind.Emotion] = SingleO.hierarchy.GetTransformA(_owner._transform, "effect/emotion");
            //_effect[(int)eEffectKind.Hand_Left] = SingleO.hierarchy.GetTransformA(_owner._transform, "effect/hand_left");
            //_effect[(int)eEffectKind.Hand_Right] = SingleO.hierarchy.GetTransformA(_owner._transform, "effect/hand_right");


            Transform effectTr = Hierarchy.GetTransform(parent, "effectIcon");
            _effect[(int)eKind.Aim] = Hierarchy.GetTransform(effectTr, "aim", true);
            _effect[(int)eKind.Dir] = Hierarchy.GetTransform(effectTr, "dir", true);
            _effect[(int)eKind.Emotion] = Hierarchy.GetTransform(effectTr, "emotion", true);
            _effect[(int)eKind.Circle] = Hierarchy.GetTransform(effectTr, "circle", true);
            _effect[(int)eKind.Bar_Red] = Hierarchy.GetTransform(effectTr, "bar_red", true);
            _effect[(int)eKind.Bar_Blue] = Hierarchy.GetTransform(effectTr, "bar_blue", true);

            _bar_red = Hierarchy.GetTypeObject<SpriteRenderer>(_effect[(int)eKind.Bar_Red], "spr");
            _bar_blue = Hierarchy.GetTypeObject<SpriteRenderer>(_effect[(int)eKind.Bar_Blue], "spr");

            //아틀라스에서 가져온 sprite로 변경하여 시험 
            //_effect[(int)eEffectKind.Aim].sprite = SingleO.resourceManager.GetSprite_Effect("aim_1");
            //_effect[(int)eEffectKind.Dir].sprite = SingleO.resourceManager.GetSprite_Effect("effect_dir");
            //_effect[(int)eEffectKind.Emotion].sprite = SingleO.resourceManager.GetSprite_Effect("effect_surprise");
            //_effect[(int)eEffectKind.Hand_Left].sprite = SingleO.resourceManager.GetSprite_Effect("effect_sheld_0");
            //_effect[(int)eEffectKind.Hand_Right].sprite = SingleO.resourceManager.GetSprite_Effect("effect_attack");
            //DebugWide.LogBlue(_effect[(int)eEffectKind.Dir].sprite.name); //chamto test
        }

        public void SetActive(eKind kind, bool value)
        {
            _effect[(int)kind].gameObject.SetActive(value);
        }

        public void Apply_BarRed(float rate)
        {
            //HP갱신 
            //_ui_hp.SetLineHP((float)_hp_cur / (float)_hp_max);

            //Vector2 temp = _effect[(int)eEffectKind.Bar_Red].size;
            //temp.x = (float)_hp_cur / (float)_hp_max;
            //_effect[(int)eEffectKind.Bar_Red].size = temp;

            if (null == (object)_bar_red) return;

            Vector2 temp = _bar_red.size;
            temp.x = rate;
            _bar_red.size = temp;

        }

    }
}
