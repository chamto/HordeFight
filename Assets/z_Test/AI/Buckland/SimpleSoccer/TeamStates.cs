using Buckland;

namespace Test_SimpleSoccer
{

    public class Attacking : State<SoccerTeam>
    { 
  
        private Attacking() { }


        //this is a singleton
        public static readonly Attacking instance = new Attacking();

        void ChangePlayerHomeRegions(SoccerTeam team, int[] NewRegions, int TeamSize)
        {
            for (int plyr=0; plyr<TeamSize; ++plyr)
            {
                team.SetPlayerHomeRegion(plyr, NewRegions[plyr]);
            }
        }

        int[] BlueRegions = new int[Const.TeamSize]{ 1, 12, 14, 6, 4 };
        int[] RedRegions = new int[Const.TeamSize]{ 16, 3, 5, 9, 13 };
        public override void Enter(SoccerTeam team)
        {
//# ifdef DEBUG_TEAM_STATES
//            debug_con << team->Name() << " entering Attacking state" << "";
//#endif

            //these define the home regions for this state of each of the players
            //const int BlueRegions[TeamSize] = { 1, 12, 14, 6, 4 };
            //const int RedRegions[TeamSize] = { 16, 3, 5, 9, 13 };

            //set up the player's home regions
            if (team.Color() == SoccerTeam.team_color.blue)
            {
                ChangePlayerHomeRegions(team, BlueRegions, Const.TeamSize);
            }
            else
            {
                ChangePlayerHomeRegions(team, RedRegions, Const.TeamSize);
            }

            //if a player is in either the Wait or ReturnToHomeRegion states, its
            //steering target must be updated to that of its new home region to enable
            //it to move into the correct position.
            team.UpdateTargetsOfWaitingPlayers();
        }

        public override void Execute(SoccerTeam team)
        {
            //if this team is no longer in control change states
            if (!team.InControl())
            {
                team.GetFSM().ChangeState(Defending.instance); return;
            }

            //calculate the best position for any supporting attacker to move to
            team.DetermineBestSupportingPosition();
        }

        public override void Exit(SoccerTeam team)
        {
            //there is no supporting player for defense
            team.SetSupportingPlayer(null);
        }

        public override bool OnMessage(SoccerTeam s, Telegram t) { return false; }
    }

//------------------------------------------------------------------------
    public  class Defending : State<SoccerTeam>
    { 
        private Defending() { }


    //this is a singleton
        public static readonly Defending instance = new Defending();


        void ChangePlayerHomeRegions(SoccerTeam team, int[] NewRegions, int TeamSize)
        {
            for (int plyr = 0; plyr < TeamSize; ++plyr)
            {
                team.SetPlayerHomeRegion(plyr, NewRegions[plyr]);
            }
        }
    
        int[] BlueRegions = new int[Const.TeamSize]{ 1, 6, 8, 3, 5 };
        int[] RedRegions = new int[Const.TeamSize]{ 16, 9, 11, 12, 14 };
        public override void Enter(SoccerTeam team)
        {
//# ifdef DEBUG_TEAM_STATES
            //debug_con << team->Name() << " entering Defending state" << "";
//#endif

            //these define the home regions for this state of each of the players
            //const int BlueRegions[TeamSize] = { 1, 6, 8, 3, 5 };
            //const int RedRegions[TeamSize] = { 16, 9, 11, 12, 14 };

            //set up the player's home regions
            if (team.Color() == SoccerTeam.team_color.blue)
            {
                ChangePlayerHomeRegions(team, BlueRegions, Const.TeamSize);
            }
            else
            {
                ChangePlayerHomeRegions(team, RedRegions , Const.TeamSize);
            }

            //if a player is in either the Wait or ReturnToHomeRegion states, its
            //steering target must be updated to that of its new home region
            team.UpdateTargetsOfWaitingPlayers();
        }

        public override void Execute(SoccerTeam team)
        {
            //if in control change states
            if (team.InControl())
            {
                team.GetFSM().ChangeState(Attacking.instance); return;
            }
        }

        //public override void Exit(SoccerTeam* team);

        public override bool OnMessage(SoccerTeam s, Telegram t) { return false; }
    }

//------------------------------------------------------------------------
    public class PrepareForKickOff : State<SoccerTeam>
    { 
        private PrepareForKickOff() { }

        //this is a singleton
        public static readonly PrepareForKickOff instance = new PrepareForKickOff();

        public override void Enter(SoccerTeam team)
        {
            //reset key player pointers
            team.SetControllingPlayer(null);
            team.SetSupportingPlayer(null);
            team.SetReceiver(null);
            team.SetPlayerClosestToBall(null);

            //send Msg_GoHome to each player.
            team.ReturnAllFieldPlayersToHome();
        }

        public override void Execute(SoccerTeam team)
        {
            //if both teams in position, start the game
            if (team.AllPlayersAtHome() && team.Opponents().AllPlayersAtHome())
            {
                team.GetFSM().ChangeState(Defending.instance);
            }
        }

        public override void Exit(SoccerTeam team)
        {
            team.Pitch().SetGameOn();
        }

        public override bool OnMessage(SoccerTeam s, Telegram t) { return false; }
    }

}

