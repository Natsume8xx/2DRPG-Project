using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerUI : MonoBehaviour
{
    public SlotHolder[] slotHolders;
    public void OnEnable()
    {
        RefreshUI();
    }
    //为 slotHolders 数组中每一个数据进行索引的设置
    public void RefreshUI()
    {
        for(int i = 0; i < slotHolders.Length; i++)
        {
            slotHolders[i].itemUI.Index = i;
            //Debug.Log("这个物体的索引是"+i);
            slotHolders[i].UpdateItem();
        }
    }
}
