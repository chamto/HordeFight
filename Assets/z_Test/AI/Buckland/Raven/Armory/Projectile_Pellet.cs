using UnityEngine;
using UtilGS9;

namespace Raven
{
    //======================================================


    public class Pellet : Raven_Projectile
    {

        //when this projectile hits something it's trajectory is rendered
        //for this amount of time
        float m_dTimeShotIsVisible;

        //tests the trajectory of the pellet for an impact
        void TestForImpact()
        {
            //a shot gun shell is an instantaneous projectile so it only gets the chance
            //to update once 
            m_bImpacted = true;

            //first find the closest wall that this ray intersects with. Then we
            //can test against all entities within this range.
            float DistToClosestImpact;
            Wall2D.FindClosestPointOfIntersectionWithWalls(m_vOrigin,
                                                    m_vPosition,
                                                    out DistToClosestImpact,
                                                    out m_vImpactPoint,
                                                    m_pWorld.GetMap().GetWalls());

            //test to see if the ray between the current position of the shell and 
            //the start position intersects with any bots.
            Raven_Bot hit = GetClosestIntersectingBot(m_vOrigin, m_vImpactPoint);

            //if no bots hit just return;
            if (null == hit) return;

            //determine the impact point with the bot's bounding circle so that the
            //shell can be rendered properly
            Geometry.GetLineSegmentCircleClosestIntersectionPoint(m_vOrigin,
                                                         m_vImpactPoint,
                                                         hit.Pos(),
                                                         hit.BRadius(),
                                                         ref m_vImpactPoint);

            //send a message to the bot to let it know it's been hit, and who the
            //shot came from
            SingleO.dispatcher.DispatchMsg(Const.SEND_MSG_IMMEDIATELY,
                                        m_iShooterID,
                                        hit.ID(),
                                        (int)eMsg.TakeThatMF,
                                        m_iDamageInflicted);
        }

        //returns true if the shot is still to be rendered
        bool isVisibleToPlayer() { return Time.time < m_dTimeOfCreation + m_dTimeShotIsVisible; }


        public Pellet(Raven_Bot shooter, Vector3 target) : base(target,
                         shooter.GetWorld(),
                         shooter.ID(),
                         shooter.Pos(),
                         shooter.Facing(),
                         Params.Pellet_Damage,
                         Params.Pellet_Scale,
                         Params.Pellet_MaxSpeed,
                         Params.Pellet_Mass,
                         Params.Pellet_MaxForce)

        {
            m_dTimeShotIsVisible = Params.Pellet_Persistance;
        }

        override public void Render()
        {
            if (isVisibleToPlayer() && m_bImpacted)
            {

                DebugWide.DrawLine(m_vOrigin, m_vImpactPoint, Color.yellow);

                DebugWide.DrawCircle(m_vImpactPoint, 3, Color.magenta);
            }
        }

        override public void Update()
        {
            if (!HasImpacted())
            {
                //calculate the steering force
                Vector3 DesiredVelocity = VOp.Normalize(m_vTarget - Pos()) * MaxSpeed();

                Vector3 sf = DesiredVelocity - Velocity();

                //update the position
                Vector3 accel = sf / m_dMass;

                m_vVelocity += accel;

                //make sure vehicle does not exceed maximum velocity
                m_vVelocity = VOp.Truncate(m_vVelocity, m_dMaxSpeed);

                //update the position
                m_vPosition += m_vVelocity;

                TestForImpact();
            }
            else if (!isVisibleToPlayer())
            {
                m_bDead = true;
            }
        }

    }

}//end namespace

