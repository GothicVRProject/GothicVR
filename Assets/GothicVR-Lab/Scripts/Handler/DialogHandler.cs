using GVR.Caches;
using GVR.Creator.Meshes;
using GVR.Extensions;
using GVR.Phoenix.Interface;
using GVR.Phoenix.Interface.Vm;
using PxCs.Interface;
using UnityEngine;
using UnityEngine.Serialization;

namespace GVR.Lab.Handler
{
    public class DialogHandler : MonoBehaviour
    {
        public GameObject bloodwynSlotGo;
        public BloodwynInstanceId bloodwynInstanceInstanceId;

        public enum BloodwynInstanceId
        {
            Deu = 6596
        }


        private void Start()
        {
            var newNpc = PrefabCache.TryGetObject(PrefabCache.PrefabType.Npc);
            newNpc.name = "Bloodwyn";
            newNpc.SetParent(bloodwynSlotGo);

            var pxNpc = PxVm.InitializeNpc(GameData.VmGothicPtr, (uint)bloodwynInstanceInstanceId);

            var mdmName = "Hum_GRDM_ARMOR.asc";
            var mdhName = "Humans_Militia.mds";
            var body = new VmGothicExternals.ExtSetVisualBodyData()
            {
                Armor = 3643,
                Body = "hum_body_Naked0",
                BodyTexColor = 1,
                BodyTexNr = 0,
                Head = "Hum_Head_Bald",
                HeadTexNr = 18,
                TeethTexNr = 1
            };

            NpcMeshCreator.CreateNpc(newNpc.name, mdmName, mdhName, body, newNpc);
        }
    }
}