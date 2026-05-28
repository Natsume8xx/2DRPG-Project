using UnityEngine;

public sealed class UnityConsoleSink : ILogSink
{
    // 初始化控制台输出
    public void Initialize(LoggerSettings settings)
    {
    }

    // 写入 Unity 控制台
    public void Write(LogEntry entry, LoggerSettings settings)
    {
        var message = entry.Format(settings, false);
        if (settings != null && settings.useRichText)
        {
            message = ApplyColor(entry.Level, message);
        }

        switch (entry.Level)
        {
            case LogLevel.Warning:
                Debug.LogWarning(message, entry.Context);
                break;
            case LogLevel.Error:
            case LogLevel.Critical:
                Debug.LogError(message, entry.Context);
                break;
            default:
                Debug.Log(message, entry.Context);
                break;
        }
    }

    // 关闭控制台通道
    public void Shutdown()
    {
    }

    // 根据等级应用颜色
    private static string ApplyColor(LogLevel level, string message)
    {
        var color = "#B8E0FF";
        switch (level)
        {
            case LogLevel.Trace:
                color = "#9AA0A6";
                break;
            case LogLevel.Debug:
                color = "#6FB7FF";
                break;
            case LogLevel.Info:
                color = "#AEE6A0";
                break;
            case LogLevel.Warning:
                color = "#FFD36F";
                break;
            case LogLevel.Error:
                color = "#FF7D7D";
                break;
            case LogLevel.Critical:
                color = "#FF4D4D";
                break;
        }

        return "<color=" + color + ">" + message + "</color>";
    }
}
