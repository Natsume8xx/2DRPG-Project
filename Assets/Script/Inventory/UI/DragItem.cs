using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ItemUI))]
public class DragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ItemUI currentItemUI;
    public SlotHolder currentSlotHolder;
    public SlotHolder targetSlotHolder;
    void Awake()
    {
        currentItemUI = GetComponent<ItemUI>();
        currentSlotHolder = GetComponentInParent<SlotHolder>();
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        //记录拖拽前的 原始数据
        InventoryManager.Instance.origionDragData = new InventoryManager.dragData();
        InventoryManager.Instance.origionDragData.origionSlotHolder = currentSlotHolder;
        InventoryManager.Instance.origionDragData.origionParentTransform = (RectTransform)transform.parent;
        transform.SetParent(InventoryManager.Instance.dragCanvas.transform,true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        //跟随鼠标位置移动
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 放下物品 交换数据
        // 此时鼠标是否指向UI物体？
        if (EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("指向UI");
            if (InventoryManager.Instance.CheckAll(eventData.position))
            {
                Debug.Log("指向对应背包UI");
                if(eventData.pointerEnter.GetComponent<SlotHolder>())
                    targetSlotHolder = eventData.pointerEnter.GetComponent<SlotHolder>();
                else
                    targetSlotHolder = eventData.pointerEnter.GetComponentInParent<SlotHolder>();
                //根据目标SlotHolder的type 来不同处理
                switch (targetSlotHolder.slotType)
                {
                    case SlotType.BAG:
                        swapItem();
                        Debug.Log("数据交换完成！");
                        break;
                    case SlotType.WEAPON:
                        if(currentItemUI.Store.items[currentItemUI.Index].inventoryDataSO.itemType == ItemType.Weapon)
                        {
                            swapItem();
                            Debug.Log("数据交换完成！");
                        }
                        else
                        {
                            Debug.Log("只能放置武器！");
                        }
                        break;
                    case SlotType.ACTION:
                        if(currentItemUI.Store.items[currentItemUI.Index].inventoryDataSO.itemType == ItemType.Useable)
                        {
                            swapItem();
                            Debug.Log("数据交换完成！");
                        }
                        else
                        {
                            Debug.Log("只能放置消耗品！");
                        }
                        break;
                }
                currentSlotHolder.UpdateItem();
                targetSlotHolder.UpdateItem();
            }
        }
        // 父级 canvas 转变回来
        transform.SetParent(InventoryManager.Instance.origionDragData.origionParentTransform);
        // 再把 RectTransform 改回来
        RectTransform t = transform as RectTransform;
        t.offsetMax = Vector2.one * 0;
        t.offsetMin = Vector2.one * 0;
    }
    //交换数据
    public void swapItem()
    {
        // 获取 双方 InventoryItem 类型 的数据
        //var targetItem = targetSlotHolder.GetComponentInParent<InventoryDataSO>().items[targetSlotHolder.itemUI.Index];
        var targetItem = targetSlotHolder.itemUI.Store.items[targetSlotHolder.itemUI.Index];
        //var tempItem = currentSlotHolder.GetComponentInParent<InventoryDataSO>().items[currentSlotHolder.itemUI.Index];
        var tempItem = currentSlotHolder.itemUI.Store.items[currentSlotHolder.itemUI.Index];
        bool isSame = targetItem.inventoryDataSO==tempItem.inventoryDataSO;
        // 交换数据
        if(isSame && targetItem.inventoryDataSO.Stackable && tempItem.inventoryDataSO.Stackable)
        {
            Debug.Log("数据交互方式：叠加");
            targetItem.amount += tempItem.amount;
            //targetSlotHolder.GetComponentInParent<InventoryDataSO>().items[targetSlotHolder.itemUI.Index] = tempItem;
            currentSlotHolder.itemUI.Store.items[currentSlotHolder.itemUI.Index].inventoryDataSO = null;
            currentSlotHolder.itemUI.Store.items[currentSlotHolder.itemUI.Index].amount = 0;
        }
        else
        {
            Debug.Log("数据交互方式：交换");
            targetSlotHolder.itemUI.Store.items[targetSlotHolder.itemUI.Index] = tempItem;
            currentSlotHolder.itemUI.Store.items[currentSlotHolder.itemUI.Index] = targetItem;
        }
    }
}
