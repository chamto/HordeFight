using System;
using System.Collections.Generic;
using UnityEngine;

public class Test_BVHNode : MonoBehaviour 
{
    Cyclone.BVHNodeDemo demo = null;
    Transform t_pos = null;

    private void Start()
    {
        demo = new Cyclone.BVHNodeDemo();
        //t_pos = GameObject.Find("t_pos").transform;
    }

    private void Update()
    {

        demo.key();
        demo.update();
    }

    private void OnDrawGizmos()
    {
        if (null == demo) return;
        demo.display();
    }
}

namespace Cyclone
{

    public class BVHNodeDemo : RigidBodyApplication
    {
        const int SPHERE_COUNT = 10;
        BSphere[] bSpheres = new BSphere[SPHERE_COUNT];

        const int POTENTIAL_COUNT = 50;
        PotentialContact[] potentialContacts = new PotentialContact[POTENTIAL_COUNT];

        BVHNode node0;

        Dictionary<RigidBody, BSphere> _find_BSphere = new Dictionary<RigidBody, BSphere>();

        public BVHNodeDemo()
        {
            for (int i = 0; i < POTENTIAL_COUNT; i++)
            {
                potentialContacts[i] = new PotentialContact();
                potentialContacts[i].Init();
            }

            for (int i=0;i< SPHERE_COUNT;i++ )
            {
                bSpheres[i] = new BSphere();

                //float rp = (float)random.randomReal(0, i * 3);
                //float rp1 = (float)random.randomReal(0, i * 3);
                float rp = 0.9f * i;
                float rp1 = 0.9f * i;
                bSpheres[i].setState(new Vector3(rp, rp1, 0), 1);

                //float flag = (float)Math.Pow(-1, i);
                //Vector3 v = new Vector3(1, flag, 0);
                //v = v * i * 0.8f;
                ////DebugWide.LogBlue(v);
                //v.x += i * 1.5f;
                //v.y += i * 1.5f * flag;
                //bSpheres[i].setState(v, 1);

                _find_BSphere.Add(bSpheres[i].body, bSpheres[i]); //강체를 키로 하는 정보구축
            }

        }

        public void ResetBVHNode()
        {
            //접촉가능성 정보 초기화 
            for (int i = 0; i < POTENTIAL_COUNT; i++)
            {
                potentialContacts[i].Reset();
            }

            //갱신안하고 새로 추가하는 처리라서 테스트를 위한 임시코드로만 사용 
            node0 = new BVHNode(null, bSpheres[0].getBoundingSphere(), bSpheres[0].body);
            for (int i = 1; i < SPHERE_COUNT; i++)
            {
                //DebugWide.LogBlue(i + "  " + node0.isLeaf());

                node0.insert(bSpheres[i].body, bSpheres[i].getBoundingSphere());
            }
        }

        protected override void updateObjects(float duration)
        {

            ResetBVHNode();

            for (int i = 0; i < SPHERE_COUNT; i++)
            {
                bSpheres[i].body.integrate(duration);
                bSpheres[i].calculateInternals();
            }
        }

        protected override void generateContacts()
        {
            cData.reset(maxContacts);
            cData.friction = 0.9f;
            cData.restitution = 0.6f;
            cData.tolerance = 0.1f;

            uint count = node0.getPotentialContacts(ref potentialContacts, SPHERE_COUNT);

            DebugWide.LogBlue(count);

            for (int i = 0; i < POTENTIAL_COUNT; i++)
            {
                if(null != potentialContacts[i].body[0])
                {
                    //if(_find_BSphere.ContainsKey(potentialContacts[i].body[0]))
                    //{
                    //    CollisionDetector.sphereAndSphere(
                    //        _find_BSphere[potentialContacts[i].body[0]],
                    //        _find_BSphere[potentialContacts[i].body[1]],
                    //        cData
                    //        );
                    //}

                }
            }
        }


        public void display()
        {
            for (int i = 0; i < SPHERE_COUNT; i++)
            {
                bSpheres[i].render(Color.blue);
            }

            for (int i = 0; i < POTENTIAL_COUNT; i++)
            {
                if (null != potentialContacts[i].body[0])
                {
                    if (_find_BSphere.ContainsKey(potentialContacts[i].body[0]))
                    {
                        _find_BSphere[potentialContacts[i].body[0]].render(Color.red);
                        _find_BSphere[potentialContacts[i].body[1]].render(Color.red);

                    }

                }
            }

            BVHNode.DrawTree(node0);

            base.drawDebug();
        }


    }

    public class BSphere : CollisionSphere
    {

        public BSphere()
        {
            body = new RigidBody();
        }

        public void render(Color cc)
        {
            DebugWide.DrawCircle(body.getPosition().ToUnity(), radius, cc);
        }

        public void setState(Vector3 pos, float r)
        {
            body.setMass(200.0f); // 200.0kg
            //body.setVelocity(0.0f, 30.0f, 40.0f); // 50m/s
            //body.setAcceleration(0.0f, -21.0f, 0.0f);
            body.setDamping(0.99f, 0.8f);
            radius = r;

            body.setCanSleep(false);
            body.setAwake(true);

            Matrix3 tensor = Matrix3.identityMatrix;
            float coeff = 0.4f * body.getMass() * radius * radius;
            tensor.setInertiaTensorCoeffs(coeff, coeff, coeff);
            body.setInertiaTensor(tensor);

            // Set the data common to all particle types
            body.setPosition(pos);

            // Clear the force accumulators
            body.calculateDerivedData();
            calculateInternals();

        }

        public BoundingSphere getBoundingSphere()
        {
            return new BoundingSphere(body.getPosition(), radius);
        }
    }
}