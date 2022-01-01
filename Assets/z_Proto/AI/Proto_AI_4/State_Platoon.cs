namespace Proto_AI_4
{
    //소대 - 분대들의 묶음 
    public class StateGlobal_Platoon : State<Character>
    {
        private StateGlobal_Platoon() { }


        //this is a singleton
        public static readonly StateGlobal_Platoon inst = new StateGlobal_Platoon();

        //public override void Enter(FieldPlayer* player) { }

        public override void Execute(Character player)
        {

        }

        //public override void Exit(Character player) { }

        public override bool OnMessage(Character player, Telegram telegram)
        {

            return false;
        }
    }

    //------------------------------------------------------------------------

    public class State_Move_Platoon : State<Character>
    {
        private State_Move_Platoon() { }

        //this is a singleton
        public static readonly State_Move_Platoon inst = new State_Move_Platoon();

        public override void Enter(Character player)
        {

        }

        public override void Execute(Character player)
        {

        }

        public override void Exit(Character player)
        {

        }

        public override bool OnMessage(Character f, Telegram t) { return false; }
    }

}//end namespace



