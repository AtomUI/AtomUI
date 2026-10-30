using System.Reflection;
using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Threading;
using Shouldly;
using Xunit;

namespace AtomUI.Toolkits.GalleryBase.Tests.Controls;

public class ShowCasePanelDeferredLoadingTests
{
    public ShowCasePanelDeferredLoadingTests() => AvaloniaTestApp.EnsureInitialized();

    [Fact]
    public void Initial_Load_Count_Skips_Items_Without_Deferred_Content()
    {
        var panel = CreatePanel(initialCount: 1);
        var window = new Window { Width = 500, Height = 500, Content = panel };
        try
        {
            window.Show();
            window.UpdateLayout();

            GetLoadableItem(panel).IsDeferredContentMaterialized.ShouldBeTrue();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Viewport_Batch_Skips_Items_Without_Deferred_Content()
    {
        var panel = CreatePanel(initialCount: 0);
        var window = new Window { Width = 500, Height = 500, Content = panel };
        try
        {
            window.Show();
            window.UpdateLayout();
            GetLoadableItem(panel).IsDeferredContentMaterialized.ShouldBeFalse();

            // Run one bounded viewport pass so a regression cannot spin the test dispatcher forever.
            SetPrivateField(panel, "_lastEffectiveViewport", new Rect(-1000, -1000, 2000, 2000));
            SetPrivateField(panel, "_deferredViewportMaterializationQueued", false);
            typeof(ShowCasePanel).GetMethod("MaterializeDeferredContentInViewport",
                                           BindingFlags.Instance | BindingFlags.NonPublic)!
                                 .Invoke(panel, null);

            GetLoadableItem(panel).IsDeferredContentMaterialized.ShouldBeTrue();
            GetPrivateField<bool>(panel, "_deferredViewportMaterializationQueued").ShouldBeFalse();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static ShowCasePanel CreatePanel(int initialCount)
    {
        var panel = new ShowCasePanel
        {
            IsDeferredLoadingEnabled = true,
            InitialDeferredLoadItemCount = initialCount,
            DeferredLoadBatchSize = 2,
            MinItemWidth = 100,
            MaxColumns = 1
        };
        for (var index = 0; index < 3; index++)
        {
            panel.Children.Add(new ShowCaseItem
            {
                Title = $"No template {index}",
                IsDeferredContentEnabled = true
            });
        }
        panel.Children.Add(new ShowCaseItem
        {
            Title = "Deferred loading disabled 1",
            DeferredContentTemplate = CreateTemplate()
        });
        panel.Children.Add(new ShowCaseItem
        {
            Title = "Deferred loading disabled 2",
            DeferredContentTemplate = CreateTemplate()
        });
        panel.Children.Add(new ShowCaseItem
        {
            Title = "Loadable",
            IsDeferredContentEnabled = true,
            DeferredContentTemplate = CreateTemplate()
        });
        return panel;
    }

    private static ShowCaseItem GetLoadableItem(ShowCasePanel panel) =>
        (ShowCaseItem)panel.Children[5];

    private static FuncDataTemplate<object?> CreateTemplate() =>
        new((_, _) => new Border { Width = 40, Height = 40 });

    private static void SetPrivateField(ShowCasePanel panel, string name, object value) =>
        typeof(ShowCasePanel).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!
                             .SetValue(panel, value);

    private static T GetPrivateField<T>(ShowCasePanel panel, string name) =>
        (T)typeof(ShowCasePanel).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!
                                .GetValue(panel)!;
}
