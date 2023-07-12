using GVR.Demo;
using GVR.Util;
using GVR.World;
using System;
using GVR.Debugging;
using UnityEngine;

public class SunMover : MonoBehaviour
{
    private FeatureFlags.SunMovementPerformance sunPerformanceSetting;
    private GameObject sun;

    private void OnEnable()
    {
        var gameTime = GameTime.I;
        sunPerformanceSetting = FeatureFlags.I.SunMovementPerformanceValue;
        switch (sunPerformanceSetting)
        {
            case FeatureFlags.SunMovementPerformance.EveryIngameSecond:
                gameTime.secondChangeCallback.AddListener(RotateSun);
                break;
            case FeatureFlags.SunMovementPerformance.EveryIngameMinute:
                gameTime.minuteChangeCallback.AddListener(RotateSun);

                break;
            case FeatureFlags.SunMovementPerformance.EveryIngameHour:
                gameTime.minuteChangeCallback.AddListener(RotateSun);
                break;
            default:
                Debug.LogError($"{sunPerformanceSetting} isnt handled correctly. Thus SunMover.cs doesnt move the sun.");
                break;
        }
    }
    private void OnDisable()
    {
        var gameTime = SingletonBehaviour<GameTime>.GetOrCreate();
        switch (sunPerformanceSetting)
        {
            case FeatureFlags.SunMovementPerformance.EveryIngameSecond:
                gameTime.secondChangeCallback.RemoveListener(RotateSun);
                break;
            case FeatureFlags.SunMovementPerformance.EveryIngameMinute:
                gameTime.secondChangeCallback.RemoveListener(RotateSun);
                break;
            case FeatureFlags.SunMovementPerformance.EveryIngameHour:
                gameTime.secondChangeCallback.RemoveListener(RotateSun);
                break;
            default:
                Debug.LogError($"{sunPerformanceSetting} isnt handled correctly. Thus SunMover.cs doesnt move the sun.");
                break;
        }
    }
    private void Start()
    {
        sun = gameObject;
    }
    /// <summary>
    /// Based on performance settings, the sun direction is changed more or less frequent.
    ///
    /// Unity rotation settings:
    /// 270° = midnight (no light)
    /// 90° = noon (full light)
    /// 
    /// Calculation: 270f is the starting midnight value
    /// Calculation: One full DateTime == 360°. --> e.g. 15° * 24h + 0min + 0sec == 360°
    /// </summary>
    void RotateSun(DateTime time)
    {
        var xRotation = 270f + (15f * (time.Hour + (time.Minute / 60f) + (time.Second / 3600f)));
        sun.transform.eulerAngles = new(xRotation, 0, 0);
    }
}
