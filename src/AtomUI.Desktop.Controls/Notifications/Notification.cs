using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace AtomUI.Desktop.Controls;

public class Notification : INotification, INotifyPropertyChanged
{
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromSeconds(4.5);

    private string _title;
    private object? _content;
    private bool _showProgress;
    private PathIcon? _icon;
    private object? _actions;
    private IDataTemplate? _actionsTemplate;

    public Notification(string title,
                        object? content,
                        NotificationType type = NotificationType.Default,
                        PathIcon? icon = null,
                        TimeSpan? expiration = null,
                        bool showProgress = false,
                        Action? onClick = null,
                        Action? onClose = null,
                        object? actions = null,
                        IDataTemplate? actionsTemplate = null)
    {
        _title           = title;
        _content         = content;
        _icon            = icon;
        _actions         = actions;
        _actionsTemplate = actionsTemplate;
        Type             = type;
        Expiration       = expiration.HasValue ? expiration.Value : DefaultExpiration;
        ShowProgress     = showProgress;
        OnClick          = onClick;
        OnClose          = onClose;
    }

    public string Title
    {
        get => _title;

        set
        {
            if (_title != value)
            {
                _title = value;
                OnPropertyChanged();
            }
        }
    }

    public object? Content
    {
        get => _content;

        set
        {
            if (!ReferenceEquals(_content, value))
            {
                _content = value;
                OnPropertyChanged();
            }
        }
    }

    public PathIcon? Icon
    {
        get => _icon;

        set
        {
            if (!ReferenceEquals(_icon, value))
            {
                _icon = value;
                OnPropertyChanged();
            }
        }
    }

    public bool ShowProgress
    {
        get => _showProgress;

        set
        {
            if (_showProgress != value)
            {
                _showProgress = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// 操作组内容，显示在通知卡片描述下方的操作区。
    /// </summary>
    public object? Actions
    {
        get => _actions;

        set
        {
            if (!ReferenceEquals(_actions, value))
            {
                _actions = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// 操作组内容的数据模板，用于自定义操作区呈现。
    /// </summary>
    public IDataTemplate? ActionsTemplate
    {
        get => _actionsTemplate;

        set
        {
            if (!ReferenceEquals(_actionsTemplate, value))
            {
                _actionsTemplate = value;
                OnPropertyChanged();
            }
        }
    }

    public NotificationType Type { get; set; }

    public TimeSpan Expiration { get; set; }

    public Action? OnClick { get; set; }

    public Action? OnClose { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
