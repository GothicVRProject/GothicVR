using GVR.Context;
using GVR.Util;
using HurricaneVR.Framework.Core.Player;
using HurricaneVR.Framework.Core.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GVR.Manager;

namespace GVR
{
    public class HVRPlayerManager : SingletonBehaviour<HVRPlayerManager>
    {
        public HVRPlayerController playerController;
        private void Start()
        {
            //find all ui canvases and add to HVR Input module
            Canvas[] allCanvases = FindObjectsOfType<Canvas>(true);
            HVRInputModule.Instance.UICanvases = allCanvases.ToList();
            //make sure UI pointers are added to input module
            HVRUIPointer[] pointers = GetComponentsInChildren<HVRUIPointer>();
            for (int i = 0; i < pointers.Length; i++)
            {
                HVRInputModule.Instance.AddPointer(pointers[i]);
            }
            StartCoroutine(TeleportToStartPos());
            //teleport to start position
            TeleportToStartPos();
        }
        public IEnumerator TeleportToStartPos()
        {
            yield return new WaitForSeconds(0.5f);
            GvrSceneManager.I.TeleportPlayerToSpot(playerController.gameObject);
        }
    }
}
