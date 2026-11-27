using System.Windows.Input;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A control that shows <see cref="Text" /> as a hyperlink and raises <see cref="Click" /> when the link is
/// activated with the left mouse button or with the Enter or Space key.
/// </summary>
[PseudoClasses(StdPseudoClass.Pressed)]
public class HyperLinkTextBlock : TemplatedControl, IMotionAwareControl
{
    #region 公共属性定义

    /// <summary>
    /// Defines the <see cref="Text" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> TextProperty =
        TextBlock.TextProperty.AddOwner<HyperLinkTextBlock>();
    
    /// <summary>
    /// Defines the <see cref="TextAlignment" /> property.
    /// </summary>
    public static readonly AttachedProperty<TextAlignment> TextAlignmentProperty = 
        TextBlock.TextAlignmentProperty.AddOwner<HyperLinkTextBlock>();

    /// <summary>
    /// Defines the <see cref="TextWrapping" /> property.
    /// </summary>
    public static readonly AttachedProperty<TextWrapping> TextWrappingProperty =
       TextBlock.TextWrappingProperty.AddOwner<HyperLinkTextBlock>();

    /// <summary>
    /// Defines the <see cref="TextTrimming" /> property.
    /// </summary>
    public static readonly AttachedProperty<TextTrimming> TextTrimmingProperty =
        TextBlock.TextTrimmingProperty.AddOwner<HyperLinkTextBlock>();
    
    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty = MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<HyperLinkTextBlock>();
    
    /// <summary>
    /// Defines the <see cref="Command" /> property.
    /// </summary>
    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<HyperLinkTextBlock, ICommand?>(nameof(Command), enableDataValidation: true);
    
    /// <summary>
    /// Defines the <see cref="CommandParameter" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> CommandParameterProperty =
        AvaloniaProperty.Register<HyperLinkTextBlock, object?>(nameof(CommandParameter));
    
    /// <summary>
    /// Gets or sets the text shown as the link. This is the XAML content property, so the text can be written
    /// as the element content. When <see langword="null" />, no text is shown.
    /// </summary>
    [Content]
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the text alignment. The default theme template does not forward this value to its text
    /// element.
    /// </summary>
    public TextAlignment TextAlignment
    {
        get => GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }
    
    /// <summary>
    /// Gets or sets how the text wraps. The default theme template does not forward this value to its text
    /// element.
    /// </summary>
    public TextWrapping TextWrapping
    {
        get => GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }
    
    /// <summary>
    /// Gets or sets how the text is trimmed when it does not fit. The default theme template does not forward
    /// this value to its text element.
    /// </summary>
    public TextTrimming TextTrimming
    {
        get => GetValue(TextTrimmingProperty);
        set => SetValue(TextTrimmingProperty, value);
    }
    
    /// <summary>
    /// Gets or sets whether the foreground transition between the normal, pointer-over and pressed link
    /// colors plays. The theme sets it from the <c>EnableMotion</c> token; set it to
    /// <see langword="false" /> on the control to switch the transition off.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the command to execute after <see cref="Click" /> is raised. It is skipped when a
    /// <see cref="Click" /> handler marked the event handled, or when the command reports that it cannot execute
    /// with <see cref="CommandParameter" />.
    /// </summary>
    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }
    
    /// <summary>
    /// Gets or sets a parameter to be passed to the <see cref="Command" />.
    /// </summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }
    
    #endregion

    #region 公共事件定义

    /// <summary>
    /// Identifies the <see cref="Click" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> ClickEvent =
        RoutedEvent.Register<HyperLinkTextBlock, RoutedEventArgs>(nameof(Click), RoutingStrategies.Bubble);
    
    /// <summary>
    /// Occurs when the link is activated with the left mouse button, the Enter key or the Space key. The mouse
    /// button and the Space key raise it at the moment selected by <see cref="ClickMode" />; the Enter key raises
    /// it on key down regardless of that value. The event is not raised while <c>IsEffectivelyEnabled</c> is
    /// false. It is raised before <see cref="Command" /> runs, and a handler that marks the event handled
    /// prevents the command from running.
    /// </summary>
    public event EventHandler<RoutedEventArgs>? Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }

    #endregion

    #region 内部属性定义
    
    internal static readonly DirectProperty<HyperLinkTextBlock, bool> IsPressedProperty =
        AvaloniaProperty.RegisterDirect<HyperLinkTextBlock, bool>(nameof(IsPressed), b => b.IsPressed);
    
    /// <summary>
    /// Identifies the <see cref="ClickMode" /> property.
    /// </summary>
    public static readonly StyledProperty<ClickMode> ClickModeProperty =
        AvaloniaProperty.Register<HyperLinkTextBlock, ClickMode>(nameof(ClickMode));
    
    /// <summary>
    /// Gets whether the link is being pressed: the left mouse button was pressed on it, or the Space key was
    /// pressed while it had focus, and the press has not ended. While <see langword="true" /> the
    /// <c>:pressed</c> pseudo-class is set. The value is managed by the control.
    /// </summary>
    public bool IsPressed
    {
        get => _isPressed;
        private set => SetAndRaise(IsPressedProperty, ref _isPressed, value);
    }
    private bool _isPressed = false;
    
    /// <summary>
    /// Gets or sets whether activation with the left mouse button and the Space key completes when the press
    /// starts (<c>ClickMode.Press</c>) or when it ends (<c>ClickMode.Release</c>, the default). The Enter key is
    /// not affected.
    /// </summary>
    public ClickMode ClickMode
    {
        get => GetValue(ClickModeProperty);
        set => SetValue(ClickModeProperty, value);
    }

    #endregion
    
    private bool _commandCanExecute = true;
    private EventHandler? _canExecuteChangeHandler = default;
    private EventHandler CanExecuteChangedHandler => _canExecuteChangeHandler ??= new(CanExecuteChanged);

    /// <summary>
    /// Applies the template and then sets the <c>:pressed</c> pseudo-class from the current
    /// <see cref="IsPressed" /> value. Call the base method from an override to keep that pseudo-class in sync.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdatePseudoClasses();
    }

    /// <summary>
    /// Raises the change notification and, when <see cref="IsPressed" /> changed, sets the <c>:pressed</c>
    /// pseudo-class from the new value. Call the base method from an override so that both still happen.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsPressedProperty)
        {
            UpdatePseudoClasses();
        }
    }

    /// <summary>
    /// For the left mouse button, marks the link as pressed and the event as handled, and raises
    /// <see cref="Click" /> when <see cref="ClickMode" /> is <c>Press</c>. Presses of other buttons are left
    /// untouched. The base method is called first; call it from an override.
    /// </summary>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            IsPressed = true;
            e.Handled = true;

            if (ClickMode == ClickMode.Press)
            {
                OnClick();
            }
        }
    }
    
    /// <summary>
    /// For a left button release that ends a press on the link, clears <see cref="IsPressed" />, marks the event
    /// as handled and, when <see cref="ClickMode" /> is <c>Release</c> and the pointer is still over the link,
    /// raises <see cref="Click" />. Releases of other buttons are left untouched. The base method is called
    /// first; call it from an override.
    /// </summary>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (IsPressed && e.InitialPressMouseButton == MouseButton.Left)
        {
            IsPressed = false;
            e.Handled = true;

            if (ClickMode == ClickMode.Release && IsPointerOverSelf(e))
            {
                OnClick();
            }
        }
    }
    
    /// <summary>
    /// For the Enter key, raises <see cref="Click" />. For the Space key, when the link has focus, raises
    /// <see cref="Click" /> if <see cref="ClickMode" /> is <c>Press</c>, then marks the link as pressed; the
    /// press ends in <see cref="OnKeyUp" />. These keys are marked as handled. The base method is called last;
    /// call it from an override.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                OnClick();
                e.Handled = true;
                break;
            case Key.Space:
                if (IsFocused)
                {
                    if (ClickMode == ClickMode.Press)
                    {
                        OnClick();
                    }

                    IsPressed = true;
                    e.Handled = true;
                }
                break;
        }
        base.OnKeyDown(e);
    }
    
    /// <summary>
    /// For the Space key, when the link has focus, raises <see cref="Click" /> if <see cref="ClickMode" /> is
    /// <c>Release</c>, then clears <see cref="IsPressed" /> and marks the event as handled. Other keys are left
    /// untouched. The base method is called last; call it from an override.
    /// </summary>
    protected override void OnKeyUp(KeyEventArgs e)
    {
        if (e.Key == Key.Space && IsFocused)
        {
            if (ClickMode == ClickMode.Release)
            {
                OnClick();
            }
            IsPressed = false;
            e.Handled = true;
        }

        base.OnKeyUp(e);
    }
    
    /// <summary>
    /// Clears <see cref="IsPressed" /> when the pointer capture is lost, so that the link does not stay pressed
    /// after an interrupted press. The base method is called first; call it from an override.
    /// </summary>
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        IsPressed = false;
    }

    private bool IsPointerOverSelf(PointerReleasedEventArgs e)
    {
        foreach (var visual in this.GetVisualsAt(e.GetPosition(this)))
        {
            if (this == visual || this.IsVisualAncestorOf(visual))
            {
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Clears <see cref="IsPressed" /> when the link loses keyboard focus, so that a Space press interrupted by
    /// the focus change does not stay pressed. The base method is called first; call it from an override.
    /// </summary>
    protected override void OnLostFocus(FocusChangedEventArgs e)
    {
        base.OnLostFocus(e);
        IsPressed = false;
    }
    
    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(StdPseudoClass.Pressed, IsPressed);
    }
    
    /// <summary>
    /// Raises the <see cref="Click" /> event and then executes <see cref="Command" /> with
    /// <see cref="CommandParameter" />, unless a handler marked the event handled or the command cannot
    /// execute. Called by the control whenever the link is activated. Does nothing while
    /// <c>IsEffectivelyEnabled</c> is false. Call the base method from an override to keep both working.
    /// </summary>
    protected virtual void OnClick()
    {
        if (IsEffectivelyEnabled)
        {
            var e = new RoutedEventArgs(ClickEvent);
            RaiseEvent(e);

            var (command, parameter) = (Command, CommandParameter);
            if (!e.Handled && command is not null && command.CanExecute(parameter))
            {
                command.Execute(parameter);
                e.Handled = true;
            }
        }
    }
    
    /// <summary>
    /// Attaches the control to the logical tree and, when <see cref="Command" /> is set, subscribes to its
    /// <c>CanExecuteChanged</c> event and re-evaluates <c>CanExecute</c> for <see cref="CommandParameter" />.
    /// The base method is called first; call it from an override.
    /// </summary>
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);

        var (command, parameter) = (Command, CommandParameter);
        if (command is not null)
        {
            command.CanExecuteChanged += CanExecuteChangedHandler;
            CanExecuteChanged(command, parameter);
        }
    }

    /// <summary>
    /// Detaches the control from the logical tree and unsubscribes from the <c>CanExecuteChanged</c> event of
    /// <see cref="Command" />. The base method is called first; call it from an override.
    /// </summary>
    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);

        if (Command is { } command)
        {
            command.CanExecuteChanged -= CanExecuteChangedHandler;
        }
    }
    
    private void CanExecuteChanged(object? sender, EventArgs e)
    {
        CanExecuteChanged(Command, CommandParameter);
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private void CanExecuteChanged(ICommand? command, object? parameter)
    {
        if (!((ILogical)this).IsAttachedToLogicalTree)
        {
            return;
        }

        var canExecute = command == null || command.CanExecute(parameter);

        if (canExecute != _commandCanExecute)
        {
            _commandCanExecute = canExecute;
            UpdateIsEffectivelyEnabled();
        }
    }

}
