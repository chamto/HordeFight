using UnityEngine;


namespace Raven
{
    public class ExploreGoal_Evaluator : Goal_Evaluator
    {

        public ExploreGoal_Evaluator(float bias) : base(bias) { }

        public float CalculateDesirability(Raven_Bot pBot)
        {
            float Desirability = 0.05f;

            Desirability *= m_dCharacterBias;

            return Desirability;
        }

        public void SetGoal(Raven_Bot pBot)
        {
            pBot.GetBrain().AddGoal_Explore();
        }

        public void RenderInfo(Vector3 Position, Raven_Bot pBot)
        {
            DebugWide.PrintText(Position, Color.black, "EX: " + CalculateDesirability(pBot));
        }
    }


}//end namespace

