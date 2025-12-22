using System.Windows.Input;
using AtomUI.Animations;
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

[PseudoClasses(StdPseudoClass.Pressed)]
public class HyperLinkTextBlock : TemplatedControl, IMotionAwareControl
{
    #region 公共属性定义

    /// <summary>
    /// Defines the <see cref="Text"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> TextProperty =
        TextBlock.TextProperty.AddOwner<HyperLinkTextBlock>();
    
    /// <summary>
    /// Defines the <see cref="TextAlignment"/> property.
    /// </summary>
    public static readonly AttachedProperty<TextAlignment> TextAlignmentProperty = 
        TextBlock.TextAlignmentProperty.AddOwner<HyperLinkTextBlock>();

    /// <summary>
    /// Defines the <see cref="TextWrapping"/> property.
    /// </summary>
    public static readonly AttachedProperty<TextWrapping> TextWrappingProperty =
       TextBlock.TextWrappingProperty.AddOwner<HyperLinkTextBlock>();

    /// <summary>
    /// Defines the <see cref="TextTrimming"/> property.
    /// </summary>
    public static readonly AttachedProperty<TextTrimming> TextTrimmingProperty =
        TextBlock.TextTrimmingProperty.AddOwner<HyperLinkTextBlock>();
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty
        = MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<HyperLinkTextBlock>();
    
    /// <summary>
    /// Defines the <see cref="Command"/> property.
    /// </summary>
    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<Button, ICommand?>(nameof(Command), enableDataValidation: true);
    
    /// <summary>
    /// Defines the <see cref="CommandParameter"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> CommandParameterProperty =
        AvaloniaProperty.Register<Button, object?>(nameof(CommandParameter));
    
    /// <summary>
    /// Gets or sets the text.
    /// </summary>
    [Content]
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the text alignment.
    /// </summary>
    public TextAlignment TextAlignment
    {
        get => GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the control's text wrapping mode.
    /// </summary>
    public TextWrapping TextWrapping
    {
        get => GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the control's text trimming mode.
    /// </summary>
    public TextTrimming TextTrimming
    {
        get => GetValue(TextTrimmingProperty);
        set => SetValue(TextTrimmingProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    /// <summary>
    /// Gets or sets an <see cref="ICommand"/> to be invoked when the button is clicked.
    /// </summary>
    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }
    
    /// <summary>
    /// Gets or sets a parameter to be passed to the <see cref="Command"/>.
    /// </summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }
    
    #endregion

    #region 公共事件定义

    public static readonly RoutedEvent<RoutedEventArgs> ClickEvent =
        RoutedEvent.Register<HyperLinkTextBlock, RoutedEventArgs>(nameof(Click), RoutingStrategies.Bubble);
    
    public event EventHandler<RoutedEventArgs>? Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }

    #endregion

    #region 内部属性定义
    
    internal static readonly DirectProperty<HyperLinkTextBlock, bool> IsPressedProperty =
        AvaloniaProperty.RegisterDirect<HyperLinkTextBlock, bool>(nameof(IsPressed), b => b.IsPressed);
    
    public static readonly StyledProperty<ClickMode> ClickModeProperty =
        AvaloniaProperty.Register<HyperLinkTextBlock, ClickMode>(nameof(ClickMode));
    
    public bool IsPressed
    {
        get => _isPressed;
        private set => SetAndRaise(IsPressedProperty, ref _isPressed, value);
    }
    private bool _isPressed = false;
    
    public ClickMode ClickMode
    {
        get => GetValue(ClickModeProperty);
        set => SetValue(ClickModeProperty, value);
    }

    #endregion
    
    private bool _commandCanExecute = true;
    private EventHandler? _canExecuteChangeHandler = default;
    private EventHandler CanExecuteChangedHandler => _canExecuteChangeHandler ??= new(CanExecuteChanged);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdatePseudoClasses();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ConfigureTransitions(false);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        Transitions = null;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsPressedProperty)
        {
            UpdatePseudoClasses();
        }

        if (IsLoaded)
        {
            if (change.Property == IsMotionEnabledProperty)
            {
                ConfigureTransitions(true);
            }
        }
    }

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
    
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (IsPressed && e.InitialPressMouseButton == MouseButton.Left)
        {
            IsPressed = false;
            e.Handled = true;

            if (ClickMode == ClickMode.Release &&
                this.GetVisualsAt(e.GetPosition(this)).Any(c => this == c || this.IsVisualAncestorOf(c)))
            {
                OnClick();
            }
        }
    }
    
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
    
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        IsPressed = false;
    }
    
    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);
        IsPressed = false;
    }
    
    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(StdPseudoClass.Pressed, IsPressed);
    }
    
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

    /// <inheritdoc/>
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
    
    private void ConfigureTransitions(bool force)
    {
        if (IsMotionEnabled)
        {
            if (force || Transitions is null)
            {
                Transitions = [
                    TransitionUtils.CreateTransition<SolidColorBrushTransition>(ForegroundProperty)
                ];
            }
        }
        else
        {
            Transitions = null;
        }
    }
}