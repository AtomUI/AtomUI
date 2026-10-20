using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using AtomUI.Controls;
using AtomUI.Controls.Data;
using AtomUI.Desktop.Controls;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace AtomUIGallery.ShowCases.Transfer;

public partial class TransferShowCase : GalleryReactiveUserControl<TransferViewModel>
{
    public const string LanguageId = nameof(TransferShowCase);

    public TransferShowCase()
    {
        this.WhenActivated(disposables =>
        {
            if (DataContext is TransferViewModel viewModel)
            {
                RefreshLocalizedTransferItems(viewModel);
                viewModel.TransferFilterValueSelector = record =>
                {
                    if (record is ListItemData listItemData)
                    {
                        return listItemData.Content;
                    }
                    return record?.ToString();
                };

                var languageManager = GalleryLocalization.GetLanguageManager();
                if (languageManager != null)
                {
                    EventHandler<LanguageChangedEventArgs> handler = (_, _) => RefreshLocalizedTransferItems(viewModel);
                    languageManager.LanguageChanged += handler;
                    Disposable.Create(() => languageManager.LanguageChanged -= handler)
                              .DisposeWith(disposables);
                }

                Disposable.Create(() =>
                {
                    viewModel.BasicTransferItems                  = null;
                    viewModel.SemanticDemoTargetKeys              = null;
                    viewModel.SemanticPreviewTargetKeys           = null;
                    viewModel.SemanticTreePreviewTargetKeys        = null;
                    viewModel.OneWayTransferItems                 = null;
                    viewModel.SearchTransferItems                 = null;
                    viewModel.ControlledTransferItems             = null;
                    viewModel.ControlledTransferTargetKeys        = null;
                    viewModel.ControlledTransferSelectedKeys      = null;
                    viewModel.PaginationTransferItems             = null;
                    viewModel.PaginationTransferDefaultTargetKeys = null;
                    viewModel.GridDataTransformItems              = null;
                    viewModel.AdvanceTransferItems                = null;
                    viewModel.AdvanceTransferDefaultTargetKeys    = null;
                    viewModel.TransferTreeNodes                   = null;

                }).DisposeWith(disposables);
            }
        });
        InitializeComponent();
    }

    private void ReloadAdvancedTransferItems(object? sender, RoutedEventArgs e)
    {
        if (sender is Control control &&
            control.FindAncestorOfType<ListTransfer>() is { } transfer &&
            DataContext is TransferViewModel viewModel)
        {
            transfer.TargetKeys = viewModel.AdvanceTransferDefaultTargetKeys;
        }
    }

    private void RefreshLocalizedTransferItems(TransferViewModel viewModel)
    {
        InitBasicTransferItems(viewModel);
        InitOneWayTransferItems(viewModel);
        InitSearchTransferItems(viewModel);
        InitControlledTransferItems(viewModel);
        InitPaginationTransferItems(viewModel);
        InitDataGridTransferItems(viewModel);
        InitAdvanceTransferItems(viewModel);
        InitTreeViewTransferItems(viewModel);
    }

    private void InitBasicTransferItems(TransferViewModel vm)
    {
        var items = new List<IListItemData>();
        for (var i = 0; i < 20; i++)
        {
            items.Add(new ListItemData()
            {
                ItemKey = $"{i}",
                Content = TransferShowCaseLanguage.Format(
                    TransferShowCaseLangResourceKind.P2ItemContentFormat,
                    "content{0}",
                    i + 1)
            });
        }

        vm.BasicTransferItems = items;
        vm.SemanticDemoTargetKeys = Enumerable.Range(11, 9)
                                              .Select(index => new EntityKey($"{index}"))
                                              .ToList();
        vm.SemanticPreviewTargetKeys = new List<EntityKey>
        {
            new("3"),
            new("9")
        };
        vm.SemanticTreePreviewTargetKeys = new List<EntityKey>
        {
            new("0-1-0"),
            new("0-1-1")
        };
    }

    private void InitOneWayTransferItems(TransferViewModel vm)
    {
        var items = new List<IListItemData>();
        for (var i = 0; i < 20; i++)
        {
            items.Add(new ListItemData()
            {
                ItemKey = $"{i}",
                Content = TransferShowCaseLanguage.Format(
                    TransferShowCaseLangResourceKind.P2ItemContentFormat,
                    "content{0}",
                    i + 1),
                IsEnabled = !(i % 3 < 1)
            });
        }

        vm.OneWayTransferItems = items;
    }

    private void InitSearchTransferItems(TransferViewModel vm)
    {
        var items = new List<IListItemData>();
        for (var i = 0; i < 20; i++)
        {
            items.Add(new SearchCaseItemData()
            {
                ItemKey   = $"{i}",
                Content   = TransferShowCaseLanguage.Format(
                    TransferShowCaseLangResourceKind.P2ItemContentFormat,
                    "content{0}",
                    i + 1),
                Description = TransferShowCaseLanguage.Format(
                    TransferShowCaseLangResourceKind.P2ItemDescriptionFormat,
                    "description of content{0}",
                    i + 1)
            });
        }

        vm.SearchTransferItems = items;
    }

    private void InitControlledTransferItems(TransferViewModel vm)
    {
        var items = new List<IListItemData>();
        for (var i = 0; i < 8; i++)
        {
            items.Add(new ListItemData()
            {
                ItemKey = $"{i}",
                Content = TransferShowCaseLanguage.Format(
                    TransferShowCaseLangResourceKind.P2ItemContentFormat,
                    "content{0}",
                    i + 1)
            });
        }

        vm.ControlledTransferItems        = items;
        vm.ControlledTransferTargetKeys   = ["1"];
        vm.ControlledTransferSelectedKeys = ["2"];
    }

    private void InitAdvanceTransferItems(TransferViewModel vm)
    {
        var items      = new List<IListItemData>();
        var targetKeys = new List<EntityKey>();
        for (var i = 0; i < 20; i++)
        {
            var item = new SearchCaseItemData()
            {
                ItemKey     = $"{i}",
                Content     = TransferShowCaseLanguage.Format(
                    TransferShowCaseLangResourceKind.P2ItemContentFormat,
                    "content{0}",
                    i + 1),
                Description = TransferShowCaseLanguage.Format(
                    TransferShowCaseLangResourceKind.P2ItemDescriptionFormat,
                    "description of content{0}",
                    i + 1)
            };
            items.Add(item);
            if (i % 2 == 0)
            {
                targetKeys.Add(item.ItemKey!.Value);
            }
        }
        vm.AdvanceTransferItems             = items;
        vm.AdvanceTransferDefaultTargetKeys = targetKeys;
    }

    private void InitPaginationTransferItems(TransferViewModel vm)
    {
        var items      = new List<IListItemData>();
        var targetKeys = new List<EntityKey>();
        for (var i = 0; i < 2000; i++)
        {
            var item = new SearchCaseItemData()
            {
                ItemKey     = $"{i}",
                Content     = TransferShowCaseLanguage.Format(
                    TransferShowCaseLangResourceKind.P2ItemContentFormat,
                    "content{0}",
                    i + 1),
                Description = TransferShowCaseLanguage.Format(
                    TransferShowCaseLangResourceKind.P2ItemDescriptionFormat,
                    "description of content{0}",
                    i + 1)
            };
            items.Add(item);
            if (i % 2 == 0)
            {
                targetKeys.Add(item.ItemKey!.Value);
            }
        }
        vm.PaginationTransferItems             = items;
        vm.PaginationTransferDefaultTargetKeys = targetKeys;
    }

    private void InitDataGridTransferItems(TransferViewModel vm)
    {
        var          items = new List<DataGridTransferData>();
        List<string> tags =
        [
            TransferShowCaseLanguage.Get(TransferShowCaseLangResourceKind.P2TagCat, "cat"),
            TransferShowCaseLanguage.Get(TransferShowCaseLangResourceKind.P2TagDog, "dog"),
            TransferShowCaseLanguage.Get(TransferShowCaseLangResourceKind.P2TagBird, "bird")
        ];
        for (var i = 0; i < 20; i++)
        {
            items.Add(new DataGridTransferData()
            {
                ItemKey     = $"{i}",
                Title       = TransferShowCaseLanguage.Format(
                    TransferShowCaseLangResourceKind.P2ItemContentFormat,
                    "content{0}",
                    i + 1),
                Description = TransferShowCaseLanguage.Format(
                    TransferShowCaseLangResourceKind.P2ItemDescriptionFormat,
                    "description of content{0}",
                    i + 1),
                Tag         = tags[i % 3]
            });
        }
        vm.GridDataTransformItems = items;
    }

    private void InitTreeViewTransferItems(TransferViewModel vm)
    {
        vm.TransferTreeNodes = [
            new TreeItemNode()
            {
                ItemKey = "0-0",
                Header = "0-0"
            },
            new TreeItemNode()
            {
                ItemKey = "0-1",
                Header = "0-1",
                Children = [
                    new TreeItemNode()
                    {
                        ItemKey = "0-1-0",
                        Header = "0-1-0",
                    },
                    new TreeItemNode()
                    {
                        ItemKey = "0-1-1",
                        Header = "0-1-1",
                    }
                ]
            },
            new TreeItemNode()
            {
                ItemKey = "0-2",
                Header = "0-2"
            },
            new TreeItemNode()
            {
                ItemKey = "0-3",
                Header = "0-3"
            },
            new TreeItemNode()
            {
                ItemKey = "0-4",
                Header = "0-4"
            }
        ];
    }

    private void AddControlledTransferTargetKey(object? sender, RoutedEventArgs e)
    {
        if (DataContext is TransferViewModel viewModel &&
            viewModel.ControlledTransferTargetKeys != null &&
            !viewModel.ControlledTransferTargetKeys.Contains("3"))
        {
            viewModel.ControlledTransferTargetKeys.Add("3");
        }
    }

    private void ClearControlledTransferTargetKeys(object? sender, RoutedEventArgs e)
    {
        if (DataContext is TransferViewModel viewModel)
        {
            viewModel.ControlledTransferTargetKeys?.Clear();
        }
    }

    private void SelectControlledTransferSourceKey(object? sender, RoutedEventArgs e)
    {
        if (DataContext is TransferViewModel viewModel &&
            viewModel.ControlledTransferSelectedKeys != null &&
            !viewModel.ControlledTransferSelectedKeys.Contains("4"))
        {
            viewModel.ControlledTransferSelectedKeys.Add("4");
        }
    }

}

internal static class TransferShowCaseLanguage
{
    public static string Get(TransferShowCaseLangResourceKind resourceKind, string fallback)
    {
        return GalleryLocalization.Get(resourceKind, fallback);
    }

    public static string Format(TransferShowCaseLangResourceKind resourceKind, string fallback, params object?[] args)
    {
        return GalleryLocalization.Format(resourceKind, fallback, args);
    }
}
