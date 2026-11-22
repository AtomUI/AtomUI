using System.Diagnostics;
using System.Reflection;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Theme.Schema;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// The AutoComplete members: their input is a nested, registered LineEdit / SearchEdit / TextArea (covered by those
/// rows), so their own Tokens are the candidate popup and the placeholder brush they hand to the input. The popup opens
/// in an overlay layer inside the test window; popup Tokens compare the popup frame.
/// </summary>
public partial class InputFamilyEffectTests
{
    public static IEnumerable<object[]> AutoCompleteRows() => RowsOf("AutoComplete");
    public static IEnumerable<object[]> AutoCompleteSearchEditRows() => RowsOf("AutoCompleteSearchEdit");
    public static IEnumerable<object[]> AutoCompleteTextAreaRows() => RowsOf("AutoCompleteTextArea");

    [Theory]
    [MemberData(nameof(AutoCompleteRows))]
    public void AutoComplete_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(AutoCompleteSetup(token, static () => new AutoComplete()), AutoCompleteTokens.Identity, token);
    }

    [Theory]
    [MemberData(nameof(AutoCompleteSearchEditRows))]
    public void AutoCompleteSearchEdit_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(AutoCompleteSetup(token, static () => new AutoCompleteSearchEdit()), AutoCompleteSearchEditTokens.Identity,
            token);
    }

    [Theory]
    [MemberData(nameof(AutoCompleteTextAreaRows))]
    public void AutoCompleteTextArea_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(AutoCompleteSetup(token, static () => new AutoCompleteTextArea()), AutoCompleteTextAreaTokens.Identity,
            token);
    }

    private static EffectSetup AutoCompleteSetup(string token, Func<AbstractAutoComplete> factory)
    {
        var placeholder = token == "ColorTextPlaceholder";
        var narrow      = token == "MinPopupWidth";
        var unbounded   = token == "MaxPopupWidth";
        // The option height only bounds the popup height (option height x displayed count), so it shows once there
        // are more candidates than the popup displays.
        var overflowing = token is "OptionHeight" or "ControlHeight";
        return new EffectSetup(
            () =>
            {
                var autoComplete = factory();
                autoComplete.Width                   = narrow ? 80 : 240;
                autoComplete.IsPopupMatchSelectWidth = !unbounded;
                autoComplete.PlaceholderText         = "Placeholder";
                autoComplete.IsPopupPinnedOpen       = !placeholder;
                autoComplete.OptionsSource = overflowing
                    ? Enumerable.Range(0, 24)
                                .Select(static index => new AutoCompleteOption { Header = $"Al {index}", Content = $"Al {index}" })
                                .ToArray()
                    : new[]
                    {
                        new AutoCompleteOption { Header = "Alpha", Content = "Alpha" },
                        new AutoCompleteOption
                        {
                            Header  = unbounded ? "Alpine option with a header longer than the maximum width" : "Alpine",
                            Content = "Alpine"
                        }
                    };
                return OverlayLayerHost(autoComplete);
            },
            placeholder
                ? null
                : token is "OptionSelectedColor" or "ColorText" or "OptionBorderRadius"
                    // The selected candidate's text (ItemSelectedColor): select the first candidate.
                    ? static control =>
                    {
                        TypeAndWaitForCandidates((AbstractAutoComplete)control);
                        TopLevel.GetTopLevel(control)!.GetVisualDescendants().OfType<CandidateList>().First().SelectedIndex = 0;
                        Dispatcher.UIThread.RunJobs();
                    }
                    : static control => TypeAndWaitForCandidates((AbstractAutoComplete)control),
            new EffectOptions
            {
                Target = static root => root.GetVisualDescendants().OfType<AbstractAutoComplete>().First(),
                Region = placeholder ? null : static control => PopupFrameOf(control),
                // Square by default: the selected candidate's background shows rounded corners.
                Perturb = token == "OptionBorderRadius" ? static _ => new Avalonia.CornerRadius(12) : null
            });
    }

    /// <summary>A layer manager with the popup overlay layer enabled, so the candidate popup renders in the window.</summary>
    private static Control OverlayLayerHost(Control content)
    {
        var layers = new VisualLayerManager
        {
            Child = new Panel { Width = 420, Height = 480, Children = { content } }
        };
        content.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
        content.VerticalAlignment   = Avalonia.Layout.VerticalAlignment.Top;
        typeof(VisualLayerManager)
            .GetProperty("EnablePopupOverlayLayer", BindingFlags.Instance | BindingFlags.NonPublic)
            .ShouldNotBeNull()
            .SetValue(layers, true);
        return layers;
    }

    private static void TypeAndWaitForCandidates(AbstractAutoComplete autoComplete)
    {
        var input = autoComplete.GetVisualDescendants().OfType<AbstractTextInput>().First();
        input.Text       = "Al";
        input.CaretIndex = 2;
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < TimeSpan.FromSeconds(5))
        {
            Dispatcher.UIThread.RunJobs();
            if (TopLevel.GetTopLevel(autoComplete)!.GetVisualDescendants().OfType<CandidateListItem>().Count() >= 2)
            {
                return;
            }
            Thread.Sleep(5);
        }

        throw new InvalidOperationException("The candidate popup did not show its options.");
    }

    private static Control PopupFrameOf(Control autoComplete) =>
        TopLevel.GetTopLevel(autoComplete)!.GetVisualDescendants().OfType<Border>()
                .First(static border => border.Name == "PopupFrame" && border.IsEffectivelyVisible);
}
