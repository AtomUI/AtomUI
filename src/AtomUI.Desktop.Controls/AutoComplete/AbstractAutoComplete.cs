using AtomUI.Animations;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using AtomUI.Controls;
using AtomUI.Controls.AsyncLoad;
using AtomUI.Controls.Utils;
using AtomUI.Data;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Input;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using AvaloniaTextBox = Avalonia.Controls.TextBox;
using ItemCollection = AtomUI.Collections.ItemCollection;

/// <summary>
/// Side of the input on which the candidate drop-down is placed.
/// </summary>
public enum AutoCompletePlacementMode
{
    /// <summary>
    /// Places the candidate drop-down above the input, aligned with its left edge.
    /// </summary>
    Top,
    /// <summary>
    /// Places the candidate drop-down below the input, aligned with its left edge. This is the default placement.
    /// </summary>
    Bottom
}

/// <summary>
/// Base class for input controls that match the typed text against a list of options and show the matches
/// in a candidate drop-down. The template supplies the text box, the popup and the candidate list.
/// </summary>
[PseudoClasses(AutoCompletePseudoClass.CandidatePopupOpen)]
public abstract class AbstractAutoComplete : TemplatedControl,
                                             ICustomizableSizeTypeAware,
                                             IMotionAwareControl,
                                             IFormItemAware,
                                             IInputControlStatusAware,
                                             IInputControlStyleVariantAware,
                                             IFormItemFeedbackAware
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="OpenMotion" /> property.
    /// </summary>
    public static readonly StyledProperty<AbstractMotion?> OpenMotionProperty =
        Popup.OpenMotionProperty.AddOwner<AbstractAutoComplete>();

    /// <summary>
    /// Identifies the <see cref="CloseMotion" /> property.
    /// </summary>
    public static readonly StyledProperty<AbstractMotion?> CloseMotionProperty =
        Popup.CloseMotionProperty.AddOwner<AbstractAutoComplete>();

    /// <summary>
    /// Identifies the <see cref="ClearIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> ClearIconProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, PathIcon?>(nameof(ClearIcon));

    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<AbstractAutoComplete>();

    /// <summary>
    /// Identifies the <see cref="CaretIndex" /> property.
    /// </summary>
    public static readonly StyledProperty<int> CaretIndexProperty =
        AvaloniaTextBox.CaretIndexProperty.AddOwner<AbstractAutoComplete>(new(
            defaultValue: 0,
            defaultBindingMode: BindingMode.TwoWay));

    /// <summary>
    /// Identifies the <see cref="MinimumPrefixLength" /> property.
    /// </summary>
    public static readonly StyledProperty<int> MinimumPrefixLengthProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, int>(
            nameof(MinimumPrefixLength), 1,
            validate: IsValidMinimumPrefixLength);

    /// <summary>
    /// Identifies the <see cref="AsyncLoadDebounce" /> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> AsyncLoadDebounceProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, TimeSpan>(
            nameof(AsyncLoadDebounce),
            TimeSpan.Zero,
            validate: IsValidAsyncLoadDebounce);

    /// <summary>
    /// Identifies the <see cref="AsyncLoadTimeout" /> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> AsyncLoadTimeoutProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, TimeSpan>(
            nameof(AsyncLoadTimeout),
            TimeSpan.FromSeconds(10));

    /// <summary>
    /// Identifies the <see cref="MaxLength" /> property.
    /// </summary>
    public static readonly StyledProperty<int> MaxLengthProperty =
        AvaloniaTextBox.MaxLengthProperty.AddOwner<AbstractAutoComplete>();

    /// <summary>
    /// Identifies the <see cref="Value" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, string?>(nameof(Value));

    /// <summary>
    /// Identifies the <see cref="IsAllowClear" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsAllowClearProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, bool>(nameof(IsAllowClear));

    /// <summary>
    /// Identifies the <see cref="IsAutoFocus" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsAutoFocusProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, bool>(nameof(IsAutoFocus));

    /// <summary>
    /// Identifies the <see cref="DefaultValue" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> DefaultValueProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, string?>(nameof(DefaultValue));

    /// <summary>
    /// Identifies the <see cref="DisplayCandidateCount" /> property.
    /// </summary>
    public static readonly StyledProperty<int> DisplayCandidateCountProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, int>(nameof (DisplayCandidateCount), 10);

    /// <summary>
    /// Identifies the <see cref="ContentLeftAddOn" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ContentLeftAddOnProperty =
        AddOnDecoratedBox.ContentLeftAddOnProperty.AddOwner<AbstractAutoComplete>();

    /// <summary>
    /// Identifies the <see cref="ContentLeftAddOnTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ContentLeftAddOnTemplateProperty =
        AddOnDecoratedBox.ContentLeftAddOnTemplateProperty.AddOwner<AbstractAutoComplete>();

    /// <summary>
    /// Identifies the <see cref="ContentRightAddOn" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ContentRightAddOnProperty =
        AddOnDecoratedBox.ContentRightAddOnProperty.AddOwner<AbstractAutoComplete>();

    /// <summary>
    /// Identifies the <see cref="ContentRightAddOnTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ContentRightAddOnTemplateProperty =
        AddOnDecoratedBox.ContentRightAddOnTemplateProperty.AddOwner<AbstractAutoComplete>();

    /// <summary>
    /// Identifies the <see cref="StyleVariant" /> property.
    /// </summary>
    public static readonly StyledProperty<InputControlStyleVariant> StyleVariantProperty =
        InputControlStyleVariantProperty.StyleVariantProperty.AddOwner<AbstractAutoComplete>();

    /// <summary>
    /// Identifies the <see cref="Status" /> property.
    /// </summary>
    public static readonly StyledProperty<InputControlStatus> StatusProperty =
        InputControlStatusProperty.StatusProperty.AddOwner<AbstractAutoComplete>();

    /// <summary>
    /// Identifies the <see cref="PlaceholderText" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> PlaceholderTextProperty =
        TextBox.PlaceholderTextProperty.AddOwner<AbstractAutoComplete>();

    /// <summary>
    /// Identifies the <see cref="PlaceholderForeground" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> PlaceholderForegroundProperty =
        TextBox.PlaceholderForegroundProperty.AddOwner<AbstractAutoComplete>();

    /// <summary>
    /// Identifies the <see cref="OptionsSource" /> property.
    /// </summary>
    public static readonly StyledProperty<IEnumerable<IAutoCompleteOption>?> OptionsSourceProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, IEnumerable<IAutoCompleteOption>?>(nameof(OptionsSource));

    /// <summary>
    /// Identifies the <see cref="OptionTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> OptionTemplateProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, IDataTemplate?>(nameof(OptionTemplate));

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractAutoComplete>();

    /// <summary>
    /// Identifies the <see cref="IsLoading" /> property.
    /// </summary>
    public static readonly DirectProperty<AbstractAutoComplete, bool> IsLoadingProperty =
        AvaloniaProperty.RegisterDirect<AbstractAutoComplete, bool>(
            nameof(IsLoading),
            o => o.IsLoading);

    /// <summary>
    /// Identifies the <see cref="IsReadOnly" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaTextBox.IsReadOnlyProperty.AddOwner<AbstractAutoComplete>();

    /// <summary>
    /// Identifies the <see cref="MaxDropDownHeight" /> property.
    /// </summary>
    public static readonly StyledProperty<double> MaxDropDownHeightProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, double>(
            nameof(MaxDropDownHeight),
            double.PositiveInfinity,
            validate: IsValidMaxDropDownHeight);

    /// <summary>
    /// Identifies the <see cref="Filter" /> property.
    /// </summary>
    public static readonly StyledProperty<IValueFilter?> FilterProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, IValueFilter?>(nameof(Filter));

    /// <summary>
    /// Identifies the <see cref="FilterValue" /> property.
    /// </summary>
    public static readonly DirectProperty<AbstractAutoComplete, string?> FilterValueProperty =
        AvaloniaProperty.RegisterDirect<AbstractAutoComplete, string?>(
            nameof(FilterValue),
            o => o.FilterValue,
            unsetValue: string.Empty);

    /// <summary>
    /// Identifies the <see cref="FilterValueSelector" /> property.
    /// </summary>
    public static readonly StyledProperty<DefaultFilterValueSelector?> FilterValueSelectorProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, DefaultFilterValueSelector?>(
            nameof(FilterValueSelector));

    /// <summary>
    /// Identifies the <see cref="OptionsAsyncLoader" /> property.
    /// </summary>
    public static readonly StyledProperty<ICompleteOptionsAsyncLoader?> OptionsAsyncLoaderProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, ICompleteOptionsAsyncLoader?>(nameof(OptionsAsyncLoader));

    /// <summary>
    /// Identifies the <see cref="IsDropDownOpen" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, bool>(
            nameof(IsDropDownOpen));

    /// <summary>
    /// Identifies the <see cref="IsPopupPinnedOpen" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        Popup.IsPopupPinnedOpenProperty.AddOwner<AbstractAutoComplete>();

    /// <summary>
    /// Identifies the <see cref="Placement" /> property.
    /// </summary>
    public static readonly StyledProperty<AutoCompletePlacementMode> PlacementProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, AutoCompletePlacementMode>(nameof(Placement), AutoCompletePlacementMode.Bottom);

    /// <summary>
    /// Identifies the <see cref="IsCompletionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsCompletionEnabledProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, bool>(
            nameof(IsCompletionEnabled));

    /// <summary>
    /// Identifies the <see cref="SelectedOption" /> property.
    /// </summary>
    public static readonly StyledProperty<IAutoCompleteOption?> SelectedOptionProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, IAutoCompleteOption?>(
            nameof(SelectedOption),
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);

    /// <summary>
    /// Identifies the <see cref="ClearSelectionOnLostFocus" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> ClearSelectionOnLostFocusProperty =
        AvaloniaTextBox.ClearSelectionOnLostFocusProperty.AddOwner<AbstractAutoComplete>();

    /// <summary>
    /// Identifies the <see cref="IsPopupMatchSelectWidth" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsPopupMatchSelectWidthProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, bool>(nameof(IsPopupMatchSelectWidth), true);

    /// <summary>
    /// Identifies the <see cref="ShouldUseOverlayPopup" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShouldUseOverlayPopupProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, bool>(nameof(ShouldUseOverlayPopup), true);

    /// <summary>
    /// Motion that plays when the candidate popup opens. It is handed to the popup of the control and is used
    /// only while <see cref="IsMotionEnabled" /> is <see langword="true" />. The theme sets it to
    /// <c>PopupMotionDefaults.SelectOpen</c>.
    /// </summary>
    public AbstractMotion? OpenMotion
    {
        get => GetValue(OpenMotionProperty);
        set => SetValue(OpenMotionProperty, value);
    }

    /// <summary>
    /// Motion that plays when the candidate popup closes. It is handed to the popup of the control and is used
    /// only while <see cref="IsMotionEnabled" /> is <see langword="true" />. The theme sets it to
    /// <c>PopupMotionDefaults.SelectClose</c>.
    /// </summary>
    public AbstractMotion? CloseMotion
    {
        get => GetValue(CloseMotionProperty);
        set => SetValue(CloseMotionProperty, value);
    }

    /// <summary>
    /// Icon shown on the clear button in the suffix area of the input. The button is rendered only while
    /// <see cref="IsAllowClear" /> is <see langword="true" />.
    /// </summary>
    public PathIcon? ClearIcon
    {
        get => GetValue(ClearIconProperty);
        set => SetValue(ClearIconProperty, value);
    }

    /// <summary>
    /// Size preset of the input area: small, medium or large. The value is handed to the input area of the
    /// template.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    /// <summary>
    /// Index of the caret in the text box, in characters. It is bound to the text box of the template in both
    /// directions, so it follows the caret and moves the caret when it is set.
    /// </summary>
    public int CaretIndex
    {
        get => GetValue(CaretIndexProperty);
        set => SetValue(CaretIndexProperty, value);
    }

    /// <summary>
    /// Number of characters that must be typed before the options are populated. The default is 1. A value below
    /// 0 disables the population; values below -1 are rejected.
    /// </summary>
    public int MinimumPrefixLength
    {
        get => GetValue(MinimumPrefixLengthProperty);
        set => SetValue(MinimumPrefixLengthProperty, value);
    }

    /// <summary>
    /// Delay between the last change of the text and the population of the options. The default,
    /// <see cref="TimeSpan.Zero" />, populates immediately. The delay is applied whether or not an
    /// <see cref="OptionsAsyncLoader" /> is set.
    /// </summary>
    public TimeSpan AsyncLoadDebounce
    {
        get => GetValue(AsyncLoadDebounceProperty);
        set => SetValue(AsyncLoadDebounceProperty, value);
    }

    /// <summary>
    /// Time limit for one load of <see cref="OptionsAsyncLoader" />. The default is 10 seconds. A load that
    /// exceeds it is dropped: the options stay unchanged and <see cref="OptionsLoaded" /> receives a result
    /// with the status code <c>Timeout</c>.
    /// </summary>
    public TimeSpan AsyncLoadTimeout
    {
        get => GetValue(AsyncLoadTimeoutProperty);
        set => SetValue(AsyncLoadTimeoutProperty, value);
    }

    /// <summary>
    /// Intended as the maximum number of characters that can be entered in the text box. Neither the control nor its
    /// themes hand this value to the text box, so setting it has no effect.
    /// </summary>
    public int MaxLength
    {
        get => GetValue(MaxLengthProperty);
        set => SetValue(MaxLengthProperty, value);
    }

    /// <summary>
    /// The text of the input. It follows the text typed in the text box and is set from the content, header or
    /// key of <see cref="SelectedOption" />. Setting it writes the text into the text box and populates the
    /// candidates once its length reaches <see cref="MinimumPrefixLength" />.
    /// </summary>
    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    /// Shows a clear button in the suffix area of the input. The value is handed to the input area of the template.
    /// </summary>
    public bool IsAllowClear
    {
        get => GetValue(IsAllowClearProperty);
        set => SetValue(IsAllowClearProperty, value);
    }

    /// <summary>
    /// Intended to focus the control automatically. No code reads this value, so setting it has no effect.
    /// </summary>
    public bool IsAutoFocus
    {
        get => GetValue(IsAutoFocusProperty);
        set => SetValue(IsAutoFocusProperty, value);
    }

    /// <summary>
    /// Intended as the initial value of the control. No code reads this value, so setting it has no effect.
    /// </summary>
    public string? DefaultValue
    {
        get => GetValue(DefaultValueProperty);
        set => SetValue(DefaultValueProperty, value);
    }

    /// <summary>
    /// Number of candidate rows that set the height of the drop-down, in addition to the padding of the popup.
    /// The default is 10.
    /// </summary>
    public int DisplayCandidateCount
    {
        get => GetValue(DisplayCandidateCountProperty);
        set => SetValue(DisplayCandidateCountProperty, value);
    }

    /// <summary>
    /// Content shown in the prefix area, to the left of the text.
    /// </summary>
    public object? ContentLeftAddOn
    {
        get => GetValue(ContentLeftAddOnProperty);
        set => SetValue(ContentLeftAddOnProperty, value);
    }

    /// <summary>
    /// Template used to render <see cref="ContentLeftAddOn" />.
    /// </summary>
    public IDataTemplate? ContentLeftAddOnTemplate
    {
        get => GetValue(ContentLeftAddOnTemplateProperty);
        set => SetValue(ContentLeftAddOnTemplateProperty, value);
    }

    /// <summary>
    /// Content shown in the suffix area, to the right of the text.
    /// </summary>
    public object? ContentRightAddOn
    {
        get => GetValue(ContentRightAddOnProperty);
        set => SetValue(ContentRightAddOnProperty, value);
    }

    /// <summary>
    /// Template used to render <see cref="ContentRightAddOn" />.
    /// </summary>
    public IDataTemplate? ContentRightAddOnTemplate
    {
        get => GetValue(ContentRightAddOnTemplateProperty);
        set => SetValue(ContentRightAddOnTemplateProperty, value);
    }

    /// <summary>
    /// Visual style of the input area: outlined, filled or borderless. The value is handed to the input area of
    /// the template.
    /// </summary>
    public InputControlStyleVariant StyleVariant
    {
        get => GetValue(StyleVariantProperty);
        set => SetValue(StyleVariantProperty, value);
    }

    /// <summary>
    /// Status of the input, such as error or warning, that selects the status coloring of the input area. The
    /// value is handed to the input area of the template.
    /// </summary>
    public InputControlStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    /// <summary>
    /// Hint text shown in the text box while it is empty.
    /// </summary>
    public string? PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    /// <summary>
    /// Foreground of the placeholder text. The theme binds it to the <c>ColorTextPlaceholder</c> token.
    /// </summary>
    public IBrush? PlaceholderForeground
    {
        get => GetValue(PlaceholderForegroundProperty);
        set => SetValue(PlaceholderForegroundProperty, value);
    }

    /// <summary>
    /// The options that are matched against the typed text. Setting it replaces the items of
    /// <see cref="Options" />. When a load of <see cref="OptionsAsyncLoader" /> succeeds, the loaded data is
    /// assigned here.
    /// </summary>
    public IEnumerable<IAutoCompleteOption>? OptionsSource
    {
        get => GetValue(OptionsSourceProperty);
        set => SetValue(OptionsSourceProperty, value);
    }

    /// <summary>
    /// Template that renders one candidate in the drop-down. Its data type is taken from the items of
    /// <see cref="OptionsSource" />. The theme sets a default template.
    /// </summary>
    [InheritDataTypeFromItems(nameof(OptionsSource))]
    public IDataTemplate? OptionTemplate
    {
        get => GetValue(OptionTemplateProperty);
        set => SetValue(OptionTemplateProperty, value);
    }

    /// <summary>
    /// Enables the open and close motions of the candidate popup. The theme binds it to the <c>EnableMotion</c>
    /// token.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    private bool _isLoading;

    /// <summary>
    /// Whether a local population of the options is in progress. It is set to <see langword="true" /> before
    /// <see cref="Populating" /> is raised and back to <see langword="false" /> before <see cref="Populated" />
    /// is raised. A load of <see cref="OptionsAsyncLoader" /> does not set it to <see langword="true" />, because
    /// <see cref="Populating" /> is not raised for that load. The property is read-only for callers.
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        internal set => SetAndRaise(IsLoadingProperty, ref _isLoading, value);
    }

    /// <summary>
    /// Makes the text of the input uneditable. While it is <see langword="true" />, the text box of the template
    /// is not focusable.
    /// </summary>
    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <summary>
    /// Maximum height of the drop-down, in device-independent pixels. The default,
    /// <c>double.PositiveInfinity</c>, imposes no limit; values below 0 are rejected. The control does not read
    /// this value: the height of the drop-down follows <see cref="DisplayCandidateCount" /> instead.
    /// </summary>
    public double MaxDropDownHeight
    {
        get => GetValue(MaxDropDownHeightProperty);
        set => SetValue(MaxDropDownHeightProperty, value);
    }

    /// <summary>
    /// Filter that decides which options match the typed text. It receives the text of an option, taken from
    /// <see cref="FilterValueSelector" /> or from the option itself. When it is <see langword="null" /> when
    /// the control is initialized, a StartsWith filter is installed. A <see langword="null" /> value afterwards
    /// shows all options.
    /// </summary>
    public IValueFilter? Filter
    {
        get => GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }

    private string? _filterValue;

    private bool _allowWrite;

    /// <summary>
    /// The text that the options are currently matched against. It is taken from <see cref="Value" /> each time
    /// the options are populated. The property is read-only: setting it throws an
    /// <see cref="InvalidOperationException" />.
    /// </summary>
    public string? FilterValue
    {
        get => _filterValue;
        private set
        {
            try
            {
                _allowWrite = true;
                SetAndRaise(FilterValueProperty, ref _filterValue, value);
            }
            finally
            {
                _allowWrite = false;
            }
        }
    }

    /// <summary>
    /// Selects the text that <see cref="Filter" /> compares for an option. When it is <see langword="null" />,
    /// the header, the content or the key of the option is used, in that order.
    /// </summary>
    public DefaultFilterValueSelector? FilterValueSelector
    {
        get => GetValue(FilterValueSelectorProperty);
        set => SetValue(FilterValueSelectorProperty, value);
    }

    /// <summary>
    /// Loads the options asynchronously. While it is set, each population asks the loader for the options that
    /// match the current <see cref="FilterValue" />, instead of matching <see cref="OptionsSource" /> locally.
    /// Every finished load raises <see cref="OptionsLoaded" />; a successful one assigns the loaded data to
    /// <see cref="OptionsSource" />.
    /// </summary>
    public ICompleteOptionsAsyncLoader? OptionsAsyncLoader
    {
        get => GetValue(OptionsAsyncLoaderProperty);
        set => SetValue(OptionsAsyncLoaderProperty, value);
    }

    /// <summary>
    /// Whether the candidate drop-down is open. Setting it to <see langword="true" /> repopulates the candidates
    /// and opens the popup when the text reaches <see cref="MinimumPrefixLength" /> and at least one candidate
    /// remains. Setting it to <see langword="false" /> raises <see cref="DropDownClosing" /> first.
    /// </summary>
    public bool IsDropDownOpen
    {
        get => GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    /// <summary>
    /// Keeps the candidate popup open. While it is <see langword="true" />, a request to close the drop-down is
    /// reverted and light dismiss cannot close the popup. Setting it, or attaching the control with it set, opens
    /// the drop-down; only detaching the control closes it. The Gallery semantic part preview uses it to keep
    /// the popup parts visible.
    /// </summary>
    public bool IsPopupPinnedOpen
    {
        get => GetValue(IsPopupPinnedOpenProperty);
        set => SetValue(IsPopupPinnedOpenProperty, value);
    }

    /// <summary>
    /// Side of the input on which the candidate drop-down opens. The default is
    /// <see cref="AutoCompletePlacementMode.Bottom" />.
    /// </summary>
    public AutoCompletePlacementMode Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    /// <summary>
    /// Completes the typed text inline while the user types at the end of the text: the first candidate that
    /// starts with the typed text is written into the text box, the added suffix is selected and
    /// <see cref="SelectedOption" /> follows that candidate. The default is <see langword="false" />.
    /// </summary>
    public bool IsCompletionEnabled
    {
        get => GetValue(IsCompletionEnabledProperty);
        set => SetValue(IsCompletionEnabledProperty, value);
    }

    /// <summary>
    /// The option that the text of the control stands for. Setting it writes the content, header or key of the
    /// option into <see cref="Value" />. Inline completion and committing a candidate update it.
    /// </summary>
    public IAutoCompleteOption? SelectedOption
    {
        get => GetValue(SelectedOptionProperty);
        set => SetValue(SelectedOptionProperty, value);
    }

    /// <summary>
    /// Removes the selection of the text box when the control loses the keyboard focus. The selection is kept
    /// while the context menu of the control or of the text box is open.
    /// </summary>
    public bool ClearSelectionOnLostFocus
    {
        get => GetValue(ClearSelectionOnLostFocusProperty);
        set => SetValue(ClearSelectionOnLostFocusProperty, value);
    }

    /// <summary>
    /// Applies the width of the control to the drop-down. The default is <see langword="true" />. While it is
    /// <see langword="false" />, the width of the control is not applied.
    /// </summary>
    public bool IsPopupMatchSelectWidth
    {
        get => GetValue(IsPopupMatchSelectWidthProperty);
        set => SetValue(IsPopupMatchSelectWidthProperty, value);
    }

    /// <summary>
    /// Shows the candidate popup in the overlay layer of the window. The default is <see langword="true" />.
    /// </summary>
    public bool ShouldUseOverlayPopup
    {
        get => GetValue(ShouldUseOverlayPopupProperty);
        set => SetValue(ShouldUseOverlayPopupProperty, value);
    }

    /// <summary>
    /// A read-only view of the items in <see cref="Options" /> before they are matched against the typed text.
    /// </summary>
    public ItemsSourceView OptionsView => ItemsSourceView.GetOrCreate(_options);

    /// <summary>
    /// The options of the control. Items written as XAML content are collected here, and setting
    /// <see cref="OptionsSource" /> replaces them with the items of that source. Every change of the collection
    /// matches the options again.
    /// </summary>
    [Content]
    public ItemCollection Options => _options;

    /// <summary>
    /// The text box part of the template. <see cref="OnApplyTemplate" /> assigns it. Assigning a text box
    /// subscribes to changes of its text and writes <see cref="Value" /> into it when a value is already set.
    /// </summary>
    protected AvaloniaTextBox? TextInputBox
    {
        get => _textInputBox;
        set
        {
            _textInputBoxSubscriptions?.Dispose();
            _textInputBox = value;

            // Attach handlers
            if (_textInputBox != null)
            {
                _textInputBoxSubscriptions =
                    _textInputBox.GetObservable(AvaloniaTextBox.TextProperty)
                            .Skip(1)
                            .Subscribe(_ => HandleTextInputBoxTextChanged());

                if (Value != null)
                {
                    UpdateValue(Value);
                }
            }
        }
    }

    /// <summary>
    /// The candidate list part of the popup. <see cref="OnApplyTemplate" /> assigns it. Assigning a list wires
    /// its commit and cancel handlers and feeds it the filtered candidates.
    /// </summary>
    protected ICandidateList? CandidateList
    {
        get => _candidateList;
        set
        {
            if (_candidateList != null)
            {
                _candidateList.Commit      -= HandleCandidateListComplete;
                _candidateList.Cancel      -= HandleCandidateListCanceled;
                _candidateList.ItemsSource =  null;
            }

            _candidateList = value;

            if (_candidateList != null)
            {
                _candidateList.Commit      += HandleCandidateListComplete;
                _candidateList.Cancel      += HandleCandidateListCanceled;
                _candidateList.ItemsSource =  _view;
            }
        }
    }

    #endregion

    #region 公共事件定义

    /// <summary>
    /// Identifies the <see cref="ValueChanged" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<CompleteValueChangedEventArgs> ValueChangedEvent =
        RoutedEvent.Register<AbstractAutoComplete, CompleteValueChangedEventArgs>(
            nameof(ValueChanged),
            RoutingStrategies.Bubble);

    /// <summary>
    /// Identifies the <see cref="SelectionChanged" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<SelectionChangedEventArgs> SelectionChangedEvent =
        RoutedEvent.Register<SelectionChangedEventArgs>(
            nameof(SelectionChanged),
            RoutingStrategies.Bubble,
            typeof(AbstractAutoComplete));

    /// <summary>
    /// Raised after <see cref="Value" /> was updated by typing, by selecting an option or by setting the
    /// property, and after a set value was written into the text box. The event bubbles up the element tree.
    /// </summary>
    public event EventHandler<CompleteValueChangedEventArgs>? ValueChanged
    {
        add => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }

    /// <summary>
    /// Raised after <see cref="SelectedOption" /> changed and the text was updated from the new option. The
    /// event args carry the previous option as removed and the new one as added. The event bubbles up the tree.
    /// </summary>
    public event EventHandler<SelectionChangedEventArgs> SelectionChanged
    {
        add => AddHandler(SelectionChangedEvent, value);
        remove => RemoveHandler(SelectionChangedEvent, value);
    }

    /// <summary>
    /// Raised when a load of <see cref="OptionsAsyncLoader" /> has finished, before a successful result is
    /// assigned to <see cref="OptionsSource" />. A failed, canceled or timed-out load raises it with a result
    /// that carries the status code <c>Timeout</c>, <c>Cancelled</c> or <c>Unknown</c>. A load that was
    /// skipped raises nothing.
    /// </summary>
    public event EventHandler<CompleteOptionsLoadedEventArgs>? OptionsLoaded;

    /// <summary>
    /// Raised before the options are matched against the typed text, while <see cref="IsLoading" /> is
    /// <see langword="true" />. It is not raised while an <see cref="OptionsAsyncLoader" /> is set. Setting
    /// <c>Cancel</c> stops the population: the options are not matched and
    /// <see cref="Populated" /> is not raised.
    /// </summary>
    public event EventHandler<CompletePopulatingEventArgs>? Populating;

    /// <summary>
    /// Raised after the options were matched against the typed text and before the drop-down opens or closes
    /// and the text is completed. <see cref="IsLoading" /> is <see langword="false" /> at this point. It is
    /// raised on every population, with or without an <see cref="OptionsAsyncLoader" />.
    /// </summary>
    public event EventHandler<CompletePopulatedEventArgs>? Populated;

    /// <summary>
    /// Raised before the candidate popup opens, after <see cref="IsDropDownOpen" /> was set to
    /// <see langword="true" /> and the candidates were refreshed. Cancelling the event restores the previous
    /// value of <see cref="IsDropDownOpen" /> and keeps the popup closed.
    /// </summary>
    public event EventHandler<CancelEventArgs>? DropDownOpening;

    /// <summary>
    /// Raised after the candidate popup opened.
    /// </summary>
    public event EventHandler? DropDownOpened;

    /// <summary>
    /// Raised before the candidate popup closes, after <see cref="IsDropDownOpen" /> was set to
    /// <see langword="false" />. Cancelling the event sets <see cref="IsDropDownOpen" /> back to
    /// <see langword="true" /> and keeps the popup open.
    /// </summary>
    public event EventHandler<CancelEventArgs>? DropDownClosing;

    /// <summary>
    /// Raised after the candidate popup closed, whichever side requested the close.
    /// </summary>
    public event EventHandler? DropDownClosed;

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<CandidatePopupLayoutTokenResourceKeys?> PopupLayoutTokenResourceKeysProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, CandidatePopupLayoutTokenResourceKeys?>(nameof(PopupLayoutTokenResourceKeys));

    internal static readonly DirectProperty<AbstractAutoComplete, double> MaxPopupHeightProperty =
        AvaloniaProperty.RegisterDirect<AbstractAutoComplete, double>(
            nameof(MaxPopupHeight),
            o => o.MaxPopupHeight,
            (o, v) => o.MaxPopupHeight = v);

    internal static readonly DirectProperty<AbstractAutoComplete, double> EffectivePopupWidthProperty =
        AvaloniaProperty.RegisterDirect<AbstractAutoComplete, double>(
            nameof(EffectivePopupWidth),
            o => o.EffectivePopupWidth,
            (o, v) => o.EffectivePopupWidth = v);

    internal static readonly DirectProperty<AbstractAutoComplete, Thickness> PopupContentPaddingProperty =
        AvaloniaProperty.RegisterDirect<AbstractAutoComplete, Thickness>(nameof(PopupContentPadding),
            o => o.PopupContentPadding,
            (o, v) => o.PopupContentPadding = v);

    internal static readonly DirectProperty<AbstractAutoComplete, PlacementMode> PopupPlacementProperty =
        AvaloniaProperty.RegisterDirect<AbstractAutoComplete, PlacementMode>(nameof(PopupPlacement),
            o => o.PopupPlacement,
            (o, v) => o.PopupPlacement = v);

    internal static readonly StyledProperty<FormValidateFeedback?> FormFeedbackProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, FormValidateFeedback?>(nameof(FormFeedback));

    internal static readonly StyledProperty<FormValidateStatus> FormStatusProperty =
        InputControlState.FormStatusProperty.AddOwner<AbstractAutoComplete>();

    /// <summary>
    /// Background of the candidate popup frame, resolved on this control so it follows the control's Token scope
    /// (the frame lives inside the owner-transparent Popup, which also hands this control's owner to its content).
    /// </summary>
    internal static readonly StyledProperty<IBrush?> PopupBackgroundProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, IBrush?>(nameof(PopupBackground));

    internal IBrush? PopupBackground
    {
        get => GetValue(PopupBackgroundProperty);
        set => SetValue(PopupBackgroundProperty, value);
    }

    internal CandidatePopupLayoutTokenResourceKeys? PopupLayoutTokenResourceKeys
    {
        get => GetValue(PopupLayoutTokenResourceKeysProperty);
        set => SetValue(PopupLayoutTokenResourceKeysProperty, value);
    }

    private OwnerTokenSet? _popupLayoutTokens;
    private double _itemHeight;
    private double _maxPopupHeight;

    internal double MaxPopupHeight
    {
        get => _maxPopupHeight;
        set => SetAndRaise(MaxPopupHeightProperty, ref _maxPopupHeight, value);
    }

    private double _effectivePopupWidth;

    internal double EffectivePopupWidth
    {
        get => _effectivePopupWidth;
        set => SetAndRaise(EffectivePopupWidthProperty, ref _effectivePopupWidth, value);
    }

    private Thickness _popupContentPadding;

    internal Thickness PopupContentPadding
    {
        get => _popupContentPadding;
        set => SetAndRaise(PopupContentPaddingProperty, ref _popupContentPadding, value);
    }

    private PlacementMode _placementMode;

    internal PlacementMode PopupPlacement
    {
        get => _placementMode;
        set => SetAndRaise(PopupPlacementProperty, ref _placementMode, value);
    }

    internal FormValidateFeedback? FormFeedback
    {
        get => GetValue(FormFeedbackProperty);
        set => SetValue(FormFeedbackProperty, value);
    }

    internal FormValidateStatus FormStatus
    {
        get => GetValue(FormStatusProperty);
        private set => SetCurrentValue(FormStatusProperty, value);
    }

    #endregion

    #region 运行时字段

    private readonly ItemCollection _options = new();

    private protected DispatcherTimer? _delayTimer;

    private protected AvaloniaTextBox? _textInputBox;

    private protected IList<IAutoCompleteOption>? _view;

    private protected ICandidateList? _candidateList;

    private protected Popup? _popup;

    private protected bool _ignorePopupClose;

    private bool _cancelRequested;

    private bool _filterInAction;

    private int _ignoreValuePropertyChange;

    private bool _ignorePropertyChange;

    private bool _popupHasOpened;

    private int _textSelectionStart;

    private CompositeDisposable? _subscriptionsOnOpen;

    private readonly AsyncSearchLoadCoordinator<string?, CompleteOptionsLoadResult> _asyncLoadCoordinator = new();

    private IDisposable? _textInputBoxSubscriptions;

    private bool _userCalledPopulate;

    private bool _ignoreTextSelectionChange;

    private bool _skipSelectedOptionTextUpdate;

    private bool _isFocused;

    private IDisposable? _deactivationSubscription;

    private IDisposable? _popupPinnedOpenBinding;

    private int _popupLifecycleCloseDepth;

    private EventHandler? _formValueChanged;

    #endregion

    #region 构造函数

    static AbstractAutoComplete()
    {
        IsTabStopProperty.OverrideDefaultValue<AbstractAutoComplete>(false);
        FocusableProperty.OverrideDefaultValue<AbstractAutoComplete>(true);
        SelectedOptionProperty.Changed.AddClassHandler<AbstractAutoComplete>((x,e) => x.HandleSelectedOptionPropertyChanged(e));
        IsDropDownOpenProperty.Changed.AddClassHandler<AbstractAutoComplete>((x,e) => x.HandleIsDropDownOpenChanged(e));
                AsyncLoadDebounceProperty.Changed.AddClassHandler<AbstractAutoComplete>((x,e) => x.HandleAsyncLoadDebounceChanged(e));
        PlacementProperty.Changed.AddClassHandler<AbstractAutoComplete>((x,e) => x.HandlePlacementChanged());
        ValueProperty.Changed.AddClassHandler<AbstractAutoComplete>((x,e) => x.HandleValuePropertyChanged(e));
        OptionsSourceProperty.Changed.AddClassHandler<AbstractAutoComplete>((x,e) => x.HandleItemsSourceChanged(e));
        FilterValueProperty.Changed.AddClassHandler<AbstractAutoComplete>((x,e) => x.HandleFilterValuePropertyChanged(e));
    }

    public AbstractAutoComplete()
    {
        Options.CollectionChanged += HandleOptionsChanged;
    }

    #endregion

    #region 控件 API

    /// <summary>
    /// Applies <see cref="Placement" /> and installs a StartsWith filter when <see cref="Filter" /> is not set.
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        HandlePlacementChanged();
        if (Filter == null)
        {
            SetCurrentValue(FilterProperty, ValueFilterFactory.BuildFilter(ValueFilterMode.StartsWith));
        }
    }

    /// <summary>
    /// Moves the keyboard focus into the text box and selects all of its text when the control gains the focus
    /// and nothing in the text box is selected.
    /// </summary>
    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);
        FocusChanged(HasFocus());
    }

    /// <summary>
    /// Closes the drop-down when the focus moves to an element outside the control, and removes the selection
    /// of the text box when <see cref="ClearSelectionOnLostFocus" /> is set and no context menu is open.
    /// </summary>
    protected override void OnLostFocus(FocusChangedEventArgs e)
    {
        base.OnLostFocus(e);
        FocusChanged(HasFocus());
    }

    /// <summary>
    /// Keeps the popup in step after the base call: a change of the popup layout tokens renews the token
    /// subscription, a change of <see cref="DisplayCandidateCount" /> or of the popup padding recalculates the
    /// maximum popup height, a change of <see cref="IsPopupMatchSelectWidth" /> re-applies the popup width, and
    /// <see cref="IsPopupPinnedOpen" /> set to <see langword="true" /> opens the drop-down.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == PopupLayoutTokenResourceKeysProperty)
        {
            UpdatePopupLayoutTokenSubscription();
        }
        else if (change.Property == DisplayCandidateCountProperty ||
                 change.Property == PopupContentPaddingProperty)
        {
            ConfigureMaxPopupHeight();
        }
        else if (change.Property == IsPopupMatchSelectWidthProperty)
        {
            ConfigurePopupMinWith(DesiredSize.Width);
        }
        else if (change.Property == IsPopupPinnedOpenProperty)
        {
            if (change.GetNewValue<bool>() && !IsDropDownOpen)
            {
                SetCurrentValue(IsDropDownOpenProperty, true);
            }
        }
    }

    /// <summary>
    /// Re-applies the popup width from the new width of the control.
    /// </summary>
    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        ConfigurePopupMinWith(e.NewSize.Width);
    }

    /// <summary>
    /// Calculates the maximum height of the popup as <see cref="DisplayCandidateCount" /> times the height of a
    /// candidate row, taken from the popup layout tokens, plus the padding of the popup. It is called when any of
    /// these values changes.
    /// </summary>
    protected virtual void ConfigureMaxPopupHeight()
    {
        MaxPopupHeight = _itemHeight * DisplayCandidateCount + PopupContentPadding.Top + PopupContentPadding.Bottom;
    }

    private void UpdatePopupLayoutTokenSubscription()
    {
        _popupLayoutTokens?.Dispose();
        _popupLayoutTokens = PopupLayoutTokenResourceKeys?.Observe(this, RefreshPopupLayout);
        RefreshPopupLayout();
    }

    private void RefreshPopupLayout()
    {
        var previousRowHeight = _itemHeight;
        _itemHeight = CandidatePopupLayoutTokenResourceKeys.ReadLength(_popupLayoutTokens?.GetValue(0), _itemHeight) ?? 0;
        var padding = CandidatePopupLayoutTokenResourceKeys.ReadPadding(_popupLayoutTokens?.GetValue(1), PopupContentPadding);
        if (padding != PopupContentPadding)
        {
            PopupContentPadding = padding;
        }
        else if (!previousRowHeight.Equals(_itemHeight))
        {
            ConfigureMaxPopupHeight();
        }
    }

    /// <summary>
    /// Returns whether the control or one of its descendants has the keyboard focus.
    /// </summary>
    protected bool HasFocus() => IsKeyboardFocusWithin;

    /// <summary>
    /// Handles the keys of the drop-down after the base call, when the control is enabled and the key is not
    /// handled yet. An open drop-down passes the key to the candidate list, and Escape cancels the list. A
    /// closed drop-down opens on Down. F4 toggles the drop-down and Enter commits the highlighted candidate.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        _ = e ?? throw new ArgumentNullException(nameof(e));
        base.OnKeyDown(e);
        if (e.Handled || !IsEnabled)
        {
            return;
        }

        // The drop down is open, pass along the key event arguments to the
        // selection adapter. If it isn't handled by the adapter's logic,
        // then we handle some simple navigation scenarios for controlling
        // the drop down.
        if (IsDropDownOpen)
        {
            if (CandidateList != null)
            {
                CandidateList.HandleKeyDown(e);
                if (e.Handled)
                {
                    return;
                }
            }

            if (e.Key == Key.Escape)
            {
                HandleCandidateListCanceled(this, new RoutedEventArgs());
                e.Handled = true;
            }
        }
        else
        {
            // The drop down is not open, the Down key will toggle it open.
            // Ignore key buttons, if they are used for XY focus.
            if (e.Key == Key.Down
                && !this.IsAllowedXYNavigationMode(e.KeyDeviceType))
            {
                SetCurrentValue(IsDropDownOpenProperty, true);
                e.Handled = true;
            }
        }

        // Standard drop down navigation
        switch (e.Key)
        {
            case Key.F4:
                SetCurrentValue(IsDropDownOpenProperty, !IsDropDownOpen);
                e.Handled = true;
                break;

            case Key.Enter:
                if (IsDropDownOpen)
                {
                    HandleCandidateListComplete(this, new RoutedEventArgs());
                    e.Handled = true;
                }
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// Sets <see cref="IsLoading" /> and raises the <see cref="Populating" /> event. It is called when the typed
    /// text reaches <see cref="MinimumPrefixLength" /> and the debounce delay, if any, has elapsed.
    /// </summary>
    protected virtual void NotifyPopulating(CompletePopulatingEventArgs e)
    {
        IsLoading = true;
        Populating?.Invoke(this, e);
    }

    /// <summary>
    /// Clears <see cref="IsLoading" /> and raises the <see cref="Populated" /> event. It is called after the
    /// options were matched, before the drop-down opens and the text is completed.
    /// </summary>
    protected virtual void NotifyPopulated(CompletePopulatedEventArgs e)
    {
        IsLoading = false;
        Populated?.Invoke(this, e);
    }

    /// <summary>
    /// Sets the filtered candidate list to <see langword="null" />. Checks of the candidate count then treat the
    /// control as having no candidates until the options are matched again.
    /// </summary>
    protected void ClearView()
    {
        _view = null;
    }

    /// <summary>
    /// Raises the <see cref="DropDownOpening" /> event. It is called after <see cref="IsDropDownOpen" /> was set
    /// to <see langword="true" /> and the candidates were refreshed, before the popup opens. When the handler
    /// cancels the event, the caller restores the previous value of <see cref="IsDropDownOpen" /> and leaves
    /// the popup closed.
    /// </summary>
    protected virtual void NotifyDropDownOpening(CancelEventArgs eventArgs)
    {
        DropDownOpening?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Raises the <see cref="DropDownOpened" /> event. It is called after the popup was opened.
    /// </summary>
    protected virtual void NotifyDropDownOpened(EventArgs eventArgs)
    {
        DropDownOpened?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Raises the <see cref="DropDownClosing" /> event. It is called after <see cref="IsDropDownOpen" /> was set
    /// to <see langword="false" />, before the popup closes. When the handler cancels the event, the caller
    /// restores <see cref="IsDropDownOpen" /> and leaves the popup open.
    /// </summary>
    protected virtual void NotifyDropDownClosing(CancelEventArgs eventArgs)
    {
        DropDownClosing?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Raises the <see cref="DropDownClosed" /> event. It is called after the popup was closed.
    /// </summary>
    protected virtual void NotifyDropDownClosed(EventArgs eventArgs)
    {
        DropDownClosed?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Raises the <see cref="ValueChanged" /> event as a bubbling routed event. It is called after
    /// <see cref="Value" /> or the text of the text box was updated.
    /// </summary>
    protected virtual void NotifyValueChanged(CompleteValueChangedEventArgs eventArgs)
    {
        RaiseEvent(eventArgs);
    }

    /// <summary>
    /// Raises the <see cref="SelectionChanged" /> event as a bubbling routed event. It is called after
    /// <see cref="SelectedOption" /> changed and the text was updated from the new option.
    /// </summary>
    protected virtual void NotifySelectionChanged(SelectionChangedEventArgs e)
    {
        RaiseEvent(e);
    }

    /// <summary>
    /// Locates the text box, the popup and the candidate list in the new template, re-attaches the popup
    /// handlers and the binding of <see cref="IsPopupPinnedOpen" />, and opens the popup when the drop-down is
    /// already marked open.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_popup != null)
        {
            _popupPinnedOpenBinding?.Dispose();
            _popupPinnedOpenBinding = null;
            _popup.Opened -= HandlePopupOpened;
            _popup.Closed -= HandlePopupClosed;
        }

        TextInputBox       = e.NameScope.Find<AvaloniaTextBox>(AutoCompleteThemeConstants.TextBoxPart);
        _popup        = e.NameScope.Find<Popup>(AutoCompleteThemeConstants.PopupPart);
        CandidateList = e.NameScope.Find<ICandidateList>(AutoCompleteThemeConstants.CandidateListPart);

        if (_popup != null)
        {
            _popupPinnedOpenBinding = BindUtils.RelayBind(
                this,
                IsPopupPinnedOpenProperty,
                _popup,
                Popup.IsPopupPinnedOpenProperty);
            _popup.Opened              += HandlePopupOpened;
            _popup.Closed              += HandlePopupClosed;
            _popup.OverlayInputPassThroughElement = TextInputBox;
        }

        // The drop-down is marked open but the popup of the new template is still closed. Open it through the
        // opening path: DropDownOpening is raised, and cancelling it sets IsDropDownOpen back to false.
        if (IsDropDownOpen && _popup != null && !_popup.IsOpen)
        {
            OpeningDropDown(false);
        }
    }

    /// <summary>
    /// Subscribes to the deactivation of the owning top level, so that the drop-down closes when the window is
    /// deactivated, and opens the drop-down when <see cref="IsPopupPinnedOpen" /> is set.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _deactivationSubscription =
            TopLevelDeactivation.Subscribe(TopLevel.GetTopLevel(this), HandleWindowDeactivated);
        if (IsPopupPinnedOpen && !IsDropDownOpen)
        {
            SetCurrentValue(IsDropDownOpenProperty, true);
        }
    }

    /// <summary>
    /// Closes the drop-down and the popup as part of the control lifecycle, without raising
    /// <see cref="DropDownClosing" />, and disposes the top level deactivation subscription.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _popupLifecycleCloseDepth++;
        try
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
            _popup?.CloseForLifecycle();
        }
        finally
        {
            _popupLifecycleCloseDepth--;
        }

        base.OnDetachedFromVisualTree(e);
        _deactivationSubscription?.Dispose();
        _deactivationSubscription = null;
    }

    /// <summary>
    /// Processes a change of the text box text as a user edit. The work is posted to the dispatcher so that the
    /// selection of the text box is up to date; the candidates are then repopulated, and discarded when the text
    /// is blank.
    /// </summary>
    protected virtual void HandleTextInputBoxTextChanged()
    {
        //Uses Dispatcher.Post to allow the TextInputBox selection to update before processing
        Dispatcher.Post(() =>
        {
            // Call the central updated text method as a user-initiated action
            HandleValueUpdated(_textInputBox!.Text, true);
            if (TextInputBox != null && string.IsNullOrWhiteSpace(TextInputBox.Text))
            {
                ClearView();
            }
        });
    }

    /// <summary>
    /// Commits the chosen candidate. It runs when the candidate list raises <see cref="ICandidateList.Commit" />
    /// and when Enter is pressed while the drop-down is open: it sets <see cref="SelectedOption" /> to the selected
    /// item of the candidate list as the current value, which leaves a binding in place, closes the drop-down,
    /// clears the selection of the text input and focuses the text input. Call the base method from an override to
    /// commit the choice.
    /// </summary>
    private protected virtual void HandleCandidateListComplete(object? sender, RoutedEventArgs e)
    {
        SetCurrentValue(SelectedOptionProperty, _candidateList!.SelectedItem);
        SetCurrentValue(IsDropDownOpenProperty, false);
        // Text should not be selected
        ClearTextInputBoxSelection();
        TextInputBox!.Focus();
    }

    /// <summary>
    /// Applies the width of the control to the drop-down while <see cref="IsPopupMatchSelectWidth" /> is
    /// <see langword="true" />, and leaves the width unchanged otherwise. It is called when the control is sized
    /// or the property changes.
    /// </summary>
    /// <param name="selectWidth">
    /// The current width of the control.
    /// </param>
    protected virtual void ConfigurePopupMinWith(double selectWidth)
    {
        if (IsPopupMatchSelectWidth)
        {
            SetCurrentValue(EffectivePopupWidthProperty, selectWidth);
        }
    }

    /// <summary>
    /// Handles presses after the base call. A press inside the popup is only marked handled. While the drop-down
    /// is open, a press in a text box keeps the drop-down open, and a press from a source that is not a control
    /// closes it. A press on the closed control sets the pressed pseudo class that the release uses to open the
    /// drop-down.
    /// </summary>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if(!e.Handled && e.Source is Visual source)
        {
            if (_popup?.IsInsidePopup(source) == true)
            {
                e.Handled = true;
                return;
            }
        }
        if (IsDropDownOpen)
        {
            // When a drop-down is open with OverlayDismissEventPassThrough enabled and the control
            // is pressed, close the drop-down
            if (e.Source is Control sourceControl)
            {
                var textInputBox = sourceControl.FindAncestorOfType<AvaloniaTextBox>();
                if (textInputBox != null)
                {
                    _ignorePopupClose = true;
                    e.Handled         = true;
                    return;
                }
            }
            else
            {
                SetCurrentValue(IsDropDownOpenProperty, false);
                e.Handled = true;
            }

            e.Handled = true;
        }
        else
        {
            PseudoClasses.Set(StdPseudoClass.Pressed, true);
            e.Handled = true;
        }
    }

    /// <summary>
    /// Toggles the drop-down when a press on the control is released outside the popup: an open drop-down closes,
    /// and a closed one opens when candidates exist. A release inside the popup is only marked handled. The
    /// pressed pseudo class is cleared before the base call.
    /// </summary>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (!e.Handled && e.Source is Visual source)
        {
            if (_popup?.IsInsidePopup(source) == true)
            {
                e.Handled = true;
            }
            else if (PseudoClasses.Contains(StdPseudoClass.Pressed))
            {
                if (IsDropDownOpen)
                {
                    SetCurrentValue(IsDropDownOpenProperty, false);
                }
                else if (_view?.Count > 0)
                {
                    _ignorePropertyChange = true;
                    SetCurrentValue(IsDropDownOpenProperty, true);
                    OpeningDropDown(false);
                }

                e.Handled = true;
            }
        }

        PseudoClasses.Set(StdPseudoClass.Pressed, false);
        base.OnPointerReleased(e);
    }

    #endregion

    #region 实现 FormItem 接口

    event EventHandler? IFormItemAware.ValueChanged
    {
        add => _formValueChanged += value;
        remove => _formValueChanged -= value;
    }

    void IFormItemAware.SetFormValue(object? value) => NotifySetFormValue(value?.ToString());

    object? IFormItemAware.GetFormValue() => NotifyGetFormValue();

    void IFormItemAware.ClearFormValue() => NotifyClearFormValue();

    void IFormItemAware.NotifyValidateStatus(FormValidateStatus status) => NotifyValidateStatus(status);

    void IFormItemFeedbackAware.SetFeedbackControl(FormValidateFeedback? value) => NotifySetFeedBackControl(value);

    /// <summary>
    /// Writes the value that the owning form item passes in into <see cref="Value" />.
    /// </summary>
    /// <param name="value">
    /// The value as text, or <see langword="null" /> to clear it.
    /// </param>
    protected virtual void NotifySetFormValue(string? value)
    {
        SetCurrentValue(ValueProperty, value);
    }

    /// <summary>
    /// Returns <see cref="Value" /> to the owning form item.
    /// </summary>
    protected virtual string? NotifyGetFormValue()
    {
        return Value;
    }

    /// <summary>
    /// Clears <see cref="Value" /> when the owning form item is reset.
    /// </summary>
    protected virtual void NotifyClearFormValue()
    {
        SetCurrentValue(ValueProperty, null);
    }

    /// <summary>
    /// Stores the validation status that the owning form item reports. A status equal to the current one is
    /// not written.
    /// </summary>
    /// <param name="status">
    /// The validation status reported by the form item.
    /// </param>
    protected virtual void NotifyValidateStatus(FormValidateStatus status)
    {
        if (FormStatus != status)
        {
            SetCurrentValue(FormStatusProperty, status);
        }
    }

    /// <summary>
    /// Stores the feedback control that the owning form item passes in; <see langword="null" /> clears it.
    /// </summary>
    /// <param name="value">
    /// The feedback control of the form item.
    /// </param>
    protected virtual void NotifySetFeedBackControl(FormValidateFeedback? value)
    {
        FormFeedback = value;
    }

    #endregion

    #region 内部实现

    private int TextInputBoxSelectionStart
    {
        get
        {
            if (TextInputBox != null)
            {
                return Math.Min(TextInputBox.SelectionStart, TextInputBox.SelectionEnd);
            }
            return 0;
        }
    }

    private int TextInputBoxSelectionLength
    {
        get
        {
            if (TextInputBox != null)
            {
                return Math.Abs(TextInputBox.SelectionEnd - TextInputBox.SelectionStart);
            }
            return 0;
        }
    }

    private void FocusChanged(bool hasFocus)
    {
        // GotFocus and LostFocus arrive asynchronously, so they only report that the focus changed at some time in
        // the past. Before the drop-down is closed, the focus of the parent scope is read from the FocusManager to
        // make sure that it really left the control.

        bool wasFocused = _isFocused;
        _isFocused = hasFocus;

        if (hasFocus)
        {

            if (!wasFocused && TextInputBox != null && TextInputBoxSelectionLength <= 0)
            {
                TextInputBox.Focus();
                TextInputBox.SelectAll();
            }
        }
        else
        {
            // Check if we still have focus in the parent's focus scope
            if (GetFocusScope() is { } scope &&
                (FocusManagerReflectionExtensions.GetFocusManager(this)?.GetFocusedElement(scope) is not { } focused ||
                 (focused != this &&
                  (focused is Visual v && !this.IsVisualAncestorOf(v)))))
            {
                SetCurrentValue(IsDropDownOpenProperty, false);
            }

            _userCalledPopulate = false;

            var textInputBoxContextMenuIsOpen = TextInputBox?.ContextFlyout?.IsOpen == true || TextInputBox?.ContextMenu?.IsOpen == true;
            var contextMenuIsOpen = ContextFlyout?.IsOpen == true || ContextMenu?.IsOpen == true;

            if (!textInputBoxContextMenuIsOpen && !contextMenuIsOpen && ClearSelectionOnLostFocus)
            {
                ClearTextInputBoxSelection();
            }
        }

        _isFocused = hasFocus;

        IFocusScope? GetFocusScope()
        {
            IInputElement? c = this;

            while (c != null)
            {
                if (c is IFocusScope scope &&
                    c is Visual v &&
                    TopLevel.GetTopLevel(v) is Visual root &&
                    root.IsVisible)
                {
                    return scope;
                }

                c = (c as Visual)?.GetVisualParent<IInputElement>();
            }

            return null;
        }
    }

    private void ClearTextInputBoxSelection()
    {
        if (TextInputBox != null)
        {
            int length = TextInputBox.Text?.Length ?? 0;
            TextInputBox.SelectionStart = length;
            TextInputBox.SelectionEnd   = length;
        }
    }

    private void HandleAsyncLoadDebounceChanged(AvaloniaPropertyChangedEventArgs e)
    {
        var newValue = (TimeSpan)e.NewValue!;

        // Always clean up the old timer first
        if (_delayTimer != null)
        {
            _delayTimer.Stop();
            _delayTimer.Tick -= PopulateDropDown;
            _delayTimer      =  null;
        }

        // Create a new timer with the new delay value if needed
        if (newValue > TimeSpan.Zero)
        {
            _delayTimer           =  new DispatcherTimer();
            _delayTimer.Interval  =  newValue;
            _delayTimer.Tick      += PopulateDropDown;
        }
    }

    private void PopulateDropDown(object? sender, EventArgs e)
    {
        _delayTimer?.Stop();

        // Update the prefix/search text.
        FilterValue = Value;

        if (TryPopulateAsync(FilterValue))
        {
            return;
        }

        // A handler of Populating can take the population over by setting Cancel: the control then stops here, the
        // options are not matched and Populated is not raised.
        var populating = new CompletePopulatingEventArgs(FilterValue);
        NotifyPopulating(populating);
        if (!populating.Cancel)
        {
            PopulateComplete();
        }
    }

    private void HandlePlacementChanged()
    {
        if (Placement == AutoCompletePlacementMode.Bottom)
        {
            PopupPlacement = PlacementMode.BottomEdgeAlignedLeft;
        }
        else
        {
            PopupPlacement = PlacementMode.TopEdgeAlignedLeft;
        }
    }

    private void HandleValuePropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        HandleValueUpdated((string?)e.NewValue, false);
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    private void HandleFilterValuePropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_ignorePropertyChange)
        {
            _ignorePropertyChange = false;
            return;
        }

        // Ensure the property is only written when expected
        if (!_allowWrite)
        {
            // Reset the old value before it was incorrectly written
            _ignorePropertyChange = true;
            SetCurrentValue(FilterValueProperty, e.OldValue);
            throw new InvalidOperationException("Cannot set read-only property FilterValue.");
        }
    }

    private void HandleItemsSourceChanged(AvaloniaPropertyChangedEventArgs change)
    {
        // Clear and set the view on the selection adapter
        ClearView();
        _options.SetItemsSource(change.GetNewValue<IEnumerable<IAutoCompleteOption>?>());
        RefreshView();
    }

    private void HandleOptionsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RefreshView();
    }

    private void HandleValueUpdated(string? newText, bool userInitiated)
    {
        // Only process this event if it is coming from someone outside
        // setting the Text dependency property directly.
        if (_ignoreValuePropertyChange > 0)
        {
            _ignoreValuePropertyChange--;
            return;
        }

        newText ??= string.Empty;

        // The TextInputBox.TextChanged event was not firing immediately and
        // was causing an immediate update, even with wrapping. If there is
        // a selection currently, no update should happen.
        if (IsCompletionEnabled && TextInputBox != null && TextInputBoxSelectionLength > 0 && TextInputBoxSelectionStart != (TextInputBox.Text?.Length ?? 0))
        {
            return;
        }

        // Evaluate the conditions needed for completion.
        // 1. Minimum prefix length
        // 2. If a delay timer is in use, use it
        bool minimumLengthReached = newText.Length >= MinimumPrefixLength && MinimumPrefixLength >= 0;

        _userCalledPopulate = minimumLengthReached && userInitiated;

        // Update the interface and values only as necessary
        UpdateValue(newText, userInitiated);

        if (minimumLengthReached)
        {
            _ignoreTextSelectionChange = true;

            if (_delayTimer != null)
            {
                _delayTimer.Start();
            }
            else
            {
                PopulateDropDown(this, EventArgs.Empty);
            }
        }
        else
        {
            FilterValue = string.Empty;
            if (SelectedOption != null)
            {
                _skipSelectedOptionTextUpdate = true;
            }

            SetCurrentValue(SelectedOptionProperty, null);

            if (IsDropDownOpen)
            {
                SetCurrentValue(IsDropDownOpenProperty, false);
            }
        }
    }

    private void UpdateValue(string? value)
    {
        UpdateValue(value, null);
    }

    private void UpdateValue(string? value, bool? userInitiated)
    {
        bool callTextChanged = false;
        // Update the Value property
        if ((userInitiated ?? true) && Value != value)
        {
            _ignoreValuePropertyChange++;
            SetCurrentValue(ValueProperty, value);
            callTextChanged = true;
        }

        // Update the TextInputBox's Text dependency property
        if ((userInitiated == null || userInitiated == false) && TextInputBox != null && TextInputBox.Text != value)
        {
            _ignoreValuePropertyChange++;
            TextInputBox.Text = value ?? string.Empty;

            // Text dependency property value was set, fire event
            if (!callTextChanged && (Value == value || Value == null))
            {
                callTextChanged = true;
            }
        }

        if (callTextChanged)
        {
            NotifyValueChanged(new CompleteValueChangedEventArgs(Value, ValueChangedEvent));
        }
    }

    private void RefreshView()
    {
        // If we have a running filter, trigger a request first
        if (_filterInAction)
        {
            _cancelRequested = true;
        }

        // Indicate that filtering is ongoing
        _filterInAction = true;

        try
        {
            // Cache the current text value
            var text = Value ?? string.Empty;

            // Determine if any filtering mode is on
            bool filtering = Filter != null;

            var items = Options;

            // cache properties
            var newViewItems = new Collection<IAutoCompleteOption>();

            foreach (var item in items)
            {
                if (item is IAutoCompleteOption option)
                {
                    // Exit the fitter when requested if cancellation is requested
                    if (_cancelRequested)
                    {
                        return;
                    }

                    var inResults = !filtering;

                    if (!inResults)
                    {
                        inResults = Filter?.Filter(GetValueByOption(option), text) ?? true;
                    }

                    if (inResults)
                    {
                        newViewItems.Add(option);
                    }
                }
            }

            _view = newViewItems;
            if (_candidateList != null)
            {
                if (_candidateList.ItemsSource != _view)
                {
                    _candidateList.ItemsSource = _view;
                }
            }
        }
        finally
        {
            // indicate that filtering is not ongoing anymore
            _filterInAction  = false;
            _cancelRequested = false;
        }
    }

    private static bool IsValidMinimumPrefixLength(int value) => value >= -1;

    private static bool IsValidAsyncLoadDebounce(TimeSpan value) => value.TotalMilliseconds >= 0.0;

    private static bool IsValidMaxDropDownHeight(double value) => value >= 0.0;

    private void HandleIsDropDownOpenChanged(AvaloniaPropertyChangedEventArgs e)
    {
        // Ignore the change if requested
        if (_ignorePropertyChange)
        {
            _ignorePropertyChange = false;
            return;
        }

        bool oldValue = (bool)e.OldValue!;
        bool newValue = (bool)e.NewValue!;

        if (!newValue && _popupLifecycleCloseDepth > 0)
        {
            UpdatePseudoClasses();
            return;
        }

        if (!newValue && IsPopupPinnedOpen)
        {
            _ignorePropertyChange = true;
            SetCurrentValue(IsDropDownOpenProperty, oldValue);
            UpdatePseudoClasses();
            return;
        }

        if (newValue)
        {
            HandleValueUpdated(Value, true);
        }
        else
        {
            ClosingDropDown(oldValue);
        }

        UpdatePseudoClasses();
    }

    private void HandleSelectedOptionPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_ignorePropertyChange)
        {
            _ignorePropertyChange = false;
            return;
        }

        // Update the text display
        if (_skipSelectedOptionTextUpdate)
        {
            _skipSelectedOptionTextUpdate = false;
        }
        else
        {
            HandleSelectedOptionChanged(e.NewValue);
        }

        // Fire the SelectionChanged event
        var removed = new List<object>(e.OldValue != null ? 1 : 0);
        if (e.OldValue != null)
        {
            removed.Add(e.OldValue);
        }

        var added = new List<object>(e.NewValue != null ? 1 : 0);
        if (e.NewValue != null)
        {
            added.Add(e.NewValue);
        }

        NotifySelectionChanged(new SelectionChangedEventArgs(SelectionChangedEvent, removed, added));
    }

    private void ClosingDropDown(bool oldValue)
    {
        var args = new CancelEventArgs();
        NotifyDropDownClosing(args);

        if (args.Cancel)
        {
            _ignorePropertyChange = true;
            SetCurrentValue(IsDropDownOpenProperty, oldValue);
        }
        else
        {
            CloseDropDown();
        }

        UpdatePseudoClasses();
    }

    private void OpenDropDown()
    {
        if (_popup != null)
        {
            _popup.IsOpen = true;
        }
        _popupHasOpened = true;
        NotifyDropDownOpened(EventArgs.Empty);
    }

    private void CloseDropDown()
    {
        if (_popupHasOpened)
        {
            if (_popup != null)
            {
                _popup.IsOpen = false;
            }
            NotifyDropDownClosed(EventArgs.Empty);
        }
    }

    private void OpeningDropDown(bool oldValue)
    {
        var args = new CancelEventArgs();

        // Opening
        NotifyDropDownOpening(args);

        if (args.Cancel)
        {
            _ignorePropertyChange = true;
            SetCurrentValue(IsDropDownOpenProperty, oldValue);
        }
        else
        {
            OpenDropDown();
        }

        UpdatePseudoClasses();
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(AutoCompletePseudoClass.CandidatePopupOpen, IsDropDownOpen);
    }

    private void HandleWindowDeactivated(object? sender, EventArgs e)
    {
        SetCurrentValue(IsDropDownOpenProperty, false);
    }

    private void HandlePopupOpened(object? sender, EventArgs e)
    {
        _subscriptionsOnOpen?.Dispose();
        _subscriptionsOnOpen = new CompositeDisposable(2);
        this.GetObservable(IsVisibleProperty).Subscribe(HandleIsVisibleChanged).DisposeWith(_subscriptionsOnOpen);
        this.GetObservable(IsEnabledProperty).Subscribe(HandleIsEnabledChanged).DisposeWith(_subscriptionsOnOpen);
        this.SubscribeAncestorIsVisible(HandleIsVisibleChanged, _subscriptionsOnOpen);
        NotifyDropDownOpened(EventArgs.Empty);
        var selectedItem = TryGetMatch(Value, _view, ValueFilterFactory.BuildFilter(ValueFilterMode.EqualsCaseSensitive));
        CandidateList!.SelectedItem = selectedItem;
    }

    private void HandlePopupClosed(object? sender, EventArgs e)
    {
        _subscriptionsOnOpen?.Dispose();
        _subscriptionsOnOpen = null;
        // Force the drop down dependency property to be false.
        if (IsDropDownOpen)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
        }

        // Fire the DropDownClosed event
        if (_popupHasOpened)
        {
            NotifyDropDownClosed(EventArgs.Empty);
        }
        NotifyDropDownClosed(EventArgs.Empty);
    }

    private void HandleIsVisibleChanged(bool isVisible)
    {
        if (!isVisible && IsDropDownOpen)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
        }
    }

    private void HandleIsEnabledChanged(bool isEnabled)
    {
        if (!isEnabled && IsDropDownOpen)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
        }
    }

    private bool TryPopulateAsync(string? filterValue)
    {
        if (OptionsAsyncLoader == null)
        {
            return false;
        }

        _asyncLoadCoordinator.Timeout = AsyncLoadTimeout;
        _ = PopulateAsync(filterValue);
        return true;
    }

    private async Task PopulateAsync(string? filterValue)
    {
        var loader = OptionsAsyncLoader;
        if (loader == null)
        {
            return;
        }

        var outcome = await _asyncLoadCoordinator.LoadAsync(
            filterValue,
            (ctx, token) => loader.LoadAsync(ctx, token));

        if (outcome.IsSkipped)
        {
            return;
        }

        if (outcome.IsSuccess && outcome.Result != null)
        {
            var result = outcome.Result;
            OptionsLoaded?.Invoke(this, new CompleteOptionsLoadedEventArgs(filterValue, result));
            SetCurrentValue(OptionsSourceProperty, result.Data);

            Dispatcher.Post(PopulateComplete);
            return;
        }

        var statusCode = outcome.Status switch
        {
            AsyncLoadStatus.TimedOut  => RpcStatusCode.Timeout,
            AsyncLoadStatus.Cancelled => RpcStatusCode.Cancelled,
            _                         => RpcStatusCode.Unknown
        };

        OptionsLoaded?.Invoke(this, new CompleteOptionsLoadedEventArgs(filterValue, new CompleteOptionsLoadResult()
        {
            UserFriendlyMessage = outcome.Error?.Message,
            StatusCode          = statusCode
        }));
    }

    private void PopulateComplete()
    {
        // Apply the search filter
        RefreshView();

        // Fire the Populated event containing the read-only view data.
        var populated = new CompletePopulatedEventArgs(_view);
        NotifyPopulated(populated);

        bool isDropDownOpen = _userCalledPopulate && (_view!.Count > 0);
        if (isDropDownOpen != IsDropDownOpen)
        {
            _ignorePropertyChange = true;
            SetCurrentValue(IsDropDownOpenProperty, isDropDownOpen);
        }
        if (IsDropDownOpen)
        {
            OpeningDropDown(false);
        }
        else
        {
            ClosingDropDown(true);
        }

        UpdateTextCompletion(_userCalledPopulate);
    }

    private void UpdateTextCompletion(bool userInitiated)
    {
        // By default this method will clear the selected value
        object? newSelectedItem = null;
        string? value           = Value;
        //
        // Text search is StartsWith explicit and only when enabled, in
        // line with WPF's ComboBox lookup. When in use it will associate
        // a Value with the Text if it is found in ItemsSource. This is
        // only valid when there is data and the user initiated the action.
        if (_view?.Count > 0)
        {
            if (IsCompletionEnabled && TextInputBox != null && userInitiated)
            {
                int currentLength = TextInputBox.Text?.Length ?? 0;
                int selectionStart = TextInputBoxSelectionStart;
                if (selectionStart == value?.Length && selectionStart > _textSelectionStart)
                {
                    // When the FilterMode dependency property is set to
                    // either StartsWith or StartsWithCaseSensitive, the
                    // first item in the view is used. This will improve
                    // performance on the lookup. It assumes that the
                    // FilterMode the user has selected is an acceptable
                    // case sensitive matching function for their scenario.
                    var top = Filter?.Mode == ValueFilterMode.StartsWith || Filter?.Mode == ValueFilterMode.StartsWithCaseSensitive
                        ? _view[0]
                        : TryGetMatch(value, _view, ValueFilterFactory.BuildFilter(ValueFilterMode.StartsWith));

                    // If the search was successful, update SelectedOption
                    if (top != null)
                    {
                        newSelectedItem = top;
                        var topString = (top.Content ?? top.Header ?? top.ItemKey)?.ToString();

                        // Only replace partially when the two words being the same
                        int minLength = Math.Min(topString?.Length ?? 0, Value?.Length ?? 0);
                        var isPrefixMatch = Value == null || topString == null
                            ? Value == topString
                            : string.Compare(Value, 0, topString, 0, minLength, StringComparison.CurrentCultureIgnoreCase) == 0;
                        if (isPrefixMatch)
                        {
                            // Update the text
                            UpdateValue(topString);
                            // Select the text past the user's caret
                            TextInputBox.SelectionStart = currentLength;
                            TextInputBox.SelectionEnd = topString?.Length ?? 0;
                        }
                    }
                }
            }
            else
            {
                // Perform an exact string lookup for the text. This is a
                // design change from the original Toolkit release when the
                // IsTextCompletionEnabled property behaved just like the
                // WPF ComboBox's IsTextSearchEnabled property.
                //
                // This change provides the behavior that most people expect
                // to find: a lookup for the value is always performed.
                newSelectedItem = TryGetMatch(value, _view, ValueFilterFactory.BuildFilter(ValueFilterMode.EqualsCaseSensitive));
            }
        }

        // Update the selected item property

        if (SelectedOption != newSelectedItem)
        {
            _skipSelectedOptionTextUpdate = true;
        }
        SetCurrentValue(SelectedOptionProperty, newSelectedItem);

        // Restore updates for TextSelection
        if (_ignoreTextSelectionChange)
        {
            _ignoreTextSelectionChange = false;
            if (TextInputBox != null)
            {
                _textSelectionStart = TextInputBoxSelectionStart;
            }
        }
    }

    private IAutoCompleteOption? TryGetMatch(string? filterValue, IList<IAutoCompleteOption>? view, IValueFilter? predicate)
    {
        if (predicate is null)
        {
            return null;
        }

        if (view != null && view.Count > 0)
        {
            foreach (var option in view)
            {
                var value = GetValueByOption(option);
                if (predicate.Filter(value, filterValue))
                {
                    return option;
                }
            }
        }

        return null;
    }

    private string? GetValueByOption(IAutoCompleteOption option)
    {
        string? value = null;
        if (FilterValueSelector != null)
        {
            value = FilterValueSelector(option)?.ToString();
        }
        else
        {
            value = option.Header?.ToString() ?? option.Content?.ToString() ?? option.ItemKey?.ToString();
        }
        return value;
    }

    private void HandleSelectedOptionChanged(object? newItem)
    {
        string? text = null;

        if (newItem == null)
        {
            text = FilterValue;
        }
        else if (newItem is IAutoCompleteOption option)
        {
            text = option.Content?.ToString() ?? option.Header?.ToString() ?? option.ItemKey?.ToString();
        }
        // Update the Text property and the TextInputBox values
        UpdateValue(text);

        // Move the caret to the end of the text box
        ClearTextInputBoxSelection();
    }

    private void HandleCandidateListCanceled(object? sender, RoutedEventArgs e)
    {
        UpdateValue(FilterValue);
        // Completion will update the selected value
        UpdateTextCompletion(false);
        SetCurrentValue(IsDropDownOpenProperty, false);
    }

    #endregion
}
