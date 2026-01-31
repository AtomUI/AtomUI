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
    
    public static readonly StyledProperty<bool> IsFilterEnabledProperty =
        Select.IsFilterEnabledProperty.AddOwner<SelectResultOptionsBox>();
    
    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        AvaloniaProperty.Register<SelectResultOptionsBox, bool>(nameof(IsDropDownOpen));
    
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<SelectResultOptionsBox>();
    
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
    
    public bool IsResponsiveTagMode
    {
        get => GetValue(IsResponsiveTagModeProperty);
        set => SetValue(IsResponsiveTagModeProperty, value);
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

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _defaultPanel  = e.NameScope.Find<WrapPanel>(SelectResultOptionsBoxThemeConstants.DefaultPanelPart);
        _maxCountAwarePanel = e.NameScope.Find<SelectMaxTagAwarePanel>(SelectResultOptionsBoxThemeConstants.MaxCountAwarePanelPart);
        _searchTextBox = new SelectFilterTextBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        _collapsedInfoTag = new SelectRemainInfoTag
        {
            IsClosable = false
        };
        BindUtils.RelayBind(this, SizeTypeProperty, _searchTextBox, SizeTypeProperty);
        if (IsFilterEnabled)
        {
            if (Mode == SelectMode.Multiple)
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
                            TagText = option.Header?.ToString(),
                            Item    = option
                        };
                       
                        TagsBindingDisposables.Add(tag, BindUtils.RelayBind(this, SizeTypeProperty, tag, SizeTypeProperty));
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
                            TagText = option.Header?.ToString(),
                            Item    = option
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
}