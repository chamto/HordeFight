using System.Collections.Generic;
using UnityEngine;
using UtilGS9;
using Buckland;

namespace Raven
{


    public class Raven_WeaponSystem
    {

        //a map of weapon instances indexed into by type
        public class WeaponMap : Dictionary<int, Raven_Weapon> { }


        Raven_Bot m_pOwner;

        //pointers to the weapons the bot is carrying (a bot may only carry one
        //instance of each weapon)
        WeaponMap m_WeaponMap = new WeaponMap();

        //a pointer to the weapon the bot is currently holding
        Raven_Weapon m_pCurrentWeapon;

        //this is the minimum amount of time a bot needs to see an opponent before
        //it can react to it. This variable is used to prevent a bot shooting at
        //an opponent the instant it becomes visible.
        float m_dReactionTime;

        //each time the current weapon is fired a certain amount of random noise is
        //added to the the angle of the shot. This prevents the bots from hitting
        //their opponents 100% of the time. The lower this value the more accurate
        //a bot's aim will be. Recommended values are between 0 and 0.2 (the value
        //represents the max deviation in radians that can be added to each shot).
        float m_dAimAccuracy;

        //the amount of time a bot will continue aiming at the position of the target
        //even if the target disappears from view.
        float m_dAimPersistance;

        //predicts where the target will be by the time it takes the current weapon's
        //projectile type to reach it. Used by TakeAimAndShoot
        Vector3 PredictFuturePositionOfTarget()
        {
          float MaxSpeed = GetCurrentWeapon().GetMaxProjectileSpeed();

                //if the target is ahead and facing the agent shoot at its current pos
                Vector3 ToEnemy = m_pOwner.GetTargetBot().Pos() - m_pOwner.Pos();

                //the lookahead time is proportional to the distance between the enemy
                //and the pursuer; and is inversely proportional to the sum of the
                //agent's velocities
                float LookAheadTime = ToEnemy.magnitude /
                                      (MaxSpeed + m_pOwner.GetTargetBot().MaxSpeed());
          
          //return the predicted future position of the enemy
          return m_pOwner.GetTargetBot().Pos() + 
                 m_pOwner.GetTargetBot().Velocity() * LookAheadTime;
        }

        //adds a random deviation to the firing angle not greater than m_dAimAccuracy 
        //rads
        void AddNoiseToAim(ref Vector3 AimingPos)
        {
          Vector3 toPos = AimingPos - m_pOwner.Pos();

           Transformations.Vec3RotateYAroundOrigin(ref toPos, Misc.RandFloat(-m_dAimAccuracy, m_dAimAccuracy));

          AimingPos = toPos + m_pOwner.Pos();
        }


        public Raven_WeaponSystem(Raven_Bot owner,
                                       float ReactionTime,
                                       float AimAccuracy,
                                       float AimPersistance)
        {
            m_pOwner = owner;
            m_dReactionTime = ReactionTime;
            m_dAimAccuracy = AimAccuracy;
            m_dAimPersistance = AimPersistance;

            Initialize();
        }

        //~Raven_WeaponSystem();

        //sets up the weapon map with just one weapon: the blaster
        public void Initialize()
        {
            //delete any existing weapons
            //WeaponMap::iterator curW;
            //for (curW = m_WeaponMap.begin(); curW != m_WeaponMap.end(); ++curW)
            //{
            //    delete curW->second;
            //}

            m_WeaponMap.Clear();

            //set up the container
            //m_pCurrentWeapon = new Blaster(m_pOwner);
            //m_WeaponMap.Add((int)eObjType.blaster, m_pCurrentWeapon);
            //m_WeaponMap.Add((int)eObjType.shotgun, null);
            //m_WeaponMap.Add((int)eObjType.rail_gun, null);
            //m_WeaponMap.Add((int)eObjType.rocket_launcher, null);

            m_pCurrentWeapon = new Blaster(m_pOwner);
            m_WeaponMap.Add((int)eObjType.blaster, m_pCurrentWeapon);
            m_WeaponMap.Add((int)eObjType.shotgun, new ShotGun(m_pOwner));
            m_WeaponMap.Add((int)eObjType.rail_gun, new RailGun(m_pOwner));
            m_WeaponMap.Add((int)eObjType.rocket_launcher, new RocketLauncher(m_pOwner));

        }

        //this method aims the bot's current weapon at the target (if there is a
        //target) and, if aimed correctly, fires a round. (Called each update-step
        //from Raven_Bot::Update)
        public void TakeAimAndShoot()
        {
            //DebugWide.LogBlue((eObjType)GetCurrentWeapon().GetType());

          //aim the weapon only if the current target is shootable or if it has only
          //very recently gone out of view (this latter condition is to ensure the 
          //weapon is aimed at the target even if it temporarily dodges behind a wall
          //or other cover)
          if (m_pOwner.GetTargetSys().isTargetShootable() ||
              (m_pOwner.GetTargetSys().GetTimeTargetHasBeenOutOfView() < 
               m_dAimPersistance) )
          {
            //the position the weapon will be aimed at
            Vector3 AimingPos = m_pOwner.GetTargetBot().Pos();
            
            //if the current weapon is not an instant hit type gun the target position
            //must be adjusted to take into account the predicted movement of the 
            //target
            if (GetCurrentWeapon().GetType() == (int)eObjType.rocket_launcher ||
                GetCurrentWeapon().GetType() == (int)eObjType.blaster)
            {
                  AimingPos = PredictFuturePositionOfTarget();

                  //if the weapon is aimed correctly, there is line of sight between the
                  //bot and the aiming position and it has been in view for a period longer
                  //than the bot's reaction time, shoot the weapon
                  if (m_pOwner.RotateFacingTowardPosition(AimingPos) &&
                       (m_pOwner.GetTargetSys().GetTimeTargetHasBeenVisible() >
                        m_dReactionTime) &&
                       m_pOwner.hasLOSto(AimingPos) )
                {
                    AddNoiseToAim(ref AimingPos);

                    GetCurrentWeapon().ShootAt(AimingPos);
                }
            }

            //no need to predict movement, aim directly at target
            else
            {
              //if the weapon is aimed correctly and it has been in view for a period
              //longer than the bot's reaction time, shoot the weapon
              if (m_pOwner.RotateFacingTowardPosition(AimingPos) &&
                   (m_pOwner.GetTargetSys().GetTimeTargetHasBeenVisible() >
                    m_dReactionTime) )
              {
                AddNoiseToAim(ref AimingPos);

                GetCurrentWeapon().ShootAt(AimingPos);
              }
            }

          }
          
          //no target to shoot at so rotate facing to be parallel with the bot's
          //heading direction
          else
          {
            m_pOwner.RotateFacingTowardPosition(m_pOwner.Pos()+ m_pOwner.Heading());
          }
        }

        //this method determines the most appropriate weapon to use given the current
        //game state. (Called every n update-steps from Raven_Bot::Update)
        public void SelectWeapon()
        {
            //if a target is present use fuzzy logic to determine the most desirable 
            //weapon.
            if (m_pOwner.GetTargetSys().isTargetPresent())
            {
                //calculate the distance to the target
                float DistToTarget = (m_pOwner.Pos() - m_pOwner.GetTargetSys().GetTarget().Pos()).magnitude;

                //for each weapon in the inventory calculate its desirability given the 
                //current situation. The most desirable weapon is selected
                float BestSoFar = float.MinValue;

                foreach(Raven_Weapon curWeap in m_WeaponMap.Values)
                {
                    //grab the desirability of this weapon (desirability is based upon
                    //distance to target and ammo remaining)
                    if (null != curWeap)
                    {
                        float score = curWeap.GetDesirability(DistToTarget);

                        //if it is the most desirable so far select it
                        if (score > BestSoFar)
                        {
                            BestSoFar = score;

                            //place the weapon in the bot's hand.
                            m_pCurrentWeapon = curWeap;
                        }
                    }
                }
            }

            else
            {
                m_pCurrentWeapon = m_WeaponMap[(int)eObjType.blaster];
            }
        }

        //this will add a weapon of the specified type to the bot's inventory. 
        //If the bot already has a weapon of this type only the ammo is added. 
        //(called by the weapon giver-triggers to give a bot a weapon)
        public void AddWeapon(int weapon_type)
        {
            //create an instance of this weapon
            Raven_Weapon w = null;

            switch (weapon_type)
            {
                case (int)eObjType.rail_gun:

                    w = new RailGun(m_pOwner); break;

                case (int)eObjType.shotgun:

                    w = new ShotGun(m_pOwner); break;

                case (int)eObjType.rocket_launcher:

                    w = new RocketLauncher(m_pOwner); break;

            }//end switch


            //if the bot already holds a weapon of this type, just add its ammo
            Raven_Weapon present = GetWeaponFromInventory(weapon_type);

            if (null != present)
            {
                present.IncrementRounds(w.NumRoundsRemaining());

                //delete w;
            }
            //if not already holding, add to inventory
            else
            {
                m_WeaponMap[weapon_type] = w;
            }
        }

        //changes the current weapon to one of the specified type (provided that type
        //is in the bot's possession)
        public void ChangeWeapon(int type)
        {
            Raven_Weapon w = GetWeaponFromInventory(type);
            //DebugWide.LogBlue(w + " " + (eObjType)type+ "  " + (eObjType)w.GetType());
            if (null != w) m_pCurrentWeapon = w;
        }

        //shoots the current weapon at the given position
        public void ShootAt(Vector3 pos)
        {
          GetCurrentWeapon().ShootAt(pos);
        }

        //returns a pointer to the current weapon
        public Raven_Weapon GetCurrentWeapon() {return m_pCurrentWeapon;}

        //returns a pointer to the specified weapon type (if in inventory, null if 
        //not)
        public Raven_Weapon GetWeaponFromInventory(int weapon_type)
        {
            return m_WeaponMap[weapon_type];
        }

        //returns the amount of ammo remaining for the specified weapon
        public int GetAmmoRemainingForWeapon(int weapon_type)
        {
            if (null != m_WeaponMap[weapon_type])
            {
                return m_WeaponMap[weapon_type].NumRoundsRemaining();
            }

            return 0;
        }

        public float ReactionTime() {return m_dReactionTime;}

        public void RenderCurrentWeapon()
        {
            GetCurrentWeapon().Render();
        }

        public void RenderDesirabilities()
        {
            Vector3 p = m_pOwner.Pos();

            int num = 0;

            foreach(Raven_Weapon curWeap in m_WeaponMap.Values)
            {
                if (null != curWeap) num++;
            }

            int offset = 15 * num;

            foreach (Raven_Weapon curWeap in m_WeaponMap.Values)
            {
              if (null != curWeap)
              {
                    float score = curWeap.GetLastDesirabilityScore();
        
                    DebugWide.PrintText(new Vector3(p.x + 10, p.y, p.z - offset) , Color.black ,"  " + score + "  " + (eObjType)curWeap.GetType());

                    offset+=15;
              }
            }
        }
    }

}//end namespace

