using UnityEngine;


namespace Raven
{
    public class ExploreGoal_Evaluator : Goal_Evaluator
    {

        public ExploreGoal_Evaluator(float bias) : base(bias) { }

        override public float CalculateDesirability(Raven_Bot pBot)
        {
            float Desirability = 0.05f;

            Desirability *= m_dCharacterBias;

            return Desirability;
        }

        override public void SetGoal(Raven_Bot pBot)
        {
            pBot.GetBrain().AddGoal_Explore();
        }

        override public void RenderInfo(Vector3 Position, Raven_Bot pBot)
        {
            string s = "ExploreGoal_Evaluator : EX: ";
            DebugWide.PrintText(Position, Color.black, s + CalculateDesirability(pBot));
        }
    }


}//end namespace

