using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace UZVR.Phoenix.Vm.Externals
{
    public static class DefaultExternals
    {
        public static UnityEvent PhoenixNotImplementedCallback = new();

        public static void NotImplementedCallback(string value)
        {
            // FIXME: Once solution is released, we can safely throw an exception as it tells us: Brace yourself! The game will not work until you implement it.
            //throw new NotImplementedException("External >" + value + "< not registered but required by DaedalusVM.");

            // DEBUG During development
            Debug.LogError("External >" + value + "< not registered but required by DaedalusVM.");

            PhoenixNotImplementedCallback.Invoke();
        }
    }
}
