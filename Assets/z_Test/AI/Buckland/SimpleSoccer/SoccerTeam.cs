using System.Collections.Generic;

namespace Test_SimpleSoccer
{
    public class SoccerTeam
    {
        public SoccerTeam Opponents()
        {
            return null; 
        }

        public List<PlayerBase> Members()
        {
            return null; 
        }

        public SoccerPitch Pitch()
        {
            return null; 
        }

        public PlayerBase DetermineBestSupportingAttacker() { return null; }

        public PlayerBase SupportingPlayer() { return null; }

        public void SetSupportingPlayer(PlayerBase p) { }

        public Goal OpponentsGoal() { return null; }
        public Goal HomeGoal() { return null; }

        public PlayerBase ControllingPlayer() { return null; }

        public PlayerBase PlayerClosestToBall() { return null; }

        public float ClosestDistToBallSq() { return 0; }
    }
}//end namespace

