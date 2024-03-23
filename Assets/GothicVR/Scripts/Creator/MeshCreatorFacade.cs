using GVR.Creator.Meshes;
using GVR.Creator.Meshes.V2;
using GVR.Vm;
using UnityEngine;
using ZenKit.Daedalus;

namespace GVR.Creator
{
    /// <summary>
    /// We leverage Facade pattern to ensure:
    ///   1. A common interface for Mesh creations
    ///   2. Instance handling of the Builder itself. (Using static instances to have function override capabilities)
    /// 
    /// @see Builder Pattern reference: https://refactoring.guru/design-patterns/facade
    /// </summary>
    public static class MeshCreatorFacade
    {
        private static readonly NpcMeshCreator NpcMeshCreator = new();

        public static void EquipNpcWeapon(GameObject npcGo, ItemInstance itemData, VmGothicEnums.ItemFlags mainFlag,
            VmGothicEnums.ItemFlags flags)
        {
            NpcMeshCreator.CreateNpcWeapon(npcGo, itemData, mainFlag, flags);
        }
    }
}
