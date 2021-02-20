namespace Raven
{
    //======================================================

    public class Params
    {

        //-------------------------[[General game parameters]]-------------------------
        //-------------------------------------------------------------------------------

        //--the number of bots the game instantiates

        public const int NumBots = 1;

        //--this is the maximum number of search cycles allocated to *all* current path
        //-- planning searches per update
        public const int MaxSearchCyclesPerUpdateStep = 1000;

        //--the name of the default map
        public static string StartMap = "Assets/z_Test/Raven/Buckland/Maps/Raven_DM1.map";
        //public static string StartMap = "/z_Test/Raven/Buckland/Maps/Raven_DM1.map";

        //--cell space partitioning defaults
        public const int NumCellsX = 10;
        public const int NumCellsY = 10;

        //--how long the graves remain on screen
        public const int GraveLifetime = 5;


        //-------------------------[[bot parameters]]----------------------------------
        //-------------------------------------------------------------------------------

        public const int Bot_MaxHealth = 100;
        public const float Bot_MaxSpeed = 1f;
        public const float Bot_Mass = 1;
        public const float Bot_MaxForce = 1.0f;
        public const float Bot_MaxHeadTurnRate = 0.2f;
        public const float Bot_Scale = 0.8f;

        //--special movement speeds(unused)
        public const float Bot_MaxSwimmingSpeed = Bot_MaxSpeed * 0.2f;
        public const float Bot_MaxCrawlingSpeed = Bot_MaxSpeed * 0.6f;

        //--the number of times a second a bot 'thinks' about weapon selection
        public const float Bot_WeaponSelectionFrequency = 2;

        //--the number of times a second a bot 'thinks' about changing strategy
        public const float Bot_GoalAppraisalUpdateFreq = 4;

        //--the number of times a second a bot updates its target info
        public const float Bot_TargetingUpdateFreq = 2;

        //--the number of times a second the triggers are updated
        public const float Bot_TriggerUpdateFreq = 8;

        //--the number of times a second a bot updates its vision
        public const float Bot_VisionUpdateFreq = 4;

        //--note that a frequency of -1 will disable the feature and a frequency of zero
        //--will ensure the feature is updated every bot update


        //--the bot's field of view (in degrees)
        public const float Bot_FOV = 180f;

        //--the bot's reaction time (in seconds)
        public const float Bot_ReactionTime = 0.2f;

        //--how long (in seconds) the bot will keep pointing its weapon at its target
        //--after the target goes out of view
        public const float Bot_AimPersistance = 1f;

        //--how accurate the bots are at aiming. 0 is very accurate, (the value represents
        //-- the max deviation in range(in radians))
        public const float Bot_AimAccuracy = 0.0f;

        //--how long a flash is displayed when the bot is hit
        public const float HitFlashTime = 0.2f;

        //--how long (in seconds) a bot's sensory memory persists
        public const float Bot_MemorySpan = 5f;

        //--goal tweakers
        public const float Bot_HealthGoalTweaker = 1.0f;
        public const float Bot_ShotgunGoalTweaker = 1.0f;
        public const float Bot_RailgunGoalTweaker = 1.0f;
        public const float Bot_RocketLauncherTweaker = 1.0f;
        public const float Bot_AggroGoalTweaker = 1.0f;


        //-------------------------[[steering parameters]]-----------------------------
        //-------------------------------------------------------------------------------

        //--use these values to tweak the amount that each steering force
        //--contributes to the total steering force
        public const float SeparationWeight = 10.0f;
        public const float WallAvoidanceWeight = 10.0f;
        public const float WanderWeight = 1.0f;
        public const float SeekWeight = 0.5f;
        public const float ArriveWeight = 1.0f;

        //--how close a neighbour must be before an agent considers it
        //--to be within its neighborhood(for separation)
        public const float ViewDistance = 15.0f;

        //--max feeler length
        public const float WallDetectionFeelerLength = 25.0f * Bot_Scale;

        //--used in path following.Determines how close a bot must be to a waypoint
        //--before it seeks the next waypoint
        public const float WaypointSeekDist = 5;

        //-------------------------[[giver-trigger parameters]]-----------------------------
        //-------------------------------------------------------------------------------

        //--how close a bot must be to a giver-trigger for it to affect it
        public const float DefaultGiverTriggerRange = 10;

        //--how many seconds before a giver-trigger reactivates itself
        public const float Health_RespawnDelay = 10;
        public const float Weapon_RespawnDelay = 15;


        //-------------------------[[weapon parameters]]-------------------------------
        //-------------------------------------------------------------------------------

        public const float Blaster_FiringFreq = 3;
        public const float Blaster_MaxSpeed = 5;
        public const float Blaster_DefaultRounds = 0; //--not used, a blaster always has ammo
        public const float Blaster_MaxRoundsCarried = 0; //--as above
        public const float Blaster_IdealRange = 50;
        public const float Blaster_SoundRange = 100;

        public const float Bolt_MaxSpeed = 5;
        public const float Bolt_Mass = 1;
        public const float Bolt_MaxForce = 100.0f;
        public const float Bolt_Scale = Bot_Scale;
        public const float Bolt_Damage = 1f;



        public const float RocketLauncher_FiringFreq = 1.5f;
        public const float RocketLauncher_DefaultRounds = 15;
        public const float RocketLauncher_MaxRoundsCarried = 50;
        public const float RocketLauncher_IdealRange = 150;
        public const float RocketLauncher_SoundRange = 400;

        public const float Rocket_BlastRadius = 20;
        public const float Rocket_MaxSpeed = 3;
        public const float Rocket_Mass = 1;
        public const float Rocket_MaxForce = 10.0f;
        public const float Rocket_Scale = Bot_Scale;
        public const float Rocket_Damage = 10;
        public const float Rocket_ExplosionDecayRate = 2.0f;   //--how fast the explosion occurs(in secs)


        public const float RailGun_FiringFreq = 1f;
        public const float RailGun_DefaultRounds = 15f;
        public const float RailGun_MaxRoundsCarried = 50;
        public const float RailGun_IdealRange = 200;
        public const float RailGun_SoundRange = 400;

        public const float Slug_MaxSpeed = 5000;
        public const float Slug_Mass = 0.1f;
        public const float Slug_MaxForce = 10000.0f;
        public const float Slug_Scale = Bot_Scale;
        public const float Slug_Persistance = 0.2f;
        public const float Slug_Damage = 10;



        public const float ShotGun_FiringFreq = 1;
        public const float ShotGun_DefaultRounds = 15;
        public const float ShotGun_MaxRoundsCarried = 50;
        public const float ShotGun_NumBallsInShell = 10;
        public const float ShotGun_Spread = 0.05f;
        public const float ShotGun_IdealRange = 100;
        public const float ShotGun_SoundRange = 400;

        public const float Pellet_MaxSpeed = 5000;
        public const float Pellet_Mass = 0.1f;
        public const float Pellet_MaxForce = 1000.0f;
        public const float Pellet_Scale = Bot_Scale;
        public const float Pellet_Persistance = 0.1f;
        public const float Pellet_Damage = 1;
    }


}//end namespace

