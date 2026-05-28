using UnityEngine;

public class LoggerBootstrap : MonoBehaviour
{
    [SerializeField] private LoggerSettings settings; // 指定日志设置
    [SerializeField] private bool dontDestroyOnLoad = true; // 是否跨场景保留

    // 启动时初始化日志系统
    private void Awake()
    {
        Logger.Initialize(settings);
        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    // 销毁时关闭日志系统
    private void OnDestroy()
    {
        Logger.Shutdown();
    }
}
