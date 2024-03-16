using System.Linq;
using GVR.Extensions;
using GVR.Globals;
using Unity.VisualScripting;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace GVR.Creator.Meshes.V2
{
    public class VobMeshBuilder : AbstractMeshBuilder
    {
        public override GameObject Build()
        {
            if (Mrm != null)
            {
                BuildViaMrm();
            }
            else if (Mdm != null && Mdh != null)
            {
                if (HasMdmTextures())
                {
                    BuildViaMdmAndMdh();
                }
            }
            else
            {
                Debug.LogError($"No suitable data for Vob to be created found >{RootGo.name}<");
                return null;
            }

            return RootGo;
        }

        /// <summary>
        /// Add ZenGineSlot collider. i.e. positions where an NPC can sit on a bench.
        /// </summary>
        private void AddZsCollider()
        {
            if (!HasMeshCollider || RootGo == null || RootGo.transform.childCount == 0)
            {
                return;
            }

            var zm = RootGo.transform.GetChild(0);
            for (var i = 0; i < zm.childCount; i++)
            {
                var child = zm.GetChild(i);
                if (!child.name.StartsWithIgnoreCase("ZS"))
                    continue;

                // ZS need to be "invisible" for the Raycast teleporter.
                child.gameObject.layer = Constants.IgnoreRaycastLayer;

                // Used for event triggers with NPCs.
                var coll = child.AddComponent<SphereCollider>();
                coll.isTrigger = true;
            }
        }

        /// <summary>
        /// Check if there are completely empty elements without any texture.
        /// G1: e.g. Harp, Flute, and WASH_SLOT (usage moved to a FreePoint within daedalus functions)
        /// </summary>
        private bool HasMdmTextures()
        {
            var noMeshTextures = Mdm.Meshes.All(mesh => mesh.Mesh.SubMeshes.All(subMesh => subMesh.Material.Texture.IsEmpty()));
            var noAttachmentTextures = Mdm.Attachments.All(att => att.Value.Materials.All(mat => mat.Texture.IsEmpty()));

            return !noMeshTextures || !noAttachmentTextures;

        }
    }
}
