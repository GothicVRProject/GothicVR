using GVR.Manager;
using UnityEngine;

namespace GVR.Player.Menu
{
    public class MenuButtonScript : MonoBehaviour
    {

        public GameObject MainMenu;
        public GameObject SettingsMenu;
        public GameObject TeleportMenu;
        public GameObject MovementMenu;
        public GameObject UIMenu;

        public void PlayFunction()
        {
            GvrSceneManager.I.LoadWorld(ConstantsManager.I.selectedWorld, ConstantsManager.I.selectedWaypoint);
        }

        public void ShowMainMenu()
        {
            MainMenu.SetActive(true);
            SettingsMenu.SetActive(false);
            TeleportMenu.SetActive(false);
            MovementMenu.SetActive(false);
            UIMenu.SetActive(false);
        }

        public void ShowSettingsMenu()
        {
            SettingsMenu.SetActive(true);
            MainMenu.SetActive(false);
            TeleportMenu.SetActive(false);
            MovementMenu.SetActive(false);
            UIMenu.SetActive(false);
        }

        public void ShowTeleportMenu()
        {
            TeleportMenu.SetActive(true);
            MainMenu.SetActive(false);
            SettingsMenu.SetActive(false);
            MovementMenu.SetActive(false);
            UIMenu.SetActive(false);
        }

        public void ShowMovementMenu()
        {
            MovementMenu.SetActive(true);
            MainMenu.SetActive(false);
            SettingsMenu.SetActive(false);
            TeleportMenu.SetActive(false);
            UIMenu.SetActive(false);
        }

        public void ShowUIMenu()
        {
            UIMenu.SetActive(true);
            MainMenu.SetActive(false);
            SettingsMenu.SetActive(false);
            TeleportMenu.SetActive(false);
            MovementMenu.SetActive(false);
        }

        public void QuitGameFunction()
        {
            Application.Quit();
        }
    }

}