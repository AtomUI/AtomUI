using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AtomUI.Controls;

/// <summary>Splash 默认主题。自定义主题可整体替换，或合并本字典后用 BasedOn 局部覆盖。</summary>
public partial class SplashDefaultTheme : ResourceDictionary
{
    public SplashDefaultTheme()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
