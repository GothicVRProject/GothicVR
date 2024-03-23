using GVR.Extensions;
using GVR.Vm;
using UnityEngine;
using ZenKit.Daedalus;

namespace GVR.Creator.Meshes.V2
{
    public class NpcWeaponMeshBuilder : AbstractMeshBuilder
    {
        private GameObject npcGo;
        private ItemInstance itemData;
        private VmGothicEnums.ItemFlags mainFlag;
        private VmGothicEnums.ItemFlags flags;

        public void SetWeaponData(GameObject npcGo, ItemInstance itemData, VmGothicEnums.ItemFlags mainFlag, VmGothicEnums.ItemFlags flags)
        {
            this.npcGo = npcGo;
            this.itemData = itemData;
            this.mainFlag = mainFlag;
            this.flags = flags;
        }

        public override GameObject Build()
        {
            switch (mainFlag)
            {
                case VmGothicEnums.ItemFlags.ITEM_KAT_NF:
                    return EquipMeleeWeapon();
                case VmGothicEnums.ItemFlags.ITEM_KAT_FF:
                    return EquipRangeWeapon();
                default:
                        Debug.LogError($"WeaponType {mainFlag} isn't handled yet.");
                    return null;
            }
        }

        private GameObject EquipMeleeWeapon()
        {
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
                return null;

            // Bugfix: e.g. there's a Buddler who has a NailMace and Club equipped at the same time.
            // Therefore we need to check if the Components are already there.
            if (!weaponGo.TryGetComponent<MeshFilter>(out var meshFilter))
                meshFilter = weaponGo.AddComponent<MeshFilter>();
            if (!weaponGo.TryGetComponent<MeshRenderer>(out var meshRenderer))
                meshRenderer = weaponGo.AddComponent<MeshRenderer>();

            PrepareMeshFilter(meshFilter, Mrm, meshRenderer);
            PrepareMeshRenderer(meshRenderer, Mrm);

            return weaponGo;
        }

        private GameObject EquipRangeWeapon()
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
                return null;

            var meshFilter = weaponGo.AddComponent<MeshFilter>();
            var meshRenderer = weaponGo.AddComponent<MeshRenderer>();

            // FIXME - We don't handle bow morphs as of now. Neet to do once fighting is implemented.
            PrepareMeshFilter(meshFilter, Mmb.Mesh, meshRenderer);
            PrepareMeshRenderer(meshRenderer, Mmb.Mesh);


            return weaponGo;
        }
    }
}
