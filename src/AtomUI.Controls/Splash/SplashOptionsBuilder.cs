using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace AtomUI.Controls;

/// <summary>Splash 内容与主题配置。图片只保存工厂：配置回调执行时平台尚未初始化，无法创建位图。</summary>
public sealed class SplashOptionsBuilder
{
    internal Func<IImage>? Logo { get; private set; }
    internal Func<IImage>? BackgroundImage { get; private set; }
    internal string? Title { get; private set; }
    internal string? Message { get; private set; }
    internal Func<ResourceDictionary>? CreateControlTheme { get; private set; }

    /// <summary>Logo 来源：avares:// 或文件 URI。在显示 Splash 时于 UI 线程加载（配置回调执行时还无法创建位图）。</summary>
    public SplashOptionsBuilder WithLogo(Uri source)
    {
        ArgumentNullException.ThrowIfNull(source);
        Logo = () => LoadBitmap(source);
        return this;
    }

    /// <summary>Logo 工厂（矢量图等自定义来源），在显示 Splash 时于 UI 线程调用。</summary>
    public SplashOptionsBuilder WithLogo(Func<IImage> factory)
    {
        Logo = factory ?? throw new ArgumentNullException(nameof(factory));
        return this;
    }

    public SplashOptionsBuilder WithBackgroundImage(Uri source)
    {
        ArgumentNullException.ThrowIfNull(source);
        BackgroundImage = () => LoadBitmap(source);
        return this;
    }

    public SplashOptionsBuilder WithBackgroundImage(Func<IImage> factory)
    {
        BackgroundImage = factory ?? throw new ArgumentNullException(nameof(factory));
        return this;
    }

    private static Bitmap LoadBitmap(Uri source) =>
        source.Scheme == "avares"
            ? new Bitmap(AssetLoader.Open(source))
            : new Bitmap(source.IsFile ? source.LocalPath : source.ToString());

    public SplashOptionsBuilder WithTitle(string title)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
        return this;
    }

    public SplashOptionsBuilder WithMessage(string message)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
        return this;
    }

    /// <summary>
    /// 指定自定义 Splash 主题：x:Class 资源字典，内含 TargetType 为 Splash（可选 SplashWindow）的隐式 ControlTheme。
    /// 只能使用 Splash 控件、Avalonia 基础元素与全局 Token；不能放 AtomUI 控件，不能依赖自定义字体。
    /// </summary>
    public SplashOptionsBuilder WithControlTheme<TTheme>() where TTheme : ResourceDictionary, new()
    {
        CreateControlTheme = static () => new TTheme();
        return this;
    }

    internal SplashContent Build() => new(Logo, BackgroundImage, Title, Message, CreateControlTheme);
}
