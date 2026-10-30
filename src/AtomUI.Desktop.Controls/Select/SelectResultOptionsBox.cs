using AtomUI.Controls;
using AtomUI.Generated.AtomUIDesktopControls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

internal class SelectResultOptionsBox : TemplatedControl
{
    #region 公共属性定义

    public static readonly DirectProperty<SelectResultOptionsBox, IList<ISelectOption>?> SelectedOptionsProperty =
        AvaloniaProperty.RegisterDirect<SelectResultOptionsBox, IList<ISelectOption>?>(
            nameof(SelectedOptions),
            o => o.SelectedOptions,
            (o, v) => o.SelectedOptions = v);

    public static readonly StyledProperty<SelectMode> ModeProperty =
        AvaloniaProperty.Register<SelectResultOptionsBox, SelectMode>(nameof(Mode));

    public static readonly StyledProperty<bool> IsFilterEnabledProperty =
        Select.IsFilterEnabledProperty.AddOwner<SelectResultOptionsBox>();

    public static readonly StyledProperty<bool> IsShowOverflowTipProperty =
        AbstractSelect.IsShowOverflowTipProperty.AddOwner<SelectResultOptionsBox>();

    public static readonly StyledProperty<int> OverflowTipDelayProperty =
        AbstractSelect.OverflowTipDelayProperty.AddOwner<SelectResultOptionsBox>();

    public static readonly StyledProperty<PlacementMode> OverflowTipPlacementProperty =
        AbstractSelect.OverflowTipPlacementProperty.AddOwner<SelectResultOptionsBox>();

    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        AvaloniaProperty.Register<SelectResultOptionsBox, bool>(nameof(IsDropDownOpen));

    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<SelectResultOptionsBox>();

    internal static readonly StyledProperty<double> CustomControlHeightProperty =
        AddOnDecoratedBox.CustomControlHeightProperty.AddOwner<SelectResultOptionsBox>();

    internal static readonly StyledProperty<Thickness> ContentFramePaddingProperty =
        AddOnDecoratedBox.ContentFramePaddingProperty.AddOwner<SelectResultOptionsBox>();

    internal static readonly StyledProperty<double> ContentMinHeightProperty =
        AddOnDecoratedBox.ContentMinHeightProperty.AddOwner<SelectResultOptionsBox>();

    internal static readonly StyledProperty<Thickness> InputBorderThicknessProperty =
        AvaloniaProperty.Register<SelectResultOptionsBox, Thickness>(nameof(InputBorderThickness));

    public static readonly StyledProperty<int?> MaxTagCountProperty =
        Select.MaxTagCountProperty.AddOwner<SelectResultOptionsBox>();

    public static readonly StyledProperty<bool> IsResponsiveTagModeProperty =
        Select.IsResponsiveTagModeProperty.AddOwner<SelectResultOptionsBox>();

    private IList<ISelectOption>? _selectedOptions;

    public IList<ISelectOption>? SelectedOptions
    {
        get => _selectedOptions;
        set => SetAndRaise(SelectedOptionsProperty, ref _selectedOptions, value);
    }

    public SelectMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public bool IsFilterEnabled
    {
        get => GetValue(IsFilterEnabledProperty);
        set => SetValue(IsFilterEnabledProperty, value);
    }

    public bool IsShowOverflowTip
    {
        get => GetValue(IsShowOverflowTipProperty);
        set => SetValue(IsShowOverflowTipProperty, value);
    }

    public int OverflowTipDelay
    {
        get => GetValue(OverflowTipDelayProperty);
        set => SetValue(OverflowTipDelayProperty, value);
    }

    public PlacementMode OverflowTipPlacement
    {
        get => GetValue(OverflowTipPlacementProperty);
        set => SetValue(OverflowTipPlacementProperty, value);
    }

    public bool IsDropDownOpen
    {
        get => GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    internal double CustomControlHeight
    {
        get => GetValue(CustomControlHeightProperty);
        set => SetValue(CustomControlHeightProperty, value);
    }

    internal Thickness ContentFramePadding
    {
        get => GetValue(ContentFramePaddingProperty);
        set => SetValue(ContentFramePaddingProperty, value);
    }

    internal double ContentMinHeight
    {
        get => GetValue(ContentMinHeightProperty);
        set => SetValue(ContentMinHeightProperty, value);
    }

    internal Thickness InputBorderThickness
    {
        get => GetValue(InputBorderThicknessProperty);
        set => SetValue(InputBorderThicknessProperty, value);
    }

    public int? MaxTagCount
    {
        get => GetValue(MaxTagCountProperty);
        set => SetValue(MaxTagCountProperty, value);
    }

    public bool IsResponsiveTagMode
    {
        get => GetValue(IsResponsiveTagModeProperty);
        set => SetValue(IsResponsiveTagModeProperty, value);
    }

    #endregion

    internal static readonly DirectProperty<SelectResultOptionsBox, double> EffectiveTagHeightProperty =
        AvaloniaProperty.RegisterDirect<SelectResultOptionsBox, double>(
            nameof(EffectiveTagHeight),
            o => o.EffectiveTagHeight,
            (o, v) => o.EffectiveTagHeight = v);

    internal static readonly DirectProperty<SelectResultOptionsBox, bool> IsSearchInputEmptyProperty =
        AvaloniaProperty.RegisterDirect<SelectResultOptionsBox, bool>(
            nameof(IsSearchInputEmpty),
            o => o.IsSearchInputEmpty,
            (o, v) => o.IsSearchInputEmpty = v);

    private double _effectiveTagHeight = double.NaN;

    internal double EffectiveTagHeight
    {
        get => _effectiveTagHeight;
        set => SetAndRaise(EffectiveTagHeightProperty, ref _effectiveTagHeight, value);
    }

    private bool _isSearchInputEmpty = true;

    internal bool IsSearchInputEmpty
    {
        get => _isSearchInputEmpty;
        private set => SetAndRaise(IsSearchInputEmptyProperty, ref _isSearchInputEmpty, value);
    }

    private WrapPanel? _defaultPanel;
    private SelectMaxTagAwarePanel? _maxCountAwarePanel;
    private SelectFilterTextBox? _searchTextBox;
    private SelectRemainInfoTag? _collapsedInfoTag;
    private IDisposable? _searchInputEmptySubscription;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SelectedOptionsProperty)
        {
            HandleSelectedOptionsChanged();
        }
        else if (change.Property == IsFilterEnabledProperty ||
                 change.Property == ModeProperty)
        {
            ConfigureSearchTextControl();
        }
        else if (change.Property == IsDropDownOpenProperty)
        {
            ConfigureSearchTextReadOnly();
        }
        else if (change.Property == IsResponsiveTagModeProperty)
        {
            _defaultPanel?.Children.Clear();
            _maxCountAwarePanel?.Children.Clear();
            HandleSelectedOptionsChanged();
        }

        if (change.Property == SizeTypeProperty ||
            change.Property == CustomControlHeightProperty ||
            change.Property == ContentFramePaddingProperty ||
            change.Property == ContentMinHeightProperty ||
            change.Property == InputBorderThicknessProperty)
        {
            ConfigureEffectiveTagHeight();
        }

        if (change.Property == MaxTagCountProperty ||
            change.Property == SelectedOptionsProperty)
        {
            ConfigureMaxTagCountInfoVisible();
            if (Mode != SelectMode.Single)
            {
                _searchTextBox?.Focus();
            }
        }

        if (change.Property == MaxTagCountProperty)
        {
            HandleSelectedOptionsChanged();
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        e.Handled = false;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        e.Handled = false;
    }

    internal void RefreshSelectedOptions()
    {
        HandleSelectedOptionsChanged();
        ConfigureMaxTagCountInfoVisible();
        if (Mode != SelectMode.Single)
        {
            _searchTextBox?.Focus();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _searchInputEmptySubscription?.Dispose();
        _searchInputEmptySubscription = null;

        _defaultPanel = e.NameScope.Find<WrapPanel>("PART_DefaultPanel");
        _maxCountAwarePanel = e.NameScope.Find<SelectMaxTagAwarePanel>("PART_MaxCountAwarePanel");
        _searchTextBox = new SelectFilterTextBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        // 多选/标签模式下的搜索输入是运行时创建的，静态模板中的 PART_SingleFilterInput
        // 在非 Single 模式保持隐藏，因此此处注入 input 部件 marker 以便语义高亮/样式可达。
        _searchTextBox.Classes.Add(SelectSemanticParts.InputClass);
        _searchInputEmptySubscription =
            _searchTextBox.GetObservable(TextBox.IsPlaceholderTextVisibleProperty)
                          .Subscribe(isEmpty => IsSearchInputEmpty = isEmpty);

        _collapsedInfoTag = new SelectRemainInfoTag
        {
            IsClosable = false
        };
        BindTagMetrics(_collapsedInfoTag);

        _searchTextBox[!FontSizeProperty]   = this[!FontSizeProperty];
        _searchTextBox[!FontFamilyProperty] = this[!FontFamilyProperty];
        _searchTextBox[!FontStyleProperty]  = this[!FontStyleProperty];
        _searchTextBox[!FontWeightProperty] = this[!FontWeightProperty];
        if (IsFilterEnabled)
        {
            if (Mode != SelectMode.Single)
            {
                if (!IsResponsiveTagMode)
                {
                    _defaultPanel?.Children.Add(_searchTextBox);
                }
            }
        }

        ConfigureSearchTextControl();
        HandleSelectedOptionsChanged();
        ConfigureMaxTagCountInfoVisible();
    }

    private void HandleSelectedOptionsChanged()
    {
        if (!IsResponsiveTagMode)
        {
            if (_defaultPanel != null)
            {
                _searchTextBox?.Clear();
                _defaultPanel.Children.Clear();
                if (_selectedOptions != null)
                {
                    for (var i = 0; i < _selectedOptions.Count; i++)
                    {
                        var option = _selectedOptions[i];
                        var tag = new SelectTag
                        {
                            Text = option.Header?.ToString(),
                            Item    = option
                        };
                        tag.Classes.Add(SelectSemanticParts.ItemClass);
                        BindTagMetrics(tag);
                        _defaultPanel.Children.Add(tag);
                    }
                }

                if (_searchTextBox != null)
                {
                    _defaultPanel.Children.Add(_searchTextBox);
                }
            }
        }
        else
        {
            if (_maxCountAwarePanel != null)
            {
                _searchTextBox?.Clear();
                _maxCountAwarePanel.Children.Clear();
                if (_selectedOptions != null)
                {
                    foreach (var option in _selectedOptions)
                    {
                        var tag = new SelectTag
                        {
                            Text = option.Header?.ToString(),
                            Item    = option
                        };
                        tag.Classes.Add(SelectSemanticParts.ItemClass);
                        BindTagMetrics(tag);
                        _maxCountAwarePanel.Children.Add(tag);
                    }
                }

                if (_collapsedInfoTag != null)
                {
                    _maxCountAwarePanel.Children.Add(_collapsedInfoTag);
                }

                if (_searchTextBox != null)
                {
                    _maxCountAwarePanel.Children.Add(_searchTextBox);
                }
            }
        }
    }

    private void ConfigureEffectiveTagHeight()
    {
        var effectiveHeight = double.NaN;
        if (CustomizableSizeLayoutHelper.TryCalculateCustomContentHeight(
                SizeType,
                CustomControlHeight,
                ContentFramePadding,
                InputBorderThickness,
                ContentMinHeight,
                out var customHeight))
        {
            effectiveHeight = customHeight;
        }

        if (!DoubleEquals(EffectiveTagHeight, effectiveHeight))
        {
            EffectiveTagHeight = effectiveHeight;
        }
    }

    private void BindTagMetrics(SelectTag tag)
    {
        tag[!SizeTypeProperty]                  = this[!SizeTypeProperty];
        tag[!SelectTag.CustomTagHeightProperty] = this[!EffectiveTagHeightProperty];
        tag[!OverflowTip.IsEnabledProperty]     = this[!IsShowOverflowTipProperty];
        tag[!OverflowTip.TextProperty]          = tag[!SelectTag.TextProperty];
        tag[!OverflowTip.ShowDelayProperty]     = this[!OverflowTipDelayProperty];
        tag[!OverflowTip.PlacementProperty]     = this[!OverflowTipPlacementProperty];
    }

    private void ConfigureSearchTextControl()
    {
        if (_searchTextBox != null)
        {
            if (Mode != SelectMode.Single)
            {
                _searchTextBox.IsVisible = IsFilterEnabled;
            }
        }
    }

    private void ConfigureSearchTextReadOnly()
    {
        if (_searchTextBox != null)
        {
            if (IsDropDownOpen)
            {
                _searchTextBox.IsReadOnly = false;
            }
            else
            {
                _searchTextBox.Clear();
                _searchTextBox.IsReadOnly = true;
            }
        }
    }

    private void ConfigureMaxTagCountInfoVisible()
    {
        if (_collapsedInfoTag != null)
        {
            if (MaxTagCount != null)
            {
                if (SelectedOptions != null && SelectedOptions.Count > 0 && MaxTagCount < SelectedOptions.Count)
                {
                    _collapsedInfoTag.IsVisible = true;
                    _collapsedInfoTag.SetRemainText(SelectedOptions.Count - MaxTagCount.Value);
                }
                else
                {
                    _collapsedInfoTag.IsVisible = false;
                }
            }
        }
    }

    private static bool DoubleEquals(double lhs, double rhs)
    {
        return double.IsNaN(lhs) && double.IsNaN(rhs) ||
               Math.Abs(lhs - rhs) < 0.001;
    }
}
