using AtomUI.Theme;
using AtomUI.Theme.Schema;
using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIButton = AtomUI.Desktop.Controls.Button;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;
using AtomUIMasonry = AtomUI.Desktop.Controls.Masonry;

namespace AtomUI.Toolkits.GalleryBase.Tests.Controls;

public class SemanticPartHighlightSessionTests
{
    public SemanticPartHighlightSessionTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Start_Creates_Native_Adorners_And_Dispose_Releases_Them()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(AtomUIButton), out var descriptor).ShouldBeTrue();
        var part = descriptor.Parts.Single(static candidate => candidate.Path == "content");
        var button = new AtomUIButton
        {
            Content = "Semantic Button"
        };

        using var context = ShowInAdornerHost(button);
        var layer = AdornerLayer.GetAdornerLayer(button).ShouldNotBeNull();
        var session = SemanticPartHighlightSession.Start(button, part, registry);

        session.TotalMatchCount.ShouldBe(1);
        session.HighlightedTargetCount.ShouldBe(1);
        var adorner = layer.Children.OfType<SemanticPartAdorner>().Single();
        AdornerLayer.GetAdornedElement(adorner).ShouldNotBeNull();

        session.Dispose();

        AdornerLayer.GetAdornedElement(adorner).ShouldBeNull();
        layer.Children.ShouldNotContain(adorner);
        session.HighlightedTargetCount.ShouldBe(0);
    }

    [Fact]
    public void Start_Uses_The_Thirty_Two_Visible_Target_Budget()
    {
        var owner = new Grid
        {
            Width  = 400,
            Height = 400
        };
        for (var index = 0; index < 40; index++)
        {
            var target = new Border
            {
                Width  = 20,
                Height = 20
            };
            target.Classes.Add("semantic-item");
            owner.Children.Add(target);
        }

        var descriptor = RuntimeDescriptor(typeof(Grid), "BudgetGrid");
        var registry = new SemanticPartRegistry([descriptor]);
        var part = descriptor.Parts.Single(static candidate => candidate.Path == "item");

        using var context = ShowInAdornerHost(owner);
        using var session = SemanticPartHighlightSession.Start(owner, part, registry);

        session.TotalMatchCount.ShouldBe(40);
        session.HighlightedTargetCount.ShouldBe(32);
        context.Layer.Children.OfType<SemanticPartAdorner>().Count().ShouldBe(32);
    }

    [Fact]
    public void Start_Resolves_Targets_Across_Multiple_Owners()
    {
        var ownerA = new Grid
        {
            Width  = 200,
            Height = 200
        };
        var targetA = new Border
        {
            Width  = 20,
            Height = 20
        };
        targetA.Classes.Add("semantic-item");
        ownerA.Children.Add(targetA);

        var ownerB = new Grid
        {
            Width  = 200,
            Height = 200
        };
        var targetB = new Border
        {
            Width  = 20,
            Height = 20
        };
        targetB.Classes.Add("semantic-item");
        ownerB.Children.Add(targetB);

        var host = new StackPanel
        {
            Children =
            {
                ownerA,
                ownerB
            }
        };
        var descriptor = RuntimeDescriptor(typeof(Grid), "BudgetGrid");
        var registry = new SemanticPartRegistry([descriptor]);
        var part = descriptor.Parts.Single(static candidate => candidate.Path == "item");

        using var context = ShowInAdornerHost(host);
        using var session = SemanticPartHighlightSession.Start([ownerA, ownerB], part, registry);

        session.TotalMatchCount.ShouldBe(2);
        session.HighlightedTargetCount.ShouldBe(2);
        context.Layer.Children.OfType<SemanticPartAdorner>().Count().ShouldBe(2);
    }

    [Fact]
    public void Start_Enforces_A_Single_Shared_Budget_Across_Owners()
    {
        var owners = new List<Control>();
        var host = new StackPanel();
        for (var ownerIndex = 0; ownerIndex < 2; ownerIndex++)
        {
            var owner = new Grid
            {
                Width  = 400,
                Height = 400
            };
            for (var index = 0; index < 20; index++)
            {
                var target = new Border
                {
                    Width  = 20,
                    Height = 20
                };
                target.Classes.Add("semantic-item");
                owner.Children.Add(target);
            }

            owners.Add(owner);
            host.Children.Add(owner);
        }

        var descriptor = RuntimeDescriptor(typeof(Grid), "BudgetGrid");
        var registry = new SemanticPartRegistry([descriptor]);
        var part = descriptor.Parts.Single(static candidate => candidate.Path == "item");

        using var context = ShowInAdornerHost(host);
        using var session = SemanticPartHighlightSession.Start(owners, part, registry);

        session.TotalMatchCount.ShouldBe(40);
        session.HighlightedTargetCount.ShouldBe(32);
        context.Layer.Children.OfType<SemanticPartAdorner>().Count().ShouldBe(32);
    }

    [Fact]
    public void Cross_Root_Session_ReResolves_An_Owner_Template_Popup()
    {
        var popupTarget = new Border
        {
            Width  = 80,
            Height = 40
        };
        popupTarget.Classes.Add("semantic-popup-content");
        var owner = new PopupSemanticOwner(popupTarget);
        var descriptor = PopupDescriptor();
        var registry = new SemanticPartRegistry([descriptor]);
        var part = descriptor.Parts.Single(static candidate => candidate.Path == "popup");

        using var context = ShowInNativeAdornerHost(owner);
        using var session = SemanticPartHighlightSession.Start(owner, part, registry);
        session.HighlightedTargetCount.ShouldBe(0);

        owner.Popup.IsOpen = true;
        Dispatcher.UIThread.RunJobs();

        var popupLayer = AdornerLayer.GetAdornerLayer(popupTarget).ShouldNotBeNull();
        session.HighlightedTargetCount.ShouldBe(1);
        popupLayer.Children.OfType<SemanticPartAdorner>().Count().ShouldBe(1);

        owner.Popup.Close();
        Dispatcher.UIThread.RunJobs();

        session.HighlightedTargetCount.ShouldBe(0);
        popupLayer.Children.OfType<SemanticPartAdorner>().ShouldBeEmpty();
    }

    private static ControlSemanticDescriptor RuntimeDescriptor(Type controlType, string id)
    {
        return new ControlSemanticDescriptor(
            controlType,
            new ControlTokenIdentity("GalleryTests", id),
            [
                Root(controlType),
                new SemanticPartDescriptor(
                    "item",
                    "item",
                    "semantic-item",
                    typeof(Control),
                    SemanticPartCardinality.Multiple,
                    SemanticPartCustomization.Selector,
                    null,
                    false,
                    null,
                    true,
                    "> .semantic-item")
            ]);
    }

    private static ControlSemanticDescriptor PopupDescriptor()
    {
        return new ControlSemanticDescriptor(
            typeof(PopupSemanticOwner),
            new ControlTokenIdentity("GalleryTests", "PopupSemanticOwner"),
            [
                Root(typeof(PopupSemanticOwner)),
                new SemanticPartDescriptor(
                    "popup",
                    "popup",
                    "semantic-popup-content",
                    typeof(Border),
                    SemanticPartCardinality.Single,
                    SemanticPartCustomization.Selector,
                    null,
                    true,
                    null,
                    false)
            ]);
    }

    private static SemanticPartDescriptor Root(Type controlType)
    {
        return new SemanticPartDescriptor(
            "root",
            "root",
            null,
            controlType,
            SemanticPartCardinality.Single,
            SemanticPartCustomization.Root,
            null,
            false,
            null,
            false);
    }

    private static AdornerHostContext ShowInAdornerHost(Control control)
    {
        var visualLayerManager = new VisualLayerManager
        {
            EnableAdornerLayer = true,
            Child              = control
        };
        var window = new AtomUIWindow
        {
            Width   = 500,
            Height  = 500,
            Content = visualLayerManager
        };

        window.Show();
        control.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();
        return new AdornerHostContext(
            window,
            AdornerLayer.GetAdornerLayer(control).ShouldNotBeNull());
    }

    private static AdornerHostContext ShowInNativeAdornerHost(Control control)
    {
        var window = new AtomUIWindow
        {
            Width   = 500,
            Height  = 500,
            Content = control
        };

        window.Show();
        control.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();
        return new AdornerHostContext(
            window,
            AdornerLayer.GetAdornerLayer(control).ShouldNotBeNull());
    }

    private sealed class AdornerHostContext : IDisposable
    {
        private readonly Window _window;

        public AdornerHostContext(Window window, AdornerLayer layer)
        {
            _window = window;
            Layer   = layer;
        }

        public AdornerLayer Layer { get; }

        public void Dispose()
        {
            _window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private sealed class PopupSemanticOwner : TemplatedControl
    {
        public PopupSemanticOwner(Control popupChild)
        {
            Popup = new Popup
            {
                Child                 = popupChild,
                PlacementTarget       = this,
                ShouldUseOverlayLayer = true
            };
            Template = new FuncControlTemplate<PopupSemanticOwner>((owner, _) =>
                new Grid
                {
                    Children =
                    {
                        new Border
                        {
                            Width  = 120,
                            Height = 80
                        },
                        owner.Popup
                    }
                });
        }

        public Popup Popup { get; }
    }

}
