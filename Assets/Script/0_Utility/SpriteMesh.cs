using UnityEngine;



//유니티의 unit 단위 , pixelsPerUnit 구하기 정보가 아래 링크에 나와있다  
//ref : https://blogs.unity3d.com/kr/2018/11/19/choosing-the-resolution-of-your-2d-art-assets/


[ExecuteInEditMode]
public class SpriteMesh : MonoBehaviour 
{
    
    //텍스쳐의 좌하단을 원점으로 y축이 위로 증가하는 좌표계를 사용 
    public Vector2 _position;
    public Vector2 _size_vertice;
    public Vector2 _size_uv;
    public Vector2 _pivot;

    public Vector2 _cuttingRate; //0~1
    public Vector2 _cuttingUnit; //절단길이 , unit 단위 

    public float _pixelsPerUnit = 16f; // 1 unit = size / pixelsPerUnit : 20200105 chamto

    public float _world_width = 0f;
    public float _world_height = 0f;

    public Material _spriteMaterial;
	private Mesh _mesh;
	private MeshRenderer _renderer;

    private Vector2 _originPixelPos;
    private Vector2 _pixelPos;
    private Vector2 _texSize;
    private Vector2 _texelPerUv;

    public Transform _ray_start = null;
    public Transform _ray_end = null;
    private Vector3 _hit_point_0;
    private Vector3 _hit_point_1;

    public Transform _test_pos_0 = null;

	//void Awake()
    void Start()
	{
		if (_spriteMaterial == null || _spriteMaterial.mainTexture == null) 
		{
			Debug.LogError("null is material");
			return;
		}

		MeshFilter mf = gameObject.GetComponent<MeshFilter>();
		if (mf == null) 
		{
			mf = gameObject.AddComponent<MeshFilter>();
		}

		_renderer = gameObject.GetComponent<MeshRenderer> ();
		if (_renderer == null) 
		{
			_renderer = gameObject.AddComponent<MeshRenderer>();
		}
		_renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		_renderer.receiveShadows = false;
		_renderer.sharedMaterial = _spriteMaterial;

        //------------------------------------------------

        _ray_start = GameObject.Find("ray_start").transform;
        _ray_end = GameObject.Find("ray_end").transform;
        _test_pos_0 = GameObject.Find("test_pos_0").transform;

        //------------------------------------------------

		_mesh = new Mesh ();
		_mesh.name = "SpriteMesh";
		mf.sharedMesh = _mesh;

        this.UpdateMesh_RayCutting ();

	}

	public bool _Update_perform = false;
	void Update () 
	{
		//if (true == _Update_perform) 
		{
            //fixme : start 로 옮기기 
            _texSize.x = _renderer.sharedMaterial.mainTexture.width;
            _texSize.y = _renderer.sharedMaterial.mainTexture.height;


            _originPixelPos = (_size_vertice / _pixelsPerUnit);
            _cuttingUnit = (_cuttingRate * _size_vertice) / _pixelsPerUnit;
            _pixelPos = _originPixelPos - _cuttingUnit;


            //uv 값의 범위를 0~1 로 만들어주기 위한 비율값을 구한다 
            _texelPerUv = new Vector2(1f / _texSize.x, 1f / _texSize.y);

            _world_width = _pixelPos.x * transform.localScale.x;
            _world_height = _pixelPos.y * transform.localScale.y;


            //this.UpdateMesh_AxisCutting();
            this.UpdateMesh_RayCutting();
		}
	}
    void UpdateMesh_AxisCutting()
    {

        //1  3
        //0  2
        Vector3 pivotPos = new Vector3(_originPixelPos.x * _pivot.x, _originPixelPos.y * _pivot.y, 0);

        Vector3[] vertSize = new Vector3[4];
        vertSize[0] = new Vector3(0, 0); //0
        vertSize[1] = new Vector3(0, _pixelPos.y); //1
        vertSize[2] = new Vector3(_pixelPos.x, 0); //2
        vertSize[3] = new Vector3(_pixelPos.x, _pixelPos.y); //3

        Vector3[] pivot_vert = new Vector3[4];
        pivot_vert[0] = -pivotPos + vertSize[0];
        pivot_vert[1] = -pivotPos + vertSize[1];
        pivot_vert[2] = -pivotPos + vertSize[2];
        pivot_vert[3] = -pivotPos + vertSize[3];


        _mesh.vertices = pivot_vert;


        //==================================================

        //1  3
        //0  2
        _mesh.triangles = new int[] { 0, 1, 3, 0, 3, 2 };


        Vector2[] uv = new Vector2[4];
        uv[0] = this.ToTexPosXY(vertSize[0], _position);
        uv[1] = this.ToTexPosXY(vertSize[1], _position);
        uv[2] = this.ToTexPosXY(vertSize[2], _position);
        uv[3] = this.ToTexPosXY(vertSize[3], _position);
        _mesh.uv = uv;


        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();


        _Update_perform = false;

    }


    //지정된 반직선으로 sprite자름 
	void UpdateMesh_RayCutting () 
	{
        
        //1  3
        //0  2
        Vector3 pivotPos = new Vector3(_originPixelPos.x * _pivot.x, _originPixelPos.y * _pivot.y, 0);

        Vector3[] vertSize = new Vector3[4];
        vertSize[0] = new Vector3(0, 0); //0
        vertSize[1] = new Vector3(0, _pixelPos.y); //1
        vertSize[2] = new Vector3(_pixelPos.x, 0); //2
        vertSize[3] = new Vector3(_pixelPos.x, _pixelPos.y); //3

        Vector3[] pivot_vert = new Vector3[4];
        pivot_vert[0] = -pivotPos + vertSize[0];
        pivot_vert[1] = -pivotPos + vertSize[1];
        pivot_vert[2] = -pivotPos + vertSize[2];
        pivot_vert[3] = -pivotPos + vertSize[3];


        //시험삼아 반직선으로 정점상자 잘라내기 해봄 
        if(true == UtilGS9.Geo.IntersectRay_AABB(pivot_vert[0] + transform.position , pivot_vert[3] + transform.position ,_ray_start.position, _ray_end.position - _ray_start.position ,out _hit_point_0))
        {
            _test_pos_0.position = _hit_point_0;
            pivot_vert[1] = _hit_point_0 - transform.position;
            vertSize[1] = pivot_vert[1] + pivotPos;

            UtilGS9.Geo.IntersectRay_AABB(pivot_vert[0] + transform.position, pivot_vert[3] + transform.position, _ray_end.position, _ray_start.position - _ray_end.position, out _hit_point_1);
            pivot_vert[3] = _hit_point_1 - transform.position;
            vertSize[3] = pivot_vert[3] + pivotPos;
        }

        _mesh.vertices = pivot_vert;
         

        //=================================================

        //1  3
        //0  2
        _mesh.triangles = new int[] { 0, 1, 3, 0, 3, 2 };


        Vector2[] uv = new Vector2[4];
        uv[0] = this.ToTexPosXY(vertSize[0], _position);
        uv[1] = this.ToTexPosXY(vertSize[1], _position);
        uv[2] = this.ToTexPosXY(vertSize[2], _position);
        uv[3] = this.ToTexPosXY(vertSize[3], _position);
        _mesh.uv = uv;


		_mesh.RecalculateNormals ();
		_mesh.RecalculateBounds ();


		_Update_perform = false;

	}

    public Vector2 ToTexPosXY(Vector3 vertSize, Vector2 texPos)
    {
        Vector2 pos;
        pos.x = vertSize.x;
        pos.y = vertSize.y;
        pos = ((pos * _pixelsPerUnit) + texPos);

        return _texelPerUv * pos;
    }

}
