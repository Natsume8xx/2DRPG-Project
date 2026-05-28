using System;
using System.Text;
using UnityEngine;

public readonly struct LogEntry
{
    public readonly DateTime Timestamp; // 生成时间
    public readonly LogLevel Level; // 日志等级
    public readonly string Category; // 分类标签
    public readonly string Message; // 主消息内容
    public readonly string StackTrace; // 可选堆栈文本
    public readonly UnityEngine.Object Context; // Unity 上下文对象

    // 构造不可变日志条目
    public LogEntry(DateTime timestamp, LogLevel level, string category, string message, string stackTrace, UnityEngine.Object context)
    {
        Timestamp = timestamp;
        Level = level;
        Category = category;
        Message = message;
        StackTrace = stackTrace;
        Context = context;
    }

    // 根据设置格式化输出内容
    public string Format(LoggerSettings settings, bool includeStackTrace)
    {
        var builder = new StringBuilder(128);
        if (settings != null && settings.includeTimestamp)
        {
            builder.Append(Timestamp.ToString(settings.timestampFormat));
            builder.Append(' ');
        }

        builder.Append('[').Append(Level.ToString().ToUpperInvariant()).Append("] ");
        if (!string.IsNullOrEmpty(Category))
        {
            builder.Append('[').Append(Category).Append("] ");
        }

        builder.Append(Message);

        if (includeStackTrace && !string.IsNullOrEmpty(StackTrace))
        {
            builder.AppendLine();
            builder.Append(StackTrace);
        }

        return builder.ToString();
    }
}
