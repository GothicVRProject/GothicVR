using System.Collections.Generic;
using ZenKit;

namespace GVR.Data.ZkEvents
{
    /// <summary>
    /// UnityEngine.JsonUtility doesn't serialize CachedEventTag. We therefore use this class to JSON-ify the data.
    /// </summary>
    public class SerializableEventTag
    {
        public int Frame;
        public EventType Type;
        public string Slot1;
        public string Slot2;
        public string Item;
        public List<int> Frames;
        public FightMode FightMode;
        public bool Attached;

        public SerializableEventTag()
        { }
        
        public SerializableEventTag(IEventTag zkEventTag)
        {
            Frame = zkEventTag.Frame;
            Type = zkEventTag.Type;
            Slot1 = zkEventTag.Slots.Item1; // Tuples aren't serialized by JsonUtility. Therefore separating its data now.
            Slot2 = zkEventTag.Slots.Item2;
            Item = zkEventTag.Item;
            Frames = zkEventTag.Frames;
            FightMode = zkEventTag.FightMode;
            Attached = zkEventTag.Attached;
        }
    }
}
