using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName ="Event/FadeEventSO")]
public class FadeEventSO : ScriptableObject
{
    public UnityAction<Color,float> fadeEvent;

    public void FadeIn(Color targetColor, float duration)
    {
        RaiseEvent(targetColor, duration);
    }

    public void FadeOut(Color targetColor, float duration)
    {
        RaiseEvent(targetColor, duration);
    }

    public void RaiseEvent(Color targetColor,float duration)
    {
        fadeEvent?.Invoke(targetColor, duration);
    }
}
