using AtomUI.Desktop.Controls;
using AtomUI.Icons.AntDesign;
using Avalonia.Controls;

namespace AtomUI.Performance;

internal static partial class Program
{
    private static IReadOnlyList<PerfScenario> CreateButtonScenarios()
    {
        return
        [
            new PerfScenario("Button.Default.Text", _ => new AtomUI.Desktop.Controls.Button
            {
                Content = "Button"
            }),
            new PerfScenario("Button.Default.NoWave", _ => new AtomUI.Desktop.Controls.Button
            {
                Content = "Button",
                IsInteractionEffectEnabled = false
            }),
            new PerfScenario("Button.Primary.Text", _ => new AtomUI.Desktop.Controls.Button
            {
                ButtonType = ButtonType.Primary,
                Content    = "Primary"
            }),
            new PerfScenario("Button.Dashed.Text", _ => new AtomUI.Desktop.Controls.Button
            {
                ButtonType = ButtonType.Dashed,
                Content    = "Dashed"
            }),
            new PerfScenario("Button.Text.Text", _ => new AtomUI.Desktop.Controls.Button
            {
                ButtonType = ButtonType.Text,
                Content    = "Text"
            }),
            new PerfScenario("Button.Link.Text", _ => new AtomUI.Desktop.Controls.Button
            {
                ButtonType = ButtonType.Link,
                Content    = "Link"
            }),
            new PerfScenario("Button.Default.IconOnly", _ => new AtomUI.Desktop.Controls.Button
            {
                Icon = CreateButtonSearchIcon()
            }),
            new PerfScenario("Button.Default.IconText", _ => new AtomUI.Desktop.Controls.Button
            {
                Content = "Search",
                Icon    = CreateButtonSearchIcon()
            }),
            new PerfScenario("Button.Primary.Loading", _ => new AtomUI.Desktop.Controls.Button
            {
                ButtonType = ButtonType.Primary,
                Content    = "Loading",
                IsLoading  = true
            }),
            new PerfScenario("Button.Primary.IconLoading", _ => new AtomUI.Desktop.Controls.Button
            {
                ButtonType = ButtonType.Primary,
                Icon       = new PoweroffOutlined(),
                IsLoading  = true
            }),
            new PerfScenario("Button.Primary.GhostDangerIcon", _ => new AtomUI.Desktop.Controls.Button
            {
                ButtonType = ButtonType.Primary,
                IsGhost    = true,
                IsDanger   = true,
                Content    = "Danger",
                Icon       = CreateButtonSearchIcon()
            }),
            new PerfScenario("Button.Shape.CircleText", _ => new AtomUI.Desktop.Controls.Button
            {
                Shape   = ButtonShape.Circle,
                Content = "AA"
            }),
            new PerfScenario("Button.Shape.RoundIconText", _ => new AtomUI.Desktop.Controls.Button
            {
                Shape   = ButtonShape.Round,
                Content = "Search",
                Icon    = CreateButtonSearchIcon()
            }),
            new PerfScenario("Button.Dropdown.Default", _ => CreateDropdownButton()),
            new PerfScenario("Button.Split.Primary", _ => CreateSplitButton()),
            new PerfScenario("Button.Mixed.Batch10", _ => CreateMixedButtonBatch())
        ];
    }

    private static PathIcon CreateButtonSearchIcon()
    {
        return new SearchOutlined();
    }

    private static DropdownButton CreateDropdownButton()
    {
        var flyout = new AtomUI.Desktop.Controls.MenuFlyout();
        flyout.Items.Add(new AtomUI.Desktop.Controls.MenuItem
        {
            Header = "Action"
        });

        return new DropdownButton
        {
            Content        = "Dropdown",
            DropdownFlyout = flyout
        };
    }

    private static AtomUI.Desktop.Controls.SplitButton CreateSplitButton()
    {
        return new AtomUI.Desktop.Controls.SplitButton
        {
            Content             = "Split",
            Icon                = CreateButtonSearchIcon(),
            IsPrimaryButtonType = true,
            Flyout              = new AtomUI.Desktop.Controls.MenuFlyout()
        };
    }

    private static WrapPanel CreateMixedButtonBatch()
    {
        var panel = new WrapPanel
        {
            Width = 720
        };

        panel.Children.Add(new AtomUI.Desktop.Controls.Button { ButtonType = ButtonType.Primary, Content = "Primary" });
        panel.Children.Add(new AtomUI.Desktop.Controls.Button { Content = "Default" });
        panel.Children.Add(new AtomUI.Desktop.Controls.Button { ButtonType = ButtonType.Dashed, Content = "Dashed" });
        panel.Children.Add(new AtomUI.Desktop.Controls.Button { ButtonType = ButtonType.Text, Content = "Text" });
        panel.Children.Add(new AtomUI.Desktop.Controls.Button { ButtonType = ButtonType.Link, Content = "Link" });
        panel.Children.Add(new AtomUI.Desktop.Controls.Button { Icon = CreateButtonSearchIcon() });
        panel.Children.Add(new AtomUI.Desktop.Controls.Button { Content = "Search", Icon = CreateButtonSearchIcon() });
        panel.Children.Add(new AtomUI.Desktop.Controls.Button { ButtonType = ButtonType.Primary, Content = "Loading", IsLoading = true });
        panel.Children.Add(new AtomUI.Desktop.Controls.Button { ButtonType = ButtonType.Primary, IsGhost = true, IsDanger = true, Content = "Danger", Icon = CreateButtonSearchIcon() });
        panel.Children.Add(new AtomUI.Desktop.Controls.Button { Shape = ButtonShape.Round, Content = "Round", Icon = CreateButtonSearchIcon() });

        return panel;
    }
}
