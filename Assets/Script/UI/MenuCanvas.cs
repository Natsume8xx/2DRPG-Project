using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuCanvas : MonoBehaviour
{
    public GameObject newGameButton;  // 新游戏按钮
    //public PlayAudioEventSO playMenuBGM;  //进入主界面播放BGM
    void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(newGameButton);
    }
    public void ExitGame()
    {
        Debug.Log("退出游戏");
        Application.Quit();
    }
}
