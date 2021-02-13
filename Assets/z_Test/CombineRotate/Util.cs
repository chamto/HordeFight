
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

        //실험노트 20210115 에 분선한 글이 있다. 참고하기 
        public static Vector3 PointToLocalSpace_2(Vector3 point,
                             Vector3 AgentHeading,
                             Vector3 AgentSide,
                             Vector3 AgentPosition)
        {

            Vector3 AgentUp = Vector3.Cross(AgentHeading, AgentSide);
            //AgentUp.Normalize();
            //AgentHeading.Normalize();
            //AgentSide.Normalize();

            //로컬좌표기준 이동량을 구한다
            Vector3 t = new Vector3();
            t.x = Vector3.Dot(AgentPosition, AgentSide);
            t.y = Vector3.Dot(AgentPosition, AgentUp);
            t.z = Vector3.Dot(AgentPosition, AgentHeading);

            //Quaternion rotInv = Quaternion.FromToRotation(AgentHeading, Vector3.forward);
            //return (rotInv * point) + t;

            //회전행렬을 만든후 그 역행렬을 구한다 
            Matrix4x4 m = Matrix4x4.identity;
            m.SetColumn(0, AgentSide);
            m.SetColumn(1, AgentUp);
            m.SetColumn(2, AgentHeading);
            m = Matrix4x4.Inverse(m);


            //로컬좌표기준 이동량을 빼준다
            return m.MultiplyPoint(point) - t;

        }

        //PointToLocalSpace_2 함수에서 내적안하는 방식 
        public static Vector3 PointToLocalSpace_2_1(Vector3 point,
                             Vector3 AgentHeading,
                             Vector3 AgentSide,
                             Vector3 AgentPosition)
        {

            Vector3 AgentUp = Vector3.Cross(AgentHeading, AgentSide);


            //회전행렬을 만든후 그 역행렬을 구한다 
            Matrix4x4 m = Matrix4x4.identity;
            m.SetColumn(0, AgentSide);
            m.SetColumn(1, AgentUp);
            m.SetColumn(2, AgentHeading);
            m = Matrix4x4.Inverse(m);

            Vector3 t = AgentPosition;

            //역회전이 적용된 o2 위치값을 빼준다 
            return m.MultiplyPoint(point) - m.MultiplyPoint(t);

        }

        //변환의 역순으로 적용하여 로컬위치를 구한다 
        public static Vector3 PointToLocalSpace_1(Vector3 point,
                             Vector3 AgentHeading,
                             Vector3 AgentSide,
                             Vector3 AgentPosition)
        {

            Vector3 AgentUp = Vector3.Cross(AgentHeading, AgentSide);
            //AgentUp.Normalize();
            //AgentHeading.Normalize();
            //AgentSide.Normalize();

            //이동량을 빼준다 
            Vector3 p1 = point - AgentPosition;

            //Quaternion rotInv = Quaternion.FromToRotation(AgentHeading, Vector3.forward);
            //return (rotInv * t);

            //회전행렬을 만든후 그 역행렬을 구한다 
            Matrix4x4 m = Matrix4x4.identity;
            m.SetColumn(0, AgentSide); //열값 삽입
            m.SetColumn(1, AgentUp);
            m.SetColumn(2, AgentHeading);
            m = Matrix4x4.Inverse(m);

            //역회전을 한다 
            return m.MultiplyPoint(p1);
        }

        //역행렬 없이 상대위치 구하기 
        public static Vector3 PointToLocalSpace_3(Vector3 point,
                             Vector3 AgentHeading,
                             Vector3 AgentSide,
                             Vector3 AgentPosition)
        {

            Vector3 AgentUp = Vector3.Cross(AgentHeading, AgentSide);

            //이동량을 빼준다 
            Vector3 p1 = point - AgentPosition;

            Vector3 t = new Vector3();
            t.x = Vector3.Dot(p1, AgentSide);
            t.y = Vector3.Dot(p1, AgentUp);
            t.z = Vector3.Dot(p1, AgentHeading);


            return t;
        }

    }
}
