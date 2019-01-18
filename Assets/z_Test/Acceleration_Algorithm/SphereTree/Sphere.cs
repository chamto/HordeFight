using UnityEngine;


public interface SphereInterface
{
    int GetVertexCount();

    //public virtual bool GetVertex(int i, Vector3d<float> &vect) = 0;
    bool GetVertex(int i, out Vector3 vect);
}



public class Sphere
{
    const float BIGNUMBER = 100000000.0f;     /* hundred million */

    //protected Vector3d<float> mCenter;
    protected Vector3 mCenter;
    private float mRadius;
    private float mRadius2; // radius squared.

    public Sphere()
    {
        mCenter = Vector3.zero;
        mRadius = 0f;
        mRadius2 = 0f;
    }

    //public Sphere(Vector3d<float> &center, float radius)
    public Sphere(ref Vector3 center, float radius)
    {
        this.Set(ref center, radius);
    }

    //public Set(Vector3d<float> &center, float radius)
    public void Set(ref Vector3 center, float radius)
    {
        mCenter = center;
        mRadius = radius;
        mRadius2 = radius * radius;
    }


    /*
    An Efficient Bounding Sphere
    by Jack Ritter
    from "Graphics Gems", Academic Press, 1990
    */

    /* Routine to calculate tight bounding sphere over    */
    /* a set of points in 3D */
    /* This contains the routine find_bounding_sphere(), */
    /* the struct definition, and the globals used for parameters. */
    /* The abs() of all coordinates must be < BIGNUMBER */
    /* Code written by Jack Ritter and Lyle Rains. */
    //void Compute(const SphereInterface &source);
    public void Compute(ref SphereInterface source)
    {
        Vector3 xmin, xmax, ymin, ymax, zmin, zmax, dia1, dia2;

        /* FIRST PASS: find 6 minima/maxima points */
        xmin = new Vector3(BIGNUMBER, BIGNUMBER, BIGNUMBER);
        xmax = new Vector3(-BIGNUMBER, -BIGNUMBER, -BIGNUMBER);
        ymin = new Vector3(BIGNUMBER, BIGNUMBER, BIGNUMBER);
        ymax = new Vector3(-BIGNUMBER, -BIGNUMBER, -BIGNUMBER);
        zmin = new Vector3(BIGNUMBER, BIGNUMBER, BIGNUMBER);
        zmax = new Vector3(-BIGNUMBER, -BIGNUMBER, -BIGNUMBER);

        int count = source.GetVertexCount();


        for (int i = 0; i < count; i++)
        {
            Vector3 caller_p;
            source.GetVertex(i, out caller_p);

            if (caller_p.x < xmin.x) xmin = caller_p; /* New xminimum point */
            if (caller_p.x > xmax.x) xmax = caller_p;
            if (caller_p.y < ymin.y) ymin = caller_p;
            if (caller_p.y > ymax.y) ymax = caller_p;
            if (caller_p.y < zmin.z) zmin = caller_p;
            if (caller_p.z > zmax.z) zmax = caller_p;
        }

        /* Set xspan = distance between the 2 points xmin & xmax (squared) */
        float dx = xmax.x - xmin.x;
        float dy = xmax.y - xmin.y;
        float dz = xmax.z - xmin.z;
        float xspan = dx * dx + dy * dy + dz * dz;

        /* Same for y & z spans */
        dx = ymax.x - ymin.x;
        dy = ymax.y - ymin.y;
        dz = ymax.z - ymin.z;
        float yspan = dx * dx + dy * dy + dz * dz;

        dx = zmax.x - zmin.x;
        dy = zmax.y - zmin.y;
        dz = zmax.z - zmin.z;
        float zspan = dx * dx + dy * dy + dz * dz;

        /* Set points dia1 & dia2 to the maximally separated pair */
        dia1 = xmin;
        dia2 = xmax; /* assume xspan biggest */
        float maxspan = xspan;

        if (yspan > maxspan)
        {
            maxspan = yspan;
            dia1 = ymin;
            dia2 = ymax;
        }

        if (zspan > maxspan)
        {
            dia1 = zmin;
            dia2 = zmax;
        }


        /* dia1,dia2 is a diameter of initial sphere */
        /* calc initial center */
        mCenter.x = (dia1.x + dia2.x) * 0.5f;
        mCenter.y = (dia1.y + dia2.y) * 0.5f;
        mCenter.z = (dia1.z + dia2.z) * 0.5f;
        /* calculate initial radius**2 and radius */
        dx = dia2.x - mCenter.x; /* x component of radius vector */
        dy = dia2.y - mCenter.y; /* y component of radius vector */
        dz = dia2.z - mCenter.z; /* z component of radius vector */
        mRadius2 = dx * dx + dy * dy + dz * dz;
        mRadius = Mathf.Sqrt(mRadius2);

        /* SECOND PASS: increment current sphere */

        for (int i = 0; i < count; i++)
        {
            Vector3 caller_p;
            source.GetVertex(i, out caller_p);

            dx = caller_p.x - mCenter.x;
            dy = caller_p.y - mCenter.y;
            dz = caller_p.z - mCenter.z;

            float old_to_p_sq = dx * dx + dy * dy + dz * dz;
            if (old_to_p_sq > mRadius2)   /* do r**2 test first */
            {   /* this point is outside of current sphere */

                float old_to_p = Mathf.Sqrt(old_to_p_sq);
                /* calc radius of new sphere */
                mRadius = (mRadius + old_to_p) * 0.5f;
                mRadius2 = mRadius * mRadius;   /* for next r**2 compare */

                float old_to_new = old_to_p - mRadius;
                /* calc center of new sphere */
                float recip = 1.0f / old_to_p;

                float cx = (mRadius * mCenter.x + old_to_new * caller_p.x) * recip;
                float cy = (mRadius * mCenter.y + old_to_new * caller_p.y) * recip;
                float cz = (mRadius * mCenter.z + old_to_new * caller_p.z) * recip;

                mCenter.x = cx;
                mCenter.y = cy;
                mCenter.z = cz;
            }
        }
    }


    public float GetRadius() { return mRadius; }
    public float GetRadius2() { return mRadius2; }
    //public Vector3d<float>& GetCenter(void) { return mCenter; }
    public Vector3 GetCenter() { return mCenter; }

    //public bool RayIntersection(Vector3d<float> &rayOrigin, Vector3d<float> &V, float distance, Vector3d<float>* intersect)
    public bool RayIntersection(ref Vector3 rayOrigin, ref Vector3 V, float distance, out Vector3 intersect)
    {
        Vector3 sect;
        bool hit = RayIntersectionInFront(ref rayOrigin, ref V, out sect);

        intersect = Vector3.zero;
        if (hit)
        {
            float d = (rayOrigin - sect).sqrMagnitude;
            if (d > (distance * distance)) return false;
            intersect = sect;
            return true;
        }
        return false;
    }


    //ray-sphere intersection test from Graphics Gems p.388
    // **NOTE** There is a bug in this Graphics Gem.  If the origin
    // of the ray is *inside* the sphere being tested, it reports the
    // wrong intersection location.  This code has a fix for the bug.
    //public bool RayIntersection(Vector3d<float> &rayOrigin, Vector3d<float> &rayDirection, Vector3d<float>* intersect)
    public bool RayIntersection(ref Vector3 rayOrigin, ref Vector3 rayDirection, out Vector3 intersect)
    {
        //notation:
        //point E  = rayOrigin
        //point O  = sphere center

        Vector3 EO = mCenter - rayOrigin;
        Vector3 V = rayDirection;
        float dist2 = EO.x * EO.x + EO.y * EO.y + EO.z * EO.z;
        // Bug Fix For Gem, if origin is *inside* the sphere, invert the
        // direction vector so that we get a valid intersection location.
        if (dist2 < mRadius2) V *= -1;

        float v = Vector3.Dot(EO, V);

        float disc = mRadius2 - (EO.sqrMagnitude - v * v);

        intersect = Vector3.zero;
        if (disc > 0.0f)
        {

            float d = Mathf.Sqrt(disc);

            //float dist2 = (rayOrigin - mCenter).sqrMagnitude;

            intersect = rayOrigin + V * (v - d);


            return true;
        }
        return false;
    }


    //public bool RayIntersectionInFront(Vector3d<float> &rayOrigin, Vector3d<float> &rayDirection, Vector3d<float>* intersect)
    public bool RayIntersectionInFront(ref Vector3 rayOrigin, ref Vector3 rayDirection, out Vector3 intersect)
    {
        Vector3 sect;
        bool hit = RayIntersection(ref rayOrigin, ref rayDirection, out sect);

        intersect = Vector3.zero;
        if (hit)
        {
            Vector3 dir = sect - rayOrigin;

            float dot = Vector3.Dot(dir, rayDirection);

            if (dot >= 0) // then it's in front!
            {
                intersect = sect;
                return true;
            }
        }
        return false;
    }

    public void Report() { }

    public void SetRadius(float radius)
    {
        mRadius = radius;
        mRadius2 = radius * radius;
    }


    public bool InSphereXY(ref Vector3 pos, float distance)
    {
        float dx = pos.x - mCenter.x;
        float dy = pos.y - mCenter.y;
        float dist = Mathf.Sqrt(dx * dx + dy * dy);
        if (dist < (mRadius + distance)) return true;
        return false;
    }

    public bool InSphere(ref Vector3 pos, float distance)
    {
        float dx = pos.x - mCenter.x;
        float dy = pos.y - mCenter.y;
        float dz = pos.z - mCenter.z;

        float dist = Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
        if (dist < (mRadius + distance)) return true;
        return false;
    }

}
