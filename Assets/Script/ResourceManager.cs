using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
//using UnityEngine.Assertions;
using UnityEngine.U2D;

using UtilGS9;



//========================================================
//==================     리소스 관리기     ==================
//========================================================
namespace HordeFight
{
    public class ResourceManager
    {

        public RuntimeAnimatorController _base_Animator = null;

        //해쉬맵 : 애니메이션 이름으로 해쉬키를 생성
        private Dictionary<int, AnimationClip> _hashKeyClips = new Dictionary<int, AnimationClip>();

        //Being.eKind, eAniBaseKind, eDirection8 : 3가지 값으로 키를 생성
        private Dictionary<uint, AnimationClip> _multiKeyClips = new Dictionary<uint, AnimationClip>();

        //기본 동작 AniClip 목록 : base_idle , base_move , base_attack , base_fallDown
        private AnimationClip[] _baseAniClips = null;

        public Dictionary<int, Sprite> _sprEffect = new Dictionary<int, Sprite>();
        public Dictionary<int, Sprite> _sprIcons = new Dictionary<int, Sprite>();
        public Dictionary<int, TileBase> _tileScripts = new Dictionary<int, TileBase>();

        public SpriteAtlas _atlas_etc = null;

        //==================== Get / Set ====================

        public AnimationClip GetBaseAniClip(eAniBaseKind baseKind)
        {
            if (eAniBaseKind.MAX <= baseKind) return null;

            return _baseAniClips[(int)baseKind];
        }

        //==================== <Method> ====================

        public void ClearAll()
        {

        }

        public void Init()
        {
            Load_Animation();
        }


        public void Load_Animation()
        {

            //=============================================
            //LOAD 
            //=============================================
            _base_Animator = Resources.Load<RuntimeAnimatorController>("Warcraft/Animation/base_Animator");


            AnimationClip[] loaded = Resources.LoadAll<AnimationClip>("Warcraft/Animation");
            foreach (AnimationClip ac in loaded)
            {
                //ac.GetHashCode 값과 ac.name.GetHashCode 값은 다르다
                _hashKeyClips.Add(ac.name.GetHashCode(), ac);

                uint multiKey = ComputeAniMultiKey(ac.name);
                if (0 != multiKey) //0 멀티키는 키생성에 문제가 있다는 것이다  
                {
                    _multiKeyClips.Add(multiKey, ac);
                }
                //else
                //{
                //    DebugWide.LogBlue(ac.name); //chamto test
                //}

            }
            _baseAniClips = ConstV.FindAniBaseClips(loaded);


            //DebugWide.LogBlue(spriteAtlas.spriteCount);
            //spriteAtlas.GetSprite()

            _atlas_etc = Resources.Load<SpriteAtlas>("Warcraft/Textures/Atlas/etc");

            //Sprite[] spres = Resources.LoadAll<Sprite>("Warcraft/Textures/ETC/effect");
            //foreach (Sprite spr in spres)
            //{
            //    _sprEffect.Add(spr.name.GetHashCode(), spr);
            //}

            //Sprite[] spres = Resources.LoadAll<Sprite>("Warcraft/Textures/ETC/Icons");
            //foreach(Sprite spr in spres)
            //{
            //    _sprIcons.Add(spr.name.GetHashCode(), spr);
            //}


            TileBase[] tiles = Resources.LoadAll<TileBase>("Warcraft/Palette/ScriptTile");
            foreach (TileBase r in tiles)
            {
                _tileScripts.Add(r.name.GetHashCode(), r);
                //DebugWide.LogBlue(r.name);
            }
        }

        public Sprite GetSprite_Effect(string spr_name)
        {
            int hash = spr_name.GetHashCode();
            if (false == _sprEffect.Keys.Contains(hash))
            {
                Sprite sprite = _atlas_etc.GetSprite(spr_name);
                if (null != sprite)
                    _sprEffect.Add(hash, sprite);
            }

            return _sprEffect[hash];
        }

        public Sprite GetSprite_Icons(string spr_name)
        {
            int hash = spr_name.GetHashCode();
            if (false == _sprIcons.Keys.Contains(hash))
            {
                Sprite sprite = _atlas_etc.GetSprite(spr_name);
                if (null != sprite)
                    _sprIcons.Add(hash, sprite);
            }

            return _sprIcons[hash];
        }

        public TileBase GetTileScript(int nameToHash)
        {
            if (true == _tileScripts.ContainsKey(nameToHash))
            {
                return _tileScripts[nameToHash];
            }

            return null;
        }

        public AnimationClip GetClip(int nameToHash)
        {
            AnimationClip animationClip = null;
            _hashKeyClips.TryGetValue(nameToHash, out animationClip);

            //DebugWide.LogRed(animationClip + "   " + Single.resourceManager._aniClips.Count); //chamto test


            return animationClip;
        }

        public AnimationClip GetClip(Being.eKind being_kind, eAniBaseKind ani_kind, eDirection8 dir)
        {
            uint multiKey = ComputeAniMultiKey(being_kind, ani_kind, dir);
            AnimationClip animationClip = null;
            _multiKeyClips.TryGetValue(multiKey, out animationClip);

            return animationClip;
        }

        public Being.eKind StringTo_BeingKind(string str)
        {
            return (Being.eKind)Enum.Parse(typeof(Being.eKind), str, true);
        }

        public eAniBaseKind StringTo_AniBaseKind(string str)
        {
            return (eAniBaseKind)Enum.Parse(typeof(eAniBaseKind), str, true);

        }

        public eDirection8 StringTo_Direction8(string str)
        {
            return (eDirection8)Enum.Parse(typeof(eDirection8), str, true);
        }

        public uint ComputeAniMultiKey(string aniName)
        {
            //DebugWide.LogBlue(aniName); //chamto test

            string[] temps = aniName.Split('_');

            if (3 != temps.Length)
            {
                return 0;
            }

            try
            {
                Being.eKind being_kind = StringTo_BeingKind(temps[0]);
                eAniBaseKind ani_kind = StringTo_AniBaseKind(temps[1]);
                eDirection8 dir = StringTo_Direction8(temps[2]);

                //DebugWide.LogBlue(aniName + " : " + being_kind.ToString() + "  " + ani_kind.ToString() + "  " + dir.ToString() + " : mkey : " + ComputeAniMultiKey(being_kind, ani_kind, dir)); //chamto test

                return ComputeAniMultiKey(being_kind, ani_kind, dir);
            }
            catch (ArgumentException e)
            {
                //Enum.Parse 수행시 들어온 문자열 값에 해당하는 열거형 값이 없을 경우, 이 예외가 발생한다 
                //DebugWide.LogException(e);
                return 0;
            }

        }

        public uint ComputeAniMultiKey(Being.eKind being_kind, eAniBaseKind ani_kind, eDirection8 dir)
        {
            //being_kind 천의 자리 : 최대 99999개
            //ani_kind 십의 자리 : 최대 99개
            //dir 일의 자리 : 최대 9개

            return (uint)being_kind * 1000 + (uint)ani_kind * 10 + (uint)dir;
        }


    }//end class


}
