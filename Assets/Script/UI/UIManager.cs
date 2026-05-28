using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    //public SceneLoadManager sceneLoadManager;
    [Header("事件监听")]
    public CharacterEventSO characterEventSO;
    public SceneLoadEventSO sceneUnLoadEventSO;  // 场景刚卸载还没有加载时事件
    public VoidEventSO loadDataEventSO;  //加载数据的事件
    public VoidEventSO gameOverEventSO; //游戏结束的事件
    public PlayerStateBar playerStateBar;
    public FloatEventSO syncVolumeEvent;  //音频数据传递事件
    public VoidEventSO backToMenuEventSO;   //回到主界面事件
    [Header("事件广播")]
    public VoidEventSO pauseEvent;  // 游戏暂停事件
    [Header("UI组件")]
    public GameObject gameOverPanel;
    public GameObject restartBut;
    public GameObject mobileTouch;
    public Button settingBut;
    public GameObject pausePanel;
    public Slider volumeSlider;
    public GameObject actionBarUI;


    void Awake()
    {
        #if UNITY_STANDALONE
        mobileTouch.SetActive(false);
        #endif
        settingBut.onClick.AddListener(TogglePausePanel);
    }


    void OnEnable()
    {
        characterEventSO.OnEventRasied += changeHealthBar;
        sceneUnLoadEventSO.LoadRequestEvent += OnLoadRequestEvent;
        loadDataEventSO.OnEventRaised += OnLoadDataEvent;
        gameOverEventSO.OnEventRaised += OnGameOverEvent;
        syncVolumeEvent.OnEventRaised += OnSyncVolumeEvent;
        backToMenuEventSO.OnEventRaised += BackToMenu;
    }

    void OnDisable()
    {
        characterEventSO.OnEventRasied -= changeHealthBar;
        sceneUnLoadEventSO.LoadRequestEvent -= OnLoadRequestEvent;
        loadDataEventSO.OnEventRaised -= OnLoadDataEvent;
        gameOverEventSO.OnEventRaised -= OnGameOverEvent;
        syncVolumeEvent.OnEventRaised -= OnSyncVolumeEvent;
        backToMenuEventSO.OnEventRaised -= BackToMenu;
    }

    // 返回主菜单的事件触发函数
    private void BackToMenu()
    {
        // 关闭 物品栏，暂停面板 UI
        actionBarUI.SetActive(false);
        pausePanel.SetActive(false);
        Time.timeScale = 1;
    }

    //音频数据传输事件的触发函数
    private void OnSyncVolumeEvent(float amount)
    {
        volumeSlider.value = (amount+80)/100;
    }

    //点开设置选项的时候跳出相应面板,并设置游戏暂停
    private void TogglePausePanel()
    {
        if (pausePanel.activeInHierarchy)
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1;
        }
        else
        {
            pauseEvent.OnEventRaised();
            pausePanel.SetActive(true);
            Time.timeScale = 0;
        }
    }
    //角色死亡时，跳出结算面板
    private void OnGameOverEvent()
    {
        gameOverPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(restartBut);
    }

    //当按下重新开始按钮后
    private void OnLoadDataEvent()
    {
        gameOverPanel.SetActive(false);
    }

    // 根据场景类型显示或隐藏玩家状态栏
    private void OnLoadRequestEvent(GameSceneSO loadScene, Vector3 posToGo, bool fadeScreen)
    {
        var isMenu = loadScene.sceneType == SceneType.Menu;
        playerStateBar.gameObject.SetActive(!isMenu);
    }

    private void changeHealthBar(Character character)
    {
        var healthChange = character.currentHealthy / character.maxHealthy;
        var powerChange = character.currentPower / character.maxPower;
        playerStateBar.OnHealthChange(healthChange);
        playerStateBar.OnPowerChange(powerChange);
    }
}
