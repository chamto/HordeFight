using System.Collections.Generic;
using UnityEngine;
using UtilGS9;


namespace Raven
{

    public class Raven_Door : BaseGameEntity
    {

        public enum eStatus { open, opening, closed, closing };


        protected eStatus m_Status;

        //a sliding door is created from two walls, back to back.These walls must
        //be added to a map's geometry in order for an agent to detect them
        protected Wall2D m_pWall1;
        protected Wall2D m_pWall2;

        //a container of the id's of the triggers able to open this door
        protected List<int> m_Switches;

        //how long the door remains open before it starts to shut again
        protected int m_iNumTicksStayOpen;

        //how long the door has been open (0 if status is not open)
        protected int m_iNumTicksCurrentlyOpen;

        //the door's position and size when in the open position
        protected Vector3 m_vP1;
        protected Vector3 m_vP2;
        protected float m_dSize;

        //a normalized vector facing along the door. This is used frequently
        //by the other methods so we might as well just calculate it once in the
        //ctor
        protected Vector3 m_vtoP2Norm;

        //the door's current size
        protected float m_dCurrentSize;

        protected void Open()
        {
            if (m_Status == eStatus.opening)
            {
                if (m_dCurrentSize < 2)
                {
                    m_Status = eStatus.open;

                    m_iNumTicksCurrentlyOpen = m_iNumTicksStayOpen;

                    return;

                }

                //reduce the current size
                m_dCurrentSize -= 1;

                m_dCurrentSize = Mathf.Clamp(m_dCurrentSize, 0, m_dSize);

                ChangePosition(m_vP1, m_vP1 + m_vtoP2Norm * m_dCurrentSize);

            }
        }
        protected void Close()
        {
            if (m_Status == eStatus.closing)
            {
                if (m_dCurrentSize == m_dSize)
                {
                    m_Status = eStatus.closed;
                    return;

                }

                //reduce the current size
                m_dCurrentSize += 1;

                m_dCurrentSize = Mathf.Clamp(m_dCurrentSize, 0, m_dSize);

                ChangePosition(m_vP1, m_vP1 + m_vtoP2Norm * m_dCurrentSize);

            }
        }

        protected void ChangePosition(Vector3 newP1, Vector3 newP2)
        {
            m_vP1 = newP1;
            m_vP2 = newP2;

            Vector3 perp = Vector3.Cross(m_vtoP2Norm, ConstV.v3_up);
            m_pWall1.SetFrom(m_vP1 + perp);
            m_pWall1.SetTo(m_vP2 + perp);

            m_pWall2.SetFrom(m_vP2 - perp);
            m_pWall2.SetTo(m_vP1 - perp);
        }


        public Raven_Door(Raven_Map pMap, int id, string line) : base(id)

        {
            m_Status = eStatus.closed;
            m_iNumTicksStayOpen = 60;                   //MGC!
            Read(line);

            m_vtoP2Norm = VOp.Normalize(m_vP2 - m_vP1);
            m_dCurrentSize = m_dSize = (m_vP2 - m_vP1).magnitude;

            Vector3 perp = Vector3.Cross(m_vtoP2Norm, ConstV.v3_up);

            //create the walls that make up the door's geometry
            m_pWall1 = pMap.AddWall(m_vP1 + perp, m_vP2 + perp);
            m_pWall2 = pMap.AddWall(m_vP2 - perp, m_vP1 - perp);
        }
        //~Raven_Door();

        //the usual suspects
        public override void Render()
        {
            DebugWide.DrawLine(m_vP1, m_vP2, Color.blue);
        }

        public override void Update()
        {
            switch (m_Status)
            {
                case eStatus.opening:

                    Open(); break;

                case eStatus.closing:

                    Close(); break;

                case eStatus.open:
                    {
                        if (m_iNumTicksCurrentlyOpen-- < 0)
                        {
                            m_Status = eStatus.closing;
                        }
                    }
                    break;
            }


        }

        public override bool HandleMessage(Telegram msg)
        {
            if (msg.Msg == (int)eMsg.OpenSesame)
            {
                if (m_Status != eStatus.open)
                {
                    m_Status = eStatus.opening;
                }

                return true;
            }

            return false;
        }

        public void Read(string line)
        {
            string[] sp = line.Split(' ');

            float x = float.Parse(sp[0]);
            float z = float.Parse(sp[1]);
            int num = int.Parse(sp[2]);

            m_vP1 = new Vector3(x, 0, z);
            m_vP2 = m_vP1;

            //save the trigger IDs
            for (int i = 0; i < num; ++i)
            {
                m_Switches.Add(int.Parse(sp[3 + i]));
            }
        }


        //adds the ID of a switch
        public void AddSwitch(int id)
        {
            //only add the trigger if it isn't already present
            if (true == m_Switches.Contains(id))
            {
                m_Switches.Add(id);
            }
        }

        public List<int> GetSwitchIDs() { return m_Switches; }
    }


}

