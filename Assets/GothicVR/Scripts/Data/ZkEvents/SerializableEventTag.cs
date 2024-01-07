using System;
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
        public Tuple<string, string> Slots;
        public string Item;
        public List<int> Frames;
        public FightMode FightMode;
        public bool Attached;

        public SerializableEventTag(IEventTag zkEventTag)
        {
            Frame = zkEventTag.Frame;
            Type = zkEventTag.Type;
            Slots = zkEventTag.Slots;
            Item = zkEventTag.Item;
            Frames = zkEventTag.Frames;
            FightMode = zkEventTag.FightMode;
            Attached = zkEventTag.Attached;
        }
    }
}
