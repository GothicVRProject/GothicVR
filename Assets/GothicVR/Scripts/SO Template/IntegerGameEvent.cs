using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/Integer Event Channel")]
public class IntegerGameEvent : ScriptableObject
{
    public UnityAction<int> OnEventRaised;
    public void RaiseEvent(int value)
    {
        OnEventRaised?.Invoke(value);
    }
}