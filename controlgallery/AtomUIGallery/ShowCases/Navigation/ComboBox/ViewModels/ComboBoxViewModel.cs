using System.Reactive;
using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ComboBox;

public class ComboBoxViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "ComboBox";

    public IScreen HostScreen { get; }

    public string? UrlPathSegment => ID.ToString();

    private List<ComboBoxItemData>? _comboBoxItems;
    private ComboBoxItemData? _boundSelectedItem;

    public List<ComboBoxItemData>? ComboBoxItems
    {
        get => _comboBoxItems;
        set => this.RaiseAndSetIfChanged(ref _comboBoxItems, value);
    }

    public ComboBoxItemData? BoundSelectedItem
    {
        get => _boundSelectedItem;
        set
        {
            this.RaiseAndSetIfChanged(ref _boundSelectedItem, value);
            this.RaisePropertyChanged(nameof(BoundSelectedItemText));
        }
    }

    public string BoundSelectedItemText => BoundSelectedItem?.Text ?? "-";

    private List<ComboBoxItemData>? _semanticPreviewItems;

    public List<ComboBoxItemData>? SemanticPreviewItems
    {
        get => _semanticPreviewItems;
        set => this.RaiseAndSetIfChanged(ref _semanticPreviewItems, value);
    }

    private List<ComboBoxItemData>? _styleClassItems;

    public List<ComboBoxItemData>? StyleClassItems
    {
        get => _styleClassItems;
        set => this.RaiseAndSetIfChanged(ref _styleClassItems, value);
    }

    public ComboBoxViewModel(IScreen screen)
    {
        HostScreen                    = screen;
        ComboBoxItems                 = CreateComboBoxItems();
        SemanticPreviewItems          = CreateSemanticPreviewItems();
        StyleClassItems               = CreateSemanticPreviewItems();
        BoundSelectedItem             = ComboBoxItems[1];
        SetBoundSelectedItemCommand   = ReactiveCommand.Create(SetBoundSelectedItem);
        ClearBoundSelectedItemCommand = ReactiveCommand.Create(ClearBoundSelectedItem);
    }

    public ReactiveCommand<Unit, Unit> SetBoundSelectedItemCommand { get; }

    public ReactiveCommand<Unit, Unit> ClearBoundSelectedItemCommand { get; }

    private void SetBoundSelectedItem()
    {
        if (ComboBoxItems is { Count: > 2 })
        {
            BoundSelectedItem = ComboBoxItems[2];
        }
    }

    private void ClearBoundSelectedItem()
    {
        BoundSelectedItem = null;
    }

    private static List<ComboBoxItemData> CreateComboBoxItems()
    {
        return
        [
            new ComboBoxItemData { Text = "床前明月光" },
            new ComboBoxItemData { Text = "疑是地上霜" },
            new ComboBoxItemData { Text = "举头望明月" },
            new ComboBoxItemData { Text = "低头思故乡" }
        ];
    }

    private static List<ComboBoxItemData> CreateSemanticPreviewItems()
    {
        return
        [
            new ComboBoxItemData { Text = "Alpha" },
            new ComboBoxItemData { Text = "Beta" },
            new ComboBoxItemData { Text = "Gamma" }
        ];
    }

}

public class ComboBoxItemData
{
    public string Text { get; set; } = string.Empty;

    // ComboBox 从候选项推导显示文本时（可编辑输入框的 Text、溢出提示、过滤匹配）会依次尝试
    // TextSearch.Text、DisplayMemberBinding，最后回落到 object.ToString()。本演示模型不带绑定，
    // 若不声明文本表示，输入框会显示类型全名而不是候选项文本。
    public override string ToString() => Text;
}
