using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueData", menuName = "DialogueData/New DialogueData")]
public class DialogueData_SO : ScriptableObject
{
    public List<DialoguePiece> dialoguePieces = new List<DialoguePiece>();
    public Dictionary<string, DialoguePiece> dialoguePieceDict = new Dictionary<string, DialoguePiece>();

#if UNITY_EDITOR  //在编辑器模式下 通过OnValidate方法来维护 dialoguePieceDict 的正确性
    void OnValidate()  
    {
        dialoguePieceDict.Clear();
        foreach (DialoguePiece piece in dialoguePieces)
        {
            if (!dialoguePieceDict.ContainsKey(piece.ID))
                dialoguePieceDict.Add(piece.ID, piece);
            else Debug.LogWarning($"DialogueData_SO 中存在重复的 DialoguePiece ID: {piece.ID}");
        }
    }
#else  //防止打包后 dialoguePieceDict 没有被正确初始化 导致运行时错误 通过 Awake 来初始化 dialoguePieceDict
    void Awake()
    {
        dialoguePieceDict.Clear();
        foreach (DialoguePiece piece in dialoguePieces)
        {
            if (!dialoguePieceDict.ContainsKey(piece.ID))
                dialoguePieceDict.Add(piece.ID, piece);
            else Debug.LogWarning($"DialogueData_SO 中存在重复的 DialoguePiece ID: {piece.ID}");
        }
    }
#endif

    public QuestData_SO GetQuest()
    {
        foreach(var dia in dialoguePieces)
        {
            if(dia.relatedQuest != null)
                return dia.relatedQuest;
        }
        return null;
    }
}