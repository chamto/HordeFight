using UnityEngine;

namespace SteeringBehavior
{
    //======================================================
    [System.Serializable]
    public class Param
    {
        public int cxClient = 100;
        public int cyClient = 100;
        [Space]
        public int NumAgents = 300;
        public int NumObstacles = 7;
        public float MinObstacleRadius = 10;
        public float MaxObstacleRadius = 30;

        //number of horizontal cells used for spatial partitioning
        public int NumCellsX = 7;
        //number of vertical cells used for spatial partitioning
        public int NumCellsY = 7;

        //how many samples the smoother will use to average a value
        public int NumSamplesForSmoothing = 10;

        //used to tweak the combined steering force (simply altering the MaxSteeringForce
        //will NOT work!This tweaker affects all the steering force multipliers
        //too).
        //this is used to multiply the steering force AND all the multipliers
        //found in SteeringBehavior
        public float SteeringForceTweaker = 200;

        public float _MaxSteeringForce = 2;
        public float MaxSpeed = 150;
        public float VehicleMass = 1;

        public float VehicleScale = 3;
        public float MaxTurnRatePerSecond = Const.Pi;

        [Space]
        //use these values to tweak the amount that each steering force
        //contributes to the total steering force
        public float _SeparationWeight = 1;
        public float _AlignmentWeight = 1;
        public float _CohesionWeight = 2;
        public float _ObstacleAvoidanceWeight = 10;
        public float _WallAvoidanceWeight = 10;
        public float _WanderWeight = 1;
        public float _SeekWeight = 1;
        public float _FleeWeight = 1;
        public float _ArriveWeight = 1;
        public float _PursuitWeight = 1;
        public float _OffsetPursuitWeight = 1;
        public float _InterposeWeight = 1;
        public float _HideWeight = 1;
        public float _EvadeWeight = 0.01f;
        public float _FollowPathWeight = 0.05f;

        //SteeringForceTweaker 값이 곱해져 들어갈 변수임 , 아래 변수를 사용해야 함 
        [HideInInspector]
        public float MaxSteeringForce_2;
        [HideInInspector]
        public float SeparationWeight_2;
        [HideInInspector]
        public float AlignmentWeight_2;
        [HideInInspector]
        public float CohesionWeight_2;
        [HideInInspector]
        public float ObstacleAvoidanceWeight_2;
        [HideInInspector]
        public float WallAvoidanceWeight_2;
        [HideInInspector]
        public float WanderWeight_2;
        [HideInInspector]
        public float SeekWeight_2;
        [HideInInspector]
        public float FleeWeight_2;
        [HideInInspector]
        public float ArriveWeight_2;
        [HideInInspector]
        public float PursuitWeight_2;
        [HideInInspector]
        public float OffsetPursuitWeight_2;
        [HideInInspector]
        public float InterposeWeight_2;
        [HideInInspector]
        public float HideWeight_2;
        [HideInInspector]
        public float EvadeWeight_2;
        [HideInInspector]
        public float FollowPathWeight_2;

        [Space]
        //how close a neighbour must be before an agent perceives it (considers it
        //to be within its neighborhood)
        public float ViewDistance = 50f;

        //used in obstacle avoidance
        public float MinDetectionBoxLength = 40f;

        //used in wall avoidance
        public float WallDetectionFeelerLength = 40f;

        //these are the probabilities that a steering behavior will be used
        //when the prioritized dither calculate method is used
        public float prWallAvoidance = 0.5f;
        public float prObstacleAvoidance = 0.5f;
        public float prSeparation = 0.2f;
        public float prAlignment = 0.3f;
        public float prCohesion = 0.6f;
        public float prWander = 0.8f;
        public float prSeek = 0.8f;
        public float prFlee = 0.6f;
        public float prEvade = 1f;
        public float prHide = 0.8f;
        public float prArrive = 0.5f;

        //the radius of the constraining circle for the wander behavior
        public float WanderRad = 1.2f;
        //distance the wander circle is projected in front of the agent
        public float WanderDist = 2.0f;
        //the maximum amount of displacement along the circle each frame
        public float WanderJitterPerSec = 80.0f;
        //used in path following
        public float WaypointSeekDist = 20f;

        public void Update()
        {
            MaxSteeringForce_2 = SteeringForceTweaker * _MaxSteeringForce;

            SeparationWeight_2 = SteeringForceTweaker * _SeparationWeight;

            AlignmentWeight_2 = SteeringForceTweaker * _AlignmentWeight;

            CohesionWeight_2 = SteeringForceTweaker * _CohesionWeight;

            ObstacleAvoidanceWeight_2 = SteeringForceTweaker * _ObstacleAvoidanceWeight;

            WallAvoidanceWeight_2 = SteeringForceTweaker * _WallAvoidanceWeight;

            WanderWeight_2 = SteeringForceTweaker * _WanderWeight;

            SeekWeight_2 = SteeringForceTweaker * _SeekWeight;

            FleeWeight_2 = SteeringForceTweaker * _FleeWeight;

            ArriveWeight_2 = SteeringForceTweaker * _ArriveWeight;

            PursuitWeight_2 = SteeringForceTweaker * _PursuitWeight;

            OffsetPursuitWeight_2 = SteeringForceTweaker * _OffsetPursuitWeight;

            InterposeWeight_2 = SteeringForceTweaker * _InterposeWeight;

            HideWeight_2 = SteeringForceTweaker * _HideWeight;

            EvadeWeight_2 = SteeringForceTweaker * _EvadeWeight;

            FollowPathWeight_2 = SteeringForceTweaker * _FollowPathWeight;
        }
    }
}

