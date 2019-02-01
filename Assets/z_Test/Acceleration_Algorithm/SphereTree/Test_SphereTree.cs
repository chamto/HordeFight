using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_SphereTree : MonoBehaviour 
{

    public CircleFactory _circleFactory = null;
    public bool _pause = false;

	// Use this for initialization
	void Start () 
    {
        _circleFactory = new CircleFactory(20);
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
        }

        //_circleFactory.Process();
        //_circleFactory.Render();
	}

	private void OnDrawGizmos()
	{


        if (null == _circleFactory) return;

        if(false == _pause)
            _circleFactory.Process();
        
        _circleFactory.Render();
	}

}
