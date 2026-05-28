using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;
using HybridCLR;
using System.Reflection;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class HotUpdateSystem : MonoBehaviour
{
    public const string hotUpdateWindowKey = "HotUpdateWindow";
    public const string dllConfigKey = "DllConfig";
    public const string gameLanucherKey = "GameLanucher"; //游戏启动器的kry，现在已经不用了
    public const string initialLoadKey = "InitialLoad"; //初始加载物体的key
    public bool isLoadedPriorityHotUpdate = false; // 是否已经完成优先热更新的标记
    private IHotUpdateWindow hotUpdateWindow;
    private DllConfig dllConfig;
    public HashSet<string> LoadedDlls = new HashSet<string>(); // 已加载的DLL名称集合，防止重复加载
    public class HotUpdateState
    {
        public int state; // 0: 热更新正在进行，1: 热更新完成
        public HotUpdateState(int state)
        {
            this.state = state;
        }
    }
    private string cataPath;
    private string statePath;   
    private string catalogPath;  // 目录文件 路径 
    void Start()
    {
        cataPath = $"{Application.persistentDataPath}/com.unity.addressables";
        statePath = $"{Application.persistentDataPath}/HotUpdateState.json";
        catalogPath = $"{cataPath}/catalog_1.0.json";
        //Addressables.ClearDependencyCacheAsync("");
        StartCoroutine(HotUpdateProcess());
    }
    // 热更新总体协程
    public IEnumerator HotUpdateProcess()
    {
        // 确保unity开屏动画完全结束，再开始热更新
        while (SplashScreen.isFinished == false)
            yield return null;
        HotUpdateState hotUpdateState = new HotUpdateState(0);
        string json;
        //断点续传的检查
        //int hotUpdateState = PlayerPrefs.GetInt("HotUpdateState",0);
        if (!File.Exists(statePath))
        {
            // 说明是第一次热更新，直接创建状态文件并且开始热更新
            hotUpdateState.state = 0;
            json = JsonConvert.SerializeObject(hotUpdateState,Formatting.Indented);
            File.WriteAllText(statePath,json);
        }
        // 读取状态文件 检查上次热更新是否完成
        json = File.ReadAllText(statePath);
        hotUpdateState = JsonConvert.DeserializeObject<HotUpdateState>(json);
        if(hotUpdateState.state == 0)
        {
            Debug.Log("上次热更新中断未完成");
            if(Directory.Exists(cataPath)) Directory.Delete(cataPath,true); // 删除上次热更新下载的目录
        }
        hotUpdateState.state = 0;
        json = JsonConvert.SerializeObject(hotUpdateState,Formatting.Indented);
        File.WriteAllText(statePath,json);
        // 初始化 Addressables
        yield return Addressables.InitializeAsync();

        // 先下载 DllConfig 配置文件
        yield return Addressables.DownloadDependenciesAsync(dllConfigKey,true); //下载依赖
        var handle = Addressables.LoadAssetAsync<DllConfig>(dllConfigKey);
        yield return handle;
        if(handle.Status == AsyncOperationStatus.Succeeded)
        {
            dllConfig = handle.Result;
        }
        // 检查更新目录
        yield return CheckForCatalogUpdates();
        // 下载更新目录
        // 下载资源
        // 热更新完成
        hotUpdateState.state = 1;
        json = JsonConvert.SerializeObject(hotUpdateState,Formatting.Indented);
        File.WriteAllText(statePath,json);

        // 无论是否热更新都要完成的操作： 加载各种程序集，补充元数据，跳转场景
        // 加载热更新程序集
        yield return LoadHotUpdateAssembly();
        // 加载AOT程序集元数据
        yield return LoadMetaDataForAOTAssembly();
        // 跳转场景
        if(hotUpdateWindow == null) // 如果热更新窗口未加载，说明没有需要热更新的内容，直接跳转,否则等热更新窗口的回调来跳转
        {
            JumpToMenu();
        }
    }

    // 检查更新目录
    private IEnumerator CheckForCatalogUpdates()
    {
        AsyncOperationHandle<List<string>> checkHandle = Addressables.CheckForCatalogUpdates(false);
        yield return checkHandle;
        // 判断协程成功与否 并以日志方式输出
        if(checkHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"检查更新目录失败: {checkHandle.OperationException}");
            yield break;
        }
        else
        {
            Debug.Log("检查更新目录成功");
            List<string> catalogsToUpdate = checkHandle.Result;
            if(catalogsToUpdate.Count > 0)
            {
                foreach (string catalog in catalogsToUpdate)
                {
                    Debug.Log($"需要更新的目录: {catalog}");
                }
                // 下载更新目录
                yield return UpdateCatalog(catalogsToUpdate);
            }
            else
            {
                Debug.Log("没有需要更新的目录");
            } 
        }
        // 释放资源
        Addressables.Release(checkHandle);

    }
    
    // 下载更新目录
    private IEnumerator UpdateCatalog(List<string> catalogToUpdate)
    {
        AsyncOperationHandle<List<IResourceLocator>> updateHandle = Addressables.UpdateCatalogs(catalogToUpdate,false);
        yield return updateHandle;
        if(updateHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"下载更新目录失败：{updateHandle.OperationException}");
            yield break;
        }
        else
        {
            Debug.Log("下载更新目录成功,先开始优先热更新");
            yield return PriorityHotUpdate();
            //Debug.Log("下载更新目录成功");
            List<IResourceLocator> resourceLocators = updateHandle.Result;   // 获取更新后的资源定位器列表
            if(resourceLocators.Count > 0)
            {
                List<object> downloadKeys = new List<object>(1000);   // 全部需要的keys
                foreach( var locator in resourceLocators)  // 遍历资源定位器，获取需要下载的资源键。 一个catalog对应一个资源定位器，资源定位器包含了该catalog中所有资源的键
                {
                    Debug.Log(locator.LocatorId);  //该定位器的ID，通常是catalog的名称
                    downloadKeys.AddRange(locator.Keys); // 添加的是一个资源定位器的所有资源键
                }
                // 获取下载量并下载
                yield return GetDownLoadSizes(downloadKeys);
            }
        }
        // 释放资源
        Addressables.Release(updateHandle);
    }

    // 获取下载量
    private IEnumerator GetDownLoadSizes(IEnumerable<object> downloadKeys)
    {
        AsyncOperationHandle<long> sizeHandle = Addressables.GetDownloadSizeAsync(downloadKeys);
        yield return sizeHandle;
        if(sizeHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"获取下载量失败：{sizeHandle.OperationException}");
            yield break;
        }
        else
        {
            long downLoadSize = sizeHandle.Result;
            if (downLoadSize > 0)
            {
                Debug.Log($"需要下载的资源量为{downLoadSize}");
                // 传递信息给热更新窗口
                hotUpdateWindow.Show(downLoadSize,JumpToMenu);
                // 下载资源依赖
                yield return DownLoadDependences(downloadKeys,downLoadSize);
            }
            else
            {
                Debug.Log("下载量为0，无需下载");
            } 
        }
        // 释放资源
        Addressables.Release(sizeHandle);
    }

    // 下载资源依赖
    private IEnumerator DownLoadDependences(IEnumerable<object> downloadKeys,long downLoadSize)
    {
        var downLoadHandle = Addressables.DownloadDependenciesAsync(downloadKeys,Addressables.MergeMode.Union,false);
        // 防止下载太快，没进循环就完成了，进度条没有机会更新
        float initialPercent = downLoadHandle.GetDownloadStatus().Percent;
        hotUpdateWindow.UpdatedBar((long)(downLoadSize * initialPercent));
        // while循环查看下载进度
        while (!downLoadHandle.IsDone)
        {
            //  下载失败
            if(downLoadHandle.Status == AsyncOperationStatus.Failed)
            {
                Debug.LogError($"下载资源失败：{downLoadHandle.OperationException}");
                yield break;
            }
            //分发下载进度
            float percentage = downLoadHandle.GetDownloadStatus().Percent; // 百分比下载进度
            float currentSize = downLoadSize * percentage; // 当前已下载的资源量(字节)
            Debug.Log($"下载进度：{currentSize}/{downLoadSize}");
            hotUpdateWindow.UpdatedBar((long)currentSize);
            yield return null; // 等待下一帧继续检查下载状态
        }
        // 确保下载完成后进度条显示100%
        hotUpdateWindow?.UpdatedBar(downLoadSize);
        Debug.Log("热更新完成");
    }

    //优先更新的部分
    public IEnumerator PriorityHotUpdate()
    {
        Debug.Log("开始优先热更新");
        // 下载 优先热更新程序集
        yield return Addressables.DownloadDependenciesAsync(dllConfig.priorityHotUpdate,Addressables.MergeMode.Union,true);  //下载依赖
        #if !UNITY_EDITOR
        // 加载 优先热更新程序集  
        foreach(string dllName in dllConfig.priorityHotUpdate)
        {
            // 检查是否为空,为空要下载后再加载
            var sizeHandle = Addressables.GetDownloadSizeAsync(dllName);
            yield return sizeHandle;
            if(sizeHandle.Status == AsyncOperationStatus.Succeeded)
            {
                if(sizeHandle.Result > 0)
                {
                    Debug.Log($"程序集 {dllName} 本地没有，需要下载，大小为{sizeHandle.Result}字节");
                    // 下载资源
                    var downLoadHandle = Addressables.DownloadDependenciesAsync(dllName,true);
                    yield return downLoadHandle;
                    if(downLoadHandle.Status != AsyncOperationStatus.Succeeded)
                    {
                        Debug.LogError($"下载程序集 {dllName} 失败：{downLoadHandle.OperationException}");
                        yield break;
                    }
                }
            }
            Addressables.Release(sizeHandle);   // 释放下载量的句柄
            //安全加载
            var loadHandle = Addressables.LoadAssetAsync<TextAsset>(dllName);
            yield return loadHandle;
            if(loadHandle.Status == AsyncOperationStatus.Succeeded && loadHandle.Result != null)
            {
                TextAsset dllText = loadHandle.Result;
                LoadDll(dllText);
                Debug.Log($"优先热更新程序集 {dllName} 加载完成");
            }
            else
            {
                Debug.LogError($"加载程序集 {dllName} 失败：{loadHandle.OperationException}");
                yield break;
            }
            Addressables.Release(loadHandle);   // 加载完成后立即释放文本资源
            Debug.Log($"优先热更新程序集 {dllName} 加载完成");
        }
        isLoadedPriorityHotUpdate = true; // 标记优先热更新完成
        #endif
        // 重载目录
        yield return ReLoadContentCatalog();
        // 先加载热更新窗口物体，当目录拿到后，先下载HotUpdateWindow依赖的资源，并加载窗口
        yield return LoadHotUpdateWindow();
        Debug.Log("优先热更新完成");
    }

    // 下载并加载HotUpdateWindow
    public IEnumerator LoadHotUpdateWindow()
    {
        yield return Addressables.DownloadDependenciesAsync(hotUpdateWindowKey,true);  //下载依赖
        //hotUpdateWindow = Addressables.InstantiateAsync(hotUpdateWindowKey).WaitForCompletion().GetComponent<IHotUpdateWindow>(); //实例化对象,注意释放时机
        var handle = Addressables.InstantiateAsync(hotUpdateWindowKey);//不会卡主线程的写法，异步加载
        yield return handle;
        if(handle.Status == AsyncOperationStatus.Succeeded)
        {
            hotUpdateWindow = handle.Result.GetComponent<IHotUpdateWindow>();
        }
        else
        {
            Debug.LogError("加载热更新窗口失败");
            yield break;
        }
    }

    // 重载目录
    public IEnumerator ReLoadContentCatalog()
    {
        yield return Addressables.LoadContentCatalogAsync(catalogPath);  // 重新加载目录
    }

    // 根据TextAsset 类型-> byte[] -> 加载程序集。 加载程序集的方法
    public void LoadDll(TextAsset dllText)
    {
        if (!LoadedDlls.Contains(dllText.name) && dllText.bytes != null && dllText.bytes.Length > 0) // 防止重复加载和空数据加载
        {
            Assembly.Load(dllText.bytes);
            LoadedDlls.Add(dllText.name);
            Debug.Log($"加载程序集:{dllText.name}");
        }
        // 其实在这里释放dllText会更安全，但是资源量不是很大，加载的时间应该不会很长，而且代码能跑，就按这样吧
    }

    // 加载热更新程序集
    IEnumerator LoadHotUpdateAssembly()
    {
        // 编译器下无需加载热更新程序集，直接返回
        #if UNITY_EDITOR
        Debug.Log("编辑器模式，无需加载热更新程序集");
        yield break;
        #endif
        Debug.Log("开始加载所有热更新程序集");
        // 加载优先热更新程序集
        if(!isLoadedPriorityHotUpdate){
            foreach(string dllName in dllConfig.priorityHotUpdate)
            {
                // 检查是否为空,为空要下载后再加载
                var sizeHandle = Addressables.GetDownloadSizeAsync(dllName);
                yield return sizeHandle;
                if(sizeHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    if(sizeHandle.Result > 0)
                    {
                        Debug.Log($"程序集 {dllName} 本地没有，需要下载，大小为{sizeHandle.Result}字节");
                        // 下载资源
                        var downLoadHandle = Addressables.DownloadDependenciesAsync(dllName,true);
                        yield return downLoadHandle;
                        if(downLoadHandle.Status != AsyncOperationStatus.Succeeded)
                        {
                            Debug.LogError($"下载程序集 {dllName} 失败：{downLoadHandle.OperationException}");
                            yield break;
                        }
                    }
                }
                Addressables.Release(sizeHandle);   // 释放下载量的句柄
                //安全加载
                var loadHandle = Addressables.LoadAssetAsync<TextAsset>(dllName);
                yield return loadHandle;
                if(loadHandle.Status == AsyncOperationStatus.Succeeded && loadHandle.Result != null)
                {
                    TextAsset dllText = loadHandle.Result;
                    LoadDll(dllText);
                    Debug.Log($"优先热更新程序集 {dllName} 加载完成");
                }
                else
                {
                    Debug.LogError($"加载程序集 {dllName} 失败：{loadHandle.OperationException}");
                    yield break;
                }
                Addressables.Release(loadHandle);   // 加载完成后立即释放文本资源
                Debug.Log($"优先热更新程序集 {dllName} 加载完成");
            }
            isLoadedPriorityHotUpdate = true; // 标记优先热更新完成
        }
        // 重载目录
        yield return ReLoadContentCatalog();
        // 加载热更新程序集
        foreach(string dllName in dllConfig.hotUpdate)
        {
            // 检查是否为空,为空要下载后再加载
            var sizeHandle = Addressables.GetDownloadSizeAsync(dllName);
            yield return sizeHandle;
            if(sizeHandle.Status == AsyncOperationStatus.Succeeded)
            {
                if(sizeHandle.Result > 0)
                {
                    Debug.Log($"程序集 {dllName} 本地没有，需要下载，大小为{sizeHandle.Result}字节");
                    // 下载资源
                    var downLoadHandle = Addressables.DownloadDependenciesAsync(dllName,true);
                    yield return downLoadHandle;
                    if(downLoadHandle.Status != AsyncOperationStatus.Succeeded)
                    {
                        Debug.LogError($"下载程序集 {dllName} 失败：{downLoadHandle.OperationException}");
                        yield break;
                    }
                }
            }
            Addressables.Release(sizeHandle);   // 释放下载量的句柄
            var loadHandle = Addressables.LoadAssetAsync<TextAsset>(dllName);
            yield return loadHandle;
            if(loadHandle.Status == AsyncOperationStatus.Succeeded && loadHandle.Result != null)
            {
                TextAsset dllText = loadHandle.Result;
                LoadDll(dllText);
                Debug.Log($"热更新程序集 {dllName} 加载完成");
            }
            else
            {
                Debug.LogError($"加载程序集 {dllName} 失败：{loadHandle.OperationException}");
                yield break;
            }
            Addressables.Release(loadHandle);   // 加载完成后立即释放文本资源
            Debug.Log($"热更新程序集 {dllName} 加载完成");
        }
        // 重载目录
        yield return ReLoadContentCatalog();
        Debug.Log("所有热更新程序集加载完成");
    }

    //加载AOT程序集元数据
    IEnumerator LoadMetaDataForAOTAssembly()
    {
        #if UNITY_EDITOR
        Debug.Log("编辑器模式，无需加载AOT程序集元数据");
        yield break;
        #endif
        foreach(string dllName in dllConfig.aot)
        {
            // 检查是否为空,为空要下载后再加载
            var sizeHandle = Addressables.GetDownloadSizeAsync(dllName);
            yield return sizeHandle;
            if(sizeHandle.Status == AsyncOperationStatus.Succeeded)
            {
                if(sizeHandle.Result > 0)
                {
                    Debug.Log($"程序集 {dllName} 本地没有，需要下载，大小为{sizeHandle.Result}字节");
                    // 下载资源
                    var downLoadHandle = Addressables.DownloadDependenciesAsync(dllName,true);
                    yield return downLoadHandle;
                    if(downLoadHandle.Status != AsyncOperationStatus.Succeeded)
                    {
                        Debug.LogError($"下载程序集 {dllName} 失败：{downLoadHandle.OperationException}");
                        yield break;
                    }
                }
            }
            Addressables.Release(sizeHandle);   // 释放下载量的句柄
            var loadHandle = Addressables.LoadAssetAsync<TextAsset>(dllName);
            yield return loadHandle;
            TextAsset dllText = null;
            if(loadHandle.Status == AsyncOperationStatus.Succeeded && loadHandle.Result != null)
            {
                dllText = loadHandle.Result;
                Debug.Log($"AOT程序集元数据 {dllName} 加载完成");
            }
            else
            {
                Debug.LogError($"加载AOT程序集元数据 {dllName} 失败：{loadHandle.OperationException}");
                yield break;
            }
            Addressables.Release(loadHandle);   // 加载完成后立即释放文本资源
            // 补充元数据时内部会自动将dllText.bytes复制一份，调用完成后请不要将dllText.bytes保存，造成无谓的内存浪费
            LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllText.bytes, HomologousImageMode.SuperSet);
            Debug.Log($"LoadMetadataForAOTAssembly:{dllText.name}. ret:{err}");
        }
    }
    
    // 跳转到新的场景
    public void JumpToMenu()
    {
        // 卸载热更新窗口物体
        if(hotUpdateWindow != null)
        {
            Addressables.Release(((Component)hotUpdateWindow).gameObject);   
        }
        // 卸载配置文件
        if(dllConfig != null)
        {
            Addressables.Release(dllConfig);   
        }
        //SceneManager.LoadScene("Menu");    // 跳转到菜单场景，现在换成另一种游戏启动器的方式进入场景，可添加的逻辑更多
        //Addressables.InstantiateAsync(gameLanucherKey).WaitForCompletion(); // 实例化游戏启动器，进入游戏
        //TODO: 将Initial物体激活
        Addressables.InstantiateAsync(initialLoadKey).WaitForCompletion(); // 实例化初始加载物体，进入游戏
    }

}
