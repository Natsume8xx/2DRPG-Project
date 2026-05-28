using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ItemTooltip : MonoBehaviour
{
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescriptionText;
    public RectTransform backgroundRectTransform;

    void Awake()
    {
        UpdateRectTransform();
    }
    void Update()
    {
        //UpdateRectTransform();  还是不要每帧更新了，会出现闪烁现象
    }

    //根据鼠标位置更新Tooltip的位置，并确保Tooltip不会超出屏幕边界
    public void UpdateRectTransform()
    {
        var mouse = Mouse.current;
        if (mouse == null)
        {
            return;
        }

        Vector3 mousePosition = mouse.position.ReadValue();
        Vector3[] rectBounds = new Vector3[4];
        backgroundRectTransform.GetWorldCorners(rectBounds);
        float length = rectBounds[3].x - rectBounds[0].x;
        float height = rectBounds[1].y - rectBounds[0].y;
        if(mousePosition.y < height)
        {
            backgroundRectTransform.position = mousePosition + Vector3.up * height*0.6f;
        }else if(Screen.width - mousePosition.x > length)
            backgroundRectTransform.position = mousePosition + Vector3.right * length*0.6f;
        else
            backgroundRectTransform.position = mousePosition + Vector3.left * length*0.6f;
           
    }

    //根据传入的物品数据设置Tooltip的显示内容
    public void SetUpTooltip(ItemData_SO itemData)
    {
        if(itemData != null)
        {
            itemNameText.text = itemData.itemName;
            itemDescriptionText.text = itemData.itemDescription;
            gameObject.SetActive(true);
            UpdateRectTransform();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
