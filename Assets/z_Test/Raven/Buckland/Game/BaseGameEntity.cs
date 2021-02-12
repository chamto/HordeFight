using System;
using UnityEngine;

namespace Raven
{

    public class BaseGameEntity
    {

        public const int default_entity_type = -1;


        //each entity has a unique ID
        int m_ID;

        //every entity has a type associated with it (health, troll, ammo etc)
        int m_iType;

        //this is a generic flag. 
        bool m_bTag;

        //this is the next valid ID. Each time a BaseGameEntity is instantiated
        //this value is updated
        static int m_iNextValidID = 0;

        //this must be called within each constructor to make sure the ID is set
        //correctly. It verifies that the value passed to the method is greater
        //or equal to the next valid ID, before setting the ID and incrementing
        //the next valid ID
        void SetID(int val)
        {
            //make sure the val is equal to or greater than the next available ID
            //assert((val >= m_iNextValidID) && "<BaseGameEntity::SetID>: invalid ID");

            m_ID = val;

            m_iNextValidID = m_ID + 1;
        }



        //its location in the environment
        protected Vector3 m_vPosition;

        protected Vector3 m_vScale;

        //the magnitude of this object's bounding radius
        protected float  m_dBoundingRadius;


        protected BaseGameEntity(int ID)
        {
            m_dBoundingRadius = 0.0f;
            m_vScale = Vector3.one;
            m_iType = default_entity_type;
            m_bTag = false;

            SetID(ID);
        }


        public virtual void Update() { }

        public virtual void Render() { }
  
        public virtual bool HandleMessage(Telegram msg){return false;}
  

        //use this to grab the next valid ID
        public static int GetNextValidID() { return m_iNextValidID; }

        //this can be used to reset the next ID
        public static void ResetNextValidID() { m_iNextValidID = 0; }



        public Vector3 Pos() {return m_vPosition;}
        public void SetPos(Vector3 new_pos) { m_vPosition = new_pos; }

        public float BRadius() {return m_dBoundingRadius;}
        public void SetBRadius(float r) { m_dBoundingRadius = r; }
        public int ID() {return m_ID;}

        public bool IsTagged() {return m_bTag;}
        public void Tag() { m_bTag = true; }
        public void UnTag() { m_bTag = false; }

        public Vector3 Scale() {return m_vScale;}
        public void SetScale(Vector3 val) { m_dBoundingRadius *= Math.Max(val.x, val.z) / Math.Max(m_vScale.x, m_vScale.z); m_vScale = val; }
        public void SetScale(float val) { m_dBoundingRadius *= (val / Math.Max(m_vScale.x, m_vScale.z)); m_vScale = new Vector3(val, val, val); }

        public int EntityType() {return m_iType;}
        public void SetEntityType(int new_type) { m_iType = new_type; }

    }


}//end namespace

