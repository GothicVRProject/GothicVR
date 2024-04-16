using ZenKit;

namespace GVR.Data.ZkEvents
{
    /// <summary>
    /// UnityEngine.JsonUtility doesn't serialize CachedEventTag. We therefore use this class to JSON-ify the data.
    /// </summary>
    public class SerializableEventMorphAnimation
    {
        public int Frame;
        public string Animation;
        public string Node;

        public SerializableEventMorphAnimation(IEventMorphAnimation morphAnimation)
        {
            Frame = morphAnimation.Frame;
            Animation = morphAnimation.Animation;
            Node = morphAnimation.Node;
        }
    }
}
