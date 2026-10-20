using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Input;
using Avalonia.VisualTree;

using AvaloniaTextBox = Avalonia.Controls.TextBox;

namespace AtomUI.Desktop.Controls;

[PseudoClasses(":cell-active", ":input-target")]
[global::System.Runtime.Versioning.UnsupportedOSPlatform("browser")]
internal class OtpLineEditCell : InputControlFrame
{
    #region 公共属性定义

    public static readonly StyledProperty<string?> DisplayTextProperty =
        AvaloniaProperty.Register<OtpLineEditCell, string?>(nameof(DisplayText));

    public static readonly StyledProperty<string?> PlaceholderTextProperty =
        AvaloniaProperty.Register<OtpLineEditCell, string?>(nameof(PlaceholderText));

    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<OtpLineEditCell, bool>(nameof(IsActive));

    public string? DisplayText
    {
        get => GetValue(DisplayTextProperty);
        set => SetValue(DisplayTextProperty, value);
    }

    public string? PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<bool> IsInputTargetProperty =
        AvaloniaProperty.Register<OtpLineEditCell, bool>(nameof(IsInputTarget));

    internal bool IsInputTarget
    {
        get => GetValue(IsInputTargetProperty);
        set => SetValue(IsInputTargetProperty, value);
    }

    #endregion

    private OtpTextBox? _textBox;
    private Avalonia.Controls.Shapes.Rectangle? _caret;
    private OtpLineEdit? _owner;

    private const string CellActivePseudoClass = ":cell-active";
    private const string InputTargetPseudoClass = ":input-target";

    internal AvaloniaTextBox? TextBoxPart => _textBox;

    private void RelayOwnerOverride(AvaloniaProperty sourceProperty, AvaloniaProperty targetProperty)
    {
        if (_owner is null)
        {
            return;
        }

        var value = _owner.GetValue(sourceProperty);
        if (value is null || ReferenceEquals(value, AvaloniaProperty.UnsetValue))
        {
            ClearValue(targetProperty);
        }
        else
        {
            SetValue(targetProperty, value, BindingPriority.LocalValue);
        }
    }

    /// <summary>
    /// Relays the owner's border brush onto this cell frame. <c>CellBorderBrush</c> is the
    /// cell-specific override and wins over the generic root <c>BorderBrush</c>; when neither is
    /// set the cell theme state machine owns the rest state.
    /// </summary>
    private void RelayOwnerBorderBrush()
    {
        if (_owner is null)
        {
            return;
        }

        var value = _owner.GetValue(OtpLineEdit.CellBorderBrushProperty);
        if (value is null || ReferenceEquals(value, AvaloniaProperty.UnsetValue))
        {
            value = _owner.GetValue(BorderBrushProperty);
        }

        if (value is null || ReferenceEquals(value, AvaloniaProperty.UnsetValue))
        {
            ClearValue(BorderBrushProperty);
        }
        else
        {
            SetValue(BorderBrushProperty, value, BindingPriority.LocalValue);
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _textBox = Content as OtpTextBox;
        _caret = e.NameScope.Find<Avalonia.Controls.Shapes.Rectangle>("PART_Caret");
        ConfigureTextBoxCursor();
        UpdateCaretVisibility();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsActiveProperty)
        {
            PseudoClasses.Set(CellActivePseudoClass, change.GetNewValue<bool>());
            IsInputFocusWithin = change.GetNewValue<bool>();
            UpdateCaretVisibility();
        }

        if (change.Property == DisplayTextProperty ||
            change.Property == IsEffectivelyEnabledProperty)
        {
            UpdateCaretVisibility();
        }

        if (change.Property == IsInputTargetProperty)
        {
            PseudoClasses.Set(InputTargetPseudoClass, change.GetNewValue<bool>());
            ConfigureTextBoxCursor();
        }

        if (change.Property == ContentProperty)
        {
            _textBox = Content as OtpTextBox;
            ConfigureTextBoxCursor();
        }
    }

    private void UpdateCaretVisibility()
    {
        if (_caret is null)
        {
            return;
        }

        _caret.IsVisible = IsActive && IsEffectivelyEnabled;

        // 有字符时插入点在字符右侧：右移半个字符 advance（数字等宽近似）
        var offsetX = string.IsNullOrEmpty(DisplayText) ? 0 : FontSize * 0.30;
        if (_caret.RenderTransform is TranslateTransform translate)
        {
            translate.X = offsetX;
        }
        else
        {
            _caret.RenderTransform = new TranslateTransform(offsetX, 0);
        }
    }

    private void ConfigureTextBoxCursor()
    {
        if (_textBox is not null)
        {
            _textBox.SetCurrentValue(
                CursorProperty,
                IsInputTarget && IsEffectivelyEnabled
                    ? new Cursor(StandardCursorType.Ibeam)
                    : new Cursor(StandardCursorType.Arrow));
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _owner = this.GetVisualAncestors().OfType<OtpLineEdit>().FirstOrDefault();
        if (_owner is not null)
        {
            _owner.PropertyChanged += OwnerPropertyChanged;
            RelayOwnerOverride(OtpLineEdit.CellWidthProperty, WidthProperty);
            RelayOwnerBorderBrush();
        }
    }

    private void OwnerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == OtpLineEdit.CellWidthProperty)
        {
            RelayOwnerOverride(change.Property, WidthProperty);
        }
        else if (change.Property == OtpLineEdit.CellBorderBrushProperty ||
                 change.Property == BorderBrushProperty)
        {
            RelayOwnerBorderBrush();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        if (_owner is not null)
        {
            _owner.PropertyChanged -= OwnerPropertyChanged;
            ClearValue(WidthProperty);
            ClearValue(BorderBrushProperty);
        }
        _owner = null;
        _textBox = null;
    }
}
