using System.Collections.Generic;


namespace Proto_AI_4
{

    public class EntityManager
    {

        private Dictionary<int, BaseGameEntity> m_EntityMap = new Dictionary<int, BaseGameEntity>();

        private static EntityManager _instance = null;
        public static EntityManager Instance()
        {
            if (null == _instance)
                _instance = new EntityManager();

            return _instance; 
        }

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
            if (true == m_EntityMap.TryGetValue(id, out b))
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

        public void Reset() { m_EntityMap.Clear(); }
    }


}//end namespace

