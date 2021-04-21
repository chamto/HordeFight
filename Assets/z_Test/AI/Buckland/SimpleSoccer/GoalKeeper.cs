using UnityEngine;
using Buckland;


namespace Test_SimpleSoccer
{
    public class GoalKeeper : PlayerBase
    {
        public GoalKeeper(SoccerTeam home_team,
                     int home_region,
                     State<GoalKeeper> start_state,
                     Vector3 heading,
                     Vector3 velocity,
                     float mass,
                     float max_force,
                     float max_speed,
                     float max_turn_rate,
                     float scale) :
            base(home_team, home_region, heading, velocity, mass, max_force, max_speed, max_turn_rate, scale, player_role.goal_keeper)

        { }
    }
}

