using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class DialogueUI : Singleton<DialogueUI>
{
    [Header("UI组件")]
    public TextMeshProUGUI dialogueText;
    public Image characterImage;
    public Button nextButton;
    public GameObject dialoguePanel;
    [Header("OptionUI相关")]
    public GameObject optionButtonPrefab;
    public Transform optionPanel;
    [Header("对话数据")]
    public DialogueData_SO currentDialogueDataSO;
    public int currentDialogueIndex = 0;

    protected override void Awake()
    {
        base.Awake();
        nextButton.onClick.AddListener(OnNextButtonClicked);  
    }

    //点击NextButton时的处理逻辑
    private void OnNextButtonClicked()
    {
        // 若是最后一句 则关闭 对话UI面板
        if(currentDialogueIndex >= currentDialogueDataSO.dialoguePieces.Count)
        {
            dialoguePanel.SetActive(false);   
            Debug.Log("对话结束");
            return;
        }
        UpdateMainUI(currentDialogueDataSO.dialoguePieces[currentDialogueIndex]);
    }

    // 保存传递进来的人物对话数据
    public void UpdateDialogueData(DialogueData_SO newDialogueDataSO)
    {
        currentDialogueDataSO = newDialogueDataSO;
        currentDialogueIndex = 0;
    }

    //根据每一条对话数据更新UI显示
    public void UpdateMainUI(DialoguePiece dialoguePiece)
    {
        if(dialoguePiece == null)
        {
            dialoguePanel.SetActive(false);
            Debug.Log("目前传入的单条对话数据为空，不刷新UI");
            return;
        }
        dialoguePanel.SetActive(true);
        if(dialoguePiece.sprite != null)
        {
            characterImage.sprite = dialoguePiece.sprite;
            characterImage.gameObject.SetActive(true);
        }
        else characterImage.gameObject.SetActive(false);
        dialogueText.text = "";
        dialogueText.text = dialoguePiece.text;
        //NextButton的显示与否由是否存在选项来决定
        if(dialoguePiece.options.Count == 0 && currentDialogueDataSO.dialoguePieces.Count>0)
        {
            nextButton.transform.GetChild(0).gameObject.SetActive(true);
            nextButton.interactable = true;
            nextButton.gameObject.SetActive(true);
            currentDialogueIndex++;
        }
        else
        {
            nextButton.interactable = false;
            nextButton.transform.GetChild(0).gameObject.SetActive(false);
        }
        // 清空所有已有选项
        foreach(Transform child in optionPanel)
        {
            Destroy(child.gameObject);
        }
        //生成新的选项
        for(int i = 0; i < dialoguePiece.options.Count; i++)
        {
            OptionUI newOption = Instantiate(optionButtonPrefab, optionPanel).GetComponent<OptionUI>();
            newOption.UpdateOptionUI(dialoguePiece.options[i], dialoguePiece);
        }
    }
}
