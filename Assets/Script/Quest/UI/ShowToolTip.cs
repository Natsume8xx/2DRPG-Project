using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// 显示物品信息的工具提示 这个是装在物品本身上的
public class ShowToolTip : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    public ItemUI currentItemUI;

    void Awake()
    {
        currentItemUI = GetComponent<ItemUI>();
    }

#region IPointerEnterHandler implementation接口实现方法
    public void OnPointerEnter(PointerEventData eventData)
    {
        QuestUI.Instance.itemTooltip.gameObject.SetActive(true);
        QuestUI.Instance.itemTooltip.SetUpTooltip(currentItemUI.itemDataSO);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        QuestUI.Instance.itemTooltip.gameObject.SetActive(false);
    }
#endregion
}
