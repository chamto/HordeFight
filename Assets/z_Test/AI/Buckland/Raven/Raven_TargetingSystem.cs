using System.Collections.Generic;
using UnityEngine;
using UtilGS9;


namespace Raven
{
    //======================================================


    public class Raven_TargetingSystem
    {

        //the owner of this system
        Raven_Bot m_pOwner;

        //the current target (this will be null if there is no target assigned)
        Raven_Bot m_pCurrentTarget;



        public Raven_TargetingSystem(Raven_Bot owner) 
        {
            m_pOwner = owner;
            m_pCurrentTarget = null;
        }

        //each time this method is called the opponents in the owner's sensory 
        //memory are examined and the closest  is assigned to m_pCurrentTarget.
        //if there are no opponents that have had their memory records updated
        //within the memory span of the owner then the current target is set
        //to null
        public void Update()
        {
            float ClosestDistSoFar = float.MaxValue;
            m_pCurrentTarget = null;

            //grab a list of all the opponents the owner can sense
            LinkedList<Raven_Bot> SensedBots = m_pOwner.GetSensoryMem().GetListOfRecentlySensedOpponents();

            foreach(Raven_Bot curBot in SensedBots)
            {
                //make sure the bot is alive and that it is not the owner
                if ((curBot).isAlive() && (curBot != m_pOwner))
                {
                    float dist = ((curBot).Pos() - m_pOwner.Pos()).sqrMagnitude;

                    if (dist < ClosestDistSoFar)
                    {
                        ClosestDistSoFar = dist;
                        m_pCurrentTarget = curBot;
                    }
                }
            }
        }

        //returns true if there is a currently assigned target
        public bool isTargetPresent() {return m_pCurrentTarget != null;}

        //returns true if the target is within the field of view of the owner
        public bool isTargetWithinFOV()
        {
            return m_pOwner.GetSensoryMem().isOpponentWithinFOV(m_pCurrentTarget);
        }

        //returns true if there is unobstructed line of sight between the target
        //and the owner
        public bool isTargetShootable()
        {
            return m_pOwner.GetSensoryMem().isOpponentShootable(m_pCurrentTarget);
        }

        //returns the position the target was last seen. Throws an exception if
        //there is no target currently assigned
        public Vector3 GetLastRecordedPosition()
        {
          return m_pOwner.GetSensoryMem().GetLastRecordedPositionOfOpponent(m_pCurrentTarget);
        }

        //returns the amount of time the target has been in the field of view
        public float GetTimeTargetHasBeenVisible()
        {
          return m_pOwner.GetSensoryMem().GetTimeOpponentHasBeenVisible(m_pCurrentTarget);
        }

        //returns the amount of time the target has been out of view
        public float GetTimeTargetHasBeenOutOfView()
        {
          return m_pOwner.GetSensoryMem().GetTimeOpponentHasBeenOutOfView(m_pCurrentTarget);
        }

        //returns a pointer to the target. null if no target current.
        public Raven_Bot GetTarget() {return m_pCurrentTarget;}

        //sets the target pointer to null
        public void ClearTarget() { m_pCurrentTarget = null; }
    }


}//end namespace

