﻿using UnityEngine;


namespace Raven
{
    public static class Feature
    {

        //returns a value between 0 and 1 based on the bot's health. The better
        //the health, the higher the rating
        public static float Health(Raven_Bot pBot)
        {
            return (float)pBot.Health() / (float)pBot.MaxHealth();

        }

        //returns a value between 0 and 1 based on the bot's closeness to the 
        //given item. the further the item, the higher the rating. If there is no
        //item of the given type present in the game world at the time this method
        //is called the value returned is 1
        public static float DistanceToItem(Raven_Bot pBot, int ItemType)
        {
            //determine the distance to the closest instance of the item type
            float DistanceToItem = pBot.GetPathPlanner().GetCostToClosestItem(ItemType);

            //if the previous method returns a negative value then there is no item of
            //the specified type present in the game world at this time.
            if (DistanceToItem < 0) return 1f;

            //these values represent cutoffs. Any distance over MaxDistance results in
            //a value of 0, and value below MinDistance results in a value of 1
            const float MaxDistance = 500.0f;
            const float MinDistance = 50.0f;

            DistanceToItem = Mathf.Clamp(DistanceToItem, MinDistance, MaxDistance);

            return DistanceToItem / MaxDistance;
        }

        //returns a value between 0 and 1 based on how much ammo the bot has for
        //the given weapon, and the maximum amount of ammo the bot can carry. The
        //closer the amount carried is to the max amount, the higher the score
        public static float IndividualWeaponStrength(Raven_Bot pBot,
                                               int WeaponType)
        {
            //grab a pointer to the gun (if the bot owns an instance)
            Raven_Weapon wp = pBot.GetWeaponSys().GetWeaponFromInventory(WeaponType);

            if (null != wp)
            {
                return wp.NumRoundsRemaining() / GetMaxRoundsBotCanCarryForWeapon(WeaponType);
            }

            else
            {
                return 0.0f;
            }
        }


        public static float GetMaxRoundsBotCanCarryForWeapon(int WeaponType)
        {
            switch (WeaponType)
            {
                case (int)eObjType.rail_gun:

                    return Params.RailGun_MaxRoundsCarried;

                case (int)eObjType.rocket_launcher:

                    return Params.RocketLauncher_MaxRoundsCarried;

                case (int)eObjType.shotgun:

                    return Params.ShotGun_MaxRoundsCarried;


                    //throw std::runtime_error("trying to calculate  of unknown weapon");

            }//end switch

            DebugWide.LogError("trying to calculate  of unknown weapon");
            return -1f;
        }

        //returns a value between 0 and 1 based on the total amount of ammo the
        //bot is carrying each of the weapons. Each of the three weapons a bot can
        //pick up can contribute a third to the score. In other words, if a bot
        //is carrying a RL and a RG and has max ammo for the RG but only half max
        //for the RL the rating will be 1/3 + 1/6 + 0 = 0.5
        public static float TotalWeaponStrength(Raven_Bot pBot)
        {
            float MaxRoundsForShotgun = GetMaxRoundsBotCanCarryForWeapon((int)eObjType.shotgun);
            float MaxRoundsForRailgun = GetMaxRoundsBotCanCarryForWeapon((int)eObjType.rail_gun);
            float MaxRoundsForRocketLauncher = GetMaxRoundsBotCanCarryForWeapon((int)eObjType.rocket_launcher);
            float TotalRoundsCarryable = MaxRoundsForShotgun + MaxRoundsForRailgun + MaxRoundsForRocketLauncher;

            float NumSlugs = (float)pBot.GetWeaponSys().GetAmmoRemainingForWeapon((int)eObjType.rail_gun);
            float NumCartridges = (float)pBot.GetWeaponSys().GetAmmoRemainingForWeapon((int)eObjType.shotgun);
            float NumRockets = (float)pBot.GetWeaponSys().GetAmmoRemainingForWeapon((int)eObjType.rocket_launcher);

            //the value of the tweaker (must be in the range 0-1) indicates how much
            //desirability value is returned even if a bot has not picked up any weapons.
            //(it basically adds in an amount for a bot's persistent weapon -- the blaster)
            const float Tweaker = 0.1f;

            return Tweaker + (1 - Tweaker) * (NumSlugs + NumCartridges + NumRockets) / (MaxRoundsForShotgun + MaxRoundsForRailgun + MaxRoundsForRocketLauncher);
        }
    }


}//end namespace

