using GVR.Phoenix.Interface;
using GVR.Properties;
using GVR.Util;
using PxCs.Interface;

namespace GVR.Npc.Hero
{
    public class Hero: SingletonBehaviour<Hero>
    {

        private void Start()
        {
            var hero = PxVm.InitializeNpc(GameData.VmGothicPtr, "hero");
            GetComponent<NpcProperties>().npc = hero;
        }
    }
}
