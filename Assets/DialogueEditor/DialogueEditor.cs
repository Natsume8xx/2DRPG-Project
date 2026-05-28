using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditorInternal;
using System;
using System.IO;

public class DialogueEditor : EditorWindow
{
    Vector2 scrollPos = Vector2.zero;
    DialogueData_SO currentDialogueData;
    ReorderableList dialoguePieceList = null;
    Dictionary<string,ReorderableList> optionListDic = new Dictionary<string, ReorderableList>();  //用来存储每个DialoguePiece对应的选项列表  key是 DialoguePiece 的ID和文本内容的组合
    [MenuItem("Natsume/Dialogue Editor")]
    // 初始化，在被创建的时候就会调用一次
    public static void Init()
    {
        DialogueEditor dialogueWindow = GetWindow<DialogueEditor>();
        dialogueWindow.autoRepaintOnSceneChange = true;  //当场景发生改变时自动重绘窗口
    }

    //根据传入的 DialogueData_SO 来生成窗口，可供外部调用
    public static void InitWindow(DialogueData_SO dialogueData)
    {
        DialogueEditor dialogueWindow = GetWindow<DialogueEditor>();
        dialogueWindow.currentDialogueData = dialogueData;
    }

    [OnOpenAsset]
    //当我们在Project窗口中双击一个 DialogueData_SO 资源时，这个方法会被调用，来创建一个editor窗口
    public static bool OpenAsset(int instanceID, int line)
    {
        DialogueData_SO dialogueData_SO = EditorUtility.InstanceIDToObject(instanceID) as DialogueData_SO;
        if(dialogueData_SO!= null)
        {
            DialogueEditor.InitWindow(dialogueData_SO);
            return true;  //返回true表示我们已经处理了这个打开事件，Unity就不会再进行默认的打开操作了
        }
        return false;  //返回false表示我们没有处理这个事件，Unity会继续执行默认的打开操作
    }

    // 当选中文件切换时调用一次
    void OnSelectionChange()
    {
        var newData = Selection.activeContext as DialogueData_SO;
        if(newData != null)
        {
            currentDialogueData = newData;
            SetUpRecorderableList();  //当选中文件切换时 更新列表显示
        }
        else
        {
            currentDialogueData = null;
            dialoguePieceList = null;  //当没有选中 DialogueData_SO 文件时 把列表置空
            optionListDic.Clear();  //清空选项列表字典
        }
        Repaint();  //刷新窗口显示
    }

    // 在窗口中绘制UI.  相当于Updated函数
    void OnGUI()
    {
        if(currentDialogueData != null)
        {
            EditorGUILayout.TextField("Dialogue Name:  "+currentDialogueData.name,EditorStyles.boldLabel);
            GUILayout.Space(10);
            // 绘制滑动条
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos,GUILayout.ExpandWidth(true),GUILayout.ExpandHeight(true));
            if(dialoguePieceList ==null)
                SetUpRecorderableList();
            dialoguePieceList.DoLayoutList();  //把列表绘制出来
            EditorGUILayout.EndScrollView();
        }
        else
        {
            // 根据按钮点击创建新的文件
            if(GUILayout.Button("Create New Dialogue Data"))
            {
                string dataPath = "Assets/DateSO/DialogueData/";
                // 如果路径不存在就创建
                if(!Directory.Exists(dataPath))
                    Directory.CreateDirectory(dataPath);
                // 生成一个新的 DialogueData_SO 文件
                DialogueData_SO newData = ScriptableObject.CreateInstance<DialogueData_SO>();
                AssetDatabase.CreateAsset(newData,dataPath+"/"+"NewDialogueData.asset");
                currentDialogueData = newData;
            }
            GUILayout.Label("No Dialogue Data Selected",EditorStyles.boldLabel);
        }
    }

    // 当窗口被关闭时调用  用来清理资源
    void OnDisable()
    {
        optionListDic.Clear();  //当窗口被关闭时 清空选项列表字典 释放资源
    }

    // 更新RecorderableList UI 显示
    public void SetUpRecorderableList()
    {
        dialoguePieceList = new ReorderableList(currentDialogueData.dialoguePieces,typeof(DialoguePiece),true,true,true,true);
        dialoguePieceList.drawHeaderCallback += OnDrawPieceHeader;
        dialoguePieceList.drawElementCallback += OnDrawElement;
        dialoguePieceList.elementHeightCallback += OnElementHeightChanged;
    }

#region 绘制 ReorderableList 的回调函数
    // 绘制 PieceList 的头部UI 的回调函数
    private void OnDrawPieceHeader(Rect rect)
    {
        GUI.Label(rect,"Dialogue Pieces",EditorStyles.boldLabel);
    }

    // 计算 PieceList 中每个元素的高度的回调函数  因为每个元素的UI可能不一样 所以需要动态计算高度
    private float OnElementHeightChanged(int index)
    {
        float height = EditorGUIUtility.singleLineHeight;
        if(currentDialogueData.dialoguePieces[index].canExpand){
            height = EditorGUIUtility.singleLineHeight*10;
            if(currentDialogueData.dialoguePieces[index].options.Count > 0)
            {
                height += EditorGUIUtility.singleLineHeight * currentDialogueData.dialoguePieces[index].options.Count;  //每个选项占用一行的高度
            }
        }
        return height;
    }

    // 绘制 PieceList 中每个元素的UI 的回调函数  包含绑定数据
    // 在函数内只需要实现一个元素UI的绘制，因为这个函数会被多次调用 来绘制列表中的每个元素
    private void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        EditorUtility.SetDirty(currentDialogueData);  //标记当前数据被修改了 以便Unity知道需要保存数据和刷新UI
        GUIStyle style = new GUIStyle("TextField");
        if(index < currentDialogueData.dialoguePieces.Count)
        {
            var piece = currentDialogueData.dialoguePieces[index];
            var tempRect = rect;
            // 判断是否被折叠
            currentDialogueData.dialoguePieces[index].canExpand = EditorGUI.Foldout(tempRect, piece.canExpand, piece.ID);
            if(currentDialogueData.dialoguePieces[index].canExpand){
                tempRect.y += EditorGUIUtility.singleLineHeight + 5;  //折叠后UI的起始位置
                // 绘制ID
                tempRect.height = EditorGUIUtility.singleLineHeight;
                tempRect.width = 30;
                EditorGUI.LabelField(tempRect,"ID:");   //不用EditorGUILayout 是因为要在一行中设置位置
                tempRect.x += tempRect.width;
                tempRect.width = 100;
                piece.ID = EditorGUI.TextField(tempRect,piece.ID);

                //绘制IndexID
                tempRect.x += tempRect.width;
                tempRect.width = 50;
                EditorGUI.LabelField(tempRect,"Index:");   //不用EditorGUILayout 是因为要在一行中设置位置
                tempRect.x += tempRect.width;
                tempRect.width = 100;
                piece.indexID = EditorGUI.IntField(tempRect,piece.indexID);

                // 绘制关联任务数据
                tempRect.x += tempRect.width;
                tempRect.width = 90;
                EditorGUI.LabelField(tempRect,"Related Quest:");
                tempRect.x += tempRect.width;
                tempRect.width = 90;
                piece.relatedQuest = (QuestData_SO)EditorGUI.ObjectField(tempRect,piece.relatedQuest,typeof(QuestData_SO),false);

                // 换行 
                tempRect.y += EditorGUIUtility.singleLineHeight + 10;
                tempRect.x = rect.x;
                
                // 绘制 图片选项UI
                tempRect.width = 85;
                EditorGUI.LabelField(tempRect,"Image Sprite:");
                tempRect.x += tempRect.width;
                tempRect.width = 60;
                tempRect.height = 60;
                piece.sprite = (Sprite)EditorGUI.ObjectField(tempRect,piece.sprite,typeof(Sprite),false);

                // 绘制文本内容UI
                tempRect.x += tempRect.width + 10;
                tempRect.width = 30;
                EditorGUI.LabelField(tempRect,"Text:");
                tempRect.x += tempRect.width;
                tempRect.width = 300;
                tempRect.height = EditorGUIUtility.singleLineHeight*4;
                style.wordWrap = true;
                piece.text = EditorGUI.TextField(tempRect,piece.text,style);

                // 绘制选项列表UI
                tempRect.y += tempRect.height + 10;
                tempRect.x = rect.x;
                tempRect.width = rect.width;

                string optionListKey = piece.ID + piece.text;  //用ID和文本内容的组合作为选项列表的唯一标识符
                if(optionListKey != string.Empty)
                {
                    if (!optionListDic.ContainsKey(optionListKey))
                    {
                        // 生成列表
                        var optionList = new ReorderableList(piece.options,typeof(DialogueOption),true,true,true,true);
                        // 绘制列表中每一个选项UI
                        optionList.drawHeaderCallback = OnDrawOptionListHeader;
                        optionList.drawElementCallback = (optionRect,optionIndex,optionActive,optionFocused) =>
                        {
                            OnDrawOptionElement(piece,optionRect,optionIndex,optionActive,optionFocused);
                        };
                        optionListDic[optionListKey] = optionList;
                    }
                    optionListDic[optionListKey].DoList(tempRect);  //把选项列表绘制出来
                }
            }
        }
    }
#endregion

#region 绘制 选项列表 的回调函数
    // 绘制 选项列表 的头部UI 的回调函数
    private void OnDrawOptionListHeader(Rect rect)
    {
        var tempRect = rect;
        tempRect.width = rect.width*0.8f;
        EditorGUI.LabelField(tempRect,"Option Text",EditorStyles.boldLabel);
        tempRect.x += tempRect.width + 5;
        tempRect.width = rect.width*0.10f;
        EditorGUI.LabelField(tempRect,"Next ID",EditorStyles.boldLabel);
        tempRect.x += tempRect.width + 10;
        EditorGUI.LabelField(tempRect,"Take?",EditorStyles.boldLabel);
    }

    // 绘制 一个 选项UI 的回调函数
    private void OnDrawOptionElement(DialoguePiece piece, Rect optionRect, int optionIndex, bool optionActive, bool optionFocused)
    {
        var option = piece.options[optionIndex];
        var tempRect = optionRect;
        tempRect.height = EditorGUIUtility.singleLineHeight;
        // 绘制选项文本UI
        tempRect.width = optionRect.width*0.8f;
        option.optionText = EditorGUI.TextField(tempRect,option.optionText);
        // 绘制选项关联的对话ID UI
        tempRect.x += tempRect.width + 5;
        tempRect.width = optionRect.width*0.10f;
        option.nextDialogueID = EditorGUI.TextField(tempRect,option.nextDialogueID);
        // 绘制选项是否接受任务的UI
        tempRect.x += tempRect.width + 10;
        option.takeQuest = EditorGUI.Toggle(tempRect,option.takeQuest); 
    }
#endregion

}
