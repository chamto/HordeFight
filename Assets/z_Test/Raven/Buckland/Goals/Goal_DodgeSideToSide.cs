using UnityEngine;
using UtilGS9;

namespace Raven
{
    public class Goal_DodgeSideToSide : Goal<Raven_Bot>
    {

        Vector3 m_vStrafeTarget;

        bool m_bClockwise;

        //Vector3 GetStrafeTarget();


        public Goal_DodgeSideToSide(Raven_Bot pBot) : base(pBot, (int)eGoal.strafe)
        {
            m_bClockwise = Misc.RandBool();
        }


        override public void Activate()
        {
            m_iStatus = (int)eStatus.active;

            m_pOwner.GetSteering().SeekOn();


            if (m_bClockwise)
            {
                if (m_pOwner.canStepRight(out m_vStrafeTarget))
                {
                    m_pOwner.GetSteering().SetTarget(m_vStrafeTarget);
                }
                else
                {
                    //debug_con << "changing" << "";
                    m_bClockwise = !m_bClockwise;
                    m_iStatus = (int)eStatus.inactive;
                }
            }

            else
            {
                if (m_pOwner.canStepLeft(out m_vStrafeTarget))
                {
                    m_pOwner.GetSteering().SetTarget(m_vStrafeTarget);
                }
                else
                {
                    // debug_con << "changing" << "";
                    m_bClockwise = !m_bClockwise;
                    m_iStatus = (int)eStatus.inactive;
                }
            }


        }
        override public int Process()
        {
            //if status is inactive, call Activate()
            ActivateIfInactive();

            //if target goes out of view terminate
            if (!m_pOwner.GetTargetSys().isTargetWithinFOV())
            {
                m_iStatus = (int)eStatus.completed;
            }

            //else if bot reaches the target position set status to inactive so the goal 
            //is reactivated on the next update-step
            else if (m_pOwner.isAtPosition(m_vStrafeTarget))
            {
                m_iStatus = (int)eStatus.inactive;
            }

            return m_iStatus;
        }

        override public void Terminate()
        {
            m_pOwner.GetSteering().SeekOff();
        }

        override public void Render()
        {

            DebugWide.DrawLine(m_pOwner.Pos(), m_vStrafeTarget, Color.yellow);
            DebugWide.DrawCircle(m_vStrafeTarget, 3, Color.yellow);

        }

    }

}//end namespace

