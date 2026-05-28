using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickUp : MonoBehaviour
{
    public ItemData_SO itemData;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            //添加到背包 并且刷新
            Debug.Log($"bag is null? {InventoryManager.Instance.bag == null}");
            InventoryManager.Instance.bag.AddItem(itemData,itemData.itemAmount);
            InventoryManager.Instance.containerBagUI.RefreshUI();
            Debug.Log("捡到了");
            // 检查是否完成了任务
            QuestManager.Instance.SetUpQuestProgress(itemData.itemName, itemData.itemAmount);
            // 删除当前场景中该物体
            Destroy(gameObject);
        }

    }
}
