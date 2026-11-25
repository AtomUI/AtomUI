using System.Globalization;
using System.Reflection;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.ControlBaseline;

/// <summary>
/// Dumps the effective surface values of every input frame inside each input host, for each style variant, status and
/// interaction state (frame hover, input focus, pressed, disabled), so two builds can be diffed as text. Frames are the
/// internal InputControlFrame instances (and their add-on / inner borders) found in the host's visual tree; their
/// interaction state is driven through the same internal properties pointer and focus handling set.
/// </summary>
internal static class FrameStateMatrix
{
    private static readonly string[] HostTypeNames =
    [
        "LineEdit", "TextArea", "SearchEdit", "Select", "TreeSelect", "Cascader", "DatePicker", "RangeDatePicker",
        "TimePicker", "NumericUpDown", "OtpLineEdit", "AutoComplete", "Mentions", "ColorPicker"
    ];

    private static readonly string[] Variants = ["Outlined", "Filled", "Borderless", "Underlined"];
    private static readonly string[] Statuses = ["Default", "Warning", "Error"];

    private static readonly (string Name, bool Hover, bool Focus, bool Pressed, bool Disabled)[] States =
    [
        ("normal", false, false, false, false),
        ("hover", true, false, false, false),
        ("focus", false, true, false, false),
        ("hover+focus", true, true, false, false),
        ("pressed", true, false, true, false),
        ("pressed+focus", true, true, true, false),
        ("disabled", false, false, false, true),
        ("disabled+focus", false, true, false, true)
    ];

    private static readonly string[] SurfaceNames =
    [
        "PART_LeftAddOn", "PART_RightAddOn", "PART_ContentFrame", "InnerBoxDecorator"
    ];

    public static int Run(IReadOnlyList<Assembly> assemblies, string outputPath)
    {
        var types = assemblies.SelectMany(assembly => assembly.GetExportedTypes())
                              .Where(type => HostTypeNames.Contains(type.Name) && typeof(Control).IsAssignableFrom(type))
                              .OrderBy(type => type.Name, StringComparer.Ordinal)
                              .ToList();
        var output = new StringBuilder();
        foreach (var type in types)
        {
            foreach (var variant in Variants)
            {
                foreach (var status in Statuses)
                {
                    foreach (var state in States)
                    {
                        Capture(output, type, variant, status, "Middle", state);
                    }

                    // Size presets change paddings and radii; the default state is enough for them.
                    foreach (var size in new[] { "Large", "Small" })
                    {
                        Capture(output, type, variant, status, size, States[0]);
                    }
                }
            }
        }

        File.WriteAllText(outputPath, output.ToString());
        Console.WriteLine($"frame matrix: hosts={types.Count} lines={output.ToString().Split('\n').Length}");
        return 0;
    }

    private static void Capture(StringBuilder output, Type type, string variant, string status, string size,
                                (string Name, bool Hover, bool Focus, bool Pressed, bool Disabled) state)
    {
        var host = (Control)Activator.CreateInstance(type)!;
        SetEnum(host, "StyleVariant", variant);
        SetEnum(host, "Status", status);
        SetEnum(host, "SizeType", size);
        type.GetProperty("IsMotionEnabled")?.SetValue(host, false);
        // Outer add-ons make the add-on borders part of the comparison.
        type.GetProperty("LeftAddOn")?.SetValue(host, "L");
        type.GetProperty("RightAddOn")?.SetValue(host, "R");
        host.IsEnabled = !state.Disabled;

        var window = new Window { Width = 600, Height = 200, Content = host };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            var frames = host.GetVisualDescendants()
                             .OfType<Control>()
                             .Where(control => IsFrame(control.GetType()))
                             .ToList();
            foreach (var frame in frames)
            {
                // Some hosts keep motion on for their frame; captured values must be end states, not transition frames.
                if (AvaloniaPropertyRegistry.Instance.FindRegistered(frame, "IsMotionEnabled") is { } motion)
                {
                    frame.SetValue(motion, false);
                }

                SetInternal(frame, "IsInnerBoxHover", state.Hover);
                SetInternal(frame, "IsInputFocusWithin", state.Focus);
                SetInternal(frame, "IsInnerBoxPressed", state.Pressed);
            }

            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            var index = 0;
            foreach (var frame in frames)
            {
                var prefix = $"{type.Name}|{variant}|{status}|{size}|{state.Name}|frame{index++}:{frame.GetType().Name}";
                output.Append(prefix).Append(" self ").AppendLine(Describe(frame));
                foreach (var surface in frame.GetVisualDescendants().OfType<Control>()
                                             .Where(control => SurfaceNames.Contains(control.Name) &&
                                                               ReferenceEquals(Owner(control), frame)))
                {
                    output.Append(prefix).Append(' ').Append(surface.Name).Append(' ').AppendLine(Describe(surface));
                }
            }
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static Control? Owner(Control control) => control.TemplatedParent as Control;

    private static bool IsFrame(Type? type)
    {
        for (; type is not null; type = type.BaseType)
        {
            if (type.Name == "InputControlFrame")
            {
                return true;
            }
        }

        return false;
    }

    private static string Describe(Control control)
    {
        return $"border={Brush(Get(control, "BorderBrush"))} background={Brush(Get(control, "Background"))} " +
               $"shadow={Get(control, "BoxShadow")} padding={Get(control, "Padding")} radius={Get(control, "CornerRadius")} visible={control.IsVisible}";
    }

    private static object? Get(Control control, string name)
    {
        var property = AvaloniaPropertyRegistry.Instance.FindRegistered(control, name);
        return property is null ? "-" : control.GetValue(property);
    }

    private static string Brush(object? value)
    {
        return value switch
        {
            null => "null",
            ISolidColorBrush solid => string.Create(CultureInfo.InvariantCulture,
                $"#{solid.Color.A:X2}{solid.Color.R:X2}{solid.Color.G:X2}{solid.Color.B:X2}@{solid.Opacity:0.###}"),
            _ => value.ToString() ?? "?"
        };
    }

    private static void SetEnum(Control control, string name, string value)
    {
        var property = control.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
        if (property is { CanWrite: true } && property.PropertyType.IsEnum &&
            Enum.TryParse(property.PropertyType, value, out var parsed))
        {
            property.SetValue(control, parsed);
        }
    }

    private static void SetInternal(Control control, string name, bool value)
    {
        for (var type = control.GetType(); type is not null; type = type.BaseType)
        {
            var property = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
                                                   BindingFlags.DeclaredOnly);
            if (property?.SetMethod is { } setter)
            {
                setter.Invoke(control, [value]);
                return;
            }
        }
    }
}
