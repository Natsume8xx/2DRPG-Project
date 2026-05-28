using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum SlotType{BAG,WEAPON,ARMOR,ACTION}
public class SlotHolder : MonoBehaviour,IPointerClickHandler,IPointerEnterHandler,IPointerExitHandler
{
    public SlotType slotType;  // 单元格的类型，准确说是所属存储容器的类型
    public ItemUI itemUI;     
    private ItemData_SO currentWeapon;

#region 三个接口的实现，分别是双击使用物品、鼠标悬停显示物品信息、鼠标离开隐藏物品信息
    /// 双击使用物品的操作，目前只实现了可用物品的使用，后续可以根据不同类型的物品添加更多的使用效果
    public void OnPointerClick(PointerEventData eventData)
    {
        if(itemUI.Store == null || itemUI.Index < 0)
        {
            Debug.LogError("SlotHolder的ItemUI没有正确设置Store或者Index");
            return;
        }
        if(eventData.clickCount == 2 && itemUI.GetInventoryItem().inventoryDataSO != null)
        {
            UseItem();
        }
    }

    // 鼠标悬停显示物品信息
    public void OnPointerEnter(PointerEventData eventData)
    {
        if(itemUI.GetInventoryItem().inventoryDataSO != null)
        {
            InventoryManager.Instance.itemTooltip.SetUpTooltip(itemUI.GetInventoryItem().inventoryDataSO);
        }
    }

    // 鼠标离开隐藏物品信息
    public void OnPointerExit(PointerEventData eventData)
    {
        InventoryManager.Instance.itemTooltip.gameObject.SetActive(false);
    }
#endregion


    public void UseItem()
    {
        if(itemUI.GetInventoryItem().inventoryDataSO.itemType == ItemType.Useable && itemUI.GetInventoryItem().amount > 0)
            {
                //使用物品，这里只以回血为例，后续可以添加更多的使用效果
                InventoryManager.Instance.player.GetComponent<Character>().HPRecover(itemUI.GetInventoryItem().inventoryDataSO.useableItemData.hpAmount);
                itemUI.GetInventoryItem().amount--;
                // 更新任务要求 ， 在此demo中，交付任务物品 实现方式是 使用对应数量的背包物品
                QuestManager.Instance.SetUpQuestProgress(itemUI.GetInventoryItem().inventoryDataSO.itemName,-1);
            }
        //刷新UI
        UpdateItem();
    }

    //判断slotType以此来更新UI
    public void UpdateItem()
    {
        switch (slotType)
        {
            case SlotType.BAG:
                itemUI.Store =  InventoryManager.Instance.bag;
                break;
            case SlotType.WEAPON:
                itemUI.Store = InventoryManager.Instance.characterStates;
                // 武器的卸下与装备
                //如果当前单元格内没有武器
                if(itemUI.GetInventoryItem().inventoryDataSO == null)
                {
                    InventoryManager.Instance.equipmentController.UnEquipWeapon();
                    currentWeapon = null;
                    InventoryManager.Instance.RefreshCharacterStates(100, 10);
                    break;
                }
                if(currentWeapon != null && currentWeapon== itemUI.GetInventoryItem().inventoryDataSO){
                    Debug.Log("是同一把武器，不进行任何操作！");
                    break;
                }
                InventoryManager.Instance.equipmentController.UnEquipWeapon();
                if(itemUI.Store.items[itemUI.Index].inventoryDataSO != null && itemUI.Store.items[itemUI.Index].inventoryDataSO.itemType == ItemType.Weapon){
                    //调用EquipmentController 来装备武器
                    InventoryManager.Instance.equipmentController.EquipWeapon(itemUI.Store.items[itemUI.Index].inventoryDataSO);
                    currentWeapon = itemUI.GetInventoryItem().inventoryDataSO;
                    InventoryManager.Instance.RefreshCharacterStates(100, 20);
                }
                break;
            case SlotType.ARMOR:
                itemUI.Store = InventoryManager.Instance.characterStates;
                break;
            case SlotType.ACTION:
                itemUI.Store = InventoryManager.Instance.actionBar;
                break;
        }
        var item = itemUI.Store.items[itemUI.Index];
        itemUI.SetUpItemUI(item.inventoryDataSO,item.amount);
    }

    void OnDisable()
    {
        InventoryManager.Instance.itemTooltip.gameObject.SetActive(false);
    }
}
