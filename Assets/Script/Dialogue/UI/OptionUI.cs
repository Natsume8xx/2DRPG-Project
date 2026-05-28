using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionUI : MonoBehaviour
{
    public TextMeshProUGUI optionText;
    private Button optionButton;
    private DialoguePiece dialoguePiece;
    private string nextDialogueID;
    private bool takeQuest = false;
    void Awake()
    {
        optionButton = GetComponent<Button>();
        optionButton.onClick.AddListener(OnOptionSelected);
    }

    // 根据传入的选项数据 更新选项UI显示
    public void UpdateOptionUI(DialogueOption option, DialoguePiece currentDialoguePiece)
    {
        optionText.text = option.optionText;
        dialoguePiece = currentDialoguePiece;
        nextDialogueID = option.nextDialogueID;
        takeQuest = option.takeQuest;
    }

    // 选项被点击时的处理逻辑
    private void OnOptionSelected()
    {
        if(dialoguePiece.relatedQuest != null)
        {
            var questTask = new QuestTask { questData = dialoguePiece.relatedQuest };
            if (takeQuest)
            {
                //添加到任务列表
                if(questTask.questData != null && !QuestManager.Instance.HaveQuest(questTask.questData))
                {
                    QuestManager.Instance.questTasks.Add(questTask);
                    QuestManager.Instance.GetQuestTask(questTask.questData).isStarted = true;
                    // 检查一下刚接到任务时任务完成情况（检测背包中的对应物品） //感觉这是未来可以优化的点，循环检测性能不好
                    foreach(var itemName in questTask.questData.GetRequireItemNamesList())
                    {
                        InventoryManager.Instance.CheckQuestItemInBag(itemName);
                    }
                }else if(questTask.questData != null && QuestManager.Instance.HaveQuest(questTask.questData))
                {
                    //判断完成情况，给予奖励
                    if (QuestManager.Instance.GetQuestTask(questTask.questData).isCompleted)
                    {
                        Debug.Log("完成任务："+questTask.questData.questName+"，给予奖励");
                        questTask.questData.GiveRewards();
                    }
                }
            }
        }
        if(nextDialogueID == null)
        {
            Debug.Log("选项的nextDialogueID为空，无法继续对话");
            return;
        }
        if(nextDialogueID == "")
        {
            Debug.Log("选项的nextDialogueID为空字符串，结束对话");
            DialogueUI.Instance.dialoguePanel.SetActive(false);
            return;
        }
        DialogueUI.Instance.UpdateMainUI(DialogueUI.Instance.currentDialogueDataSO.dialoguePieceDict[nextDialogueID]);
        DialogueUI.Instance.currentDialogueIndex = DialogueUI.Instance.currentDialogueDataSO.dialoguePieceDict[nextDialogueID].indexID+1;
    }
}
