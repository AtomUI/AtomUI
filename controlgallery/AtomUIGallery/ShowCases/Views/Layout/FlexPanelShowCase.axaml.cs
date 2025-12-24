using System;
using AtomUI.Desktop.Controls;
using AtomUIGallery.ShowCases.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using ReactiveUI.Avalonia;
using NumericUpDown = AtomUI.Desktop.Controls.NumericUpDown;
using RadioButton = AtomUI.Desktop.Controls.RadioButton;

namespace AtomUIGallery.ShowCases.Views;

public partial class FlexPanelShowCase : ReactiveUserControl<FlexPanelViewModel>
{
    private const double GapSmallValue = 8;
    private const double GapMiddleValue = 16;
    private const double GapLargeValue = 24;
    private const double BasisAbsoluteDefault = 60;
    private bool _suppressBasisValueChanged;

    public FlexPanelShowCase()
    {
        InitializeComponent();
        SetupBasicItems();
        SetupEventHandlers();
        SetBasicDirection(FlexDirection.Row);
        SetGap(GapSmallValue);
        InitializePropertyDemos();
    }

    private void SetupBasicItems()
    {
        ConfigureBasicItem(BasicItem1);
        ConfigureBasicItem(BasicItem2);
        ConfigureBasicItem(BasicItem3);
        ConfigureBasicItem(BasicItem4);
    }

    private static void ConfigureBasicItem(Layoutable item)
    {
        Flex.SetGrow(item, 1);
    }

    private void SetupEventHandlers()
    {
        DirectionHorizontal.IsCheckedChanged += (_, _) =>
        {
            if (DirectionHorizontal.IsChecked == true)
            {
                SetBasicDirection(FlexDirection.Row);
            }
        };

        DirectionVertical.IsCheckedChanged += (_, _) =>
        {
            if (DirectionVertical.IsChecked == true)
            {
                SetBasicDirection(FlexDirection.Column);
            }
        };

        JustifySegmented.SelectionChanged += HandleJustifySelectionChanged;
        AlignSegmented.SelectionChanged += HandleAlignSelectionChanged;
        GapSmall.IsCheckedChanged += HandleGapRadioChanged;
        GapMiddle.IsCheckedChanged += HandleGapRadioChanged;
        GapLarge.IsCheckedChanged += HandleGapRadioChanged;
        GapCustomize.IsCheckedChanged += HandleGapRadioChanged;
        GapValueSlider.ValueChanged += HandleGapValueSliderChanged;

        WrapEnabled.IsCheckedChanged += (_, _) =>
        {
            if (WrapEnabled.IsChecked == true)
            {
                WrapFlexPanel.Wrap = FlexWrap.Wrap;
                WrapScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            }
        };

        WrapDisabled.IsCheckedChanged += (_, _) =>
        {
            if (WrapDisabled.IsChecked == true)
            {
                WrapFlexPanel.Wrap = FlexWrap.NoWrap;
                WrapScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            }
        };

        OrderSegmented.SelectionChanged += HandleOrderSelectionChanged;
        BasisKindSegmented.SelectionChanged += HandleBasisKindSelectionChanged;
        BasisValueInput.PropertyChanged += HandleBasisValueChanged;
        BasisValueSlider.ValueChanged += HandleBasisValueSliderChanged;
        AlignSelfSegmented.SelectionChanged += HandleAlignSelfSelectionChanged;
        GrowInputA.PropertyChanged += HandleGrowValueChanged;
        GrowInputB.PropertyChanged += HandleGrowValueChanged;
    }

    private void HandleJustifySelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var index = JustifySegmented.SelectedIndex;
        AlignFlexPanel.JustifyContent = index switch
        {
            0 => JustifyContent.FlexStart,
            1 => JustifyContent.Center,
            2 => JustifyContent.FlexEnd,
            3 => JustifyContent.SpaceBetween,
            4 => JustifyContent.SpaceAround,
            5 => JustifyContent.SpaceEvenly,
            _ => AlignFlexPanel.JustifyContent
        };
    }

    private void HandleAlignSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var index = AlignSegmented.SelectedIndex;
        AlignFlexPanel.AlignItems = index switch
        {
            0 => AlignItems.FlexStart,
            1 => AlignItems.Center,
            2 => AlignItems.FlexEnd,
            3 => AlignItems.Stretch,
            _ => AlignFlexPanel.AlignItems
        };
    }

    private void HandleGapRadioChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton { IsChecked: true } radioButton)
        {
            return;
        }

        if (radioButton == GapSmall)
        {
            GapCustomPanel.IsVisible = false;
            SetGap(GapSmallValue);
        }
        else if (radioButton == GapMiddle)
        {
            GapCustomPanel.IsVisible = false;
            SetGap(GapMiddleValue);
        }
        else if (radioButton == GapLarge)
        {
            GapCustomPanel.IsVisible = false;
            SetGap(GapLargeValue);
        }
        else if (radioButton == GapCustomize)
        {
            GapCustomPanel.IsVisible = true;
            SetGap(GetCustomGap());
        }
    }

    private void HandleGapValueSliderChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (GapCustomPanel.IsVisible)
        {
            SetGap(GetCustomGap());
        }
    }

    private double GetCustomGap()
    {
        return Math.Max(0, GapValueSlider.Value);
    }

    private void SetGap(double gap)
    {
        GapFlexPanel.ColumnSpacing = gap;
        GapFlexPanel.RowSpacing = gap;
    }

    private void SetBasicDirection(FlexDirection direction)
    {
        BasicFlexPanel.Direction = direction;
        BasicFlexPanel.AlignItems = direction == FlexDirection.Column
            ? AlignItems.FlexStart
            : AlignItems.Stretch;
        var grow = direction == FlexDirection.Row ? 1.0 : 0.0;
        Flex.SetGrow(BasicItem1, grow);
        Flex.SetGrow(BasicItem2, grow);
        Flex.SetGrow(BasicItem3, grow);
        Flex.SetGrow(BasicItem4, grow);
    }

    private void InitializePropertyDemos()
    {
        ApplyOrderPreset(0);
        BasisValueInput.Value = (decimal)BasisAbsoluteDefault;
        BasisValueSlider.Value = 100;
        BasisPercentText.Text = "100%";
        UpdateBasis();
        UpdateAlignSelf();
        UpdateGrow();
    }

    private void HandleOrderSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        ApplyOrderPreset(OrderSegmented.SelectedIndex);
    }

    private void ApplyOrderPreset(int index)
    {
        switch (index)
        {
            case 0:
                Flex.SetOrder(OrderItemA, 0);
                Flex.SetOrder(OrderItemB, 0);
                Flex.SetOrder(OrderItemC, 0);
                Flex.SetOrder(OrderItemD, 0);
                break;
            case 1:
                Flex.SetOrder(OrderItemA, 3);
                Flex.SetOrder(OrderItemB, 2);
                Flex.SetOrder(OrderItemC, 1);
                Flex.SetOrder(OrderItemD, 0);
                break;
            case 2:
                Flex.SetOrder(OrderItemA, 2);
                Flex.SetOrder(OrderItemB, -1);
                Flex.SetOrder(OrderItemC, 3);
                Flex.SetOrder(OrderItemD, 1);
                break;
            default:
                break;
        }
    }

    private void HandleBasisKindSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _suppressBasisValueChanged = true;
        if (BasisKindSegmented.SelectedIndex == 2)
        {
            BasisValueSlider.Value = 0;
            BasisPercentText.Text = "0%";
        }
        else if (BasisKindSegmented.SelectedIndex == 1 && BasisValueInput.Value is null)
        {
            BasisValueInput.Value = (decimal)BasisAbsoluteDefault;
        }
        _suppressBasisValueChanged = false;
        UpdateBasis();
    }

    private void HandleBasisValueChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == NumericUpDown.ValueProperty && !_suppressBasisValueChanged)
        {
            UpdateBasis();
        }
    }

    private void HandleBasisValueSliderChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (_suppressBasisValueChanged)
        {
            return;
        }

        BasisPercentText.Text = $"{Math.Round(BasisValueSlider.Value)}%";
        UpdateBasis();
    }

    private void UpdateBasis()
    {
        var kindIndex = BasisKindSegmented.SelectedIndex;
        var value = Convert.ToDouble(BasisValueInput.Value ?? 0);

        switch (kindIndex)
        {
            case 0:
                BasisValuePanel.IsVisible = false;
                BasisPercentPanel.IsVisible = false;
                Flex.SetBasis(BasisItem, FlexBasis.Auto);
                break;
            case 1:
                BasisValuePanel.IsVisible = true;
                BasisPercentPanel.IsVisible = false;
                BasisValueInput.Minimum = 0;
                BasisValueInput.Maximum = 1000;
                BasisValueUnit.Text = "px";
                Flex.SetBasis(BasisItem, new FlexBasis(Math.Max(0, value)));
                break;
            case 2:
                BasisValuePanel.IsVisible = false;
                BasisPercentPanel.IsVisible = true;
                var sliderValue = Math.Max(0, Math.Min(100, BasisValueSlider.Value));
                BasisPercentText.Text = $"{Math.Round(sliderValue)}%";
                var relative = sliderValue / 100;
                Flex.SetBasis(BasisItem, new FlexBasis(relative, FlexBasisKind.Relative));
                break;
        }

        Flex.SetGrow(BasisItem, 0);
        Flex.SetGrow(BasisGrowItem1, 1);
        Flex.SetGrow(BasisGrowItem2, 1);
    }

    private void HandleAlignSelfSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        UpdateAlignSelf();
    }

    private void UpdateAlignSelf()
    {
        var align = AlignSelfSegmented.SelectedIndex switch
        {
            0 => AlignItems.FlexStart,
            1 => AlignItems.Center,
            2 => AlignItems.FlexEnd,
            3 => AlignItems.Stretch,
            _ => AlignItems.FlexStart
        };
        Flex.SetAlignSelf(AlignSelfTarget, align);
    }

    private void HandleGrowValueChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == NumericUpDown.ValueProperty)
        {
            UpdateGrow();
        }
    }

    private void UpdateGrow()
    {
        var growA = Convert.ToDouble(GrowInputA.Value ?? 0);
        var growB = Convert.ToDouble(GrowInputB.Value ?? 0);
        Flex.SetGrow(GrowItemA, Math.Max(0, growA));
        Flex.SetGrow(GrowItemB, Math.Max(0, growB));
        Flex.SetGrow(GrowItemC, 1);
    }
}
