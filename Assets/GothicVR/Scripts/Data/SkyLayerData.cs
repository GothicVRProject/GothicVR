using UnityEngine;

namespace GVR.Data
{
    public class SkyLayerData
    {
        public Texture2D[] texBox = null;
        public Texture2D tex = null;
        public string texName = "";
        public float texAlpha = 0;
        public float texScale = 1;
        public Vector2 texSpeed = new(0.9f, 1.1f);
    }
}