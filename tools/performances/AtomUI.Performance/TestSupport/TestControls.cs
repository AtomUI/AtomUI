using AtomUI.Controls;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;

namespace AtomUI.Performance;

internal sealed class ProbeIcon : Icon
{
}

internal sealed class MarkerDataTemplate : IDataTemplate
{
    private readonly string _marker;

    public MarkerDataTemplate(string marker)
    {
        _marker = marker;
    }

    public bool Match(object? data)
    {
        return true;
    }

    public Control Build(object? param)
    {
        return new Avalonia.Controls.TextBlock
        {
            Name = _marker,
            Text = _marker
        };
    }
}

internal sealed class EffectiveBrushSet
{
    public IBrush DefaultBorder { get; } = ProgramTestBrush(10, 10, 10);
    public IBrush HoverBorder { get; } = ProgramTestBrush(20, 20, 20);
    public IBrush ActiveBorder { get; } = ProgramTestBrush(30, 30, 30);
    public IBrush FilledBackground { get; } = ProgramTestBrush(40, 40, 40);
    public IBrush FilledBorder { get; } = ProgramTestBrush(50, 50, 50);
    public IBrush FilledHoverBackground { get; } = ProgramTestBrush(60, 60, 60);
    public IBrush ActiveBackground { get; } = ProgramTestBrush(70, 70, 70);
    public IBrush DisabledBackground { get; } = ProgramTestBrush(80, 80, 80);
    public IBrush ErrorBorder { get; } = ProgramTestBrush(90, 20, 20);
    public IBrush ErrorHoverBorder { get; } = ProgramTestBrush(100, 30, 30);
    public IBrush ErrorBackground { get; } = ProgramTestBrush(110, 40, 40);
    public IBrush ErrorFilledBorder { get; } = ProgramTestBrush(120, 50, 50);
    public IBrush ErrorHoverBackground { get; } = ProgramTestBrush(130, 60, 60);
    public IBrush WarningBorder { get; } = ProgramTestBrush(90, 90, 20);
    public IBrush WarningHoverBorder { get; } = ProgramTestBrush(100, 100, 30);
    public IBrush WarningBackground { get; } = ProgramTestBrush(110, 110, 40);
    public IBrush WarningFilledBorder { get; } = ProgramTestBrush(120, 120, 50);
    public IBrush WarningHoverBackground { get; } = ProgramTestBrush(130, 130, 60);

    private static IBrush ProgramTestBrush(byte red, byte green, byte blue)
    {
        return new SolidColorBrush(Color.FromRgb(red, green, blue));
    }
}
