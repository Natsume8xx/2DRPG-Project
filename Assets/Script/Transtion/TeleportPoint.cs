using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportPoint : MonoBehaviour, IInteractable
{
    public SceneLoadEventSO sceneLoadEventSO;  // 场景加载事件
    public GameSceneSO teleportScene;  // 传送的场景
    public Vector3 teleportTarget;
    public void TriggerAction()
    {
        Debug.Log("传送！");
        sceneLoadEventSO.RaiseLoadRequestEvent(teleportScene, teleportTarget, true);
    }
}
