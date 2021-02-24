using UnityEngine;
using UtilGS9;

namespace Raven
{
    //====================================================

    public class Blaster : Raven_Weapon
    {

        public Blaster(Raven_Bot owner) :

                      base((int)eObjType.blaster,
                                   Params.Blaster_DefaultRounds,
                                   Params.Blaster_MaxRoundsCarried,
                                   Params.Blaster_FiringFreq,
                                   Params.Blaster_IdealRange,
                                   Params.Bolt_MaxSpeed,
                                   owner)
        {
            //setup the vertex buffer
            const int NumWeaponVerts = 4;
            Vector3[] weapon = new Vector3[NumWeaponVerts]{
                                                new Vector3(-1,0, 0),
                                                   new Vector3(-1,0, 10),
                                                   new Vector3(1,0, 10),
                                                   new Vector3(1,0, 0)
                                                   };


            for (int vtx = 0; vtx < NumWeaponVerts; ++vtx)
            {
                m_vecWeaponVB.Add(weapon[vtx]);
            }

            //setup the fuzzy module
            InitializeFuzzyModule();
        }

        public override void InitializeFuzzyModule()
        {
            //FuzzyVariable & DistToTarget = m_FuzzyModule.CreateFLV("DistToTarget");

            //FzSet & Target_Close = DistToTarget.AddLeftShoulderSet("Target_Close", 0, 25, 150);
            //FzSet & Target_Medium = DistToTarget.AddTriangularSet("Target_Medium", 25, 150, 300);
            //FzSet & Target_Far = DistToTarget.AddRightShoulderSet("Target_Far", 150, 300, 1000);

            //FuzzyVariable & Desirability = m_FuzzyModule.CreateFLV("Desirability");
            //FzSet & VeryDesirable = Desirability.AddRightShoulderSet("VeryDesirable", 50, 75, 100);
            //FzSet & Desirable = Desirability.AddTriangularSet("Desirable", 25, 50, 75);
            //FzSet & Undesirable = Desirability.AddLeftShoulderSet("Undesirable", 0, 25, 50);

            //m_FuzzyModule.AddRule(Target_Close, Desirable);
            //m_FuzzyModule.AddRule(Target_Medium, FzVery(Undesirable));
            //m_FuzzyModule.AddRule(Target_Far, FzVery(Undesirable));
        }

        public override void Render()
        {
            Vector3 perp = Vector3.Cross(m_pOwner.Facing(), ConstV.v3_up);
            Transformation.Draw_WorldTransform(m_vecWeaponVB,
                                            m_pOwner.Pos(),
                                            m_pOwner.Facing(),
                                            perp,
                                            m_pOwner.Scale(), Color.green);

        }

        public override void ShootAt(Vector3 pos)
        {
            if (isReadyForNextShot())
            {
                //fire!
                m_pOwner.GetWorld().AddBolt(m_pOwner, pos);

                UpdateTimeWeaponIsNextAvailable();

                //add a trigger to the game so that the other bots can hear this shot
                //(provided they are within range)
                m_pOwner.GetWorld().GetMap().AddSoundTrigger(m_pOwner, Params.Blaster_SoundRange);
            }
        }

        public override float GetDesirability(float DistToTarget)
        {
            //fuzzify distance and amount of ammo
            //m_FuzzyModule.Fuzzify("DistToTarget", DistToTarget);

            //m_dLastDesirabilityScore = m_FuzzyModule.DeFuzzify("Desirability", FuzzyModule::max_av);

            //return m_dLastDesirabilityScore;
            return 0;
        }
    }


    //====================================================
    public class RailGun : Raven_Weapon
    {

        public RailGun(Raven_Bot owner) :

                      base((int)eObjType.blaster,
                                   Params.Blaster_DefaultRounds,
                                   Params.Blaster_MaxRoundsCarried,
                                   Params.Blaster_FiringFreq,
                                   Params.Blaster_IdealRange,
                                   Params.Bolt_MaxSpeed,
                                   owner)
        {
        }
    }
    public class ShotGun : Raven_Weapon
    {

        public ShotGun(Raven_Bot owner) :

                      base((int)eObjType.blaster,
                                   Params.Blaster_DefaultRounds,
                                   Params.Blaster_MaxRoundsCarried,
                                   Params.Blaster_FiringFreq,
                                   Params.Blaster_IdealRange,
                                   Params.Bolt_MaxSpeed,
                                   owner)
        {
        }
    }
    public class RocketLauncher : Raven_Weapon
    {

        public RocketLauncher(Raven_Bot owner) :

                      base((int)eObjType.blaster,
                                   Params.Blaster_DefaultRounds,
                                   Params.Blaster_MaxRoundsCarried,
                                   Params.Blaster_FiringFreq,
                                   Params.Blaster_IdealRange,
                                   Params.Bolt_MaxSpeed,
                                   owner)
        {
        }
    }


}//end namespace

