using System.Collections;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

internal class SelectTagAwareTextBox : TemplatedControl
{
    #region 公共属性定义

    public static readonly DirectProperty<SelectTagAwareTextBox, IList?> SelectedItemsProperty =
        AvaloniaProperty.RegisterDirect<SelectTagAwareTextBox, IList?>(
            nameof(SelectedItems),
            o => o.SelectedItems,
            (o, v) => o.SelectedItems = v);

    public static readonly StyledProperty<bool> IsFilterEnabledProperty =
        Select.IsFilterEnabledProperty.AddOwner<SelectTagAwareTextBox>();

    public static readonly StyledProperty<bool> IsShowOverflowTipProperty =
        AbstractSelect.IsShowOverflowTipProperty.AddOwner<SelectTagAwareTextBox>();

    public static readonly StyledProperty<int> OverflowTipDelayProperty =
        AbstractSelect.OverflowTipDelayProperty.AddOwner<SelectTagAwareTextBox>();

    public static readonly StyledProperty<PlacementMode> OverflowTipPlacementProperty =
        AbstractSelect.OverflowTipPlacementProperty.AddOwner<SelectTagAwareTextBox>();

    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        AvaloniaProperty.Register<SelectTagAwareTextBox, bool>(nameof(IsDropDownOpen));

    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<SelectTagAwareTextBox>();

    public static readonly StyledProperty<int?> MaxTagCountProperty =
        Select.MaxTagCountProperty.AddOwner<SelectTagAwareTextBox>();

    public static readonly StyledProperty<bool> IsResponsiveTagModeProperty =
        Select.IsResponsiveTagModeProperty.AddOwner<SelectResultOptionsBox>();

    private IList? _selectedItems;

    public IList? SelectedItems
    {
        get => _selectedItems;
        set => SetAndRaise(SelectedItemsProperty, ref _selectedItems, value);
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

    #region 内部属性定义

    internal static readonly StyledProperty<double> CustomControlHeightProperty =
        AddOnDecoratedBox.CustomControlHeightProperty.AddOwner<SelectTagAwareTextBox>();

    internal static readonly StyledProperty<Thickness> ContentFramePaddingProperty =
        AddOnDecoratedBox.ContentFramePaddingProperty.AddOwner<SelectTagAwareTextBox>();

    internal static readonly StyledProperty<double> ContentMinHeightProperty =
        AddOnDecoratedBox.ContentMinHeightProperty.AddOwner<SelectTagAwareTextBox>();

    internal static readonly StyledProperty<Thickness> InputBorderThicknessProperty =
        AvaloniaProperty.Register<SelectTagAwareTextBox, Thickness>(nameof(InputBorderThickness));

    internal static readonly DirectProperty<SelectTagAwareTextBox, double> EffectiveTagHeightProperty =
        AvaloniaProperty.RegisterDirect<SelectTagAwareTextBox, double>(
            nameof(EffectiveTagHeight),
            o => o.EffectiveTagHeight,
            (o, v) => o.EffectiveTagHeight = v);

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

    private double _effectiveTagHeight = double.NaN;

    internal double EffectiveTagHeight
    {
        get => _effectiveTagHeight;
        set => SetAndRaise(EffectiveTagHeightProperty, ref _effectiveTagHeight, value);
    }

    #endregion

    #region 内部协作 API

    /// 多选标签容器标记：Select 族共享，宿主（如 Cascader）的 item 部件经
    /// descendant 路由命中该标记。
    internal const string TagItemClass = "semantic-item";

    /// 多选模式行内搜索输入标记：搜索框运行时创建，TreeSelect/Cascader 的
    /// input 部件经模板路由命中该标记。
    internal const string TagSearchInputClass = "semantic-input";

    #endregion

    private WrapPanel? _defaultPanel;
    private SelectMaxTagAwarePanel? _maxCountAwarePanel;
    private SelectFilterTextBox? _searchTextBox;
    private SelectRemainInfoTag? _collapsedInfoTag;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SelectedItemsProperty)
        {
            HandleEffectiveSelectedItemsChanged();
        }
        else if (change.Property == IsFilterEnabledProperty)
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
            HandleEffectiveSelectedItemsChanged();
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
            change.Property == SelectedItemsProperty)
        {
            ConfigureMaxTagCountInfoVisible();
        }

        if (change.Property == MaxTagCountProperty)
        {
            HandleEffectiveSelectedItemsChanged();
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

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _defaultPanel = e.NameScope.Find<WrapPanel>("PART_DefaultPanel");
        _maxCountAwarePanel = e.NameScope.Find<SelectMaxTagAwarePanel>("PART_MaxCountAwarePanel");
        _searchTextBox = new SelectFilterTextBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        // 多选/标签模式下的搜索输入是运行时创建的，静态模板中的 PART_SingleFilterInput
        // 在非 Single 模式保持隐藏，因此此处注入 input 部件 marker 以便语义高亮/样式可达。
        _searchTextBox.Classes.Add(TagSearchInputClass);
        _collapsedInfoTag = new SelectRemainInfoTag()
        {
            IsClosable = false
        };
        BindTagMetrics(_collapsedInfoTag);
        // This runtime-created content node has no TemplatedParent; TemplateBinding cannot
        // provide the preparation-time owner binding and its local-value priority.
        _searchTextBox[!FontSizeProperty]   = this[!FontSizeProperty];
        _searchTextBox[!FontFamilyProperty] = this[!FontFamilyProperty];
        _searchTextBox[!FontStyleProperty]  = this[!FontStyleProperty];
        _searchTextBox[!FontWeightProperty] = this[!FontWeightProperty];
        if (IsFilterEnabled)
        {
            if (!IsResponsiveTagMode)
            {
                _defaultPanel?.Children.Add(_searchTextBox);
            }
        }

        ConfigureSearchTextControl();
        HandleEffectiveSelectedItemsChanged();
        ConfigureMaxTagCountInfoVisible();
    }

    private void HandleEffectiveSelectedItemsChanged()
    {
        Panel? panel = IsResponsiveTagMode ? _maxCountAwarePanel : _defaultPanel;
        if (panel == null)
        {
            return;
        }
        _searchTextBox?.Clear();
        // Keep the input attached so rebuilding tags does not lose focus or the matching KeyUp.
        foreach (var child in panel.Children.ToArray())
        {
            if (!ReferenceEquals(child, _searchTextBox))
            {
                panel.Children.Remove(child);
            }
        }
        var index = 0;
        if (_selectedItems != null)
        {
            foreach (var item in _selectedItems)
            {
                if (item is ISelectTagTextProvider tagTextProvider)
                {
                    var tag = new SelectTag
                    {
                        Text = tagTextProvider.TagText,
                        Item = item
                    };
                    tag.Classes.Add(TagItemClass);
                    BindTagMetrics(tag);
                    panel.Children.Insert(index++, tag);
                }
            }
        }
        if (IsResponsiveTagMode && _collapsedInfoTag != null)
        {
            panel.Children.Insert(index, _collapsedInfoTag);
        }
        if (_searchTextBox != null && !ReferenceEquals(_searchTextBox.Parent, panel))
        {
            panel.Children.Add(_searchTextBox);
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
            _searchTextBox.IsVisible = IsFilterEnabled;
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
                if (SelectedItems != null && SelectedItems.Count > 0 && MaxTagCount < SelectedItems.Count)
                {
                    _collapsedInfoTag.IsVisible = true;
                    _collapsedInfoTag.SetRemainText(SelectedItems.Count - MaxTagCount.Value);
                }
                else
                {
                    _collapsedInfoTag.IsVisible = false;
                }
            }
            else
            {
                _collapsedInfoTag.IsVisible = SelectedItems != null && SelectedItems.Count > 0;
            }
        }
    }

    private static bool DoubleEquals(double lhs, double rhs)
    {
        return double.IsNaN(lhs) && double.IsNaN(rhs) ||
               Math.Abs(lhs - rhs) < 0.001;
    }
}
