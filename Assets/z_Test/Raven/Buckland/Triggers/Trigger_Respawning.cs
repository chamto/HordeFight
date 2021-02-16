namespace Raven
{
    public class Trigger_Respawning<entity_type> : Trigger<entity_type>
    {

        //When a bot comes within this trigger's area of influence it is triggered
        //but then becomes inactive for a specified amount of time. These values
        //control the amount of time required to pass before the trigger becomes 
        //active once more.
        protected int m_iNumUpdatesBetweenRespawns;
        protected int m_iNumUpdatesRemainingUntilRespawn;

        //sets the trigger to be inactive for m_iNumUpdatesBetweenRespawns 
        //update-steps
        protected void Deactivate()
        {
            SetInactive();
            m_iNumUpdatesRemainingUntilRespawn = m_iNumUpdatesBetweenRespawns;
        }


        public Trigger_Respawning(int id) : base(id)
        {
            m_iNumUpdatesBetweenRespawns = 0;
            m_iNumUpdatesRemainingUntilRespawn = 0;
        }



        //this is called each game-tick to update the trigger's internal state
        public override void Update()
        {
            if ((--m_iNumUpdatesRemainingUntilRespawn <= 0) && !isActive())
            {
                SetActive();
            }
        }

        public void SetRespawnDelay(int numTicks)
        { m_iNumUpdatesBetweenRespawns = numTicks; }

    }

}


