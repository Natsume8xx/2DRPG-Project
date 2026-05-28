using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestUI : Singleton<QuestUI>
{
    [Header("属性")]
    public GameObject questPanel;
    public bool isOpen;
    public ItemTooltip itemTooltip;
    [Header("任务名称UI组件")]
    public RectTransform questListTransform;
    public QuestNameButton questNameButtonPrefab;
    [Header("任务内容UI组件")]
    public TextMeshProUGUI questContentText;
    [Header("任务需求UI组件")]
    public RectTransform requirementsTransform;
    public QuestRequirement requirementPrefab;
    [Header("任务奖励UI组件")]
    public RectTransform rewardsTransform;
    public ItemUI rewardPrefab;

    [Header("玩家控制输入")]
    public PlayerInputControl playerInputControl;
    protected override void Awake()
    {
        base.Awake();
        playerInputControl = new PlayerInputControl();
        playerInputControl.Enable();
    }

    void Update()
    {
        if (playerInputControl.GamePlay.QuestCheck.triggered)
        {
            Debug.Log("打开/关闭任务面板");
            isOpen = !isOpen;
            questPanel.SetActive(isOpen);
            if (!isOpen)
                return;
            questContentText.text = string.Empty;
            //显示面板内容
            SetUpQuestList();
            if(!isOpen)
               itemTooltip.gameObject.SetActive(false);
        }
    }

    //  初始化面板
    public void SetUpQuestList()
    {
        //清空之前的面板内容
        foreach(Transform obj in questListTransform)
        {
            Destroy(obj.gameObject);
        }
        foreach(Transform obj in requirementsTransform)
        {
            Destroy(obj.gameObject);
        }
        foreach(Transform obj in rewardsTransform)
        {
            Destroy(obj.gameObject);
        }
        // 生成 任务按钮 列表
        foreach(var task in QuestManager.Instance.questTasks)
        {
            var newTask = Instantiate(questNameButtonPrefab, questListTransform);
            newTask.GetComponent<QuestNameButton>().questDescriptionText = questContentText;
            newTask.GetComponent<QuestNameButton>().SetUpQuestNameButton(task.questData);
        }
    }

    // 生成 任务要求 列表
    public void SetUpQuestRequirements(QuestData_SO questData)
    {
        foreach(Transform obj in requirementsTransform)
        {
            Destroy(obj.gameObject);
        }
        foreach(var require in questData.questRequires)
        {
            var requirement = Instantiate(requirementPrefab, requirementsTransform);
            requirement.SetUpRequirement(require.requireName, require.currentAmount, require.requireAmount);
        }
    }

    //重载方法，用于任务已经完成的情况
    public void SetUpQuestRequirements(QuestData_SO questData, bool isFinished)
    {
        if (isFinished)
        {
            foreach(Transform obj in requirementsTransform)
            {
                Destroy(obj.gameObject);
            }
            foreach(var require in questData.questRequires)
            {
                var requirement = Instantiate(requirementPrefab, requirementsTransform);
                requirement.progressNumberText.text = "已完成";
                requirement.progressNumberText.color = Color.gray;
                requirement.requirementNameText.color = Color.gray;
            }
        }
    }

    // 设置奖励列表的内容
    public void SetUpRewardList(ItemData_SO itemData,int rewardAmount)
    {
        var reward = Instantiate(rewardPrefab, rewardsTransform);
        reward.SetUpItemUI(itemData, rewardAmount);
    }
}
