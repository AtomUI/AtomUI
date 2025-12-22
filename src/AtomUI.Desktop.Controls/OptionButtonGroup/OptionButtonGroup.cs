using System.Collections.Specialized;
using System.Diagnostics;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Controls.Utils;
using AtomUI.Data;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

public class OptionCheckedChangedEventArgs : RoutedEventArgs
{
    public OptionCheckedChangedEventArgs(RoutedEvent routedEvent, OptionButton option, int index)
        : base(routedEvent)
    {
        CheckedOption = option;
        Index         = index;
    }

    public OptionButton CheckedOption { get; }
    public int Index { get; }
}

public class OptionButtonGroup : SelectingItemsControl,
                                 ISizeTypeAware,
                                 IWaveSpiritAwareControl,
                                 IControlSharedTokenResourcesHost
{
    #region 公共属性定义

    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<OptionButtonGroup>();

    public static readonly StyledProperty<OptionButtonStyle> ButtonStyleProperty =
        AvaloniaProperty.Register<OptionButtonGroup, OptionButtonStyle>(nameof(ButtonStyle));

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<OptionButtonGroup>();

    public static readonly StyledProperty<bool> IsWaveSpiritEnabledProperty =
        WaveSpiritAwareControlProperty.IsWaveSpiritEnabledProperty.AddOwner<OptionButtonGroup>();

    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    public OptionButtonStyle ButtonStyle
    {
        get => GetValue(ButtonStyleProperty);
        set => SetValue(ButtonStyleProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    public bool IsWaveSpiritEnabled
    {
        get => GetValue(IsWaveSpiritEnabledProperty);
        set => SetValue(IsWaveSpiritEnabledProperty, value);
    }
    
    #endregion

    #region 公共事件定义
    public static readonly RoutedEvent<OptionCheckedChangedEventArgs> OptionCheckedChangedEvent =
        RoutedEvent.Register<OptionButtonGroup, OptionCheckedChangedEventArgs>(
            nameof(OptionCheckedChanged),
            RoutingStrategies.Bubble);
    
    public event EventHandler<OptionCheckedChangedEventArgs>? OptionCheckedChanged
    {
        add => AddHandler(OptionCheckedChangedEvent, value);
        remove => RemoveHandler(OptionCheckedChangedEvent, value);
    }
    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<OptionButtonGroup, Thickness> EffectiveBorderThicknessProperty =
        AvaloniaProperty.RegisterDirect<OptionButtonGroup, Thickness>(nameof(EffectiveBorderThickness),
            o => o.EffectiveBorderThickness,
            (o, v) => o.EffectiveBorderThickness = v);

    internal static readonly StyledProperty<IBrush?> SelectedOptionBorderColorProperty =
        AvaloniaProperty.Register<OptionButtonGroup, IBrush?>(nameof(SelectedOptionBorderColor));

    private Thickness _effectiveBorderThickness;

    internal Thickness EffectiveBorderThickness
    {
        get => _effectiveBorderThickness;
        set => SetAndRaise(EffectiveBorderThicknessProperty, ref _effectiveBorderThickness, value);
    }

    internal IBrush? SelectedOptionBorderColor
    {
        get => GetValue(SelectedOptionBorderColorProperty);
        set => SetValue(SelectedOptionBorderColorProperty, value);
    }

    private static readonly FuncTemplate<Panel?> DefaultPanel =
        new(() => new StackPanel
        {
            Orientation = Orientation.Horizontal
        });
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => OptionButtonToken.ID;

    #endregion

    private readonly BorderRenderHelper _borderRenderHelper = new();
    private readonly Dictionary<OptionButton, CompositeDisposable> _itemsBindingDisposables = new();

    static OptionButtonGroup()
    {
        SelectionModeProperty.OverrideDefaultValue<OptionButtonGroup>(SelectionMode.Single | SelectionMode.AlwaysSelected);
        AutoScrollToSelectedItemProperty.OverrideDefaultValue<OptionButtonGroup>(false);
        ItemsPanelProperty.OverrideDefaultValue<OptionButtonGroup>(DefaultPanel);
        AffectsRender<OptionButtonGroup>(SelectionModeProperty);

        AffectsMeasure<OptionButtonGroup>(SizeTypeProperty);
        AffectsRender<OptionButtonGroup>(SelectedOptionBorderColorProperty,
            ButtonStyleProperty, SelectedItemProperty);
    }

    public OptionButtonGroup()
    {
        this.RegisterResources();
        LogicalChildren.CollectionChanged += HandleCollectionChanged;
        if (this is IChildIndexProvider childIndexProvider)
        {
            childIndexProvider.ChildIndexChanged += (sender, args) => { UpdateOptionButtonsPosition(); };
        }
    }
    
    private void HandleCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems.Count > 0)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is OptionButton optionButton)
                    {
                        if (_itemsBindingDisposables.TryGetValue(optionButton, out var disposable))
                        {
                            disposable.Dispose();
                            _itemsBindingDisposables.Remove(optionButton);
                        }
                    }
                }
            }
        }
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new OptionButton();
    }

    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        base.OnGotFocus(e);

        if (e.NavigationMethod == NavigationMethod.Directional && e.Source is OptionButton)
        {
            e.Handled = UpdateSelectionFromEventSource(e.Source);
        }
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is OptionButton optionButton)
        {
            var disposables = new CompositeDisposable(4);
            
            if (item != null && item is not Visual)
            {
                {
                    if (!optionButton.IsSet(OptionButton.ContentProperty))
                    {
                        if (ItemTemplate != null)
                        {
                            optionButton.SetCurrentValue(OptionButton.ContentProperty, item);
                            
                        }
                        else
                        {
                            if (item is IOptionButtonData optionButtonData)
                            {
                                optionButton.SetCurrentValue(OptionButton.ContentProperty, optionButtonData.Header);
                            }
                        }
                    }
                }

                {
                    if (item is IOptionButtonData optionButtonData)
                    {
                        if (!optionButton.IsSet(OptionButton.IsEnabledProperty))
                        {
                            optionButton.SetCurrentValue(OptionButton.IsEnabledProperty, optionButtonData.IsEnabled);
                        }
                        
                        if (!optionButton.IsSet(OptionButton.IconProperty))
                        {
                            optionButton.SetCurrentValue(OptionButton.IconProperty, optionButtonData.Icon);
                        }
                    }
                }
            }
            
            if (ItemTemplate != null)
            {
                disposables.Add(BindUtils.RelayBind(this, ItemTemplateProperty, optionButton, OptionButton.ContentTemplateProperty));
            }
            
            disposables.Add(BindUtils.RelayBind(this, SizeTypeProperty, optionButton, OptionButton.SizeTypeProperty));
            disposables.Add(BindUtils.RelayBind(this, ButtonStyleProperty, optionButton, OptionButton.ButtonStyleProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, optionButton, OptionButton.IsMotionEnabledProperty));
            disposables.Add(BindUtils.RelayBind(this, IsWaveSpiritEnabledProperty, optionButton,
                OptionButton.IsWaveSpiritEnabledProperty));
            
            PrepareOptionButton(optionButton, item, index, disposables);
            
            if (_itemsBindingDisposables.TryGetValue(optionButton, out var oldDisposables))
            {
                oldDisposables.Dispose();
                _itemsBindingDisposables.Remove(optionButton);
            }
            _itemsBindingDisposables.Add(optionButton, disposables);
            optionButton.IsCheckedChanged += HandleOptionButtonChecked;
        }  
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type OptionButton.");
        }
    }
    
    protected virtual void PrepareOptionButton(OptionButton optionButton, object? item, int index, CompositeDisposable compositeDisposable)
    {
    }

    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        base.ContainerForItemPreparedOverride(container, item, index);
        if (container is OptionButton optionButton)
        {
            if (optionButton.IsChecked.HasValue && optionButton.IsChecked.Value)
            {
                UpdateSelectionFromEventSource(optionButton);
                RaiseEvent(new OptionCheckedChangedEventArgs(OptionCheckedChangedEvent, optionButton,
                    index));
            }
        }
    }

    private void HandleOptionButtonChecked(object? sender, RoutedEventArgs args)
    {
        if (sender is OptionButton optionButton && optionButton.IsChecked.HasValue && optionButton.IsChecked.Value)
        {
            UpdateSelectionFromEventSource(args.Source);
            RaiseEvent(new OptionCheckedChangedEventArgs(OptionCheckedChangedEvent, optionButton,
                SelectedIndex));
        }
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<OptionButton>(item, out recycleKey);
    }

    protected override void ContainerIndexChangedOverride(Control container, int oldIndex, int newIndex)
    {
        if (container is OptionButton optionButton)
        {
            if (newIndex == 0)
            {
                optionButton.GroupPositionTrait = OptionButtonPositionTrait.First;
            }
            else if (newIndex == ItemCount - 1)
            {
                optionButton.GroupPositionTrait = OptionButtonPositionTrait.Last;
            }
            else
            {
                optionButton.GroupPositionTrait = OptionButtonPositionTrait.Middle;
            }
        }
    }

    private void UpdateOptionButtonsPosition()
    {
        for (var i = 0; i < Items.Count; i++)
        {
            var button = Items[i] as OptionButton;
            Debug.Assert(button != null);
            if (Items.Count > 1)
            {
                if (i == 0)
                {
                    button.GroupPositionTrait = OptionButtonPositionTrait.First;
                }
                else if (i == Items.Count - 1)
                {
                    button.GroupPositionTrait = OptionButtonPositionTrait.Last;
                }
                else
                {
                    button.GroupPositionTrait = OptionButtonPositionTrait.Middle;
                }
            }
        }
    }

    public override void Render(DrawingContext context)
    {
        _borderRenderHelper.Render(context,
            new Size(DesiredSize.Width, DesiredSize.Height),
            new Thickness(1),
            CornerRadius,
            BackgroundSizing.CenterBorder,
            null,
            BorderBrush);
        for (var i = 0; i < ItemCount; ++i)
        {
            var optionButton = ContainerFromIndex(i);
            Debug.Assert(optionButton != null);
            if (ButtonStyle == OptionButtonStyle.Solid)
            {
                if (i <= ItemCount - 2)
                {
                    var nextOption = ContainerFromIndex(i + 1);
                    if (nextOption == SelectedItem || optionButton == SelectedItem)
                    {
                        continue;
                    }
                }
            }

            if (i != ItemCount - 1)
            {
                var offsetX    = optionButton.Bounds.Right - BorderThickness.Left / 2;
                var startPoint = new Point(offsetX, 0);
                var endPoint   = new Point(offsetX, Bounds.Height);
                using var optionState = context.PushRenderOptions(new RenderOptions
                {
                    EdgeMode = EdgeMode.Aliased
                });
                context.DrawLine(new Pen(BorderBrush, BorderThickness.Left), startPoint, endPoint);
            }

            if (ButtonStyle == OptionButtonStyle.Outline)
            {
                if (IsEnabled && optionButton.IsEnabled && optionButton == SelectedItem)
                {
                    // 绘制选中边框
                    var offsetX = optionButton.Bounds.X;
                    var width   = optionButton.DesiredSize.Width;
                    if (i > 0)
                    {
                        offsetX -= BorderThickness.Left;
                        width   += BorderThickness.Left;
                    }

                    var       translationMatrix = Matrix.CreateTranslation(offsetX, 0);
                    using var state             = context.PushTransform(translationMatrix);
                    var       cornerRadius      = new CornerRadius(0);
                    if (i == 0)
                    {
                        cornerRadius = new CornerRadius(CornerRadius.TopLeft, 0, 0, CornerRadius.BottomLeft);
                    }
                    else if (i == ItemCount - 1)
                    {
                        cornerRadius = new CornerRadius(0, CornerRadius.TopRight, CornerRadius.BottomRight, 0);
                    }

                    _borderRenderHelper.Render(context,
                        new Size(width, DesiredSize.Height),
                        BorderThickness,
                        cornerRadius,
                        BackgroundSizing.InnerBorderEdge,
                        null,
                        SelectedOptionBorderColor);
                }
            }
        }
    }
}