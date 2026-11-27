using AtomUI.Controls;
using AtomUI.Icons.AntDesign;
using Avalonia.Controls;
using Avalonia.Layout;
using AtomButton = AtomUI.Desktop.Controls.Button;
using AtomRadioButton = AtomUI.Desktop.Controls.RadioButton;
using AtomRadioButtonGroup = AtomUI.Desktop.Controls.RadioButtonGroup;

namespace AtomUI.Performance;

internal static partial class Program
{
    private static IReadOnlyList<PerfScenario> CreateRadioButtonScenarios()
    {
        return
        [
            new PerfScenario("RadioButton.Default.Unchecked", _ => new AtomRadioButton
            {
                Content = "Radio"
            }),
            new PerfScenario("RadioButton.Default.Checked", _ => new AtomRadioButton
            {
                Content   = "Radio",
                IsChecked = true
            }),
            new PerfScenario("RadioButton.Disabled.Checked", _ => new AtomRadioButton
            {
                Content   = "Disabled",
                IsChecked = true,
                IsEnabled = false
            }),
            new PerfScenario("RadioButton.Contentless.Unchecked", _ => new AtomRadioButton()),
            new PerfScenario("RadioButton.NoWave", _ => new AtomRadioButton
            {
                Content             = "NoWave",
                IsInteractionEffectEnabled = false
            }),
            new PerfScenario("RadioButtonGroup.Static4.Text", _ => CreateTextRadioButtonGroup()),
            new PerfScenario("RadioButtonGroup.Static4.IconContent", _ => CreateIconRadioButtonGroup()),
            new PerfScenario("RadioButtonGroup.ItemsSource4", _ => CreateItemsSourceRadioButtonGroup()),
            new PerfScenario("RadioButton.GalleryShape.RoundOnly", _ => CreateRadioButtonShowCaseRoundShape())
        ];
    }

    private static AtomRadioButtonGroup CreateTextRadioButtonGroup()
    {
        var group = new AtomRadioButtonGroup
        {
            HorizontalAlignment = HorizontalAlignment.Left
        };
        group.Items.Add(new AtomRadioButton { Content = "Option A", IsChecked = true });
        group.Items.Add(new AtomRadioButton { Content = "Option B" });
        group.Items.Add(new AtomRadioButton { Content = "Option C" });
        group.Items.Add(new AtomRadioButton { Content = "Option D" });
        return group;
    }

    private static AtomRadioButtonGroup CreateIconRadioButtonGroup()
    {
        var group = new AtomRadioButtonGroup();
        group.Items.Add(CreateIconRadioButton(new LineChartOutlined(), "LineChart"));
        group.Items.Add(CreateIconRadioButton(new DotChartOutlined(), "DotChart"));
        group.Items.Add(CreateIconRadioButton(new BarChartOutlined(), "BarChart"));
        group.Items.Add(CreateIconRadioButton(new PieChartOutlined(), "PieChart"));
        return group;
    }

    private static AtomRadioButton CreateIconRadioButton(Icon icon, string label)
    {
        return new AtomRadioButton
        {
            Content = new StackPanel
            {
                Spacing     = 5,
                Orientation = Orientation.Vertical,
                Children =
                {
                    icon,
                    new TextBlock { Text = label }
                }
            }
        };
    }

    private static AtomRadioButtonGroup CreateItemsSourceRadioButtonGroup()
    {
        var optionA = new RadioButtonOption { Content = "Option A" };
        var optionB = new RadioButtonOption { Content = "Option B", IsChecked = true };
        var optionC = new RadioButtonOption { Content = "Option C" };
        var optionD = new RadioButtonOption { Content = "Option D", IsEnabled = false };
        return new AtomRadioButtonGroup
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            ItemsSource         = new[] { optionA, optionB, optionC, optionD },
            CheckedItem         = optionB
        };
    }

    private static Control CreateRadioButtonShowCaseRoundShape()
    {
        var panel = new StackPanel
        {
            Spacing = 10
        };

        panel.Children.Add(new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            Spacing             = 10,
            Orientation         = Orientation.Horizontal,
            Children =
            {
                new AtomRadioButton { Content = "Radio" }
            }
        });

        panel.Children.Add(new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            Orientation         = Orientation.Vertical,
            Children =
            {
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        new AtomRadioButton { Content = "Radio1" },
                        new AtomRadioButton { Content = "Radio2", IsChecked = true }
                    }
                },
                new AtomButton
                {
                    Content = "toggle disabled",
                    Margin  = new Avalonia.Thickness(0, 20, 0, 0)
                }
            }
        });

        panel.Children.Add(CreateIconRadioButtonGroup());

        var verticalGroup = CreateTextRadioButtonGroup();
        verticalGroup.Orientation = Orientation.Vertical;
        panel.Children.Add(verticalGroup);

        panel.Children.Add(CreateItemsSourceRadioButtonGroup());

        panel.Children.Add(new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing     = 10,
            Children =
            {
                new StackPanel
                {
                    Orientation         = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Children =
                    {
                        new AtomRadioButton { Content = "Apple", IsChecked = true },
                        new AtomRadioButton { Content = "Pear" },
                        new AtomRadioButton { Content = "Orange" }
                    }
                },
                new StackPanel
                {
                    Orientation         = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Children =
                    {
                        new AtomRadioButton { Content = "Apple", IsChecked = true },
                        new AtomRadioButton { Content = "Pear" },
                        new AtomRadioButton { Content = "Orange", IsEnabled = false }
                    }
                }
            }
        });

        return panel;
    }
}
