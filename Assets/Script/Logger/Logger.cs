using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public static class Logger
{
    private static readonly object sync = new object(); // 同步锁
    private static readonly List<ILogSink> sinks = new List<ILogSink>(); // 输出通道集合
    private static LoggerSettings settings; // 当前设置
    private static bool initialized; // 是否已初始化

    public static LoggerSettings Settings // 公开当前设置
    {
        get { return settings; }
    }

    // 初始化日志系统
    public static void Initialize(LoggerSettings customSettings = null)
    {
        lock (sync)
        {
            if (initialized)
            {
                return;
            }

            settings = customSettings ?? LoadSettingsAsset() ?? LoggerSettings.CreateRuntimeDefault();
            sinks.Clear();

            if (settings.enableConsole)
            {
                AddSinkInternal(new UnityConsoleSink());
            }

            if (settings.enableFile)
            {
                AddSinkInternal(new FileLogSink());
            }

            initialized = true;
        }
    }

    // 关闭日志系统
    public static void Shutdown()
    {
        lock (sync)
        {
            if (!initialized)
            {
                return;
            }

            foreach (var sink in sinks)
            {
                sink.Shutdown();
            }

            sinks.Clear();
            initialized = false;
        }
    }

    // 增加输出通道
    public static void AddSink(ILogSink sink)
    {
        if (sink == null)
        {
            return;
        }

        EnsureInitialized();
        lock (sync)
        {
            sinks.Add(sink);
            sink.Initialize(settings);
        }
    }

    // 移除输出通道
    public static void RemoveSink(ILogSink sink)
    {
        if (sink == null)
        {
            return;
        }

        lock (sync)
        {
            if (!sinks.Remove(sink))
            {
                return;
            }

            sink.Shutdown();
        }
    }

    // 修改最小输出等级
    public static void SetMinLevel(LogLevel level)
    {
        EnsureInitialized();
        settings.minLevel = level;
    }

    // 输出 Trace 等级日志
    public static void Trace(string message, string category = "General", UnityEngine.Object context = null)
    {
        Log(LogLevel.Trace, message, category, context, null);
    }

    // 输出 Debug 等级日志
    public static void Debug(string message, string category = "General", UnityEngine.Object context = null)
    {
        Log(LogLevel.Debug, message, category, context, null);
    }

    // 输出 Info 等级日志
    public static void Info(string message, string category = "General", UnityEngine.Object context = null)
    {
        Log(LogLevel.Info, message, category, context, null);
    }

    // 输出 Warning 等级日志
    public static void Warning(string message, string category = "General", UnityEngine.Object context = null)
    {
        Log(LogLevel.Warning, message, category, context, null);
    }

    // 输出 Error 等级日志
    public static void Error(string message, string category = "General", UnityEngine.Object context = null, Exception exception = null)
    {
        Log(LogLevel.Error, message, category, context, exception);
    }

    // 输出 Critical 等级日志
    public static void Critical(string message, string category = "General", UnityEngine.Object context = null, Exception exception = null)
    {
        Log(LogLevel.Critical, message, category, context, exception);
    }

    // 统一日志入口
    public static void Log(LogLevel level, string message, string category = "General", UnityEngine.Object context = null, Exception exception = null)
    {
        EnsureInitialized();

        if (!ShouldLog(level, category))
        {
            return;
        }

        var stackTrace = BuildStackTrace(level, exception);
        var entry = new LogEntry(DateTime.Now, level, category, message, stackTrace, context);

        lock (sync)
        {
            foreach (var sink in sinks)
            {
                sink.Write(entry, settings);
            }
        }
    }

    // 确保已初始化
    private static void EnsureInitialized()
    {
        if (initialized)
        {
            return;
        }

        Initialize();
    }

    // 内部添加通道并初始化
    private static void AddSinkInternal(ILogSink sink)
    {
        sinks.Add(sink);
        sink.Initialize(settings);
    }

    // 判定是否满足输出条件
    private static bool ShouldLog(LogLevel level, string category)
    {
        if (settings == null)
        {
            return false;
        }

        if (level < settings.minLevel || settings.minLevel == LogLevel.None)
        {
            return false;
        }

        if (string.IsNullOrEmpty(category))
        {
            category = "General";
        }

        if (settings.useCategoryAllowList)
        {
            return settings.allowedCategories != null
                && settings.allowedCategories.Count > 0
                && settings.allowedCategories.Contains(category);
        }

        return settings.blockedCategories == null || !settings.blockedCategories.Contains(category);
    }

    // 生成异常或堆栈文本
    private static string BuildStackTrace(LogLevel level, Exception exception)
    {
        if (exception != null)
        {
            return exception.ToString();
        }

        if (settings == null || !settings.includeStackTrace || level < LogLevel.Error)
        {
            return null;
        }

        var trace = new StackTrace(2, true);
        return trace.ToString();
    }

    // 从 Resources 加载设置资源
    private static LoggerSettings LoadSettingsAsset()
    {
        return Resources.Load<LoggerSettings>("LoggerSettings");
    }
}
