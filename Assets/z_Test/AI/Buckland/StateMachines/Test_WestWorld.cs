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

    //======================================================

    

    //======================================================

    public class BaseGameEntity
    {

        //every entity must have a unique identifying number
        int m_ID;

        //this is the next valid ID. Each time a BaseGameEntity is instantiated
        //this value is updated
        //static int m_iNextValidID;

        //this must be called within the constructor to make sure the ID is set
        //correctly. It verifies that the value passed to the method is greater
        //or equal to the next valid ID, before setting the ID and incrementing
        //the next valid ID
        void SetID(int val)
        {
            //make sure the val is equal to or greater than the next available ID
            //assert((val >= m_iNextValidID) && "<BaseGameEntity::SetID>: invalid ID");

            m_ID = val;

            //m_iNextValidID = m_ID + 1;
        }


        public BaseGameEntity(int id)
        {
            SetID(id);
        }


        //all entities must implement an update function
        public virtual void Update() { }

        //all entities can communicate using messages. They are sent
        //using the MessageDispatcher singleton class
        public virtual bool HandleMessage(Telegram msg) { return false; }

        public int ID() {return m_ID;}
    }


    public class Miner : BaseGameEntity
    {

        //an instance of the state machine class
        StateMachine<Miner> m_pStateMachine;

        location_type m_Location;

        //how many nuggets the miner has in his pockets
        int m_iGoldCarried;

        int m_iMoneyInBank;

        //the higher the value, the thirstier the miner
        int m_iThirst;

        //the higher the value, the more tired the miner
        int m_iFatigue;

    
        public Miner(int id):base(id)

        {
            m_Location = location_type.shack;
            m_iGoldCarried = 0;
            m_iMoneyInBank = 0;
            m_iThirst = 0;
            m_iFatigue = 0;
                          

            //set up state machine
            m_pStateMachine = new StateMachine<Miner>(this);

            m_pStateMachine.SetCurrentState(GoHomeAndSleepTilRested.Instance);

            /* NOTE, A GLOBAL STATE HAS NOT BEEN IMPLEMENTED FOR THE MINER */
        }


        //this must be implemented
        public override void Update()
        {
            m_iThirst += 1;

            m_pStateMachine.Update();
        }

        //so must this
        public override bool HandleMessage(Telegram msg)
        {
            return m_pStateMachine.HandleMessage(msg);
        }


        public StateMachine<Miner> GetFSM() {return m_pStateMachine;}



//-------------------------------------------------------------accessors
        public location_type Location() {return m_Location;}
        public void ChangeLocation(location_type loc) { m_Location = loc; }

        public int GoldCarried() {return m_iGoldCarried;}
        public void SetGoldCarried(int val) { m_iGoldCarried = val; }
        public void AddToGoldCarried(int val)
        {
          m_iGoldCarried += val;

          if (m_iGoldCarried< 0) m_iGoldCarried = 0;
        }
        public bool PocketsFull() {return m_iGoldCarried >= Const.MaxNuggets;}

        public bool Fatigued()
        {
          if (m_iFatigue > Const. TirednessThreshold)
          {
            return true;
          }

          return false;
        }

        public void DecreaseFatigue() { m_iFatigue -= 1; }
        public void IncreaseFatigue() { m_iFatigue += 1; }

        public int Wealth() {return m_iMoneyInBank;}
        public void SetWealth(int val) { m_iMoneyInBank = val; }
        public void AddToWealth(int val)
        {
            m_iMoneyInBank += val;

            if (m_iMoneyInBank < 0) m_iMoneyInBank = 0;
        }

        public bool Thirsty()
        {
          if (m_iThirst >= Const.ThirstLevel){return true;}

          return false;
        }
        public void BuyAndDrinkAWhiskey() { m_iThirst = 0; m_iMoneyInBank -= 2; }

    }

    //======================================================

    public class MinersWife : BaseGameEntity
    {
    
        //an instance of the state machine class
        StateMachine<MinersWife> m_pStateMachine;

        location_type m_Location;

        //is she presently cooking?
        bool m_bCooking;


        public MinersWife(int id) : base(id)
        {
            m_Location = location_type.shack;
            m_bCooking = false;

            //set up the state machine
            m_pStateMachine = new StateMachine<MinersWife>(this);

            m_pStateMachine.SetCurrentState(DoHouseWork.Instance);

            m_pStateMachine.SetGlobalState(WifesGlobalState.Instance);
        }


        //this must be implemented
        public override void Update()
        {
            m_pStateMachine.Update();
        }

        //so must this
        public override bool HandleMessage(Telegram msg)
        {
            return m_pStateMachine.HandleMessage(msg);
        }

        public StateMachine<MinersWife> GetFSM() {return m_pStateMachine;}

        //----------------------------------------------------accessors
        public location_type Location(){return m_Location;}
        public void ChangeLocation(location_type loc) { m_Location = loc; }

        public bool Cooking(){return m_bCooking;}
        public void SetCooking(bool val) { m_bCooking = val; }
       
    }
}

