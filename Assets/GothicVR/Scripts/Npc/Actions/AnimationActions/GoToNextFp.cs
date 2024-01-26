using System.Linq;
using GVR.Manager;
using GVR.Vob.WayNet;
using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public class GoToNextFp : GoToFp
    {
        public GoToNextFp(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        { }
    }
}
