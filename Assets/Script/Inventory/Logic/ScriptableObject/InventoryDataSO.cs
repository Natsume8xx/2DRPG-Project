using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/New Inventory Data")]
public class InventoryDataSO : ScriptableObject
{
    //该存储空间的类型
    public InventoryType inventoryType;
    public List<InventoryItem> items = new List<InventoryItem>();

    // 添加物品的函数
    public void AddItem(ItemData_SO itemData,int itemAmount)
    {
        //根据物体是否可堆叠进行分类处理
        if (itemData.Stackable)
        {
            foreach(var item in items)
            {
                if(item.inventoryDataSO == itemData)
                {
                    item.amount += itemAmount;
                    return;
                }
            }
            //目前背包中没有这个物品
            int index = 0;
            for(int i = 0; i < items.Count; i++)
            {
                if(items[i].inventoryDataSO != null){
                    index++;
                }
                else
                {
                    items[index].inventoryDataSO = itemData;
                    items[index].amount = itemAmount;
                    return;
                }
            }
        }
        else
        {
            int index = 0;
            for(int i = 0; i < items.Count; i++)
            {
                if(items[i].inventoryDataSO != null){
                    index++;
                }
                else
                {
                    items[index].inventoryDataSO = itemData;
                    items[index].amount = 1;
                    return;
                }
            }
        }
    }    
}

[System.Serializable]
public class InventoryItem
{
    public ItemData_SO inventoryDataSO; // 物品SO数据
    public int amount;
}
