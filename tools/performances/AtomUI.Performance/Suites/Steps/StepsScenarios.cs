using AtomUI.Desktop.Controls;
using AtomUI.Icons.AntDesign;
using Avalonia.Controls;
using Avalonia.Layout;
using AtomTextBlock = AtomUI.Desktop.Controls.TextBlock;

namespace AtomUI.Performance;

internal static partial class Program
{
    private static IReadOnlyList<PerfScenario> CreateStepsScenarios()
    {
        return
        [
            new PerfScenario("Steps.Basic.Items3", _ => CreateSteps()),
            new PerfScenario("Steps.Vertical.Items3", _ => CreateSteps(orientation: Orientation.Vertical, current: 1)),
            new PerfScenario("Steps.Dot.Items4", _ => CreateSteps(count: 4, type: StepsType.Dot, current: 1)),
            new PerfScenario("Steps.Navigation.Items4", _ => CreateSteps(count: 4, type: StepsType.Navigation, isClickable: true)),
            new PerfScenario("Steps.Inline.Items3", _ => CreateSteps(type: StepsType.Inline, current: 1)),
            new PerfScenario("Steps.Progress.Items3", _ => CreateSteps(current: 1, percent: 60)),
            new PerfScenario("Steps.Icon.Items4", _ => CreateIconSteps()),
            new PerfScenario("Steps.GalleryShape", _ => CreateStepsGalleryShape())
        ];
    }

    private static Steps CreateSteps(int count = 3,
                                     Orientation orientation = Orientation.Horizontal,
                                     StepsType type = StepsType.Default,
                                     int current = 0,
                                     bool isClickable = false,
                                     double? percent = null)
    {
        var steps = new Steps
        {
            Width           = orientation == Orientation.Horizontal ? 760 : 360,
            Current         = current,
            Orientation     = orientation,
            Type            = type,
            IsItemClickable = isClickable,
            Percent         = percent
        };

        for (var i = 0; i < count; i++)
        {
            steps.Items.Add(new StepsItem
            {
                Header    = $"Step {i + 1}",
                SubHeader = i == 1 ? "Left 00:00:08" : null,
                Content   = "This is a description."
            });
        }

        return steps;
    }

    private static Steps CreateIconSteps()
    {
        var steps = CreateSteps(count: 0);
        steps.Items.Add(new StepsItem { Header = "Login", Status = StepsStatus.Finish, Icon = new UserOutlined() });
        steps.Items.Add(new StepsItem { Header = "Verification", Status = StepsStatus.Finish, Icon = new SolutionOutlined() });
        steps.Items.Add(new StepsItem { Header = "Pay", Status = StepsStatus.Process, Icon = new LoadingOutlined() });
        steps.Items.Add(new StepsItem { Header = "Done", Status = StepsStatus.Wait, Icon = new SmileOutlined() });
        return steps;
    }

    private static Control CreateStepsGalleryShape()
    {
        var root = new StackPanel
        {
            Spacing = 12
        };

        root.Children.Add(CreateSteps());
        root.Children.Add(CreateSteps(current: 1, orientation: Orientation.Vertical));
        root.Children.Add(CreateIconSteps());
        root.Children.Add(CreateSteps(count: 4, type: StepsType.Dot, current: 1));
        root.Children.Add(CreateSteps(count: 4, type: StepsType.Dot, orientation: Orientation.Vertical, current: 1));
        root.Children.Add(CreateSteps(count: 4, type: StepsType.Navigation, isClickable: true));
        root.Children.Add(CreateSteps(count: 4, type: StepsType.Navigation, orientation: Orientation.Vertical, isClickable: true));
        root.Children.Add(CreateSteps(current: 1, percent: 60));
        var verticalTitleSteps = CreateSteps(current: 1, percent: 45);
        verticalTitleSteps.TitlePlacement = Orientation.Vertical;
        root.Children.Add(verticalTitleSteps);
        root.Children.Add(CreateSteps(type: StepsType.Inline, current: 1));
        root.Children.Add(new AtomTextBlock { Text = "Steps gallery shape sentinel" });

        return root;
    }
}
