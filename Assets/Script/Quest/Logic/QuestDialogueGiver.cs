using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DialogueController))]
public class QuestDialogueGiver : MonoBehaviour
{
    public DialogueController dialogueController;
    public QuestData_SO questData;
    public DialogueData_SO startedDialogueData;  // 接取任务前的对话
    public DialogueData_SO progressedDialogueData; // 任务未完成的对话
    public DialogueData_SO completedDialogueData; // 任务完成但未交付的对话
    public DialogueData_SO finishedDialogueData; // 任务完成并交付后的对话
    #region 任务状态 的更新
    public bool isStarted
    {
        get
        {
            if(questData != null)
                return questData.isAccepted;
            return false;
        }
    }
    public bool isCompleted
    {
        get
        {
            if(questData != null)
                return questData.isCompleted;
            return false;
        }
    }
    public bool isFinished
    {
        get
        {
            if(questData != null)
                return questData.isFinished;
            return false;
        }
    }
    #endregion
    void Awake()
    {
        dialogueController = GetComponent<DialogueController>();
        questData = dialogueController.dialogueDataSO.GetQuest();
    }
    void Start()
    {
        dialogueController.dialogueDataSO = startedDialogueData;
    }

    void Update()
    {
        if (isStarted)
        {
            if (!isCompleted)   //开始任务，但是没完成
            {
                dialogueController.dialogueDataSO = progressedDialogueData;
            }
            else                //开始任务，并且已经完成
            {
                dialogueController.dialogueDataSO = completedDialogueData;
            }
        }
        if (isFinished)
        {
            dialogueController.dialogueDataSO = finishedDialogueData;
        }
    }

}
