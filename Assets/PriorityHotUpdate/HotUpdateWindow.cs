using System;
using System.Collections;
using System.Collections.Generic;
//using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HotUpdateWindow : MonoBehaviour, IHotUpdateWindow
{
    [Header("UI")]
    public TextMeshProUGUI barText;
    public Image fillImage;

    [Header("Parameters")]
    public long allBytes;
    public float currentProgress;
    public float showProgress;
    public float lerpSpeed = 0.5f;
    public Action onEnd;

    // 初始化进度
    public void Show(long allBytes,Action onEnd)
    {
        this.gameObject.SetActive(true);
        this.allBytes = allBytes;
        this.onEnd = onEnd;
    }

    // 更新进度
    public void UpdatedBar(long downLoadBytes)
    {
        currentProgress = (float)downLoadBytes /  allBytes;
    }
    void Update()
    {
        // 插值显示进度
        showProgress = Mathf.MoveTowards(showProgress,currentProgress,Time.deltaTime * lerpSpeed);
        barText.text = $"热更新进度：{allBytes * showProgress/1024f/1024f:F2}MB/{allBytes/1024f/1024f:F2}MB";
        fillImage.fillAmount = showProgress;

        // 当显示的进度达到100%时，执行结束回调
        if(showProgress >= 1f)
        {
            onEnd?.Invoke();
            onEnd = null; // 确保回调只执行一次
        }
    }
}
