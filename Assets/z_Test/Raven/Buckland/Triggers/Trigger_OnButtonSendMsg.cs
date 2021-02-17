using UnityEngine;


namespace Raven
{
    //=======


    public class Trigger_OnButtonSendMsg<entity_type> : Trigger<entity_type> where entity_type : BaseGameEntity
    {
        //when triggered a message is sent to the entity with the following ID
        int m_iReceiver;

        //the message that is sent
        int m_iMessageToSend;


        public Trigger_OnButtonSendMsg(int id, string line) : base(id)
        {
            Read(line);
        }

        public override void Try(entity_type pEnt)
        {

            if (isTouchingTrigger(pEnt.Pos(), pEnt.BRadius()))
            {

                SingleO.dispatcher.DispatchMsg(Const.SEND_MSG_IMMEDIATELY,
                                        ID(),
                                        m_iReceiver,
                                        m_iMessageToSend,
                                        null);

            }
        }

        //public override void Update();

        public override void Render()
        {

            float sz = BRadius();

            DebugWide.DrawLine(new Vector3(Pos().x - sz, 0, Pos().z - sz), new Vector3(Pos().x + sz, 0, Pos().z - sz), Color.yellow);
            DebugWide.DrawLine(new Vector3(Pos().x + sz, 0, Pos().z - sz), new Vector3(Pos().x + sz, 0, Pos().z + sz), Color.yellow);
            DebugWide.DrawLine(new Vector3(Pos().x + sz, 0, Pos().z + sz), new Vector3(Pos().x - sz, 0, Pos().z + sz), Color.yellow);
            DebugWide.DrawLine(new Vector3(Pos().x - sz, 0, Pos().z + sz), new Vector3(Pos().x - sz, 0, Pos().z - sz), Color.yellow);

        }

        //public void Write(std::ostream&  os)const{}
        public void Read(string line)
        {
            string[] sp = line.Split(' ');

            m_iReceiver = int.Parse(sp[0]); //grab the id of the entity it messages
            m_iMessageToSend = int.Parse(sp[1]); //grab the message type
            float x = float.Parse(sp[2]);
            float z = float.Parse(sp[3]);
            float r = float.Parse(sp[4]);

            SetPos(new Vector3(x, 0, z));
            SetBRadius(r);

            //create and set this trigger's region of fluence
            AddRectangularTriggerRegion(Pos() - new Vector3(BRadius(), 0, BRadius()),   //top left corner
                                        Pos() + new Vector3(BRadius(), 0, BRadius()));  //bottom right corner
        }

        public override bool HandleMessage(Telegram msg)
        {
            return false;
        }
    }


    ///////////////////////////////////////////////////////////////////////////////


}


