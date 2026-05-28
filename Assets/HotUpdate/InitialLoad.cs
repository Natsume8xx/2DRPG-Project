using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class InitialLoad : MonoBehaviour
{
    public AssetReference persistentScene;
    void Awake()
    {
        Debug.Log("测试测试");
        Addressables.LoadSceneAsync(persistentScene);
    }
}
