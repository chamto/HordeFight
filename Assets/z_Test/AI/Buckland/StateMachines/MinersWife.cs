namespace WestWorld
{
    //======================================================

    public class MinersWife : BaseGameEntity
    {

        //an instance of the state machine class
        StateMachine<MinersWife> m_pStateMachine;

        location_type m_Location;

        //is she presently cooking?
        bool m_bCooking;


        public MinersWife(int id) : base(id)
        {
            m_Location = location_type.shack;
            m_bCooking = false;

            //set up the state machine
            m_pStateMachine = new StateMachine<MinersWife>(this);

            m_pStateMachine.SetCurrentState(DoHouseWork.Instance);

            m_pStateMachine.SetGlobalState(WifesGlobalState.Instance);
        }


        //this must be implemented
        public override void Update()
        {
            m_pStateMachine.Update();
        }

        //so must this
        public override bool HandleMessage(Telegram msg)
        {
            return m_pStateMachine.HandleMessage(msg);
        }

        public StateMachine<MinersWife> GetFSM() { return m_pStateMachine; }

        //----------------------------------------------------accessors
        public location_type Location() { return m_Location; }
        public void ChangeLocation(location_type loc) { m_Location = loc; }

        public bool Cooking() { return m_bCooking; }
        public void SetCooking(bool val) { m_bCooking = val; }

    }
}

