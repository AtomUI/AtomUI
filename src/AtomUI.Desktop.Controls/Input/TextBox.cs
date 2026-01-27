using AtomUI.Controls;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Icons.AntDesign;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

using AvaloniaTextBox = Avalonia.Controls.TextBox;

public class TextBox : AvaloniaTextBox,
                       IControlSharedTokenResourcesHost,
                       IMotionAwareControl,
                       ISizeTypeAware
{
    #region 公共属性定义

    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<TextBox>();

    public static readonly StyledProperty<bool> IsEnableClearButtonProperty =
        AvaloniaProperty.Register<TextBox, bool>(nameof(IsEnableClearButton));

    public static readonly StyledProperty<bool> IsEnableRevealButtonProperty =
        AvaloniaProperty.Register<TextBox, bool>(nameof(IsEnableRevealButton));
    
    public static readonly StyledProperty<bool> IsCustomFontSizeProperty =
        AvaloniaProperty.Register<TextBox, bool>(nameof(IsCustomFontSize));
    
    public static readonly StyledProperty<bool> IsShowCountProperty =
        AvaloniaProperty.Register<TextBox, bool>(nameof(IsShowCount));
    
    public static readonly StyledProperty<PathIcon?> ClearIconProperty =
        AvaloniaProperty.Register<TextBox, PathIcon?>(nameof(ClearIcon));
    
    public static readonly StyledProperty<IBrush?> WatermarkForegroundProperty =
        AvaloniaProperty.Register<TextBox, IBrush?>(nameof(WatermarkForeground));
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty = 
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<TextBox>();
    
    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    public bool IsEnableClearButton
    {
        get => GetValue(IsEnableClearButtonProperty);
        set => SetValue(IsEnableClearButtonProperty, value);
    }

    public bool IsEnableRevealButton
    {
        get => GetValue(IsEnableRevealButtonProperty);
        set => SetValue(IsEnableRevealButtonProperty, value);
    }
    
    public bool IsCustomFontSize
    {
        get => GetValue(IsCustomFontSizeProperty);
        set => SetValue(IsCustomFontSizeProperty, value);
    }
    
    public bool IsShowCount
    {
        get => GetValue(IsShowCountProperty);
        set => SetValue(IsShowCountProperty, value);
    }
    
    public PathIcon? ClearIcon
    {
        get => GetValue(ClearIconProperty);
        set => SetValue(ClearIconProperty, value);
    }

    public IBrush? WatermarkForeground
    {
        get => GetValue(WatermarkForegroundProperty);
        set => SetValue(WatermarkForegroundProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<TextBox, bool> IsEffectiveShowClearButtonProperty =
        AvaloniaProperty.RegisterDirect<TextBox, bool>(nameof(IsEffectiveShowClearButton),
            o => o.IsEffectiveShowClearButton,
            (o, v) => o.IsEffectiveShowClearButton = v);
    
    internal static readonly DirectProperty<TextBox, string?> CountTextProperty =
        AvaloniaProperty.RegisterDirect<TextBox, string?>(nameof(CountText),
            o => o.CountText,
            (o, v) => o.CountText = v);

    private bool _isEffectiveShowClearButton;

    internal bool IsEffectiveShowClearButton
    {
        get => _isEffectiveShowClearButton;
        set => SetAndRaise(IsEffectiveShowClearButtonProperty, ref _isEffectiveShowClearButton, value);
    }

    private string? _countText;

    internal string? CountText
    {
        get => _countText;
        set => SetAndRaise(CountTextProperty, ref _countText, value);
    }
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => LineEditToken.ID;
    #endregion
    
    private IconButton? _clearButton;

    public TextBox()
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

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == AcceptsReturnProperty ||
            change.Property == IsReadOnlyProperty ||
            change.Property == TextProperty ||
            change.Property == IsEnableClearButtonProperty)
        {
            ConfigureEffectiveShowClearButton();
        }
        else if (change.Property == IsShowCountProperty)
        {
            HandleInputChanged(Text);
        }
    }

    private void ConfigureEffectiveShowClearButton()
    {
        if (!IsEnableClearButton)
        {
            SetCurrentValue(IsEffectiveShowClearButtonProperty, false);
            return;
        }
        
        SetCurrentValue(IsEffectiveShowClearButtonProperty, !IsReadOnly && !AcceptsReturn && !string.IsNullOrEmpty(Text));
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_clearButton != null)
        {
            _clearButton.Click -= HandleClearButtonClicked;
        }
        
        _clearButton = e.NameScope.Find<IconButton>(TextBoxThemeConstants.ClearButtonPart);
        if (_clearButton != null)
        {
            _clearButton.Click += HandleClearButtonClicked;
        }
        ConfigureEffectiveShowClearButton();
        HandleInputChanged(Text);
    }

    private void HandleClearButtonClicked(object? sender, RoutedEventArgs args)
    {
        NotifyClearButtonClicked();
    }
    
    protected virtual void NotifyClearButtonClicked()
    {
        Clear();
    }

    protected override void OnTextInput(TextInputEventArgs e)
    {
        base.OnTextInput(e);
        HandleInputChanged(Text);
    }

    private void HandleInputChanged(string? text)
    {
        if (IsShowCount)
        {
            SetCurrentValue(CountTextProperty, $"{text?.Length ?? 0} / {MaxLength}");
        }
    }
}