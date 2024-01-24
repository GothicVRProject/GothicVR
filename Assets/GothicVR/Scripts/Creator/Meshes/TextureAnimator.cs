using System.Collections;
using UnityEngine;
using ZenKit;
using Material = UnityEngine.Material;

namespace GVR.Creator.Meshes
{

    public class TextureAnimator : MonoBehaviour
    {
        public Texture2D[] frames; // Array of textures for the animation frames
        public Material targetMaterial; // Material where the textures will be applied
        public IMaterial materialData;
        
        private void Start()
        {
            if (frames.Length > 0)
                StartCoroutine(AnimateTextures());
        }

        private IEnumerator AnimateTextures()
        {
            int frameIndex = 0;
            while (true)
            {
                targetMaterial.mainTexture = frames[frameIndex]; // Set the material's texture to the current frame
                frameIndex = (frameIndex + 1) % frames.Length; // Loop back to the start of the array after the last frame
                yield return new WaitForSeconds(1.0f / materialData.TextureAnimationFps); // Wait for the next frame based on the frame rate
            }
        }
    }
}