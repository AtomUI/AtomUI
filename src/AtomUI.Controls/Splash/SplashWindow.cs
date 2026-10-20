using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace AtomUI.Controls;

/// <summary>
/// Splash 宿主窗口：继承 Avalonia Window（不依赖 AtomUI 主题）。资源完全自带：全局 Token、默认主题、可选自定义主题。
/// </summary>
public class SplashWindow : Window
{
    internal Splash? Splash { get; private set; }

    // Avalonia 的 Window 把 StyleKeyOverride 固定为 typeof(Window)：不覆盖的话，派生窗口既找不到以
    // SplashWindow 为键的隐式主题，显式指定的 SplashWindow 主题也会因 TargetType 与样式键不匹配而不生效。
    protected override Type StyleKeyOverride => typeof(SplashWindow);

    internal void Initialize(IReadOnlyDictionary<object, object?> globalResources, SplashContent content)
    {
        var tokens = new ResourceDictionary();
        foreach (var (key, value) in globalResources)
        {
            tokens[key] = value;
        }
        Resources.MergedDictionaries.Add(tokens);
        Resources.MergedDictionaries.Add(new SplashDefaultTheme());
        if (content.CreateControlTheme is { } createTheme)
        {
            // 后合并者优先：自定义主题中的同键 ControlTheme 覆盖默认主题。
            Resources.MergedDictionaries.Add(createTheme());
        }
        Splash = new Splash
        {
            // 图片工厂在此（UI 线程、平台已初始化）调用：配置回调执行时还无法创建位图。
            Logo = content.Logo?.Invoke(),
            BackgroundImage = content.BackgroundImage?.Invoke(),
            Title = content.Title,
            Message = content.Message
        };
        Content = Splash;
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        Splash?.SetOpaque(ActualTransparencyLevel == WindowTransparencyLevel.None);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Escape && Splash is { Status: SplashStatus.Error } splash)
        {
            splash.RaiseEvent(new RoutedEventArgs(Splash.CloseRequestedEvent));
            e.Handled = true;
        }
    }
}
