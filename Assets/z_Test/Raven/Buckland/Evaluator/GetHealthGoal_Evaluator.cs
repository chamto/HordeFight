using UnityEngine;


namespace Raven
{
    public class GetHealthGoal_Evaluator : Goal_Evaluator
    {

        public GetHealthGoal_Evaluator(float bias) : base(bias) { }

        override public float CalculateDesirability(Raven_Bot pBot)
        {
            //first grab the distance to the closest instance of a health item
            float Distance = Feature.DistanceToItem(pBot, (int)eObjType.health);

            //if the distance feature is rated with a value of 1 it means that the
            //item is either not present on the map or too far away to be worth 
            //considering, therefore the desirability is zero
            if (Distance == 1f)
            {
                return 0;
            }
            else
            {
                //value used to tweak the desirability
                const float Tweaker = 0.2f;

                //the desirability of finding a health item is proportional to the amount
                //of health remaining and inversely proportional to the distance from the
                //nearest instance of a health item.
                float Desirability = Tweaker * (1 - Feature.Health(pBot)) /
                                    (Feature.DistanceToItem(pBot, (int)eObjType.health));

                //ensure the value is in the range 0 to 1
                Desirability = Mathf.Clamp(Desirability, 0, 1f);

                //bias the value according to the personality of the bot
                Desirability *= m_dCharacterBias;

                return Desirability;
            }
        }

        override public void SetGoal(Raven_Bot pBot)
        {
            pBot.GetBrain().AddGoal_GetItem((int)eObjType.health);
        }

        override public void RenderInfo(Vector3 Position, Raven_Bot pBot)
        {

            DebugWide.PrintText(Position, Color.black, "H: " + CalculateDesirability(pBot));
            //return;

            //std::string s = ttos(1 - Raven_Feature::Health(pBot)) + ", " + ttos(Raven_Feature::DistanceToItem(pBot, type_health));
            //gdi->TextAtPos(Position + Vector2D(0, 15), s);
        }
    }


}//end namespace

