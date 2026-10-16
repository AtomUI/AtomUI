using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using AtomUI.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Resources;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Shouldly;
using Xunit;

namespace AtomUI.Controls.Tests.Splash;

public class SplashDefaultThemeTests
{
    // 契约：Splash 在内核提交之前显示，默认主题只能依赖全局 Token 与基础元素。
    // 运行时资源查找无法守住这一点（测试应用已提交，应用级资源里什么都有），因此直接检查主题源文件：
    // 只允许 SharedTokenResource、只允许 Splash 自有的 atom 类型，并核对每个全局 Token 都在 Prepare 结果中。
    private static readonly HashSet<string> AllowedAtomTypes = new(StringComparer.Ordinal)
    {
        "Splash", "SplashWindow", "SplashProgressBar", "SharedTokenResource"
    };

    [Fact]
    public void Default_theme_references_only_global_tokens_and_splash_types()
    {
        var source = File.ReadAllText(ThemeSourcePath());
        var atomTypes = Regex.Matches(source, @"atom[:|](\w+)").Select(match => match.Groups[1].Value).ToHashSet();
        atomTypes.Except(AllowedAtomTypes).ShouldBeEmpty();
        // 只允许 SharedTokenResource；控件 Token 扩展（任何不以 Shared 开头的 *TokenResource）一律不许出现。
        Regex.Matches(source, @"(?<!Shared)TokenResource ").Count.ShouldBe(0);
        source.ShouldNotContain("FontFamily");

        var prepared = ThemeStartupPreparation.Prepare(
            new ThemeManagerBuilder(typeof(Avalonia.Application)).CaptureStartupInputs());
        var resources = prepared.GlobalResources(ThemeAppearance.Light);
        var used = Regex.Matches(source, @"SharedTokenResource (\w+)").Select(match => match.Groups[1].Value).Distinct();
        used.Where(name => !resources.ContainsKey(Enum.Parse<SharedTokenKind>(name))).ShouldBeEmpty();
    }

    // 契约：自定义主题可以"合并默认主题 + BasedOn"局部覆盖。模板内的本地值优先级高于样式，写进 ControlTemplate 的
    // 外观值无法被派生主题覆盖（启动指南的示例改不动卡片宽度），因此模板只描述结构，外观值都写在样式里。
    [Fact]
    public void Default_theme_templates_keep_appearance_values_in_styles()
    {
        var source = File.ReadAllText(ThemeSourcePath());
        var templates = Regex.Matches(source, @"<ControlTemplate>(.*?)</ControlTemplate>", RegexOptions.Singleline);
        templates.Count.ShouldBe(2);
        foreach (Match template in templates)
        {
            template.Value.ShouldNotContain("SharedTokenResource");
            Regex.Matches(template.Value,
                     @"\s(Width|Height|Margin|Padding|Spacing|Background|Foreground|FontSize|CornerRadius|BoxShadow|Stroke|StrokeThickness|IndicatorBrush|TrackBrush)=""")
                 .Select(match => match.Groups[1].Value)
                 .ShouldBeEmpty();
        }
    }

    [AvaloniaFact]
    public void Splash_window_applies_the_default_theme_from_its_own_resources()
    {
        var prepared = ThemeStartupPreparation.Prepare(
            new ThemeManagerBuilder(typeof(Avalonia.Application)).CaptureStartupInputs());
        var window = new SplashWindow();
        window.Initialize(prepared.GlobalResources(ThemeAppearance.Light),
            new AtomUISplashContent(null, null, "Title", "Message", null));
        window.Show();
        try
        {
            // 窗口自身的 ControlTheme 必须生效（无边框、随内容定尺寸）；仅检查内部 Splash 模板无法发现
            // 窗口在构造期就已完成样式化、后合并的主题从未应用的问题。
            window.WindowDecorations.ShouldBe(WindowDecorations.None);
            window.SizeToContent.ShouldBe(SizeToContent.WidthAndHeight);
            window.Splash.ShouldNotBeNull();
            // ApplyTemplate() 在 Avalonia 12.1.3 返回 void，无法断言返回值；
            // 改为断言模板已从自带的默认主题实例化：模板子树里存在默认主题声明的关闭按钮部件。
            window.Splash!.ApplyTemplate();
            window.Splash!.GetTemplateDescendants()
                  .OfType<Avalonia.Controls.Shapes.Path>()
                  .ShouldContain(path => path.Name == "PART_CloseButton");
            Avalonia.VisualTree.VisualExtensions.GetVisualChildren(window.Splash).ShouldNotBeEmpty();
        }
        finally
        {
            window.Close();
        }
    }

    private static string ThemeSourcePath([CallerFilePath] string testFile = "")
    {
        var repository = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(testFile)!, "..", "..", ".."));
        return Path.Combine(repository, "src", "AtomUI.Controls", "Splash", "Themes", "SplashDefaultTheme.axaml");
    }
}
