using System;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/DateTime Event Channel")]

public class DateTimeGameEvent : ScriptableObject
{
    public UnityAction<DateTime> OnEventRaised;
    public void RaiseEvent(DateTime value)
    {
        OnEventRaised?.Invoke(value);
    }
}
