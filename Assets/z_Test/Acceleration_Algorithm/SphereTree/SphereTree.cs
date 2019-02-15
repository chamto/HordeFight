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

    //======================================================
    //                   메모리풀 전용 변수
    //======================================================
    private SphereModel _pool_next = null; 
    private SphereModel _pool_prev = null; 

    //======================================================
    //                    트리 링크 변수
    //======================================================
    private SphereModel _parent = null; //트리 깊이 위로
    private SphereModel _children = null;  //트리 깊이 아래로 
    private SphereModel _sibling_next = null; //트리 같은 차수 왼쪽으로
    private SphereModel _sibling_prev = null; //트리 같은 차수 오른쪽으로 


    private Flag _flags; 
    private QFifo<SphereModel>.Out_Point _recompute_fifoOut; 
    private QFifo<SphereModel>.Out_Point _intergrate_fifoOut; 

    private SphereModel _data_rootTree = null;

    //======================================================
    private int _childCount = 0;
    private float _bindingDistance = 0f;

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
    public void Init(Vector3 pos, float radius)
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
        _bindingDistance = 0f;
        _data_rootTree = null;

    }


    public void SetPos(Vector3 pos)
    {
        _center = pos; 

        if (null != _parent && false == HasSpherePackFlag(Flag.INTEGRATE))
        {

            float sqrDist = (_center - _parent._center).sqrMagnitude;

            if (sqrDist >= _bindingDistance) 
            {

                if (false == _parent.HasSpherePackFlag(Flag.RECOMPUTE))
                {
                    
                    //mFactory.AddRecompute(mParent); //todo fixme
                }

                //Unlink(); //todo fixme
                //mFactory.AddIntegrate(this); //todo fixme
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
                //ComputeBindingDistance(_parent); //todo fixme
            }

            float sqrDist = (_center - _parent._center).sqrMagnitude;

            if (sqrDist >= _bindingDistance)
            {
                if (false == _parent.HasSpherePackFlag(Flag.RECOMPUTE)) 
                {
                    //mFactory.AddRecompute(mParent);   //todo fixme 
                }
                //Unlink(); //todo fixme
                //mFactory.AddIntegrate(this); //todo fixme
            }
            else
            {
                if (false == _parent.HasSpherePackFlag(Flag.RECOMPUTE)) 
                {
                    //mFactory.AddRecompute(mParent);  //todo fixme   
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

        //#ifdef _DEBUG  //////////////////////////////////////////////
        //에러검출 테스트 : 테스트 필요 
        float dist = ToDistanceSquared(pack);
        float radius = Mathf.Sqrt(dist) + pack.GetRadius();
        if(radius > GetRadius())
        {
            DebugWide.LogError("newRadius > GetRadius()"); //assert
        }
        //#endif //////////////////////////////////////////////
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
        if (null == _children || 0 == _childCount) DebugWide.LogError("null == _children || 0 == _childCount"); //assert

        //#ifdef _DEBUG  //////////////////////////////////////////////
        SphereModel pack = _children;
        bool found = false;
        while (null != pack)
        {
            if (pack == model)
            {
                if (true == found) DebugWide.LogError("true == found"); //assert
                found = true;
            }
            pack = pack.GetNextSibling();
        }
        if (false == found) DebugWide.LogError("false == found"); //assert
        //#endif //////////////////////////////////////////////

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
            //mFactory.Remove(this); //todo fixme
        }
    }


}

public class SphereTree
{
    private SphereModel _root = null;     
    private SphereModel _leaf = null;     

    private Pool<SphereModel> _spheres = null;  

    private QFifo<SphereModel> _integrate = null; 
    private QFifo<SphereModel> _recompute = null; 

    private float _maxRootSize;              
    private float _maxLeafSize;              
    private float _superSphereGravy;         //여분의 양. 여분은 객체들이 부모로 부터 너무 자주 떨어지지 않도록 경계구의 크기를 넉넉하게 만드는 역할을 한다



    public SphereTree(int maxspheres, float rootsize, float leafsize, float gravy)
    {
        maxspheres *= 4; // include room for both trees, the root node and leaf node tree, and the supersheres
        _maxRootSize = rootsize;
        _maxLeafSize = leafsize;
        _superSphereGravy = gravy;

        _integrate = new QFifo<SphereModel>(maxspheres);
        _recompute = new QFifo<SphereModel>(maxspheres);
        _spheres = new Pool<SphereModel>();
        _spheres.Init(maxspheres);       // init pool to hold all possible SpherePack instances.

        Vector3 pos = Vector3.zero;

        _root = _spheres.GetFreeLink(); // initially empty
        _root.Init(pos, 65536);
        _root.AddSpherePackFlag(SphereModel.Flag.SUPERSPHERE | SphereModel.Flag.ROOTNODE | SphereModel.Flag.ROOT_TREE);


        _leaf = _spheres.GetFreeLink();  // initially empty
        _leaf.Init(pos, 16384);
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

        pack.Init(pos, radius); //AddSpherePackFlag 함수 보다 먼저 호출되어야 한다. _flags 정보가 초기화 되기 때문이다. 

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
        QFifo<SphereModel>.Out_Point fifo = _integrate.Push(pack); // add it to the integration stack.
        pack.SetIntergrate_FifoOut(fifo);
    }

    public void AddRecomputeQ(SphereModel pack)     // add to the recomputation (balancing) FIFO.
    {
        if (false == pack.HasSpherePackFlag(SphereModel.Flag.RECOMPUTE))
        {
            if (0 != pack.GetChildCount())
            {
                pack.AddSpherePackFlag(SphereModel.Flag.RECOMPUTE); // needs to be recalculated!
                QFifo<SphereModel>.Out_Point fifo = _recompute.Push(pack);
                pack.SetRecompute_FifoOut(fifo);
            }
            else
            {
                Remove_SuperSphereOfLeafTree(pack);
            }
        }
    }

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
}

