using System.Diagnostics;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;
using AtomUI.Theme;

namespace AtomUI.Desktop.Controls;

// Token-owner transparent part: a FormItem belongs to one Form and has no Own Tokens; its theme reads the Form
// TokenResource, so Controls[Form] reaches the items, their transitions and the identity-less parts inside them.
/// <summary>
/// A row of a <see cref="Form" />. It shows a label, the value control given as <see cref="Content" />, an
/// optional tooltip icon, extra content, and the help text and validation messages of the item, and it runs the
/// validators of the item for the form.
/// </summary>
[TokenOwnerTransparent]
public partial class FormItem : AbstractFormItem, IFormItem
{
    /// <summary>
    /// Identifies the <see cref="ValidateChanged" /> routed event.
    /// </summary>
    public new static readonly RoutedEvent<FormItemValidateChangedEventArgs> ValidateChangedEvent = AbstractFormItem.ValidateChangedEvent;

    internal static readonly RoutedEvent<RoutedEventArgs> DeleteRequestEvent =
        RoutedEvent.Register<FormItem, RoutedEventArgs>(nameof(DeleteRequest), RoutingStrategies.Bubble);

    #region 公共属性定义
    
    /// <summary>
    /// Identifies the <see cref="Extra" /> property.
    /// </summary>
    public new static readonly StyledProperty<object?> ExtraProperty =
        AbstractFormItem.ExtraProperty.AddOwner<FormItem>();

    /// <summary>
    /// Identifies the <see cref="ExtraTemplate" /> property.
    /// </summary>
    public new static readonly StyledProperty<IDataTemplate?> ExtraTemplateProperty =
        AbstractFormItem.ExtraTemplateProperty.AddOwner<FormItem>();
    
    /// <summary>
    /// Identifies the <see cref="Help" /> property.
    /// </summary>
    public new static readonly StyledProperty<string?> HelpProperty =
        AbstractFormItem.HelpProperty.AddOwner<FormItem>();
    
    /// <summary>
    /// Identifies the <see cref="IsValidateFeedbackEnabled" /> property.
    /// </summary>
    public new static readonly StyledProperty<bool> IsValidateFeedbackEnabledProperty =
        AbstractFormItem.IsValidateFeedbackEnabledProperty.AddOwner<FormItem>();
    
    /// <summary>
    /// Identifies the <see cref="InitialValue" /> property.
    /// </summary>
    public new static readonly StyledProperty<object?> InitialValueProperty =
        AbstractFormItem.InitialValueProperty.AddOwner<FormItem>();
    
    /// <summary>
    /// Identifies the <see cref="LabelText" /> property.
    /// </summary>
    public new static readonly StyledProperty<string?> LabelTextProperty =
        AbstractFormItem.LabelTextProperty.AddOwner<FormItem>();
    
    /// <summary>
    /// Identifies the <see cref="LabelAlign" /> property.
    /// </summary>
    public static readonly StyledProperty<FormLabelAlign> LabelAlignProperty =
        Form.LabelAlignProperty.AddOwner<FormItem>();
    
    /// <summary>
    /// Identifies the <see cref="LabelWrapping" /> property.
    /// </summary>
    public static readonly StyledProperty<TextWrapping> LabelWrappingProperty =
        Form.LabelWrappingProperty.AddOwner<FormItem>();
    
    /// <summary>
    /// Identifies the <see cref="LabelColInfo" /> property.
    /// </summary>
    public static readonly StyledProperty<MediaBreakGridLength?> LabelColInfoProperty =
        Form.LabelColInfoProperty.AddOwner<FormItem>();
    
    /// <summary>
    /// Identifies the <see cref="WrapperColInfo" /> property.
    /// </summary>
    public static readonly StyledProperty<MediaBreakGridLength?> WrapperColInfoProperty =
        Form.WrapperColInfoProperty.AddOwner<FormItem>();
    
    /// <summary>
    /// Identifies the <see cref="FieldName" /> property.
    /// </summary>
    public new static readonly StyledProperty<string?> FieldNameProperty =
        AbstractFormItem.FieldNameProperty.AddOwner<FormItem>();
    
    /// <summary>
    /// Identifies the <see cref="IsRequired" /> property.
    /// </summary>
    public new static readonly StyledProperty<bool> IsRequiredProperty =
        AbstractFormItem.IsRequiredProperty.AddOwner<FormItem>();
    
    /// <summary>
    /// Identifies the <see cref="Validators" /> property.
    /// </summary>
    public new static readonly StyledProperty<IList<IFormValidator>?> ValidatorsProperty =
        AbstractFormItem.ValidatorsProperty.AddOwner<FormItem>();
    
    /// <summary>
    /// Identifies the <see cref="Tooltip" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> TooltipProperty =
        AvaloniaProperty.Register<FormItem, string?>(nameof(Tooltip));
    
    /// <summary>
    /// Identifies the <see cref="TooltipIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> TooltipIconProperty =
        AvaloniaProperty.Register<FormItem, PathIcon?>(nameof(TooltipIcon));
    
    /// <summary>
    /// Identifies the <see cref="ValidateTrigger" /> property.
    /// </summary>
    public new static readonly StyledProperty<FormValidateTrigger> ValidateTriggerProperty =
        AbstractFormItem.ValidateTriggerProperty.AddOwner<FormItem>();
    
    /// <summary>
    /// Identifies the <see cref="ValidateDebounce" /> property.
    /// </summary>
    public new static readonly StyledProperty<TimeSpan> ValidateDebounceProperty =
        AbstractFormItem.ValidateDebounceProperty.AddOwner<FormItem>();
    
    /// <summary>
    /// Identifies the <see cref="ValidateStrategy" /> property.
    /// </summary>
    public new static readonly StyledProperty<FormValidateStrategy> ValidateStrategyProperty =
        AbstractFormItem.ValidateStrategyProperty.AddOwner<FormItem>();
    
    /// <summary>
    /// Identifies the <see cref="ValidateStatus" /> property.
    /// </summary>
    public new static readonly DirectProperty<FormItem, FormValidateStatus> ValidateStatusProperty =
        AbstractFormItem.ValidateStatusProperty.AddOwner<FormItem>(owner => owner.ValidateStatus);
    
    /// <summary>
    /// Identifies the <see cref="ValidateResult" /> property.
    /// </summary>
    public new static readonly DirectProperty<FormItem, FormValidateResult> ValidateResultProperty =
        AbstractFormItem.ValidateResultProperty.AddOwner<FormItem>(owner => owner.ValidateResult);
    
    /// <summary>
    /// Identifies the <see cref="Layout" /> property.
    /// </summary>
    public new static readonly StyledProperty<FormItemLayout> LayoutProperty =
        AbstractFormItem.LayoutProperty.AddOwner<FormItem>();
    
    /// <summary>
    /// Identifies the <see cref="ChildrenSpacing" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ChildrenSpacingProperty =
        AvaloniaProperty.Register<FormItem, double>(nameof(ChildrenSpacing));
    
    /// <summary>
    /// Identifies the <see cref="Content" /> property.
    /// </summary>
    public new static readonly StyledProperty<Control?> ContentProperty =
        AbstractFormItem.ContentProperty.AddOwner<FormItem>();
    
    /// <summary>
    /// Identifies the <see cref="IsValidateContentType" /> property.
    /// </summary>
    public new static readonly StyledProperty<bool> IsValidateContentTypeProperty =
        AbstractFormItem.IsValidateContentTypeProperty.AddOwner<FormItem>();
    
    /// <summary>
    /// Extra content of the item, shown under the input. It is shown only while this value is not
    /// <see langword="null" />, the default.
    /// </summary>
    [DependsOn(nameof(ExtraTemplate))]
    public new object? Extra
    {
        get => GetValue(ExtraProperty);
        set => SetValue(ExtraProperty, value);
    }
    
    /// <summary>
    /// The template used to present <see cref="Extra" />. When it is <see langword="null" />, the default, the
    /// content is presented without a template.
    /// </summary>
    public new IDataTemplate? ExtraTemplate
    {
        get => GetValue(ExtraTemplateProperty);
        set => SetValue(ExtraTemplateProperty, value);
    }
    
    /// <summary>
    /// The help text of the item, shown in every validation state. When it is <see langword="null" />, the default,
    /// no help text is shown.
    /// </summary>
    public new string? Help
    {
        get => GetValue(HelpProperty);
        set => SetValue(HelpProperty, value);
    }
    
    /// <summary>
    /// Whether the item hands <see cref="Content" /> a feedback control that visualizes the validation state. It
    /// applies only to content that implements <see cref="IFormItemFeedbackAware" />, and the feedback control is
    /// built from the feedback template of the owner form. The default is <see langword="false" />. The owner form
    /// sets this value on the item, and a value set directly on the item overrides it.
    /// </summary>
    public new bool IsValidateFeedbackEnabled
    {
        get => GetValue(IsValidateFeedbackEnabledProperty);
        set => SetValue(IsValidateFeedbackEnabledProperty, value);
    }
    
    /// <summary>
    /// The value written to <see cref="Content" /> when a content control is set and when
    /// <see cref="ResetItemValue()" /> restores the item. While it is not set, the default, a new content
    /// control keeps its own value and resetting clears the value. An initial value that the owner form
    /// holds for <see cref="FieldName" /> takes precedence over this value.
    /// </summary>
    public new object? InitialValue
    {
        get => GetValue(InitialValueProperty);
        set => SetValue(InitialValueProperty, value);
    }
    
    /// <summary>
    /// The text of the label at the start of the item. No label is shown while the value is <see langword="null" />,
    /// the default. Default validation messages name the field by this text and fall back to <see cref="FieldName" />.
    /// In the horizontal layout a colon follows the text while <c>IsShowColon</c> is set.
    /// </summary>
    public new string? LabelText
    {
        get => GetValue(LabelTextProperty);
        set => SetValue(LabelTextProperty, value);
    }
    
    /// <summary>
    /// Alignment of the label in the horizontal layout; a vertical layout is styled like
    /// <see cref="FormLabelAlign.Left" />. <see cref="FormLabelAlign.Right" /> is the default. The owner form sets
    /// this value on the item, and a value set directly on the item overrides it.
    /// </summary>
    public FormLabelAlign LabelAlign
    {
        get => GetValue(LabelAlignProperty);
        set => SetValue(LabelAlignProperty, value);
    }
    
    /// <summary>
    /// How the label text wraps. <see cref="TextWrapping.NoWrap" />, the default, keeps it on one line.
    /// </summary>
    public TextWrapping LabelWrapping
    {
        get => GetValue(LabelWrappingProperty);
        set => SetValue(LabelWrappingProperty, value);
    }
    
    /// <summary>
    /// Width of the label column of the item, as a grid length per media breakpoint. It applies only in the
    /// horizontal layout. The item uses the length of the current breakpoint of the owning media query host, or the
    /// <see cref="MediaBreakPoint.Large" /> length when it has none. When it is <see langword="null" />, the default,
    /// the column is one star unit wide.
    /// </summary>
    public MediaBreakGridLength? LabelColInfo
    {
        get => GetValue(LabelColInfoProperty);
        set => SetValue(LabelColInfoProperty, value);
    }
    
    /// <summary>
    /// Width of the column that holds <see cref="Content" /> next to the label, as a grid length per media
    /// breakpoint. It applies only in the horizontal layout. The item uses the length of the current breakpoint of
    /// the owning media query host, or the <see cref="MediaBreakPoint.Large" /> length when it has none. When it is
    /// <see langword="null" />, the default, the column is three star units wide.
    /// </summary>
    public MediaBreakGridLength? WrapperColInfo
    {
        get => GetValue(WrapperColInfoProperty);
        set => SetValue(WrapperColInfoProperty, value);
    }

    /// <summary>
    /// The name of the field that the item edits. It is the key of the value of the item in the owner form, the name
    /// passed to <see cref="Validators" />, and the field name in the validation report. The item is a field only
    /// while the name is neither <see langword="null" /> nor blank and <see cref="LabelText" /> is set as well.
    /// </summary>
    public new string? FieldName
    {
        get => GetValue(FieldNameProperty);
        set => SetValue(FieldNameProperty, value);
    }
    
    /// <summary>
    /// Whether the field of the item is required. Whether the label shows a mark for that follows
    /// <see cref="AbstractFormItem.RequiredMark" />, and marks are shown only on items that are value items.
    /// </summary>
    public new bool IsRequired
    {
        get => GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
    }
    
    /// <summary>
    /// The validators that the item runs on the value of <see cref="Content" />. When it is <see langword="null" />
    /// or empty, the default, a validation reports success.
    /// </summary>
    public new IList<IFormValidator>? Validators
    {
        get => GetValue(ValidatorsProperty);
        set => SetValue(ValidatorsProperty, value);
    }
    
    /// <summary>
    /// Text of the tooltip that opens over <see cref="TooltipIcon" />. The icon is hidden while this value is
    /// <see langword="null" />, the default, so an item without a tooltip shows no icon.
    /// </summary>
    public string? Tooltip
    {
        get => GetValue(TooltipProperty);
        set => SetValue(TooltipProperty, value);
    }
    
    /// <summary>
    /// Icon that shows the <see cref="Tooltip" /> text in a tooltip. When it is <see langword="null" /> while the item
    /// is initialized, a question-circle icon is set instead. The icon is shown only while <see cref="Tooltip" /> is
    /// not <see langword="null" />.
    /// </summary>
    public PathIcon? TooltipIcon
    {
        get => GetValue(TooltipIconProperty);
        set => SetValue(TooltipIconProperty, value);
    }
    
    /// <summary>
    /// When the item starts a validation on its own. <see cref="FormValidateTrigger.OnChanged" />, the default,
    /// validates after the value changes, waiting <see cref="ValidateDebounce" /> first.
    /// <see cref="FormValidateTrigger.OnBlur" /> validates when keyboard focus leaves the item, and
    /// <see cref="FormValidateTrigger.OnSubmit" /> starts no validation itself.
    /// <see cref="FormValidateTrigger.OnChanged" /> and <see cref="FormValidateTrigger.OnBlur" /> need an owner form
    /// and are suppressed while a value is written through <see cref="SetItemValue" /> or by the owner form.
    /// A value set on the item overrides the value of the owner form.
    /// </summary>
    public new FormValidateTrigger ValidateTrigger
    {
        get => GetValue(ValidateTriggerProperty);
        set => SetValue(ValidateTriggerProperty, value);
    }
    
    /// <summary>
    /// How long the item waits after a value change before it validates. It applies only with
    /// <see cref="FormValidateTrigger.OnChanged" />. <see cref="TimeSpan.Zero" />, the default, validates without
    /// waiting, and every further value change restarts the wait.
    /// </summary>
    public new TimeSpan ValidateDebounce
    {
        get => GetValue(ValidateDebounceProperty);
        set => SetValue(ValidateDebounceProperty, value);
    }
    
    /// <summary>
    /// How the item runs <see cref="Validators" />. <see cref="FormValidateStrategy.StopWhenFirstFailed" />, the
    /// default, runs them one after another and stops at the first one that fails.
    /// <see cref="FormValidateStrategy.Sequential" /> runs all of them one after another, and
    /// <see cref="FormValidateStrategy.Parallel" /> starts them all at once.
    /// </summary>
    public new FormValidateStrategy ValidateStrategy
    {
        get => GetValue(ValidateStrategyProperty);
        set => SetValue(ValidateStrategyProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the validation status of the item. The theme selects the status style of the item by this value,
    /// and the validation feedback control follows it.
    /// While a validation runs the status is <see cref="FormValidateStatus.Validating" />. Otherwise it is
    /// <see cref="FormValidateStatus.Error" /> while error messages exist, or else the status that the last finished
    /// validation produced; before the first one and after a reset it is <see cref="FormValidateStatus.Default" />.
    /// A value set from outside lasts only until the next refresh of the validation state.
    /// </summary>
    public new FormValidateStatus ValidateStatus
    {
        get => base.ValidateStatus;
        set => base.ValidateStatus = value;
    }

    /// <summary>
    /// Gets the validation result derived from <see cref="ValidateStatus" />. <see cref="FormValidateResult.Error" />
    /// and <see cref="FormValidateResult.Warning" /> follow the status, and every other status, including
    /// <see cref="FormValidateStatus.Validating" />, is reported as <see cref="FormValidateResult.Success" />.
    /// </summary>
    public new FormValidateResult ValidateResult => base.ValidateResult;

    /// <summary>
    /// Arranges the label beside the value control (<see cref="FormItemLayout.Horizontal" />, the default) or above
    /// it (<see cref="FormItemLayout.Vertical" />). <see cref="LabelColInfo" />, <see cref="WrapperColInfo" /> and the
    /// label colon apply only in the horizontal layout. The owner form sets this value on the item from its own
    /// <see cref="FormLayout" />, and a value set directly on the item overrides it.
    /// </summary>
    public new FormItemLayout Layout
    {
        get => GetValue(LayoutProperty);
        set => SetValue(LayoutProperty, value);
    }
    
    /// <summary>
    /// Spacing between the children of the item. The default is <c>0</c>. Nothing reads this property at present, so
    /// setting it has no effect.
    /// </summary>
    public double ChildrenSpacing
    {
        get => GetValue(ChildrenSpacingProperty);
        set => SetValue(ChildrenSpacingProperty, value);
    }
    
    /// <summary>
    /// The control that holds the value of the item. It is the XAML content of the element.
    /// The control must implement <see cref="IFormItemAware" /> so that the item can read and write its value; other
    /// content is rejected when it is set, unless <see cref="IsValidateContentType" /> is <see langword="false" />.
    /// While it is <see langword="null" />, the default, <see cref="GetItemValue()" /> returns <see langword="null" />
    /// and <see cref="SetItemValue" /> does nothing.
    /// </summary>
    [Content]
    public new Control? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }
    
    /// <summary>
    /// Requires <see cref="Content" /> to implement <see cref="IFormItemAware" />. While it is <see langword="true" />,
    /// the default, setting other content throws an <see cref="InvalidOperationException" />. When it is
    /// <see langword="false" />, any control is accepted, but the item cannot read or write the value of a control
    /// that does not implement <see cref="IFormItemAware" />. <see cref="FormActionsItem" /> defaults to
    /// <see langword="false" />.
    /// </summary>
    public new bool IsValidateContentType
    {
        get => GetValue(IsValidateContentTypeProperty);
        set => SetValue(IsValidateContentTypeProperty, value);
    }
    #endregion

    #region 公共事件定义

    /// <summary>
    /// Occurs when a validation of the item has finished and updated its validation state. It is raised after
    /// <see cref="ValidateStatus" />, <see cref="ValidateResult" /> and the messages are committed, and it carries the
    /// new status. It is not raised for a validation that was canceled or replaced by a newer one.
    /// </summary>
    public new event EventHandler<FormItemValidateChangedEventArgs>? ValidateChanged
    {
        add => AddHandler(ValidateChangedEvent, value);
        remove => RemoveHandler(ValidateChangedEvent, value);
    }

    #endregion

    #region 内部属性定义
    internal static readonly StyledProperty<bool> IsShowColonProperty =
        Form.IsShowColonProperty.AddOwner<FormItem>();

    internal static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<FormItem>();
    
    internal static readonly StyledProperty<InputControlStyleVariant> StyleVariantProperty =
        InputControlStyleVariantProperty.StyleVariantProperty.AddOwner<FormItem>();
    
    internal static readonly DirectProperty<FormItem, bool> IsColonVisibleProperty =
        AvaloniaProperty.RegisterDirect<FormItem, bool>(
            nameof(IsColonVisible),
            o => o.IsColonVisible,
            (o, v) => o.IsColonVisible = v);
    
    internal new static readonly StyledProperty<bool> IsValueItemProperty =
        AbstractFormItem.IsValueItemProperty.AddOwner<FormItem>();
    
    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<FormItem>();
    
    internal new static readonly StyledProperty<FormRequiredMark> RequiredMarkProperty =
        AbstractFormItem.RequiredMarkProperty.AddOwner<FormItem>();
    

    

    
    internal static readonly StyledProperty<object?> CustomRequireMarkProperty =
        Form.CustomRequireMarkProperty.AddOwner<FormItem>();
    
    internal static readonly StyledProperty<IDataTemplate?> CustomRequireMarkTemplateProperty =
        Form.CustomRequireMarkTemplateProperty.AddOwner<FormItem>();
    
    internal static readonly StyledProperty<object?> CustomOptionalMarkProperty =
        Form.CustomOptionalMarkProperty.AddOwner<FormItem>();
    
    internal static readonly StyledProperty<IDataTemplate?> CustomOptionalMarkTemplateProperty =
        Form.CustomOptionalMarkTemplateProperty.AddOwner<FormItem>();
    
    internal static readonly DirectProperty<FormItem, bool> HasCustomRequireMarkProperty =
        AvaloniaProperty.RegisterDirect<FormItem, bool>(
            nameof(HasCustomRequireMark),
            o => o.HasCustomRequireMark,
            (o, v) => o.HasCustomRequireMark = v);
    
    internal static readonly DirectProperty<FormItem, bool> HasCustomOptionalMarkProperty =
        AvaloniaProperty.RegisterDirect<FormItem, bool>(
            nameof(HasCustomOptionalMark),
            o => o.HasCustomOptionalMark,
            (o, v) => o.HasCustomOptionalMark = v);
    
    internal static readonly DirectProperty<FormItem, double> LabelMaxWidthProperty =
        AvaloniaProperty.RegisterDirect<FormItem, double>(
            nameof(LabelMaxWidth),
            o => o.LabelMaxWidth,
            (o, v) => o.LabelMaxWidth = v);
    
    internal static readonly DirectProperty<FormItem, double> ContentPresenterMaxWidthProperty =
        AvaloniaProperty.RegisterDirect<FormItem, double>(
            nameof(ContentPresenterMaxWidth),
            o => o.ContentPresenterMaxWidth,
            (o, v) => o.ContentPresenterMaxWidth = v);
    
    internal static readonly DirectProperty<FormItem, bool> HasErrorOrWarningMsgProperty =
        AvaloniaProperty.RegisterDirect<FormItem, bool>(
            nameof(HasErrorOrWarningMsg),
            o => o.HasErrorOrWarningMsg,
            (o, v) => o.HasErrorOrWarningMsg = v);

    internal static readonly StyledProperty<IBrush?> ErrorMessageForegroundProperty =
        Form.ErrorMessageForegroundProperty.AddOwner<FormItem>();
    
    internal static readonly StyledProperty<IBrush?> WarningMessageForegroundProperty =
        Form.WarningMessageForegroundProperty.AddOwner<FormItem>();
    
    internal static readonly StyledProperty<IDataTemplate?> FeedbackTemplateProperty =
        Form.FeedbackTemplateProperty.AddOwner<FormItem>();
    
    internal static readonly DirectProperty<FormItem, bool> IsShowItemDeleteButtonProperty =
        AvaloniaProperty.RegisterDirect<FormItem, bool>(
            nameof(IsShowItemDeleteButton),
            o => o.IsShowItemDeleteButton,
            (o, v) => o.IsShowItemDeleteButton = v);
    
    internal static readonly DirectProperty<FormItem, bool> IsEffectiveShowItemDeleteButtonProperty =
        AvaloniaProperty.RegisterDirect<FormItem, bool>(
            nameof(IsEffectiveShowItemDeleteButton),
            o => o.IsEffectiveShowItemDeleteButton,
            (o, v) => o.IsEffectiveShowItemDeleteButton = v);
    
    internal static readonly DirectProperty<FormItem, PathIcon?> ItemDeleteButtonIconProperty =
        AvaloniaProperty.RegisterDirect<FormItem, PathIcon?>(
            nameof(ItemDeleteButtonIcon),
            o => o.ItemDeleteButtonIcon,
            (o, v) => o.ItemDeleteButtonIcon = v);
    
    internal static readonly DirectProperty<FormItem, IIconTemplate?> ItemDeleteButtonIconTemplateProperty =
        AvaloniaProperty.RegisterDirect<FormItem, IIconTemplate?>(
            nameof(ItemDeleteButtonIconTemplate),
            o => o.ItemDeleteButtonIconTemplate,
            (o, v) => o.ItemDeleteButtonIconTemplate = v);
    
    internal new static readonly StyledProperty<FormLayout> FormLayoutProperty =
        AbstractFormItem.FormLayoutProperty.AddOwner<FormItem>();
    
    /// <summary>
    /// Identifies the <see cref="IsHideItemLabel" /> property.
    /// </summary>
    public static readonly DirectProperty<FormItem, bool> IsHideItemLabelProperty =
        AvaloniaProperty.RegisterDirect<FormItem, bool>(
            nameof(IsHideItemLabel),
            o => o.IsHideItemLabel,
            (o, v) => o.IsHideItemLabel = v);
    
    internal bool IsShowColon
    {
        get => GetValue(IsShowColonProperty);
        set => SetValue(IsShowColonProperty, value);
    }
    
    internal CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    internal InputControlStyleVariant StyleVariant
    {
        get => GetValue(StyleVariantProperty);
        set => SetValue(StyleVariantProperty, value);
    }
    
    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    private bool _isColonVisible;

    internal bool IsColonVisible
    {
        get => _isColonVisible;
        set => SetAndRaise(IsColonVisibleProperty, ref _isColonVisible, value);
    }
    
    // A FormItem that has both a label and a field name is a value item.
    internal new bool IsValueItem
    {
        get => GetValue(IsValueItemProperty);
        set => SetValue(IsValueItemProperty, value);
    }
    
    internal new FormRequiredMark RequiredMark
    {
        get => GetValue(RequiredMarkProperty);
        set => SetValue(RequiredMarkProperty, value);
    }
    
    internal object? CustomRequireMark
    {
        get => GetValue(CustomRequireMarkProperty);
        set => SetValue(CustomRequireMarkProperty, value);
    }
    
    internal IDataTemplate? CustomRequireMarkTemplate
    {
        get => GetValue(CustomRequireMarkTemplateProperty);
        set => SetValue(CustomRequireMarkTemplateProperty, value);
    }
    
    [DependsOn(nameof(CustomOptionalMarkTemplate))]
    internal object? CustomOptionalMark
    {
        get => GetValue(CustomOptionalMarkProperty);
        set => SetValue(CustomOptionalMarkProperty, value);
    }
    
    internal IDataTemplate? CustomOptionalMarkTemplate
    {
        get => GetValue(CustomOptionalMarkTemplateProperty);
        set => SetValue(CustomOptionalMarkTemplateProperty, value);
    }
    
    private bool _hasCustomRequireMark;
    
    internal bool HasCustomRequireMark
    {
        get => _hasCustomRequireMark;
        set => SetAndRaise(HasCustomRequireMarkProperty, ref _hasCustomRequireMark, value);
    }
    
    private bool _hasCustomOptionalMark;
    
    internal bool HasCustomOptionalMark
    {
        get => _hasCustomOptionalMark;
        set => SetAndRaise(HasCustomOptionalMarkProperty, ref _hasCustomOptionalMark, value);
    }
    
    private double _labelMaxWidth = double.PositiveInfinity;
    
    internal double LabelMaxWidth
    {
        get => _labelMaxWidth;
        set => SetAndRaise(LabelMaxWidthProperty, ref _labelMaxWidth, value);
    }
    
    private double _contentPresenterMaxWidth = double.PositiveInfinity;
    
    internal double ContentPresenterMaxWidth
    {
        get => _contentPresenterMaxWidth;
        set => SetAndRaise(ContentPresenterMaxWidthProperty, ref _contentPresenterMaxWidth, value);
    }

    private bool _hasErrorOrWarningMsg;

    internal bool HasErrorOrWarningMsg
    {
        get => _hasErrorOrWarningMsg;
        set => SetAndRaise(HasErrorOrWarningMsgProperty, ref _hasErrorOrWarningMsg, value);
    }

    internal IBrush? ErrorMessageForeground
    {
        get => GetValue(ErrorMessageForegroundProperty);
        set => SetValue(ErrorMessageForegroundProperty, value);
    }
    
    internal IBrush? WarningMessageForeground
    {
        get => GetValue(WarningMessageForegroundProperty);
        set => SetValue(WarningMessageForegroundProperty, value);
    }
    
    internal IDataTemplate? FeedbackTemplate
    {
        get => GetValue(FeedbackTemplateProperty);
        set => SetValue(FeedbackTemplateProperty, value);
    }
    
    private bool _isShowItemDeleteButton;

    internal bool IsShowItemDeleteButton
    {
        get => _isShowItemDeleteButton;
        set => SetAndRaise(IsShowItemDeleteButtonProperty, ref _isShowItemDeleteButton, value);
    }
    
    private bool _isEffectiveShowItemDeleteButton;

    internal bool IsEffectiveShowItemDeleteButton
    {
        get => _isEffectiveShowItemDeleteButton;
        set => SetAndRaise(IsEffectiveShowItemDeleteButtonProperty, ref _isEffectiveShowItemDeleteButton, value);
    }
    
    private PathIcon? _itemDeleteButtonIcon;

    internal PathIcon? ItemDeleteButtonIcon
    {
        get => _itemDeleteButtonIcon;
        set => SetAndRaise(ItemDeleteButtonIconProperty, ref _itemDeleteButtonIcon, value);
    }
    
    private IIconTemplate? _itemDeleteButtonIconTemplate;

    internal IIconTemplate? ItemDeleteButtonIconTemplate
    {
        get => _itemDeleteButtonIconTemplate;
        set => SetAndRaise(ItemDeleteButtonIconTemplateProperty, ref _itemDeleteButtonIconTemplate, value);
    }
    
    /// <summary>
    /// Layout of the owner form as this item sees it. While it is <see cref="FormLayout.Inline" />, the item reports
    /// no width limit for its content presenter; in the other layouts the limit is the width of the content layout,
    /// updated on every layout pass. A value set on the item overrides the value taken over from the form.
    /// </summary>
    public new FormLayout FormLayout
    {
        get => GetValue(FormLayoutProperty);
        set => SetValue(FormLayoutProperty, value);
    }
    
    private bool _isHideItemLabel;

    /// <summary>
    /// Places <see cref="Content" /> across the full width of the item instead of in the column next to the label.
    /// While it is <see langword="true" />, <see cref="Layout" />, <see cref="LabelColInfo" /> and
    /// <see cref="WrapperColInfo" /> have no effect on the grid of the item.
    /// </summary>
    public bool IsHideItemLabel
    {
        get => _isHideItemLabel;
        set => SetAndRaise(IsHideItemLabelProperty, ref _isHideItemLabel, value);
    }
    
    #endregion

    #region 内部事件定义

    internal event EventHandler<RoutedEventArgs>? ValueChanged
    {
        add => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }
    
    internal event EventHandler<RoutedEventArgs>? DeleteRequest
    {
        add => AddHandler(DeleteRequestEvent, value);
        remove => RemoveHandler(DeleteRequestEvent, value);
    }

    #endregion
    
    /// <summary>
    /// Uses <see cref="FormItem" /> as the style key, so the FormItem theme applies to subclasses as well.
    /// </summary>
    protected override Type StyleKeyOverride => typeof(FormItem);

    private Grid? _bodyLayout;
    private Panel? _contentLayout;
    private Panel? _labelLayout;
    private StackPanel? _extraInfoLayout;
    private Avalonia.Controls.TextBlock? _helpText;
    private readonly List<Avalonia.Controls.TextBlock> _messageItems = new();
    private MediaBreakPoint? _breakPoint;
    private CompositeDisposable? _disposables;
    private FormValidateFeedback? _feedback;
    private IDisposable? _feedbackDisposable;
    internal new Form? OwnerForm
    {
        get => base.OwnerForm as Form;
        set => base.OwnerForm = value;
    }
    private IMediaBreakAwareControl? _mediaOwner;
    
    static FormItem()
    {
        AffectsMeasure<FormItem>(SizeTypeProperty);
        IconButton.ClickEvent.AddClassHandler<FormItem>((formItem, args) => formItem.HandleDeleteButtonClicked(args));
    }
    
    public FormItem()
    {
        LayoutUpdated += HandleLayoutUpdated;
    }

    /// <summary>
    /// Sets a question-circle icon as <see cref="TooltipIcon" /> when the item is initialized without one.
    /// </summary>
    /// <remarks>
    /// An override must call the base member.
    /// </remarks>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (TooltipIcon == null)
        {
            SetCurrentValue(TooltipIconProperty, new QuestionCircleOutlined());
        }
    }

    private void HandleDeleteButtonClicked(RoutedEventArgs args)
    {
        if (args.Source is ItemDeleteButton)
        {
            RaiseEvent(new RoutedEventArgs(DeleteRequestEvent, this));
            args.Handled = true;
        }
    }

    /// <summary>
    /// Connects the new <see cref="Content" /> to this item after the base member has processed the change.
    /// </summary>
    /// <param name="change">
    /// The change of the <see cref="Content" /> property.
    /// </param>
    /// <remarks>
    /// It replaces the relays to the previous content: the content follows the size type of the item, and also its
    /// motion flag and style variant when it implements <see cref="IMotionAwareControl" /> or
    /// <see cref="IInputControlStyleVariantAware" />. A content that implements <see cref="IFormItemFeedbackAware" />
    /// receives the validation feedback control of the item. An override must call the base member.
    /// </remarks>
    protected override void NotifyContentAdded(AvaloniaPropertyChangedEventArgs change)
    {
        base.NotifyContentAdded(change);
        _disposables?.Dispose();
        // Content is supplied at runtime and can be any form-aware control, outside this ControlTemplate. ControlTheme
        // cannot bind these interface-selected targets; replacing Content disposes the previous bindings.
        _disposables = new CompositeDisposable();
        if (Content is not null)
        {
            _disposables.Add(FormSizeTypeBindingHelper.RelaySizeType(this, SizeTypeProperty, Content));
        }
        if (Content is IMotionAwareControl)
        {
            _disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, Content, IsMotionEnabledProperty));
        }
        if (Content is IInputControlStyleVariantAware)
        {
            _disposables.Add(BindUtils.RelayBind(this, StyleVariantProperty, Content, StyleVariantProperty));
        }
        
        if (change.NewValue is IFormItemFeedbackAware newFormItemFeedbackAware)
        {
            BuildFeedback(false);
            newFormItemFeedbackAware.SetFeedbackControl(_feedback);
        }
    }

    /// <inheritdoc />
    protected override void OnLostFocus(FocusChangedEventArgs e) => base.OnLostFocus(e);

    /// <summary>
    /// Returns the value that <see cref="Content" /> reports through <see cref="IFormItemAware" />, or
    /// <see langword="null" /> while <see cref="Content" /> is <see langword="null" /> or does not implement
    /// <see cref="IFormItemAware" />.
    /// </summary>
    public new object? GetItemValue() => base.GetItemValue();

    /// <summary>
    /// Writes the value to <see cref="Content" /> through <see cref="IFormItemAware" />. It does nothing while
    /// <see cref="Content" /> is <see langword="null" />. The write is not a user change: it does not mark the item
    /// as touched and does not start the automatic validation of the item.
    /// </summary>
    /// <param name="value">
    /// The value to write.
    /// </param>
    public new void SetItemValue(object? value) => base.SetItemValue(value);

    /// <summary>
    /// Resets the item to its initial state. It writes <see cref="InitialValue" /> to <see cref="Content" />, or
    /// clears the value while no initial value is set, and it clears the validation state, the messages and the
    /// touched flag. An initial value that the owner form holds for <see cref="FieldName" /> takes precedence.
    /// </summary>
    public new void ResetItemValue() => base.ResetItemValue();

    /// <summary>
    /// Updates the state that the item derives from its properties after the base member has handled the change.
    /// </summary>
    /// <param name="change">
    /// The change of the property that was set.
    /// </param>
    /// <remarks>
    /// Label and layout changes update the label colon and the grid of the item. A changed message foreground
    /// recreates the message items. A changed feedback template or feedback switch rebuilds the feedback control,
    /// a changed delete button icon template rebuilds its icon, and a changed <see cref="Help" /> text updates the
    /// message flag. An override must call the base member.
    /// </remarks>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsShowColonProperty ||
            change.Property == LabelTextProperty ||
            change.Property == LayoutProperty)
        {
            ConfigureLabelColonVisible();
        }
        
        if (change.Property == LayoutProperty ||
            change.Property == LabelColInfoProperty ||
            change.Property == WrapperColInfoProperty ||
            change.Property == IsHideItemLabelProperty)
        {
            ConfigureLayout();
        }

        if (change.Property == CustomRequireMarkProperty)
        {
            HasCustomRequireMark = CustomRequireMark != null;
        }
        else if (change.Property == CustomOptionalMarkProperty)
        {
            HasCustomOptionalMark = CustomOptionalMark != null;
        }
        else if (change.Property == ErrorMessageForegroundProperty ||
                 change.Property == WarningMessageForegroundProperty)
        {
            BuildMessageItems();
        }

        if (change.Property == FeedbackTemplateProperty ||
            change.Property == IsValidateFeedbackEnabledProperty)
        {
            HandleFeedbackTemplateChanged();
        }
        else if (change.Property == ItemDeleteButtonIconTemplateProperty)
        {
            if (ItemDeleteButtonIconTemplate == null)
            {
                ItemDeleteButtonIcon = null;
            }
            else
            {
                ItemDeleteButtonIcon = ItemDeleteButtonIconTemplate.Build();
            }
        }
        else if (change.Property == HelpProperty)
        {
            HasErrorOrWarningMsg = ValidateErrorMessages?.Count > 0 || ValidateWarningMessages?.Count > 0 || !string.IsNullOrWhiteSpace(Help);
        }
    }

    private Control? BuildFeedback(bool force = false)
    {
        if (OwnerForm == null)
        {
            return null;
        }
        if (force)
        {
            ReleaseFeedback();
        }
        if (_feedback == null || force)
        {
            _feedback = FeedbackTemplate?.Build(OwnerForm) as FormValidateFeedback;
            if (_feedback != null)
            {
                _feedback.DataContext = OwnerForm;
                _feedbackDisposable = BindUtils.RelayBind(this, ValidateStatusProperty, _feedback, FormValidateFeedback.ValidateStatusProperty);
            }
        }
        return _feedback;
    }
    
    /// <summary>
    /// Captures the layout parts of the template, arranges the grid of the item, creates the message items and hands
    /// the validation feedback control to the content.
    /// </summary>
    /// <remarks>
    /// The content receives a feedback control only while <see cref="IsValidateFeedbackEnabled" /> is
    /// <see langword="true" /> and the content implements <see cref="IFormItemFeedbackAware" />. Debug builds assert
    /// that the item belongs to a form. An override must call the base member.
    /// </remarks>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _bodyLayout      = e.NameScope.Find<Grid>("PART_BodyLayout");
        _labelLayout     = e.NameScope.Find<Panel>("PART_LabelLayout");
        _contentLayout   = e.NameScope.Find<Panel>("PART_ContentLayout");
        _extraInfoLayout = e.NameScope.Find<StackPanel>("ExtraInfoLayout");
        _helpText        = e.NameScope.Find<Avalonia.Controls.TextBlock>("HelpText");
        ConfigureLayout();
        BuildMessageItems();
        Debug.Assert(OwnerForm != null);
        if (IsValidateFeedbackEnabled)
        {
            if (Content is IFormItemFeedbackAware feedbackAware)
            {
                BuildFeedback(false);
                feedbackAware.SetFeedbackControl(_feedback);
            }
        }
    }

    private void HandleLayoutUpdated(object? sender, EventArgs e)
    {
        if (_contentLayout != null)
        {
            if (FormLayout != FormLayout.Inline)
            {
                ContentPresenterMaxWidth = _contentLayout.Bounds.Width;
            }
            else
            {
                ContentPresenterMaxWidth = double.PositiveInfinity;
            }

        }
        
        if (_labelLayout != null)
        {
            if (Layout == FormItemLayout.Horizontal)
            {
                LabelMaxWidth = _labelLayout.Bounds.Width;
            }
            else
            {
                LabelMaxWidth = double.PositiveInfinity;
            }
        }
    }
    
    /// <summary>
    /// Subscribes to the media breakpoint of the owning media query host when the item enters the visual tree, so
    /// that the grid of the item follows breakpoint changes.
    /// </summary>
    /// <remarks>
    /// An override must call the base member.
    /// </remarks>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (MediaQueryHost.FindOwner(this) is { } mediaOwner)
        {
            _mediaOwner = mediaOwner;
            _breakPoint = mediaOwner.MediaBreakPoint;
            mediaOwner.MediaBreakPointChanged += HandleMediaBreakChanged;
        }
    }

    /// <summary>
    /// Ends the subscription to the media breakpoint of the owner when the item leaves the visual tree.
    /// </summary>
    /// <remarks>
    /// An override must call the base member.
    /// </remarks>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (_mediaOwner != null)
        {
            _mediaOwner.MediaBreakPointChanged -= HandleMediaBreakChanged;
        }

        _mediaOwner = null;
    }
    
    private void HandleMediaBreakChanged(object? sender, MediaBreakPointChangedEventArgs args)
    {
        _breakPoint = args.MediaBreakPoint;
        if (_breakPoint != null)
        {
            if (Layout == FormItemLayout.Horizontal)
            {
                ConfigureLayout();
            }
        }
    }

    private void ConfigureLabelColonVisible()
    {
        IsColonVisible = !string.IsNullOrWhiteSpace(LabelText) && IsShowColon && Layout == FormItemLayout.Horizontal;
    }

    private void ConfigureLayout()
    {
        if (_bodyLayout == null || _labelLayout == null || _contentLayout == null)
        {
            return;
        }

        _bodyLayout.RowDefinitions.Clear();
        _bodyLayout.ColumnDefinitions.Clear();
        if (!IsHideItemLabel)
        {
            if (Layout == FormItemLayout.Horizontal)
            {
                _bodyLayout.ColumnDefinitions.Add(new ColumnDefinition(GetGridLengthForMediaBreak(_breakPoint ?? MediaBreakPoint.Large, 
                    LabelColInfo ?? new MediaBreakGridLength(new GridLength(1, GridUnitType.Star)))));
                _bodyLayout.ColumnDefinitions.Add(new ColumnDefinition(GetGridLengthForMediaBreak(_breakPoint ?? MediaBreakPoint.Large,
                    WrapperColInfo ?? new MediaBreakGridLength(new GridLength(3, GridUnitType.Star)))));
                _bodyLayout.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                Grid.SetRow(_labelLayout, 0);
                Grid.SetRow(_contentLayout, 0);
                Grid.SetColumn(_labelLayout, 0);
                Grid.SetColumn(_contentLayout, 1);
            }
            else if (Layout == FormItemLayout.Vertical)
            {
                _bodyLayout.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                _bodyLayout.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                _bodyLayout.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                Grid.SetRow(_labelLayout, 0);
                Grid.SetRow(_contentLayout, 1);
                Grid.SetColumn(_labelLayout, 0);
                Grid.SetColumn(_contentLayout, 0);
            }
        }
        else
        {
            _bodyLayout.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            _bodyLayout.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            Grid.SetRow(_contentLayout, 0);
            Grid.SetColumn(_contentLayout, 0);
        }
        
    }
    
    private GridLength GetGridLengthForMediaBreak(MediaBreakPoint breakPoint, MediaBreakGridLength info)
    {
        var gridLength = GridLength.Auto;
        if (breakPoint == MediaBreakPoint.ExtraSmall)
        {
            gridLength = info.ExtraSmall;
        }
        else if (breakPoint == MediaBreakPoint.Small)
        {
            gridLength = info.Small;
        }
        else if (breakPoint == MediaBreakPoint.Medium)
        {
            gridLength = info.Medium;
        }
        else if (breakPoint == MediaBreakPoint.Large)
        {
            gridLength = info.Large;
        }
        else if (breakPoint == MediaBreakPoint.ExtraLarge)
        {
            gridLength = info.ExtraLarge;
        }
        else if (breakPoint == MediaBreakPoint.ExtraExtraLarge)
        {
            gridLength = info.ExtraExtraLarge;
        }
        else if (breakPoint == MediaBreakPoint.ExtraExtraExtraLarge)
        {
            gridLength = info.ExtraExtraLarge;
        }

        return gridLength;
    }
    
    internal override bool IsSkipValidate()
    {
        return !IsVisible;
    }

    /// <inheritdoc />
    protected internal override void NotifyFormItemChanged(IFormItem formItem)
    {
    }

    /// <summary>
    /// Whether the item is a field that the owner form takes into account. A <see cref="FormItem" /> is one only
    /// while <see cref="FieldName" /> and <see cref="LabelText" /> are both neither <see langword="null" /> nor blank.
    /// </summary>
    /// <remarks>
    /// The result is copied into <see cref="AbstractFormItem.IsValueItem" /> whenever the content, the field name or
    /// the label changes. Only value items count for the validity of the owner form and show required marks.
    /// </remarks>
    protected override bool IsField => base.IsField && !string.IsNullOrWhiteSpace(LabelText);

    /// <summary>
    /// Recreates the message items of the item after the item refreshed its validation state.
    /// </summary>
    /// <remarks>
    /// It also updates the flag that says whether there is something to show: error or warning messages, or a
    /// non-blank <see cref="Help" /> text. The call is repeated on every refresh, even when no value changed.
    /// </remarks>
    protected override void OnValidationMessagesChanged()
    {
        HasErrorOrWarningMsg = ValidateErrorMessages?.Count > 0 || ValidateWarningMessages?.Count > 0 || !string.IsNullOrWhiteSpace(Help);
        BuildMessageItems();
    }

    private void HandleFeedbackTemplateChanged()
    {
        ReleaseFeedback();
        if (IsValidateFeedbackEnabled)
        {
            if (Content is IFormItemFeedbackAware itemFeedbackAware)
            {
                BuildFeedback(true);
                itemFeedbackAware.SetFeedbackControl(_feedback);
            }
        }
    }

    private void ReleaseFeedback()
    {
        _feedbackDisposable?.Dispose();
        _feedbackDisposable = null;
        _feedback           = null;
    }
}

/// <summary>
/// A <see cref="FormItem" /> that holds the action buttons of a form, such as submit and reset.
/// It accepts any content without an <see cref="IFormItemAware" /> implementation, and it is no value item by
/// default, so the owner form does not judge its validity.
/// </summary>
public class FormActionsItem : FormItem
{
    static FormActionsItem()
    {
        IsValidateContentTypeProperty.OverrideDefaultValue<FormActionsItem>(false);
        IsValueItemProperty.OverrideDefaultValue<FormActionsItem>(false);
    }
    
    private new bool IsValidateContentType
    {
        get => GetValue(IsValidateContentTypeProperty);
        set => SetValue(IsValidateContentTypeProperty, value);
    }
}
