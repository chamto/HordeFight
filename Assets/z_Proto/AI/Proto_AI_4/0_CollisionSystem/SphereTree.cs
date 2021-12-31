using System;
using UnityEngine;
using UtilGS9;

namespace Proto_AI_4
{
    
    //=================================================================

    public class SphereTree
    {
        //트리깊이가 4일때 
        //_levels[3] 트리의 슈퍼구를 복제한 _levels[2] 자식구를 연결 
        //_levels[2] 트리의 슈퍼구를 복제한 _levels[1] 자식구를 연결 
        //_levels[1] 트리의 슈퍼구를 복제한 _levels[0] 자식구를 연결 
        //각각의 레벨을 외부에서는 하나의 트리인것 처럼 보이게 한다
        private SphereModel[] _levels = null;

        private Pool<SphereModel> _pool_sphere = null; //메모리풀

        public QFifo<SphereModel>[] _q_recompute_super = null; //재계산 해야하는 슈퍼구
        public QFifo<SphereModel>[] _q_integrate_child = null; //재계산 해야하는 자식구 
        //public QFifo<SphereModel> _q_integrate_super = null;

        public const int MAX_LEVEL = 10;
        public const int CREATE_LEVEL_LAST = -1;

        public int _max_level = MAX_LEVEL;
        public float[] _maxRadius_childSphere = null; //자식구(링크구)의 최대값임 
        public float _gravy_supersphere;     //슈퍼구 증가값임 !! , 이 값으로 인해 _maxRadius_childSphere 보다 크기가 커진 구가 된다       
        // - 자식구를 포함하는 최대 크기에서 gravy양 만큼 크게 슈퍼구를 조정한다 
        // - 값이 커지면 통합계산시 가까운슈퍼구 계산에 실패해 새로운 슈퍼구를 생성하게 만든다. 
        //   최대 슈퍼구의 길이에서 gravy를 뺀 길이 보다 "자식구+가까운슈퍼구 반지름"이 작을때만 가까운슈퍼구 계산에 성공한다  
        // - 자식구가 gravy 보다 작게 이동할때 슈퍼구를 벗어나지 않기에, 잦은 계산을 피하기위한 용도인 것으로 추정된다 



        public SphereTree(int maxspheres, float[] list_maxRadius, float gravy)
        {
            _max_level = list_maxRadius.Length;

            if (MAX_LEVEL < _max_level) _max_level = MAX_LEVEL;

            //DebugWide.LogBlue(_max_level);

            _levels = new SphereModel[_max_level];
            _maxRadius_childSphere = new float[_max_level];
            for (int i = 0; i < _max_level;i++)
            {
                _maxRadius_childSphere[i] = list_maxRadius[i];
            }
            _gravy_supersphere = gravy;

            //최대레벨이 2일 경우
            //메모리풀 크기를 4배 하는 이유 : 각각의 레벨트리는 자식구에 대해 1개의 슈퍼구를 각각 만든다. 레벨트리 1개당 최대개수 *2 의 크기를 가져야 한다. 
            //레벨트리가 2개 이므로 *2*2 가 된다.
            //구의 최대개수가 5일때의 최대 메모리 사용량 : 레벨2트리 <루트1개 + 구5개 + 슈퍼구5개> , 레벨1트리 <루트1개 + 슈퍼구5개 + 복제된슈퍼구5개>

            int max_pool = (maxspheres * 2 * _max_level) + _max_level;
            int max_levelQ = (maxspheres * 2); //각각의 레벨에 있는 루트구는 계산의 대상이 아니다 , 자신의 레벨만 계산대상으로 한다 
            _pool_sphere = new Pool<SphereModel>();
            _pool_sphere.Init(max_pool);       // init pool to hold all possible SpherePack instances.

            _q_recompute_super = new QFifo<SphereModel>[_max_level];
            _q_integrate_child = new QFifo<SphereModel>[_max_level];
            for (int i = 0; i < _max_level; i++)
            {
                _q_recompute_super[i] = new QFifo<SphereModel>(max_levelQ);
                _q_integrate_child[i] = new QFifo<SphereModel>(max_levelQ);
            }
            //_q_integrate_super = new QFifo<SphereModel>(max_levelQ);

            for (int i = 0; i < _max_level;i++)
            {
                //루트구 생성 
                _levels[i] = _pool_sphere.GetFreeLink(); // initially empty
                _levels[i].Init(this, i, Vector3.zero, 65536);
                _levels[i].AddFlag(SphereModel.Flag.SUPERSPHERE | SphereModel.Flag.ROOTNODE);

            }

        }



        public SphereModel AddSphere(Vector3 pos, float radius, int level)
        {

            SphereModel pack = _pool_sphere.GetFreeLink();
            //DebugWide.LogBlue(_pool_sphere.GetFreeCount() + "  " + _pool_sphere.GetUsedCount());
            if (null == pack)
            {
                DebugWide.LogError("AddSphere : GetFreeLink() is Null !!");
                return null;
            }


            //CREATE_LEVEL_LAST 요청이 들어올 경우 마지막 레벨트리의 인덱스를 찾아 넣어준다 
            //if(0 != (flags & SphereModel.Flag.CREATE_LEVEL_LAST))
            //{
            //    level = _levels.Length - 1;
            //    //int last_level_idx = _levels.Length - 1;
            //    //flags = (SphereModel.Flag)((int)(SphereModel.Flag.TREE_LEVEL_0) << last_level_idx);
            //    //DebugWide.LogBlue(flags);
            //}

            if(level <= CREATE_LEVEL_LAST)
            {
                level = _levels.Length - 1;
            }

            pack.Init(this, level, pos, radius); //AddSpherePackFlag 함수 보다 먼저 호출되어야 한다. _flags 정보가 초기화 되기 때문이다. 


            //if (0 == (flags & SphereModel.Flag.TREE_LEVEL_0123))
            //{
            //    DebugWide.LogError("AddSphere : TREE_LEVEL is None !!  " + flags);
            //    return null;
            //}
            //pack.AddFlag(flags); //level 1~4 flag 만 통과시킨다 

            _levels[level].AddFirst_Child(pack);

            return pack;
        }

        //!!자식구만 통합의 대상이 된다. 함수에 슈퍼구를 넣으면 안됨 
        //대상구를 어떤 슈퍼구에 포함하거나 , 포함 할 슈퍼구가 없으면 새로 만든다 
        public void AddIntegrateQ(SphereModel pack)
        {

            int level = pack._level;
            if(false == pack.HasFlag(SphereModel.Flag.INTEGRATE))
            {
                //_levels[level].AddChild(pack); //자식구를 unlink 하고 AddIntegrateQ 가 호출되었을 경우 
                //링크가 사라진 자식구를 루트레벨구에 붙이는 처리를 한다 
                //통합계산시 루트레벨 목록에서 자식구 정보가 필요없다. 필요없는 정보를 루트레벨 목록에서 제외하기 위해 주석한다  


                pack.AddFlag(SphereModel.Flag.INTEGRATE); // still needs to be integrated!
                _q_integrate_child[level].Push(pack);
            }

        }

        //!!자식이 있는 구, 즉 슈퍼구만 재계산의 대상이 된다. 
        //슈퍼구의 위치,반지름이 자식들의 정보에 따라 재계산 된다.
        public void AddRecomputeQ(SphereModel superSphere)     // add to the recomputation (balancing) FIFO.
        {
            if (null == superSphere) return; //unlink후 호출되면 null이 된다. 통합에서의 처리 때문에 발생  
            if (false == superSphere.HasFlag(SphereModel.Flag.SUPERSPHERE)) return;

            int level = superSphere._level;
            if (false == superSphere.HasFlag(SphereModel.Flag.RECOMPUTE))
            {
                superSphere.AddFlag(SphereModel.Flag.RECOMPUTE); // needs to be recalculated!
                _q_recompute_super[level].Push(superSphere);

                //if (0 != superSphere.GetChildCount())
                //{
                //    //--------------------->
                //    //bool contain = false;
                //    //if (_recomputeQ.Contain(superSphere))
                //    //{
                //    //    DebugWide.LogGreen("AddRecomputeQ add : " + superSphere.GetID() + "  lv: " + superSphere.GetLevelIndex() + "  ");
                //    //    DebugWide.LogGreen("1 Q list : " + ToStringQ(_recomputeQ));
                //    //    contain = true;
                //    //}
                //    //--------------------->

                //    superSphere.AddFlag(SphereModel.Flag.RECOMPUTE); // needs to be recalculated!
                //    _recomputeQ[level].Push(superSphere);


                //    //--------------------->
                //    //if (contain)
                //    //{
                //        //DebugWide.LogGreen("2 Q list : " + ToStringQ(_recomputeQ));
                //    //}
                //    //--------------------->
                //}
                //else
                //{
                //    //DebugWide.LogWhite("AddRecomputeQ Remove : " + superSphere.GetID());
                //    //Remove_SuperSphereAndLinkSphere(superSphere);
                //    superSphere.Unlink_SuperSphereAndLinkSphere();
                //}
            }
        }


        public void ReleasePool(SphereModel pack)
        {
            _pool_sphere.Release(pack);
        }

        public void ResetFlag()
        {

            for (int i = 0; i < _levels.Length; i++)
            {
                _levels[i].ResetFlag();
            }
        }


        private void ProcessLevel(int level)
        {
            //DebugWide.LogBlue(level);
            //슈퍼구 재계산
            {
                //DebugWide.LogBlue(level +  "<<<<<< Process _recomputeQ : " + ToStringQ(_recomputeQ[level]));
                int maxrecompute = _q_recompute_super[level].GetCount();
                for (int i = 0; i < maxrecompute; i++)
                {
                    SphereModel superSphere = _q_recompute_super[level].Pop();
                    if (null == superSphere) continue;
                    if (false == superSphere.IsUsed()) continue; //Q에 들어있는 데이터가 릴리즈된 것일 수 있다 

                    //DebugWide.LogBlue(" idx : " +i + " - - - - - - - -pop after Q list: " + ToStringQ(_recomputeQ[level]));


                    //if(false == superSphere.IsUsed())
                    //{
                    //    DebugWide.LogRed("---------- "+superSphere._level +  "  " +superSphere.GetID() + "  " + superSphere.GetFlag());
                    //    DebugWide.LogRed("Q list : " + ToStringQ(_recomputeQ[level]));
                    //}
                    //superSphere.RecomputeSuperSphere(_gravy_supersphere);
                    superSphere.RecomputeSuperSphere_Center(_gravy_supersphere, _maxRadius_childSphere[level]);
                }
            }

            //자식구 통합
            {
                //DebugWide.LogBlue(level + "<<<<<< Process _integrateQ : " + ToStringQ(_integrateQ[level]));
                // Now, process the integration step.
                int maxintegrate = _q_integrate_child[level].GetCount();
                for (int i = 0; i < maxintegrate; i++)
                {
                    SphereModel childSphere = _q_integrate_child[level].Pop();
                    if (null == childSphere) continue; //null 데이터는 처리할 수 없다 
                    if (false == childSphere.IsUsed()) continue; //Q에 들어있는 데이터가 릴리즈된 것일 수 있다 

                    int child_level = childSphere._level;
                    Integrate(childSphere, _levels[child_level], _maxRadius_childSphere[child_level]);
                }
            }

        }

        public string ToStringQ(QFifo<SphereModel> Q)
        {
            string temp = "size: " + Q.mFifoSize + " ct : " + Q.mCount + " head : " + Q._head + " tail : " + Q._tail + "  list : ";
            int head = Q._head;
            while (Q._tail != head) //데이터가 있다면 
            {

                SphereModel ret = Q.mFifo[head];
                head++;
                if (head == Q.mFifoSize) head = 0;
                if (null != ret)
                {
                    temp += " " + ret.GetID() + "(" + ret.IsUsed() + ")  ";
                }
                else
                {
                    temp += "  null  ";
                }
            }
            return temp;
        }

        public string ToStringLevel(int level)
        {
            if (level < 0) return "";
            if (_max_level < level) return "";

            string temp = " lv : "+ level +" ct : " + _levels[level].GetChildCount() + "  list : ";
            SphereModel search = _levels[level].GetHeadChild();
            for (int i = 0; i < _levels[level].GetChildCount(); i++)
            {
                if (search.HasFlag(SphereModel.Flag.SUPERSPHERE) &&
                    false == search.HasFlag(SphereModel.Flag.ROOTNODE))
                {
                    temp += " " + search.GetID() + "(" + search.IsUsed() + ")(" + search.GetChildCount()+")";
                }


                search = search.GetNextSibling();
            }

            return temp;        
        }

        public void Process()
        {
            //DebugWide.LogBlue("-----------");
            int level_leaf_idx = _max_level - 1; //최하단 레벨
            for (int i=0;i< _max_level;i++)
            {
                //DebugWide.LogBlue(ToStringLevel(level_leaf_idx - i));
                ProcessLevel(level_leaf_idx - i); //하단레벨에서 상위레벨 순서로 계산한다  
            }

        }


        //src_pack에는 자식구만 들어가야 한다 
        public void Integrate(SphereModel src_pack, SphereModel rootsphere, float maxRadius_supersphere)
        {

            SphereModel containing_supersphere = null;  //src_pack를 포함하는 슈퍼구 
            float includedSqrDist = 1e9f;     // enclosed within. 10의9승. 1000000000.0

            SphereModel nearest_supersphere = null; //src_pack와 가까이에 있는 슈퍼구
            float nearDist = 1e9f;    // add ourselves to.

            SphereModel prev_pack_super = src_pack.GetSuperSphere(); //src_pack 은 초기상태인 경우에만 슈퍼구가 있다. 슈퍼구가 있다면 루트슈퍼구임 
            //if(null != prev_pack_super)
            //{
            //    DebugWide.LogBlue(" p_id : " + src_pack.GetID() +" root_lv: "+ rootsphere._level + " s_id: "+ prev_pack_super.GetID() + " s_lv: " + prev_pack_super._level + " s_ct: " + prev_pack_super.GetChildCount());
            //}

            SphereModel search = rootsphere.GetHeadChild();

            // 1 **** src 와 가장 가까운 슈퍼구 구하기 : src를 포함하는 슈퍼구를 먼저 구함. 없으면 src와 가장 가까운 슈퍼구를 구한다.
            //=====================================================================================
            //DebugWide.LogBlue("root id: "+rootsphere.GetID() + "  src id: " + src_pack.GetID()+ "  ct: " +rootsphere.GetChildCount() + "  lv: " + rootsphere.GetLevelIndex());
            //while (null != search)
            for (int i = 0; i < rootsphere.GetChildCount(); i++)
            {
                //if (null == search)
                //{
                //    DebugWide.LogError("Integrate --a-- i: " + i + "  root id: " + rootsphere.GetID() + "  ct: " + rootsphere.GetChildCount()); //test
                //    break;
                //}
                //else if(search == search.GetNextSibling())
                //{
                //    DebugWide.LogError("Integrate --b-- i: " + i + "  root id: "+ rootsphere.GetID() + "  ct: " + rootsphere.GetChildCount() + "  " + search.GetFlag() + "  " + search.IsUsed()); //test
                //    break; 
                //}

                if (search.HasFlag(SphereModel.Flag.SUPERSPHERE) &&
                    false == search.HasFlag(SphereModel.Flag.ROOTNODE) && 0 < search.GetChildCount())
                {

                    float sqrDist = src_pack.ToDistanceSquared(search);

                    //조건1 - src구가 완전 포함 
                    if (null != containing_supersphere)
                    {
                        if (sqrDist < includedSqrDist)
                        {

                            float dist = (float)Math.Sqrt(sqrDist) + src_pack.GetRadius();

                            //조건1 전용 처리
                            if (dist <= search.GetRadius()) //슈퍼구에 src구가 완전 포함 
                            {
                                includedSqrDist = sqrDist;
                                containing_supersphere = search;
                            }
                        }
                    }
                    //조건2 - 슈퍼구에 걸쳐 있거나 포함되지 않음
                    else
                    {
                        //search 원에 내부에서 spr_pack 원이 접했을때 dist 는 0이 된다 
                        //src_pack 원이 search 원을 벗어난 길이 
                        float dist = ((float)Math.Sqrt(sqrDist) + src_pack.GetRadius()) - search.GetRadius();

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
            }//end for


            //=====================================================================================

            //조건1 - src구가 완전 포함 
            if (null != containing_supersphere)
            {
                //DebugWide.LogBlue(" " + src_pack.GetSuperSphere().GetID());

                //DebugWide.LogBlue("a Integrate : 완전포함 : s_id: " + containing_supersphere.GetID()+" p_id: " + src_pack.GetID() + " isUsed: " + containing_supersphere.IsUsed());
                //containing_supersphere.AddChild(src_pack); //src_pack 의 트리정보를 설정
                //DebugWide.LogBlue("b Integrate : 완전포함 : s_id: " + containing_supersphere.GetID() + " p_id: " + src_pack.GetID() + " isUsed: " + containing_supersphere.IsUsed());

                src_pack.Unlink(); //루트슈퍼구에 붙어 있는 경우를 위해 있는 처리임 
                containing_supersphere.AddFirst_Child(src_pack);

                bool isCals = false;
                //bool isCals = containing_supersphere.RecomputeSuperSphere(_gravy_supersphere);
                isCals = containing_supersphere.RecomputeSuperSphere_Center(_gravy_supersphere, maxRadius_supersphere);


                //---------------------------


            }
            //조건2 - 슈퍼구에 걸쳐 있거나 포함되지 않음
            else
            {
                bool newsphere = true;

                //가까운 거리에 슈퍼구가 있다
                if (null != nearest_supersphere)
                {

                    //float newRadius = nearDist + nearest_supersphere.GetRadius() + _gravy_supersphere ;

                    bool isInclude = Geo.Include_Sphere2_Fully(maxRadius_supersphere * 2 ,
                        nearest_supersphere.GetPos(), nearest_supersphere.GetRadius(), src_pack.GetPos(), src_pack.GetRadius() + _gravy_supersphere);
                    //if (0 == rootsphere._level)
                    //{
                    //    DebugWide.LogGreen("Include_Sphere2_Fully: " + isInclude);
                    //    DebugWide.LogGreen(" ch_id: "+ src_pack.GetID() + " s_id: "+ nearest_supersphere.GetID() + " new_r: " + newRadius + "  nearDist: " + nearDist + "  s_r: " + nearest_supersphere.GetRadius() + " max_s_r: " + maxRadius_supersphere);
                    //}

                    if(isInclude) //기존 newRadius 검사는 슈퍼구의 반지름 만큼 늘어나 있어서 검사가 제대로 안되었다 
                    {
                        //DebugWide.LogBlue(" " + src_pack.GetSuperSphere().GetID());

                        //DebugWide.LogBlue("a Integrate : 크기변경 : s_id: " + nearest_supersphere.GetID() + " p_id: " + src_pack.GetID() + " isUsed: " + nearest_supersphere.IsUsed());
                        //nearest_supersphere.AddChild(src_pack);
                        //DebugWide.LogBlue("b Integrate : 크기변경 : s_id: " + nearest_supersphere.GetID() + " p_id: " + src_pack.GetID() + " isUsed: " + nearest_supersphere.IsUsed());

                        src_pack.Unlink();
                        nearest_supersphere.AddFirst_Child(src_pack);


                        bool isCals = false;
                        isCals = nearest_supersphere.RecomputeSuperSphere_Center(_gravy_supersphere, maxRadius_supersphere);
                        //DebugWide.LogBlue("nearest_supersphere  - s_id: " + nearest_supersphere.GetID() + " ch_id: "+src_pack.GetID()+ " isCals " + isCals + "  s_r: " + nearest_supersphere.GetRadius() + "  r_lv: " + rootsphere._level);


                        //재계산에 성공하면 새로운 구를 추가할 필요가 없다 
                        if (true == isCals)
                        {
                            newsphere = false;
                        }

                    }

                }

                //조건3 - !포함될 슈퍼구가 하나도 없는 경우 , !!슈퍼구 최대크기 보다 큰 경우
                if (newsphere)
                {

                    src_pack.Unlink();
                    //DebugWide.LogBlue("Integrate : 새로운 슈퍼구 생성 : p_id: " + src_pack.GetID());
                    SphereModel superSphere = AddSphere(src_pack.GetPos(), src_pack.GetRadius() + _gravy_supersphere, rootsphere._level);
                    //rootsphere.AddChild(superSphere);
                    superSphere.AddFlag(SphereModel.Flag.SUPERSPHERE);
                    superSphere.AddFirst_Child(src_pack);
                    //superSphere.RecomputeSuperSphere(_gravy_supersphere); //반지름과 중점을 다시 계산할 필요 없으며, 링크구도 없는 상태이므로 호출할 필요 없다 
                    src_pack.Compute_BindingDistanceSquared(superSphere);


                    if(0 < superSphere._level)
                    {
                        //parent 가 level2 이라면, 생성하는 구는 level1 이어야 한다
                        //level2 => level1 , level3 => level2 ... 
                        int upLevel_idx = superSphere._level - 1;
                        //int up_flag = (int)(SphereModel.Flag.TREE_LEVEL_0) << upLevel_idx;

                        // need to create parent association!
                        SphereModel link = AddSphere(superSphere.GetPos(), superSphere.GetRadius(), upLevel_idx);
                        //_levels[upLevel_idx].AddChild(link);
                        link.SetLink_DownLevel_SuperSphere(superSphere);
                        superSphere.SetLink_UpLevel_ChildSphere(link);

                        //DebugWide.LogWhite(" s_id : " + superSphere.GetID() + "  link_id : " + link.GetID() + "  " + link.GetLevelIndex() + "  " + link.GetFlag() + "  l_s_id: " + link.GetSuperSphere().GetID());

                        //---------------------------


                        //AddRecomputeQ(link.GetSuperSphere()); //초기 생성시에 루트슈퍼구에 포함되므로 Recompute 계산에 실패하게 된다
                        link.Unlink(); //통합계산에서 unlink 할것이지만 디버그 편하게 미리 떼어낸다 
                        AddIntegrateQ(link);


                        //DebugWide.LogBlue("new link " + link.GetID() + "  " + link.GetFlag() + "  " + link.GetSuperSphere() + " - " + link._level);
                        //내부조건에서 초기에 지정된 루트슈퍼구에 포함되어 있기에 AddIntegrateQ 가 수행될 수 없다
                        //NewPosRadius 를 호출하면 안되고  AddIntegrateQ 를 직접 호출해주어야 한다 
                        //link.NewPosRadius(superSphere.GetPos(), superSphere.GetRadius());
                        //---------------------------
                    }



                }
            }//end if

            src_pack.ClearFlag(SphereModel.Flag.INTEGRATE); // we've been integrated!
        }

        //==================================================
        //첫번째 충돌체를 반환한다 (start 거리에서 가까운 것일 수도 아닐 수도 있음)
        public SphereModel RayTrace_FirstReturn(Vector3 start, Vector3 end, SphereModel exceptModel)
        {
            return _levels[0].RayTrace_FirstReturn(start, end, exceptModel);
        }

        public void RangeTest_MinDisReturn(ref ObjectManager.Param_RangeTest param)
        {
            _levels[0].RangeTest_MinDisReturn(Frustum.ViewState.PARTIAL, ref param);
        }

        //==================================================
        // debug 용 
        //==================================================

        public void Debug_RayTrace(Vector3 start, Vector3 end)
        {
            _levels[0].Debug_RayTrace(start, end);
        }

        public void Debug_RangeTest(Vector3 pos, float range)
        {

            _levels[0].Debug_RangeTest(pos, range, Frustum.ViewState.PARTIAL);
        }

        public void Debug_FrustumTest(Frustum f, Frustum.ViewState state)
        {

            _levels[0].Debug_VisibilityTest(f, state);
        }

        public void Render_Debug(bool isText)
        {
            //Render_Debug(3, isText);
            //return;

            for (int i = 0; i < _levels.Length; i++)
            {
                Render_Debug(i, isText);
            }

        }

        //ref : https://doc.instantreality.org/tools/color_calculator/
        public void Render_Debug(int treeLevel, bool isText)
        {
            //DebugWide.LogBlue(_levels.Length + "  " + treeLevel);

            //if(0 <= treeLevel && treeLevel < _levels.Length)
            //_levels[treeLevel].Debug_Render(color, isText);

            Color color = Color.white;
            switch(treeLevel)
            {
                case 0:
                    //color.r = color.r * 0.4f;
                    //color.g = color.g * 0.4f;
                    color = new Color(0.435f, 0.862f, 0.180f);
                    break;
                case 1:
                    //color.r = color.r * 0.6f;
                    //color.g = color.g * 0.6f;
                    color = new Color(0.819f, 0.862f, 0.180f);
                    break;
                case 2:
                    //color.r = color.r * 0.8f;
                    //color.g = color.g * 0.8f;
                    color = new Color(0.862f, 0.6f, 0.180f);
                    break;
                case 3:
                    //color = Color.white;
                    color = new Color(0.862f, 0.180f, 0.188f);
                    break;
            }


            if (_max_level <= treeLevel) return;

            _levels[treeLevel].Debug_Render(color, isText);

        }

    }
}


