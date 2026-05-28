using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryManager : Singleton<InventoryManager>,ISaveable
{
    public class dragData // 被拖拽物品的元数据
    {
        public SlotHolder origionSlotHolder;  // 原始格子
        public RectTransform origionParentTransform;  // 原始父物体的RectTransform
    }
    [Header("Inventory Data")]
    public InventoryDataSO bag;
    public InventoryDataSO actionBar;
    public InventoryDataSO characterStates;

    [Header("Containers")]
    public ContainerUI containerBagUI;
    public ContainerUI containerActionBarUI;
    public ContainerUI containerCharacterStatesUI;
    [Header("DragCanvas")]
    public Canvas dragCanvas;
    public dragData origionDragData;
    [Header("Player")]
    public GameObject player;
    [Header("EquipmentController")]
    public EquipmentController equipmentController;

    [Header("UIPanel")]
    public GameObject bagPanel;
    public GameObject characterStatesPanel;
    private bool isOpenBag = false;
    [Header("StateText")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI attackText;
    [Header("ItemTooltip")]
    public ItemTooltip itemTooltip;
    [Header("Items")]
    public Dictionary<string,ItemData_SO> itemDataDic = new Dictionary<string, ItemData_SO>();
    public List<ItemData_SO> itemDataList = new List<ItemData_SO>();

    protected override void Awake()
    {
        base.Awake();
        DataManager.Instance.saveData.AddInventorySaveBlock(bag);
        DataManager.Instance.saveData.AddInventorySaveBlock(actionBar);
        DataManager.Instance.saveData.AddInventorySaveBlock(characterStates);
        FillItemDataDic();
    }
    void OnEnable()
    {
        ISaveable saveable = this;
        saveable.RegisterSaveData();
    }

    void OnDisable()
    {
        ISaveable saveable = this;
        saveable.UnRegisterSaveData();
    }


    void Start()
    {
        equipmentController = player.GetComponent<EquipmentController>();
    }

    void Update()
    {
        // TODO：教程原因先用旧的输入系统，后续有时间会改成新的输入系统
        if(Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame)
        {
            isOpenBag = !isOpenBag;
            bagPanel.SetActive(isOpenBag);
            LayoutRebuilder.ForceRebuildLayoutImmediate(bagPanel.GetComponent<RectTransform>());
            characterStatesPanel.SetActive(isOpenBag);
            LayoutRebuilder.ForceRebuildLayoutImmediate(characterStatesPanel.GetComponent<RectTransform>());
        }
    }

    //填充物品数据字典
    public void FillItemDataDic()
    {
        foreach(var itemData in itemDataList)
        {
            if(!itemDataDic.ContainsKey(itemData.itemID))
            {
                itemDataDic.Add(itemData.itemID,itemData);
            }
            else
            {
                Debug.LogWarning("物品数据字典中已经包含ID为"+itemData.itemID+"的物品数据，请确保物品数据列表中没有重复的ID");
            }
        }
    }

    //根据物品数据ID从物品数据字典中获取物品数据
    public ItemData_SO GetItemDataByID(string itemID)
    {
        if(itemDataDic.ContainsKey(itemID))
        {
            return itemDataDic[itemID];
        }
        else
        {
            Debug.LogError("无法根据ID"+itemID+"找到对应的物品数据，请确保物品数据字典中包含该ID");
            return null;
        }
    }

    //根据存储空间类型返回对应的InventoryDataSO
    public InventoryDataSO GetInventoryDataSOByType(InventoryType inventoryType)
    {
        switch(inventoryType)
        {
            case InventoryType.Bag:
                return bag;
            case InventoryType.Action:
                return actionBar;
            case InventoryType.CharacterState:
                return characterStates;
            default:
                Debug.LogError("无法根据存储空间类型"+inventoryType+"找到对应的InventoryDataSO，请确保传入了正确的存储空间类型");
                return null;
        }
    }

    //刷新角色状态UI显示
    public void RefreshCharacterStates(int maxHealth,int attack)
    {
        healthText.text = maxHealth.ToString();
        attackText.text = attack.ToString();
    }

    #region 检查拖拽结束时 鼠标指针是否在 存储格子UI的范围内
    //三个一起检查
    public bool CheckAll(Vector3 Position)
    {
        return CheckInActionBarUI(Position)||CheckInBagUI(Position)||CheckInCharacterStatesUI(Position);
    }
    //背包检查
    public bool CheckInBagUI(Vector3 Position)
    {
        foreach(var slotHolder in containerBagUI.slotHolders)
        {
            RectTransform rt = slotHolder.transform as RectTransform;
            if(RectTransformUtility.RectangleContainsScreenPoint(rt,Position))
                return true;
        }
        return false;
    }
    //ActionBar检查
    public bool CheckInActionBarUI(Vector3 Position)
    {
        foreach(var slotHolder in containerActionBarUI.slotHolders)
        {
            RectTransform rt = slotHolder.transform as RectTransform;
            if(RectTransformUtility.RectangleContainsScreenPoint(rt,Position))
                return true;
        }
        return false;
    }
    //CharacteStates检查
    public bool CheckInCharacterStatesUI(Vector3 Position)
    {
        foreach(var slotHolder in containerCharacterStatesUI.slotHolders)
        {
            RectTransform rt = slotHolder.transform as RectTransform;
            if(RectTransformUtility.RectangleContainsScreenPoint(rt,Position))
                return true;
        }
        return false;
    }
    #endregion

    #region ISaveable接口实现
    public DataDefination GetDataID()
    {
        return GetComponent<DataDefination>();
    }

    public void GetSaveData(Data data)
    {
        DataManager.Instance.saveData.SetInventorySaveBlock();
    }

    public void LoadSaveData(Data data)
    {
        DataManager.Instance.saveData.GetInventoryDataFromSaveBlock();
    }
    #endregion

    #region 检查背包中物品 是否满足任务要求，并更新任务要求

    public void CheckQuestItemInBag(string questItemName){
        // 检查背包
        foreach(var item in bag.items){
            if(item.inventoryDataSO != null && item.inventoryDataSO.itemName == questItemName)
            {
                // 执行任务物品检查逻辑
                QuestManager.Instance.SetUpQuestProgress(item.inventoryDataSO.itemName,item.amount);
            }
        }
        // 检查 actionbar
        foreach(var item in actionBar.items){
            if(item.inventoryDataSO != null && item.inventoryDataSO.itemName == questItemName)
            {
                // 执行任务物品检查逻辑
                QuestManager.Instance.SetUpQuestProgress(item.inventoryDataSO.itemName,item.amount);
            }
        }
    }

    #endregion

    #region 检测 背包和物品栏中 指定物品 并返回 InventoryItem
    public InventoryItem GetItemInBag(ItemData_SO itemData)
    {
        return bag.items.Find(item => item.inventoryDataSO == itemData);
    }

    public InventoryItem GetItemInActionBar(ItemData_SO itemData)
    {
        return actionBar.items.Find(item => item.inventoryDataSO == itemData);
    }
    #endregion
}
