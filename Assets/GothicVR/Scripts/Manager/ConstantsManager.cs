using UnityEngine;
using GVR.Util;

namespace GVR.Manager
{
    public class ConstantsManager : SingletonBehaviour<ConstantsManager>
    {
        //Layer for all Items, specifically to disable collision physics between player and items
        public LayerMask ItemLayer;

        //Tags for components to exchange the default font with custom Gothic title and subtitle / ingame fonts
        public string MenuFontTag = "Title";
        public string SubtitleFontTag = "IngameText";
        public string ClimbableTag = "Climbable";

        public int MeshPerFrame = 10;
        public int VObPerFrame = 75;

        //Default / Start values
        public float turnAngleDefault = 45;
        public float moveSpeedDefault = 8;

        //Collection of PlayerPref entries for settings
        public string moveSpeedPlayerPref = "MoveSpeed";
        public string turnSettingPlayerPref = "TurnSetting";
        public string uiBehaviourPlayerPref = "UIBehaviour";

        public string selectedWorld = "world.zen";
        public string selectedWaypoint = "START"; 
        public string turnAnglePlayerPref = "TurnAngleSetting";
        protected override void Awake()
        {
            base.Awake();
            ItemLayer = LayerMask.NameToLayer("Item");
        }
    }
}
