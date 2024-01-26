using ZenKit;

namespace GVR.Data.ZkEvents
{
    /// <summary>
    /// UnityEngine.JsonUtility doesn't serialize CachedEventTag. We therefore use this class to JSON-ify the data.
    /// </summary>
    public class SerializableEventSoundEffect
    {
        public SerializableEventSoundEffect()
        { }

        public SerializableEventSoundEffect(IEventSoundEffect zkEventSoundEffect)
        {
            Frame = zkEventSoundEffect.Frame;
            Name = zkEventSoundEffect.Name;
            Range = zkEventSoundEffect.Range;
            EmptySlot = zkEventSoundEffect.EmptySlot;
        }

        public int Frame;
        public string Name;
        public float Range;
        public bool EmptySlot;
    }
}
