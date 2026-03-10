using System.Collections.ObjectModel;
using AtomUI.Controls;
using DynamicData;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public partial class DataGridViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "DataGrid";

    public IScreen HostScreen { get; }

    public string UrlPathSegment { get; } = ID.ToString();

    public ObservableCollection<DataGridBaseInfo> BasicCaseDataSource { get; }
    public ObservableCollection<DataGridBaseInfo> FilterAndSorterDataSource { get; }
    public ObservableCollection<MultiSorterDataType> MultiSorterDataSource { get; }
    public ObservableCollection<ExpandableRowDataType> ExpandableRowDataSource { get; }
    public ObservableCollection<GroupHeaderDataType> GroupHeaderDataSource { get; }
    public ObservableCollection<DataGridBaseInfo> FixedHeaderDataSource { get; }
    public ObservableCollection<DataGridBaseInfo> FixedColumnsDataSource { get; }
    public ObservableCollection<DataGridBaseInfo> FixedColumnsAndHeadersDataSource { get; }
    public ObservableCollection<DragColumnDataType> DragColumnDataSource { get; }
    public ObservableCollection<DataGridBaseInfo> DragRowDataSource { get; }
    public ObservableCollection<DataGridBaseInfo> DragRowManyDataSource { get; }
    public ObservableCollection<DataGridBaseInfo> CustomEmptyDataSource { get; }
    public ObservableCollection<DataGridBaseInfo> EditableCellsDataSource { get; }
    public ObservableCollection<DataGridBaseInfo> EditableRowsDataSource { get; }
    public ObservableCollection<DataGridBaseInfo> PagingGridDataSource { get; }

    public DataGridViewModel(IScreen screen)
    {
        HostScreen                       = screen;
        BasicCaseDataSource              = new ObservableCollection<DataGridBaseInfo>();
        FilterAndSorterDataSource        = new ObservableCollection<DataGridBaseInfo>();
        MultiSorterDataSource            = new ObservableCollection<MultiSorterDataType>();
        ExpandableRowDataSource          = new ObservableCollection<ExpandableRowDataType>();
        GroupHeaderDataSource            = new ObservableCollection<GroupHeaderDataType>();
        FixedHeaderDataSource            = new ObservableCollection<DataGridBaseInfo>();
        FixedColumnsDataSource           = new ObservableCollection<DataGridBaseInfo>();
        FixedColumnsAndHeadersDataSource = new ObservableCollection<DataGridBaseInfo>();
        DragColumnDataSource             = new ObservableCollection<DragColumnDataType>();
        DragRowDataSource                = new ObservableCollection<DataGridBaseInfo>();
        DragRowManyDataSource            = new ObservableCollection<DataGridBaseInfo>();
        CustomEmptyDataSource            = new ObservableCollection<DataGridBaseInfo>();
        EditableCellsDataSource          = new ObservableCollection<DataGridBaseInfo>();
        EditableRowsDataSource           = new ObservableCollection<DataGridBaseInfo>();
        PagingGridDataSource             = new ObservableCollection<DataGridBaseInfo>();
        InitBasicShowCaseDataSource();
        InitFilterAndSorterDataSource();
        InitMultiSorterDataSource();
        InitExpandableRowDataSource();
        InitGroupDataDataSource();
        InitFixedHeaderDataSource();
        InitFixedColumnsDataSource();
        InitFixedColumnsAndHeadersDataSource();
        InitDragColumnDataSource();
        InitDragRowDataSource();
        InitCustomEmptyDataSource();
        InitEditableCellsDataSource();
        InitEditableRowsDataSource();
        InitPagingGridDataSource();
    }

    private void InitBasicShowCaseDataSource()
    {
        List<DataGridBaseInfo> items =
        [
            new DataGridBaseInfo
            {
                Key   = "1", Name = "John Brown", Age = 32, Address = "New York No. 1 Lake Park",
                Money = "￥300,000.00",
                Tags =
                [
                    new TagInfo { Name = "NICE", Color      = "green" },
                    new TagInfo { Name = "DEVELOPER", Color = "geekblue" }
                ]
            },
            new DataGridBaseInfo
            {
                Key   = "2", Name = "Jim Green", Age = 42, Address = "London No. 1 Lake Park",
                Money = "￥1,256,000.00",
                Tags =
                [
                    new TagInfo { Name = "LOSER", Color = "volcano" }
                ]
            },
            new DataGridBaseInfo
            {
                Key   = "3", Name = "Joe Black", Age = 32, Address = "Sydney No. 1 Lake Park",
                Money = "￥120,000.00",
                Tags =
                [
                    new TagInfo { Name = "COOL", Color    = "green" },
                    new TagInfo { Name = "TEACHER", Color = "geekblue" }
                ]
            }
        ];
        BasicCaseDataSource.AddRange(items);
    }

    private void InitFilterAndSorterDataSource()
    {
        List<DataGridBaseInfo> items =
        [
            new DataGridBaseInfo { Key = "1", Name = "John Brown", Age = 32, Address = "New York No. 1 Lake Park" },
            new DataGridBaseInfo { Key = "2", Name = "Jim Green", Age  = 42, Address = "London No. 1 Lake Park" },
            new DataGridBaseInfo { Key = "3", Name = "Joe Black", Age  = 32, Address = "Sydney No. 1 Lake Park" },
            new DataGridBaseInfo { Key = "4", Name = "Joe Red", Age    = 32, Address = "London No. 2 Lake Park" }
        ];
        FilterAndSorterDataSource.AddRange(items);
    }

    private void InitMultiSorterDataSource()
    {
        List<MultiSorterDataType> items =
        [
            new MultiSorterDataType { Key = "1", Name = "John Brown", Chinese = 98, Math = 60, English = 70 },
            new MultiSorterDataType { Key = "2", Name = "Jim Green", Chinese  = 98, Math = 66, English = 89 },
            new MultiSorterDataType { Key = "3", Name = "Joe Black", Chinese  = 98, Math = 90, English = 70 },
            new MultiSorterDataType { Key = "3", Name = "Jim Red", Chinese    = 88, Math = 99, English = 89 },
        ];
        MultiSorterDataSource.AddRange(items);
    }

    private void InitExpandableRowDataSource()
    {
        List<ExpandableRowDataType> items =
        [
            new ExpandableRowDataType
            {
                Key         = "1", Name = "John Brown", Age = 32, Address = "New York No. 1 Lake Park",
                Description = "My name is John Brown, I am 32 years old, living in New York No. 1 Lake Park."
            },
            new ExpandableRowDataType
            {
                Key         = "2", Name = "Jim Green", Age = 42, Address = "London No. 1 Lake Park",
                Description = "London No. 1 Lake Park"
            },
            new ExpandableRowDataType
            {
                Key         = "3", Name = "Joe Black", Age = 32, Address = "Sydney No. 1 Lake Park",
                Description = "My name is Joe Black, I am 32 years old, living in Sydney No. 1 Lake Park."
            },
            new ExpandableRowDataType
            {
                Key         = "5", Name = "Joe Red", Age = 78, Address = "London No. 2 Lake Park",
                Description = "My name is Joe Black, I am 78 years old, London No. 2 Lake Park"
            }
        ];
        ExpandableRowDataSource.AddRange(items);
    }

    public void InitGroupDataDataSource()
    {
        var items = new List<GroupHeaderDataType>();
        for (var i = 0; i < 6; i++)
        {
            items.Add(new GroupHeaderDataType
            {
                Key            = i.ToString(),
                Name           = "John Brown",
                Age            = i + 1,
                Street         = "Lake Park",
                Building       = "C",
                Number         = 2035,
                CompanyAddress = "Lake Street 42",
                CompanyName    = "SoftLake Co",
                Gender         = "M"
            });
        }

        GroupHeaderDataSource.AddRange(items);
    }

    private void InitFixedHeaderDataSource()
    {
        List<DataGridBaseInfo> items = new List<DataGridBaseInfo>();
        for (var i = 0; i < 30; i++)
        {
            items.Add(new DataGridBaseInfo()
            {
                Key     = i.ToString(),
                Name    = $"Edward King {i}",
                Age     = 32,
                Address = $"London No. 1 Lake Park {i}",
            });
        }

        FixedHeaderDataSource.AddRange(items);
    }

    private void InitFixedColumnsDataSource()
    {
        List<DataGridBaseInfo> items = new List<DataGridBaseInfo>()
        {
            new DataGridBaseInfo { Key = "1", Name = "John Brown", Age = 32, Address = "New York No. 1 Lake Park" },
            new DataGridBaseInfo { Key = "2", Name = "Jim Green", Age  = 42, Address = "London No. 1 Lake Park" },
        };
        FixedColumnsDataSource.AddRange(items);
    }

    private void InitFixedColumnsAndHeadersDataSource()
    {
        List<DataGridBaseInfo> items = new List<DataGridBaseInfo>();
        for (var i = 0; i < 30; i++)
        {
            items.Add(new DataGridBaseInfo()
            {
                Key     = i.ToString(),
                Name    = $"Edward King {i}",
                Age     = 32,
                Address = $"London No. 1 Lake Park {i}",
            });
        }

        FixedColumnsAndHeadersDataSource.AddRange(items);
    }

    private void InitDragColumnDataSource()
    {
        List<DragColumnDataType> items = new List<DragColumnDataType>();
        items.Add(new DragColumnDataType()
        {
            Name    = "John Brown",
            Gender  = "male",
            Age     = 32,
            Email   = "John Brown@example.com",
            Address = "London No. 1 Lake Park"
        });
        items.Add(new DragColumnDataType()
        {
            Name    = "Jim Green",
            Gender  = "female",
            Age     = 42,
            Email   = "jimGreen@example.com",
            Address = "London No. 1 Lake Park"
        });
        items.Add(new DragColumnDataType()
        {
            Name    = "Joe Black",
            Gender  = "female",
            Age     = 32,
            Email   = "JoeBlack@example.com",
            Address = "Sidney No. 1 Lake Park"
        });
        items.Add(new DragColumnDataType()
        {
            Name    = "George Hcc",
            Gender  = "male",
            Age     = 20,
            Email   = "george@example.com",
            Address = "Sidney No. 1 Lake Park"
        });
        DragColumnDataSource.AddRange(items);
    }

    private void InitDragRowDataSource()
    {
        List<DataGridBaseInfo> items = new List<DataGridBaseInfo>();
        items.Add(new DataGridBaseInfo()
        {
            Name    = "John Brown",
            Age     = 32,
            Address = "London No. 1 Lake Park"
        });
        items.Add(new DataGridBaseInfo()
        {
            Name    = "Jim Green",
            Age     = 42,
            Address = "London No. 1 Lake Park"
        });
        items.Add(new DataGridBaseInfo()
        {
            Name    = "Joe Black",
            Age     = 32,
            Address = "Sidney No. 1 Lake Park"
        });
        items.Add(new DataGridBaseInfo()
        {
            Name    = "George Hcc",
            Age     = 20,
            Address = "Sidney No. 1 Lake Park"
        });
        
        DragRowDataSource.AddRange(items);

        var items1 = new List<DataGridBaseInfo>();
        for (var i = 0; i < 30; i++)
        {
            items1.Add(new DataGridBaseInfo()
            {
                Name    = "John Brown",
                Age     = 32,
                Address = $"London No. {i + 1} Lake Park"
            });
        }
        DragRowManyDataSource.AddRange(items1);
    }

    private void InitCustomEmptyDataSource()
    {
        List<DataGridBaseInfo> items = new List<DataGridBaseInfo>();
        items.Add(new DataGridBaseInfo()
        {
            Name    = "John Brown",
            Age     = 32,
            Address = "London No. 1 Lake Park"
        });
        items.Add(new DataGridBaseInfo()
        {
            Name    = "Jim Green",
            Age     = 42,
            Address = "London No. 1 Lake Park"
        });
        items.Add(new DataGridBaseInfo()
        {
            Name    = "Joe Black",
            Age     = 32,
            Address = "Sidney No. 1 Lake Park"
        });
        items.Add(new DataGridBaseInfo()
        {
            Name    = "George Hcc",
            Age     = 18,
            Address = "Sidney No. 1 Lake Park"
        });
        items.Add(new DataGridBaseInfo()
        {
            Name    = "Joe Black",
            Age     = 32,
            Address = "Sidney No. 1 Lake Park"
        });
        items.Add(new DataGridBaseInfo()
        {
            Name    = "George Hcc",
            Age     = 44,
            Address = "Sidney No. 2 Lake Park"
        });
        CustomEmptyDataSource.AddRange(items);
    }

    private void InitEditableCellsDataSource()
    {
        List<DataGridBaseInfo> items = new List<DataGridBaseInfo>();
        items.Add(new DataGridBaseInfo()
        {
            Name    = "John Brown",
            Age     = 32,
            Address = "London No. 1 Lake Park"
        });
        items.Add(new DataGridBaseInfo()
        {
            Name    = "Jim Green",
            Age     = 42,
            Address = "London No. 3 Lake Park"
        });
        EditableCellsDataSource.AddRange(items);
    }

    private void InitEditableRowsDataSource()
    {
        List<DataGridBaseInfo> items = new List<DataGridBaseInfo>();
        for (int i = 0; i < 30; i++)
        {
            items.Add(new  DataGridBaseInfo
            {
                Name    = $"Edward {i + 1}",
                Age    = 32,
                Address = $"London Park no. {i + 1}"
            });
        }
        EditableRowsDataSource.AddRange(items);
    }
    
    private void InitPagingGridDataSource()
    {
        PagingGridDataSource.AddRange(RandomDataGenerator.GenerateRandomData(100));
    }
}

static class RandomDataGenerator
{
    private static readonly Random Random = new Random();
    
    private static readonly string[] FirstNames = { "James", "Mary", "John", "Patricia", "Robert", "Jennifer", "Michael", "Linda", "William", "Elizabeth" };
    private static readonly string[] LastNames = { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez" };
    private static readonly string[] Cities = { "New York", "Los Angeles", "Chicago", "Houston", "Phoenix", "Philadelphia", "San Antonio", "San Diego", "Dallas", "San Jose" };
    private static readonly string[] Streets = { "Main St", "Park Ave", "Elm St", "Oak St", "Pine Rd", "Maple Ave", "Cedar Ln", "Washington Blvd", "Lake Shore Dr", "Sunset Blvd" };
    private static readonly string[] TagNames = { "VIP", "NEW", "STAFF", "MANAGER", "DEVELOPER", "DESIGNER", "ANALYST", "LEADER", "EXPERT", "TRAINEE", "VIP", "SPECIAL" };
    private static readonly string[] Colors = { "red", "volcano", "orange", "gold", "yellow", "lime", "green", "cyan", "blue", "geekblue", "purple" };

    public static List<DataGridBaseInfo> GenerateRandomData(int count = 10)
    {
        return Enumerable.Range(1, count).Select(i => new DataGridBaseInfo
        {
            Key = i.ToString(),
            Name = $"{FirstNames[Random.Next(FirstNames.Length)]} {LastNames[Random.Next(LastNames.Length)]}",
            Age = Random.Next(18, 66),
            Address = $"{Random.Next(1, 10000)} {Streets[Random.Next(Streets.Length)]}, {Cities[Random.Next(Cities.Length)]}",
            Money = $"￥{Random.Next(1000, 10000000):N2}",
            Tags = GenerateRandomTags(Random.Next(1, 4))
        }).ToList();
    }

    private static List<TagInfo> GenerateRandomTags(int count)
    {
        return Enumerable.Range(0, count).Select(_ => new TagInfo
        {
            Name  = TagNames[Random.Next(TagNames.Length)],
            Color = Colors[Random.Next(Colors.Length)]
        }).ToList();
    }
}

public class DataGridBaseInfo
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Money { get; set; } = string.Empty;
    public List<TagInfo> Tags { get; set; } = new();
}

public class TagInfo
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}

public class MultiSorterDataType
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Chinese { get; set; }
    public int Math { get; set; }
    public int English { get; set; }
}

public class ExpandableRowDataType : DataGridBaseInfo
{
    public string Description { get; set; } = string.Empty;
}

public class GroupHeaderDataType
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Street { get; set; } = string.Empty;
    public string Building { get; set; } = string.Empty;
    public string CompanyAddress { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public int Number { get; set; }
}

public class DragColumnDataType
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}