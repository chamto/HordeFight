using Buckland;

namespace Test_SimpleSoccer
{
    public class Defending : State<SoccerTeam>
    {
        private Defending() { }

        //this is a singleton
        public static readonly Defending instance = new Defending();
    }

}

