using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePoint : MonoBehaviour, IInteractable
{
    [Header("广播")]
    public VoidEventSO saveDataEvent;
    [Header("变量参数")]
    public SpriteRenderer childSprite;
    public GameObject lightObj;
    public Sprite lightSprite;
    public Sprite darkSprite;
    public bool isActivated;

    void OnEnable()
    {
        lightObj.SetActive(isActivated);
    }

    public void TriggerAction()
    {
        if (!isActivated)
        {
            isActivated = true;
            lightObj.SetActive(true);
            childSprite.sprite = lightSprite;
            this.GetComponent<Collider2D>().enabled = false;
            saveDataEvent.RaiseEvent();
        }
    }
}
