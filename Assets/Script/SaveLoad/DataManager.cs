using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Newtonsoft.Json;
using System.IO;

[DefaultExecutionOrder(-100)]   //确保优先执行
public class DataManager : MonoBehaviour
{
    //单例模式,确保全局只有一个DataManager实例
    public static DataManager Instance;
    //注册了数据管理的物体的列表
    public List<ISaveable> saveables = new List<ISaveable>();
    public List<ISaveable> laterRegList = new List<ISaveable>();
    public List<ISaveable> laterUnRegList = new List<ISaveable>();
    //存储数据类
    public Data saveData;
    //判断是否在注册注销的处理中
    public bool isProcessing = false;
    [Header("事件监听")]
    public VoidEventSO saveDataEvent;
    public VoidEventSO loadDataEvent;

    private string jsonFolder;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        saveData = new Data();
        jsonFolder = Application.persistentDataPath + "/SAVE DATA/";
        ReadSaveData();
    }
    void OnEnable()
    {
        saveDataEvent.OnEventRaised += Save;
        loadDataEvent.OnEventRaised += Load;
    }
    void Update()
    {
        if (Keyboard.current.nKey.wasPressedThisFrame)
        {
            Load();
        }
    }
    void OnDisable()
    {
        saveDataEvent.OnEventRaised -= Save;
        loadDataEvent.OnEventRaised -= Load;
    }

    //观察者模式,为实现接口的数据类提供注册和注销方法
    public void RegisterSaveable(ISaveable saveable)
    {
        if(!isProcessing && !saveables.Contains(saveable)){
            saveables.Add(saveable);
        }else if(isProcessing && !laterRegList.Contains(saveable)){
            laterRegList.Add(saveable);
            // 如果此前计划在延迟注销中移除，则撤销该注销
            if (laterUnRegList.Contains(saveable))
                laterUnRegList.Remove(saveable);
        }
    }
    public void UnRegisterSaveable(ISaveable saveable)
    {
        if(!isProcessing && saveables.Contains(saveable)){
            saveables.Remove(saveable);
        }else if(isProcessing && !laterUnRegList.Contains(saveable))
        {
            laterUnRegList.Add(saveable);
            // 如果此前计划在延迟注册中添加，则撤销该注册
            if (laterRegList.Contains(saveable))
                laterRegList.Remove(saveable);
        }
    }

    //通知所有注册的物体，执行保存
    public void Save()
    {
        isProcessing = true;
        // 使用快照遍历，避免在枚举期间被修改导致异常
        foreach(var saveable in saveables.ToArray())
        {
            saveable.GetSaveData(saveData);
        }
        //序列化保存数据
        var resultPath = jsonFolder + "data.sav";
        var jsonData = JsonConvert.SerializeObject(saveData);
        if (!File.Exists(resultPath))
        {
            Directory.CreateDirectory(jsonFolder);
        }
        File.WriteAllText(resultPath,jsonData);
        isProcessing = false;
        laterProcessing();
    }
    //通知所有注册的物体，执行加载
    public void Load()
    {
        isProcessing = true;
        // 使用快照遍历，避免在枚举期间被修改导致异常
        foreach(var saveable in saveables.ToArray())
        {
            saveable.LoadSaveData(saveData);
        }
        isProcessing = false;
        laterProcessing();
    }

    //处理延迟队列
    public void laterProcessing()
    {
        // 先批量处理延迟注册（先合并再清空），避免在迭代中修改延迟集合
        if (laterRegList.Count > 0)
        {
            foreach (var save in laterRegList)
            {
                if (!saveables.Contains(save))
                    saveables.Add(save);
            }
            laterRegList.Clear();
        }
        // 再批量处理延迟注销
        if (laterUnRegList.Count > 0)
        {
            foreach (var save in laterUnRegList)
            {
                if (saveables.Contains(save))
                    saveables.Remove(save);
            }
            laterUnRegList.Clear();
        }
    }

    // 反序列化读取Json中的数据
    private void ReadSaveData()
    {
        var resultPath = jsonFolder + "data.sav";
        if (File.Exists(resultPath))
        {
            var stringData = File.ReadAllText(resultPath);
            var jsonData = JsonConvert.DeserializeObject<Data>(stringData);
            saveData = jsonData;
        }
    }
}
