using UnityEngine;
using UtilGS9;
using Buckland;

namespace Raven
{
    public class ShotGun : Raven_Weapon
    {

        //how much shot the each shell contains
        int m_iNumBallsInShell;

        //how much the shot spreads out when a cartridge is discharged
        float m_dSpread;

    
        public ShotGun(Raven_Bot owner) : base((int)eObjType.shotgun,
                                   Params.ShotGun_DefaultRounds,
                                   Params.ShotGun_MaxRoundsCarried,
                                   Params.ShotGun_FiringFreq,
                                   Params.ShotGun_IdealRange,
                                   Params.Pellet_MaxSpeed,
                                   owner)

            
        {
            m_iNumBallsInShell = Params.ShotGun_NumBallsInShell;
            m_dSpread = Params.ShotGun_Spread;

            //setup the vertex buffer
            const int NumWeaponVerts = 8;
            Vector3[] weapon = new Vector3[NumWeaponVerts]{
                                                   new Vector3(0, 0),
                                                   new Vector3(0, -2),
                                                   new Vector3(10, -2),
                                                   new Vector3(10, 0),
                                                   new Vector3(0, 0),
                                                   new Vector3(0, 2),
                                                   new Vector3(10, 2),
                                                   new Vector3(10, 0)
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
            //FuzzyVariable & DistanceToTarget = m_FuzzyModule.CreateFLV("DistanceToTarget");

            //FzSet & Target_Close = DistanceToTarget.AddLeftShoulderSet("Target_Close", 0, 25, 150);
            //FzSet & Target_Medium = DistanceToTarget.AddTriangularSet("Target_Medium", 25, 150, 300);
            //FzSet & Target_Far = DistanceToTarget.AddRightShoulderSet("Target_Far", 150, 300, 1000);

            //FuzzyVariable & Desirability = m_FuzzyModule.CreateFLV("Desirability");

            //FzSet & VeryDesirable = Desirability.AddRightShoulderSet("VeryDesirable", 50, 75, 100);
            //FzSet & Desirable = Desirability.AddTriangularSet("Desirable", 25, 50, 75);
            //FzSet & Undesirable = Desirability.AddLeftShoulderSet("Undesirable", 0, 25, 50);

            //FuzzyVariable & AmmoStatus = m_FuzzyModule.CreateFLV("AmmoStatus");
            //FzSet & Ammo_Loads = AmmoStatus.AddRightShoulderSet("Ammo_Loads", 30, 60, 100);
            //FzSet & Ammo_Okay = AmmoStatus.AddTriangularSet("Ammo_Okay", 0, 30, 60);
            //FzSet & Ammo_Low = AmmoStatus.AddTriangularSet("Ammo_Low", 0, 0, 30);


            //m_FuzzyModule.AddRule(FzAND(Target_Close, Ammo_Loads), VeryDesirable);
            //m_FuzzyModule.AddRule(FzAND(Target_Close, Ammo_Okay), VeryDesirable);
            //m_FuzzyModule.AddRule(FzAND(Target_Close, Ammo_Low), VeryDesirable);

            //m_FuzzyModule.AddRule(FzAND(Target_Medium, Ammo_Loads), VeryDesirable);
            //m_FuzzyModule.AddRule(FzAND(Target_Medium, Ammo_Okay), Desirable);
            //m_FuzzyModule.AddRule(FzAND(Target_Medium, Ammo_Low), Undesirable);

            //m_FuzzyModule.AddRule(FzAND(Target_Far, Ammo_Loads), Desirable);
            //m_FuzzyModule.AddRule(FzAND(Target_Far, Ammo_Okay), Undesirable);
            //m_FuzzyModule.AddRule(FzAND(Target_Far, Ammo_Low), Undesirable);
        }

        public override void Render()
        {
            Vector3 perp = Vector3.Cross(m_pOwner.Facing(), Vector3.up);
            Transformations.Draw_WorldTransform(m_vecWeaponVB,
                                            m_pOwner.Pos(),
                                            m_pOwner.Facing(),
                                            perp,
                                            m_pOwner.Scale(), Color.blue);


        }

        public override void ShootAt(Vector3 pos)
        {
            if (NumRoundsRemaining() > 0 && isReadyForNextShot())
            {
                //a shotgun cartridge contains lots of tiny metal balls called pellets. 
                //Therefore, every time the shotgun is discharged we have to calculate
                //the spread of the pellets and add one for each trajectory
                for (int b = 0; b < m_iNumBallsInShell; ++b)
                {
                    //determine deviation from target using a bell curve type distribution
                    float deviation = Misc.RandFloat(0, m_dSpread) + Misc.RandFloat(0, m_dSpread) - m_dSpread;

                    Vector3 AdjustedTarget = pos - m_pOwner.Pos();

                    //rotate the target vector by the deviation
                    Transformations.Vec3RotateYAroundOrigin(ref AdjustedTarget, deviation);

                    //add a pellet to the game world
                    m_pOwner.GetWorld().AddShotGunPellet(m_pOwner, AdjustedTarget + m_pOwner.Pos());

                }

                m_iNumRoundsLeft--;

                UpdateTimeWeaponIsNextAvailable();

                //add a trigger to the game so that the other bots can hear this shot
                //(provided they are within range)
                m_pOwner.GetWorld().GetMap().AddSoundTrigger(m_pOwner, Params.ShotGun_SoundRange);
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
            //    m_FuzzyModule.Fuzzify("AmmoStatus", (double)m_iNumRoundsLeft);

            //    m_dLastDesirabilityScore = m_FuzzyModule.DeFuzzify("Desirability", FuzzyModule::max_av);
            //}

            return m_dLastDesirabilityScore;
        }
}


}//end namespace

