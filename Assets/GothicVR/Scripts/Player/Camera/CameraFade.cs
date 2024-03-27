using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GVR
{
    public class CameraFade : MonoBehaviour
    {
        public Image cameraFadeImage;

        private void Start()
        {
            Fade(0.1f, 0);
        }

        public void Fade(float duration, float targetAlpha)
        {
            StartCoroutine(FadeCamera(duration, targetAlpha));
        }

        private IEnumerator FadeCamera(float duration, float targetAlpha)
        {
            float currentTime = 0;

            while (currentTime < duration)
            {
                currentTime += Time.fixedDeltaTime;
                cameraFadeImage.color = Color.Lerp(cameraFadeImage.color, new Color(cameraFadeImage.color.r, cameraFadeImage.color.g, cameraFadeImage.color.b, targetAlpha), currentTime / duration);
                yield return null;
            }
            yield break;
        }
    }
}
