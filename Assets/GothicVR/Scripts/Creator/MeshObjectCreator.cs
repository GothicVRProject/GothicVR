using GVR.Creator.Meshes;
using GVR.Phoenix.Interface.Vm;
using UnityEngine;
using ZenKit;
using ZenKit.Daedalus;
using ZenKit.Vobs;

namespace GVR.Creator
{
    /// <summary>
    /// We leverage Builder pattern to ensure:
    ///   1. A common interface for Mesh creations
    ///   2. Instance handling of the Builder itself. (Using static instances to have function override capabilities)
    /// 
    /// @see Builder Pattern reference: https://refactoring.guru/design-patterns/builder
    /// </summary>
    public static class MeshObjectCreator
    {
        private static readonly NpcMeshCreator NpcMeshCreator = new();
        private static readonly VobMeshCreator VobMeshCreator = new();

        public static GameObject CreateNpc(string npcName, string mdmName, string mdhName,
            VmGothicExternals.ExtSetVisualBodyData bodyData, GameObject root)
        {
            return NpcMeshCreator.CreateNpc(npcName, mdmName, mdhName, bodyData, root);
        }

        public static void EquipNpcWeapon(GameObject npcGo, ItemInstance itemData, VmGothicEnums.ItemFlags mainFlag,
            VmGothicEnums.ItemFlags flags)
        {
            NpcMeshCreator.CreateNpcWeapon(npcGo, itemData, mainFlag, flags);
        }

        public static GameObject CreateVob(string objectName, IMultiResolutionMesh mrm, Vector3 position,
            Quaternion rotation, bool withCollider, GameObject parent = null, GameObject rootGo = null)
        {
            return VobMeshCreator.CreateVob(objectName, mrm, position, rotation, withCollider, parent, rootGo);
        }

        public static GameObject CreateVob(string objectName, IModel mdl, Vector3 position, Quaternion rotation,
            GameObject parent = null, GameObject rootGo = null)
        {
            return VobMeshCreator.CreateVob(objectName, mdl, position, rotation, parent, rootGo);
        }

        public static GameObject CreateVob(string objectName, IModelMesh mdm, IModelHierarchy mdh,
            Vector3 position, Quaternion rotation, GameObject parent = null, GameObject rootGo = null)
        {
            return VobMeshCreator.CreateVob(objectName, mdm, mdh, position, rotation, parent, rootGo);
        }

        public static GameObject CreateVobDecal(IVirtualObject vob, VisualDecal decal, GameObject parent)
        {
            return VobMeshCreator.CreateVobDecal(vob, decal, parent);
        }
    }
}
