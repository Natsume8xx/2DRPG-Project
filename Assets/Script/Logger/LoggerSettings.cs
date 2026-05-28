using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(menuName = "Logger/Logger Settings", fileName = "LoggerSettings")]
public class LoggerSettings : ScriptableObject
{
    public LogLevel minLevel = LogLevel.Info; // 最低输出等级
    public bool enableConsole = true; // 是否启用控制台
    public bool enableFile = false; // 是否启用文件输出
    public string fileName = "game.log"; // 日志文件名
    public string fileDirectory = "Logs"; // 日志目录名
    public bool includeTimestamp = true; // 是否包含时间戳
    public string timestampFormat = "yyyy-MM-dd HH:mm:ss.fff"; // 时间戳格式
    public bool includeStackTrace = true; // 是否包含堆栈
    public bool useRichText = true; // 控制台富文本着色

    public bool useCategoryAllowList = false; // 是否启用分类白名单
    public List<string> allowedCategories = new List<string>(); // 允许输出的分类
    public List<string> blockedCategories = new List<string>(); // 禁止输出的分类

    // 获取日志文件的完整路径
    public string GetLogFilePath()
    {
        var directory = string.IsNullOrEmpty(fileDirectory)
            ? Application.persistentDataPath
            : Path.Combine(Application.persistentDataPath, fileDirectory);

        var safeName = string.IsNullOrEmpty(fileName) ? "game.log" : fileName;
        return Path.Combine(directory, safeName);
    }

    // 创建一个运行时默认设置实例
    public static LoggerSettings CreateRuntimeDefault()
    {
        return CreateInstance<LoggerSettings>();
    }
}
