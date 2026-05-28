using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(DataDefination))]
public class SceneLoadManager : MonoBehaviour,ISaveable
{
    [Header("玩家初始参数")]
    public Vector3 firstPlayerPos;  // 初始玩家位置
    public Transform playerTrans;
    [Header("场景加载管理")]
    public GameSceneSO menuScene;  // 菜单场景
    public GameSceneSO firstScene;  // 新游戏场景
    public bool isLoadingScene = false;  // 是否正在加载场景
    private GameSceneSO currentScene;  // 当前场景
    private GameSceneSO sceneToLoad;  // 要加载的场景
    private Vector3 positionToGo;  // 要前往的位置
    [Header("淡入淡出参数")]
    private bool fadeScreen;  // 是否淡入淡出
    private float fadeDuration = 0.7f;  // 淡入淡出的持续时间
    [Header("事件监听")]
    public VoidEventSO newGameEventSO;  // 新游戏事件
    public VoidEventSO backToMenuEventSO;   //回到主界面事件
    public SceneLoadEventSO sceneLoadEventSO;  // 场景加载事件
    [Header("事件广播")]
    public VoidEventSO afterSceneLoadEventSO;  // 场景加载完成事件
    public SceneLoadEventSO sceneUnloadEventSO;  // 场景刚卸载还没有加载时事件
    public FadeEventSO fadeEventSO;  // 淡入淡出事件
    [Header("下方武器栏UI")]
    public GameObject actionBarUI;
    void Start()
    {
        //newGame();
        //加载主界面
        sceneLoadEventSO.RaiseLoadRequestEvent(menuScene, firstPlayerPos, false);
    }
    
    void OnEnable()
    {
        sceneLoadEventSO.LoadRequestEvent += OnLoadRequestEvent;
        newGameEventSO.OnEventRaised += newGame;
        backToMenuEventSO.OnEventRaised += OnBackToMenuEvent;

        ISaveable saveable = this;
        saveable.RegisterSaveData();
    }

    void OnDisable()
    {
        sceneLoadEventSO.LoadRequestEvent -= OnLoadRequestEvent;
        newGameEventSO.OnEventRaised -= newGame;
        backToMenuEventSO.OnEventRaised -= OnBackToMenuEvent;

        ISaveable saveable = this;
        saveable.UnRegisterSaveData();
    }

    //返回主界面的事件响应函数
    private void OnBackToMenuEvent()
    {
        sceneToLoad = menuScene;
        sceneLoadEventSO.RaiseLoadRequestEvent(sceneToLoad, firstPlayerPos, true); //我触发我自己的监听事件
    }

    //做完主界面后更改
    public void newGame()
    {
        playerTrans.GetComponent<Character>().currentHealthy = playerTrans.GetComponent<Character>().maxHealthy;
        playerTrans.GetComponent<Character>().currentPower = playerTrans.GetComponent<Character>().maxPower;
        sceneLoadEventSO.RaiseLoadRequestEvent(firstScene, firstPlayerPos, true);
        //清空背包数据
        DataManager.Instance.saveData.ClearInventoryData();
    }
    
#region 场景卸载、加载
    //加载场景的事件响应函数 用来接收场景加载事件传递的参数 并且加载场景
    private void OnLoadRequestEvent(GameSceneSO loadScene, Vector3 posToGo, bool fadeScreen)
    {
        //保存到本地，便于异步加载
        sceneToLoad = loadScene;
        positionToGo = posToGo;
        this.fadeScreen = fadeScreen;
        Debug.Log("接收到加载场景事件，准备加载场景：" + sceneToLoad.name);
        if(currentScene != null){
            StartCoroutine(UnLoadPreviousSceneAndJumpToNewScene());
        }
        else
        {
            LoadNewScene();
        }
    }
    /// // 卸载当前场景，加载新场景，并且前往目标位置
    public IEnumerator UnLoadPreviousSceneAndJumpToNewScene()
    {
        if(isLoadingScene)
            yield return null;
        isLoadingScene = true;
        // 关闭玩家控制，隐藏玩家
        playerTrans.gameObject.SetActive(false);
        //处理加载场景的淡入淡出需求
        if (this.fadeScreen)
        {
            //淡入动画
            fadeEventSO.FadeIn(Color.black, fadeDuration);
        }
        yield return new WaitForSeconds(fadeDuration);
        sceneUnloadEventSO.RaiseLoadRequestEvent(sceneToLoad, positionToGo, true);
        Debug.Log("淡入动画完成，准备卸载当前场景");
        //卸载当前场景
        if(currentScene != null)
            yield return currentScene.sceneRefrence.UnLoadScene();
        Debug.Log("卸载场景完成，准备加载新场景");
        //加载新场景
        LoadNewScene();
        Debug.Log("加载新场景完成，准备前往目标位置");
    }

    // 加载新场景
    public void LoadNewScene()
    {
        if(sceneToLoad != null)
        {
            // 异步加载新场景
            var loadingOption =sceneToLoad.sceneRefrence.LoadSceneAsync(LoadSceneMode.Additive,true);
            loadingOption.Completed += OnLoadComplete;
        }
    }

    //当异步场景加载完成后的响应函数
    private void OnLoadComplete(AsyncOperationHandle<SceneInstance> handle)
    {
        currentScene = sceneToLoad;
        playerTrans.position = positionToGo;
        if (fadeScreen)
        {
            //淡出动画
            fadeEventSO.FadeOut(Color.clear, fadeDuration);
        }
        isLoadingScene = false; 
        if(currentScene.sceneType != SceneType.Menu)
        {
            //移动玩家位置
            playerTrans.position = positionToGo;
            // 重新启用玩家控制，显示玩家
            playerTrans.gameObject.SetActive(true);
            //触发场景加载完成事件
            afterSceneLoadEventSO.RaiseEvent();
        }
        if(currentScene.sceneType == SceneType.Menu)
            playerTrans.gameObject.SetActive(true);
        //强制更新一下血量
        playerTrans.GetComponent<Character>().OnHealthChange?.Invoke(playerTrans.GetComponent<Character>());
        playerTrans.GetComponent<PlayerController>().isDead = false;
        if(sceneToLoad.sceneType != SceneType.Menu)
            actionBarUI.SetActive(true);
        // 刷新一下物品栏UI
        InventoryManager.Instance.containerActionBarUI?.RefreshUI();
    }

#endregion

#region ISaveable接口实现
    // 接口方法实现——获取数据ID
    public DataDefination GetDataID()
    {
        return GetComponent<DataDefination>();
    }

    //接口实现——保存场景数据
    public void GetSaveData(Data data)
    {
        Debug.Log("我是注册了的，准备保存场景数据！");
        data.SaveGameScene(currentScene);
    }

    //接口方法实现——加载保存过的场景数据
    public void LoadSaveData(Data data)
    {
        var playerID = playerTrans.GetComponent<DataDefination>().ID;
        //判断有没有保存过场景，只要判断人物数据有没有保存过
        if(data.characterPosDic.ContainsKey(playerID)){
            positionToGo = data.characterPosDic[playerID].returnVector3();
            //Debug.Log("找到了玩家ID，确实保存过一次");
            sceneToLoad = data.GetSavedScene();
            OnLoadRequestEvent(sceneToLoad,positionToGo,true);
        }
    }
#endregion
}
