using UnityEngine;


namespace Raven
{
    public class GetWeaponGoal_Evaluator : Goal_Evaluator
    {
        int m_iWeaponType;


        public GetWeaponGoal_Evaluator(float bias,
                              int WeaponType) : base(bias)
        {
            m_iWeaponType = WeaponType;
        }

        override public float CalculateDesirability(Raven_Bot pBot)
        {
            //grab the distance to the closest instance of the weapon type
            float Distance = Feature.DistanceToItem(pBot, m_iWeaponType);

            //if the distance feature is rated with a value of 1 it means that the
            //item is either not present on the map or too far away to be worth 
            //considering, therefore the desirability is zero
            if (Distance == 1)
            {
                return 0;
            }
            else
            {
                //value used to tweak the desirability
                const float Tweaker = 0.15f;

                float Health, WeaponStrength;

                Health = Feature.Health(pBot);

                WeaponStrength = Feature.IndividualWeaponStrength(pBot,
                                                                         m_iWeaponType);

                float Desirability = (Tweaker * Health * (1 - WeaponStrength)) / Distance;

                //ensure the value is in the range 0 to 1
                Desirability = Mathf.Clamp(Desirability, 0, 1f);

                Desirability *= m_dCharacterBias;

                return Desirability;
            }
        }

        override public void SetGoal(Raven_Bot pBot)
        {
            pBot.GetBrain().AddGoal_GetItem(m_iWeaponType);
        }

        override public void RenderInfo(Vector3 Position, Raven_Bot pBot)
        {
            string s = "GetWeaponGoal_Evaluator : ";
            switch (m_iWeaponType)
            {
                case (int)eObjType.rail_gun:
                    s += "RailGun: "; break;
                case (int)eObjType.rocket_launcher:
                    s += "RocketLauncher: "; break;
                case (int)eObjType.shotgun:
                    s += "ShotGun: "; break;
            }

            DebugWide.PrintText(Position, Color.black, s + CalculateDesirability(pBot));

        }
    }


}//end namespace

