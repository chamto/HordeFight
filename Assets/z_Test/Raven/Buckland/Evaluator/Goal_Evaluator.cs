using UnityEngine;


namespace Raven
{
    public class Goal_Evaluator
    {

        //when the desirability score for a goal has been evaluated it is multiplied 
        //by this value. It can be used to create bots with preferences based upon
        //their personality
        protected float m_dCharacterBias;


        public Goal_Evaluator(float CharacterBias)
        {
            m_dCharacterBias = CharacterBias;
        }


        //returns a score between 0 and 1 representing the desirability of the
        //strategy the concrete subclass represents
        public virtual double CalculateDesirability(Raven_Bot pBot) { return 0; }

        //adds the appropriate goal to the given bot's brain
        public virtual void SetGoal(Raven_Bot pBot) { }

        //used to provide debugging/tweaking support
        public virtual void RenderInfo(Vector3 Position, Raven_Bot pBot) { }
    }


}//end namespace

