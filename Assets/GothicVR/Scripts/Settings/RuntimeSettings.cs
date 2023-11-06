using GVR.Util;
using UnityEngine;
using GVR.Manager;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.SceneManagement;

namespace GVR.Debugging
{
    public class RuntimeSettings : SingletonBehaviour<RuntimeSettings>
    {
        public enum TurnType
        {  
            SnapTurn,
            ContinuousTurn
        };

        [Header("__________Movement__________")]
        [Tooltip("Movement settings")]

        public static TurnType turntype;
        public TurnType turntypeUI;

        public GameObject locomotionsystem;
        public ActionBasedSnapTurnProvider snapTurn;
        public ActionBasedContinuousTurnProvider continuousTurn;

        protected override void Awake()
        {
            Debug.Log("Initial PlayerPref: " + PlayerPrefs.GetInt(ConstantsManager.turnSettingPlayerPref).ToString());
            base.Awake();
            turntypeUI = (TurnType)PlayerPrefs.GetInt(ConstantsManager.turnSettingPlayerPref);
            Debug.Log(PlayerPrefs.GetInt(ConstantsManager.turnSettingPlayerPref).ToString());
            Debug.Log(turntypeUI.ToString());
            turntype = turntypeUI;
            DropdownItemSelected(turntypeUI);
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            DropdownItemSelected(turntypeUI);
        }
#endif

        public void DropdownItemSelected(TurnType turntypeUI)
        {
            switch(turntypeUI)
            {
                case TurnType.ContinuousTurn:
                    EnableContinuousTurn();
                    Debug.Log("Cont");
                    break;
                case TurnType.SnapTurn:
                default:
                    EnableSnapTurn();
                    Debug.Log("Snap");
                    break;
            }
        }

        void EnableSnapTurn()
        {
            SaveIntegerSettingsToPlayerPrefs(ConstantsManager.turnSettingPlayerPref, 0);
            Debug.Log("Saved: " + 0);

            if (!locomotionsystem)
                return;

            snapTurn.enabled = true;
            continuousTurn.enabled = false;
            turntype = TurnType.SnapTurn;
        }

        void EnableContinuousTurn()
        {
            SaveIntegerSettingsToPlayerPrefs(ConstantsManager.turnSettingPlayerPref, 1);
            Debug.Log("Saved: " + 1);

            if (!locomotionsystem)
                return;

            snapTurn.enabled = false;
            continuousTurn.enabled = true;
            turntype = TurnType.ContinuousTurn;
        }

        void SaveIntegerSettingsToPlayerPrefs(string playerPrefEntry, int settingsValue)
        {
            PlayerPrefs.SetInt(playerPrefEntry, settingsValue);
        }

        public static int LoadSettingsFromPlayerPrefs(string playerPrefEntry)
        {
            int playerPrefValue = PlayerPrefs.GetInt(playerPrefEntry);
            return playerPrefValue;
        }

    }
}