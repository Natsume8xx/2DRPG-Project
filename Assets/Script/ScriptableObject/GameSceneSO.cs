using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(menuName ="Game Scene/GameSceneSO")]
public class GameSceneSO : ScriptableObject
{
    public SceneType sceneType;  // 场景类型
    public AssetReference sceneRefrence;  // 引用的场景参数
}
