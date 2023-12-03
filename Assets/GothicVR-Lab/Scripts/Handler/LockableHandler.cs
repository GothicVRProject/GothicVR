using GVR.Caches;
using GVR.Creator;
using GVR.Creator.Meshes;
using UnityEngine;

namespace GVR.Lab.Handler
{
    public class LockableHandler : MonoBehaviour
    {
        public GameObject chestsGo;
        public GameObject doorsGo;

        private void Start()
        {
            var chestName = "CHESTBIG_OCCHESTLARGELOCKED.MDS";
            var mdh = AssetCache.TryGetMdh(chestName);
            var mdm = AssetCache.TryGetMdm(chestName);

            MeshObjectCreator.CreateVob(chestName, mdm, mdh, Vector3.zero, Quaternion.identity, chestsGo);


            var doorName = "DOOR_WOODEN";
            var mdlDoor = AssetCache.TryGetMdl(doorName);

            MeshObjectCreator.CreateVob(doorName, mdlDoor, Vector3.zero, Quaternion.identity, doorsGo);
        }
    }
}
