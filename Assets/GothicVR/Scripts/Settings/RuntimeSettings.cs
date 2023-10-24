using GVR.Util;
using UnityEngine;
using GVR.Manager;
using UnityEngine.XR.Interaction.Toolkit;

namespace GVR.Debugging
{
    public class RuntimeSettings : SingletonBehaviour<RuntimeSettings>
    {
        public enum TurnType
        {
            ContinuousTurn,
            SnapTurn
        };

        [Header("__________Movement__________")]
        [Tooltip("Movement settings")]

        public TurnType turntype;

        public GameObject locomotionsystem;
        public ActionBasedSnapTurnProvider snapTurn;
        public ActionBasedContinuousTurnProvider continuousTurn;

        void Awake()
        {
            //TurnTypeSelected(PlayerPrefs.GetInt(ConstantsManager.turnSettingPlayerPref));
        }

        public void OnValidate()
        {
            DropdownItemSelected(turntype);
        }

        public void DropdownItemSelected(TurnType turntype)
        {
            switch (turntype)
            {
                case TurnType.ContinuousTurn:
                    EnableContinuousTurn();
                    break;
                case TurnType.SnapTurn:
                default:
                    EnableSnapTurn();
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
        }

        void EnableContinuousTurn()
        {
            SaveIntegerSettingsToPlayerPrefs(ConstantsManager.turnSettingPlayerPref, 1);

            if (!locomotionsystem)
                return;

            snapTurn.enabled = false;
            continuousTurn.enabled = true;
        }

        void TurnTypeSelected(int value)
        {
            switch (value)
            {
                case 1:
                    turntype = TurnType.ContinuousTurn;
                    break;
                case 0:
                default:
                    turntype = TurnType.SnapTurn;
                    break;
            }
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