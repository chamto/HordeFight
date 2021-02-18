using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

namespace Raven
{
    //=======

    public class Trigger_WeaponGiver : Trigger_Respawning<Raven_Bot>
    {

        //vrtex buffers for rocket shape
        List<Vector3> m_vecRLVB = new List<Vector3>();
        List<Vector3> m_vecRLVBTrans;


        //this type of trigger is created when reading a map file
        public Trigger_WeaponGiver(int id, string line) : base(id)
        {
            Read(line);

            //create the vertex buffer for the rocket shape
            const int NumRocketVerts = 8;
            Vector3[] rip = new Vector3[NumRocketVerts] {
                                               new Vector3(0, 0, 3),
                                               new Vector3(1, 0, 2),
                                               new Vector3(1, 0, 0),
                                               new Vector3(2, 0, -2),
                                               new Vector3(-2, 0, -2),
                                               new Vector3(-1, 0, 0),
                                               new Vector3(-1, 0, 2),
                                               new Vector3(0, 0, 3)};

            for (int i = 0; i < NumRocketVerts; ++i)
            {
                m_vecRLVB.Add(rip[i]);
            }
        }

        //if triggered, this trigger will call the PickupWeapon method of the
        //bot. PickupWeapon will instantiate a weapon of the appropriate type.
        public override void Try(Raven_Bot type)
        {
            if (this.isActive() && this.isTouchingTrigger(type.Pos(), type.BRadius()))
            {
                type.GetWeaponSys().AddWeapon(EntityType());

                Deactivate();
            }
        }

        //draws a symbol representing the weapon type at the trigger's location
        public override void Render()
        {
            if (isActive())
            {
                switch (EntityType())
                {
                    case (int)eObjType.rail_gun:
                        {

                            DebugWide.DrawCircle(Pos(), 3, Color.blue);

                            Vector3 PosTo = Pos();
                            PosTo.z -= PosTo.z;
                            DebugWide.DrawLine(Pos(), PosTo, Color.blue);

                        }

                        break;

                    case (int)eObjType.shotgun:
                        {

                            const float sz = 3.0f;
                            Vector3 cen = Pos();
                            cen.x -= sz;
                            DebugWide.DrawCircle(cen, sz, Color.black);

                            cen = Pos();
                            cen.x += sz;
                            DebugWide.DrawCircle(cen, sz, Color.black);

                        }

                        break;

                    case (int)eObjType.rocket_launcher:
                        {

                            Vector3 facing = new Vector3(-1, 0, 0);

                            Transformation.Draw_WorldTransform(m_vecRLVB,
                                                           Pos(),
                                                           facing,
                                                           Vector3.Cross(facing, Vector3.up),
                                                           new Vector3(2.5f, 0, 2.5f),
                                                           Color.red);

                        }

                        break;

                }//end switch
            }
        }

        public void Read(string line)
        {
            string[] sp = HandyString.SplitBlank(line);

            float x = float.Parse(sp[0]);
            float z = float.Parse(sp[1]);
            float r = float.Parse(sp[2]);

            int graphNodeIndex = int.Parse(sp[3]);

            SetPos(new Vector3(x, 0, z));
            SetBRadius(r);
            SetGraphNodeIndex(graphNodeIndex);

            //create this trigger's region of fluence
            AddCircularTriggerRegion(Pos(), Params.DefaultGiverTriggerRange);


            SetRespawnDelay((int)(Params.Weapon_RespawnDelay * Const.FrameRate));
        }
    }


    ///////////////////////////////////////////////////////////////////////////////


}


