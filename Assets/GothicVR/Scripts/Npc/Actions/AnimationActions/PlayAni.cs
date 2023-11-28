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
            AnimationStartPos = NpcGo.transform.localPosition;
            var mdh = AssetCache.TryGetMdh(Props.overlayMdhName);

            // FIXME - We need to handle both mds and mdh options! (base vs overlay)
            AnimationData = AnimationCreator.PlayAnimation(Props.baseMdsName, Action.String0, mdh, NpcGo);
        }

        public override void Tick(Transform transform)
        {
            base.Tick(transform);

            HandleRootMotion(transform);
        }
    }
}
