using Avalonia.Media;

namespace AtomUI;

/// <summary>
/// 应用提供的 Splash 内容（由 UseSplash 收集）。图片是工厂：配置回调早于平台初始化，此时无法创建位图，
/// 呈现器在显示 Splash 时于 UI 线程调用工厂。
/// </summary>
internal sealed record AtomUISplashContent(Func<IImage>? Logo, Func<IImage>? BackgroundImage, string? Title, string? Message,
    Func<Avalonia.Controls.ResourceDictionary>? CreateControlTheme);

internal sealed record AtomUISplashRegistration(SplashMode Mode, Func<IAtomUISplashPresenter> CreatePresenter,
    AtomUISplashContent Content);

/// <summary>Core 与 AtomUI.Controls 之间的 Splash 接缝。全部方法在 UI 线程调用。</summary>
internal interface IAtomUISplashPresenter
{
    /// <summary>创建并显示 Splash；返回 false 表示本次不显示（原因已写日志）。</summary>
    bool Show(IReadOnlyDictionary<object, object?> globalResources, AtomUISplashContent content);

    /// <summary>Splash 首个合成批次渲染完成。</summary>
    Task FirstFrameRendered { get; }

    void ReportProgress(AtomUIStartupProgress progress);

    /// <summary>切换错误态；之后用户可关闭，关闭后 <see cref="ErrorAcknowledged"/> 完成。</summary>
    void ShowError(Exception exception);

    Task ErrorAcknowledged { get; }

    /// <summary>主窗体首帧完成后隐藏 Splash；窗口随后由启动编排安全关闭。</summary>
    Task HideAsync();

    /// <summary>不等待动画立即关闭；用于启动取消或呈现失败时释放窗口。</summary>
    void Close();

    /// <summary>用户在交接前关闭了 Splash（不含错误态的确认关闭与交接关闭）。</summary>
    event EventHandler? ClosedByUser;
}
