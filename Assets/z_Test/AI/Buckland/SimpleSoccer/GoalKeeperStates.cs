using Buckland;

namespace Test_SimpleSoccer
{
    public class TendGoal : State<GoalKeeper>
    {
        private TendGoal() { }

        //this is a singleton
        public static readonly TendGoal instance = new TendGoal();


    }


}//end namespace

