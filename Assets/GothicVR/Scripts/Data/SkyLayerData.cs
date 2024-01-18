using UnityEngine;

namespace GVR.Data
{
    public class SkyLayerData
    {
        public Texture2D[] texBox;
        public Texture2D tex;
        public string texName = "";
        public float texAlpha;
        public float texScale = 1;
        public Vector2 texSpeed = new(0.9f, 1.1f);
    }
}