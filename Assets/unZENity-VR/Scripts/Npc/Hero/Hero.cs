using UZVR.Phoenix.Bridge;
using UZVR.Phoenix.Bridge.Vm.Gothic;
using UZVR.Util;

namespace UZVR.Npc.Hero
{
    public class Hero: SingletonBehaviour<Hero>
    {

        private void Start()
        {
            var userPtr = PhoenixBridge.VmGothicNpcBridge.InitNpcInstance("hero");
            var symbolId = PhoenixBridge.VmGothicNpcBridge.GetNpcSymbolId(userPtr);

            GetComponent<Properties>().DaedalusSymbolId = symbolId;
        }
    }
}
