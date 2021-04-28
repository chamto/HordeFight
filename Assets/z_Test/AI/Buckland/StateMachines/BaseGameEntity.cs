namespace WestWorld
{
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

        public int ID() { return m_ID; }
    }
}

