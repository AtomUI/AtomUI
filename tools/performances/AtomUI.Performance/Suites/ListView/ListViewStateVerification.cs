using AtomUI.Controls.Data;
using AtomUI.Controls.Utils;
using AtomUI.Desktop.Controls;

namespace AtomUI.Performance;

internal static partial class Program
{
    private static bool RunListViewStateVerification()
    {
        var failures = new List<string>();

        VerifyListViewSelectedIndicatorLifecycle(failures);
        VerifyListViewCollectionFilterLifecycle(failures);
        VerifyListViewEmptyIndicatorLazyMaterialization(failures);
        VerifyListViewPaginationLifecycle(failures);
        // VerifyListViewSelectionModelLifecycle 已删除：ListView.Selection 变为只读的内部固定
        // ListViewSelectionModel（不再支持外部替换选择模型），替换模型的订阅释放验证已无等价 API。

        if (failures.Count == 0)
        {
            Console.WriteLine("ListView state verification passed.");
            return true;
        }

        Console.Error.WriteLine("ListView state verification failed:");
        foreach (var failure in failures)
        {
            Console.Error.WriteLine($"- {failure}");
        }
        return false;
    }

    private static void VerifyListViewSelectedIndicatorLifecycle(ICollection<string> failures)
    {
        var listView = CreateListView(
            CreateListViewItems(5),
            isShowSelectedIndicator: true,
            selectedIndex: 0);
        using var realized = RealizeControl(listView);

        var selectedItem = listView.ContainerFromIndex(0) as ListViewItem;
        Expect(selectedItem != null,
            "Selected ListView item should have a realized container.",
            failures);
        if (selectedItem == null)
        {
            return;
        }

        Expect(FindSelectedIndicatorPresenter(selectedItem) != null,
            "Selected ListView item should keep selected indicator presenter in the static template.",
            failures);
        Expect(FindSelectedIndicatorPresenter(selectedItem)?.IsVisible == true,
            "Selected ListView item should show selected indicator presenter.",
            failures);

        listView.SelectedIndex = -1;
        RefreshLayout(realized.Window);
        Expect(FindSelectedIndicatorPresenter(selectedItem)?.IsVisible == false,
            "Unselected ListView item should hide selected indicator presenter.",
            failures);

        listView.SelectedIndex = 1;
        RefreshLayout(realized.Window);
        var nextSelectedItem = listView.ContainerFromIndex(1) as ListViewItem;
        Expect(nextSelectedItem != null && FindSelectedIndicatorPresenter(nextSelectedItem)?.IsVisible == true,
            "Selecting another ListView item should show selected indicator presenter again.",
            failures);
    }

    private static void VerifyListViewCollectionFilterLifecycle(ICollection<string> failures)
    {
        var listView = CreateListView(CreateListViewItems(20));
        using var realized = RealizeControl(listView);
        var collectionView = listView.ItemsSource as IListCollectionView;

        Expect(collectionView != null,
            "ListView should wrap ItemsSource in IListCollectionView.",
            failures);
        if (collectionView == null)
        {
            return;
        }

        Expect(collectionView.Filter == null,
            "Default ListView collection view should not install a filter callback.",
            failures);

        listView.Filter = ValueFilterFactory.BuildFilter(ValueFilterMode.Contains);
        listView.FilterValue = "1";
        RefreshLayout(realized.Window);
        Expect(collectionView.Filter != null,
            "Filtered ListView should install a filter callback.",
            failures);

        listView.FilterValue = null;
        RefreshLayout(realized.Window);
        Expect(collectionView.Filter == null,
            "Clearing ListView filter should remove the internal filter callback.",
            failures);
    }

    private static void VerifyListViewEmptyIndicatorLazyMaterialization(ICollection<string> failures)
    {
        var listView = CreateListView(CreateListViewItems(5));
        using var realized = RealizeControl(listView);

        Expect(FindVisualByType<Empty>(listView)?.IsVisible == false,
            "Non-empty ListView should keep the default Empty indicator hidden.",
            failures);

        listView.ItemsSource = Array.Empty<IListItemData>();
        RefreshLayout(realized.Window);
        Expect(FindVisualByType<Empty>(listView)?.IsVisible == true,
            "Empty ListView should show the default Empty indicator.",
            failures);
    }

    private static void VerifyListViewPaginationLifecycle(ICollection<string> failures)
    {
        var oldPagination = new Pagination();
        var listView = CreateListView(
            CreateListViewItems(200),
            pageSize: 100,
            pagination: oldPagination);
        using var realized = RealizeControl(listView);

        Expect(oldPagination.Align == listView.BottomPaginationAlign,
            "Bottom pagination should sync align from ListView.",
            failures);
        Expect(oldPagination.IsMotionEnabled == listView.IsMotionEnabled,
            "Bottom pagination should sync motion from ListView.",
            failures);

        listView.PaginationVisibility = ListPaginationVisibility.None;
        RefreshLayout(realized.Window);
        Expect(!oldPagination.IsVisible,
            "Bottom pagination should hide when PaginationVisibility is None.",
            failures);
        Expect(oldPagination.IsMotionEnabled == listView.IsMotionEnabled,
            "Changing PaginationVisibility must not overwrite pagination motion state.",
            failures);

        var collectionView = listView.ItemsSource as IListCollectionView;
        var firstPageItems = collectionView?.Cast<IListItemData>().ToList();
        Expect(firstPageItems?.Count == 100,
            "Paged ListView should enumerate only the current page.",
            failures);
        Expect(firstPageItems?.FirstOrDefault()?.Content?.ToString() == "Content 0",
            "Paged ListView first page should start with the first source item.",
            failures);

        collectionView?.MoveToPage(1);
        RefreshLayout(realized.Window);
        var secondPageItems = collectionView?.Cast<IListItemData>().ToList();
        Expect(secondPageItems?.Count == 100,
            "Paged ListView should enumerate the requested page size after page changes.",
            failures);
        Expect(secondPageItems?.FirstOrDefault()?.Content?.ToString() == "Content 100",
            "Paged ListView second page should start at the page offset.",
            failures);

        listView.PaginationVisibility = ListPaginationVisibility.Bottom;
        RefreshLayout(realized.Window);
        Expect(oldPagination.IsVisible,
            "Bottom pagination should show when PaginationVisibility is Bottom.",
            failures);

        listView.BottomPagination = null;
        RefreshLayout(realized.Window);
        listView.PaginationVisibility = ListPaginationVisibility.None;
        RefreshLayout(realized.Window);
        Expect(oldPagination.IsVisible,
            "Detached bottom pagination should not be mutated by later ListView visibility changes.",
            failures);

        var newPagination = new Pagination();
        listView.BottomPagination = newPagination;
        RefreshLayout(realized.Window);
        Expect(newPagination.PageSize == listView.PageSize,
            "New bottom pagination should receive the current ListView page size.",
            failures);
    }
}
