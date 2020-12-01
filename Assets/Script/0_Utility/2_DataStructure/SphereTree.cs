using System;
using UnityEngine;


namespace UtilGS9
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

        private QFifo<SphereModel> _integrateQ = null;
        private QFifo<SphereModel> _recomputeQ = null;

        //private float _maxRadius_supersphere_root;   //루트트리 슈퍼구의 최대 반지름 크기 (gravy 을 합친 최대크기임)          
        //private float _maxRadius_supersphere_leaf;   //리프트리 슈퍼구의 최대 반지름 크기 (gravy 을 합친 최대크기임) 
        public int _max_level = 4;
        public float[] _maxRadius_supersphere = null;
        public float _gravy_supersphere;         //여분의 양. 여분은 객체들이 부모로 부터 너무 자주 떨어지지 않도록 경계구의 크기를 넉넉하게 만드는 역할을 한다



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
                int level_flag = (int)(SphereModel.Flag.TREE_LEVEL_1) << i;

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
                flags = (SphereModel.Flag)((int)(SphereModel.Flag.TREE_LEVEL_1) << last_level_idx);
                //DebugWide.LogBlue(flags);
            }

            if (0 == (flags & SphereModel.Flag.TREE_LEVEL_1234))
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
            QFifo<SphereModel>.Out_Point fifo = _integrateQ.Push(pack); //추가되기전 마지막 큐값을 반환 한다 
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

                            float dist = (float)Math.Sqrt(sqrDist) + src_pack.GetRadius();

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
            }
            //=====================================================================================

            //조건1 - src구가 완전 포함 
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

                    if (false == parent.HasFlag(SphereModel.Flag.TREE_LEVEL_1))
                    {
                        //parent 가 level2 이라면, 생성하는 구는 level1 이어야 한다
                        //level2 => level1 , level3 => level2 ... 
                        int up_level_idx = parent.GetLevelIndex() - 1;
                        int up_flag = (int)(SphereModel.Flag.TREE_LEVEL_1) << up_level_idx;

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

        public void RangeTest_MinDisReturn(ref HordeFight.ObjectManager.Param_RangeTest param)
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
            
            for (int i = 0; i < _levels.Length; i++)
            {
                Render_Debug(i, isText);
            }

        }

        public void Render_Debug(int treeLevel, bool isText)
        {
            //if(0 <= treeLevel && treeLevel < _levels.Length)
            //_levels[treeLevel].Debug_Render(color, isText);

            Color color = Color.white;
            switch(treeLevel)
            {
                case 0:
                    color.r = color.r * 0.4f;
                    color.g = color.g * 0.4f;
                    break;
                case 1:
                    color.r = color.r * 0.6f;
                    color.g = color.g * 0.6f;
                    break;
                case 2:
                    color.r = color.r * 0.8f;
                    color.g = color.g * 0.8f;
                    break;
                case 3:
                    color = Color.white;
                    break;
            }
            _levels[treeLevel].Debug_Render(color, isText);

        }

    }
}


