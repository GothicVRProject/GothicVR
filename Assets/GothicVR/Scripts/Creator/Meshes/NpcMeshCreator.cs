using System.Text.RegularExpressions;
using GVR.Caches;
using GVR.Phoenix.Interface.Vm;
using GVR.Phoenix.Util;
using PxCs.Data.Mesh;
using PxCs.Data.Model;
using PxCs.Data.Vm;
using PxCs.Interface;
using UnityEngine;

namespace GVR.Creator.Meshes
{
    public class NpcMeshCreator : AbstractMeshCreator<NpcMeshCreator>
    {
        private int tmpBodyTexNr;
        private int tmpBodyTexColor;

        private VmGothicBridge.Mdl_SetVisualBodyData tempBodyData;

        public GameObject CreateNpc(string npcName, PxModelMeshData mdm, PxModelHierarchyData mdh,
            PxMorphMeshData morphMesh, VmGothicBridge.Mdl_SetVisualBodyData bodyData, GameObject root)
        {
            tmpBodyTexNr = bodyData.bodyTexNr;
            tmpBodyTexColor = bodyData.bodyTexColor;

            var npcGo = Create(npcName, mdm, mdh, default, default, null, root);

            AddHead(npcName, npcGo, morphMesh);

            return npcGo;
        }

        private void AddHead(string npcName, GameObject npcGo, PxMorphMeshData morphMesh)
        {
            var headGo = npcGo.FindChildRecursively("BIP01 HEAD");

            if (headGo == null)
            {
                Debug.LogWarning($"No NPC head found for {npcName}");
                return;
            }

            var headMeshFilter = headGo.AddComponent<MeshFilter>();
            var headMeshRenderer = headGo.AddComponent<MeshRenderer>();

            PrepareMeshRenderer(headMeshRenderer, morphMesh.mesh);
            PrepareMeshFilter(headMeshFilter, morphMesh.mesh);
        }

        /// <summary>
        /// Change texture name based on VisualBodyData.
        /// </summary>
        protected override Texture2D GetTexture(string name)
        {
            // FIXME: Dirty hack. Needs to be optimized.
            if (name.ToUpper().Contains("MOUTH") || name.ToUpper().Contains("TEETH"))
                return base.GetTexture(name);

            if (!name.ToUpper().EndsWith("V0_C0.TGA"))
            {
                // Debug.LogError($"The format of body texture isn't right for ${name}");
                return base.GetTexture(name);
            }

            // This regex replaces the suffix of V0_C0 with values of corresponding data.
            // e.g. Some_Texture_V0_C0.TGA --> Some_Texture_V1_C2.TGA
            var formattedTextureName = Regex.Replace(name, "(?<=.*?)V0_C0", $"V{tmpBodyTexNr}_C{tmpBodyTexColor}");

            return AssetCache.I.TryGetTexture(formattedTextureName);
        }


        public void EquipWeapon(GameObject npcGo, PxVmItemData itemData, PxVm.PxVmItemFlags mainFlag, PxVm.PxVmItemFlags flags)
        {
            switch (mainFlag)
            {
                case PxVm.PxVmItemFlags.ITEM_KAT_NF:
                    EquipMeleeWeapon(npcGo, itemData);
                    return;
                case PxVm.PxVmItemFlags.ITEM_KAT_FF:
                    EquipRangeWeapon(npcGo, itemData);
                    return;
            }
        }

        private void EquipMeleeWeapon(GameObject npcGo, PxVmItemData itemData)
        {
            var mrm = AssetCache.I.TryGetMrm(itemData.visual);

            string slotName;
            switch (itemData.flags)
            {
                case PxVm.PxVmItemFlags.ITEM_2HD_AXE:
                case PxVm.PxVmItemFlags.ITEM_2HD_SWD:
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
            PrepareMeshFilter(meshFilter, mrm);
        }

        private void EquipRangeWeapon(GameObject npcGo, PxVmItemData itemData)
        {
            string slotName;
            switch (itemData.flags)
            {
                case PxVm.PxVmItemFlags.ITEM_CROSSBOW:
                    slotName = "ZS_CROSSBOW";
                    break;
                default:
                    slotName =  "ZS_BOW";
                    break;
            }
            
            var weaponGo = npcGo.FindChildRecursively(slotName);
            if (weaponGo == null)
                return;

            var mms = AssetCache.I.TryGetMmb(itemData.visual);

            var meshFilter = weaponGo.AddComponent<MeshFilter>();
            var meshRenderer = weaponGo.AddComponent<MeshRenderer>();

            PrepareMeshRenderer(meshRenderer, mms.mesh);
            PrepareMeshFilter(meshFilter, mms.mesh);
        }
    }
}