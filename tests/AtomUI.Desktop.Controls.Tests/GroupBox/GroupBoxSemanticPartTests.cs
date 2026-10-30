using System.Reflection;
using System.Xml.Linq;
using AtomUI.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIGroupBox = AtomUI.Desktop.Controls.GroupBox;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.GroupBox;

/// <summary>
/// GroupBox 的四个语义部件全部是 <c>GroupBoxTheme.axaml</c> 单模板内的静态节点
/// （<c>RuntimeCreated=false</c>，无 <c>.semantic-scope-*</c> 锚点，无 C# marker 注入）。
/// 这些测试锁定 descriptor 契约、模板 marker 契约、生成 Style 的精确命中与优先级，
/// 以及 <c>header</c> 承载节点提升为 <c>Border</c> 后缺口几何、Token 内边距与对齐仍然成立。
/// </summary>
public class GroupBoxSemanticPartTests
{
    private const string HeaderClass  = "semantic-header";
    private const string IconClass    = "semantic-icon";
    private const string TitleClass   = "semantic-title";
    private const string ContentClass = "semantic-content";
    private const string OwnerClass   = "semantic-owner";

    static GroupBoxSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_Only_The_Approved_Parts()
    {
        var descriptor = GetDescriptor();

        // Parts 按 root 优先、其余按 Path ordinal 排序，因此不是声明顺序。
        descriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(["root", "content", "header", "icon", "title"]);

        AssertRoot(descriptor.Parts.Single(static part => part.Name == "root"));
        AssertPart(
            descriptor.Parts.Single(static part => part.Name == "header"),
            HeaderClass,
            typeof(Border),
            "GroupBoxHeaderStyle");
        AssertPart(
            descriptor.Parts.Single(static part => part.Name == "icon"),
            IconClass,
            typeof(IconPresenter),
            "GroupBoxIconStyle");
        // title 的 ContractType 取 Avalonia.Controls.TextBlock 基类而不是节点派生类型，
        // 以便后续把节点换成普通 TextBlock 不构成破坏性变更。
        AssertPart(
            descriptor.Parts.Single(static part => part.Name == "title"),
            TitleClass,
            typeof(Avalonia.Controls.TextBlock),
            "GroupBoxTitleStyle");
        AssertPart(
            descriptor.Parts.Single(static part => part.Name == "content"),
            ContentClass,
            typeof(ContentPresenter),
            "GroupBoxContentStyle");
    }

    [Fact]
    public void Templates_Implement_Only_The_Approved_Static_Markers()
    {
        var document = XDocument.Load(
            GetRepoFile("src/AtomUI.Desktop.Controls/GroupBox/Themes/GroupBoxTheme.axaml"),
            LoadOptions.SetLineInfo);

        var literalSemanticMarkers = document.Descendants()
                                             .Attributes()
                                             .Where(static attribute =>
                                                 attribute.Name.LocalName == "Classes" &&
                                                 attribute.Value.Split(
                                                             (char[]?)null,
                                                             StringSplitOptions.RemoveEmptyEntries)
                                                         .Any(static value => value.StartsWith(
                                                             "semantic-",
                                                             StringComparison.Ordinal)))
                                             .ToArray();
        var classPropertyMarkers = document.Descendants()
                                           .SelectMany(static element => element.Attributes()
                                               .Where(static attribute => attribute.Name.LocalName.StartsWith(
                                                   "Classes.semantic-",
                                                   StringComparison.Ordinal))
                                               .Select(attribute => (Element: element, Attribute: attribute)))
                                           .ToArray();

        // 字面量 Classes="semantic-*" 不是合法静态 marker；必须使用 Classes.semantic-*="True"。
        literalSemanticMarkers.ShouldBeEmpty();
        classPropertyMarkers.ShouldAllBe(static marker =>
            string.Equals(marker.Attribute.Value, "true", StringComparison.OrdinalIgnoreCase));
        classPropertyMarkers.Select(static marker =>
                                $"{marker.Attribute.Name.LocalName["Classes.".Length..]}:{marker.Element.Name.LocalName}")
                            .OrderBy(static value => value, StringComparer.Ordinal)
                            .ShouldBe([
                                "semantic-content:ContentPresenter",
                                "semantic-header:Border",
                                "semantic-icon:IconPresenter",
                                "semantic-title:TextBlock"
                            ]);

        // root 由生成器隐式产生，模板不得声明 .semantic-root。
        classPropertyMarkers.ShouldNotContain(static marker =>
            marker.Attribute.Name.LocalName == "Classes.semantic-root");

        // GroupBox 没有容器生命周期与跨视觉根部件，因此模板里不应出现任何 scope 锚点。
        classPropertyMarkers.ShouldNotContain(static marker =>
            marker.Attribute.Name.LocalName.StartsWith("Classes.semantic-scope-", StringComparison.Ordinal));

        // 只有一个 ControlTemplate，没有 Browser 或派生变体，因此不存在 marker 覆盖缺口。
        document.Descendants()
                .Count(static element => element.Name.LocalName == "ControlTemplate")
                .ShouldBe(1);
    }

    [Fact]
    public void Owner_Scoped_Markers_Stay_Exactly_One_Per_Declared_Part()
    {
        var groupBox = CreateGroupBox();

        ShowInWindow(groupBox, () =>
        {
            GetOwnerMarkers(groupBox)
                .SelectMany(static control => control.Classes)
                .Where(static className => className.StartsWith("semantic-", StringComparison.Ordinal))
                .OrderBy(static className => className, StringComparer.Ordinal)
                .ShouldBe(["semantic-content", "semantic-header", "semantic-icon", "semantic-title"]);

            FindOwnerMarker<Border>(groupBox, HeaderClass);
            FindOwnerMarker<IconPresenter>(groupBox, IconClass);
            FindOwnerMarker<AtomUI.Desktop.Controls.TextBlock>(groupBox, TitleClass);
            FindOwnerMarker<ContentPresenter>(groupBox, ContentClass);

            // 四个部件都在 owner 的模板作用域内：单一 /template/ 路由成立，不需要显式 SelectorRoute。
            foreach (var marker in new[] { HeaderClass, IconClass, TitleClass, ContentClass })
            {
                ReferenceEquals(FindOwnerMarker(groupBox, marker).TemplatedParent, groupBox)
                    .ShouldBeTrue();
            }

            AssertMarkerCounts(groupBox, 1, 1, 1, 1);
        });
    }

    [Fact]
    public void Generated_Semantic_Styles_Hit_Exactly_One_Node_Per_Part()
    {
        var groupBox = CreateGroupBox();
        groupBox.Classes.Add(OwnerClass);

        var descriptor = GetDescriptor();
        var ownerStyle = new Style(selector => selector!.OfType<AtomUIGroupBox>().Class(OwnerClass));
        foreach (var part in descriptor.Parts.Where(static part => part.StyleType != null))
        {
            var partStyle = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull())
                                        .ShouldNotBeNull();
            partStyle.Setters.Add(new Setter(Control.TagProperty, part.Name));
            ownerStyle.Children.Add(partStyle);
        }

        groupBox.Styles.Add(ownerStyle);

        ShowInWindow(groupBox, () =>
        {
            var tagged = groupBox.GetVisualDescendants()
                                 .OfType<Control>()
                                 .Where(static control => control.Tag is string)
                                 .ToArray();

            foreach (var name in new[] { "content", "header", "icon", "title" })
            {
                tagged.Count(control => Equals(control.Tag, name)).ShouldBe(
                    1,
                    $"GroupBox part '{name}' must hit exactly one node; hits: " +
                    string.Join(
                        " | ",
                        tagged.Where(control => Equals(control.Tag, name))
                              .Select(static control => $"{control.GetType().Name}#{control.Name}")));
            }
        });
    }

    [Fact]
    public void Semantic_Selectors_Match_Their_Declared_Owner_Routes()
    {
        var groupBox = CreateGroupBox();
        groupBox.Classes.Add(OwnerClass);
        groupBox.Styles.Add(CreateRouteStyle(HeaderClass, "header"));
        groupBox.Styles.Add(CreateRouteStyle(IconClass, "icon"));
        groupBox.Styles.Add(CreateRouteStyle(TitleClass, "title"));
        groupBox.Styles.Add(CreateRouteStyle(ContentClass, "content"));

        ShowInWindow(groupBox, () =>
        {
            FindOwnerMarker(groupBox, HeaderClass).Tag.ShouldBe("header");
            FindOwnerMarker(groupBox, IconClass).Tag.ShouldBe("icon");
            FindOwnerMarker(groupBox, TitleClass).Tag.ShouldBe("title");
            FindOwnerMarker(groupBox, ContentClass).Tag.ShouldBe("content");
        });
    }

    /// <summary>
    /// 生成的 Semantic Style 通过 class selector 生效，Setter 以 <c>StyleTrigger</c> 优先级应用，
    /// 高于主题普通 Style Setter 与 <c>TemplateBinding</c>。因此 Part Setter 可以有意覆盖
    /// <c>HeaderContentPadding</c> Token 基线与 <c>HeaderTitleColor</c> / <c>HeaderFontSize</c> 的投影值。
    /// </summary>
    [Fact]
    public void Header_Title_And_Content_Setters_Override_Token_And_Property_Defaults()
    {
        var headerBackground = new SolidColorBrush(Color.FromRgb(0x8A, 0x2B, 0xE2));
        var groupBox = CreateGroupBox();
        groupBox.Classes.Add(OwnerClass);
        groupBox.HeaderTitleColor = Brushes.Black;

        groupBox.Styles.Add(new Style(selector => selector!.OfType<AtomUIGroupBox>().Class(OwnerClass))
        {
            Children =
            {
                new Style(selector => selector!.Nesting().Template().Class(HeaderClass))
                {
                    Setters =
                    {
                        new Setter(Decorator.PaddingProperty, new Thickness(21, 2)),
                        new Setter(Border.BackgroundProperty, headerBackground)
                    }
                },
                new Style(selector => selector!.Nesting().Template().Class(TitleClass))
                {
                    Setters =
                    {
                        new Setter(Avalonia.Controls.TextBlock.ForegroundProperty, Brushes.Magenta),
                        new Setter(Avalonia.Controls.TextBlock.FontSizeProperty, 29d)
                    }
                },
                new Style(selector => selector!.Nesting().Template().Class(ContentClass))
                {
                    Setters = { new Setter(ContentPresenter.PaddingProperty, new Thickness(17)) }
                }
            }
        });

        ShowInWindow(groupBox, () =>
        {
            var header = FindOwnerMarker<Border>(groupBox, HeaderClass);
            // 主题的 HeaderContentPadding 横向非零；Semantic Setter 必须整体覆盖它。
            header.Padding.ShouldBe(new Thickness(21, 2));
            header.Background.ShouldBeSameAs(headerBackground);

            var title = FindOwnerMarker<AtomUI.Desktop.Controls.TextBlock>(groupBox, TitleClass);
            title.Foreground.ShouldBe(Brushes.Magenta);
            title.FontSize.ShouldBe(29d);

            var content = FindOwnerMarker<ContentPresenter>(groupBox, ContentClass);
            content.Padding.ShouldBe(new Thickness(17));
        });
    }

    /// <summary>
    /// <c>PART_HeaderContent</c> 由 <c>Decorator</c> 提升为 <c>Border</c> 以获得 <c>Background</c> 能力。
    /// 该提升必须保持 <c>Find&lt;Decorator&gt;</c> 查找路径与基于 <c>Bounds</c> 的缺口几何不变；
    /// 主题中两个以节点类型开头的 selector 也必须同步，否则 Token 内边距与标题对齐会静默失效。
    /// </summary>
    [Theory]
    [InlineData(GroupBoxTitlePosition.Left, HorizontalAlignment.Left)]
    [InlineData(GroupBoxTitlePosition.Center, HorizontalAlignment.Center)]
    [InlineData(GroupBoxTitlePosition.Right, HorizontalAlignment.Right)]
    public void Header_Node_Is_A_Background_Capable_Border_That_Keeps_Token_Padding_And_Alignment(
        GroupBoxTitlePosition position,
        HorizontalAlignment expectedAlignment)
    {
        var groupBox = CreateGroupBox();
        groupBox.HeaderTitlePosition = position;

        ShowInWindow(groupBox, () =>
        {
            var header = FindOwnerMarker<Border>(groupBox, HeaderClass);

            // Border 继承 Decorator，控件既有的 NameScope.Find<Decorator> 查找路径保持有效。
            GetCachedHeaderContainer(groupBox).ShouldBeSameAs(header);

            // HeaderContentPadding 来自 GroupBoxToken（横向非零）；若主题 selector 未同步到
            // Border#PART_HeaderContent，这里会退回 Decorator 默认的 0 并失败。
            header.Padding.Left.ShouldBeGreaterThan(0d);
            header.Padding.Right.ShouldBeGreaterThan(0d);
            header.HorizontalAlignment.ShouldBe(expectedAlignment);
        });
    }

    /// <summary>
    /// 缺口是几何排除而不是背景遮挡：<c>GroupBox.Render</c> 把 Header bounds 从边框几何中排除。
    /// 该关系必须与 Header 自身的 <c>Background</c> 无关——不透明背景不得被用作遮挡层。
    /// </summary>
    [Fact]
    public void Header_Background_Is_Not_Used_As_A_Gap_Mask_And_The_Border_Drawing_Excludes_The_Gap()
    {
        var headerBackground = new SolidColorBrush(Color.FromRgb(0x8A, 0x2B, 0xE2));
        var groupBox = CreateGroupBox();
        groupBox.Background   = Brushes.White;
        groupBox.BorderBrush  = Brushes.Black;

        ShowInWindow(groupBox, () =>
        {
            var header = FindOwnerMarker<Border>(groupBox, HeaderClass);
            header.Background = headerBackground;
            Dispatcher.UIThread.RunJobs();

            var excluding = GetCachedBorderGeometry(groupBox)
                .ShouldNotBeNull()
                .ShouldBeAssignableTo<CombinedGeometry>()
                .ShouldNotBeNull();
            excluding.GeometryCombineMode.ShouldBe(GeometryCombineMode.Exclude);

            // 缺口矩形按 Header 实际 bounds 从边框几何中排除，与 Header 背景无关。
            var gap = excluding.Geometry2.ShouldBeAssignableTo<RectangleGeometry>().ShouldNotBeNull();
            var headerOffset = header.TranslatePoint(default, groupBox);
            headerOffset.ShouldNotBeNull();
            var offset = headerOffset!.Value;

            gap.Rect.X.ShouldBe(offset.X, 0.001);
            gap.Rect.Y.ShouldBe(offset.Y, 0.001);
            gap.Rect.Width.ShouldBe(header.Bounds.Width, 0.001);
            gap.Rect.Height.ShouldBe(header.Bounds.Height, 0.001);

            // GroupBox 只自绘 Background 与 BorderBrush 两个几何；Header 背景由模板子节点绘制，
            // 不得作为遮挡几何出现在 owner 的 Render 输出中。
            var drawingGroup = RenderToDrawingGroup(groupBox);
            EnumerateGeometryDrawings(drawingGroup)
                .Select(static drawing => drawing.Brush)
                .OfType<ISolidColorBrush>()
                .ShouldNotContain(brush => brush.Color == headerBackground.Color);
        });
    }

    [Fact]
    public void Hiding_The_Header_Icon_Keeps_The_Icon_Marker_On_The_Owner_Without_Reserving_Width()
    {
        var groupBox = CreateGroupBox();

        ShowInWindow(groupBox, () =>
        {
            var icon = FindOwnerMarker<IconPresenter>(groupBox, IconClass);
            icon.IsVisible.ShouldBeTrue();
            var headerWidthWithIcon = FindOwnerMarker<Border>(groupBox, HeaderClass).Bounds.Width;

            groupBox.HeaderIcon = null;
            Dispatcher.UIThread.RunJobs();

            icon.IsVisible.ShouldBeFalse();
            // 节点与 marker 仍存在（Single），只是不参与布局、不保留图标占位宽度。
            GetOwnerMarkers(groupBox, IconClass).Length.ShouldBe(1);
            AssertMarkerCounts(groupBox, 1, 1, 1, 1);
            FindOwnerMarker<Border>(groupBox, HeaderClass).Bounds.Width
                          .ShouldBeLessThan(headerWidthWithIcon);
        });
    }

    [Fact]
    public void Marker_Counts_Stay_Stable_Across_State_Switches_And_Template_Reapplication()
    {
        var groupBox = CreateGroupBox();
        var window = new AvaloniaWindow
        {
            Width   = 640,
            Height  = 480
        };

        try
        {
            window.Show();
            window.Content = groupBox;
            Dispatcher.UIThread.RunJobs();
            AssertMarkerCounts(groupBox, 1, 1, 1, 1);

            groupBox.HeaderTitlePosition = GroupBoxTitlePosition.Center;
            groupBox.HeaderTitle         = null;
            groupBox.HeaderIcon          = null;
            Dispatcher.UIThread.RunJobs();
            AssertMarkerCounts(groupBox, 1, 1, 1, 1);

            groupBox.HeaderTitlePosition = GroupBoxTitlePosition.Right;
            groupBox.HeaderFontStyle      = FontStyle.Italic;
            groupBox.HeaderFontWeight     = FontWeight.Bold;
            groupBox.HeaderTitleColor     = Brushes.Coral;
            groupBox.Background           = Brushes.Transparent;
            groupBox.BorderThickness      = new Thickness(3);
            groupBox.CornerRadius         = new CornerRadius(10);
            Dispatcher.UIThread.RunJobs();
            AssertMarkerCounts(groupBox, 1, 1, 1, 1);

            // 模板重应用：静态 marker 随模板重建，数量按重建时的状态恢复。
            window.Content = null;
            Dispatcher.UIThread.RunJobs();

            window.Content = groupBox;
            Dispatcher.UIThread.RunJobs();
            AssertMarkerCounts(groupBox, 1, 1, 1, 1);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Auto_Height_Contract_Is_Unaffected_By_The_Semantic_Markers()
    {
        var content = new Border { Height = 160 };
        var groupBox = CreateGroupBox();
        groupBox.Content = content;

        ShowInWindow(groupBox, () =>
        {
            var header = FindOwnerMarker<Border>(groupBox, HeaderClass);
            groupBox.Bounds.Height.ShouldBeGreaterThan(
                content.Bounds.Height + header.Bounds.Height / 2,
                "auto height must keep including the fieldset header lane after markers were added");
        });
    }

    /// <summary>
    /// 尺寸基线失败回归：GroupBox 没有 <c>SizeType</c>，高度由 <c>MeasureOverride</c> 测量
    /// <c>PART_Frame</c> 自然得出（Header 通道 + 内容内边距 + 内容 <c>DesiredSize</c>）。
    /// 因此给 <c>header</c> 或 <c>content</c> 设置固定 <c>Height</c> / <c>MinHeight</c> 会绕过自然测量，
    /// 使自动高度契约失效——这是固定布局类 Setter 不作为推荐定制路径的原因，
    /// 见 [semantic-part.md §5](../../../../docs/controls/desktop/data-display/group-box/semantic-part.md)。
    /// </summary>
    [Fact]
    public void Fixed_Height_On_The_Header_Part_Suppresses_The_Natural_Auto_Height_Baseline()
    {
        var baseline = CreateGroupBox();
        baseline.Content = new Border { Height = 160 };

        // 反例：把 header 压成固定高度后，Header 通道不再参与自然测量。
        var constrained = CreateGroupBox();
        constrained.Content = new Border { Height = 160 };
        constrained.Styles.Add(new Style(selector =>
            selector!.OfType<AtomUIGroupBox>().Template().Class(HeaderClass))
        {
            Setters = { new Setter(Layoutable.HeightProperty, 1d) }
        });

        var root = new StackPanel
        {
            Children = { baseline, constrained }
        };

        ShowInWindow(root, () =>
        {
            FindOwnerMarker<Border>(constrained, HeaderClass).Height.ShouldBe(1d);
            baseline.Bounds.Height.ShouldBeGreaterThan(160);
            constrained.Bounds.Height.ShouldBeLessThan(baseline.Bounds.Height);
        });
    }

    /// <summary>
    /// 可见边框与背景由 <c>GroupBox.Render</c> 自绘，模板中不存在承载它们的节点。
    /// 因此 <c>PART_Frame</c> 不是 Part，也不得成为分组边框的定制入口。
    /// </summary>
    [Fact]
    public void Frame_Stays_A_Layout_Root_Without_Background_Or_Border_Brush_Of_Its_Own()
    {
        var groupBox = CreateGroupBox();

        ShowInWindow(groupBox, () =>
        {
            var frame = groupBox.GetVisualDescendants()
                                .OfType<Border>()
                                .Single(static control => control.Name == "PART_Frame");

            frame.Background.ShouldBeNull();
            frame.BorderBrush.ShouldBeNull();
            frame.Classes.ShouldNotContain(HeaderClass);
            frame.Classes.ShouldNotContain(ContentClass);
        });
    }

    /// <summary>
    /// <c>root</c> 的定制契约：分组边框与背景由 owner 自绘，模板中没有承载节点，
    /// 因此只能通过 owner 的公开属性或 owner 作用域 Style 定制。该测试锁定
    /// <c>BorderBrush</c> / <c>BorderThickness</c> / <c>CornerRadius</c> / <c>Background</c>
    /// 确实能被 owner 作用域 Style 覆盖（ControlTheme 的默认 Setter 不得压制用户样式），
    /// 并且覆盖后的值真正进入自绘几何——只断言属性不足以证明“边框可以定制”。
    /// </summary>
    [Fact]
    public void Root_Border_And_Background_Are_Customizable_Through_Owner_Scoped_Style()
    {
        var borderBrush = new SolidColorBrush(Color.FromRgb(0x72, 0x2E, 0xD1));
        var background  = new SolidColorBrush(Color.FromRgb(0xF9, 0xF0, 0xFF));

        var groupBox = CreateGroupBox();
        groupBox.Classes.Add("semantic-border");
        groupBox.Styles.Add(new Style(selector => selector!.OfType<AtomUIGroupBox>().Class("semantic-border"))
        {
            Setters =
            {
                new Setter(TemplatedControl.BorderBrushProperty, borderBrush),
                new Setter(TemplatedControl.BorderThicknessProperty, new Thickness(3)),
                new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(10)),
                new Setter(TemplatedControl.BackgroundProperty, background)
            }
        });

        ShowInWindow(groupBox, () =>
        {
            groupBox.BorderBrush.ShouldBeSameAs(borderBrush);
            groupBox.BorderThickness.ShouldBe(new Thickness(3));
            groupBox.CornerRadius.ShouldBe(new CornerRadius(10));
            groupBox.Background.ShouldBeSameAs(background);

            // 自绘几何必须使用覆盖后的值与画笔，而不只是属性表面被改。
            Dispatcher.UIThread.RunJobs();
            GetCachedBorderThickness(groupBox).ShouldBe(new Thickness(3));
            GetPrivateField(groupBox, "_cachedCornerRadius").ShouldBe(new CornerRadius(10));

            // GroupBox.Render 把背景与边框都作为「填充几何」绘制（DrawGeometry 的 pen 为 null）：
            // 背景用 Background 填充，边框用 BorderBrush 填充 outer-minus-inner 几何。
            var brushedColors = EnumerateGeometryDrawings(RenderToDrawingGroup(groupBox))
                                .Select(static drawing => drawing.Brush)
                                .OfType<ISolidColorBrush>()
                                .Select(static brush => brush.Color)
                                .ToArray();
            brushedColors.ShouldContain(background.Color);
            brushedColors.ShouldContain(borderBrush.Color);
        });
    }

    private static ControlSemanticDescriptor GetDescriptor()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUIGroupBox), out var descriptor).ShouldBeTrue();
        return descriptor.ShouldNotBeNull();
    }

    private static AtomUIGroupBox CreateGroupBox()
    {
        return new AtomUIGroupBox
        {
            HeaderTitle = "Title",
            HeaderIcon  = new PathIcon(),
            Content     = new Border { Height = 48 }
        };
    }

    private static Style CreateRouteStyle(string partClass, string tag)
    {
        return new Style(selector =>
            selector!.OfType<AtomUIGroupBox>().Class(OwnerClass).Template().Class(partClass))
        {
            Setters = { new Setter(Control.TagProperty, tag) }
        };
    }

    private static void AssertMarkerCounts(
        AtomUIGroupBox groupBox,
        int expectedHeaders,
        int expectedIcons,
        int expectedTitles,
        int expectedContents)
    {
        GetOwnerMarkers(groupBox, HeaderClass).Length.ShouldBe(expectedHeaders);
        GetOwnerMarkers(groupBox, IconClass).Length.ShouldBe(expectedIcons);
        GetOwnerMarkers(groupBox, TitleClass).Length.ShouldBe(expectedTitles);
        GetOwnerMarkers(groupBox, ContentClass).Length.ShouldBe(expectedContents);
    }

    /// <summary>
    /// 只统计 <c>TemplatedParent</c> 为 owner 自身的节点，确保 marker 不会来自嵌套模板边界。
    /// </summary>
    private static Control[] GetOwnerMarkers(TemplatedControl owner, string? marker = null)
    {
        return owner.GetVisualDescendants()
                    .OfType<Control>()
                    .Where(control => ReferenceEquals(control.TemplatedParent, owner))
                    .Where(static control => control.Classes.Any(
                        className => className.StartsWith("semantic-", StringComparison.Ordinal)))
                    .Where(control => marker is null || control.Classes.Contains(marker))
                    .ToArray();
    }

    private static Control FindOwnerMarker(TemplatedControl owner, string marker)
    {
        return GetOwnerMarkers(owner, marker).Single();
    }

    private static T FindOwnerMarker<T>(TemplatedControl owner, string marker)
        where T : Control
    {
        var control = FindOwnerMarker(owner, marker);
        control.ShouldBeOfType<T>();
        return (T)control;
    }

    private static Control? GetCachedHeaderContainer(AtomUIGroupBox groupBox)
    {
        return (Control?)GetPrivateField(groupBox, "_headerContentContainer");
    }

    private static Geometry? GetCachedBorderGeometry(AtomUIGroupBox groupBox)
    {
        return (Geometry?)GetPrivateField(groupBox, "_borderGeometryCache");
    }

    private static Thickness GetCachedBorderThickness(AtomUIGroupBox groupBox)
    {
        return (Thickness)GetPrivateField(groupBox, "_cachedBorderThickness")!;
    }

    private static object? GetPrivateField(AtomUIGroupBox groupBox, string fieldName)
    {
        var field = typeof(AtomUIGroupBox).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        field.ShouldNotBeNull($"GroupBox must keep its '{fieldName}' implementation field");
        return field!.GetValue(groupBox);
    }

    private static DrawingGroup RenderToDrawingGroup(AtomUIGroupBox groupBox)
    {
        var drawingGroup = new DrawingGroup();
        using var context = drawingGroup.Open();
        groupBox.Render(context);
        return drawingGroup;
    }

    private static IEnumerable<GeometryDrawing> EnumerateGeometryDrawings(Drawing drawing)
    {
        if (drawing is GeometryDrawing geometryDrawing)
        {
            yield return geometryDrawing;
        }
        else if (drawing is DrawingGroup drawingGroup)
        {
            foreach (var child in drawingGroup.Children.SelectMany(EnumerateGeometryDrawings))
            {
                yield return child;
            }
        }
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 640,
            Height  = 480,
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
        }
    }

    private static void AssertRoot(SemanticPartDescriptor part)
    {
        part.Path.ShouldBe("root");
        part.SelectorClass.ShouldBeNull();
        part.SelectorRoute.ShouldBeNull();
        part.ContractType.ShouldBe(typeof(AtomUIGroupBox));
        part.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        part.Customization.ShouldBe(SemanticPartCustomization.Root);
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBeFalse();
        part.StyleType.ShouldBeNull();
    }

    private static void AssertPart(
        SemanticPartDescriptor part,
        string selectorClass,
        Type contractType,
        string expectedStyleTypeName)
    {
        part.Path.ShouldBe(part.Name);
        part.SelectorClass.ShouldBe(selectorClass);
        // 静态根模板 Part 未声明 SelectorRoute，由生成器规范化为 "/template/ .<class>"。
        part.SelectorRoute.ShouldBe($"/template/ .{selectorClass}");
        part.ContractType.ShouldBe(contractType);
        part.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        part.Customization.ShouldBe(SemanticPartCustomization.Selector);
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBeFalse();
        part.CrossNestedOwners.ShouldBeFalse();
        part.Since.ShouldBe("6.2.0");
        part.Theme.ShouldBeNull();
        part.StyleType.ShouldNotBeNull();
        part.StyleType!.Name.ShouldBe(expectedStyleTypeName);
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

        throw new FileNotFoundException($"Could not locate repository file '{relativePath}'.");
    }
}
