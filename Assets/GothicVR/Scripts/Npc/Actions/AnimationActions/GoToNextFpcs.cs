using GVR.Manager;
using GVR.Vob.WayNet;
using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public class GoToNextFp : AbstractWalkAnimationAction
    {
        private FreePoint fp;
        
        public GoToNextFp(Ai.Action action, GameObject npcGo) : base(action, npcGo)
        { }
        
        public override void Start()
        {
            var pos = npcGo.transform.position;

            fp = WayNetManager.I.FindNearestFreePoint(pos, action.str0);

            movingLocation = fp.Position;
        }
    }
}