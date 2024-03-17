using System.Linq;
using GVR.Extensions;
using GVR.Vm;
using Unity.VisualScripting;
using UnityEngine;
using ZenKit;
using ZenKit.Daedalus;
using ZenKit.Vobs;

namespace GVR.Creator.Meshes.V2
{
    public static class MeshFactory
    {
        public static GameObject CreateNpc(string npcName, string mdmName, string mdhName,
            VmGothicExternals.ExtSetVisualBodyData bodyData, GameObject root)
        {
            var npcBuilder = new NpcMeshBuilder();
            npcBuilder.SetGameObject(root, npcName);
            npcBuilder.SetMdh(mdhName);
            npcBuilder.SetMdm(mdmName);
            npcBuilder.SetBodyData(bodyData);

            var npcGo = npcBuilder.Build();

            var npcHeadBuilder = new NpcHeadMeshBuilder();
            npcHeadBuilder.SetGameObject(npcGo);
            npcHeadBuilder.SetBodyData(bodyData);

            // returns body+head
            return npcHeadBuilder.Build();
        }

        public static GameObject CreateNpcWeapon(GameObject npcGo, ItemInstance itemData, VmGothicEnums.ItemFlags mainFlag, VmGothicEnums.ItemFlags flags)
        {
            var npcWeaponBuilder = new NpcWeaponMeshBuilder();
            npcWeaponBuilder.SetWeaponData(npcGo, itemData, mainFlag, flags);

            switch (mainFlag)
            {
                case VmGothicEnums.ItemFlags.ITEM_KAT_NF:
                    npcWeaponBuilder.SetMrm(itemData.Visual);
                    break;
                case VmGothicEnums.ItemFlags.ITEM_KAT_FF:
                    npcWeaponBuilder.SetMmb(itemData.Visual);
                    break;
                default:
                    // NOP - e.g. for armor.
                    return null;
            }

            return npcWeaponBuilder.Build();
        }

        public static GameObject CreateVob(string objectName, IMultiResolutionMesh mrm, Vector3 position,
            Quaternion rotation, bool withCollider, GameObject parent = null, GameObject rootGo = null)
        {
            var vobBuilder = new VobMeshBuilder();
            vobBuilder.SetRootPosAndRot(position, rotation);
            vobBuilder.SetGameObject(rootGo, objectName);
            vobBuilder.SetParent(parent);
            vobBuilder.SetMrm(mrm);

            if (!withCollider)
            {
                vobBuilder.DisableMeshCollider();
            }

            return vobBuilder.Build();
        }

        public static GameObject CreateVob(string objectName, IModel mdl, Vector3 position, Quaternion rotation,
            GameObject parent = null, GameObject rootGo = null)
        {
            var vobBuilder = new VobMeshBuilder();
            vobBuilder.SetRootPosAndRot(position, rotation);
            vobBuilder.SetGameObject(rootGo, objectName);
            vobBuilder.SetParent(parent, resetRotation: true); // If we don't reset these, all objects will be rotated wrong!
            vobBuilder.SetMdl(mdl);

            return vobBuilder.Build();
        }

        public static GameObject CreateVob(string objectName, IModelMesh mdm, IModelHierarchy mdh,
            Vector3 position, Quaternion rotation, GameObject parent = null, GameObject rootGo = null)
        {
            // Check if there are completely empty elements without any texture.
            // G1: e.g. Harp, Flute, and WASH_SLOT (usage moved to a FreePoint within daedalus functions)
            var noMeshTextures = mdm.Meshes.All(mesh => mesh.Mesh.SubMeshes.All(subMesh => subMesh.Material.Texture.IsEmpty()));
            var noAttachmentTextures = mdm.Attachments.All(att => att.Value.Materials.All(mat => mat.Texture.IsEmpty()));

            if (noMeshTextures && noAttachmentTextures)
            {
                return null;
            }

            var vobBuilder = new VobMeshBuilder();
            vobBuilder.SetRootPosAndRot(position, rotation);
            vobBuilder.SetGameObject(rootGo, objectName);
            vobBuilder.SetParent(parent);
            vobBuilder.SetMdh(mdh);
            vobBuilder.SetMdm(mdm);

            return vobBuilder.Build();
        }

        public static GameObject CreateVobDecal(IVirtualObject vob, VisualDecal decal, GameObject parent)
        {
            var vobDecalBuilder = new VobDecalMeshBuilder();
            vobDecalBuilder.SetGameObject(null, vob.Name);
            vobDecalBuilder.SetParent(parent);
            vobDecalBuilder.SetRootPosAndRot(vob.Position.ToUnityVector(), vob.Rotation.ToUnityQuaternion());
            vobDecalBuilder.SetDecalData(vob, decal);

            return vobDecalBuilder.Build();
        }

        public static GameObject CreateBarrier(string objectName, IMesh mesh)
        {
            var barrierBuilder = new BarrierMeshBuilder();
            barrierBuilder.SetGameObject(null, objectName);
            barrierBuilder.SetBarrierMesh(mesh);

            return barrierBuilder.Build();
        }

        public static GameObject CreatePolyStrip(GameObject go, int numberOfSegments, Vector3 startPoint, Vector3 endPoint)
        {
            var polyStripBuilder = new PolyStripMeshBuilder();
            polyStripBuilder.SetGameObject(go);
            polyStripBuilder.SetPolyStripData(numberOfSegments, startPoint, endPoint);

            return polyStripBuilder.Build();
        }
    }
}
