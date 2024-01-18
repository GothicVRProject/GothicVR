using GVR.Caches;
using GVR.Creator;
using GVR.Creator.Meshes;
using UnityEngine;

namespace GVR.Lab.Handler
{
    public class LockableHandler : MonoBehaviour, IHandler
    {
        public GameObject chestsGo;
        public GameObject doorsGo;

        public void Bootstrap()
        {
            var chestName = "CHESTBIG_OCCHESTLARGELOCKED.MDS";
            var mdh = AssetCache.TryGetMdh(chestName);
            var mdm = AssetCache.TryGetMdm(chestName);

            MeshCreatorFacade.CreateVob(chestName, mdm, mdh, Vector3.zero, Quaternion.identity, chestsGo);


            var doorName = "DOOR_WOODEN";
            var mdlDoor = AssetCache.TryGetMdl(doorName);

            MeshCreatorFacade.CreateVob(doorName, mdlDoor, Vector3.zero, Quaternion.identity, doorsGo);
        }
    }
}
