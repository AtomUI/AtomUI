using System.Collections.Specialized;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Lays out the action buttons of a dialog: the standard buttons selected by <see cref="StandardButtons" />
/// and the buttons of <see cref="CustomButtons" />.
/// </summary>
/// <remarks>
/// Buttons are placed by <see cref="DialogButton.Role" />. Accept, yes, reject and no buttons go to the right
/// group, destructive buttons to the center group, and help, reset, apply and action buttons go to the left
/// group. Custom buttons with <see cref="DialogButtonRole.CustomRole" /> go to the right group, after the
/// others.
/// </remarks>
public class DialogButtonBox : TemplatedControl, IMotionAwareControl
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="StandardButtons" /> property.
    /// </summary>
    public static readonly StyledProperty<DialogStandardButtons> StandardButtonsProperty =
        AvaloniaProperty.Register<DialogButtonBox, DialogStandardButtons>(nameof(StandardButtons), DialogStandardButton.NoButton);

    /// <summary>
    /// Identifies the <see cref="DefaultStandardButton" /> property.
    /// </summary>
    public static readonly StyledProperty<DialogStandardButton> DefaultStandardButtonProperty =
        AvaloniaProperty.Register<DialogButtonBox, DialogStandardButton>(nameof(DefaultStandardButton));

    /// <summary>
    /// Identifies the <see cref="EscapeStandardButton" /> property.
    /// </summary>
    public static readonly StyledProperty<DialogStandardButton> EscapeStandardButtonProperty =
        AvaloniaProperty.Register<DialogButtonBox, DialogStandardButton>(nameof(EscapeStandardButton));

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<DialogButtonBox>();

    /// <summary>
    /// Selects the standard buttons the box creates, in addition to <see cref="CustomButtons" />. The default is
    /// an empty set, which creates none.
    /// </summary>
    /// <remarks>
    /// Each standard button takes its text from the text property of its kind, such as
    /// <see cref="OkButtonText" /> for <see cref="DialogStandardButton.Ok" />.
    /// </remarks>
    public DialogStandardButtons StandardButtons
    {
        get => GetValue(StandardButtonsProperty);
        set => SetValue(StandardButtonsProperty, value);
    }

    /// <summary>
    /// Names the standard button that is shown as the primary button and that the Enter key triggers in a dialog.
    /// The default is <see cref="DialogStandardButton.NoButton" />, which selects none. A button is shown as
    /// primary only while <see cref="StandardButtons" /> contains it.
    /// </summary>
    public DialogStandardButton DefaultStandardButton
    {
        get => GetValue(DefaultStandardButtonProperty);
        set => SetValue(DefaultStandardButtonProperty, value);
    }

    /// <summary>
    /// Names the standard button that the Escape key triggers in a dialog. The default is
    /// <see cref="DialogStandardButton.NoButton" />: while the property is not set, no button is the escape
    /// button and a dialog falls back to its Cancel button when <see cref="StandardButtons" /> contains it.
    /// </summary>
    public DialogStandardButton EscapeStandardButton
    {
        get => GetValue(EscapeStandardButtonProperty);
        set => SetValue(EscapeStandardButtonProperty, value);
    }

    /// <summary>
    /// Whether the standard buttons the box creates animate their state changes. The value is relayed to each of
    /// them; the buttons in <see cref="CustomButtons" /> keep their own. The theme takes it from the
    /// <c>EnableMotion</c> token.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// The buttons the caller supplies, shown in addition to the standard buttons. Each goes to the group of its
    /// <see cref="DialogButton.Role" /> and raises the same click events; a button listed more than once is
    /// shown once. The layout follows each change of the collection.
    /// </summary>
    /// <remarks>
    /// A dialog surface fills the collection with the buttons of <see cref="Dialog.CustomButtons" />, clearing it
    /// first.
    /// </remarks>
    public AvaloniaList<DialogButton> CustomButtons { get; } = new();

    #endregion

    #region 公共事件定义

    /// <summary>
    /// Raised when a button with the <see cref="DialogButtonRole.AcceptRole" /> role is clicked, after
    /// <see cref="Clicked" />. A button with the <see cref="DialogButtonRole.YesRole" /> role does not raise it.
    /// </summary>
    public event EventHandler? Accepted;
    /// <summary>
    /// Raised when any button in the box is clicked, before the event of the button's role. The argument carries
    /// the clicked button as <see cref="DialogButtonClickedEventArgs.SourceButton" />. Setting
    /// <see cref="DialogButtonClickedEventArgs.Handled" /> does not keep the role event from being raised.
    /// </summary>
    public event EventHandler<DialogButtonClickedEventArgs>? Clicked;
    /// <summary>
    /// Raised when a button with the <see cref="DialogButtonRole.HelpRole" /> role is clicked, after
    /// <see cref="Clicked" />.
    /// </summary>
    public event EventHandler? HelpRequested;
    /// <summary>
    /// Raised when a button with the <see cref="DialogButtonRole.RejectRole" /> role is clicked, after
    /// <see cref="Clicked" />. A button with the <see cref="DialogButtonRole.NoRole" /> role does not raise it.
    /// </summary>
    public event EventHandler? Rejected;

    #endregion

    #region 按钮语言属性定义

    /// <summary>
    /// Identifies the <see cref="OkButtonText" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> OkButtonTextProperty =
        AvaloniaProperty.Register<DialogButtonBox, string?>(nameof(OkButtonText));

    /// <summary>
    /// Identifies the <see cref="OpenButtonText" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> OpenButtonTextProperty =
        AvaloniaProperty.Register<DialogButtonBox, string?>(nameof(OpenButtonText));

    /// <summary>
    /// Identifies the <see cref="SaveButtonText" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> SaveButtonTextProperty =
        AvaloniaProperty.Register<DialogButtonBox, string?>(nameof(SaveButtonText));

    /// <summary>
    /// Identifies the <see cref="CancelButtonText" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> CancelButtonTextProperty =
        AvaloniaProperty.Register<DialogButtonBox, string?>(nameof(CancelButtonText));

    /// <summary>
    /// Identifies the <see cref="CloseButtonText" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> CloseButtonTextProperty =
        AvaloniaProperty.Register<DialogButtonBox, string?>(nameof(CloseButtonText));

    /// <summary>
    /// Identifies the <see cref="DiscardButtonText" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> DiscardButtonTextProperty =
        AvaloniaProperty.Register<DialogButtonBox, string?>(nameof(DiscardButtonText));

    /// <summary>
    /// Identifies the <see cref="ApplyButtonText" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> ApplyButtonTextProperty =
        AvaloniaProperty.Register<DialogButtonBox, string?>(nameof(ApplyButtonText));

    /// <summary>
    /// Identifies the <see cref="ResetButtonText" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> ResetButtonTextProperty =
        AvaloniaProperty.Register<DialogButtonBox, string?>(nameof(ResetButtonText));

    /// <summary>
    /// Identifies the <see cref="ReloadButtonText" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> ReloadButtonTextProperty =
        AvaloniaProperty.Register<DialogButtonBox, string?>(nameof(ReloadButtonText));

    /// <summary>
    /// Identifies the <see cref="RestoreDefaultsButtonText" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> RestoreDefaultsButtonTextProperty =
        AvaloniaProperty.Register<DialogButtonBox, string?>(nameof(RestoreDefaultsButtonText));

    /// <summary>
    /// Identifies the <see cref="HelpButtonText" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> HelpButtonTextProperty =
        AvaloniaProperty.Register<DialogButtonBox, string?>(nameof(HelpButtonText));

    /// <summary>
    /// Identifies the <see cref="SaveAllButtonText" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> SaveAllButtonTextProperty =
        AvaloniaProperty.Register<DialogButtonBox, string?>(nameof(SaveAllButtonText));

    /// <summary>
    /// Identifies the <see cref="YesButtonText" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> YesButtonTextProperty =
        AvaloniaProperty.Register<DialogButtonBox, string?>(nameof(YesButtonText));

    /// <summary>
    /// Identifies the <see cref="YesToAllButtonText" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> YesToAllButtonTextProperty =
        AvaloniaProperty.Register<DialogButtonBox, string?>(nameof(YesToAllButtonText));

    /// <summary>
    /// Identifies the <see cref="NoButtonText" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> NoButtonTextProperty =
        AvaloniaProperty.Register<DialogButtonBox, string?>(nameof(NoButtonText));

    /// <summary>
    /// Identifies the <see cref="NoToAllButtonText" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> NoToAllButtonTextProperty =
        AvaloniaProperty.Register<DialogButtonBox, string?>(nameof(NoToAllButtonText));

    /// <summary>
    /// Identifies the <see cref="AbortButtonText" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> AbortButtonTextProperty =
        AvaloniaProperty.Register<DialogButtonBox, string?>(nameof(AbortButtonText));

    /// <summary>
    /// Identifies the <see cref="RetryButtonText" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> RetryButtonTextProperty =
        AvaloniaProperty.Register<DialogButtonBox, string?>(nameof(RetryButtonText));

    /// <summary>
    /// Identifies the <see cref="IgnoreButtonText" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> IgnoreButtonTextProperty =
        AvaloniaProperty.Register<DialogButtonBox, string?>(nameof(IgnoreButtonText));

    /// <summary>
    /// Text of the standard button for <see cref="DialogStandardButton.Ok" />. While it is not set on the box,
    /// the theme supplies the localized text; a value set on the box replaces it.
    /// </summary>
    public string? OkButtonText
    {
        get => GetValue(OkButtonTextProperty);
        set => SetValue(OkButtonTextProperty, value);
    }

    /// <summary>
    /// Text of the standard button for <see cref="DialogStandardButton.Open" />. While it is not set on the box,
    /// the theme supplies the localized text; a value set on the box replaces it.
    /// </summary>
    public string? OpenButtonText
    {
        get => GetValue(OpenButtonTextProperty);
        set => SetValue(OpenButtonTextProperty, value);
    }

    /// <summary>
    /// Text of the standard button for <see cref="DialogStandardButton.Save" />. While it is not set on the box,
    /// the theme supplies the localized text; a value set on the box replaces it.
    /// </summary>
    public string? SaveButtonText
    {
        get => GetValue(SaveButtonTextProperty);
        set => SetValue(SaveButtonTextProperty, value);
    }

    /// <summary>
    /// Text of the standard button for <see cref="DialogStandardButton.Cancel" />. While it is not set on the
    /// box, the theme supplies the localized text; a value set on the box replaces it.
    /// </summary>
    public string? CancelButtonText
    {
        get => GetValue(CancelButtonTextProperty);
        set => SetValue(CancelButtonTextProperty, value);
    }

    /// <summary>
    /// Text of the standard button for <see cref="DialogStandardButton.Close" />. While it is not set on the
    /// box, the theme supplies the localized text; a value set on the box replaces it.
    /// </summary>
    public string? CloseButtonText
    {
        get => GetValue(CloseButtonTextProperty);
        set => SetValue(CloseButtonTextProperty, value);
    }

    /// <summary>
    /// Text of the standard button for <see cref="DialogStandardButton.Discard" />. While it is not set on the
    /// box, the theme supplies the localized text; a value set on the box replaces it.
    /// </summary>
    public string? DiscardButtonText
    {
        get => GetValue(DiscardButtonTextProperty);
        set => SetValue(DiscardButtonTextProperty, value);
    }

    /// <summary>
    /// Text of the standard button for <see cref="DialogStandardButton.Apply" />. While it is not set on the
    /// box, the theme supplies the localized text; a value set on the box replaces it.
    /// </summary>
    public string? ApplyButtonText
    {
        get => GetValue(ApplyButtonTextProperty);
        set => SetValue(ApplyButtonTextProperty, value);
    }

    /// <summary>
    /// Text of the standard button for <see cref="DialogStandardButton.Reset" />. While it is not set on the
    /// box, the theme supplies the localized text; a value set on the box replaces it.
    /// </summary>
    public string? ResetButtonText
    {
        get => GetValue(ResetButtonTextProperty);
        set => SetValue(ResetButtonTextProperty, value);
    }

    /// <summary>
    /// Text of the standard button for <see cref="DialogStandardButton.Reload" />. While it is not set on the
    /// box, the theme supplies the localized text; a value set on the box replaces it.
    /// </summary>
    public string? ReloadButtonText
    {
        get => GetValue(ReloadButtonTextProperty);
        set => SetValue(ReloadButtonTextProperty, value);
    }

    /// <summary>
    /// Text of the standard button for <see cref="DialogStandardButton.RestoreDefaults" />. While it is not set
    /// on the box, the theme supplies the localized text; a value set on the box replaces it.
    /// </summary>
    public string? RestoreDefaultsButtonText
    {
        get => GetValue(RestoreDefaultsButtonTextProperty);
        set => SetValue(RestoreDefaultsButtonTextProperty, value);
    }

    /// <summary>
    /// Text of the standard button for <see cref="DialogStandardButton.Help" />. While it is not set on the box,
    /// the theme supplies the localized text; a value set on the box replaces it.
    /// </summary>
    public string? HelpButtonText
    {
        get => GetValue(HelpButtonTextProperty);
        set => SetValue(HelpButtonTextProperty, value);
    }

    /// <summary>
    /// Text of the standard button for <see cref="DialogStandardButton.SaveAll" />. While it is not set on the
    /// box, the theme supplies the localized text; a value set on the box replaces it.
    /// </summary>
    public string? SaveAllButtonText
    {
        get => GetValue(SaveAllButtonTextProperty);
        set => SetValue(SaveAllButtonTextProperty, value);
    }

    /// <summary>
    /// Text of the standard button for <see cref="DialogStandardButton.Yes" />. While it is not set on the box,
    /// the theme supplies the localized text; a value set on the box replaces it.
    /// </summary>
    public string? YesButtonText
    {
        get => GetValue(YesButtonTextProperty);
        set => SetValue(YesButtonTextProperty, value);
    }

    /// <summary>
    /// Text of the standard button for <see cref="DialogStandardButton.YesToAll" />. While it is not set on the
    /// box, the theme supplies the localized text; a value set on the box replaces it.
    /// </summary>
    public string? YesToAllButtonText
    {
        get => GetValue(YesToAllButtonTextProperty);
        set => SetValue(YesToAllButtonTextProperty, value);
    }

    /// <summary>
    /// Text of the standard button for <see cref="DialogStandardButton.No" />. While it is not set on the box,
    /// the theme supplies the localized text; a value set on the box replaces it.
    /// </summary>
    public string? NoButtonText
    {
        get => GetValue(NoButtonTextProperty);
        set => SetValue(NoButtonTextProperty, value);
    }

    /// <summary>
    /// Text of the standard button for <see cref="DialogStandardButton.NoToAll" />. While it is not set on the
    /// box, the theme supplies the localized text; a value set on the box replaces it.
    /// </summary>
    public string? NoToAllButtonText
    {
        get => GetValue(NoToAllButtonTextProperty);
        set => SetValue(NoToAllButtonTextProperty, value);
    }

    /// <summary>
    /// Text of the standard button for <see cref="DialogStandardButton.Abort" />. While it is not set on the
    /// box, the theme supplies the localized text; a value set on the box replaces it.
    /// </summary>
    public string? AbortButtonText
    {
        get => GetValue(AbortButtonTextProperty);
        set => SetValue(AbortButtonTextProperty, value);
    }

    /// <summary>
    /// Text of the standard button for <see cref="DialogStandardButton.Retry" />. While it is not set on the
    /// box, the theme supplies the localized text; a value set on the box replaces it.
    /// </summary>
    public string? RetryButtonText
    {
        get => GetValue(RetryButtonTextProperty);
        set => SetValue(RetryButtonTextProperty, value);
    }

    /// <summary>
    /// Text of the standard button for <see cref="DialogStandardButton.Ignore" />. While it is not set on the
    /// box, the theme supplies the localized text; a value set on the box replaces it.
    /// </summary>
    public string? IgnoreButtonText
    {
        get => GetValue(IgnoreButtonTextProperty);
        set => SetValue(IgnoreButtonTextProperty, value);
    }
    #endregion

    #region 内部协作 API

    internal event EventHandler? EffectiveButtonsChanged;

    internal IReadOnlyList<DialogButton> EffectiveButtons => _effectiveButtons;

    #endregion

    private static readonly (
        DialogStandardButton Type,
        DialogButtonRole Role,
        StyledProperty<string?> ContentProperty)[] StandardButtonDefinitions =
    {
        (DialogStandardButton.Ok, DialogButtonRole.AcceptRole, OkButtonTextProperty),
        (DialogStandardButton.Open, DialogButtonRole.AcceptRole, OpenButtonTextProperty),
        (DialogStandardButton.Save, DialogButtonRole.AcceptRole, SaveButtonTextProperty),
        (DialogStandardButton.SaveAll, DialogButtonRole.AcceptRole, SaveAllButtonTextProperty),
        (DialogStandardButton.Retry, DialogButtonRole.AcceptRole, RetryButtonTextProperty),
        (DialogStandardButton.Ignore, DialogButtonRole.AcceptRole, IgnoreButtonTextProperty),
        (DialogStandardButton.Yes, DialogButtonRole.YesRole, YesButtonTextProperty),
        (DialogStandardButton.YesToAll, DialogButtonRole.YesRole, YesToAllButtonTextProperty),
        (DialogStandardButton.Cancel, DialogButtonRole.RejectRole, CancelButtonTextProperty),
        (DialogStandardButton.Close, DialogButtonRole.RejectRole, CloseButtonTextProperty),
        (DialogStandardButton.Abort, DialogButtonRole.RejectRole, AbortButtonTextProperty),
        (DialogStandardButton.No, DialogButtonRole.NoRole, NoButtonTextProperty),
        (DialogStandardButton.NoToAll, DialogButtonRole.NoRole, NoToAllButtonTextProperty),
        (DialogStandardButton.Discard, DialogButtonRole.DestructiveRole, DiscardButtonTextProperty),
        (DialogStandardButton.Help, DialogButtonRole.HelpRole, HelpButtonTextProperty),
        (DialogStandardButton.Reset, DialogButtonRole.ResetRole, ResetButtonTextProperty),
        (DialogStandardButton.Reload, DialogButtonRole.ActionRole, ReloadButtonTextProperty),
        (DialogStandardButton.RestoreDefaults, DialogButtonRole.ResetRole, RestoreDefaultsButtonTextProperty),
        (DialogStandardButton.Apply, DialogButtonRole.ApplyRole, ApplyButtonTextProperty)
    };

    private DockPanel? _leftGroup;
    private DockPanel? _centerGroup;
    private DockPanel? _rightGroup;
    private readonly List<DialogButton> _standardButtons = new();
    private readonly List<IDisposable> _standardButtonBindings = new();
    private readonly HashSet<DialogButton> _subscribedCustomButtons = new();
    private readonly AvaloniaList<DialogButton> _effectiveButtons = new();

    static DialogButtonBox()
    {
        AffectsMeasure<DialogButtonBox>(StandardButtonsProperty);
    }

    public DialogButtonBox()
    {
        CustomButtons.CollectionChanged += new(HandleCustomButtonsChanged);
    }

    /// <summary>
    /// Called when a styled property of the box changes. A new template releases the panel parts, a change of
    /// <see cref="StandardButtons" /> rebuilds the standard buttons, and a change of
    /// <see cref="DefaultStandardButton" /> or <see cref="EscapeStandardButton" /> updates which of them is the
    /// primary and the escape button. Call the base member first.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TemplateProperty)
        {
            ReleaseTemplateParts();
        }
        else if (change.Property == StandardButtonsProperty)
        {
            BuildStandardButtons();
            SynchronizeButtons();
        }
        else if (change.Property == DefaultStandardButtonProperty ||
                 change.Property == EscapeStandardButtonProperty)
        {
            UpdateStandardButtonDefaults();
            NotifyButtonsSynchronized();
        }
    }

    /// <summary>
    /// Called when the control template is applied. It looks up the <c>PART_LeftGroup</c>,
    /// <c>PART_CenterGroup</c> and <c>PART_RightGroup</c> panels and places the buttons in them; without all
    /// three panels no button is placed. Call the base member first.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ReleaseTemplateParts();
        _leftGroup = e.NameScope.Find<DockPanel>("PART_LeftGroup");
        _centerGroup = e.NameScope.Find<DockPanel>("PART_CenterGroup");
        _rightGroup = e.NameScope.Find<DockPanel>("PART_RightGroup");
        BuildStandardButtons();
        SynchronizeButtons();
    }

    private void HandleCustomButtonsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SynchronizeCustomButtonSubscriptions();
        SynchronizeButtons();
    }

    private void BuildStandardButtons()
    {
        ReleaseStandardButtons();

        foreach (var definition in StandardButtonDefinitions)
        {
            if (!StandardButtons.HasFlag(definition.Type))
            {
                continue;
            }

            var isDefault = definition.Type == DefaultStandardButton;
            var button = new DialogButton
            {
                Role = definition.Role,
                ButtonType = isDefault ? ButtonType.Primary : ButtonType.Default,
                IsDefaultConfirmButton = isDefault,
                IsDefaultEscapeButton = definition.Type == EscapeStandardButton,
                StandardButtonType = definition.Type
            };
            _standardButtonBindings.Add(
                button.Bind(
                    Button.ContentProperty,
                    this.GetObservable(definition.ContentProperty),
                    BindingPriority.Template));
            _standardButtonBindings.Add(
                button.Bind(Button.IsMotionEnabledProperty, this.GetObservable(IsMotionEnabledProperty)));
            button.Click += HandleButtonClicked;
            _standardButtons.Add(button);
        }
    }

    private void UpdateStandardButtonDefaults()
    {
        foreach (var button in _standardButtons)
        {
            var standardButton = button.StandardButtonType;
            var isDefault = standardButton == DefaultStandardButton;
            button.ButtonType = isDefault ? ButtonType.Primary : ButtonType.Default;
            button.IsDefaultConfirmButton = isDefault;
            button.IsDefaultEscapeButton = standardButton == EscapeStandardButton;
        }
    }

    private void ReleaseStandardButtons()
    {
        var retainedValues = _standardButtons
            .Select(button => (button.Content, button.IsMotionEnabled))
            .ToArray();
        foreach (var button in _standardButtons)
        {
            button.Click -= HandleButtonClicked;
        }

        foreach (var binding in _standardButtonBindings)
        {
            binding.Dispose();
        }

        for (var index = 0; index < _standardButtons.Count; index++)
        {
            var button = _standardButtons[index];
            var values = retainedValues[index];
            button.SetCurrentValue(Button.ContentProperty, values.Content);
            button.SetCurrentValue(Button.IsMotionEnabledProperty, values.IsMotionEnabled);
        }

        _standardButtonBindings.Clear();
        _standardButtons.Clear();
    }

    internal void ReleaseButtons()
    {
        ReleaseTemplateParts();
        ReleaseStandardButtons();
        ReleaseCustomButtonSubscriptions();
        _effectiveButtons.Clear();
    }

    private void SynchronizeCustomButtonSubscriptions()
    {
        var currentButtons = new HashSet<DialogButton>(CustomButtons);
        foreach (var button in new List<DialogButton>(_subscribedCustomButtons))
        {
            if (currentButtons.Contains(button))
            {
                continue;
            }

            button.Click -= HandleButtonClicked;
            _subscribedCustomButtons.Remove(button);
        }

        foreach (var button in currentButtons)
        {
            if (_subscribedCustomButtons.Add(button))
            {
                button.Click += HandleButtonClicked;
            }
        }
    }

    private void ReleaseCustomButtonSubscriptions()
    {
        foreach (var button in _subscribedCustomButtons)
        {
            button.Click -= HandleButtonClicked;
        }

        _subscribedCustomButtons.Clear();
    }

    private void ReleaseTemplateParts()
    {
        ClearButtonPanel(_leftGroup);
        ClearButtonPanel(_centerGroup);
        ClearButtonPanel(_rightGroup);
        _leftGroup = null;
        _centerGroup = null;
        _rightGroup = null;
    }

    private void SynchronizeButtons()
    {
        if (_leftGroup is null || _centerGroup is null || _rightGroup is null)
        {
            return;
        }

        ClearButtonPanel(_leftGroup);
        ClearButtonPanel(_centerGroup);
        ClearButtonPanel(_rightGroup);

        var customButtons = GetDistinctCustomButtons();
        var rightButtons = BuildRightGroupButtons(customButtons);
        var centerButtons = BuildCenterGroupButtons(customButtons);
        var leftButtons = BuildLeftGroupButtons(customButtons);

        PopulateButtonPanel(_rightGroup, rightButtons, Dock.Right);
        PopulateButtonPanel(_centerGroup, centerButtons, Dock.Left);
        PopulateButtonPanel(_leftGroup, leftButtons, Dock.Left);

        _effectiveButtons.Clear();
        _effectiveButtons.AddRange(rightButtons);
        _effectiveButtons.AddRange(centerButtons);
        _effectiveButtons.AddRange(leftButtons);
        NotifyButtonsSynchronized();
    }

    private List<DialogButton> BuildRightGroupButtons(IReadOnlyList<DialogButton> customButtons)
    {
        var standardAccept = GetRoleButtons(_standardButtons, DialogButtonRole.AcceptRole);
        var customAccept = GetRoleButtons(customButtons, DialogButtonRole.AcceptRole);
        var standardYes = GetRoleButtons(_standardButtons, DialogButtonRole.YesRole);
        var customYes = GetRoleButtons(customButtons, DialogButtonRole.YesRole);
        var buttons = new List<DialogButton>();

        var standardAcceptIndex = AddFirst(buttons, standardAccept);
        var customAcceptIndex = AddFirst(buttons, customAccept);
        var standardYesIndex = AddFirst(buttons, standardYes);
        var customYesIndex = AddFirst(buttons, customYes);

        buttons.AddRange(GetRoleButtons(_standardButtons, DialogButtonRole.RejectRole));
        buttons.AddRange(GetRoleButtons(customButtons, DialogButtonRole.RejectRole));
        buttons.AddRange(GetRoleButtons(_standardButtons, DialogButtonRole.NoRole));
        buttons.AddRange(GetRoleButtons(customButtons, DialogButtonRole.NoRole));
        AddRemaining(buttons, standardAccept, standardAcceptIndex);
        AddRemaining(buttons, customAccept, customAcceptIndex);
        AddRemaining(buttons, standardYes, standardYesIndex);
        AddRemaining(buttons, customYes, customYesIndex);
        buttons.AddRange(GetRoleButtons(customButtons, DialogButtonRole.CustomRole));
        return buttons;
    }

    private List<DialogButton> BuildCenterGroupButtons(IReadOnlyList<DialogButton> customButtons)
    {
        var buttons = new List<DialogButton>();
        buttons.AddRange(GetRoleButtons(_standardButtons, DialogButtonRole.DestructiveRole));
        buttons.AddRange(GetRoleButtons(customButtons, DialogButtonRole.DestructiveRole));
        return buttons;
    }

    private List<DialogButton> BuildLeftGroupButtons(IReadOnlyList<DialogButton> customButtons)
    {
        var buttons = new List<DialogButton>();
        AppendRoleButtons(buttons, customButtons, DialogButtonRole.HelpRole);
        AppendRoleButtons(buttons, customButtons, DialogButtonRole.ResetRole);
        AppendRoleButtons(buttons, customButtons, DialogButtonRole.ApplyRole);
        AppendRoleButtons(buttons, customButtons, DialogButtonRole.ActionRole);
        return buttons;
    }

    private void AppendRoleButtons(
        List<DialogButton> target,
        IReadOnlyList<DialogButton> customButtons,
        DialogButtonRole role)
    {
        target.AddRange(GetRoleButtons(_standardButtons, role));
        target.AddRange(GetRoleButtons(customButtons, role));
    }

    private List<DialogButton> GetDistinctCustomButtons()
    {
        var result = new List<DialogButton>(CustomButtons.Count);
        var seen = new HashSet<DialogButton>();
        foreach (var button in CustomButtons)
        {
            if (seen.Add(button))
            {
                result.Add(button);
            }
        }

        return result;
    }

    private static List<DialogButton> GetRoleButtons(
        IReadOnlyList<DialogButton> buttons,
        DialogButtonRole role)
    {
        var result = new List<DialogButton>();
        foreach (var button in buttons)
        {
            if (button.Role == role)
            {
                result.Add(button);
            }
        }

        return result;
    }

    private static int AddFirst(List<DialogButton> target, IReadOnlyList<DialogButton> source)
    {
        if (source.Count == 0)
        {
            return 0;
        }

        target.Add(source[0]);
        return 1;
    }

    private static void AddRemaining(
        List<DialogButton> target,
        IReadOnlyList<DialogButton> source,
        int startIndex)
    {
        for (var index = startIndex; index < source.Count; index++)
        {
            target.Add(source[index]);
        }
    }

    private static void PopulateButtonPanel(
        Panel panel,
        IReadOnlyList<DialogButton> buttons,
        Dock dock)
    {
        foreach (var button in buttons)
        {
            DockPanel.SetDock(button, dock);
            panel.Children.Add(button);
        }

        panel.Children.Add(new Control());
    }

    private void NotifyButtonsSynchronized()
    {
        if (_leftGroup is null || _centerGroup is null || _rightGroup is null)
        {
            return;
        }

        EffectiveButtonsChanged?.Invoke(this, EventArgs.Empty);
    }

    private static void ClearButtonPanel(Panel? panel)
    {
        panel?.Children.Clear();
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
