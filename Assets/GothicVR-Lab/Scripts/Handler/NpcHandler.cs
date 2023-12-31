using System.Linq;
using GVR.Caches;
using GVR.Creator;
using GVR.Extensions;
using GVR.Globals;
using GVR.Phoenix.Interface.Vm;
using GVR.Properties;
using UnityEngine;
using ZenKit.Daedalus;

namespace GVR.Lab.Handler
{
    public class NpcHandler : MonoBehaviour, IHandler
    {
        public GameObject bloodwynSlotGo;
        public BloodwynInstanceId bloodwynInstanceInstanceId;

        public enum BloodwynInstanceId
        {
            Deu = 6596
        }


        public void Bootstrap()
        {
            var newNpc = PrefabCache.TryGetObject(PrefabCache.PrefabType.Npc);
            newNpc.SetParent(bloodwynSlotGo);

            var npcSymbol = GameData.GothicVm.GetSymbolByIndex((int)bloodwynInstanceInstanceId);
            var npcInstance = GameData.GothicVm.AllocInstance<NpcInstance>(npcSymbol!);
            LookupCache.NpcCache[npcInstance.Index] = newNpc.GetComponent<NpcProperties>();

            GameData.GothicVm.InitInstance(npcInstance);
            
            newNpc.name = npcInstance.GetName(NpcNameSlot.Slot0);
            
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

            MeshObjectCreator.CreateNpc(newNpc.name, mdmName, mdhName, body, newNpc);
        }
    }
}
