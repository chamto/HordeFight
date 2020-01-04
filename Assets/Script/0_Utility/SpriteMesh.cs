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

    public float _pixelsPerUnit = 16f; // 1 unit = size / pixelsPerUnit : 20200105 chamto

    public float _world_width = 0f;
    public float _world_height = 0f;

    public Material _spriteMaterial;
	private Mesh _mesh;
	private MeshRenderer _renderer;

    private Vector2 _pixelPos;
    private Vector2 _texSize;
    private Vector2 _texelPerUvUnit;


	void Awake()
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

		_mesh = new Mesh ();
		_mesh.name = "SpriteMesh";
		mf.sharedMesh = _mesh;

		this.UpdateMesh ();

	}

	public bool _Update_perform = false;
	void Update () 
	{
		if (true == _Update_perform) 
		{
            //fixme : start 로 옮기기 
            _texSize.x = _renderer.sharedMaterial.mainTexture.width;
            _texSize.y = _renderer.sharedMaterial.mainTexture.height;

            _pixelPos.x = _size_vertice.x / _pixelsPerUnit;
            _pixelPos.y = _size_vertice.y / _pixelsPerUnit;

            //uv 값의 범위를 0~1 로 만들어주기 위한 비율값을 구한다 
            _texelPerUvUnit = new Vector2(1f / _texSize.x, 1f / _texSize.y);

            _world_width = _pixelPos.x * transform.localScale.x;
            _world_height = _pixelPos.y * transform.localScale.y;
            //

			this.UpdateMesh();
		}
	}
	

	void UpdateMesh () 
	{


		{
			//_mesh.vertices = new Vector3[]
			//{
			//	new Vector3(0,-_size.y) * _pixelsPerUnit,
			//	new Vector3(0, -topBottomCutting.y) * _pixelsPerUnit,
			//	new Vector3(_size.x,-_size.y ) * _pixelsPerUnit,
			//	new Vector3(_size.x, -topBottomCutting.y) * _pixelsPerUnit
			//};
		}


        {

            //1  3
            //0  2
            Vector3 pivotPos = new Vector3(_pixelPos.x * _pivot.x, _pixelPos.y * _pivot.y, 0);

            _mesh.vertices = new Vector3[]
            {
                -pivotPos + new Vector3(0, 0),  //0
                -pivotPos + new Vector3(0, _pixelPos.y), //1
                -pivotPos + new Vector3(_pixelPos.x, 0), //2
                -pivotPos + new Vector3(_pixelPos.x , _pixelPos.y ), //3
            };


            //1  3
            //0  2
            _mesh.triangles = new int[] { 0, 1, 3, 0, 3, 2 };

        }
		

        {
            //1  3
            //0  2
            //정점배치 순서에 따라 uv 순서를 맞춘다 
            _mesh.uv = new Vector2[]
            {
            new Vector2(_texelPerUvUnit.x * _position.x,
                        (_texelPerUvUnit.y * (_position.y + 0))),   //left-down 0
            
            new Vector2(_texelPerUvUnit.x * _position.x,
                        (_texelPerUvUnit.y * (_position.y  + _size_uv.y ))), //left-up 1

            new Vector2(_texelPerUvUnit.x * (_position.x + _size_uv.x),
                        (_texelPerUvUnit.y * (_position.y + 0))),        //right-down 2


            new Vector2(_texelPerUvUnit.x * (_position.x + _size_uv.x),
                        (_texelPerUvUnit.y * (_position.y + _size_uv.y ))), //right-up 3

            };
        }

		
		_mesh.RecalculateNormals ();
		_mesh.RecalculateBounds ();


		_Update_perform = false;

	}

}
