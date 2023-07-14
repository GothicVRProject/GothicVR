using GVR.Phoenix.Interface;
using GVR.Util;
using PxCs.Interface;

namespace GVR.Npc.Hero
{
    public class Hero: SingletonBehaviour<Hero>
    {

        private void Start()
        {
            var hero = PxVm.InitializeNpc(GameData.I.VmGothicPtr, "hero");
            GetComponent<Properties>().npc = hero;
        }
    }
}
