using UnityEngine;

namespace GVR.Globals
{
    public static class Constants
    {
        public const float NpcWalkingSpeed = 1f;
        public const float NpcRotationSpeed = 3f;
        
        public const string SceneBootstrap = "Bootstrap";
        public const string SceneGeneral = "General";
        public const string SceneMainMenu = "MainMenu";
        public const string SceneLoading = "Loading";

        //Layer for all Items, specifically to disable collision physics between player and items
        public static LayerMask ItemLayer { get; set; } = LayerMask.NameToLayer("Item");

        // solves some weird interactions between the teleport raycast and collider (musicZone/worldTriggerChange)
        public static LayerMask IgnoreRaycastLayer { get; set; } = LayerMask.NameToLayer("Ignore Raycast");

        //Tags for components to exchange the default font with custom Gothic title and subtitle / ingame fonts
        public const string MenuFontTag = "Title";
        public const string SubtitleFontTag = "IngameText";
        public const string ClimbableTag = "Climbable";
        public const string SpotTag = "PxVob_zCVobSpot";
        public const string PlayerTag = "Player";

        public static int MeshPerFrame { get; } = 10;
        public static int VObPerFrame { get; } = 75;

        //Collection of PlayerPref entries for settings
        public const string moveSpeedPlayerPref = "MoveSpeed";
        public const string turnSettingPlayerPref = "TurnSetting";
        public const string musicVolumePlayerPref = "BackgroundMusicVolume";
        public const string soundEffectsVolumePlayerPref = "SoundEffectsVolume";
        public static float moveSpeed { get; set; } = 8f;

        public static string selectedWorld { get; set; } = "world.zen";
        public static string selectedWaypoint { get; set; } = "START";
    }
}
