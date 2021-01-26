using UnityEngine;


namespace Raven
{
    public class AttackTargetGoal_Evaluator : Goal_Evaluator
    {

        public AttackTargetGoal_Evaluator(float bias) : base(bias) { }

        public float CalculateDesirability(Raven_Bot pBot)
        {
            float Desirability = 0.0f;

            //only do the calculation if there is a target present
            if (pBot.GetTargetSys().isTargetPresent())
            {
                const float Tweaker = 1.0f;

                Desirability = Tweaker *
                               Feature.Health(pBot) *
                               Feature.TotalWeaponStrength(pBot);

                //bias the value according to the personality of the bot
                Desirability *= m_dCharacterBias;
            }

            return Desirability;
        }

        public void SetGoal(Raven_Bot pBot)
        {
            pBot.GetBrain().AddGoal_AttackTarget();
        }

        public void RenderInfo(Vector3 Position, Raven_Bot pBot)
        {
            DebugWide.PrintText(Position, Color.black, "AT: " + CalculateDesirability(pBot));
            //return;

            //std::string s = ttos(Raven_Feature::Health(pBot)) + ", " + ttos(Raven_Feature::TotalWeaponStrength(pBot));
            //gdi->TextAtPos(Position + Vector2D(0, 12), s);
        }
    }


}//end namespace

