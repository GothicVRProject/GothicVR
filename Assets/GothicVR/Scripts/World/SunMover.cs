using GVR.Demo;
using GVR.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunMover : MonoBehaviour
{
    private DebugSettings.SunMovementPerformance sunPerformanceSetting;
    private GameObject sun;

    public DateTimeGameEvent secondChangeEvent;
    public DateTimeGameEvent minuteChangeEvent;
    public DateTimeGameEvent hourChangeEvent;

    private void OnEnable()
    {
        sunPerformanceSetting = SingletonBehaviour<DebugSettings>.GetOrCreate().SunMovementPerformanceValue;
        switch (sunPerformanceSetting)
        {
            case DebugSettings.SunMovementPerformance.EveryIngameSecond:
                secondChangeEvent.OnEventRaised += RotateSun;
                break;
            case DebugSettings.SunMovementPerformance.EveryIngameMinute:
                minuteChangeEvent.OnEventRaised += RotateSun;
                break;
            case DebugSettings.SunMovementPerformance.EveryIngameHour:
                hourChangeEvent.OnEventRaised += RotateSun;
                break;
            default:
                break;
        }
    }
    private void OnDisable()
    {
        switch (sunPerformanceSetting)
        {
            case DebugSettings.SunMovementPerformance.EveryIngameSecond:
                secondChangeEvent.OnEventRaised -= RotateSun;
                break;
            case DebugSettings.SunMovementPerformance.EveryIngameMinute:
                minuteChangeEvent.OnEventRaised -= RotateSun;
                break;
            case DebugSettings.SunMovementPerformance.EveryIngameHour:
                hourChangeEvent.OnEventRaised -= RotateSun;
                break;
            default:
                break;
        }
    }
    private void Start()
    {
        sun = gameObject;
    }
    void RotateSun(DateTime time)
    {
        var xRotation = 270f + (15f * (time.Hour + (time.Minute / 60f) + (time.Second / 3600f)));
        sun.transform.eulerAngles = new(xRotation, 0, 0);
    }
}
