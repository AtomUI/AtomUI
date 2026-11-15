using AtomUI.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.LogicalTree;

namespace AtomUI.Controls;

// Factories in the owner template create optional direct children while retaining semantic-part routes.
internal sealed class InputAccessoryPanel : StackPanel
{
    public static readonly StyledProperty<IDataTemplate?> ClearButtonTemplateProperty =
        AvaloniaProperty.Register<InputAccessoryPanel, IDataTemplate?>(nameof(ClearButtonTemplate));

    public static readonly StyledProperty<IDataTemplate?> CountTemplateProperty =
        AvaloniaProperty.Register<InputAccessoryPanel, IDataTemplate?>(nameof(CountTemplate));

    private AbstractTextInput? _input;
    private Button? _clearButton;
    private Control? _count;

    public IDataTemplate? ClearButtonTemplate
    {
        get => GetValue(ClearButtonTemplateProperty);
        set => SetValue(ClearButtonTemplateProperty, value);
    }

    public IDataTemplate? CountTemplate
    {
        get => GetValue(CountTemplateProperty);
        set => SetValue(CountTemplateProperty, value);
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        if ((TemplatedParent as AbstractTextInput ?? this.GetLogicalAncestors().OfType<AbstractTextInput>().FirstOrDefault()) is { } input)
        {
            _input = input;
            _input.PropertyChanged += OnInputPropertyChanged;
            SyncAccessories();
        }
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        if (_input is not null)
        {
            _input.PropertyChanged -= OnInputPropertyChanged;
            _input = null;
        }
        base.OnDetachedFromLogicalTree(e);
    }

    private void OnInputPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == AbstractTextInput.IsAllowClearProperty || e.Property == AbstractTextInput.IsShowCountProperty)
        {
            SyncAccessories();
        }
    }

    private void SyncAccessories()
    {
        if (_input is not { } input)
        {
            return;
        }

        if (_clearButton is null && input.IsAllowClear && ClearButtonTemplate?.Build(input) is Button button)
        {
            _clearButton = button;
            button.SetTemplatedParent(input);
            Children.Insert(0, button);
        }

        if (_clearButton is not null)
        {
            input.SetClearButton(_clearButton);
        }

        if (_count is null && input.IsShowCount && CountTemplate?.Build(input) is { } count)
        {
            _count = count;
            count.SetTemplatedParent(input);
            Children.Add(count);
        }
    }
}
