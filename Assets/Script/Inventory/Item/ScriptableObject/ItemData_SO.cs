using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType{Useable,Weapon,Armor}
[CreateAssetMenu(menuName ="Inventory/New Item Data")]
public class ItemData_SO : ScriptableObject
{
    //物品的唯一ID
    public string itemID;
    //物品的分类
    public ItemType itemType ;
    //物品名称
    public string itemName;
    //物品的图标
    public Sprite itemIcon;
    //物品拾取数量
    public int itemAmount;
    //物品简介
    [TextArea]
    public string itemDescription = "";
    // 是否可堆叠
    public bool Stackable;
    [Header("Weapon")]
    public GameObject weaponPrefab;
    [Header("Useable")]
    public UseableItemData_SO useableItemData;
}
