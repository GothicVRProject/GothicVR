using GVR.Caches;
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
            // Load all the chests from G1. ;-)
            var offset = 0f;
            foreach (var chestName in new []{"CHESTBIG_OCCHESTLARGE", "CHESTBIG_OCCHESTMEDIUM", "CHESTBIG_OCCRATELARGE", "CHESTSMALL_OCCHESTSMALL", "CHESTSMALL_OCCRATESMALL"})
            {
                foreach (var suffix in new[]{"", "LOCKED"})
                {
                    var fullName = chestName + suffix;
                    var mdh = AssetCache.TryGetMdh(fullName);
                    var mdm = AssetCache.TryGetMdm(fullName);

                    var obj = PrefabCache.TryGetObject(PrefabCache.PrefabType.VobContainer);
                    MeshFactory.CreateVob(chestName, mdm, mdh, Vector3.zero, Quaternion.identity, rootGo: obj, parent: chestsGo, useTextureArray: false);

                    obj.transform.localPosition += new Vector3(0, 0, offset);
                    offset -= 1.5f;
                }
            }



            var doorName = "DOOR_WOODEN";
            var mdlDoor = AssetCache.TryGetMdl(doorName);

            MeshFactory.CreateVob(doorName, mdlDoor, Vector3.zero, Quaternion.identity, doorsGo, useTextureArray: false);
        }
    }
}
