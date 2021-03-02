using UnityEngine;

namespace Raven
{
    public class RocketLauncher : Raven_Weapon
    {

    
        public RocketLauncher(Raven_Bot owner) : base((int)eObjType.rocket_launcher,
                                   Params.RocketLauncher_DefaultRounds,
                                   Params.RocketLauncher_MaxRoundsCarried,
                                   Params.RocketLauncher_FiringFreq,
                                   Params.RocketLauncher_IdealRange,
                                   Params.Rocket_MaxSpeed,
                                   owner)
        {
            //setup the vertex buffer
            const int NumWeaponVerts = 8;
            Vector3[] weapon = new Vector3[NumWeaponVerts] {
                                                    new Vector3(0, -3),
                                                   new Vector3(6, -3),
                                                   new Vector3(6, -1),
                                                   new Vector3(15, -1),
                                                   new Vector3(15, 1),
                                                   new Vector3(6, 1),
                                                   new Vector3(6, 3),
                                                   new Vector3(0, 3)
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

        //FuzzyVariable & AmmoStatus = m_FuzzyModule.CreateFLV("AmmoStatus");
        //FzSet & Ammo_Loads = AmmoStatus.AddRightShoulderSet("Ammo_Loads", 10, 30, 100);
        //FzSet & Ammo_Okay = AmmoStatus.AddTriangularSet("Ammo_Okay", 0, 10, 30);
        //FzSet & Ammo_Low = AmmoStatus.AddTriangularSet("Ammo_Low", 0, 0, 10);


        //m_FuzzyModule.AddRule(FzAND(Target_Close, Ammo_Loads), Undesirable);
        //m_FuzzyModule.AddRule(FzAND(Target_Close, Ammo_Okay), Undesirable);
        //m_FuzzyModule.AddRule(FzAND(Target_Close, Ammo_Low), Undesirable);

        //m_FuzzyModule.AddRule(FzAND(Target_Medium, Ammo_Loads), VeryDesirable);
        //m_FuzzyModule.AddRule(FzAND(Target_Medium, Ammo_Okay), VeryDesirable);
        //m_FuzzyModule.AddRule(FzAND(Target_Medium, Ammo_Low), Desirable);

        //m_FuzzyModule.AddRule(FzAND(Target_Far, Ammo_Loads), Desirable);
        //m_FuzzyModule.AddRule(FzAND(Target_Far, Ammo_Okay), Undesirable);
        //m_FuzzyModule.AddRule(FzAND(Target_Far, Ammo_Low), Undesirable);
    }

    public override void Render()
    {
            Vector3 perp = Vector3.Cross(m_pOwner.Facing(), Vector3.up);
            Transformation.Draw_WorldTransform(m_vecWeaponVB,
                                            m_pOwner.Pos(),
                                            m_pOwner.Facing(),
                                            perp,
                                            m_pOwner.Scale(), Color.red);

    }

        public override void ShootAt(Vector3 pos)
        {
            if (NumRoundsRemaining() > 0 && isReadyForNextShot())
            {
                //fire off a rocket!
                m_pOwner.GetWorld().AddRocket(m_pOwner, pos);

                m_iNumRoundsLeft--;

                UpdateTimeWeaponIsNextAvailable();

                //add a trigger to the game so that the other bots can hear this shot
                //(provided they are within range)
                m_pOwner.GetWorld().GetMap().AddSoundTrigger(m_pOwner, Params.RocketLauncher_SoundRange);
            }
        }

        public override float GetDesirability(float DistToTarget)
        {
            //if (m_iNumRoundsLeft == 0)
            //{
            //    m_dLastDesirabilityScore = 0;
            //}
            //else
            //{
            //    //fuzzify distance and amount of ammo
            //    m_FuzzyModule.Fuzzify("DistToTarget", DistToTarget);
            //    m_FuzzyModule.Fuzzify("AmmoStatus", (double)m_iNumRoundsLeft);

            //    m_dLastDesirabilityScore = m_FuzzyModule.DeFuzzify("Desirability", FuzzyModule::max_av);
            //}

            return m_dLastDesirabilityScore;
        }
    }


}//end namespace

