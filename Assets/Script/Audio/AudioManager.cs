using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [Header("组件")]
    public AudioSource BGM;
    public AudioSource FX;
    public AudioMixer mixer;
    [Header("事件监听")]
    public PlayAudioEventSO BGMEvent;
    public PlayAudioEventSO FXEvent;
    public FloatEventSO volumeChangeEvent;
    public VoidEventSO pauseEvent;  //游戏暂停事件
    [Header("事件广播")]
    public FloatEventSO syncVolumeEvent;  //传递音频数据信息的事件

    void OnEnable()
    {
        FXEvent.OnEventRasied += PlayFX;
        BGMEvent.OnEventRasied += PlayBGM;
        volumeChangeEvent.OnEventRaised += OnVolumeChangeEvent;
        pauseEvent.OnEventRaised += OnPauseEvent;
    }


    void OnDisable()
    {
        FXEvent.OnEventRasied -= PlayFX;
        BGMEvent.OnEventRasied -= PlayBGM;
        volumeChangeEvent.OnEventRaised -= OnVolumeChangeEvent;
        pauseEvent.OnEventRaised -= OnPauseEvent;
    }

    // 游戏暂停事件的响应函数
    private void OnPauseEvent()
    {
        float amount;
        mixer.GetFloat("MasterVolume",out amount);
        syncVolumeEvent.RaiseEvent(amount);
    }

    // 修改音量的响应函数
    private void OnVolumeChangeEvent(float amount)
    {
        mixer.SetFloat("MasterVolume",amount*100-80);
    }

    private void PlayFX(AudioClip audioClip)
    {
        FX.clip = audioClip;
        FX.Play();
    }
    private void PlayBGM(AudioClip audioClip)
    {
        BGM.clip = audioClip;
        BGM.Play();
    }
}
