using System.Collections.Generic;
using UnityEngine;
using UtilGS9;


namespace Raven
{
    //======================================================

    public class Goal_Think : Goal_Composite<Raven_Bot>
    {
        public class GoalEvaluators : List<Goal_Evaluator> { }

        GoalEvaluators m_Evaluators = new GoalEvaluators();

        public int ItemTypeToGoalType(int gt)
        {
            switch (gt)
            {
                case (int)eObjType.health:

                    return (int)eGoal.get_health;

                case (int)eObjType.shotgun:

                    return (int)eGoal.get_shotgun;

                case (int)eObjType.rail_gun:

                    return (int)eGoal.get_railgun;

                case (int)eObjType.rocket_launcher:

                    return (int)eGoal.get_rocket_launcher;

                    //default: throw std::runtime_error("Goal_GetItem cannot determine item type");

            }//end switch

            DebugWide.LogRed("Goal_GetItem cannot determine item type");
            return -1;
        }

        public Goal_Think(Raven_Bot pBot) : base(pBot, (int)eGoal.think)
        {

            //these biases could be loaded in from a script on a per bot basis
            //but for now we'll just give them some random values
            const float LowRangeOfBias = 0.5f;
            const float HighRangeOfBias = 1.5f;

            float HealthBias = Misc.RandFloat(LowRangeOfBias, HighRangeOfBias);
            float ShotgunBias = Misc.RandFloat(LowRangeOfBias, HighRangeOfBias);
            float RocketLauncherBias = Misc.RandFloat(LowRangeOfBias, HighRangeOfBias);
            float RailgunBias = Misc.RandFloat(LowRangeOfBias, HighRangeOfBias);
            float ExploreBias = Misc.RandFloat(LowRangeOfBias, HighRangeOfBias);
            float AttackBias = Misc.RandFloat(LowRangeOfBias, HighRangeOfBias);

            //create the evaluator objects
            m_Evaluators.Add(new GetHealthGoal_Evaluator(HealthBias));
            m_Evaluators.Add(new ExploreGoal_Evaluator(ExploreBias));
            m_Evaluators.Add(new AttackTargetGoal_Evaluator(AttackBias));
            m_Evaluators.Add(new GetWeaponGoal_Evaluator(ShotgunBias,
                                                     (int)eObjType.shotgun));
            m_Evaluators.Add(new GetWeaponGoal_Evaluator(RailgunBias,
                                                     (int)eObjType.rail_gun));
            m_Evaluators.Add(new GetWeaponGoal_Evaluator(RocketLauncherBias,
                                                     (int)eObjType.rocket_launcher));
        }

        //this method iterates through each goal evaluator and selects the one
        //that has the highest score as the current goal
        public void Arbitrate()
        {

            //Goal_Evaluator cur = m_Evaluators[0];
            //cur.CalculateDesirability(m_pOwner);
            //return;

            float best = 0;
            Goal_Evaluator MostDesirable = null;

            //iterate through all the evaluators to see which produces the highest score
            foreach (Goal_Evaluator curDes in m_Evaluators)
            {
                float desirabilty = curDes.CalculateDesirability(m_pOwner);

                if (desirabilty >= best)
                {
                    best = desirabilty;
                    MostDesirable = curDes;
                }
            }

            //assert(MostDesirable && "<Goal_Think::Arbitrate>: no evaluator selected");

            MostDesirable.SetGoal(m_pOwner);
        }

        //returns true if the given goal is not at the front of the subgoal list
        public bool notPresent(int GoalType)
        {
            if (0 != m_SubGoals.Count)
            {

                return m_SubGoals.First.Value.GetType() != GoalType;
            }

            return true;
        }

        //the usual suspects
        override public int Process()
        {

            ActivateIfInactive();

            int SubgoalStatus = ProcessSubgoals();

            if (SubgoalStatus == (int)eStatus.completed || SubgoalStatus == (int)eStatus.failed)
            {
                if (!m_pOwner.isPossessed())
                {
                    m_iStatus = (int)eStatus.inactive;
                }
            }

            return m_iStatus;
        }

        override public void Activate()
        {
            if (!m_pOwner.isPossessed())
            {
                Arbitrate();
            }

            m_iStatus = (int)eStatus.active;
        }
        override public void Terminate() { }


        //top level goal types
        public void AddGoal_MoveToPosition(Vector3 pos)
        {
            AddSubgoal(new Goal_MoveToPosition(m_pOwner, pos));
        }

        public void AddGoal_GetItem(int ItemType)
        {
            if (notPresent(ItemTypeToGoalType(ItemType)))
            {
                RemoveAllSubgoals();
                AddSubgoal(new Goal_GetItem(m_pOwner, ItemType));
            }
        }

        public void AddGoal_Explore()
        {
            if (notPresent((int)eGoal.explore))
            {
                RemoveAllSubgoals();
                AddSubgoal(new Goal_Explore(m_pOwner));
            }
        }
        public void AddGoal_AttackTarget()
        {
            if (notPresent((int)eGoal.attack_target))
            {
                RemoveAllSubgoals();
                AddSubgoal(new Goal_AttackTarget(m_pOwner));
            }
        }

        //this adds the MoveToPosition goal to the *back* of the subgoal list.
        public void QueueGoal_MoveToPosition(Vector3 pos)
        {
            m_SubGoals.AddLast(new Goal_MoveToPosition(m_pOwner, pos));
        }

        //this renders the evaluations (goal scores) at the specified location
        public void RenderEvaluations(Vector3 pos)
        {
            //gdi->TextColor(Cgdi::black);
            //DebugWide.LogBlue(m_Evaluators.Count + "  " + m_SubGoals.Count);

            foreach (Goal_Evaluator cur in m_Evaluators)
            {
                cur.RenderInfo(pos, m_pOwner);
                pos.z -= 2;
            }
        }

        override public void Render()
        {
            foreach (Goal<Raven_Bot> cur in m_SubGoals)
            {
                cur.Render();
            }

        }


    }


}//end namespace

