using System;
using UnityEngine;
using Buckland;
using UtilGS9;

namespace Test_SimpleSoccer
{

    public class FieldPlayer : PlayerBase
    {

        //an instance of the state machine class
        StateMachine<FieldPlayer> m_pStateMachine;

        //limits the number of kicks a player may take per second
        Regulator m_pKickLimiter;



        public FieldPlayer(SoccerTeam home_team,
                 int home_region,
                 State<FieldPlayer> start_state,
                 Vector3 heading,
                 Vector3 velocity,
                 float mass,
                 float max_force,
                 float max_speed,
                 float max_turn_rate,
                 float scale,
                 player_role role) :
            base(home_team, home_region, heading, velocity, mass, max_force, max_speed, max_turn_rate, scale, role)
        {
            //set up the state machine
            m_pStateMachine = new StateMachine<FieldPlayer>(this);

            if (null != start_state)
            {
                m_pStateMachine.SetCurrentState(start_state);
                m_pStateMachine.SetPreviousState(start_state);
                m_pStateMachine.SetGlobalState(GlobalPlayerState.instance);

                m_pStateMachine.CurrentState().Enter(this);
            }

            m_pSteering.SeparationOn();

            //set up the kick regulator
            m_pKickLimiter = new Regulator(Prm.PlayerKickFrequency);
        }

        //call this to update the player's position and orientation
        public override void Update()
        {
            //run the logic for the current state
            m_pStateMachine.Update();

            //calculate the combined steering force
            m_pSteering.Calculate();

            //if no steering force is produced decelerate the player by applying a
            //braking force
            if (Misc.IsZero(m_pSteering.Force()))
            {
                const float BrakingRate = 0.8f;

                m_vVelocity = m_vVelocity * BrakingRate;
            }

            //the steering force's side component is a force that rotates the 
            //player about its axis. We must limit the rotation so that a player
            //can only turn by PlayerMaxTurnRate rads per update.
            float TurningForce = m_pSteering.SideComponent();

            //라디안 0.4는 약 22도 , 60분의1프레임에서 최대 22도 회전가능  
            TurningForce = Mathf.Clamp(TurningForce, -Prm.PlayerMaxTurnRate, Prm.PlayerMaxTurnRate);

            //rotate the heading vector
            Transformations.Vec3RotateYAroundOrigin(ref m_vHeading, TurningForce);

            //make sure the velocity vector points in the same direction as
            //the heading vector
            m_vVelocity = m_vHeading * m_vVelocity.magnitude;

            //and recreate m_vSide
            //m_vSide = m_vHeading.Perp();
            m_vSide = Vector3.Cross(Vector3.up, m_vHeading);


            //now to calculate the acceleration due to the force exerted by
            //the forward component of the steering force in the direction
            //of the player's heading
            Vector3 accel = m_vHeading * m_pSteering.ForwardComponent() / m_dMass;

            m_vVelocity += accel;

            //make sure player does not exceed maximum velocity
            m_vVelocity = VOp.Truncate(m_vVelocity, m_dMaxSpeed);

            //update the position
            m_vPosition += m_vVelocity;


            //enforce a non-penetration constraint if desired
            if (Prm.bNonPenetrationConstraint)
            {

                EntityFunctionTemplates.EnforceNonPenetrationContraint(this, AutoList < PlayerBase >.GetAllMembers());
            }
        }

        public override void Render()
        {
            //gdi->TransparentText();
            //gdi->TextColor(Cgdi::grey);

            Color cc;
            //set appropriate team color
            if (Team().Color() == SoccerTeam.team_color.blue) { cc = Color.blue; }
            else { cc = Color.red; }


            //render the player's body
            Transformations.Draw_WorldTransform(m_vecPlayerVB,
                                                   Pos(),
                                                   Heading(),
                                                   Side(),
                                                   Scale(), cc);


            //and 'is 'ead
            //gdi->BrownBrush();
            cc = Color.cyan;
            if (Prm.bHighlightIfThreatened && (Team().ControllingPlayer() == this) && isThreatened())
                cc = Color.yellow;
            //gdi->Circle(Pos(), 6);

            DebugWide.DrawCircle(Pos(), 6, cc);


            //render the state
            if (Prm.ViewStates)
            {
                //gdi->TextColor(0, 170, 0);
                //gdi->TextAtPos(m_vPosition.x, m_vPosition.y - 20, std::string(m_pStateMachine->GetNameOfCurrentState()));

                DebugWide.PrintText(new Vector3(m_vPosition.x, 0,m_vPosition.z - 20), new Color(0, 170, 0), m_pStateMachine.GetNameOfCurrentState());
            }

            //show IDs
            if (Prm.ViewIDs)
            {
                //gdi->TextColor(0, 170, 0);
                //gdi->TextAtPos(Pos().x - 20, Pos().y - 20, ttos(ID()));

                DebugWide.PrintText(new Vector3(Pos().x - 20, 0, Pos().z - 20), new Color(0,170,0), ID() + "");
            }


            if (Prm.bViewTargets)
            {
                //gdi->RedBrush();
                //gdi->Circle(Steering()->Target(), 3);
                //gdi->TextAtPos(Steering()->Target(), ttos(ID()));

                DebugWide.DrawCircle(Steering().Target(), 3, Color.red);
                DebugWide.PrintText(Steering().Target(), Color.red, ID() + "");
            }
        }

        public override bool HandleMessage(Telegram msg)
        {
            return m_pStateMachine.HandleMessage(msg);
        }

        public StateMachine<FieldPlayer> GetFSM() {return m_pStateMachine;}

        public bool isReadyForNextKick() {return m_pKickLimiter.isReady();}
         
    }
}

