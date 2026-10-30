using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AtomUI.Controls.Primitives;
using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIButton = AtomUI.Desktop.Controls.Button;
using AtomUILineEdit = AtomUI.Desktop.Controls.LineEdit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUIGallery.Tests.ShowCases;

public class CustomizeThemeShowCasePageTests
{
    [Fact]
    public void CustomizeTheme_Token_Only_Configs_Inherit_Current_Algorithm()
    {
        var viewModel = new AtomUIGallery.ShowCases.CustomizeTheme.CustomizeThemeViewModel(null!);
        var configs = new[]
        {
            viewModel.GreenThemeConfig,
            viewModel.RedThemeConfig,
            viewModel.PurpleThemeConfig,
            viewModel.NestedBlueThemeConfig,
            viewModel.NestedGreenThemeConfig,
            viewModel.RuntimeThemeConfig
        };

        foreach (var config in configs)
        {
            config.Algorithms.ShouldBeNull();
        }
    }

    [Fact]
    public void CustomizeTheme_Inherited_Config_Does_Not_Force_Light_Container_Color()
    {
        var viewModel = new AtomUIGallery.ShowCases.CustomizeTheme.CustomizeThemeViewModel(null!);

        viewModel.GreenThemeConfig.Tokens
                 .ContainsKey(nameof(AtomUI.Theme.DesignTokens.DesignToken.ColorBgContainer))
                 .ShouldBeFalse();
    }

    [Fact]
    public void CustomizeTheme_Control_Algorithm_Configs_Render_Component_Primary_Colors()
    {
        AvaloniaTestApp.EnsureInitialized();

        var viewModel = new AtomUIGallery.ShowCases.CustomizeTheme.CustomizeThemeViewModel(null!);
        foreach (var config in new[]
                 {
                     viewModel.ControlAlgorithmEnabledConfig,
                     viewModel.ControlAlgorithmDisabledConfig
                 })
        {
            var lineEdit = new AtomUILineEdit
            {
                Width           = 200,
                IsMotionEnabled = false
            };
            var button = new AtomUIButton
            {
                ButtonType      = AtomUI.Desktop.Controls.ButtonType.Primary,
                Content         = "Submit",
                IsMotionEnabled = false
            };
            var provider = new ThemeConfigProvider
            {
                Config = config,
                Child  = new StackPanel
                {
                    Children = { lineEdit, button }
                }
            };

            ShowInWindow(provider, () =>
            {
                var decoratedBox = lineEdit.GetVisualDescendants()
                                           .OfType<Control>()
                                           .Single(item => item.Name == "PART_AddOnDecoratedBox");
                decoratedBox.Focus();
                Dispatcher.UIThread.RunJobs();

                var contentFrame = lineEdit.GetVisualDescendants()
                                           .OfType<PixelAlignedBorder>()
                                           .Single(control => control.Name == "PART_ContentFrame");
                BrushColor(button.Background).ShouldBe(Color.Parse("#00b96b"));
                BrushColor(contentFrame.BorderBrush).ShouldBe(Color.Parse("#eb2f96"));
            });
        }
    }

    private static string ExtractCustomizeThemeExampleItems(string source)
    {
        const string firstItemMarker  = "<gallery:ShowCaseItem";
        const string panelCloseMarker = "</gallery:ShowCasePanel>";

        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        var panelCloseStart = source.IndexOf(panelCloseMarker, firstItemStart, StringComparison.Ordinal);
        panelCloseStart.ShouldBeGreaterThan(firstItemStart);

        return source[firstItemStart..panelCloseStart];
    }

    private static string NormalizeMarkup(string source)
    {
        return ShowCaseSnapshotMarkup.Normalize(source);
    }

    private static string ComputeSha256(string source)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(source));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string ReadSnapshotHash(string source)
    {
        return source
            .Split('\n')
            .First(line => line.StartsWith("sha256:", StringComparison.Ordinal))
            .Split(':', 2)[1]
            .Trim();
    }

    private static int ReadSnapshotCount(string source)
    {
        return int.Parse(source
            .Split('\n')
            .First(line => line.StartsWith("count:", StringComparison.Ordinal))
            .Split(':', 2)[1]
            .Trim());
    }

    private static int CountShowCaseItemElements(string source)
    {
        return Regex.Matches(source, @"<gallery:ShowCaseItem(\s|>)", RegexOptions.CultureInvariant).Count;
    }

    private static int CountOccurrences(string source, string value)
    {
        var count      = 0;
        var startIndex = 0;
        while (true)
        {
            var matchIndex = source.IndexOf(value, startIndex, StringComparison.Ordinal);
            if (matchIndex < 0)
            {
                return count;
            }

            count++;
            startIndex = matchIndex + value.Length;
        }
    }

    private static string ReadRepoFile(string relativePath)
    {
        var path = GetRepoFile(relativePath);
        File.Exists(path).ShouldBeTrue($"Expected repository file to exist: {relativePath}");
        return File.ReadAllText(path);
    }

    private static string GetRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return Path.Combine(AppContext.BaseDirectory, relativePath);
    }

    private static Color BrushColor(IBrush? brush)
    {
        brush.ShouldNotBeNull();
        brush.ShouldBeAssignableTo<ISolidColorBrush>();
        return ((ISolidColorBrush)brush).Color;
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 480,
            Height  = 240,
            Content = content
        };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
