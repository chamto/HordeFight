using UnityEngine;



namespace UtilGS9
{



    public partial class Geo
    {
        //==================================================
        //==================================================
        //==================================================

        //통합모델은 데이터 공유, 함수만 다른 모델 객체로 나눈다 
        public class Model
        {
            //public enum eBranch
            //{
            //    none = 0,

            //    head_0,

            //    //============
            //    LEFT_START,
            //    arm_left_0,
            //    arm_left_1,

            //    leg_left_0,
            //    leg_left_1,

            //    wing_left_0,
            //    LEFT_END,

            //    //============
            //    RIGHT_START,
            //    arm_right_0,
            //    arm_right_1,

            //    leg_right_0,
            //    leg_right_1,

            //    wing_right_0,
            //    RIGHT_END,
            //    //============

            //    tail_0,
            //}

            public enum eKind
            {
                FreePlane = 0,  //자유평면
                Circle,
                DeformationCircle,
                Tornado,
                Cylinder,
                Arc, //호 
            }
            //public const int FreePlane = 0; //자유평면
            //public const int Circle = 1;
            //public const int DeformationCircle = 2;
            //public const int Tornado = 3;
            //public const int Cylinder = 4;
            //public const int Arc = 5;  //호 

            //public eBranch branch = eBranch.none;
            public eKind kind = eKind.FreePlane;

            public Vector3 upDir;
            public Vector3 origin;
            public float radius;

            //deformaitonCircle
            public Vector3 dir;
            public float length;
            public Vector3 anchorA;
            public Vector3 anchorB;
            public int interpolationNumber;

            //tornadno
            //public Vector3 dir; //회오리 시작방향
            //public float length; //회오리 반지름
            public float maxAngle; //최대회전각
            public Vector3 unlaceDir; //풀어지는 방향 

            //cylinder
            //public float radius_origin;       //시작점에서 가까운 원의 반지름 
            //public Vector3 dir;             //origin 원에서 다른원까지의 방향 
            //public float length;            //길이 
            public float radius_far;        //시작점에서 먼 원의 반지름

            //----------------------------------------------
            public FreePlane freePlane = new FreePlane();
            public Circle circle = new Circle();
            public DeformationCircle deformationCircle = new DeformationCircle();
            public Tornado tornado = new Tornado();
            public Cylinder cylinder = new Cylinder();

            public Model()
            {
                freePlane.model = this;
                circle.model = this;
                deformationCircle.model = this;
                tornado.model = this;
                cylinder.model = this;
            }

            //public bool IsLeft()
            //{
            //    if (eBranch.LEFT_START < this.branch && this.branch < eBranch.LEFT_END)
            //    {
            //        return true;
            //    }
            //    return false;
            //}

            //public bool IsRight()
            //{
            //    if (eBranch.RIGHT_START < this.branch && this.branch < eBranch.RIGHT_END)
            //    {
            //        return true;
            //    }
            //    return false;
            //}


            public void SetModel(Vector3 in_upDir, Vector3 in_center, float in_radius, Vector3 in_highest, float in_far_radius,
                          float in_tornadoAngle, Vector3 in_tornadoUnlace)
            {
                switch (this.kind)
                {
                    case Geo.Model.eKind.FreePlane:
                        {
                            this.freePlane.Set(in_upDir, in_center);
                        }
                        break;
                    case Geo.Model.eKind.Circle:
                        {
                            this.circle.Set(in_upDir, in_center, in_radius);
                        }
                        break;
                    case Geo.Model.eKind.DeformationCircle:
                        {
                            this.deformationCircle.Set(in_upDir, in_center, in_radius, in_highest, 1);
                        }
                        break;
                    case Geo.Model.eKind.Tornado:
                        {
                            this.tornado.Set(in_upDir, in_center, in_radius, in_highest, in_tornadoUnlace, in_tornadoAngle);
                        }
                        break;
                    case Geo.Model.eKind.Cylinder:
                        {
                            this.cylinder.Set(in_upDir, in_center, in_radius, in_highest, in_far_radius);
                        }
                        break;
                }
            }

            public Vector3 CollisionPos(Vector3 handle, out Vector3 upDir2)
            {
                upDir2 = this.upDir;
                Vector3 aroundCalcPos = UtilGS9.ConstV.v3_zero;
                switch (this.kind)
                {
                    case Geo.Model.eKind.FreePlane:
                        {
                            aroundCalcPos = this.freePlane.CollisionPos(handle);
                        }
                        break;
                    case Geo.Model.eKind.Circle:
                        {
                            aroundCalcPos = this.circle.CollisionPos(handle);
                        }
                        break;
                    case Geo.Model.eKind.DeformationCircle:
                        {
                            aroundCalcPos = this.deformationCircle.CollisionPos(handle);
                        }
                        break;
                    case Geo.Model.eKind.Tornado:
                        {
                            aroundCalcPos = this.tornado.CollisionPos3D(handle);
                        }
                        break;
                    case Geo.Model.eKind.Cylinder:
                        {
                            aroundCalcPos = this.cylinder.CollisionPos(handle, out upDir2);
                        }
                        break;
                }

                return aroundCalcPos;
            }



            public void Draw(Color cc)
            {
                switch (kind)
                {
                    case eKind.FreePlane:
                        {
                            freePlane.Draw(cc);
                        }
                        break;
                    case eKind.Circle:
                        {
                            circle.Draw(cc);
                        }
                        break;
                    case eKind.DeformationCircle:
                        {
                            deformationCircle.Draw(cc);
                        }
                        break;
                    case eKind.Tornado:
                        {
                            tornado.Draw(cc);
                        }
                        break;
                    case eKind.Cylinder:
                        {
                            cylinder.Draw(cc);
                        }
                        break;
                }

            }
        }


        //==================================================

        //자유평면을 정의
        public class FreePlane
        {
            public Model model = null;
            //public FreePlane()
            //{
            //    base.kind = Model.FreePlane;
            //}

            public void Set(Vector3 p_upDir, Vector3 p_orign)
            {
                model.upDir = p_upDir;
                model.origin = p_orign;
            }

            public Vector3 CollisionPos(Vector3 handlePos)// Vector3 upDir)
            {
                Vector3 toHandle = handlePos - model.origin;
                Vector3 proj_toHandle = model.upDir * Vector3.Dot(toHandle, model.upDir) / model.upDir.sqrMagnitude; //up벡터가 정규화 되었다면 "up벡터 제곱길이"로 나누는 연산을 뺄수  있다 

                Vector3 proj_handlePos = handlePos - proj_toHandle; //바로 투영점을 구한다 

                return proj_handlePos;
            }

            public void Draw(Color cc)//(Vector3 upDir, Color cc)
            {
                DebugWide.DrawCirclePlane(model.origin, 2f, model.upDir, cc);
            }
        }

        //평면상의 이차원 원을 정의 
        public class Circle
        {
            public Model model = null;
            //public float radius;

            //public Circle()
            //{
            //    base.kind = Model.Circle;
            //}

            public void Set(Vector3 p_upDir, Vector3 p_orign, float p_radius)
            {
                model.upDir = p_upDir;
                model.origin = p_orign;
                model.radius = p_radius;
            }

            public Vector3 CollisionPos(Vector3 handlePos)//, Vector3 upDir)
            {
                Vector3 toHandle = handlePos - model.origin;
                Vector3 proj_toHandle = model.upDir * Vector3.Dot(toHandle, model.upDir) / model.upDir.sqrMagnitude; //up벡터가 정규화 되었다면 "up벡터 제곱길이"로 나누는 연산을 뺄수  있다 

                Vector3 proj_handlePos = handlePos - proj_toHandle; //바로 투영점을 구한다 
                Vector3 n_circleToHand = (proj_handlePos - model.origin).normalized;

                return model.origin + n_circleToHand * model.radius;
            }

            public void Draw(Color cc)//(Vector3 upDir, Color cc)
            {
                DebugWide.DrawCirclePlane(model.origin, model.radius, model.upDir, cc);
            }
        }

        //평면상의 이차원 변형원을 정의 
        public class DeformationCircle
        {

            public Model model = null;
            //public float radius;
            //public Vector3 dir;
            //public float length;
            //public Vector3 anchorA;
            //public Vector3 anchorB;
            //public int interpolationNumber;


            //public DeformationCircle()
            //{
            //    base.kind = Model.DeformationCircle;
            //}


            public void Set(Vector3 p_upDir, Vector3 p_orign, float p_radius, Vector3 p_highestPoint, int p_interpolationNumber)
            {
                model.upDir = p_upDir;
                model.origin = p_orign;
                model.radius = p_radius;

                Vector3 toDir = p_highestPoint - p_orign;
                Vector3 proj_toDir = toDir - p_upDir * Vector3.Dot(p_upDir, toDir) / p_upDir.sqrMagnitude;

                model.length = proj_toDir.magnitude;
                if (model.length <= float.Epsilon)
                {
                    model.length = model.radius;
                    model.dir = Vector3.zero;
                }
                else
                    model.dir = proj_toDir / model.length;


                model.interpolationNumber = p_interpolationNumber;
            }


            public void Set(Vector3 p_upDir, Vector3 p_orign, float p_radius, Vector3 p_highestPoint, Vector3 p_anchorA, Vector3 p_anchorB, int p_interpolationNumber)
            {
                model.upDir = p_upDir;
                model.origin = p_orign;
                model.radius = p_radius;

                Vector3 toDir = p_highestPoint - p_orign;
                Vector3 proj_toDir = toDir - p_upDir * Vector3.Dot(p_upDir, toDir) / p_upDir.sqrMagnitude;

                model.length = proj_toDir.magnitude;
                if (model.length <= float.Epsilon)
                {
                    model.length = model.radius;
                    model.dir = Vector3.zero;
                }
                else
                    model.dir = proj_toDir / model.length;

                model.anchorA = p_anchorA;
                model.anchorB = p_anchorB;
                model.interpolationNumber = p_interpolationNumber;
            }

            private float Calc_Td(float t,
                                  float angleA, float angleH, float angleB, float angleTarget)
            {

                //앵커를 고정으로 놓기 때문에 음수각도에 대한 예외처리를 할 필요가 없어짐
                //float angleA = 45f;
                //float angleH = 90f;
                //float angleB = 135f;


                //비례식을 이용하여 td 구하기 
                //angleD : td  = angleH : t
                //td * angleH = angleD * t
                //td = (angleD * t) / angleH
                float maxAngle = angleB;
                float minAngle = angleA;
                float maxTd = (maxAngle * t) / angleH;
                float minTd = (minAngle * t) / angleH;


                //Vector3 tdDir = Quaternion.AngleAxis(rotateAngle, upDir) * initialDir;
                float td = 0f;

                //t 가 0 이면 0 으로 나누는 문제가 발생함, 이를 막는 예외처리 추가 
                if (t != 0 && minAngle <= angleTarget && angleTarget <= maxAngle)
                {

                    td = (angleTarget * t) / angleH;

                    //최고점이 중심원의 외부에 위치한 경우
                    bool outside_highestPoint = td < t;

                    if (model.length < model.radius)
                    {   //최고점이 중심원의 내부에 위치한 경우의 예외처리 
                        outside_highestPoint = !outside_highestPoint;
                    }

                    //회오리 값의 지정구간 비율값을 0~1 , 1~0 으로 변환시킨다
                    if (outside_highestPoint)
                    {
                        td = td - minTd; //minTd ~ t => 0 ~ (t - minTd)
                        td /= (t - minTd); //0~1로 변환
                    }
                    else
                    {
                        //최고점을 기준으로 대칭형을 만들어 준다    
                        td = maxTd - td; //t ~ maxTd => (maxTd - t) ~ 0
                        td /= (maxTd - t); //1~0로 변환
                    }

                    //0 또는 범위외값 : 보간없음
                    //1~4 : 번호가 높을 수록 표현이 날카로워 진다 
                    switch (model.interpolationNumber)
                    {

                        case 1:
                            td = UtilGS9.Interpolation.easeInSine(0, 1f, td); //살짝 둥근 표현 
                            break;
                        case 2:
                            td = UtilGS9.Interpolation.easeInCirc(0, 1f, td); //직선에 가까운 표현 가능 *
                            break;
                        case 3:
                            td = UtilGS9.Interpolation.easeInQuart(0, 1f, td); //직선에 가까운 표현 가능 **
                            break;
                        case 4:
                            td = UtilGS9.Interpolation.easeInBack(0, 1f, td); //직선에 가까운 표현 가능 ***
                            break;


                    }

                    td *= t; //0~t 범위로 변환 
                }

                return td;
            }


            //앵커를 고정된 각도로 놓아 계산량을 줄인 함수 버젼
            public Vector3 CollisionPos(Vector3 handlePoint)//Vector3 upDir)
            {
                //늘어남계수 = 원점에서 최고점까지의 길이 - 반지름 
                float t = model.length - model.radius;


                //목표점을 변형원의 평면상으로 투영 (벡터합을 이용하여 도출)
                Vector3 centerToTarget = handlePoint - model.origin;
                Vector3 proj_targetToUp = model.upDir * Vector3.Dot(centerToTarget, model.upDir) / model.upDir.sqrMagnitude; //up벡터가 정규화 되었다면 "up벡터 제곱길이"로 나누는 연산을 뺄수  있다 
                Vector3 tdDir = centerToTarget - proj_targetToUp;


                //Vector3 tdDir = targetPoint - sphereCenter;
                tdDir.Normalize();

                //최고점 기준으로 좌우90,90도 최대 180도를 표현한다 
                Vector3 initialDir = Quaternion.AngleAxis(-90f, model.upDir) * model.dir;
                //initialDir.Normalize();

                float angleTarget = Vector3.SignedAngle(initialDir, tdDir, model.upDir);

                //앵커를 고정으로 놓기 때문에 음수각도에 대한 예외처리를 할 필요가 없어짐
                //float angleA = 45f;
                //float angleH = 90f;
                //float angleB = 135f;


                float td = this.Calc_Td(t, 45f, 90f, 135f, angleTarget);
                return model.origin + tdDir * (model.radius + td);
            }

            //upDir 을 앵커A,B값으로 만든다  
            public Vector3 CollisionPos_Anchor(Vector3 handlePoint)
            {

                //늘어남계수 = 원점에서 최고점까지의 길이 - 반지름 
                float t = model.length - model.radius;

                Vector3 upDir = Vector3.Cross(model.anchorA - model.origin, model.anchorB - model.origin);
                //upDir.Normalize();

                //최고점 기준으로 좌우90,90도 최대 180도를 표현한다 
                Vector3 initialDir = Quaternion.AngleAxis(-90f, upDir) * model.dir;
                //initialDir.Normalize();


                //목표점을 변형원의 평면상으로 투영 (벡터합을 이용하여 도출)
                Vector3 centerToTarget = handlePoint - model.origin;
                Vector3 proj_targetToUp = upDir * Vector3.Dot(centerToTarget, upDir) / upDir.sqrMagnitude; //up벡터가 정규화 되었다면 "up벡터 제곱길이"로 나누는 연산을 뺄수  있다 
                Vector3 tdDir = centerToTarget - proj_targetToUp;

                //Vector3 tdDir = targetPoint - sphereCenter;
                tdDir.Normalize();

                float angleTarget = Vector3.SignedAngle(initialDir, tdDir, upDir);
                float angleA = Vector3.SignedAngle(initialDir, model.anchorA - model.origin, upDir);
                float angleB = Vector3.SignedAngle(initialDir, model.anchorB - model.origin, upDir);
                //float angleA = 45f;
                //float angleB = 135f;
                float angleH = 90f;

                //-1~-179 각도표현을 1~179 로 변환한다
                //각도가 음수영역으로 들어가면 양수영역 각도로 변환한다 (각도가 음수영역으로 들어가면 궤적이 올바르게 표현이 안됨)  
                if (0 > angleA)
                    angleA *= -1;
                if (0 > angleB)
                    angleB *= -1;


                if (angleH > angleA && angleH > angleB)
                {   //최고점 위영역에 앵커 두개가 있을 때의 예외처리 

                    //최고점과 가까운 각도 찾기 
                    if (angleA > angleB)
                    {
                        angleA = 91f;
                    }
                    else
                    {
                        angleB = 91f;
                    }
                }
                if (angleH < angleA && angleH < angleB)
                {   //최고점 아래영역에 앵커 두개가 있을 떄의 예외처리 

                    if (angleA < angleB)
                    {
                        angleA = 89f;
                    }
                    else
                    {
                        angleB = 89f;
                    }
                }



                float td = this.Calc_Td(t, angleA, 90f, angleB, angleTarget);
                return model.origin + tdDir * (model.radius + td);
            }



            public Vector3 CollisionPos_Anchor(float rotateAngle)
            {

                //늘어남계수 = 원점에서 최고점까지의 길이 - 반지름 
                float t = model.length - model.radius;

                Vector3 upDir = Vector3.Cross(model.anchorA - model.origin, model.anchorB - model.origin);
                upDir.Normalize();

                //최고점 기준으로 좌우90,90도 최대 180도를 표현한다 
                Vector3 initialDir = Quaternion.AngleAxis(-90f, upDir) * model.dir;
                initialDir.Normalize();

                float angleA = Vector3.SignedAngle(initialDir, model.anchorA - model.origin, upDir);
                float angleB = Vector3.SignedAngle(initialDir, model.anchorB - model.origin, upDir);
                //float angleA = 45f;
                //float angleB = 135f;
                float angleH = 90f;

                //-1~-179 각도표현을 1~179 로 변환한다
                //각도가 음수영역으로 들어가면 양수영역 각도로 변환한다 (각도가 음수영역으로 들어가면 궤적이 올바르게 표현이 안됨)  
                if (0 > angleA)
                    angleA *= -1;
                if (0 > angleB)
                    angleB *= -1;


                if (angleH > angleA && angleH > angleB)
                {   //최고점 위영역에 앵커 두개가 있을 때의 예외처리 

                    //최고점과 가까운 각도 찾기 
                    if (angleA > angleB)
                    {
                        angleA = 91f;
                    }
                    else
                    {
                        angleB = 91f;
                    }
                }
                if (angleH < angleA && angleH < angleB)
                {   //최고점 아래영역에 앵커 두개가 있을 떄의 예외처리 

                    if (angleA < angleB)
                    {
                        angleA = 89f;
                    }
                    else
                    {
                        angleB = 89f;
                    }
                }


                Vector3 tdDir = Quaternion.AngleAxis(rotateAngle, upDir) * initialDir;

                float td = this.Calc_Td(t, angleA, 90f, angleB, rotateAngle);
                return model.origin + tdDir * (model.radius + td);
            }

            //plusPos : 중요한 인자 아님. 단순히 다른위치에 나타내고 싶을때 사용하는 값임 
            public void Draw_WithAnchor()
            {
                Vector3 prev = Vector3.zero;
                Vector3 cur = Vector3.zero;
                int count = 36;
                for (int i = 0; i < count; i++)
                {
                    cur = CollisionPos_Anchor(i * 10);

                    if (0 != i)
                        DebugWide.DrawLine(prev, cur, Color.cyan);

                    prev = cur;
                }

                //=============================
                //늘어남계수 = 원점에서 최고점까지의 길이 - 반지름 
                float t = model.length - model.radius;

                Vector3 upDir = Vector3.Cross(model.anchorA - model.origin, model.anchorB - model.origin);
                upDir.Normalize();

                //최고점 기준으로 좌우90,90도 최대 180도를 표현한다 
                Vector3 initialDir = Quaternion.AngleAxis(-90f, upDir) * model.dir;
                initialDir.Normalize();
                //----------- debug print -----------
                Vector3 angle_M45 = initialDir;
                Vector3 angle_P45 = Quaternion.AngleAxis(180f, upDir) * initialDir;
                DebugWide.DrawLine(model.origin, model.origin + angle_M45 * model.radius, Color.red);
                DebugWide.DrawLine(model.origin, model.origin + angle_P45 * model.radius, Color.red);
                //----------- debug print -----------
                //DebugWide.DrawCircle(dPos + sphereCenter, sphereRadius, Color.black);
                DebugWide.DrawLine(model.origin, model.anchorA, Color.gray);
                DebugWide.DrawLine(model.origin, model.anchorB, Color.gray);

                DebugWide.DrawLine(model.anchorA, model.origin + model.dir * model.length, Color.green);
                DebugWide.DrawLine(model.anchorB, model.origin + model.dir * model.length, Color.green);
                DebugWide.DrawLine(model.origin, model.origin + model.dir * model.length, Color.red);
                //----------- debug print -----------

            }

            public void Draw(Color cc)//(Vector3 upDir, Color cc)
            {
                //=============================
                //늘어남계수 = 원점에서 최고점까지의 길이 - 반지름 
                //float t = model.length - model.radius;

                //최고점 기준으로 좌우90,90도 최대 180도를 표현한다 
                //Vector3 initialDir = Quaternion.AngleAxis(-90f, model.upDir) * model.dir;
                //initialDir.Normalize();
                //=============================

                Vector3 prev = Vector3.zero;
                Vector3 cur = Vector3.zero;
                Vector3 targetPos = Vector3.zero;
                float deltaLength = 100f; //이 값이 클수록 상세히 표현된다 
                int count = 36;
                for (int i = 0; i < count; i++)
                {
                    targetPos = Quaternion.AngleAxis(i * 10f, model.upDir) * Vector3.forward * model.length * deltaLength; //최고점 보다 밖에 targetPos가 있어야 한다  
                    cur = CollisionPos(targetPos);

                    if (0 != i)
                        DebugWide.DrawLine(prev, cur, cc);

                    prev = cur;
                }


                //----------- debug print -----------
                //Vector3 angle_M45 = initialDir;
                //Vector3 angle_P45 = Quaternion.AngleAxis(180f, model.upDir) * initialDir;
                //DebugWide.DrawLine(model.origin, model.origin + angle_M45 * model.radius, cc);
                //DebugWide.DrawLine(model.origin, model.origin + angle_P45 * model.radius, cc);
                //----------- debug print -----------
                DebugWide.DrawLine(model.origin, model.origin + model.dir * model.length, cc);
                DebugWide.DrawLine(model.origin, model.origin + model.upDir, cc);
                //----------- debug print -----------
            }
        }

        //평면상의 이차원 회오리를 정의 
        public class Tornado
        {

            public Model model = null;

            //public float radius;
            //public Vector3 dir; //회오리 시작방향
            //public float length; //회오리 반지름
            //public float maxAngle; //최대회전각

            //public Tornado()
            //{
            //    base.kind = Model.Tornado;
            //}

            public void Set(Vector3 p_upDir, Vector3 p_orign, float p_radius, Vector3 p_highestPoint, Vector3 p_unlaceDir, float p_maxAngle)
            {
                model.upDir = p_upDir;
                //model.upDir = VOp.Normalize(p_upDir); //test

                model.origin = p_orign;
                model.radius = p_radius;

                model.length = (p_highestPoint - p_orign).magnitude;

                if (model.length <= float.Epsilon)
                {
                    model.length = model.radius;
                    model.dir = Vector3.zero;
                }

                else
                    model.dir = (p_highestPoint - p_orign);
                //model.dir = (p_highestPoint - p_orign) / model.length;

                if (0 == p_maxAngle) p_maxAngle = 1f; //0나누기 연산을 피하기 위한 예외처리 
                model.maxAngle = p_maxAngle;
                model.unlaceDir = p_unlaceDir;
                this.Trans_UnlaceDir();
            }

            //회오리 자체의 방향을 바꾸는 것이 아님. 회오리 풀어지는 방향만 특정 방향으로 바꾸는 것임  
            private void Trans_UnlaceDir()
            {
                Vector3 cur_dir = Vector3.Cross(model.dir, model.upDir);
                if (Vector3.Dot(cur_dir, model.unlaceDir) < 0)
                {
                    model.upDir *= -1f;
                    //model.maxAngle *= -1f;
                }


            }

            //private Vector3 Trans_UnlaceDir(Vector3 unlace_dir, Vector3 upDir, Vector3 forward)
            //{
            //    Vector3 cur_dir = Vector3.Cross(forward, upDir);
            //    if (Vector3.Dot(cur_dir, unlace_dir) < 0)
            //        upDir *= -1f;
            //    return upDir;
            //}



            //2차원 회오리상의 목표위치만 구하는 함수 , 최대점으로 3차원 표현시 올바로 계산 못해줌 
            public Vector3 CollisionPos2D(Vector3 handlePos) //Vector3 n_upDir)
            {
                //늘어남계수 = 원점에서 최고점까지의 길이 - 반지름 
                float t = model.length - model.radius;
                //if (0 == t) t = 0.0001f; //0으로 나누는 문제를 피하기 위한 예외처리 

                //==================================================
                Vector3 centerToTarget = handlePos - model.origin;
                float t_target = centerToTarget.magnitude - model.radius; //target_pos에 대한 td를 바로 구한다 
                float t_angleD = (t_target * model.maxAngle) / t;

                if (t_angleD > model.maxAngle) t_angleD = model.maxAngle; //최대각도 이상 계산을 막는다 
                else if (t_angleD < 0) t_angleD = 0;

                //==================================================

                Vector3 initialDir = Quaternion.AngleAxis(360f - model.maxAngle, model.upDir) * model.dir;
                initialDir.Normalize();


                //if (true == Misc.IsZero(initialDir)) return model.origin;//initialDir = Vector3.forward;
                //initialDir 이 0 값인겨우 NaN 에러가 발생함 , origin 값 반환하게 예외처리 

                //비례식을 이용하여 td 구하기 
                //angleD : td  = angleH : t
                //td * angleH = angleD * t
                //td = (angleD * t) / angleH
                //angleD = (td * angleH) / t 

                float angleD = Geo.Angle360(initialDir, centerToTarget, model.upDir); //upDir벡터 기준으로 두벡터의 최소각 반환 
                int weight = (int)((t_angleD - angleD) / 360f); //회오리 두께구하기 , angleD(첫번째 회오리 두께의 각도)를 빼지 않으면 회오리가 아닌 원이 된다 

                //DebugWide.LogBlue("  - 00  " + angleD + "   " + initialDir);

                angleD += weight * 360f; //회오리 두꼐에 따라 각도를 더한다 
                if (angleD > model.maxAngle) angleD -= 360f; //더한 각도가 최대범위를 벗어나면 한두께 아래 회오리를 선택한다 

                Vector3 tdPos = Quaternion.AngleAxis(angleD, model.upDir) * initialDir;
                //DebugWide.LogBlue(tdPos + "  - 11  ");
                float td = (angleD * t) / model.maxAngle;
                tdPos = model.origin + tdPos * (model.radius + td);


                //DebugWide.LogBlue(tdPos);
                return tdPos;
            }


            //n_upDir 은 정규화된 값이 들어와야 한다 
            public Vector3 CollisionPos3D(Vector3 handlePos)//Vector3 n_upDir)
            {
                //늘어남계수 = 원점에서 최고점까지의 길이 - 반지름 
                float t = model.length - model.radius;


                //==================================================
                //t, t_target 모두 upDir 기준으로 투영평면에 투영한 값을 사용해야 highest 의 높이 변화시에도 올바른 위치를 계산할 수 있다 
                Vector3 proj_centerToHighest = model.dir - model.upDir * Vector3.Dot(model.upDir, model.dir);
                float proj_highestPointLength = proj_centerToHighest.magnitude;
                float proj_t = proj_highestPointLength - model.radius; //proj_t 가 음수가 되는 경우 : 반지름 보다 작은 최고점길이 일때 예외처리가 현재 없다 


                Vector3 centerToTarget = handlePos - model.origin;
                Vector3 proj_target = centerToTarget - model.upDir * Vector3.Dot(model.upDir, centerToTarget);
                float t_target = proj_target.magnitude - model.radius; //target_pos에 대한 td를 바로 구한다.  proj_target 는 이미 벡터값이므로 다시 원점에서 출발하는 점으로 계산하면 안된다 
                float t_angleD = (t_target * model.maxAngle) / proj_t;

                if (t_angleD > model.maxAngle) t_angleD = model.maxAngle; //최대각도 이상 계산을 막는다 
                else if (t_angleD < 0) t_angleD = 0;

                //==================================================


                Vector3 initialDir = Quaternion.AngleAxis(360f - model.maxAngle, model.upDir) * model.dir;
                initialDir.Normalize();


                //비례식을 이용하여 td 구하기 
                //angleD : td  = angleH : t
                //td * angleH = angleD * t
                //td = (angleD * t) / angleH
                //angleD = (td * angleH) / t 
                //float angleH = circle_maxAngle; //이 각도 값이 클수록 회오리가 작아진다.

                //float angleD = Geo.Angle360_AxisRotate(initialDir, target_pos, n_upDir); //음수표현없이 양수로 반환  
                float angleD = Geo.Angle360_AxisRotate_Normal_Axis(initialDir, centerToTarget, model.upDir);
                int weight = (int)((t_angleD - angleD) / 360f); //회오리 두께구하기 , angleD(첫번째 회오리 두께의 각도)를 빼지 않으면 회오리가 아닌 원이 된다 


                angleD += weight * 360f; //회오리 두꼐에 따라 각도를 더한다 
                if (angleD > model.maxAngle) angleD -= 360f; //더한 각도가 최대범위를 벗어나면 한두께 아래 회오리를 선택한다 


                Vector3 tdPos = Quaternion.AngleAxis(angleD, model.upDir) * initialDir;
                float td = (angleD * t) / model.maxAngle;
                tdPos = model.origin + tdPos * (model.radius + td);

                return tdPos;
            }

            public void Draw_T2AndAngle2(Vector3 handlePos)
            {

                float t = model.length - model.radius;

                float t_td = (handlePos - model.origin).magnitude - model.radius; //target_pos에 대한 td를 바로 구한다 
                float t_angleD = (t_td * model.maxAngle) / t;

                Vector3 initialDir = Quaternion.AngleAxis(360f - model.maxAngle, model.upDir) * model.dir;
                initialDir.Normalize();


                Vector3 tdPos = Quaternion.AngleAxis(t_angleD, model.upDir) * initialDir;
                tdPos = model.origin + tdPos * (model.radius + t_td);

                DebugWide.DrawLine(model.origin, tdPos, Color.black);

            }

            public void Draw(Color cc)//(Vector3 upDir, Color cc)
            {

                //=================================

                //늘어남계수 = 원점에서 최고점까지의 길이 - 반지름 
                float t = model.length - model.radius;

                //Vector3 initialDir = centerToHighestPoint / highestPointLength;
                Vector3 initialDir = Quaternion.AngleAxis(360f - model.maxAngle, model.upDir) * model.dir;
                initialDir.Normalize();

                //==================================================

                //비례식을 이용하여 td 구하기 
                //angleD : td  = angleH : t
                //td * angleH = angleD * t
                //td = (angleD * t) / angleH
                float minAngle = 0;
                float angleH = model.maxAngle; //이 각도 값이 클수록 회오리가 작아진다. 
                float angleD = 0f;
                float count = 300; //5
                Vector3 prevPos = model.origin;
                Vector3 tdPos = model.origin;

                /*for (int i = 0; i < count; i++)
                {

                    //5도 간격으로 각도를 늘린다 
                    angleD = i * 5f; //계속 증가하는 각도 .. 파도나치 수열의 소용돌이 모양이 나옴 

                    tdPos = Quaternion.AngleAxis(angleD, upDir) * initialDir;

                    float td = (angleD * t) / angleH;

                    tdPos = circle_pos + tdPos * (circle_radius + td);
                    //tdPos = this.DeformationCirclePos_Tornado(angleD, circle_pos, circle_radius, upDir, circle_highest, circle_maxAngle);
                    //tdPos = this.DeformationCirclePos_Tornado(tdPos, circle_pos, circle_radius, upDir, circle_highest, circle_maxAngle);

                    //----------- debug print -----------
                    if (0 != i)
                        DebugWide.DrawLine(plus_pos + prevPos, plus_pos + tdPos, Color.gray);
                    //----------- debug print -----------

                    prevPos = tdPos;

                }*/
                //==================================================

                count = 30;
                for (int i = 0; i < count + 1; i++)
                {

                    //angleD = Mathf.LerpAngle(minAngle, maxAngle, i / (float)count); //180도 이상 계산못함 
                    angleD = Mathf.Lerp(minAngle, model.maxAngle, i / (float)count);
                    //DebugWide.LogBlue(i + " : " + angleD);
                    tdPos = Quaternion.AngleAxis(angleD, model.upDir) * initialDir;


                    float td = (angleD * t) / angleH;
                    //DebugWide.PrintText(tdPos * _radius, Color.black, " " + td + "  " + angleD);


                    tdPos = model.origin + tdPos * (model.radius + td);

                    //----------- debug print -----------
                    //DebugWide.DrawLine(target_pos + circle_pos, target_pos + tdPos, cc);
                    if (0 != i)
                        DebugWide.DrawLine(prevPos, tdPos, cc);
                    //----------- debug print -----------

                    prevPos = tdPos;

                }


                //----------- debug print -----------
                DebugWide.DrawCirclePlane(model.origin, model.radius, model.upDir, cc);
                //DebugWide.DrawCircle( model.origin, model.radius, cc);
                DebugWide.DrawLine(model.origin, model.origin + model.dir, cc);
            }
        }


        //실린더는 선분의 특징을 가지고 있다 
        public class Cylinder //: Model_Intergration
        {
            public Model model = null;
            //public float radius;       //시작점에서 가까운 원의 반지름 
            //public Vector3 dir;             //origin 원에서 다른원까지의 방향 
            //public float length;            //길이 
            //public float radius_far;        //시작점에서 먼 원의 반지름

            //public Cylinder()
            //{
            //    base.kind = Model.Cylinder;
            //}

            public void Set(Vector3 p_upDir, Vector3 p_orign, float p_radius_origin, Vector3 p_far_pos, float p_radius_far)
            {
                model.upDir = p_upDir;
                model.origin = p_orign;
                model.length = (p_far_pos - p_orign).magnitude;

                if (model.length <= float.Epsilon)
                    model.dir = Vector3.zero;
                else
                    model.dir = (p_far_pos - p_orign) / model.length;

                model.radius = p_radius_origin;
                model.radius_far = p_radius_far;
            }

            public Vector3 CollisionPos(Vector3 handlePos, out Vector3 upDir2)
            {
                Vector3 colPos = model.origin;
                Vector3 farPos = model.origin + model.dir * model.length;
                Vector3 toHandle = handlePos - model.origin;
                upDir2 = model.upDir;

                float test = Vector3.Dot(model.dir, toHandle);
                //방향값이 0일 경우 조기 검사후 반환한다 
                if (0 == test && true == UtilGS9.Misc.IsZero(model.dir))
                {   //먼원과 가까운원의 위치가 일치 할 경우 

                    float max = model.radius < model.radius_far ? model.radius_far : model.radius;
                    Vector3 proj_centerToHandle = toHandle - model.upDir * Vector3.Dot(model.upDir, toHandle) / model.upDir.sqrMagnitude;

                    return model.origin + proj_centerToHandle.normalized * max; //큰쪽원 위치를 구하고 바로 반환     
                }

                //dir 의 방향에 맞게 upDir을 새로 구한다 
                Vector3 dirRight = Vector3.Cross(model.upDir, model.dir);
                upDir2 = Vector3.Cross(model.dir, dirRight);

                //nearToPos 를 upDir2 공간에 투영해야 한다 
                Vector3 proj_originToHandle = toHandle - upDir2 * Vector3.Dot(upDir2, toHandle) / upDir2.sqrMagnitude;
                proj_originToHandle.Normalize();
                Vector3 proj_handleMaxPos = model.origin + proj_originToHandle * (model.radius_far + model.length); //반직선의 초기위치가 너무 크면 결과값이 이상하게 나온다. 확인필요


                if (test < 0)
                {   //가까운 반구 충돌위치 찾기

                    colPos = model.origin + proj_originToHandle * model.radius;
                }
                else
                {
                    dirRight.Normalize();
                    if (0 > Vector3.Dot(dirRight, toHandle))
                        dirRight = dirRight * -1; //dirRight 를 toHandle 쪽을 바라보게 방향을 바꾸기 위한 용도 

                    UtilGS9.LineSegment3 line1 = new LineSegment3(model.origin, proj_handleMaxPos); //실린더 안에 nearToPos 가 못있게 연장된 최대위치값을 사용한다 
                    UtilGS9.LineSegment3 line2 = new LineSegment3(model.origin + dirRight * model.radius, farPos + dirRight * model.radius_far);
                    Vector3 pt0, pt1;
                    UtilGS9.LineSegment3.ClosestPoints(out pt0, out pt1, line1, line2);

                    colPos = pt0;

                    //DebugWide.DrawCircle(pt0, 1f, Color.gray);
                    //DebugWide.DrawCircle(pt1, 1f, Color.gray);
                    //DebugWide.DrawLine(pt0, pt1, Color.gray);
                    //DebugWide.DrawLine(this.pos + dirRight * radius_near, farPos + dirRight * radius_far, Color.red);


                    //먼 원안에 pt0이 포함되는지 검사한다 
                    //if((farPos - pt0).sqrMagnitude <= radius_far*radius_far)
                    if (false == UtilGS9.Misc.IsZero(pt0 - pt1))
                    {   //먼 반구 충돌위치 찾기
                        Vector3 interPos;
                        UtilGS9.Geo.IntersectRay2(farPos, model.radius_far, proj_handleMaxPos, -proj_originToHandle, out interPos);
                        colPos = interPos;

                        //DebugWide.DrawLine(this.pos, proj_handleMaxPos, Color.red);
                        //DebugWide.LogBlue(farPos + "  " + radius_far + "  " + toHandlePos);
                    }
                }

                return colPos;
            }


            public void Draw(Color cc)//(Vector3 upDir, Color cc)
            {

                //Color cc = Color.white;

                Vector3 farPos = model.origin + model.dir * model.length;

                Vector3 dirRight = Vector3.Cross(model.upDir, model.dir);
                dirRight.Normalize();
                Vector3 upDir2 = Vector3.Cross(model.dir, dirRight);
                upDir2.Normalize();

                if (true == UtilGS9.Misc.IsZero(model.dir))
                {
                    upDir2 = model.upDir;
                }

                DebugWide.DrawLine(model.origin, farPos, cc);
                DebugWide.DrawLine(model.origin + dirRight * model.radius, farPos + dirRight * model.radius_far, cc);
                DebugWide.DrawLine(model.origin + -dirRight * model.radius, farPos + -dirRight * model.radius_far, cc);
                //DebugWide.DrawCircle(cld.pos, cld.radius_near, cc);
                //DebugWide.DrawCircle(farPos, cld.radius_far, cc);
                DebugWide.DrawCirclePlane(model.origin, model.radius, upDir2, cc);
                DebugWide.DrawCirclePlane(farPos, model.radius_far, upDir2, cc);
            }

            public override string ToString()
            {

                return "origin: " + model.origin + "  dir: " + model.dir + "  length: " + model.length
                                         + "  radius_origin: " + model.radius + "  radius_far: " + model.radius_far;
            }
        }

    }//end geo

    //======================================================


}//end namespace

