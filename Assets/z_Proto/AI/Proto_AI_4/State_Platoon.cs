namespace Proto_AI_4
{
    //소대 - 분대들의 묶음 
    public class StateGlobal_Platoon : State<Unit>
    {
        private StateGlobal_Platoon() { }


        //this is a singleton
        public static readonly StateGlobal_Platoon inst = new StateGlobal_Platoon();

        //public override void Enter(FieldPlayer* player) { }

        public override void Execute(Unit player)
        {

        }

        //public override void Exit(Character player) { }

        public override bool OnMessage(Unit player, Telegram telegram)
        {

            return false;
        }
    }

    //------------------------------------------------------------------------

    public class State_Move_Platoon : State<Unit>
    {
        private State_Move_Platoon() { }

        //this is a singleton
        public static readonly State_Move_Platoon inst = new State_Move_Platoon();

        public override void Enter(Unit player)
        {

        }

        public override void Execute(Unit player)
        {

        }

        public override void Exit(Unit player)
        {

        }

        public override bool OnMessage(Unit f, Telegram t) { return false; }
    }

}//end namespace



