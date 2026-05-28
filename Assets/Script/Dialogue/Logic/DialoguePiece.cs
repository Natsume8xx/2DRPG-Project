using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialoguePiece 
{
    public string ID;
    public int indexID;
    public Sprite sprite;
    [HideInInspector]
    public bool canExpand;
    public QuestData_SO relatedQuest;  //关联的任务数据 用于在对话中触发任务状态变化
    [TextArea]
    public string text;
    public List<DialogueOption> options = new List<DialogueOption>();
}
