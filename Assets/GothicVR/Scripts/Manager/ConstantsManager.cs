using UnityEngine;
using GVR.Util;

namespace GVR.Manager
{
    public class ConstantsManager : SingletonBehaviour<ConstantsManager>
    {
        public const string SceneBootstrap = "Bootstrap";
        public const string SceneGeneral = "General";
        public const string SceneMainMenu = "MainMenu";
        public const string SceneLoading = "Loading";
        
        //Layer for all Items, specifically to disable collision physics between player and items
        public LayerMask ItemLayer;

        //Tags for components to exchange the default font with custom Gothic title and subtitle / ingame fonts
        public const string MenuFontTag = "Title";
        public const string SubtitleFontTag = "IngameText";
        public const string ClimbableTag = "Climbable";
        public const string SpotTag = "PxVob_zCVobSpot";
        public const string PlayerTag = "Player";

        public int MeshPerFrame = 10;
        public int VObPerFrame = 75;

        //Collection of PlayerPref entries for settings
        public string moveSpeedPlayerPref = "MoveSpeed";

        public string turnSettingPlayerPref = "TurnSetting";

        public float moveSpeed = 8f;

        public string selectedWorld = "world.zen";
        public string selectedWaypoint = "START"; 

        protected override void Awake()
        {
            base.Awake();
            ItemLayer = LayerMask.NameToLayer("Item");
        }
    }
}
