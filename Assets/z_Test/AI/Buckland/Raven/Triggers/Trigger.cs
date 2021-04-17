using UnityEngine;
using Buckland;

namespace Raven
{
    public class Trigger<entity_type> : BaseGameEntity
    {

        //Every trigger owns a trigger region. If an entity comes within this 
        //region the trigger is activated
        TriggerRegion m_pRegionOfInfluence;

        //if this is true the trigger will be removed from the game
        bool m_bRemoveFromGame;

        //it's convenient to be able to deactivate certain types of triggers
        //on an event. Therefore a trigger can only be triggered when this
        //value is true (respawning triggers make good use of this facility)
        bool m_bActive;

        //some types of trigger are twinned with a graph node. This enables
        //the pathfinding component of an AI to search a navgraph for a specific
        //type of trigger.
        int m_iGraphNodeIndex;


        protected void SetGraphNodeIndex(int idx) { m_iGraphNodeIndex = idx; }

        protected void SetToBeRemovedFromGame() { m_bRemoveFromGame = true; }
        protected void SetInactive() { m_bActive = false; }
        protected void SetActive() { m_bActive = true; }

        //returns true if the entity given by a position and bounding radius is
        //overlapping the trigger region
        protected bool isTouchingTrigger(Vector3 EntityPos, float EntityRadius)
        {
            if (null != m_pRegionOfInfluence)
            {
                return m_pRegionOfInfluence.isTouching(EntityPos, EntityRadius);
            }

            return false;
        }

        //child classes use one of these methods to initialize the trigger region
        protected void AddCircularTriggerRegion(Vector3 center, float radius)
        {
            //if this replaces an existing region, tidy up memory
            //if (null != m_pRegionOfInfluence) delete m_pRegionOfInfluence;

            m_pRegionOfInfluence = new TriggerRegion_Circle(center, radius);
        }
        protected void AddRectangularTriggerRegion(Vector3 TopLeft, Vector3 BottomRight)
        {
            //if this replaces an existing region, tidy up memory
            //if (null != m_pRegionOfInfluence) delete m_pRegionOfInfluence;

            m_pRegionOfInfluence = new TriggerRegion_Rectangle(TopLeft, BottomRight);
        }


        public Trigger(int id) : base(id)
        {
            m_bRemoveFromGame = false;
            m_bActive = true;
            m_iGraphNodeIndex = -1;
            m_pRegionOfInfluence = null;
        }

        //virtual ~Trigger() { delete m_pRegionOfInfluence; }

        //when this is called the trigger determines if the entity is within the
        //trigger's region of influence. If it is then the trigger will be 
        //triggered and the appropriate action will be taken.
        public virtual void Try(entity_type type) { }

        //called each update-step of the game. This methods updates any internal
        //state the trigger may have
        //public virtual void Update() { }

        public int GraphNodeIndex() { return m_iGraphNodeIndex; }
        public bool isToBeRemoved() { return m_bRemoveFromGame; }
        public bool isActive() { return m_bActive; }
    }
}


