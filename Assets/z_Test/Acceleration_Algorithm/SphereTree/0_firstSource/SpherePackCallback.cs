using UnityEngine;

// Virtual base class, used to implement callbacks for RayTracing,
// range testing, and frustum culling.
public class SpherePackCallback
{
    //f : frustum clipped against 
    //sphere : leaf node sphere in question
    //state : new state it is in.
    //public virtual void VisibilityCallback(const Frustum &f, SpherePack* sphere, DefineO.ViewState state) {}
    public virtual void VisibilityCallback(Frustum f, SpherePack sphere, Frustum.ViewState state) {}

    //p1 : source pos of ray
    //dir : direction of ray
    //distance : distance of ray
    //sect : intersection location
    //sphere : sphere ray hit
    //public virtual void RayTraceCallback(const Vector3d<float> &p1, const Vector3d<float> &dir, float distance, const Vector3d<float> &sect, SpherePack* sphere) {}
    public virtual void RayTraceCallback(ref Vector3 p1, ref Vector3 dir, float distance, ref Vector3 sect, SpherePack sphere) {}

    //searchpos : position we are performing range test against.
    //distance : squared distance we are range searching against.
    //state : sphere within range, VS_PARTIAL if sphere straddles range test
    //public virtual void RangeTestCallback(const Vector3d<float> &searchpos, float distance, SpherePack* sphere, ViewState state) {}
    public virtual void RangeTestCallback(ref Vector3 searchpos, float distance, SpherePack sphere, Frustum.ViewState state) {}

}