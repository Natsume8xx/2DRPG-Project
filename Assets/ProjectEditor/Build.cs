using UnityEngine;
using UnityEditor;
using System;
using HybridCLR.Editor;
using System.IO;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using HybridCLR.Editor.Commands;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEditor.AddressableAssets.Build;

public static class Build 
{
    private static string buildPath => Path.Combine(new DirectoryInfo(Application.dataPath).Parent.FullName,"Builds/Game.app");
    [MenuItem("Build/GenerateDllFiles")]
    //生成或者说搬运dll文件 
    public static void GenerateDllFiles()
    {
        // 搬运Dll文件
        Debug.Log("开始生成Dll文本文件");
        // 找到各路文件夹路径
        string environmentDir = Environment.CurrentDirectory;  // 项目根目录环境
        string aotDllDir = Path.Combine(environmentDir,SettingsUtil.GetAssembliesPostIl2CppStripDir(EditorUserBuildSettings.activeBuildTarget)); // aotDll目录
        string hotUpdateDllDir = Path.Combine(environmentDir,SettingsUtil.GetHotUpdateDllsOutputDirByTarget(EditorUserBuildSettings.activeBuildTarget)); // 热更新dll目录
        string aotTextDir = Path.Combine(environmentDir,"Assets/DllBytes/AOT"); // 要存放到的aot文本目录
        string hotUpdateTextDir = Path.Combine(environmentDir,"Assets/DllBytes/HotUpdate"); // 要存放到的热更新文本目录
        string priorityHotUpdateTextDir = Path.Combine(environmentDir,"Assets/DllBytes/PriorityHotUpdate"); // 要存放到的优先热更新文本目录

        //获取Addressable 包位置
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        AddressableAssetGroup aotGroup = settings.FindGroup("AOT");
        AddressableAssetGroup hotUpdateGroup = settings.FindGroup("HotUpdate");
        AddressableAssetGroup priorityHotUpdateGroup = settings.FindGroup("PriorityHotUpdate");

        // 获取配置文件
        DllConfig config = AssetDatabase.LoadAssetAtPath<DllConfig>("Assets/DateSO/DllConfig/DllConfig.asset");

        // aot文本文件生成
        // 根据配置文件 遍历列表 生成文件 
        foreach(string dllName in config.aot)
        {
            // 拼接路径
            string dllPath = Path.Combine(aotDllDir,$"{dllName}.dll");
            if(!File.Exists(dllPath)) dllPath = Path.Combine(hotUpdateDllDir,$"{dllName}.dll"); //如果aot目录没有 就在热更新目录找
            string dllBytesPath = Path.Combine(aotTextDir,$"{dllName}.dll.bytes");

            // 复制转移
            File.Copy(dllPath,dllBytesPath,true);
            AssetDatabase.Refresh();

            // 设置Addressable
            AddressableAssetEntry entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID($"Assets/DllBytes/AOT/{dllName}.dll.bytes"),aotGroup); // 进入unity了，路径要用相对路径
            entry.SetAddress($"{dllName}");
        }

        // HotUpdate文本文件生成
        foreach(string dllName in config.hotUpdate)
        {
            // 拼接路径
            string dllPath = Path.Combine(hotUpdateDllDir,$"{dllName}.dll");
            string dllBytesPath = Path.Combine(hotUpdateTextDir,$"{dllName}.dll.bytes");

            // 复制转移
            File.Copy(dllPath,dllBytesPath,true);
            AssetDatabase.Refresh();

            // 设置Addressable
            AddressableAssetEntry entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID($"Assets/DllBytes/HotUpdate/{dllName}.dll.bytes"),hotUpdateGroup); // 进入unity了，路径要用相对路径
            entry.SetAddress($"{dllName}");
        }

        // PriorityHotUpdate文本文件生成
        foreach(string dllName in config.priorityHotUpdate)
        {
            // 拼接路径
            string dllPath = Path.Combine(hotUpdateDllDir,$"{dllName}.dll");
            string dllBytesPath = Path.Combine(priorityHotUpdateTextDir,$"{dllName}.dll.bytes");

            // 复制转移
            File.Copy(dllPath,dllBytesPath,true);
            AssetDatabase.Refresh();

            // 设置Addressable
            AddressableAssetEntry entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID($"Assets/DllBytes/PriorityHotUpdate/{dllName}.dll.bytes"),priorityHotUpdateGroup); // 进入unity了，路径要用相对路径
            entry.SetAddress($"{dllName}");
        }
        EditorUtility.SetDirty(settings); //标记addressable设置有修改
        AssetDatabase.SaveAssets(); // 保存修改
        AssetDatabase.Refresh();
        Debug.Log("Dll文本文件生成完成");
    }

    [MenuItem("Build/NewClient")]
    // 构建新客户端(一键打包)
    public static void NewClient()
    {
        Debug.Log("开始构建新客户端");
        // HybirdCLR 构建
        PrebuildCommand.GenerateAll();
        // 生成与搬运 dll，bytes 文件
        GenerateDllFiles();
        // Addressables 本地进行一次构建
        AddressableAssetSettings.BuildPlayerContent();
        // 最终打包，并产出可运行程序
        // 收集Build Setting 中配置的 场景路径
        string[] scenes = new string[EditorSceneManager.sceneCountInBuildSettings];
        for(int i = 0; i < EditorSceneManager.sceneCountInBuildSettings; i++)
        {
            scenes[i] = SceneUtility.GetScenePathByBuildIndex(i);
            Debug.Log($"添加场景{scenes[i]}");
        }
        // 构造 Player Build 的参数
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions()
        {
            scenes = scenes,   //构建场景
            locationPathName = buildPath,  // 构建产物路径
            target = EditorUserBuildSettings.activeBuildTarget,  // 构建平台
            options = BuildOptions.Development | BuildOptions.AllowDebugging // 构建选项，这里选择开发版本并允许调试
        };
        // 最终打包       
        BuildPipeline.BuildPlayer(buildPlayerOptions);
        Debug.Log("新客户端构建完成");
    }

    // 一键更新客户端（增量更新）
    [MenuItem("Build/UpdateClient")]
    public static void UpdateClient()
    {
        Debug.Log("开始更新客户端");
        // HirbridCLR 构建
        PrebuildCommand.GenerateAll();
        // 生成与搬运 dll，bytes 文件
        GenerateDllFiles();
        // Addressables 本地增量更新
        string path = ContentUpdateScript.GetContentStateDataPath(false); //这是增量更新的路径，增量更新会在这个路径下生成一个新的catalog文件和一些新的资源文件夹，里面是本次更新相对于上次构建的差异内容 
        var settings = AddressableAssetSettingsDefaultObject.Settings; // 获取Addressable设置
        ContentUpdateScript.BuildContentUpdate(settings,path); 
        Debug.Log("客户端更新完成");
    }

}

