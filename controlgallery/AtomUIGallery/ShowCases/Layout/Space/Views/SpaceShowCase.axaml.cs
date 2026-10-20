using System.Reactive.Disposables;
using AtomUI;
using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using AtomUISlider = AtomUI.Desktop.Controls.Slider;
using Avalonia.Controls.Primitives;

namespace AtomUIGallery.ShowCases.Space;

public partial class SpaceShowCase : GalleryReactiveUserControl<SpaceViewModel>
{
    public const string LanguageId = nameof(SpaceShowCase);

    public SpaceShowCase()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            RefreshCurrentViewModelData();
            var languageManager = Application.Current is { } application
                ? global::AtomUI.ApplicationExtensions.GetLanguageManager(application)
                : null;
            if (languageManager != null)
            {
                EventHandler<LanguageChangedEventArgs> handler = (_, _) => RefreshCurrentViewModelData();
                languageManager.LanguageChanged += handler;
                disposables.Add(Disposable.Create(() => languageManager.LanguageChanged -= handler));
            }
        });
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is SpaceViewModel viewModel)
        {
            viewModel.SizeType = CustomizableSizeType.Small;
            RefreshLocalizedOptionData(viewModel);
        }

    }

    public void HandleSizeTypeChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not AtomUIRadioButton radioButton ||
            radioButton.IsChecked != true ||
            radioButton.Tag is not CustomizableSizeType sizeType)
        {
            return;
        }

        if (DataContext is SpaceViewModel viewModel)
        {
            viewModel.SizeType = sizeType;
        }

        if (!TryFindTemplateControl<AtomUISlider>(radioButton, "CustomSizeSlider", out var customSizeSlider))
        {
            return;
        }

        customSizeSlider.IsVisible = sizeType == CustomizableSizeType.Custom;
        if (TryFindTemplateControl<AtomUI.Desktop.Controls.Space>(radioButton, "SizeDemoSpace", out var sizeDemoSpace))
        {
            ApplySizeDemoSpacing(sizeDemoSpace, sizeType, customSizeSlider.Value);
        }
    }

    public void HandleCustomSpacingValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (sender is not AtomUISlider slider ||
            DataContext is not SpaceViewModel { SizeType: CustomizableSizeType.Custom })
        {
            return;
        }

        if (TryFindTemplateControl<AtomUI.Desktop.Controls.Space>(slider, "SizeDemoSpace", out var sizeDemoSpace))
        {
            ApplySizeDemoSpacing(sizeDemoSpace, CustomizableSizeType.Custom, slider.Value);
        }
    }

    private static void ApplySizeDemoSpacing(AtomUI.Desktop.Controls.Space space,
                                             CustomizableSizeType sizeType,
                                             double customSpacing)
    {
        if (sizeType == CustomizableSizeType.Custom)
        {
            var spacing = Math.Max(0, customSpacing);
            space.ItemSpacing = spacing;
            space.LineSpacing = spacing;
            return;
        }

        space.ClearValue(AtomUI.Desktop.Controls.Space.ItemSpacingProperty);
        space.ClearValue(AtomUI.Desktop.Controls.Space.LineSpacingProperty);
    }

    private void RefreshCurrentViewModelData()
    {
        if (DataContext is SpaceViewModel viewModel)
        {
            RefreshLocalizedOptionData(viewModel);
        }
    }

    private static void RefreshLocalizedOptionData(SpaceViewModel viewModel)
    {
        viewModel.ProvinceOptions =
        [
            SelectOption(SpaceShowCaseLangResourceKind.P2HeaderZhejiang, "Zhejiang", "Zhejiang"),
            SelectOption(SpaceShowCaseLangResourceKind.P2HeaderJiangsu, "Jiangsu", "Jiangsu")
        ];
        viewModel.BasicOptions =
        [
            SelectOption(SpaceShowCaseLangResourceKind.P2HeaderOption1, "Option1", "Option1"),
            SelectOption(SpaceShowCaseLangResourceKind.P2HeaderOption2, "Option2", "Option2")
        ];
        viewModel.FirstNestedOptions =
        [
            SelectOption(SpaceShowCaseLangResourceKind.P2HeaderOption1N1, "Option1-1", "Option1-1"),
            SelectOption(SpaceShowCaseLangResourceKind.P2HeaderOption2N1, "Option2-1", "Option2-1")
        ];
        viewModel.SecondNestedOptions =
        [
            SelectOption(SpaceShowCaseLangResourceKind.P2HeaderOption2N1, "Option2-1", "Option2-1"),
            SelectOption(SpaceShowCaseLangResourceKind.P2HeaderOption2N2, "Option2-2", "Option2-2")
        ];
        viewModel.ConditionOptions =
        [
            SelectOption(SpaceShowCaseLangResourceKind.P2HeaderBetween, "Between", "1"),
            SelectOption(SpaceShowCaseLangResourceKind.P2HeaderExcept, "Except", "2")
        ];
        viewModel.AuthActionOptions =
        [
            SelectOption(SpaceShowCaseLangResourceKind.P2HeaderSignUp, "Sign Up", "Sign Up"),
            SelectOption(SpaceShowCaseLangResourceKind.P2HeaderSignIn, "Sign In", "Sign In")
        ];
        viewModel.AutoCompleteTextOptions =
        [
            AutoCompleteOption(SpaceShowCaseLangResourceKind.P2HeaderTextN1, "text 1", "text 1"),
            AutoCompleteOption(SpaceShowCaseLangResourceKind.P2HeaderTextN2, "text 2", "text 2")
        ];
        viewModel.AddressCascaderOptions = BuildAddressCascaderOptions();
        viewModel.TreeSelectNodes = BuildTreeSelectNodes();
    }

    private static List<ICascaderOption> BuildAddressCascaderOptions()
    {
        return
        [
            CascaderOption(SpaceShowCaseLangResourceKind.P2HeaderZhejiang, "Zhejiang", "zhejiang",
            [
                CascaderOption(SpaceShowCaseLangResourceKind.P2HeaderHangzhou, "Hangzhou", "hangzhou",
                [
                    CascaderOption(SpaceShowCaseLangResourceKind.P2HeaderWestLake, "West Lake", "xihu"),
                    CascaderOption(SpaceShowCaseLangResourceKind.P2HeaderLingyinShi, "Lingyin Temple", "lingyinshi")
                ])
            ]),
            CascaderOption(SpaceShowCaseLangResourceKind.P2HeaderJiangsu, "Jiangsu", "jiangsu",
            [
                CascaderOption(SpaceShowCaseLangResourceKind.P2HeaderNanjing, "Nanjing", "nanjing",
                [
                    CascaderOption(SpaceShowCaseLangResourceKind.P2HeaderZhongHuaMen, "Zhong Hua Men", "zhonghuamen")
                ])
            ])
        ];
    }

    private static List<ITreeItemNode> BuildTreeSelectNodes()
    {
        return
        [
            TreeNode(SpaceShowCaseLangResourceKind.P2HeaderParentN1, "parent 1", "parent 1",
            [
                TreeNode(SpaceShowCaseLangResourceKind.P2HeaderParentN1N0, "parent 1-0", "parent 1-0",
                [
                    TreeNode(SpaceShowCaseLangResourceKind.P2HeaderLeaf1, "leaf1", "leaf1"),
                    TreeNode(SpaceShowCaseLangResourceKind.P2HeaderLeaf2, "leaf2", "leaf2")
                ]),
                TreeNode(SpaceShowCaseLangResourceKind.P2HeaderParentN1N1, "parent 1-1", "parent 1-1",
                [
                    TreeNode(SpaceShowCaseLangResourceKind.P2HeaderLeaf3, "leaf3", "leaf3")
                ])
            ])
        ];
    }

    private static SelectOption SelectOption(SpaceShowCaseLangResourceKind header, string fallback, string content)
    {
        return new SelectOption
        {
            Header  = SpaceShowCaseLanguage.Get(header, fallback),
            Content = content
        };
    }

    private static AutoCompleteOption AutoCompleteOption(SpaceShowCaseLangResourceKind header, string fallback, string content)
    {
        return new AutoCompleteOption
        {
            Header  = SpaceShowCaseLanguage.Get(header, fallback),
            Content = content
        };
    }

    private static CascaderOption CascaderOption(
        SpaceShowCaseLangResourceKind header,
        string fallback,
        string value,
        IList<ICascaderOption>? children = null)
    {
        return new CascaderOption
        {
            Header   = SpaceShowCaseLanguage.Get(header, fallback),
            Value    = value,
            Children = children ?? []
        };
    }

    private static TreeItemNode TreeNode(
        SpaceShowCaseLangResourceKind header,
        string fallback,
        string value,
        IList<ITreeItemNode>? children = null)
    {
        return new TreeItemNode
        {
            Header   = SpaceShowCaseLanguage.Get(header, fallback),
            Value    = value,
            Children = children ?? []
        };
    }

    private static bool TryFindTemplateControl<T>(Control source, string name, out T control)
        where T : Control
    {
        var current = source;
        while (current is not null)
        {
            if (current is T directControl &&
                current.Name == name)
            {
                control = directControl;
                return true;
            }

            var descendantControl = FindNamedDescendant<T>(current, name);
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

    private static T? FindNamedDescendant<T>(Control root, string name)
        where T : Control
    {
        if (root is T typedRoot && typedRoot.Name == name)
        {
            return typedRoot;
        }

        return root.GetVisualDescendants().OfType<T>().FirstOrDefault(control => control.Name == name)
               ?? root.GetLogicalDescendants().OfType<T>().FirstOrDefault(control => control.Name == name);
    }
}

internal static class SpaceShowCaseLanguage
{
    public static string Get(SpaceShowCaseLangResourceKind resourceKind, string fallback)
    {
        return GalleryLocalization.Get(resourceKind, fallback);
    }
}
