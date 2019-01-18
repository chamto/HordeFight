using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_SphereTree : MonoBehaviour {

    public CircleFactory _circleFactory = null;


	// Use this for initialization
	void Start () 
    {
        _circleFactory = new CircleFactory(1000);
        _circleFactory.SetState(Circle.CircleState.CS_SHOW_ALL); //CS_SHOW_RAYTRACE , CS_SHOW_FRUSTUM , CS_SHOW_RANGE_TEST
		
	}
	
	// Update is called once per frame
	void Update () 
    {
        
        switch (Input.inputString)
        {
            case "a":
            case "A":
                _circleFactory.SetState(Circle.CircleState.CS_SHOW_ALL);
                break;
            case "t":
            case "T":
                _circleFactory.SetState(Circle.CircleState.CS_SHOW_RAYTRACE);
                break;
            case "f":
            case "F":
                _circleFactory.SetState(Circle.CircleState.CS_SHOW_FRUSTUM);
                break;
            case "r":
            case "R":
                _circleFactory.SetState(Circle.CircleState.CS_SHOW_RANGE_TEST);
                break;
        }

        //_circleFactory.Process();
        //_circleFactory.Render();
	}

	private void OnDrawGizmos()
	{
        if (null == _circleFactory) return;
        _circleFactory.Process();
        _circleFactory.Render();
	}

}
