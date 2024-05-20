using HurricaneVR.Framework.Core.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GVR
{
    public class HVR_RegisterUiCanvas : MonoBehaviour
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
