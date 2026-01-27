using AtomUI.Controls;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Icons.AntDesign;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

using AvaloniaNumericUpDown = Avalonia.Controls.NumericUpDown;

public class NumericUpDown : AvaloniaNumericUpDown, IMotionAwareControl, IControlSharedTokenResourcesHost
{
    #region 公共属性定义
    public static readonly StyledProperty<PathIcon?> ClearIconProperty =
        AvaloniaProperty.Register<NumericUpDown, PathIcon?>(nameof(ClearIcon));

    public static readonly StyledProperty<object?> LeftAddOnProperty =
        AddOnDecoratedBox.LeftAddOnProperty.AddOwner<NumericUpDown>();
    
    public static readonly StyledProperty<IDataTemplate?> LeftAddOnTemplateProperty =
        AddOnDecoratedBox.LeftAddOnTemplateProperty.AddOwner<NumericUpDown>();

    public static readonly StyledProperty<object?> RightAddOnProperty =
        AddOnDecoratedBox.RightAddOnProperty.AddOwner<NumericUpDown>();
    
    public static readonly StyledProperty<IDataTemplate?> RightAddOnTemplateProperty =
        AddOnDecoratedBox.RightAddOnTemplateProperty.AddOwner<NumericUpDown>();
    
    public static readonly StyledProperty<IDataTemplate?> InnerLeftContentTemplateProperty =
        AvaloniaProperty.Register<NumericUpDown, IDataTemplate?>(nameof(InnerLeftContentTemplate));
    
    public static readonly StyledProperty<IDataTemplate?> InnerRightContentTemplateProperty =
        AvaloniaProperty.Register<NumericUpDown, IDataTemplate?>(nameof(InnerRightContentTemplate));

    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<NumericUpDown>();

    public static readonly StyledProperty<AddOnDecoratedVariant> StyleVariantProperty =
        AddOnDecoratedBox.StyleVariantProperty.AddOwner<NumericUpDown>();

    public static readonly StyledProperty<AddOnDecoratedStatus> StatusProperty =
        AddOnDecoratedBox.StatusProperty.AddOwner<NumericUpDown>();

    public static readonly StyledProperty<bool> IsEnableClearButtonProperty =
        TextBox.IsEnableClearButtonProperty.AddOwner<NumericUpDown>();

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<NumericUpDown>();
    
    public PathIcon? ClearIcon
    {
        get => GetValue(ClearIconProperty);
        set => SetValue(ClearIconProperty, value);
    }

    [DependsOn(nameof(LeftAddOnTemplate))]
    public object? LeftAddOn
    {
        get => GetValue(LeftAddOnProperty);
        set => SetValue(LeftAddOnProperty, value);
    }
    
    public object? LeftAddOnTemplate
    {
        get => GetValue(LeftAddOnTemplateProperty);
        set => SetValue(LeftAddOnTemplateProperty, value);
    }

    [DependsOn(nameof(RightAddOnTemplate))]
    public object? RightAddOn
    {
        get => GetValue(RightAddOnProperty);
        set => SetValue(RightAddOnProperty, value);
    }
    
    public object? RightAddOnTemplate
    {
        get => GetValue(RightAddOnTemplateProperty);
        set => SetValue(RightAddOnTemplateProperty, value);
    }
    
    public object? InnerLeftContentTemplate
    {
        get => GetValue(InnerLeftContentTemplateProperty);
        set => SetValue(InnerLeftContentTemplateProperty, value);
    }
    
    public object? InnerRightContentTemplate
    {
        get => GetValue(InnerRightContentTemplateProperty);
        set => SetValue(InnerRightContentTemplateProperty, value);
    }

    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    public AddOnDecoratedVariant StyleVariant
    {
        get => GetValue(StyleVariantProperty);
        set => SetValue(StyleVariantProperty, value);
    }

    public AddOnDecoratedStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public bool IsEnableClearButton
    {
        get => GetValue(IsEnableClearButtonProperty);
        set => SetValue(IsEnableClearButtonProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    #endregion
    
    #region 内部属性定义

    internal static readonly StyledProperty<double> SpinnerHandleWidthProperty =
        ButtonSpinner.SpinnerHandleWidthProperty.AddOwner<NumericUpDown>();
    
    internal static readonly StyledProperty<bool> IsCustomFontSizeProperty =
        AvaloniaProperty.Register<NumericUpDown, bool>(nameof(IsCustomFontSize));
    
    internal static readonly DirectProperty<NumericUpDown, bool> IsEffectiveShowClearButtonProperty =
        AvaloniaProperty.RegisterDirect<NumericUpDown, bool>(nameof(IsEffectiveShowClearButton),
            o => o.IsEffectiveShowClearButton,
            (o, v) => o.IsEffectiveShowClearButton = v);
    
    internal double SpinnerHandleWidth
    {
        get => GetValue(SpinnerHandleWidthProperty);
        set => SetValue(SpinnerHandleWidthProperty, value);
    }
    
    public bool IsCustomFontSize
    {
        get => GetValue(IsCustomFontSizeProperty);
        set => SetValue(IsCustomFontSizeProperty, value);
    }
    
    private bool _isEffectiveShowClearButton;

    internal bool IsEffectiveShowClearButton
    {
        get => _isEffectiveShowClearButton;
        set => SetAndRaise(IsEffectiveShowClearButtonProperty, ref _isEffectiveShowClearButton, value);
    }
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => NumericUpDownToken.ID;
    
    #endregion
    
    private IconButton? _clearButton;
    
    public NumericUpDown()
    {
        this.RegisterResources();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (ClearIcon == null)
        {
            SetCurrentValue(ClearIconProperty, new CloseCircleFilled());
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _clearButton      = e.NameScope.Find<IconButton>(NumericUpDownThemeConstants.ClearButtonPart);
        if (_clearButton is not null)
        {
            _clearButton.Click += (sender, args) => { NotifyClearButtonClicked(); };
        }
        ConfigureEffectiveShowClearButton();
    }
    
    protected virtual void NotifyClearButtonClicked()
    {
        Value = null;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsReadOnlyProperty ||
            change.Property == TextProperty ||
            change.Property == IsEnableClearButtonProperty)
        {
            ConfigureEffectiveShowClearButton();
        }
    }

    private void ConfigureEffectiveShowClearButton()
    {
        if (!IsEnableClearButton)
        {
            SetCurrentValue(IsEffectiveShowClearButtonProperty, false);
            return;
        }
        
        SetCurrentValue(IsEffectiveShowClearButtonProperty, !IsReadOnly && !string.IsNullOrEmpty(Text));
    }
}