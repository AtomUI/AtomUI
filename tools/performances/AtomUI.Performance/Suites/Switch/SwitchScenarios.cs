using AtomUI.Desktop.Controls;
using AtomUI.Icons.AntDesign;
using Avalonia.Controls;
using Avalonia.Layout;
using AtomSwitch = AtomUI.Desktop.Controls.ToggleSwitch;

namespace AtomUI.Performance;

internal static partial class Program
{
    private static IReadOnlyList<PerfScenario> CreateSwitchScenarios()
    {
        return
        [
            new PerfScenario("Switch.Default", _ => new AtomSwitch()),
            new PerfScenario("Switch.Checked", _ => new AtomSwitch { IsChecked = true }),
            new PerfScenario("Switch.Small", _ => new AtomSwitch { SizeType = CustomizableSizeType.Small }),
            new PerfScenario("Switch.NoWave", _ => new AtomSwitch { IsInteractionEffectEnabled = false }),
            new PerfScenario("Switch.Text", _ => new AtomSwitch { OnContent = "On", OffContent = "Off", IsChecked = true }),
            new PerfScenario("Switch.Icon", _ => new AtomSwitch { OnContent = new TwitterOutlined(), OffContent = new WechatOutlined() }),
            new PerfScenario("Switch.Loading", _ => new AtomSwitch { IsLoading = true, IsChecked = true }),
            new PerfScenario("Switch.GalleryShape", _ => CreateSwitchGalleryShape())
        ];
    }

    private static Control CreateSwitchGalleryShape()
    {
        var root = new StackPanel
        {
            Spacing      = 10,
            Orientation = Orientation.Vertical
        };

        root.Children.Add(CreateSwitchRow(new AtomSwitch()));
        root.Children.Add(CreateSwitchRow(
            new AtomSwitch(),
            new AtomUI.Desktop.Controls.Button
            {
                ButtonType = ButtonType.Primary,
                Content    = "toggle disabled"
            }));
        root.Children.Add(CreateSwitchColumn(
            new AtomSwitch { OnContent = "On", OffContent = "Off", IsChecked = true },
            new AtomSwitch { OnContent = "开", OffContent = "关" },
            new AtomSwitch { OnContent = new TwitterOutlined(), OffContent = new WechatOutlined() },
            new AtomSwitch { SizeType = CustomizableSizeType.Small, OnContent = new CheckOutlined(), OffContent = new WechatOutlined() },
            new AtomSwitch { SizeType = CustomizableSizeType.Small, OnContent = new CheckOutlined(), OffContent = new CloseOutlined() }));
        root.Children.Add(CreateSwitchColumn(
            new AtomSwitch(),
            new AtomSwitch { SizeType = CustomizableSizeType.Small }));
        root.Children.Add(CreateSwitchColumn(
            new AtomSwitch { IsLoading = true, IsChecked = true },
            new AtomSwitch { SizeType = CustomizableSizeType.Small, IsLoading = true },
            new AtomUI.Desktop.Controls.Button
            {
                ButtonType = ButtonType.Primary,
                Content    = "toggle loading"
            }));

        return root;
    }

    private static StackPanel CreateSwitchRow(params Control[] children)
    {
        var row = new StackPanel
        {
            Spacing      = 10,
            Orientation = Orientation.Horizontal
        };
        foreach (var child in children)
        {
            row.Children.Add(child);
        }
        return row;
    }

    private static StackPanel CreateSwitchColumn(params Control[] children)
    {
        var column = new StackPanel
        {
            Spacing      = 10,
            Orientation = Orientation.Vertical
        };
        foreach (var child in children)
        {
            column.Children.Add(child);
        }
        return column;
    }
}
