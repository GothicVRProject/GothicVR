using UnityEngine;

namespace GVR.Globals
{
    public static class Constants
    {
        // Unity shaders
        public static readonly Shader ShaderUnlit = Shader.Find("Universal Render Pipeline/Unlit"); // "Unlit/Transparent Cutout";
        public static readonly Shader ShaderUnlitParticles = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        public static readonly Shader ShaderTMPSprite = Shader.Find("TextMeshPro/Sprite");
        public static readonly Shader ShaderDecal = Shader.Find("Shader Graphs/Decal");
        public static readonly Shader ShaderStandard = Shader.Find("Standard");

        // Custom GVR shaders
        public static readonly Shader ShaderUnlitAlphaToCoverage = Shader.Find("Unlit/Unlit-AlphaToCoverage");
        // public static readonly Shader ShaderWater = Shader.Find("Shader Graphs/Unlit_Both_ScrollY"); //Vince's moving texture water shader
        public static readonly Shader ShaderWater = Shader.Find("Unlit/Water"); //Vince's moving texture water shader
        public static readonly Shader ShaderBarrier = Shader.Find("Unlit/Barrier");
        public static readonly Shader ShaderThunder = Shader.Find("Unlit/ThunderShader");


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
