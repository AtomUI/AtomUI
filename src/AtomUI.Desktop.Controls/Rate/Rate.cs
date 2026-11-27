using Avalonia;
using AtomUI.Controls.Commons;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Desktop rate control. It adds the per-star tooltips of <see cref="ToolTips" /> to
/// <see cref="AbstractRate" />.
/// </summary>
public class Rate : AbstractRate
{
    public Rate()
    {
    }

    /// <summary>
    /// Identifies the <see cref="ToolTips" /> property.
    /// </summary>
    public static readonly StyledProperty<IList<string>?> ToolTipsProperty =
        AvaloniaProperty.Register<Rate, IList<string>?>(nameof(ToolTips));

    /// <summary>
    /// Tooltips of the stars, one entry per star in display order: the star at index <c>i</c> shows entry
    /// <c>i</c>. A star whose entry is missing or <see langword="null" /> shows no tooltip, as do all stars when
    /// the value is <see langword="null" />. The list is not observed, so replace the value to apply later changes.
    /// </summary>
    public IList<string>? ToolTips
    {
        get => GetValue(ToolTipsProperty);
        set => SetValue(ToolTipsProperty, value);
    }

    /// <summary>
    /// Reapplies the star tooltips when <see cref="ToolTips" /> changes. The base method runs first; an override
    /// must call it to keep the tooltips in step.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ToolTipsProperty)
        {
            OnRateItemsChanged();
        }
    }

    /// <summary>
    /// Sets the tooltip of every star from <see cref="ToolTips" />: the star at index <c>i</c> gets entry <c>i</c>,
    /// and a star without an entry has its tooltip removed. It is called after the stars were created and when
    /// <see cref="ToolTips" /> changes, and it does nothing before the template that holds the stars is applied.
    /// </summary>
    private protected override void OnRateItemsChanged()
    {
        if (RateItems is not { } items)
        {
            return;
        }

        for (var index = 0; index < items.Items.Count; index++)
        {
            if (items.Items[index] is RateItem item)
            {
                if (ToolTips is { } tips && index < tips.Count)
                {
                    ToolTip.SetTip(item, tips[index]);
                }
                else
                {
                    item.ClearValue(ToolTip.TipProperty);
                }
            }
        }
    }
}
