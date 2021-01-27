using UnityEngine;
using UtilGS9;


namespace Raven
{
    public class Goal_AttackTarget : Goal_Composite<Raven_Bot>
    {

        public Goal_AttackTarget(Raven_Bot pOwner) : base(pOwner, (int)eGoal.attack_target)
        { }

        override public void Activate()
        {
            m_iStatus = (int)eStatus.active;

            //if this goal is reactivated then there may be some existing subgoals that
            //must be removed
            RemoveAllSubgoals();

            //it is possible for a bot's target to die whilst this goal is active so we
            //must test to make sure the bot always has an active target
            if (!m_pOwner.GetTargetSys().isTargetPresent())
            {
                m_iStatus = (int)eStatus.completed;

                return;
            }

            //if the bot is able to shoot the target (there is LOS between bot and
            //target), then select a tactic to follow while shooting
            if (m_pOwner.GetTargetSys().isTargetShootable())
            {
                //if the bot has space to strafe then do so
                Vector3 dummy = ConstV.v3_zero;
                if (m_pOwner.canStepLeft(dummy) || m_pOwner.canStepRight(dummy))
                {
                    AddSubgoal(new Goal_DodgeSideToSide(m_pOwner));
                }

                //if not able to strafe, head directly at the target's position 
                else
                {
                    AddSubgoal(new Goal_SeekToPosition(m_pOwner, m_pOwner.GetTargetBot().Pos()));
                }
            }

            //if the target is not visible, go hunt it.
            else
            {
                AddSubgoal(new Goal_HuntTarget(m_pOwner));
            }
        }

        override public int Process()
        {
            //if status is inactive, call Activate()
            ActivateIfInactive();

            //process the subgoals
            m_iStatus = ProcessSubgoals();

            ReactivateIfFailed();

            return m_iStatus;
        }

        override public void Terminate() { m_iStatus = (int)eStatus.completed; }

    }


}//end namespace

