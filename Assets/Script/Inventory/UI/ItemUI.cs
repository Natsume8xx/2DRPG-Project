using System.Collections;
using System.Collections.Generic;
using TMPro;

//using System.Drawing;
using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    public Image iconUI = null;
    public TextMeshProUGUI amountUI = null;
    public ItemData_SO itemDataSO;

    //未来存储空间的引用
    public InventoryDataSO Store{get;set;}
    //存储空间的索引
    public int Index{get;set;} = -1;

    //根据传入的数据设置单元格的贴图以及物品数量
    public void SetUpItemUI(ItemData_SO itemData,int amount)
    {
        // 如果物品被消耗到数量为0，则清空索引中数据，并且不显示图标
        if(amount == 0)
        {
            Store.items[Index].inventoryDataSO = null;
            Store.items[Index].amount = 0;
            iconUI.gameObject.SetActive(false);
            return;
        }
        if(amount < 0)
            itemData = null;

        if(itemData != null)
        {
            itemDataSO = itemData;
            iconUI.sprite = itemData.itemIcon;
            amountUI.text = amount.ToString();
            //激活显示
            iconUI.gameObject.SetActive(true);
            //Debug.Log("不为空，激活！");
        }
        else
        {
            iconUI.gameObject.SetActive(false);
        }
    }

    /// 根据当前的索引和存储空间获取物品数据
    public InventoryItem GetInventoryItem()
    {
        return Store.items[Index];
    }
}
