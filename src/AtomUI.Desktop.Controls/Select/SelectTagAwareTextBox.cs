using System.Collections;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Desktop.Controls.Themes;
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
    
    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        AvaloniaProperty.Register<SelectTagAwareTextBox, bool>(nameof(IsDropDownOpen));
    
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<SelectTagAwareTextBox>();
    
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
        _defaultPanel  = e.NameScope.Find<WrapPanel>(SelectTagAwareTextBoxThemeConstants.DefaultPanelPart);
        _maxCountAwarePanel = e.NameScope.Find<SelectMaxTagAwarePanel>(SelectTagAwareTextBoxThemeConstants.MaxCountAwarePanelPart);
        _searchTextBox = new SelectFilterTextBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        _collapsedInfoTag = new SelectRemainInfoTag()
        {
            IsClosable = false
        };
        BindUtils.RelayBind(this, SizeTypeProperty, _searchTextBox, SizeTypeProperty);
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
                if (_selectedItems != null)
                {
                    for (var i = 0; i < _selectedItems.Count; i++)
                    {
                        var item = _selectedItems[i];
                        if (item is ISelectTagTextProvider tagTextProvider)
                        {
                            var tag = new SelectTag
                            {
                                TagText = tagTextProvider.TagText,
                                Item    = item
                            };
                            TagsBindingDisposables.Add(tag, BindUtils.RelayBind(this, SizeTypeProperty, tag, SizeTypeProperty));
                            _defaultPanel.Children.Add(tag);
                        }
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
                if (_selectedItems != null)
                {
                    foreach (var item in _selectedItems)
                    {
                        if (item is ISelectTagTextProvider tagTextProvider)
                        {
                            var tag = new SelectTag
                            {
                                TagText = tagTextProvider.TagText,
                                Item    = item
                            };
                            TagsBindingDisposables.Add(tag, BindUtils.RelayBind(this, SizeTypeProperty, tag, SizeTypeProperty));
                            _maxCountAwarePanel.Children.Add(tag);
                        }
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
        }
    }
}