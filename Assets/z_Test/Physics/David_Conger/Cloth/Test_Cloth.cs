using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

namespace pmframework
{
    public class Test_Cloth : MonoBehaviour
    {
        const int MILLISECONDS_PER_FRAME = 33;
        float STEP_SIZE = ((float)MILLISECONDS_PER_FRAME / 1000.0f);
		
        int CLOTH_PARTICLE_ROWS = 5;
        int CLOTH_PARTICLE_COLS = 5;
        Vector3 CLOTH_UP_LEFT_CORNER = new Vector3(-6.0f, 4.0f, 10.0f);
        float CLOTH_PARTICLE_MASS = 0.1f;
        float CLOTH_PARTICLE_RADIUS = 0.5f;
        float CLOTH_PARTICLE_ELASTICITY = 0.01f;
        float CLOTH_PARTICLE_GAP = 1.5f;
        float CLOTH_SPRING_STIFFNESS = 50.0f;
        float CLOTH_SPRING_DAMPENING_FACTOR = 1000.0f;
        float CLOTH_LINEAR_DAMPENING_FACTOR = 0.1f;
        float GRAVITATIONAL_ACCELERATION = -9.8f;
        Vector3 INITIAL_IMPULSE_FORCE = new Vector3(30.0f, 0.0f, 0.0f);

        //==================================================

        Cloth theCloth = null;

        // Use this for initialization
        void Start()
        {
            theCloth = new Cloth(
                CLOTH_PARTICLE_ROWS,
                CLOTH_PARTICLE_COLS,
                CLOTH_PARTICLE_MASS,
                CLOTH_PARTICLE_RADIUS,
                CLOTH_PARTICLE_ELASTICITY,
                CLOTH_PARTICLE_GAP,
                CLOTH_SPRING_STIFFNESS,
                CLOTH_SPRING_DAMPENING_FACTOR,
                CLOTH_LINEAR_DAMPENING_FACTOR,
                CLOTH_UP_LEFT_CORNER);

            theCloth.IsParticleImmovable(0, 0, true);
            theCloth.IsParticleImmovable(0, CLOTH_PARTICLE_COLS - 1, true);
            //theCloth.LoadMesh("ball.x");

            // Set the initial force on one ball.
            theCloth.ParticleImpulseForce(
                CLOTH_PARTICLE_ROWS - 1,
                CLOTH_PARTICLE_COLS - 1,
                INITIAL_IMPULSE_FORCE);

            for (int i = 0; i < CLOTH_PARTICLE_ROWS; i++)
            {
                for (int j = 0; j < CLOTH_PARTICLE_COLS; j++)
                {
                    // Add gravity.
                    theCloth.ParticleConstantForce(
                        i, j,
                        new Vector3(
                            0.0f,
                            0.0f,
                            GRAVITATIONAL_ACCELERATION * CLOTH_PARTICLE_MASS));
                }
            }
        }

        // Update is called once per frame
        //void Update () {
        //}

        private void OnDrawGizmos()
        {
            if (null == theCloth) return;

            UpdateFrame();
            theCloth.Render();
        }

        static float lastTime = 0;
        bool TimeToUpdateFrame(float currentTime)
        {
            // This initialization is only done once.
            //static float lastTime = 0;

            // This initialization happens each time the function is called.
            bool updateFrame = false;

            // If enough milliseconds have elapsed...
            if (currentTime - lastTime >= MILLISECONDS_PER_FRAME/1000f)
            {
                // It's time to update the frame.
                updateFrame = true;

                // Save the time that the frame was updated.
                lastTime = currentTime;
            }
            return (updateFrame);
        }

        public float RandomScalar(float maxValue)
        {
            // Generate a random number.
            float randomNumber = (float)Misc.rand.Next();

            // While the number is greater than maxValue...
            while (randomNumber > maxValue)
            {
                // Divide the number by 10.
                randomNumber /= 10;
            }
            return (randomNumber);
        }

        static float frameCount = 0;
        static int currentRow = 0;
        bool UpdateFrame()
        {
            // Is it time to update the frame?
            float currentTime = Time.time;
            if (!TimeToUpdateFrame(currentTime))
                return (true);

            // After a certain number of frames go by...
            if (frameCount >= CLOTH_PARTICLE_ROWS * 10)
            {
                //
                // Apply a force to a row of particles.
                //
                Vector3 impulse = Vector3.zero;

                // Set random x, y, and z values.
                impulse.x = RandomScalar(20);
                impulse.y = RandomScalar(20);
                impulse.z = RandomScalar(20);
                //DebugWide.LogBlue(frameCount);
                // Apply the force vector to every particle in the row.
                for (int i = 0; i < CLOTH_PARTICLE_COLS; i++)
                {
                    theCloth.ParticleImpulseForce(
                        currentRow++,
                        i,
                        impulse);
                }

                /* If the current row is greater than or equal to the last 
                row... */
                if (currentRow >= CLOTH_PARTICLE_ROWS)
                {
                    // Reset the current row to zero.
                    currentRow = 0;
                }
                frameCount = 0;
            }
            else
            {
                frameCount++;
            }

            theCloth.Update(STEP_SIZE);
            return (true);
        }

    }


    //======================================================

}

