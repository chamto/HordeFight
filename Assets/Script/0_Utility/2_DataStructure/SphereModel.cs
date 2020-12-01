using System;
using UnityEngine;


namespace UtilGS9
{
    public class SphereModel : IPoolConnector<SphereModel>
    {
        //UserData 대상이 되는 객체는 이 인터페이스를 상속받아야 한다 
        public interface IUserData
        { }

        [FlagsAttribute]
        public enum Flag
        {
            //NONE = 0,

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

            CREATE_LEVEL_LAST = (1 << 11), //마지막레벨에 생성하라는 생성지시자로 사용 

            //TREE_LEVEL_ROOT = TREE_LEVEL_1,
            TREE_LEVEL_1234 = TREE_LEVEL_1 | TREE_LEVEL_2 | TREE_LEVEL_3 | TREE_LEVEL_4,

        }

        public Vector3 _center;
        public float _radius;
        public float _radius_sqr;


        //------------------------------------------------------
        //                   메모리풀 전용 변수
        //------------------------------------------------------
        private int _id = -1;
        private bool _isUsed = false;
        private SphereModel _pool_next = null;
        private SphereModel _pool_prev = null;

        //------------------------------------------------------
        //                    부모 노드 변수
        //------------------------------------------------------
        private SphereModel _head_children = null;  //형제노드의 첫번째 노드 
        private SphereModel _link_upLevelTree = null; //윗 level 의 트리 링크 (전체 트리에서 보았을 때는 같은 level 의 노드링크임)
        private SphereModel _link_downLevelTree = null; //아랫 level 의 트리 링크 (전체 트리에서 보았을 때는 같은 level 의 노드링크임)

        //------------------------------------------------------
        //                    자식 노드 변수
        //------------------------------------------------------
        private SphereModel _parent = null; //트리 깊이 위로
        private SphereModel _sibling_next = null; //내기준 왼쪽 형제노드 (내가 머리라면 null값임)
        private SphereModel _sibling_prev = null; //내기준 오른쪽 형제노드 (내가 꼬리라면 null값임) 


        private Flag _flags;
        private QFifo<SphereModel>.Out_Point _recompute_fifoOut;
        private QFifo<SphereModel>.Out_Point _intergrate_fifoOut;


        private IUserData _link_userData = null;

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
        public float ToDistanceSquared(SphereModel pack) { return VOp.Minus(_center, pack._center).sqrMagnitude; }

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
            if (0 != (_flags & Flag.TREE_LEVEL_1)) return 0;
            if (0 != (_flags & Flag.TREE_LEVEL_2)) return 1;
            if (0 != (_flags & Flag.TREE_LEVEL_3)) return 2;
            if (0 != (_flags & Flag.TREE_LEVEL_4)) return 3;

            return -1;
        }

        //=====================================================
        //트리 링크 다루는 함수
        //=====================================================
        public void SetParent(SphereModel model) { _parent = model; }
        public SphereModel GetParent() { return _parent; }
        public SphereModel GetChildren() { return _head_children; }
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


        //public T GetLink_UserData<T>() where T : IUserData { return (T)_link_userData; }
        public IUserData GetLink_UserData() { return _link_userData; }
        public void SetLink_UserData<T>(T data) where T : IUserData { _link_userData = (IUserData)data; }

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
            _head_children = null;
            _sibling_next = null;
            _sibling_prev = null;

            _flags = 0;
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
                if (float.Epsilon < Math.Abs(radius - _radius))
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

            SphereModel next_child = _head_children;
            _head_children = pack; // new head of list

            pack.SetNextSibling(next_child); // his next is my old next
            pack.SetPrevSibling(null); // at head of list, no previous
            pack.SetParent(this);

            if (null != next_child) next_child.SetPrevSibling(pack); //다음자식의 이전형제 정보를 넣어준다 

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

            if (null == _head_children || 0 == _childCount) DebugWide.LogError("null == _children || 0 == _childCount"); //assert


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
                _head_children = next;
                if (null != _head_children) _head_children.SetPrevSibling(null);
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
            if (null == _head_children) return true; // kill it!
            if (HasFlag(Flag.ROOTNODE)) return false; // don't recompute root nodes!

            //#if 1
            // recompute bounding sphere!
            Vector3 total = ConstV.v3_zero;
            int count = 0;
            SphereModel pack = _head_children;
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

                pack = _head_children;

                while (null != pack)
                {
                    float dist = ToDistanceSquared(pack);
                    float radius = (float)Math.Sqrt(dist) + pack.GetRadius();
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
                pack = _head_children;

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

            SphereModel pack = _head_children;
            while (null != pack)
            {
                pack.ResetFlag();
                pack = pack.GetNextSibling();
            }
        }

        //==================================================

        public SphereModel RayTrace_FirstReturn(Vector3 line_origin, Vector3 line_last, SphereModel exceptModel)
        {
            bool hit = false;
            SphereModel sm = null;
            if (HasFlag(Flag.SUPERSPHERE))
            {

                hit = Geo.IntersectRay(_center, _radius, line_origin, VOp.Minus(line_last, line_origin));
                if (hit)
                {

                    SphereModel pack = _head_children;
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
                        if (this != exceptModel)
                            return this;
                    }
                    //하위 레벨트리 검사
                    else if (null != downLink)
                        return downLink.RayTrace_FirstReturn(line_origin, line_last, exceptModel);
                }
            }
            return null;
        }


        public void RangeTest_MinDisReturn(Frustum.ViewState state, ref HordeFight.ObjectManager.Param_RangeTest param)
        {

            float between_sqr = VOp.Minus(param.src_pos, _center).sqrMagnitude;
            if (state == Frustum.ViewState.PARTIAL)
            {
                //float between_sqr = (dstCenter - _center).sqrMagnitude;

                //완전비포함 검사
                float sqrSumRd = (_radius + param.maxRadius) * (_radius + param.maxRadius);
                if (between_sqr > sqrSumRd) return;

                //완전포함 검사 
                if (param.maxRadius >= _radius)
                {
                    float sqrSubRd = (param.maxRadius - _radius) * (param.maxRadius - _radius);
                    if (between_sqr <= sqrSubRd) state = Frustum.ViewState.INSIDE; //슈퍼구가 포함되면 자식구까지 모두 포함되므로, 계산할 필요가 없어진다     
                }

            }

            if (HasFlag(Flag.SUPERSPHERE))
            {
                SphereModel pack = _head_children;
                while (null != pack)
                {
                    pack.RangeTest_MinDisReturn(state, ref param);
                    pack = pack.GetNextSibling();
                }

                //SingleO.debugViewer.AddDraw_Circle(_center, _radius, Color.gray);
            }
            else
            {
                SphereModel upLink = this.GetLink_UpLevelTree();
                SphereModel downLink = this.GetLink_DownLevelTree();

                //자식노드에 연결된 하위레벨 슈퍼구 
                if (null != downLink)
                    downLink.RangeTest_MinDisReturn(state, ref param);
                
                //전체트리의 최하위 자식노드 
                if (null == upLink && null == downLink)
                {
                    //최소반지름 밖에 있어야 한다
                    float sqrSumRd = (_radius + param.minRadius) * (_radius + param.minRadius);
                    if (between_sqr > sqrSumRd)
                    {
                        //등록된 검사용 콜백함수 호출
                        if (null != param.callback)
                        {
                            //SingleO.debugViewer.AddDraw_Circle(_center, _radius, Color.white);
                            //검사용 콜백함수를 통과하면 최하위 노드인 자기자신을 반환한다.
                            if (true == param.callback(ref param, this))
                            {
                                param.find = this;

                            }
                                
                        }
                        else
                        {
                            //검사함수가 없으면 자기자신을 반환한다 
                            param.find = this;
                        }

                    }
                }

            }

        }

        //==================================================
        // debug용
        //==================================================

        public void Debug_RayTrace(Vector3 line_origin, Vector3 line_last)
        {
            bool hit = false;

            if (HasFlag(Flag.SUPERSPHERE))
            {

                hit = Geo.IntersectRay(_center, _radius, line_origin, VOp.Minus(line_last, line_origin));
                if (hit)
                {

                    DebugWide.DrawCircle(_center, GetRadius(), Color.gray);

                    SphereModel pack = _head_children;
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
                SphereModel upLink = this.GetLink_UpLevelTree();
                SphereModel downLink = this.GetLink_DownLevelTree();

                hit = Geo.IntersectLineSegment(_center, _radius, line_origin, line_last);

                //DebugWide.LogBlue(GetFlag() + " - " + upLink + " - " + downLink + " - " + hit + "  " + _center + "  " + _radius);

                if (hit)
                {
                    
                    //if (null != downLink) downLink.RayTrace(line_origin, dir, distance);
                    if (null != downLink) downLink.Debug_RayTrace(line_origin, line_last);

                    Color cc = Color.gray;
                    //if (HasFlag(Flag.TREE_LEVEL_4))
                        //DebugWide.LogBlue("L4 : " + upLink + " - " + downLink);
                    if (null == upLink && null == downLink) cc = Color.red; //전체트리의 최하위 자식노드 
                    DebugWide.DrawCircle(_center, GetRadius(), cc);
                }
            }
        }

        public void Debug_RangeTest(Vector3 dstCenter, float dstRadius, Frustum.ViewState state)
        {
            if (state == Frustum.ViewState.PARTIAL)
            {
                float between_sqr = VOp.Minus(dstCenter, _center).sqrMagnitude;

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
                SphereModel pack = _head_children;
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

                SphereModel pack = _head_children;

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

        public void Debug_Render(Color color, bool isText)
        {

            if (null != _head_children)
            {
                SphereModel pack = _head_children;

                while (null != pack)
                {
                    pack.Debug_Render(color, isText);
                    pack = pack.GetNextSibling();
                }
            }

            if (false == HasFlag(Flag.ROOTNODE))
            {
                string temp = string.Empty;
                //Color color = Color.green;

                if (HasFlag(Flag.SUPERSPHERE))
                {

                    DebugWide.DrawCircle(_center, GetRadius(), color);

                    //if (HasFlag(Flag.TREE_LEVEL_2))
                    {

                        SphereModel link = GetLink_UpLevelTree();

                        if (null != link)
                            link = link.GetParent(); //level_1 트리의 슈퍼구노드를 의미한다 

                        if (null != link && false == link.HasFlag(Flag.ROOTNODE))
                        {
                            DebugWide.DrawLine(_center, link._center, Color.green);
                        }
                    }


                }
                else
                {
                    //SphereModel up = GetLink_UpLevelTree();
                    //SphereModel down = GetLink_DownLevelTree();
                    //DebugWide.LogBlue(GetFlag() + " - " + up +" - " + down);
                    temp += "\n";
                    DebugWide.DrawCircle(_center, GetRadius(), color);
                }

                if (true == isText)
                {
                    int level = 0;
                    if (HasFlag(Flag.TREE_LEVEL_1)) { temp += "\n"; level = 1; }
                    if (HasFlag(Flag.TREE_LEVEL_2)) { temp += "\n        "; level = 2; }
                    if (HasFlag(Flag.TREE_LEVEL_3)) { temp += "\n                "; level = 3; }
                    if (HasFlag(Flag.TREE_LEVEL_4)) { temp += "\n                        "; level = 4; }
                    if (HasFlag(Flag.SUPERSPHERE)) { temp += "s"; }

                    //DebugWide.PrintText(_center, color, temp + GetID());
                    DebugWide.PrintText(_center, Color.black, temp + level);
                }

            }

        }

    }
}


