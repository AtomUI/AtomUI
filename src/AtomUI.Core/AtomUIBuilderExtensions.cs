namespace AtomUI;

/// <summary>
/// 分级启动的 Builder 扩展（opt-in）。IAtomUIBuilder 公开表面保持不变（契约测试钉死
/// {Localization, Theme}），分期声明经扩展访问——内部向下转型 AtomUIBuilder。
/// </summary>
public static class AtomUIBuilderExtensions
{
    /// <summary>
    /// 声明 Stage 2 注册块（分级启动）：支持包、主题来源和本地化声明，在主窗体就绪后按声明序执行。
    /// 本方法及服务、初始主题、字体、应用标识、initializer 等初始配置仅允许在初始注册窗口调用。
    /// 不支持的 Deferred 操作会立即抛错，未捕获的异常使 Stage 2 在发布前失败。
    /// </summary>
    public static IAtomUIBuilder Deferred(this IAtomUIBuilder builder, Action registration)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(registration);
        return ((AtomUIBuilder)builder).Deferred(registration);
    }

}
