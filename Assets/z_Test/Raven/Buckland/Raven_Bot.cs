using UnityEngine;
using UtilGS9;


namespace Raven
{
    public class Raven_Bot : BaseGameEntity
    {

        Goal_Think m_pBrain;
        //the bot uses this to plan paths
        Raven_PathPlanner m_pPathPlanner;

        Raven_TargetingSystem m_pTargSys;
        Raven_WeaponSystem m_pWeaponSys;
        Raven_Game m_pWorld;

        int m_iHealth = 10;
        int m_iMaxHealth = 10;

        //set to true when a human player takes over control of the bot
        bool m_bPossessed;

        public Raven_Bot(Raven_Game world, int id) : base(id)
        {
            m_pWorld = world;
            m_pBrain = new Goal_Think(this);
            m_pPathPlanner = new Raven_PathPlanner(this);

            m_pTargSys = new Raven_TargetingSystem();
            m_pWeaponSys = new Raven_WeaponSystem();
        }



        override public void Update()
        {
            m_pBrain.Process();
        }

        public Vector3 Pos() { return ConstV.v3_zero; }

        public int Health() { return m_iHealth; }
        public int MaxHealth() { return m_iMaxHealth; }

        public bool isPossessed() { return m_bPossessed; }
        public bool hasLOSto(Vector3 pos) { return false; }

        public Raven_Game GetWorld() { return m_pWorld; }
        public Raven_TargetingSystem GetTargetSys() { return m_pTargSys; }
        public Raven_WeaponSystem GetWeaponSys() { return m_pWeaponSys; }
        public Goal_Think GetBrain() { return m_pBrain; }
        public Raven_PathPlanner GetPathPlanner() { return m_pPathPlanner; }
        public Raven_Bot GetTargetBot() { return m_pTargSys.GetTarget(); }
        public bool canStepLeft(Vector3 PositionOfStep) { return false; }
        public bool canStepRight(Vector3 PositionOfStep) { return false; }
        public bool canWalkBetween(Vector3 from, Vector3 to) { return false; }
        public bool canWalkTo(Vector3 pos) { return false; }
    }


}//end namespace

