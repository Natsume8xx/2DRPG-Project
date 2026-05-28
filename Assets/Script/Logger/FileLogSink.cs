using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Pool;

public sealed class FileLogSink : ILogSink
{
    private readonly object fileLock = new object(); // 文件写入锁
    private StreamWriter writer; // 文件写入器
    private string filePath; // 日志文件路径

    // 初始化文件输出
    public void Initialize(LoggerSettings settings)
    {
        filePath = ResolveLogPath(settings);
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        writer = new StreamWriter(filePath, true, new UTF8Encoding(false));
        writer.AutoFlush = true;
    }

    // 写入日志到文件
    public void Write(LogEntry entry, LoggerSettings settings)
    {
        if (writer == null)
        {
            return;
        }

        var line = entry.Format(settings, settings != null && settings.includeStackTrace);
        lock (fileLock)
        {
            writer.WriteLine(line);
        }
    }

    // 关闭文件通道
    public void Shutdown()
    {
        if (writer == null)
        {
            return;
        }

        lock (fileLock)
        {
            writer.Flush();
            writer.Dispose();
            writer = null;
        }
    }

    // 解析日志文件路径
    private static string ResolveLogPath(LoggerSettings settings)
    {
        if (settings != null)
        {
            return settings.GetLogFilePath();
        }

        return Path.Combine(Application.persistentDataPath, "Logs", "game.log");
    }

    //public ObjectPool<Logger>
}
