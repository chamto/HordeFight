using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_SphereTree : MonoBehaviour 
{

    public CircleFactory _circleFactory = null;
    public bool _pause = false;
    public float _sphereRadius = 10;
    public eRender_Tree _render_tree = eRender_Tree.ALL1;

	// Use this for initialization
	void Start () 
    {
        _circleFactory = new CircleFactory(100);
        _circleFactory.SetState(Circle.State.SHOW_ALL); //CS_SHOW_RAYTRACE , CS_SHOW_FRUSTUM , CS_SHOW_RANGE_TEST
		
	}
	
	// Update is called once per frame
	void Update () 
    {
        
        switch (Input.inputString)
        {
            case "a":
            case "A":
                _circleFactory.SetState(Circle.State.SHOW_ALL);
                break;
            case "t":
            case "T":
                _circleFactory.SetState(Circle.State.SHOW_RAYTRACE);
                break;
            case "f":
            case "F":
                _circleFactory.SetState(Circle.State.SHOW_FRUSTUM);
                break;
            case "r":
            case "R":
                _circleFactory.SetState(Circle.State.SHOW_RANGE_TEST);
                break;
            case "p":
            case "P":
                _pause = !_pause;
                break;
            case "1":
                _render_tree = _render_tree ^ eRender_Tree.ROOT;
                break;
            case "2":
                _render_tree ^= eRender_Tree.LEAF;
                break;
            case "3":
                _render_tree ^= eRender_Tree.TEXT;
                break;
            case "4": 
                _render_tree = eRender_Tree.ALL1;
                break;
        }

        if (null == _circleFactory) return;

        _circleFactory.SetRenderMode(_render_tree);
        if (false == _pause)
            _circleFactory.Process();
	}

	private void OnDrawGizmos()
	{

        DefineO.DrawCircle(0, 0, _sphereRadius, 0x00ffffff);
        //return;

        if (null == _circleFactory) return;

        //if(false == _pause)
            //_circleFactory.Process();
        
        _circleFactory.Render();
	}

}
