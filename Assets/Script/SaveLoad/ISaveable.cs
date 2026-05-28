using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISaveable 
{
    DataDefination GetDataID();
    //注册和注销保存数据
    void RegisterSaveData()=> DataManager.Instance.RegisterSaveable(this);
    void UnRegisterSaveData()=> DataManager.Instance.UnRegisterSaveable(this);

    //保存和加载数据
    void GetSaveData(Data data);
    void LoadSaveData(Data data);

}
