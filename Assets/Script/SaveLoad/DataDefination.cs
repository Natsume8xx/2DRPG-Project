using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 数据描述，定义生成GUID
public class DataDefination : MonoBehaviour
{
    public PresistentType presistentType;
    public string ID;

    public void OnValidate()
    {
        if(presistentType == PresistentType.DoNotPresistent)
        {
            ID = string.Empty;
            return;
        }
        if (string.IsNullOrEmpty(ID))
        {
            ID = System.Guid.NewGuid().ToString();
        }
    }
}
