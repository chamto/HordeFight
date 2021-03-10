using UnityEngine;
using UtilGS9;

namespace pmframework
{
    class Cloth
    {

        public struct index_pair
        {
            public int row, col;
        }

        public const int PARTICLES_PER_SQUARE = 4;
        public const int TOP_LEFT_PARTICLE = 0;
        public const int TOP_RIGHT_PARTICLE = 1;
        public const int BOTTOM_LEFT_PARTICLE = 2;
        public const int BOTTOM_RIGHT_PARTICLE = 3;

        public const int SPRINGS_PER_SQUARE = 6;
        public const int TOP_SPRING = 0;
        public const int BOTTOM_SPRING = 1;
        public const int RIGHT_SPRING = 2;
        public const int LEFT_SPRING = 3;
        public const int TOP_RIGHT_TO_BOTTOM_LEFT_SPRING = 4;
        public const int TOP_LEFT_TO_BOTTOM_RIGHT_SPRING = 5;
        public struct cloth_square
        {
            public index_pair[] particleIndex;
            public int[] springIndex;

            public void Init()
            {
                particleIndex = new index_pair[PARTICLES_PER_SQUARE];
                springIndex = new int[SPRINGS_PER_SQUARE];
            }
        }


        int totalRows;
        int totalCols;
        int totalSprings;
        //point_mass** allParticles;
        Point_mass[,] allParticles;

        //spring* allSprings;
        Spring[] allSprings;

        //cloth_square** allSquares;
        cloth_square[,] allSquares;

        float linearDampeningCoefficient;


        //파티클 불안정해지는 문제가 발생 , 원인 찾기 , 최대힘에 대한 제약이 없어서 생기는 문제로 추정 - 20210311 chamto
        void HandleCollision(Vector3 separationDistance, float changeInTime, index_pair firstParticle, index_pair secondParticle)
        {
            int row1 = firstParticle.row;
            int col1 = firstParticle.col;
            int row2 = secondParticle.row;
            int col2 = secondParticle.col;
            //
            // Find the outgoing velicities.
            //
            /* First, normalize the displacement vector because it's 
            perpendicular to the collision. */
            Vector3 unitNormal = VOp.Normalize(separationDistance);

            /* Compute the projection of the velocities in the direction
            perpendicular to the collision. */
            float velocity1 = Vector3.Dot(allParticles[row1, col1].LinearVelocity(), unitNormal);

            float velocity2 = Vector3.Dot(allParticles[row2, col2].LinearVelocity(), unitNormal);

            // Find the average coefficent of restitution.
            float averageE =
                (allParticles[row1, col1].Elasticity() *
                allParticles[row2, col2].Elasticity()) / 2;

            // Calculate the final velocities.
            float finalVelocity1 =
                (((allParticles[row1, col1].Mass() -
                (averageE * allParticles[row2, col2].Mass())) * velocity1) +
                ((1 + averageE) * allParticles[row2, col2].Mass() *
                velocity2)) / (allParticles[row1, col1].Mass() +
                allParticles[row2, col2].Mass());

            float finalVelocity2 =
                (((allParticles[row2, col2].Mass() -
                (averageE * allParticles[row1, col1].Mass())) * velocity2) +
                ((1 + averageE) * allParticles[row1, col1].Mass() * velocity1)) /
                (allParticles[row1, col1].Mass() + allParticles[row2, col2].Mass());

            allParticles[row1, col1].LinearVelocity(
                (finalVelocity1 - velocity1) * unitNormal +
                allParticles[row1, col1].LinearVelocity());
            allParticles[row2, col2].LinearVelocity(
                (finalVelocity2 - velocity2) * unitNormal +
                allParticles[row1, col1].LinearVelocity());

            //
            // Convert the velocities to accelerations.
            //
            Vector3 acceleration1 =
                allParticles[row1, col1].LinearVelocity() / changeInTime;
            Vector3 acceleration2 =
                allParticles[row2, col2].LinearVelocity() / changeInTime;

            // Find the force on each ball.
            allParticles[row1, col1].ImpulseForce(
                allParticles[row1, col1].ImpulseForce() +
                acceleration1 * allParticles[row1, col1].Mass());
            allParticles[row2, col2].ImpulseForce(
                allParticles[row2, col2].ImpulseForce() +
                acceleration2 * allParticles[row2, col2].Mass());
        }


        //    Parameters
        //Input:  particleRows - Contains the number of rows of particles in
        //    the cloth's particle-spring grid.
        //particleCols - Contains the number of columns of particles 
        //    in the cloth's particle-spring grid.
        //particleMass - Specifies the mass of each particle in the
        //    grid.
        //particleRadius - Specifies the radius of each particle in
        //    the grid.
        //particleElasticity - Contains the coefficient of
        //    restitution for each particle in the grid.
        //spaceBetweenParticles - Specifies the horizontal and
        //    vertical gap between the particles.
        //clothStiffness - Specifies the stiffness of the grid's
        //    springs.
        //dampeningFactor - Applies a dampening factor to each
        //    spring in the grid.
        //linearDampeningFactor - Applies an additional dampening
        //    factor to the particles for added stability. Set this
        //    to zero if you do not want the extra dampening.
        //upLeftCorner - Specifies the location of the upper left
        //corner of the cloth.
        public Cloth(
            int particleRows,
            int particleCols,
            float particleMass,
            float particleRadius,
            float particleElasticity,
            float spaceBetweenParticles,
            float clothStiffness,
            float dampeningFactor,
            float linearDampeningFactor,
            Vector3 upLeftCorner)
        {
            //assert(particleRows >= 2);
            //assert(particleCols >= 2);

            linearDampeningCoefficient = linearDampeningFactor;

            // Allocate memory for one dimension of the array of particles.
            //allParticles = new point_mass*[particleRows];
            allParticles = new Point_mass[particleCols, particleRows];
            for (int i = 0; i < particleRows; i++)
            {
                for (int j = 0; j < particleCols; j++)
                {
                    allParticles[i, j] = new Point_mass();
                }
            }

            // Set the total number of springs needed for the grid.
            totalSprings =
            (particleRows * (particleCols - 1)) +
            ((particleRows - 1) * particleCols) +
            ((particleRows - 1) * (particleCols - 1) * 2);

            // Allocate the springs.
            allSprings = new Spring[totalSprings];
            for (int i = 0; i < totalSprings; i++)
            {
                allSprings[i] = new Spring();
            }


            // Allocate the rows for the array of squares.
            //allSquares = new cloth_square*[particleRows - 1];
            allSquares = new cloth_square[particleCols - 1, particleRows - 1];
            for (int i = 0; i < particleRows - 1; i++)
            {
                for (int j = 0; j < particleCols - 1; j++)
                {
                    allSquares[i, j] = new cloth_square();
                    allSquares[i, j].Init();
                }
            }
            //==============================================

            Vector3 location = upLeftCorner;

            // Set the properties of each particle.
            for (int i = 0; i < particleRows; i++)
            {
                for (int j = 0; j < particleCols; j++)
                {
                    allParticles[i, j].Mass(particleMass);
                    allParticles[i, j].BoundingSphereRadius(particleRadius);
                    allParticles[i, j].Elasticity(particleElasticity);
                    allParticles[i, j].Location(location);
                    location.x = location.x + spaceBetweenParticles;
                }
                location.x = upLeftCorner.x;
                location.z = location.z - spaceBetweenParticles;
            }

            //
            /* For each square, connect the horizontal springs for the top 
            and bottom of each square. */
            //
            index_pair tempIndex;
            int currentSpring = 0;
            for (int i = 0; i < particleRows; i++)
            {
                for (int j = 0; j < particleCols - 1; j++)
                {

                    // Connect the top
                    allSprings[currentSpring].EndpointMass1(allParticles[i, j]);
                    allSprings[currentSpring].EndpointMass2(allParticles[i, j + 1]);

                    // If this is not the last row of particles...
                    if (i < particleRows - 1)
                    {
                        // Store the index of the top spring
                        allSquares[i, j].springIndex[TOP_SPRING] =
                            currentSpring;

                        // Store the indices of the two particles it connects.
                        tempIndex.row = i;
                        tempIndex.col = j;
                        allSquares[i, j].particleIndex[TOP_LEFT_PARTICLE] =
                            tempIndex;
                        tempIndex.col = j + 1;
                        allSquares[i, j].particleIndex[TOP_RIGHT_PARTICLE] =
                            tempIndex;
                    }

                    // If this is not the first row of particles...
                    if (i > 0)
                    {
                        /* This spring is already connected, store it as the 
                        index of the bottom spring */
                        allSquares[i - 1, j].springIndex[BOTTOM_SPRING] =
                            currentSpring;

                        // Store the indices of the two particles it connects.
                        tempIndex.row = i;
                        tempIndex.col = j;
                        allSquares[i - 1, j].particleIndex[BOTTOM_LEFT_PARTICLE] =
                            tempIndex;
                        tempIndex.col = j + 1;
                        allSquares[i - 1, j].particleIndex[BOTTOM_RIGHT_PARTICLE] =
                            tempIndex;
                    }

                    currentSpring++;
                }
            }

            //
            /* For each square, connect the vertical springs for the left 
            and right of each square. */
            //
            for (int i = 0; i < particleRows - 1; i++)
            {
                for (int j = 0; j < particleCols; j++)
                {

                    // Connect the left
                    allSprings[currentSpring].EndpointMass1(allParticles[i, j]);
                    allSprings[currentSpring].EndpointMass2(allParticles[i + 1, j]);

                    //If this is not the last column of particles...
                    if (j < particleCols - 1)
                    {
                        // Store the index of the left spring
                        allSquares[i, j].springIndex[LEFT_SPRING] = currentSpring;

                        // Store the indices of the two particles it connects.
                        tempIndex.row = i;
                        tempIndex.col = j;
                        allSquares[i, j].particleIndex[TOP_LEFT_PARTICLE] = tempIndex;
                        tempIndex.row = i + 1;
                        allSquares[i, j].particleIndex[BOTTOM_LEFT_PARTICLE] = tempIndex;
                    }

                    // If this is not the first column of particles...
                    if (j > 0)
                    {
                        // Store the index of the bottom spring
                        allSquares[i, j - 1].springIndex[RIGHT_SPRING] = currentSpring;

                        // Store the indices of the two particles it connects.
                        tempIndex.row = i;
                        tempIndex.col = j;
                        allSquares[i, j - 1].particleIndex[TOP_RIGHT_PARTICLE] = tempIndex;
                        tempIndex.row = i + 1;
                        allSquares[i, j - 1].particleIndex[BOTTOM_RIGHT_PARTICLE] = tempIndex;
                    }

                    currentSpring++;
                }
            }

            //
            // For each square, connect the diagonal springs.
            //
            for (int i = 0; i < particleRows - 1; i++)
            {
                for (int j = 0; j < particleCols - 1; j++)
                {
                    /* Connect the spring from the top left to the bottom 
                    right. */
                    allSprings[currentSpring].EndpointMass1(allParticles[i, j]);
                    allSprings[currentSpring].EndpointMass2(allParticles[i + 1, j + 1]);
                    allSquares[i, j].springIndex[TOP_RIGHT_TO_BOTTOM_LEFT_SPRING] =
                    currentSpring++;

                    /* Connect the spring from the top right to the bottom 
                    left. */
                    allSprings[currentSpring].EndpointMass1(allParticles[i, j + 1]);
                    allSprings[currentSpring].EndpointMass2(allParticles[i + 1, j]);
                    allSquares[i, j].springIndex[TOP_LEFT_TO_BOTTOM_RIGHT_SPRING] =
                    currentSpring++;
                }
            }

            // Now set the common properties of all springs.
            for (int i = 0; i < totalSprings; i++)
            {
                allSprings[i].DampeningFactor(dampeningFactor);
                allSprings[i].ForceConstant(clothStiffness);
                Vector3 tempVector =
                    allSprings[i].EndpointMass1().Location() - allSprings[i].EndpointMass2().Location();
                allSprings[i].Length(tempVector.magnitude);
            }

            totalRows = particleRows;
            totalCols = particleCols;
        }

        public void ParticleImpulseForce(int row, int col, Vector3 impulseForce)
        {
            //assert((row >= 0) && (row < totalRows));
            //assert((col >= 0) && (col < totalCols));

            allParticles[row, col].ImpulseForce(impulseForce);
        }
        public Vector3 ParticleImpulseForce(int row, int col)
        {
            //assert((row >= 0) && (row < totalRows));
            //assert((col >= 0) && (col < totalCols));

            return (allParticles[row, col].ImpulseForce());
        }

        public void ParticleConstantForce(int row, int col, Vector3 constantForce)
        {
            //assert((row >= 0) && (row < totalRows));
            //assert((col >= 0) && (col < totalCols));

            allParticles[row, col].ConstantForce(constantForce);
        }

        public Vector3 ParticleConstantForce(int row, int col)
        {
            //assert((row >= 0) && (row < totalRows));
            //assert((col >= 0) && (col < totalCols));

            return (allParticles[row, col].ConstantForce());
        }

        public void IsParticleImmovable(int row, int col, bool isMassImmovable)
        {
            //assert((row >= 0) && (row < totalRows));
            //assert((col >= 0) && (col < totalCols));

            allParticles[row, col].IsImmovable(isMassImmovable);
        }

        public bool IsParticleImmovable(int row, int col)
        {
            //assert((row >= 0) && (row < totalRows));
            //assert((col >= 0) && (col < totalCols));

            return (allParticles[row, col].IsImmovable());
        }

        //public bool LoadMesh(string meshFileName)
        //{
        //    //assert(allSquares != NULL);
        //    //assert(allSprings != NULL);
        //    //assert(allParticles != NULL);
        //    //assert(totalSprings > 0);
        //    //assert(totalRows > 0);
        //    //assert(totalCols > 0);

        //    // Load the mesh for the first particle.
        //    bool meshLoaded =
        //        allParticles[0][0].LoadMesh(meshFileName);

        //    // Share the mesh with all the other particles.
        //    for (int i = 0; (meshLoaded) && (i < totalRows); i++)
        //    {
        //        for (int j = 0; j < totalCols; j++)
        //        {
        //            if (!((i == 0) && (j == 0)))
        //            {
        //                allParticles[i][j].ShareMesh(allParticles[0][0]);
        //            }
        //        }
        //    }

        //    return (meshLoaded);
        //}

        public bool Update(float changeInTime)
        {
            int i, j;

            // Calculate the force exerted by each spring.
            for (i = 0; i < totalSprings; i++)
            {
                if (allSprings[i].IsDisplaced())
                {
                    allSprings[i].CalculateReactions(changeInTime);
                }
            }

            // Update the position of every particle in the grid.
            for (i = 0; i < totalRows; i++)
            {
                for (j = 0; j < totalCols; j++)
                {
                    // 
                    /* Test for a collision between the current particle and
                    the remaining particles. Don't bother to test against
                    previous particles in the array. */
                    //

                    for (int k = i; k < totalCols; k++)
                    {
                        for (int m = j + 1; m < totalCols; m++)
                        {
                            // Find the distance vector between the particles.
                            Vector3 distance =
                                allParticles[i, j].Location() -
                                allParticles[k, m].Location();
                            float distanceSquared = distance.sqrMagnitude;

                            // Find the square of the sum of the radii of the balls.
                            float minDistanceSquared =
                                allParticles[i, j].BoundingSphereRadius() +
                                allParticles[k, m].BoundingSphereRadius();
                            minDistanceSquared *= minDistanceSquared;

                            // If there is a collision...
                            if (distanceSquared < minDistanceSquared)
                            {
                                index_pair firstParticle, secondParticle;
                                firstParticle.row = i;
                                firstParticle.col = j;
                                secondParticle.row = k;
                                secondParticle.col = m;

                                // Handle the collision.
                                HandleCollision(
                                    distance, changeInTime,
                                    firstParticle, secondParticle);
                            }
                        }
                    }
                }
            }

            //
            /* This is cheating. Spring-particle systems are notoriously
            unstable. To make this behave more like cloth, dampen the 
            movement of the particles in the system.*/
            //
            Vector3 dampening;
            for (i = 0; i < totalRows; i++)
            {
                for (j = 0; j < totalCols; j++)
                {
                    dampening =
                        -linearDampeningCoefficient *
                        allParticles[i, j].LinearVelocity();
                    allParticles[i, j].LinearVelocity(
                        allParticles[i, j].LinearVelocity() +
                        dampening);
                }
            }

            // Update each ball.
            for (i = 0; i < totalRows; i++)
            {
                for (j = 0; j < totalCols; j++)
                {
                    allParticles[i, j].Update(changeInTime);
                }
            }

            return (true);
        }

        public bool Render()
        {
            bool renderOK = true;

            for (int i = 0; (renderOK) && (i < totalRows); i++)
            {
                for (int j = 0; (renderOK) && (j < totalCols); j++)
                {
                    renderOK = allParticles[i, j].Render();
                }
            }

            for (int i = 0; i < totalSprings; i++)
            {
                allSprings[i].Render();
            }

            return (renderOK);
        }
    }



}

