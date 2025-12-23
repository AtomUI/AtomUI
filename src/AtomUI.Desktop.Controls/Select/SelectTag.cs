using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

internal class SelectTag : Tag
{
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<SelectTag>();
    
    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    public ISelectOption? Option { get; set; }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (CloseButton != null)
        {
            CloseButton.IsPassthroughMouseEvent = true;
        }
    }
}

internal class SelectRemainInfoTag : SelectTag
{
    protected override Type StyleKeyOverride { get; } = typeof(SelectTag);
    
    public void SetRemainText(int remainCount)
    {
        TagText = $"+ {remainCount} ...";
    }
}