using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;
public class CameraControl : MonoBehaviour
{
    //场景切换完成时的事件
    public VoidEventSO afterSceneLoadEventSO;
    private CinemachineConfiner2D cinemachineConfiner2D;
    public CinemachineImpulseSource impulseSource;
    public VoidEventSO camerShakeEvent;
    private void Awake() {
        cinemachineConfiner2D = GetComponent<CinemachineConfiner2D>();
    }

    void Start()
    {
        GetNewCameraBounds();
    }
    void OnEnable() {
        camerShakeEvent.OnEventRaised += ShakeCamera;
        afterSceneLoadEventSO.OnEventRaised += OnSceneLoadCompleteEvent;
    }
    void OnDisable() {
        camerShakeEvent.OnEventRaised -= ShakeCamera;
        afterSceneLoadEventSO.OnEventRaised -= OnSceneLoadCompleteEvent;
    }

    // 场景切换事件的订阅函数
    private void OnSceneLoadCompleteEvent()
    {
        GetNewCameraBounds();
    }

    // 震动摄像机
    public void ShakeCamera() {
        impulseSource.GenerateImpulse();
    }


    // 在场景切换后，获取新的摄像机边界
    public void GetNewCameraBounds() {
        var bounds = GameObject.FindGameObjectWithTag("Bounds");
        if(bounds == null)
            return;
        cinemachineConfiner2D.m_BoundingShape2D = bounds.GetComponent<Collider2D>();
        cinemachineConfiner2D.InvalidateCache();
    }
}
