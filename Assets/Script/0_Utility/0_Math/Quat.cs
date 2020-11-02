using System;
using UnityEngine;
using UtilGS9;

namespace UtilGS9
{
   
    public struct Quat
    {
        public float x, y, z, w;

        internal void Set(float v, object p1, object p2, object p3)
        {
            throw new NotImplementedException();
        }

        public void Set(float ww, float xx, float yy, float zz)
        {
            w = ww; x = xx; y = yy; z = zz;
        }

        public void Identity()
        {
            x = y = z = 0.0f;
            w = 1.0f;
        }

        public void Normalize()
        {
            float lengthsq = w * w + x * x + y * y + z * z;

            if (Misc.IsZero(lengthsq))
            {
                x = y = z = w = 0f;
            }
            else
            {
                float factor = 1f / (float)System.Math.Sqrt(lengthsq);
                w *= factor;
                x *= factor;
                y *= factor;
                z *= factor;
            }

        }

        //angle_rd : 라디안 값 넣어야함 
        public Quaternion AngleAxis(float angle_rd, Vector3 axis)
        {
            // if axis of rotation is zero vector, just set to identity quat
            float sqrLength = axis.sqrMagnitude;
            if (Misc.IsZero(sqrLength))
            {
                Identity();
                return Quaternion.identity;
            }

            // take half-angle
            angle_rd *= 0.5f;

            float sintheta = 0, costheta = 0;
            sintheta = (float)System.Math.Sin(angle_rd);
            costheta = (float)System.Math.Cos(angle_rd);


            float scaleFactor = sintheta / (float)System.Math.Sqrt(sqrLength);

            w = costheta;
            x = scaleFactor * axis.x;
            y = scaleFactor * axis.y;
            z = scaleFactor * axis.z;


            return new Quaternion(x, y, z, w);
        }

        public Quaternion FromToRotation(Vector3 from, Vector3 to)
        {

            // get axis of rotation
            Vector3 axis = Vector3.Cross(from, to);


            // get scaled cos of angle between vectors and set initial quaternion
            Set(Vector3.Dot(from, to), axis.x, axis.y, axis.z);
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
            if (w <= float.Epsilon)
            {
                // rotate pi radians around orthogonal vector
                // take cross product with x axis
                if (from.z * from.z > from.x * from.x)
                    Set(0.0f, 0.0f, from.z, -from.y);
                // or take cross product with z axis
                else
                    Set(0.0f, from.y, -from.x, 0.0f);
            }

            // normalize again to get rotation quaternion
            Normalize();

            return new Quaternion(x, y, z, w);

        }


        public static Quat operator *(Quat a, Quat b)
        {
            Quat quat = new Quat();
            quat.Set(a.w * b.w - a.x * b.x - a.y * b.y - a.z * b.z,
                           a.w * b.x + a.x * b.w + a.y * b.z - a.z * b.y,
                           a.w * b.y + a.y * b.w + a.z * b.x - a.x * b.z,
                           a.w * b.z + a.z * b.w + a.x * b.y - a.y * b.x);
            return quat;

        }

        public bool IsUnit()
        {
            return Misc.IsZero(1.0f - w * w - x * x - y * y - z * z);

        }

        public Vector3 Rotate(Vector3 vector )
        {
            if (true == IsUnit())
                Normalize();

            float vMult = 2.0f*(x*vector.x + y*vector.y + z*vector.z);
            float crossMult = 2.0f*w;
            float pMult = crossMult*w - 1.0f;

            return new Vector3( pMult*vector.x + vMult*x + crossMult*(y*vector.z - z*vector.y),
                              pMult*vector.y + vMult*y + crossMult*(z*vector.x - x*vector.z),
                              pMult*vector.z + vMult*z + crossMult*(x*vector.y - y*vector.x) );

        }  
    }

    //==============
      
}
