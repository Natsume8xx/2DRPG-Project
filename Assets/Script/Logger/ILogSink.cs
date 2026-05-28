// 输出通道类的抽象接口
public interface ILogSink
{
    // 初始化输出通道
    void Initialize(LoggerSettings settings);
    // 写入一条日志
    void Write(LogEntry entry, LoggerSettings settings);
    // 释放通道资源
    void Shutdown();
}
