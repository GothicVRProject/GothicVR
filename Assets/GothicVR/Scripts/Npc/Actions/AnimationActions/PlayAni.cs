using GVR.Caches;
using GVR.Creator;
using GVR.Creator.Meshes;
using UnityEngine;

namespace GVR.Npc.Actions.AnimationActions
{
    public class PlayAni : AbstractAnimationAction
    {
        public PlayAni(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        { }

        public override void Start()
        {
            var mdh = AssetCache.TryGetMdh(props.overlayMdhName);
            // FIXME - We need to handle both mds and mdh options! (base vs overlay)
            AnimationCreator.PlayAnimation(props.baseMdsName, action.String0, mdh, npcGo);
        }
    }
}
