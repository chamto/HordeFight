﻿using System;
using UnityEngine;


namespace UtilGS9
{
    public class SphereModel : IPoolConnector<SphereModel>
    {
        [FlagsAttribute]
        public enum Flag
        {
            NONE = 0,

            ROOTNODE = (1 << 0),    //트리의 최상위 노드 
            SUPERSPHERE = (1 << 1), //슈퍼구 : 다른 자식구를 포한한다 
            RECOMPUTE = (1 << 2),   //경계구를 다시 계산
            INTEGRATE = (1 << 3),   //대상구를 트리에 통합시킨다. 
                                    //  Level_2 트리의 슈퍼구일 경우 Level_1의 자식구로 똑같이 생성시킨다. 
                                    //  이 자식구를 Level_2의 슈퍼구에 연결시킨다  

            HIDDEN = (1 << 4), // outside of view frustum
            PARTIAL = (1 << 5), // partially inside view frustum
            INSIDE = (1 << 6),  // completely inside view frustum

            TREE_LEVEL_1 = (1 << 7),   //전체트리의 Level_1 을 구성하는 분리된 트리 
            TREE_LEVEL_2 = (1 << 8),   //전체트리의 Level_2 을 구성하는 분리된 트리 
            TREE_LEVEL_3 = (1 << 9),   //전체트리의 Level_3 을 구성하는 분리된 트리 
            TREE_LEVEL_4 = (1 << 10),   //전체트리의 Level_4 을 구성하는 분리된 트리 
            TREE_LEVEL_LAST = (1 << 11),

            TREE_LEVEL_ROOT = TREE_LEVEL_1,
            TREE_LEVEL_1234 = TREE_LEVEL_1 | TREE_LEVEL_2 | TREE_LEVEL_3 | TREE_LEVEL_4,

        }

        protected Vector3 _center;
        protected float _radius;
        protected float _radius_sqr;


        //------------------------------------------------------
        //                   메모리풀 전용 변수
        //------------------------------------------------------
        private int _id = -1;
        private bool _isUsed = false;
        private SphereModel _pool_next = null;
        private SphereModel _pool_prev = null;

        //------------------------------------------------------
        //                    트리 링크 변수
        //------------------------------------------------------
        private SphereModel _parent = null; //트리 깊이 위로
        private SphereModel _children = null;  //트리 깊이 아래로 
        private SphereModel _sibling_next = null; //트리 같은 차수 왼쪽으로
        private SphereModel _sibling_prev = null; //트리 같은 차수 오른쪽으로 


        private Flag _flags;
        private QFifo<SphereModel>.Out_Point _recompute_fifoOut;
        private QFifo<SphereModel>.Out_Point _intergrate_fifoOut;

        private SphereModel _link_upLevelTree = null; //윗 level 의 트리 링크 (전체 트리에서 보았을 때는 같은 level 의 노드링크임)
        private SphereModel _link_downLevelTree = null; //아랫 level 의 트리 링크 (전체 트리에서 보았을 때는 같은 level 의 노드링크임)


        //------------------------------------------------------

        private int _childCount = 0;
        private float _binding_distance_sqr = 0f;

        private SphereTree _treeController = null;

        //______________________________________________________________________________________________________

        //=====================================================
        //interface 구현 
        //=====================================================
        public void InitID(int id) { _id = id; }
        public int GetID() { return _id; }
        public void SetUsed(bool used) { _isUsed = used; }
        public bool IsUsed() { return _isUsed; }

        public SphereModel GetPoolNext() { return _pool_next; }
        public SphereModel GetPoolPrevious() { return _pool_prev; }

        public void SetPoolNext(SphereModel model) { _pool_next = model; }
        public void SetPoolPrevious(SphereModel model) { _pool_prev = model; }

        //=====================================================
        //구 정보 다루는 함수 
        //=====================================================
        public Vector3 GetPos() { return _center; }
        public void SetRadius(float radius)
        {
            _radius = radius;
            _radius_sqr = radius * radius;
        }

        public float GetRadius() { return _radius; }
        public float GetRadiusSqr() { return _radius_sqr; }
        public float ToDistanceSquared(SphereModel pack) { return (_center - pack._center).sqrMagnitude; }

        //=====================================================
        //Flag 열거값 다루는 함수
        //=====================================================
        public Flag GetFlag() { return _flags; }
        public void AddFlag(Flag flag) { _flags |= flag; }
        public void ClearFlag(Flag flag) { _flags &= ~flag; }
        public bool HasFlag(Flag flag)
        {
            if (0 != (_flags & flag)) return true;
            return false;
        }
        public int GetLevelIndex()
        {
            if (Flag.NONE != (_flags & Flag.TREE_LEVEL_1)) return 0;
            if (Flag.NONE != (_flags & Flag.TREE_LEVEL_2)) return 1;
            if (Flag.NONE != (_flags & Flag.TREE_LEVEL_3)) return 2;
            if (Flag.NONE != (_flags & Flag.TREE_LEVEL_4)) return 3;

            return -1;
        }

        //=====================================================
        //트리 링크 다루는 함수
        //=====================================================
        public void SetParent(SphereModel model) { _parent = model; }
        public SphereModel GetParent() { return _parent; }
        public SphereModel GetChildren() { return _children; }
        public void SetNextSibling(SphereModel child) { _sibling_next = child; }
        public void SetPrevSibling(SphereModel child) { _sibling_prev = child; }
        public SphereModel GetNextSibling() { return _sibling_next; }
        public SphereModel GetPrevSibling() { return _sibling_prev; }

        public void SetRecompute_FifoOut(QFifo<SphereModel>.Out_Point fifo_out) { _recompute_fifoOut = fifo_out; }
        public void SetIntergrate_FifoOut(QFifo<SphereModel>.Out_Point fifo_out) { _intergrate_fifoOut = fifo_out; }
        public void InitRecompute_FifoOut() { _recompute_fifoOut.Init(); }
        public void InitIntergrate_FifoOut() { _intergrate_fifoOut.Init(); }

        public void SetLink_UpLevelTree(SphereModel data) { _link_upLevelTree = data; }
        public SphereModel GetLink_UpLevelTree() { return _link_upLevelTree; }
        public void SetLink_DownLevelTree(SphereModel data) { _link_downLevelTree = data; }
        public SphereModel GetLink_DownLevelTree() { return _link_downLevelTree; }

        //======================================================
        public int GetChildCount() { return _childCount; }

        //=====================================================
        //초기화 
        //=====================================================
        public void Init(SphereTree controller, Vector3 pos, float radius)
        {
            _center = pos;
            SetRadius(radius);

            _parent = null;
            _children = null;
            _sibling_next = null;
            _sibling_prev = null;

            _flags = Flag.NONE;
            _recompute_fifoOut.Init();
            _intergrate_fifoOut.Init();
            _childCount = 0;
            _binding_distance_sqr = 0f;
            _link_upLevelTree = null;
            _link_downLevelTree = null;

            _treeController = controller;
        }


        public void SetPos(Vector3 pos)
        {
            _center = pos;

            if (null != _parent && false == HasFlag(Flag.INTEGRATE))
            {
                float sqrDist = ToDistanceSquared(_parent);

                //자식구가 슈퍼구에서 벗어났음
                if (sqrDist >= _binding_distance_sqr)
                {

                    if (false == _parent.HasFlag(Flag.RECOMPUTE))
                    {
                        _treeController.AddRecomputeQ(_parent); //슈퍼구 다시 계산  
                    }

                    Unlink();
                    _treeController.AddIntegrateQ(this); //자식구 어디에 통합시킬지 다시 계산
                }

            }

        }


        public void SetPosRadius(Vector3 pos, float radius)
        {
            //string temp = "1 -- " + pos + "  " + radius + "  == " + _flags.ToString()+" ==  ";

            _center = pos;

            if (null != _parent && false == HasFlag(Flag.INTEGRATE)) //첫번째 통합요청이 선점. 두번째 이상의 통합요청은 무시된다  
            {
                //DebugWide.LogBlue(GetID());
                if (Mathf.Epsilon < Mathf.Abs(radius - _radius))
                {
                    SetRadius(radius);
                    Compute_BindingDistanceSquared(_parent); //반지름 변경에 따른 슈퍼구와 묶인거리 다시 계산
                }

                float sqrDist = ToDistanceSquared(_parent);

                //자식구가 슈퍼구에서 벗어났음
                if (sqrDist >= _binding_distance_sqr)
                {
                    if (false == _parent.HasFlag(Flag.RECOMPUTE))
                    {
                        _treeController.AddRecomputeQ(_parent); //슈퍼구 다시 계산
                    }
                    Unlink();
                    _treeController.AddIntegrateQ(this); //자식구 어디에 통합시킬지 다시 계산
                }
                else
                {
                    if (false == _parent.HasFlag(Flag.RECOMPUTE))
                    {
                        _treeController.AddRecomputeQ(_parent); //슈퍼구 다시 계산
                    }
                }

            }

            //temp += "  | 2 -- " + _center + "  " + _radius + "   id:" + GetID(); //chamto test
            //DebugWide.LogBlue(temp);
        }

        public void AddChild(SphereModel pack)
        {

            SphereModel my_child = _children;
            _children = pack; // new head of list

            pack.SetNextSibling(my_child); // his next is my old next
            pack.SetPrevSibling(null); // at head of list, no previous
            pack.SetParent(this);

            if (null != my_child) my_child.SetPrevSibling(pack); // previous now this..

            _childCount++;

        }

        public void Unlink()
        {
            if (false == _recompute_fifoOut.IsNull())
            {
                _recompute_fifoOut.Unlink();
                _recompute_fifoOut.Init();
            }

            if (false == _intergrate_fifoOut.IsNull())
            {
                _intergrate_fifoOut.Unlink();
                _intergrate_fifoOut.Init();
            }

            if (null != _parent) _parent.LostChild(this);


            _parent = null;
        }

        public void LostChild(SphereModel model)
        {
            
            if (null == _children || 0 == _childCount) DebugWide.LogError("null == _children || 0 == _childCount"); //assert


            // first patch old linked list.. his previous now points to his next
            SphereModel prev = model.GetPrevSibling();

            if (null != prev)
            {
                SphereModel next = model.GetNextSibling();
                prev.SetNextSibling(next); // my previous now points to my next
                if (null != next) next.SetPrevSibling(prev);
                // list is patched!
            }
            else
            {
                SphereModel next = model.GetNextSibling();
                _children = next;
                if (null != _children) _children.SetPrevSibling(null);
            }

            _childCount--;

            //자식없는 슈퍼구는 제거한다 
            if (0 == _childCount && HasFlag(Flag.SUPERSPHERE))
            {
                _treeController.Remove_SuperSphereAndLinkSphere(this);
            }
        }

        //fixme
        //제약조건이 없으나, 인자값은 부모가 와야 한다. 의도에 맞게 함수를 수정할 필요가 있음 
        public void Compute_BindingDistanceSquared(SphereModel parent)
        {
            _binding_distance_sqr = parent.GetRadius() - GetRadius();
            if (_binding_distance_sqr <= 0) _binding_distance_sqr = 0;

            _binding_distance_sqr = _binding_distance_sqr * _binding_distance_sqr;
        }

        //슈퍼구만 계산의 대상이 된다
        public bool Recompute(float gravy)
        {
            if (null == _children) return true; // kill it!
            if (HasFlag(Flag.ROOTNODE)) return false; // don't recompute root nodes!

            //#if 1
            // recompute bounding sphere!
            Vector3 total = Vector3.zero;
            int count = 0;
            SphereModel pack = _children;
            while (null != pack)
            {
                total += pack._center;
                count++;
                pack = pack.GetNextSibling();
            }

            if (0 != count)
            {
                float recip = 1.0f / (float)(count);
                total *= recip;

                Vector3 oldpos = _center;

                _center = total; // new origin!
                float maxradius = 0;

                pack = _children;

                while (null != pack)
                {
                    float dist = ToDistanceSquared(pack);
                    float radius = Mathf.Sqrt(dist) + pack.GetRadius();
                    if (radius > maxradius)
                    {
                        maxradius = radius;
                        if ((maxradius + gravy) >= GetRadius())
                        {
                            _center = oldpos;
                            ClearFlag(Flag.RECOMPUTE);
                            return false;
                        }
                    }
                    pack = pack.GetNextSibling();
                }

                maxradius += gravy;

                SetRadius(maxradius); //최종 반지름 갱신

                // now all children have to recompute binding distance!!
                pack = _children;

                while (null != pack)
                {
                    pack.Compute_BindingDistanceSquared(this);
                    pack = pack.GetNextSibling();
                }

                //==============================================
                //!!!! 슈퍼구에 연결된 상위레벨 자식구도 갱신한다 
                SphereModel link = GetLink_UpLevelTree();
                if (null != link)
                {
                    link.SetPosRadius(_center, maxradius);
                    link.SetRadius(maxradius); //반지름이 갱신 안되는 경우가 있다 (통합상태)
                }
                //==============================================
            }

            //#endif

            ClearFlag(Flag.RECOMPUTE);

            return false;
        }

        public void ResetFlag()
        {
            ClearFlag(Flag.HIDDEN | Flag.PARTIAL | Flag.INSIDE);

            SphereModel pack = _children;
            while (null != pack)
            {
                pack.ResetFlag();
                pack = pack.GetNextSibling();
            }
        }

        //==================================================

        public SphereModel RayTrace_FirstReturn(Vector3 line_origin, Vector3 line_last , SphereModel exceptModel)
        {
            bool hit = false;
            SphereModel sm = null;
            if (HasFlag(Flag.SUPERSPHERE))
            {

                hit = Geo.IntersectRay(_center, _radius, line_origin, line_last - line_origin);
                if (hit)
                {
                    
                    SphereModel pack = _children;
                    while (null != pack)
                    {
                        sm = pack.RayTrace_FirstReturn(line_origin, line_last, exceptModel);
                        if (null != sm) return sm;


                        pack = pack.GetNextSibling();
                    }
                }

            }
            else
            {
                
                hit = Geo.IntersectLineSegment(_center, _radius, line_origin, line_last);
                if (hit)
                {
                    SphereModel upLink = this.GetLink_UpLevelTree();
                    SphereModel downLink = this.GetLink_DownLevelTree();


                    //전체트리의 최하위 자식노드
                    if (null == upLink && null == downLink) 
                    {
                        if(this != exceptModel)
                            return this;
                    }
                    //하위 레벨트리 검사
                    else if (null != downLink) 
                            return downLink.RayTrace_FirstReturn(line_origin, line_last, exceptModel);
                }
            }
            return null;
        }

        //==================================================
        // debug용
        //==================================================

        public void Debug_RayTrace(Vector3 line_origin, Vector3 line_last)
        {
            bool hit = false;

            if (HasFlag(Flag.SUPERSPHERE))
            {

                hit = Geo.IntersectRay(_center, _radius, line_origin, line_last - line_origin);
                if (hit)
                {
                    
                    DebugWide.DrawCircle(_center, GetRadius(), Color.gray);

                    SphereModel pack = _children;
                    while (null != pack)
                    {
                        //pack.RayTrace(p1, dir, distance);
                        pack.Debug_RayTrace(line_origin, line_last);
                        pack = pack.GetNextSibling();
                    }
                }

            }
            else
            {


                hit = Geo.IntersectLineSegment(_center, _radius, line_origin, line_last);
                if (hit)
                {
                    SphereModel upLink = this.GetLink_UpLevelTree();
                    SphereModel downLink = this.GetLink_DownLevelTree();
                    //if (null != downLink) downLink.RayTrace(line_origin, dir, distance);
                    if (null != downLink) downLink.Debug_RayTrace(line_origin, line_last);

                    Color cc = Color.gray;
                    if (null == upLink && null == downLink) cc = Color.red; //전체트리의 최하위 자식노드 
                    DebugWide.DrawCircle(_center, GetRadius(), cc);
                }
            }
        }

        public void Debug_RangeTest(Vector3 dstCenter, float dstRadius, Frustum.ViewState state)
        {
            if (state == Frustum.ViewState.PARTIAL)
            {
                float between_sqr = (dstCenter - _center).sqrMagnitude;

                //완전비포함 검사
                float sqrSumRd = (_radius + dstRadius) * (_radius + dstRadius);
                if (between_sqr > sqrSumRd) return;

                //완전포함 검사 
                if (dstRadius >= _radius)
                {
                    float sqrSubRd = (dstRadius - _radius) * (dstRadius - _radius);
                    if (between_sqr <= sqrSubRd) state = Frustum.ViewState.INSIDE; //슈퍼구가 포함되면 자식구까지 모두 포함되므로, 계산할 필요가 없어진다     
                }

            }

            if (HasFlag(Flag.SUPERSPHERE))
            {
                //#if DEMO
                if (state == Frustum.ViewState.PARTIAL)
                {
                    DebugWide.DrawCircle(_center, GetRadius(), Color.gray);
                }
                //#endif
                SphereModel pack = _children;
                while (null != pack)
                {
                    pack.Debug_RangeTest(dstCenter, dstRadius, state);
                    pack = pack.GetNextSibling();
                }

            }
            else
            {
                SphereModel upLink = this.GetLink_UpLevelTree();
                SphereModel downLink = this.GetLink_DownLevelTree();
                if (null != downLink) downLink.Debug_RangeTest(dstCenter, dstRadius, state);

                Color cc = Color.gray;
                if (null == upLink && null == downLink) cc = Color.red; //전체트리의 최하위 자식노드 
                DebugWide.DrawCircle(_center, GetRadius(), cc);
            }
        }

        public void Debug_VisibilityTest(Frustum f, Frustum.ViewState state)
        {
            if (state == Frustum.ViewState.PARTIAL)
            {
                state = f.ViewVolumeTest(ref _center, GetRadius());
                //#if DEMO
                if (state != Frustum.ViewState.OUTSIDE)
                {
                    DebugWide.DrawCircle(_center, GetRadius(), Color.gray);
                }
                //#endif
            }

            if (HasFlag(Flag.SUPERSPHERE))
            {

                if (state == Frustum.ViewState.OUTSIDE)
                {
                    if (HasFlag(Flag.HIDDEN)) return; // no state change
                    ClearFlag(Flag.INSIDE | Flag.PARTIAL);
                    AddFlag(Flag.HIDDEN);
                }
                else
                {
                    if (state == Frustum.ViewState.INSIDE)
                    {
                        if (HasFlag(Flag.INSIDE)) return; // no state change
                        ClearFlag(Flag.PARTIAL | Flag.HIDDEN);
                        AddFlag(Flag.INSIDE);
                    }
                    else
                    {
                        ClearFlag(Flag.HIDDEN | Flag.INSIDE);
                        AddFlag(Flag.PARTIAL);
                    }
                }

                SphereModel pack = _children;

                while (null != pack)
                {
                    pack.Debug_VisibilityTest(f, state);
                    pack = pack.GetNextSibling();
                }

            }
            else
            {
                SphereModel link = this.GetLink_DownLevelTree();
                switch (state)
                {
                    case Frustum.ViewState.INSIDE:
                        if (!HasFlag(Flag.INSIDE))
                        {
                            ClearFlag(Flag.HIDDEN | Flag.PARTIAL);
                            AddFlag(Flag.INSIDE);
                            //callback.VisibilityCallback(f, this, state);

                            if (null != link) link.Debug_VisibilityTest(f, state); //테스트 필요 
                            DebugWide.DrawCircle(_center, GetRadius(), Color.red);
                        }
                        break;
                    case Frustum.ViewState.OUTSIDE:
                        if (!HasFlag(Flag.HIDDEN))
                        {
                            ClearFlag(Flag.INSIDE | Flag.PARTIAL);
                            AddFlag(Flag.HIDDEN);
                            //callback.VisibilityCallback(f, this, state);

                            if (null != link) link.Debug_VisibilityTest(f, state); //테스트 필요 
                            DebugWide.DrawCircle(_center, GetRadius(), Color.black);
                        }
                        break;
                    case Frustum.ViewState.PARTIAL:
                        if (!HasFlag(Flag.PARTIAL))
                        {
                            ClearFlag(Flag.INSIDE | Flag.HIDDEN);
                            AddFlag(Flag.PARTIAL);
                            //callback.VisibilityCallback(f, this, state);

                            if (null != link) link.Debug_VisibilityTest(f, state); //테스트 필요 
                            DebugWide.DrawCircle(_center, GetRadius(), Color.blue);
                        }
                        break;
                }

            }
        }

        public void Debug_Render(bool isText)
        {

            if (null != _children)
            {
                SphereModel pack = _children;

                while (null != pack)
                {
                    pack.Debug_Render(isText);
                    pack = pack.GetNextSibling();
                }
            }

            if (false == HasFlag(Flag.ROOTNODE))
            {
                string temp = string.Empty;
                Color color = Color.green;

                if (HasFlag(Flag.SUPERSPHERE))
                {

                    DebugWide.DrawCircle(_center, GetRadius(), Color.green);

                    //if (HasFlag(Flag.TREE_LEVEL_2))
                    {

                        SphereModel link = GetLink_UpLevelTree();

                        if (null != link)
                            link = link.GetParent(); //level_1 트리의 슈퍼구노드를 의미한다 

                        if (null != link && false == link.HasFlag(Flag.ROOTNODE))
                        {
                            DebugWide.DrawLine(_center, link._center, Color.cyan);
                        }
                    }


                }
                else
                {
                    temp += "\n";
                    DebugWide.DrawCircle(_center, GetRadius(), Color.white);
                }

                if (true == isText)
                {
                    if (HasFlag(Flag.TREE_LEVEL_1)) { temp += ""; color = Color.magenta; }
                    if (HasFlag(Flag.TREE_LEVEL_2)) { temp += "\n        "; }
                    if (HasFlag(Flag.TREE_LEVEL_3)) { temp += "\n                "; }
                    if (HasFlag(Flag.TREE_LEVEL_4)) { temp += "\n                        "; }
                    if (HasFlag(Flag.SUPERSPHERE)) { temp += "s"; }

                    DebugWide.PrintText(_center, color, temp + GetID());
                }

            }

        }
    }


    //=================================================================
    //=================================================================
    //=================================================================
    //=================================================================



    public class SphereTree
    {
        //총 거리3 (루트 -> 자식1 -> 자식2)의 전체적인 트리를 구성한다 
        //트리의 거리별로 독립적인 트리를 구성한다. _level1 트리 한개 , _level2 트리 한개
        //_level1 의 자식구와 _level2 트리의 슈퍼구를 연결하여 외부에서는 하나의 트리인것 처럼 보이게 한다
        //private SphereModel _level_1 = null; //root : 트리의 거리(level) 1. 루트노드를 의미한다 
        //private SphereModel _level_2 = null; //leaf : 트리의 거리(level) 2. 루트노드 바로 아래의 자식노드를 의미한다   
        private SphereModel[] _levels = null;

        private Pool<SphereModel> _spheres = null;

        private QFifo<SphereModel> _integrateQ = null;
        private QFifo<SphereModel> _recomputeQ = null;

        //private float _maxRadius_supersphere_root;   //루트트리 슈퍼구의 최대 반지름 크기 (gravy 을 합친 최대크기임)          
        //private float _maxRadius_supersphere_leaf;   //리프트리 슈퍼구의 최대 반지름 크기 (gravy 을 합친 최대크기임) 
        private float[] _maxRadius_supersphere = null;
        private float _gravy_supersphere;         //여분의 양. 여분은 객체들이 부모로 부터 너무 자주 떨어지지 않도록 경계구의 크기를 넉넉하게 만드는 역할을 한다



        //public SphereTree(int maxspheres, float rootsize, float leafsize, float gravy)
        public SphereTree(int maxspheres, float[] list_maxRadius, float gravy)
        {
            //최대 4개까지만 만들 수 있게 한다 
            int maxLevel = list_maxRadius.Length;
            if (4 < maxLevel) maxLevel = 4; 
            _levels = new SphereModel[maxLevel];
            _maxRadius_supersphere = new float[maxLevel];
            for (int i = 0; i < maxLevel;i++)
            {
                _maxRadius_supersphere[i] = list_maxRadius[i];
            }
            _gravy_supersphere = gravy;

            //메모리풀 크기를 4배 하는 이유 : 각각의 레벨트리는 자식구에 대해 1개의 슈퍼구를 각각 만든다. 레벨트리 1개당 최대개수 *2 의 크기를 가져야 한다. 
            //레벨트리가 2개 이므로 *2*2 가 된다.
            //구의 최대개수가 5일때의 최대 메모리 사용량 : 레벨2트리 구5개 + 슈퍼구5개 , 레벨1트리 슈퍼구5개 + 복제된슈퍼구5개 
            maxspheres *= 2 * maxLevel;

            //_maxRadius_supersphere_root = rootsize;
            //_maxRadius_supersphere_leaf = leafsize;
            //_gravy_supersphere = gravy;

            _integrateQ = new QFifo<SphereModel>(maxspheres);
            _recomputeQ = new QFifo<SphereModel>(maxspheres);
            _spheres = new Pool<SphereModel>();
            _spheres.Init(maxspheres);       // init pool to hold all possible SpherePack instances.

            //Vector3 pos = Vector3.zero;
            //_level_1 = _spheres.GetFreeLink(); // initially empty
            //_level_1.Init(this, pos, 65536);
            //_level_1.AddFlag(SphereModel.Flag.SUPERSPHERE | SphereModel.Flag.ROOTNODE | SphereModel.Flag.TREE_LEVEL_1);
            ////
            //_level_2 = _spheres.GetFreeLink();  // initially empty
            //_level_2.Init(this, pos, 16384);
            //_level_2.AddFlag(SphereModel.Flag.SUPERSPHERE | SphereModel.Flag.ROOTNODE | SphereModel.Flag.TREE_LEVEL_2);



            for (int i = 0; i < maxLevel;i++)
            {
                int level_flag = (int)(SphereModel.Flag.TREE_LEVEL_ROOT) << i;

                _levels[i] = _spheres.GetFreeLink(); // initially empty
                _levels[i].Init(this, Vector3.zero, 65536);
                _levels[i].AddFlag(SphereModel.Flag.SUPERSPHERE | SphereModel.Flag.ROOTNODE | (SphereModel.Flag)level_flag);    
            }



        }




        public SphereModel AddSphere(Vector3 pos, float radius, SphereModel.Flag flags)
        {

            SphereModel pack = _spheres.GetFreeLink();
            if (null == pack)
            {
                DebugWide.LogError("AddSphere : GetFreeLink() is Null !!");
                return null;
            }

            pack.Init(this, pos, radius); //AddSpherePackFlag 함수 보다 먼저 호출되어야 한다. _flags 정보가 초기화 되기 때문이다. 

            //TREE_LEVEL_LAST 요청이 들어올 경우 마지막 레벨트리의 인덱스를 찾아 넣어준다 
            if(SphereModel.Flag.NONE != (flags & SphereModel.Flag.TREE_LEVEL_LAST))
            {
                int last_level_idx = _levels.Length - 1;
                flags = (SphereModel.Flag)((int)(SphereModel.Flag.TREE_LEVEL_ROOT) << last_level_idx);
                //DebugWide.LogBlue(flags);
            }

            if (SphereModel.Flag.NONE == (flags & SphereModel.Flag.TREE_LEVEL_1234))
            {
                DebugWide.LogError("AddSphere : TREE_LEVEL is None !!  " + flags);
                return null;
            }
                
            pack.AddFlag(flags & SphereModel.Flag.TREE_LEVEL_1234); //level 1~4 flag 만 통과시킨다 

            //if (SphereModel.Flag.NONE != (flags & SphereModel.Flag.TREE_LEVEL_1)) //루트트리가 들어 있다면 
            //{
            //    pack.AddFlag(SphereModel.Flag.TREE_LEVEL_1);
            //}
            //else
            //{
            //    pack.AddFlag(SphereModel.Flag.TREE_LEVEL_2);
            //}


            return pack;
        }

        //!!자식구만 통합의 대상이 된다. 함수에 슈퍼구를 넣으면 안됨 
        //대상구를 어떤 슈퍼구에 포함하거나 , 포함 할 슈퍼구가 없으면 새로 만든다 
        public void AddIntegrateQ(SphereModel pack)
        {

            int level_idx = pack.GetLevelIndex();
            _levels[level_idx].AddChild(pack); 

            //if (pack.HasFlag(SphereModel.Flag.TREE_LEVEL_1))
            //    _level_1.AddChild(pack);
            //else
                //_level_2.AddChild(pack);

            pack.AddFlag(SphereModel.Flag.INTEGRATE); // still needs to be integrated!
            QFifo<SphereModel>.Out_Point fifo = _integrateQ.Push(pack); // add it to the integration stack.
            pack.SetIntergrate_FifoOut(fifo);
        }

        //!!자식이 있는 구, 즉 슈퍼구만 재계산의 대상이 된다. 
        //슈퍼구의 위치,반지름이 자식들의 정보에 따라 재계산 된다.
        public void AddRecomputeQ(SphereModel pack)     // add to the recomputation (balancing) FIFO.
        {
            if (false == pack.HasFlag(SphereModel.Flag.RECOMPUTE))
            {
                if (0 != pack.GetChildCount())
                {
                    pack.AddFlag(SphereModel.Flag.RECOMPUTE); // needs to be recalculated!
                    QFifo<SphereModel>.Out_Point fifo = _recomputeQ.Push(pack);
                    pack.SetRecompute_FifoOut(fifo);
                }
                else
                {
                    Remove_SuperSphereAndLinkSphere(pack);
                }
            }
        }

        //LeafTree 의 슈퍼구와 연결된 레벨1 자식구를 지운다 
        public void Remove_SuperSphereAndLinkSphere(SphereModel pack)
        {
            if (null == pack) return;
            if (pack.HasFlag(SphereModel.Flag.ROOTNODE)) return; // CAN NEVER REMOVE THE ROOT NODE EVER!!!

            //if (pack.HasFlag(SphereModel.Flag.SUPERSPHERE) && pack.HasFlag(SphereModel.Flag.TREE_LEVEL_2))
            if (pack.HasFlag(SphereModel.Flag.SUPERSPHERE))
            {
                SphereModel link = pack.GetLink_UpLevelTree();

                Remove_SuperSphereAndLinkSphere(link);
            }

            pack.Unlink();

            _spheres.Release(pack);
        }

        public void ResetFlag()
        {
            //_level_1.ResetFlag();
            //_level_2.ResetFlag();

            for (int i = 0; i < _levels.Length; i++)
            {
                _levels[i].ResetFlag();
            }
        }

        public void Process()
        {
            //슈퍼구 재계산
            if (true)
            {
                // First recompute anybody that needs to be recomputed!!
                // When leaf node spheres exit their parent sphere, then the parent sphere needs to be rebalanced.  In fact,it may now be empty and
                // need to be removed.
                // This is the location where (n) number of spheres in the recomputation FIFO are allowed to be rebalanced in the tree.
                int maxrecompute = _recomputeQ.GetCount();
                for (int i = 0; i < maxrecompute; i++)
                {
                    SphereModel pack = _recomputeQ.Pop();
                    if (null == pack) break;

                    pack.InitRecompute_FifoOut(); //큐 연결정보를 초기화 한다 

                    bool isRemove = pack.Recompute(_gravy_supersphere);
                    if (isRemove) Remove_SuperSphereAndLinkSphere(pack);
                }
            }

            //자식구 통합
            if (true)
            {
                // Now, process the integration step.
                int maxintegrate = _integrateQ.GetCount();
                for (int i = 0; i < maxintegrate; i++)
                {
                    SphereModel pack = _integrateQ.Pop();
                    if (null == pack) break; //큐가 비어있을 때만 null을 반환한다. Unlink 에 의해 데이터가 null인 항목은 반환되지 않는다  

                    pack.InitIntergrate_FifoOut(); //큐 연결정보를 초기화 한다 

                    //if (pack.HasFlag(SphereModel.Flag.TREE_LEVEL_1))
                    //    Integrate(pack, _level_1, _maxRadius_supersphere_root); // integrate this one single dude against the root node.
                    //else
                        //Integrate(pack, _level_2, _maxRadius_supersphere_leaf); // integrate this one single dude against the root node.

                    int level_idx = pack.GetLevelIndex();
                    Integrate(pack, _levels[level_idx], _maxRadius_supersphere[level_idx]); 
                }
            }

        }

        //src_pack에는 자식구만 들어가야 한다 
        public void Integrate(SphereModel src_pack, SphereModel supersphere, float maxRadius_supersphere)
        {

            SphereModel search = supersphere.GetChildren();

            SphereModel containing_supersphere = null;  //src_pack를 포함하는 슈퍼구 
            float includedSqrDist = 1e9f;     // enclosed within. 10의9승. 1000000000.0

            SphereModel nearest_supersphere = null; //src_pack와 가까이에 있는 슈퍼구
            float nearDist = 1e9f;    // add ourselves to.


            // 1 **** src 와 가장 가까운 슈퍼구 구하기 : src를 포함하는 슈퍼구를 먼저 구함. 없으면 src와 가장 가까운 슈퍼구를 구한다.
            //=====================================================================================
            while (null != search)
            {
                if (search.HasFlag(SphereModel.Flag.SUPERSPHERE) &&
                    false == search.HasFlag(SphereModel.Flag.ROOTNODE) && 0 != search.GetChildCount())
                {

                    float sqrDist = src_pack.ToDistanceSquared(search);

                    //조건1 - src구가 완저 포함 
                    if (null != containing_supersphere)
                    {
                        if (sqrDist < includedSqrDist)
                        {

                            float dist = Mathf.Sqrt(sqrDist) + src_pack.GetRadius();

                            //조건1 전용 처리
                            if (dist <= search.GetRadius()) //슈퍼구에 src구가 완저 포함 
                            {
                                includedSqrDist = sqrDist;
                                containing_supersphere = search;
                            }
                        }
                    }
                    //조건2 - 슈퍼구에 걸쳐 있거나 포함되지 않음
                    else
                    {

                        float dist = (Mathf.Sqrt(sqrDist) + src_pack.GetRadius()) - search.GetRadius();

                        if (dist < nearDist)
                        {
                            //조건1에 들어가기 위하여, 최대1번 수행된다 
                            if (dist < 0) //슈퍼구에 src구가 완전 포함
                            {
                                includedSqrDist = sqrDist;
                                containing_supersphere = search;
                            }
                            //조건2 전용 처리
                            else //슈퍼구에 걸쳐 있거나 포함되지 않음 
                            {
                                nearDist = dist;
                                nearest_supersphere = search;
                            }
                        }
                    }
                }
                search = search.GetNextSibling();
            }
            //=====================================================================================

            //조건1 - src구가 완저 포함 
            if (null != containing_supersphere)
            {
                src_pack.Unlink(); //큐 연결정보를 Process 에서 해제 했기 때문에, 내부에서 LostChild만 수행된다 
                containing_supersphere.AddChild(src_pack); //src_pack 의 트리정보를 설정

                src_pack.Compute_BindingDistanceSquared(containing_supersphere);
                containing_supersphere.Recompute(_gravy_supersphere);

            }
            //조건2 - 슈퍼구에 걸쳐 있거나 포함되지 않음
            else
            {
                bool newsphere = true;

                //가까운 거리에 슈퍼구가 있다
                if (null != nearest_supersphere)
                {
                    float newRadius = nearDist + nearest_supersphere.GetRadius() + _gravy_supersphere;

                    //!!슈퍼구 최대크기 보다 작을 경우, 포함할 수 있는 크기로 변경한다 
                    if (newRadius <= maxRadius_supersphere)
                    {
                        src_pack.Unlink();

                        nearest_supersphere.SetRadius(newRadius);
                        nearest_supersphere.AddChild(src_pack);

                        nearest_supersphere.Recompute(_gravy_supersphere);
                        src_pack.Compute_BindingDistanceSquared(nearest_supersphere);

                        newsphere = false;

                    }

                }

                //조건3 - !포함될 슈퍼구가 하나도 없는 경우 , !!슈퍼구 최대크기 보다 큰 경우
                if (newsphere)
                {
                    src_pack.Unlink();

                    SphereModel parent = AddSphere(src_pack.GetPos(), src_pack.GetRadius() + _gravy_supersphere, supersphere.GetFlag());
                    parent.AddFlag(SphereModel.Flag.SUPERSPHERE);
                    parent.AddChild(src_pack);
                    supersphere.AddChild(parent);

                    parent.Recompute(_gravy_supersphere);
                    src_pack.Compute_BindingDistanceSquared(parent);

                    if (false == parent.HasFlag(SphereModel.Flag.TREE_LEVEL_ROOT))
                    {
                        //parent 가 level2 이라면, 생성하는 구는 level1 이어야 한다
                        //level2 => level1 , level3 => level2 ... 
                        int up_level_idx = parent.GetLevelIndex() - 1;
                        int up_flag = (int)(SphereModel.Flag.TREE_LEVEL_ROOT) << up_level_idx;

                        // need to create parent association!
                        SphereModel link = AddSphere(parent.GetPos(), parent.GetRadius(), (SphereModel.Flag)up_flag);
                        AddIntegrateQ(link);
                        link.SetLink_DownLevelTree(parent);
                        parent.SetLink_UpLevelTree(link);
                    }

                }
            }

            src_pack.ClearFlag(SphereModel.Flag.INTEGRATE); // we've been integrated!
        }

        //==================================================
        //첫번째 충돌체를 반환한다 (start 거리에서 가까운 것일 수도 아닐 수도 있음)
        public SphereModel RayTrace_FirstReturn(Vector3 start, Vector3 end, SphereModel exceptModel)
        {
            return _levels[0].RayTrace_FirstReturn(start, end, exceptModel);
        }

        //==================================================
        // debug 용 
        //==================================================

        public void Render_RayTrace(Vector3 start, Vector3 end)
        {
            _levels[0].Debug_RayTrace(start, end);
        }

        public void Render_RangeTest(Vector3 pos, float range)
        {

            _levels[0].Debug_RangeTest(pos, range, Frustum.ViewState.PARTIAL);
        }

        public void Render_FrustumTest(Frustum f, Frustum.ViewState state)
        {

            _levels[0].Debug_VisibilityTest(f, state);
        }

        public void Render_Debug(bool isText)
        {
            
            for (int i = 0; i < _levels.Length; i++)
            {
                _levels[i].Debug_Render(isText);
            }
        }
    }
}

