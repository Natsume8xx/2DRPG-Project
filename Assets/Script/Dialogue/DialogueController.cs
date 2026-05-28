using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueController : MonoBehaviour
{
    public DialogueData_SO dialogueDataSO;
    public bool canTalk = false;

#region 对话的触发与结束
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player")&&dialogueDataSO!=null)
            canTalk = true;
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
           DialogueUI.Instance.dialoguePanel.gameObject.SetActive(false);
           canTalk = false;
        }
    }
#endregion

    // 开始对话
    public void StartDialogue()
    {
        //根据canTalk 的状态 修改 标签
        if(dialogueDataSO == null || canTalk == false)
            return;
        //打开UI面板 传输对话数据
        DialogueUI.Instance.UpdateDialogueData(dialogueDataSO);
        DialogueUI.Instance.UpdateMainUI(dialogueDataSO.dialoguePieces[0]);
    }
    
}
