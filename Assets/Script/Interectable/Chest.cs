using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour,IInteractable
{
    public Sprite openedChestSprite;
    public Sprite closedChestSprite;
    public bool isSignOver;
    void Start()
    {
        isSignOver = false;
        this.GetComponent<SpriteRenderer>().sprite = closedChestSprite;
        this.GetComponent<Collider2D>().enabled = isSignOver==false?true:false; //防止重新加载时，箱子已经被打开了，但collider没有被禁用
    }
    public void TriggerAction()
    {
        Debug.Log("打开宝箱");
        if (!isSignOver)
        {
            this.GetComponent<SpriteRenderer>().sprite = openedChestSprite;
            isSignOver = true;
            this.GetComponent<AudioDefination>().PlayAudio();
            this.GetComponent<Collider2D>().enabled = false;
        }
    }

}
