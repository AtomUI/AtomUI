using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Toolkits.GalleryBase.Tests.Controls;

public class IconGalleryTests
{
    private static readonly TimeSpan SearchDebounceDelay = TimeSpan.FromMilliseconds(120);

    static IconGalleryTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Text_Changes_Are_Debounced_And_Use_The_Latest_Query()
    {
        var gallery = new IconGallery();
        var window  = Show(gallery);

        try
        {
            var searchEdit = FindSearchEdit(gallery);
            var reloads    = ObserveReloads(gallery);

            searchEdit.Text = "h";
            searchEdit.Text = "ho";
            searchEdit.Text = "home";
            Dispatcher.UIThread.RunJobs();

            reloads.Count.ShouldBe(0);

            PumpDispatcherUntil(() => reloads.Count != 0);

            reloads.Count.ShouldBe(1);
            gallery.IconInfos.ShouldNotBeNull();
            gallery.IconInfos.ShouldNotBeEmpty();
            gallery.IconInfos.ShouldAllBe(item =>
                item.IconName.Contains("home", StringComparison.InvariantCultureIgnoreCase));
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Search_Request_Bypasses_Debounce_And_Cancels_Pending_Reload()
    {
        var gallery = new IconGallery();
        var window  = Show(gallery);

        try
        {
            var searchEdit = FindSearchEdit(gallery);
            var reloads    = ObserveReloads(gallery);

            searchEdit.Text = "home";
            Dispatcher.UIThread.RunJobs();
            searchEdit.RaiseEvent(new SearchRequestedEventArgs(
                SearchEdit.SearchRequestedEvent,
                searchEdit,
                searchEdit.Text,
                SearchTriggerSource.Button));
            Dispatcher.UIThread.RunJobs();

            reloads.Count.ShouldBe(1);

            PumpDispatcherFor(SearchDebounceDelay + TimeSpan.FromMilliseconds(80));

            reloads.Count.ShouldBe(1);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Detach_Cancels_Pending_Search_Reload()
    {
        var gallery = new IconGallery();
        var window  = Show(gallery);

        var searchEdit = FindSearchEdit(gallery);
        var reloads    = ObserveReloads(gallery);

        searchEdit.Text = "home";
        Dispatcher.UIThread.RunJobs();

        window.Content = null;
        window.Close();
        Dispatcher.UIThread.RunJobs();
        var reloadsAfterDetach = reloads.Count;

        PumpDispatcherFor(SearchDebounceDelay + TimeSpan.FromMilliseconds(80));

        reloads.Count.ShouldBe(reloadsAfterDetach);
    }

    [Fact]
    public void Detached_Search_Edit_No_Longer_Schedules_Reload()
    {
        var gallery = new IconGallery();
        var window  = Show(gallery);

        var searchEdit = FindSearchEdit(gallery);
        var reloads    = ObserveReloads(gallery);

        window.Content = null;
        window.Close();
        Dispatcher.UIThread.RunJobs();
        var reloadsAfterDetach = reloads.Count;

        searchEdit.Text = "home";
        Dispatcher.UIThread.RunJobs();
        PumpDispatcherFor(SearchDebounceDelay + TimeSpan.FromMilliseconds(80));

        reloads.Count.ShouldBe(reloadsAfterDetach);
    }

    [Fact]
    public void Theme_Change_Cancels_Pending_Text_Search()
    {
        var gallery = new IconGallery();
        var window  = Show(gallery);

        try
        {
            var searchEdit = FindSearchEdit(gallery);
            var reloads    = ObserveReloads(gallery);

            searchEdit.Text = "home";
            Dispatcher.UIThread.RunJobs();

            gallery.IconThemeType = IconThemeType.Filled;

            reloads.Count.ShouldBe(1);

            PumpDispatcherFor(SearchDebounceDelay + TimeSpan.FromMilliseconds(80));

            reloads.Count.ShouldBe(1);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static SearchEdit FindSearchEdit(Control gallery)
    {
        return gallery.GetVisualDescendants()
                      .OfType<SearchEdit>()
                      .Single();
    }

    private static List<AvaloniaPropertyChangedEventArgs> ObserveReloads(IconGallery gallery)
    {
        var changes = new List<AvaloniaPropertyChangedEventArgs>();
        gallery.PropertyChanged += (_, e) =>
        {
            if (e.Property == IconGallery.IconInfosProperty)
            {
                changes.Add(e);
            }
        };
        return changes;
    }

    private static AvaloniaWindow Show(Control content)
    {
        var window = new AvaloniaWindow
        {
            Width   = 800,
            Height  = 600,
            Content = content
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();
        content.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static void PumpDispatcherUntil(Func<bool> condition)
    {
        var frame = new DispatcherFrame();
        var elapsed = System.Diagnostics.Stopwatch.StartNew();
        var timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(5)
        };
        timer.Tick += (_, _) =>
        {
            if (condition() || elapsed.Elapsed >= TimeSpan.FromSeconds(5))
            {
                frame.Continue = false;
            }
        };
        timer.Start();
        try
        {
            Dispatcher.UIThread.PushFrame(frame);
        }
        finally
        {
            timer.Stop();
        }
        condition().ShouldBeTrue("the search reload must complete within the dispatcher guard");
    }

    private static void PumpDispatcherFor(TimeSpan duration)
    {
        using var cancellation = new CancellationTokenSource(duration);
        Dispatcher.UIThread.MainLoop(cancellation.Token);
    }
}
