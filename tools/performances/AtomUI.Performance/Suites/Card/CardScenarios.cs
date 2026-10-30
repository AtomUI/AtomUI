using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using AtomUI.Icons.AntDesign;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using AtomTabItem = AtomUI.Desktop.Controls.TabItem;
using AtomTextBlock = AtomUI.Desktop.Controls.TextBlock;
using AtomToggleSwitch = AtomUI.Desktop.Controls.ToggleSwitch;

namespace AtomUI.Performance;

internal static partial class Program
{
    private static readonly Lazy<Bitmap> CardCoverBitmap = new(CreateCardCoverBitmap);

    private static IReadOnlyList<PerfScenario> CreateCardScenarios()
    {
        return
        [
            new PerfScenario("Card.ContentOnly", _ => CreateContentOnlyCard()),
            new PerfScenario("Card.HeaderExtra", _ => CreateHeaderExtraCard()),
            new PerfScenario("Card.Borderless", _ => CreateHeaderExtraCard(CardStyleVariant.Borderless)),
            new PerfScenario("Card.CoverMeta", _ => CreateCoverMetaCard()),
            new PerfScenario("Card.Actions3", _ => CreateActionsCard()),
            new PerfScenario("Card.LoadingFalse", _ => CreateLoadingCard(isLoading: false)),
            new PerfScenario("Card.LoadingTrue", _ => CreateLoadingCard(isLoading: true)),
            new PerfScenario("Card.Grid7", _ => CreateGridCard()),
            new PerfScenario("Card.Tabs", _ => CreateTabsCard()),
            new PerfScenario("Card.GalleryShape.Batch18", _ => CreateCardGalleryShape())
        ];
    }

    private static Card CreateContentOnlyCard()
    {
        return new Card
        {
            Width               = 300,
            HorizontalAlignment = HorizontalAlignment.Left,
            Content             = CreateCardTextStack()
        };
    }

    private static Card CreateHeaderExtraCard(CardStyleVariant styleVariant = CardStyleVariant.Outlined)
    {
        return new Card
        {
            Header              = "Card title",
            Extra               = CreateCardMoreLink(),
            StyleVariant        = styleVariant,
            Width               = 300,
            HorizontalAlignment = HorizontalAlignment.Left,
            Content             = CreateCardTextStack()
        };
    }

    private static Card CreateCoverMetaCard()
    {
        return new Card
        {
            Width               = 240,
            IsHoverable         = true,
            HorizontalAlignment = HorizontalAlignment.Left,
            Cover               = CreateCardCoverImage(),
            Content = new CardMetaContent
            {
                Header  = "Europe Street beat",
                Content = "www.instagram.com"
            }
        };
    }

    private static Card CreateActionsCard()
    {
        var card = new Card
        {
            Width               = 300,
            HorizontalAlignment = HorizontalAlignment.Left,
            Cover               = CreateCardCoverImage(),
            Content = new CardMetaContent
            {
                Header  = "Card title",
                Avatar  = CreateCardAvatar(),
                Content = "This is the description"
            }
        };
        AddCardActions(card);
        return card;
    }

    private static Card CreateLoadingCard(bool isLoading)
    {
        var card = new Card
        {
            MinWidth            = 300,
            IsLoading           = isLoading,
            HorizontalAlignment = HorizontalAlignment.Left,
            Content = new CardMetaContent
            {
                Header  = "Card title",
                Avatar  = CreateCardAvatar(),
                Content = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Spacing     = 3,
                    Children =
                    {
                        new AtomTextBlock { Text = "This is the description" },
                        new AtomTextBlock { Text = "This is the description" }
                    }
                }
            }
        };
        AddCardActions(card);
        return card;
    }

    private static Card CreateGridCard()
    {
        return new Card
        {
            Header              = "Card title",
            SizeType            = SizeType.Large,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Content             = CreateCardGridContent()
        };
    }

    private static Card CreateTabsCard()
    {
        return new Card
        {
            Header              = "Card title",
            Extra               = CreateCardMoreLink(),
            SizeType            = SizeType.Large,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Content             = CreateCardTabsContent(includeExtra: false)
        };
    }

    private static Control CreateCardGalleryShape()
    {
        var root = new StackPanel
        {
            Spacing = 20,
            Width   = 980
        };

        var basic = new WrapPanel
        {
            ItemSpacing = 20,
            LineSpacing = 20
        };
        basic.Children.Add(CreateHeaderExtraCardWithSize("Large size card", SizeType.Large));
        basic.Children.Add(CreateHeaderExtraCardWithSize("Default size card", SizeType.Middle));
        basic.Children.Add(CreateHeaderExtraCardWithSize("Small size card", SizeType.Small));
        root.Children.Add(basic);

        root.Children.Add(CreateHeaderExtraCard(CardStyleVariant.Borderless));
        root.Children.Add(CreateContentOnlyCard());
        root.Children.Add(CreateCoverMetaCard());

        var columnGrid = new Grid
        {
            ColumnDefinitions = CreateEqualColumns(3),
            ColumnSpacing     = 20
        };
        for (var i = 0; i < 3; i++)
        {
            var card = CreateHeaderExtraCard(CardStyleVariant.Borderless);
            card.HorizontalAlignment = HorizontalAlignment.Stretch;
            card.Extra               = null;
            Grid.SetColumn(card, i);
            columnGrid.Children.Add(card);
        }
        root.Children.Add(columnGrid);

        root.Children.Add(new AtomToggleSwitch());
        root.Children.Add(CreateLoadingCard(isLoading: false));
        root.Children.Add(CreateLoadingCard(isLoading: false));
        root.Children.Add(CreateGridCard());

        var innerCard = new Card
        {
            Header              = "Card title",
            SizeType            = SizeType.Large,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Content = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing     = 20,
                Children =
                {
                    CreateInnerCard(),
                    CreateInnerCard()
                }
            }
        };
        root.Children.Add(innerCard);

        root.Children.Add(new StackPanel
        {
            Spacing = 20,
            Children =
            {
                CreateTabsCard(),
                new Card
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Content             = CreateCardTabsContent(includeExtra: true)
                }
            }
        });

        root.Children.Add(CreateActionsCard());
        return root;
    }

    private static Card CreateHeaderExtraCardWithSize(string header, SizeType sizeType)
    {
        var card = CreateHeaderExtraCard();
        card.Header   = header;
        card.SizeType = sizeType;
        return card;
    }

    private static Card CreateInnerCard()
    {
        return new Card
        {
            Header              = "Card title",
            Extra               = CreateCardMoreLink(),
            IsInnerMode         = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Content             = CreateCardTextStack()
        };
    }

    private static StackPanel CreateCardTextStack()
    {
        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing     = 3,
            Children =
            {
                new AtomTextBlock { Text = "Card content" },
                new AtomTextBlock { Text = "Card content" },
                new AtomTextBlock { Text = "Card content" }
            }
        };
    }

    private static HyperLinkButton CreateCardMoreLink()
    {
        return new HyperLinkButton
        {
            Content = "More"
        };
    }

    private static Image CreateCardCoverImage()
    {
        return new Image
        {
            Width  = 300,
            Height = 180,
            Source = CardCoverBitmap.Value
        };
    }

    private static Avatar CreateCardAvatar()
    {
        return new Avatar
        {
            Icon = new UserOutlined()
        };
    }

    private static CardGridContent CreateCardGridContent()
    {
        var content = new CardGridContent
        {
            ColumnDefinitions = CreateEqualColumns(4),
            RowDefinitions    = CreateAutoRows(2)
        };

        var positions = new[]
        {
            (Row: 0, Column: 0, IsHoverable: true),
            (Row: 0, Column: 1, IsHoverable: false),
            (Row: 0, Column: 2, IsHoverable: true),
            (Row: 0, Column: 3, IsHoverable: true),
            (Row: 1, Column: 0, IsHoverable: true),
            (Row: 1, Column: 1, IsHoverable: true),
            (Row: 1, Column: 2, IsHoverable: true)
        };
        foreach (var position in positions)
        {
            content.Items.Add(new CardGridItem
            {
                Row         = position.Row,
                Column      = position.Column,
                IsHoverable = position.IsHoverable,
                Content     = "Content"
            });
        }

        return content;
    }

    private static CardTabsContent CreateCardTabsContent(bool includeExtra)
    {
        var content = new CardTabsContent();
        if (includeExtra)
        {
            content.TabBarExtraContent = CreateCardMoreLink();
            content.Items.Add(new AtomTabItem { Header = "article", Content = "article content" });
            content.Items.Add(new AtomTabItem { Header = "app", Content = "app content" });
            content.Items.Add(new AtomTabItem { Header = "project", Content = "project content" });
        }
        else
        {
            content.Items.Add(new AtomTabItem { Header = "Tab1", Content = "content1" });
            content.Items.Add(new AtomTabItem { Header = "Tab2", Content = "content2" });
        }

        return content;
    }

    private static void AddCardActions(Card card)
    {
        card.Actions.Add(new CardActionButton { Icon = new EditOutlined() });
        card.Actions.Add(new CardActionButton { Icon = new SettingOutlined() });
        card.Actions.Add(new CardActionButton { Icon = new EllipsisOutlined() });
    }

    private static ColumnDefinitions CreateEqualColumns(int count)
    {
        var columns = new ColumnDefinitions();
        for (var i = 0; i < count; i++)
        {
            columns.Add(new ColumnDefinition(GridLength.Star));
        }
        return columns;
    }

    private static RowDefinitions CreateAutoRows(int count)
    {
        var rows = new RowDefinitions();
        for (var i = 0; i < count; i++)
        {
            rows.Add(new RowDefinition(GridLength.Auto));
        }
        return rows;
    }

    private static Bitmap CreateCardCoverBitmap()
    {
        var bytes = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAFgwJ/lw7v9wAAAABJRU5ErkJggg==");
        using var stream = new MemoryStream(bytes);
        return new Bitmap(stream);
    }
}
