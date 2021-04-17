using UnityEngine;
using UtilGS9;
using Buckland;

namespace Raven
{
    //======================================================

    public class Rocket : Raven_Projectile
    {

        //the radius of damage, once the rocket has impacted
        float m_dBlastRadius;

        //this is used to render the splash when the rocket impacts
        float m_dCurrentBlastRadius;

        //If the rocket has impacted we test all bots to see if they are within the 
        //blast radius and reduce their health accordingly
        void InflictDamageOnBotsWithinBlastRadius()
        {

            foreach (Raven_Bot curBot in m_pWorld.GetAllBots())
            {
                if ((Pos() - (curBot).Pos()).magnitude < m_dBlastRadius + (curBot).BRadius())
                {
                    //send a message to the bot to let it know it's been hit, and who the
                    //shot came from
                    MessageDispatcher.Instance().DispatchMsg(Const.SEND_MSG_IMMEDIATELY,
                                            m_iShooterID,
                                            (curBot).ID(),
                                            (int)eMsg.TakeThatMF,
                                            m_iDamageInflicted);

                }
            }
        }

        //tests the trajectory of the shell for an impact
        void TestForImpact()
        {

            //if the projectile has reached the target position or it hits an entity
            //or wall it should explode/inflict damage/whatever and then mark itself
            //as dead


            //test to see if the line segment connecting the rocket's current position
            //and previous position intersects with any bots.
            Raven_Bot hit = GetClosestIntersectingBot(m_vPosition - m_vVelocity, m_vPosition);

            //if hit
            if (null != hit)
            {
                m_bImpacted = true;

                //send a message to the bot to let it know it's been hit, and who the
                //shot came from
                MessageDispatcher.Instance().DispatchMsg(Const.SEND_MSG_IMMEDIATELY,
                                        m_iShooterID,
                                        hit.ID(),
                                        (int)eMsg.TakeThatMF,
                                        m_iDamageInflicted);

                //test for bots within the blast radius and inflict damage
                InflictDamageOnBotsWithinBlastRadius();
            }

            //test for impact with a wall
            float dist;
            if (Wall2D.FindClosestPointOfIntersectionWithWalls(m_vPosition - m_vVelocity,
                                                        m_vPosition,
                                                        out dist,
                                                        out m_vImpactPoint,
                                                        m_pWorld.GetMap().GetWalls()))
            {
                m_bImpacted = true;

                //test for bots within the blast radius and inflict damage
                InflictDamageOnBotsWithinBlastRadius();

                m_vPosition = m_vImpactPoint;

                return;
            }

            //test to see if rocket has reached target position. If so, test for
            //all bots in vicinity
            const float tolerance = 5.0f;
            if ((Pos() - m_vTarget).sqrMagnitude < tolerance * tolerance)
            {
                m_bImpacted = true;

                InflictDamageOnBotsWithinBlastRadius();
            }
        }


        public Rocket(Raven_Bot shooter, Vector3 target) : base(target,
                         shooter.GetWorld(),
                         shooter.ID(),
                         shooter.Pos(),
                         shooter.Facing(),
                         Params.Rocket_Damage,
                         Params.Rocket_Scale,
                         Params.Rocket_MaxSpeed,
                         Params.Rocket_Mass,
                         Params.Rocket_MaxForce)

        {
            m_dCurrentBlastRadius = 0;
            m_dBlastRadius = Params.Rocket_BlastRadius;
            //assert(target != Vector2D());
        }

        override public void Render()
        {

            DebugWide.DrawCircle(Pos(), 2, Color.red);

            if (m_bImpacted)
            {
                DebugWide.DrawCircle(Pos(), m_dCurrentBlastRadius, Color.red);
            }
        }

        override public void Update()
        {
            if (!m_bImpacted)
            {
                m_vVelocity = MaxSpeed() * Heading();

                //make sure vehicle does not exceed maximum velocity
                m_vVelocity = VOp.Truncate(m_vVelocity, m_dMaxSpeed);

                //update the position
                m_vPosition += m_vVelocity;

                TestForImpact();
            }

            else
            {
                m_dCurrentBlastRadius += Params.Rocket_ExplosionDecayRate;

                //when the rendered blast circle becomes equal in size to the blast radius
                //the rocket can be removed from the game
                if (m_dCurrentBlastRadius > m_dBlastRadius)
                {
                    m_bDead = true;
                }
            }
        }

    }

}//end namespace

