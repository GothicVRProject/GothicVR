using GVR.Caches;
using GVR.Creator;
using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public class PlayAni : AnimationAction
    {
        public PlayAni(Ai.Action action, GameObject npcGo) : base(action, npcGo)
        { }

        public override void Start()
        {
            var mdh = AssetCache.I.TryGetMdh(props.overlayMdhName);
            // FIXME - We need to handle both mds and mdh options! (base vs overlay)
            AnimationCreator.I.PlayAnimation(props.baseMdsName, action.str0, mdh, npcGo);
        }
    }
}