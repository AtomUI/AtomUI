using AtomUI.Desktop.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Primitives;

public class CandidateListBehaviorTests
{
    static CandidateListBehaviorTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Pointer_And_Keyboard_Navigation_Share_One_Active_Candidate()
    {
        var alpha = new AutoCompleteOption { Header = "Alpha", Content = "Alpha" };
        var beta  = new AutoCompleteOption { Header = "Beta", Content = "Beta" };
        var candidateList = new CandidateList
        {
            Width       = 240,
            Height      = 120,
            ItemsSource = new[] { alpha, beta }
        };
        object? committedItem = null;
        candidateList.Commit += (_, _) => committedItem = candidateList.SelectedItem;
        var window = Show(candidateList);

        try
        {
            var alphaContainer = GetContainer(candidateList, 0);
            var betaContainer  = GetContainer(candidateList, 1);

            MovePointerTo(alphaContainer, window);

            candidateList.CandidateSelectedItem.ShouldBeSameAs(alpha);
            alphaContainer.IsCandidateSelected.ShouldBeTrue();
            betaContainer.IsCandidateSelected.ShouldBeFalse();

            PressCandidateKey(candidateList, Key.Down);
            Dispatcher.UIThread.RunJobs();

            alphaContainer.IsPointerOver.ShouldBeTrue(
                "the regression requires the pointer to remain over the old candidate while keyboard navigation continues.");
            candidateList.CandidateSelectedItem.ShouldBeSameAs(beta);
            alphaContainer.IsCandidateSelected.ShouldBeFalse();
            betaContainer.IsCandidateSelected.ShouldBeTrue();

            PressCandidateKey(candidateList, Key.Enter);
            Dispatcher.UIThread.RunJobs();

            committedItem.ShouldBeSameAs(beta);
            candidateList.SelectedItem.ShouldBeNull(
                "the primitive clears its transient ListBox selection after notifying the owner.");
            candidateList.CandidateSelectedIndex.ShouldBe(-1);
            candidateList.CandidateSelectedItem.ShouldBeNull();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Initial_Keyboard_Navigation_Skips_Disabled_Candidate()
    {
        var disabled = new AutoCompleteOption
        {
            Header    = "Disabled",
            Content   = "Disabled",
            IsEnabled = false
        };
        var available = new AutoCompleteOption { Header = "Available", Content = "Available" };
        var candidateList = new CandidateList
        {
            Width       = 240,
            Height      = 120,
            ItemsSource = new[] { disabled, available }
        };
        var window = Show(candidateList);

        try
        {
            MovePointerTo(GetContainer(candidateList, 0), window);
            candidateList.CandidateSelectedItem.ShouldBeNull();

            PressCandidateKey(candidateList, Key.Down);
            Dispatcher.UIThread.RunJobs();

            candidateList.CandidateSelectedItem.ShouldBeSameAs(available);
            GetContainer(candidateList, 0).IsCandidateSelected.ShouldBeFalse();
            GetContainer(candidateList, 1).IsCandidateSelected.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    // The active candidate is an index; the virtualizing panel prepares recycled containers for other indexes.
    // Option data (IListItemData) and plain items (Mentions) take the same path.
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Highlight_Follows_The_Active_Index_Across_Container_Recycling(bool optionData)
    {
        var items = Enumerable.Range(0, 200)
                              .Select(i => optionData
                                  ? (object)new AutoCompleteOption { Header = $"Item {i}", Content = $"Item {i}" }
                                  : $"Item {i}")
                              .ToArray();
        var candidateList = new CandidateList
        {
            Width       = 240,
            Height      = 120,
            ItemsSource = items
        };
        var window = Show(candidateList);

        try
        {
            for (var i = 0; i < 31; i++)
            {
                PressCandidateKey(candidateList, Key.Down);
            }
            Dispatcher.UIThread.RunJobs();
            candidateList.CandidateSelectedIndex.ShouldBe(30);
            HighlightedIndexes(candidateList).ShouldBe(new[] { 30 });

            candidateList.ScrollIntoView(199);
            candidateList.UpdateLayout();
            candidateList.ContainerFromIndex(30).ShouldBeNull();
            HighlightedIndexes(candidateList).ShouldBeEmpty("recycled containers must not inherit the highlight");

            candidateList.ScrollIntoView(30);
            candidateList.UpdateLayout();
            HighlightedIndexes(candidateList).ShouldBe(new[] { 30 });
        }
        finally
        {
            window.Close();
        }
    }

    private static int[] HighlightedIndexes(CandidateList candidateList)
    {
        return candidateList.GetRealizedContainers()
                            .OfType<CandidateListItem>()
                            .Where(item => item.IsCandidateSelected)
                            .Select(candidateList.IndexFromContainer)
                            .ToArray();
    }

    private static CandidateListItem GetContainer(CandidateList candidateList, int index)
    {
        var container = candidateList.ContainerFromIndex(index) as CandidateListItem;
        container.ShouldNotBeNull();
        return container;
    }

    private static void MovePointerTo(Control target, AvaloniaWindow window)
    {
        var point = target.TranslatePoint(
            new Point(target.Bounds.Width / 2, target.Bounds.Height / 2),
            window);
        point.ShouldNotBeNull();

        window.MouseMove(point.Value);
        Dispatcher.UIThread.RunJobs();
    }

    private static void PressCandidateKey(CandidateList candidateList, Key key)
    {
        candidateList.HandleKeyDown(new KeyEventArgs
        {
            RoutedEvent  = InputElement.KeyDownEvent,
            Source       = candidateList,
            Key          = key,
            PhysicalKey  = key switch
            {
                Key.Enter => PhysicalKey.Enter,
                Key.Up    => PhysicalKey.ArrowUp,
                Key.Down  => PhysicalKey.ArrowDown,
                _         => PhysicalKey.None
            },
            KeyModifiers = KeyModifiers.None
        });
    }

    private static AvaloniaWindow Show(Control content)
    {
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 240,
            Content = content
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }
}
