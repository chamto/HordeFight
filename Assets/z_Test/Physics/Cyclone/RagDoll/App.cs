﻿using UnityEngine;
using System.Diagnostics;

namespace Cyclone
{
    public class RigidBodyApplication
    {
        Stopwatch stopWatch = Stopwatch.StartNew(); //new Stopwatch();

        /** Holds the maximum number of contacts. */
        protected static readonly uint maxContacts = 256;

        /** Holds the array of contacts. */
        protected Contact[] contacts = new Contact[maxContacts];

        /** Holds the collision data structure for collision detection. */
        protected CollisionData cData = new CollisionData();

        /** Holds the contact resolver. */
        protected ContactResolver resolver;

        /** Holds the camera angle. */
        protected float theta;

        /** Holds the camera elevation. */
        protected float phi;

        /** Holds the position of the mouse at the last frame of a drag. */
        protected float last_x, last_y, last_z;

        /** True if the contacts should be rendered. */
        protected bool renderDebugInfo;

        /** True if the simulation is paused. */
        protected bool pauseSimulation;

        /** Pauses the simulation after the next frame automatically */
        protected bool autoPauseSimulation;

        /** Processes the contact generation code. */
        protected virtual void generateContacts() { }

        /** Processes the objects in the simulation forward in time. */
        protected virtual void updateObjects(float duration) { }


        public RigidBodyApplication()
        {
            theta = 0.0f;
            phi = 15.0f;
            resolver = new ContactResolver(maxContacts * 8);

            renderDebugInfo = false;
            pauseSimulation = true;
            autoPauseSimulation = false;

            for (int i = 0; i < maxContacts; i++)
            {
                contacts[i] = new Contact();
            }
            cData.contactArray = contacts;
        }

        /** 
         * Finishes drawing the frame, adding debugging information 
         * as needed.
         */
        //protected void drawDebug();
        protected void drawDebug()
        {
            if (!renderDebugInfo) return;

            // Recalculate the contacts, so they are current (in case we're
            // paused, for example).
            generateContacts();

            // Render the contacts, if required
            //glBegin(GL_LINES);
            for (uint i = 0; i < cData.contactCount; i++)
            {
                Color cc = Color.green;
                // Interbody contacts are in green, floor contacts are red.
                if (null != contacts[i].body[1])
                {
                    //glColor3f(0, 1, 0);
                    cc = Color.white;
                }
                else
                {
                    //glColor3f(1, 0, 0);
                    cc = Color.red;
                }

                Vector3 vec = contacts[i].contactPoint;
                //glVertex3f(vec.x, vec.y, vec.z);

                Vector3 vec2 = vec + contacts[i].contactNormal;
                //glVertex3f(vec.x, vec.y, vec.z);

                UnityEngine.Vector3 start = new UnityEngine.Vector3(vec.x, vec.y, vec.z);
                UnityEngine.Vector3 end = new UnityEngine.Vector3(vec2.x, vec2.y, vec2.z);
                DebugWide.DrawLine(start, end, cc);
            }
            //glEnd();
        }

        /** Resets the simulation. */
        //protected virtual void reset() = 0;


        /** 
         * Creates a new application object.
         */
        //public RigidBodyApplication();

        ///** Display the application. */
        //public virtual void display();

        ///** Update the objects. */
        long __prevMs = 0;
        long __timeStepMs = 0;
        public void update()
        {
            __timeStepMs = (stopWatch.ElapsedMilliseconds - __prevMs);
            __prevMs = stopWatch.ElapsedMilliseconds;

            // Find the duration of the last frame in seconds
            //float duration = (float)TimingData::get().lastFrameDuration * 0.001f;
            float duration = (float)__prevMs * 0.001f;
            if (duration <= 0.0f) return;
            else if (duration > 0.05f) duration = 0.05f;

            //// Exit immediately if we aren't running the simulation
            if (pauseSimulation)
            {
                //update();
                return;
            }
            else if (autoPauseSimulation)
            {
                pauseSimulation = true;
                autoPauseSimulation = false;
            }

            // Update the objects
            updateObjects(duration);

            // Perform the contact generation
            generateContacts();

            // Resolve detected contacts
            resolver.resolveContacts(
                cData.contactArray,
                cData.contactCount,
                duration
                );

            //Application::update();
        }

        ///** Handle a mouse click. */
        //public virtual void mouse(int button, int state, int x, int y);

        ///** Handle a mouse drag */
        //public virtual void mouseDrag(int x, int y);

        ///** Handles a key press. */
        //public virtual void key(unsigned char key);

        public void Input_RenderDebugInfo()
        {
            renderDebugInfo = !renderDebugInfo;
        }
        public void Input_PauseSimul()
        {
            pauseSimulation = !pauseSimulation;
        }
        public void Input_AutoPause()
        {
            autoPauseSimulation = true;
            pauseSimulation = false;
        }

    }
}