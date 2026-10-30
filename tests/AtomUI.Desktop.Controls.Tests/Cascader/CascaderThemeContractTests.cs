using AtomUI.Controls.Primitives;
using AtomUI.Icons.AntDesign;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Cascader;

public class CascaderThemeContractTests
{
    static CascaderThemeContractTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Template_Right_AddOn_And_Handle_State_Are_Bound_From_Axaml()
    {
        var themeSource = ReadRepoFile("src/AtomUI.Desktop.Controls/Cascader/Themes/CascaderTheme.axaml");

        themeSource.ShouldContain("MaxCount=\"{CompiledBinding $parent[atom:Cascader].MaxCount}\"");
        themeSource.ShouldContain("SelectedCount=\"{CompiledBinding $parent[atom:Cascader].SelectedCount}\"");
        themeSource.ShouldContain("IsVisible=\"{CompiledBinding $parent[atom:Cascader].IsShowMaxCountIndicator}\"");
        themeSource.ShouldContain("Content=\"{CompiledBinding $parent[atom:Cascader].ContentRightAddOn}\"");
        themeSource.ShouldContain("ContentTemplate=\"{CompiledBinding $parent[atom:Cascader].ContentRightAddOnTemplate}\"");
        themeSource.ShouldContain("IsInputHover=\"{CompiledBinding $parent[atom:CascaderAddOnDecoratedBox].IsInnerBoxHover}\"");
        themeSource.ShouldContain("IsInputPressed=\"{CompiledBinding $parent[atom:CascaderAddOnDecoratedBox].IsInnerBoxPressed}\"");
    }

    [Fact]
    public void Template_Binds_Right_AddOn_Count_And_Handle_State_At_Runtime()
    {
        var jack = new CascaderOption
        {
            Header = "Jack",
            Value  = "jack"
        };
        var lucy = new CascaderOption
        {
            Header = "Lucy",
            Value  = "lucy"
        };
        var rightAddOn  = new TextBlock { Text = "extra" };
        var suffixIcon  = new DownOutlined();
        var loadingIcon = new LoadingOutlined();
        var cascader = new Desktop.Controls.Cascader
        {
            Width                   = 240,
            IsMultiple              = true,
            IsFilterEnabled         = true,
            IsAllowClear            = true,
            IsMotionEnabled         = false,
            MaxCount                = 3,
            IsShowMaxCountIndicator = true,
            SelectedOptions         = [jack, lucy],
            ContentRightAddOn       = rightAddOn,
            SuffixIcon              = suffixIcon,
            SuffixLoadingIcon       = loadingIcon
        };

        ShowInWindow(cascader, () =>
        {
            var indicator = GetVisualDescendant<SelectMaxCountIndicator>(cascader, "PART_SelectMaxCountIndicator");
            indicator.MaxCount.ShouldBe(3);
            indicator.SelectedCount.ShouldBe(2);
            indicator.IsVisible.ShouldBeTrue();

            var contentPresenter = GetVisualDescendant<ContentPresenter>(cascader, "PART_ContentRightAddOnPresenter");
            contentPresenter.Content.ShouldBeSameAs(rightAddOn);
            contentPresenter.IsVisible.ShouldBeTrue();

            var handle = GetVisualDescendant<SelectHandle>(cascader, "PART_SelectHandle");
            handle.OpenIndicator.ShouldBeSameAs(suffixIcon);
            handle.LoadingIcon.ShouldBeSameAs(loadingIcon);
            handle.IsFilterEnabled.ShouldBeTrue();
            handle.IsMotionEnabled.ShouldBeFalse();
            handle.IsAllowClear.ShouldBeTrue();
            handle.IsSelectionEmpty.ShouldBeFalse();

            var addOnBox = GetVisualDescendant<AddOnDecoratedBox>(
                cascader,
                AddOnDecoratedBox.AddOnDecoratedBoxPart);

            addOnBox.IsInnerBoxHover   = true;
            addOnBox.IsInnerBoxPressed = true;

            handle.IsInputHover.ShouldBeTrue();
            handle.IsInputPressed.ShouldBeTrue();
        });
    }

    private static string ReadRepoFile(string relativePath)
    {
        return File.ReadAllText(Path.Combine(GetRepositoryRoot(), relativePath));
    }

    private static string GetRepositoryRoot()
    {
        var directory = AppContext.BaseDirectory;
        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory, "AtomUI.slnx")))
            {
                return directory;
            }

            directory = Directory.GetParent(directory)?.FullName;
        }

        throw new DirectoryNotFoundException("Unable to locate repository root.");
    }

    private static T GetVisualDescendant<T>(Control control, string name)
        where T : Control
    {
        var descendant = control.GetVisualDescendants()
                                .OfType<T>()
                                .SingleOrDefault(x => x.Name == name);
        descendant.ShouldNotBeNull();
        return descendant;
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var overlayPanel = new ScopeAwareOverlayLayerPanel
        {
            Width  = 420,
            Height = 320
        };
        overlayPanel.Children.Add(content);

        var visualLayerManager = new VisualLayerManager
        {
            Child = overlayPanel
        };
        EnablePopupOverlayLayer(visualLayerManager);

        var window = new AvaloniaWindow
        {
            Width   = 420,
            Height  = 320,
            Content = visualLayerManager
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
        }
    }

    private static void EnablePopupOverlayLayer(VisualLayerManager visualLayerManager)
    {
        var property = typeof(VisualLayerManager).GetProperty(
            "EnablePopupOverlayLayer",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        property.ShouldNotBeNull();
        property.SetValue(visualLayerManager, true);
    }
}
