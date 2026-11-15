using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace AtomUI.ControlBaseline;

/// <summary>
/// Renders every control type across its visual states into one PNG per type, so an unoptimized and an optimized
/// build can be compared pixel by pixel. States cover the default, each value of each visual enum property (one at a
/// time), common visual booleans, the disabled state and forced interaction pseudo-classes. Motion is switched off
/// where a control exposes it, so captures do not depend on transition timing.
/// </summary>
internal static class StateSnapshots
{
    // Enum properties that change appearance; every value is rendered once with all other properties at default.
    private static readonly HashSet<string> VisualEnumProperties = new(StringComparer.Ordinal)
    {
        "SizeType", "StyleVariant", "Status", "ButtonType", "Variant", "Color", "Shape", "Type",
        "IconPlacement", "TitlePosition", "Orientation", "TabStripPlacement", "Placement", "PanelVariant",
        "LabelPlacement", "Presentation", "Mode", "PickerMode", "SelectionMode", "Direction", "ValidateStatus",
        "BadgeType", "ProgressStatus", "IndicatorPosition", "ExpandIconPosition", "TriggerType"
    };

    // Boolean properties that change appearance; each is rendered once set to the opposite of its default.
    private static readonly HashSet<string> VisualBoolProperties = new(StringComparer.Ordinal)
    {
        "IsDanger", "IsGhost", "IsLoading", "IsChecked", "IsBordered", "IsReadOnly", "IsAllowClear", "IsShowIcon",
        "IsClosable", "IsPlain", "IsBlock", "IsDashed", "IsSelected", "IsExpanded", "IsActive", "IsShowTime",
        "IsMultiple", "IsShowLine", "IsCheckable", "IsIndeterminate", "IsGhostStyle", "IsEnableRevealButton"
    };

    private static readonly string[] PseudoClasses =
    [
        ":pointerover", ":pressed", ":focus", ":focus-within", ":focus-visible", ":checked", ":selected"
    ];

    public static int Run(IReadOnlyList<Type> types, string directory)
    {
        Directory.CreateDirectory(directory);
        var failures = 0;
        foreach (var type in types)
        {
            try
            {
                var variants = BuildVariants(type);
                var panel    = new WrapPanel { Width = 1200 };
                foreach (var (label, control) in variants)
                {
                    panel.Children.Add(new StackPanel
                    {
                        Margin   = new Thickness(4),
                        Children =
                        {
                            new Avalonia.Controls.TextBlock { Text = label, FontSize = 9, Foreground = Brushes.Gray },
                            control
                        }
                    });
                }

                var window = new Avalonia.Controls.Window
                {
                    Width         = 1220,
                    Height        = 400,
                    Background    = Brushes.White,
                    Content       = panel,
                    ShowInTaskbar = false
                };
                window.Show();
                try
                {
                    // Pseudo-classes are applied after attach so control logic that sets them on load does not win.
                    foreach (var (label, control) in variants)
                    {
                        if (label.StartsWith(':'))
                        {
                            ((IPseudoClasses)control.Classes).Set(label, true);
                        }
                    }

                    // ATOMUI_STATE_SETTLE_PASSES lengthens the settle loop so that running transitions reach their end
                    // state, to tell a transition captured mid-way from a stable difference.
                    var passes = int.TryParse(Environment.GetEnvironmentVariable("ATOMUI_STATE_SETTLE_PASSES"), out var settle)
                        ? settle
                        : 6;
                    for (var i = 0; i < passes; i++)
                    {
                        Dispatcher.UIThread.RunJobs();
                        window.UpdateLayout();
                        // Headless windows do not size to content; grow to the panel so tall controls are not clipped.
                        window.Height = Math.Clamp(panel.Bounds.Height + 8, 100, 8000);
                        AvaloniaHeadlessPlatform.ForceRenderTimerTick(2);
                    }

                    Dispatcher.UIThread.RunJobs();
                    using var frame = window.CaptureRenderedFrame()
                                      ?? throw new InvalidOperationException("No rendered frame was captured.");
                    frame.Save(Path.Combine(directory, type.Name + ".png"), PngBitmapEncoderOptions.Default);
                }
                finally
                {
                    window.Close();
                    Dispatcher.UIThread.RunJobs();
                }
            }
            catch (Exception ex)
            {
                failures++;
                var inner = ex is TargetInvocationException { InnerException: { } innerException } ? innerException : ex;
                Console.WriteLine($"{type.Name}: FAILED {inner.GetType().Name}: {inner.Message}");
            }
        }

        Console.WriteLine($"state snapshots: types={types.Count} failed={failures}");
        return 0;
    }

    private static List<(string Label, Control Control)> BuildVariants(Type type)
    {
        var variants = new List<(string, Control)> { ("default", Create(type)) };

        var disabled = Create(type);
        disabled.IsEnabled = false;
        variants.Add(("disabled", disabled));

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!property.CanWrite || property.GetSetMethod() is null || property.GetIndexParameters().Length > 0)
            {
                continue;
            }

            var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            if (propertyType.IsEnum && VisualEnumProperties.Contains(property.Name))
            {
                foreach (var value in Enum.GetValues(propertyType))
                {
                    var control = Create(type);
                    if (TrySet(control, property, value))
                    {
                        variants.Add(($"{property.Name}={value}", control));
                    }
                }
            }
            else if (propertyType == typeof(bool) && VisualBoolProperties.Contains(property.Name))
            {
                var control = Create(type);
                var current = property.GetValue(control) as bool?;
                if (TrySet(control, property, !(current ?? false)))
                {
                    variants.Add(($"{property.Name}={!(current ?? false)}", control));
                }
            }
        }

        foreach (var pseudoClass in PseudoClasses)
        {
            variants.Add((pseudoClass, Create(type)));
        }

        return variants;
    }

    private static bool TrySet(Control control, PropertyInfo property, object? value)
    {
        try
        {
            property.SetValue(control, value);
            return true;
        }
        catch (TargetInvocationException)
        {
            return false;
        }
    }

    private static Control Create(Type type)
    {
        var control = (Control)Activator.CreateInstance(type)!;
        switch (control)
        {
            case HeaderedContentControl headered:
                headered.Header  ??= "Header";
                headered.Content ??= "Text";
                break;
            case ContentControl content:
                content.Content ??= "Text";
                break;
            case Avalonia.Controls.TextBox textBox:
                textBox.Text = "Text";
                break;
            case Avalonia.Controls.TextBlock textBlock:
                textBlock.Text = "Text";
                break;
        }

        // Transitions would make captures depend on timing; motion-capable controls render their end state.
        type.GetProperty("IsMotionEnabled", BindingFlags.Public | BindingFlags.Instance)?.SetValue(control, false);
        control.HorizontalAlignment = HorizontalAlignment.Left;
        control.VerticalAlignment   = VerticalAlignment.Top;
        return control;
    }
}
