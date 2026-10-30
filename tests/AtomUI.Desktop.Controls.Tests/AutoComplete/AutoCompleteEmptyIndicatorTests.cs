using System.Reflection;
using AtomUI.Controls.Primitives;
using AtomUI.Desktop.Controls.Primitives;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.AutoComplete;

public class AutoCompleteEmptyIndicatorTests
{
    static AutoCompleteEmptyIndicatorTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Empty_Options_Dropdown_Shows_Default_Empty_Indicator()
    {
        var autoComplete = new Desktop.Controls.AutoComplete
        {
            Width           = 240,
            IsMotionEnabled = false,
            Value           = "A",
            OptionsSource   = new List<IAutoCompleteOption>()
        };

        ShowInWindow(autoComplete, window =>
        {
            autoComplete.IsPopupPinnedOpen = true;
            Dispatcher.UIThread.RunJobs();

            var candidateList = window.GetVisualDescendants()
                                      .OfType<CandidateList>()
                                      .Single(c => c.Name == AutoCompleteThemeConstants.CandidateListPart);
            candidateList.IsEffectiveEmptyVisible.ShouldBeTrue();
            candidateList.IsDefaultEmptyIndicatorVisible
                .ShouldBeTrue("空数据源时应显示内置默认空指示（对应 Ant Design 的 No data）");

            var defaultEmpty = window.GetVisualDescendants()
                                     .OfType<Desktop.Controls.Empty>()
                                     .Single(e => e.Name == "DefaultEmptyIndicator");
            defaultEmpty.IsVisible.ShouldBeTrue();
        });
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
