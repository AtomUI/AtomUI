using AtomUI;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using System.Reactive.Disposables;
using AtomDrawer = AtomUI.Desktop.Controls.Drawer;
using AtomDrawerPlacement = AtomUI.Desktop.Controls.DrawerPlacement;

namespace AtomUIGallery.ShowCases.Drawer;

public partial class DrawerShowCase : GalleryReactiveUserControl<DrawerViewModel>
{
    public const string LanguageId = nameof(DrawerShowCase);

    public DrawerShowCase()
    {
        this.WhenActivated(disposables =>
        {
            if (DataContext is DrawerViewModel viewModel)
            {
                RefreshLocalizedOptionData(viewModel);
                var languageManager = Application.Current is { } application
                    ? global::AtomUI.ApplicationExtensions.GetLanguageManager(application)
                    : null;
                if (languageManager is not null)
                {
                    EventHandler<LanguageChangedEventArgs> handler = (_, _) => RefreshLocalizedOptionData(viewModel);
                    languageManager.LanguageChanged += handler;
                    disposables.Add(Disposable.Create(() => languageManager.LanguageChanged -= handler));
                }
            }
        });

        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

    }

    private void HandleOpenLargeSizeDrawer(object? sender, RoutedEventArgs e)
    {
        if (TryFindTemplateControl(sender, "PresetSizeDrawer", out AtomDrawer drawer))
        {
            drawer.SizeType = CustomizableSizeType.Large;
            drawer.IsOpen   = true;
        }
    }

    private void HandleOpenCustomSizeDrawer(object? sender, RoutedEventArgs e)
    {
        if (TryFindTemplateControl(sender, "PresetSizeDrawer", out AtomDrawer drawer))
        {
            drawer.SizeType   = CustomizableSizeType.Custom;
            drawer.DialogSize = new Dimension(400);
            drawer.IsOpen     = true;
        }
    }

    private void HandleOpenCustomPercentageSizeDrawer(object? sender, RoutedEventArgs e)
    {
        if (TryFindTemplateControl(sender, "PresetSizeDrawer", out AtomDrawer drawer))
        {
            drawer.SizeType   = CustomizableSizeType.Custom;
            drawer.DialogSize = new Dimension(50, DimensionUnitType.Percentage);
            drawer.IsOpen     = true;
        }
    }

    private void HandleOpenDefaultSizeDrawer(object? sender, RoutedEventArgs e)
    {
        if (TryFindTemplateControl(sender, "PresetSizeDrawer", out AtomDrawer drawer))
        {
            drawer.SizeType = CustomizableSizeType.Small;
            drawer.IsOpen   = true;
        }
    }

    private void HandleOpenFormDrawer(object? sender, RoutedEventArgs e)
    {
        if (TryFindTemplateControl(sender, "FormDrawer", out AtomDrawer drawer))
        {
            drawer.IsOpen = true;
        }
    }

    private void HandleCloseFormDrawer(object? sender, RoutedEventArgs e)
    {
        if (TryFindTemplateControl(sender, "FormDrawer", out AtomDrawer drawer))
        {
            drawer.IsOpen = false;
        }
    }

    private void HandleOpenMultilevelLevelTwoDrawer(object? sender, RoutedEventArgs e)
    {
        if (TryFindTemplateControl(sender, "MultiLevelDrawerLevelTwo", out AtomDrawer drawer))
        {
            drawer.IsOpen = true;
        }
    }

    private void HandleMultiLevelPlacementChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        var option = args.CheckedOption;
        if (option.IsChecked == true && option.Tag is AtomDrawerPlacement placement)
        {
            if (DataContext is DrawerViewModel vm)
            {
                vm.MultiLevelPlacement = placement;
            }
        }
    }

    private void HandleExtraAndFooterPlacementChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        var option = args.CheckedOption;
        if (option.IsChecked == true && option.Tag is AtomDrawerPlacement placement)
        {
            if (DataContext is DrawerViewModel vm)
            {
                vm.ExtraAndFooterPlacement = placement;
            }
        }
    }

    private void HandleCustomPlacementChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        var option = args.CheckedOption;
        if (option.IsChecked == true && option.Tag is AtomDrawerPlacement placement)
        {
            if (DataContext is DrawerViewModel vm)
            {
                vm.CustomPlacement = placement;
            }
        }
    }

    private static bool TryFindTemplateControl<T>(object? source, string name, out T control)
        where T : Control
    {
        var current = source as Control;
        while (current is not null)
        {
            if (current is T directControl &&
                directControl.Name == name)
            {
                control = directControl;
                return true;
            }

            var descendantControl = FindDescendantByName<T>(current, name);
            if (descendantControl is not null)
            {
                control = descendantControl;
                return true;
            }

            current = current.Parent as Control;
        }

        control = null!;
        return false;
    }

    private static T? FindDescendantByName<T>(Control root, string name)
        where T : Control
    {
        if (root is T typedRoot && typedRoot.Name == name)
        {
            return typedRoot;
        }

        return root.GetVisualDescendants().OfType<T>().FirstOrDefault(control => control.Name == name)
               ?? root.GetLogicalDescendants().OfType<T>().FirstOrDefault(control => control.Name == name);
    }

    private static void RefreshLocalizedOptionData(DrawerViewModel viewModel)
    {
        viewModel.AccountOwnerOptions =
        [
            SelectOption(DrawerShowCaseLangResourceKind.P2HeaderXiaoxiaoFu, "Xiaoxiao Fu", "xiao"),
            SelectOption(DrawerShowCaseLangResourceKind.P2HeaderMaomaoZhou, "Maomao Zhou", "mao")
        ];
        viewModel.AccountTypeOptions =
        [
            SelectOption(DrawerShowCaseLangResourceKind.P2HeaderPrivate, "private", "private"),
            SelectOption(DrawerShowCaseLangResourceKind.P2HeaderPublic, "public", "public")
        ];
        viewModel.AccountApproverOptions =
        [
            SelectOption(DrawerShowCaseLangResourceKind.P2HeaderJackMa, "Jack Ma", "jack"),
            SelectOption(DrawerShowCaseLangResourceKind.P2HeaderTomLiu, "Tom Liu", "tom")
        ];
    }

    private static SelectOption SelectOption(DrawerShowCaseLangResourceKind header, string fallback, string content)
    {
        return new SelectOption
        {
            Header  = DrawerShowCaseLanguage.Get(header, fallback),
            Content = content
        };
    }
}

internal static class DrawerShowCaseLanguage
{
    public static string Get(DrawerShowCaseLangResourceKind resourceKind, string fallback)
    {
        return GalleryLocalization.Get(resourceKind, fallback);
    }
}
