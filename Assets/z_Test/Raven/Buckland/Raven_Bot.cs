using System;
using UnityEngine;
using System.Collections.Generic;
using UtilGS9;
//using DWORD = System.UInt32;

namespace Raven
{

    //*
    public class Raven_Bot : MovingEntity //BaseGameEntity
    {

        Goal_Think m_pBrain;

        Raven_Steering m_pSteering;

        //the bot uses this to plan paths
        Raven_PathPlanner m_pPathPlanner;

        Raven_TargetingSystem m_pTargSys;
        Raven_WeaponSystem m_pWeaponSys;
        Raven_Game m_pWorld;

        int m_iHealth = 10;
        int m_iMaxHealth = 10;

        //set to true when a human player takes over control of the bot
        bool m_bPossessed;

        Regulator m_pGoalArbitrationRegulator;

        public Raven_Bot(Raven_Game world, Vector3 pos) : base(pos,
               Params.Bot_Scale,
               ConstV.v3_zero,
               0.3f,//Params.Bot_MaxSpeed, 
               ConstV.v3_forward,
               Params.Bot_Mass,
               new Vector3(Params.Bot_Scale, Params.Bot_Scale, Params.Bot_Scale),
               Params.Bot_MaxHeadTurnRate,
               Params.Bot_MaxForce)
        {
            m_pWorld = world;
            m_pBrain = new Goal_Think(this);
            m_pPathPlanner = new Raven_PathPlanner(this);

            m_pSteering = new Raven_Steering(world, this);

            m_pTargSys = new Raven_TargetingSystem(this);
            m_pWeaponSys = new Raven_WeaponSystem(this, 0,0,0);

            m_pGoalArbitrationRegulator = new Regulator(5);
        }

        void UpdateMovement()
        {
            //calculate the combined steering force
            Vector3 force = m_pSteering.Calculate();

            //if no steering force is produced decelerate the player by applying a
            //braking force
            if (Misc.IsZero(m_pSteering.Force()))
            {
                const float BrakingRate = 0.8f;

                m_vVelocity = m_vVelocity * BrakingRate;
            }

            //calculate the acceleration
            Vector3 accel = force / m_dMass;

            //update the velocity
            m_vVelocity += accel;

            //DebugWide.LogBlue(m_vVelocity.magnitude + "   " + m_dMaxSpeed);
            //make sure vehicle does not exceed maximum velocity
            //m_vVelocity.Truncate(m_dMaxSpeed);
            m_vVelocity = VOp.Truncate(m_vVelocity, m_dMaxSpeed);

            //update the position
            m_vPosition += m_vVelocity;

            //if the vehicle has a non zero velocity the heading and side vectors must 
            //be updated
            if (!Misc.IsZero(m_vVelocity))
            {
                m_vHeading = VOp.Normalize(m_vVelocity);

                //m_vSide = m_vHeading.Perp();
                m_vSide = Vector3.Cross(m_vHeading, ConstV.v3_up);
            }
        }

        override public void Update()
        {
            UpdateMovement();
            m_pBrain.Process();

            if(m_pGoalArbitrationRegulator.isReady())
            {
                m_pBrain.Arbitrate();
                //DebugWide.LogBlue(Time.time);
            }
        }

        public void Render()
        {
            {

                DebugWide.DrawCircle(m_vPosition, BRadius() + 1, Color.red);
            }
        }
          

        public int Health() { return m_iHealth; }
        public int MaxHealth() { return m_iMaxHealth; }
        public float MaxSpeed() {return m_dMaxSpeed;}

        public bool isPossessed() { return m_bPossessed; }
        public bool isAtPosition(Vector3 pos)
        {
            const float tolerance = 1.0f;
            //DebugWide.LogWhite(Pos() + " __ " + pos + "________" +(Pos() - pos).sqrMagnitude + "  < " + (tolerance * tolerance));
            return (Pos() - pos).sqrMagnitude < tolerance * tolerance;
        }
        public bool hasLOSto(Vector3 pos) { return false; }

        public Raven_Game GetWorld() { return m_pWorld; }
        public Raven_Steering  GetSteering(){return m_pSteering;}
        public Raven_TargetingSystem GetTargetSys() { return m_pTargSys; }
        public Raven_WeaponSystem GetWeaponSys() { return m_pWeaponSys; }
        public Goal_Think GetBrain() { return m_pBrain; }
        public Raven_PathPlanner GetPathPlanner() { return m_pPathPlanner; }
        public Raven_Bot GetTargetBot() { return m_pTargSys.GetTarget(); }
        public bool canStepLeft(Vector3 PositionOfStep) { return false; }
        public bool canStepRight(Vector3 PositionOfStep) { return false; }
        public bool canWalkBetween(Vector3 from, Vector3 to) { return false; }
        public bool canWalkTo(Vector3 pos) { return false; }

        public float CalculateTimeToReachPosition(Vector3 pos)
        {
            return (Pos() - pos).magnitude / (MaxSpeed() * Const.FrameRate);
        }

        public Vector3 Facing() { return ConstV.v3_zero; }
        public float FieldOfView() { return 0f; }
        public void IncreaseHealth(int i) { }
        public bool isReadyForTriggerUpdate() { return false; }
        public bool isAlive() { return false; }

    }
    //*/
    
    /*
    public class Raven_Bot :  MovingEntity
    {
    
        public enum eStatus { alive, dead, spawning };

        //alive, dead or spawning?
        eStatus m_Status;

        //a pointer to the world data
        Raven_Game m_pWorld;

        //this object handles the arbitration and processing of high level goals
        Goal_Think m_pBrain;

        //this is a class that acts as the bots sensory memory. Whenever this
        //bot sees or hears an opponent, a record of the event is updated in the 
        //memory.
        Raven_SensoryMemory m_pSensoryMem;

        //the bot uses this object to steer
        Raven_Steering m_pSteering;

        //the bot uses this to plan paths
        Raven_PathPlanner m_pPathPlanner;

        //this is responsible for choosing the bot's current target
        Raven_TargetingSystem m_pTargSys;

        //this handles all the weapons. and has methods for aiming, selecting and
        //shooting them
        Raven_WeaponSystem m_pWeaponSys;

        //A regulator object limits the update frequency of a specific AI component
        Regulator m_pWeaponSelectionRegulator;
        Regulator m_pGoalArbitrationRegulator;
        Regulator m_pTargetSelectionRegulator;
        Regulator m_pTriggerTestRegulator;
        Regulator m_pVisionUpdateRegulator;

        //the bot's health. Every time the bot is shot this value is decreased. If
        //it reaches zero then the bot dies (and respawns)
        int m_iHealth;

        //the bot's maximum health value. It starts its life with health at this value
        int m_iMaxHealth;

        //each time this bot kills another this value is incremented
        int m_iScore;

        //the direction the bot is facing (and therefore the direction of aim). 
        //Note that this may not be the same as the bot's heading, which always
        //points in the direction of the bot's movement
        Vector3 m_vFacing;

        //a bot only perceives other bots within this field of view
        float m_dFieldOfView;

        //to show that a player has been hit it is surrounded by a thick 
        //red circle for a fraction of a second. This variable represents
        //the number of update-steps the circle gets drawn
        int m_iNumUpdatesHitPersistant;

        //set to true when the bot is hit, and remains true until 
        //m_iNumUpdatesHitPersistant becomes zero. (used by the render method to
        //draw a thick red circle around a bot to indicate it's been hit)
        bool m_bHit;

        //set to true when a human player takes over control of the bot
        bool m_bPossessed;

        //a vertex buffer containing the bot's geometry
        List<Vector3> m_vecBotVB = new List<Vector3>();
        //the buffer for the transformed vertices
        List<Vector3> m_vecBotVBTrans = null;


        //bots shouldn't be copied, only created or respawned
        //Raven_Bot(const Raven_Bot&);
        //Raven_Bot& operator=(const Raven_Bot&);

        //this method is called from the update method. It calculates and applies
        //the steering force for this time-step.
        void UpdateMovement()
        {
            //calculate the combined steering force
            Vector3 force = m_pSteering.Calculate();

            //if no steering force is produced decelerate the player by applying a
            //braking force
            if (Misc.IsZero( m_pSteering.Force()))
            {
                const float BrakingRate = 0.8f;

                m_vVelocity = m_vVelocity * BrakingRate;
            }

            //calculate the acceleration
            Vector3 accel = force / m_dMass;

            //update the velocity
            m_vVelocity += accel;

            //make sure vehicle does not exceed maximum velocity
            //m_vVelocity.Truncate(m_dMaxSpeed);
            m_vVelocity = VOp.Truncate(m_vVelocity, m_dMaxSpeed);

            //update the position
            m_vPosition += m_vVelocity;

            //if the vehicle has a non zero velocity the heading and side vectors must 
            //be updated
            if (!Misc.IsZero(m_vVelocity))
            {
                m_vHeading = VOp.Normalize(m_vVelocity);

                //m_vSide = m_vHeading.Perp();
                m_vSide = Vector3.Cross(m_vHeading, ConstV.v3_up);
            }
        }

        //initializes the bot's VB with its geometry
        void SetUpVertexBuffer()
        {
            //setup the vertex buffers and calculate the bounding radius
            const int NumBotVerts = 4;
            Vector3[] bot = new Vector3[NumBotVerts] {new Vector3(-3, 0, 8),
                                     new Vector3(3 , 0, 10),
                                     new Vector3(3 , 0, -10),
                                     new Vector3(-3 , 0, -8)};

            m_dBoundingRadius = 0.0f;
            float scale = Params.Bot_Scale;

            for (int vtx = 0; vtx < NumBotVerts; ++vtx)
            {
                m_vecBotVB.Add(bot[vtx]);

                //set the bounding radius to the length of the 
                //greatest extent
                if (Math.Abs(bot[vtx].x) * scale > m_dBoundingRadius)
                {
                    m_dBoundingRadius = Math.Abs(bot[vtx].x * scale);
                }

                if (Math.Abs(bot[vtx].y) * scale > m_dBoundingRadius)
                {
                    m_dBoundingRadius = Math.Abs(bot[vtx].y) * scale;
                }
            }
        }



        public Raven_Bot(Raven_Game world, Vector3 pos) :
            base(pos,
               Params.Bot_Scale,
               ConstV.v3_zero,
               Params.Bot_MaxSpeed,
               ConstV.v3_forward,
               Params.Bot_Mass,
               new Vector3(Params.Bot_Scale, Params.Bot_Scale, Params.Bot_Scale),
               Params.Bot_MaxHeadTurnRate,
               Params.Bot_MaxForce)
                 
        {

            m_iMaxHealth = Params.Bot_MaxHealth;
            m_iHealth = Params.Bot_MaxHealth;
            m_pPathPlanner = null;
            m_pSteering = null;
            m_pWorld = world;
            m_pBrain = null;
            m_iNumUpdatesHitPersistant = ((int)(Const.FrameRate * Params.HitFlashTime));
            m_bHit = false;
            m_iScore = 0;
            m_Status = eStatus.spawning;
            m_bPossessed = false;
            m_dFieldOfView = Params.Bot_FOV * Mathf.Deg2Rad;

            SetEntityType((int)eObjType.bot);

            SetUpVertexBuffer();

            //a bot starts off facing in the direction it is heading
            m_vFacing = m_vHeading;

            //create the navigation module
            m_pPathPlanner = new Raven_PathPlanner(this);

            //create the steering behavior class
            m_pSteering = new Raven_Steering(world, this);

            //create the regulators
            m_pWeaponSelectionRegulator = new Regulator(Params.Bot_WeaponSelectionFrequency);
            m_pGoalArbitrationRegulator =  new Regulator(Params.Bot_GoalAppraisalUpdateFreq);
            m_pTargetSelectionRegulator = new Regulator(Params.Bot_TargetingUpdateFreq);
            m_pTriggerTestRegulator = new Regulator(Params.Bot_TriggerUpdateFreq);
            m_pVisionUpdateRegulator = new Regulator(Params.Bot_VisionUpdateFreq);

            //create the goal queue
            m_pBrain = new Goal_Think(this);

            //create the targeting system
            m_pTargSys = new Raven_TargetingSystem(this);

            m_pWeaponSys = new Raven_WeaponSystem(this,
                                              Params.Bot_ReactionTime,
                                            Params.Bot_AimAccuracy,
                                        Params.Bot_AimPersistance);

            m_pSensoryMem = new Raven_SensoryMemory(this, Params.Bot_MemorySpan);
        }

    
        //the usual suspects
        public override void Render()
        {
            //when a bot is hit by a projectile this value is set to a constant user
            //defined value which dictates how long the bot should have a thick red
            //circle drawn around it (to indicate it's been hit) The circle is drawn
            //as long as this value is positive. (see Render)
            m_iNumUpdatesHitPersistant--;


            if (isDead() || isSpawning()) return;

            Vector3 perp = Vector3.Cross(m_vFacing, ConstV.v3_up);
            Transformation.Draw_WorldTransform(m_vecBotVB,
                                             Pos(),
                                             Facing(),
                                             perp,
                                             Scale(),
                                             Color.blue);



            //draw the head
            DebugWide.DrawCircle(Pos(), 6f * Scale().x, Color.magenta);

            //render the bot's weapon
            m_pWeaponSys.RenderCurrentWeapon();

            //render a thick red circle if the bot gets hit by a weapon
            if (m_bHit)
            {

                DebugWide.DrawCircle(m_vPosition, BRadius() + 1, Color.red);

                if (m_iNumUpdatesHitPersistant <= 0)
                {
                    m_bHit = false;
                }
            }

            Vector3 temp = Pos();
            if (UserOptions.m_bShowBotIDs)
            {
                temp.x -= 10;
                temp.z -= 20;
                DebugWide.PrintText(temp, Color.green, "" + ID());
            }

            if (UserOptions.m_bShowBotHealth)
            {
                temp.x -= 40;
                temp.z -= 5;
                DebugWide.PrintText(temp, Color.green, "" + Health());
            }

            if (UserOptions.m_bShowScore)
            {
                temp.x -= 40;
                temp.z += 10;
                DebugWide.PrintText(temp, Color.green, "" + Score());
            }
        }

        public override void Update()
        {
            //process the currently active goal. Note this is required even if the bot
            //is under user control. This is because a goal is created whenever a user 
            //clicks on an area of the map that necessitates a path planning request.
            m_pBrain.Process();

            //Calculate the steering force and update the bot's velocity and position
            UpdateMovement();

            //------------
            //m_pBrain.Arbitrate();
            //GetBrain().AddGoal_Explore();

            //Vector3 perp = Vector3.Cross(m_vFacing, ConstV.v3_up);
            //Transformation.Draw_WorldTransform(m_vecBotVB,
            //                                 Pos(),
            //                                 Facing(),
            //                                 perp,
            //                                 Scale(),
            //                                 Color.blue);



            ////draw the head
            //DebugWide.DrawCircle(Pos(), 6f * Scale().x, Color.magenta);


            //return;
            //------------

            //if the bot is under AI control but not scripted
            if (!isPossessed())
            {
                //examine all the opponents in the bots sensory memory and select one
                //to be the current target
                if (m_pTargetSelectionRegulator.isReady())
                {
                    m_pTargSys.Update();
                }

                //appraise and arbitrate between all possible high level goals
                if (m_pGoalArbitrationRegulator.isReady())
                {
                    m_pBrain.Arbitrate();
                }

                //update the sensory memory with any visual stimulus
                if (m_pVisionUpdateRegulator.isReady())
                {
                    m_pSensoryMem.UpdateVision();
                }

                //select the appropriate weapon to use from the weapons currently in
                //the inventory
                if (m_pWeaponSelectionRegulator.isReady())
                {
                    m_pWeaponSys.SelectWeapon();
                }

                //this method aims the bot's current weapon at the current target
                //and takes a shot if a shot is possible
                m_pWeaponSys.TakeAimAndShoot();
            }
        }

        public override bool HandleMessage(Telegram msg)
        {
          //first see if the current goal accepts the message
          if (GetBrain().HandleMessage(msg)) return true;
         
          //handle any messages not handles by the goals
          switch(msg.Msg)
          {
          case (int)eMsg.TakeThatMF:
                    {
                        //just return if already dead or spawning
                        if (isDead() || isSpawning()) return true;

                        //the extra info field of the telegram carries the amount of damage
                        ReduceHealth((int)(msg.ExtraInfo));

                        //if this bot is now dead let the shooter know
                        if (isDead())
                        {

                            SingleO.dispatcher.DispatchMsg(Const.SEND_MSG_IMMEDIATELY,
                                          ID(),
                                          msg.Sender,
                                          (int)eMsg.YouGotMeYouSOB,
                                          null);
                        }

                        return true;
                    }
          case (int)eMsg.YouGotMeYouSOB:
                    {
                        IncrementScore();

                        //the bot this bot has just killed should be removed as the target
                        m_pTargSys.ClearTarget();

                        return true;
                    }
          case (int)eMsg.GunshotSound:
                    {
                        //add the source of this sound to the bot's percepts
                        GetSensoryMem().UpdateWithSoundSource((Raven_Bot)msg.ExtraInfo);

                        return true;
                    }
          case (int)eMsg.UserHasRemovedBot:
                    {
                        Raven_Bot pRemovedBot = (Raven_Bot)msg.ExtraInfo;

                        GetSensoryMem().RemoveBotFromMemory(pRemovedBot);

                        //if the removed bot is the target, make sure the target is cleared
                        if (pRemovedBot == GetTargetSys().GetTarget())
                        {
                            GetTargetSys().ClearTarget();
                        }

                        return true;
                    }
          default: return false;
          }
        }



        //this rotates the bot's heading until it is facing directly at the target
        //position. Returns false if not facing at the target.
        public bool RotateFacingTowardPosition(Vector3 target)
        {
            Vector3 toTarget = VOp.Normalize(target - m_vPosition);

            float dot = Vector3.Dot(m_vFacing, toTarget);

            //clamp to rectify any rounding errors
            dot = Mathf.Clamp(dot, -1f, 1f);

            //determine the angle between the heading vector and the target
            float angle = (float)Mathf.Acos(dot);

            //return true if the bot's facing is within WeaponAimTolerance degs of
            //facing the target
            const float WeaponAimTolerance = 0.01f; //2 degs approx

            if (angle < WeaponAimTolerance)
            {
                m_vFacing = toTarget;
                return true;
            }

            //clamp the amount to turn to the max turn rate
            if (angle > m_dMaxTurnRate) angle = m_dMaxTurnRate;

            Quaternion rotQ = Quaternion.AngleAxis(angle, ConstV.v3_up);
            m_vFacing = rotQ * m_vFacing;

            return false;
        }

        //methods for accessing attribute data
        public int Health() {return m_iHealth;}
        public int MaxHealth() {return m_iMaxHealth;}
        public void ReduceHealth(int val)
        {
            m_iHealth -= val;

            if (m_iHealth <= 0)
            {
                SetDead();
            }

            m_bHit = true;

            m_iNumUpdatesHitPersistant = (int)(Const.FrameRate * Params.HitFlashTime);
        }

        public void IncreaseHealth(int val)
        {
            m_iHealth += val;
            m_iHealth = Mathf.Clamp(m_iHealth, 0, m_iMaxHealth);
        }
        public void RestoreHealthToMaximum() { m_iHealth = m_iMaxHealth; }

        public int Score() {return m_iScore;}
        public void IncrementScore() { ++m_iScore; }

        public Vector3 Facing() {return m_vFacing;}
        public float FieldOfView() {return m_dFieldOfView;}

        public bool isPossessed() {return m_bPossessed;}
        public bool isDead() {return m_Status == eStatus.dead;}
        public bool isAlive() {return m_Status == eStatus.alive;}
        public bool isSpawning() {return m_Status == eStatus.spawning;}
      
        public void SetSpawning() { m_Status = eStatus.spawning; }
        public void SetDead() { m_Status = eStatus.dead; }
        public void SetAlive() { m_Status = eStatus.alive; }

        //returns a value indicating the time in seconds it will take the bot
        //to reach the given position at its current speed.
        public float CalculateTimeToReachPosition(Vector3 pos)
        {
            return (Pos() - pos).magnitude / (MaxSpeed() * Const.FrameRate);
        }

        //returns true if the bot is close to the given position
        public bool isAtPosition(Vector3 pos)
        {
            const float tolerance = 10.0f;
  
            return (Pos() - pos).sqrMagnitude < tolerance* tolerance;
        }


        //interface for human player
        public void FireWeapon(Vector3 pos)
        {
            m_pWeaponSys.ShootAt(pos);
        }
        public void ChangeWeapon(int type)
        {
            m_pWeaponSys.ChangeWeapon(type);
        }
        public void TakePossession()
        {
            if (!(isSpawning() || isDead()))
            {
                m_bPossessed = true;

                DebugWide.LogBlue("Player Possesses bot " + ID());
            }
        }
        public void Exorcise()
        {
            m_bPossessed = false;

            //when the player is exorcised then the bot should resume normal service
            m_pBrain.AddGoal_Explore();

            DebugWide.LogBlue("Player is exorcised from bot " + ID());
        }

        //spawns the bot at the given position
        public void Spawn(Vector3 pos)
        {
            SetAlive();
            m_pBrain.RemoveAllSubgoals();
            m_pTargSys.ClearTarget();
            SetPos(pos);
            m_pWeaponSys.Initialize();
            RestoreHealthToMaximum();
        }

        //returns true if this bot is ready to test against all triggers
        public bool isReadyForTriggerUpdate()
        {
            return m_pTriggerTestRegulator.isReady();
        }

        //returns true if the bot has "line of sight" to the given position.
        public bool hasLOSto(Vector3 pos)
        {
          return m_pWorld.isLOSOkay(Pos(), pos);
        }

        //returns true if this bot can move directly to the given position
        //without bumping into any walls
        public bool canWalkTo(Vector3 pos)
        {
          return !m_pWorld.isPathObstructed(Pos(), pos, BRadius());
        }

        //similar to above. Returns true if the bot can move between the two
        //given positions without bumping into any walls
        public bool canWalkBetween(Vector3 from, Vector3 to)
        {
         return !m_pWorld.isPathObstructed(from, to, BRadius());
        }

        //returns true if there is space enough to step in the indicated direction
        //If true PositionOfStep will be assigned the offset position
        public bool canStepLeft(Vector3 PositionOfStep)
        {
            float StepDistance = BRadius() * 2f;
            Vector3 perp = Vector3.Cross(m_vFacing, ConstV.v3_up);
            PositionOfStep = Pos() - perp * StepDistance - perp * BRadius();

            return canWalkTo(PositionOfStep);
        }
        public bool canStepRight(Vector3 PositionOfStep)
        {
            float StepDistance = BRadius() * 2f;
            Vector3 perp = Vector3.Cross(m_vFacing, ConstV.v3_up);
            PositionOfStep = Pos() + perp * StepDistance + perp * BRadius();

            return canWalkTo(PositionOfStep);
        }
        public bool canStepForward(Vector3 PositionOfStep)
        {
            float StepDistance = BRadius() * 2f;
            
            PositionOfStep = Pos() + Facing() * StepDistance + Facing() * BRadius();

            return canWalkTo(PositionOfStep);
        }
        public bool canStepBackward(Vector3 PositionOfStep)
        {
            float StepDistance = BRadius() * 2f;

            PositionOfStep = Pos() - Facing() * StepDistance - Facing() * BRadius();

            return canWalkTo(PositionOfStep);
        }


        public Raven_Game GetWorld(){return m_pWorld;}
        public Raven_Steering GetSteering(){return m_pSteering;}
        public Raven_PathPlanner GetPathPlanner(){return m_pPathPlanner;}
        public Goal_Think GetBrain(){return m_pBrain;}
        public Raven_TargetingSystem GetTargetSys() {return m_pTargSys;}
        public Raven_Bot GetTargetBot(){return m_pTargSys.GetTarget();}
        public Raven_WeaponSystem GetWeaponSys(){return m_pWeaponSys;}
        public Raven_SensoryMemory GetSensoryMem(){return m_pSensoryMem;}


    }
    //*/

}//end namespace

