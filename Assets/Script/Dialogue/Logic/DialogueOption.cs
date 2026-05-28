using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueOption 
{
    [TextArea]
    public string optionText;
    public string nextDialogueID;
    public bool takeQuest;
}
