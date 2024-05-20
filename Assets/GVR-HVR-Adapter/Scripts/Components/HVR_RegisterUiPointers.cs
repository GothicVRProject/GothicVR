#if GVR_HVR_INSTALLED
using HurricaneVR.Framework.Core.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GVR
{
    public class HVR_RegisterUiPointers : MonoBehaviour
    {
        private void Start()
        {
            HVRUIPointer[] pointers = GetComponentsInChildren<HVRUIPointer>();
            for (int i = 0; i < pointers.Length; i++)
            {
                HVRInputModule.Instance.AddPointer(pointers[i]);
            }
        }
    }
}
#endif
