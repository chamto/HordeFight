using System.Collections.Generic;
using UnityEngine;
using UtilGS9;
using Buckland;

namespace Raven
{
    //======================================================

    public class Slug : Raven_Projectile
    {

        //when this projectile hits something it's trajectory is rendered
        //for this amount of time
        float m_dTimeShotIsVisible;

        //tests the trajectory of the shell for an impact
        void TestForImpact()
        {
            // a rail gun slug travels VERY fast. It only gets the chance to update once 
            m_bImpacted = true;

            //first find the closest wall that this ray intersects with. Then we
            //can test against all entities within this range.
            float DistToClosestImpact;
            Wall2D.FindClosestPointOfIntersectionWithWalls(m_vOrigin,
                                                    m_vPosition,
                                                    out DistToClosestImpact,
                                                    out m_vImpactPoint,
                                                    m_pWorld.GetMap().GetWalls());

            //test to see if the ray between the current position of the slug and 
            //the start position intersects with any bots.
            LinkedList<Raven_Bot> hits = GetListOfIntersectingBots(m_vOrigin, m_vPosition);

            //if no bots hit just return;
            if (0 == hits.Count) return;

            //give some damage to the hit bots
            foreach (Raven_Bot it in hits)
            {
                //send a message to the bot to let it know it's been hit, and who the
                //shot came from
                MessageDispatcher.Instance().DispatchMsg(Const.SEND_MSG_IMMEDIATELY,
                                        m_iShooterID,
                                        (it).ID(),
                                        (int)eMsg.TakeThatMF,
                                        m_iDamageInflicted);

            }
        }

        //returns true if the shot is still to be rendered
        bool isVisibleToPlayer() { return Time.time < m_dTimeOfCreation + m_dTimeShotIsVisible; }


        public Slug(Raven_Bot shooter, Vector3 target) : base(target,
                         shooter.GetWorld(),
                         shooter.ID(),
                         shooter.Pos(),
                         shooter.Facing(),
                         Params.Slug_Damage,
                         Params.Slug_Scale,
                         Params.Slug_MaxSpeed,
                         Params.Slug_Mass,
                         Params.Slug_MaxForce)

        {
            m_dTimeShotIsVisible = Params.Slug_Persistance;
        }

        override public void Render()
        {
            if (isVisibleToPlayer() && m_bImpacted)
            {
                DebugWide.DrawLine(m_vOrigin, m_vImpactPoint, Color.green);
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

                //make sure the slug does not exceed maximum velocity
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

