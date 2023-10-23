using GVR.Util;
using UnityEngine;
using GVR.Manager;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEditor;

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
            //PlayerPrefs.SetInt(ConstantsManager.I.turnSettingPlayerPref, 0);

            if (!locomotionsystem)
                return;

            snapTurn.enabled = true;
            continuousTurn.enabled = false;
        }

        void EnableContinuousTurn()
        {
            //PlayerPrefs.SetInt(ConstantsManager.I.turnSettingPlayerPref, 1);

            if (!locomotionsystem)
                return;

            snapTurn.enabled = false;
            continuousTurn.enabled = true;
        }

    }
}