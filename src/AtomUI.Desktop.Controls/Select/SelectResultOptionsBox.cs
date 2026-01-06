using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Desktop.Controls.Themes;
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
    
    public static readonly StyledProperty<bool> IsSearchEnabledProperty =
        Select.IsSearchEnabledProperty.AddOwner<SelectResultOptionsBox>();
    
    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        AvaloniaProperty.Register<SelectResultOptionsBox, bool>(nameof(IsDropDownOpen));
    
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<SelectResultOptionsBox>();
    
    public static readonly StyledProperty<int?> MaxTagCountProperty =
        Select.MaxTagCountProperty.AddOwner<SelectResultOptionsBox>();
    
    public static readonly StyledProperty<bool?> IsResponsiveMaxTagCountProperty =
        Select.IsResponsiveMaxTagCountProperty.AddOwner<SelectResultOptionsBox>();
    
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
    
    public bool IsSearchEnabled
    {
        get => GetValue(IsSearchEnabledProperty);
        set => SetValue(IsSearchEnabledProperty, value);
    }
    
    public bool IsDropDownOpen
    {
        get => GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }
    
    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    public int? MaxTagCount
    {
        get => GetValue(MaxTagCountProperty);
        set => SetValue(MaxTagCountProperty, value);
    }
    
    public bool? IsResponsiveMaxTagCount
    {
        get => GetValue(IsResponsiveMaxTagCountProperty);
        set => SetValue(IsResponsiveMaxTagCountProperty, value);
    }

    #endregion

    #region 内部属性定义

    public static readonly DirectProperty<SelectResultOptionsBox, bool> IsShowDefaultPanelProperty =
        AvaloniaProperty.RegisterDirect<SelectResultOptionsBox, bool>(
            nameof(IsShowDefaultPanel),
            o => o.IsShowDefaultPanel,
            (o, v) => o.IsShowDefaultPanel = v);
    
    private bool _isShowDefaultPanel;

    public bool IsShowDefaultPanel
    {
        get => _isShowDefaultPanel;
        set => SetAndRaise(IsShowDefaultPanelProperty, ref _isShowDefaultPanel, value);
    }
    #endregion

    private WrapPanel? _defaultPanel;
    private SelectMaxTagAwarePanel? _maxCountAwarePanel;
    private SelectFilterTextBox? _searchTextBox;
    private SelectRemainInfoTag? _collapsedInfoTag;
    private protected readonly Dictionary<object, IDisposable> TagsBindingDisposables = new();

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SelectedOptionsProperty)
        {
            HandleSelectedOptionsChanged();
        }
        else if (change.Property == IsSearchEnabledProperty ||
                 change.Property == ModeProperty)
        {
            ConfigureSearchTextControl();
        }
        else if (change.Property == IsDropDownOpenProperty)
        {
            ConfigureSearchTextReadOnly();
        }
        else if (change.Property == IsResponsiveMaxTagCountProperty)
        {
            if (IsResponsiveMaxTagCount == true)
            {
                SetCurrentValue(IsShowDefaultPanelProperty, false);
            }
            else
            {
                SetCurrentValue(IsShowDefaultPanelProperty, true);
            }
        }
        
        if (change.Property == IsShowDefaultPanelProperty)
        {
            _defaultPanel?.Children.Clear();
            _maxCountAwarePanel?.Children.Clear();
            HandleSelectedOptionsChanged();
        }
        
        if (change.Property == MaxTagCountProperty ||
            change.Property == SelectedOptionsProperty)
        {
            ConfigureMaxTagCountInfoVisible();
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

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _defaultPanel  = e.NameScope.Find<WrapPanel>(SelectResultOptionsBoxThemeConstants.DefaultPanelPart);
        _maxCountAwarePanel = e.NameScope.Find<SelectMaxTagAwarePanel>(SelectResultOptionsBoxThemeConstants.MaxCountAwarePanelPart);
        _searchTextBox = new SelectFilterTextBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        _collapsedInfoTag = new SelectRemainInfoTag()
        {
            IsClosable = false
        };
        BindUtils.RelayBind(this, SizeTypeProperty, _searchTextBox, SizeTypeProperty);
        if (IsSearchEnabled)
        {
            if (Mode == SelectMode.Multiple)
            {
                if (IsShowDefaultPanel)
                {
                    _defaultPanel?.Children.Add(_searchTextBox);
                }
            }
        }

        ConfigureSearchTextControl();
        HandleSelectedOptionsChanged();
        if (IsResponsiveMaxTagCount == true)
        {
            SetCurrentValue(IsShowDefaultPanelProperty, false);
        }
        else
        {
            SetCurrentValue(IsShowDefaultPanelProperty, true);
        }

        ConfigureMaxTagCountInfoVisible();
    }

    private void HandleSelectedOptionsChanged()
    {
        if (_isShowDefaultPanel)
        {
            if (_defaultPanel != null)
            {
                _searchTextBox?.Clear();
                _defaultPanel.Children.Clear();
                foreach (var entry in TagsBindingDisposables)
                {
                    entry.Value.Dispose();
                }
                TagsBindingDisposables.Clear();
                if (_selectedOptions != null)
                {
                    for (var i = 0; i < _selectedOptions.Count; i++)
                    {
                        var option = _selectedOptions[i];
                        var tag = new SelectTag
                        {
                            TagText = option.Header,
                            Option  = option
                        };
                        if (MaxTagCount.HasValue)
                        {
                            if (i < MaxTagCount)
                            {
                                tag.IsVisible = true;
                            }
                            else
                            {
                                tag.IsVisible = false;
                            }
                        }
                       
                        TagsBindingDisposables.Add(tag, BindUtils.RelayBind(this, SizeTypeProperty, tag, SizeTypeProperty));
                        _defaultPanel.Children.Add(tag);
                    }
                }
                
                if (_collapsedInfoTag != null)
                {
                    _defaultPanel.Children.Add(_collapsedInfoTag);
                }

                if (_searchTextBox != null)
                {
                    _defaultPanel.Children.Add(_searchTextBox);
                    _searchTextBox.Focus();
                }
            }
        }
        else
        {
            if (_maxCountAwarePanel != null)
            {
                _searchTextBox?.Clear();
                _maxCountAwarePanel.Children.Clear();
                foreach (var entry in TagsBindingDisposables)
                {
                    entry.Value.Dispose();
                }
                
                TagsBindingDisposables.Clear();
                if (_selectedOptions != null)
                {
                    foreach (var option in _selectedOptions)
                    {
                        var tag = new SelectTag
                        {
                            TagText = option.Header,
                            Option  = option
                        };
                        TagsBindingDisposables.Add(tag, BindUtils.RelayBind(this, SizeTypeProperty, tag, SizeTypeProperty));
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
                    _searchTextBox.Focus();
                }
            }
        }
    }
    
    private void ConfigureSearchTextControl()
    {
        if (_searchTextBox != null)
        {
            if (Mode == SelectMode.Multiple)
            {
                _searchTextBox.IsVisible = IsSearchEnabled;
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
            if (MaxTagCount != null && SelectedOptions != null && SelectedOptions.Count > 0 && MaxTagCount < SelectedOptions.Count)
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