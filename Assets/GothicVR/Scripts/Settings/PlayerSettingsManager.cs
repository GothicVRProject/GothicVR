using GVR.Util;
using UnityEngine;
using GVR.Manager;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.SceneManagement;

namespace GVR.Debugging
{
    public class PlayerSettingsManager : SingletonBehaviour<PlayerSettingsManager>
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
        private bool isAwoken = false;

        protected void OnEnable()
        {
            base.Awake();
            isAwoken = true;
            turntypeUI = (TurnType)PlayerPrefs.GetInt(ConstantsManager.turnSettingPlayerPref);
            turntype = turntypeUI;
            DropdownItemSelected(turntypeUI);
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (isAwoken)
            {
                DropdownItemSelected(turntypeUI);
            }
        }
#endif

        public void DropdownItemSelected(TurnType selectedturntypeUI)
        {
            switch(selectedturntypeUI)
            {
                case TurnType.ContinuousTurn:
                    EnableContinuousTurn();
                    turntypeUI = TurnType.ContinuousTurn;
                    break;
                case TurnType.SnapTurn:
                default:
                    EnableSnapTurn();
                    turntypeUI = TurnType.SnapTurn;
                    break;
            }
        }

        void EnableSnapTurn()
        {
            SaveIntegerSettingsToPlayerPrefs(ConstantsManager.turnSettingPlayerPref, 0);

            if (!locomotionsystem)
                return;

            snapTurn.enabled = true;
            continuousTurn.enabled = false;
            turntype = TurnType.SnapTurn;
        }

        void EnableContinuousTurn()
        {
            SaveIntegerSettingsToPlayerPrefs(ConstantsManager.turnSettingPlayerPref, 1);

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