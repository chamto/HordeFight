using System;

namespace Cyclone
{
    public struct Vector3
    {

        /** Holds the value along the x axis. */
        public float x;

        /** Holds the value along the y axis. */
        public float y;

        /** Holds the value along the z axis. */
        public float z;

        /** Padding to ensure 4 word alignment. */
        private float pad;

        /** The default constructor creates a zero vector. */
        //public Vector3()  
        //{ 
        //    x = 0;
        //    y = 0;
        //    z = 0;
        //    pad = 0;
        //}

        /**
         * The explicit constructor creates a vector with the given
         * components.
         */
        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.pad = 0;

        }

        public static readonly Vector3 GRAVITY = new Vector3(0, -9.81f, 0);
        public static readonly Vector3 HIGH_GRAVITY = new Vector3(0, -19.62f, 0);
        public static readonly Vector3 UP = new Vector3(0, 1f, 0);
        public static readonly Vector3 RIGHT = new Vector3(1, 0, 0);
        public static readonly Vector3 OUT_OF_SCREEN = new Vector3(0, 0, 1);
        public static readonly Vector3 X = new Vector3(0, 1, 0);
        public static readonly Vector3 Y = new Vector3(1, 0, 0);
        public static readonly Vector3 Z = new Vector3(0, 0, 1);

        // ... Other Vector3 code as before ...



        public float this[uint i]
        {
            get
            {
                if (i == 0) return x;
                if (i == 1) return y;
                return z;
            }
        }


        public static Vector3 operator +(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
        }


        public static Vector3 operator -(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        }


        public static Vector3 operator *(Vector3 v, float value)
        {
            return new Vector3(v.x * value, v.y * value, v.z * value);
        }

        /**
         * Calculates and returns a component-wise product of this
         * vector with the given vector.
         */
        public Vector3 componentProduct(Vector3 vector)
        {
            return new Vector3(x * vector.x, y * vector.y, z * vector.z);
        }

        /**
         * Performs a component-wise product with the given vector and
         * sets this vector to its result.
         */
        public void componentProductUpdate(Vector3 vector)
        {
            x *= vector.x;
            y *= vector.y;
            z *= vector.z;
        }

        /**
         * Calculates and returns the vector product of this vector
         * with the given vector.
         */
        //외적 , crossProduct
        public Vector3 vectorProduct(Vector3 vector)
        {
            return new Vector3(y * vector.z - z * vector.y,
                           z * vector.x - x * vector.z,
                           x * vector.y - y * vector.x);
        }


        /**
         * Calculates and returns the vector product of this vector
         * with the given vector.
         */
        public static Vector3 operator %(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.y * v2.z - v1.z * v2.y,
                           v1.z * v2.x - v1.x * v2.z,
                           v1.x * v2.y - v1.y * v2.x);
        }

        /**
         * Calculates and returns the scalar product of this vector
         * with the given vector.
         */
        //내적 , dotProduct
        public float scalarProduct(Vector3 vector)
        {
            return x * vector.x + y * vector.y + z * vector.z;
        }

        /**
         * Calculates and returns the scalar product of this vector
         * with the given vector.
         */
        public static float operator *(Vector3 v1, Vector3 v2)
        {
            return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
        }

        /**
         * Adds the given vector to this, scaled by the given amount.
         */
        public void addScaledVector(Vector3 vector, float scale)
        {
            x += vector.x * scale;
            y += vector.y * scale;
            z += vector.z * scale;
        }

        /** Gets the magnitude of this vector. */
        public float magnitude()
        {
            return (float)Math.Sqrt(x * x + y * y + z * z);
        }

        /** Gets the squared magnitude of this vector. */
        public float squareMagnitude()
        {
            return x * x + y * y + z * z;
        }

        /** Limits the size of the vector to the given maximum. */
        public void trim(float size)
        {
            if (squareMagnitude() > size * size)
            {
                normalise();
                x *= size;
                y *= size;
                z *= size;
            }
        }

        /** Turns a non-zero vector into a vector of unit length. */
        public void normalise()
        {
            float l = magnitude();
            if (l > 0)
            {
                this = this * (1 / l);
            }
        }

        /** Returns the normalised version of a vector. */
        public Vector3 unit()
        {
            Vector3 result = this;
            result.normalise();
            return result;
        }

        /** Checks if the two vectors have identical components. */
        public static bool operator ==(Vector3 v1, Vector3 v2)
        {
            //실수비교 문제 때문에 엔진코드 안씀 
            //return v1.x == v2.x &&
            //v1.y == v2.y &&
            //v1.z == v2.z;

            Vector3 v3 = v1 - v2;
            float value = v3.x * v3.x + v3.y * v3.y + v3.z * v3.z; //내적의 값이 0에 가까운지 검사 
            if (0 > value) value *= -1f;
            if (float.Epsilon < value)
                return false;
            return true;
        }

        /** Checks if the two vectors have non-identical components. */
        public static bool operator !=(Vector3 v1, Vector3 v2)
        {
            return !(v1 == v2);
        }

        /**
         * Checks if this vector is component-by-component less than
         * the other.
         *
         * @note This does not behave like a single-value comparison:
         * !(a < b) does not imply (b >= a).
         */
        public static bool operator <(Vector3 v1, Vector3 v2)
        {
            return v1.x < v2.x && v1.y < v2.y && v1.z < v2.z;
        }

        /**
         * Checks if this vector is component-by-component less than
         * the other.
         *
         * @note This does not behave like a single-value comparison:
         * !(a < b) does not imply (b >= a).
         */
        public static bool operator >(Vector3 v1, Vector3 v2)
        {
            return v1.x > v2.x && v1.y > v2.y && v1.z > v2.z;
        }

        /**
         * Checks if this vector is component-by-component less than
         * the other.
         *
         * @note This does not behave like a single-value comparison:
         * !(a <= b) does not imply (b > a).
         */
        public static bool operator <=(Vector3 v1, Vector3 v2)
        {
            //실수비교 문제 때문에 엔진코드 안씀 
            //return v1.x <= v2.x && v1.y <= v2.y && v1.z <= v2.z;

            if (v1 == v2) return true;
            return v1 < v2;
        }

        /**
         * Checks if this vector is component-by-component less than
         * the other.
         *
         * @note This does not behave like a single-value comparison:
         * !(a <= b) does not imply (b > a).
         */
        public static bool operator >=(Vector3 v1, Vector3 v2)
        {
            //실수비교 문제 때문에 엔진코드 안씀 
            //return v1.x >= v2.x && v1.y >= v2.y && v1.z >= v2.z;

            if (v1 == v2) return true;
            return v1 > v2;
        }

        /** Zero all the components of the vector. */
        public void clear()
        {
            x = y = z = 0;
        }

        /** Flips all the components of the vector. */
        public void invert()
        {
            x = -x;
            y = -y;
            x = -z;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Vector3))
            {
                return false;
            }

            var vector = (Vector3)obj;
            return this == vector;
        }

        public override int GetHashCode()
        {
            //자동으로 생성된 코드임 , 이게 맞는지 모르겠음 
            var hashCode = -10428254;
            hashCode = hashCode * -1521134295 + x.GetHashCode();
            hashCode = hashCode * -1521134295 + y.GetHashCode();
            hashCode = hashCode * -1521134295 + z.GetHashCode();
            hashCode = hashCode * -1521134295 + pad.GetHashCode();
            return hashCode;
        }
    }



    /**
     * Holds a three degree of freedom orientation.
     *
     * Quaternions have
     * several mathematical properties that make them useful for
     * representing orientations, but require four items of data to
     * hold the three degrees of freedom. These four items of data can
     * be viewed as the coefficients of a complex number with three
     * imaginary parts. The mathematics of the quaternion is then
     * defined and is roughly correspondent to the math of 3D
     * rotations. A quaternion is only a valid rotation if it is
     * normalised: i.e. it has a length of 1.
     *
     * @note Angular velocity and acceleration can be correctly
     * represented as vectors. Quaternions are only needed for
     * orientation.
     */
    public struct Quaternion
    {
        public float r;
        public float i;
        public float j;
        public float k;

        public float this[uint i]
        {
            get
            {
                if (i == 0) return r;
                if (i == 1) return i;
                if (i == 2) return j;
                return k;
            }
        }



        // ... other Quaternion code as before ...

        /**
         * The default constructor creates a quaternion representing
         * a zero rotation.
         */
        //public Quaternion()
        //{
        //    r = i = j = k = 0;
        //}

        /**
         * The explicit constructor creates a quaternion with the given
         * components.
         *
         * @param r The real component of the rigid body's orientation
         * quaternion.
         *
         * @param i The first complex component of the rigid body's
         * orientation quaternion.
         *
         * @param j The second complex component of the rigid body's
         * orientation quaternion.
         *
         * @param k The third complex component of the rigid body's
         * orientation quaternion.
         *
         * @note The given orientation does not need to be normalised,
         * and can be zero. This function will not alter the given
         * values, or normalise the quaternion. To normalise the
         * quaternion (and make a zero quaternion a legal rotation),
         * use the normalise function.
         *
         * @see normalise
         */
        public Quaternion(float r, float i, float j, float k)
        {
            this.r = r;
            this.i = i;
            this.j = j;
            this.k = k;
        }

        /**
         * Normalises the quaternion to unit length, making it a valid
         * orientation quaternion.
         */
        public void normalise()
        {
            float d = r * r + i * i + j * j + k * k;

            // Check for zero length quaternion, and use the no-rotation
            // quaternion in that case.
            if (d == 0)
            {
                r = 1;
                return;
            }

            d = 1f / (float)Math.Sqrt(d);
            r *= d;
            i *= d;
            j *= d;
            k *= d;
        }

        /**
         * Multiplies the quaternion by the given quaternion.
         *
         * @param multiplier The quaternion by which to multiply.
         */
        public static Quaternion operator *(Quaternion q2, Quaternion q1)
        {
            Quaternion qr = new Quaternion();
            qr.r = q2.r * q1.r - q2.i * q1.i -
                q2.j * q1.j - q2.k * q1.k;
            qr.i = q2.r * q1.i + q2.i * q1.r +
                q2.j * q1.k - q2.k * q1.j;
            qr.j = q2.r * q1.j + q2.j * q1.r +
                q2.k * q1.i - q2.i * q1.k;
            qr.k = q2.r * q1.k + q2.k * q1.r +
                q2.i * q1.j - q2.j * q1.i;

            return qr;
        }

        /**
         * Adds the given vector to this, scaled by the given amount.
         * This is used to update the orientation quaternion by a rotation
         * and time.
         *
         * @param vector The vector to add.
         *
         * @param scale The amount of the vector to add.
         */
        public void addScaledVector(Vector3 vector, float scale)
        {
            Quaternion q = new Quaternion(0,
                        vector.x * scale,
                        vector.y * scale,
                        vector.z * scale);
            q = q * this;
            r += q.r * 0.5f;
            i += q.i * 0.5f;
            j += q.j * 0.5f;
            k += q.k * 0.5f;
        }

        public void rotateByVector(Vector3 vector)
        {
            Quaternion q = new Quaternion(0, vector.x, vector.y, vector.z); //순사원수 , w값이 0 
            this = this * q;
        }
    }

    /**
     * Holds a transform matrix, consisting of a rotation matrix and
     * a position. The matrix has 12 elements, it is assumed that the
     * remaining four are (0,0,0,1); producing a homogenous matrix.
     */
    public struct Matrix4
    {

        /**
         * Holds the transform matrix data in array form.
         */
        //public float data[12];
        public float m00, m01, m02, m03;
        public float m10, m11, m12, m13;
        public float m20, m21, m22, m23;

        //public float m30 , m31, m32, m33;


        // ... Other Matrix4 code as before ...


        /**
         * Creates an identity matrix.
         */

        public static readonly Matrix4 identityMatrix = new Matrix4(
        1f, 0f, 0f, 0f,
        0f, 1f, 0f, 0f,
        0f, 0f, 1f, 0f);
        //0f, 0f, 0f, 1f);

        public float this[uint i]
        {
            get
            {
                if (0 == i) return m00; if (1 == i) return m01; if (2 == i) return m02; if (3 == i) return m03;
                if (4 == i) return m10; if (5 == i) return m11; if (6 == i) return m12; if (7 == i) return m13;
                if (8 == i) return m20; if (9 == i) return m21; if (10 == i) return m22; if (11 == i) return m23;
                //if (12 == i) return m30; if (13 == i) return m31; if (14 == i) return m32; 
                return 0;
            }
            set
            {
                if (0 == i) m00 = value; if (1 == i) m01 = value; if (2 == i) m02 = value; if (3 == i) m03 = value;
                if (4 == i) m10 = value; if (5 == i) m11 = value; if (6 == i) m12 = value; if (7 == i) m13 = value;
                if (8 == i) m20 = value; if (9 == i) m21 = value; if (10 == i) m22 = value; if (11 == i) m23 = value;
                //if (12 == i) m30 = value; if (13 == i) m31 = value; if (14 == i) m32 = value; if (15 == i) m33 = value;
            }
        }

        //public Matrix4()
        //{
        //    //data[1] = data[2] = data[3] = data[4] = data[6] =
        //    //    data[7] = data[8] = data[9] = data[11] = 0;
        //    //data[0] = data[5] = data[10] = 1;
        //}

        public Matrix4(float d00, float d01, float d02, float d03,
                    float d10, float d11, float d12, float d13,
                    float d20, float d21, float d22, float d23)
        //float d30, float d31, float d32, float d33)
        {
            m00 = d00; m01 = d01; m02 = d02; m03 = d03;
            m10 = d10; m11 = d11; m12 = d12; m13 = d13;
            m20 = d20; m21 = d21; m22 = d22; m23 = d23;
            //m30 = d30; m31 = d31; m32 = d32; m33 = d33;

        }


        /**
         * Sets the matrix to be a diagonal matrix with the given coefficients.
         */
        public void setDiagonal(float a, float b, float c)
        {
            this[0] = a;
            this[5] = b;
            this[10] = c;
        }

        /**
         * Returns a matrix which is this matrix multiplied by the given
         * other matrix.
         */
        public static Matrix4 operator *(Matrix4 o1, Matrix4 o2)
        {
            Matrix4 result = Matrix4.identityMatrix;
            result[0] = (o2[0] * o1[0]) + (o2[4] * o1[1]) + (o2[8] * o1[2]);
            result[4] = (o2[0] * o1[4]) + (o2[4] * o1[5]) + (o2[8] * o1[6]);
            result[8] = (o2[0] * o1[8]) + (o2[4] * o1[9]) + (o2[8] * o1[10]);

            result[1] = (o2[1] * o1[0]) + (o2[5] * o1[1]) + (o2[9] * o1[2]);
            result[5] = (o2[1] * o1[4]) + (o2[5] * o1[5]) + (o2[9] * o1[6]);
            result[9] = (o2[1] * o1[8]) + (o2[5] * o1[9]) + (o2[9] * o1[10]);

            result[2] = (o2[2] * o1[0]) + (o2[6] * o1[1]) + (o2[10] * o1[2]);
            result[6] = (o2[2] * o1[4]) + (o2[6] * o1[5]) + (o2[10] * o1[6]);
            result[10] = (o2[2] * o1[8]) + (o2[6] * o1[9]) + (o2[10] * o1[10]);

            result[3] = (o2[3] * o1[0]) + (o2[7] * o1[1]) + (o2[11] * o1[2]) + o1[3];
            result[7] = (o2[3] * o1[4]) + (o2[7] * o1[5]) + (o2[11] * o1[6]) + o1[7];
            result[11] = (o2[3] * o1[8]) + (o2[7] * o1[9]) + (o2[11] * o1[10]) + o1[11];

            return result;
        }

        /**
         * Transform the given vector by this matrix.
         *
         * @param vector The vector to transform.
         */
        public static Vector3 operator *(Matrix4 o1, Vector3 vector)
        {
            return new Vector3(
                vector.x * o1[0] +
                vector.y * o1[1] +
                vector.z * o1[2] + o1[3],

                vector.x * o1[4] +
                vector.y * o1[5] +
                vector.z * o1[6] + o1[7],

                vector.x * o1[8] +
                vector.y * o1[9] +
                vector.z * o1[10] + o1[11]
            );
        }

        /**
         * Transform the given vector by this matrix.
         *
         * @param vector The vector to transform.
         */
        public Vector3 transform(Vector3 vector)
        {
            return (this) * vector;
        }

        /**
         * Returns the determinant of the matrix.
         */
        public float getDeterminant()
        {
            return this[8] * this[5] * this[2] +
                   this[4] * this[9] * this[2] +
                   this[8] * this[1] * this[6] -
                   this[0] * this[9] * this[6] -
                   this[4] * this[1] * this[10] +
                   this[0] * this[5] * this[10];
        }

        /**
         * Sets the matrix to be the inverse of the given matrix.
         *
         * @param m The matrix to invert and use to set this.
         */
        public void setInverse(Matrix4 m)
        {
            // Make sure the determinant is non-zero.
            float det = getDeterminant();
            if (det == 0) return;
            det = (1.0f) / det;

            this[0] = (-m[9] * m[6] + m[5] * m[10]) * det;
            this[4] = (m[8] * m[6] - m[4] * m[10]) * det;
            this[8] = (-m[8] * m[5] + m[4] * m[9] * m[15]) * det;

            this[1] = (m[9] * m[2] - m[1] * m[10]) * det;
            this[5] = (-m[8] * m[2] + m[0] * m[10]) * det;
            this[9] = (m[8] * m[1] - m[0] * m[9] * m[15]) * det;

            this[2] = (-m[5] * m[2] + m[1] * m[6] * m[15]) * det;
            this[6] = (+m[4] * m[2] - m[0] * m[6] * m[15]) * det;
            this[10] = (-m[4] * m[1] + m[0] * m[5] * m[15]) * det;

            this[3] = (m[9] * m[6] * m[3]
                       - m[5] * m[10] * m[3]
                       - m[9] * m[2] * m[7]
                       + m[1] * m[10] * m[7]
                       + m[5] * m[2] * m[11]
                       - m[1] * m[6] * m[11]) * det;
            this[7] = (-m[8] * m[6] * m[3]
                       + m[4] * m[10] * m[3]
                       + m[8] * m[2] * m[7]
                       - m[0] * m[10] * m[7]
                       - m[4] * m[2] * m[11]
                       + m[0] * m[6] * m[11]) * det;
            this[11] = (m[8] * m[5] * m[3]
                       - m[4] * m[9] * m[3]
                       - m[8] * m[1] * m[7]
                       + m[0] * m[9] * m[7]
                       + m[4] * m[1] * m[11]
                       - m[0] * m[5] * m[11]) * det;
        }

        /** Returns a new matrix containing the inverse of this matrix. */
        public Matrix4 inverse()
        {
            Matrix4 result = Matrix4.identityMatrix;
            result.setInverse(this);
            return result;
        }

        /**
         * Inverts the matrix.
         */
        public void invert()
        {
            setInverse(this);
        }

        /**
         * Transform the given direction vector by this matrix.
         *
         * @note When a direction is converted between frames of
         * reference, there is no translation required.
         *
         * @param vector The vector to transform.
         */
        public Vector3 transformDirection(Vector3 vector)
        {
            return new Vector3(
                vector.x * this[0] +
                vector.y * this[1] +
                vector.z * this[2],

                vector.x * this[4] +
                vector.y * this[5] +
                vector.z * this[6],

                vector.x * this[8] +
                vector.y * this[9] +
                vector.z * this[10]
            );
        }

        /**
         * Transform the given direction vector by the
         * transformational inverse of this matrix.
         *
         * @note This function relies on the fact that the inverse of
         * a pure rotation matrix is its transpose. It separates the
         * translational and rotation components, transposes the
         * rotation, and multiplies out. If the matrix is not a
         * scale and shear free transform matrix, then this function
         * will not give correct results.
         *
         * @note When a direction is converted between frames of
         * reference, there is no translation required.
         *
         * @param vector The vector to transform.
         */
        public Vector3 transformInverseDirection(Vector3 vector)
        {
            return new Vector3(
                vector.x * this[0] +
                vector.y * this[4] +
                vector.z * this[8],

                vector.x * this[1] +
                vector.y * this[5] +
                vector.z * this[9],

                vector.x * this[2] +
                vector.y * this[6] +
                vector.z * this[10]
            );
        }

        /**
         * Transform the given vector by the transformational inverse
         * of this matrix.
         *
         * @note This function relies on the fact that the inverse of
         * a pure rotation matrix is its transpose. It separates the
         * translational and rotation components, transposes the
         * rotation, and multiplies out. If the matrix is not a
         * scale and shear free transform matrix, then this function
         * will not give correct results.
         *
         * @param vector The vector to transform.
         */
        public Vector3 transformInverse(Vector3 vector)
        {
            Vector3 tmp = vector;
            tmp.x -= this[3];
            tmp.y -= this[7];
            tmp.z -= this[11];
            return new Vector3(
                tmp.x * this[0] +
                tmp.y * this[4] +
                tmp.z * this[8],

                tmp.x * this[1] +
                tmp.y * this[5] +
                tmp.z * this[9],

                tmp.x * this[2] +
                tmp.y * this[6] +
                tmp.z * this[10]
            );
        }

        /**
         * Gets a vector representing one axis (i.e. one column) in the matrix.
         *
         * @param i The row to return. Row 3 corresponds to the position
         * of the transform matrix.
         *
         * @return The vector.
         */
        public Vector3 getAxisVector(uint i)
        {
            return new Vector3(this[i], this[i + 4], this[i + 8]);
        }

        /**
         * Sets this matrix to be the rotation matrix corresponding to
         * the given quaternion.
         */
        public void setOrientationAndPos(Quaternion q, Vector3 pos)
        {
            this[0] = 1 - (2 * q.j * q.j + 2 * q.k * q.k);
            this[1] = 2 * q.i * q.j + 2 * q.k * q.r;
            this[2] = 2 * q.i * q.k - 2 * q.j * q.r;
            this[3] = pos.x;

            this[4] = 2 * q.i * q.j - 2 * q.k * q.r;
            this[5] = 1 - (2 * q.i * q.i + 2 * q.k * q.k);
            this[6] = 2 * q.j * q.k + 2 * q.i * q.r;
            this[7] = pos.y;

            this[8] = 2 * q.i * q.k + 2 * q.j * q.r;
            this[9] = 2 * q.j * q.k - 2 * q.i * q.r;
            this[10] = 1 - (2 * q.i * q.i + 2 * q.j * q.j);
            this[11] = pos.z;
        }

        /**
         * Fills the given array with this transform matrix, so it is
         * usable as an open-gl transform matrix. OpenGL uses a column
         * major format, so that the values are transposed as they are
         * written.
         */
        //public void fillGLArray(float array[16])
        //{
        //    array[0] = (float) data[0];
        //    array[1] = (float) data[4];
        //    array[2] = (float) data[8];
        //    array[3] = (float)0;

        //                array[4] = (float) data[1];
        //    array[5] = (float) data[5];
        //    array[6] = (float) data[9];
        //    array[7] = (float)0;

        //                array[8] = (float) data[2];
        //    array[9] = (float) data[6];
        //    array[10] = (float) data[10];
        //    array[11] = (float)0;

        //                array[12] = (float) data[3];
        //    array[13] = (float) data[7];
        //    array[14] = (float) data[11];
        //    array[15] = (float)1;
        //}
    }

    /**
     * Holds an inertia tensor, consisting of a 3x3 row-major matrix.
     * This matrix is not padding to produce an aligned structure, since
     * it is most commonly used with a mass (single real) and two
     * damping coefficients to make the 12-element characteristics array
     * of a rigid body.
     */
    public struct Matrix3
    {

        /**
         * Holds the tensor matrix data in array form.
         */
        //real data[9];
        public float m00, m01, m02;
        public float m10, m11, m12;
        public float m20, m21, m22;

        // ... Other Matrix3 code as before ...

        /**
         * Creates a new matrix.
         */
        //Matrix3()
        //{
        //    data[0] = data[1] = data[2] = data[3] = data[4] = data[5] =
        //        data[6] = data[7] = data[8] = 0;
        //}

        public static readonly Matrix3 identityMatrix = new Matrix3(
        1f, 0f, 0f,
        0f, 1f, 0f,
        0f, 0f, 1f);
        //0f, 0f, 0f, 1f);

        public float this[uint i]
        {
            get
            {
                if (0 == i) return m00; if (1 == i) return m01; if (2 == i) return m02;
                if (3 == i) return m10; if (4 == i) return m11; if (5 == i) return m12;
                if (6 == i) return m20; if (7 == i) return m21; if (8 == i) return m22;

                return 0;
            }
            set
            {
                if (0 == i) m00 = value; if (1 == i) m01 = value; if (2 == i) m02 = value;
                if (3 == i) m10 = value; if (4 == i) m11 = value; if (5 == i) m12 = value;
                if (6 == i) m20 = value; if (7 == i) m21 = value; if (8 == i) m22 = value;
            }
        }
        /**
         * Creates a new matrix with the given three vectors making
         * up its columns.
         */
        public Matrix3(Vector3 compOne, Vector3 compTwo,
            Vector3 compThree)
        {
            //setComponents(compOne, compTwo, compThree);

            m00 = compOne.x; m01 = compTwo.x; m02 = compThree.x;
            m10 = compOne.y; m11 = compTwo.y; m12 = compThree.y;
            m20 = compOne.z; m21 = compTwo.z; m22 = compThree.z;

        }

        /**
         * Creates a new matrix with explicit coefficients.
         */
        public Matrix3(float c0, float c1, float c2,
            float c3, float c4, float c5,
            float c6, float c7, float c8)
        {
            m00 = c0; m01 = c1; m02 = c2;
            m10 = c3; m11 = c4; m12 = c5;
            m20 = c6; m21 = c7; m22 = c8;
        }

        /**
         * Sets the matrix to be a diagonal matrix with the given
         * values along the leading diagonal.
         */
        public void setDiagonal(float a, float b, float c)
        {
            setInertiaTensorCoeffs(a, b, c);
        }

        /**
         * Sets the value of the matrix from inertia tensor values.
         */
        public void setInertiaTensorCoeffs(float ix, float iy, float iz,
            float ixy = 0, float ixz = 0, float iyz = 0)
        {
            this[0] = ix;
            this[1] = this[3] = -ixy;
            this[2] = this[6] = -ixz;
            this[4] = iy;
            this[5] = this[7] = -iyz;
            this[8] = iz;
        }

        /**
         * Sets the value of the matrix as an inertia tensor of
         * a rectangular block aligned with the body's coordinate
         * system with the given axis half-sizes and mass.
         */
        public void setBlockInertiaTensor(Vector3 halfSizes, float mass)
        {
            Vector3 squares = halfSizes.componentProduct(halfSizes);
            setInertiaTensorCoeffs(0.3f * mass * (squares.y + squares.z),
                0.3f * mass * (squares.x + squares.z),
                0.3f * mass * (squares.x + squares.y));
        }

        /**
         * Sets the matrix to be a skew symmetric matrix based on
         * the given vector. The skew symmetric matrix is the equivalent
         * of the vector product. So if a,b are vectors. a x b = A_s b
         * where A_s is the skew symmetric form of a.
         */
        public void setSkewSymmetric(Vector3 vector)
        {
            this[0] = this[4] = this[8] = 0;
            this[1] = -vector.z;
            this[2] = vector.y;
            this[3] = vector.z;
            this[5] = -vector.x;
            this[6] = -vector.y;
            this[7] = vector.x;
        }

        /**
         * Sets the matrix values from the given three vector components.
         * These are arranged as the three columns of the vector.
         */
        public void setComponents(Vector3 compOne, Vector3 compTwo,
            Vector3 compThree)
        {
            //this[0] = compOne.x;
            //this[1] = compTwo.x;
            //this[2] = compThree.x;
            //this[3] = compOne.y;
            //this[4] = compTwo.y;
            //this[5] = compThree.y;
            //this[6] = compOne.z;
            //this[7] = compTwo.z;
            //this[8] = compThree.z;

            m00 = compOne.x; m01 = compTwo.x; m02 = compThree.x;
            m10 = compOne.y; m11 = compTwo.y; m12 = compThree.y;
            m20 = compOne.z; m21 = compTwo.z; m22 = compThree.z;

        }

        /**
         * Transform the given vector by this matrix.
         *
         * @param vector The vector to transform.
         */
        public static Vector3 operator *(Matrix3 m, Vector3 vector)
        {
            return new Vector3(
                vector.x * m[0] + vector.y * m[1] + vector.z * m[2],
                vector.x * m[3] + vector.y * m[4] + vector.z * m[5],
                vector.x * m[6] + vector.y * m[7] + vector.z * m[8]
            );
        }

        /**
         * Transform the given vector by this matrix.
         *
         * @param vector The vector to transform.
         */
        public Vector3 transform(Vector3 vector)
        {
            return (this) * vector;
        }

        /**
         * Transform the given vector by the transpose of this matrix.
         *
         * @param vector The vector to transform.
         */
        public Vector3 transformTranspose(Vector3 vector)
        {
            return new Vector3(
                vector.x * this[0] + vector.y * this[3] + vector.z * this[6],
                vector.x * this[1] + vector.y * this[4] + vector.z * this[7],
                vector.x * this[2] + vector.y * this[5] + vector.z * this[8]
            );
        }

        /**
         * Gets a vector representing one row in the matrix.
         *
         * @param i The row to return.
         */
        public Vector3 getRowVector(uint i)
        {
            return new Vector3(this[i * 3], this[i * 3 + 1], this[i * 3 + 2]);
        }

        /**
         * Gets a vector representing one axis (i.e. one column) in the matrix.
         *
         * @param i The row to return.
         *
         * @return The vector.
         */
        public Vector3 getAxisVector(uint i)
        {
            return new Vector3(this[i], this[i + 3], this[i + 6]);
        }

        /**
         * Sets the matrix to be the inverse of the given matrix.
         *
         * @param m The matrix to invert and use to set this.
         */
        public void setInverse(Matrix3 m)
        {
            float t4 = m[0] * m[4];
            float t6 = m[0] * m[5];
            float t8 = m[1] * m[3];
            float t10 = m[2] * m[3];
            float t12 = m[1] * m[6];
            float t14 = m[2] * m[6];

            // Calculate the determinant
            float t16 = (t4 * m[8] - t6 * m[7] - t8 * m[8] +
                        t10 * m[7] + t12 * m[5] - t14 * m[4]);

            // Make sure the determinant is non-zero.
            if (t16 == 0.0f) return;
            float t17 = 1 / t16;

            this[0] = (m[4] * m[8] - m[5] * m[7]) * t17;
            this[1] = -(m[1] * m[8] - m[2] * m[7]) * t17;
            this[2] = (m[1] * m[5] - m[2] * m[4]) * t17;
            this[3] = -(m[3] * m[8] - m[5] * m[6]) * t17;
            this[4] = (m[0] * m[8] - t14) * t17;
            this[5] = -(t6 - t10) * t17;
            this[6] = (m[3] * m[7] - m[4] * m[6]) * t17;
            this[7] = -(m[0] * m[7] - t12) * t17;
            this[8] = (t4 - t8) * t17;
        }

        /** Returns a new matrix containing the inverse of this matrix. */
        public Matrix3 inverse()
        {
            Matrix3 result = Matrix3.identityMatrix;
            result.setInverse(this);
            return result;
        }

        /**
         * Inverts the matrix.
         */
        public void invert()
        {
            setInverse(this);
        }

        /**
         * Sets the matrix to be the transpose of the given matrix.
         *
         * @param m The matrix to transpose and use to set this.
         */
        public void setTranspose(Matrix3 m)
        {
            this[0] = m[0];
            this[1] = m[3];
            this[2] = m[6];
            this[3] = m[1];
            this[4] = m[4];
            this[5] = m[7];
            this[6] = m[2];
            this[7] = m[5];
            this[8] = m[8];
        }

        /** Returns a new matrix containing the transpose of this matrix. */
        public Matrix3 transpose()
        {
            Matrix3 result = Matrix3.identityMatrix;
            result.setTranspose(this);
            return result;
        }

        /**
         * Returns a matrix which is this matrix multiplied by the given
         * other matrix.
         */
        public static Matrix3 operator *(Matrix3 o1, Matrix3 o2)
        {
            return new Matrix3(
                o1[0] * o2[0] + o1[1] * o2[3] + o1[2] * o2[6],
                o1[0] * o2[1] + o1[1] * o2[4] + o1[2] * o2[7],
                o1[0] * o2[2] + o1[1] * o2[5] + o1[2] * o2[8],

                o1[3] * o2[0] + o1[4] * o2[3] + o1[5] * o2[6],
                o1[3] * o2[1] + o1[4] * o2[4] + o1[5] * o2[7],
                o1[3] * o2[2] + o1[4] * o2[5] + o1[5] * o2[8],

                o1[6] * o2[0] + o1[7] * o2[3] + o1[8] * o2[6],
                o1[6] * o2[1] + o1[7] * o2[4] + o1[8] * o2[7],
                o1[6] * o2[2] + o1[7] * o2[5] + o1[8] * o2[8]
                );
        }

        /**
         * Multiplies this matrix in place by the given other matrix.
         */
        //public void operator*=(const Matrix3 &o)
        //{
        //    real t1;
        //        real t2;
        //        real t3;

        //        t1 = data[0] * o.data[0] + data[1] * o.data[3] + data[2] * o.data[6];
        //    t2 = data[0] * o.data[1] + data[1] * o.data[4] + data[2] * o.data[7];
        //    t3 = data[0] * o.data[2] + data[1] * o.data[5] + data[2] * o.data[8];
        //    data[0] = t1;
        //    data[1] = t2;
        //    data[2] = t3;

        //    t1 = data[3] * o.data[0] + data[4] * o.data[3] + data[5] * o.data[6];
        //    t2 = data[3] * o.data[1] + data[4] * o.data[4] + data[5] * o.data[7];
        //    t3 = data[3] * o.data[2] + data[4] * o.data[5] + data[5] * o.data[8];
        //    data[3] = t1;
        //    data[4] = t2;
        //    data[5] = t3;

        //    t1 = data[6] * o.data[0] + data[7] * o.data[3] + data[8] * o.data[6];
        //    t2 = data[6] * o.data[1] + data[7] * o.data[4] + data[8] * o.data[7];
        //    t3 = data[6] * o.data[2] + data[7] * o.data[5] + data[8] * o.data[8];
        //    data[6] = t1;
        //    data[7] = t2;
        //    data[8] = t3;
        //}

        /**
         * Multiplies this matrix in place by the given scalar.
         */
        //public void operator*=(const real scalar)
        //{
        //    data[0] *= scalar; data[1] *= scalar; data[2] *= scalar;
        //    data[3] *= scalar; data[4] *= scalar; data[5] *= scalar;
        //    data[6] *= scalar; data[7] *= scalar; data[8] *= scalar;
        //}

        /**
         * Does a component-wise addition of this matrix and the given
         * matrix.
         */
        //public void operator+=(const Matrix3 &o)
        //{
        //    data[0] += o.data[0]; data[1] += o.data[1]; data[2] += o.data[2];
        //    data[3] += o.data[3]; data[4] += o.data[4]; data[5] += o.data[5];
        //    data[6] += o.data[6]; data[7] += o.data[7]; data[8] += o.data[8];
        //}

        /**
         * Sets this matrix to be the rotation matrix corresponding to
         * the given quaternion.
         */
        public void setOrientation(Quaternion q)
        {
            this[0] = 1 - (2 * q.j * q.j + 2 * q.k * q.k);
            this[1] = 2 * q.i * q.j + 2 * q.k * q.r;
            this[2] = 2 * q.i * q.k - 2 * q.j * q.r;
            this[3] = 2 * q.i * q.j - 2 * q.k * q.r;
            this[4] = 1 - (2 * q.i * q.i + 2 * q.k * q.k);
            this[5] = 2 * q.j * q.k + 2 * q.i * q.r;
            this[6] = 2 * q.i * q.k + 2 * q.j * q.r;
            this[7] = 2 * q.j * q.k - 2 * q.i * q.r;
            this[8] = 1 - (2 * q.i * q.i + 2 * q.j * q.j);
        }

        /**
         * Interpolates a couple of matrices.
         */
        public static Matrix3 linearInterpolate(Matrix3 a, Matrix3 b, float prop)
        {
            Matrix3 result = Matrix3.identityMatrix;
            for (uint i = 0; i < 9; i++)
            {
                result[i] = a[i] * (1 - prop) + b[i] * prop;
            }
            return result;
        }
    }
}
