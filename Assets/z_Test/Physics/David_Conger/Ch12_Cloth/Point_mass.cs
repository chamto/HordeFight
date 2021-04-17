using UnityEngine;

namespace pmframework
{
    public class Point_mass_base
    {

        float mass;
        Vector3 centerOfMassLocation;
        Vector3 linearVelocity;
        Vector3 linearAcceleration;
        Vector3 constantForce;
        Vector3 impulseForce;

        float radius;
        float coefficientOfRestitution;

        bool isImmovable;


        public Point_mass_base()
        {
            mass = 0.0f;
            radius = 0.0f;
            coefficientOfRestitution = 0.0f;
            isImmovable = false;
        }

        public void Mass(float massValue)
        {
            //assert(massValue > 0.0);

            mass = massValue;
        }

        public float Mass()
        {
            return (mass);
        }

        public void Location(Vector3 locationCenterOfMass)
        {
            centerOfMassLocation = locationCenterOfMass;
        }

        public Vector3 Location()
        {
            return (centerOfMassLocation);
        }

        public void LinearVelocity(Vector3 newVelocity)
        {
            if (!isImmovable)
            {
                linearVelocity = newVelocity;
            }
            else
            {
                linearVelocity = new Vector3(0.0f, 0.0f, 0.0f);
            }
        }

        public Vector3 LinearVelocity()
        {
            return (linearVelocity);
        }

        public void LinearAcceleration(Vector3 newAcceleration)
        {
            if (!isImmovable)
            {
                linearAcceleration = newAcceleration;
            }
            else
            {
                linearAcceleration = new Vector3(0.0f, 0.0f, 0.0f);
            }
        }

        public Vector3 LinearAcceleration()
        {
            return (linearAcceleration);
        }

        public void ConstantForce(Vector3 sumConstantForces)
        {
            if (!isImmovable)
            {
                constantForce = sumConstantForces;
            }
            else
            {
                constantForce = new Vector3(0.0f, 0.0f, 0.0f);
            }
        }

        public Vector3 ConstantForce()
        {
            return (constantForce);
        }

        public void ImpulseForce(Vector3 sumImpulseForces)
        {
            if (!isImmovable)
            {
                impulseForce = sumImpulseForces;
            }
            else
            {
                impulseForce = new Vector3(0.0f, 0.0f, 0.0f);
            }
        }

        public Vector3 ImpulseForce()
        {
            return (impulseForce);
        }

        public void BoundingSphereRadius(float sphereRadius)
        {
            radius = sphereRadius;
        }

        public float BoundingSphereRadius()
        {
            return (radius);
        }

        public void Elasticity(float elasticity)
        {
            coefficientOfRestitution = elasticity;
        }

        public float Elasticity()
        {
            return (coefficientOfRestitution);
        }

        public void IsImmovable(bool isMassImmovable)
        {
            isImmovable = isMassImmovable;
        }

        public bool IsImmovable()
        {
            return (isImmovable);
        }

        public virtual bool Update(float changeInTime)
        {
            //
            // Begin calculating linear dynamics.
            //

            // Find the linear acceleration.
            // a = F/m
            //assert(mass != 0);
            linearAcceleration = (constantForce + impulseForce) / mass;

            // Set the impulse force to zero.
            impulseForce = new Vector3(0.0f, 0.0f, 0.0f);

            // Find the linear velocity.
            linearVelocity += linearAcceleration * changeInTime;

            // Find the new location of the center of mass.
            centerOfMassLocation += linearVelocity * changeInTime;

            //
            // End calculating linear dynamics.
            //

            return (true);
        }
    }



    // Begin point_mass--------------------------------------------------
    public class Point_mass : Point_mass_base
    {

        //mesh objectMesh;

        //D3DXMATRIX worldMatrix;


        //public bool LoadMesh(string meshFileName)
        //{
        //    //assert(meshFileName.length() > 0);

        //    return (objectMesh.Load(meshFileName));
        //}

        //public void ShareMesh(ref point_mass sourceMass)
        //{
        //    objectMesh = sourceMass.objectMesh;
        //}

        override public bool Update(float changeInTime)
        {
            base.Update(changeInTime);

            // Create the translation matrix.
            //D3DXMatrixTranslation(
            //&worldMatrix,
            //Location().x,
            //Location().y,
            //Location().z);

            return (true);
        }

        public bool Render()
        {
            // Save the world transformation matrix.
            //D3DXMATRIX saveWorldMatrix;
            //theApp.D3DRenderingDevice()->GetTransform(
            //    D3DTS_WORLD,
            //    &saveWorldMatrix);

            //// Apply the world transformation matrix for this object.
            //theApp.D3DRenderingDevice()->SetTransform(
            //    D3DTS_WORLD, &worldMatrix);

            //// Now render the object with its transformations.
            //bool renderedOK = objectMesh.Render();

            //// Restore the world transformation matrix.
            //theApp.D3DRenderingDevice()->SetTransform(
            //    D3DTS_WORLD,
            //    &saveWorldMatrix);

            //return (renderedOK);

            DebugWide.DrawCircle(Location(), BoundingSphereRadius(), Color.blue);

            return true;
        }
    }



}

