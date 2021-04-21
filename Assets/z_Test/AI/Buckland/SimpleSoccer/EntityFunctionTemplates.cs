using System.Collections.Generic;
using UnityEngine;

namespace Test_SimpleSoccer
{
    public class EntityFunctionTemplates 
    {
        //------------------- EnforceNonPenetrationContraint ---------------------
        //
        //  Given a pointer to an entity and a std container of pointers to nearby
        //  entities, this function checks to see if there is an overlap between
        //  entities. If there is, then the entities are moved away from each
        //  other
        //------------------------------------------------------------------------

        static public void EnforceNonPenetrationContraint(PlayerBase entity, LinkedList<PlayerBase> others)
        {

            //iterate through all entities checking for any overlap of bounding
            //radii
            foreach (PlayerBase it in others)
            {
                //make sure we don't check against this entity
                if (it == entity) continue;

                //calculate the distance between the positions of the entities
                Vector3 ToEntity = entity.Pos() - (it).Pos();

                float DistFromEachOther = ToEntity.magnitude;

                //if this distance is smaller than the sum of their radii then this
                //entity must be moved away in the direction parallel to the
                //ToEntity vector   
                float AmountOfOverLap = (it).BRadius() + entity.BRadius() -
                                         DistFromEachOther;

                if (AmountOfOverLap >= 0)
                {
                  //move the entity a distance away equivalent to the amount of overlap.
                  entity.SetPos(entity.Pos() + (ToEntity/DistFromEachOther) *
                                 AmountOfOverLap);
                }
            }//next entity
        }
     
     }

}//end namespace

