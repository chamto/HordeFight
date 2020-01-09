using UnityEngine;
using UtilGS9;


//유니티의 unit 단위 , pixelsPerUnit 구하기 정보가 아래 링크에 나와있다  
//ref : https://blogs.unity3d.com/kr/2018/11/19/choosing-the-resolution-of-your-2d-art-assets/


[ExecuteInEditMode]
public class SpriteMesh : MonoBehaviour 
{
    
    //텍스쳐의 좌하단을 원점으로 y축이 위로 증가하는 좌표계를 사용 
    public Vector2 _position;
    public Vector2 _size_vertice;
    public Vector2 _size_tex;
    public Vector2 _pivot;

    public Vector2 _cuttingRate; //0~1

    public float _pixelsPerUnit = 16f; // 1 unit = size / pixelsPerUnit : 20200105 chamto

    //======================================================
    //인스펙터 읽기전용
    public Vector3 _cutting_vert; //절단길이 , unit 단위 
    public Vector3 _cutting_tex; //절단길이 , unit 단위 
    public float _world_width = 0f;
    public float _world_height = 0f;
    //======================================================

    private Material _spriteMaterial;
	private Mesh _mesh;
	private MeshRenderer _renderer;

    private Vector2 _vert_unit;
    private Vector2 _tex_unit;
    private Vector2 _pixelPos = ConstV.v2_zero;
    private Vector2 _texSize;
    private Vector2 _texelPerUv = ConstV.v2_zero;

    //======================================================

    public Vector3 _ray_start = ConstV.v3_zero;
    public Vector3 _ray_end = ConstV.v3_zero;
    private Vector3 _hit_point_0;
    private Vector3 _hit_point_1;

    public bool _update_perform = false;
    public bool _enable_ray_cutting = false;


	//void Awake()
    void Start()
	{
        //_spriteMaterial = GameObject.Instantiate<Material>(Resources.Load<Material>("Shader/WeaponSprite"));
        _spriteMaterial = Resources.Load<Material>("Shader/WeaponSprite");

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

        //_ray_start = GameObject.Find("ray_start").transform;
        //_ray_end = GameObject.Find("ray_end").transform;

        //------------------------------------------------

		_mesh = new Mesh ();
		_mesh.name = "SpriteMesh";
		mf.sharedMesh = _mesh;

        //------------------------------------------------

        _texSize.x = _renderer.sharedMaterial.mainTexture.width;
        _texSize.y = _renderer.sharedMaterial.mainTexture.height;

        for (int i = 0; i < 4;i++)
        {
            vertSize[i] = UtilGS9.ConstV.v3_zero;
            texSize[i] = UtilGS9.ConstV.v3_zero;
        }


        _update_perform = true;

	}

	
	void Update () 
	{
		if (true == _update_perform) 
		{
            
            _vert_unit = (_size_vertice / _pixelsPerUnit);
            _tex_unit = (_size_tex / _pixelsPerUnit);
            _cutting_vert = (_cuttingRate * _size_vertice) / _pixelsPerUnit;
            _cutting_tex = (_cuttingRate * _size_tex) / _pixelsPerUnit;

            //uv 값의 범위를 0~1 로 만들어주기 위한 비율값을 구한다 
            _texelPerUv.x = 1f / _texSize.x;
            _texelPerUv.y = 1f / _texSize.y;

            _pixelPos.x = _vert_unit.x - _cutting_vert.x;
            _pixelPos.y = _vert_unit.y - _cutting_vert.y;
            _world_width = _pixelPos.x * transform.localScale.x;
            _world_height = _pixelPos.y * transform.localScale.y;

            //------------------------------------------------

            pivotPos.x = _vert_unit.x * _pivot.x;
            pivotPos.y = _vert_unit.y * _pivot.y;


            vertSize[0] = ConstV.v3_zero;
            vertSize[1] = new Vector3(0, _vert_unit.y); //1
            vertSize[2] = new Vector3(_vert_unit.x, 0); //2
            vertSize[3] = new Vector3(_vert_unit.x, _vert_unit.y); //3

            texSize[0] = ConstV.v3_zero;
            texSize[1] = new Vector3(0, _tex_unit.y); //1
            texSize[2] = new Vector3(_tex_unit.x, 0); //2
            texSize[3] = new Vector3(_tex_unit.x, _tex_unit.y); //3


            //------------------------------------------------


            if(false == _enable_ray_cutting)
                this.UpdateMesh_AxisCutting();
            else
                this.UpdateMesh_RayCutting();
		}
	}


    Vector3 pivotPos = UtilGS9.ConstV.v3_zero;
    Vector3[] vertSize = new Vector3[4];
    Vector3[] texSize = new Vector3[4];
    Vector3[] pivot_vert = new Vector3[4];
    Vector2[] uv = new Vector2[4];
    void UpdateMesh_AxisCutting()
    {

        //1  3
        //0  2
        //pivotPos = new Vector3(_vert_unit.x * _pivot.x, _vert_unit.y * _pivot.y, 0);

        //Vector3[] vertSize = new Vector3[4];
        //vertSize[0] = new Vector3(0, 0); //0
        //vertSize[1] = new Vector3(0, _vert_unit.y); //1
        //vertSize[2] = new Vector3(_vert_unit.x, 0); //2
        //vertSize[3] = new Vector3(_vert_unit.x, _vert_unit.y); //3

        //Vector3[] texSize = new Vector3[4];
        //texSize[0] = new Vector3(0, 0); //0
        //texSize[1] = new Vector3(0, _tex_unit.y); //1
        //texSize[2] = new Vector3(_tex_unit.x, 0); //2
        //texSize[3] = new Vector3(_tex_unit.x, _tex_unit.y); //3


        if(0 > _cuttingRate.y)
        {
            vertSize[1].y += _cutting_vert.y;
            vertSize[3].y += _cutting_vert.y;
            texSize[1].y += _cutting_tex.y;
            texSize[3].y += _cutting_tex.y;
        }
        else if (0 < _cuttingRate.y)
        {
            vertSize[0].y += _cutting_vert.y;
            vertSize[2].y += _cutting_vert.y;
            texSize[0].y += _cutting_tex.y;
            texSize[2].y += _cutting_tex.y;
        }

        if (0 > _cuttingRate.x)
        {
            vertSize[3].x += _cutting_vert.x;
            vertSize[2].x += _cutting_vert.x;
            texSize[3].x += _cutting_tex.x;
            texSize[2].x += _cutting_tex.x;
        }
        else if (0 < _cuttingRate.x)
        {
            vertSize[1].x += _cutting_vert.x;
            vertSize[0].x += _cutting_vert.x;
            texSize[1].x += _cutting_tex.x;
            texSize[0].x += _cutting_tex.x;
        }


        //pivot_vert = new Vector3[4];
        pivot_vert[0] = -pivotPos + vertSize[0];
        pivot_vert[1] = -pivotPos + vertSize[1];
        pivot_vert[2] = -pivotPos + vertSize[2];
        pivot_vert[3] = -pivotPos + vertSize[3];


        _mesh.vertices = pivot_vert;


        //==================================================

        //1  3
        //0  2
        _mesh.triangles = new int[] { 0, 1, 3, 0, 3, 2 };

        //정점 설정에 맞게 UV비율 맞추기 
        //Vector2[] uv = new Vector2[4];
        //uv[0] = this.ToTexPosXY(vertSize[0]);
        //uv[1] = this.ToTexPosXY(vertSize[1]);
        //uv[2] = this.ToTexPosXY(vertSize[2]);
        //uv[3] = this.ToTexPosXY(vertSize[3]);

        //정점 설정에 따라 UV비율 늘리거나 줄이기 
        uv[0] = this.ToTexPosXY(texSize[0]);
        uv[1] = this.ToTexPosXY(texSize[1]);
        uv[2] = this.ToTexPosXY(texSize[2]);
        uv[3] = this.ToTexPosXY(texSize[3]);

        _mesh.uv = uv;


        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();


        _update_perform = false;

    }


    //지정된 반직선으로 sprite자름 
	void UpdateMesh_RayCutting () 
	{
        
        //1  3
        //0  2
        //pivotPos = new Vector3(_vert_unit.x * _pivot.x, _vert_unit.y * _pivot.y, 0);

        //Vector3[] vertSize = new Vector3[4];
        //vertSize[0] = new Vector3(0, 0); //0
        //vertSize[1] = new Vector3(0, _vert_unit.y); //1
        //vertSize[2] = new Vector3(_vert_unit.x, 0); //2
        //vertSize[3] = new Vector3(_vert_unit.x, _vert_unit.y); //3

        //Vector3[] pivot_vert = new Vector3[4];
        pivot_vert[0] = -pivotPos + vertSize[0];
        pivot_vert[1] = -pivotPos + vertSize[1];
        pivot_vert[2] = -pivotPos + vertSize[2];
        pivot_vert[3] = -pivotPos + vertSize[3];

        //1  3 상단 잘라내기 
        //시험삼아 반직선으로 정점상자 잘라내기 해봄 
        if(true == UtilGS9.Geo.IntersectRay_AABB(pivot_vert[0] + transform.position , pivot_vert[3] + transform.position ,_ray_start, _ray_end - _ray_start ,out _hit_point_0))
        {
            //_test_pos_0.position = _hit_point_0;
            pivot_vert[1] = _hit_point_0 - transform.position;
            vertSize[1] = pivot_vert[1] + pivotPos;

            UtilGS9.Geo.IntersectRay_AABB(pivot_vert[0] + transform.position, pivot_vert[3] + transform.position, _ray_end, _ray_start - _ray_end, out _hit_point_1);
            pivot_vert[3] = _hit_point_1 - transform.position;
            vertSize[3] = pivot_vert[3] + pivotPos;
        }

        _mesh.vertices = pivot_vert;
         

        //=================================================

        //1  3
        //0  2
        _mesh.triangles = new int[] { 0, 1, 3, 0, 3, 2 };

        //정점 설정에 맞게 UV비율 맞추기 
        //Vector2[] uv = new Vector2[4];
        uv[0] = this.ToTexPosXY(vertSize[0]);
        uv[1] = this.ToTexPosXY(vertSize[1]);
        uv[2] = this.ToTexPosXY(vertSize[2]);
        uv[3] = this.ToTexPosXY(vertSize[3]);
        _mesh.uv = uv;


		_mesh.RecalculateNormals ();
		_mesh.RecalculateBounds ();


		_update_perform = false;

	}

    public Vector2 ToTexPosXY(Vector3 size_unit)
    {
        Vector2 texPos;
        texPos.x = size_unit.x;
        texPos.y = size_unit.y;
        texPos = ((texPos * _pixelsPerUnit) + _position);

        return _texelPerUv * texPos;
    }

}
