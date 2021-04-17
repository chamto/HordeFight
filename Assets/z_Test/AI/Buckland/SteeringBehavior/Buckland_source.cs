using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;


namespace Buckland
{
    public class Const
    {
        //--------------------------- Constants ----------------------------------
        public const float Pi = 3.14159f;
        public const float TwoPi = Pi * 2f;
        public const float HalfPi = Pi / 2f;
        public const float QuarterPi = Pi / 4f;
        //------------------------------------------------------------------------
    }

    public class Util
    {

        public static T MaxOf<T>(T a, T b) where T : IComparable
        {
            if (a.CompareTo(b) > 0) return a;
            return b;
        }

        public static T MinOf<T>(T a, T b) where T : IComparable
        {
            if (a.CompareTo(b) > 0) return b;
            return a;
        }

        //public static Vector2 Perp(Vector2 v2)
        //{
        //    return new Vector2(-v2.y, v2.x);
        //}

        //treats a window as a toroid
        public static Vector2 WrapAround(Vector2 pos, int MaxX, int MaxY)
        {
            if (pos.x > MaxX) { pos.x = 0.0f; }

            if (pos.x < 0) { pos.x = (float)MaxX; }

            if (pos.y < 0) { pos.y = (float)MaxY; }

            if (pos.y > MaxY) { pos.y = 0.0f; }

            return pos;
        }

        //----------------------------- Truncate ---------------------------------
        //
        //  truncates a vector so that its length does not exceed max
        //------------------------------------------------------------------------
        public static Vector2 Truncate(Vector2 v2, float max)
        {
            if (v2.sqrMagnitude > max * max)
            {
                v2.Normalize();

                return v2 *= max;
            }

            return v2;
        }

        //--------------------------- Reflect ------------------------------------
        //
        //  given a normalized vector this method reflects the vector it
        //  is operating upon. (like the path of a ball bouncing off a wall)
        //------------------------------------------------------------------------
        public static Vector2 Reflect(Vector2 v2, Vector2 norm)
        {
            return v2 + 2f * Vector2.Dot(v2, norm) * (-norm);
        }

    //--------------------------- WorldTransform -----------------------------
    //
    //  given a std::vector of 2D vectors, a position, orientation and scale,
    //  this function transforms the 2D vectors into the object's world space
    //------------------------------------------------------------------------
    public static List<Vector2> WorldTransform(List<Vector2> points,
                                             Vector2 pos,
                                             Vector2 forward,
                                             Vector2 side,
                                             Vector2 scale)
        {
            //copy the original vertices into the buffer about to be transformed
            List<Vector2> TranVector2Ds = new List<Vector2>(points);
            //DebugWide.LogBlue(pos + "  " + forward + "  " + side + "   " + scale);
            //create a transformation matrix
            C2DMatrix matTransform = C2DMatrix.identity;

            //scale
            if ((scale.x != 1.0f) || (scale.y != 1.0f))
            {
                matTransform.Scale(scale.x, scale.y);
            }

            //rotate
            matTransform.RotateY(forward, side);

            //and translate
            matTransform.Translate(pos.x, pos.y);

            //now transform the object's vertices
            C2DMatrix.Transform(ref matTransform, ref TranVector2Ds);

            return TranVector2Ds;
        }

        //--------------------------- WorldTransform -----------------------------
        //
        //  given a std::vector of 2D vectors, a position and  orientation
        //  this function transforms the 2D vectors into the object's world space
        //------------------------------------------------------------------------
        public static List<Vector2> WorldTransform(List<Vector2> points,
                                  Vector2 pos,
                                  Vector2 forward,
                                  Vector2 side)
        {
            //copy the original vertices into the buffer about to be transformed
            List<Vector2> TranVector2Ds = new List<Vector2>(points);

            //create a transformation matrix
            C2DMatrix matTransform = C2DMatrix.identity;
            //rotate
            matTransform.RotateY(forward, side);

            //and translate
            matTransform.Translate(pos.x, pos.y);

            //now transform the object's vertices
            C2DMatrix.Transform(ref matTransform, ref TranVector2Ds);

            return TranVector2Ds;
        }

        //--------------------- PointToWorldSpace --------------------------------
        //
        //  Transforms a point from the agent's local space into world space
        //------------------------------------------------------------------------
        public static Vector2 PointToWorldSpace(Vector2 point,
                                     Vector2 AgentHeading,
                                     Vector2 AgentSide,
                                     Vector2 AgentPosition)
        {
            //make a copy of the point
            Vector2 TransPoint = point;

            //create a transformation matrix
            C2DMatrix matTransform = C2DMatrix.identity;

            //rotate
            matTransform.RotateY(AgentHeading, AgentSide);

            //and translate
            matTransform.Translate(AgentPosition.x, AgentPosition.y);

            //now transform the vertices
            C2DMatrix.Transform(ref matTransform, ref point);

            return point;
        }

        //실험노트 20210115 에 분선한 글이 있다. 참고하기 
        public static Vector2 PointToLocalSpace(Vector2 point,
                             Vector2 AgentHeading,
                             Vector2 AgentSide,
                             Vector2 AgentPosition)
        {

            //make a copy of the point
            Vector2 TransPoint = point;

            //create a transformation matrix
            C2DMatrix matTransform = C2DMatrix.identity;

            //로컬좌표기준 이동량을 구한다 
            float Tx = -Vector2.Dot(AgentPosition, AgentHeading);
            float Ty = -Vector2.Dot(AgentPosition, AgentSide);

            //기저축 정보를 이용하여 바로 역행렬을 만든다 
            matTransform._11 = AgentHeading.x; matTransform._12 = AgentSide.x;
            matTransform._21 = AgentHeading.y; matTransform._22 = AgentSide.y;

            //이동량 적용
            matTransform._31 = Tx; matTransform._32 = Ty;

            //내부적으로 대상점에 회전행렬을 곱한 후 이동량이 더해진다 p' = (m*p)+t
            C2DMatrix.Transform(ref matTransform, ref TransPoint);

            return TransPoint;
        }

        public static Vector2 VectorToLocalSpace(Vector2 vec,
                              Vector2 AgentHeading,
                              Vector2 AgentSide)
        {

            //make a copy of the point
            Vector2 TransPoint = vec;

            //create a transformation matrix
            C2DMatrix matTransform = C2DMatrix.identity;

            //create the transformation matrix
            matTransform._11 = AgentHeading.x; matTransform._12 = AgentSide.x;
            matTransform._21 = AgentHeading.y; matTransform._22 = AgentSide.y;

            //now transform the vertices
            C2DMatrix.Transform(ref matTransform, ref TransPoint);

            return TransPoint;
        }

        //--------------------- VectorToWorldSpace --------------------------------
        //
        //  Transforms a vector from the agent's local space into world space
        //------------------------------------------------------------------------
        public static Vector2 VectorToWorldSpace(Vector2 vec,
                                      Vector2 AgentHeading,
                                      Vector2 AgentSide)
        {
            //make a copy of the point
            Vector2 TransVec = vec;

            //create a transformation matrix
            C2DMatrix matTransform = C2DMatrix.identity;

            //rotate
            matTransform.RotateY(AgentHeading, AgentSide);

            //now transform the vertices
            C2DMatrix.Transform(ref matTransform, ref TransVec);

            return TransVec;
        }

        //-------------------------- Vec2DRotateAroundOrigin --------------------------
        //
        //  rotates a vector ang rads around the origin
        //-----------------------------------------------------------------------------
        public static Vector2 Vec2DRotateAroundOrigin(Vector2 v, float ang)
        {
            //create a transformation matrix
            C2DMatrix mat = C2DMatrix.identity;

            //rotate
            mat.RotateY(ang);

            //now transform the object's vertices
            C2DMatrix.Transform(ref mat, ref v);

            return v;
        }


        //----------------------------- TwoCirclesOverlapped ---------------------
        //
        //  Returns true if the two circles overlap
        //------------------------------------------------------------------------
        public static bool TwoCirclesOverlapped(Vector2 c1, float r1,
                                  Vector2 c2, float r2)
        {
            float DistBetweenCenters = (float)Math.Sqrt((c1.x - c2.x) * (c1.x - c2.x) +
                                              (c1.y - c2.y) * (c1.y - c2.y));

            if ((DistBetweenCenters < (r1 + r2)) || (DistBetweenCenters < Math.Abs(r1 - r2)))
            {
                return true;
            }

            return false;
        }


        //------------------------ Sign ------------------------------------------
        //
        //  returns positive if v2 is clockwise of this vector,
        //  minus if anticlockwise (Y axis pointing down, X axis to right)
        //------------------------------------------------------------------------
        public const int clockwise = 1;
        public const int anticlockwise = -1;
        public static int Sign(Vector2 a, Vector2 b)
        {
            if (a.y * b.x > a.x * b.y)
            {
                return anticlockwise;
            }
            else
            {
                return clockwise;
            }
        }

        //-------------------------- Vec2DRotateAroundOrigin --------------------------
        //
        //  rotates a vector ang rads around the origin
        //-----------------------------------------------------------------------------
        public static void Vec2DRotateAroundOrigin(ref Vector2 v, float ang)
        {
            //create a transformation matrix
            C2DMatrix mat = C2DMatrix.identity;

            //rotate
            mat.RotateY(ang);

            //now transform the object's vertices
            C2DMatrix.Transform(ref mat, ref v);
        }

        //------------------------- Overlapped -----------------------------------
        //
        //  tests to see if an entity is overlapping any of a number of entities
        //  stored in a std container
        //------------------------------------------------------------------------
        public static bool Overlapped<T, conT>(T ob, List<conT> conOb, float MinDistBetweenObstacles = 40.0f)
            where T : BaseGameEntity
            where conT : BaseGameEntity
        {

            foreach (BaseGameEntity it in conOb)
            {
                if (TwoCirclesOverlapped(ob.Pos(),
                                     ob.BRadius() + MinDistBetweenObstacles,
                                     it.Pos(),
                                     it.BRadius()))
                {
                    return true;
                }
            }

            return false;
        }

        //----------------------- TagNeighbors ----------------------------------
        //
        //  tags any entities contained in a std container that are within the
        //  radius of the single entity parameter
        //------------------------------------------------------------------------

        public static void TagNeighbors<T, conT>(T entity, List<conT> ContainerOfEntities, float radius)
            where T : BaseGameEntity
            where conT : BaseGameEntity
        {
            //iterate through all entities checking for range
            conT curEntity;
            for (int i = 0; i < ContainerOfEntities.Count; i++)
            {
                curEntity = ContainerOfEntities[i];
                //first clear any current tag
                curEntity.UnTag();

                Vector2 to = curEntity.Pos() - entity.Pos();

                //the bounding radius of the other is taken into account by adding it 
                //to the range
                float range = radius + curEntity.BRadius();

                //if entity within range, tag for further consideration. (working in
                //distance-squared space to avoid sqrts)
                if ((curEntity != entity) && (to.sqrMagnitude < range * range))
                {
                    curEntity.Tag();
                }

            }//next entity
        }


        //------------------- EnforceNonPenetrationConstraint ---------------------
        //
        //  Given a pointer to an entity and a std container of pointers to nearby
        //  entities, this function checks to see if there is an overlap between
        //  entities. If there is, then the entities are moved away from each
        //  other
        //------------------------------------------------------------------------

        public static void EnforceNonPenetrationConstraint<T>(T entity, List<T> ContainerOfEntities) where T : BaseGameEntity
        {
            T curEntity;
            //iterate through all entities checking for any overlap of bounding radii
            for (int i = 0; i < ContainerOfEntities.Count; i++)
            {
                curEntity = ContainerOfEntities[i];
                //make sure we don't check against the individual
                if (curEntity == entity) continue;

                //calculate the distance between the positions of the entities
                Vector2 ToEntity = entity.Pos() - (curEntity).Pos();

                float DistFromEachOther = ToEntity.magnitude;

                //if this distance is smaller than the sum of their radii then this
                //entity must be moved away in the direction parallel to the
                //ToEntity vector   
                float AmountOfOverLap = (curEntity).BRadius() + entity.BRadius() -
                                         DistFromEachOther;

                if (AmountOfOverLap >= 0)
                {
                    //move the entity a distance away equivalent to the amount of overlap.
                    entity.SetPos(entity.Pos() + (ToEntity / DistFromEachOther) *
                                   AmountOfOverLap);
                }
            }//next entity
        }

        public static float Vec2DDistanceSq(Vector2 v1, Vector2 v2)
        {
            float ySeparation = v2.y - v1.y;
            float xSeparation = v2.x - v1.x;

            return ySeparation * ySeparation + xSeparation * xSeparation;
        }

        //==================================================
        //                    geometry
        //==================================================

        //given a plane and a ray this function determins how far along the ray 
        //an interestion occurs. Returns negative if the ray is parallel
        public static float DistanceToRayPlaneIntersection(Vector2 RayOrigin,
                                                     Vector2 RayHeading,
                                                     Vector2 PlanePoint,  //any point on the plane
                                                     Vector2 PlaneNormal)
        {

            float d = -Vector2.Dot(PlaneNormal, PlanePoint);
            float numer = Vector2.Dot(PlaneNormal, RayOrigin) + d;
            float denom = Vector2.Dot(PlaneNormal, RayHeading);

            // normal is parallel to vector
            if ((denom < 0.000001f) && (denom > -0.000001f))
            {
                return (-1.0f);
            }

            return -(numer / denom);
        }


        public enum span_type { plane_backside, plane_front, on_plane };
        public static span_type WhereIsPoint(Vector2 point,
                                      Vector2 PointOnPlane, //any point on the plane
                                      Vector2 PlaneNormal)
        {
            Vector2 dir = PointOnPlane - point;

            double d = Vector2.Dot(dir, PlaneNormal);

            if (d < -0.000001)
            {
                return span_type.plane_front;
            }

            else if (d > 0.000001)
            {
                return span_type.plane_backside;
            }

            return span_type.on_plane;
        }

        //--------------------LineIntersection2D-------------------------
        //
        //  Given 2 lines in 2D space AB, CD this returns true if an 
        //  intersection occurs.
        //
        //----------------------------------------------------------------- 

        public static bool LineIntersection2D(Vector2 A,
                                       Vector2 B,
                                       Vector2 C,
                                       Vector2 D)
        {
            float rTop = (A.y - C.y) * (D.x - C.x) - (A.x - C.x) * (D.y - C.y);
            float sTop = (A.y - C.y) * (B.x - A.x) - (A.x - C.x) * (B.y - A.y);

            float Bot = (B.x - A.x) * (D.y - C.y) - (B.y - A.y) * (D.x - C.x);

            if (Bot == 0)//parallel
            {
                return false;
            }

            float invBot = 1.0f / Bot;
            float r = rTop * invBot;
            float s = sTop * invBot;

            if ((r > 0) && (r < 1) && (s > 0) && (s < 1))
            {
                //lines intersect
                return true;
            }

            //lines do not intersect
            return false;
        }

        //-------------------- LineIntersection2D-------------------------
        //
        //  Given 2 lines in 2D space AB, CD this returns true if an 
        //  intersection occurs and sets dist to the distance the intersection
        //  occurs along AB. Also sets the 2d vector point to the point of
        //  intersection
        //----------------------------------------------------------------- 
        public static bool LineIntersection2D(Vector2 A,
                                       Vector2 B,
                                       Vector2 C,
                                       Vector2 D,
                                       float dist,
                                       Vector2 point)
        {

            float rTop = (A.y - C.y) * (D.x - C.x) - (A.x - C.x) * (D.y - C.y);
            float rBot = (B.x - A.x) * (D.y - C.y) - (B.y - A.y) * (D.x - C.x);

            float sTop = (A.y - C.y) * (B.x - A.x) - (A.x - C.x) * (B.y - A.y);
            float sBot = (B.x - A.x) * (D.y - C.y) - (B.y - A.y) * (D.x - C.x);

            if ((rBot == 0) || (sBot == 0))
            {
                //lines are parallel
                return false;
            }

            float r = rTop / rBot;
            float s = sTop / sBot;

            if ((r > 0) && (r < 1) && (s > 0) && (s < 1))
            {
                dist = (A - B).magnitude * r;

                point = A + r * (B - A);

                return true;
            }

            else
            {
                dist = 0;

                return false;
            }
        }

    }

    public struct C2DMatrix
    {
        public float _11, _12, _13;
        public float _21, _22, _23;
        public float _31, _32, _33;

        //multiplies m_Matrix with mIn
        public static C2DMatrix operator *(C2DMatrix a, C2DMatrix b)
        {
            C2DMatrix matrix = new C2DMatrix();

            //first row
            matrix._11 = (a._11 * b._11) + (a._12 * b._21) + (a._13 * b._31);
            matrix._12 = (a._11 * b._12) + (a._12 * b._22) + (a._13 * b._32);
            matrix._13 = (a._11 * b._13) + (a._12 * b._23) + (a._13 * b._33);

            //second
            matrix._21 = (a._21 * b._11) + (a._22 * b._21) + (a._23 * b._31);
            matrix._22 = (a._21 * b._12) + (a._22 * b._22) + (a._23 * b._32);
            matrix._23 = (a._21 * b._13) + (a._22 * b._23) + (a._23 * b._33);

            //third
            matrix._31 = (a._31 * b._11) + (a._32 * b._21) + (a._33 * b._31);
            matrix._32 = (a._31 * b._12) + (a._32 * b._22) + (a._33 * b._32);
            matrix._33 = (a._31 * b._13) + (a._32 * b._23) + (a._33 * b._33);

            return matrix;
        }

        public override string ToString()
        {
            return _11 + " " + _12 + " " + _13 + "\n" +
                _21 + " " + _22 + " " + _23 + "\n" +
                _31 + " " + _32 + " " + _33 ;
        }

        public void InitZero()
        {
            _11 = 0.0f; _12 = 0.0f; _13 = 0.0f;
            _21 = 0.0f; _22 = 0.0f; _23 = 0.0f;
            _31 = 0.0f; _32 = 0.0f; _33 = 0.0f;
        }

        //create an identity matrix
        public void Identity()
        {
            _11 = 1; _12 = 0; _13 = 0;

            _21 = 0; _22 = 1; _23 = 0;

            _31 = 0; _32 = 0; _33 = 1;
        }

        static public C2DMatrix identity
        {
            get 
            {
                C2DMatrix m = new C2DMatrix();
                m.Identity();
                return m; 
            }
        }

        //create a transformation matrix
        public void Translate(float x, float y)
        {
            C2DMatrix mat = new C2DMatrix();

            mat._11 = 1; mat._12 = 0; mat._13 = 0;

            mat._21 = 0; mat._22 = 1; mat._23 = 0;

            mat._31 = x; mat._32 = y; mat._33 = 1;

            //and multiply
            this = this * mat;
            //MatrixMultiply(mat);
        }

        //create a scale matrix
        public void Scale(float xScale, float yScale)
        {
            C2DMatrix mat = new C2DMatrix();

            mat._11 = xScale; mat._12 = 0; mat._13 = 0;

            mat._21 = 0; mat._22 = yScale; mat._23 = 0;

            mat._31 = 0; mat._32 = 0; mat._33 = 1;

            //and multiply
            this = this * mat;
            //MatrixMultiply(mat);
        }

        //create a rotation matrix
        public void RotateY(float rot)
        {
            C2DMatrix mat;

            float Sin = (float)Math.Sin(rot);
            float Cos = (float)Math.Cos(rot);

            mat._11 = Cos; mat._12 = Sin; mat._13 = 0;

            mat._21 = -Sin; mat._22 = Cos; mat._23 = 0;

            mat._31 = 0; mat._32 = 0; mat._33 = 1;

            //and multiply
            this = this * mat;
            //MatrixMultiply(mat);
        }

        // | cos@  sin@|   | head |   | @      |
        // |-sin@  cos@| = | side | = | @ + 90 |
        // side = perp(head)
        // 회전행렬을 가로축으로 배치된 벡터로 보았을때 첫번째행 백터와 두번째행 벡터는 90도가 차이가 난다 
        // 이 특징을 이용하여 정규화된 특정 방향백터 하나의 값으로 회전행렬을 구할 수 있다  
        //create a rotation matrix from a fwd and side 2D vector
        public void RotateY(Vector2 fwd, Vector2 side)
        {
            C2DMatrix mat = new C2DMatrix();

            mat._11 = fwd.x; mat._12 = fwd.y; mat._13 = 0;

            mat._21 = side.x; mat._22 = side.y; mat._23 = 0;

            mat._31 = 0; mat._32 = 0; mat._33 = 1;

            //DebugWide.LogBlue(this + " \n " + mat);
            //and multiply
            this = this * mat;
            //MatrixMultiply(mat);
        }

        //applys a transformation matrix to a std::vector of points
        public static void Transform(ref C2DMatrix matrix, ref List<Vector2> vPoints)
        {
            
            for (int i = 0; i < vPoints.Count; ++i)
            {
                Vector2 temp = new Vector2();
                temp.x = (matrix._11 * vPoints[i].x) + (matrix._21 * vPoints[i].y) + (matrix._31);

                temp.y = (matrix._12 * vPoints[i].x) + (matrix._22 * vPoints[i].y) + (matrix._32);

                vPoints[i] = temp;
            }
        }

        //applys a transformation matrix to a point
        public static void Transform(ref C2DMatrix matrix, ref Vector2 vPoint)
        {
            Vector2 temp = new Vector2();
            temp.x = (matrix._11 * vPoint.x) + (matrix._21 * vPoint.y) + (matrix._31);

            temp.y = (matrix._12 * vPoint.x) + (matrix._22 * vPoint.y) + (matrix._32);

            vPoint = temp;
        }
    }

    public class BaseGameEntity
    {

        //public enum { default_entity_type = -1 };
        public const int default_entity_type = -1;

        //each entity has a unique ID
        int m_ID;

        //every entity has a type associated with it (health, troll, ammo etc)
        int m_EntityType;

        //this is a generic flag. 
        bool m_bTag;

        //used by the constructor to give each entity a unique ID
        //int m_NextID = 0;
        //int NextValidID() { return m_NextID++; } //의도에 맞게 수정할 필요 있음 



        //its location in the environment
        protected Vector2 m_vPos;

        protected Vector2 m_vScale;

        //the length of this object's bounding radius
        float m_dBoundingRadius;


        public BaseGameEntity()
        {
            //m_ID = NextValidID();
            m_dBoundingRadius = 0f;
            m_vPos = new Vector2();
            m_vScale = new Vector2(1f, 1f);
            m_EntityType = default_entity_type;
            m_bTag = false;
        }


        public BaseGameEntity(int entity_type)
        {

            //m_ID = NextValidID();
            m_dBoundingRadius = 0f;
            m_vPos = new Vector2();
            m_vScale = new Vector2(1f, 1f);
            m_EntityType = entity_type;
            m_bTag = false;
        }


        public BaseGameEntity(int entity_type, Vector2 pos, float r)
        {
            //m_ID = NextValidID();
            m_dBoundingRadius = r;
            m_vPos = pos;
            m_vScale = new Vector2(1f, 1f);
            m_EntityType = entity_type;
            m_bTag = false;

        }

        //this can be used to create an entity with a 'forced' ID. It can be used
        //when a previously created entity has been removed and deleted from the
        //game for some reason. For example, The Raven map editor uses this ctor 
        //in its undo/redo operations. 
        //USE WITH CAUTION!
        public BaseGameEntity(int entity_type, int ForcedID)
        {
            m_ID = ForcedID;
            m_dBoundingRadius = 0f;
            m_vPos = new Vector2();
            m_vScale = new Vector2(1f, 1f);
            m_EntityType = entity_type;
            m_bTag = false;
        }



        public virtual void Update(double time_elapsed) { }

        public virtual void Render() { }

        //public virtual bool HandleMessage(Telegram msg) { return false; }


        public Vector2 Pos() { return m_vPos; }
        public void SetPos(Vector2 new_pos) { m_vPos = new_pos; }

        public float BRadius() { return m_dBoundingRadius; }
        public void SetBRadius(float r) { m_dBoundingRadius = r; }
        public int ID() { return m_ID; }

        public bool IsTagged() { return m_bTag; }
        public void Tag() { m_bTag = true; }
        public void UnTag() { m_bTag = false; }

        public Vector2 Scale() { return m_vScale; }
        public void SetScale(Vector2 val) { m_dBoundingRadius *= Util.MaxOf(val.x, val.y) / Util.MaxOf(m_vScale.x, m_vScale.y); m_vScale = val; }
        public void SetScale(float val) { m_dBoundingRadius *= (val / Util.MaxOf(m_vScale.x, m_vScale.y)); m_vScale = new Vector2(val, val); }

        public int EntityType() { return m_EntityType; }
        public void SetEntityType(int new_type) { m_EntityType = new_type; }

    }

    public class MovingEntity : BaseGameEntity
    {

        protected Vector2 m_vVelocity;

        //a normalized vector pointing in the direction the entity is heading. 
        protected Vector2 m_vHeading;

        //a vector perpendicular to the heading vector
        protected Vector2 m_vSide;

        protected float m_dMass;

        //the maximum speed this entity may travel at.
        protected float m_dMaxSpeed;

        //the maximum force this entity can produce to power itself 
        //(think rockets and thrust)
        protected float m_dMaxForce;

        //the maximum rate (radians per second)this vehicle can rotate         
        protected float m_dMaxTurnRate;


        public MovingEntity(Vector2 position,
                               float radius,
                               Vector2 velocity,
                               float max_speed,
                               Vector2 heading,
                               float mass,
                               Vector2 scale,
                               float turn_rate,
                               float max_force) : base(0, position, radius)
        {
            m_vHeading = heading;
            m_vVelocity = velocity;
            m_dMass = mass;
            m_vSide = VOp.Perp(m_vHeading);
            m_dMaxSpeed = max_speed;
            m_dMaxTurnRate = turn_rate;
            m_dMaxForce = max_force;
            m_vScale = scale;
        }


        //accessors
        public Vector2 Velocity() { return m_vVelocity; }
        public void SetVelocity(Vector2 NewVel) { m_vVelocity = NewVel; }

        public float Mass() { return m_dMass; }

        public Vector2 Side() { return m_vSide; }

        public float MaxSpeed() { return m_dMaxSpeed; }
        public void SetMaxSpeed(float new_speed) { m_dMaxSpeed = new_speed; }

        public float MaxForce() { return m_dMaxForce; }
        public void SetMaxForce(float mf) { m_dMaxForce = mf; }

        public bool IsSpeedMaxedOut() { return m_dMaxSpeed * m_dMaxSpeed >= m_vVelocity.sqrMagnitude; }
        public float Speed() { return m_vVelocity.magnitude; }
        public float SpeedSq() { return m_vVelocity.sqrMagnitude; }

        public float MaxTurnRate() { return m_dMaxTurnRate; }
        public void SetMaxTurnRate(float val) { m_dMaxTurnRate = val; }

        public Vector2 Heading() { return m_vHeading; }

        //------------------------- SetHeading ----------------------------------------
        //
        //  first checks that the given heading is not a vector of zero length. If the
        //  new heading is valid this fumction sets the entity's heading and side 
        //  vectors accordingly
        //-----------------------------------------------------------------------------
        public void SetHeading(Vector2 new_heading)
        {
            //assert((new_heading.sqrMagnitude - 1.0f) < 0.00001f);

            m_vHeading = new_heading;

            //the side vector must always be perpendicular to the heading
            m_vSide = VOp.Perp(m_vHeading);
        }

        //--------------------------- RotateHeadingToFacePosition ---------------------
        //
        //  given a target position, this method rotates the entity's heading and
        //  side vectors by an amount not greater than m_dMaxTurnRate until it
        //  directly faces the target.
        //
        //  returns true when the heading is facing in the desired direction
        //-----------------------------------------------------------------------------
        public bool RotateHeadingToFacePosition(Vector2 target)
        {
            Vector2 toTarget = (target - m_vPos).normalized;

            //first determine the angle between the heading vector and the target
            float angle = (float)Math.Acos(Vector2.Dot(m_vHeading, toTarget));

            //return true if the player is facing the target
            if (angle < 0.00001f) return true;

            //clamp the amount to turn to the max turn rate
            if (angle > m_dMaxTurnRate) angle = m_dMaxTurnRate;

            //The next few lines use a rotation matrix to rotate the player's heading
            //vector accordingly

            C2DMatrix RotationMatrix = C2DMatrix.identity;
            //notice how the direction of rotation has to be determined when creating
            //the rotation matrix
            RotationMatrix.RotateY(angle * Util.Sign(m_vHeading, toTarget));
            C2DMatrix.Transform(ref RotationMatrix, ref m_vHeading);
            C2DMatrix.Transform(ref RotationMatrix, ref m_vVelocity);

            //finally recreate m_vSide
            m_vSide = VOp.Perp(m_vHeading);

            return false;
        }

    }


    public class Obstacle : BaseGameEntity
    {

        public Obstacle(float x,
               float y,
               float r) : base(0, new Vector2(x, y), r)
        { }

        public Obstacle(Vector2 pos, float radius) : base(0, pos, radius)
        { }


        //this is defined as a pure virtual function in BasegameEntity so
        //it must be implemented
        public void Update(float time_elapsed) { }

        public void Render()
        {
            DebugWide.DrawCircle(Pos(), BRadius(), Color.black);
        }


    }

    public class Vehicle : MovingEntity
    {

        //a pointer to the world data. So a vehicle can access any obstacle,
        //path, wall or agent data
        Test_SteeringBehavior m_pWorld; //Test_SteeringBehavior

        //the steering behavior class
        SteeringBehavior m_pSteering;


        //some steering behaviors give jerky looking movement. The
        //following members are used to smooth the vehicle's heading
        Smoother<Vector2, Vectoc2Calc> m_pHeadingSmoother;

        //this vector represents the average of the vehicle's heading
        //vector smoothed over the last few frames
        Vector2 m_vSmoothedHeading;

        //when true, smoothing is active
        bool m_bSmoothingOn;


        //keeps a track of the most recent update time. (some of the
        //steering behaviors make use of this - see Wander)
        float m_dTimeElapsed;


        //buffer for the vehicle shape
        List<Vector2> m_vecVehicleVB = new List<Vector2>();

        //fills the buffer with vertex data
        void InitializeBuffer()
        {
            const int NumVehicleVerts = 3;

            Vector2[] vehicle = new Vector2[NumVehicleVerts]
                        { new Vector2(-1.0f,0.6f),
                                        new Vector2(1.0f,0.0f),
                                        new Vector2(-1.0f,-0.6f)
                        };

            //setup the vertex buffers and calculate the bounding radius
            for (int vtx = 0; vtx < NumVehicleVerts; ++vtx)
            {
                m_vecVehicleVB.Add(vehicle[vtx]);
                //DebugWide.LogBlue(vehicle[vtx] + " -=-=-= ");
            }
        }

        //disallow the copying of Vehicle types
        //Vehicle(const Vehicle&);
        //Vehicle& operator=(const Vehicle&);


        public Vehicle(Test_SteeringBehavior world,
                         Vector2 position,
                         float rotation,
                         Vector2 velocity,
                         float mass,
                         float max_force,
                         float max_speed,
                         float max_turn_rate,
                         float scale) : base(position,
                                                 scale,
                                                 velocity,
                                                 max_speed,
                                                 new Vector2((float)Math.Sin(rotation), -(float)Math.Cos(rotation)),
                                                 mass,
                                                 new Vector2(scale, scale),
                                                 max_turn_rate,
                                                 max_force)
        {

            m_pWorld = world;
            m_vSmoothedHeading = Vector2.zero;
            m_bSmoothingOn = false;
            m_dTimeElapsed = 0f;

            InitializeBuffer();

            //set up the steering behavior class
            m_pSteering = new SteeringBehavior(this);

            //set up the smoother
            m_pHeadingSmoother = new Smoother<Vector2, Vectoc2Calc>(SingleO.prm.NumSamplesForSmoothing, new Vector2(0.0f, 0.0f));

        }


        //updates the vehicle's position and orientation
        public void Update(float time_elapsed)
        {
            //update the time elapsed
            m_dTimeElapsed = time_elapsed;

            //keep a record of its old position so we can update its cell later
            //in this method
            Vector2 OldPos = Pos();


            Vector2 SteeringForce;

            //calculate the combined force from each steering behavior in the 
            //vehicle's list
            SteeringForce = m_pSteering.Calculate();

            //Acceleration = Force/Mass
            Vector2 acceleration = SteeringForce / m_dMass;

            //update velocity
            m_vVelocity += acceleration * time_elapsed;

            //make sure vehicle does not exceed maximum velocity
            m_vVelocity = Util.Truncate(m_vVelocity, m_dMaxSpeed);

            //update the position
            m_vPos += m_vVelocity * time_elapsed;

            //update the heading if the vehicle has a non zero velocity
            if (m_vVelocity.sqrMagnitude > 0.00000001f)
            {
                m_vHeading = m_vVelocity.normalized;

                m_vSide = VOp.Perp(m_vHeading);
            }

            //EnforceNonPenetrationConstraint(this, World()->Agents());

            //treat the screen as a toroid
            m_vPos = Util.WrapAround(m_vPos, m_pWorld.cxClient(), m_pWorld.cyClient());

            //update the vehicle's current cell if space partitioning is turned on
            if (Steering().isSpacePartitioningOn())
            {
                World().CellSpace().UpdateEntity(this, OldPos);
            }

            if (isSmoothingOn())
            {
                m_vSmoothedHeading = m_pHeadingSmoother.Update(Heading());
            }
        }

        public void Render()
        {
            //a vector to hold the transformed vertices
            List<Vector2> m_vecVehicleVBTrans;

            Color color = Color.blue;
            //render neighboring vehicles in different colors if requested
            if (m_pWorld.RenderNeighbors())
            {

                if (ID() == 0) color = Color.red;
                else if (IsTagged()) color = Color.green;
                else color = Color.blue;
            }


            if (Steering().isInterposeOn())
            {
                color = Color.red;
            }

            if (Steering().isHideOn())
            {
                color = Color.green;
            }

            if (isSmoothingOn())
            {
                m_vecVehicleVBTrans = Util.WorldTransform(m_vecVehicleVB,
                                                     Pos(),
                                                     SmoothedHeading(),
                                                          VOp.Perp(SmoothedHeading()),
                                                     Scale());
            }

            else
            {
                m_vecVehicleVBTrans = Util.WorldTransform(m_vecVehicleVB,
                                                     Pos(),
                                                     Heading(),
                                                     Side(),
                                                     Scale());
            }


            //gdi->ClosedShape(m_vecVehicleVBTrans);
            List<Vector2> lines = m_vecVehicleVBTrans;
            //DebugWide.LogBlue(isSmoothingOn() + "  " + m_vecVehicleVB[0] + " ----OO " + m_vecVehicleVB[1] + "  " + m_vecVehicleVB[2]);
            //DebugWide.LogBlue(lines[0] + " ----OO " + lines[1] + "  " + lines[2]);
            DebugWide.DrawLine(lines[0], lines[1], color);
            DebugWide.DrawLine(lines[1], lines[2], color);
            DebugWide.DrawLine(lines[2], lines[0], color);


            //render any visual aids / and or user options
            if (m_pWorld.ViewKeys())
            {
                Steering().RenderAids();
            }
        }


        //-------------------------------------------accessor methods
        public SteeringBehavior Steering() { return m_pSteering; }
        public Test_SteeringBehavior World() { return m_pWorld; }


        public Vector2 SmoothedHeading() { return m_vSmoothedHeading; }

        public bool isSmoothingOn() { return m_bSmoothingOn; }
        public void SmoothingOn() { m_bSmoothingOn = true; }
        public void SmoothingOff() { m_bSmoothingOn = false; }
        public void ToggleSmoothing() { m_bSmoothingOn = !m_bSmoothingOn; }

        public float TimeElapsed() { return m_dTimeElapsed; }

    }

    public class Wall2D
    {

        Vector2 m_vA, m_vB, m_vN;

        void CalculateNormal()
        {
            Vector2 temp = (m_vB - m_vA).normalized;

            m_vN.x = -temp.y;
            m_vN.y = temp.x;
        }


        public Wall2D() { }

        public Wall2D(Vector2 A, Vector2 B)
        {
            m_vA = A;
            m_vB = B;
            CalculateNormal();
        }

        public Wall2D(Vector2 A, Vector2 B, Vector2 N)
        {
            m_vA = A;
            m_vB = B;
            m_vN = N;
        }


        public virtual void Render(bool RenderNormals = false)
        {
            //gdi->Line(m_vA, m_vB);
            DebugWide.DrawLine(m_vA, m_vB, Color.white);

            ////render the normals if rqd
            if (RenderNormals)
            {
                int MidX = (int)((m_vA.x + m_vB.x) / 2);
                int MidY = (int)((m_vA.y + m_vB.y) / 2);
                DebugWide.DrawLine(new Vector2(MidX, MidY), new Vector2((int)(MidX + (m_vN.x * 5)), (int)(MidY + (m_vN.y * 5))), Color.black);
                //gdi->Line(MidX, MidY, (int)(MidX+(m_vN.x* 5)), (int) (MidY+(m_vN.y* 5)));
            }
        }

        public Vector2 From() { return m_vA; }
        public void SetFrom(Vector2 v) { m_vA = v; CalculateNormal(); }

        public Vector2 To() { return m_vB; }
        public void SetTo(Vector2 v) { m_vB = v; CalculateNormal(); }

        public Vector2 Normal() { return m_vN; }
        public void SetNormal(Vector2 n) { m_vN = n; }

        public Vector2 Center() { return (m_vA + m_vB) / 2.0f; }

    }

    public class Path
    {

        LinkedList<Vector2> m_WayPoints = new LinkedList<Vector2>();

        //points to the current waypoint
        LinkedListNode<Vector2> curWaypoint;

        //flag to indicate if the path should be looped
        //(The last waypoint connected to the first)
        bool m_bLooped;


        public Path() { m_bLooped = false; }

        //constructor for creating a path with initial random waypoints. MinX/Y
        //& MaxX/Y define the bounding box of the path.
        public Path(int NumWaypoints,
                    float MinX,
                    float MinY,
                    float MaxX,
                    float MaxY,
                    bool looped)
        {
            m_bLooped = looped;
            CreateRandomPath(NumWaypoints, MinX, MinY, MaxX, MaxY);

            curWaypoint = m_WayPoints.First;
        }


        //returns the current waypoint
        public Vector2 CurrentWaypoint()
        {
            //assert(curWaypoint != NULL); 
            return curWaypoint.Value;
        }

        //returns true if the end of the list has been reached
        public bool Finished()
        {
            //return !(curWaypoint != m_WayPoints.Last); 
            if (null == curWaypoint) return true;

            return (curWaypoint == m_WayPoints.Last);
        }

        //moves the iterator on to the next waypoint in the list
        public void SetNextWaypoint()
        {
            //assert(m_WayPoints.size() > 0);

            curWaypoint = curWaypoint.Next;
            if (curWaypoint == m_WayPoints.Last)
            {
                if (m_bLooped)
                {
                    curWaypoint = m_WayPoints.First;
                }
            }

        }

        //creates a random path which is bound by rectangle described by
        //the min/max values
        public LinkedList<Vector2> CreateRandomPath(int NumWaypoints,
                                         float MinX,
                                         float MinY,
                                         float MaxX,
                                         float MaxY)
        {
            m_WayPoints.Clear();

            float midX = (MaxX + MinX) / 2.0f;
            float midY = (MaxY + MinY) / 2.0f;

            float smaller = Util.MinOf(midX, midY);

            float spacing = Const.TwoPi / (float)NumWaypoints;

            for (int i = 0; i < NumWaypoints; ++i)
            {

                float RadialDist = Misc.RandFloat(smaller * 0.2f, smaller);

                Vector2 temp = new Vector2(RadialDist, 0.0f);

                Util.Vec2DRotateAroundOrigin(ref temp, i * spacing);

                temp.x += midX; temp.y += midY;

                m_WayPoints.AddLast(temp);

            }

            curWaypoint = m_WayPoints.First;

            return m_WayPoints;
        }


        public void LoopOn() { m_bLooped = true; }
        public void LoopOff() { m_bLooped = false; }

        //adds a waypoint to the end of the path
        //public void AddWayPoint(Vector2 new_point);

        //methods for setting the path with either another Path or a list of vectors
        public void Set(LinkedList<Vector2> new_path) { m_WayPoints = new_path; curWaypoint = m_WayPoints.First; }
        public void Set(Path path) { m_WayPoints = path.GetPath(); curWaypoint = m_WayPoints.First; }


        public void Clear() { m_WayPoints.Clear(); }

        public LinkedList<Vector2> GetPath() { return m_WayPoints; }

        //renders the path in orange
        public void Render()
        {

            LinkedListNode<Vector2> it = m_WayPoints.First;
            LinkedListNode<Vector2> wp = it;
            it = it.Next;
            while (it != m_WayPoints.Last) //마지막 노드는 못그리는 문제가 있을 것으로 예상 
            {
                DebugWide.DrawLine(wp.Value, it.Value, Color.magenta);
                wp = it;
                it = it.Next;
            }


            if (m_bLooped) DebugWide.DrawLine(m_WayPoints.Last.Value, m_WayPoints.First.Value, Color.magenta);
        }
    }
}

