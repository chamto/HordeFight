using System;
using UnityEngine;
using UtilGS9;



public class SphereModel : IPoolConnector<SphereModel>
{
    [FlagsAttribute]
    public enum Flag
    {
        NONE = 0,

        SUPERSPHERE = (1 << 0), // this is a supersphere, allocated and deleted by us
        ROOT_TREE = (1 << 1), // member of the root tree
        LEAF_TREE = (1 << 2), // member of the leaf node tree
        ROOTNODE = (1 << 3), // this is the root node
        RECOMPUTE = (1 << 4), // needs recomputed bounding sphere
        INTEGRATE = (1 << 5), // needs to be reintegrated into tree
                              // Frame-to-frame view frustum status.  Only does callbacks when a
                              // state change occurs.
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

    private SphereModel _data_rootTree = null;


    //------------------------------------------------------

    private int _childCount = 0;
    private float _binding_distance_sqr = 0f;

    private SphereTree _treeController = null;

    //______________________________________________________________________________________________________

    //=====================================================
    //interface 구현 
    //=====================================================
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
    public float ToDistanceSquared(SphereModel pack) { return (_center - pack._center).sqrMagnitude; }

    //=====================================================
    //Flag 열거값 다루는 함수
    //=====================================================
    public void AddSpherePackFlag(Flag flag) { _flags |= flag; }
    public void ClearSpherePackFlag(Flag flag) { _flags &= ~flag; }
    public bool HasSpherePackFlag(Flag flag)
    {
        if (0 != (_flags & flag)) return true;
        return false;
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
    public void InitRecompute_FifoOut(){_recompute_fifoOut.Init(); }
    public void InitIntergrate_FifoOut() { _intergrate_fifoOut.Init(); }

    public void SetData_RootTree(SphereModel data) { _data_rootTree = data; }
    public SphereModel GetData_RootTree() { return _data_rootTree; }

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
        _data_rootTree = null;

        _treeController = controller;
    }


    public void SetPos(Vector3 pos)
    {
        _center = pos; 

        if (null != _parent && false == HasSpherePackFlag(Flag.INTEGRATE))
        {
            float sqrDist = ToDistanceSquared(_parent);

            if (sqrDist >= _binding_distance_sqr) 
            {

                if (false == _parent.HasSpherePackFlag(Flag.RECOMPUTE))
                {
                    _treeController.AddRecomputeQ(_parent); 
                }

                Unlink();
                _treeController.AddIntegrateQ(this); 
            }
        }

    }


    public void SetPosRadius(Vector3 pos, float radius)
    {
        
        _center = pos;

        if (null != _parent && false == HasSpherePackFlag(Flag.INTEGRATE))
        {
            
            if (Mathf.Epsilon < Mathf.Abs(radius - _radius))
            {
                SetRadius(radius);
                Compute_BindingDistanceSquared(_parent);
            }

            float sqrDist = ToDistanceSquared(_parent);

            if (sqrDist >= _binding_distance_sqr)
            {
                if (false == _parent.HasSpherePackFlag(Flag.RECOMPUTE)) 
                {
                    _treeController.AddRecomputeQ(_parent);
                }
                Unlink();
                _treeController.AddIntegrateQ(this);
            }
            else
            {
                if (false == _parent.HasSpherePackFlag(Flag.RECOMPUTE)) 
                {
                    _treeController.AddRecomputeQ(_parent);
                }
            }
        }
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

        //#ifdef _DEBUG  //____________________________________________________________
        //에러검출 테스트 : 테스트 필요 
        float dist = ToDistanceSquared(pack);
        float radius = Mathf.Sqrt(dist) + pack.GetRadius();
        if(radius > GetRadius())
        {
            DebugWide.LogError("newRadius > GetRadius()"); //assert
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


        if (null == _children) DebugWide.LogError("null == _children"); //assert

        _parent = null; 
    }

    public void LostChild(SphereModel model)
    {
        //#ifdef _DEBUG  //____________________________________________________________
        if (null == _children || 0 == _childCount) DebugWide.LogError("null == _children || 0 == _childCount"); //assert

        SphereModel pack = _children;
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
        if (0 == _childCount && HasSpherePackFlag(Flag.SUPERSPHERE))
        {
            _treeController.Remove_SuperSphereOfLeafTree(this); 
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

    public bool Recompute(float gravy)
    {
        if (null == _children) return true; // kill it!
        if (HasSpherePackFlag(Flag.ROOTNODE)) return false; // don't recompute root nodes!

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
                        ClearSpherePackFlag(Flag.RECOMPUTE);
                        return false;
                    }
                }
                pack = pack.GetNextSibling();
            }

            maxradius += gravy;

            SetRadius(maxradius);

            // now all children have to recompute binding distance!!
            pack = _children;

            while (null != pack)
            {
                pack.Compute_BindingDistanceSquared(this);
                pack = pack.GetNextSibling();
            }

        }

        //#endif

        ClearSpherePackFlag(Flag.RECOMPUTE);

        return false;
    }


}

public class SphereTree
{
    private SphereModel _root = null;     
    private SphereModel _leaf = null;     

    private Pool<SphereModel> _spheres = null;  

    private QFifo<SphereModel> _integrateQ = null; 
    private QFifo<SphereModel> _recomputeQ = null; 

    private float _maxRadius_supersphere_root;   //루트트리 슈퍼구의 최대 반지름 크기 (gravy 을 합친 최대크기임)          
    private float _maxRadius_supersphere_leaf;   //리프트리 슈퍼구의 최대 반지름 크기 (gravy 을 합친 최대크기임)             
    private float _superSphereGravy;         //여분의 양. 여분은 객체들이 부모로 부터 너무 자주 떨어지지 않도록 경계구의 크기를 넉넉하게 만드는 역할을 한다



    public SphereTree(int maxspheres, float rootsize, float leafsize, float gravy)
    {
        maxspheres *= 4; // include room for both trees, the root node and leaf node tree, and the supersheres
        _maxRadius_supersphere_root = rootsize;
        _maxRadius_supersphere_leaf = leafsize;
        _superSphereGravy = gravy;

        _integrateQ = new QFifo<SphereModel>(maxspheres);
        _recomputeQ = new QFifo<SphereModel>(maxspheres);
        _spheres = new Pool<SphereModel>();
        _spheres.Init(maxspheres);       // init pool to hold all possible SpherePack instances.

        Vector3 pos = Vector3.zero;

        _root = _spheres.GetFreeLink(); // initially empty
        _root.Init(this, pos, 65536);
        _root.AddSpherePackFlag(SphereModel.Flag.SUPERSPHERE | SphereModel.Flag.ROOTNODE | SphereModel.Flag.ROOT_TREE);


        _leaf = _spheres.GetFreeLink();  // initially empty
        _leaf.Init(this, pos, 16384);
        _leaf.AddSpherePackFlag(SphereModel.Flag.SUPERSPHERE | SphereModel.Flag.ROOTNODE | SphereModel.Flag.LEAF_TREE);

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

        if (SphereModel.Flag.NONE != (flags & SphereModel.Flag.ROOT_TREE)) //루트트리가 들어 있다면 
        {
            pack.AddSpherePackFlag(SphereModel.Flag.ROOT_TREE);
        }
        else
        {
            pack.AddSpherePackFlag(SphereModel.Flag.LEAF_TREE);
        }

        AddIntegrateQ(pack); // add to integration list.


        return pack;
    }


    public void AddIntegrateQ(SphereModel pack)      
    {

        if (pack.HasSpherePackFlag(SphereModel.Flag.ROOT_TREE))
            _root.AddChild(pack);
        else
            _leaf.AddChild(pack);

        pack.AddSpherePackFlag(SphereModel.Flag.INTEGRATE); // still needs to be integrated!
        QFifo<SphereModel>.Out_Point fifo = _integrateQ.Push(pack); // add it to the integration stack.
        pack.SetIntergrate_FifoOut(fifo);
    }

    public void AddRecomputeQ(SphereModel pack)     // add to the recomputation (balancing) FIFO.
    {
        if (false == pack.HasSpherePackFlag(SphereModel.Flag.RECOMPUTE))
        {
            if (0 != pack.GetChildCount())
            {
                pack.AddSpherePackFlag(SphereModel.Flag.RECOMPUTE); // needs to be recalculated!
                QFifo<SphereModel>.Out_Point fifo = _recomputeQ.Push(pack);
                pack.SetRecompute_FifoOut(fifo);
            }
            else
            {
                Remove_SuperSphereOfLeafTree(pack);
            }
        }
    }

    //LeafTree 의 슈퍼구만 지운다 
    public void Remove_SuperSphereOfLeafTree(SphereModel pack)
    {

        if (pack.HasSpherePackFlag(SphereModel.Flag.ROOTNODE)) return; // CAN NEVER REMOVE THE ROOT NODE EVER!!!

        if (pack.HasSpherePackFlag(SphereModel.Flag.SUPERSPHERE) && pack.HasSpherePackFlag(SphereModel.Flag.LEAF_TREE))
        {

            SphereModel link = pack.GetData_RootTree();

            Remove_SuperSphereOfLeafTree(link);
        }

        pack.Unlink();

        _spheres.Release(pack);
    }

    public void Process()
    {
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

                bool isRemove = pack.Recompute(_superSphereGravy); //fixme
                if (isRemove) Remove_SuperSphereOfLeafTree(pack); //fixme
            }
        }

        if (true)
        {
            // Now, process the integration step.

            int maxintegrate = _integrateQ.GetCount();

            for (int i = 0; i < maxintegrate; i++)
            {
                SphereModel pack = _integrateQ.Pop();
                if (null == pack) break; //큐가 비어있을 때만 null을 반환한다. Unlink 에 의해 데이터가 null인 항목은 반환되지 않는다  

                pack.InitIntergrate_FifoOut(); //큐 연결정보를 초기화 한다 

                if (pack.HasSpherePackFlag(SphereModel.Flag.ROOT_TREE))
                    Integrate(pack, _root, _maxRadius_supersphere_root); // integrate this one single dude against the root node.
                else
                    Integrate(pack, _leaf, _maxRadius_supersphere_leaf); // integrate this one single dude against the root node.
            }
        }

    }

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
            if (search.HasSpherePackFlag(SphereModel.Flag.SUPERSPHERE) &&
                false == search.HasSpherePackFlag(SphereModel.Flag.ROOTNODE) && 0 != search.GetChildCount())
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
            containing_supersphere.Recompute(_superSphereGravy);

            if (containing_supersphere.HasSpherePackFlag(SphereModel.Flag.LEAF_TREE))
            {
                //슈퍼구 전용 함수 : GetData_RootTree
                SphereModel link = containing_supersphere.GetData_RootTree();
                link.SetPosRadius(containing_supersphere.GetPos(), containing_supersphere.GetRadius());
            }

        }
        //조건2 - 슈퍼구에 걸쳐 있거나 포함되지 않음
        else
        {
            bool newsphere = true;

            //가까운 거리에 슈퍼구가 있다
            if (null != nearest_supersphere)
            {
                float newRadius = nearDist + nearest_supersphere.GetRadius() + _superSphereGravy;

                //!!슈퍼구 최대크기 보다 작을 경우, 포함할 수 있는 크기로 변경한다 
                if (newRadius <= maxRadius_supersphere)
                {
                    src_pack.Unlink();

                    nearest_supersphere.SetRadius(newRadius);
                    nearest_supersphere.AddChild(src_pack);
                    nearest_supersphere.Recompute(_superSphereGravy);
                    src_pack.Compute_BindingDistanceSquared(nearest_supersphere);

                    if (nearest_supersphere.HasSpherePackFlag(SphereModel.Flag.LEAF_TREE))
                    {
                        SphereModel link = nearest_supersphere.GetData_RootTree();
                        link.SetPosRadius(nearest_supersphere.GetPos(), nearest_supersphere.GetRadius());
                    }

                    newsphere = false;

                }

            }

            //조건3 - 포함될 슈퍼구가 하나도 없는 경우 , !!슈퍼구 최대크기 보다 큰 경우
            if (newsphere)
            {
                //assert(supersphere->HasSpherePackFlag(SPF_ROOTNODE));
                // we are going to create a new superesphere around this guy!
                src_pack.Unlink();

                SphereModel parent = _spheres.GetFreeLink();
                //assert(parent);
                parent.Init(this, src_pack.GetPos(), src_pack.GetRadius() + _superSphereGravy);

                if (supersphere.HasSpherePackFlag(SphereModel.Flag.ROOT_TREE))
                    parent.AddSpherePackFlag(SphereModel.Flag.ROOT_TREE);
                else
                    parent.AddSpherePackFlag(SphereModel.Flag.LEAF_TREE);

                parent.AddSpherePackFlag(SphereModel.Flag.SUPERSPHERE);


                parent.AddChild(src_pack);

                supersphere.AddChild(parent);

                parent.Recompute(_superSphereGravy);
                src_pack.Compute_BindingDistanceSquared(parent);

                if (parent.HasSpherePackFlag(SphereModel.Flag.LEAF_TREE))
                {
                    // need to create parent association!
                    SphereModel link = AddSphere(parent.GetPos(), parent.GetRadius(), SphereModel.Flag.ROOT_TREE);
                    link.SetData_RootTree(parent);
                    parent.SetData_RootTree(link); // hook him up!!
                }

            }
        }

        src_pack.ClearSpherePackFlag(SphereModel.Flag.INTEGRATE); // we've been integrated!
    }
}

