using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestRequirement : MonoBehaviour
{
    public TextMeshProUGUI requirementNameText;
    public TextMeshProUGUI progressNumberText;
    void Awake()
    {
        requirementNameText = GetComponent<TextMeshProUGUI>();
        progressNumberText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
    }

    // 设置任务需求UI的内容 包括需求名称，当前进度和需求数量
    public void SetUpRequirement(string requirementName,int currentAmount,int requiredAmount)
    {
        requirementNameText.text = requirementName;
        progressNumberText.text = $"{currentAmount}/{requiredAmount}";
    }
}
