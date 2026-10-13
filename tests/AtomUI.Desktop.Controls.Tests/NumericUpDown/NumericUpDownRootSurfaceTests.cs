using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUINumericUpDown = AtomUI.Desktop.Controls.NumericUpDown;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.NumericUpDown;

public class NumericUpDownRootSurfaceTests
{
    static NumericUpDownRootSurfaceTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Spinner_Theme_Preserves_Application_Base_Replacement_And_Local_Overrides()
    {
        var application = Application.Current!;
        application.TryFindResource(typeof(ButtonSpinner), out var baseValue).ShouldBeTrue();
        var replacement = new ControlTheme(typeof(ButtonSpinner))
        {
            BasedOn = baseValue.ShouldBeOfType<ControlTheme>(),
            Setters = { new Setter(Control.TagProperty, "application-spinner-theme") }
        };
        var priorEntry = application.Resources.FirstOrDefault(pair => Equals(pair.Key, typeof(ButtonSpinner)));
        application.Resources[typeof(ButtonSpinner)] = replacement;
        var window = new AvaloniaWindow();
        try
        {
            var builder = new AtomUIBuilder(new Application());
            builder.UseDesktopControls();
            // 分区包：余量段须先收集，才能看到包的完整贡献（关键集 + 余量）。
            builder.ThemeManagerBuilder.RunDeferredCollects();
            var registration = builder.ThemeManagerBuilder.EnumerateContributions()
                .Where(p => p.Id == "AtomUI.Desktop.Controls")
                .SelectMany(p => p.Resources)
                .Single(r => r.AssetId.EndsWith("NumericUpDown/Themes/NumericUpDownSpinnerTheme.axaml", StringComparison.Ordinal));
            var wrapper = registration.Factory().ShouldBeAssignableTo<ResourceDictionary>();
            var dictionary = wrapper.MergedDictionaries.Single().ShouldBeAssignableTo<ResourceDictionary>();
            var theme = dictionary[typeof(NumericUpDownSpinner)].ShouldBeOfType<ControlTheme>();
            theme.BasedOn.ShouldBeSameAs(replacement);
            dictionary["NumericUpDownSpinnerContentPadding"].ShouldBe(new Thickness(0));
            var spinner = new NumericUpDownSpinner { Theme = theme, CornerRadius = new CornerRadius(13), IsMotionEnabled = false };
            window.Content = spinner;
            window.Show();
            spinner.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();
            spinner.Tag.ShouldBe("application-spinner-theme");
            spinner.CornerRadius.ShouldBe(new CornerRadius(13));
            spinner.IsMotionEnabled.ShouldBeFalse();
            spinner.GetVisualDescendants().OfType<ButtonSpinnerDecoratedBox>().ShouldHaveSingleItem();
        }
        finally
        {
            window.Close();
            if (priorEntry.Key is not null) application.Resources[typeof(ButtonSpinner)] = priorEntry.Value;
            else application.Resources.Remove(typeof(ButtonSpinner));
        }
    }

    [Fact]
    public void Root_BorderBrush_Relves_Onto_The_Input_Frame()
    {
        var borderBrush = new SolidColorBrush(Colors.MediumPurple);
        var numericUpDown = new AtomUINumericUpDown
        {
            Width = 320,
            Value = 66m,
            BorderBrush = borderBrush
        };

        var window = Show(numericUpDown);
        try
        {
            FrameOf(numericUpDown).BorderBrush.ShouldBe(borderBrush);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Owner_Scoped_BorderBrush_Style_Reaches_The_Input_Frame()
    {
        var borderBrush = new SolidColorBrush(Colors.RoyalBlue);
        var numericUpDown = new AtomUINumericUpDown
        {
            Width = 320,
            Value = 66m,
            Classes = { "semantic-object" }
        };
        var ownerStyle = new Style(selector => selector.OfType<AtomUINumericUpDown>().Class("semantic-object"));
        ownerStyle.Setters.Add(new Setter(AtomUINumericUpDown.BorderBrushProperty, borderBrush));
        numericUpDown.Styles.Add(ownerStyle);

        var window = Show(numericUpDown);
        try
        {
            FrameOf(numericUpDown).BorderBrush.ShouldBe(borderBrush);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Clearing_The_Root_BorderBrush_Restores_The_Frame_State_Machine()
    {
        var borderBrush = new SolidColorBrush(Colors.MediumPurple);
        var numericUpDown = new AtomUINumericUpDown
        {
            Width = 320,
            Value = 66m,
            BorderBrush = borderBrush
        };

        var window = Show(numericUpDown);
        try
        {
            var frame = FrameOf(numericUpDown);
            frame.BorderBrush.ShouldBe(borderBrush);

            numericUpDown.BorderBrush = null;
            Dispatcher.UIThread.RunJobs();

            frame.BorderBrush.ShouldNotBe(borderBrush);
            frame.BorderBrush.ShouldNotBeNull();
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>
    /// Both NumericUpDown templates host a <c>ButtonSpinnerDecoratedBox</c> frame whose theme
    /// defaults <c>IsMotionEnabled</c> to the shared motion token. Without an explicit owner
    /// propagation the frame keeps its BorderBrush/Background transitions running even when the
    /// owner turns motion off, and an in-flight transition then outranks the root BorderBrush relay.
    /// </summary>
    [Theory]
    [InlineData(NumericUpDownMode.Input)]
    [InlineData(NumericUpDownMode.Spinner)]
    public void Motion_Setting_Reaches_The_Input_Frame(NumericUpDownMode mode)
    {
        var numericUpDown = new AtomUINumericUpDown
        {
            Width           = 320,
            Value           = 66m,
            Mode            = mode,
            IsMotionEnabled = false
        };

        var window = Show(numericUpDown);
        try
        {
            FrameOf(numericUpDown).IsMotionEnabled.ShouldBeFalse(
                "The owner IsMotionEnabled setting must reach the input frame in both mode templates.");
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [InlineData(NumericUpDownMode.Input)]
    [InlineData(NumericUpDownMode.Spinner)]
    public void Root_BorderBrush_Live_Change_Applies_Immediately_When_Motion_Is_Disabled(NumericUpDownMode mode)
    {
        var numericUpDown = new AtomUINumericUpDown
        {
            Width           = 320,
            Value           = 66m,
            Mode            = mode,
            IsMotionEnabled = false
        };

        var window = Show(numericUpDown);
        try
        {
            var frame        = FrameOf(numericUpDown);
            var customBorder = new SolidColorBrush(Colors.MediumPurple);

            numericUpDown.BorderBrush = customBorder;
            Dispatcher.UIThread.RunJobs();

            frame.BorderBrush.ShouldBe(customBorder,
                "With motion disabled the relayed border must be visible immediately, not animated from the frame rest state.");
        }
        finally
        {
            window.Close();
        }
    }

    private static ButtonSpinnerDecoratedBox FrameOf(AtomUINumericUpDown numericUpDown)
    {
        return numericUpDown.GetVisualDescendants()
                            .OfType<ButtonSpinnerDecoratedBox>()
                            .Single();
    }

    private static AvaloniaWindow Show(Control content)
    {
        var window = new AvaloniaWindow
        {
            Width = 480,
            Height = 160,
            Content = content
        };
        window.Show();
        content.ApplyTemplate();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        return window;
    }
}
