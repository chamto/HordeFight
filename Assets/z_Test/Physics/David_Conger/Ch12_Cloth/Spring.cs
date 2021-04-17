using UnityEngine;
using UtilGS9;

namespace pmframework
{

    public class Spring
    {

        float restLength;
        float forceConstant;
        float dampeningFactor;

        Point_mass_base pointMass1;
        Point_mass_base pointMass2;


        public Spring()
        {
            restLength = 0.0f;
            forceConstant = 0.0f;
            dampeningFactor = 0.0f;

            pointMass1 = null;
            pointMass2 = null;
        }

        public void Length(float springLength)
        {
            restLength = springLength;
        }

        public float Length()
        {
            return (restLength);
        }

        public void ForceConstant(float springForceConstant)
        {
            forceConstant = springForceConstant;
        }

        public float ForceConstant()
        {
            return (forceConstant);
        }

        public void DampeningFactor(float dampeningConstant)
        {
            dampeningFactor = dampeningConstant;
        }

        public float DampeningFactor()
        {
            return (dampeningFactor);
        }

        public void EndpointMass1(Point_mass_base particle1)
        {
            pointMass1 = particle1;
        }

        public Point_mass_base EndpointMass1()
        {
            return (pointMass1);
        }

        public void EndpointMass2(Point_mass_base particle2)
        {
            pointMass2 = particle2;
        }

        public Point_mass_base EndpointMass2()
        {
            return (pointMass2);
        }

        public bool IsDisplaced()
        {
            //assert(pointMass1 != NULL);
            //assert(pointMass2 != NULL);

            bool isDisplaced = false;

            Vector3 currentLength;

            /* Find the distance between the particles to which the spring is
            connected. */
            currentLength =
                pointMass1.Location() - pointMass2.Location();

            /* Find the difference between the rest length and the current
            length. */
            float lengthDifference =
                currentLength.sqrMagnitude - (restLength * restLength);

            // If the difference is not essentially zero...
            if (!Misc.IsZero(lengthDifference))
            {
                isDisplaced = true;
            }

            return (isDisplaced);
        }

        public void CalculateReactions(float changeInTime)
        {
            //assert(pointMass1 != NULL);
            //assert(pointMass2 != NULL);

            Vector3 currentLength;

            // Get the current length of the spring.
            currentLength =
                pointMass1.Location() - pointMass2.Location();

            // Change it to a scalar.
            float currentLengthMagnitude = currentLength.magnitude;

            /* Find the difference between the current length and the rest 
            length. */
            float changeInLength = currentLengthMagnitude - restLength;

            // If the change in length is close to zero...
            if (Misc.IsZero(changeInLength))
            {
                // Set it to zero.
                changeInLength = 0.0f;
            }

            // Find the magnitude of the force the spring exerts.
            float springForceMagnitude =
                forceConstant * changeInLength;

            // Find the magnitude of the dampening force on the spring.
            float dampeningForceMagnitude;
            if (changeInTime < 1.0f)
            {
                dampeningForceMagnitude =
                    dampeningFactor * changeInLength * changeInTime;
            }
            else
            {
                dampeningForceMagnitude =
                    dampeningFactor * changeInLength / changeInTime;
            }

            // The dampening force can't be greater than the spring force.
            if (dampeningForceMagnitude > springForceMagnitude)
            {
                dampeningForceMagnitude = springForceMagnitude;
            }

            //Dampen the spring force.
            float responseForceMagnitude =
                springForceMagnitude - dampeningForceMagnitude;

            // Change the response force to a vector.
            Vector3 responseForce =
                responseForceMagnitude *
                VOp.Normalize(currentLength);

            // Apply the response force to the particles.
            pointMass1.ImpulseForce(
                pointMass1.ImpulseForce() + -1 * responseForce);

            pointMass2.ImpulseForce(
                pointMass2.ImpulseForce() + responseForce);
        }

        public bool Render()
        {
            DebugWide.DrawLine(pointMass1.Location(), pointMass2.Location(), Color.blue);
            return true;
        }

    }



}

