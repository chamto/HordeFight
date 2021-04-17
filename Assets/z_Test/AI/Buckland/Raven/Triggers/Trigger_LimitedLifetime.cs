using Buckland;

namespace Raven
{
    public class Trigger_LimitedLifetime<entity_type> : Trigger<entity_type>
    {

        //the lifetime of this trigger in update-steps
        protected int m_iLifetime;


        public Trigger_LimitedLifetime(int lifetime) : base(BaseGameEntity.GetNextValidID())

        {
            m_iLifetime = lifetime;
        }

        //virtual ~Trigger_LimitedLifetime() { }

        //children of this class should always make sure this is called from within
        //their own update method
        public override void Update()
        {
            //if the lifetime counter expires set this trigger to be removed from
            //the game
            if (--m_iLifetime <= 0)
            {
                SetToBeRemovedFromGame();
            }
        }

    }

}


