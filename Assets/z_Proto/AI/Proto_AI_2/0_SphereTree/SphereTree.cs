using System;
using UnityEngine;


namespace Proto_AI_2
{
    
    //=================================================================

    public class SphereTree
    {
        //총 거리3 (루트 -> 자식1 -> 자식2)의 전체적인 트리를 구성한다 
        //트리의 거리별로 독립적인 트리를 구성한다. _level1 트리 한개 , _level2 트리 한개
        //_level1 의 자식구와 _level2 트리의 슈퍼구를 연결하여 외부에서는 하나의 트리인것 처럼 보이게 한다
        //private SphereModel _level_1 = null; //root : 트리의 거리(level) 1. 루트노드를 의미한다 
        //private SphereModel _level_2 = null; //leaf : 트리의 거리(level) 2. 루트노드 바로 아래의 자식노드를 의미한다   
        private SphereModel[] _levels = null;

        private Pool<SphereModel> _pool_sphere = null;

        public QFifo<SphereModel> _integrateQ = null;
        public QFifo<SphereModel> _recomputeQ = null;

        //private float _maxRadius_supersphere_root;   //루트트리 슈퍼구의 최대 반지름 크기 (gravy 을 합친 최대크기임)          
        //private float _maxRadius_supersphere_leaf;   //리프트리 슈퍼구의 최대 반지름 크기 (gravy 을 합친 최대크기임) 
        public int _max_level = 4;
        public float[] _maxRadius_supersphere = null;
        public float _gravy_supersphere;         //여분의 양. 여분은 객체들이 부모로 부터 너무 자주 떨어지지 않도록 경계구의 크기를 넉넉하게 만드는 역할을 한다
                     //자식구를 포함하는 최대 크기에서 gravy양 만큼 크게 슈퍼구를 조정한다 


        //public SphereTree(int maxspheres, float rootsize, float leafsize, float gravy)
        public SphereTree(int maxspheres, float[] list_maxRadius, float gravy)
        {
            //최대 4개까지만 만들 수 있게 한다 
            _max_level = list_maxRadius.Length;
            //DebugWide.LogBlue(_max_level);
            if (4 < _max_level) _max_level = 4; 
            _levels = new SphereModel[_max_level];
            _maxRadius_supersphere = new float[_max_level];
            for (int i = 0; i < _max_level;i++)
            {
                _maxRadius_supersphere[i] = list_maxRadius[i];
            }
            _gravy_supersphere = gravy;

            //메모리풀 크기를 4배 하는 이유 : 각각의 레벨트리는 자식구에 대해 1개의 슈퍼구를 각각 만든다. 레벨트리 1개당 최대개수 *2 의 크기를 가져야 한다. 
            //레벨트리가 2개 이므로 *2*2 가 된다.
            //구의 최대개수가 5일때의 최대 메모리 사용량 : 레벨2트리 <루트1개 + 구5개 + 슈퍼구5개> , 레벨1트리 <루트1개 + 슈퍼구5개 + 복제된슈퍼구5개>
            maxspheres = (maxspheres * 2 * _max_level) + _max_level;

            //_maxRadius_supersphere_root = rootsize;
            //_maxRadius_supersphere_leaf = leafsize;
            //_gravy_supersphere = gravy;

            _integrateQ = new QFifo<SphereModel>(maxspheres);
            _recomputeQ = new QFifo<SphereModel>(maxspheres);
            _pool_sphere = new Pool<SphereModel>();
            _pool_sphere.Init(maxspheres);       // init pool to hold all possible SpherePack instances.

            //Vector3 pos = Vector3.zero;
            //_level_1 = _spheres.GetFreeLink(); // initially empty
            //_level_1.Init(this, pos, 65536);
            //_level_1.AddFlag(SphereModel.Flag.SUPERSPHERE | SphereModel.Flag.ROOTNODE | SphereModel.Flag.TREE_LEVEL_1);
            ////
            //_level_2 = _spheres.GetFreeLink();  // initially empty
            //_level_2.Init(this, pos, 16384);
            //_level_2.AddFlag(SphereModel.Flag.SUPERSPHERE | SphereModel.Flag.ROOTNODE | SphereModel.Flag.TREE_LEVEL_2);


            for (int i = 0; i < _max_level;i++)
            {
                int level_flag = (int)(SphereModel.Flag.TREE_LEVEL_0) << i;

                _levels[i] = _pool_sphere.GetFreeLink(); // initially empty
                _levels[i].Init(this, Vector3.zero, 65536);
                _levels[i].AddFlag(SphereModel.Flag.SUPERSPHERE | SphereModel.Flag.ROOTNODE | (SphereModel.Flag)level_flag);

            }

        }



        public SphereModel AddSphere(Vector3 pos, float radius, SphereModel.Flag flags)
        {

            SphereModel pack = _pool_sphere.GetFreeLink();
            //DebugWide.LogBlue(_pool_sphere.GetFreeCount() + "  " + _pool_sphere.GetUsedCount());
            if (null == pack)
            {
                DebugWide.LogError("AddSphere : GetFreeLink() is Null !!");
                return null;
            }

            pack.Init(this, pos, radius); //AddSpherePackFlag 함수 보다 먼저 호출되어야 한다. _flags 정보가 초기화 되기 때문이다. 

            //CREATE_LEVEL_LAST 요청이 들어올 경우 마지막 레벨트리의 인덱스를 찾아 넣어준다 
            if(0 != (flags & SphereModel.Flag.CREATE_LEVEL_LAST))
            {
                int last_level_idx = _levels.Length - 1;
                flags = (SphereModel.Flag)((int)(SphereModel.Flag.TREE_LEVEL_0) << last_level_idx);
                //DebugWide.LogBlue(flags);
            }

            if (0 == (flags & SphereModel.Flag.TREE_LEVEL_0123))
            {
                DebugWide.LogError("AddSphere : TREE_LEVEL is None !!  " + flags);
                return null;
            }
                
            pack.AddFlag(flags & SphereModel.Flag.TREE_LEVEL_0123); //level 1~4 flag 만 통과시킨다 

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
            QFifo<SphereModel>.Out_Point fifo = _integrateQ.Push(pack); //추가되기전 마지막 큐값을 반환 한다 
            pack.SetIntergrate_FifoOut(fifo);
        }

        //!!자식이 있는 구, 즉 슈퍼구만 재계산의 대상이 된다. 
        //슈퍼구의 위치,반지름이 자식들의 정보에 따라 재계산 된다.
        public void AddRecomputeQ(SphereModel superSphere)     // add to the recomputation (balancing) FIFO.
        {
            if (false == superSphere.HasFlag(SphereModel.Flag.RECOMPUTE))
            {
                if (0 != superSphere.GetChildCount())
                {
                    //--------------------->
                    //bool contain = false;
                    //if (_recomputeQ.Contain(superSphere))
                    //{
                    //    DebugWide.LogGreen("AddRecomputeQ add : " + superSphere.GetID() + "  lv: " + superSphere.GetLevelIndex() + "  ");
                    //    DebugWide.LogGreen("1 Q list : " + ToStringQ(_recomputeQ));
                    //    contain = true;
                    //}
                    //--------------------->

                    superSphere.AddFlag(SphereModel.Flag.RECOMPUTE); // needs to be recalculated!
                    QFifo<SphereModel>.Out_Point fifo = _recomputeQ.Push(superSphere);
                    superSphere.SetRecompute_FifoOut(fifo);

                    //--------------------->
                    //if (contain)
                    //{
                    //    DebugWide.LogGreen("2 Q list : " + ToStringQ(_recomputeQ));
                    //}
                    //--------------------->
                }
                else
                {
                    //DebugWide.LogWhite("AddRecomputeQ Remove : " + superSphere.GetID());
                    //Remove_SuperSphereAndLinkSphere(superSphere);
                    superSphere.Unlink_SuperSphereAndLinkSphere();
                }
            }
        }

        //슈퍼구와 그리고 슈퍼구와 연결된상위 자식구를 지운다 
        //public void Remove_SuperSphereAndLinkSphere(SphereModel pack)
        //{
        //    if (null == pack) return;
        //    if (pack.HasFlag(SphereModel.Flag.ROOTNODE)) return; // CAN NEVER REMOVE THE ROOT NODE EVER!!!

        //    string temp = "";
        //    if (pack.HasFlag(SphereModel.Flag.SUPERSPHERE))
        //    {
        //        SphereModel link = pack.GetLink_UpLevel_ChildSphere();

        //        Remove_SuperSphereAndLinkSphere(link);

        //        if (null != link)
        //            temp = "--> link_id : " + link.GetID();
        //        else
        //            temp = "--> link_id : null ";
        //    }

        //    pack.Unlink();

        //    DebugWide.LogGreen(temp+"  Remove_SuperSphereAndLinkSphere !!--  s_id: " + pack.GetID() + "  flag: " + pack.GetFlag().ToString() + " ct: " +pack.GetChildCount());
        //    //DebugWide.LogGreen(temp+" Q list : " + ToStringQ(_recomputeQ));
        //    _pool_sphere.Release(pack);
        //}

        public void ReleasePool(SphereModel pack)
        {
            _pool_sphere.Release(pack);
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

        public string ToStringQ(QFifo<SphereModel> Q)
        {
            string temp = "size: " + Q.mFifoSize +  " ct : "+ Q.mCount + " head : " + Q._head + " tail : " + Q._tail + "  list : ";
            int head = Q._head;
            while (Q._tail != head) //데이터가 있다면 
            {

                SphereModel ret = Q.mFifo[head];
                head++;
                if (head == Q.mFifoSize) head = 0;
                if (null != ret)
                {
                    temp += " " + ret.GetID() + "(" + ret.IsUsed() + ")  ";
                }else
                {
                    temp += "  null  "; 
                }
            }
            return temp;
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
                //DebugWide.LogBlue("<<<<<< Process _recomputeQ : " + ToStringQ(_recomputeQ));
                int maxrecompute = _recomputeQ.GetCount();
                for (int i = 0; i < maxrecompute; i++)
                {
                    SphereModel superSphere = _recomputeQ.Pop();
                    if (null == superSphere) continue;

                    //DebugWide.LogBlue(" - - - - - - - -pop after Q list: " + ToStringQ(_recomputeQ));

                    superSphere.InitRecompute_FifoOut(); //큐 연결정보를 초기화 한다 
                    //if(false == superSphere.IsUsed())
                    //{
                    //    DebugWide.LogRed("---------- "+superSphere.GetLevelIndex() +  "  " +superSphere.GetID() + "  " + superSphere.HasFlag(SphereModel.Flag.SUPERSPHERE));
                    //    //DebugWide.LogRed("Q list : " + ToStringQ(_recomputeQ));
                    //}
                    bool isRemove = superSphere.RecomputeSuperSphere(_gravy_supersphere);
                    if (isRemove)
                    {
                        //DebugWide.LogWhite("Process recomputeQ Remove : " + superSphere.GetID());
                        //Remove_SuperSphereAndLinkSphere(superSphere);
                        superSphere.Unlink_SuperSphereAndLinkSphere();
                    }

                }
            }

            //자식구 통합
            if (true)
            {
                //DebugWide.LogBlue("<<<<<< Process _integrateQ : " + ToStringQ(_integrateQ));
                // Now, process the integration step.
                int maxintegrate = _integrateQ.GetCount();
                for (int i = 0; i < maxintegrate; i++)
                {
                    SphereModel childSphere = _integrateQ.Pop();
                    if (null == childSphere) continue; //null 데이터는 처리할 수 없다 

                    childSphere.InitIntergrate_FifoOut(); //큐 연결정보를 초기화 한다 


                    int level_idx = childSphere.GetLevelIndex();
                    Integrate(childSphere, _levels[level_idx], _maxRadius_supersphere[level_idx]); 
                }
            }

        }

        //src_pack에는 자식구만 들어가야 한다 
        public void Integrate(SphereModel src_pack, SphereModel rootsphere, float maxRadius_supersphere)
        {

            SphereModel search = rootsphere.GetChildren();

            SphereModel containing_supersphere = null;  //src_pack를 포함하는 슈퍼구 
            float includedSqrDist = 1e9f;     // enclosed within. 10의9승. 1000000000.0

            SphereModel nearest_supersphere = null; //src_pack와 가까이에 있는 슈퍼구
            float nearDist = 1e9f;    // add ourselves to.


            // 1 **** src 와 가장 가까운 슈퍼구 구하기 : src를 포함하는 슈퍼구를 먼저 구함. 없으면 src와 가장 가까운 슈퍼구를 구한다.
            //=====================================================================================
            //DebugWide.LogBlue("root id: "+rootsphere.GetID() + "  src id: " + src_pack.GetID()+ "  ct: " +rootsphere.GetChildCount() + "  lv: " + rootsphere.GetLevelIndex());
            //while (null != search)
            for (int i = 0; i < rootsphere.GetChildCount(); i++)
            {
                if (null == search)
                {
                    DebugWide.LogRed("Integrate --a-- i: " + i + "  root id: " + rootsphere.GetID() + "  ct: " + rootsphere.GetChildCount()); //test
                    break;
                }
                else if(search == search.GetNextSibling())
                {
                    DebugWide.LogRed("Integrate --b-- i: " + i + "  root id: "+ rootsphere.GetID() + "  ct: " + rootsphere.GetChildCount() + "  " + search.HasFlag(SphereModel.Flag.SUPERSPHERE)); //test
                    break; 
                }

                if (search.HasFlag(SphereModel.Flag.SUPERSPHERE) &&
                    false == search.HasFlag(SphereModel.Flag.ROOTNODE) && 0 != search.GetChildCount())
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
                //src_pack.Unlink(); //큐 연결정보를 Process 에서 해제 했기 때문에, 내부에서 LostChild만 수행된다 
                //DebugWide.LogBlue("a Integrate : 완전포함 : s_id: " + containing_supersphere.GetID()+" p_id: " + src_pack.GetID() + " isUsed: " + containing_supersphere.IsUsed());
                //containing_supersphere.AddChild(src_pack); //src_pack 의 트리정보를 설정
                //DebugWide.LogBlue("b Integrate : 완전포함 : s_id: " + containing_supersphere.GetID() + " p_id: " + src_pack.GetID() + " isUsed: " + containing_supersphere.IsUsed());

                //자식구가 1개 일때 링크구까지 지우므로 슈퍼구가 다를때만 처리한다 
                if (containing_supersphere != src_pack.GetSuperSphere())
                {
                    containing_supersphere.AddChild(src_pack);
                }

                containing_supersphere.RecomputeSuperSphere(_gravy_supersphere);
                src_pack.Compute_BindingDistanceSquared(containing_supersphere); //RecomputeSuperSphere 계산실패 할 경우가 있기 때문에 수행해준다

                //---------------------------
                SphereModel link = containing_supersphere.GetLink_UpLevel_ChildSphere();
                if(null != link)
                {
                    int upLevel_idx = containing_supersphere.GetLevelIndex() - 1;
                    if(0 <= upLevel_idx)
                    {
                        Integrate(link, _levels[upLevel_idx], _maxRadius_supersphere[upLevel_idx]);
                    }
                }
                //---------------------------


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
                        //DebugWide.LogBlue(" " + src_pack.GetSuperSphere().GetID());
                        //src_pack.Unlink();
                        //DebugWide.LogBlue("a Integrate : 크기변경 : s_id: " + nearest_supersphere.GetID() + " p_id: " + src_pack.GetID() + " isUsed: " + nearest_supersphere.IsUsed());
                        //nearest_supersphere.AddChild(src_pack);
                        //DebugWide.LogBlue("b Integrate : 크기변경 : s_id: " + nearest_supersphere.GetID() + " p_id: " + src_pack.GetID() + " isUsed: " + nearest_supersphere.IsUsed());

                        //자식구가 1개 일때 링크구까지 지우므로 슈퍼구가 다를때만 처리한다 
                        if(nearest_supersphere != src_pack.GetSuperSphere())
                        {
                            nearest_supersphere.AddChild(src_pack);
                        }

                        nearest_supersphere.SetRadius(newRadius); //uplevel 크기도 함께 갱신된다 
                        nearest_supersphere.RecomputeSuperSphere(_gravy_supersphere);
                        src_pack.Compute_BindingDistanceSquared(nearest_supersphere);

                        //---------------------------
                        SphereModel link = nearest_supersphere.GetLink_UpLevel_ChildSphere();
                        if (null != link)
                        {
                            int upLevel_idx = nearest_supersphere.GetLevelIndex() - 1;
                            if (0 <= upLevel_idx)
                            {
                                Integrate(link, _levels[upLevel_idx], _maxRadius_supersphere[upLevel_idx]);
                            }
                        }
                        //---------------------------

                        newsphere = false;

                    }

                }

                //조건3 - !포함될 슈퍼구가 하나도 없는 경우 , !!슈퍼구 최대크기 보다 큰 경우
                if (newsphere)
                {
                    //src_pack.Unlink();
                    //DebugWide.LogBlue("Integrate : 새로운 슈퍼구 생성 : p_id: " + src_pack.GetID());
                    SphereModel superSphere = AddSphere(src_pack.GetPos(), src_pack.GetRadius() + _gravy_supersphere, rootsphere.GetFlag());
                    //superSphere.ClearFlag(SphereModel.Flag.ROOTNODE); //루트노드 설정이 있으면 지울수 없는 슈퍼구가 된다 , AddSphere 에서 ROOTNODE 플래그는 걸러진다 
                    superSphere.AddFlag(SphereModel.Flag.SUPERSPHERE);
                    superSphere.AddChild(src_pack);

                    //DebugWide.LogYellow(superSphere.GetID());
                    rootsphere.AddChild(superSphere);

                    superSphere.RecomputeSuperSphere(_gravy_supersphere);
                    src_pack.Compute_BindingDistanceSquared(superSphere);

                    if (false == superSphere.HasFlag(SphereModel.Flag.TREE_LEVEL_0))
                    {
                        //parent 가 level2 이라면, 생성하는 구는 level1 이어야 한다
                        //level2 => level1 , level3 => level2 ... 
                        int upLevel_idx = superSphere.GetLevelIndex() - 1;
                        int up_flag = (int)(SphereModel.Flag.TREE_LEVEL_0) << upLevel_idx;

                        // need to create parent association!
                        SphereModel link = AddSphere(superSphere.GetPos(), superSphere.GetRadius(), (SphereModel.Flag)up_flag);

                        //AddIntegrateQ(link);
                        _levels[upLevel_idx].AddChild(link);
                        link.SetLink_DownLevel_SuperSphere(superSphere);
                        superSphere.SetLink_UpLevel_ChildSphere(link);

                        //DebugWide.LogWhite(" s_id : " + superSphere.GetID() + "  link_id : " + link.GetID() + "  " + link.GetLevelIndex() + "  " + link.GetFlag() + "  l_s_id: " + link.GetSuperSphere().GetID());

                        //---------------------------
                        if (0 <= upLevel_idx)
                        {
                            Integrate(link, _levels[upLevel_idx], _maxRadius_supersphere[upLevel_idx]);
                        }
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
            _levels[treeLevel].Debug_Render(color, isText);

        }

    }
}


