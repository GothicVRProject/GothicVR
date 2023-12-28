using GVR.Phoenix.Interface;
using GVR.Properties;
using GVR.Util;
using PxCs.Interface;
using ZenKit.Daedalus;

namespace GVR.Npc.Hero
{
    public class Hero: SingletonBehaviour<Hero>
    {

        private void Start()
        {
            var hero = GameData.GothicVm.AllocInstance<NpcInstance>("hero");
            GameData.GothicVm.InitInstance(hero);

            GetComponent<NpcProperties>().npcInstance = hero;
        }
    }
}
