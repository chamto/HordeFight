using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmedInfo : MonoBehaviour 
{
    public enum eKind
    {
        Sword,
        SideSword,
        LongSword,
        Shield,
        Spear,
        Staff,
        Bow,
    }

    public string _name = "";
    public eKind _eKind = eKind.Sword;
    public float _length = 1f;
	
}
