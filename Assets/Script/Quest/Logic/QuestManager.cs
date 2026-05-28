using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class QuestManager : Singleton<QuestManager>,ISaveable
{
    public List<QuestTask> questTasks = new List<QuestTask>();
    public List<QuestData_SO> allQuestDataSO = new List<QuestData_SO>();
    void OnEnable()
    {
        ISaveable saveable = this;
        saveable.RegisterSaveData();
    }
    void OnDisable()
    {
        ISaveable saveable = this;
        saveable.UnRegisterSaveData();
    }

    //添加任务时检查是否重复
    public bool HaveQuest(QuestData_SO questData)
    {
        if(questData != null)
            return questTasks.Any(q => q.questData.questName == questData.questName);
        return false;
    }

    // 获取指定的任务（QuestTask类型）
    public QuestTask GetQuestTask(QuestData_SO questData)
    {
        if(questData != null)
            return questTasks.Find(q => q.questData.questName == questData.questName);
        return null;
    }

    // 更新任务进度
    public void SetUpQuestProgress(string requireName, int amount)
    {
        foreach(var task in questTasks)
        {
            if(task.isFinished)
                continue; //如果任务已经完成并交付，就不再更新进度了
            var require = task.questData.questRequires.Find(r => r.requireName == requireName);
            if(require != null)
            {
                require.currentAmount += amount;
                task.questData.CheckIfComplete();
            }

        }
    }
    #region 任务数据的保存与读取

    public QuestData_SO GetQuestData_SO(string quest_Name)
    {
        return allQuestDataSO.Find(q => q.questName == quest_Name);
    }

    public DataDefination GetDataID()
    {
        throw new System.NotImplementedException();
    }

    public void GetSaveData(Data data)
    {
        DataManager.Instance.saveData.SaveQuestData();
    }

    public void LoadSaveData(Data data)
    {
        DataManager.Instance.saveData.ReadQuestDataFromSave();
    }

    #endregion


}
// 对任务进行进一步抽象
    [System.Serializable]
    public class QuestTask
    {
        public QuestData_SO questData;
        public bool isStarted{get{return questData.isAccepted;} set{questData.isAccepted = value;}}
        public bool isCompleted{get{return questData.isCompleted;} set{questData.isCompleted = value;}}
        public bool isFinished{get{return questData.isFinished;} set{questData.isFinished = value;}}  //任务完成后是否已经交付
    }
