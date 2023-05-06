using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/Float Event Channel")]

public class FloatGameEvent : ScriptableObject
{
    public UnityAction<float> OnEventRaised;
    public void RaiseEvent(float value)
    {
        OnEventRaised?.Invoke(value);
    }
}
