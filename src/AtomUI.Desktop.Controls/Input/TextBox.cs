using Avalonia;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Text box for plain input; NumericUpDown, pickers and select filters embed a derived text box.
/// Its effective corner radius follows <c>CornerRadius</c>, adjusted to its position among adjacent items when
/// the box is used in a compact space.
/// </summary>
public class TextBox : AbstractTextInput
{
    #region 内部属性定义

    internal static readonly DirectProperty<TextBox, CornerRadius> EffectiveCornerRadiusProperty =
        AvaloniaProperty.RegisterDirect<TextBox, CornerRadius>(
            nameof(EffectiveCornerRadius),
            o => o.EffectiveCornerRadius,
            (o, value) => o.EffectiveCornerRadius = value);

    private CornerRadius _effectiveCornerRadius;

    internal CornerRadius EffectiveCornerRadius
    {
        get => _effectiveCornerRadius;
        private set => SetAndRaise(EffectiveCornerRadiusProperty, ref _effectiveCornerRadius, value);
    }

    #endregion

    public TextBox()
    {
    }

    /// <summary>
    /// Calls the base implementation, then recalculates the effective corner radius when <c>CornerRadius</c>,
    /// the compact space position or the compact space orientation changes.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == CornerRadiusProperty ||
            change.Property == CompactSpaceItemPositionProperty ||
            change.Property == CompactSpaceOrientationProperty)
        {
            ConfigureCornerRadius();
        }
    }

    /// <summary>
    /// Calls the base implementation, then recalculates the effective corner radius for the applied template.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        ConfigureCornerRadius();
    }

    private void ConfigureCornerRadius()
    {
        EffectiveCornerRadius = CompactSpace.CalculateEffectiveCornerRadius(
            CornerRadius,
            IsUsedInCompactSpace,
            CompactSpaceItemPosition,
            CompactSpaceOrientation);
    }
}
