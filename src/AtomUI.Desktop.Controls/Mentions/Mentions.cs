using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using AtomUI.Controls;
using AtomUI.Controls.AsyncLoad;
using AtomUI.Controls.Utils;
using AtomUI.Data;
using AtomUI.Desktop.Controls.DataLoad;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Icons.AntDesign;
using AtomUI.Input;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Multi-line text input that opens a list of candidates when one of the <see cref="TriggerPrefix" />
/// characters is typed. Picking a candidate inserts its text into the input. The candidates come from
/// <see cref="OptionsSource" />, filtered by the text typed after the trigger, or from
/// <see cref="OptionsAsyncLoader" />. <see cref="Value" /> holds the resulting plain text.
/// </summary>
[PseudoClasses(MentionPseudoClass.CandidatePopupOpen)]
public partial class Mentions : TemplatedControl,
                        IMotionAwareControl,
                        IFormItemAware,
                        IInputControlStatusAware,
                        IInputControlStyleVariantAware,
                        ICustomizableSizeTypeAware,
                        IFormItemFeedbackAware
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="ClearIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> ClearIconProperty =
        AvaloniaProperty.Register<Mentions, PathIcon?>(nameof(ClearIcon));
    
    /// <summary>
    /// Identifies the <see cref="Value" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<Mentions, string?>(
            nameof(Value),
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);
    
    /// <summary>
    /// Identifies the <see cref="IsAllowClear" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsAllowClearProperty =
        AvaloniaProperty.Register<Mentions, bool>(nameof(IsAllowClear));
    
    /// <summary>
    /// Identifies the <see cref="IsAutoFocus" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsAutoFocusProperty =
        AvaloniaProperty.Register<Mentions, bool>(nameof(IsAutoFocus));
    
    /// <summary>
    /// Identifies the <see cref="IsAutoSize" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsAutoSizeProperty =
        AvaloniaProperty.Register<Mentions, bool>(nameof(IsAutoSize));

    /// <summary>
    /// Identifies the <see cref="DefaultValue" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> DefaultValueProperty =
        AvaloniaProperty.Register<Mentions, string?>(nameof(DefaultValue));

    /// <summary>
    /// Identifies the <see cref="Placement" /> property.
    /// </summary>
    public static readonly StyledProperty<MentionsPlacementMode> PlacementProperty =
        AvaloniaProperty.Register<Mentions, MentionsPlacementMode>(nameof(Placement), MentionsPlacementMode.Bottom);
    
    /// <summary>
    /// Identifies the <see cref="EmptyIndicator" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> EmptyIndicatorProperty =
        AvaloniaProperty.Register<Mentions, object?>(nameof(EmptyIndicator));
    
    /// <summary>
    /// Identifies the <see cref="EmptyIndicatorTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> EmptyIndicatorTemplateProperty =
        AvaloniaProperty.Register<Mentions, IDataTemplate?>(nameof(EmptyIndicatorTemplate));
    
    /// <summary>
    /// Identifies the <see cref="IsShowEmptyIndicator" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowEmptyIndicatorProperty =
        AvaloniaProperty.Register<Mentions, bool>(nameof(IsShowEmptyIndicator), true);
    
    /// <summary>
    /// Identifies the <see cref="EmptyIndicatorPadding" /> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> EmptyIndicatorPaddingProperty =
        AvaloniaProperty.Register<Mentions, Thickness>(nameof(EmptyIndicatorPadding));
    
    /// <summary>
    /// Identifies the <see cref="TriggerPrefix" /> property.
    /// </summary>
    public static readonly StyledProperty<IList<string>?> TriggerPrefixProperty =
        AvaloniaProperty.Register<Mentions, IList<string>?>(nameof(TriggerPrefix));
    
    /// <summary>
    /// Identifies the <see cref="Split" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> SplitProperty =
        AvaloniaProperty.Register<Mentions, string?>(nameof(Split));

    /// <summary>
    /// Identifies the <see cref="ShouldUseOverlayPopup" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShouldUseOverlayPopupProperty =
        AvaloniaProperty.Register<Mentions, bool>(nameof(ShouldUseOverlayPopup), true);
    
    /// <summary>
    /// Identifies the <see cref="ContentLeftAddOn" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ContentLeftAddOnProperty =
        AddOnDecoratedBox.ContentLeftAddOnProperty.AddOwner<Mentions>();
    
    /// <summary>
    /// Identifies the <see cref="ContentLeftAddOnTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ContentLeftAddOnTemplateProperty =
        AddOnDecoratedBox.ContentLeftAddOnTemplateProperty.AddOwner<Mentions>();

    /// <summary>
    /// Identifies the <see cref="ContentRightAddOn" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ContentRightAddOnProperty =
        AddOnDecoratedBox.ContentRightAddOnProperty.AddOwner<Mentions>();
    
    /// <summary>
    /// Identifies the <see cref="ContentRightAddOnTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ContentRightAddOnTemplateProperty =
        AddOnDecoratedBox.ContentRightAddOnTemplateProperty.AddOwner<Mentions>();
    
    /// <summary>
    /// Identifies the <see cref="StyleVariant" /> property.
    /// </summary>
    public static readonly StyledProperty<InputControlStyleVariant> StyleVariantProperty =
        InputControlStyleVariantProperty.StyleVariantProperty.AddOwner<Mentions>();

    /// <summary>
    /// Identifies the <see cref="Status" /> property.
    /// </summary>
    public static readonly StyledProperty<InputControlStatus> StatusProperty =
        InputControlStatusProperty.StatusProperty.AddOwner<Mentions>();
    
    /// <summary>
    /// Identifies the <see cref="PlaceholderText" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> PlaceholderTextProperty =
        AvaloniaProperty.Register<Mentions, string?>(nameof(PlaceholderText));
    
    /// <summary>
    /// Identifies the <see cref="OptionsSource" /> property.
    /// </summary>
    public static readonly StyledProperty<IEnumerable<IMentionOption>?> OptionsSourceProperty =
        AvaloniaProperty.Register<Mentions, IEnumerable<IMentionOption>?>(nameof(OptionsSource));
    
    /// <summary>
    /// Identifies the <see cref="OptionTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> OptionTemplateProperty =
        AvaloniaProperty.Register<Mentions, IDataTemplate?>(nameof(OptionTemplate));
    
    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty = 
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Mentions>();
    
    /// <summary>
    /// Identifies the <see cref="Lines" /> property.
    /// </summary>
    public static readonly StyledProperty<int> LinesProperty =
        TextArea.LinesProperty.AddOwner<Mentions>();
    
    /// <summary>
    /// Identifies the <see cref="MinLines" /> property.
    /// </summary>
    public static readonly StyledProperty<int> MinLinesProperty =
        TextArea.MinLinesProperty.AddOwner<Mentions>();
    
    /// <summary>
    /// Identifies the <see cref="MaxLines" /> property.
    /// </summary>
    public static readonly StyledProperty<int> MaxLinesProperty =
        TextArea.MaxLinesProperty.AddOwner<Mentions>();
    
    /// <summary>
    /// Identifies the <see cref="OptionsAsyncLoader" /> property.
    /// </summary>
    public static readonly StyledProperty<IMentionOptionsAsyncLoader?> OptionsAsyncLoaderProperty =
        AvaloniaProperty.Register<Mentions, IMentionOptionsAsyncLoader?>(nameof(OptionsAsyncLoader));
    
    /// <summary>
    /// Identifies the <see cref="IsLoading" /> property.
    /// </summary>
    public static readonly DirectProperty<Mentions, bool> IsLoadingProperty =
        AvaloniaProperty.RegisterDirect<Mentions, bool>(
            nameof(IsLoading),
            o => o.IsLoading);
    
    /// <summary>
    /// Identifies the <see cref="DisplayCandidateCount" /> property.
    /// </summary>
    public static readonly StyledProperty<int> DisplayCandidateCountProperty = 
        AvaloniaProperty.Register<Mentions, int>(nameof (DisplayCandidateCount), 10);
    
    /// <summary>
    /// Identifies the <see cref="IsReadOnly" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        TextArea.IsReadOnlyProperty.AddOwner<Mentions>();
    
    /// <summary>
    /// Identifies the <see cref="Filter" /> property.
    /// </summary>
    public static readonly StyledProperty<IValueFilter?> FilterProperty =
        AvaloniaProperty.Register<Mentions, IValueFilter?>(nameof(Filter));
    
    /// <summary>
    /// Identifies the <see cref="FilterValueSelector" /> property.
    /// </summary>
    public static readonly StyledProperty<DefaultFilterValueSelector?> FilterValueSelectorProperty =
        AvaloniaProperty.Register<Mentions, DefaultFilterValueSelector?>(
            nameof(FilterValueSelector));
    
    /// <summary>
    /// Identifies the <see cref="AsyncLoadDebounce" /> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> AsyncLoadDebounceProperty =
        AvaloniaProperty.Register<Mentions, TimeSpan>(
            nameof(AsyncLoadDebounce),
            TimeSpan.Zero,
            validate: IsValidAsyncLoadDebounce);

    /// <summary>
    /// Identifies the <see cref="AsyncLoadTimeout" /> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> AsyncLoadTimeoutProperty =
        AvaloniaProperty.Register<Mentions, TimeSpan>(
            nameof(AsyncLoadTimeout),
            TimeSpan.FromSeconds(10));
    
    /// <summary>
    /// Identifies the <see cref="IsDropDownOpen" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        AvaloniaProperty.Register<Mentions, bool>(
            nameof(IsDropDownOpen));
    
    /// <summary>
    /// Identifies the <see cref="IsPopupPinnedOpen" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        Popup.IsPopupPinnedOpenProperty.AddOwner<Mentions>();

    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<Mentions>();
    
    /// <summary>
    /// Icon of the clear button that <see cref="IsAllowClear" /> shows. When it is <see langword="null" /> when
    /// the control initializes, the control uses a <c>CloseCircleFilled</c> icon.
    /// </summary>
    public PathIcon? ClearIcon
    {
        get => GetValue(ClearIconProperty);
        set => SetValue(ClearIconProperty, value);
    }
    
    /// <summary>
    /// The text of the input, with inserted candidates as plain text. The property binds two-way by default and
    /// receives data validation errors from its binding. While it is <see langword="null" /> when the control
    /// initializes, <see cref="DefaultValue" /> is assigned to it.
    /// </summary>
    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    
    /// <summary>
    /// Shows a clear button in the suffix of the input frame while <see langword="true" />. The value is passed
    /// to the inner text area.
    /// </summary>
    public bool IsAllowClear
    {
        get => GetValue(IsAllowClearProperty);
        set => SetValue(IsAllowClearProperty, value);
    }
    
    /// <summary>
    /// Moves focus to the input area when the control is loaded. The focus call is posted to the dispatcher.
    /// </summary>
    public bool IsAutoFocus
    {
        get => GetValue(IsAutoFocusProperty);
        set => SetValue(IsAutoFocusProperty, value);
    }
    
    /// <summary>
    /// Lets the height of the input area follow its text while <see langword="true" />, within the bounds of
    /// <see cref="MinLines" /> and <see cref="MaxLines" />. The value is passed to the inner text area.
    /// </summary>
    public bool IsAutoSize
    {
        get => GetValue(IsAutoSizeProperty);
        set => SetValue(IsAutoSizeProperty, value);
    }
    
    /// <summary>
    /// Side of the trigger character's line at which the candidate list opens:
    /// <see cref="MentionsPlacementMode.Bottom" /> places it below the line and
    /// <see cref="MentionsPlacementMode.Top" /> above it. In both cases the left edge of the list aligns with
    /// the trigger character. The default is <see cref="MentionsPlacementMode.Bottom" />.
    /// </summary>
    public MentionsPlacementMode Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }
    
    /// <summary>
    /// Content shown in place of the candidate list when no candidate matches. The default theme does not read
    /// this value, so it takes effect only in a template that passes it on to the candidate list.
    /// </summary>
    [DependsOn(nameof(EmptyIndicatorTemplate))]
    public object? EmptyIndicator
    {
        get => GetValue(EmptyIndicatorProperty);
        set => SetValue(EmptyIndicatorProperty, value);
    }

    /// <summary>
    /// Template that presents <see cref="EmptyIndicator" />. The default theme does not read this value, so it
    /// takes effect only in a template that passes it on to the candidate list.
    /// </summary>
    public IDataTemplate? EmptyIndicatorTemplate
    {
        get => GetValue(EmptyIndicatorTemplateProperty);
        set => SetValue(EmptyIndicatorTemplateProperty, value);
    }
    
    /// <summary>
    /// Whether the candidate list shows the empty indicator when no candidate matches; the default is
    /// <see langword="true" />. The default theme does not read this value, so it takes effect only in a
    /// template that passes it on to the candidate list.
    /// </summary>
    public bool IsShowEmptyIndicator
    {
        get => GetValue(IsShowEmptyIndicatorProperty);
        set => SetValue(IsShowEmptyIndicatorProperty, value);
    }
    
    /// <summary>
    /// Padding around the empty indicator. The default theme does not read this value, so it takes effect only
    /// in a template that passes it on to the candidate list.
    /// </summary>
    public Thickness EmptyIndicatorPadding
    {
        get => GetValue(EmptyIndicatorPaddingProperty);
        set => SetValue(EmptyIndicatorPaddingProperty, value);
    }

    /// <summary>
    /// Text that becomes <see cref="Value" /> while <see cref="Value" /> is <see langword="null" /> when the
    /// control initializes. A change afterwards has no effect.
    /// </summary>
    public string? DefaultValue
    {
        get => GetValue(DefaultValueProperty);
        set => SetValue(DefaultValueProperty, value);
    }
    
    /// <summary>
    /// Characters that open the candidate list when one of them is typed, such as <c>@</c> or <c>#</c>. The value
    /// is passed to the inner text area. When it is <see langword="null" /> when the control initializes, the
    /// control uses a list that contains only <c>@</c>.
    /// </summary>
    public IList<string>? TriggerPrefix
    {
        get => GetValue(TriggerPrefixProperty);
        set => SetValue(TriggerPrefixProperty, value);
    }

    /// <summary>
    /// Text that is inserted after a picked candidate, for example a space. <see langword="null" /> inserts the
    /// candidate text alone.
    /// </summary>
    public string? Split
    {
        get => GetValue(SplitProperty);
        set => SetValue(SplitProperty, value);
    }

    /// <summary>
    /// Renders the candidate list in the overlay layer of the window while <see langword="true" />, the default,
    /// so that no ancestor clips or covers it. <see langword="false" /> renders it inside the control's own
    /// visual tree.
    /// </summary>
    public bool ShouldUseOverlayPopup
    {
        get => GetValue(ShouldUseOverlayPopupProperty);
        set => SetValue(ShouldUseOverlayPopupProperty, value);
    }
    
    /// <summary>
    /// Content shown in the add-on area at the left end of the input frame. The value is passed to the inner
    /// text area.
    /// </summary>
    public object? ContentLeftAddOn
    {
        get => GetValue(ContentLeftAddOnProperty);
        set => SetValue(ContentLeftAddOnProperty, value);
    }
    
    /// <summary>
    /// Template that presents <see cref="ContentLeftAddOn" />. When it is <see langword="null" />, the content
    /// is shown without a template.
    /// </summary>
    public IDataTemplate? ContentLeftAddOnTemplate
    {
        get => GetValue(ContentLeftAddOnTemplateProperty);
        set => SetValue(ContentLeftAddOnTemplateProperty, value);
    }

    /// <summary>
    /// Content shown in the add-on area at the right end of the input frame. The value is passed to the inner
    /// text area.
    /// </summary>
    public object? ContentRightAddOn
    {
        get => GetValue(ContentRightAddOnProperty);
        set => SetValue(ContentRightAddOnProperty, value);
    }
    
    /// <summary>
    /// Template that presents <see cref="ContentRightAddOn" />. When it is <see langword="null" />, the content
    /// is shown without a template.
    /// </summary>
    public IDataTemplate? ContentRightAddOnTemplate
    {
        get => GetValue(ContentRightAddOnTemplateProperty);
        set => SetValue(ContentRightAddOnTemplateProperty, value);
    }
    
    /// <summary>
    /// Appearance of the input frame: <c>Outlined</c>, <c>Filled</c> or <c>Borderless</c>. The value is passed
    /// to the inner text area.
    /// </summary>
    public InputControlStyleVariant StyleVariant
    {
        get => GetValue(StyleVariantProperty);
        set => SetValue(StyleVariantProperty, value);
    }

    /// <summary>
    /// Status of the input frame, such as <c>Error</c> or <c>Warning</c>, which the inner text area shows in the
    /// color of its border. The value is passed to the inner text area.
    /// </summary>
    public InputControlStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }
    
    /// <summary>
    /// Text that the input area shows while it contains no text. The value is passed to the inner text area.
    /// </summary>
    public string? PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }
    
    /// <summary>
    /// The candidates that the control filters by the text typed after the trigger. The control caches the items
    /// when the value is assigned or the control is attached. A collection that implements
    /// <see cref="INotifyCollectionChanged" /> keeps the cache current; after any other collection changed its
    /// items, assign it again. <see langword="null" /> leaves the list without candidates. A successful load
    /// through <see cref="OptionsAsyncLoader" /> replaces the value.
    /// </summary>
    public IEnumerable<IMentionOption>? OptionsSource
    {
        get => GetValue(OptionsSourceProperty);
        set => SetValue(OptionsSourceProperty, value);
    }
    
    /// <summary>
    /// Template that presents one candidate in the list; its data type is <see cref="IMentionOption" />. The
    /// default theme supplies a template that shows the Header of the candidate as text.
    /// </summary>
    [InheritDataTypeFromItems(nameof(OptionsSource))]
    public IDataTemplate? OptionTemplate
    {
        get => GetValue(OptionTemplateProperty);
        set => SetValue(OptionTemplateProperty, value);
    }
    
    /// <summary>
    /// Enables the motion of the inner text area and of the candidate list while <see langword="true" />. The
    /// theme initializes the value from the Mentions <c>EnableMotion</c> token.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// Number of text rows that sets the height of the input area while <see cref="IsAutoSize" /> is
    /// <see langword="false" />. The value is passed to the inner text area. The default is 1.
    /// </summary>
    public int Lines
    {
        get => GetValue(LinesProperty);
        set => SetValue(LinesProperty, value);
    }
    
    /// <summary>
    /// Lower limit for the height of the input area in text rows; a smaller height counts as this many rows.
    /// The value is passed to the inner text area.
    /// </summary>
    public int MinLines
    {
        get => GetValue(MinLinesProperty);
        set => SetValue(MinLinesProperty, value);
    }
    
    /// <summary>
    /// Upper limit for the height of the input area in text rows; a value that is not positive leaves the
    /// height unlimited. The value is passed to the inner text area.
    /// </summary>
    public int MaxLines
    {
        get => GetValue(MaxLinesProperty);
        set => SetValue(MaxLinesProperty, value);
    }
    
    /// <summary>
    /// Loads the candidates for the text typed after the trigger. While it is set, the control asks it for the
    /// candidates instead of raising <see cref="Populating" />, and a successful result replaces
    /// <see cref="OptionsSource" />. A newer request, closing the list or detaching the control cancels a
    /// running load.
    /// </summary>
    public IMentionOptionsAsyncLoader? OptionsAsyncLoader
    {
        get => GetValue(OptionsAsyncLoaderProperty);
        set => SetValue(OptionsAsyncLoaderProperty, value);
    }
    
    private bool _isLoading;

    /// <summary>
    /// <see langword="true" /> while the control waits for candidates: from when <see cref="Populating" /> is
    /// raised or an asynchronous load starts until the load finished or the pending populate was canceled.
    /// Meanwhile the popup shows a loading indicator in place of the candidate list.
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        internal set => SetAndRaise(IsLoadingProperty, ref _isLoading, value);
    }
    
    /// <summary>
    /// Number of candidates the list shows before it scrolls. The popup is at most as high as this many candidate
    /// rows plus the popup padding. The default is 10.
    /// </summary>
    public int DisplayCandidateCount
    {
        get => GetValue(DisplayCandidateCountProperty);
        set => SetValue(DisplayCandidateCountProperty, value);
    }
    
    /// <summary>
    /// Makes the input read-only while <see langword="true" />. The inner text area then cannot take focus, so
    /// tab navigation skips the input.
    /// </summary>
    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }
    
    /// <summary>
    /// Decides whether a candidate matches the text typed after the trigger. It receives the candidate's filter
    /// text, see <see cref="FilterValueSelector" />, and the typed text, which is never blank. When it is
    /// <see langword="null" />, the control keeps a candidate when its filter text contains the typed text.
    /// The filter also runs over candidates that <see cref="OptionsAsyncLoader" /> loaded.
    /// </summary>
    public IValueFilter? Filter
    {
        get => GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }
    
    /// <summary>
    /// Function that returns the text of a candidate that <see cref="Filter" /> matches against the typed text.
    /// When it is <see langword="null" />, the control uses the first non-<see langword="null" /> of the option's
    /// Header, Value and Key.
    /// </summary>
    public DefaultFilterValueSelector? FilterValueSelector
    {
        get => GetValue(FilterValueSelectorProperty);
        set => SetValue(FilterValueSelectorProperty, value);
    }
    
    /// <summary>
    /// Time the control waits before it populates the candidate list after the list opened. A new request that
    /// arrives during the wait restarts it. The default, <see cref="TimeSpan.Zero" />, populates immediately.
    /// Negative values are rejected.
    /// </summary>
    public TimeSpan AsyncLoadDebounce
    {
        get => GetValue(AsyncLoadDebounceProperty);
        set => SetValue(AsyncLoadDebounceProperty, value);
    }

    /// <summary>
    /// Time after which a load started through <see cref="OptionsAsyncLoader" /> is canceled. When it elapses,
    /// <see cref="OptionsLoaded" /> receives a result with the timeout status and the candidates stay unchanged.
    /// The default is 10 seconds.
    /// </summary>
    public TimeSpan AsyncLoadTimeout
    {
        get => GetValue(AsyncLoadTimeoutProperty);
        set => SetValue(AsyncLoadTimeoutProperty, value);
    }
    
    /// <summary>
    /// Whether the candidate list is open. Setting it to <see langword="true" /> opens the list with the
    /// candidates that match the current filter text. The control resets it when a candidate is inserted, the
    /// list is canceled, the window is deactivated or the control becomes invisible or disabled. While
    /// <see cref="IsPopupPinnedOpen" /> is <see langword="true" />, setting it to <see langword="false" /> is
    /// undone.
    /// </summary>
    public bool IsDropDownOpen
    {
        get => GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }
    
    /// <summary>
    /// Keeps the candidate list open while <see langword="true" />: closing is refused, so setting
    /// <see cref="IsDropDownOpen" /> to <see langword="false" /> restores the previous value, and the popup
    /// does not pass pointer input on its surface through to the text area. Changing the value to
    /// <see langword="true" /> opens the list when it is closed. The value is relayed to the popup.
    /// </summary>
    public bool IsPopupPinnedOpen
    {
        get => GetValue(IsPopupPinnedOpenProperty);
        set => SetCurrentValue(IsPopupPinnedOpenProperty, value);
    }

    /// <summary>
    /// Size preset of the input area, for example <c>Small</c>, <c>Middle</c> or <c>Large</c>. The value is passed
    /// to the inner text area.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    #endregion

    #region 公共事件定义

    /// <summary>
    /// Raised with the outcome of each asynchronous load that was not superseded by a newer request, including
    /// a load that timed out or failed. It is raised before a successful result replaces
    /// <see cref="OptionsSource" />; any other outcome leaves the candidates unchanged.
    /// </summary>
    public event EventHandler<MentionOptionsLoadedEventArgs>? OptionsLoaded;
    /// <summary>
    /// Raised when the candidate list is about to be populated, which is when it opens and after the
    /// <see cref="AsyncLoadDebounce" /> delay, while <see cref="OptionsAsyncLoader" /> is <see langword="null" />.
    /// <see cref="IsLoading" /> is <see langword="true" /> while the event is raised. A handler that cancels
    /// the arguments filters on its own and calls <see cref="PopulateComplete" /> to show the result;
    /// otherwise the control filters and raises <see cref="Populated" /> itself.
    /// </summary>
    public event EventHandler<MentionsPopulatingEventArgs>? Populating;
    /// <summary>
    /// Raised after the filtered candidates were given to the candidate list, at the end of
    /// <see cref="PopulateComplete" />. <see cref="IsLoading" /> is <see langword="false" /> by then. The
    /// arguments carry the filtered candidates.
    /// </summary>
    public event EventHandler<MentionsPopulatedEventArgs>? Populated;
    /// <summary>
    /// Raised after <see cref="IsDropDownOpen" /> changed to <see langword="true" /> and before the candidate
    /// list opens. Cancelling the arguments keeps the list closed and restores <see cref="IsDropDownOpen" />.
    /// </summary>
    public event EventHandler<CancelEventArgs>? DropDownOpening;
    /// <summary>
    /// Raised after the candidate list opened. Focus returns to the text area right after the event.
    /// </summary>
    public event EventHandler? DropDownOpened;
    /// <summary>
    /// Raised after <see cref="IsDropDownOpen" /> changed to <see langword="false" /> and before the candidate
    /// list closes. Cancelling the arguments keeps the list open and restores <see cref="IsDropDownOpen" />.
    /// </summary>
    public event EventHandler<CancelEventArgs>? DropDownClosing;
    /// <summary>
    /// Raised after the candidate list closed and a pending populate was canceled.
    /// </summary>
    public event EventHandler? DropDownClosed;
    /// <summary>
    /// Raised when a trigger character was typed in the text area, right before the control places and opens
    /// the candidate list. The arguments carry the trigger character.
    /// </summary>
    public event EventHandler<MentionCandidateTriggeredEventArgs>? CandidateTriggered;
    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<Mentions, string?> FilterValueProperty =
        AvaloniaProperty.RegisterDirect<Mentions, string?>(
            nameof(FilterValue),
            o => o.FilterValue,
            (o, v) => o.FilterValue = v);
    
    internal static readonly StyledProperty<bool> UsePopupLayoutTokenResourcesProperty =
        AvaloniaProperty.Register<Mentions, bool>(nameof(UsePopupLayoutTokenResources));
    
    internal static readonly DirectProperty<Mentions, double> MaxPopupHeightProperty =
        AvaloniaProperty.RegisterDirect<Mentions, double>(
            nameof(MaxPopupHeight),
            o => o.MaxPopupHeight,
            (o, v) => o.MaxPopupHeight = v);
    
    internal static readonly DirectProperty<Mentions, Thickness> PopupContentPaddingProperty =
        AvaloniaProperty.RegisterDirect<Mentions, Thickness>(nameof(PopupContentPadding),
            o => o.PopupContentPadding,
            (o, v) => o.PopupContentPadding = v);
    
    internal static readonly DirectProperty<Mentions, PlacementMode> PopupPlacementProperty =
        AvaloniaProperty.RegisterDirect<Mentions, PlacementMode>(nameof(PopupPlacement),
            o => o.PopupPlacement,
            (o, v) => o.PopupPlacement = v);
    
    internal static readonly StyledProperty<FormValidateFeedback?> FormFeedbackProperty = 
        AvaloniaProperty.Register<Mentions, FormValidateFeedback?>(nameof(FormFeedback));

    internal static readonly StyledProperty<FormValidateStatus> FormStatusProperty =
        InputControlState.FormStatusProperty.AddOwner<Mentions>();
    
    private string? _filterValue;

    internal string? FilterValue
    {
        get => _filterValue;
        set => SetAndRaise(FilterValueProperty, ref _filterValue, value);
    }
    
    internal bool UsePopupLayoutTokenResources
    {
        get => GetValue(UsePopupLayoutTokenResourcesProperty);
        set => SetValue(UsePopupLayoutTokenResourcesProperty, value);
    }
    private static readonly CandidatePopupLayoutTokenResourceKeys s_popupLayoutTokenResourceKeys =
        new(MentionsTokenKind.OptionHeight, MentionsTokenKind.PopupContentPadding);
    private OwnerTokenSet? _popupLayoutTokens;
    private double _itemHeight;
    
    private double _maxPopupHeight;

    internal double MaxPopupHeight
    {
        get => _maxPopupHeight;
        set => SetAndRaise(MaxPopupHeightProperty, ref _maxPopupHeight, value);
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
    
    /// <summary>
    /// The candidate list part of the template, found as <c>PART_CandidateList</c>. Assigning a list detaches
    /// the previous one and connects the new one to the control: its Commit and Cancel events and its items.
    /// Passing <see langword="null" /> only detaches the current list.
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
                if (_candidateList is Primitives.CandidateList oldCandidateList)
                {
                    oldCandidateList.ItemEnabledResolver = null;
                }
            }

            _candidateList = value;

            if (_candidateList != null)
            {
                _candidateList.Commit      += HandleCandidateListComplete;
                _candidateList.Cancel      += HandleCandidateListCanceled;
                if (_candidateList is Primitives.CandidateList newCandidateList)
                {
                    newCandidateList.ItemEnabledResolver = IsOptionEnabled;
                }
                _candidateList.ItemsSource =  _view;
            }
        }
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
    
    private static bool IsValidAsyncLoadDebounce(TimeSpan value) => value.TotalMilliseconds >= 0.0;

    // Shared by the candidate list (availability and container IsEnabled) and the open-time pre-selection.
    private static readonly Func<object?, bool> IsOptionEnabled = item => item is not IMentionOption { IsEnabled: false };

    private MentionTextArea? _textArea;
    private Popup? _popup;
    private CompositeDisposable? _subscriptionsOnOpen;
    private ICandidateList? _candidateList;
    private DispatcherTimer? _delayTimer;
    private List<IMentionOption>? _items;
    private IList<IMentionOption>? _view;
    private DropDownOpenChangeReason _dropDownOpenChangeReason;
    private long _populateRequestId;
    private IDisposable? _collectionChangeSubscription;
    private readonly AsyncSearchLoadCoordinator<string?, MentionOptionsLoadResult> _asyncLoadCoordinator = new();
    private IDisposable? _deactivationSubscription;
    private IDisposable? _popupPinnedOpenBinding;

    private enum DropDownOpenChangeReason
    {
        None,
        InternalSync
    }
    
    static Mentions()
    {
        FocusableProperty.OverrideDefaultValue<Mentions>(true);
        // The inner text area is the keyboard stop; the host would only add a stop with no visual.
        IsTabStopProperty.OverrideDefaultValue<Mentions>(false);
        LinesProperty.OverrideDefaultValue<Mentions>(1);
        
        IsDropDownOpenProperty.Changed.AddClassHandler<Mentions>((mentions,e) => mentions.HandleIsDropDownOpenChanged(e));
        AsyncLoadDebounceProperty.Changed.AddClassHandler<Mentions>((mentions,e) => mentions.HandleAsyncLoadDebounceChanged(e));
        PlacementProperty.Changed.AddClassHandler<Mentions>((mentions,e) => mentions.ConfigurePopupPlacement());
        OptionsSourceProperty.Changed.AddClassHandler<Mentions>((mentions,e) => mentions.HandleItemsSourceChanged((IEnumerable?)e.NewValue));
        FilterValueProperty.Changed.AddClassHandler<Mentions>((mentions,e) => mentions.HandleFilterValueChanged());
        ValueProperty.Changed.AddClassHandler<Mentions>((mentions, args) => mentions.HandleValueChanged());
    }
    
    public Mentions()
    {
    }

    #region 实现 FormItem 接口

    private EventHandler? _formValueChanged;

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
    private void HandleValueChanged()
    {
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Writes the value that a FormItem hands over into the input by setting <see cref="Value" /> with
    /// <c>SetCurrentValue</c>, which keeps a binding on <see cref="Value" /> in place. Call the base method
    /// to apply the value.
    /// </summary>
    /// <param name="value">
    /// The text to show, or <see langword="null" /> to clear the input.
    /// </param>
    protected virtual void NotifySetFormValue(string? value)
    {
        SetCurrentValue(ValueProperty, value);
    }

    /// <summary>
    /// Reads the value that a FormItem validates and submits.
    /// </summary>
    /// <returns>
    /// The current <see cref="Value" />.
    /// </returns>
    protected virtual string? NotifyGetFormValue()
    {
        return Value;
    }

    /// <summary>
    /// Clears the input when a FormItem asks for it: <see cref="Value" /> is set to <see langword="null" />
    /// with <c>SetCurrentValue</c>, which keeps a binding on <see cref="Value" /> in place. Call the base
    /// method to clear the input.
    /// </summary>
    protected virtual void NotifyClearFormValue()
    {
        SetCurrentValue(ValueProperty, null);
    }

    /// <summary>
    /// Stores the validation result that a FormItem reports in <c>FormStatus</c>; an unchanged status is not
    /// set again. Call the base method to apply the status.
    /// </summary>
    /// <param name="status">
    /// The validation status of the FormItem.
    /// </param>
    protected virtual void NotifyValidateStatus(FormValidateStatus status)
    {
        if (FormStatus != status)
        {
            SetCurrentValue(FormStatusProperty, status);
        }
    }

    /// <summary>
    /// Stores the feedback control that a FormItem hands over, which the theme passes to the inner text area.
    /// Call the base method to apply the value.
    /// </summary>
    /// <param name="value">
    /// The feedback control, or <see langword="null" /> to remove the current one.
    /// </param>
    protected virtual void NotifySetFeedBackControl(FormValidateFeedback? value)
    {
        FormFeedback = value;
    }

    #endregion

    #region 生命周期

    /// <summary>
    /// Assigns <see cref="DefaultValue" /> to <see cref="Value" /> while <see cref="Value" /> is
    /// <see langword="null" />, a list containing only <c>@</c> to a <see langword="null" />
    /// <see cref="TriggerPrefix" /> and a <c>CloseCircleFilled</c> icon to a <see langword="null" />
    /// <see cref="ClearIcon" />, and then aligns the popup placement with <see cref="Placement" />.
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (Value == null)
        {
            SetCurrentValue(ValueProperty, DefaultValue);
        }

        if (TriggerPrefix == null)
        {
            SetCurrentValue(TriggerPrefixProperty, ["@"]);
        }

        if (ClearIcon == null)
        {
            SetCurrentValue(ClearIconProperty, new CloseCircleFilled());
        }
        ConfigurePopupPlacement();
    }

    /// <summary>
    /// Connects the template parts to the control: the text area (<c>PART_TextArea</c>) reports trigger
    /// requests to it, the candidate list (<c>PART_CandidateList</c>) shows the filtered candidates, and the
    /// popup (<c>PART_Popup</c>) follows <see cref="IsPopupPinnedOpen" />. Parts of a previous template are
    /// released first. When <see cref="IsDropDownOpen" /> is <see langword="true" />, the popup is opened
    /// again.
    /// </summary>
    /// <param name="e">
    /// The arguments of the event.
    /// </param>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_textArea != null)
        {
            _textArea.CandidateOpenRequest  -= HandleCandidateOpenRequest;
            _textArea.CandidateCloseRequest -= HandleCandidateCloseRequest;
            _textArea.Owner                 =  null;
        }

        if (_popup != null)
        {
            _popupPinnedOpenBinding?.Dispose();
            _popupPinnedOpenBinding = null;
            _popup.Opened -= HandlePopupOpened;
            _popup.Closed -= HandlePopupClosed;
        }
        
        _textArea     = e.NameScope.Find<MentionTextArea>("PART_TextArea");
        _popup        = e.NameScope.Find<Popup>("PART_Popup");
        CandidateList = e.NameScope.Find<ICandidateList>("PART_CandidateList");

        if (_textArea != null)
        {
            _textArea.CandidateOpenRequest  += HandleCandidateOpenRequest;
            _textArea.CandidateCloseRequest += HandleCandidateCloseRequest;
            _textArea.Owner                 =  this;
        }

        if (_popup != null)
        {
            _popupPinnedOpenBinding = BindUtils.RelayBind(
                this,
                IsPopupPinnedOpenProperty,
                _popup,
                Popup.IsPopupPinnedOpenProperty);
            _popup.Opened              += HandlePopupOpened;
            _popup.Closed              += HandlePopupClosed;
            if (!IsPopupPinnedOpen)
            {
                _popup.OverlayInputPassThroughElement = _textArea;
            }

        }
        
        ConfigurePopupPlacement();
        if (IsDropDownOpen)
        {
            OpenDropDown();
            if (_view == null && !IsLoading)
            {
                SchedulePopulateDropDown();
            }
        }
    }

    /// <summary>
    /// Starts observing <see cref="OptionsSource" /> when it notifies of collection changes, caches its items
    /// and subscribes to the deactivation of the window. It opens the candidate list while
    /// <see cref="IsPopupPinnedOpen" /> is <see langword="true" /> and the list is closed.
    /// </summary>
    /// <param name="e">
    /// The arguments of the event.
    /// </param>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (_collectionChangeSubscription == null && OptionsSource is INotifyCollectionChanged notifyCollectionChanged)
        {
            _collectionChangeSubscription = notifyCollectionChanged.WeakSubscribe(ItemsCollectionChanged);
        }
        _items = OptionsSource == null ? null : BuildItemsCache(OptionsSource);

        _deactivationSubscription =
            TopLevelDeactivation.Subscribe(TopLevel.GetTopLevel(this), HandleWindowDeactivated);
        if (IsPopupPinnedOpen && !IsDropDownOpen)
        {
            SetCurrentValue(IsDropDownOpenProperty, true);
        }
    }

    /// <summary>
    /// Ends a pending populate, which is a running debounce delay or an asynchronous load, disposes the
    /// subscriptions taken on attach and while the popup was open, closes the popup and resets
    /// <see cref="IsDropDownOpen" /> without raising <see cref="DropDownClosing" />.
    /// </summary>
    /// <param name="e">
    /// The arguments of the event.
    /// </param>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        CancelPendingPopulate();
        _subscriptionsOnOpen?.Dispose();
        _subscriptionsOnOpen = null;
        _collectionChangeSubscription?.Dispose();
        _collectionChangeSubscription = null;
        _deactivationSubscription?.Dispose();
        _deactivationSubscription = null;
        _popup?.CloseForLifecycle();
        if (IsDropDownOpen)
        {
            SetDropDownOpenFromInternalSync(false);
        }
    }

    /// <summary>
    /// Moves focus that reaches the control itself to the inner text area, which is the input and shows the
    /// focus frame. The host takes focus only programmatically, because it is not a tab stop.
    /// </summary>
    /// <param name="e">
    /// The arguments of the focus change.
    /// </param>
    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);
        if (ReferenceEquals(e.Source, this) && _textArea is { } textArea)
        {
            textArea.Focus(e.NavigationMethod, e.KeyModifiers);
        }
    }

    /// <summary>
    /// Posts focus to the text area when the control is loaded while <see cref="IsAutoFocus" /> is
    /// <see langword="true" />, and synchronizes the candidate popup pseudo-class with
    /// <see cref="IsDropDownOpen" />.
    /// </summary>
    /// <param name="e">
    /// The arguments of the event.
    /// </param>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (IsAutoFocus)
        {
            Dispatcher.Post(() => _textArea?.Focus());
        }

        UpdatePseudoClasses();
    }

    /// <summary>
    /// Reacts to property changes that affect derived state: a changed <c>UsePopupLayoutTokenResources</c>
    /// refreshes the popup layout tokens, a changed <see cref="DisplayCandidateCount" /> or
    /// <c>PopupContentPadding</c> recalculates the maximum popup height, and <see cref="IsPopupPinnedOpen" />
    /// changing to <see langword="true" /> opens the candidate list.
    /// </summary>
    /// <param name="change">
    /// The arguments of the property change.
    /// </param>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == UsePopupLayoutTokenResourcesProperty)
        {
            UpdatePopupLayoutTokenSubscription();
        }
        else if (change.Property == DisplayCandidateCountProperty ||
                 change.Property == PopupContentPaddingProperty)
        {
            ConfigureMaxPopupHeight();
        }
        else if (change.Property == IsPopupPinnedOpenProperty)
        {
            if (change.GetNewValue<bool>() && !IsDropDownOpen)
            {
                SetCurrentValue(IsDropDownOpenProperty, true);
            }
        }
    }


    #endregion

    #region 属性变更处理

    private void HandleIsDropDownOpenChanged(AvaloniaPropertyChangedEventArgs e)
    {
        bool oldValue = (bool)e.OldValue!;
        bool newValue = (bool)e.NewValue!;

        if (_dropDownOpenChangeReason == DropDownOpenChangeReason.InternalSync)
        {
            UpdatePseudoClasses();
            return;
        }

        if (!newValue && IsPopupPinnedOpen)
        {
            SetDropDownOpenFromInternalSync(oldValue);
            return;
        }

        if (newValue)
        {
            if (OpeningDropDown(oldValue))
            {
                SchedulePopulateDropDown();
            }
        }
        else
        {
            ClosingDropDown(oldValue);
        }

        UpdatePseudoClasses();
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

    private void HandleItemsSourceChanged(IEnumerable? newValue)
    {
        // Remove handler for oldValue.CollectionChanged (if present)
        _collectionChangeSubscription?.Dispose();
        _collectionChangeSubscription = null;

        // Add handler for newValue.CollectionChanged (if possible)
        if (newValue is INotifyCollectionChanged newValueINotifyCollectionChanged)
        {
            _collectionChangeSubscription = newValueINotifyCollectionChanged.WeakSubscribe(ItemsCollectionChanged);
        }

        // Store a local cached copy of the data
        _items = newValue == null ? null : BuildItemsCache(newValue);

        // Clear and set the view shown in the candidate list.
        ClearView();
        if (IsDropDownOpen)
        {
            RefreshView();
        }
    }

    private void HandleFilterValueChanged()
    {
        if (IsDropDownOpen)
        {
            RefreshView();
        }
    }

    #endregion

    #region 布局和状态同步

    private void ConfigurePopupPlacement()
    {
        if (Placement == MentionsPlacementMode.Bottom)
        {
            PopupPlacement = PlacementMode.BottomEdgeAlignedLeft;
        }
        else
        {
            PopupPlacement = PlacementMode.TopEdgeAlignedLeft;
        }
    }

    /// <summary>
    /// Calculates the maximum popup height from <see cref="DisplayCandidateCount" />, the measured height of a
    /// candidate row and the popup content padding. The control calls it when one of these values changes.
    /// Call the base method to keep the height bound to these values.
    /// </summary>
    protected virtual void ConfigureMaxPopupHeight()
    {
        MaxPopupHeight = _itemHeight * DisplayCandidateCount + PopupContentPadding.Top + PopupContentPadding.Bottom;
    }

    private void UpdatePopupLayoutTokenSubscription()
    {
        _popupLayoutTokens?.Dispose();
        _popupLayoutTokens = UsePopupLayoutTokenResources ? s_popupLayoutTokenResourceKeys.Observe(this, RefreshPopupLayout) : null;
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

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(MentionPseudoClass.CandidatePopupOpen, IsDropDownOpen);
    }

    #endregion

    #region 下拉状态管理

    private void SetDropDownOpenFromInternalSync(bool value)
    {
        if (IsDropDownOpen == value)
        {
            UpdatePseudoClasses();
            return;
        }

        _dropDownOpenChangeReason = DropDownOpenChangeReason.InternalSync;
        try
        {
            SetCurrentValue(IsDropDownOpenProperty, value);
        }
        finally
        {
            _dropDownOpenChangeReason = DropDownOpenChangeReason.None;
        }
        UpdatePseudoClasses();
    }

    private void CancelPendingPopulate()
    {
        _delayTimer?.Stop();
        Interlocked.Increment(ref _populateRequestId);
        _asyncLoadCoordinator.Cancel();
        IsLoading = false;
    }

    private bool OpeningDropDown(bool oldValue)
    {
        var args = new CancelEventArgs();

        // Opening
        NotifyDropDownOpening(args);

        if (args.Cancel)
        {
            SetDropDownOpenFromInternalSync(oldValue);
            UpdatePseudoClasses();
            return false;
        }

        OpenDropDown();

        UpdatePseudoClasses();
        return true;
    }

    private void OpenDropDown()
    {
        if (_popup != null && !_popup.IsOpen)
        {
            _popup.IsOpen = true;
        }
    }

    private void ClosingDropDown(bool oldValue)
    {
        var args = new CancelEventArgs();
        NotifyDropDownClosing(args);

        if (args.Cancel)
        {
            SetDropDownOpenFromInternalSync(oldValue);
        }
        else
        {
            CloseDropDown();
        }

        UpdatePseudoClasses();
    }

    private void CloseDropDown()
    {
        CancelPendingPopulate();
        if (_popup?.IsOpen == true)
        {
            _popup.IsOpen = false;
        }
    }

    private void HandlePopupOpened(object? sender, EventArgs e)
    {
        _subscriptionsOnOpen?.Dispose();
        _subscriptionsOnOpen = new CompositeDisposable(2);
        this.GetObservable(IsVisibleProperty).Subscribe(HandleIsVisibleChanged).DisposeWith(_subscriptionsOnOpen);
        this.GetObservable(IsEnabledProperty).Subscribe(HandleIsEnabledChanged).DisposeWith(_subscriptionsOnOpen);
        this.SubscribeAncestorIsVisible(HandleIsVisibleChanged, _subscriptionsOnOpen);
        NotifyDropDownOpened(EventArgs.Empty);
        _textArea?.Focus();
    }

    private void HandlePopupClosed(object? sender, EventArgs e)
    {
        _subscriptionsOnOpen?.Dispose();
        _subscriptionsOnOpen = null;
        CancelPendingPopulate();
        if (IsDropDownOpen)
        {
            SetDropDownOpenFromInternalSync(false);
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

    private void HandleWindowDeactivated(object? sender, EventArgs e)
    {
        SetCurrentValue(IsDropDownOpenProperty, false);
    }

    /// <summary>
    /// Raises the <see cref="DropDownOpening" /> event. The control calls it after <see cref="IsDropDownOpen" />
    /// changed to <see langword="true" /> and before the candidate list opens. Cancelling the arguments keeps
    /// the list closed and restores <see cref="IsDropDownOpen" />. Call the base method to raise the event.
    /// </summary>
    /// <param name="eventArgs">
    /// The arguments that a handler can cancel.
    /// </param>
    protected virtual void NotifyDropDownOpening(CancelEventArgs eventArgs)
    {
        DropDownOpening?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Raises the <see cref="DropDownOpened" /> event. The control calls it after the candidate list opened and
    /// before focus returns to the text area. Call the base method to raise the event.
    /// </summary>
    /// <param name="eventArgs">
    /// The arguments passed to the handlers.
    /// </param>
    protected virtual void NotifyDropDownOpened(EventArgs eventArgs)
    {
        DropDownOpened?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Raises the <see cref="DropDownClosing" /> event. The control calls it after <see cref="IsDropDownOpen" />
    /// changed to <see langword="false" /> and before the candidate list closes. Cancelling the arguments keeps
    /// the list open and restores <see cref="IsDropDownOpen" />. Call the base method to raise the event.
    /// </summary>
    /// <param name="eventArgs">
    /// The arguments that a handler can cancel.
    /// </param>
    protected virtual void NotifyDropDownClosing(CancelEventArgs eventArgs)
    {
        DropDownClosing?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Raises the <see cref="DropDownClosed" /> event. The control calls it after the candidate list closed and
    /// a pending populate was canceled. Call the base method to raise the event.
    /// </summary>
    /// <param name="eventArgs">
    /// The arguments passed to the handlers.
    /// </param>
    protected virtual void NotifyDropDownClosed(EventArgs eventArgs)
    {
        DropDownClosed?.Invoke(this, eventArgs);
    }

    #endregion

    #region 候选交互

    private void HandleCandidateOpenRequest(object? sender, ShowMentionCandidateRequestEventArgs eventArgs)
    {
        CandidateTriggered?.Invoke(this, new MentionCandidateTriggeredEventArgs(eventArgs.TriggerChar));
        if (_popup != null && _textArea != null)
        {
            var triggerBounds       = _textArea.GetTriggerBounds(eventArgs.TriggerBounds, this);
            _popup.HorizontalOffset = triggerBounds.X;
            if (Placement == MentionsPlacementMode.Bottom)
            {
                _popup.VerticalOffset = Math.Min(-(DesiredSize.Height - triggerBounds.Y) + triggerBounds.Height, 0);
            }
            else
            {
                _popup.VerticalOffset = triggerBounds.Y;
            }
        }

        SetCurrentValue(IsDropDownOpenProperty, true);
    }

    private void HandleCandidateCloseRequest(object? sender, EventArgs eventArgs)
    {
        FilterValue = null;
        SetCurrentValue(IsDropDownOpenProperty, false);
        _textArea!.Focus();
    }

    private void HandleCandidateListComplete(object? sender, RoutedEventArgs e)
    {
        if (CandidateList!.SelectedItem is IMentionOption option)
        {
            InsertCandidateOption(option);
        }
        SetCurrentValue(IsDropDownOpenProperty, false);
        _textArea!.Focus();
    }

    private void HandleCandidateListCanceled(object? sender, RoutedEventArgs e)
    {
        FilterValue = null;
        SetCurrentValue(IsDropDownOpenProperty, false);
        _textArea!.Focus();
    }

    private void InsertCandidateOption(IMentionOption option)
    {
        Debug.Assert(_textArea != null);
        var value = option.Value?.ToString() ?? option.Header?.ToString() ?? string.Empty;
        _textArea?.InsertMentionOption(value, Split);
    }

    #endregion

    #region 键盘交互

    /// <summary>
    /// Handles keys that reach the control unhandled while it is enabled. While the candidate list is open, the
    /// list gets the key first, Escape cancels it and Enter inserts the selected candidate. While the list is
    /// closed, Down opens it unless the key is in use for directional focus navigation. F4 toggles the list.
    /// </summary>
    /// <param name="e">
    /// The arguments of the key event.
    /// </param>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        _ = e ?? throw new ArgumentNullException(nameof(e));
        base.OnKeyDown(e);
        if (e.Handled || !IsEnabled)
        {
            return;
        }

        // The drop down is open, pass the key event arguments to the candidate list. If the list does not handle
        // the key, the code below handles the simple navigation that controls the drop down.
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

    #endregion

    #region 异步加载

    private void SchedulePopulateDropDown()
    {
        if (_delayTimer != null)
        {
            _delayTimer.Stop();
            _delayTimer.Start();
        }
        else
        {
            PopulateDropDown(this, EventArgs.Empty);
        }
    }

    private void PopulateDropDown(object? sender, EventArgs e)
    {
        _delayTimer?.Stop();

        if (TryPopulateAsync(FilterValue))
        {
            return;
        }

        // A handler of Populating can cancel it and filter on its own: it assigns OptionsSource and calls
        // PopulateComplete to show the result.
        var populating = new MentionsPopulatingEventArgs(FilterValue);
        NotifyPopulating(populating);
        if (!populating.Cancel)
        {
            PopulateComplete();
        }
    }

    private bool TryPopulateAsync(string? searchText)
    {
        var loader = OptionsAsyncLoader;
        if (loader == null)
        {
            return false;
        }

        var requestId = Interlocked.Increment(ref _populateRequestId);
        _asyncLoadCoordinator.Timeout = AsyncLoadTimeout;
        IsLoading = true;
        _ = PopulateAsync(searchText, loader, requestId);
        return true;
    }

    private async Task PopulateAsync(string? filterValue, IMentionOptionsAsyncLoader loader, long requestId)
    {
        var outcome = await _asyncLoadCoordinator.LoadAsync(
            filterValue,
            (ctx, token) => loader.LoadAsync(ctx, token));

        if (outcome.IsSkipped)
        {
            return;
        }

        await Dispatcher.InvokeAsync(() => CompleteAsyncPopulate(filterValue, loader, requestId, outcome));
    }

    private void CompleteAsyncPopulate(
        string? filterValue,
        IMentionOptionsAsyncLoader loader,
        long requestId,
        AsyncLoadOutcome<MentionOptionsLoadResult> outcome)
    {
        if (Interlocked.Read(ref _populateRequestId) != requestId ||
            !ReferenceEquals(OptionsAsyncLoader, loader))
        {
            return;
        }

        IsLoading = false;

        if (outcome.IsSuccess && outcome.Result != null)
        {
            var result = outcome.Result;
            OptionsLoaded?.Invoke(this, new MentionOptionsLoadedEventArgs(filterValue, result));

            if (result.IsSuccess)
            {
                SetCurrentValue(OptionsSourceProperty, result.Data);
                PopulateComplete();
            }
            return;
        }

        var statusCode = outcome.Status switch
        {
            AsyncLoadStatus.TimedOut  => RpcStatusCode.Timeout,
            AsyncLoadStatus.Cancelled => RpcStatusCode.Cancelled,
            _                         => RpcStatusCode.Unknown
        };

        OptionsLoaded?.Invoke(this, new MentionOptionsLoadedEventArgs(filterValue, new MentionOptionsLoadResult()
        {
            UserFriendlyMessage = outcome.Error?.Message,
            StatusCode          = statusCode
        }));
    }

    /// <summary>
    /// Finishes a populate sequence: it filters <see cref="OptionsSource" /> with <see cref="Filter" /> and the
    /// text typed after the trigger, gives the result to the candidate list and raises <see cref="Populated" />.
    /// The control calls it itself after <see cref="Populating" /> when that was not canceled, and after a
    /// successful asynchronous load. A handler that cancels <see cref="Populating" /> calls it after providing
    /// the candidates, for example through <see cref="OptionsSource" />.
    /// </summary>
    public void PopulateComplete()
    {
        // Apply the search filter
        RefreshView();

        // Fire the Populated event containing the read-only view data.
        var populated = new MentionsPopulatedEventArgs(_view);
        NotifyPopulated(populated);
    }

    /// <summary>
    /// Raises the <see cref="Populating" /> event and sets <see cref="IsLoading" /> to <see langword="true" />
    /// before doing so. The control calls it when the candidate list is populated and
    /// <see cref="OptionsAsyncLoader" /> is <see langword="null" />. Call the base method to raise the event.
    /// </summary>
    /// <param name="e">
    /// The arguments that a handler can cancel.
    /// </param>
    protected virtual void NotifyPopulating(MentionsPopulatingEventArgs e)
    {
        IsLoading = true;
        Populating?.Invoke(this, e);
    }

    /// <summary>
    /// Resets <see cref="IsLoading" /> to <see langword="false" /> and then raises the <see cref="Populated" />
    /// event. The control calls it from <see cref="PopulateComplete" />.
    /// Call the base method to raise the event.
    /// </summary>
    /// <param name="e">
    /// The arguments that carry the filtered candidates.
    /// </param>
    protected virtual void NotifyPopulated(MentionsPopulatedEventArgs e)
    {
        IsLoading = false;
        Populated?.Invoke(this, e);
    }

    #endregion

    #region 数据视图

    private void ItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_items == null)
        {
            _items = OptionsSource == null ? null : BuildItemsCache(OptionsSource);
            RefreshView();
            return;
        }

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Remove when e.OldItems != null:
                for (var index = 0; index < e.OldItems.Count; index++)
                {
                    _items.RemoveAt(e.OldStartingIndex);
                }
                break;
            case NotifyCollectionChangedAction.Add when e.NewItems != null:
                for (var index = 0; index < e.NewItems.Count; index++)
                {
                    _items.Insert(e.NewStartingIndex + index, (IMentionOption)e.NewItems[index]!);
                }
                break;
            case NotifyCollectionChangedAction.Replace when e.NewItems != null:
                for (var index = 0; index < e.NewItems.Count; index++)
                {
                    _items[e.NewStartingIndex + index] = (IMentionOption)e.NewItems[index]!;
                }
                break;
            case NotifyCollectionChangedAction.Move when e.OldItems != null:
                var movedItems = e.OldItems.Cast<IMentionOption>().ToList();
                for (var index = 0; index < movedItems.Count; index++)
                {
                    _items.RemoveAt(e.OldStartingIndex);
                }
                for (var index = 0; index < movedItems.Count; index++)
                {
                    _items.Insert(e.NewStartingIndex + index, movedItems[index]);
                }
                break;
            case NotifyCollectionChangedAction.Reset:
                _items = OptionsSource == null ? null : BuildItemsCache(OptionsSource);
                break;
        }

        // Refresh the collection shown in the candidate list.
        RefreshView();
    }

    private static List<IMentionOption> BuildItemsCache(IEnumerable source)
    {
        var items = source switch
        {
            ICollection collection => new List<IMentionOption>(collection.Count),
            IReadOnlyCollection<IMentionOption> collection => new List<IMentionOption>(collection.Count),
            _ => new List<IMentionOption>()
        };
        foreach (var item in source)
        {
            items.Add((IMentionOption)item!);
        }
        return items;
    }

    private void ClearView()
    {
        _view = null;
        if (_candidateList != null)
        {
            _candidateList.ItemsSource = null;
            _candidateList.SelectedItem = null;
        }
    }

    private void RefreshView()
    {
        if (_items == null)
        {
            ClearView();
            return;
        }

        // Determine if any filtering mode is on
        var filter = Filter ?? ValueFilterFactory.BuildFilter(ValueFilterMode.Contains);
        Debug.Assert(filter != null);
        var items = _items;

        // cache properties
        var newViewItems = new Collection<IMentionOption>();

        foreach (var item in items)
        {
            bool inResults = string.IsNullOrWhiteSpace(FilterValue) || filter.Filter(GetValueByOption(item), FilterValue);

            if (inResults)
            {
                newViewItems.Add(item);
            }
        }

        _view = newViewItems;

        if (_candidateList != null)
        {
            if (_candidateList.ItemsSource != _view)
            {
                _candidateList.ItemsSource = _view;
            }
            _candidateList.SelectedItem = FindFirstEnabledOption(_view);
        }
    }

    private static IMentionOption? FindFirstEnabledOption(IList<IMentionOption> options)
    {
        foreach (var option in options)
        {
            if (IsOptionEnabled(option))
            {
                return option;
            }
        }

        return null;
    }

    private string? GetValueByOption(IMentionOption option)
    {
        string? value = null;
        if (FilterValueSelector != null)
        {
            value = FilterValueSelector(option)?.ToString();
        }
        else
        {
            value = option.Header?.ToString() ?? option.Value?.ToString() ?? option.Key;
        }
        return value;
    }

    #endregion

    #region 文本区域指针协作

    internal void NotifyTextAreaPointerPressed(PointerPressedEventArgs e)
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
            if (e.Source is Control sourceControl)
            {
                var textArea = sourceControl.FindAncestorOfType<MentionTextArea>();
                if (textArea != null)
                {
                    return;
                }
            }
            else
            {
                IsDropDownOpen = false;
                e.Handled      = true;
            }
        }
        else
        {
            PseudoClasses.Set(StdPseudoClass.Pressed, true);
        }
    }

    internal void NotifyTextAreaPointerReleased(PointerReleasedEventArgs e)
    {
        if (!e.Handled && e.Source is Visual source)
        {
            if (_popup?.IsInsidePopup(source) == true || PseudoClasses.Contains(StdPseudoClass.Pressed))
            {
                e.Handled = true;
            }
        }

        PseudoClasses.Set(StdPseudoClass.Pressed, false);
        base.OnPointerReleased(e);
    }

    #endregion
}
