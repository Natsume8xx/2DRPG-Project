using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event/PlayAudioEventSO")]
public class PlayAudioEventSO :ScriptableObject
{
    public UnityAction<AudioClip> OnEventRasied;
    public void RaiseEvent(AudioClip audioClip)
    {
        OnEventRasied?.Invoke(audioClip);
    }
}
