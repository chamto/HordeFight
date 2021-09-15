using System;
using UnityEngine;
using UtilGS9;


namespace ST_Test_003
{
    public class PT_SphereModel : IPoolConnector<PT_SphereModel>
    {
        [FlagsAttribute]
        public enum Flag
        {
            NONE = 0,

            SUPERSPHERE = (1 << 0), //슈퍼구 : 다른 자식구를 포한한다 
            TREE_LEVEL_1 = (1 << 1),   //전체트리의 Level_1 을 구성하는 분리된 트리 
            TREE_LEVEL_2 = (1 << 2),   //전체트리의 Level_2 을 구성하는 분리된 트리 
            ROOTNODE = (1 << 3),    //트리의 최상위 노드 
            RECOMPUTE = (1 << 4),   //경계구를 다시 계산
            INTEGRATE = (1 << 5),   //대상구를 트리에 통합시킨다. 
                                    //  Level_2 트리의 슈퍼구일 경우 Level_1의 자식구로 똑같이 생성시킨다. 
                                    //  이 자식구를 Level_2의 슈퍼구에 연결시킨다  

            HIDDEN = (1 << 6), // outside of view frustum
            PARTIAL = (1 << 7), // partially inside view frustum
            INSIDE = (1 << 8)  // completely inside view frustum
        }

        protected Vector3 _center;
        protected float _radius;
        protected float _radius_sqr;



        //------------------------------------------------------
        //                   메모리풀 전용 변수
        //------------------------------------------------------
        private int _id = -1;
        private bool _isUsed = false;
        private PT_SphereModel _pool_next = null;
        private PT_SphereModel _pool_prev = null;

        //------------------------------------------------------
        //                    트리 링크 변수
        //------------------------------------------------------
        private PT_SphereModel _parent = null; //트리 깊이 위로
        private PT_SphereModel _children = null;  //트리 깊이 아래로 
        private PT_SphereModel _sibling_next = null; //트리 같은 차수 왼쪽으로
        private PT_SphereModel _sibling_prev = null; //트리 같은 차수 오른쪽으로 


        private Flag _flags;
        private QFifo<PT_SphereModel>.Out_Point _recompute_fifoOut;
        private QFifo<PT_SphereModel>.Out_Point _intergrate_fifoOut;

        private PT_SphereModel _link_upLevelTree = null; //윗 level 의 트리 링크 (전체 트리에서 보았을 때는 같은 level 의 노드링크임)
        private PT_SphereModel _link_downLevelTree = null; //아랫 level 의 트리 링크 (전체 트리에서 보았을 때는 같은 level 의 노드링크임)


        //------------------------------------------------------

        private int _childCount = 0;
        private float _binding_distance_sqr = 0f;

        private PT_SphereTree _treeController = null;

        //______________________________________________________________________________________________________

        //=====================================================
        //interface 구현 
        //=====================================================
        public void InitID(int id) { _id = id; }
        public int GetID() { return _id; }
        public void SetUsed(bool used) { _isUsed = used; }
        public bool IsUsed() { return _isUsed; }

        public PT_SphereModel GetPoolNext() { return _pool_next; }
        public PT_SphereModel GetPoolPrevious() { return _pool_prev; }

        public void SetPoolNext(PT_SphereModel model) { _pool_next = model; }
        public void SetPoolPrevious(PT_SphereModel model) { _pool_prev = model; }

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
        public float ToDistanceSquared(PT_SphereModel pack) { return (_center - pack._center).sqrMagnitude; }

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

        //=====================================================
        //트리 링크 다루는 함수
        //=====================================================
        public void SetParent(PT_SphereModel model) { _parent = model; }
        public PT_SphereModel GetParent() { return _parent; }
        public PT_SphereModel GetChildren() { return _children; }
        public void SetNextSibling(PT_SphereModel child) { _sibling_next = child; }
        public void SetPrevSibling(PT_SphereModel child) { _sibling_prev = child; }
        public PT_SphereModel GetNextSibling() { return _sibling_next; }
        public PT_SphereModel GetPrevSibling() { return _sibling_prev; }

        public void SetRecompute_FifoOut(QFifo<PT_SphereModel>.Out_Point fifo_out) { _recompute_fifoOut = fifo_out; }
        public void SetIntergrate_FifoOut(QFifo<PT_SphereModel>.Out_Point fifo_out) { _intergrate_fifoOut = fifo_out; }
        public void InitRecompute_FifoOut() { _recompute_fifoOut.Init(); }
        public void InitIntergrate_FifoOut() { _intergrate_fifoOut.Init(); }

        public void SetLink_UpLevelTree(PT_SphereModel data) { _link_upLevelTree = data; }
        public PT_SphereModel GetLink_UpLevelTree() { return _link_upLevelTree; }
        public void SetLink_DownLevelTree(PT_SphereModel data) { _link_downLevelTree = data; }
        public PT_SphereModel GetLink_DownLevelTree() { return _link_downLevelTree; }

        //======================================================
        public int GetChildCount() { return _childCount; }

        //=====================================================
        //초기화 
        //=====================================================
        public void Init(PT_SphereTree controller, Vector3 pos, float radius)
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

        public void AddChild(PT_SphereModel pack)
        {

            PT_SphereModel my_child = _children;
            _children = pack; // new head of list

            pack.SetNextSibling(my_child); // his next is my old next
            pack.SetPrevSibling(null); // at head of list, no previous
            pack.SetParent(this);

            if (null != my_child) my_child.SetPrevSibling(pack); // previous now this..

            _childCount++;

            //#ifdef _DEBUG  //____________________________________________________________
            //에러검출 테스트 : 테스트 필요 
            float dist = ToDistanceSquared(pack);
            float radius = Mathf.Sqrt(dist) + pack.GetRadius();
            if (radius > GetRadius())
            {
                //DebugWide.LogError("newRadius > GetRadius()"); //assert //테스트 필요 
            }
            //#endif //____________________________________________________________
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


            //if (null == _children) DebugWide.LogError("null == _children"); //assert요 //테스트 필요 

            _parent = null;
        }

        public void LostChild(PT_SphereModel model)
        {
            //#ifdef _DEBUG  //____________________________________________________________
            if (null == _children || 0 == _childCount) DebugWide.LogError("null == _children || 0 == _childCount"); //assert

            PT_SphereModel pack = _children;
            bool found = false;
            while (null != pack)
            {
                if (pack == model)
                {
                    //model 이 두개이상 있다
                    if (true == found) DebugWide.LogError("true == found"); //assert
                    found = true;
                }
                pack = pack.GetNextSibling();
            }

            //내 없는 자식을 지울려고 했다 
            if (false == found) DebugWide.LogError("false == found"); //assert
                                                                      //#endif //____________________________________________________________________

            // first patch old linked list.. his previous now points to his next
            PT_SphereModel prev = model.GetPrevSibling();

            if (null != prev)
            {
                PT_SphereModel next = model.GetNextSibling();
                prev.SetNextSibling(next); // my previous now points to my next
                if (null != next) next.SetPrevSibling(prev);
                // list is patched!
            }
            else
            {
                PT_SphereModel next = model.GetNextSibling();
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
        public void Compute_BindingDistanceSquared(PT_SphereModel parent)
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
            PT_SphereModel pack = _children;
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
                PT_SphereModel link = GetLink_UpLevelTree();
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

            PT_SphereModel pack = _children;
            while (null != pack)
            {
                pack.ResetFlag();
                pack = pack.GetNextSibling();
            }
        }

        public void RayTrace(Vector3 line_origin, Vector3 line_last)
        {
            bool hit = false;
            //Vector3 sect;

            if (HasFlag(Flag.SUPERSPHERE))
            {

                //hit = Geo.IntersectRay(_center, _radius, line_origin, dir, out sect);
                hit = Geo.IntersectRay(_center, _radius, line_origin, line_last - line_origin);
                if (hit)
                {
                    //#if DEMO
                    DebugWide.DrawCircle(_center, GetRadius(), Color.gray);
                    //#endif
                    PT_SphereModel pack = _children;

                    while (null != pack)
                    {
                        //pack.RayTrace(p1, dir, distance);
                        pack.RayTrace(line_origin, line_last);
                        pack = pack.GetNextSibling();
                    }
                }

            }
            else
            {

                //hit = Geo.IntersectLineSegment(_center, _radius, line_origin, dir, distance, out sect);
                hit = Geo.IntersectLineSegment(_center, _radius, line_origin, line_last);
                if (hit)
                {
                    PT_SphereModel upLink = this.GetLink_UpLevelTree();
                    PT_SphereModel downLink = this.GetLink_DownLevelTree();
                    //if (null != downLink) downLink.RayTrace(line_origin, dir, distance);
                    if (null != downLink) downLink.RayTrace(line_origin, line_last);

                    Color cc = Color.gray;
                    if (null == upLink && null == downLink) cc = Color.red; //전체트리의 최하위 자식노드 
                    DebugWide.DrawCircle(_center, GetRadius(), cc);
                }
            }
        }

        public void RangeTest(Vector3 dstCenter, float dstRadius, Frustum.ViewState state)
        {
            if (state == Frustum.ViewState.PARTIAL)
            {
                float between_sqr = (dstCenter - _center).sqrMagnitude;

                //완전비포함 검사
                float sqrSumRd = (_radius + dstRadius) * (_radius + dstRadius);
                if (between_sqr  > sqrSumRd) return;

                //완전포함 검사 
                if(dstRadius >= _radius)
                {
                    float sqrSubRd = (dstRadius - _radius) * (dstRadius - _radius);
                    if (between_sqr <= sqrSubRd) state = Frustum.ViewState.INSIDE; //슈퍼구가 포함되면 자식구까지 모두 포함되므로, 계산할 필요가 없어진다     
                }

                //제곱근 계산 방식은 사용안함 
                //float d = (dstCenter - _center).magnitude;
                //if ((d - dstRadius) > GetRadius()) return;
                //if ((GetRadius() + d) < dstRadius) state = Frustum.ViewState.INSIDE;
            }

            if (HasFlag(Flag.SUPERSPHERE))
            {
                //#if DEMO
                if (state == Frustum.ViewState.PARTIAL)
                {
                    DebugWide.DrawCircle(_center, GetRadius(), Color.gray);
                }
                //#endif
                PT_SphereModel pack = _children;
                while (null != pack)
                {
                    pack.RangeTest(dstCenter, dstRadius, state);
                    pack = pack.GetNextSibling();
                }

            }
            else
            {
                PT_SphereModel upLink = this.GetLink_UpLevelTree();
                PT_SphereModel downLink = this.GetLink_DownLevelTree();
                if (null != downLink) downLink.RangeTest(dstCenter, dstRadius, state);

                Color cc = Color.gray;
                if (null == upLink && null == downLink) cc = Color.red; //전체트리의 최하위 자식노드 
                DebugWide.DrawCircle(_center, GetRadius(), cc);
            }
        }

        public void VisibilityTest(Frustum f, Frustum.ViewState state)
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

                PT_SphereModel pack = _children;

                while (null != pack)
                {
                    pack.VisibilityTest(f, state);
                    pack = pack.GetNextSibling();
                }

            }
            else
            {
                PT_SphereModel link = this.GetLink_DownLevelTree();
                switch (state)
                {
                    case Frustum.ViewState.INSIDE:
                        if (!HasFlag(Flag.INSIDE))
                        {
                            ClearFlag(Flag.HIDDEN | Flag.PARTIAL);
                            AddFlag(Flag.INSIDE);
                            //callback.VisibilityCallback(f, this, state);

                            if (null != link) link.VisibilityTest(f, state); //테스트 필요 
                            DebugWide.DrawCircle(_center, GetRadius(), Color.red);
                        }
                        break;
                    case Frustum.ViewState.OUTSIDE:
                        if (!HasFlag(Flag.HIDDEN))
                        {
                            ClearFlag(Flag.INSIDE | Flag.PARTIAL);
                            AddFlag(Flag.HIDDEN);
                            //callback.VisibilityCallback(f, this, state);

                            if (null != link) link.VisibilityTest(f, state); //테스트 필요 
                            DebugWide.DrawCircle(_center, GetRadius(), Color.black);
                        }
                        break;
                    case Frustum.ViewState.PARTIAL:
                        if (!HasFlag(Flag.PARTIAL))
                        {
                            ClearFlag(Flag.INSIDE | Flag.HIDDEN);
                            AddFlag(Flag.PARTIAL);
                            //callback.VisibilityCallback(f, this, state);

                            if (null != link) link.VisibilityTest(f, state); //테스트 필요 
                            DebugWide.DrawCircle(_center, GetRadius(), Color.blue);
                        }
                        break;
                }

            }
        }

        public void Render_Debug(bool isText)
        {

            if (null != _children)
            {
                PT_SphereModel pack = _children;

                while (null != pack)
                {
                    pack.Render_Debug(isText);
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

                    if (HasFlag(Flag.TREE_LEVEL_2))
                    {

                        PT_SphereModel link = GetLink_UpLevelTree();

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

                if(true == isText)
                {
                    if (HasFlag(Flag.TREE_LEVEL_1)) { temp += ""; color = Color.magenta; }
                    if (HasFlag(Flag.TREE_LEVEL_2)) { temp += "\n        "; }
                    if (HasFlag(Flag.SUPERSPHERE)) { temp += "s"; }

                    //DebugWide.PrintText(_center, color, temp + GetID());
                    //DefineI.PrintText(_center, Color.black, ((Flag)_flags).ToString() ); 

                    //ref : https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings   
                    DebugWide.PrintText(_center, color, _radius.ToString("F2"));
                }

            }

        }
    }



    //=================================================================
    //=================================================================
    //=================================================================
    //=================================================================



    public class PT_SphereTree
    {
        //총 거리3 (루트 -> 자식1 -> 자식2)의 전체적인 트리를 구성한다 
        //트리의 거리별로 독립적인 트리를 구성한다. _level1 트리 한개 , _level2 트리 한개
        //_level1 의 자식구와 _level2 트리의 슈퍼구를 연결하여 외부에서는 하나의 트리인것 처럼 보이게 한다
        private PT_SphereModel _level_1 = null; //root : 트리의 거리(level) 1. 루트노드를 의미한다 
        private PT_SphereModel _level_2 = null; //leaf : 트리의 거리(level) 2. 루트노드 바로 아래의 자식노드를 의미한다   

        private Pool<PT_SphereModel> _spheres = null;

        private QFifo<PT_SphereModel> _integrateQ = null;
        private QFifo<PT_SphereModel> _recomputeQ = null;

        public float _maxRadius_supersphere_root;   //루트트리 슈퍼구의 최대 반지름 크기 (gravy 을 합친 최대크기임)          
        public float _maxRadius_supersphere_leaf;   //리프트리 슈퍼구의 최대 반지름 크기 (gravy 을 합친 최대크기임)             
        public float _gravy_supersphere;         //여분의 양. 여분은 객체들이 부모로 부터 너무 자주 떨어지지 않도록 경계구의 크기를 넉넉하게 만드는 역할을 한다



        public PT_SphereTree(int maxspheres, float rootsize, float leafsize, float gravy)
        {
            //메모리풀 크기를 4배 하는 이유 : 각각의 레벨트리는 자식구에 대해 1개의 슈퍼구를 각각 만든다. 레벨트리 1개당 최대개수 *2 의 크기를 가져야 한다. 
            //레벨트리가 2개 이므로 *2*2 가 된다.
            //구의 최대개수가 5일때의 최대 메모리 사용량 : 레벨2트리 구5개 + 슈퍼구5개 , 레벨1트리 슈퍼구5개 + 복제된슈퍼구5개 
            maxspheres *= 4;

            _maxRadius_supersphere_root = rootsize;
            _maxRadius_supersphere_leaf = leafsize;
            _gravy_supersphere = gravy;

            _integrateQ = new QFifo<PT_SphereModel>(maxspheres);
            _recomputeQ = new QFifo<PT_SphereModel>(maxspheres);
            _spheres = new Pool<PT_SphereModel>();
            _spheres.Init(maxspheres);       // init pool to hold all possible SpherePack instances.

            Vector3 pos = Vector3.zero;

            _level_1 = _spheres.GetFreeLink(); // initially empty
            _level_1.Init(this, pos, 65536);
            _level_1.AddFlag(PT_SphereModel.Flag.SUPERSPHERE | PT_SphereModel.Flag.ROOTNODE | PT_SphereModel.Flag.TREE_LEVEL_1);


            _level_2 = _spheres.GetFreeLink();  // initially empty
            _level_2.Init(this, pos, 16384);
            _level_2.AddFlag(PT_SphereModel.Flag.SUPERSPHERE | PT_SphereModel.Flag.ROOTNODE | PT_SphereModel.Flag.TREE_LEVEL_2);

        }




        public PT_SphereModel AddSphere(Vector3 pos, float radius, PT_SphereModel.Flag flags)
        {

            PT_SphereModel pack = _spheres.GetFreeLink();
            if (null == pack)
            {
                DebugWide.LogError("AddSphere : GetFreeLink() is Null !!");
                return null;
            }

            pack.Init(this, pos, radius); //AddSpherePackFlag 함수 보다 먼저 호출되어야 한다. _flags 정보가 초기화 되기 때문이다. 

            if (PT_SphereModel.Flag.NONE != (flags & PT_SphereModel.Flag.TREE_LEVEL_1)) //루트트리가 들어 있다면 
            {
                pack.AddFlag(PT_SphereModel.Flag.TREE_LEVEL_1);
            }
            else
            {
                pack.AddFlag(PT_SphereModel.Flag.TREE_LEVEL_2);
            }

            //AddIntegrateQ(pack); // add to integration list.


            return pack;
        }

        //!!자식구만 통합의 대상이 된다. 함수에 슈퍼구를 넣으면 안됨 
        //대상구를 어떤 슈퍼구에 포함하거나 , 포함 할 슈퍼구가 없으면 새로 만든다 
        public void AddIntegrateQ(PT_SphereModel pack)
        {

            if (pack.HasFlag(PT_SphereModel.Flag.TREE_LEVEL_1))
                _level_1.AddChild(pack);
            else
                _level_2.AddChild(pack);

            pack.AddFlag(PT_SphereModel.Flag.INTEGRATE); // still needs to be integrated!
            QFifo<PT_SphereModel>.Out_Point fifo = _integrateQ.Push(pack); // add it to the integration stack.
            pack.SetIntergrate_FifoOut(fifo);
        }

        //!!자식이 있는 구, 즉 슈퍼구만 재계산의 대상이 된다. 
        //슈퍼구의 위치,반지름이 자식들의 정보에 따라 재계산 된다.
        public void AddRecomputeQ(PT_SphereModel pack)     // add to the recomputation (balancing) FIFO.
        {
            if (false == pack.HasFlag(PT_SphereModel.Flag.RECOMPUTE))
            {
                if (0 != pack.GetChildCount())
                {
                    pack.AddFlag(PT_SphereModel.Flag.RECOMPUTE); // needs to be recalculated!
                    QFifo<PT_SphereModel>.Out_Point fifo = _recomputeQ.Push(pack);
                    pack.SetRecompute_FifoOut(fifo);
                }
                else
                {
                    Remove_SuperSphereAndLinkSphere(pack);
                }
            }
        }

        //LeafTree 의 슈퍼구와 연결된 레벨1 자식구를 지운다 
        public void Remove_SuperSphereAndLinkSphere(PT_SphereModel pack)
        {
            if (null == pack) return;
            if (pack.HasFlag(PT_SphereModel.Flag.ROOTNODE)) return; // CAN NEVER REMOVE THE ROOT NODE EVER!!!

            if (pack.HasFlag(PT_SphereModel.Flag.SUPERSPHERE) && pack.HasFlag(PT_SphereModel.Flag.TREE_LEVEL_2))
            {

                PT_SphereModel link = pack.GetLink_UpLevelTree();

                Remove_SuperSphereAndLinkSphere(link);
            }

            pack.Unlink();

            _spheres.Release(pack);
        }

        public void ResetFlag()
        {
            _level_1.ResetFlag();
            _level_2.ResetFlag();
        }

        public void Process(out int maxrecompute , out int maxintegrate)
        {
            //슈퍼구 재계산
            if (true)
            {
                // First recompute anybody that needs to be recomputed!!
                // When leaf node spheres exit their parent sphere, then the parent sphere needs to be rebalanced.  In fact,it may now be empty and
                // need to be removed.
                // This is the location where (n) number of spheres in the recomputation FIFO are allowed to be rebalanced in the tree.
                maxrecompute = _recomputeQ.GetCount();
                for (int i = 0; i < maxrecompute; i++)
                {
                    PT_SphereModel pack = _recomputeQ.Pop();
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
                maxintegrate = _integrateQ.GetCount();
                for (int i = 0; i < maxintegrate; i++)
                {
                    PT_SphereModel pack = _integrateQ.Pop();
                    if (null == pack) break; //큐가 비어있을 때만 null을 반환한다. Unlink 에 의해 데이터가 null인 항목은 반환되지 않는다  

                    pack.InitIntergrate_FifoOut(); //큐 연결정보를 초기화 한다 

                    if (pack.HasFlag(PT_SphereModel.Flag.TREE_LEVEL_1))
                        Integrate(pack, _level_1, _maxRadius_supersphere_root); // integrate this one single dude against the root node.
                    else
                        Integrate(pack, _level_2, _maxRadius_supersphere_leaf); // integrate this one single dude against the root node.
                }
            }

        }

        //src_pack에는 자식구만 들어가야 한다 
        public void Integrate(PT_SphereModel src_pack, PT_SphereModel supersphere, float maxRadius_supersphere)
        {

            PT_SphereModel search = supersphere.GetChildren();

            PT_SphereModel containing_supersphere = null;  //src_pack를 포함하는 슈퍼구 
            float includedSqrDist = 1e9f;     // enclosed within. 10의9승. 1000000000.0

            PT_SphereModel nearest_supersphere = null; //src_pack와 가까이에 있는 슈퍼구
            float nearDist = 1e9f;    // add ourselves to.


            // 1 **** src 와 가장 가까운 슈퍼구 구하기 : src를 포함하는 슈퍼구를 먼저 구함. 없으면 src와 가장 가까운 슈퍼구를 구한다.
            //=====================================================================================
            while (null != search)
            {
                if (search.HasFlag(PT_SphereModel.Flag.SUPERSPHERE) &&
                    false == search.HasFlag(PT_SphereModel.Flag.ROOTNODE) && 0 != search.GetChildCount())
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

                //반드시 상위레벨트리의 구와 크기와 위치를 같게 맞추어야 한다
                //자식구는 Recompute 의 대상이 아니다. 
                //if (containing_supersphere.HasFlag(SphereModel.Flag.TREE_LEVEL_2))
                //{
                //    //연결된 상위 레벨트리의 자식노드를 갱신한다 
                //    SphereModel link = containing_supersphere.GetLink_UpLevelTree();
                //    link.SetPosRadius(containing_supersphere.GetPos(), containing_supersphere.GetRadius());

                //    //SetPosRadius 함수는 통합 플래그가 있을 경우 반지름 갱신을 안한다 
                //    //바로 생성된 레벨1의 자식구는 통합처리가 안되었을 때, SetPosRadius 를 호출하면 반지름 갱신에 실패한다 
                //    //통합처리중일때는 따로 반지름을 갱신해 주어야 한다 
                //    if (link.HasFlag(SphereModel.Flag.INTEGRATE))
                //    {
                //        link.SetRadius(containing_supersphere.GetRadius());
                //    }
                //}

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

                        //반드시 상위레벨트리의 구와 크기와 위치를 같게 맞추어야 한다
                        //if (nearest_supersphere.HasFlag(SphereModel.Flag.TREE_LEVEL_2))
                        //{
                        //    //연결된 상위 레벨트리의 자식노드를 갱신한다 
                        //    SphereModel link = nearest_supersphere.GetLink_UpLevelTree();
                        //    link.SetPosRadius(nearest_supersphere.GetPos(), nearest_supersphere.GetRadius());

                        //    //SetPosRadius 함수에서 통합 플래그가 있을 경우 반지름 갱신을 안한다 
                        //    //바로 생성된 레벨1의 자식구는 통합처리가 안되었을 때는 갱신이 실패가 된다
                        //    //따로 통합처리중일때 반지름을 갱신해 주어야 한다 
                        //    if(link.HasFlag(SphereModel.Flag.INTEGRATE))
                        //    {
                        //        link.SetRadius(nearest_supersphere.GetRadius());    
                        //    }
                        //}

                        newsphere = false;

                    }

                }

                //조건3 - !포함될 슈퍼구가 하나도 없는 경우 , !!슈퍼구 최대크기 보다 큰 경우
                if (newsphere)
                {
                    //assert(supersphere->HasSpherePackFlag(SPF_ROOTNODE));

                    src_pack.Unlink();


                    //SphereModel parent = _spheres.GetFreeLink();
                    //parent.Init(this, src_pack.GetPos(), src_pack.GetRadius() + _gravy_supersphere);

                    //if (supersphere.HasFlag(SphereModel.Flag.TREE_LEVEL_1))
                    //    parent.AddFlag(SphereModel.Flag.TREE_LEVEL_1);
                    //else
                    //parent.AddFlag(SphereModel.Flag.TREE_LEVEL_2);

                    PT_SphereModel parent = AddSphere(src_pack.GetPos(), src_pack.GetRadius() + _gravy_supersphere, supersphere.GetFlag());
                    parent.AddFlag(PT_SphereModel.Flag.SUPERSPHERE);
                    parent.AddChild(src_pack);
                    supersphere.AddChild(parent);

                    parent.Recompute(_gravy_supersphere);
                    src_pack.Compute_BindingDistanceSquared(parent);

                    if (parent.HasFlag(PT_SphereModel.Flag.TREE_LEVEL_2))
                    {
                        // need to create parent association!
                        PT_SphereModel link = AddSphere(parent.GetPos(), parent.GetRadius(), PT_SphereModel.Flag.TREE_LEVEL_1);
                        AddIntegrateQ(link);
                        link.SetLink_DownLevelTree(parent);
                        parent.SetLink_UpLevelTree(link);
                    }

                }
            }

            src_pack.ClearFlag(PT_SphereModel.Flag.INTEGRATE); // we've been integrated!
        }


        public void Render_RayTrace(Vector3 start, Vector3 end)
        {
            _level_1.RayTrace(start, end);
        }

        public void Render_RangeTest(Vector3 pos, float range)
        {

            _level_1.RangeTest(pos, range, Frustum.ViewState.PARTIAL);
        }

        public void Render_FrustumTest(Frustum f, Frustum.ViewState state)
        {

            _level_1.VisibilityTest(f, state);
        }

        public void Render_Debug(int mode , bool isText)
        {
            if (1 == mode || 3 == mode)
                _level_1.Render_Debug(isText);
            if (2 == mode || 3 == mode)
                _level_2.Render_Debug(isText);
        }
    }
}


