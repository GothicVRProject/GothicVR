using GVR.Manager;
using UnityEngine;

namespace GVR.Player.Menu
{
    public class MenuManager : MonoBehaviour
    {
        public GameObject MainMenu;
        public GameObject SettingsMenu;
        public GameObject TeleportMenu;
        public GameObject MovementMenu;
        public GameObject UIMenu;

        [SerializeField]
        private MoveSpeedController moveSpeedController;

        [SerializeField]
        private TurnSettingDropdownController turnSettingDropdownController;

        void Awake()
        {
            SetSettingsValues();
        }

        public void SetSettingsValues()
        {
            if (moveSpeedController == null || turnSettingDropdownController == null)
                return;

            moveSpeedController.ChangeMoveSpeed(PlayerPrefs.GetFloat(ConstantsManager.moveSpeedPlayerPref));
            turnSettingDropdownController.DropdownItemSelected(PlayerPrefs.GetInt(ConstantsManager.turnSettingPlayerPref));
        }

        public void PlayFunction()
        {
#pragma warning disable CS4014 // It's intended, that this async call is not awaited.
            GvrSceneManager.I.LoadWorld(ConstantsManager.selectedWorld, ConstantsManager.selectedWaypoint);
#pragma warning restore CS4014
        }

        public void SwitchMenu(GameObject menu)
        {
            MainMenu.SetActive(menu == MainMenu);
            SettingsMenu.SetActive(menu == SettingsMenu);
            TeleportMenu.SetActive(menu == TeleportMenu);
            MovementMenu.SetActive(menu == MovementMenu);
            UIMenu.SetActive(menu == UIMenu);
        }

        public void QuitGameFunction()
        {
            Application.Quit();
        }
    }
}