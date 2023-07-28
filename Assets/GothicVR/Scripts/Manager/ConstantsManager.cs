using UnityEngine;
using GVR.Util;


namespace GVR.Manager
{
    public class ConstantsManager : SingletonBehaviour<ConstantsManager>
    {
        public LayerMask ItemLayer;
        public string MenuFontTag = "Title";
        public string SubtitleFontTag = "IngameText";

        public int MeshPerFrame = 10;
        public int VObPerFrame = 75;

        protected override void Awake()
        {
            base.Awake();
            ItemLayer = LayerMask.NameToLayer("Item");
        }
    }
}
