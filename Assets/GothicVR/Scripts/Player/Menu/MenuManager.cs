using GVR.Creator;
using GVR.Manager;
using GVR.Phoenix.Util;
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

        void Awake()
        {
            SetSettingsValues();
        }

        public void SetSettingsValues()
        {
            var moveSetting = MovementMenu.FindChildRecursively("MoveSpeedSlider").GetComponent<MoveSpeedController>();
            moveSetting.ChangeMoveSpeed(PlayerPrefs.GetFloat(ConstantsManager.I.moveSpeedPlayerPref));

            var turnSetting = MovementMenu.FindChildRecursively("TurnSettingDropdown").GetComponent<TurnSettingDropdownController>();
            turnSetting.DropdownItemSelected(PlayerPrefs.GetInt(ConstantsManager.I.turnSettingPlayerPref));
        }

        public void PlayFunction()
        {
#pragma warning disable CS4014 // It's intended, that this async call is not awaited.
            GvrSceneManager.I.LoadWorld(ConstantsManager.I.selectedWorld, ConstantsManager.I.selectedWaypoint);
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