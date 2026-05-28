using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentController : MonoBehaviour
{
    [Header("WeaponInstantiatePoint")]
    public Transform weaponInstantiatePoint;
    [Header("Animator Controller")]
    public RuntimeAnimatorController origionAnimatorController;

    /// 卸下武器
    public void UnEquipWeapon(){
        foreach(Transform child in weaponInstantiatePoint)
        {
            Destroy(child.gameObject);
        }
    }

    /// 装备武器
    public void EquipWeapon(ItemData_SO weaponData){
        //Debug.Log("尝试装备武器："+weaponData.itemName);
        if(weaponData == null || weaponData.itemType != ItemType.Weapon)
        {
            Debug.LogError("传入的物品数据不是武器！");
            return;
        }
        if(weaponData.weaponPrefab != null)
        {
            Instantiate(weaponData.weaponPrefab,weaponInstantiatePoint);
        }    
        //TODO: 人物数值切换到武器状态 ，切换动画状态机到武器状态
    }
}
