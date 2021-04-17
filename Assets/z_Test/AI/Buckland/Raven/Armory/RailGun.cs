using UnityEngine;

namespace Raven
{
    //====================================================
    public class RailGun : Raven_Weapon
    {
    

        public RailGun(Raven_Bot owner) :base((int)eObjType.rail_gun,
                                   Params.RailGun_DefaultRounds,
                                   Params.RailGun_MaxRoundsCarried,
                                   Params.RailGun_FiringFreq,
                                   Params.RailGun_IdealRange,
                                   Params.Slug_MaxSpeed,
                                   owner)
        {
            

                //setup the vertex buffer
                const int NumWeaponVerts = 4;
                Vector3[] weapon = new Vector3[NumWeaponVerts]
                                                {new Vector3(0, -1),
                                                   new Vector3(10, -1),
                                                   new Vector3(10, 1),
                                                   new Vector3(0, 1)
                                                   };

          
                for (int vtx=0; vtx<NumWeaponVerts; ++vtx)
                {
                    m_vecWeaponVB.Add(weapon[vtx]);
                }

                //setup the fuzzy module
                InitializeFuzzyModule();

        }

        public override void InitializeFuzzyModule()
        {

            //FuzzyVariable & DistanceToTarget = m_FuzzyModule.CreateFLV("DistanceToTarget");

            //FzSet & Target_Close = DistanceToTarget.AddLeftShoulderSet("Target_Close", 0, 25, 150);
            //FzSet & Target_Medium = DistanceToTarget.AddTriangularSet("Target_Medium", 25, 150, 300);
            //FzSet & Target_Far = DistanceToTarget.AddRightShoulderSet("Target_Far", 150, 300, 1000);

            //FuzzyVariable & Desirability = m_FuzzyModule.CreateFLV("Desirability");

            //FzSet & VeryDesirable = Desirability.AddRightShoulderSet("VeryDesirable", 50, 75, 100);
            //FzSet & Desirable = Desirability.AddTriangularSet("Desirable", 25, 50, 75);
            //FzSet & Undesirable = Desirability.AddLeftShoulderSet("Undesirable", 0, 25, 50);

            //FuzzyVariable & AmmoStatus = m_FuzzyModule.CreateFLV("AmmoStatus");
            //FzSet & Ammo_Loads = AmmoStatus.AddRightShoulderSet("Ammo_Loads", 15, 30, 100);
            //FzSet & Ammo_Okay = AmmoStatus.AddTriangularSet("Ammo_Okay", 0, 15, 30);
            //FzSet & Ammo_Low = AmmoStatus.AddTriangularSet("Ammo_Low", 0, 0, 15);



            //m_FuzzyModule.AddRule(FzAND(Target_Close, Ammo_Loads), FzFairly(Desirable));
            //m_FuzzyModule.AddRule(FzAND(Target_Close, Ammo_Okay), FzFairly(Desirable));
            //m_FuzzyModule.AddRule(FzAND(Target_Close, Ammo_Low), Undesirable);

            //m_FuzzyModule.AddRule(FzAND(Target_Medium, Ammo_Loads), VeryDesirable);
            //m_FuzzyModule.AddRule(FzAND(Target_Medium, Ammo_Okay), Desirable);
            //m_FuzzyModule.AddRule(FzAND(Target_Medium, Ammo_Low), Desirable);

            //m_FuzzyModule.AddRule(FzAND(Target_Far, Ammo_Loads), FzVery(VeryDesirable));
            //m_FuzzyModule.AddRule(FzAND(Target_Far, Ammo_Okay), FzVery(VeryDesirable));
            //m_FuzzyModule.AddRule(FzAND(Target_Far, FzFairly(Ammo_Low)), VeryDesirable);
        }

        public override void Render()
        {

            Vector3 perp = Vector3.Cross(m_pOwner.Facing(), Vector3.up);
            Transformation.Draw_WorldTransform(m_vecWeaponVB,
                                            m_pOwner.Pos(),
                                            m_pOwner.Facing(),
                                            perp,
                                            m_pOwner.Scale(), Color.blue);


        }

        public override void ShootAt(Vector3 pos)
        {
            if (NumRoundsRemaining() > 0 && isReadyForNextShot())
            {
                //fire a round
                m_pOwner.GetWorld().AddRailGunSlug(m_pOwner, pos);

                UpdateTimeWeaponIsNextAvailable();

                m_iNumRoundsLeft--;

                //add a trigger to the game so that the other bots can hear this shot
                //(provided they are within range)
                m_pOwner.GetWorld().GetMap().AddSoundTrigger(m_pOwner, Params.RailGun_SoundRange);
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
            //    m_FuzzyModule.Fuzzify("DistanceToTarget", DistToTarget);
            //    m_FuzzyModule.Fuzzify("AmmoStatus", m_iNumRoundsLeft);

            //    m_dLastDesirabilityScore = m_FuzzyModule.DeFuzzify("Desirability", FuzzyModule::max_av);
            //}

            return m_dLastDesirabilityScore;
        }
    }


}//end namespace

