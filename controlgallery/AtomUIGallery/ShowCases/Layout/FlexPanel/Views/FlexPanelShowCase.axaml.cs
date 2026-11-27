using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using AtomFlexPanel = AtomUI.Controls.FlexPanel;
using AtomScrollViewer = AtomUI.Desktop.Controls.ScrollViewer;
using AtomTextBlock = AtomUI.Desktop.Controls.TextBlock;
using AvaloniaSlider = Avalonia.Controls.Slider;
using Flex = AtomUI.Controls.Flex;
using FlexBasis = AtomUI.Controls.FlexBasis;
using FlexBasisKind = AtomUI.Controls.FlexBasisKind;
using FlexDirection = AtomUI.Controls.FlexDirection;
using FlexWrap = AtomUI.Controls.FlexWrap;
using NumericUpDown = AtomUI.Desktop.Controls.NumericUpDown;

namespace AtomUIGallery.ShowCases.FlexPanel;

public partial class FlexPanelShowCase : GalleryReactiveUserControl<FlexPanelViewModel>
{
    public const string LanguageId = nameof(FlexPanelShowCase);

    private const double GapSmallValue       = 8;
    private const double GapMiddleValue      = 16;
    private const double GapLargeValue       = 24;
    private const double BasisAbsoluteDefault = 60;

    private static readonly object DeferredTemplateInitialized = new();

    public FlexPanelShowCase()
    {
        InitializeComponent();
    }

    /// <inheritdoc />
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

    }

    private void InitializeBasicExample(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (!TryMarkInitialized(sender, out var root))
        {
            return;
        }

        var directionHorizontal = FindRequired<AtomUIRadioButton>(root, "DirectionHorizontal");
        var directionVertical   = FindRequired<AtomUIRadioButton>(root, "DirectionVertical");
        var basicFlexPanel      = FindRequired<AtomFlexPanel>(root, "BasicFlexPanel");
        var basicItem1          = FindRequired<Border>(root, "BasicItem1");
        var basicItem2          = FindRequired<Border>(root, "BasicItem2");
        var basicItem3          = FindRequired<Border>(root, "BasicItem3");
        var basicItem4          = FindRequired<Border>(root, "BasicItem4");

        ConfigureBasicItem(basicItem1);
        ConfigureBasicItem(basicItem2);
        ConfigureBasicItem(basicItem3);
        ConfigureBasicItem(basicItem4);
        SetBasicDirection(FlexDirection.Row);

        directionHorizontal.IsCheckedChanged += (_, _) =>
        {
            if (directionHorizontal.IsChecked == true)
            {
                SetBasicDirection(FlexDirection.Row);
            }
        };

        directionVertical.IsCheckedChanged += (_, _) =>
        {
            if (directionVertical.IsChecked == true)
            {
                SetBasicDirection(FlexDirection.Column);
            }
        };

        static void ConfigureBasicItem(Layoutable item)
        {
            Flex.SetGrow(item, 1);
        }

        void SetBasicDirection(FlexDirection direction)
        {
            basicFlexPanel.Direction  = direction;
            basicFlexPanel.AlignItems = direction == FlexDirection.Column
                ? AlignItems.FlexStart
                : AlignItems.Stretch;
            var grow = direction == FlexDirection.Row ? 1.0 : 0.0;
            Flex.SetGrow(basicItem1, grow);
            Flex.SetGrow(basicItem2, grow);
            Flex.SetGrow(basicItem3, grow);
            Flex.SetGrow(basicItem4, grow);
        }
    }

    private void InitializeAlignmentExample(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (!TryMarkInitialized(sender, out var root))
        {
            return;
        }

        var justifySegmented = FindRequired<AtomUISegmented>(root, "JustifySegmented");
        var alignSegmented   = FindRequired<AtomUISegmented>(root, "AlignSegmented");
        var alignFlexPanel   = FindRequired<AtomFlexPanel>(root, "AlignFlexPanel");

        justifySegmented.SelectionChanged += (_, _) =>
        {
            alignFlexPanel.JustifyContent = justifySegmented.SelectedIndex switch
            {
                0 => JustifyContent.FlexStart,
                1 => JustifyContent.Center,
                2 => JustifyContent.FlexEnd,
                3 => JustifyContent.SpaceBetween,
                4 => JustifyContent.SpaceAround,
                5 => JustifyContent.SpaceEvenly,
                _ => alignFlexPanel.JustifyContent
            };
        };

        alignSegmented.SelectionChanged += (_, _) =>
        {
            alignFlexPanel.AlignItems = alignSegmented.SelectedIndex switch
            {
                0 => AlignItems.FlexStart,
                1 => AlignItems.Center,
                2 => AlignItems.FlexEnd,
                3 => AlignItems.Stretch,
                _ => alignFlexPanel.AlignItems
            };
        };
    }

    private void InitializeGapExample(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (!TryMarkInitialized(sender, out var root))
        {
            return;
        }

        var gapSmall       = FindRequired<AtomUIRadioButton>(root, "GapSmall");
        var gapMiddle      = FindRequired<AtomUIRadioButton>(root, "GapMiddle");
        var gapLarge       = FindRequired<AtomUIRadioButton>(root, "GapLarge");
        var gapCustomize   = FindRequired<AtomUIRadioButton>(root, "GapCustomize");
        var gapCustomPanel = FindRequired<StackPanel>(root, "GapCustomPanel");
        var gapValueSlider = FindRequired<AvaloniaSlider>(root, "GapValueSlider");
        var gapFlexPanel   = FindRequired<AtomFlexPanel>(root, "GapFlexPanel");

        gapSmall.IsCheckedChanged += HandleGapRadioChanged;
        gapMiddle.IsCheckedChanged += HandleGapRadioChanged;
        gapLarge.IsCheckedChanged += HandleGapRadioChanged;
        gapCustomize.IsCheckedChanged += HandleGapRadioChanged;
        gapValueSlider.ValueChanged += (_, _) =>
        {
            if (gapCustomPanel.IsVisible)
            {
                SetGap(GetCustomGap());
            }
        };

        SetGap(GapSmallValue);

        void HandleGapRadioChanged(object? radioSender, RoutedEventArgs args)
        {
            if (radioSender is not AtomUIRadioButton { IsChecked: true } radioButton)
            {
                return;
            }

            if (ReferenceEquals(radioButton, gapSmall))
            {
                gapCustomPanel.IsVisible = false;
                SetGap(GapSmallValue);
            }
            else if (ReferenceEquals(radioButton, gapMiddle))
            {
                gapCustomPanel.IsVisible = false;
                SetGap(GapMiddleValue);
            }
            else if (ReferenceEquals(radioButton, gapLarge))
            {
                gapCustomPanel.IsVisible = false;
                SetGap(GapLargeValue);
            }
            else if (ReferenceEquals(radioButton, gapCustomize))
            {
                gapCustomPanel.IsVisible = true;
                SetGap(GetCustomGap());
            }
        }

        double GetCustomGap()
        {
            return Math.Max(0, gapValueSlider.Value);
        }

        void SetGap(double gap)
        {
            gapFlexPanel.ColumnSpacing = gap;
            gapFlexPanel.RowSpacing    = gap;
        }
    }

    private void InitializeAutoWrapExample(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (!TryMarkInitialized(sender, out var root))
        {
            return;
        }

        var wrapEnabled      = FindRequired<AtomUIRadioButton>(root, "WrapEnabled");
        var wrapDisabled     = FindRequired<AtomUIRadioButton>(root, "WrapDisabled");
        var wrapFlexPanel    = FindRequired<AtomFlexPanel>(root, "WrapFlexPanel");
        var wrapScrollViewer = FindRequired<AtomScrollViewer>(root, "WrapScrollViewer");

        wrapEnabled.IsCheckedChanged += (_, _) =>
        {
            if (wrapEnabled.IsChecked == true)
            {
                wrapFlexPanel.Wrap = FlexWrap.Wrap;
                wrapScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            }
        };

        wrapDisabled.IsCheckedChanged += (_, _) =>
        {
            if (wrapDisabled.IsChecked == true)
            {
                wrapFlexPanel.Wrap = FlexWrap.NoWrap;
                wrapScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            }
        };
    }

    private void InitializeAlignSelfExample(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (!TryMarkInitialized(sender, out var root))
        {
            return;
        }

        var alignSelfSegmented = FindRequired<AtomUISegmented>(root, "AlignSelfSegmented");
        var alignSelfTarget    = FindRequired<Border>(root, "AlignSelfTarget");

        alignSelfSegmented.SelectionChanged += (_, _) => UpdateAlignSelf();
        UpdateAlignSelf();

        void UpdateAlignSelf()
        {
            var align = alignSelfSegmented.SelectedIndex switch
            {
                0 => AlignItems.FlexStart,
                1 => AlignItems.Center,
                2 => AlignItems.FlexEnd,
                3 => AlignItems.Stretch,
                _ => AlignItems.FlexStart
            };
            Flex.SetAlignSelf(alignSelfTarget, align);
        }
    }

    private void InitializeOrderExample(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (!TryMarkInitialized(sender, out var root))
        {
            return;
        }

        var orderSegmented = FindRequired<AtomUISegmented>(root, "OrderSegmented");
        var orderItemA     = FindRequired<Border>(root, "OrderItemA");
        var orderItemB     = FindRequired<Border>(root, "OrderItemB");
        var orderItemC     = FindRequired<Border>(root, "OrderItemC");
        var orderItemD     = FindRequired<Border>(root, "OrderItemD");

        orderSegmented.SelectionChanged += (_, _) => ApplyOrderPreset(orderSegmented.SelectedIndex);
        ApplyOrderPreset(0);

        void ApplyOrderPreset(int index)
        {
            switch (index)
            {
                case 0:
                    Flex.SetOrder(orderItemA, 0);
                    Flex.SetOrder(orderItemB, 0);
                    Flex.SetOrder(orderItemC, 0);
                    Flex.SetOrder(orderItemD, 0);
                    break;
                case 1:
                    Flex.SetOrder(orderItemA, 3);
                    Flex.SetOrder(orderItemB, 2);
                    Flex.SetOrder(orderItemC, 1);
                    Flex.SetOrder(orderItemD, 0);
                    break;
                case 2:
                    Flex.SetOrder(orderItemA, 2);
                    Flex.SetOrder(orderItemB, -1);
                    Flex.SetOrder(orderItemC, 3);
                    Flex.SetOrder(orderItemD, 1);
                    break;
            }
        }
    }

    private void InitializeBasisExample(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (!TryMarkInitialized(sender, out var root))
        {
            return;
        }

        var basisKindSegmented = FindRequired<AtomUISegmented>(root, "BasisKindSegmented");
        var basisValuePanel    = FindRequired<StackPanel>(root, "BasisValuePanel");
        var basisValueInput    = FindRequired<NumericUpDown>(root, "BasisValueInput");
        var basisValueUnit     = FindRequired<AtomTextBlock>(root, "BasisValueUnit");
        var basisPercentPanel  = FindRequired<StackPanel>(root, "BasisPercentPanel");
        var basisValueSlider   = FindRequired<AvaloniaSlider>(root, "BasisValueSlider");
        var basisPercentText   = FindRequired<AtomTextBlock>(root, "BasisPercentText");
        var basisItem          = FindRequired<Border>(root, "BasisItem");
        var basisGrowItem1     = FindRequired<Border>(root, "BasisGrowItem1");
        var basisGrowItem2     = FindRequired<Border>(root, "BasisGrowItem2");

        var suppressBasisValueChanged = false;

        basisKindSegmented.SelectionChanged += (_, _) =>
        {
            suppressBasisValueChanged = true;
            if (basisKindSegmented.SelectedIndex == 2)
            {
                basisValueSlider.Value = 0;
                basisPercentText.Text  = "0%";
            }
            else if (basisKindSegmented.SelectedIndex == 1 && basisValueInput.Value is null)
            {
                basisValueInput.Value = (decimal)BasisAbsoluteDefault;
            }

            suppressBasisValueChanged = false;
            UpdateBasis();
        };

        basisValueInput.PropertyChanged += (_, args) =>
        {
            if (args.Property == NumericUpDown.ValueProperty && !suppressBasisValueChanged)
            {
                UpdateBasis();
            }
        };

        basisValueSlider.ValueChanged += (_, _) =>
        {
            if (suppressBasisValueChanged)
            {
                return;
            }

            basisPercentText.Text = $"{Math.Round(basisValueSlider.Value)}%";
            UpdateBasis();
        };

        basisValueInput.Value  = (decimal)BasisAbsoluteDefault;
        basisValueSlider.Value = 100;
        basisPercentText.Text  = "100%";
        UpdateBasis();

        void UpdateBasis()
        {
            var kindIndex = basisKindSegmented.SelectedIndex;
            var value     = Convert.ToDouble(basisValueInput.Value ?? 0);

            switch (kindIndex)
            {
                case 0:
                    basisValuePanel.IsVisible   = false;
                    basisPercentPanel.IsVisible = false;
                    Flex.SetBasis(basisItem, FlexBasis.Auto);
                    break;
                case 1:
                    basisValuePanel.IsVisible   = true;
                    basisPercentPanel.IsVisible = false;
                    basisValueInput.Minimum     = 0;
                    basisValueInput.Maximum     = 1000;
                    basisValueUnit.Text         = "px";
                    Flex.SetBasis(basisItem, new FlexBasis(Math.Max(0, value)));
                    break;
                case 2:
                    basisValuePanel.IsVisible   = false;
                    basisPercentPanel.IsVisible = true;
                    var sliderValue = Math.Max(0, Math.Min(100, basisValueSlider.Value));
                    basisPercentText.Text = $"{Math.Round(sliderValue)}%";
                    Flex.SetBasis(basisItem, new FlexBasis(sliderValue / 100, FlexBasisKind.Relative));
                    break;
            }

            Flex.SetGrow(basisItem, 0);
            Flex.SetGrow(basisGrowItem1, 1);
            Flex.SetGrow(basisGrowItem2, 1);
        }
    }

    private void InitializeGrowExample(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (!TryMarkInitialized(sender, out var root))
        {
            return;
        }

        var growInputA = FindRequired<NumericUpDown>(root, "GrowInputA");
        var growInputB = FindRequired<NumericUpDown>(root, "GrowInputB");
        var growItemA  = FindRequired<Border>(root, "GrowItemA");
        var growItemB  = FindRequired<Border>(root, "GrowItemB");
        var growItemC  = FindRequired<Border>(root, "GrowItemC");

        growInputA.PropertyChanged += HandleGrowValueChanged;
        growInputB.PropertyChanged += HandleGrowValueChanged;
        UpdateGrow();

        void HandleGrowValueChanged(object? growSender, AvaloniaPropertyChangedEventArgs args)
        {
            if (args.Property == NumericUpDown.ValueProperty)
            {
                UpdateGrow();
            }
        }

        void UpdateGrow()
        {
            var growA = Convert.ToDouble(growInputA.Value ?? 0);
            var growB = Convert.ToDouble(growInputB.Value ?? 0);
            Flex.SetGrow(growItemA, Math.Max(0, growA));
            Flex.SetGrow(growItemB, Math.Max(0, growB));
            Flex.SetGrow(growItemC, 1);
        }
    }

    private void InitializeShrinkExample(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (!TryMarkInitialized(sender, out var root))
        {
            return;
        }

        var shrinkWidthSlider = FindRequired<AvaloniaSlider>(root, "ShrinkWidthSlider");
        var shrinkWidthText   = FindRequired<AtomTextBlock>(root, "ShrinkWidthText");
        var shrinkContainer   = FindRequired<Control>(root, "ShrinkContainer");

        shrinkWidthSlider.ValueChanged += (_, _) => UpdateShrinkWidth();
        UpdateShrinkWidth();

        void UpdateShrinkWidth()
        {
            var width = Math.Max(0, Math.Round(shrinkWidthSlider.Value));
            shrinkContainer.Width = width;
            shrinkWidthText.Text  = $"{width:0}px";
        }
    }

    private void InitializePlaygroundExample(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (!TryMarkInitialized(sender, out var root))
        {
            return;
        }

        var playgroundDirectionSegmented    = FindRequired<AtomUISegmented>(root, "PlaygroundDirectionSegmented");
        var playgroundWrapSegmented         = FindRequired<AtomUISegmented>(root, "PlaygroundWrapSegmented");
        var playgroundJustifySegmented      = FindRequired<AtomUISegmented>(root, "PlaygroundJustifySegmented");
        var playgroundAlignItemsSegmented   = FindRequired<AtomUISegmented>(root, "PlaygroundAlignItemsSegmented");
        var playgroundAlignContentSegmented = FindRequired<AtomUISegmented>(root, "PlaygroundAlignContentSegmented");
        var playgroundColumnSpacingSlider   = FindRequired<AvaloniaSlider>(root, "PlaygroundColumnSpacingSlider");
        var playgroundColumnSpacingText     = FindRequired<AtomTextBlock>(root, "PlaygroundColumnSpacingText");
        var playgroundRowSpacingSlider      = FindRequired<AvaloniaSlider>(root, "PlaygroundRowSpacingSlider");
        var playgroundRowSpacingText        = FindRequired<AtomTextBlock>(root, "PlaygroundRowSpacingText");
        var playgroundWidthSlider           = FindRequired<AvaloniaSlider>(root, "PlaygroundWidthSlider");
        var playgroundWidthText             = FindRequired<AtomTextBlock>(root, "PlaygroundWidthText");
        var playgroundHeightSlider          = FindRequired<AvaloniaSlider>(root, "PlaygroundHeightSlider");
        var playgroundHeightText            = FindRequired<AtomTextBlock>(root, "PlaygroundHeightText");
        var playgroundItemSegmented         = FindRequired<AtomUISegmented>(root, "PlaygroundItemSegmented");
        var playgroundAlignSelfSegmented    = FindRequired<AtomUISegmented>(root, "PlaygroundAlignSelfSegmented");
        var playgroundBasisKindSegmented    = FindRequired<AtomUISegmented>(root, "PlaygroundBasisKindSegmented");
        var playgroundBasisValuePanel       = FindRequired<StackPanel>(root, "PlaygroundBasisValuePanel");
        var playgroundBasisValueInput       = FindRequired<NumericUpDown>(root, "PlaygroundBasisValueInput");
        var playgroundBasisValueUnit        = FindRequired<AtomTextBlock>(root, "PlaygroundBasisValueUnit");
        var playgroundBasisPercentPanel     = FindRequired<StackPanel>(root, "PlaygroundBasisPercentPanel");
        var playgroundBasisValueSlider      = FindRequired<AvaloniaSlider>(root, "PlaygroundBasisValueSlider");
        var playgroundBasisPercentText      = FindRequired<AtomTextBlock>(root, "PlaygroundBasisPercentText");
        var playgroundGrowInput             = FindRequired<NumericUpDown>(root, "PlaygroundGrowInput");
        var playgroundShrinkInput           = FindRequired<NumericUpDown>(root, "PlaygroundShrinkInput");
        var playgroundOrderInput            = FindRequired<NumericUpDown>(root, "PlaygroundOrderInput");
        var playgroundContainer             = FindRequired<Control>(root, "PlaygroundContainer");
        var playgroundFlexPanel             = FindRequired<AtomFlexPanel>(root, "PlaygroundFlexPanel");
        var playgroundItemA                 = FindRequired<Border>(root, "PlaygroundItemA");
        var playgroundItemB                 = FindRequired<Border>(root, "PlaygroundItemB");
        var playgroundItemC                 = FindRequired<Border>(root, "PlaygroundItemC");

        var suppressPlaygroundValueChanged = false;

        playgroundDirectionSegmented.SelectionChanged += HandlePlaygroundPanelSettingsChanged;
        playgroundWrapSegmented.SelectionChanged += HandlePlaygroundPanelSettingsChanged;
        playgroundJustifySegmented.SelectionChanged += HandlePlaygroundPanelSettingsChanged;
        playgroundAlignItemsSegmented.SelectionChanged += HandlePlaygroundPanelSettingsChanged;
        playgroundAlignContentSegmented.SelectionChanged += HandlePlaygroundPanelSettingsChanged;
        playgroundColumnSpacingSlider.ValueChanged += HandlePlaygroundSpacingChanged;
        playgroundRowSpacingSlider.ValueChanged += HandlePlaygroundSpacingChanged;
        playgroundWidthSlider.ValueChanged += HandlePlaygroundContainerSizeChanged;
        playgroundHeightSlider.ValueChanged += HandlePlaygroundContainerSizeChanged;
        playgroundItemSegmented.SelectionChanged += HandlePlaygroundItemSelectionChanged;
        playgroundAlignSelfSegmented.SelectionChanged += HandlePlaygroundAlignSelfChanged;
        playgroundBasisKindSegmented.SelectionChanged += HandlePlaygroundBasisKindChanged;
        playgroundBasisValueInput.PropertyChanged += HandlePlaygroundBasisValueChanged;
        playgroundBasisValueSlider.ValueChanged += HandlePlaygroundBasisValueSliderChanged;
        playgroundGrowInput.PropertyChanged += HandlePlaygroundItemNumericChanged;
        playgroundShrinkInput.PropertyChanged += HandlePlaygroundItemNumericChanged;
        playgroundOrderInput.PropertyChanged += HandlePlaygroundItemNumericChanged;

        UpdatePlaygroundPanelSettings();
        UpdatePlaygroundContainerSize();
        UpdatePlaygroundItemEditor();

        void HandlePlaygroundPanelSettingsChanged(object? segmentedSender, SelectionChangedEventArgs args)
        {
            if (!suppressPlaygroundValueChanged)
            {
                UpdatePlaygroundPanelSettings();
            }
        }

        void HandlePlaygroundSpacingChanged(object? sliderSender, RangeBaseValueChangedEventArgs args)
        {
            if (!suppressPlaygroundValueChanged)
            {
                UpdatePlaygroundPanelSettings();
            }
        }

        void UpdatePlaygroundPanelSettings()
        {
            playgroundFlexPanel.Direction = playgroundDirectionSegmented.SelectedIndex switch
            {
                1 => FlexDirection.Column,
                2 => FlexDirection.RowReverse,
                3 => FlexDirection.ColumnReverse,
                _ => FlexDirection.Row
            };

            playgroundFlexPanel.Wrap = playgroundWrapSegmented.SelectedIndex switch
            {
                0 => FlexWrap.NoWrap,
                2 => FlexWrap.WrapReverse,
                _ => FlexWrap.Wrap
            };

            playgroundFlexPanel.JustifyContent = playgroundJustifySegmented.SelectedIndex switch
            {
                1 => JustifyContent.Center,
                2 => JustifyContent.FlexEnd,
                3 => JustifyContent.SpaceBetween,
                4 => JustifyContent.SpaceAround,
                5 => JustifyContent.SpaceEvenly,
                _ => JustifyContent.FlexStart
            };

            playgroundFlexPanel.AlignItems = playgroundAlignItemsSegmented.SelectedIndex switch
            {
                0 => AlignItems.FlexStart,
                1 => AlignItems.Center,
                2 => AlignItems.FlexEnd,
                _ => AlignItems.Stretch
            };

            playgroundFlexPanel.AlignContent = playgroundAlignContentSegmented.SelectedIndex switch
            {
                0 => AlignContent.FlexStart,
                1 => AlignContent.Center,
                2 => AlignContent.FlexEnd,
                4 => AlignContent.SpaceBetween,
                5 => AlignContent.SpaceAround,
                6 => AlignContent.SpaceEvenly,
                _ => AlignContent.Stretch
            };

            var columnSpacing = Math.Max(0, Math.Round(playgroundColumnSpacingSlider.Value));
            var rowSpacing    = Math.Max(0, Math.Round(playgroundRowSpacingSlider.Value));
            playgroundFlexPanel.ColumnSpacing = columnSpacing;
            playgroundFlexPanel.RowSpacing    = rowSpacing;
            playgroundColumnSpacingText.Text  = $"{columnSpacing:0}px";
            playgroundRowSpacingText.Text     = $"{rowSpacing:0}px";
        }

        void HandlePlaygroundContainerSizeChanged(object? sliderSender, RangeBaseValueChangedEventArgs args)
        {
            if (!suppressPlaygroundValueChanged)
            {
                UpdatePlaygroundContainerSize();
            }
        }

        void UpdatePlaygroundContainerSize()
        {
            var width  = Math.Max(0, Math.Round(playgroundWidthSlider.Value));
            var height = Math.Max(0, Math.Round(playgroundHeightSlider.Value));
            playgroundContainer.Width  = width;
            playgroundContainer.Height = height;
            playgroundWidthText.Text   = $"{width:0}px";
            playgroundHeightText.Text  = $"{height:0}px";
        }

        void HandlePlaygroundItemSelectionChanged(object? segmentedSender, SelectionChangedEventArgs args)
        {
            if (!suppressPlaygroundValueChanged)
            {
                UpdatePlaygroundItemEditor();
            }
        }

        void HandlePlaygroundAlignSelfChanged(object? segmentedSender, SelectionChangedEventArgs args)
        {
            if (suppressPlaygroundValueChanged)
            {
                return;
            }

            var item = GetPlaygroundItem();
            var align = playgroundAlignSelfSegmented.SelectedIndex switch
            {
                1 => AlignItems.FlexStart,
                2 => AlignItems.Center,
                3 => AlignItems.FlexEnd,
                4 => AlignItems.Stretch,
                _ => (AlignItems?)null
            };
            Flex.SetAlignSelf(item, align);
        }

        void HandlePlaygroundBasisKindChanged(object? segmentedSender, SelectionChangedEventArgs args)
        {
            if (!suppressPlaygroundValueChanged)
            {
                UpdatePlaygroundBasis();
            }
        }

        void HandlePlaygroundBasisValueChanged(object? numericSender, AvaloniaPropertyChangedEventArgs args)
        {
            if (!suppressPlaygroundValueChanged && args.Property == NumericUpDown.ValueProperty)
            {
                UpdatePlaygroundBasis();
            }
        }

        void HandlePlaygroundBasisValueSliderChanged(object? sliderSender, RangeBaseValueChangedEventArgs args)
        {
            if (suppressPlaygroundValueChanged)
            {
                return;
            }

            playgroundBasisPercentText.Text = $"{Math.Round(playgroundBasisValueSlider.Value)}%";
            UpdatePlaygroundBasis();
        }

        void UpdatePlaygroundBasis()
        {
            var item      = GetPlaygroundItem();
            var kindIndex = playgroundBasisKindSegmented.SelectedIndex;
            var value     = GetNumericValue(playgroundBasisValueInput);

            switch (kindIndex)
            {
                case 0:
                    playgroundBasisValuePanel.IsVisible   = false;
                    playgroundBasisPercentPanel.IsVisible = false;
                    Flex.SetBasis(item, FlexBasis.Auto);
                    break;
                case 1:
                    playgroundBasisValuePanel.IsVisible   = true;
                    playgroundBasisPercentPanel.IsVisible = false;
                    playgroundBasisValueUnit.Text         = "px";
                    Flex.SetBasis(item, new FlexBasis(Math.Max(0, value)));
                    break;
                case 2:
                    playgroundBasisValuePanel.IsVisible   = false;
                    playgroundBasisPercentPanel.IsVisible = true;
                    var sliderValue = Math.Max(0, Math.Min(100, playgroundBasisValueSlider.Value));
                    playgroundBasisPercentText.Text = $"{Math.Round(sliderValue)}%";
                    Flex.SetBasis(item, new FlexBasis(sliderValue / 100, FlexBasisKind.Relative));
                    break;
            }
        }

        void HandlePlaygroundItemNumericChanged(object? numericSender, AvaloniaPropertyChangedEventArgs args)
        {
            if (!suppressPlaygroundValueChanged && args.Property == NumericUpDown.ValueProperty)
            {
                UpdatePlaygroundItemNumbers();
            }
        }

        void UpdatePlaygroundItemNumbers()
        {
            var item = GetPlaygroundItem();
            Flex.SetGrow(item, Math.Max(0, GetNumericValue(playgroundGrowInput)));
            Flex.SetShrink(item, Math.Max(0, GetNumericValue(playgroundShrinkInput)));
            Flex.SetOrder(item, GetIntValue(playgroundOrderInput));
        }

        void UpdatePlaygroundItemEditor()
        {
            suppressPlaygroundValueChanged = true;

            var item  = GetPlaygroundItem();
            var basis = Flex.GetBasis(item);
            switch (basis.Kind)
            {
                case FlexBasisKind.Auto:
                    playgroundBasisKindSegmented.SelectedIndex = 0;
                    playgroundBasisValuePanel.IsVisible        = false;
                    playgroundBasisPercentPanel.IsVisible      = false;
                    break;
                case FlexBasisKind.Absolute:
                    playgroundBasisKindSegmented.SelectedIndex = 1;
                    playgroundBasisValuePanel.IsVisible        = true;
                    playgroundBasisPercentPanel.IsVisible      = false;
                    playgroundBasisValueUnit.Text              = "px";
                    playgroundBasisValueInput.Value            = (decimal)basis.Value;
                    break;
                case FlexBasisKind.Relative:
                    playgroundBasisKindSegmented.SelectedIndex = 2;
                    playgroundBasisValuePanel.IsVisible        = false;
                    playgroundBasisPercentPanel.IsVisible      = true;
                    playgroundBasisValueSlider.Value           = basis.Value * 100;
                    playgroundBasisPercentText.Text            = $"{Math.Round(playgroundBasisValueSlider.Value)}%";
                    break;
            }

            playgroundAlignSelfSegmented.SelectedIndex = Flex.GetAlignSelf(item) switch
            {
                null => 0,
                AlignItems.FlexStart => 1,
                AlignItems.Center    => 2,
                AlignItems.FlexEnd   => 3,
                AlignItems.Stretch   => 4,
                _                    => 0
            };

            playgroundGrowInput.Value   = (decimal)Math.Max(0, Flex.GetGrow(item));
            playgroundShrinkInput.Value = (decimal)Math.Max(0, Flex.GetShrink(item));
            playgroundOrderInput.Value  = Flex.GetOrder(item);

            suppressPlaygroundValueChanged = false;
        }

        Layoutable GetPlaygroundItem()
        {
            return playgroundItemSegmented.SelectedIndex switch
            {
                1 => playgroundItemB,
                2 => playgroundItemC,
                _ => playgroundItemA
            };
        }
    }

    private static double GetNumericValue(NumericUpDown input)
    {
        return Convert.ToDouble(input.Value ?? 0);
    }

    private static int GetIntValue(NumericUpDown input)
    {
        return Convert.ToInt32(input.Value ?? 0);
    }

    private static bool TryMarkInitialized(object? sender, out Control root)
    {
        if (sender is not Control control)
        {
            root = null!;
            return false;
        }

        if (ReferenceEquals(control.Tag, DeferredTemplateInitialized))
        {
            root = control;
            return false;
        }

        control.Tag = DeferredTemplateInitialized;
        root        = control;
        return true;
    }

    private static T FindRequired<T>(Control root, string name)
        where T : Control
    {
        if (root is T typedRoot && typedRoot.Name == name)
        {
            return typedRoot;
        }

        return root.GetVisualDescendants()
                   .OfType<T>()
                   .FirstOrDefault(control => control.Name == name)
               ?? root.GetLogicalDescendants()
                      .OfType<T>()
                      .FirstOrDefault(control => control.Name == name)
               ?? throw new InvalidOperationException($"Could not find '{name}' in {nameof(FlexPanelShowCase)}.");
    }
}
