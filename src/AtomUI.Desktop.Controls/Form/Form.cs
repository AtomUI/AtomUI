using System.Collections;
using System.Collections.Specialized;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A form that stacks its items in one column, or in one row while <see cref="FormLayout" /> is
/// <see cref="FormLayout.Inline" />, and wraps every item that is not a <see cref="FormItem" /> in one.
/// Only form items take part in the values, validation and submit of the form. The layout and style settings of
/// the form are bound to each item.
/// A <see cref="SubmitButton" /> or <see cref="ResetButton" /> inside the form submits or resets it, and the delete
/// button of an item removes the item from the form.
/// </summary>
public class Form : AbstractForm,
                    ICustomizableSizeTypeAware,
                    IMotionAwareControl,
                    IForm,
                    IInputControlStyleVariantAware
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="IsShowColon" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowColonProperty =
        AvaloniaProperty.Register<Form, bool>(
            nameof(IsShowColon), true);

    /// <summary>
    /// Identifies the <see cref="BoxShadow" /> property.
    /// </summary>
    public static readonly StyledProperty<BoxShadows?> BoxShadowProperty =
        AvaloniaProperty.Register<Form, BoxShadows?>(nameof(BoxShadow));
    
    /// <summary>
    /// Identifies the <see cref="StyleVariant" /> property.
    /// </summary>
    public static readonly StyledProperty<InputControlStyleVariant> StyleVariantProperty =
        InputControlStyleVariantProperty.StyleVariantProperty.AddOwner<Form>();
    
    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<Form>();
    
    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Form>();
    
    /// <summary>
    /// Identifies the <see cref="RequiredMark" /> property.
    /// </summary>
    public new static readonly StyledProperty<FormRequiredMark> RequiredMarkProperty =
        AbstractForm.RequiredMarkProperty.AddOwner<Form>();
    
    /// <summary>
    /// Identifies the <see cref="IsValidateFeedbackEnabled" /> property.
    /// </summary>
    public new static readonly StyledProperty<bool> IsValidateFeedbackEnabledProperty =
        AbstractForm.IsValidateFeedbackEnabledProperty.AddOwner<Form>();
    
    /// <summary>
    /// Identifies the <see cref="SuccessFeedback" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> SuccessFeedbackProperty =
        FormValidateFeedback.SuccessFeedbackProperty.AddOwner<Form>();
    
    /// <summary>
    /// Identifies the <see cref="SuccessFeedbackTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> SuccessFeedbackTemplateProperty =
        FormValidateFeedback.SuccessFeedbackTemplateProperty.AddOwner<Form>();
    
    /// <summary>
    /// Identifies the <see cref="WarningFeedback" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> WarningFeedbackProperty =
        FormValidateFeedback.WarningFeedbackProperty.AddOwner<Form>();
    
    /// <summary>
    /// Identifies the <see cref="WarningFeedbackTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> WarningFeedbackTemplateProperty =
        FormValidateFeedback.WarningFeedbackTemplateProperty.AddOwner<Form>();
    
    /// <summary>
    /// Identifies the <see cref="ErrorFeedback" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ErrorFeedbackProperty =
        FormValidateFeedback.ErrorFeedbackProperty.AddOwner<Form>();
    
    /// <summary>
    /// Identifies the <see cref="ErrorFeedbackTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ErrorFeedbackTemplateProperty =
        FormValidateFeedback.ErrorFeedbackTemplateProperty.AddOwner<Form>();
    
    /// <summary>
    /// Identifies the <see cref="ValidatingFeedback" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ValidatingFeedbackProperty =
        FormValidateFeedback.ValidatingFeedbackProperty.AddOwner<Form>();
    
    /// <summary>
    /// Identifies the <see cref="ValidatingFeedbackTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ValidatingFeedbackTemplateProperty =
        FormValidateFeedback.ValidatingFeedbackTemplateProperty.AddOwner<Form>();
    
    /// <summary>
    /// Identifies the <see cref="FeedbackTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> FeedbackTemplateProperty =
        AvaloniaProperty.Register<Form, IDataTemplate?>(nameof(FeedbackTemplate));
    
    /// <summary>
    /// Identifies the <see cref="LabelAlign" /> property.
    /// </summary>
    public static readonly StyledProperty<FormLabelAlign> LabelAlignProperty =
        AvaloniaProperty.Register<Form, FormLabelAlign>(nameof(LabelAlign), FormLabelAlign.Right);
    
    /// <summary>
    /// Identifies the <see cref="LabelWrapping" /> property.
    /// </summary>
    public static readonly StyledProperty<TextWrapping> LabelWrappingProperty =
        AvaloniaProperty.Register<Form, TextWrapping>(nameof(LabelWrapping), TextWrapping.NoWrap);
    
    /// <summary>
    /// Identifies the <see cref="FormLayout" /> property.
    /// </summary>
    public new static readonly StyledProperty<FormLayout> FormLayoutProperty =
        AbstractForm.FormLayoutProperty.AddOwner<Form>();
    
    /// <summary>
    /// Identifies the <see cref="CustomRequireMark" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> CustomRequireMarkProperty =
        AvaloniaProperty.Register<Form, object?>(nameof(CustomRequireMark));
    
    /// <summary>
    /// Identifies the <see cref="CustomRequireMarkTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> CustomRequireMarkTemplateProperty =
        AvaloniaProperty.Register<Form, IDataTemplate?>(nameof(CustomRequireMarkTemplate));
    
    /// <summary>
    /// Identifies the <see cref="CustomOptionalMark" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> CustomOptionalMarkProperty =
        AvaloniaProperty.Register<Form, object?>(nameof(CustomOptionalMark));
    
    /// <summary>
    /// Identifies the <see cref="CustomOptionalMarkTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> CustomOptionalMarkTemplateProperty =
        AvaloniaProperty.Register<Form, IDataTemplate?>(nameof(CustomOptionalMarkTemplate));
    
    /// <summary>
    /// Identifies the <see cref="IsScrollToFirstErrorEnabled" /> property.
    /// </summary>
    public new static readonly StyledProperty<bool> IsScrollToFirstErrorEnabledProperty =
        AbstractForm.IsScrollToFirstErrorEnabledProperty.AddOwner<Form>();
    
    /// <summary>
    /// Identifies the <see cref="ValidateTrigger" /> property.
    /// </summary>
    public new static readonly StyledProperty<FormValidateTrigger> ValidateTriggerProperty =
        AbstractForm.ValidateTriggerProperty.AddOwner<Form>();
    
    /// <summary>
    /// Identifies the <see cref="LabelColInfo" /> property.
    /// </summary>
    public static readonly StyledProperty<MediaBreakGridLength?> LabelColInfoProperty =
        AvaloniaProperty.Register<Form, MediaBreakGridLength?>(nameof(LabelColInfo));
    
    /// <summary>
    /// Identifies the <see cref="WrapperColInfo" /> property.
    /// </summary>
    public static readonly StyledProperty<MediaBreakGridLength?> WrapperColInfoProperty =
        AvaloniaProperty.Register<Form, MediaBreakGridLength?>(nameof(WrapperColInfo));
    
    /// <summary>
    /// Identifies the <see cref="Values" /> property.
    /// </summary>
    public new static readonly DirectProperty<Form, IFormValues?> ValuesProperty =
        AbstractForm.ValuesProperty.AddOwner<Form>(owner => owner.Values);
    
    /// <summary>
    /// Identifies the <see cref="InitialValues" /> property.
    /// </summary>
    public new static readonly DirectProperty<Form, IFormValues?> InitialValuesProperty =
        AbstractForm.InitialValuesProperty.AddOwner<Form>(owner => owner.InitialValues, (owner, value) => owner.InitialValues = value);
    
    /// <summary>
    /// Identifies the <see cref="IsFormValid" /> property.
    /// </summary>
    public new static readonly DirectProperty<Form, bool?> IsFormValidProperty =
        AbstractForm.IsFormValidProperty.AddOwner<Form>(owner => owner.IsFormValid);
    
    /// <summary>
    /// Identifies the <see cref="ErrorMessageForeground" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> ErrorMessageForegroundProperty =
        AvaloniaProperty.Register<Form, IBrush?>(nameof(ErrorMessageForeground));
    
    /// <summary>
    /// Identifies the <see cref="WarningMessageForeground" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> WarningMessageForegroundProperty =
        AvaloniaProperty.Register<Form, IBrush?>(nameof(WarningMessageForeground));
    
    /// <summary>
    /// Identifies the <see cref="MinItemCount" /> property.
    /// </summary>
    public static readonly StyledProperty<int> MinItemCountProperty =
        AvaloniaProperty.Register<Form, int>(nameof(MinItemCount), 1);
    
    /// <summary>
    /// Identifies the <see cref="ItemDeleteButtonIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<IIconTemplate?> ItemDeleteButtonIconProperty =
        AvaloniaProperty.Register<Form, IIconTemplate?>(nameof(ItemDeleteButtonIcon));
    
    /// <summary>
    /// Identifies the <see cref="IsShowItemDeleteButton" /> property.
    /// </summary>
    public static readonly DirectProperty<Form, bool> IsShowItemDeleteButtonProperty =
        AvaloniaProperty.RegisterDirect<Form, bool>(
            nameof(IsShowItemDeleteButton),
            o => o.IsShowItemDeleteButton,
            (o, v) => o.IsShowItemDeleteButton = v);
    
    /// <summary>
    /// Identifies the <see cref="IsHideItemLabel" /> property.
    /// </summary>
    public static readonly DirectProperty<Form, bool> IsHideItemLabelProperty =
        AvaloniaProperty.RegisterDirect<Form, bool>(
            nameof(IsHideItemLabel),
            o => o.IsHideItemLabel,
            (o, v) => o.IsHideItemLabel = v);
    
    /// <summary>
    /// Gets or sets whether a colon is shown after the label text of an item, which applies only in the horizontal
    /// item layout. The default is <see langword="true" />.
    /// The value is set on the form and bound to every item.
    /// </summary>
    public bool IsShowColon
    {
        get => GetValue(IsShowColonProperty);
        set => SetValue(IsShowColonProperty, value);
    }

    /// <summary>
    /// Gets or sets the shadow drawn outside the frame that surrounds the items of the form.
    /// <see langword="null" />, the default, draws no shadow.
    /// </summary>
    public BoxShadows? BoxShadow
    {
        get => GetValue(BoxShadowProperty);
        set => SetValue(BoxShadowProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the style variant that every item relays to its content while the content implements
    /// <see cref="IInputControlStyleVariantAware" />. The value is set on the form and bound to every item.
    /// </summary>
    public InputControlStyleVariant StyleVariant
    {
        get => GetValue(StyleVariantProperty);
        set => SetValue(StyleVariantProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the size that every item uses for its label area and delete button area and relays to a content
    /// that is size aware. The value is set on the form and bound to every item.
    /// The theme sets no value, so a cleared value falls back to the default of the shared size type property.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    /// <summary>
    /// Gets or sets whether motion is enabled for the items of the form. The value is bound to every item, which
    /// relays it to a content that is motion aware.
    /// The default comes from the <c>EnableMotion</c> token of the form, so clearing a value restores the token.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the mark shown with the label of an item to tell whether its value is required; the values are
    /// described by <see cref="FormRequiredMark" />. Marks are shown only on items with
    /// <see cref="AbstractFormItem.IsValueItem" /> set.
    /// The value is set on the form and bound to every item. The default is <see cref="FormRequiredMark.Default" />.
    /// </summary>
    public new FormRequiredMark RequiredMark
    {
        get => GetValue(RequiredMarkProperty);
        set => SetValue(RequiredMarkProperty, value);
    }
    
    /// <summary>
    /// Gets or sets whether each item attaches a validate feedback to its content, an icon that shows the success,
    /// warning, error or validating state. The default is <see langword="false" />.
    /// The value is set on the form and copied to every item; a value set directly on an item takes precedence.
    /// </summary>
    public new bool IsValidateFeedbackEnabled
    {
        get => GetValue(IsValidateFeedbackEnabledProperty);
        set => SetValue(IsValidateFeedbackEnabledProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the content the validate feedback shows while an item is in the
    /// <see cref="FormValidateStatus.Success" /> state and <see cref="IsValidateFeedbackEnabled" /> is set.
    /// It is presented with <see cref="SuccessFeedbackTemplate" />. The default is <see langword="null" />.
    /// </summary>
    [DependsOn(nameof(SuccessFeedbackTemplate))]
    public object? SuccessFeedback
    {
        get => GetValue(SuccessFeedbackProperty);
        set => SetValue(SuccessFeedbackProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the template that builds the validate feedback while an item is in the
    /// <see cref="FormValidateStatus.Success" /> state, with <see cref="SuccessFeedback" /> as its data.
    /// The default Form theme builds a success-colored check-circle icon.
    /// </summary>
    public IDataTemplate? SuccessFeedbackTemplate
    {
        get => GetValue(SuccessFeedbackTemplateProperty);
        set => SetValue(SuccessFeedbackTemplateProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the content the validate feedback shows while an item is in the
    /// <see cref="FormValidateStatus.Warning" /> state and <see cref="IsValidateFeedbackEnabled" /> is set.
    /// It is presented with <see cref="WarningFeedbackTemplate" />. The default is <see langword="null" />.
    /// </summary>
    [DependsOn(nameof(WarningFeedbackTemplate))]
    public object? WarningFeedback
    {
        get => GetValue(WarningFeedbackProperty);
        set => SetValue(WarningFeedbackProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the template that builds the validate feedback while an item is in the
    /// <see cref="FormValidateStatus.Warning" /> state, with <see cref="WarningFeedback" /> as its data.
    /// The default Form theme builds a warning-colored exclamation-circle icon.
    /// </summary>
    public IDataTemplate? WarningFeedbackTemplate
    {
        get => GetValue(WarningFeedbackTemplateProperty);
        set => SetValue(WarningFeedbackTemplateProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the content the validate feedback shows while an item is in the
    /// <see cref="FormValidateStatus.Error" /> state and <see cref="IsValidateFeedbackEnabled" /> is set.
    /// It is presented with <see cref="ErrorFeedbackTemplate" />. The default is <see langword="null" />.
    /// </summary>
    [DependsOn(nameof(ErrorFeedbackTemplate))]
    public object? ErrorFeedback
    {
        get => GetValue(ErrorFeedbackProperty);
        set => SetValue(ErrorFeedbackProperty, value);
    }

    /// <summary>
    /// Gets or sets the template that builds the validate feedback while an item is in the
    /// <see cref="FormValidateStatus.Error" /> state, with <see cref="ErrorFeedback" /> as its data.
    /// The default Form theme builds an error-colored close-circle icon.
    /// </summary>
    public IDataTemplate? ErrorFeedbackTemplate
    {
        get => GetValue(ErrorFeedbackTemplateProperty);
        set => SetValue(ErrorFeedbackTemplateProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the content the validate feedback shows while an item is in the
    /// <see cref="FormValidateStatus.Validating" /> state and <see cref="IsValidateFeedbackEnabled" /> is set.
    /// It is presented with <see cref="ValidatingFeedbackTemplate" />. The default is <see langword="null" />.
    /// </summary>
    [DependsOn(nameof(ValidatingFeedbackTemplate))]
    public object? ValidatingFeedback
    {
        get => GetValue(ValidatingFeedbackProperty);
        set => SetValue(ValidatingFeedbackProperty, value);
    }

    /// <summary>
    /// Gets or sets the template that builds the validate feedback while an item is in the
    /// <see cref="FormValidateStatus.Validating" /> state, with <see cref="ValidatingFeedback" /> as its data.
    /// The default Form theme builds a spinning loading icon.
    /// </summary>
    public IDataTemplate? ValidatingFeedbackTemplate
    {
        get => GetValue(ValidatingFeedbackTemplateProperty);
        set => SetValue(ValidatingFeedbackTemplateProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the template that builds the validate feedback of the items, with the form as its data. An item
    /// uses the result only when it is a <see cref="FormValidateFeedback" />, and hands it to its content only while
    /// <see cref="IsValidateFeedbackEnabled" /> is set.
    /// The default Form theme creates one bound to the feedback content and template properties of the form;
    /// <see langword="null" /> gives the items no feedback.
    /// </summary>
    public IDataTemplate? FeedbackTemplate
    {
        get => GetValue(FeedbackTemplateProperty);
        set => SetValue(FeedbackTemplateProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the horizontal alignment of the label text of the items. The vertical item layout left-aligns
    /// the label whatever the value is. The default is <see cref="FormLabelAlign.Right" />.
    /// The value is set on the form and copied to every item; a value set directly on an item overrides it.
    /// </summary>
    public FormLabelAlign LabelAlign
    {
        get => GetValue(LabelAlignProperty);
        set => SetValue(LabelAlignProperty, value);
    }
    
    /// <summary>
    /// Gets or sets how the label text of the items wraps. The value is set on the form and bound to every item.
    /// The default is <see cref="TextWrapping.NoWrap" />.
    /// </summary>
    public TextWrapping LabelWrapping
    {
        get => GetValue(LabelWrappingProperty);
        set => SetValue(LabelWrappingProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the layout every item receives: <see cref="FormLayout.Vertical" /> puts the label above the
    /// content, <see cref="FormLayout.Horizontal" /> and <see cref="FormLayout.Inline" /> put it beside the content.
    /// The default is <see cref="FormLayout.Horizontal" />. The value is set on the form and copied to every item;
    /// a value set directly on an item takes precedence.
    /// <see cref="FormLayout.Inline" /> also lays the items out in one row, spaced by the inline item spacing; the
    /// other values stack them in one column.
    /// </summary>
    public new FormLayout FormLayout
    {
        get => GetValue(FormLayoutProperty);
        set => SetValue(FormLayoutProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the content shown as the required mark of the required items while <see cref="RequiredMark" /> is
    /// <see cref="FormRequiredMark.Customize" />. It is presented with <see cref="CustomRequireMarkTemplate" />.
    /// The value is set on the form and bound to every item; <see langword="null" />, the default, shows no mark.
    /// </summary>
    [DependsOn(nameof(CustomRequireMarkTemplate))]
    public object? CustomRequireMark
    {
        get => GetValue(CustomRequireMarkProperty);
        set => SetValue(CustomRequireMarkProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the template that builds the required mark of the required items while <see cref="RequiredMark" />
    /// is <see cref="FormRequiredMark.Customize" />, with <see cref="CustomRequireMark" /> as its data.
    /// The default is <see langword="null" />.
    /// </summary>
    public IDataTemplate? CustomRequireMarkTemplate
    {
        get => GetValue(CustomRequireMarkTemplateProperty);
        set => SetValue(CustomRequireMarkTemplateProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the content shown as the optional mark of the items that are not required while
    /// <see cref="RequiredMark" /> is <see cref="FormRequiredMark.Customize" />. It is presented with
    /// <see cref="CustomOptionalMarkTemplate" />.
    /// The value is set on the form and bound to every item; <see langword="null" />, the default, shows no mark.
    /// </summary>
    [DependsOn(nameof(CustomOptionalMarkTemplate))]
    public object? CustomOptionalMark
    {
        get => GetValue(CustomOptionalMarkProperty);
        set => SetValue(CustomOptionalMarkProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the template that builds the optional mark of the items that are not required while
    /// <see cref="RequiredMark" /> is <see cref="FormRequiredMark.Customize" />, with
    /// <see cref="CustomOptionalMark" /> as its data. The default is <see langword="null" />.
    /// </summary>
    public IDataTemplate? CustomOptionalMarkTemplate
    {
        get => GetValue(CustomOptionalMarkTemplateProperty);
        set => SetValue(CustomOptionalMarkTemplateProperty, value);
    }
    
    /// <summary>
    /// Gets or sets whether <see cref="Submit" /> reveals the first visible item in error when the validation
    /// fails. The default is <see langword="false" />.
    /// </summary>
    public new bool IsScrollToFirstErrorEnabled
    {
        get => GetValue(IsScrollToFirstErrorEnabledProperty);
        set => SetValue(IsScrollToFirstErrorEnabledProperty, value);
    }
    
    /// <summary>
    /// Gets or sets when an item is validated on its own: <see cref="FormValidateTrigger.OnChanged" /> after the user
    /// changes its value, <see cref="FormValidateTrigger.OnBlur" /> after its content loses focus, and
    /// <see cref="FormValidateTrigger.OnSubmit" /> never, which leaves <see cref="ValidateAsync(CancellationToken)" />,
    /// <see cref="Submit" /> and the item's own validation as triggers.
    /// The default is <see cref="FormValidateTrigger.OnChanged" />. The value is set on the form and copied to every
    /// item; a value set directly on an item takes precedence.
    /// </summary>
    public new FormValidateTrigger ValidateTrigger
    {
        get => GetValue(ValidateTriggerProperty);
        set => SetValue(ValidateTriggerProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the width of the label column in the grid of the items, as a <see cref="MediaBreakGridLength" />.
    /// It applies only in the horizontal item layout. The value is set on the form and bound to every item;
    /// <see langword="null" />, the default, gives the label column a width of <c>1*</c>.
    /// </summary>
    public MediaBreakGridLength? LabelColInfo
    {
        get => GetValue(LabelColInfoProperty);
        set => SetValue(LabelColInfoProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the width of the column that holds the content of the items, as a
    /// <see cref="MediaBreakGridLength" />. It applies only in the horizontal item layout. The value is set on the
    /// form and bound to every item; <see langword="null" />, the default, gives the column a width of <c>3*</c>.
    /// </summary>
    public MediaBreakGridLength? WrapperColInfo
    {
        get => GetValue(WrapperColInfoProperty);
        set => SetValue(WrapperColInfoProperty, value);
    }
    
    private new IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }
    
    private new IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }
    
    /// <summary>
    /// Gets the current values of the items keyed by field name. The property is replaced by a new snapshot whenever
    /// an item value changes or an item is added, removed or reordered. Items with a null or blank field name are
    /// left out.
    /// </summary>
    public new IFormValues? Values => base.Values;

    /// <summary>
    /// Gets or sets the values keyed by field name that <see cref="Reset" /> restores the items to. An item whose
    /// field name is not a key restores its own initial value or is cleared.
    /// The value stored for an item is written into the item when it is attached to the form with content and a
    /// field name, and not again while it stays attached; a value assigned afterwards takes effect on the next reset
    /// only.
    /// </summary>
    public new IFormValues? InitialValues
    {
        get => base.InitialValues;
        set => base.InitialValues = value;
    }

    /// <summary>
    /// Gets whether every item that is a value item has the validation status
    /// <see cref="FormValidateStatus.Success" /> or <see cref="FormValidateStatus.Warning" />.
    /// An item that is validating or was not validated yet makes the value <see langword="false" />.
    /// The value is <see langword="null" /> until the first validation state update, for example a finished
    /// validation or a call to <c>SetFieldErrors</c> or <c>ClearFieldErrors</c>; <see cref="Reset" /> sets it to
    /// <see langword="false" />.
    /// </summary>
    public new bool? IsFormValid => base.IsFormValid;

    /// <summary>
    /// Gets or sets the foreground of the validation error messages of the items. The value is bound to every item.
    /// The default comes from the <c>ColorErrorText</c> token of the form.
    /// </summary>
    public IBrush? ErrorMessageForeground
    {
        get => GetValue(ErrorMessageForegroundProperty);
        set => SetValue(ErrorMessageForegroundProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the foreground of the validation warning messages of the items. The value is bound to every
    /// item. The default comes from the <c>ColorWarningText</c> token of the form.
    /// </summary>
    public IBrush? WarningMessageForeground
    {
        get => GetValue(WarningMessageForegroundProperty);
        set => SetValue(WarningMessageForegroundProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the number of value items the form must exceed for the delete button to be shown on an item
    /// while <see cref="IsShowItemDeleteButton" /> is set. The default is <c>1</c>.
    /// The buttons are re-evaluated only when <see cref="IsShowItemDeleteButton" /> or the items change, so a change
    /// of this value alone takes effect after the next of those.
    /// </summary>
    public int MinItemCount
    {
        get => GetValue(MinItemCountProperty);
        set => SetValue(MinItemCountProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the icon of the delete button of the items. The value is set on the form and bound to every
    /// item. The default Form theme uses an outlined minus-circle icon.
    /// </summary>
    public IIconTemplate? ItemDeleteButtonIcon
    {
        get => GetValue(ItemDeleteButtonIconProperty);
        set => SetValue(ItemDeleteButtonIconProperty, value);
    }
    
    private bool _isShowItemDeleteButton;

    /// <summary>
    /// Gets or sets whether the items of the form show a delete button that removes the item from the form. The
    /// default is <see langword="false" />.
    /// The button itself is shown only on items that are value items, and only while the form holds more than
    /// <see cref="MinItemCount" /> of them.
    /// </summary>
    public bool IsShowItemDeleteButton
    {
        get => _isShowItemDeleteButton;
        set => SetAndRaise(IsShowItemDeleteButtonProperty, ref _isShowItemDeleteButton, value);
    }
    
    private bool _isHideItemLabel;

    /// <summary>
    /// Gets or sets whether the content of every item is placed across the full width of the item instead of in the
    /// column next to the label. While it is set, <see cref="FormLayout" />, <see cref="LabelColInfo" /> and
    /// <see cref="WrapperColInfo" /> have no effect on the grid of the item. The default is <see langword="false" />.
    /// The value is set on the form and bound to every item.
    /// </summary>
    public bool IsHideItemLabel
    {
        get => _isHideItemLabel;
        set => SetAndRaise(IsHideItemLabelProperty, ref _isHideItemLabel, value);
    }
    #endregion

    #region 公共事件定义

    /// <summary>
    /// Occurs when a validation started by <see cref="Validate" />, <see cref="ValidateAsync(CancellationToken)" /> or
    /// <see cref="Submit" /> begins, before any item is validated. A silent validation does not raise it.
    /// </summary>
    public new event EventHandler<EventArgs>? AboutToValidate
    {
        add => base.AboutToValidate += value;
        remove => base.AboutToValidate -= value;
    }
    /// <summary>
    /// Occurs after a validation that is not silent finished and the item states were updated.
    /// <see cref="Submit" /> raises it for both results and passes the values on success. An awaited
    /// <see cref="ValidateAsync(CancellationToken)" /> raises it only for an error result, with no values and every
    /// error and warning message in the event args.
    /// </summary>
    public new event EventHandler<FormValidatedEventArgs>? Validated
    {
        add => base.Validated += value;
        remove => base.Validated -= value;
    }
    /// <summary>
    /// Occurs when <see cref="Submit" /> passes validation, after <see cref="NotifySubmit" /> was called. The event
    /// args carry the values that were validated. A failed validation raises <see cref="AbstractForm.SubmitFailed" />
    /// instead, and a submission canceled by <see cref="AbstractForm.CancelValidation" />, by a newer validation that
    /// is not silent or by detaching the form raises neither.
    /// </summary>
    public new event EventHandler<FormSubmittedEventArgs>? Submitted
    {
        add => base.Submitted += value;
        remove => base.Submitted -= value;
    }
    /// <summary>
    /// Occurs when the content of an item reports a new value, after <see cref="Values" /> was refreshed. The event
    /// args carry the item and its new value. It is raised for any change the content reports, including the writes
    /// of <see cref="SetFormValues" />, and does not tell a user edit from a programmatic write.
    /// </summary>
    public new event EventHandler<FormItemValueChangedEventArgs>? ItemValueChanged
    {
        add => base.ItemValueChanged += value;
        remove => base.ItemValueChanged -= value;
    }
    /// <summary>
    /// Occurs at the end of <see cref="Reset" />, after the items were restored, <see cref="NotifyReset" /> was called
    /// and <see cref="IsFormValid" /> was set to <see langword="false" />.
    /// </summary>
    public new event EventHandler? ResetCompleted
    {
        add => base.ResetCompleted += value;
        remove => base.ResetCompleted -= value;
    }
    
    #endregion
    
    #region 内部属性定义
    internal static readonly DirectProperty<Form, Orientation> FormLayoutOrientationProperty =
        AvaloniaProperty.RegisterDirect<Form, Orientation>(
            nameof(FormLayoutOrientation),
            o => o.FormLayoutOrientation);
    
    internal static readonly DirectProperty<Form, bool> IsEffectiveShowItemDeleteButtonProperty =
        AvaloniaProperty.RegisterDirect<Form, bool>(
            nameof(IsEffectiveShowItemDeleteButton),
            o => o.IsEffectiveShowItemDeleteButton,
            (o, v) => o.IsEffectiveShowItemDeleteButton = v);
    
    internal static readonly StyledProperty<double> FormLayoutSpacingProperty =
        AvaloniaProperty.Register<Form, double>(nameof(FormLayoutSpacing));
    
    private Orientation _formLayoutOrientation;

    internal Orientation FormLayoutOrientation
    {
        get => _formLayoutOrientation;
        set => SetAndRaise(FormLayoutOrientationProperty, ref _formLayoutOrientation, value);
    }
    
    private bool _isEffectiveShowItemDeleteButton;

    internal bool IsEffectiveShowItemDeleteButton
    {
        get => _isEffectiveShowItemDeleteButton;
        set => SetAndRaise(IsEffectiveShowItemDeleteButtonProperty, ref _isEffectiveShowItemDeleteButton, value);
    }
    
    internal double FormLayoutSpacing
    {
        get => GetValue(FormLayoutSpacingProperty);
        set => SetValue(FormLayoutSpacingProperty, value);
    }
    
    #endregion

    static Form()
    {
        AffectsMeasure<Form>(SizeTypeProperty);
        AffectsMeasure<Form>(FormLayoutOrientationProperty);
        SubmitButton.SubmitEvent.AddClassHandler<Form>((form, args) => form.HandleSubmitButtonClick(args));
        ResetButton.ResetEvent.AddClassHandler<Form>((form, args) => form.HandleResetButtonClick(args));
        FormItem.DeleteRequestEvent.AddClassHandler<Form>((form, args) => form.HandleFormItemDelete(args));
    }
    
    public Form()
    {
        LogicalChildren.CollectionChanged += HandleCollectionChanged;
        Items.CollectionChanged           += HandleItemsCollectionChanged;
    }

    private void HandleCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ConfigureShowItemDeleteButton();
    }

    private void HandleItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        InvalidateMeasure();
    }
    
    /// <summary>
    /// Returns a new <see cref="FormItem" /> whose owner form is this form. It is called for an item that needs a
    /// container, which is every item that is not itself a <see cref="FormItem" />.
    /// An override must return a <see cref="FormItem" />, because <see cref="PrepareContainerForItemOverride" />
    /// throws for any other control.
    /// </summary>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new FormItem
        {
            OwnerForm = this
        };
    }

    /// <summary>
    /// Returns whether the item needs a container: every item does, except one that is itself a
    /// <see cref="FormItem" />, which is its own container.
    /// </summary>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<FormItem>(item, out recycleKey);
    }
    
    /// <summary>
    /// After the base class prepared the container, binds the layout and style settings of the form to the item and
    /// calls <see cref="PrepareFormItem" />. An attached item takes part in values, validation and submit.
    /// An override must call the base method, otherwise the item stays detached from the form.
    /// </summary>
    /// <param name="container">
    /// The container created for the item. It must be a <see cref="FormItem" />.
    /// </param>
    /// <param name="item">
    /// The item the container was created for.
    /// </param>
    /// <param name="index">
    /// The index of the item in the items of the form.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="container" /> is not a <see cref="FormItem" />.
    /// </exception>
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is FormItem formItem)
        {
            formItem[!FormItem.SizeTypeProperty]                        = this[!SizeTypeProperty];
            formItem[!FormItem.IsMotionEnabledProperty]                 = this[!IsMotionEnabledProperty];
            formItem[!FormItem.ErrorMessageForegroundProperty]          = this[!ErrorMessageForegroundProperty];
            formItem[!FormItem.WarningMessageForegroundProperty]        = this[!WarningMessageForegroundProperty];
            formItem[!FormItem.IsShowColonProperty]                     = this[!IsShowColonProperty];
            formItem[!FormItem.RequiredMarkProperty]                    = this[!RequiredMarkProperty];
            formItem[!FormItem.LabelWrappingProperty]                   = this[!LabelWrappingProperty];
            formItem[!FormItem.StyleVariantProperty]                    = this[!StyleVariantProperty];
            formItem[!FormItem.CustomRequireMarkProperty]               = this[!CustomRequireMarkProperty];
            formItem[!FormItem.CustomRequireMarkTemplateProperty]       = this[!CustomRequireMarkTemplateProperty];
            formItem[!FormItem.CustomOptionalMarkProperty]              = this[!CustomOptionalMarkProperty];
            formItem[!FormItem.CustomOptionalMarkTemplateProperty]      = this[!CustomOptionalMarkTemplateProperty];
            formItem[!FormItem.LabelColInfoProperty]                    = this[!LabelColInfoProperty];
            formItem[!FormItem.WrapperColInfoProperty]                  = this[!WrapperColInfoProperty];
            formItem[!FormItem.FeedbackTemplateProperty]                = this[!FeedbackTemplateProperty];
            formItem[!FormItem.IsShowItemDeleteButtonProperty]          = this[!IsShowItemDeleteButtonProperty];
            formItem[!FormItem.IsEffectiveShowItemDeleteButtonProperty] = this[!IsEffectiveShowItemDeleteButtonProperty];
            formItem[!FormItem.ItemDeleteButtonIconTemplateProperty] = this[!ItemDeleteButtonIconProperty];
            formItem[!FormItem.FormLayoutProperty] = this[!FormLayoutProperty];
            formItem[!FormItem.IsHideItemLabelProperty] = this[!IsHideItemLabelProperty];
    
            PrepareFormItem(formItem, item, index);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type FormItem.");
        }
    }

    /// <summary>
    /// Called at the end of <see cref="PrepareContainerForItemOverride" />, after the settings of the form were bound
    /// to the item. The base method does nothing.
    /// </summary>
    /// <param name="formItem">
    /// The form item created for the item, already bound to the form.
    /// </param>
    /// <param name="item">
    /// The item the form item was created for.
    /// </param>
    /// <param name="index">
    /// The index of the item in the items of the form.
    /// </param>
    protected virtual void PrepareFormItem(FormItem formItem, object? item, int index)
    {
    }
    
    /// <summary>
    /// Starts a validation of every item on the dispatcher and returns before it finishes. Its result is visible
    /// through the item states, <see cref="IsFormValid" /> and <see cref="Validated" />.
    /// A run canceled by a newer validation that is not silent, by <see cref="AbstractForm.CancelValidation" /> or by
    /// detaching the form ends without raising an event.
    /// </summary>
    public new void Validate() => base.Validate();

    /// <summary>
    /// Validates every item and shows the result on the items. The task completes when the validation finished.
    /// </summary>
    /// <param name="cancellationToken">
    /// Cancels the run.
    /// </param>
    /// <returns>
    /// <see cref="FormValidateResult.Error" /> when an item has an error; a warning does not count as an error.
    /// Otherwise <see cref="FormValidateResult.Success" />.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// The run was canceled.
    /// </exception>
    public new Task<FormValidateResult> ValidateAsync(CancellationToken cancellationToken) => base.ValidateAsync(cancellationToken);

    /// <summary>
    /// Writes the values into the items whose field name is a key of <paramref name="formValues" />, then refreshes
    /// <see cref="Values" />. Keys without an item and items without a key are left alone.
    /// The writes do not count as user input: they start no validation and keep the errors that <c>SetFieldErrors</c>
    /// set on the form.
    /// </summary>
    /// <param name="formValues">
    /// The values keyed by field name.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="formValues" /> is <see langword="null" />.
    /// </exception>
    public new void SetFormValues(IFormValues formValues) => base.SetFormValues(formValues);

    /// <summary>
    /// Validates every item on the dispatcher and returns before the validation finishes. When the validation
    /// passes, <see cref="Validated" /> is raised, <see cref="NotifySubmit" /> is called and
    /// <see cref="Submitted" /> is raised with the values that were validated. When it fails,
    /// <see cref="Validated" /> and <see cref="AbstractForm.SubmitFailed" /> are raised, and the first item in error
    /// is revealed when <see cref="IsScrollToFirstErrorEnabled" /> is enabled.
    /// A submission canceled by <see cref="AbstractForm.CancelValidation" />, by a newer validation that is not silent
    /// or by detaching the form raises neither <see cref="Submitted" /> nor <see cref="AbstractForm.SubmitFailed" />.
    /// </summary>
    public new void Submit() => base.Submit();

    /// <inheritdoc />
    protected override void NotifySubmit(FormValues values) => base.NotifySubmit(values);

    /// <summary>
    /// Cancels the pending validations and restores every item to its initial value: the value from
    /// <see cref="InitialValues" /> when the item's field name is a key there, otherwise the item's own initial
    /// value; an item that has neither is cleared. The validation state of the items and the errors set with
    /// <c>SetFieldErrors</c> are cleared.
    /// Then <see cref="Values" /> is refreshed, <see cref="NotifyReset" /> is called, <see cref="IsFormValid" /> is
    /// set to <see langword="false" /> and <see cref="ResetCompleted" /> is raised.
    /// </summary>
    public new void Reset() => base.Reset();

    /// <inheritdoc />
    protected override void NotifyReset() => base.NotifyReset();

    /// <summary>
    /// After the base class applied the template, copies <see cref="LabelAlign" />,
    /// <see cref="IsValidateFeedbackEnabled" />, <see cref="ValidateTrigger" /> and the layout derived from
    /// <see cref="FormLayout" /> to every item, and updates the orientation of the items for <see cref="FormLayout" />.
    /// An override must call the base method.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        SyncConfigToItems();
        ConfigureFormLayoutOrientation();
    }

    /// <summary>
    /// After the base class handled the change, copies <see cref="LabelAlign" />,
    /// <see cref="IsValidateFeedbackEnabled" />, <see cref="ValidateTrigger" /> and the layout derived from
    /// <see cref="FormLayout" /> to every item when <see cref="LabelAlign" />, <see cref="FormLayout" /> or
    /// <see cref="ValidateTrigger" /> changed; updates the orientation of the items when <see cref="FormLayout" />
    /// changed; and re-evaluates the delete buttons when <see cref="IsShowItemDeleteButton" /> changed.
    /// An override must call the base method, or the items keep the old value.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == LabelAlignProperty ||
            change.Property == FormLayoutProperty ||
            change.Property == ValidateTriggerProperty)
        {
            SyncConfigToItems();
        }

        if (change.Property == FormLayoutProperty)
        {
            ConfigureFormLayoutOrientation();
        }
        else if (change.Property == IsShowItemDeleteButtonProperty)
        {
            ConfigureShowItemDeleteButton();
        }
    }

    private void SyncConfigToItems()
    {
        foreach (var item in Items)
        {
            if (item is FormItem formItem)
            {
                SyncConfigToItem(formItem);
            }
        }
    }

    private void SyncConfigToItem(FormItem formItem)
    {
        formItem.SetValue(FormItem.LabelAlignProperty, LabelAlign, BindingPriority.Style);
        formItem.SetValue(FormItem.IsValidateFeedbackEnabledProperty, IsValidateFeedbackEnabled, BindingPriority.Style);
        formItem.SetValue(FormItem.ValidateTriggerProperty, ValidateTrigger, BindingPriority.Style);
        
        if (FormLayout == FormLayout.Horizontal || FormLayout == FormLayout.Inline)
        {
            formItem.SetValue(FormItem.LayoutProperty, FormItemLayout.Horizontal, BindingPriority.Style);
        }
        else
        {
            formItem.SetValue(FormItem.LayoutProperty, FormItemLayout.Vertical, BindingPriority.Style);
        }
    }

    private void ConfigureFormLayoutOrientation()
    {
        FormLayoutOrientation = (FormLayout == FormLayout.Horizontal || FormLayout == FormLayout.Vertical) ? Orientation.Vertical : Orientation.Horizontal;
    }

    private void HandleSubmitButtonClick(RoutedEventArgs args)
    {
        Submit();
        args.Handled = true;
    }

    private void HandleResetButtonClick(RoutedEventArgs args)
    {
        Reset();
        args.Handled = true;
    }

    /// <inheritdoc />
    protected override void OnLoaded(RoutedEventArgs e) => base.OnLoaded(e);

    /// <summary>
    /// Removes the form item from the items of the form. With <see cref="ItemsControl.ItemsSource" /> set to
    /// <see langword="null" />, the item is removed from <see cref="ItemsControl.Items" />. With an
    /// <see cref="ItemsControl.ItemsSource" /> that is a writable list of variable size, the entry at the item's
    /// index is removed from that list. Nothing happens for a read-only or fixed-size list, or for an item the form
    /// does not contain.
    /// </summary>
    /// <param name="formItem">
    /// The form item to remove.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="formItem" /> is <see langword="null" />.
    /// </exception>
    public new void DeleteFormItem(IFormItem formItem) => base.DeleteFormItem(formItem);

    private void HandleFormItemDelete(RoutedEventArgs args)
    {
        if (args.Source is FormItem formItem)
        {
            DeleteFormItem(formItem);
        }
    }

    private void ConfigureShowItemDeleteButton()
    {
        var count = 0;
        foreach (var item in LogicalChildren)
        {
            if (item is FormItem formItem && formItem.IsValueItem)
            {
                count++;
            }
        }
        IsEffectiveShowItemDeleteButton = IsShowItemDeleteButton && count > MinItemCount;
    }
}
