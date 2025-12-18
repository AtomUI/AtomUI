using System.Collections.Specialized;
using System.Diagnostics;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public class DialogButtonBox : TemplatedControl, 
                               IControlSharedTokenResourcesHost,
                               IMotionAwareControl
{
    #region 公共属性定义

    public static readonly StyledProperty<DialogStandardButtons> StandardButtonsProperty =
        AvaloniaProperty.Register<DialogButtonBox, DialogStandardButtons>(nameof (StandardButtons), DialogStandardButton.NoButton);
    
    public static readonly StyledProperty<DialogStandardButton> DefaultStandardButtonProperty =
        AvaloniaProperty.Register<DialogButtonBox, DialogStandardButton>(nameof (DefaultStandardButton));
    
    public static readonly StyledProperty<DialogStandardButton> EscapeStandardButtonProperty =
        AvaloniaProperty.Register<DialogButtonBox, DialogStandardButton>(nameof (EscapeStandardButton));
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<DialogButtonBox>();
    
    public DialogStandardButtons StandardButtons
    {
        get => GetValue(StandardButtonsProperty);
        set => SetValue(StandardButtonsProperty, value);
    }
    
    public DialogStandardButton DefaultStandardButton
    {
        get => GetValue(DefaultStandardButtonProperty);
        set => SetValue(DefaultStandardButtonProperty, value);
    }
    
    public DialogStandardButton EscapeStandardButton
    {
        get => GetValue(EscapeStandardButtonProperty);
        set => SetValue(EscapeStandardButtonProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    public AvaloniaList<DialogButton> CustomButtons { get; } = new ();

    public Dictionary<DialogButtonRole, List<DialogButton>> ButtonGroup => _buttonGroup;
    public Dictionary<DialogButtonRole, List<DialogButton>> StandardButtonGroup => _standardButtonGroup;
    
    #endregion

    #region 公共事件定义

    public event EventHandler? Accepted;
    public event EventHandler<DialogButtonClickedEventArgs>? Clicked;
    public event EventHandler? HelpRequested;
    public event EventHandler? Rejected;
    public event EventHandler<DialogBoxButtonSyncEventArgs>? ButtonsSynchronized;

    #endregion

    #region 按钮语言属性定义

    public static readonly StyledProperty<string?> OkButtonTextProperty =
        AvaloniaProperty.Register<DialogButtonBox, string?>(nameof (OkButtonText));
    
    public static readonly StyledProperty<string?> OpenButtonTextProperty =
        AvaloniaProperty.Register<DialogButtonBox, string?>(nameof (OpenButtonText));

    public static readonly StyledProperty<string?> SaveButtonTextProperty =
        AvaloniaProperty.Register<DialogButtonBox, string?>(nameof (SaveButtonText));
    
    public static readonly StyledProperty<string?> CancelButtonTextProperty =
        AvaloniaProperty.Register<DialogButtonBox, string?>(nameof (CancelButtonText));
    
    public static readonly StyledProperty<string?> CloseButtonTextProperty =
        AvaloniaProperty.Register<DialogButtonBox, string?>(nameof (CloseButtonText));

    public static readonly StyledProperty<string?> DiscardButtonTextProperty =
        AvaloniaProperty.Register<DialogButtonBox, string?>(nameof (DiscardButtonText));
    
    public static readonly StyledProperty<string?> ApplyButtonTextProperty =
        AvaloniaProperty.Register<DialogButtonBox, string?>(nameof (ApplyButtonText));
    
    public static readonly StyledProperty<string?> ResetButtonTextProperty =
        AvaloniaProperty.Register<DialogButtonBox, string?>(nameof (ResetButtonText));
    
    public static readonly StyledProperty<string?> ReloadButtonTextProperty =
        AvaloniaProperty.Register<DialogButtonBox, string?>(nameof (ReloadButtonText));
    
    public static readonly StyledProperty<string?> RestoreDefaultsButtonTextProperty =
        AvaloniaProperty.Register<DialogButtonBox, string?>(nameof (RestoreDefaultsButtonText));
    
    public static readonly StyledProperty<string?> HelpButtonTextProperty =
        AvaloniaProperty.Register<DialogButtonBox, string?>(nameof (HelpButtonText));
    
    public static readonly StyledProperty<string?> SaveAllButtonTextProperty =
        AvaloniaProperty.Register<DialogButtonBox, string?>(nameof (SaveAllButtonText));
    
    public static readonly StyledProperty<string?> YesButtonTextProperty =
        AvaloniaProperty.Register<DialogButtonBox, string?>(nameof (YesButtonText));
    
    public static readonly StyledProperty<string?> YesToAllButtonTextProperty =
        AvaloniaProperty.Register<DialogButtonBox, string?>(nameof (YesToAllButtonText));
    
    public static readonly StyledProperty<string?> NoButtonTextProperty =
        AvaloniaProperty.Register<DialogButtonBox, string?>(nameof (NoButtonText));
    
    public static readonly StyledProperty<string?> NoToAllButtonTextProperty =
        AvaloniaProperty.Register<DialogButtonBox, string?>(nameof (NoToAllButtonText));
    
    public static readonly StyledProperty<string?> AbortButtonTextProperty =
        AvaloniaProperty.Register<DialogButtonBox, string?>(nameof (AbortButtonText));
    
    public static readonly StyledProperty<string?> RetryButtonTextProperty =
        AvaloniaProperty.Register<DialogButtonBox, string?>(nameof (RetryButtonText));
    
    public static readonly StyledProperty<string?> IgnoreButtonTextProperty =
        AvaloniaProperty.Register<DialogButtonBox, string?>(nameof (IgnoreButtonText));
    
    public string? OkButtonText
    {
        get => GetValue(OkButtonTextProperty);
        set => SetValue(OkButtonTextProperty, value);
    }
    
    public string? OpenButtonText
    {
        get => GetValue(OpenButtonTextProperty);
        set => SetValue(OpenButtonTextProperty, value);
    }
    
    public string? SaveButtonText
    {
        get => GetValue(SaveButtonTextProperty);
        set => SetValue(SaveButtonTextProperty, value);
    }
    
    public string? CancelButtonText
    {
        get => GetValue(CancelButtonTextProperty);
        set => SetValue(CancelButtonTextProperty, value);
    }
    
    public string? CloseButtonText
    {
        get => GetValue(CloseButtonTextProperty);
        set => SetValue(CloseButtonTextProperty, value);
    }
    
    public string? DiscardButtonText
    {
        get => GetValue(DiscardButtonTextProperty);
        set => SetValue(DiscardButtonTextProperty, value);
    }
    
    public string? ApplyButtonText
    {
        get => GetValue(ApplyButtonTextProperty);
        set => SetValue(ApplyButtonTextProperty, value);
    }
    
    public string? ResetButtonText
    {
        get => GetValue(ResetButtonTextProperty);
        set => SetValue(ResetButtonTextProperty, value);
    }
    
    public string? ReloadButtonText
    {
        get => GetValue(ReloadButtonTextProperty);
        set => SetValue(ReloadButtonTextProperty, value);
    }
    
    public string? RestoreDefaultsButtonText
    {
        get => GetValue(RestoreDefaultsButtonTextProperty);
        set => SetValue(RestoreDefaultsButtonTextProperty, value);
    }
    
    public string? HelpButtonText
    {
        get => GetValue(HelpButtonTextProperty);
        set => SetValue(HelpButtonTextProperty, value);
    }
    
    public string? SaveAllButtonText
    {
        get => GetValue(SaveAllButtonTextProperty);
        set => SetValue(SaveAllButtonTextProperty, value);
    }
    
    public string? YesButtonText
    {
        get => GetValue(YesButtonTextProperty);
        set => SetValue(YesButtonTextProperty, value);
    }
    
    public string? YesToAllButtonText
    {
        get => GetValue(YesToAllButtonTextProperty);
        set => SetValue(YesToAllButtonTextProperty, value);
    }
    
    public string? NoButtonText
    {
        get => GetValue(NoButtonTextProperty);
        set => SetValue(NoButtonTextProperty, value);
    }
    
    public string? NoToAllButtonText
    {
        get => GetValue(NoToAllButtonTextProperty);
        set => SetValue(NoToAllButtonTextProperty, value);
    }
    
    public string? AbortButtonText
    {
        get => GetValue(AbortButtonTextProperty);
        set => SetValue(AbortButtonTextProperty, value);
    }
    
    public string? RetryButtonText
    {
        get => GetValue(RetryButtonTextProperty);
        set => SetValue(RetryButtonTextProperty, value);
    }
    
    public string? IgnoreButtonText
    {
        get => GetValue(IgnoreButtonTextProperty);
        set => SetValue(IgnoreButtonTextProperty, value);
    }
    #endregion
    
    #region 内部属性定义
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => DialogToken.ID;
    #endregion

    private DockPanel? _leftGroup;
    private DockPanel? _centerGroup;
    private DockPanel? _rightGroup;
    private List<DialogButton>? _standardButtons;
    private Dictionary<DialogButtonRole, List<DialogButton>> _buttonGroup;
    private Dictionary<DialogButtonRole, List<DialogButton>> _standardButtonGroup;
    private Dictionary<DialogButton, CompositeDisposable> _bindingDisposables;
    
    static DialogButtonBox()
    {
        AffectsMeasure<DialogButtonBox>(StandardButtonsProperty);
    }
    
    public DialogButtonBox()
    {
        this.RegisterResources();
        CustomButtons.CollectionChanged += new NotifyCollectionChangedEventHandler(HandleCustomButtonsChanged);
        _standardButtonGroup            =  new Dictionary<DialogButtonRole, List<DialogButton>>();
        _buttonGroup                    =  new Dictionary<DialogButtonRole, List<DialogButton>>();
        _bindingDisposables             =  new Dictionary<DialogButton, CompositeDisposable>();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == StandardButtonsProperty)
        {
            BuildStandardButtons();
            if (this.IsAttachedToVisualTree())
            {
                SyncButtonsToGroup();
            }
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _leftGroup   = e.NameScope.Find<DockPanel>(DialogButtonBoxThemeConstants.LeftGroupPart);
        _centerGroup = e.NameScope.Find<DockPanel>(DialogButtonBoxThemeConstants.CenterGroupPart);
        _rightGroup  = e.NameScope.Find<DockPanel>(DialogButtonBoxThemeConstants.RightGroupPart);
        BuildStandardButtons();
        SyncButtonsToGroup();
    }

    private void HandleCustomButtonsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                var newItems = e.NewItems!.OfType<DialogButton>();
                foreach (var button in newItems)
                {
                    AddButtonToGroup(button.Role, button, false);
                }
                break;
            case NotifyCollectionChangedAction.Remove:
                var oldItems = e.OldItems!.OfType<DialogButton>();
                foreach (var button in oldItems)
                {
                    RemoveButtonFromGroup(button.Role, button, false);
                }
                break;
            case NotifyCollectionChangedAction.Replace:
            case NotifyCollectionChangedAction.Move:
            case NotifyCollectionChangedAction.Reset:
                throw new NotSupportedException();
        }

        SyncButtonsToGroup();
    }

    private void AddButtonToGroup(DialogButtonRole buttonRole, DialogButton button, bool isStandard)
    {
        List<DialogButton>? buttons;
        var targetGroup = isStandard ? _standardButtonGroup : _buttonGroup;
        if (!targetGroup.ContainsKey(buttonRole))
        {
            buttons = new List<DialogButton>();
            targetGroup.Add(buttonRole, buttons);
        }
        else
        {
            buttons = targetGroup[buttonRole];
        }

        button.Click += HandleButtonClicked;
        buttons.Add(button);
    }
    
    private void RemoveButtonFromGroup(DialogButtonRole buttonRole, DialogButton button, bool isStandard)
    {
        var                    targetGroup = isStandard ? _standardButtonGroup : _buttonGroup;
        List<DialogButton>? buttons;
        if (!targetGroup.ContainsKey(buttonRole))
        {
            buttons = new List<DialogButton>();
            targetGroup.Add(buttonRole, buttons);
        }
        else
        {
            buttons = targetGroup[buttonRole];
        }
        button.Click -= HandleButtonClicked;
        buttons.Remove(button);
    }

    private void BuildStandardButtons()
    {
        if (_standardButtons != null)
        {
            foreach (var button in _standardButtons)
            {
                RemoveButtonFromGroup(button.Role, button, true);
                if (_bindingDisposables.TryGetValue(button, out var disposables))
                {
                    disposables.Dispose();
                    _bindingDisposables.Remove(button);
                }
            }
        }
        if (StandardButtons == DialogStandardButton.NoButton)
        {
            return;
        }
        
        _standardButtons = new List<DialogButton>();
        
        if (StandardButtons.HasFlag(DialogStandardButton.Ok))
        {
            var button = new DialogButton
            {
                Role = DialogButtonRole.AcceptRole,
                ButtonType = DefaultStandardButton == DialogStandardButton.Ok ? ButtonType.Primary : ButtonType.Default,
                IsDefaultConfirmButton = DefaultStandardButton == DialogStandardButton.Ok,
                IsDefaultEscapeButton = EscapeStandardButton == DialogStandardButton.Ok,
                StandardButtonType = DialogStandardButton.Ok
            };
            var disposables = new CompositeDisposable(2);
            disposables.Add(BindUtils.RelayBind(this, OkButtonTextProperty, button, Button.ContentProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, button, Button.IsMotionEnabledProperty));
            _bindingDisposables.Add(button, disposables);
            _standardButtons.Add(button);
            AddButtonToGroup(DialogButtonRole.AcceptRole, button, true);
        }

        if (StandardButtons.HasFlag(DialogStandardButton.Open))
        {
            var button = new DialogButton
            {
                Role        = DialogButtonRole.AcceptRole,
                ButtonType = DefaultStandardButton == DialogStandardButton.Open ? ButtonType.Primary : ButtonType.Default,
                IsDefaultConfirmButton = DefaultStandardButton == DialogStandardButton.Open,
                IsDefaultEscapeButton = EscapeStandardButton == DialogStandardButton.Open,
                StandardButtonType = DialogStandardButton.Open
            };
            var disposables = new CompositeDisposable(2);
            disposables.Add(BindUtils.RelayBind(this, OpenButtonTextProperty, button, Button.ContentProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, button, Button.IsMotionEnabledProperty));
            _bindingDisposables.Add(button, disposables);
            _standardButtons.Add(button);
            AddButtonToGroup(DialogButtonRole.AcceptRole, button, true);
        }
        
        if (StandardButtons.HasFlag(DialogStandardButton.Save))
        {
            var button = new DialogButton
            {
                Role        = DialogButtonRole.AcceptRole,
                ButtonType = DefaultStandardButton == DialogStandardButton.Save ? ButtonType.Primary : ButtonType.Default,
                IsDefaultConfirmButton = DefaultStandardButton == DialogStandardButton.Save,
                IsDefaultEscapeButton = EscapeStandardButton == DialogStandardButton.Save,
                StandardButtonType = DialogStandardButton.Save
            };
            var disposables = new CompositeDisposable(2);
            disposables.Add(BindUtils.RelayBind(this, SaveButtonTextProperty, button, Button.ContentProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, button, Button.IsMotionEnabledProperty));
            _bindingDisposables.Add(button, disposables);
            _standardButtons.Add(button);
            AddButtonToGroup(DialogButtonRole.AcceptRole, button, true);
        }
        
        if (StandardButtons.HasFlag(DialogStandardButton.SaveAll))
        {
            var button = new DialogButton
            {
                Role = DialogButtonRole.AcceptRole,
                ButtonType = DefaultStandardButton == DialogStandardButton.SaveAll ? ButtonType.Primary : ButtonType.Default,
                IsDefaultConfirmButton = DefaultStandardButton == DialogStandardButton.SaveAll,
                IsDefaultEscapeButton = EscapeStandardButton == DialogStandardButton.SaveAll,
                StandardButtonType = DialogStandardButton.SaveAll
            };
            var disposables = new CompositeDisposable(2);
            disposables.Add(BindUtils.RelayBind(this, SaveAllButtonTextProperty, button, Button.ContentProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, button, Button.IsMotionEnabledProperty));
            _bindingDisposables.Add(button, disposables);
            _standardButtons.Add(button);
            AddButtonToGroup(DialogButtonRole.AcceptRole, button, true);
        }
        
        if (StandardButtons.HasFlag(DialogStandardButton.Retry))
        {
            var button = new DialogButton
            {
                Role = DialogButtonRole.AcceptRole,
                ButtonType = DefaultStandardButton == DialogStandardButton.Retry ? ButtonType.Primary : ButtonType.Default,
                IsDefaultConfirmButton = DefaultStandardButton == DialogStandardButton.Retry,
                IsDefaultEscapeButton = EscapeStandardButton == DialogStandardButton.Retry,
                StandardButtonType = DialogStandardButton.Retry
            };
            var disposables = new CompositeDisposable(2);
            disposables.Add(BindUtils.RelayBind(this, RetryButtonTextProperty, button, Button.ContentProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, button, Button.IsMotionEnabledProperty));
            _bindingDisposables.Add(button, disposables);
            _standardButtons.Add(button);
            AddButtonToGroup(DialogButtonRole.AcceptRole, button, true);
        }
        
        if (StandardButtons.HasFlag(DialogStandardButton.Ignore))
        {
            var button = new DialogButton
            {
                Role = DialogButtonRole.AcceptRole,
                ButtonType = DefaultStandardButton == DialogStandardButton.Ignore ? ButtonType.Primary : ButtonType.Default,
                IsDefaultConfirmButton = DefaultStandardButton == DialogStandardButton.Ignore,
                IsDefaultEscapeButton = EscapeStandardButton == DialogStandardButton.Ignore,
                StandardButtonType = DialogStandardButton.Ignore
            };
            var disposables = new CompositeDisposable(2);
            disposables.Add(BindUtils.RelayBind(this, IgnoreButtonTextProperty, button, Button.ContentProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, button, Button.IsMotionEnabledProperty));
            _bindingDisposables.Add(button, disposables);
            _standardButtons.Add(button);
            AddButtonToGroup(DialogButtonRole.AcceptRole, button, true);
        }
        
        if (StandardButtons.HasFlag(DialogStandardButton.Yes))
        {
            var button = new DialogButton
            {
                Role = DialogButtonRole.YesRole,
                ButtonType = DefaultStandardButton == DialogStandardButton.Yes ? ButtonType.Primary : ButtonType.Default,
                IsDefaultConfirmButton = DefaultStandardButton == DialogStandardButton.Yes,
                IsDefaultEscapeButton = EscapeStandardButton == DialogStandardButton.Yes,
                StandardButtonType = DialogStandardButton.Yes
            };
            var disposables = new CompositeDisposable(2);
            disposables.Add(BindUtils.RelayBind(this, YesButtonTextProperty, button, Button.ContentProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, button, Button.IsMotionEnabledProperty));
            _bindingDisposables.Add(button, disposables);
            _standardButtons.Add(button);
            AddButtonToGroup(DialogButtonRole.YesRole, button, true);
        }
        
        if (StandardButtons.HasFlag(DialogStandardButton.YesToAll))
        {
            var button = new DialogButton
            {
                Role = DialogButtonRole.YesRole,
                ButtonType = DefaultStandardButton == DialogStandardButton.YesToAll ? ButtonType.Primary : ButtonType.Default,
                IsDefaultConfirmButton = DefaultStandardButton == DialogStandardButton.YesToAll,
                IsDefaultEscapeButton = EscapeStandardButton == DialogStandardButton.YesToAll,
                StandardButtonType = DialogStandardButton.YesToAll
            };
            var disposables = new CompositeDisposable(2);
            disposables.Add(BindUtils.RelayBind(this, YesToAllButtonTextProperty, button, Button.ContentProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, button, Button.IsMotionEnabledProperty));
            _bindingDisposables.Add(button, disposables);
            _standardButtons.Add(button);
            AddButtonToGroup(DialogButtonRole.YesRole, button, true);
        }
        
        if (StandardButtons.HasFlag(DialogStandardButton.Cancel))
        {
            var button = new DialogButton
            {
                Role = DialogButtonRole.RejectRole,
                ButtonType = DefaultStandardButton == DialogStandardButton.Cancel ? ButtonType.Primary : ButtonType.Default,
                IsDefaultConfirmButton = DefaultStandardButton == DialogStandardButton.Cancel,
                IsDefaultEscapeButton = EscapeStandardButton == DialogStandardButton.Cancel,
                StandardButtonType = DialogStandardButton.Cancel
            };
            var disposables = new CompositeDisposable(2);
            disposables.Add(BindUtils.RelayBind(this, CancelButtonTextProperty, button, Button.ContentProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, button, Button.IsMotionEnabledProperty));
            _bindingDisposables.Add(button, disposables);
            _standardButtons.Add(button);
            AddButtonToGroup(DialogButtonRole.RejectRole, button, true);
        }
        
        if (StandardButtons.HasFlag(DialogStandardButton.Close))
        {
            var button = new DialogButton
            {
                Role = DialogButtonRole.RejectRole,
                ButtonType = DefaultStandardButton == DialogStandardButton.Close ? ButtonType.Primary : ButtonType.Default,
                IsDefaultConfirmButton = DefaultStandardButton == DialogStandardButton.Close,
                IsDefaultEscapeButton = EscapeStandardButton == DialogStandardButton.Close,
                StandardButtonType = DialogStandardButton.Close
            };
            var disposables = new CompositeDisposable(2);
            disposables.Add(BindUtils.RelayBind(this, CloseButtonTextProperty, button, Button.ContentProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, button, Button.IsMotionEnabledProperty));
            _bindingDisposables.Add(button, disposables);
            _standardButtons.Add(button);
            AddButtonToGroup(DialogButtonRole.RejectRole, button, true);
        }
        
        if (StandardButtons.HasFlag(DialogStandardButton.Abort))
        {
            var button = new DialogButton
            {
                Role = DialogButtonRole.RejectRole,
                ButtonType = DefaultStandardButton == DialogStandardButton.Abort ? ButtonType.Primary : ButtonType.Default,
                IsDefaultConfirmButton = DefaultStandardButton == DialogStandardButton.Abort,
                IsDefaultEscapeButton = EscapeStandardButton == DialogStandardButton.Abort,
                StandardButtonType = DialogStandardButton.Abort
            };
            var disposables = new CompositeDisposable(2);
            disposables.Add(BindUtils.RelayBind(this, AbortButtonTextProperty, button, Button.ContentProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, button, Button.IsMotionEnabledProperty));
            _bindingDisposables.Add(button, disposables);
            _standardButtons.Add(button);
            AddButtonToGroup(DialogButtonRole.RejectRole, button, true);
        }
        
        if (StandardButtons.HasFlag(DialogStandardButton.No))
        {
            var button = new DialogButton
            {
                Role = DialogButtonRole.NoRole,
                ButtonType = DefaultStandardButton == DialogStandardButton.No ? ButtonType.Primary : ButtonType.Default,
                IsDefaultConfirmButton = DefaultStandardButton == DialogStandardButton.No,
                IsDefaultEscapeButton = EscapeStandardButton == DialogStandardButton.No,
                StandardButtonType = DialogStandardButton.No
            };
            var disposables = new CompositeDisposable(2);
            disposables.Add(BindUtils.RelayBind(this, NoButtonTextProperty, button, Button.ContentProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, button, Button.IsMotionEnabledProperty));
            _bindingDisposables.Add(button, disposables);
            _standardButtons.Add(button);
            AddButtonToGroup(DialogButtonRole.NoRole, button, true);
        }
        
        if (StandardButtons.HasFlag(DialogStandardButton.NoToAll))
        {
            var button = new DialogButton
            {
                Role = DialogButtonRole.NoRole,
                ButtonType = DefaultStandardButton == DialogStandardButton.NoToAll ? ButtonType.Primary : ButtonType.Default,
                IsDefaultConfirmButton = DefaultStandardButton == DialogStandardButton.NoToAll,
                IsDefaultEscapeButton = EscapeStandardButton == DialogStandardButton.NoToAll,
                StandardButtonType = DialogStandardButton.NoToAll
            };
            var disposables = new CompositeDisposable(2);
            disposables.Add(BindUtils.RelayBind(this, NoToAllButtonTextProperty, button, Button.ContentProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, button, Button.IsMotionEnabledProperty));
            _bindingDisposables.Add(button, disposables);
            _standardButtons.Add(button);
            AddButtonToGroup(DialogButtonRole.NoRole, button, true);
        }
        
        if (StandardButtons.HasFlag(DialogStandardButton.Discard))
        {
            var button = new DialogButton
            {
                Role = DialogButtonRole.DestructiveRole,
                ButtonType = DefaultStandardButton == DialogStandardButton.Discard ? ButtonType.Primary : ButtonType.Default,
                IsDefaultConfirmButton = DefaultStandardButton == DialogStandardButton.Discard,
                IsDefaultEscapeButton = EscapeStandardButton == DialogStandardButton.Discard,
                StandardButtonType = DialogStandardButton.Discard
            };
            var disposables = new CompositeDisposable(2);
            disposables.Add(BindUtils.RelayBind(this, DiscardButtonTextProperty, button, Button.ContentProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, button, Button.IsMotionEnabledProperty));
            _bindingDisposables.Add(button, disposables);
            _standardButtons.Add(button);
            AddButtonToGroup(DialogButtonRole.DestructiveRole, button, true);
        }
        
        if (StandardButtons.HasFlag(DialogStandardButton.Help))
        {
            var button = new DialogButton
            {
                Role = DialogButtonRole.HelpRole,
                ButtonType = DefaultStandardButton == DialogStandardButton.Help ? ButtonType.Primary : ButtonType.Default,
                IsDefaultConfirmButton = DefaultStandardButton == DialogStandardButton.Help,
                IsDefaultEscapeButton = EscapeStandardButton == DialogStandardButton.Help,
                StandardButtonType = DialogStandardButton.Help
            };
            var disposables = new CompositeDisposable(2);
            disposables.Add(BindUtils.RelayBind(this, HelpButtonTextProperty, button, Button.ContentProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, button, Button.IsMotionEnabledProperty));
            _bindingDisposables.Add(button, disposables);
            _standardButtons.Add(button);
            AddButtonToGroup(DialogButtonRole.HelpRole, button, true);
        }
        
        if (StandardButtons.HasFlag(DialogStandardButton.Reset))
        {
            var button = new DialogButton
            {
                Role = DialogButtonRole.ResetRole,
                ButtonType = DefaultStandardButton == DialogStandardButton.Reset ? ButtonType.Primary : ButtonType.Default,
                IsDefaultConfirmButton = DefaultStandardButton == DialogStandardButton.Reset,
                IsDefaultEscapeButton = EscapeStandardButton == DialogStandardButton.Reset,
                StandardButtonType = DialogStandardButton.Reset
            };
            var disposables = new CompositeDisposable(2);
            disposables.Add(BindUtils.RelayBind(this, ResetButtonTextProperty, button, Button.ContentProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, button, Button.IsMotionEnabledProperty));
            _bindingDisposables.Add(button, disposables);
            _standardButtons.Add(button);
            AddButtonToGroup(DialogButtonRole.ResetRole, button, true);
        }
        
        if (StandardButtons.HasFlag(DialogStandardButton.Reload))
        {
            var button = new DialogButton
            {
                Role = DialogButtonRole.ActionRole,
                ButtonType = DefaultStandardButton == DialogStandardButton.Reload ? ButtonType.Primary : ButtonType.Default,
                IsDefaultConfirmButton = DefaultStandardButton == DialogStandardButton.Reload,
                IsDefaultEscapeButton = EscapeStandardButton == DialogStandardButton.Reload,
                StandardButtonType = DialogStandardButton.Reload
            };
            var disposables = new CompositeDisposable(2);
            disposables.Add(BindUtils.RelayBind(this, ResetButtonTextProperty, button, Button.ContentProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, button, Button.IsMotionEnabledProperty));
            _bindingDisposables.Add(button, disposables);
            _standardButtons.Add(button);
            AddButtonToGroup(DialogButtonRole.ActionRole, button, true);
        }
        
        if (StandardButtons.HasFlag(DialogStandardButton.RestoreDefaults))
        {
            var button = new DialogButton
            {
                Role = DialogButtonRole.ResetRole,
                ButtonType = DefaultStandardButton == DialogStandardButton.RestoreDefaults ? ButtonType.Primary : ButtonType.Default,
                IsDefaultConfirmButton = DefaultStandardButton == DialogStandardButton.RestoreDefaults,
                IsDefaultEscapeButton = EscapeStandardButton == DialogStandardButton.RestoreDefaults,
                StandardButtonType = DialogStandardButton.RestoreDefaults
            };
            var disposables = new CompositeDisposable(2);
            disposables.Add(BindUtils.RelayBind(this, RestoreDefaultsButtonTextProperty, button, Button.ContentProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, button, Button.IsMotionEnabledProperty));
            _bindingDisposables.Add(button, disposables);
            _standardButtons.Add(button);
            AddButtonToGroup(DialogButtonRole.ResetRole, button, true);
        }
        
        if (StandardButtons.HasFlag(DialogStandardButton.Apply))
        {
            var button = new DialogButton
            {
                Role = DialogButtonRole.ApplyRole,
                ButtonType = DefaultStandardButton == DialogStandardButton.Apply ? ButtonType.Primary : ButtonType.Default,
                IsDefaultConfirmButton = DefaultStandardButton == DialogStandardButton.Apply,
                IsDefaultEscapeButton = EscapeStandardButton == DialogStandardButton.Apply,
                StandardButtonType = DialogStandardButton.Apply
            };
            var disposables = new CompositeDisposable(2);
            disposables.Add(BindUtils.RelayBind(this, ApplyButtonTextProperty, button, Button.ContentProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, button, Button.IsMotionEnabledProperty));
            _bindingDisposables.Add(button, disposables);
            _standardButtons.Add(button);
            AddButtonToGroup(DialogButtonRole.ApplyRole, button, true);
        }
    }

    private void SyncButtonsToGroup()
    {
        if (_leftGroup == null ||
            _rightGroup == null ||
            _centerGroup == null)
        {
            return;
        }
        var buttons            = new List<DialogButton>();
        var rightButtons       = SyncRightGroupButtons();
        var centerGroupButtons = SyncCenterGroupButtons();
        var leftGroupButtons   = SyncLeftGroupButtons();
        buttons.AddRange(rightButtons);
        buttons.AddRange(centerGroupButtons);
        buttons.AddRange(leftGroupButtons);
        ButtonsSynchronized?.Invoke(this, new DialogBoxButtonSyncEventArgs(buttons));
    }
    
    private List<DialogButton> SyncRightGroupButtons()
    {
        Debug.Assert(_rightGroup != null);
        var rightGroupButtons = new List<DialogButton>();
        
        // 后面可以定义各种操作系统的风格，现在先写死
        // 标准
        var standardAcceptList  = _standardButtonGroup.GetValueOrDefault(DialogButtonRole.AcceptRole) ?? new List<DialogButton>();
        var standardAcceptIndex = 0;
        
        if (standardAcceptList.Count > 0)
        {
            var acceptButton = standardAcceptList[standardAcceptIndex++];
            rightGroupButtons.Add(acceptButton);
        }
        
        // 自定义
        var acceptList  = _buttonGroup.GetValueOrDefault(DialogButtonRole.AcceptRole) ?? new List<DialogButton>();
        var acceptIndex = 0;
        
        if (acceptList.Count > 0)
        {
            var acceptButton = acceptList[acceptIndex++];
            rightGroupButtons.Add(acceptButton);
        }
        
        // 标准
        var standardYesList  = _standardButtonGroup.GetValueOrDefault(DialogButtonRole.YesRole) ?? new List<DialogButton>();
        var standardYesIndex = 0;
        
        if (standardYesList.Count > 0)
        {
            var yesButton = standardYesList[standardYesIndex++];
            rightGroupButtons.Add(yesButton);
        }
        // 自定义
        
        var yesList  = _buttonGroup.GetValueOrDefault(DialogButtonRole.YesRole) ?? new List<DialogButton>();
        var yesIndex = 0;
        
        if (yesList.Count > 0)
        {
            var yesButton = yesList[yesIndex++];
            rightGroupButtons.Add(yesButton);
        }
        
        // 输出 reject
        // 标准
        var standardRejectList  = _standardButtonGroup.GetValueOrDefault(DialogButtonRole.RejectRole) ?? new List<DialogButton>();
        foreach (var rejectButton in standardRejectList)
        {
            rightGroupButtons.Add(rejectButton);
        }
        
        // 自定义
        var rejectList = _buttonGroup.GetValueOrDefault(DialogButtonRole.RejectRole) ?? new List<DialogButton>();
        foreach (var rejectButton in rejectList)
        {
            rightGroupButtons.Add(rejectButton);
        }
        
        // 输出 No
        var standardNoList  = _standardButtonGroup.GetValueOrDefault(DialogButtonRole.NoRole) ?? new List<DialogButton>();
        foreach (var button in standardNoList)
        {
            rightGroupButtons.Add(button);
        }
        
        // 自定义
        var noList = _buttonGroup.GetValueOrDefault(DialogButtonRole.NoRole) ?? new List<DialogButton>();
        foreach (var button in noList)
        {
            rightGroupButtons.Add(button);
        }
        
        // 输出剩下的 Accept
        // 标准
        for (var i = standardAcceptIndex; i < standardAcceptList.Count; i++)
        {
            foreach (var acceptButton in standardAcceptList)
            {
                rightGroupButtons.Add(acceptButton);
            }
        }
        
        // 自定义

        for (var i = acceptIndex; i < acceptList.Count; i++)
        {
            foreach (var acceptButton in acceptList)
            {
                rightGroupButtons.Add(acceptButton);
            }
        }
        
        // 输出剩下的 Yes
        // 标准
        for (var i = standardYesIndex; i < standardYesList.Count; i++)
        {
            foreach (var button in standardYesList)
            {
                rightGroupButtons.Add(button);
            }
        }
        
        // 自定义

        for (var i = yesIndex; i < yesList.Count; i++)
        {
            foreach (var button in yesList)
            {
                rightGroupButtons.Add(button);
            }
        }

        foreach (var button in rightGroupButtons)
        {
            DockPanel.SetDock(button, Dock.Right);
            button.SetLogicalParent(null);
            button.SetVisualParent(null);
        }
        
        ClearButtonPanel(_rightGroup);
        _rightGroup.Children.AddRange(rightGroupButtons);
        _rightGroup.Children.Add(new Control());
        return rightGroupButtons;
    }

    private List<DialogButton> SyncCenterGroupButtons()
    {
        Debug.Assert(_centerGroup != null);
        var centerGroupButtons = new List<DialogButton>();
        // 标准
        var standardDestructiveList  = _standardButtonGroup.GetValueOrDefault(DialogButtonRole.DestructiveRole) ?? new List<DialogButton>();
        
        foreach (var button in standardDestructiveList)
        {
            centerGroupButtons.Add(button);
        }
        
        // 自定义
        var destructiveList = _buttonGroup.GetValueOrDefault(DialogButtonRole.DestructiveRole) ?? new List<DialogButton>();
        foreach (var button in destructiveList)
        {
            centerGroupButtons.Add(button);
        }
        foreach (var button in centerGroupButtons)
        {
            DockPanel.SetDock(button, Dock.Left);
            button.SetLogicalParent(null);
            button.SetVisualParent(null);
        }
        ClearButtonPanel(_centerGroup);
        _centerGroup.Children.AddRange(centerGroupButtons);
        _centerGroup.Children.Add(new Control());
        return centerGroupButtons;
    }

    private List<DialogButton> SyncLeftGroupButtons()
    {
        Debug.Assert(_leftGroup != null);
        var leftGroupButtons = new List<DialogButton>();
        // 标准
        var standardHelpList = _standardButtonGroup.GetValueOrDefault(DialogButtonRole.HelpRole) ?? new List<DialogButton>();
        foreach (var button in standardHelpList)
        {
            leftGroupButtons.Add(button);
        }
        
        // 自定义
        var helpList = _buttonGroup.GetValueOrDefault(DialogButtonRole.HelpRole) ?? new List<DialogButton>();
        foreach (var button in helpList)
        {
            leftGroupButtons.Add(button);
        }
        
        // 标准
        var standardResetList = _standardButtonGroup.GetValueOrDefault(DialogButtonRole.ResetRole) ?? new List<DialogButton>();
        foreach (var button in standardResetList)
        {
            leftGroupButtons.Add(button);
        }
        
        // 自定义
        var resetList = _buttonGroup.GetValueOrDefault(DialogButtonRole.ResetRole) ?? new List<DialogButton>();
        foreach (var button in resetList)
        {
            leftGroupButtons.Add(button);
        }
        
        // 标准
        var standardApplyList = _standardButtonGroup.GetValueOrDefault(DialogButtonRole.ApplyRole) ?? new List<DialogButton>();
        foreach (var button in standardApplyList)
        {
            leftGroupButtons.Add(button);
        }
        
        // 自定义
        var applyList = _buttonGroup.GetValueOrDefault(DialogButtonRole.ApplyRole) ?? new List<DialogButton>();
        foreach (var button in applyList)
        {
            leftGroupButtons.Add(button);
        }
        
        // 标准
        var standardActionList = _standardButtonGroup.GetValueOrDefault(DialogButtonRole.ActionRole) ?? new List<DialogButton>();
        foreach (var button in standardActionList)
        {
            leftGroupButtons.Add(button);
        }
        
        // 自定义
        var actionList = _buttonGroup.GetValueOrDefault(DialogButtonRole.ActionRole) ?? new List<DialogButton>();
        foreach (var button in actionList)
        {
            leftGroupButtons.Add(button);
        }
        
        foreach (var button in leftGroupButtons)
        {
            DockPanel.SetDock(button, Dock.Left);
            button.SetLogicalParent(null);
            button.SetVisualParent(null);
        }

        ClearButtonPanel(_leftGroup);
        _leftGroup.Children.AddRange(leftGroupButtons);
        _leftGroup.Children.Add(new Control());
        return leftGroupButtons;
    }

    private void ClearButtonPanel(Panel panel)
    {
        var tobeRemoved = new List<Control>();
        tobeRemoved.AddRange(panel.Children);
        foreach (var item in tobeRemoved)
        {
            panel.Children.Remove(item);
        }
    }

    private void HandleButtonClicked(object? sender, EventArgs args)
    {
        if (sender is DialogButton button)
        {
            Clicked?.Invoke(this, new DialogButtonClickedEventArgs(button));
            if (button.Role == DialogButtonRole.AcceptRole)
            {
                Accepted?.Invoke(this, EventArgs.Empty);
            }
            else if (button.Role == DialogButtonRole.RejectRole)
            {
                Rejected?.Invoke(this, EventArgs.Empty);
            }
            else if (button.Role == DialogButtonRole.HelpRole)
            {
                HelpRequested?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}