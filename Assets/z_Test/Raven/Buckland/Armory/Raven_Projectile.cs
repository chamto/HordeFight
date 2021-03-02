using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

namespace Raven
{


    //public class Rocket : Raven_Projectile
    //{
    //    public Rocket(Raven_Bot shooter, Vector3 target)
    //                     : base(target,
    //                     shooter.GetWorld(),
    //                     shooter.ID(),
    //                     shooter.Pos(),
    //                     shooter.Facing(),
    //                     1, 1, 1, 1, 1)
    //    { }
    //}
    //public class Slug : Raven_Projectile
    //{
    //    public Slug(Raven_Bot shooter, Vector3 target)
    //    : base(target,
    //                     shooter.GetWorld(),
    //                     shooter.ID(),
    //                     shooter.Pos(),
    //                     shooter.Facing(),
    //                     1, 1, 1, 1, 1)
    //    { }
    //}
    //public class Pellet : Raven_Projectile
    //{
    //    public Pellet(Raven_Bot shooter, Vector3 target)
    //        : base(target,
    //                     shooter.GetWorld(),
    //                     shooter.ID(),
    //                     shooter.Pos(),
    //                     shooter.Facing(),
    //                     1, 1, 1, 1, 1)
    //    { }
    //}

    //=======================================================

    public class Raven_Projectile : MovingEntity
    {

        //the ID of the entity that fired this
        protected int m_iShooterID;

        //the place the projectile is aimed at
        protected Vector3 m_vTarget;

        //a pointer to the world data
        protected Raven_Game m_pWorld;

        //where the projectile was fired from
        protected Vector3 m_vOrigin;

        //how much damage the projectile inflicts
        protected int m_iDamageInflicted;

        //is it dead? A dead projectile is one that has come to the end of its
        //trajectory and cycled through any explosion sequence. A dead projectile
        //can be removed from the world environment and deleted.
        protected bool m_bDead;

        //this is set to true as soon as a projectile hits something
        protected bool m_bImpacted;

        //the position where this projectile impacts an object
        protected Vector3 m_vImpactPoint;

        //this is stamped with the time this projectile was instantiated. This is
        //to enable the shot to be rendered for a specific length of time
        protected float m_dTimeOfCreation;

        protected Raven_Bot GetClosestIntersectingBot(Vector3 From, Vector3 To)
        {
                Raven_Bot ClosestIntersectingBot = null;
                float ClosestSoFar = float.MaxValue;

                //iterate through all entities checking against the line segment FromTo
                foreach(Raven_Bot curBot in m_pWorld.GetAllBots())
                {
                    //make sure we don't check against the shooter of the projectile
                    if (((curBot).ID() != m_iShooterID))
                    {
                        //if the distance to FromTo is less than the entity's bounding radius then
                        //there is an intersection
                        if (Geometry.DistToLineSegment(From, To, (curBot).Pos()) < (curBot).BRadius())
                        {
                            //test to see if this is the closest so far
                            float Dist = ((curBot).Pos() -  m_vOrigin).sqrMagnitude;

                            if (Dist<ClosestSoFar)
                            {
                              Dist = ClosestSoFar;
                              ClosestIntersectingBot = curBot;
                            }
                        }
                    }

              }

            return ClosestIntersectingBot;
        }


        protected LinkedList<Raven_Bot> GetListOfIntersectingBots(Vector3 From, Vector3 To)
        {
          //this will hold any bots that are intersecting with the line segment
          LinkedList<Raven_Bot> hits = new LinkedList<Raven_Bot>();

            //iterate through all entities checking against the line segment FromTo
            foreach (Raven_Bot curBot in m_pWorld.GetAllBots())
            {
                //make sure we don't check against the shooter of the projectile
                if (((curBot).ID() != m_iShooterID))
                {
                  //if the distance to FromTo is less than the entities bounding radius then
                  //there is an intersection so add it to hits
                  if (Geometry.DistToLineSegment(From, To, (curBot).Pos()) < (curBot).BRadius())
                  {
                    hits.AddLast(curBot);
                  }
                }

            }

            return hits;
        }


        public Raven_Projectile(Vector3 target,   //the target's position
                   Raven_Game world,  //a pointer to the world data
                   int ShooterID, //the ID of the bot that fired this shot
                   Vector3 origin,  //the start position of the projectile
                   Vector3 heading,   //the heading of the projectile
                   int damage,    //how much damage it inflicts
                   float scale,
                   float MaxSpeed,
                   float mass,
                   float MaxForce) : base(origin,
                                                   scale,
                                                   ConstV.v3_zero,
                                                     MaxSpeed,
                                                     heading,
                                                     mass,
                                                     new Vector3(scale, scale, scale),
                                                     0, //max turn rate irrelevant here, all shots go straight
                                                     MaxForce)


        {
            m_vTarget = target;
            m_bDead = false;
            m_bImpacted = false;
            m_pWorld = world;
            m_iDamageInflicted = damage;
            m_vOrigin = origin;
            m_iShooterID = ShooterID;
            m_dTimeOfCreation = Time.time;
        }


        //must be implemented
        public virtual void Update() { }
        public virtual void Render() { }

        //set to true if the projectile has impacted and has finished any explosion 
        //sequence. When true the projectile will be removed from the game
        public bool isDead() {return m_bDead;}

        //true if the projectile has impacted but is not yet dead (because it
        //may be exploding outwards from the point of impact for example)
        public bool HasImpacted() {return m_bImpacted;}



    }

}//end namespace

