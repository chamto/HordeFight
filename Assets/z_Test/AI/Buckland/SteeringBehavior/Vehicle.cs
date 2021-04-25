using System;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;
using Buckland;

namespace SteeringBehavior
{
    public class Vehicle : MovingEntity
    {

        //a pointer to the world data. So a vehicle can access any obstacle,
        //path, wall or agent data
        Test_SteeringBehavior m_pWorld; //Test_SteeringBehavior

        //the steering behavior class
        SteeringBehavior m_pSteering;


        //some steering behaviors give jerky looking movement. The
        //following members are used to smooth the vehicle's heading
        Buckland.Smoother<Vector2, Buckland.Vectoc2Calc> m_pHeadingSmoother;

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
            m_pHeadingSmoother = new Buckland.Smoother<Vector2, Buckland.Vectoc2Calc>(SingleO.prm.NumSamplesForSmoothing, new Vector2(0.0f, 0.0f));

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
                m_vecVehicleVBTrans = Transformations.WorldTransform(m_vecVehicleVB,
                                                     Pos(),
                                                     SmoothedHeading(),
                                                          VOp.Perp(SmoothedHeading()),
                                                     Scale());
            }

            else
            {
                m_vecVehicleVBTrans = Transformations.WorldTransform(m_vecVehicleVB,
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
}

