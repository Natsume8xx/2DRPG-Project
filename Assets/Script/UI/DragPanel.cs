using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DragPanel : MonoBehaviour,IDragHandler,IPointerDownHandler
{
    public RectTransform panelRectTransform;
    public Canvas canvas;

    void Awake()
    {
        panelRectTransform = GetComponent<RectTransform>();
    }
#region 接口实现
    public void OnDrag(PointerEventData eventData)
    {
        panelRectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        panelRectTransform.SetSiblingIndex(2);
    }
#endregion
}
