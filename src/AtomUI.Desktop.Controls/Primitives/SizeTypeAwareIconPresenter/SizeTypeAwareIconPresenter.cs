using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls.Primitives;

using IconControl = Icon;

internal class SizeTypeAwareIconPresenter : TemplatedControl, ISizeTypeAware, IMotionAwareControl
{
    #region 公共属性定义
    
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<SizeTypeAwareIconPresenter>();
    
    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<SizeTypeAwareIconPresenter, PathIcon?>(nameof(Icon));
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<SizeTypeAwareIconPresenter>();
    
    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    [Content]
    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    #endregion
    
    
    static SizeTypeAwareIconPresenter()
    {
        AffectsMeasure<SizeTypeAwareIconPresenter>(IconProperty, PaddingProperty, SizeTypeProperty);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdatePseudoClasses();
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(StdPseudoClass.Empty, Icon is null);
    }
}