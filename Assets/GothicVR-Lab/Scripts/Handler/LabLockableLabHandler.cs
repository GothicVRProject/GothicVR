using GVR.Caches;
using GVR.Creator;
using GVR.Creator.Meshes;
using GVR.Creator.Meshes.V2;
using UnityEngine;

namespace GVR.Lab.Handler
{
    public class LabLockableLabHandler : MonoBehaviour, ILabHandler
    {
        public GameObject chestsGo;
        public GameObject doorsGo;

        public void Bootstrap()
        {
            var chestName = "CHESTBIG_OCCHESTLARGELOCKED.MDS";
            var mdh = AssetCache.TryGetMdh(chestName);
            var mdm = AssetCache.TryGetMdm(chestName);

            MeshFactory.CreateVob(chestName, mdm, mdh, Vector3.zero, Quaternion.identity, chestsGo, useTextureArray: false);


            var doorName = "DOOR_WOODEN";
            var mdlDoor = AssetCache.TryGetMdl(doorName);

            MeshFactory.CreateVob(doorName, mdlDoor, Vector3.zero, Quaternion.identity, doorsGo, useTextureArray: false);
        }
    }
}
