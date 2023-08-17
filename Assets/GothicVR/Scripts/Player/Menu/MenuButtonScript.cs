using GVR.Manager;
using UnityEngine;

namespace GVR.Player.Menu
{
    public class MenuButtonScript : MonoBehaviour
    {
        public void PlayFunction()
        {
            GvrSceneManager.I.LoadWorld(ConstantsManager.I.selectedWorld, ConstantsManager.I.selectedWaypoint);
        }

        public void QuitGameFunction()
        {
            Application.Quit();
        }
    }

}