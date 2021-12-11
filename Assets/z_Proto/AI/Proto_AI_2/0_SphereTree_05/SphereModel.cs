﻿using System;
using UnityEngine;
using UtilGS9;

namespace ST_Test_005
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

            TREE_LEVEL_0 = (1 << 7),   //전체트리의 Level_0 을 구성하는 분리된 트리 
            TREE_LEVEL_1 = (1 << 8),   //전체트리의 Level_1 을 구성하는 분리된 트리 
            TREE_LEVEL_2 = (1 << 9),   //전체트리의 Level_2 을 구성하는 분리된 트리 
            TREE_LEVEL_3 = (1 << 10),   //전체트리의 Level_3 을 구성하는 분리된 트리 

            CREATE_LEVEL_LAST = (1 << 11), //마지막레벨에 생성하라는 생성지시자로 사용 


            TREE_LEVEL_0123 = TREE_LEVEL_0 | TREE_LEVEL_1 | TREE_LEVEL_2 | TREE_LEVEL_3,

        }

        private Vector3 _center;
        private float _radius;
        //public float _radius_sqr;


        //------------------------------------------------------
        //                   메모리풀 전용 변수
        //------------------------------------------------------
        private int _id = -1;
        private bool _isUsed = false;
        private SphereModel _pool_next = null;
        private SphereModel _pool_prev = null;

        //------------------------------------------------------
        //                    레벨 링크 변수
        //------------------------------------------------------
        private SphereModel _link_upLevel_childSphere = null; //윗 level 의 트리 링크
        private SphereModel _link_downLevel_supherSphere = null; //아랫 level 의 트리 링크 

        //------------------------------------------------------
        //                    형제 노드 변수
        //------------------------------------------------------
        private SphereModel _sibling_next = null; //내기준 왼쪽 형제노드 (내가 머리라면 null값임)
        private SphereModel _sibling_prev = null; //내기준 오른쪽 형제노드 (내가 꼬리라면 null값임) 

        //------------------------------------------------------
        //                    슈퍼구 노드 변수
        //------------------------------------------------------
        private SphereModel _superSphere = null; //슈퍼구를 가리킨다 - _link_upLevelTree 과는 크기와 위치는 같지만 레벨이 다르다 , 현재레벨에서의 생성된 슈퍼구임  

        //------------------------------------------------------
        //                  슈퍼구에서의 자식 정보
        //------------------------------------------------------
        private SphereModel _head_children = null;  //형제노드의 첫번째 노드 
        private int _childCount = 0;
        //------------------------------------------------------

        public QFifo<SphereModel>.Out_Point _recompute_fifoOut;
        public QFifo<SphereModel>.Out_Point _intergrate_fifoOut;

        //------------------------------------------------------

        private Flag _flags;


        private IUserData _link_userData = null;

        //------------------------------------------------------

        
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

        public void SetPos(Vector3 pos)
        {
            _center = pos;
            SphereModel link = GetLink_UpLevel_ChildSphere();
            if (null != link)
            {
                //uplevel 자식구도 동일하게 맞춰준다
                link._center = _center;
            }
        }

        public void SetRadius(float radius)
        {
            _radius = radius;
            //_radius_sqr = radius * radius;
            SphereModel link = GetLink_UpLevel_ChildSphere();
            if (null != link)
            {
                //uplevel 자식구의 크기를 동일하게 맞춰준다
                link._radius = _radius;
            }
        }

        public float GetRadius() { return _radius; }
        //public float GetRadiusSqr() { return _radius_sqr; }
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
            if (0 != (_flags & Flag.TREE_LEVEL_0)) return 0;
            if (0 != (_flags & Flag.TREE_LEVEL_1)) return 1;
            if (0 != (_flags & Flag.TREE_LEVEL_2)) return 2;
            if (0 != (_flags & Flag.TREE_LEVEL_3)) return 3;

            return -1;
        }

        //=====================================================
        //트리 링크 다루는 함수
        //=====================================================
        public void SetSuperSphere(SphereModel model) { _superSphere = model; }
        public SphereModel GetSuperSphere() { return _superSphere; }
        public SphereModel GetChildren() { return _head_children; }
        public void SetNextSibling(SphereModel child) { _sibling_next = child; }
        public void SetPrevSibling(SphereModel child) { _sibling_prev = child; }
        public SphereModel GetNextSibling() { return _sibling_next; }
        public SphereModel GetPrevSibling() { return _sibling_prev; }

        public void SetRecompute_FifoOut(QFifo<SphereModel>.Out_Point fifo_out) { _recompute_fifoOut = fifo_out; }
        public void SetIntergrate_FifoOut(QFifo<SphereModel>.Out_Point fifo_out) { _intergrate_fifoOut = fifo_out; }
        public void InitRecompute_FifoOut() { _recompute_fifoOut.Init(); }
        public void InitIntergrate_FifoOut() { _intergrate_fifoOut.Init(); }

        public void SetLink_UpLevel_ChildSphere(SphereModel data) { _link_upLevel_childSphere = data; }
        public SphereModel GetLink_UpLevel_ChildSphere() { return _link_upLevel_childSphere; }
        public void SetLink_DownLevel_SuperSphere(SphereModel data) { _link_downLevel_supherSphere = data; }
        public SphereModel GetLink_DownLevel_SuperSphere() { return _link_downLevel_supherSphere; }


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
            //SetRadius(radius);
            _radius = radius;

            _superSphere = null;
            _head_children = null;
            _sibling_next = null;
            _sibling_prev = null;

            _flags = 0;
            _recompute_fifoOut.Init();
            _intergrate_fifoOut.Init();
            _childCount = 0;
            _binding_distance_sqr = 0f;
            _link_upLevel_childSphere = null;
            _link_downLevel_supherSphere = null;

            _treeController = controller;
        }


        public void NewPos(Vector3 pos)
        {
            //_center = pos;

            SetPos(pos);

            if (null != _superSphere && false == HasFlag(Flag.INTEGRATE))
            {
                float sqrDist = ToDistanceSquared(_superSphere);

                //자식구가 슈퍼구에서 벗어났음
                if (sqrDist >= _binding_distance_sqr)
                {

                    if (false == _superSphere.HasFlag(Flag.RECOMPUTE))
                    {
                        //DebugWide.LogGreen("NewPos recomp : s_id : " + _superSphere._id + "  id : " + _id + "  s_flag: " + _superSphere._flags.ToString());
                        _treeController.AddRecomputeQ(_superSphere); //슈퍼구 다시 계산  
                    }

                    Unlink();

                    //슈퍼구의 자식이 1일 경우 unlink를 통해 제거대상이 된다. 불필요한 처리로 추정 
                    //DebugWide.LogGreen("NewPos interg : s_id : " + _superSphere._id + "  id : " + _id + "  s_flag: " + _superSphere._flags.ToString());
                    _treeController.AddIntegrateQ(this); //자식구 어디에 통합시킬지 다시 계산
                }

            }

        }


        public void NewPosRadius(Vector3 pos, float radius)
        {
            //_center = pos;

            SetPos(pos);
            SetRadius(radius);

            if (null != _superSphere && false == HasFlag(Flag.INTEGRATE)) //첫번째 통합요청이 선점. 두번째 이상의 통합요청은 무시된다  
            {

                Compute_BindingDistanceSquared(_superSphere); //반지름 변경에 따른 슈퍼구와 묶인거리 다시 계산
                float sqrDist = ToDistanceSquared(_superSphere);

                //자식구가 슈퍼구에서 벗어났음
                if (sqrDist >= _binding_distance_sqr)
                {
                    if (false == _superSphere.HasFlag(Flag.RECOMPUTE))
                    {
                        //DebugWide.LogGreen("NewPosRadius : s_id : " + _superSphere._id + "  id : " + _id + "  s_flag: " + _superSphere._flags.ToString());
                        _treeController.AddRecomputeQ(_superSphere); //슈퍼구 다시 계산
                    }
                    Unlink();
                    _treeController.AddIntegrateQ(this); //자식구 어디에 통합시킬지 다시 계산
                }
                else
                {
                    if (false == _superSphere.HasFlag(Flag.RECOMPUTE))
                    {
                        //DebugWide.LogGreen("NewPosRadius : s_id : " + _superSphere._id + "  id : " + _id + "  s_flag: " + _superSphere._flags.ToString());
                        _treeController.AddRecomputeQ(_superSphere); //슈퍼구 다시 계산
                    }
                }

            }

            //string temp = "";
            //if (null != _superSphere)
            //{
            //    temp += "  s_id: " + _superSphere.GetID() + "  s_fl: " + _superSphere._flags.ToString();
            //}
            //DebugWide.LogBlue(" setPosRadius _id: " + _id + "  _fl: " + _flags.ToString() + temp);

        }

        public void AddChild(SphereModel pack)
        {
            //if(null == pack)
            //{
            //    DebugWide.LogGreen("AddChild !--  s_id: " + _id + "  p_id: " + pack.GetID() + "------ null pack !!!!");
            //}
            //if (false == HasFlag(Flag.SUPERSPHERE))
            //{
            //    DebugWide.LogGreen("AddChild !--  s_id: " + _id + "  p_id: " + pack.GetID() + "------ not supersphere !!!!");
            //}

            //pack.Unlink(); //기존정보를 해제후 추가한다 

            SphereModel next_child = _head_children;
            //test-------------------------------------
            //string temp = "";
            //int ct = 0;
            //while (null != next_child)
            //{
            //    temp += " " + next_child._id + " "; 
            //    ct++;
            //    next_child = next_child.GetNextSibling();
            //    if (15 < ct) break;
            //}
            //DebugWide.LogBlue("add before: s_id: " +_id + "  p_id: " + pack.GetID() + "  _cCt: " + _childCount +" : "+ temp + "  s_used: " + _isUsed);
            //test-------------------------------------


            next_child = _head_children;
            _head_children = pack; // new head of list

            pack.SetNextSibling(next_child); // his next is my old next
            pack.SetPrevSibling(null); // at head of list, no previous

            pack.SetSuperSphere(this);

            if (null != next_child) next_child.SetPrevSibling(pack); //다음자식의 이전형제 정보를 넣어준다 

            _childCount++;

            //test-------------------------------------
            //next_child = _head_children;
            //int count = 0;
            //while (null != next_child)
            //{
            //    count++;
            //    next_child = next_child.GetNextSibling();
            //    if (100 < count) break;
            //}
            //if (_childCount != count)
            //{
            //    //DebugWide.Log(_lostChildLog);
            //    DebugWide.LogGreen("AddChild !!--  s_id: " + _id + "  p_id: " + pack.GetID() + "  _cCt: " + _childCount + "  wCt: " + count + "  isUsed: " + IsUsed() + "  isSuper: " + pack.HasFlag(Flag.SUPERSPHERE));
            //    DebugWide.LogBlue(temp);
            //    next_child = _head_children;
            //    count = 0;
            //    temp = "";
            //    while (null != next_child)
            //    {
            //        //DebugWide.LogGreen(count+ " child id: "+ next_child._id + "");
            //        temp += " " + next_child._id + " ";
            //        count++;
            //        next_child = next_child.GetNextSibling();
            //        if (15 < count) break;
            //    }
            //    DebugWide.LogGreen(temp);
            //}
            //test-------------------------------------

        }

        public void Unlink()
        {
            //Q에 등록되어 있다면 해제한다 
            //SphereModel qdata = null;
            //int getid = -1;
            //if (null != _recompute_fifoOut.packFifo)
            //{
            //    qdata = _recompute_fifoOut.packFifo.mFifo[_recompute_fifoOut.queueIndex];
            //    if(null != qdata)
            //    {
            //        getid = qdata._id; 
            //    }
            //}
            //int s_id = -1;
            //if(null != _superSphere)
            //{
            //    s_id = _superSphere._id;
            //}
            //DebugWide.LogBlue("unlink recomputeQ : s_id : " + s_id +  " id : " + _id + "  flag: " + HasFlag(Flag.SUPERSPHERE) + "  qidx: " + _recompute_fifoOut.queueIndex + "  qpack_id: " + getid + "  qnull: " + _recompute_fifoOut.IsNull());
            if (false == _recompute_fifoOut.IsNull())
            {
                //Q의 슈퍼구 연결정보를 날린다 
                _recompute_fifoOut.Unlink();
                _recompute_fifoOut.Init();
            }
            if (false == _intergrate_fifoOut.IsNull())
            {
                _intergrate_fifoOut.Unlink();
                _intergrate_fifoOut.Init();
            }

            if (null != _superSphere) _superSphere.LostChild(this);


            _superSphere = null;
        }

        //string _lostChildLog = "";
        private void LostChild(SphereModel child)
        {
            //슈퍼구에 자식정보가 없는 경우는 없다고 가정된다 , 슈퍼구 생성후 바로 LostChild 한 경우도 비정상이라 판단 
            if (null == _head_children || 0 == _childCount)
            {
                //DebugWide.LogError("null == _children || 0 == _childCount"); //assert
                DebugWide.LogError("lostChild --  p_id: " + child.GetID()  + " s_id :"+_id+ "  p_lv: " + child.GetLevelIndex() + "  s_ct: "+ _childCount + "  s_head: " + _head_children + "  s_flag: " + _flags.ToString());

            }

            //DebugWide.LogBlue("a - LostChild -- s_id : " + _id + "  s_ct :" + _childCount + "  p_id: " + child._id);

            //test-------------------------------------
            //SphereModel next_child = _head_children;
            //int count = 0;
            //while (null != next_child)
            //{
            //    count++;
            //    next_child = next_child.GetNextSibling();
            //    if (100 < count) break;
            //}
            //if (_childCount != count)
            //{
            //    DebugWide.LogGreen("LostChild !!--  s_id: " + _id + "  p_id: " + child.GetID() + "  _cCt: " + _childCount + "  wCt: " + count);
            //}
            //test-------------------------------------

            //next_child = _head_children;
            //string temp = "";
            //int ct = 0;
            //while (null != next_child)
            //{
            //    temp += " " + next_child._id + " ";
            //    ct++;
            //    next_child = next_child.GetNextSibling();
            //    if (30 < ct) break;
            //}
            //temp = "A lost : s_id: " + _id + " p_id : " + child._id + "  ct: "+ _childCount +" > " + temp + "\n";
            //DebugWide.LogBlue("A lost : s_id: " + _id + " p_id : " + child._id + " > " + temp);
            //test-------------------------------------

            // first patch old linked list.. his previous now points to his next
            SphereModel prev = child.GetPrevSibling();

            if (null != prev)
            {
                SphereModel next = child.GetNextSibling();
                prev.SetNextSibling(next); // my previous now points to my next
                if (null != next) next.SetPrevSibling(prev);
                // list is patched!
            }
            else
            {
                SphereModel next = child.GetNextSibling();
                _head_children = next;
                if (null != _head_children) _head_children.SetPrevSibling(null);
            }

            child.SetPrevSibling(null);
            child.SetNextSibling(null);

            _childCount--;

            //DebugWide.LogBlue("b - LostChild -- s_id : " + _id + "  s_ct :" + _childCount + "  p_id: " + child._id);

            //test-------------------------------------
            //next_child = _head_children;
            //string temp2 = "";
            //ct = 0;
            //while (null != next_child)
            //{
            //    temp2 += " " + next_child._id + " ";
            //    ct++;
            //    next_child = next_child.GetNextSibling();
            //    if (30 < ct) break;
            //}
            //temp2 = "B lost : s_id: " + _id + " p_id : " + child._id + "  ct: "+ _childCount+ " > " + temp2 ;
            //_lostChildLog = temp + temp2;
            //DebugWide.LogBlue(temp2);
            //test-------------------------------------

            if (null == _head_children || 0 == _childCount)
            {
                //자식없는 슈퍼구는 제거한다 
                if (HasFlag(Flag.SUPERSPHERE))
                {
                    //_treeController.Remove_SuperSphereAndLinkSphere(this);
                    Unlink_SuperSphereAndLinkSphere();
                }

            }
        }

        //슈퍼구lv3 -> 자식구lv2 -> 슈퍼구lv2 -> 자식구lv1 ... 순서로 해제 
        //슈퍼구와 그리고 슈퍼구와 연결된상위 자식구를 해제 
        public void Unlink_SuperSphereAndLinkSphere()
        {
            //string temp = "";
            //if(null != _superSphere)
            //{
            //    temp = "  : super : " + _superSphere._id + "  " + _flags.ToString() + "  " + _childCount;
            //}
            //DebugWide.LogGreen(" a  Remove_SuperSphereAndLinkSphere !!--  _id: " + GetID() + "  flag: " + GetFlag().ToString() + " ct: " + GetChildCount() + temp);

            if (HasFlag(SphereModel.Flag.ROOTNODE)) return; // CAN NEVER REMOVE THE ROOT NODE EVER!!!

            //슈퍼구해제 
            Unlink(); //_superSphere.LostChild(this); => Remove_SuperSphereAndLinkSphere
            _treeController.ReleasePool(this);


            //자식구해제
            //temp = "";
            //if (HasFlag(SphereModel.Flag.SUPERSPHERE))
            SphereModel link = GetLink_UpLevel_ChildSphere();
            if(null != link)
            {
                link.Unlink_SuperSphereAndLinkSphere();

                //temp = "--> link_id : " + link.GetID();
            }

            //DebugWide.LogGreen(temp + " b  Remove_SuperSphereAndLinkSphere !!--  s_id: " + GetID() + "  flag: " + GetFlag().ToString() + " ct: " + GetChildCount());
            //DebugWide.LogGreen(temp+" Q list : " + ToStringQ(_recomputeQ));

        }


        //Geo.Include_Sphere_Fully 함수와 알고리즘이 같다. 완전포함 조건 살펴보기 
        //제약조건이 없으나, 인자값은 슈퍼구가 와야 한다. 의도에 맞게 사용해야함 
        public void Compute_BindingDistanceSquared(SphereModel superSphere)
        {
            _binding_distance_sqr = superSphere.GetRadius() - GetRadius();
            if (_binding_distance_sqr <= 0) _binding_distance_sqr = 0;

            _binding_distance_sqr = _binding_distance_sqr * _binding_distance_sqr;
        }

        //슈퍼구만 계산의 대상이 된다
        public bool RecomputeSuperSphere(float gravy)
        {
            if (null == _head_children) return true; // kill it!
            if (HasFlag(Flag.ROOTNODE)) return false; // don't recompute root nodes!
            if (false == HasFlag(Flag.SUPERSPHERE)) return false; 

            //DebugWide.LogGreen(_id + "  " + _flags.ToString());

            //#if 1
            // recompute bounding sphere!
            Vector3 total = ConstV.v3_zero;
            int count = 0;
            SphereModel pack = _head_children;
            //while (null != pack)
            for (int i = 0; i < _childCount; i++)
            {
                if (null == pack)
                {
                    DebugWide.LogRed("Recompute --a-- i: " + i + "  !!!!!!!! id: " + GetID() + "  ct: " + GetChildCount()); //test
                    return false;
                }
                else if (pack == pack.GetNextSibling())
                {
                    DebugWide.LogRed("Recompute --b-- i: " + i + "  !!!!!!!! id: " + GetID() + "  ct: " + GetChildCount()); //test
                    return false;
                }

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

                //while (null != pack)
                for (int i = 0; i < _childCount; i++)
                {
                    //float dist = ToDistanceSquared(pack);
                    float dist = (_center - pack._center).sqrMagnitude;
                    float radius = (float)Math.Sqrt(dist) + pack.GetRadius();
                    if (radius > maxradius)
                    {
                        maxradius = radius;
                        if ((maxradius + gravy) >= GetRadius())
                        {
                            _center = oldpos; //새로운 센터기준으로 자식들이 경계를 벗어났으면 기존센터값으로 되돌린다 , 통합에서 처리 
                            ClearFlag(Flag.RECOMPUTE);
                            //DebugWide.LogBlue("false -- - - Recompute s_id: " + _id + " flag: " + _flags.ToString());
                            return false;
                        }
                    }
                    pack = pack.GetNextSibling();
                }

                maxradius += gravy;

                _radius = maxradius;
                //SetRadius(maxradius); //최종 반지름 갱신

                // now all children have to recompute binding distance!!
                pack = _head_children;

                //while (null != pack)
                for (int i = 0; i < _childCount; i++)
                {
                    pack.Compute_BindingDistanceSquared(this);
                    pack = pack.GetNextSibling();
                }

                //==============================================
                //이렇게 사용하면 안됨 , 큐에서 데이터를 꺼내는 루프중에 큐에 추가하는 잘못된 처리이다  
                //!!!! 슈퍼구에 연결된 상위레벨 자식구도 갱신한다 
                //SphereModel link = GetLink_UpLevelTree();
                //if (null != link)
                //{
                //    DebugWide.LogGreen("before Q list  --- " +_treeController.ToStringQ(_treeController._recomputeQ));
                //    //DebugWide.LogBlue("a -- - - Recompute s_id: " + _id + " flag: " + _flags.ToString() + "  link_id: " + link._id + "  link_flag: " + link._flags.ToString());
                //    link.SetPosRadius(_center, maxradius);
                //    link.SetRadius(maxradius); //반지름이 갱신 안되는 경우가 있다 (통합상태)
                //    DebugWide.LogGreen(" after Q list  --- " + _treeController.ToStringQ(_treeController._recomputeQ));

                //}

                SphereModel link = GetLink_UpLevel_ChildSphere();
                if (null != link)
                {
                    //uplevel 자식구의 크기와 위치를 동일하게 맞춰준다
                    link._center = _center;
                    link._radius = _radius;

                    //DebugWide.LogBlue("true -- - - Recompute s_id: " + _id + " flag: " + _flags.ToString() + "  link_id: " + link._id + "  link_flag: " + link._flags.ToString() + "  " + link.GetLevelIndex() + "  " + link.GetSuperSphere() + "  l_used: " + link.IsUsed());
                    //if(null != link._superSphere)
                    {
                        link._superSphere.RecomputeSuperSphere(gravy);
                    }

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
                    SphereModel upLink = this.GetLink_UpLevel_ChildSphere();
                    SphereModel downLink = this.GetLink_DownLevel_SuperSphere();


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

        //fixme
        //public void RangeTest_MinDisReturn(Frustum.ViewState state, ref Param_RangeTest param)
        //{

        //    //float between_sqr = VOp.Minus(param.src_pos, _center).sqrMagnitude; //stakoverflow ?? 스택오버플로우가 발생하기 때문에 사용하지 말기 
        //    float between_sqr = (param.src_pos - _center).sqrMagnitude; //여기서도 스택오버플러우 발생 
        //    if (state == Frustum.ViewState.PARTIAL)
        //    {
            
        //        //완전비포함 검사
        //        float sqrSumRd = (_radius + param.maxRadius) * (_radius + param.maxRadius);
        //        if (between_sqr > sqrSumRd) return;

        //        //완전포함 검사 
        //        if (param.maxRadius >= _radius)
        //        {
        //            float sqrSubRd = (param.maxRadius - _radius) * (param.maxRadius - _radius);
        //            if (between_sqr <= sqrSubRd) state = Frustum.ViewState.INSIDE; //슈퍼구가 포함되면 자식구까지 모두 포함되므로, 계산할 필요가 없어진다     
        //        }

        //    }

        //    if (HasFlag(Flag.SUPERSPHERE))
        //    {
        //        SphereModel pack = _head_children;

        //        //while (null != pack)
        //        for (int i = 0; i < this.GetChildCount(); i++)
        //        {
        //            if (null == pack)
        //            {
        //                DebugWide.LogRed("RangeTest_MinDisReturn --a--" + i + "  id:" + this._id + "  " + this._childCount); //test
        //                //break;
        //            }
        //            else if (pack == pack.GetNextSibling())
        //            {
        //                DebugWide.LogRed("RangeTest_MinDisReturn --b--" + i + "  id:"+ this._id + "  " + this._childCount); //test
        //                //break;
        //            }

        //            pack.RangeTest_MinDisReturn(state, ref param);
        //            pack = pack.GetNextSibling();
        //        }

        //        //SingleO.debugViewer.AddDraw_Circle(_center, _radius, Color.gray);
        //    }
        //    else
        //    {
        //        SphereModel upLink = this.GetLink_UpLevel_ChildSphere();
        //        SphereModel downLink = this.GetLink_DownLevel_SuperSphere();

        //        //자식노드에 연결된 하위레벨 슈퍼구 
        //        if (null != downLink)
        //            downLink.RangeTest_MinDisReturn(state, ref param);
                
        //        //전체트리의 최하위 자식노드 
        //        if (null == upLink && null == downLink)
        //        {
        //            //최소반지름의 완전포함을 걸러낸다 
        //            bool isFully = Geo.Include_Sphere_Fully(param.src_pos, param.minRadius, _center, _radius);
        //            if (false == isFully)
        //            {
        //                //등록된 검사용 콜백함수 호출
        //                if (null != param.callback)
        //                {
        //                    //SingleO.debugViewer.AddDraw_Circle(_center, _radius, Color.white);
        //                    //검사용 콜백함수를 통과하면 최하위 노드인 자기자신을 반환한다.
        //                    if (true == param.callback(ref param, this))
        //                    {
        //                        param.find = this;

        //                    }
                                
        //                }
        //                else
        //                {
        //                    //검사함수가 없으면 자기자신을 반환한다 
        //                    param.find = this;
        //                }

        //            }
        //        }

        //    }

        //}


        public struct Param_RangeTest<ENTITY> where ENTITY : class, new()
        {
            //==============================================
            public SphereModel find; //결과값 

            public ENTITY unit;
            //public Camp.eRelation vsRelation;
            //public Camp.eKind unit_campKind;

            public Vector3 src_pos;
            public float minRadius;
            public float maxRadius;
            //public float maxRadiusSqr;

            public delegate bool Proto_ConditionCheck(ref Param_RangeTest<ENTITY> param, SphereModel dstModel);
            public Proto_ConditionCheck callback;
            //==============================================

            
            public Param_RangeTest(ENTITY in_srcUnit, Vector3 pos, float meter_minRadius, float meter_maxRadius)
            {
                find = null;

                unit = in_srcUnit;
                //vsRelation = in_vsRelation;
                //unit_campKind = in_srcUnit._campKind;
                src_pos = pos;
                minRadius = meter_minRadius;
                maxRadius = meter_maxRadius;
                //maxRadiusSqr = maxRadius * maxRadius;

                callback = Param_RangeTest<ENTITY>.Func_ConditionCheck;
            }

            //==============================================

            static public bool Func_ConditionCheck(ref Param_RangeTest<ENTITY> param, SphereModel dstModel)
            {
                //return true;

                //기준객체는 검사대상에서 제외한다 - fixme 
                //if (null != param.unit && param.unit._sphereModel == dstModel) return false;

                ENTITY dstBeing = dstModel.GetLink_UserData() as ENTITY;
                //BaseEntity dstUnit = dstModel.GetLink_UserData() as BaseEntity;

                if (null != dstBeing)
                {
                    //가시거리 검사 
                    return true;
                    //return GridManager.Inst.IsVisibleTile(param.src_pos, dstModel.GetPos(), 10);
                }

                return false;
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
                SphereModel upLink = this.GetLink_UpLevel_ChildSphere();
                SphereModel downLink = this.GetLink_DownLevel_SuperSphere();

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
                SphereModel upLink = this.GetLink_UpLevel_ChildSphere();
                SphereModel downLink = this.GetLink_DownLevel_SuperSphere();
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
                SphereModel link = this.GetLink_DownLevel_SuperSphere();
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
                int count = 0;
                while (null != pack)
                {
                    count++;

                    pack.Debug_Render(color, isText);
                    pack = pack.GetNextSibling();
                }
                //DebugWide.LogBlue("  -----  " + count + "  " + _childCount + " "  + HasFlag(Flag.SUPERSPHERE));
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

                        SphereModel link = GetLink_UpLevel_ChildSphere();

                        if (null != link)
                            link = link.GetSuperSphere(); //level_1 트리의 슈퍼구노드를 의미한다 

                        if (null != link && false == link.HasFlag(Flag.ROOTNODE))
                        {
                            DebugWide.DrawLine(_center, link._center, Color.white);
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
                    if (HasFlag(Flag.TREE_LEVEL_0)) { temp += "\n"; level = 0; }
                    if (HasFlag(Flag.TREE_LEVEL_1)) { temp += "\n        "; level = 1; }
                    if (HasFlag(Flag.TREE_LEVEL_2)) { temp += "\n                "; level = 2; }
                    if (HasFlag(Flag.TREE_LEVEL_3)) { temp += "\n                        "; level = 3; }
                    if (HasFlag(Flag.SUPERSPHERE)) { temp += "s"; }

                    //DebugWide.PrintText(_center, color, temp + GetID());
                    //DebugWide.PrintText(_center, Color.black, temp + level);
                    DebugWide.PrintText(_center, Color.black, _radius.ToString("F2"));
                }

            }

        }

    }
}

