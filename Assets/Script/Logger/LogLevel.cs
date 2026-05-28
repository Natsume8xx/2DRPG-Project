public enum LogLevel
{
    Trace = 0, // 最细粒度的诊断输出
    Debug = 1, // 调试阶段的详细信息
    Info = 2, // 正常运行信息
    Warning = 3, // 可恢复的潜在问题
    Error = 4, // 需要关注的错误
    Critical = 5, // 严重故障
    None = 6 // 关闭所有日志
}
