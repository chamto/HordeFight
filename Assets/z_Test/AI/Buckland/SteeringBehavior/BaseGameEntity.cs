using UnityEngine;

namespace SteeringBehavior
{
    public class BaseGameEntity
    {

        //public enum { default_entity_type = -1 };
        public const int default_entity_type = -1;

        //each entity has a unique ID
        int m_ID;

        //every entity has a type associated with it (health, troll, ammo etc)
        int m_EntityType;

        //this is a generic flag. 
        bool m_bTag;

        //used by the constructor to give each entity a unique ID
        //int m_NextID = 0;
        //int NextValidID() { return m_NextID++; } //의도에 맞게 수정할 필요 있음 



        //its location in the environment
        protected Vector2 m_vPos;

        protected Vector2 m_vScale;

        //the length of this object's bounding radius
        float m_dBoundingRadius;


        public BaseGameEntity()
        {
            //m_ID = NextValidID();
            m_dBoundingRadius = 0f;
            m_vPos = new Vector2();
            m_vScale = new Vector2(1f, 1f);
            m_EntityType = default_entity_type;
            m_bTag = false;
        }


        public BaseGameEntity(int entity_type)
        {

            //m_ID = NextValidID();
            m_dBoundingRadius = 0f;
            m_vPos = new Vector2();
            m_vScale = new Vector2(1f, 1f);
            m_EntityType = entity_type;
            m_bTag = false;
        }


        public BaseGameEntity(int entity_type, Vector2 pos, float r)
        {
            //m_ID = NextValidID();
            m_dBoundingRadius = r;
            m_vPos = pos;
            m_vScale = new Vector2(1f, 1f);
            m_EntityType = entity_type;
            m_bTag = false;

        }

        //this can be used to create an entity with a 'forced' ID. It can be used
        //when a previously created entity has been removed and deleted from the
        //game for some reason. For example, The Raven map editor uses this ctor 
        //in its undo/redo operations. 
        //USE WITH CAUTION!
        public BaseGameEntity(int entity_type, int ForcedID)
        {
            m_ID = ForcedID;
            m_dBoundingRadius = 0f;
            m_vPos = new Vector2();
            m_vScale = new Vector2(1f, 1f);
            m_EntityType = entity_type;
            m_bTag = false;
        }



        public virtual void Update(double time_elapsed) { }

        public virtual void Render() { }

        //public virtual bool HandleMessage(Telegram msg) { return false; }


        public Vector2 Pos() { return m_vPos; }
        public void SetPos(Vector2 new_pos) { m_vPos = new_pos; }

        public float BRadius() { return m_dBoundingRadius; }
        public void SetBRadius(float r) { m_dBoundingRadius = r; }
        public int ID() { return m_ID; }

        public bool IsTagged() { return m_bTag; }
        public void Tag() { m_bTag = true; }
        public void UnTag() { m_bTag = false; }

        public Vector2 Scale() { return m_vScale; }
        public void SetScale(Vector2 val) { m_dBoundingRadius *= Util.MaxOf(val.x, val.y) / Util.MaxOf(m_vScale.x, m_vScale.y); m_vScale = val; }
        public void SetScale(float val) { m_dBoundingRadius *= (val / Util.MaxOf(m_vScale.x, m_vScale.y)); m_vScale = new Vector2(val, val); }

        public int EntityType() { return m_EntityType; }
        public void SetEntityType(int new_type) { m_EntityType = new_type; }

    }
}

