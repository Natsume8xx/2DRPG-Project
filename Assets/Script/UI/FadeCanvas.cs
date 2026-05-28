using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class FadeCanvas : MonoBehaviour
{
    public Image fadeImage;  // 用于淡入淡出的UI Image
    public FadeEventSO fadeEventSO;  // 淡入淡出事件
    void OnEnable()
    {
        fadeEventSO.fadeEvent += OnFadeEvent;
    }

    void OnDisable()
    {
        fadeEventSO.fadeEvent -= OnFadeEvent;
    }

    private void OnFadeEvent(Color color, float duration)
    {
        // 使用DOTween进行淡入淡出动画
        fadeImage.DOBlendableColor(color, duration);
    }
}
