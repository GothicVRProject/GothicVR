using System;
using GVR.Debugging;
using GVR.Globals;
using GVR.World;
using UnityEngine;

public class SunMover : MonoBehaviour
{
    private FeatureFlags.SunMovementPerformance sunPerformanceSetting;
    private GameObject sun; // aka this

    private void Start()
    {
        sun = gameObject; // aka this

        var gameTime = GameTime.I;
        sunPerformanceSetting = FeatureFlags.I.sunMovementPerformanceValue;
        switch (sunPerformanceSetting)
        {
            case FeatureFlags.SunMovementPerformance.EveryIngameSecond:
                GvrEvents.GameTimeSecondChangeCallback.AddListener(RotateSun);
                break;
            case FeatureFlags.SunMovementPerformance.EveryIngameMinute:
                GvrEvents.GameTimeMinuteChangeCallback.AddListener(RotateSun);
                break;
            case FeatureFlags.SunMovementPerformance.EveryIngameHour:
                GvrEvents.GameTimeHourChangeCallback.AddListener(RotateSun);
                break;
            default:
                Debug.LogError($"{sunPerformanceSetting} isn't handled correctly. Therefore SunMover.cs won't move the sun.");
                break;
        }
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
