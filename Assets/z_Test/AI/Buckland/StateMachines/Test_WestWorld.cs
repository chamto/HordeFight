using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

namespace WestWorld
{
    public class Test_WestWorld : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {
            SingleO.Init();

            Miner Bob = new Miner((int)entity_id.Miner_Bob);

            //create his wife
            MinersWife Elsa = new MinersWife((int)entity_id.Elsa);

            //register them with the entity manager
            SingleO.entityMgr.RegisterEntity(Bob);
            SingleO.entityMgr.RegisterEntity(Elsa);

            //run Bob and Elsa through a few Update calls
            //for (int i = 0; i < 30; ++i)
            //{
            //    Bob.Update();
            //    Elsa.Update();

            //    //dispatch any delayed messages
            //    SingleO.dispatcher.DispatchDelayedMessages();
            //}

            StartCoroutine(WaitAndUpdate(0.8f));


        }

        public IEnumerator WaitAndUpdate(float waitTime)
        {
            for (int i = 0; i < 30; ++i)
            {
                BaseGameEntity bob = SingleO.entityMgr.GetEntityFromID(0);
                BaseGameEntity elsa = SingleO.entityMgr.GetEntityFromID(1);
                bob.Update();
                elsa.Update();

                //dispatch any delayed messages
                SingleO.dispatcher.DispatchDelayedMessages();

                yield return new WaitForSeconds(waitTime);
            }

        }

        private void OnDrawGizmos()
        {
            Vector3 pos = Vector3.zero;
            pos.x = 5;
            DebugWide.DrawCircle(pos, 2, Color.black);
            DebugWide.PrintText(pos,Color.black, EnterMineAndDigForNugget.Instance.GetType()+"");
        }

        // Update is called once per frame
        //void Update () 
        //{
        //}
    }

    //======================================================

    public static class Const
    {
        //the amount of gold a miner must have before he feels he can go home
        public const int ComfortLevel = 5;
        //the amount of nuggets a miner can carry
        public const int MaxNuggets = 3;
        //above this value a miner is thirsty
        public const int ThirstLevel = 5;
        //above this value a miner is sleepy
        public const int TirednessThreshold = 5;
    }

    public enum location_type
    {
        shack,
        goldmine,
        bank,
        saloon
    }

    public enum entity_id
    {
        Miner_Bob = 0,

        Elsa = 1
    }

    public enum msg_type
    {
        HiHoneyImHome = 0,
        StewReady = 1,
    }

    public static class SingleO
    {
        public static EntityManager entityMgr = null;
        public static MessageDispatcher dispatcher = null;

        public static void Init()
        {
            entityMgr = new EntityManager();
            dispatcher = new MessageDispatcher(); 
        }
    }


    //======================================================

    public class EntityManager
    {
        //public static EntityManager Instance = null;

        private Dictionary<int, BaseGameEntity> m_EntityMap = new Dictionary<int, BaseGameEntity>();



        //this method stores a pointer to the entity in the std::vector
        //m_Entities at the index position indicated by the entity's ID
        //(makes for faster access)
        public void RegisterEntity(BaseGameEntity NewEntity)
        {
            m_EntityMap.Add(NewEntity.ID(), NewEntity);
        }

        //returns a pointer to the entity with the ID given as a parameter
        public BaseGameEntity GetEntityFromID(int id)
        {

            BaseGameEntity b;
            if(true == m_EntityMap.TryGetValue(id, out b))
            {
                return b; 
            }

            return null;
        }

        //this method removes the entity from the list
        public void RemoveEntity(BaseGameEntity pEntity)
        {
            m_EntityMap.Remove(pEntity.ID());
        }
    }
}

