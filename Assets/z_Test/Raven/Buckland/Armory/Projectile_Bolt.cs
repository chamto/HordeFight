using UnityEngine;
using UtilGS9;

namespace Raven
{
    //======================================================
    public class Bolt : Raven_Projectile
    {

        //tests the trajectory of the shell for an impact
        //void TestForImpact();


        public Bolt(Raven_Bot shooter, Vector3 target) :
                    base(target,
                         shooter.GetWorld(),
                         shooter.ID(),
                         shooter.Pos(),
                         shooter.Facing(),
                         Params.Bolt_Damage,
                         Params.Bolt_Scale,
                         Params.Bolt_MaxSpeed,
                         Params.Bolt_Mass,
                         Params.Bolt_MaxForce)
        { }

        override public void Render()
        {
            DebugWide.DrawLine(Pos(), Pos() - Velocity(), Color.green);
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


                //if the projectile has reached the target position or it hits an entity
                //or wall it should explode/inflict damage/whatever and then mark itself
                //as dead


                //test to see if the line segment connecting the bolt's current position
                //and previous position intersects with any bots.
                Raven_Bot hit = GetClosestIntersectingBot(m_vPosition - m_vVelocity,
                                                           m_vPosition);

                //if hit
                if (null != hit)
                {
                    m_bImpacted = true;
                    m_bDead = true;

                    //send a message to the bot to let it know it's been hit, and who the
                    //shot came from

                    SingleO.dispatcher.DispatchMsg(Const.SEND_MSG_IMMEDIATELY,
                                            m_iShooterID,
                                            hit.ID(),
                                            (int)eMsg.TakeThatMF,
                                            m_iDamageInflicted);
                }

                //test for impact with a wall
                float dist;
                if (Wall2D.FindClosestPointOfIntersectionWithWalls(m_vPosition - m_vVelocity,
                                                            m_vPosition,
                                                            out dist,
                                                            out m_vImpactPoint,
                                                            m_pWorld.GetMap().GetWalls()))
                {
                    m_bDead = true;
                    m_bImpacted = true;

                    m_vPosition = m_vImpactPoint;

                    return;
                }
            }
        }

    }

}//end namespace

