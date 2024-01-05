using System.Collections.Generic;
using System.Text.RegularExpressions;
using GVR.Caches;
using GVR.Extensions;
using GVR.Vm;
using UnityEngine;
using ZenKit;
using ZenKit.Daedalus;

namespace GVR.Creator.Meshes
{
    public class NpcMeshCreator : AbstractMeshCreator
    {
        private VmGothicExternals.ExtSetVisualBodyData tempBodyData;

        public GameObject CreateNpc(string npcName, string mdmName, string mdhName,
            VmGothicExternals.ExtSetVisualBodyData bodyData, GameObject root)
        {
            tempBodyData = bodyData;
            var mdm = AssetCache.TryGetMdm(mdmName);
            var mdh = AssetCache.TryGetMdh(mdhName);
            
            if (mdm == null)
            {
                Debug.LogError($"MDH from name >{mdmName}< for object >{root.name}< not found.");
                return null;
            }
            
            if (mdh == null)
            {
                Debug.LogError($"MDH from name >{mdhName}< for object >{root.name}< not found.");
                return null;
            }
            
            var npcGo = Create(npcName, mdm, mdh, default, default, null, root);

            if (!string.IsNullOrEmpty(bodyData.Head))
            {
                var mmb = AssetCache.TryGetMmb(bodyData.Head);
                AddHead(npcName, npcGo, mmb);
            }

            return npcGo;
        }

        private void AddHead(string npcName, GameObject npcGo, IMorphMesh morphMesh)
        {
            var headGo = npcGo.FindChildRecursively("BIP01 HEAD");

            if (headGo == null)
            {
                Debug.LogWarning($"No NPC head found for {npcName}");
                return;
            }

            var headMeshFilter = headGo.AddComponent<MeshFilter>();
            var headMeshRenderer = headGo.AddComponent<MeshRenderer>();

            PrepareMeshRenderer(headMeshRenderer, morphMesh.Mesh);
            PrepareMeshFilter(headMeshFilter, morphMesh.Mesh, true);
        }

        /// <summary>
        /// Change texture name based on VisualBodyData.
        /// </summary>
        protected override Texture2D GetTexture(string name)
        {
            string finalTextureName;
            
            // FIXME - We don't have different mouths in Gothic1. Need to recheck it in Gothic2.
            if (name.ToUpper().EndsWith("MOUTH_V0.TGA"))
                finalTextureName = name;
            else if (name.ToUpper().EndsWith("TEETH_V0.TGA"))
                // e.g. Some_Texture_V0.TGA --> Some_Texture_V1.TGA
                finalTextureName = Regex.Replace(name, "(?<=.*?)V0", $"V{tempBodyData.TeethTexNr}");
            else if (name.ToUpper().Contains("BODY") && name.ToUpper().EndsWith("V0_C0.TGA"))
                // This regex replaces the suffix of V0_C0 with values of corresponding data.
                // e.g. Some_Texture_V0_C0.TGA --> Some_Texture_V1_C2.TGA
                finalTextureName = Regex.Replace(name, "(?<=.*?)V0_C0",
                    $"V{tempBodyData.BodyTexNr}_C{tempBodyData.BodyTexColor}");
            else if (name.ToUpper().Contains("HEAD") && name.ToUpper().EndsWith("V0_C0.TGA"))
                finalTextureName = Regex.Replace(name, "(?<=.*?)V0_C0",
                    $"V{tempBodyData.HeadTexNr}_C{tempBodyData.BodyTexColor}");
            else
                // No changeable texture needed? Skip updating texture name.
                finalTextureName = name;

            return base.GetTexture(finalTextureName);
        }
        
        protected override Dictionary<string, IMultiResolutionMesh> GetFilteredAttachments(Dictionary<string, IMultiResolutionMesh> attachments)
        {
            Dictionary<string, IMultiResolutionMesh> newAttachments = new(attachments);

            // Remove head as it will be loaded later.
            if (newAttachments.Remove("BIP01 HEAD"))
                Debug.Log("Removed default >BIP01 HEAD< attachment mesh from NPC.");
            
            return newAttachments;
        }

        public void CreateNpcWeapon(GameObject npcGo, ItemInstance itemData, VmGothicEnums.ItemFlags mainFlag, VmGothicEnums.ItemFlags flags)
        {
            switch (mainFlag)
            {
                case VmGothicEnums.ItemFlags.ITEM_KAT_NF:
                    EquipMeleeWeapon(npcGo, itemData);
                    return;
                case VmGothicEnums.ItemFlags.ITEM_KAT_FF:
                    EquipRangeWeapon(npcGo, itemData);
                    return;
            }
        }

        private void EquipMeleeWeapon(GameObject npcGo, ItemInstance itemData)
        {
            var mrm = AssetCache.TryGetMrm(itemData.Visual);

            string slotName;
            switch ((VmGothicEnums.ItemFlags)itemData.Flags)
            {
                case VmGothicEnums.ItemFlags.ITEM_2HD_AXE:
                case VmGothicEnums.ItemFlags.ITEM_2HD_SWD:
                    slotName = "ZS_LONGSWORD";
                    break;
                default:
                    slotName =  "ZS_SWORD";
                    break;
            }

            var weaponGo = npcGo.FindChildRecursively(slotName);
            if (weaponGo == null)
                return;

            // Bugfix: e.g. there's a Buddler who has a NailMace and Club equipped at the same time.
            // Therefore we need to check if the Components are already there.
            if (!weaponGo.TryGetComponent<MeshFilter>(out var meshFilter))
                meshFilter = weaponGo.AddComponent<MeshFilter>();
            if (!weaponGo.TryGetComponent<MeshRenderer>(out var meshRenderer))
                meshRenderer = weaponGo.AddComponent<MeshRenderer>();

            PrepareMeshRenderer(meshRenderer, mrm);
            PrepareMeshFilter(meshFilter, mrm, false);
        }

        private void EquipRangeWeapon(GameObject npcGo, ItemInstance itemData)
        {
            string slotName;
            switch ((VmGothicEnums.ItemFlags)itemData.Flags)
            {
                case VmGothicEnums.ItemFlags.ITEM_CROSSBOW:
                    slotName = "ZS_CROSSBOW";
                    break;
                default:
                    slotName =  "ZS_BOW";
                    break;
            }
            
            var weaponGo = npcGo.FindChildRecursively(slotName);
            if (weaponGo == null)
                return;

            var mms = AssetCache.TryGetMmb(itemData.Visual);

            var meshFilter = weaponGo.AddComponent<MeshFilter>();
            var meshRenderer = weaponGo.AddComponent<MeshRenderer>();

            PrepareMeshRenderer(meshRenderer, mms.Mesh);
            PrepareMeshFilter(meshFilter, mms.Mesh, true);
        }
    }
}
