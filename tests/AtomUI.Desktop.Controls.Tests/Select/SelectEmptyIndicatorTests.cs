using System.Reflection;
using AtomUI.Controls.Primitives;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.SelectControl;

public class SelectEmptyIndicatorTests
{
    static SelectEmptyIndicatorTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Empty_Options_Dropdown_Shows_Default_Empty_Indicator()
    {
        var select = new Desktop.Controls.Select
        {
            Width           = 240,
            IsMotionEnabled = false,
            OptionsSource   = new List<ISelectOption>()
        };

        ShowInWindow(select, window =>
        {
            select.IsPopupPinnedOpen = true;
            Dispatcher.UIThread.RunJobs();

            var candidateList = GetCandidateList(select);
            candidateList.ShouldNotBeNull();
            candidateList.IsEffectiveEmptyVisible.ShouldBeTrue();
            candidateList.IsDefaultEmptyIndicatorVisible
                .ShouldBeTrue("空数据源时应显示内置默认空指示（对应 Ant Design 的 No data）");

            var defaultEmpty = window.GetVisualDescendants()
                                     .OfType<Desktop.Controls.Empty>()
                                     .Single(e => e.Name == "DefaultEmptyIndicator");
            defaultEmpty.IsVisible.ShouldBeTrue();
        });
    }

    [Fact]
    public void Custom_EmptyIndicator_Should_Be_Honored_And_Hide_Default()
    {
        var select = new Desktop.Controls.Select
        {
            Width           = 240,
            IsMotionEnabled = false,
            OptionsSource   = new List<ISelectOption>(),
            EmptyIndicator  = "Custom Empty Content"
        };

        ShowInWindow(select, window =>
        {
            select.IsPopupPinnedOpen = true;
            Dispatcher.UIThread.RunJobs();

            var candidateList = GetCandidateList(select);
            candidateList.ShouldNotBeNull();
            candidateList.EmptyIndicator.ShouldBe("Custom Empty Content");
            candidateList.IsDefaultEmptyIndicatorVisible
                .ShouldBeFalse("自定义 EmptyIndicator 时不应再显示默认空指示");

            var indicatorPresenter = window.GetVisualDescendants()
                                           .OfType<ContentPresenter>()
                                           .Single(p => p.Name == "EmptyIndicator");
            indicatorPresenter.IsVisible.ShouldBeTrue();

            var defaultEmpty = window.GetVisualDescendants()
                                     .OfType<Desktop.Controls.Empty>()
                                     .Single(e => e.Name == "DefaultEmptyIndicator");
            defaultEmpty.IsVisible.ShouldBeFalse();
        });
    }

    [Fact]
    public void IsShowEmptyIndicator_False_Hides_Empty_Indicator()
    {
        var select = new Desktop.Controls.Select
        {
            Width               = 240,
            IsMotionEnabled     = false,
            OptionsSource       = new List<ISelectOption>(),
            IsShowEmptyIndicator = false
        };

        ShowInWindow(select, window =>
        {
            select.IsPopupPinnedOpen = true;
            Dispatcher.UIThread.RunJobs();

            var candidateList = GetCandidateList(select);
            candidateList.ShouldNotBeNull();
            candidateList.IsEffectiveEmptyVisible.ShouldBeFalse();
            candidateList.IsDefaultEmptyIndicatorVisible.ShouldBeFalse();
        });
    }

    private static SelectCandidateList? GetCandidateList(Desktop.Controls.Select select)
    {
        var field = typeof(Desktop.Controls.Select).GetField(
            "_candidateList",
            BindingFlags.Instance | BindingFlags.NonPublic);

        field.ShouldNotBeNull();
        return field.GetValue(select) as SelectCandidateList;
    }

    private static void ShowInWindow(Control content, Action<AvaloniaWindow> assertion)
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
            assertion(window);
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
            BindingFlags.Instance | BindingFlags.NonPublic);
        property.ShouldNotBeNull();
        property.SetValue(visualLayerManager, true);
    }
}
