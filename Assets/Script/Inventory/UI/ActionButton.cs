using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ActionButton : MonoBehaviour
{
    [Header("Input")]
    public Key actionKey = Key.None;  // 使用该格子的按键，后续在Inspector中配置

    public SlotHolder currentSlotHolder;
    void Awake()
    {
        currentSlotHolder = GetComponent<SlotHolder>();
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null || actionKey == Key.None)
        {
            return;
        }

        if (keyboard[actionKey].wasPressedThisFrame && currentSlotHolder.itemUI.GetInventoryItem().inventoryDataSO != null)
        {
            currentSlotHolder.UseItem();
        }
    }
}
