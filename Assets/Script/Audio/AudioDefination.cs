using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 音频自定义的类
public class AudioDefination : MonoBehaviour
{
    public PlayAudioEventSO playAudioEventSO;  // 在项目中 是和 FX 同一个事件
    public AudioClip audioClip;
    public bool isEnablePlayAudio;   // 是否在启用时就播放音频
    void OnEnable()
    {
        if (isEnablePlayAudio)
        {
            PlayAudio();
        }
    }
    public void PlayAudio()
    {
        playAudioEventSO.RaiseEvent(audioClip);
    }
}
