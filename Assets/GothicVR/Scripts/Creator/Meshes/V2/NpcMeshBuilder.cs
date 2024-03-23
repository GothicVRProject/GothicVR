using System.Collections.Generic;
using GVR.Caches;
using GVR.Vm;
using UnityEngine;
using ZenKit;

namespace GVR.Creator.Meshes.V2
{
    public class NpcMeshBuilder : AbstractMeshBuilder
    {
        protected VmGothicExternals.ExtSetVisualBodyData bodyData;

        public virtual void SetBodyData(VmGothicExternals.ExtSetVisualBodyData body)
        {
            bodyData = body;
        }

        public override GameObject Build()
        {
            return BuildViaMdmAndMdh();
        }

        protected override Dictionary<string, IMultiResolutionMesh> GetFilteredAttachments(Dictionary<string, IMultiResolutionMesh> attachments)
        {
            Dictionary<string, IMultiResolutionMesh> newAttachments = new(attachments);

            // Remove head as it will be loaded later.
            if (newAttachments.Remove("BIP01 HEAD"))
            {
                Debug.Log("Removed default >BIP01 HEAD< attachment mesh from NPC.");
            }

            return newAttachments;
        }

        /// <summary>
        /// Positions in mdm files for NPC armor isn't what it seems to be. We need to calculate the real data from weights.
        /// Please check the Cache class for more details.
        /// </summary>
        protected override List<System.Numerics.Vector3> GetSoftSkinMeshPositions(ISoftSkinMesh softSkinMesh)
        {
            return NpcArmorPositionCache.TryGetPositions(softSkinMesh, Mdh);
        }
    }
}
