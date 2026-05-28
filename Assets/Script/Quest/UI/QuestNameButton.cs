using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestNameButton : MonoBehaviour
{
    public TextMeshProUGUI questNameText;
    public QuestData_SO currentQuestDataSO;
    public TextMeshProUGUI questDescriptionText;

    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(UpdatedQuestDescription);
    }

    // 点击按钮时更新任务详细信息 包括描述，任务要求，奖励等
    void UpdatedQuestDescription()
    {
        if(currentQuestDataSO != null)
        {
            questDescriptionText.text = currentQuestDataSO.questDescription;
            if(currentQuestDataSO.isFinished)
            {
                QuestUI.Instance.SetUpQuestRequirements(currentQuestDataSO,currentQuestDataSO.isFinished);
            }
            else
                QuestUI.Instance.SetUpQuestRequirements(currentQuestDataSO);  //更新任务列表显示，反映当前选中任务的状态变化
            //清空之前的奖励显示
            foreach(Transform obj in QuestUI.Instance.rewardsTransform)
            {
                Destroy(obj.gameObject);
            }
            
            // 循环生成显示任务奖励
            foreach(var reward in currentQuestDataSO.rewards)
            {
                QuestUI.Instance.SetUpRewardList(reward.inventoryDataSO, reward.amount);
            }
        }
    }

    // 初始化按钮时显示任务名称
    public void SetUpQuestNameButton(QuestData_SO questData)
    {
        currentQuestDataSO = questData;
        questNameText.text = questData.isCompleted ? "[完成] " + currentQuestDataSO.questName : currentQuestDataSO.questName;
    }

}
