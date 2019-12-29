
//===============================================================================
// @ IvQuat.h
// 
// Quaternion class
// ------------------------------------------------------------------------------
// Copyright (C) 2004 by Elsevier, Inc. All rights reserved.
//
//
//
//===============================================================================

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ML
{
	
	public class Util
	{
		static public float  kEpsilon = 1.0e-6f; //허용오차
		//float.Epsilon : 실수 오차값이 너무 작아, 계산 범위에 못 들어 올 수 있다.

		static public bool IsZero( float a ) 
		{
			
			return ( Mathf.Abs(a) < kEpsilon );
			//return ( Mathf.Abs(a) < float.Epsilon );

		}

		static public float InvSqrt( float val )     
		{ 
			return 1.0f/ Mathf.Sqrt( val ); 
		}

		static public void SinCos( float a, out float sina, out float cosa )
		{
			sina = Mathf.Sin(a);
			cosa = Mathf.Cos(a);
		}


		static public string ToBit(double number , bool setBigEndian = true) 
		{
			byte[] arByte =  BitConverter.GetBytes (number);
			if (true == setBigEndian && true == BitConverter.IsLittleEndian) 
			{
				Array.Reverse (arByte); //사용자 직관적인 빅인디언으로 바꿔준다 
			}
			string buffer = "";
			foreach(byte b in arByte)
			{
				buffer += string.Format ("{0,8} ", Convert.ToString(b,2).PadLeft(8,'0')); //2진수 문자열로 변환
			}

			//Debug.Log (BitConverter.ToDouble (arByte, 0)); //chamto test

			return buffer;
		}

		static public string ToBit(float number , bool setBigEndian = true) 
		{
			byte[] arByte =  BitConverter.GetBytes (number);
			if (true == setBigEndian && true == BitConverter.IsLittleEndian) 
			{
				Array.Reverse (arByte); //사용자 직관적인 빅인디언으로 바꿔준다 
			}
			string buffer = "";
			foreach(byte b in arByte)
			{
				buffer += string.Format ("{0,8} ", Convert.ToString(b,2).PadLeft(8,'0')); //2진수 문자열로 변환
			}

			//Debug.Log (BitConverter.ToDouble (arByte, 0)); //chamto test

			return buffer;
		}

		static public string ToBit(int number , bool setBigEndian = true) 
		{
			byte[] arByte =  BitConverter.GetBytes (number);
			if (true == setBigEndian && true == BitConverter.IsLittleEndian) 
			{
				Array.Reverse (arByte); //사용자 직관적인 빅인디언으로 바꿔준다 
			}
			string buffer = "";
			foreach(byte b in arByte)
			{
				buffer += string.Format ("{0,8} ", Convert.ToString(b,2).PadLeft(8,'0')); //2진수 문자열로 변환
			}

			return buffer;
		}


		//123 456 789
		//789 456 123
		//321 654 987
		static public  float ConvertEndian(float number)
		{
			byte[] arByte = BitConverter.GetBytes(number);
		
			Array.Reverse(arByte);
		
			number = BitConverter.ToInt32(arByte, 0);

			return number;
		}



	}
}
