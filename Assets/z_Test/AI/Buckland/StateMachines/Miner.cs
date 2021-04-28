namespace WestWorld
{
    public class Miner : BaseGameEntity
    {

        //an instance of the state machine class
        StateMachine<Miner> m_pStateMachine;

        location_type m_Location;

        //how many nuggets the miner has in his pockets
        int m_iGoldCarried;

        int m_iMoneyInBank;

        //the higher the value, the thirstier the miner
        int m_iThirst;

        //the higher the value, the more tired the miner
        int m_iFatigue;


        public Miner(int id) : base(id)

        {
            m_Location = location_type.shack;
            m_iGoldCarried = 0;
            m_iMoneyInBank = 0;
            m_iThirst = 0;
            m_iFatigue = 0;


            //set up state machine
            m_pStateMachine = new StateMachine<Miner>(this);

            m_pStateMachine.SetCurrentState(GoHomeAndSleepTilRested.Instance);

            /* NOTE, A GLOBAL STATE HAS NOT BEEN IMPLEMENTED FOR THE MINER */
        }


        //this must be implemented
        public override void Update()
        {
            m_iThirst += 1;

            m_pStateMachine.Update();
        }

        //so must this
        public override bool HandleMessage(Telegram msg)
        {
            return m_pStateMachine.HandleMessage(msg);
        }


        public StateMachine<Miner> GetFSM() { return m_pStateMachine; }



        //-------------------------------------------------------------accessors
        public location_type Location() { return m_Location; }
        public void ChangeLocation(location_type loc) { m_Location = loc; }

        public int GoldCarried() { return m_iGoldCarried; }
        public void SetGoldCarried(int val) { m_iGoldCarried = val; }
        public void AddToGoldCarried(int val)
        {
            m_iGoldCarried += val;

            if (m_iGoldCarried < 0) m_iGoldCarried = 0;
        }
        public bool PocketsFull() { return m_iGoldCarried >= Const.MaxNuggets; }

        public bool Fatigued()
        {
            if (m_iFatigue > Const.TirednessThreshold)
            {
                return true;
            }

            return false;
        }

        public void DecreaseFatigue() { m_iFatigue -= 1; }
        public void IncreaseFatigue() { m_iFatigue += 1; }

        public int Wealth() { return m_iMoneyInBank; }
        public void SetWealth(int val) { m_iMoneyInBank = val; }
        public void AddToWealth(int val)
        {
            m_iMoneyInBank += val;

            if (m_iMoneyInBank < 0) m_iMoneyInBank = 0;
        }

        public bool Thirsty()
        {
            if (m_iThirst >= Const.ThirstLevel) { return true; }

            return false;
        }
        public void BuyAndDrinkAWhiskey() { m_iThirst = 0; m_iMoneyInBank -= 2; }

    }
}

