using PxCs;
using System;
using UZVR.Phoenix.Interface;
using UZVR.Util;

namespace UZVR.Npc.Hero
{
    public class Hero: SingletonBehaviour<Hero>
    {

        private void Start()
        {
            var hero = PxVm.InitializeNpc(PhoenixBridge.VmGothicPtr, "hero");
            GetComponent<Properties>().npc = hero;
        }
    }
}
