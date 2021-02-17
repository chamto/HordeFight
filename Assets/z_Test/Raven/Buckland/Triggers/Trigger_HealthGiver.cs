using UnityEngine;


namespace Raven
{
    public class Trigger_HealthGiver : Trigger_Respawning<Raven_Bot>
    {

        //the amount of health an entity receives when it runs over this trigger
        int m_iHealthGiven;


        public Trigger_HealthGiver(int id, string line) : base(id)
        {
            Read(line);
        }

        //if triggered, the bot's health will be incremented
        public override void Try(Raven_Bot type)
        {
            if (isActive() && isTouchingTrigger(type.Pos(), type.BRadius()))
            {
                type.IncreaseHealth(m_iHealthGiven);

                Deactivate();
            }
        }

        //draws a box with a red cross at the trigger's location
        public override void Render()
        {
            if (isActive())
            {
                //gdi->BlackPen();
                //gdi->WhiteBrush();
                const int sz = 5;

                DebugWide.DrawCube(Pos(), Vector3.one * 5, Color.black);

                Vector3 A = Pos();
                Vector3 B = Pos();
                A.z -= sz;
                B.z += sz + 1;
                DebugWide.DrawLine(A, B, Color.red);

                A = Pos();
                B = Pos();
                A.x -= sz;
                B.x += sz + 1;
                DebugWide.DrawLine(A, B, Color.red);

            }
        }

        public void Read(string line)
        {
            string[] sp = line.Split(' ');

            float x = float.Parse(sp[0]);
            float z = float.Parse(sp[1]);
            float r = float.Parse(sp[2]);
            m_iHealthGiven = int.Parse(sp[3]);
            int graphNodeIndex = int.Parse(sp[4]);


            SetPos(new Vector3(x, 0, z));
            SetBRadius(r);
            SetGraphNodeIndex(graphNodeIndex);

            //create this trigger's region of fluence
            AddCircularTriggerRegion(Pos(), Params.DefaultGiverTriggerRange);

            SetRespawnDelay((int)(Params.Health_RespawnDelay * Const.FrameRate));
            SetEntityType((int)eObjType.health);
        }
    }


    ///////////////////////////////////////////////////////////////////////////////


}


