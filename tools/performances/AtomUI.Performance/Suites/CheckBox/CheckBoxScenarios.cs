using AtomUI.Controls;
using Avalonia.Controls;
using Avalonia.Layout;

namespace AtomUI.Performance;

internal static partial class Program
{
    private static IReadOnlyList<PerfScenario> CreateCheckBoxScenarios()
    {
        return
        [
            new PerfScenario("CheckBox.Default.Unchecked", _ => new AtomUI.Desktop.Controls.CheckBox
            {
                Content = "Unchecked"
            }),
            new PerfScenario("CheckBox.Default.Checked", _ => new AtomUI.Desktop.Controls.CheckBox
            {
                Content   = "Checked",
                IsChecked = true
            }),
            new PerfScenario("CheckBox.Default.Indeterminate", _ => new AtomUI.Desktop.Controls.CheckBox
            {
                Content   = "Indeterminate",
                IsChecked = null
            }),
            new PerfScenario("CheckBox.Disabled.Checked", _ => new AtomUI.Desktop.Controls.CheckBox
            {
                Content   = "Disabled",
                IsChecked = true,
                IsEnabled = false
            }),
            new PerfScenario("CheckBox.Contentless.Unchecked", _ => new AtomUI.Desktop.Controls.CheckBox()),
            new PerfScenario("CheckBox.Contentless.Checked", _ => new AtomUI.Desktop.Controls.CheckBox
            {
                IsChecked = true
            }),
            new PerfScenario("CheckBox.NoWave", _ => new AtomUI.Desktop.Controls.CheckBox
            {
                Content             = "NoWave",
                IsInteractionEffectEnabled = false
            }),
            new PerfScenario("CheckBoxGroup.Static3", _ => CreateStaticCheckBoxGroup()),
            new PerfScenario("CheckBoxGroup.ItemsSource3", _ => CreateItemsSourceCheckBoxGroup()),
            new PerfScenario("CheckBox.Batch50.Mixed", _ => CreateMixedCheckBoxBatch())
        ];
    }

    private static AtomUI.Desktop.Controls.CheckBoxGroup CreateStaticCheckBoxGroup()
    {
        var group = new AtomUI.Desktop.Controls.CheckBoxGroup();
        group.Items.Add(new AtomUI.Desktop.Controls.CheckBox { Content = "Apple", IsChecked = true });
        group.Items.Add(new AtomUI.Desktop.Controls.CheckBox { Content = "Pear" });
        group.Items.Add(new AtomUI.Desktop.Controls.CheckBox { Content = "Orange", IsEnabled = false });
        return group;
    }

    private static AtomUI.Desktop.Controls.CheckBoxGroup CreateItemsSourceCheckBoxGroup()
    {
        var apple  = new CheckBoxOption { Content = "Apple" };
        var pear   = new CheckBoxOption { Content = "Pear", IsChecked = true };
        var orange = new CheckBoxOption { Content = "Orange", IsEnabled = false };
        return new AtomUI.Desktop.Controls.CheckBoxGroup
        {
            ItemsSource  = new[] { apple, pear, orange },
            CheckedItems = new List<CheckBoxOption> { pear }
        };
    }

    private static WrapPanel CreateMixedCheckBoxBatch()
    {
        var panel = new WrapPanel
        {
            Width       = 720,
            Orientation = Orientation.Horizontal
        };

        for (var i = 0; i < 50; i++)
        {
            panel.Children.Add(new AtomUI.Desktop.Controls.CheckBox
            {
                Content   = i % 5 == 0 ? null : $"Item {i}",
                IsChecked = i % 7 == 0 ? null : i % 3 == 0,
                IsEnabled = i % 11 != 0
            });
        }

        return panel;
    }
}
