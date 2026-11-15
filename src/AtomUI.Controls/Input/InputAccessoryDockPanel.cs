using AtomUI.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.LogicalTree;

namespace AtomUI.Controls;

// Keeps the deferred count in the same dock slot as an eagerly declared footer.
internal sealed class InputAccessoryDockPanel : DockPanel
{
    public static readonly StyledProperty<IDataTemplate?> CountTemplateProperty =
        AvaloniaProperty.Register<InputAccessoryDockPanel, IDataTemplate?>(nameof(CountTemplate));

    private AbstractTextInput? _input;
    private Control? _count;

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
            if (!EnsureCount())
            {
                input.PropertyChanged += OnInputPropertyChanged;
            }
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
        if (e.Property == AbstractTextInput.IsShowCountProperty && EnsureCount() && _input is not null)
        {
            _input.PropertyChanged -= OnInputPropertyChanged;
        }
    }

    private bool EnsureCount()
    {
        if (_count is null && _input is { IsShowCount: true } input && CountTemplate?.Build(input) is { } count)
        {
            _count = count;
            count.SetTemplatedParent(input);
            Children.Insert(0, count);
        }

        return _count is not null;
    }
}
