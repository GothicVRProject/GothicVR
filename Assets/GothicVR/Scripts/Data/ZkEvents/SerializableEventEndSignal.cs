using JetBrains.Annotations;

namespace GVR.Data.ZkEvents
{
    /// <summary>
    /// UnityEngine.JsonUtility doesn't serialize CachedEventTag. We therefore use this class to JSON-ify the data.
    /// </summary>
    public class SerializableEventEndSignal
    {
        public string NextAnimation;

        public SerializableEventEndSignal([NotNull] string nextAnimation)
        {
            NextAnimation = nextAnimation;
        }
    }
}
