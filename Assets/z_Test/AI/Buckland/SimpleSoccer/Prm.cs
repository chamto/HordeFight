namespace Test_SimpleSoccer
{
    public static class Prm
    {
        //static public float Friction = -0.015f;
        //static public float PlayerKickingAccuracy = 0.99f;

        public const int GoalWidth = 100;

        //use to set up the sweet spot calculator
        public const int NumSweetSpotsX = 13; //NumSupportSpotsX
        public const int NumSweetSpotsY = 6; //NumSupportSpotsY

        //these values tweak the various rules used to calculate the support spots
        public const float Spot_CanPassScore = 2.0f; //Spot_PassSafeScore
        public const float Spot_CanScoreFromPositionScore = 1.0f;
        public const float Spot_DistFromControllingPlayerScore = 2.0f;
        public const float Spot_ClosenessToSupportingPlayerScore = 0.0f;
        public const float Spot_AheadOfAttackerScore = 0.0f;

        //how many times per second the support spots will be calculated
        public const int SupportSpotUpdateFreq = 1;

        //the chance a player might take a random pot shot at the goal
        public const float ChancePlayerAttemptsPotShot = 0.005f;

        //this is the chance that a player will receive a pass using the arrive
        //steering behavior, rather than Pursuit
        public const float ChanceOfUsingArriveTypeReceiveBehavior = 0.5f;

        public const float BallSize = 5.0f;
        public const float BallMass = 1.0f;
        public const float Friction = -0.015f;

        //the goalkeeper has to be this close to the ball to be able to interact with it
        public const float KeeperInBallRange = 10.0f;
        public const float PlayerInTargetRange = 10.0f;

        //player has to be this close to the ball to be able to kick it. The higher
        //the value this gets, the easier it gets to tackle. 
        public const float PlayerKickingDistance = 6.0f;

        //the number of times a player can kick the ball per second
        public const int PlayerKickFrequency = 8;

        public const float PlayerMass = 3.0f;
        public const float PlayerMaxForce = 1.0f;
        public const float PlayerMaxSpeedWithBall = 1.2f;
        public const float PlayerMaxSpeedWithoutBall = 1.6f;
        public const float PlayerMaxTurnRate = 0.4f;
        public const float PlayerScale = 1.0f;

        //when an opponents comes within this range the player will attempt to pass
        //the ball. Players tend to pass more often, the higher the value
        public const float PlayerComfortZone = 60.0f;

        //in the range zero to 1.0. adjusts the amount of noise added to a kick,
        //the lower the value the worse the players get.
        public const float PlayerKickingAccuracy = 0.99f;

        //the number of times the SoccerTeam::CanShoot method attempts to find
        //a valid shot
        public const int NumAttemptsToFindValidStrike = 5;

        public const float MaxDribbleForce = 1.5f;
        public const float MaxShootingForce = 6.0f;
        public const float MaxPassingForce = 3.0f;


        //the distance away from the center of its home region a player
        //must be to be considered at home
        public const float WithinRangeOfHome = 15.0f;

        //how close a player must get to a sweet spot before he can change state
        public const float WithinRangeOfSweetSpot = 15.0f; //WithinRangeOfSweetSpot

        //the minimum distance a receiving player must be from the passing player
        public const float MinPassDistance = 120.0f; //MinPassDist
        //the minimum distance a player must be from the goalkeeper before it will
        //pass the ball
        public const float GoalkeeperMinPassDistance = 50.0f; //GoalkeeperMinPassDist

        //this is the distance the keeper puts between the back of the net 
        //and the ball when using the interpose steering behavior
        public const float GoalKeeperTendingDistance = 20.0f;

        //when the ball becomes within this distance of the goalkeeper he
        //changes state to intercept the ball
        public const float GoalKeeperInterceptRange = 100.0f;

        //how close the ball must be to a receiver before he starts chasing it
        public const float BallWithinReceivingRange = 10.0f;

        //these (boolean) values control the amount of player and pitch info shown
        //1=ON; 0=OFF
        public static bool ViewStates = true; //bStates
        public static bool ViewIDs = true; //bIDs
        public static bool ViewSupportSpots = true; //bSupportSpots
        public static bool ViewRegions = false; //bRegions
        public static bool bShowControllingTeam = true;
        public static bool bViewTargets = false; //bViewTargets
        public static bool bHighlightIfThreatened = false; //bHighlightIfThreatened

        //simple soccer's physics are calculated using each tick as the unit of time
        //so changing this will adjust the speed
        public const int FrameRate = 60;


        //--------------------------------------------steering behavior stuff
        public const float SeparationCoefficient = 10.0f;

        //how close a neighbour must be to be considered for separation
        public const float ViewDistance = 30.0f;

        //1=ON; 0=OFF
        public static bool bNonPenetrationConstraint = false;

        public static bool SHOW_TEAM_STATE = false;
        public static bool SHOW_SUPPORTING_PLAYERS_TARGET = false;

        public const float BallWithinReceivingRangeSq = BallWithinReceivingRange* BallWithinReceivingRange;
        public const float KeeperInBallRangeSq = KeeperInBallRange* KeeperInBallRange;
        public const float PlayerInTargetRangeSq = PlayerInTargetRange* PlayerInTargetRange;
        public const float PlayerKickingDistanceSq = (BallSize + PlayerKickingDistance) * (BallSize + PlayerKickingDistance);
        public const float PlayerComfortZoneSq = PlayerComfortZone* PlayerComfortZone;
        public const float GoalKeeperInterceptRangeSq = GoalKeeperInterceptRange* GoalKeeperInterceptRange;
        public const float WithinRangeOfSupportSpotSq = WithinRangeOfSweetSpot * WithinRangeOfSweetSpot;
    }


}//end namespace

