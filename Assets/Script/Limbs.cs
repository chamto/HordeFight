using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

//가지 , 팔다리 
public class Limbs : MonoBehaviour 
{

    public class Control
    { 
    }

    public class PathCircle
    {
    }


    static public GameObject CreatePrefab(string prefabPath, Transform parent, string name)
    {
        const string root = "Warcraft/Prefab/3_limbs/";
        GameObject obj = MonoBehaviour.Instantiate(Resources.Load(root + prefabPath)) as GameObject;
        obj.transform.SetParent(parent, false);
        obj.transform.name = name;


        return obj;
    }

    static public Limbs CreateLimbs_TwoHand(Transform parent)
    {
        Limbs limbs = parent.gameObject.AddComponent<Limbs>();

        GameObject limbPrefab = Limbs.CreatePrefab("limbs_twoHand", parent, "limbs_twoHand"); 

        return limbs;
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
