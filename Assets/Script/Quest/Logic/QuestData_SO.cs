using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "New Quest Data", menuName = "Quest/QuestData_SO")]
public class QuestData_SO : ScriptableObject
{
    public string questName;
    [TextArea]
    public string questDescription;
    [Header("任务状态")]
    public bool isAccepted;
    public bool isCompleted;
    public bool isFinished;
    [Header("任务目标")]
    public List<QuestRequire> questRequires = new List<QuestRequire>();
    [Header("任务奖励")]
    public List<InventoryItem> rewards = new List<InventoryItem>();

    // 查看任务是否已经完成
    public void CheckIfComplete()
    {
        var check = questRequires.Where(q => q.currentAmount>= q.requireAmount);
        isCompleted = check.Count() == questRequires.Count;
        Debug.Log($"任务{questName}完成状态：{isCompleted}");
    }

    // 返回任务所需要  收集/毁灭 的物品 的名称列表
    public List<string> GetRequireItemNamesList()
    {
        List<string> ls = new List<string>();
        foreach(var require in questRequires)
        {
            ls.Add(require.requireName);
        }
        return ls;
    }

    // 任务完成后给予奖励
    public void GiveRewards()
    {
        foreach(var item in rewards)
        {
            // 数量大于零，是奖励，添加到背包  //TODO： 可以优化 背包满了怎么办
            if(item.amount >0)
                InventoryManager.Instance.bag.AddItem(item.inventoryDataSO, item.amount);
            else//数量小于零，代表是要交付的物品,需要从背包和物品栏中扣除
            {
                int requireAmount = -item.amount; //需要交付的物品数量
                var bagItem = InventoryManager.Instance.GetItemInBag(item.inventoryDataSO);
                var actionBarItem = InventoryManager.Instance.GetItemInActionBar(item.inventoryDataSO);
                // 先判断背包里面有没有
                if (bagItem != null)
                {
                    //Debug.Log($"背包中有{item.inventoryDataSO.itemName}  "+"数量："+bagItem.amount);
                    if(bagItem.amount <= requireAmount) // 背包中不够或者刚好够
                    {
                        requireAmount -= bagItem.amount;
                        bagItem.amount = 0;
                        if(actionBarItem!=null)
                            actionBarItem.amount -= requireAmount;
                    }else //背包中足够
                    {
                        bagItem.amount -= requireAmount;
                    }
                }else //actionbar 中有，直接扣除
                {
                    actionBarItem.amount -= requireAmount;
                }

            }
        }
        // 刷新 背包 物品栏 UI
        InventoryManager.Instance.containerBagUI.RefreshUI();
        InventoryManager.Instance.containerActionBarUI.RefreshUI();
        // 切换任务状态
        isAccepted = false;
        isFinished = true;
    }
}
[System.Serializable]
    public class QuestRequire
    {
        public string requireName;
        public int requireAmount;
        public int currentAmount;
    }
