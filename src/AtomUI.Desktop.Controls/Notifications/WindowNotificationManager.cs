using System.Collections;
using System.Collections.Specialized;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

[TemplatePart(WindowNotificationManagerThemeConstants.ItemsPart, typeof(Panel))]
[PseudoClasses(NotificationPseudoClass.TopLeft, 
    NotificationPseudoClass.TopRight,
    NotificationPseudoClass.BottomLeft,
    NotificationPseudoClass.BottomRight, 
    NotificationPseudoClass.TopCenter,
    NotificationPseudoClass.BottomCenter)]
public class WindowNotificationManager : TemplatedControl, 
                                         INotificationManager,
                                         IMotionAwareControl,
                                         IControlSharedTokenResourcesHost
{
    private IList? _items;
    private readonly Queue<NotificationCard> _cleanupQueue;
    private readonly DispatcherTimer _cardExpiredTimer;
    private readonly DispatcherTimer _cleanupTimer;
    private IDisposable? _bindingDisposable;

    #region 公共属性定义
    public static readonly StyledProperty<NotificationPosition> PositionProperty =
        AvaloniaProperty.Register<WindowNotificationManager, NotificationPosition>(
            nameof(Position), NotificationPosition.TopRight);
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<WindowNotificationManager>();

    public static readonly StyledProperty<int> MaxItemsProperty =
        AvaloniaProperty.Register<WindowNotificationManager, int>(nameof(MaxItems), 5);

    public static readonly StyledProperty<bool> IsPauseOnHoverProperty =
        AvaloniaProperty.Register<WindowNotificationManager, bool>(nameof(IsPauseOnHover), true);

    public NotificationPosition Position
    {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    public int MaxItems
    {
        get => GetValue(MaxItemsProperty);
        set => SetValue(MaxItemsProperty, value);
    }

    public bool IsPauseOnHover
    {
        get => GetValue(IsPauseOnHoverProperty);
        set => SetValue(IsPauseOnHoverProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    #endregion
    
    #region 内部属性定义
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => NotificationToken.ID;

    #endregion

    public WindowNotificationManager(TopLevel? host) : this()
    {
        if (host is not null)
        {
            InstallFromTopLevel(host);
        }
    }

    public WindowNotificationManager()
    {
        this.RegisterResources();
        _cardExpiredTimer      =  new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50), Tag = this };
        _cardExpiredTimer.Tick += HandleCardExpiredTimer;
        _cleanupTimer          =  new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50), Tag = this };
        _cleanupTimer.Tick     += HandleCleanupTimerTick;
        _cleanupQueue          =  new Queue<NotificationCard>();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        var itemsControl = e.NameScope.Find<Panel>(WindowNotificationManagerThemeConstants.ItemsPart);
        _items = itemsControl?.Children;
        if (itemsControl is not null)
        {
            itemsControl.Children.CollectionChanged += HandleCollectionChanged;
        }
    }

    private void HandleCardExpiredTimer(object? sender, EventArgs eventArgs)
    {
        if (_items is not null)
        {
            foreach (var item in _items)
            {
                if (item is NotificationCard card)
                {
                    if (card.NotifyCloseTick(_cardExpiredTimer.Interval))
                    {
                        if (!_cleanupQueue.Contains(card))
                        {
                            _cleanupQueue.Enqueue(card);
                            if (!_cleanupTimer.IsEnabled)
                            {
                                _cleanupTimer.Start();
                            }
                        }
                    }
                }
            }
        }
    }

    private void HandleCleanupTimerTick(object? sender, EventArgs eventArgs)
    {
        if (_cleanupQueue.Count > 0)
        {
            var card = _cleanupQueue.Peek();
            if (!card.IsClosing)
            {
                card.Close();
            }
            else if (card.IsClosed)
            {
                _cleanupQueue.Dequeue();
                if (_cleanupQueue.Count == 0)
                {
                    _cleanupTimer.Stop();
                }
            }
        }
    }

    private void HandleCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (_items is not null)
        {
            if (_items.Count > 0)
            {
                _cardExpiredTimer.Start();
            }
            else
            {
                _cardExpiredTimer.Stop();
            }
        }
    }

    public void Show(INotification notification, string[]? classes = null)
    {
        var expiration = notification.Expiration;
        var onClick    = notification.OnClick;
        var onClose    = notification.OnClose;
        Dispatcher.UIThread.VerifyAccess();

        var notificationControl = new NotificationCard(this)
        {
            Title            = notification.Title,
            Content          = notification.Content,
            Icon             = notification.Icon,
            NotificationType = notification.Type,
            Expiration       = expiration == TimeSpan.Zero ? null : expiration,
            IsShowProgress   = notification.ShowProgress
        };
        _bindingDisposable?.Dispose();
        _bindingDisposable = BindUtils.RelayBind(this, PositionProperty, notificationControl, NotificationCard.PositionProperty);

        // Add style classes if any
        if (classes != null)
        {
            foreach (var cls in classes)
            {
                notificationControl.Classes.Add(cls);
            }
        }

        notificationControl.PointerPressed += (sender, args) => { onClick?.Invoke(); };

        notificationControl.NotificationClosed += (sender, args) =>
        {
            onClose?.Invoke();
            _bindingDisposable?.Dispose();
            _items?.Remove(sender);
        };

        Dispatcher.UIThread.Post(() =>
        {
            _items?.Add(notificationControl);

            if (_items?.OfType<NotificationCard>().Count(i => !i.IsClosing) > MaxItems)
            {
                _items.OfType<NotificationCard>().First(i => !i.IsClosing).Close();
            }
        });
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == PositionProperty)
        {
            UpdatePseudoClasses(change.GetNewValue<NotificationPosition>());
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdatePseudoClasses(Position);
    }

    private void InstallFromTopLevel(TopLevel topLevel)
    {
        topLevel.TemplateApplied += TopLevelOnTemplateApplied;
        var adorner = topLevel.FindDescendantOfType<VisualLayerManager>()?.AdornerLayer;
        if (adorner is not null)
        {
            adorner.Children.Add(this);
            AdornerLayer.SetAdornedElement(this, adorner);
        }
    }

    private void TopLevelOnTemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        if (Parent is AdornerLayer adornerLayer)
        {
            adornerLayer.Children.Remove(this);
            AdornerLayer.SetAdornedElement(this, null);
        }

        // Reinstall notification manager on template reapplied.
        var topLevel = (TopLevel)sender!;
        topLevel.TemplateApplied -= TopLevelOnTemplateApplied;
        InstallFromTopLevel(topLevel);
    }

    private void UpdatePseudoClasses(NotificationPosition position)
    {
        PseudoClasses.Set(NotificationPseudoClass.TopLeft, position == NotificationPosition.TopLeft);
        PseudoClasses.Set(NotificationPseudoClass.TopRight, position == NotificationPosition.TopRight);
        PseudoClasses.Set(NotificationPseudoClass.BottomLeft, position == NotificationPosition.BottomLeft);
        PseudoClasses.Set(NotificationPseudoClass.BottomRight, position == NotificationPosition.BottomRight);
        PseudoClasses.Set(NotificationPseudoClass.TopCenter, position == NotificationPosition.TopCenter);
        PseudoClasses.Set(NotificationPseudoClass.BottomCenter, position == NotificationPosition.BottomCenter);
    }

    internal void StopExpiredTimer()
    {
        _cardExpiredTimer.Stop();
    }

    internal void StartExpiredTimer()
    {
        _cardExpiredTimer.Start();
    }
}