using AtomUI.Generated.AtomUIDesktopControls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Step indicator that renders one dot per step and fills the dot of the active step with
/// <see cref="IndicatorActiveColor" />.
/// The dots are created in code as <see cref="Ellipse" /> elements that carry only the marker class of the
/// <c>popup.indicator</c> part and, when active, the <c>active</c> class; their size, color and spacing come from
/// the theme selectors and bindings, so <c>TourPopupIndicatorStyle</c> can restyle every dot across visual roots.
/// </summary>
public class DefaultTourIndicator : TourIndicator
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="IndicatorSize" /> property.
    /// </summary>
    public static readonly StyledProperty<double> IndicatorSizeProperty = 
        AvaloniaProperty.Register<DefaultTourIndicator, double>(nameof(IndicatorSize));
    
    /// <summary>
    /// Identifies the <see cref="IndicatorColor" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> IndicatorColorProperty = 
        AvaloniaProperty.Register<DefaultTourIndicator, IBrush?>(nameof(IndicatorColor));
    
    /// <summary>
    /// Identifies the <see cref="IndicatorActiveColor" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> IndicatorActiveColorProperty = 
        AvaloniaProperty.Register<DefaultTourIndicator, IBrush?>(nameof(IndicatorActiveColor));
    
    /// <summary>
    /// Identifies the <see cref="ItemSpacing" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ItemSpacingProperty = 
        AvaloniaProperty.Register<DefaultTourIndicator, double>(nameof(ItemSpacing));

    /// <summary>
    /// Edge length, in device-independent pixels, of every indicator dot. The indicator reports this length as its
    /// height, and a change re-measures the indicator. Clearing a local value falls back to the theme value, the
    /// <c>TourToken.IndicatorSize</c> token.
    /// </summary>
    public double IndicatorSize
    {
        get => GetValue(IndicatorSizeProperty);
        set => SetValue(IndicatorSizeProperty, value);
    }
        
    /// <summary>
    /// Brush that fills the indicator dots of the steps that are not active. A <see langword="null" /> value leaves
    /// those dots unfilled. Clearing a local value falls back to the theme color, which comes from the <c>ColorFill</c>
    /// token, or from the <c>PrimaryPrevBtnBg</c> tour token when <see cref="TourIndicator.StyleType" /> is <see
    /// cref="TourStyleType.Primary" />.
    /// </summary>
    public IBrush? IndicatorColor
    {
        get => GetValue(IndicatorColorProperty);
        set => SetValue(IndicatorColorProperty, value);
    }
    
    /// <summary>
    /// Brush that fills the indicator dot of the active step. A <see langword="null" /> value leaves that dot
    /// unfilled. Clearing a local value falls back to the theme color, which comes from the <c>ColorPrimary</c>
    /// token, or from the <c>ColorTextLightSolid</c> token when <see cref="TourIndicator.StyleType" /> is
    /// <see cref="TourStyleType.Primary" />.
    /// </summary>
    public IBrush? IndicatorActiveColor
    {
        get => GetValue(IndicatorActiveColorProperty);
        set => SetValue(IndicatorActiveColorProperty, value);
    }
    
    /// <summary>
    /// Gap, in device-independent pixels, between adjacent indicator dots. The same amount is also the margin on
    /// every side of the dot row, and a change re-measures the indicator. Clearing a local value falls back to the
    /// theme value, the <c>SpacingXXS</c> token.
    /// </summary>
    public double ItemSpacing
    {
        get => GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }
    #endregion

    private Panel? _dotsLayout;

    static DefaultTourIndicator()
    {
        AffectsMeasure<DefaultTourIndicator>(ItemSpacingProperty, IndicatorSizeProperty, StepCountProperty);
    }

    /// <summary>
    /// Measures the indicator as <c>StepCount * IndicatorSize + ItemSpacing * (StepCount + 1)</c> in width and
    /// <see cref="IndicatorSize" /> in height, whatever <paramref name="availableSize" /> is. One gap is counted on
    /// each side of the dots, which is the layout contract of the earlier self-drawn indicator; the footer of the
    /// steps view centers the row vertically.
    /// </summary>
    protected override Size MeasureOverride(Size availableSize)
    {
        var height = IndicatorSize;
        var width  = StepCount * IndicatorSize + ItemSpacing * (StepCount + 1);
        return new Size(width, height);
    }

    /// <summary>
    /// Called when the control template is applied. After the base call it resolves the <c>DotsLayout</c> panel and
    /// synchronizes the indicator dots with <see cref="TourIndicator.StepCount" /> and <see
    /// cref="TourIndicator.ActiveIndex" />. A further override must call this member to keep the dots synchronized.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _dotsLayout = e.NameScope.Find<Panel>("DotsLayout");
        SyncDots();
    }

    /// <summary>
    /// Called when the value of a property changes. After the base call it synchronizes the indicator dots when <see
    /// cref="TourIndicator.StepCount" /> or <see cref="TourIndicator.ActiveIndex" /> changed. A further override must
    /// call this member to keep the dots synchronized.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == StepCountProperty || change.Property == ActiveIndexProperty)
        {
            SyncDots();
        }
    }

    private void SyncDots()
    {
        if (_dotsLayout is null)
        {
            return;
        }

        while (_dotsLayout.Children.Count > StepCount)
        {
            _dotsLayout.Children.RemoveAt(_dotsLayout.Children.Count - 1);
        }

        while (_dotsLayout.Children.Count < StepCount)
        {
            var dot = new Ellipse();
            dot.Classes.Add(TourSemanticParts.PopupIndicatorClass);
            _dotsLayout.Children.Add(dot);
        }

        for (var i = 0; i < _dotsLayout.Children.Count; i++)
        {
            var dot = (Ellipse)_dotsLayout.Children[i];
            var isActive = i == ActiveIndex;
            if (isActive && !dot.Classes.Contains(ActiveClass))
            {
                dot.Classes.Add(ActiveClass);
            }
            else if (!isActive && dot.Classes.Contains(ActiveClass))
            {
                dot.Classes.Remove(ActiveClass);
            }
        }
    }

    private const string ActiveClass = "active";
}
