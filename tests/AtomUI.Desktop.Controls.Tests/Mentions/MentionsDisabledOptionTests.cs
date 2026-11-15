using System.Reflection;
using AtomUI.Controls.Primitives;
using AtomUI.Desktop.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomUIMentions = AtomUI.Desktop.Controls.Mentions;

namespace AtomUI.Desktop.Controls.Tests.Mention;

// IMentionOption.IsEnabled = false: the option renders disabled and can never become the active candidate or be
// committed, whether reached by keyboard, pointer, the open-time pre-selection or after container recycling.
public class MentionsDisabledOptionTests
{
    private const int LargeCount = 200;

    static MentionsDisabledOptionTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Disabled_Option_Container_Renders_Disabled()
    {
        using var host = MentionsHost.Open(Options(5, disabled: [2]));

        var disabledContainer = host.Container(2);
        disabledContainer.IsEnabled.ShouldBeFalse();
        disabledContainer.Classes.Contains(":disabled").ShouldBeTrue();
        host.Container(1).IsEnabled.ShouldBeTrue();
        host.Container(3).IsEnabled.ShouldBeTrue();
    }

    [Fact]
    public void Pre_Selection_Skips_Leading_Disabled_Options()
    {
        var options = Options(5, disabled: [0, 1]);
        using var host = MentionsHost.Open(options);

        host.List.SelectedItem.ShouldBeSameAs(options[2]);
        host.Press(Key.Enter);

        host.Mentions.Value.ShouldNotBeNull();
        host.Mentions.Value.ShouldContain("@user002");
        host.Mentions.Value.ShouldNotContain("@user000");
    }

    [Fact]
    public void Enter_Inserts_Nothing_When_Every_Option_Is_Disabled()
    {
        using var host = MentionsHost.Open(Options(3, disabled: [0, 1, 2]));

        host.List.SelectedItem.ShouldBeNull();
        host.Press(Key.Down);
        host.List.CandidateSelectedIndex.ShouldBe(-1);
        host.Press(Key.Enter);

        host.Mentions.Value.ShouldBe("@");
        host.Mentions.IsDropDownOpen.ShouldBeFalse();
    }

    [Fact]
    public void Keyboard_Skips_Disabled_Option_Inside_Viewport()
    {
        using var host = MentionsHost.Open(Options(6, disabled: [2]));

        // Option 0 is pre-selected, so the first Down activates 1.
        host.Press(Key.Down);
        host.List.CandidateSelectedIndex.ShouldBe(1);
        host.Press(Key.Down);
        host.List.CandidateSelectedIndex.ShouldBe(3);
        host.Container(2).IsCandidateSelected.ShouldBeFalse();
        host.Press(Key.Up);
        host.List.CandidateSelectedIndex.ShouldBe(1);
        host.Press(Key.Enter);

        host.Mentions.Value.ShouldNotBeNull();
        host.Mentions.Value.ShouldContain("@user001");
    }

    [Fact]
    public void Keyboard_Skips_Disabled_Options_Across_Viewport_And_Wrap()
    {
        using var host = MentionsHost.Open(Options(LargeCount, disabled: [11, 12, 13, LargeCount - 1]));
        host.ShouldBeVirtualizing();
        host.List.ContainerFromIndex(12).ShouldBeNull("index 12 must start outside the realized range");

        host.Press(Key.Down, 10);
        host.List.CandidateSelectedIndex.ShouldBe(10);
        host.Press(Key.Down);
        host.List.CandidateSelectedIndex.ShouldBe(14);
        host.Press(Key.Up);
        host.List.CandidateSelectedIndex.ShouldBe(10);

        host.Press(Key.Down);
        host.Press(Key.Down, LargeCount - 1 - 14 - 1);
        host.List.CandidateSelectedIndex.ShouldBe(LargeCount - 2);
        host.Press(Key.Down);
        host.List.CandidateSelectedIndex.ShouldBe(0, "Down wraps past the disabled last option");
        host.Press(Key.Up);
        host.List.CandidateSelectedIndex.ShouldBe(LargeCount - 2, "Up wraps past the disabled last option");
    }

    [Fact]
    public void Pointer_Hover_And_Click_On_Disabled_Option_Do_Nothing()
    {
        var options = Options(5, disabled: [2]);
        using var host = MentionsHost.Open(options);

        host.Press(Key.Down);
        host.List.CandidateSelectedIndex.ShouldBe(1);

        var disabledContainer = host.Container(2);
        host.MovePointerTo(disabledContainer);
        host.List.CandidateSelectedIndex.ShouldBe(1);
        disabledContainer.IsCandidateSelected.ShouldBeFalse();

        host.Click(disabledContainer);
        host.Mentions.Value.ShouldBe("@");
        host.Mentions.IsDropDownOpen.ShouldBeTrue();

        // An enabled option still responds to the pointer.
        host.MovePointerTo(host.Container(3));
        host.List.CandidateSelectedIndex.ShouldBe(3);
        host.Click(host.Container(3));
        host.Mentions.Value.ShouldNotBeNull();
        host.Mentions.Value.ShouldContain("@user003");
    }

    [Fact]
    public void Recycled_Containers_Follow_Option_Enabled_State()
    {
        var disabled = new HashSet<int> { 1, LargeCount - 3 };
        using var host = MentionsHost.Open(Options(LargeCount, disabled));
        host.ShouldBeVirtualizing();
        host.AssertRealizedEnabledStates(disabled, "initial viewport");

        host.ScrollToEnd();
        host.List.ContainerFromIndex(1).ShouldBeNull("the disabled option's container must be recycled");
        host.AssertRealizedEnabledStates(disabled, "scrolled to the end");

        host.ScrollTo(0);
        host.AssertRealizedEnabledStates(disabled, "scrolled back to the top");
    }

    [Fact]
    public void Filtered_View_Applies_Enabled_State_By_Option_Not_By_Index()
    {
        // Index 1 is disabled in the full list; after filtering to the user01x options, index 1 is user011 (enabled)
        // and the disabled user012 moves to index 2.
        var options = Options(30, disabled: [1, 12]);
        using var host = MentionsHost.Open(options);
        host.Container(1).IsEnabled.ShouldBeFalse();

        host.TypeFilter("user01");
        host.List.ItemCount.ShouldBe(10);
        host.List.Items[1].ShouldBeSameAs(options[11]);
        host.Container(1).IsEnabled.ShouldBeTrue();
        host.Container(2).IsEnabled.ShouldBeFalse();

        host.Press(Key.Down);
        host.List.CandidateSelectedIndex.ShouldBe(1);
        host.Press(Key.Down);
        host.List.CandidateSelectedIndex.ShouldBe(3);
    }

    private static MentionOption[] Options(int count, ICollection<int> disabled)
    {
        return Enumerable.Range(0, count)
                         .Select(i => new MentionOption
                         {
                             Header    = $"user{i:000}",
                             Value     = $"user{i:000}",
                             IsEnabled = !disabled.Contains(i)
                         })
                         .ToArray();
    }

    // A headless window (popup overlay layer enabled) with Mentions open on "@" and keyboard focus in its text area.
    private sealed class MentionsHost : IDisposable
    {
        private readonly AvaloniaWindow _window;
        private readonly MentionTextArea _textArea;

        public AtomUIMentions Mentions { get; }
        public CandidateList List { get; private set; }

        private MentionsHost(AvaloniaWindow window, AtomUIMentions mentions, MentionTextArea textArea, CandidateList list)
        {
            _window   = window;
            Mentions  = mentions;
            _textArea = textArea;
            List      = list;
        }

        public static MentionsHost Open(IEnumerable<IMentionOption> options)
        {
            var mentions = new AtomUIMentions
            {
                Width           = 240,
                IsMotionEnabled = false,
                OptionsSource   = options
            };
            var overlayPanel = new ScopeAwareOverlayLayerPanel
            {
                Width  = 420,
                Height = 320
            };
            overlayPanel.Children.Add(mentions);
            var visualLayerManager = new VisualLayerManager
            {
                Child = overlayPanel
            };
            var property = typeof(VisualLayerManager).GetProperty(
                "EnablePopupOverlayLayer",
                BindingFlags.Instance | BindingFlags.NonPublic);
            property.ShouldNotBeNull();
            property.SetValue(visualLayerManager, true);

            var window = new AvaloniaWindow
            {
                Width   = 420,
                Height  = 320,
                Content = visualLayerManager
            };
            window.Show();
            Pump();

            var textArea = mentions.GetVisualDescendants().OfType<MentionTextArea>().Single();
            textArea.Focus(NavigationMethod.Tab);
            textArea.Text       = "@";
            textArea.CaretIndex = 1;
            Pump();
            mentions.IsDropDownOpen.ShouldBeTrue();

            CandidateList? list = null;
            for (var i = 0; i < 40 && list is null; i++)
            {
                Dispatcher.UIThread.RunJobs();
                list = window.GetVisualDescendants().OfType<CandidateList>().SingleOrDefault();
            }

            list.ShouldNotBeNull("the candidate list popup must be realized");
            list.UpdateLayout();
            Pump();
            return new MentionsHost(window, mentions, textArea, list);
        }

        public CandidateListItem Container(int index)
        {
            var container = List.ContainerFromIndex(index) as CandidateListItem;
            container.ShouldNotBeNull($"index {index} must be realized");
            return container;
        }

        public void Press(Key key, int times = 1)
        {
            for (var i = 0; i < times; i++)
            {
                var physicalKey = key switch
                {
                    Key.Down  => PhysicalKey.ArrowDown,
                    Key.Up    => PhysicalKey.ArrowUp,
                    Key.Enter => PhysicalKey.Enter,
                    _         => PhysicalKey.None
                };
                _window.KeyPress(key, RawInputModifiers.None, physicalKey, null);
                _window.KeyRelease(key, RawInputModifiers.None, physicalKey, null);
                Pump();
            }
        }

        public void TypeFilter(string filter)
        {
            _textArea.Text       = "@" + filter;
            _textArea.CaretIndex = _textArea.Text.Length;
            Pump();
            List.UpdateLayout();
            Pump();
        }

        public void MovePointerTo(Control target)
        {
            _window.MouseMove(CenterOf(target));
            Pump();
        }

        public void Click(Control target)
        {
            var point = CenterOf(target);
            _window.MouseMove(point);
            _window.MouseDown(point, MouseButton.Left);
            _window.MouseUp(point, MouseButton.Left);
            Pump();
        }

        public void ShouldBeVirtualizing()
        {
            List.ItemsPanelRoot.ShouldBeAssignableTo<VirtualizingPanel>();
            List.GetRealizedContainers().Count().ShouldBeLessThan(List.ItemCount);
        }

        public void ScrollToEnd()
        {
            var scrollViewer = ScrollViewer();
            for (var i = 0; i < 6; i++)
            {
                scrollViewer.Offset = new Vector(scrollViewer.Offset.X, scrollViewer.Extent.Height);
                List.UpdateLayout();
                Pump();
            }
        }

        public void ScrollTo(double offsetY)
        {
            var scrollViewer = ScrollViewer();
            for (var i = 0; i < 3; i++)
            {
                scrollViewer.Offset = new Vector(scrollViewer.Offset.X, offsetY);
                List.UpdateLayout();
                Pump();
            }
        }

        public void AssertRealizedEnabledStates(ISet<int> disabled, string because)
        {
            var realized = List.GetRealizedContainers().OfType<CandidateListItem>().ToList();
            realized.ShouldNotBeEmpty();
            foreach (var container in realized)
            {
                var index = List.IndexFromContainer(container);
                container.IsEnabled.ShouldBe(!disabled.Contains(index), $"container at index {index} [{because}]");
            }
        }

        public void Dispose()
        {
            _window.Close();
            Dispatcher.UIThread.RunJobs();
        }

        private ScrollViewer ScrollViewer()
        {
            var panel = List.ItemsPanelRoot.ShouldNotBeNull();
            return panel.GetVisualAncestors().OfType<ScrollViewer>().First();
        }

        private Point CenterOf(Control target)
        {
            var point = target.TranslatePoint(new Point(target.Bounds.Width / 2, target.Bounds.Height / 2), _window);
            point.ShouldNotBeNull();
            return point.Value;
        }

        private static void Pump()
        {
            for (var i = 0; i < 4; i++)
            {
                Dispatcher.UIThread.RunJobs();
            }
        }
    }
}
