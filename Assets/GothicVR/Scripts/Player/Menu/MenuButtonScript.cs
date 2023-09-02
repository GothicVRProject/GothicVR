using GVR.Manager;
using UnityEngine;

namespace GVR.Player.Menu
{
    public class MenuButtonScript : MonoBehaviour
    {
        public void PlayFunction()
        {
#pragma warning disable CS4014 // It's intended, that this async call is not awaited.
            GvrSceneManager.I.LoadWorld(ConstantsManager.I.selectedWorld, ConstantsManager.I.selectedWaypoint);
#pragma warning restore CS4014
        }

        public void QuitGameFunction()
        {
            Application.Quit();
        }
    }

}