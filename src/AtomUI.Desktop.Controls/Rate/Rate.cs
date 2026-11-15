using Avalonia;
using Avalonia.Controls;
using AtomUI.Controls.Commons;

namespace AtomUI.Desktop.Controls;

public class Rate : AbstractRate
{
    public Rate()
    {
    }

    public static readonly StyledProperty<IList<string>?> ToolTipsProperty =
        AvaloniaProperty.Register<Rate, IList<string>?>(nameof(ToolTips));

    public IList<string>? ToolTips
    {
        get => GetValue(ToolTipsProperty);
        set => SetValue(ToolTipsProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ToolTipsProperty)
        {
            OnRateItemsChanged();
        }
    }

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
