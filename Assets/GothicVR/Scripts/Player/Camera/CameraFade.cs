using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GVR.Util;

namespace GVR.Player.Camera
{
    public class CameraFade : SingletonBehaviour<CameraFade>
    {
        public Image cameraFadeImage;
        public const float defaultCameraFadeDuration = 0.15f;

        private void Start()
        {
            Fade(defaultCameraFadeDuration, 0);
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
                currentTime += Time.deltaTime;
                cameraFadeImage.color = Color.Lerp(cameraFadeImage.color, new Color(cameraFadeImage.color.r, cameraFadeImage.color.g, cameraFadeImage.color.b, targetAlpha), currentTime / duration);
                yield return null;
            }
            yield break;
        }
    }
}
