using UnityEngine;
namespace SpiderVR
{
    public interface IHeadTracker
    {
        Quaternion Rotation { get; }
        void Recenter();
        void Begin();
        void End();
    }
}
