namespace AtomUI;

/// <summary>应用汇报的启动进度。Message 为 null 表示不改消息；Fraction 为 null 表示保持不确定进度，否则取 0–1。</summary>
public readonly record struct StartupProgress(string? Message, double? Fraction = null);
