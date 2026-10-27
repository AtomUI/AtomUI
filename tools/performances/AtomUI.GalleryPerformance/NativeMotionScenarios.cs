using System.Reactive.Linq;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using AtomUI.MotionScene;
using AtomUIGallery.Workspace.Views;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using NativeNavMenu = AtomUI.Desktop.Controls.NavMenu;
using NativePopup = AtomUI.Desktop.Controls.Popup;
using ScrollViewer = Avalonia.Controls.ScrollViewer;

namespace AtomUI.GalleryPerformance;

internal sealed class NativeMotionScenarios : IDisposable
{
    internal required Control Target { get; init; }
    internal required TopLevel Root { get; init; }
    internal required string PropertyName { get; init; }
    internal required string[] Directions { get; init; }
    internal required Action<string> Change { get; init; }
    internal required Action<string> Validate { get; init; }
    internal required Func<double> ReadFinalValue { get; init; }
    internal Action<NativeMotionObservation>? ObserveAfterChange { get; init; }
    internal int MinimumSampleMilliseconds { get; init; } = 400;
    internal bool RequiresMotion { get; init; } = true;
    internal string AnimationContract { get; init; } = "Animated values are required";
    private Action? Cleanup { get; init; }
    internal double FinalValue => ReadFinalValue();

    internal static async Task<NativeMotionScenarios> CreateAsync(string name, WorkspaceWindow window)
    {
        if (name is "progress" or "statistic" or "beam" or "beam-offscreen")
        {
            var route = name switch { "progress" => "ProgressBar", "statistic" => "Statistic", _ => "BorderBeam" };
            using var navigation = window.ViewModel!.CaseNavigation.NavigateToCommand.Execute(new EntityKey(route)).Subscribe();
            await NativeMotionProbe.WaitUntilAsync(() => FindTarget(name, window) is { IsLoaded: true, IsEffectivelyVisible: true });
            await Task.Delay(1200);
            var target = FindTarget(name, window)!;
            target.BringIntoView();
            await Task.Delay(300);
            if (name == "progress")
            {
                var progress = (AtomUI.Controls.Commons.AbstractProgressBar)target;
                return new NativeMotionScenarios
                {
                    Root = window, Target = target, PropertyName = "Value", Directions = ["low", "high"],
                    MinimumSampleMilliseconds = 1000,
                    Change = direction => progress.Value = progress.Minimum + (progress.Maximum - progress.Minimum) * (direction == "low" ? 0.2 : 0.8),
                    Validate = direction => Require(Math.Abs(progress.Value - (progress.Minimum + (progress.Maximum - progress.Minimum) * (direction == "low" ? 0.2 : 0.8))) < 0.001, "Progress did not settle"),
                    ReadFinalValue = () => progress.Value
                };
            }
            if (name == "statistic")
            {
                var statistic = (StatisticCountUp)target;
                return new NativeMotionScenarios
                {
                    Root = window, Target = target, PropertyName = "AnimatingValue", Directions = ["low", "high"],
                    MinimumSampleMilliseconds = 1000,
                    Change = direction => statistic.EndValue = direction == "low" ? 1000 : 9000,
                    Validate = direction => Require(Math.Abs(statistic.AnimatingValue - (direction == "low" ? 1000 : 9000)) < 0.001, "Statistic did not settle"),
                    ReadFinalValue = () => statistic.AnimatingValue
                };
            }

            var scroll = target.GetVisualAncestors().OfType<ScrollViewer>().FirstOrDefault(view => view.Extent.Height > view.Viewport.Height);
            var offset = scroll?.Offset ?? default;
            if (name == "beam-offscreen")
            {
                Require(scroll is not null, "Beam scene has no scrollable viewport");
                scroll!.Offset = new Vector(offset.X, scroll.Extent.Height - scroll.Viewport.Height);
                await Task.Delay(300);
                var point = target.TranslatePoint(default, scroll);
                Require(point.HasValue && !new Rect(scroll.Viewport).Intersects(new Rect(point.Value, target.Bounds.Size)), "Beam target is not outside its viewport");
            }
            return new NativeMotionScenarios
            {
                Root = window, Target = target, PropertyName = "Progress", Directions = ["observe"],
                Change = _ => { }, Validate = _ => Require(target.IsEffectivelyVisible, "Beam was hidden instead of scrolled"),
                ReadFinalValue = () => target.Bounds.Width,
                RequiresMotion = name != "beam-offscreen",
                AnimationContract = name == "beam-offscreen" ? "Offscreen loop window; zero updates are a valid observation" : "Visible Beam progress updates are required",
                Cleanup = () => { if (scroll is not null) { scroll.Offset = offset; } }
            };
        }

        var menu = window.GetVisualDescendants().OfType<NativeNavMenu>().First();
        menu.IsInlineCollapsed = name == "popup";
        await Task.Delay(500);
        if (name == "sidebar")
        {
            var expandedWidth = menu.Bounds.Width;
            return new NativeMotionScenarios
            {
                Root = window, Target = menu, PropertyName = "Width", Directions = ["collapse", "expand"],
                Change = direction => menu.IsInlineCollapsed = direction == "collapse",
                Validate = direction => Require(Math.Abs(menu.Bounds.Width - (direction == "collapse" ? menu.InlineCollapsedWidth : expandedWidth)) <= 0.5, "Sidebar did not settle"),
                ReadFinalValue = () => menu.Bounds.Width
            };
        }

        var item = menu.GetVisualDescendants().OfType<INavMenuItem>().First(node => node.HasSubMenu);
        if (name == "popup")
        {
            // The template popup is not a visual/logical descendant while hosted in a separate root.
            // Read the existing container reference in this diagnostic tool; do not add a product hook.
            var popup = item.GetType().GetProperty("Popup", System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)?.GetValue(item) as NativePopup
                ?? throw new NotSupportedException("NavMenu popup diagnostic reference changed.");
            BaseMotionActor? actor = null;
            return new NativeMotionScenarios
            {
                Root = window, Target = menu, PropertyName = "Opacity", Directions = ["open", "close"],
                Change = direction => { if (direction == "open") { item.Open(); } else { item.Close(); } },
                ObserveAfterChange = observation =>
                {
                    if (popup.IsOpen)
                    {
                        actor = popup.Child?.GetVisualAncestors().OfType<BaseMotionActor>().FirstOrDefault()
                            ?? throw new InvalidOperationException("Opened popup has no motion actor.");
                    }
                    observation.ObserveTarget(actor ?? throw new InvalidOperationException("Popup actor was not captured."));
                },
                Validate = direction => Require(popup.IsOpen == (direction == "open"), "Popup did not settle"),
                ReadFinalValue = () => actor?.Opacity ?? 0,
                RequiresMotion = popup.OpenMotion is not null && popup.CloseMotion is not null,
                AnimationContract = $"Real NavMenu Popup: OpenMotion={popup.OpenMotion?.GetType().Name ?? "none"}; CloseMotion={popup.CloseMotion?.GetType().Name ?? "none"}. Absent motions are reported as static open/close, not FPS.",
                Cleanup = item.Close
            };
        }

        item.IsSubMenuOpen = false;
        await Task.Delay(500);
        var contentActor = ((Control)item).GetVisualDescendants().OfType<BaseMotionActor>().First(actor => actor.Name == "PART_ChildItemsLayoutTransform");
        return new NativeMotionScenarios
        {
            Root = window, Target = contentActor, PropertyName = "Opacity", Directions = ["open", "close"],
            Change = direction => item.IsSubMenuOpen = direction == "open",
            Validate = direction => Require(item.IsSubMenuOpen == (direction == "open") && contentActor.IsVisible == (direction == "open"), "Submenu did not settle"),
            ReadFinalValue = () => contentActor.Bounds.Height
        };
    }

    private static Control? FindTarget(string name, WorkspaceWindow window)
    {
        return window.GetVisualDescendants().OfType<Control>().FirstOrDefault(control => control.IsEffectivelyVisible &&
            control.Bounds.Width > 0 && (name switch
            {
                "progress" => control is AtomUI.Controls.Commons.AbstractProgressBar,
                "statistic" => control is StatisticCountUp,
                _ => control.GetType().Name == "BorderBeamPresenter"
            }));
    }

    private static void Require(bool condition, string error)
    {
        if (!condition)
        {
            throw new InvalidOperationException(error);
        }
    }

    public void Dispose() => Cleanup?.Invoke();
}
