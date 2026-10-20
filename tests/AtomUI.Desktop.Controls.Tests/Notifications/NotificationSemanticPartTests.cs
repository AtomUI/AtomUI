using System.Xml.Linq;
using AtomUI.Controls;
using AtomUI.MotionScene;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUISelectableTextBlock = AtomUI.Desktop.Controls.SelectableTextBlock;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Notifications;

public class NotificationSemanticPartTests
{
    private const string CardThemePath =
        "src/AtomUI.Desktop.Controls/Notifications/Themes/NotificationCardTheme.axaml";

    private const string ManagerThemePath =
        "src/AtomUI.Desktop.Controls/Notifications/Themes/WindowNotificationManagerTheme.axaml";

    // manifest 顺序：隐式 root 在前，其余按 path 字典序。
    private static readonly string[] ApprovedCardPartNames =
        ["root", "actions", "close", "description", "icon", "progress", "section", "title", "wrapper"];

    private static readonly string[] ApprovedManagerPartNames = ["root", "listContent"];

    static NotificationSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptors_Expose_Only_The_Approved_Notification_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(NotificationCard), out var cardDescriptor).ShouldBeTrue();
        cardDescriptor.ShouldNotBeNull();
        cardDescriptor.Parts.Select(static part => part.Name).ShouldBe(ApprovedCardPartNames);
        AssertRoot(cardDescriptor, typeof(NotificationCard));
        AssertPart(cardDescriptor, "wrapper", "semantic-wrapper", typeof(DockPanel));
        AssertPart(cardDescriptor, "icon", "semantic-icon", typeof(IconPresenter));
        AssertPart(cardDescriptor, "section", "semantic-section", typeof(StackPanel));
        AssertPart(cardDescriptor, "title", "semantic-title", typeof(AtomUISelectableTextBlock));
        AssertPart(cardDescriptor, "description", "semantic-description", typeof(ContentPresenter));
        AssertPart(cardDescriptor, "actions", "semantic-actions", typeof(ContentPresenter));
        AssertPart(cardDescriptor, "close", "semantic-close", typeof(IconButton));
        AssertPart(cardDescriptor, "progress", "semantic-progress", typeof(Control), runtimeCreated: true);

        registry.TryGetControl(typeof(WindowNotificationManager), out var managerDescriptor).ShouldBeTrue();
        managerDescriptor.ShouldNotBeNull();
        managerDescriptor.Parts.Select(static part => part.Name).ShouldBe(ApprovedManagerPartNames);
        AssertRoot(managerDescriptor, typeof(WindowNotificationManager));
        AssertPart(managerDescriptor, "listContent", "semantic-list-content", typeof(ItemsControl));
    }

    [Fact]
    public void Built_In_Themes_Carry_The_Approved_Static_Markers()
    {
        var cardDocument = XDocument.Load(GetRepoFile(CardThemePath), LoadOptions.SetLineInfo);
        CollectMarkers(cardDocument).ShouldBe(
        [
            "semantic-actions:ContentPresenter",
            "semantic-close:IconButton",
            "semantic-description:ContentPresenter",
            "semantic-icon:IconPresenter",
            "semantic-section:StackPanel",
            "semantic-title:SelectableTextBlock",
            "semantic-wrapper:DockPanel"
        ]);
        // progress 由运行时创建，不进入静态模板 marker 集合。
        cardDocument.Descendants().Any(static element => HasMarker(element, "semantic-progress")).ShouldBeFalse();
        cardDocument.Descendants().Any(static element => HasMarker(element, "semantic-root")).ShouldBeFalse();

        var managerDocument = XDocument.Load(GetRepoFile(ManagerThemePath), LoadOptions.SetLineInfo);
        CollectMarkers(managerDocument).ShouldBe(["semantic-list-content:FeedbackStackPresenter"]);
        managerDocument.Descendants().Any(static element => HasMarker(element, "semantic-root")).ShouldBeFalse();
    }

    [Fact]
    public void Built_In_Themes_Do_Not_Consume_Semantic_Selectors()
    {
        foreach (var path in new[] { CardThemePath, ManagerThemePath })
        {
            var document = XDocument.Load(GetRepoFile(path), LoadOptions.SetLineInfo);
            document.Descendants()
                    .Where(static element => element.Name.LocalName == "Style")
                    .Select(static element => (string?)element.Attribute("Selector"))
                    .Where(static selector => selector?.Contains(".semantic-", StringComparison.Ordinal) == true)
                    .ShouldBeEmpty(path);
        }
    }

    [Fact]
    public void NotificationCard_Template_Exposes_The_Upstream_Notice_Structure()
    {
        var manager = CreateManager();
        var card = CreateCard(manager, NotificationType.Success, "Deployment completed", "Build #42 finished");
        var window = ShowInWindow(card);
        try
        {
            // 模板结构：wrapper > (icon, section > (title, description)) + actions + close。
            var wrapper   = FindSemanticControl<DockPanel>(card, "semantic-wrapper");
            var icon      = FindSemanticControl<IconPresenter>(card, "semantic-icon");
            var section   = FindSemanticControl<StackPanel>(card, "semantic-section");
            var title     = FindSemanticControl<AtomUISelectableTextBlock>(card, "semantic-title");
            var description = FindSemanticControl<ContentPresenter>(card, "semantic-description");
            var actions   = FindSemanticControl<ContentPresenter>(card, "semantic-actions");
            var close     = FindSemanticControl<IconButton>(card, "semantic-close");

            // wrapper 是 icon 与 section 的共同父级；close / actions 不在 wrapper 内。
            icon.GetVisualAncestors().ShouldContain(wrapper);
            section.GetVisualAncestors().ShouldContain(wrapper);
            ReferenceEquals(title.GetVisualParent(), section).ShouldBeTrue();
            ReferenceEquals(description.GetVisualParent(), section).ShouldBeTrue();
            close.GetVisualAncestors().ShouldNotContain(wrapper);
            ReferenceEquals(actions.GetVisualParent(), wrapper).ShouldBeFalse();

            // close 是覆盖层：垂直靠上，不参与 wrapper 的流式排列。
            close.VerticalAlignment.ShouldBe(Avalonia.Layout.VerticalAlignment.Top);

            // title 在关闭区保留内边距，避免文本压到关闭按钮；描述在标题下方，不预留。
            title.Padding.Right.ShouldBeGreaterThan(0);
            description.Padding.Right.ShouldBe(0);

            // 图标顶部与标题行顶部对齐，而不是被 DockPanel 的 Stretch 垂直居中（视觉上会低一截）。
            var iconTop  = icon.TranslatePoint(new Point(0, 0), wrapper)!.Value.Y;
            var titleTop = title.TranslatePoint(new Point(0, 0), wrapper)!.Value.Y;
            Math.Abs(iconTop - titleTop).ShouldBeLessThan(2);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Actions_Part_Reacts_To_The_Actions_Content()
    {
        var manager = CreateManager();
        var card = CreateCard(manager, NotificationType.Information, "Title", "Body");
        var window = ShowInWindow(card);
        try
        {
            var actions = FindSemanticControl<ContentPresenter>(card, "semantic-actions");
            actions.IsVisible.ShouldBeFalse();

            card.Actions = new Button { Content = "Details" };
            Dispatcher.UIThread.RunJobs();

            actions.IsVisible.ShouldBeTrue();
            actions.Content.ShouldBeOfType<Button>();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Root_Surface_Properties_Project_Onto_The_Frame_And_Keep_Token_Defaults()
    {
        // root 的定制契约：卡片表面必须能从 owner 属性投影到表面 Border。
        var manager = CreateManager();
        var card = CreateCard(manager, NotificationType.Success, "Styled", "Body");
        card.Background      = Brushes.Red;
        card.BorderBrush     = Brushes.Lime;
        card.BorderThickness = new Thickness(2);
        card.CornerRadius    = new CornerRadius(16);
        card.Padding         = new Thickness(20);
        card.BoxShadow       = BoxShadows.Parse("4 4 0 #D9F7BE");

        var window = ShowInWindow(card);
        try
        {
            var frame = FindFrame(card);
            frame.Background.ShouldBe(Brushes.Red);
            frame.BorderBrush.ShouldBe(Brushes.Lime);
            frame.BorderThickness.ShouldBe(new Thickness(2));
            frame.CornerRadius.ShouldBe(new CornerRadius(16));
            frame.BoxShadow.ShouldBe(BoxShadows.Parse("4 4 0 #D9F7BE"));

            // 内边距由 Frame 内部的 ContentBox 消费：Frame 自身必须无内边距，否则 PART_Layout 会被内缩，
            // 阴影被裁剪、close / progress 的覆盖定位也会偏移（见 semantic-part.md §5.4）。
            frame.Padding.ShouldBe(new Thickness(0));
            var contentBox = card.GetVisualDescendants()
                                 .OfType<Border>()
                                 .Single(static border => border.Name == "ContentBox");
            contentBox.Padding.ShouldBe(new Thickness(20));

            // BoxShadow 必须是 NotificationCard 自己的 StyledProperty，才能在 owner-scoped Style 中定制。
            NotificationCard.BoxShadowProperty.ShouldNotBeNull();
        }
        finally
        {
            window.Close();
        }

        var defaultCard = CreateCard(CreateManager(), NotificationType.Success, "Default", "Body");
        var defaultWindow = ShowInWindow(defaultCard);
        try
        {
            var frame = FindFrame(defaultCard);
            frame.Background.ShouldNotBeNull();
            frame.BoxShadow.ShouldNotBe(default(BoxShadows));
            frame.CornerRadius.ShouldNotBe(new CornerRadius(0));
            // Frame 自身无内边距（阴影/覆盖层定位需要），内边距落在内部 ContentBox 上。
            frame.Padding.ShouldBe(new Thickness(0));
            defaultCard.GetVisualDescendants()
                       .OfType<Border>()
                       .Single(static border => border.Name == "ContentBox")
                       .Padding.ShouldNotBe(new Thickness(0));
        }
        finally
        {
            defaultWindow.Close();
        }
    }

    [Fact]
    public void Frame_Sits_Directly_In_The_Motion_Actor_So_BoxShadow_Is_Not_Clipped()
    {
        // 回归：Frame 负责画 BoxShadow，必须直接作为 MotionActor 的内容。曾经在两者之间夹了一层
        // Panel#PART_Layout，阴影被裁到右/下各只剩 1 个逻辑像素，卡片看起来「右下角少了一块」。
        // Message 的 PART_Frame 正是 MotionActor 的直接内容，所以它的硬阴影一直正常。
        // Frame 与 MotionActor 之间只允许有 ContentControl 自带的 PART_ContentPresenter，
        // 不允许再出现任何布局 Panel。
        var manager = CreateManager();
        var card = CreateCard(manager, NotificationType.Success, "Shadow", "Body");
        card.BoxShadow = BoxShadows.Parse("4 4 0 #D9F7BE");

        var window = ShowInWindow(card);
        try
        {
            var frame = FindFrame(card);
            var motionActor = card.GetVisualDescendants()
                                  .OfType<BaseMotionActor>()
                                  .Single();
            motionActor.ClipToBounds.ShouldBeFalse();

            var between = frame.GetVisualAncestors()
                               .TakeWhile(static ancestor => ancestor is not BaseMotionActor)
                               .ToArray();
            between.ShouldNotBeEmpty();
            between.ShouldAllBe(static ancestor => ancestor is ContentPresenter);

            // PART_Layout 与内边距层都必须落在 Frame 内部：Frame 无内边距时 PART_Layout 才与卡片
            // 边框内侧重合，close / progress 的覆盖定位才贴着边框。
            var layout = card.GetVisualDescendants().OfType<Panel>().Single(static p => p.Name == "PART_Layout");
            layout.GetVisualAncestors().ShouldContain(frame);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void WindowNotificationManager_Template_Exposes_The_ListContent_Marker()
    {
        var manager = CreateManager();
        var window = ShowInWindow(manager);
        try
        {
            var listContent = FindSemanticControl<FeedbackStackPresenter>(manager, "semantic-list-content");
            listContent.Name.ShouldBe("PART_Items");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Generated_Semantic_Styles_Apply_To_The_Card_Targets()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(NotificationCard), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        var manager = CreateManager();
        var card = CreateCard(manager, NotificationType.Success, "Styled text", "Body");
        card.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector.OfType<NotificationCard>().Class("semantic-owner"));
        foreach (var part in descriptor.Parts.Where(static part => part.StyleType != null))
        {
            var partStyle = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
            partStyle.Setters.Add(new Setter(Control.TagProperty, part.Name));
            ownerStyle.Children.Add(partStyle);
        }
        card.Styles.Add(ownerStyle);

        var window = ShowInWindow(card);
        try
        {
            FindSemanticControl<DockPanel>(card, "semantic-wrapper").Tag.ShouldBe("wrapper");
            FindSemanticControl<IconPresenter>(card, "semantic-icon").Tag.ShouldBe("icon");
            FindSemanticControl<StackPanel>(card, "semantic-section").Tag.ShouldBe("section");
            FindSemanticControl<AtomUISelectableTextBlock>(card, "semantic-title").Tag.ShouldBe("title");
            FindSemanticControl<ContentPresenter>(card, "semantic-description").Tag.ShouldBe("description");
            FindSemanticControl<ContentPresenter>(card, "semantic-actions").Tag.ShouldBe("actions");
            FindSemanticControl<IconButton>(card, "semantic-close").Tag.ShouldBe("close");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Generated_Semantic_Styles_Apply_To_The_Manager_ListContent_Target()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(WindowNotificationManager), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        var manager = CreateManager();
        manager.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector.OfType<WindowNotificationManager>().Class("semantic-owner"));
        foreach (var part in descriptor.Parts.Where(static part => part.StyleType != null))
        {
            var partStyle = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
            partStyle.Setters.Add(new Setter(Control.TagProperty, part.Name));
            ownerStyle.Children.Add(partStyle);
        }
        manager.Styles.Add(ownerStyle);

        var window = ShowInWindow(manager);
        try
        {
            FindSemanticControl<FeedbackStackPresenter>(manager, "semantic-list-content").Tag
                .ShouldBe("listContent");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Root_Frame_Has_No_Own_Margin_And_List_Insets_The_Stack()
    {
        // root 语义几何契约（卡片自身零外边距，manager 承担列表内边距 marginLG，
        // listContent 承担卡片之间的间距）：
        //  1) root 高亮框必须等于可见卡片 —— 四边间距为 0，间距不能再塞进卡片外边距；
        //  2) 卡片排列区域由 manager 的内边距承担，通知集合不再自带边缘外边距。
        var host = new Border { Width = 600, Height = 260 };
        var manager = CreateManager();
        manager.MaxItems = 3;
        host.Child = manager;
        var window = new AvaloniaWindow { Width = 700, Height = 320, Content = host };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        manager.Show(new Notification(
            title: "Notification Title",
            content: "Body",
            expiration: TimeSpan.Zero));
        for (var i = 0; i < 6; i++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }

        try
        {
            var card  = manager.GetVisualDescendants().OfType<NotificationCard>().First();
            var frame = FindFrame(card);
            var cardOrigin  = card.TranslatePoint(new Point(0, 0), host)!.Value;
            var frameOrigin = frame.TranslatePoint(new Point(0, 0), host)!.Value;
            frameOrigin.X.ShouldBe(cardOrigin.X);
            frameOrigin.Y.ShouldBe(cardOrigin.Y);
            frame.Bounds.Width.ShouldBe(card.Bounds.Width);
            frame.Bounds.Height.ShouldBe(card.Bounds.Height);

            // list 的内边距承担边缘偏移：TopRight 放置时上边与右边各内缩一个 Padding。
            var listContent  = FindSemanticControl<FeedbackStackPresenter>(manager, "semantic-list-content");
            var listOrigin   = manager.TranslatePoint(new Point(0, 0), host)!.Value;
            var contentOrigin = listContent.TranslatePoint(new Point(0, 0), host)!.Value;
            (contentOrigin.Y - listOrigin.Y).ShouldBe(manager.Padding.Top, 0.5);
            ((listOrigin.X + manager.Bounds.Width) - (contentOrigin.X + listContent.Bounds.Width))
                .ShouldBe(manager.Padding.Right, 0.5);
            listContent.ExpandedGap.ShouldBeGreaterThan(0);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Manager_Shows_Cards_With_Markers_And_Removes_Them_On_Close()
    {
        var manager = CreateManager();
        manager.MaxItems = 3;
        var window = ShowInWindow(manager);
        try
        {
            manager.Show(new Notification("First", "One", expiration: TimeSpan.Zero));
            manager.Show(new Notification("Second", "Two", expiration: TimeSpan.Zero));
            Dispatcher.UIThread.RunJobs();

            var cards = manager.GetVisualDescendants().OfType<NotificationCard>().ToArray();
            cards.Length.ShouldBe(2);
            // marker 静态声明在卡片模板节点上，不注入到卡片实例本身。
            cards.ShouldAllBe(static card => !card.Classes.Contains("semantic-wrapper") &&
                                             !card.Classes.Contains("semantic-icon") &&
                                             !card.Classes.Contains("semantic-title"));

            foreach (var card in cards)
            {
                FindSemanticControl<DockPanel>(card, "semantic-wrapper").ShouldNotBeNull();
                FindSemanticControl<ContentPresenter>(card, "semantic-description").ShouldNotBeNull();
                FindSemanticControl<IconButton>(card, "semantic-close").ShouldNotBeNull();
            }

            var listContent = FindSemanticControl<FeedbackStackPresenter>(manager, "semantic-list-content");
            listContent.Items.Count.ShouldBe(2);

            cards[0].Close();
            Dispatcher.UIThread.RunJobs();
            listContent.Items.Count.ShouldBe(1);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Runtime_Created_Progress_Marker_Follows_Progress_Visibility()
    {
        var manager = CreateManager();
        var card = CreateCard(manager, NotificationType.Information, "With progress", "Body");
        card.Expiration     = TimeSpan.FromSeconds(4.5);
        card.IsShowProgress = true;

        var window = ShowInWindow(card);
        try
        {
            var progress = FindSemanticControl<NotificationProgressBar>(card, "semantic-progress");
            // progress 是贴卡片底边的覆盖层，不参与内容区的流式排版。
            progress.VerticalAlignment.ShouldBe(Avalonia.Layout.VerticalAlignment.Bottom);
            progress.HorizontalAlignment.ShouldBe(Avalonia.Layout.HorizontalAlignment.Stretch);
            progress.Margin.Left.ShouldBeGreaterThan(0);
            progress.Margin.Right.ShouldBe(progress.Margin.Left);
            // 进度条先铺底槽再叠加彩色进度值；缺少底槽时剩余时间不可见。
            progress.ProgressTrackBrush.ShouldNotBeNull();
            progress.ProgressIndicatorBrush.ShouldNotBeNull();

            // 生成的 Semantic Style 必须能命中运行时创建的节点（route 为 /template/ .semantic-progress，
            // 且节点显式 SetTemplatedParent 到卡片）。这是 progress Part 可用性的关键。
            var registry = Application.Current.ShouldNotBeNull()
                                      .GetThemeManager().ShouldNotBeNull()
                                      .SemanticParts;
            registry.TryGetControl(typeof(NotificationCard), out var descriptor).ShouldBeTrue();
            descriptor.ShouldNotBeNull();
            var progressPart = descriptor.Parts.Single(static part => part.Name == "progress");
            progressPart.RuntimeCreated.ShouldBeTrue();
            var progressStyle = (Style)Activator.CreateInstance(progressPart.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
            progressStyle.Setters.Add(new Setter(Control.TagProperty, "progress"));
            var ownerStyle = new Style(selector => selector.OfType<NotificationCard>());
            ownerStyle.Children.Add(progressStyle);
            card.Styles.Add(ownerStyle);
            Dispatcher.UIThread.RunJobs();
            FindSemanticControl<NotificationProgressBar>(card, "semantic-progress").Tag.ShouldBe("progress");

            // 关闭进度后 marker 与节点一起从卡片释放。
            card.IsShowProgress = false;
            Dispatcher.UIThread.RunJobs();
            card.GetVisualDescendants()
                .OfType<Control>()
                .Any(static control => control.Classes.Contains("semantic-progress"))
                .ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Host_Detach_Clears_Cards_And_Releases_The_Host_Layer()
    {
        var manager = CreateManager();
        var window = ShowInWindow(manager);
        try
        {
            manager.Show(new Notification("Transient", "Body", expiration: TimeSpan.Zero));
            Dispatcher.UIThread.RunJobs();
            window.GetVisualDescendants().OfType<NotificationCard>().Count().ShouldBe(1);
        }
        finally
        {
            window.Close();
        }

        Dispatcher.UIThread.RunJobs();
        manager.GetVisualDescendants().OfType<NotificationCard>().ShouldBeEmpty();
    }

    private static NotificationCard CreateCard(
        WindowNotificationManager manager,
        NotificationType type,
        string title,
        object? content)
    {
        return new NotificationCard(manager)
        {
            Title            = title,
            Content          = content,
            NotificationType = type,
            IsMotionEnabled  = false
        };
    }

    private static WindowNotificationManager CreateManager()
    {
        return new WindowNotificationManager { IsMotionEnabled = false };
    }

    private static AvaloniaWindow ShowInWindow(Control content)
    {
        var window = new AvaloniaWindow
        {
            Width   = 480,
            Height  = 320,
            Content = content
        };
        window.Show();
        content.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static Border FindFrame(NotificationCard card)
    {
        return card.GetVisualDescendants().OfType<Border>().Single(static border => border.Name == "Frame");
    }

    private static void AssertRoot(ControlSemanticDescriptor descriptor, Type controlType)
    {
        var root = descriptor.Parts.Single(static part => part.Name == "root");
        root.Path.ShouldBe("root");
        root.SelectorClass.ShouldBeNull();
        root.SelectorRoute.ShouldBeNull();
        root.ContractType.ShouldBe(controlType);
        root.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        root.Customization.ShouldBe(SemanticPartCustomization.Root);
        root.StyleType.ShouldBeNull();
        root.CrossVisualRoot.ShouldBeFalse();
        root.RuntimeCreated.ShouldBeFalse();
        root.Since.ShouldBe("6.2.0");
    }

    private static void AssertPart(
        ControlSemanticDescriptor descriptor,
        string name,
        string selectorClass,
        Type contractType,
        bool runtimeCreated = false)
    {
        var part = descriptor.Parts.Single(candidate => candidate.Name == name);
        part.Path.ShouldBe(name);
        part.SelectorClass.ShouldBe(selectorClass);
        part.SelectorRoute.ShouldBe($"/template/ .{selectorClass}");
        part.ContractType.ShouldBe(contractType);
        part.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        part.Customization.ShouldBe(SemanticPartCustomization.Selector);
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBe(runtimeCreated);
        part.Since.ShouldBe("6.2.0");
        part.StyleType.ShouldNotBeNull();
        part.StyleType.Name.ShouldBe($"{descriptor.ControlType.Name}{Pascal(name)}Style");
    }

    private static string Pascal(string partName)
    {
        return char.ToUpperInvariant(partName[0]) + partName[1..];
    }

    private static T FindSemanticControl<T>(Control owner, string marker)
        where T : Control
    {
        return owner.GetVisualDescendants()
                    .OfType<T>()
                    .Single(control => control.Classes.Contains(marker));
    }

    private static string[] CollectMarkers(XDocument document)
    {
        return document.Descendants()
                       .SelectMany(static element => element.Attributes()
                           .Where(static attribute => attribute.Name.LocalName.StartsWith(
                               "Classes.semantic-", StringComparison.Ordinal))
                           .Select(attribute =>
                               $"{attribute.Name.LocalName["Classes.".Length..]}:{attribute.Parent!.Name.LocalName}"))
                       .OrderBy(static marker => marker, StringComparer.Ordinal)
                       .ToArray();
    }

    private static bool HasMarker(XElement element, string marker)
    {
        var classProperty = element.Attribute($"Classes.{marker}");
        return classProperty is not null &&
               bool.TryParse(classProperty.Value, out var isEnabled) &&
               isEnabled;
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

        throw new FileNotFoundException($"Cannot locate repository file '{relativePath}'.");
    }
}
