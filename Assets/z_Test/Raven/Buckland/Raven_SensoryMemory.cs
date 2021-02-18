using System;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;


namespace Raven
{

    public struct MemoryRecord
    {

        //records the time the opponent was last sensed (seen or heard). This
        //is used to determine if a bot can 'remember' this record or not. 
        //(if CurrentTime() - m_dTimeLastSensed is greater than the bot's
        //memory span, the data in this record is made unavailable to clients)
        public float fTimeLastSensed;

        //it can be useful to know how long an opponent has been visible. This 
        //variable is tagged with the current time whenever an opponent first becomes
        //visible. It's then a simple matter to calculate how long the opponent has
        //been in view (CurrentTime - fTimeBecameVisible)
        public float fTimeBecameVisible;

        //it can also be useful to know the last time an opponent was seen
        public float fTimeLastVisible;

        //a vector marking the position where the opponent was last sensed. This can
        // be used to help hunt down an opponent if it goes out of view
        public Vector3 vLastSensedPosition;

        //set to true if opponent is within the field of view of the owner
        public bool bWithinFOV;

        //set to true if there is no obstruction between the opponent and the owner, 
        //permitting a shot.
        public bool bShootable;


        public void Init()
        {
            fTimeLastSensed = -999;
            fTimeBecameVisible = -999;
            fTimeLastVisible = 0;
            bWithinFOV = false;
            bShootable = false;
        }

    }

    public class Raven_SensoryMemory
    {
        public class MemoryMap : Dictionary<Raven_Bot, MemoryRecord> { }

        //the owner of this instance
        Raven_Bot m_pOwner;

        //this container is used to simulate memory of sensory events. A MemoryRecord
        //is created for each opponent in the environment. Each record is updated 
        //whenever the opponent is encountered. (when it is seen or heard)
        MemoryMap m_MemoryMap = new MemoryMap();

        //a bot has a memory span equivalent to this value. When a bot requests a 
        //list of all recently sensed opponents this value is used to determine if 
        //the bot is able to remember an opponent or not.
        float m_dMemorySpan;

        //this methods checks to see if there is an existing record for pBot. If
        //not a new MemoryRecord record is made and added to the memory map.(called
        //by UpdateWithSoundSource & UpdateVision)
        void MakeNewRecordIfNotAlreadyPresent(Raven_Bot pOpponent)
        {
            //else check to see if this Opponent already exists in the memory. If it doesn't,
            //create a new record
            //var a =  m_MemoryMap.Values.Last();
            //if (m_MemoryMap.find(pOpponent) == m_MemoryMap.end())
            if (false == m_MemoryMap.ContainsKey(pOpponent))
            {
                MemoryRecord mr = new MemoryRecord();
                mr.Init();
                //m_MemoryMap[pOpponent] = mr;
                m_MemoryMap.Add(pOpponent, mr);

            }
        }


        public Raven_SensoryMemory(Raven_Bot owner, float MemorySpan)
        {
            m_pOwner = owner;
            m_dMemorySpan = MemorySpan;
        }

        //this method is used to update the memory map whenever an opponent makes
        //a noise
        public void UpdateWithSoundSource(Raven_Bot pNoiseMaker)
        {
            //make sure the bot being examined is not this bot
            if (m_pOwner != pNoiseMaker)
            {
                //if the bot is already part of the memory then update its data, else
                //create a new memory record and add it to the memory
                MakeNewRecordIfNotAlreadyPresent(pNoiseMaker);

                MemoryRecord info = m_MemoryMap[pNoiseMaker];

                //test if there is LOS between bots 
                if (m_pOwner.GetWorld().isLOSOkay(m_pOwner.Pos(), pNoiseMaker.Pos()))
                {
                    info.bShootable = true;

                    //record the position of the bot
                    info.vLastSensedPosition = pNoiseMaker.Pos();
                }
                else
                {
                    info.bShootable = false;
                }

                //record the time it was sensed
                info.fTimeLastSensed = Time.time;
                m_MemoryMap[pNoiseMaker] = info;
            }
        }

        //this removes a bot's record from memory
        public void RemoveBotFromMemory(Raven_Bot pBot)
        {
            if (true == m_MemoryMap.ContainsKey(pBot))
            {
                m_MemoryMap.Remove(pBot);
            }
        }

        //this method iterates through all the opponents in the game world and 
        //updates the records of those that are in the owner's FOV
        public void UpdateVision()
        {
            //for each bot in the world test to see if it is visible to the owner of
            //this class
            LinkedList<Raven_Bot> bots = m_pOwner.GetWorld().GetAllBots();
            foreach (Raven_Bot curBot in bots)
            {
                //make sure the bot being examined is not this bot
                if (m_pOwner != curBot)
                {
                    //make sure it is part of the memory map
                    MakeNewRecordIfNotAlreadyPresent(curBot);

                    //get a reference to this bot's data
                    MemoryRecord info = m_MemoryMap[curBot];

                    //test if there is LOS between bots 
                    if (m_pOwner.GetWorld().isLOSOkay(m_pOwner.Pos(), curBot.Pos()))
                    {
                        info.bShootable = true;

                        //test if the bot is within FOV
                        if (isSecondInFOVOfFirst(m_pOwner.Pos(),
                                                 m_pOwner.Facing(),
                                                 curBot.Pos(),
                                                  m_pOwner.FieldOfView()))
                        {
                            info.fTimeLastSensed = Time.time;
                            info.vLastSensedPosition = curBot.Pos();
                            info.fTimeLastVisible = Time.time;

                            if (info.bWithinFOV == false)
                            {
                                info.bWithinFOV = true;
                                info.fTimeBecameVisible = info.fTimeLastSensed;

                            }
                        }

                        else
                        {
                            info.bWithinFOV = false;
                        }
                    }

                    else
                    {
                        info.bShootable = false;
                        info.bWithinFOV = false;
                    }

                    m_MemoryMap[curBot] = info;
                }
            }//next bot
        }

        public bool isOpponentShootable(Raven_Bot pOpponent)
        {

            if (true == m_MemoryMap.ContainsKey(pOpponent))
            {
                return m_MemoryMap[pOpponent].bShootable;
            }

            return false;
        }

        public bool isOpponentWithinFOV(Raven_Bot pOpponent)
        {
            if (true == m_MemoryMap.ContainsKey(pOpponent))
            {
                return m_MemoryMap[pOpponent].bWithinFOV;
            }

            return false;
        }

        public Vector3 GetLastRecordedPositionOfOpponent(Raven_Bot pOpponent)
        {
            if (true == m_MemoryMap.ContainsKey(pOpponent))
            {
                return m_MemoryMap[pOpponent].vLastSensedPosition;

            }

            DebugWide.LogRed("< Raven_SensoryMemory::GetLastRecordedPositionOfOpponent>: Attempting to get position of unrecorded bot");
            return ConstV.v3_zero;
        }

        public float GetTimeOpponentHasBeenVisible(Raven_Bot pOpponent)
        {
            if (true == m_MemoryMap.ContainsKey(pOpponent))
            {
                if (m_MemoryMap[pOpponent].bWithinFOV)
                {
                    return Time.time - m_MemoryMap[pOpponent].fTimeBecameVisible;
                }
            }

            return 0;
        }

        public float GetTimeSinceLastSensed(Raven_Bot pOpponent)
        {
            if (true == m_MemoryMap.ContainsKey(pOpponent))
            {
                if (m_MemoryMap[pOpponent].bWithinFOV)
                {
                    return Time.time - m_MemoryMap[pOpponent].fTimeLastSensed;
                }
            }

            return 0;
        }

        public float GetTimeOpponentHasBeenOutOfView(Raven_Bot pOpponent)
        {
            if (true == m_MemoryMap.ContainsKey(pOpponent))
            {
                return Time.time - m_MemoryMap[pOpponent].fTimeLastVisible;
            }

            return float.MaxValue;
        }

        //this method returns a list of all the opponents that have had their
        //records updated within the last m_dMemorySpan seconds.
        public LinkedList<Raven_Bot> GetListOfRecentlySensedOpponents()
        {
            //this will store all the opponents the bot can remember
            LinkedList<Raven_Bot> opponents = new LinkedList<Raven_Bot>();

            float CurrentTime = Time.time;

            foreach (Raven_Bot bot in m_MemoryMap.Keys)
            {
                //if this bot has been updated in the memory recently, add to list
                if ((CurrentTime - m_MemoryMap[bot].fTimeLastSensed) <= m_dMemorySpan)
                {
                    opponents.AddLast(bot);
                }
            }


            return opponents;
        }

        public void RenderBoxesAroundRecentlySensed()
        {
            LinkedList<Raven_Bot> opponents = GetListOfRecentlySensedOpponents();
            foreach (Raven_Bot bot in opponents)
            {
                Vector3 p = bot.Pos();
                float b = bot.BRadius();

                DebugWide.DrawLine(new Vector3(p.x - b, p.y, p.z - b), new Vector3(p.x + b, p.y, p.z - b), Color.magenta);
                DebugWide.DrawLine(new Vector3(p.x + b, p.y, p.z - b), new Vector3(p.x + b, p.y, p.z + b), Color.magenta);
                DebugWide.DrawLine(new Vector3(p.x + b, p.y, p.z + b), new Vector3(p.x - b, p.y, p.z + b), Color.magenta);
                DebugWide.DrawLine(new Vector3(p.x - b, p.y, p.z + b), new Vector3(p.x - b, p.y, p.z - b), Color.magenta);
            }
            //gdi->OrangePen();

        }

        //------------------ isSecondInFOVOfFirst -------------------------------------
        //
        //  returns true if the target position is in the field of view of the entity
        //  positioned at posFirst facing in facingFirst
        //-----------------------------------------------------------------------------
        public bool isSecondInFOVOfFirst(Vector3 posFirst,
                                         Vector3 facingFirst,
                                         Vector3 posSecond,
                                         float fov)
        {
            Vector3 toTarget = VOp.Normalize(posSecond - posFirst);
            
            return Vector3.Dot(facingFirst, toTarget) >= Math.Cos(fov / 2.0f); // 범위 : -0.5 <= 1 <= 0.5 , cos가 1일때 0도 0.5일때 45도 0일떄 90도 
        }

    }

}//end namespace

