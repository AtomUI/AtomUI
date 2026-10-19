using System.Windows.Input;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUIGallery.ShowCases.Masonry;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUIGallery.Tests.ShowCases;

public class MasonryShowCasePageTests
{
    static MasonryShowCasePageTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Masonry_ShowCase_Uses_AtomUI_AsyncImage_Without_Runtime_Binding()
    {
        var source = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/Layout/Masonry/Views/MasonryShowCase.axaml");
        var codeBehind = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/Layout/Masonry/Views/MasonryShowCase.axaml.cs");

        source.ShouldContain("<atom:AsyncImage");
        source.ShouldContain("<atom:AsyncImage.LoadingContent>");
        source.ShouldNotContain("asyncImageLoader:");
        codeBehind.ShouldNotContain("BindUtils.RelayBind");
        codeBehind.ShouldNotContain("new Binding");
    }

    [Fact]
    public void Masonry_ShowCase_Declares_A_Deferred_Semantic_Part_Preview()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Layout/Masonry/Views/MasonryShowCase.axaml");

        source.ShouldContain("<gallery:GalleryShowCaseHost");
        source.ShouldContain("<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>");
        source.ShouldContain("<gallery:SemanticPartPreview");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:Masonry}\"");
        source.ShouldContain("Name=\"MasonrySemanticOwner\"");
        CountOccurrences(source, "<gallery:SemanticPartDescription").ShouldBe(2);
        source.ShouldContain("Path=\"root\"");
        source.ShouldContain("Path=\"item\"");
        source.ShouldNotContain("GalleryStickyTabsHost");
    }

    [Fact]
    public void Masonry_Semantic_Preview_Is_Materialized_Only_After_The_Tab_Is_Selected()
    {
        var page = new MasonryShowCase
        {
            DataContext = new MasonryViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldBeEmpty();
            page.GetVisualDescendants()
                .OfType<AtomUI.Desktop.Controls.Masonry>()
                .ShouldNotContain(static masonry => masonry.Name == "MasonrySemanticOwner");

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            page.GetVisualDescendants().OfType<SemanticPartPreview>().Count().ShouldBe(1);
            var semanticMasonry = page.GetVisualDescendants()
                                      .OfType<AtomUI.Desktop.Controls.Masonry>()
                                      .Single(static masonry => masonry.Name == "MasonrySemanticOwner");
            page.GetVisualDescendants()
                .OfType<AtomUI.Desktop.Controls.Masonry>()
                .Count(static masonry => masonry.Name == "MasonrySemanticOwner")
                .ShouldBe(1);
            semanticMasonry.GetVisualDescendants()
                           .OfType<Control>()
                           .Count(static control => control.Classes.Contains("semantic-item"))
                           .ShouldBe(7);
        });
    }

    [Fact]
    public void Masonry_Semantic_Part_Example_Is_The_Last_ShowCaseItem()
    {
        var page = new MasonryShowCase
        {
            DataContext = new MasonryViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            var panel = page.GetVisualDescendants().OfType<ShowCasePanel>().Single();
            panel.Children.OfType<ShowCaseItem>().Last().SourceKey.ShouldBe("masonry-semantic-part");
        });
    }

    [Fact]
    public void Masonry_Semantic_Part_Example_Matches_Contract()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Layout/Masonry/Views/MasonryShowCase.axaml");
        var localization = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Layout/Masonry/Localization/en-US.xlf");

        var item = ExtractShowCaseItemByTitle(
            source,
            "MasonryShowCaseLangResource SemanticPartStyleTitle");

        item.ShouldContain("SourceKey=\"masonry-semantic-part\"");
        item.ShouldContain("Span=\"Full\"");
        item.ShouldContain("IsDeferredContentEnabled=\"True\"");
        item.ShouldContain("SemanticPartStyleDescription");
        item.ShouldContain("SemanticObjectStylesTitle");
        item.ShouldContain("SemanticFunctionStylesTitle");
        item.ShouldContain("Name=\"MasonrySemanticObject\"");
        item.ShouldContain("Name=\"MasonrySemanticFunction\"");
        item.ShouldContain("Selector=\"atom|Masonry.semantic-style-demo.semantic-object-styles\"");
        item.ShouldContain("Selector=\"atom|Masonry.semantic-style-demo.semantic-function-styles[ColumnCount=3]\"");
        CountOccurrences(item, "<atom:MasonryItemStyle x:SetterTargetType=\"Border\">").ShouldBe(2);
        item.ShouldContain("<Setter Property=\"Background\" Value=\"#FAFAFA\" />");
        item.ShouldContain("<Setter Property=\"Background\" Value=\"#F0F8FF\" />");
        item.ShouldContain("<Setter Property=\"BorderBrush\" Value=\"#D9D9D9\" />");
        item.ShouldContain("<Setter Property=\"BorderBrush\" Value=\"#1890FF\" />");
        item.ShouldContain("<Setter Property=\"Padding\" Value=\"16\" />");
        item.ShouldContain("<Setter Property=\"Padding\" Value=\"16,12\" />");
        item.ShouldContain("ColumnCount=\"4\"");
        item.ShouldContain("ColumnCount=\"3\"");
        item.ShouldNotContain("Selector=\".semantic-item\"");
        item.ShouldNotContain("> .semantic-item");
        localization.ShouldContain("<source>Custom Semantic Part styling</source>");
        localization.ShouldContain(
            "<source>Use owner-scoped styles and the MasonryItemStyle semantic part style to customize Masonry's published Semantic Parts.</source>");
        localization.ShouldContain("<source>Style matched by class</source>");
        localization.ShouldContain("<source>Style matched by property state</source>");
        localization.ShouldContain(
            "<source>Root element, provides the root chrome, column and gutter layout inputs, and the ItemsControl entry.</source>");
        localization.ShouldContain(
            "<source>Item container measured, column-assigned and arranged by the masonry layout, carrying the Masonry.Column and Masonry.Span metadata.</source>");
        localization.ShouldNotContain("classNames", Case.Insensitive);
        localization.ShouldNotContain("semantic dom", Case.Insensitive);
        localization.ShouldNotContain("absolute positioning", Case.Insensitive);
        localization.ShouldNotContain("flex layout", Case.Insensitive);
    }

    [Fact]
    public void Masonry_Semantic_Part_Example_Applies_Styles_To_Item_Containers()
    {
        var page = new MasonryShowCase
        {
            DataContext = new MasonryViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 1000, () =>
        {
            var panel = page.GetVisualDescendants().OfType<ShowCasePanel>().Single();
            var item = panel.Children
                            .OfType<ShowCaseItem>()
                            .Single(static candidate => candidate.SourceKey == "masonry-semantic-part");
            item.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            var objectMasonry = page.GetVisualDescendants()
                                    .OfType<AtomUI.Desktop.Controls.Masonry>()
                                    .Single(static candidate => candidate.Name == "MasonrySemanticObject");
            objectMasonry.ColumnCount.ShouldBe(4);
            objectMasonry.ColumnGap.ShouldBe(16);
            objectMasonry.RowGap.ShouldBe(16);
            objectMasonry.Height.ShouldBe(260);

            var objectContainers = objectMasonry.GetVisualDescendants()
                                                .OfType<Border>()
                                                .Where(static border => border.Classes.Contains("semantic-item"))
                                                .ToArray();
            objectContainers.Length.ShouldBe(8);
            objectContainers.ShouldAllBe(static container => container.BorderThickness == new Avalonia.Thickness(1));
            objectContainers.ShouldAllBe(static container => container.CornerRadius == new Avalonia.CornerRadius(12));
            objectContainers.ShouldAllBe(static container => container.Padding == new Avalonia.Thickness(16, 12));

            var functionMasonry = page.GetVisualDescendants()
                                      .OfType<AtomUI.Desktop.Controls.Masonry>()
                                      .Single(static candidate => candidate.Name == "MasonrySemanticFunction");
            functionMasonry.ColumnCount.ShouldBe(3);
            functionMasonry.ColumnGap.ShouldBe(16);
            functionMasonry.RowGap.ShouldBe(16);
            functionMasonry.Height.ShouldBe(280);

            var functionContainers = functionMasonry.GetVisualDescendants()
                                                    .OfType<Border>()
                                                    .Where(static border => border.Classes.Contains("semantic-item"))
                                                    .ToArray();
            functionContainers.Length.ShouldBe(6);
            functionContainers.ShouldAllBe(static container => container.BorderThickness == new Avalonia.Thickness(1));
            functionContainers.ShouldAllBe(static container => container.CornerRadius == new Avalonia.CornerRadius(12));
            functionContainers.ShouldAllBe(static container => container.Padding == new Avalonia.Thickness(16, 12));
        });
    }

    [Fact]
    public void Masonry_ShowCase_Basic_Demo_Matches_Ant_Design_Structure()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Layout/Masonry/Views/MasonryShowCase.axaml");
        var basicDemoMarkup = ExtractBasicDemoMarkup(source);

        basicDemoMarkup.ShouldContain("ItemsSource=\"{Binding BasicItems}\"");
        basicDemoMarkup.ShouldContain("ColumnCount=\"4\"");
        basicDemoMarkup.ShouldContain("ColumnGap=\"16\"");
        basicDemoMarkup.ShouldContain("RowGap=\"16\"");
        basicDemoMarkup.ShouldContain("Height=\"{Binding Height}\"");
        basicDemoMarkup.ShouldContain("IsVisible=\"{Binding !IsSpecial}\"");
        basicDemoMarkup.ShouldContain("IsVisible=\"{Binding IsSpecial}\"");
        basicDemoMarkup.ShouldContain("<atom:CardMetaContent Header=\"{Binding Title}\"");
        basicDemoMarkup.ShouldContain("Content=\"{Binding Description}\"");
        basicDemoMarkup.ShouldContain("Name=\"SpecialCoverImage\"");
        basicDemoMarkup.ShouldContain("<atom:Skeleton IsLoading=\"True\"");
        basicDemoMarkup.ShouldContain("Padding=\"16,16\"");
        basicDemoMarkup.ShouldContain("IsActive=\"True\"");
        basicDemoMarkup.ShouldContain("IsShowAvatar=\"False\"");
        basicDemoMarkup.ShouldContain("IsShowTitle=\"True\"");
        basicDemoMarkup.ShouldContain("ParagraphRows=\"3\"");
        basicDemoMarkup.ShouldContain("IsRound=\"True\"");
        basicDemoMarkup.ShouldContain("Name=\"SpecialCoverSkeleton\"");
        basicDemoMarkup.ShouldContain("<atom:AsyncImage.LoadingContent>");
        basicDemoMarkup.ShouldNotContain("{Binding #");
        basicDemoMarkup.ShouldNotContain("IsVisible=\"{Binding #SpecialCoverImage.(asyncImageLoader:ImageLoader.IsLoading)}\"");
        basicDemoMarkup.ShouldNotContain("<atom:SkeletonLine");
        basicDemoMarkup.ShouldNotContain("<atom:SkeletonImage");
        basicDemoMarkup.ShouldNotContain("MaxWidth=\"Infinity\"");
        basicDemoMarkup.ShouldNotContain("HorizontalAlignment=\"Center\"");
        basicDemoMarkup.ShouldNotContain("VerticalAlignment=\"Center\"");
        basicDemoMarkup.ShouldNotContain("Margin=\"20\"");
        basicDemoMarkup.ShouldNotContain("Padding=\"8\"");
        basicDemoMarkup.ShouldNotContain("Padding=\"16,28\"");
    }

    [Fact]
    public void Masonry_ShowCase_Basic_Items_Mirror_Ant_Design_Basic_Demo()
    {
        var viewModel = new MasonryViewModel(null!);

        viewModel.BasicItems.ShouldNotBeNull();
        viewModel.BasicItems!.Select(item => item.Height).ShouldBe(new double[]
        {
            150, 50, 90, 70, 110, 150, 130, 80, 50, 90, 100, 150, 60, 50, 80
        });
        viewModel.BasicItems.Select(item => item.Index).ShouldBe(Enumerable.Range(1, 15));
        viewModel.BasicItems.Count(item => item.IsSpecial).ShouldBe(1);
        viewModel.BasicItems[4].IsSpecial.ShouldBeTrue();
        viewModel.BasicItems[4].CoverSource.ShouldNotBeNull();
        viewModel.BasicItems[4].CoverSource!.ToString().ShouldBe(
            ImageSource.Parse(
                "https://images.unsplash.com/photo-1491961865842-98f7befd1a60?w=523&auto=format").ToString());
        viewModel.BasicItems[4].Title.ShouldBe("I'm Special");
        viewModel.BasicItems[4].Description.ShouldBe("Let's have a meal");
    }

    [Fact]
    public void Masonry_ShowCase_Semantic_Items_Mirror_Ant_Design_Semantic_Demo()
    {
        var viewModel = new MasonryViewModel(null!);

        viewModel.SemanticItems.ShouldNotBeNull();
        viewModel.SemanticItems!.Select(item => item.Height).ShouldBe(new double[]
        {
            75, 50, 70, 60, 85, 75, 50
        });
        viewModel.SemanticItems.Select(item => item.Index).ShouldBe(Enumerable.Range(1, 7));
    }

    [Fact]
    public void Masonry_ShowCase_Responsive_Demo_Matches_Ant_Design_Responsive_Demo()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Layout/Masonry/Views/MasonryShowCase.axaml");
        var responsiveDemoMarkup = ExtractMasonryMarkup(source, "ItemsSource=\"{Binding ResponsiveItems}\"");

        responsiveDemoMarkup.ShouldContain("ColumnInfo=\"xs: 1, sm: 2, md: 3, lg: 4\"");
        responsiveDemoMarkup.ShouldContain("Gutter=\"xs: 8, sm: 12, md: 16; xs: 8, sm: 12, md: 16\"");
        responsiveDemoMarkup.ShouldContain("Height=\"{Binding Height}\"");
        responsiveDemoMarkup.ShouldNotContain("ColumnCount=\"4\"");
        responsiveDemoMarkup.ShouldNotContain("ColumnGap=\"16\"");
        responsiveDemoMarkup.ShouldNotContain("RowGap=\"16\"");
        responsiveDemoMarkup.ShouldNotContain("SpecialCoverImage");
        responsiveDemoMarkup.ShouldNotContain("<atom:Skeleton");
    }

    [Fact]
    public void Masonry_ShowCase_Responsive_Items_Mirror_Ant_Design_Responsive_Demo()
    {
        var viewModel = new MasonryViewModel(null!);

        viewModel.ResponsiveItems.ShouldNotBeNull();
        viewModel.ResponsiveItems!.Select(item => item.Height).ShouldBe(new double[]
        {
            120, 55, 85, 160, 95, 140, 75, 110, 65, 130, 90, 145, 55, 100, 80
        });
        viewModel.ResponsiveItems.Select(item => item.Index).ShouldBe(Enumerable.Range(1, 15));
        viewModel.ResponsiveItems.ShouldAllBe(item => !item.IsSpecial);
    }

    [Fact]
    public void Masonry_ShowCase_Image_Demo_Matches_Ant_Design_Image_Demo()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Layout/Masonry/Views/MasonryShowCase.axaml");
        var imageDemoMarkup = ExtractMasonryMarkup(source, "ItemsSource=\"{Binding ImageItems}\"");

        imageDemoMarkup.ShouldContain("ColumnCount=\"4\"");
        imageDemoMarkup.ShouldContain("ColumnGap=\"16\"");
        imageDemoMarkup.ShouldContain("RowGap=\"16\"");
        imageDemoMarkup.ShouldContain("Source=\"{Binding ImageSource}\"");
        imageDemoMarkup.ShouldNotContain("asyncImageLoader:ImageLoader.Source");
        imageDemoMarkup.ShouldContain("Name=\"MasonryImage\"");
        imageDemoMarkup.ShouldContain("Stretch=\"Uniform\"");
        imageDemoMarkup.ShouldContain("HorizontalAlignment=\"Stretch\"");
        imageDemoMarkup.ShouldContain("ClipToBounds=\"True\"");
        imageDemoMarkup.ShouldContain("MinHeight=\"210\"");
        imageDemoMarkup.ShouldContain("Name=\"MasonryImageSkeleton\"");
        imageDemoMarkup.ShouldContain("<atom:AsyncImage.LoadingContent>");
        imageDemoMarkup.ShouldNotContain("{Binding #");
        imageDemoMarkup.ShouldContain("<atom:Skeleton IsLoading=\"True\"");
        imageDemoMarkup.ShouldContain("Padding=\"16,16\"");
        imageDemoMarkup.ShouldContain("IsActive=\"True\"");
        imageDemoMarkup.ShouldContain("IsShowAvatar=\"False\"");
        imageDemoMarkup.ShouldContain("IsShowTitle=\"True\"");
        imageDemoMarkup.ShouldContain("ParagraphRows=\"3\"");
        imageDemoMarkup.ShouldContain("IsRound=\"True\"");
        imageDemoMarkup.ShouldNotContain("<atom:Card");
        imageDemoMarkup.ShouldNotContain("<atom:SkeletonImage");
        imageDemoMarkup.ShouldNotContain("<Panel MinHeight=");
        imageDemoMarkup.ShouldNotContain("Height=\"{Binding Height}\"");
    }

    [Fact]
    public void Masonry_ShowCase_Image_Items_Mirror_Ant_Design_Image_Demo()
    {
        var viewModel = new MasonryViewModel(null!);

        viewModel.ImageItems.ShouldNotBeNull();
        viewModel.ImageItems!.Select(item => item.Index).ShouldBe(Enumerable.Range(1, 16));
        viewModel.ImageItems.Select(item => item.ImageSource.ToString()).ShouldBe(new[]
        {
            "https://images.unsplash.com/photo-1510001618818-4b4e3d86bf0f?w=523&auto=format",
            "https://images.unsplash.com/photo-1507513319174-e556268bb244?w=523&auto=format",
            "https://images.unsplash.com/photo-1474181487882-5abf3f0ba6c2?w=523&auto=format",
            "https://images.unsplash.com/photo-1492778297155-7be4c83960c7?w=523&auto=format",
            "https://images.unsplash.com/photo-1508062878650-88b52897f298?w=523&auto=format",
            "https://images.unsplash.com/photo-1506158278516-d720e72406fc?w=523&auto=format",
            "https://images.unsplash.com/photo-1552203274-e3c7bd771d26?w=523&auto=format",
            "https://images.unsplash.com/photo-1528163186890-de9b86b54b51?w=523&auto=format",
            "https://images.unsplash.com/photo-1727423304224-6d2fd99b864c?w=523&auto=format",
            "https://images.unsplash.com/photo-1675090391405-432434e23595?w=523&auto=format",
            "https://images.unsplash.com/photo-1554196967-97a8602084d9?w=523&auto=format",
            "https://images.unsplash.com/photo-1491961865842-98f7befd1a60?w=523&auto=format",
            "https://images.unsplash.com/photo-1721728613411-d56d2ddda959?w=523&auto=format",
            "https://images.unsplash.com/photo-1731901245099-20ac7f85dbaa?w=523&auto=format",
            "https://images.unsplash.com/photo-1617694455303-59af55af7e58?w=523&auto=format",
            "https://images.unsplash.com/photo-1709198165282-1dab551df890?w=523&auto=format"
        }.Select(source => ImageSource.Parse(source).ToString()));
    }

    [Fact]
    public void Masonry_ShowCase_Dynamic_Demo_Matches_Ant_Design_Dynamic_Demo()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Layout/Masonry/Views/MasonryShowCase.axaml");
        var dynamicDemoMarkup = ExtractMasonryMarkup(source, "ItemsSource=\"{Binding DynamicItems}\"");

        dynamicDemoMarkup.ShouldContain("ColumnCount=\"4\"");
        dynamicDemoMarkup.ShouldContain("ColumnGap=\"16\"");
        dynamicDemoMarkup.ShouldContain("RowGap=\"16\"");
        dynamicDemoMarkup.ShouldContain("ItemContainerTheme");
        dynamicDemoMarkup.ShouldContain("x:DataType=\"vm:MasonryDynamicItem\"");
        dynamicDemoMarkup.ShouldContain("Property=\"atom:Masonry.Column\"");
        dynamicDemoMarkup.ShouldContain("Value=\"{CompiledBinding Column}\"");
        dynamicDemoMarkup.ShouldNotContain("x:CompileBindings=\"False\"");
        dynamicDemoMarkup.ShouldNotContain("Value=\"{Binding Column}\"");
        dynamicDemoMarkup.ShouldContain("LayoutChanged=\"HandleDynamicMasonryLayoutChanged\"");
        dynamicDemoMarkup.ShouldContain("Height=\"{Binding Height}\"");
        dynamicDemoMarkup.ShouldContain("Text=\"{Binding DisplayText}\"");
        dynamicDemoMarkup.ShouldContain("Click=\"HandleRemoveDynamicMasonryItemClick\"");
        dynamicDemoMarkup.ShouldContain("Icon=\"{antdicons:AntDesignIconProvider Kind=CloseOutlined}\"");
        source.ShouldContain("Command=\"{Binding AddDynamicMasonryItemCommand}\"");
        source.ShouldContain("Content=\"{gallery:MasonryShowCaseLangResource DynamicAddItemLabel}\"");
    }

    [Fact]
    public void Masonry_ShowCase_Dynamic_Items_Mirror_Ant_Design_Dynamic_Demo()
    {
        var viewModel = new MasonryViewModel(null!);

        viewModel.DynamicItems.ShouldNotBeNull();
        viewModel.DynamicItems!.Select(item => item.Key).ShouldBe(Enumerable.Range(0, 15));
        viewModel.DynamicItems.Select(item => item.Height).ShouldBe(new[]
        {
            150, 50, 90, 70, 110, 150, 130, 80, 50, 90, 100, 150, 70, 50, 80
        });
        viewModel.DynamicItems.Select(item => item.Column).ShouldBe(new int?[]
        {
            0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2
        });
        viewModel.DynamicItems.Select(item => item.DisplayText).ShouldBe(Enumerable.Range(1, 15).Select(index => index.ToString()));

        ((ICommand)viewModel.RemoveDynamicMasonryItemCommand).Execute(4);
        viewModel.DynamicItems.Count.ShouldBe(14);
        viewModel.DynamicItems.Select(item => item.Key).ShouldNotContain(4);

        ((ICommand)viewModel.AddDynamicMasonryItemCommand).Execute(null);
        viewModel.DynamicItems.Count.ShouldBe(15);
        var addedItem = viewModel.DynamicItems[^1];
        addedItem.Key.ShouldBe(15);
        addedItem.Height.ShouldBeInRange(50, 149);
        addedItem.Column.ShouldBeNull();
        addedItem.DisplayText.ShouldBe("16");

        viewModel.UpdateDynamicMasonryColumns(new[]
        {
            new MasonryItemLayout(new Border(), 0, 3, false),
            new MasonryItemLayout(new Border(), 2, 1, false)
        });
        viewModel.DynamicItems[0].Column.ShouldBe(3);
        viewModel.DynamicItems[2].Column.ShouldBe(1);
    }

    private static string ExtractBasicDemoMarkup(string source)
    {
        return ExtractMasonryMarkup(source, "ItemsSource=\"{Binding BasicItems}\"");
    }

    private static string ExtractMasonryMarkup(string source, string containsMarker)
    {
        const string masonryStartMarker = "<atom:Masonry";
        const string masonryEndMarker   = "</atom:Masonry>";

        var marker = source.IndexOf(containsMarker, StringComparison.Ordinal);
        marker.ShouldBeGreaterThanOrEqualTo(0);

        var masonryStart = source.LastIndexOf(masonryStartMarker, marker, StringComparison.Ordinal);
        masonryStart.ShouldBeGreaterThanOrEqualTo(0);

        var masonryEnd = source.IndexOf(masonryEndMarker, masonryStart, StringComparison.Ordinal);
        masonryEnd.ShouldBeGreaterThan(masonryStart);

        return source[masonryStart..(masonryEnd + masonryEndMarker.Length)];
    }

    private static string ExtractShowCaseItemByTitle(string source, string titleMarker)
    {
        var titleIndex = source.IndexOf(titleMarker, StringComparison.Ordinal);
        titleIndex.ShouldBeGreaterThanOrEqualTo(0);

        const string itemStartMarker = "<gallery:ShowCaseItem";
        const string itemEndMarker   = "</gallery:ShowCaseItem>";
        var itemStart = source.LastIndexOf(itemStartMarker, titleIndex, StringComparison.Ordinal);
        itemStart.ShouldBeGreaterThanOrEqualTo(0);

        var itemEnd = source.IndexOf(itemEndMarker, itemStart, StringComparison.Ordinal);
        itemEnd.ShouldBeGreaterThan(itemStart);
        return source[itemStart..(itemEnd + itemEndMarker.Length)];
    }

    private static int CountOccurrences(string value, string marker)
    {
        var count = 0;
        var index = 0;
        while ((index = value.IndexOf(marker, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += marker.Length;
        }

        return count;
    }

    private static void ShowInWindow(Control content, int width, int height, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = width,
            Height  = height,
            Content = content
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static string ReadRepoFile(string relativePath)
    {
        var path = GetRepoFile(relativePath);
        File.Exists(path).ShouldBeTrue($"Expected repository file to exist: {relativePath}");
        return File.ReadAllText(path);
    }

    private static string GetRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return Path.Combine(AppContext.BaseDirectory, relativePath);
    }
}
