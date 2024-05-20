#if GVR_HVR_INSTALLED
using HurricaneVR.Framework.Core.UI;
using UnityEngine;

namespace GVR
{
    public class HVRRegisterUiCanvas : MonoBehaviour
    {
        private void Start()
        {
            Invoke("RegisterCanvas", 0.5f);
        }

        private void RegisterCanvas()
        {
            Canvas canvas = GetComponent<Canvas>();
            HVRInputModule.Instance.UICanvases.Add(canvas);
        }
    }
}
#endif
