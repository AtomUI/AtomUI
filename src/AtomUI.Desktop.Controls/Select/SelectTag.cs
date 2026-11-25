using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

internal partial class SelectTag : Tag
{
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<SelectTag>();

    internal static readonly StyledProperty<double> CustomTagHeightProperty =
        AvaloniaProperty.Register<SelectTag, double>(nameof(CustomTagHeight), double.NaN);

    internal static readonly DirectProperty<SelectTag, bool> HasCustomTagHeightProperty =
        AvaloniaProperty.RegisterDirect<SelectTag, bool>(
            nameof(HasCustomTagHeight),
            o => o.HasCustomTagHeight,
            (o, v) => o.HasCustomTagHeight = v);

    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    internal double CustomTagHeight
    {
        get => GetValue(CustomTagHeightProperty);
        set => SetValue(CustomTagHeightProperty, value);
    }

    private bool _hasCustomTagHeight;

    internal bool HasCustomTagHeight
    {
        get => _hasCustomTagHeight;
        set => SetAndRaise(HasCustomTagHeightProperty, ref _hasCustomTagHeight, value);
    }

    public object? Item { get; set; }

    static SelectTag()
    {
        AffectsMeasure<SelectTag>(CustomTagHeightProperty, HasCustomTagHeightProperty);
    }

    internal static bool IsCloseButtonSource(Control sourceControl)
    {
        var closeButton = sourceControl.FindAncestorOfType<IconButton>(includeSelf: true);
        return closeButton?.FindAncestorOfType<SelectTag>() != null;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == CustomTagHeightProperty)
        {
            HasCustomTagHeight = IsUsableCustomHeight(CustomTagHeight);
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (CloseButton != null)
        {
            CloseButton.IsPassthroughMouseEvent = true;
        }
    }

    private static bool IsUsableCustomHeight(double height)
    {
        return !double.IsNaN(height) &&
               !double.IsInfinity(height) &&
               height > 0;
    }
}

internal class SelectRemainInfoTag : SelectTag
{
    protected override Type StyleKeyOverride { get; } = typeof(SelectTag);

    public void SetRemainText(int remainCount)
    {
        Text = $"+ {remainCount} ...";
    }
}
