using System.Collections.ObjectModel;
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

public class MentionsBehaviorTests
{
    static MentionsBehaviorTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void IsDropDownOpen_Opens_And_Closes_Popup_With_Single_Event_Pair()
    {
        var mentions = new AtomUIMentions
        {
            Width           = 240,
            IsMotionEnabled = false,
            OptionsSource =
            [
                new MentionOption { Header = "afc163", Value = "afc163" },
                new MentionOption { Header = "zombieJ", Value = "zombieJ" }
            ]
        };
        var openedCount = 0;
        var closedCount = 0;
        mentions.DropDownOpened += (_, _) => openedCount++;
        mentions.DropDownClosed += (_, _) => closedCount++;
        var window = CreateWindow(mentions);

        try
        {
            var popup = FindPopup(mentions);

            mentions.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            mentions.IsDropDownOpen.ShouldBeTrue();
            popup.IsOpen.ShouldBeTrue();
            openedCount.ShouldBe(1);

            mentions.IsDropDownOpen = false;
            Dispatcher.UIThread.RunJobs();

            mentions.IsDropDownOpen.ShouldBeFalse();
            popup.IsOpen.ShouldBeFalse();
            closedCount.ShouldBe(1);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Popup_Pin_Opens_Business_State_And_Physical_Popup()
    {
        var mentions = new AtomUIMentions
        {
            Width           = 240,
            IsMotionEnabled = false,
            OptionsSource =
            [
                new MentionOption { Header = "afc163", Value = "afc163" },
                new MentionOption { Header = "zombieJ", Value = "zombieJ" }
            ]
        };
        var window = CreateWindow(mentions);

        try
        {
            var popup = FindPopup(mentions);

            mentions.IsPopupPinnedOpen = true;
            Dispatcher.UIThread.RunJobs();

            mentions.IsDropDownOpen.ShouldBeTrue();
            popup.IsPopupPinnedOpen.ShouldBeTrue();
            popup.IsOpen.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Popup_Pin_Suppresses_Light_Dismiss_Before_First_Open_And_Unpin_Restores_It()
    {
        var mentions = new AtomUIMentions
        {
            Width             = 240,
            IsMotionEnabled   = false,
            IsPopupPinnedOpen = true,
            OptionsSource =
            [
                new MentionOption { Header = "afc163", Value = "afc163" },
                new MentionOption { Header = "zombieJ", Value = "zombieJ" }
            ]
        };
        var window = CreateWindow(mentions);

        try
        {
            var popup = FindPopup(mentions);

            popup.IsPopupPinnedOpen.ShouldBeTrue();
            popup.IsOpen.ShouldBeTrue();
            popup.IsLightDismissEnabled.ShouldBeFalse();

            mentions.IsPopupPinnedOpen = false;
            Dispatcher.UIThread.RunJobs();

            popup.IsPopupPinnedOpen.ShouldBeFalse();
            popup.IsLightDismissEnabled.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Popup_Pin_Rejects_Business_Close_Request()
    {
        var mentions = new AtomUIMentions
        {
            Width           = 240,
            IsMotionEnabled = false,
            OptionsSource =
            [
                new MentionOption { Header = "afc163", Value = "afc163" },
                new MentionOption { Header = "zombieJ", Value = "zombieJ" }
            ]
        };
        var window = CreateWindow(mentions);

        try
        {
            var popup = FindPopup(mentions);
            mentions.IsPopupPinnedOpen = true;
            Dispatcher.UIThread.RunJobs();

            mentions.IsDropDownOpen = false;
            Dispatcher.UIThread.RunJobs();

            mentions.IsDropDownOpen.ShouldBeTrue();
            popup.IsOpen.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Pinned_Mentions_Detach_Closes_Popup_And_Reattach_Reopens_It()
    {
        var mentions = new AtomUIMentions
        {
            Width           = 240,
            IsMotionEnabled = false,
            OptionsSource =
            [
                new MentionOption { Header = "afc163", Value = "afc163" },
                new MentionOption { Header = "zombieJ", Value = "zombieJ" }
            ]
        };
        var window = CreateWindow(mentions);

        try
        {
            var visualLayerManager = window.Content.ShouldBeOfType<VisualLayerManager>();
            var overlayPanel = visualLayerManager.Child.ShouldBeOfType<ScopeAwareOverlayLayerPanel>();
            var popup = FindPopup(mentions);
            mentions.IsPopupPinnedOpen = true;
            Dispatcher.UIThread.RunJobs();
            popup.IsOpen.ShouldBeTrue();

            overlayPanel.Children.Remove(mentions);
            Dispatcher.UIThread.RunJobs();

            mentions.IsPopupPinnedOpen.ShouldBeTrue();
            mentions.IsDropDownOpen.ShouldBeFalse();
            popup.IsOpen.ShouldBeFalse();
            window.GetVisualDescendants().OfType<OverlayPopupHost>().ShouldBeEmpty();

            overlayPanel.Children.Add(mentions);
            Dispatcher.UIThread.RunJobs();

            mentions.IsDropDownOpen.ShouldBeTrue();
            popup.IsOpen.ShouldBeTrue();
            window.GetVisualDescendants().OfType<OverlayPopupHost>().ShouldHaveSingleItem();
        }
        finally
        {
            mentions.IsPopupPinnedOpen = false;
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void OptionsSource_Replace_Before_First_Open_Does_Not_Require_View()
    {
        var options = new ObservableCollection<IMentionOption>
        {
            new MentionOption { Header = "alpha", Value = "alpha" },
            new MentionOption { Header = "beta", Value = "beta" }
        };
        var mentions = new AtomUIMentions
        {
            Width         = 240,
            OptionsSource = options
        };

        Should.NotThrow(() =>
        {
            options[1] = new MentionOption { Header = "gamma", Value = "gamma" };
        });

        var window = CreateWindow(mentions);

        try
        {
            mentions.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var candidateList = FindCandidateList(window);
            candidateList.Items.Cast<IMentionOption>()
                         .Select(option => option.Header?.ToString())
                         .ShouldBe(["alpha", "gamma"]);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Multiline_Popup_VerticalOffset_Aligns_To_Trigger_Line()
    {
        var mentions = new AtomUIMentions
        {
            Width              = 240,
            Lines              = 3,
            VerticalAlignment  = Avalonia.Layout.VerticalAlignment.Top,
            IsMotionEnabled    = false,
            OptionsSource =
            [
                new MentionOption { Header = "afc163", Value = "afc163" },
                new MentionOption { Header = "zombieJ", Value = "zombieJ" }
            ]
        };
        var window = CreateWindow(mentions);

        try
        {
            var textArea = mentions.GetVisualDescendants()
                                   .OfType<MentionTextArea>()
                                   .Single();
            textArea.Text       = "@";
            textArea.CaretIndex = 1;
            Dispatcher.UIThread.RunJobs();

            mentions.IsDropDownOpen.ShouldBeTrue();
            var popup = FindPopup(mentions);

            // The trigger sits on the first line of a 3-line control. The popup
            // must be pulled well above the control's bottom edge to align with
            // that line, rather than staying anchored near the last line.
            var controlHeight = mentions.DesiredSize.Height;
            controlHeight.ShouldBeGreaterThan(0);
            popup.VerticalOffset.ShouldBeLessThan(-(controlHeight / 3));
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Committing_Candidate_Without_Current_Trigger_Does_Not_Insert_Stale_Mention()
    {
        var option = new MentionOption { Header = "afc163", Value = "afc163" };
        var mentions = new AtomUIMentions
        {
            Width         = 240,
            Value         = "plain text",
            OptionsSource = [option]
        };
        var window = CreateWindow(mentions);

        try
        {
            mentions.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var candidateList = FindCandidateList(window);
            candidateList.SelectedItem = option;
            candidateList.RaiseEvent(new RoutedEventArgs(CandidateList.CommitEvent));
            Dispatcher.UIThread.RunJobs();

            mentions.Value.ShouldBe("plain text");
            mentions.IsDropDownOpen.ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Pointer_Move_Then_Enter_Inserts_The_Pointer_Candidate()
    {
        var anna = new MentionOption { Header = "anna", Value = "anna" };
        var amy  = new MentionOption { Header = "amy", Value = "amy" };
        var mentions = new AtomUIMentions
        {
            Width           = 240,
            IsMotionEnabled = false,
            OptionsSource   = [anna, amy]
        };
        var window = CreateWindow(mentions);

        try
        {
            var textArea = mentions.GetVisualDescendants()
                                   .OfType<MentionTextArea>()
                                   .Single();
            textArea.Text       = "@";
            textArea.CaretIndex = 1;
            Dispatcher.UIThread.RunJobs();

            mentions.IsDropDownOpen.ShouldBeTrue();
            var candidateList = FindCandidateList(window);
            var annaContainer = GetContainer(candidateList, 0);
            var amyContainer  = GetContainer(candidateList, 1);

            PressCandidateKey(candidateList, Key.Down);
            MovePointerTo(amyContainer, window);
            Dispatcher.UIThread.RunJobs();

            annaContainer.IsCandidateSelected.ShouldBeFalse();
            amyContainer.IsCandidateSelected.ShouldBeTrue();
            candidateList.CandidateSelectedItem.ShouldBeSameAs(amy);

            PressCandidateKey(candidateList, Key.Enter);
            Dispatcher.UIThread.RunJobs();

            mentions.Value.ShouldNotBeNull();
            mentions.Value.ShouldContain("@amy");
            mentions.Value.ShouldNotContain("@anna");
            mentions.IsDropDownOpen.ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    private static AvaloniaWindow CreateWindow(Control content)
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

        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static Popup FindPopup(Visual root)
    {
        return root.GetVisualDescendants()
                   .OfType<Popup>()
                   .Single();
    }

    private static CandidateList FindCandidateList(Visual root)
    {
        for (var i = 0; i < 20; i++)
        {
            Dispatcher.UIThread.RunJobs();
            var candidateList = root.GetVisualDescendants()
                                    .OfType<CandidateList>()
                                    .SingleOrDefault();
            if (candidateList != null)
            {
                return candidateList;
            }
        }

        throw new InvalidOperationException("Expected Mentions popup candidate list to be realized.");
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
                Key.Down  => PhysicalKey.ArrowDown,
                _         => PhysicalKey.None
            },
            KeyModifiers = KeyModifiers.None
        });
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
