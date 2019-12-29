
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


public class IvQuat 
{
	// member variables
	public float w, x, y, z;      

	// useful defaults
	static public IvQuat   zero = new IvQuat(0,0,0,0);
	static public IvQuat   identity = new IvQuat(1f,0,0,0);



	public IvQuat()
	{
		w = 1.0f; x = 0f; y = 0f; z = 0f;
	}

	public IvQuat(float w, float x, float y, float z)
	{
		this.w = w; this.x = x; this.y = y; this.z = z;
	}

	//-------------------------------------------------------------------------------
	// @ IvQuat::IvQuat()
	//-------------------------------------------------------------------------------
	// Axis-angle constructor
	//-------------------------------------------------------------------------------
	public IvQuat(Vector3 axis, float angle )
	{
		Set( axis, angle );
	}   // End of IvQuat::IvQuat()


	//-------------------------------------------------------------------------------
	// @ IvQuat::IvQuat()
	//-------------------------------------------------------------------------------
	// To-from vector constructor
	//-------------------------------------------------------------------------------
	public IvQuat(  Vector3 from,  Vector3 to )
	{
		Set( from, to );
	}   // End of IvQuat::IvQuat()

	//-------------------------------------------------------------------------------
	// @ IvQuat::IvQuat()
	//-------------------------------------------------------------------------------
	// Vector constructor
	//-------------------------------------------------------------------------------
	public IvQuat( ref Vector3 vector )
	{
		Set( 0.0f, vector.x, vector.y, vector.z );
	}   // End of IvQuat::IvQuat()


	//-------------------------------------------------------------------------------
	// @ IvQuat::IvQuat()
	//-------------------------------------------------------------------------------
	// Rotation matrix constructor
	//-------------------------------------------------------------------------------
	//public IvQuat( ref Matrix33 rotation )
	public IvQuat(  Matrix4x4 rotation )
	{
		
		//float trace = rotation.Trace();
		float trace = rotation.m00 + rotation.m11 + rotation.m22;
		if ( trace > 0.0f )
		{
			float s = Mathf.Sqrt( trace + 1.0f );
			w = s*0.5f;
			float recip = 0.5f/s;
			x = (rotation.m21 - rotation.m12)*recip;
			y = (rotation.m02 - rotation.m20)*recip;
			z = (rotation.m10 - rotation.m01)*recip;
		}
		else
		{ 

			int i = 0;
			if ( rotation.m11 > rotation.m00 )
				i = 1;
			if ( rotation[2,2] > rotation[i,i] )
				i = 2;
			int j = (i+1)%3;
			int k = (j+1)%3;
			float s = Mathf.Sqrt( rotation[i,i] - rotation[j,j] - rotation[k,k] + 1.0f );
			//(*this)[i] = 0.5f*s;
			this.setXYZ(i, 0.5f * s);
			float recip = 0.5f/s;
			w = (rotation[k,j] - rotation[j,k])*recip;
			//(*this)[j] = (rotation[j,i] + rotation[i,j])*recip;
			this.setXYZ(j, (rotation[j,i] + rotation[i,j])*recip);
			//(*this)[k] = (rotation[k,i] + rotation[i,k])*recip;
			this.setXYZ(k, (rotation[k,i] + rotation[i,k])*recip);
		}

	}   // End of IvQuat::IvQuat()

	private void setXYZ(int index, float value)
	{
		switch (index) 
		{
		case 0:
			this.x = value;
			break;
		case 1:
			this.y = value;
			break;
		case 2:
			this.z = value;
			break;
		}
	}

	public void Set( float _w, float _x, float _y, float _z )
	{
		w = _w; x = _x; y = _y; z = _z;
	}   // End of IvQuat::Set()

	//-------------------------------------------------------------------------------
	// @ IvQuat::Set()
	//-------------------------------------------------------------------------------
	// Set quaternion based on axis-angle
	//-------------------------------------------------------------------------------
	public void Set( Vector3 axis, float angle )
	{
		// if axis of rotation is zero vector, just set to identity quat
		float length = axis.sqrMagnitude;
		if ( ML.Util.IsZero( length ) )
		{
			Identity();
			return;
		}

		// take half-angle
		angle *= 0.5f;

		float sintheta=0, costheta=0;
		ML.Util.SinCos(angle, out sintheta, out costheta);

		float scaleFactor = sintheta/Mathf.Sqrt( length );

		w = costheta;
		x = scaleFactor * axis.x;
		y = scaleFactor * axis.y;
		z = scaleFactor * axis.z;

	}   // End of IvQuat::Set()


	//-------------------------------------------------------------------------------
	// @ IvQuat::Set()
	//-------------------------------------------------------------------------------
	// Set quaternion based on start and end vectors
	// 
	// This is a slightly faster method than that presented in the book, and it 
	// doesn't require unit vectors as input.  Found on GameDev.net, in an article by
	// minorlogic.  Original source unknown.
	//-------------------------------------------------------------------------------
	public void Set( Vector3 from,  Vector3 to )
	{
		// get axis of rotation
		Vector3 axis = Vector3.Cross (from, to);


		// get scaled cos of angle between vectors and set initial quaternion
		Set(  Vector3.Dot(from, to), axis.x, axis.y, axis.z );
		// quaternion at this point is ||from||*||to||*( cos(theta), r*sin(theta) )

		// normalize to remove ||from||*||to|| factor
		Normalize();
		// quaternion at this point is ( cos(theta), r*sin(theta) )
		// what we want is ( cos(theta/2), r*sin(theta/2) )

		// set up for half angle calculation
		w += 1.0f;

		// now when we normalize, we'll be dividing by sqrt(2*(1+cos(theta))), which is 
		// what we want for r*sin(theta) to give us r*sin(theta/2)  (see pages 487-488)
		// 
		// w will become 
		//                 1+cos(theta)
		//            ----------------------
		//            sqrt(2*(1+cos(theta)))        
		// which simplifies to
		//                cos(theta/2)

		// before we normalize, check if vectors are opposing
		if ( w <= ML.Util.kEpsilon )
		{
			// rotate pi radians around orthogonal vector
			// take cross product with x axis
			if ( from.z*from.z > from.x*from.x )
				Set( 0.0f, 0.0f, from.z, -from.y );
			// or take cross product with z axis
			else
				Set( 0.0f, from.y, -from.x, 0.0f );
		}

		// normalize again to get rotation quaternion
		Normalize();

	}   // End of IvQuat::Set()



	//-------------------------------------------------------------------------------
	// @ IvQuat::Set()
	//-------------------------------------------------------------------------------
	// Set quaternion based on fixed angles
	//
	// 오일러각: x(월드) -> y(로컬) -> z(로컬) 순서로 결합
	// 고정각 : z(월드) -> y(월드) -> x(월드) 순서로 결합
	//-------------------------------------------------------------------------------
	public void Set( float zRotation, float yRotation, float xRotation ) 
	{
		zRotation *= 0.5f;
		yRotation *= 0.5f;
		xRotation *= 0.5f;

		// get sines and cosines of half angles
		float Cx=0, Sx=0;
		ML.Util.SinCos(xRotation, out Sx, out Cx);

		float Cy=0, Sy=0;
		ML.Util.SinCos(yRotation, out Sy, out Cy);

		float Cz=0, Sz=0;
		ML.Util.SinCos(zRotation, out Sz, out Cz);

		// multiply it out
		w = Cx*Cy*Cz - Sx*Sy*Sz;
		x = Sx*Cy*Cz + Cx*Sy*Sz;
		y = Cx*Sy*Cz - Sx*Cy*Sz;
		z = Cx*Cy*Sz + Sx*Sy*Cx;

	}   // End of IvQuat::Set()

	//-------------------------------------------------------------------------------
	// @ IvQuat::Zero()
	//-------------------------------------------------------------------------------
	// Zero all elements
	//-------------------------------------------------------------------------------
	public void Zero()
	{
	    x = y = z = w = 0.0f;
	}   // End of IvQuat::Zero()

	//-------------------------------------------------------------------------------
	// @ IvQuat::Identity()
	//-------------------------------------------------------------------------------
	// Set to identity quaternion
	//-------------------------------------------------------------------------------
	public void Identity()
	{
	    x = y = z = 0.0f;
	    w = 1.0f;
	}   // End of IvQuat::Identity







	//-------------------------------------------------------------------------------
	// @ IvQuat::Magnitude()
	//-------------------------------------------------------------------------------
	// Quaternion magnitude (square root of norm)
	//-------------------------------------------------------------------------------
	public float Magnitude()
	{
		return Mathf.Sqrt( w*w + x*x + y*y + z*z );

	}   // End of IvQuat::Magnitude()


	//-------------------------------------------------------------------------------
	// @ IvQuat::Norm()
	//-------------------------------------------------------------------------------
	// Quaternion norm
	//-------------------------------------------------------------------------------
	public float Norm()
	{
		return ( w*w + x*x + y*y + z*z );

	}   // End of IvQuat::Norm()



	//-------------------------------------------------------------------------------
	// @ IvQuat::IsZero()
	//-------------------------------------------------------------------------------
	// Check for zero quat
	//-------------------------------------------------------------------------------
	public bool IsZero()
	{
		return ML.Util.IsZero(w*w + x*x + y*y + z*z);

	}   // End of IvQuat::IsZero()


	//-------------------------------------------------------------------------------
	// @ IvQuat::IsUnit()
	//-------------------------------------------------------------------------------
	// Check for unit quat
	//-------------------------------------------------------------------------------
	public bool IsUnit()
	{
		return ML.Util.IsZero(1.0f - w*w - x*x - y*y - z*z);

	}   // End of IvQuat::IsUnit()


	//-------------------------------------------------------------------------------
	// @ IvQuat::IsIdentity()
	//-------------------------------------------------------------------------------
	// Check for identity quat
	//-------------------------------------------------------------------------------
	public bool IsIdentity()
	{
		return ML.Util.IsZero(1.0f - w)
			&& ML.Util.IsZero( x ) 
			&& ML.Util.IsZero( y )
			&& ML.Util.IsZero( z );

	}   // End of IvQuat::IsIdentity()





	//-------------------------------------------------------------------------------
	// @ IvQuat::GetAxisAngle()
	//-------------------------------------------------------------------------------
	// Get axis-angle based on quaternion
	//-------------------------------------------------------------------------------
	public void GetAxisAngle( out Vector3 axis, out float angle )
	{
		axis = Vector3.zero;
		angle = 2.0f*Mathf.Acos( w );
		float length = Mathf.Sqrt( 1.0f - w*w );
		if ( ML.Util.IsZero(length) )
			axis = Vector3.zero;
		else
		{
			length = 1.0f/length;
			axis.Set( x*length, y*length, z*length );
		}

	}   // End of IvQuat::GetAxisAngle()


	//-------------------------------------------------------------------------------
	// @ IvQuat::Clean()
	//-------------------------------------------------------------------------------
	// Set elements close to zero equal to zero
	//-------------------------------------------------------------------------------
	public void Clean()
	{
		if ( ML.Util.IsZero( w ) )
			w = 0.0f;
		if ( ML.Util.IsZero( x ) )
			x = 0.0f;
		if ( ML.Util.IsZero( y ) )
			y = 0.0f;
		if ( ML.Util.IsZero( z ) )
			z = 0.0f;

	}   // End of IvQuat::Clean()


	//-------------------------------------------------------------------------------
	// @ IvQuat::Normalize()
	//-------------------------------------------------------------------------------
	// Set to unit quaternion
	//-------------------------------------------------------------------------------
	public void Normalize()
	{
		float lengthsq = w*w + x*x + y*y + z*z;

		if ( ML.Util.IsZero( lengthsq ) )
		{
			Zero();
		}
		else
		{
			float factor = ML.Util.InvSqrt( lengthsq );
			w *= factor;
			x *= factor;
			y *= factor;
			z *= factor;
		}

	}   // End of IvQuat::Normalize()

	//-------------------------------------------------------------------------------
	// @ IvQuat::Dot()
	//-------------------------------------------------------------------------------
	// Dot product by self
	//-------------------------------------------------------------------------------
	public float Dot( IvQuat quat )
	{
		return ( w*quat.w + x*quat.x + y*quat.y + z*quat.z);

	}   // End of IvQuat::Dot()


	//-------------------------------------------------------------------------------
	// @ Dot()
	//-------------------------------------------------------------------------------
	// Dot product friend operator
	//-------------------------------------------------------------------------------
	static public float Dot( IvQuat quat1,  IvQuat quat2 )
	{
		return (quat1.w*quat2.w + quat1.x*quat2.x + quat1.y*quat2.y + quat1.z*quat2.z);

	}   // End of Dot()

	//-------------------------------------------------------------------------------
	// @ ::Conjugate()
	//-------------------------------------------------------------------------------
	// Compute complex conjugate
	//-------------------------------------------------------------------------------
	public IvQuat Conjugate( IvQuat quat ) 
	{
		return new IvQuat( quat.w, -quat.x, -quat.y, -quat.z );

	}   // End of Conjugate()


	//-------------------------------------------------------------------------------
	// @ IvQuat::Conjugate()
	//-------------------------------------------------------------------------------
	// Set self to complex conjugate
	//-------------------------------------------------------------------------------
	public IvQuat Conjugate()
	{
		x = -x;
		y = -y;
		z = -z;

		return this;

	}   // End of Conjugate()


	//-------------------------------------------------------------------------------
	// @ ::Inverse()
	//-------------------------------------------------------------------------------
	// Compute quaternion inverse
	//-------------------------------------------------------------------------------
	public IvQuat Inverse(IvQuat quat )
	{
		float norm = quat.w*quat.w + quat.x*quat.x + quat.y*quat.y + quat.z*quat.z;
		// if we're the zero quaternion, just return identity
		if ( false == ML.Util.IsZero( norm ) )
		{
			//ASSERT( false );
			return new IvQuat();
		}

		float normRecip = 1.0f / norm;
		return new IvQuat( normRecip*quat.w, -normRecip*quat.x, -normRecip*quat.y, 
			-normRecip*quat.z );

	}   // End of Inverse()


	//-------------------------------------------------------------------------------
	// @ IvQuat::Inverse()
	//-------------------------------------------------------------------------------
	// Set self to inverse
	//-------------------------------------------------------------------------------
	public  IvQuat Inverse()
	{
		float norm = w*w + x*x + y*y + z*z;
		// if we're the zero quaternion, just return
		if ( ML.Util.IsZero( norm ) )
			return this;

		float normRecip = 1.0f / norm;
		w = normRecip*w;
		x = -normRecip*x;
		y = -normRecip*y;
		z = -normRecip*z;

		return this;

	}   // End of Inverse()



	//-------------------------------------------------------------------------------
	// @ IvQuat::Rotate()
	//-------------------------------------------------------------------------------
	// Rotate vector by quaternion
	// Assumes quaternion is normalized!
	//-------------------------------------------------------------------------------
	public Vector3  Rotate( Vector3 vector ) //???
	{
		//ASSERT( IsUnit() );

		float vMult = 2.0f*(x*vector.x + y*vector.y + z*vector.z);
		float crossMult = 2.0f*w;
		float pMult = crossMult*w - 1.0f;

		return new Vector3( pMult*vector.x + vMult*x + crossMult*(y*vector.z - z*vector.y),
			pMult*vector.y + vMult*y + crossMult*(z*vector.x - x*vector.z),
			pMult*vector.z + vMult*z + crossMult*(x*vector.y - y*vector.x) );

	}   // End of IvQuat::Rotate()


	//-------------------------------------------------------------------------------
	// @ Lerp()
	//-------------------------------------------------------------------------------
	// Linearly interpolate two quaternions
	// This will always take the shorter path between them
	//-------------------------------------------------------------------------------
	public IvQuat Lerp(ref IvQuat start, ref IvQuat end, float t )
	{
		// get cos of "angle" between quaternions
		float cosTheta = start.Dot( end );

		// initialize result
		IvQuat result = t*end;

		// if "angle" between quaternions is less than 90 degrees
		if ( cosTheta >= ML.Util.kEpsilon )
		{
			// use standard interpolation
			result += (1.0f-t)*start;
		}
		else
		{
			// otherwise, take the shorter path
			result += (t-1.0f)*start;
		}

		return result;

	}   // End of Lerp()


	//-------------------------------------------------------------------------------
	// @ Slerp()
	//-------------------------------------------------------------------------------
	// Spherical linearly interpolate two quaternions
	// This will always take the shorter path between them
	//-------------------------------------------------------------------------------
	public IvQuat Slerp(ref IvQuat start, ref IvQuat end, float t )
	{
		// get cosine of "angle" between quaternions
		float cosTheta = start.Dot( end );
		float startInterp, endInterp;

		// if "angle" between quaternions is less than 90 degrees
		if ( cosTheta >= ML.Util.kEpsilon )
		{
			// if angle is greater than zero
			if ( (1.0f - cosTheta) > ML.Util.kEpsilon )
			{
				// use standard slerp
				float theta = Mathf.Acos( cosTheta );
				float recipSinTheta = 1.0f/ Mathf.Sin( theta );

				startInterp = Mathf.Sin( (1.0f - t)*theta )*recipSinTheta;
				endInterp = Mathf.Sin( t*theta )*recipSinTheta;
			}
			// angle is close to zero
			else
			{
				// use linear interpolation
				startInterp = 1.0f - t;
				endInterp = t;
			}
		}
		// otherwise, take the shorter route
		else
		{
			// if angle is less than 180 degrees
			if ( (1.0f + cosTheta) > ML.Util.kEpsilon )
			{
				// use slerp w/negation of start quaternion
				float theta = Mathf.Acos( -cosTheta );
				float recipSinTheta = 1.0f/Mathf.Sin( theta );

				startInterp = Mathf.Sin( (t-1.0f)*theta )*recipSinTheta;
				endInterp = Mathf.Sin( t*theta )*recipSinTheta;
			}
			// angle is close to 180 degrees
			else
			{
				// use lerp w/negation of start quaternion
				startInterp = t - 1.0f;
				endInterp = t;
			}
		}

		return startInterp*start + endInterp*end;

	}   // End of Slerp()


	//-------------------------------------------------------------------------------
	// @ ApproxSlerp()
	//-------------------------------------------------------------------------------
	// Approximate spherical linear interpolation of two quaternions
	// Based on "Hacking Quaternions", Jonathan Blow, Game Developer, March 2002.
	// See Game Developer, February 2004 for an alternate method.
	//-------------------------------------------------------------------------------
	public IvQuat ApproxSlerp( ref IvQuat start, ref IvQuat end, float t )
	{
		float cosTheta = start.Dot( end );

		// correct time by using cosine of angle between quaternions
		float factor = 1.0f - 0.7878088f*cosTheta;
		float k = 0.5069269f;
		factor *= factor;
		k *= factor;

		float b = 2*k;
		float c = -3*k;
		float d = 1 + k;

		t = t*(b*t + c) + d;

		// initialize result
		IvQuat result = t*end;

		// if "angle" between quaternions is less than 90 degrees
		if ( cosTheta >= ML.Util.kEpsilon )
		{
			// use standard interpolation
			result += (1.0f-t)*start;
		}
		else
		{
			// otherwise, take the shorter path
			result += (t-1.0f)*start;
		}

		return result;

	}   // End of ApproxSlerp()



	//-------------------------------------------------------------------------------
	// @ IvQuat::operator=()
	//-------------------------------------------------------------------------------
	// Assignment operator
	//-------------------------------------------------------------------------------
	//	IvQuat operator=(const IvQuat& other)
	//	{
	//		// if same object
	//		if ( this == &other )
	//			return *this;
	//
	//		w = other.w;
	//		x = other.x;
	//		y = other.y;
	//		z = other.z;
	//
	//		return *this;
	//
	//	}   // End of IvQuat::operator=()

	//-------------------------------------------------------------------------------
	// @ IvQuat::operator==()
	//-------------------------------------------------------------------------------
	// Comparison operator
	//-------------------------------------------------------------------------------
	//	public bool operator==( const IvQuat& other ) const
	//	{
	//		if ( ::IsZero( other.w - w )
	//			&& ::IsZero( other.x - x )
	//			&& ::IsZero( other.y - y )
	//			&& ::IsZero( other.z - z ) )
	//			return true;
	//
	//		return false;   
	//	}   // End of IvQuat::operator==()


	//-------------------------------------------------------------------------------
	// @ IvQuat::operator!=()
	//-------------------------------------------------------------------------------
	// Comparison operator
	//-------------------------------------------------------------------------------
	//	bool operator!=( const IvQuat& other ) const
	//	{
	//		if ( ::IsZero( other.w - w )
	//			|| ::IsZero( other.x - x )
	//			|| ::IsZero( other.y - y )
	//			|| ::IsZero( other.z - z ) )
	//			return false;
	//
	//		return true;
	//	}   // End of IvQuat::operator!=()



	//-------------------------------------------------------------------------------
	// @ IvQuat::operator+()
	//-------------------------------------------------------------------------------
	// Add quat to self and return
	//-------------------------------------------------------------------------------
	//	public IvQuat operator+( const IvQuat& other ) const
	//	{
	//		return IvQuat( w + other.w, x + other.x, y + other.y, z + other.z );
	//
	//	}   // End of IvQuat::operator+()
	public static IvQuat operator +(IvQuat left, IvQuat right) 
	{
		return new IvQuat( left.w + right.w, left.x + right.x, left.y + right.y, left.z + right.z );
	}

	//-------------------------------------------------------------------------------
	// @ IvQuat::operator+=()
	//-------------------------------------------------------------------------------
	// Add quat to self, store in self
	//-------------------------------------------------------------------------------
	//	IvQuat& operator+=( const IvQuat& other )
	//	{
	//		w += other.w;
	//		x += other.x;
	//		y += other.y;
	//		z += other.z;
	//
	//		return *this;
	//
	//	}   // End of IvQuat::operator+=()


	//-------------------------------------------------------------------------------
	// @ IvQuat::operator-()
	//-------------------------------------------------------------------------------
	// Subtract quat from self and return
	//-------------------------------------------------------------------------------
	//	IvQuat operator-( const IvQuat& other ) const
	//	{
	//		return IvQuat( w - other.w, x - other.x, y - other.y, z - other.z );
	//
	//	}   // End of IvQuat::operator-()


	//-------------------------------------------------------------------------------
	// @ IvQuat::operator-=()
	//-------------------------------------------------------------------------------
	// Subtract quat from self, store in self
	//-------------------------------------------------------------------------------
	//	IvQuat& operator-=( const IvQuat& other )
	//	{
	//		w -= other.w;
	//		x -= other.x;
	//		y -= other.y;
	//		z -= other.z;
	//
	//		return *this;
	//
	//	}   // End of IvQuat::operator-=()


	//-------------------------------------------------------------------------------
	// @ IvQuat::operator-=() (unary)
	//-------------------------------------------------------------------------------
	// Negate self and return
	//-------------------------------------------------------------------------------
	//	IvQuat operator-() const
	//	{
	//		return IvQuat(-w, -x, -y, -z);
	//	}    // End of IvQuat::operator-()


	//-------------------------------------------------------------------------------
	// @ operator*()
	//-------------------------------------------------------------------------------
	// Scalar multiplication
	//-------------------------------------------------------------------------------
	//	IvQuat operator*( float scalar, const IvQuat& quat )
	//	{
	//		return IvQuat( scalar*quat.w, scalar*quat.x, scalar*quat.y, scalar*quat.z );
	//
	//	}   // End of operator*()
	public static IvQuat operator *(float scalar, IvQuat quat) 
	{
		return new IvQuat( scalar*quat.w, scalar*quat.x, scalar*quat.y, scalar*quat.z );
	}

	//-------------------------------------------------------------------------------
	// @ IvQuat::operator*=()
	//-------------------------------------------------------------------------------
	// Scalar multiplication by self
	//-------------------------------------------------------------------------------
	//	IvQuat& operator*=( float scalar )
	//	{
	//		w *= scalar;
	//		x *= scalar;
	//		y *= scalar;
	//		z *= scalar;
	//
	//		return *this;
	//
	//	}   // End of IvQuat::operator*=()


	//-------------------------------------------------------------------------------
	// @ IvQuat::operator*()
	//-------------------------------------------------------------------------------
	// Quaternion multiplication
	//-------------------------------------------------------------------------------
	//	IvQuat operator*( const IvQuat& other ) const
	//	{
	//		return IvQuat( w*other.w - x*other.x - y*other.y - z*other.z,
	//			w*other.x + x*other.w + y*other.z - z*other.y,
	//			w*other.y + y*other.w + z*other.x - x*other.z,
	//			w*other.z + z*other.w + x*other.y - y*other.x );
	//
	//	}   // End of IvQuat::operator*()
	public static IvQuat operator *(IvQuat left, IvQuat right) 
	{
		return new IvQuat( 
			left.w * right.w - left.x * right.x - left.y * right.y - left.z * right.z,
			left.w * right.x + left.x * right.w + left.y * right.z - left.z * right.y,
			left.w * right.y + left.y * right.w + left.z * right.x - left.x * right.z,
			left.w * right.z + left.z * right.w + left.x * right.y - left.y * right.x );
	}

	//-------------------------------------------------------------------------------
	// @ IvQuat::operator*=()
	//-------------------------------------------------------------------------------
	// Quaternion multiplication by self
	//-------------------------------------------------------------------------------
	//	IvQuat& operator*=( const IvQuat& other )
	//	{
	//		Set( w*other.w - x*other.x - y*other.y - z*other.z,
	//			w*other.x + x*other.w + y*other.z - z*other.y,
	//			w*other.y + y*other.w + z*other.x - x*other.z,
	//			w*other.z + z*other.w + x*other.y - y*other.x );
	//
	//		return *this;
	//
	//	}   // End of IvQuat::operator*=()



	//----------------------------------------------------------------------------
	//----------------------------------------------------------------------------
	//----------------------------------------------------------------------------



	//----------------------------------------------------------------------------
	// @ IvMatrix44::GetFixedAnglesFrom()
	// ---------------------------------------------------------------------------
	// Gets one set of possible z-y-x fixed angles that will generate this matrix
	// Assumes that upper 3x3 is a rotation matrix
	//----------------------------------------------------------------------------
	static public Vector3 GetFixedAnglesFrom(ref Matrix4x4 mV)
	{
		//IvMatrixx
		// 0  4  8   12
		// 1  5  9   13
		// 2  6  10  14
		// 3  7  11  15

		//Unity Matrix
		//m00  m01  m02  m03
		//m10  m11  m12  m13
		//m20  m21  m22  m23
		//m30  m31  m32  m33

		Vector3 rotation;
		float Cx, Sx;
		float Cy, Sy;
		float Cz, Sz;



		Sy = mV.m02; //mV[8]

		//chamto test
		//Sy = 1.00000011920929f; //  1 + 1/(2^23) : 문제가 되는 Sy의 실제 비트값
		//DebugWide.LogGreen(ML.Util.ToBit(Sy) + ": Sy: " + Sy); //비정상 1.0f
		//DebugWide.LogGreen(ML.Util.ToBit(1.0f) + ": 1.0f "); //정상 1.0f


		//Sy가 1.0f보다 아주 작은 차이로 크기 때문에 (1.0f - Sy*Sy)는 음수가 나오게 된다.
		//제곱근시 음수가 들어가면 NaN이 나오게 된다.
		Cy = Mathf.Sqrt( Mathf.Abs(1.0f - Sy*Sy) ); 

		// normal case
		if ( false == ML.Util.IsZero( Cy ) )
		{
			float factor = 1.0f/Cy;
			//DebugWide.LogBlue (Sy + " , " + Cy + " , " + (1.0f-Sy*Sy) + " , " + (1.0f - 1.0f*1.0f)); //chamto test
			//DebugWide.LogGreen(ML.Util.ToBit(Cy) + ": Cy : " + Cy);
			//DebugWide.LogGreen(ML.Util.ToBit(Sy*Sy) + ": Sy*Sy : " + Sy*Sy);

			Sx = -mV.m12 * factor; //mV[9]
			Cx =  mV.m22 * factor; //mV[10]
			Sz = -mV.m01 * factor; //mV[4]
			Cz =  mV.m00 * factor;  //mV[0]
		}
		// x and z axes aligned
		else
		{
			Sz = 0.0f;
			Cz = 1.0f;
			Sx = mV.m21;  //mV[6]
			Cx = mV.m11;  //mV[5]
		}

		//DebugWide.LogYellow (" Sz: " + Sz + " Cz: " + Cz + " Sy: " + Sy + " Cy: " + Cy + " Sx: " + Sx + " Cx: " + Cx); //chamto test

		rotation.z = Mathf.Atan2( Sz, Cz );
		rotation.y = Mathf.Atan2( Sy, Cy );
		rotation.x = Mathf.Atan2( Sx, Cx );

		return rotation;

	}  // End of IvMatrix44::GetFixedAngles()


	//----------------------------------------------------------------------------
	// @ IvMatrix44::GetAxisAngle()
	// ---------------------------------------------------------------------------
	// Gets one possible axis-angle pair that will generate this matrix
	// Assumes that upper 3x3 is a rotation matrix
	//----------------------------------------------------------------------------
	static public void GetAxisAngleFrom(ref Matrix4x4 mV , out Vector3 axis, out float angle )
	{
		//IvMatrixx - i + 4 * j = [i,j]
		// 0  4  8   12
		// 1  5  9   13
		// 2  6  10  14
		// 3  7  11  15


		//Unity Matrix
		//m00  m01  m02  m03
		//m10  m11  m12  m13
		//m20  m21  m22  m23
		//m30  m31  m32  m33


		axis = Vector3.zero;
		float trace = mV[0,0] + mV[1,1] + mV[2,2]; 
		float cosTheta = 0.5f*(trace - 1.0f);
		angle = Mathf.Acos( cosTheta );

		// angle is zero, axis can be anything
		if ( ML.Util.IsZero( angle ) )
		{
			axis = Vector3.right;
		}
		// standard case
		else if ( angle < Mathf.PI - ML.Util.kEpsilon)
		{
			axis = new Vector3( mV[2,1]-mV[1,2], mV[0,2]-mV[2,0], mV[1,0]-mV[0,1] );
			axis.Normalize();
		}
		// angle is 180 degrees
		else
		{
			int i = 0;
			if ( mV[1,1] > mV[0,0] )
				i = 1;
			if ( mV[2,2] > mV[i,i] )
				i = 2;
			int j = (i+1)%3;
			int k = (j+1)%3;
			float s = Mathf.Sqrt( mV[i,i] - mV[j,j] - mV[k,k] + 1.0f );
			axis[i] = 0.5f*s;
			float recip = 1.0f/s;
			axis[j] = (mV[i,j])*recip;
			axis[k] = (mV[k,i])*recip;
		}

	}  // End of IvMatrix44::GetAxisAngle()


	//-------------------------------------------------------------------------------
	// @ IvMatrix44::GetMatrix()
	//-------------------------------------------------------------------------------
	// Set as rotation matrix based on quaternion
	//-------------------------------------------------------------------------------
	static public Matrix4x4 GetMatrix(  IvQuat rotate )
	{

		//IvMatrixx - i + 4 * j = [i,j]
		// 0  4  8   12
		// 1  5  9   13
		// 2  6  10  14
		// 3  7  11  15


		//Unity Matrix
		//m00  m01  m02  m03
		//m10  m11  m12  m13
		//m20  m21  m22  m23
		//m30  m31  m32  m33


		//ASSERT( rotate.IsUnit() );

		float xs, ys, zs, wx, wy, wz, xx, xy, xz, yy, yz, zz;

		xs = rotate.x+rotate.x;   
		ys = rotate.y+rotate.y;
		zs = rotate.z+rotate.z;
		wx = rotate.w*xs;
		wy = rotate.w*ys;
		wz = rotate.w*zs;
		xx = rotate.x*xs;
		xy = rotate.x*ys;
		xz = rotate.x*zs;
		yy = rotate.y*ys;
		yz = rotate.y*zs;
		zz = rotate.z*zs;

		Matrix4x4 mV = Matrix4x4.zero;

		mV[0,0] = 1.0f - (yy + zz);
		mV[0,1] = xy - wz;
		mV[0,2] = xz + wy;
		mV[0,3] = 0.0f;

		mV[1,0] = xy + wz;
		mV[1,1] = 1.0f - (xx + zz);
		mV[1,2] = yz - wx;
		mV[1,3] = 0.0f;

		mV[2,0] = xz - wy;
		mV[2,1] = yz + wx;
		mV[2,2] = 1.0f - (xx + yy);
		mV[2,3] = 0.0f;

		mV[3,0] = 0.0f;
		mV[3,1] = 0.0f;
		mV[3,2] = 0.0f;
		mV[3,3] = 1.0f;

		return mV;

	}   // End of Rotation()

	static public Matrix4x4 GetMatrix( ref Quaternion rotate )
	{
		IvQuat ivQ = new IvQuat (rotate.w, rotate.x, rotate.y, rotate.z);
		return IvQuat.GetMatrix ( ivQ);
	}

	/// <summary>
	/// Gets the matrix.
	/// </summary>
	/// <returns>The matrix.</returns>
	/// <param name="fixedAngles">Fixed angles. Degree value </param>
	static public Matrix4x4 GetMatrix(Vector3 fixedAngles)
	{
		IvQuat ivQ = new IvQuat ();
		ivQ.Set (fixedAngles.z * Mathf.Deg2Rad, fixedAngles.y * Mathf.Deg2Rad, fixedAngles.x * Mathf.Deg2Rad);
		return IvQuat.GetMatrix (ivQ);
	}

	public Matrix4x4 GetMatrix()
	{
		return IvQuat.GetMatrix ( this);
	}
}





