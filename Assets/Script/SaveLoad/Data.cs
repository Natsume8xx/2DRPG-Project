using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

public class Data 
{
    // 保存任务数据
    public Dictionary<string,QuestDataSave> questDataDic = new Dictionary<string, QuestDataSave>();

    //保存背包数据
    public Dictionary<InventoryType,InventorySaveBlock> inventorySaveBlocks = new Dictionary<InventoryType, InventorySaveBlock>();
    
    //保存物体位置
    //Vector3无法被序列化保存，所以使用SerilizeVector3类进行拆分存储，后续重建
    //该字典的添加方法直接写在了Character类的GetSaveData方法中，因为要获取他们自己的ID，所以让他们自己添加比较方便
    public Dictionary<string,SerilizeVector3> characterPosDic = new Dictionary<string, SerilizeVector3>();
    
    //存储所有 float 类型 的数据 （血量，能力条）
    public Dictionary<string,float> floatDataDic = new Dictionary<string, float>();
    
    //保存加载的场景
    public string sceneToSave;

    //保存场景（当时写的时候使用了JsonUtility，改成Newtonsoft.Json也行，不过方便一些）
    public void SaveGameScene(GameSceneSO gameSceneSO)
    {
        sceneToSave = JsonUtility.ToJson(gameSceneSO); 
        Debug.Log($"保存场景{sceneToSave}完毕！");
    }

    //加载保存的场景
    public GameSceneSO GetSavedScene()
    {
        var savedScene = ScriptableObject.CreateInstance<GameSceneSO>();
        JsonUtility.FromJsonOverwrite(sceneToSave,savedScene);
        return savedScene;
    }

#region 背包数据的存储映射
    //添加存储空间数据
    public void AddInventorySaveBlock(InventoryDataSO inventoryDataSO)
    {
        if(!inventorySaveBlocks.ContainsKey(inventoryDataSO.inventoryType))
        {
            InventorySaveBlock newBlock = new InventorySaveBlock(inventoryDataSO.inventoryType);
            inventorySaveBlocks.Add(newBlock.inventoryType, newBlock);
        }
    }

    //设置每个InventorySaveBlock中列表slotSaves的数据
    public void SetInventorySaveBlock()
    {
        foreach(var block in inventorySaveBlocks)
        {
            //先清空slotSaves列表中的数据
            block.Value.slotSaves.Clear();
            InventoryDataSO inventoryDataSO = InventoryManager.Instance.GetInventoryDataSOByType(block.Value.inventoryType);
            for(int i = 0; i < inventoryDataSO.items.Count; i++)
            {
                var item = inventoryDataSO.items[i];
                if(item.inventoryDataSO == null)
                {
                    continue;
                }
                // 转化为可序列化的类型 InventorySlotSave
                InventorySlotSave slotSave = new InventorySlotSave(item.inventoryDataSO.itemID,
                item.amount,i);
                block.Value.slotSaves.Add(slotSave);
            }
        }
    }
#endregion

#region 背包数据的读取方法
    //通过InventorySaveBlock中的数据恢复InventoryDataSO中items列表的数据
    public void GetInventoryDataFromSaveBlock()
    {
        foreach(var block in inventorySaveBlocks)
        {
            InventoryDataSO inventoryDataSO = InventoryManager.Instance.GetInventoryDataSOByType(block.Value.inventoryType);
            //清空对应items列表中的数据,注意不能使用clear方法，因为items列表中的元素是预设中设置好的，不能被删除，只能把它们的值重置
            inventoryDataSO.items.ForEach(item=>{item.inventoryDataSO = null;item.amount = 0;});
            foreach(var slotSave in block.Value.slotSaves)
            {
                inventoryDataSO.items[slotSave.slotIndex].inventoryDataSO = InventoryManager.Instance.GetItemDataByID(slotSave.itemID);
                inventoryDataSO.items[slotSave.slotIndex].amount = slotSave.itemAmount;
            }
        }
    }

    //新游戏清空背包数据
    public void ClearInventoryData()
    {
        foreach(var block in inventorySaveBlocks)
        {
            InventoryDataSO inventoryDataSO = InventoryManager.Instance.GetInventoryDataSOByType(block.Value.inventoryType);
            //清空对应items列表中的数据,注意不能使用clear方法，因为items列表中的元素是预设中设置好的，不能被删除，只能把它们的值重置
            inventoryDataSO.items.ForEach(item=>{item.inventoryDataSO = null;item.amount = 0;});
            block.Value.slotSaves.Clear();
        }
    }
#endregion

#region 任务数据的保存和读取
    //保存任务数据
    public void SaveQuestData()
    {
        //先记得 清空 字典
        questDataDic.Clear();
        foreach(var quest in QuestManager.Instance.questTasks)
        {
            if(quest.questData!= null && !questDataDic.ContainsKey(quest.questData.questName))
            {
                questDataDic.Add(quest.questData.questName, new QuestDataSave(quest.questData));
            }
        }
    }
    //读取任务数据
    public void ReadQuestDataFromSave()
    {
        // 先记得清空 QuestManager中的任务列表
        QuestManager.Instance.questTasks.Clear();
        // 遍历字典中的数据，转换成QuestTask类型，并添加到QuestManager的任务列表中
        foreach(var questData in questDataDic)
        {
            QuestTask questTask = questData.Value.ToQuestTask();
            if(questTask != null)
                QuestManager.Instance.questTasks.Add(questTask);
        }
    }
#endregion

}

// Newtonsoft.Json 无法直接将Vector3类型进行序列化存储，所以拆分存储，后续重建
public class SerilizeVector3
{
    public float x,y,z;
    public SerilizeVector3(Vector3 pos)
    {
        this.x = pos.x;
        this.y = pos.y;
        this.z = pos.z;
    }
    // 将SerilizeVector3转换成Vector3类型，方便使用
    public Vector3 returnVector3()
    {
        return new Vector3(x,y,z);
    }
}

//保存单个单元格中数据的快照类 -> 具体的物品
public class InventorySlotSave
{
    public string itemID;
    public int itemAmount;
    public int slotIndex;
    public InventorySlotSave(string itemID,int itemAmount,int slotIndex)
    {
        this.itemID = itemID;
        this.itemAmount = itemAmount;
        this.slotIndex = slotIndex;
    }
}

//保存整个存储空间数据的快照类 -> 背包，装备栏，快捷栏
public class InventorySaveBlock
{
    //public InventoryDataSO inventoryDataSO; //SO类型无法序列化存储
    public InventoryType inventoryType;
    public List<InventorySlotSave> slotSaves = new List<InventorySlotSave>();
    public InventorySaveBlock(InventoryType inventoryType)
    {
        //this.inventoryDataSO = inventoryDataSO;
        this.inventoryType = inventoryType;
    }
}

// 任务数据快照类
public class QuestDataSave
{
    public string questNameSave;
    public bool isAcceptedSave;
    public bool isCompletedSave;
    public bool isFinishedSave;
    public List<QuestRequire> questRequires = new List<QuestRequire>();
    public QuestDataSave(){} //无参构造函数，方便反序列化
    public QuestDataSave(QuestData_SO questDataSO)
    {
        this.questNameSave = questDataSO.questName;
        this.isAcceptedSave = questDataSO.isAccepted;
        this.isCompletedSave = questDataSO.isCompleted;
        this.isFinishedSave = questDataSO.isFinished;
        this.questRequires = CopyQuestRequireList(questDataSO.questRequires);
    }
    // 将QuestDataSave转换成QuestTask类型，方便恢复到QuestManager中
    public QuestTask ToQuestTask()
    {
        QuestData_SO questData = QuestManager.Instance.GetQuestData_SO(this.questNameSave);
        if(questData != null)
        {
            questData.isAccepted = this.isAcceptedSave;
            questData.isCompleted = this.isCompletedSave;
            questData.isFinished = this.isFinishedSave;
            questData.questRequires = this.questRequires;
            return new QuestTask(){questData = questData};
        }
        else
        {
            Debug.LogError($"在QuestManager中没有找到任务数据SO，任务名称：{this.questNameSave}");
            return null;
        }
    }

    // 由于QuestRequire类中没有引用类型的成员，所以可以直接进行浅复制，如果有引用类型的成员，则需要进行深复制
    public List<QuestRequire> CopyQuestRequireList(List<QuestRequire> originalList)
    {
        List<QuestRequire> newList = new List<QuestRequire>();
        foreach(var require in originalList)
        {
            QuestRequire newRequire = new QuestRequire();
            newRequire.requireName = require.requireName;
            newRequire.requireAmount = require.requireAmount;
            newRequire.currentAmount = require.currentAmount;
            newList.Add(newRequire);
        }
        return newList;
    }
}


