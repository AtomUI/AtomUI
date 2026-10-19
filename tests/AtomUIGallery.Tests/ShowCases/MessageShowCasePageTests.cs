using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AtomUI.Desktop.Controls;
using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUIGallery.Localization;
using AtomUIGallery.ShowCases.Message;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AvaloniaTextBlock = Avalonia.Controls.TextBlock;
using DesktopButton = AtomUI.Desktop.Controls.Button;
using DesktopNumericUpDown = AtomUI.Desktop.Controls.NumericUpDown;
using DesktopToggleSwitch = AtomUI.Desktop.Controls.ToggleSwitch;

namespace AtomUIGallery.Tests.ShowCases;

public class MessageShowCasePageTests
{

    [Fact]
    public void Message_Stack_ShowCase_Isolates_Configuration_Lifetime_And_Destroy_Scope()
    {
        AvaloniaTestApp.EnsureInitialized();
        var page = new MessageShowCase();

        ShowInWindow(page, window =>
        {
            var items = page.GetVisualDescendants().OfType<ShowCaseItem>().ToArray();
            var stackItem = items.Single(item => item.SourceKey == "message-stack");
            stackItem.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var enabledSwitch = stackItem.GetVisualDescendants()
                                         .OfType<DesktopToggleSwitch>()
                                         .Single(control => control.Name == "StackEnabledSwitch");
            var thresholdInput = stackItem.GetVisualDescendants()
                                          .OfType<DesktopNumericUpDown>()
                                          .Single(control => control.Name == "StackThresholdInput");
            var openButton = stackItem.GetVisualDescendants()
                                      .OfType<DesktopButton>()
                                      .Single(control => control.Name == "OpenStackMessageButton");
            var destroyButton = stackItem.GetVisualDescendants()
                                         .OfType<DesktopButton>()
                                         .Single(control => control.Name == "DestroyStackMessagesButton");

            enabledSwitch.IsChecked.ShouldBe(true);
            thresholdInput.Value.ShouldBe(3m);
            thresholdInput.Minimum.ShouldBe(1m);
            thresholdInput.Maximum.ShouldBe(10m);
            thresholdInput.IsEnabled.ShouldBeTrue();

            RaiseClick(destroyButton);
            window.GetVisualDescendants().OfType<WindowMessageManager>().ShouldBeEmpty();

            RaiseClick(openButton);
            RaiseClick(openButton);
            window.UpdateLayout();

            var stackManager = window.GetVisualDescendants().OfType<WindowMessageManager>().Single();
            stackManager.IsStackEnabled.ShouldBeTrue();
            stackManager.StackThreshold.ShouldBe(3);
            stackManager.GetVisualDescendants()
                        .OfType<MessageCard>()
                        .Select(card => card.Message)
                        .ShouldBe([
                            "Message 1: This is a stacked message.",
                            "Message 2: This is a slightly longer stacked message."
                        ]);

            thresholdInput.Value = 5m;
            stackManager.StackThreshold.ShouldBe(5);

            enabledSwitch.IsChecked = false;
            stackManager.IsStackEnabled.ShouldBeFalse();
            thresholdInput.IsEnabled.ShouldBeFalse();

            enabledSwitch.IsChecked = true;
            stackManager.IsStackEnabled.ShouldBeTrue();
            thresholdInput.IsEnabled.ShouldBeTrue();

            var basicItem = items[0];
            var normalButton = basicItem.GetVisualDescendants().OfType<DesktopButton>().Single();
            RaiseClick(normalButton);
            window.UpdateLayout();

            var managers = window.GetVisualDescendants().OfType<WindowMessageManager>().ToArray();
            managers.Length.ShouldBe(2);
            var defaultManager = managers.Single(manager => !ReferenceEquals(manager, stackManager));
            defaultManager.IsStackEnabled.ShouldBeFalse();
            defaultManager.GetVisualDescendants().OfType<MessageCard>().Count().ShouldBe(1);

            stackManager.IsMotionEnabled = false;
            RaiseClick(destroyButton);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            stackManager.GetVisualDescendants().OfType<MessageCard>().ShouldBeEmpty();
            defaultManager.GetVisualDescendants().OfType<MessageCard>().Count().ShouldBe(1);

            RaiseClick(openButton);
            window.UpdateLayout();
            stackManager.GetVisualDescendants()
                        .OfType<MessageCard>()
                        .Single()
                        .Message.ShouldBe("Message 3: This is a stacked message.");

            window.Content = null;
            Dispatcher.UIThread.RunJobs();
            foreach (var manager in managers)
            {
                manager.GetVisualParent().ShouldBeNull();
            }
        });
    }

    [Fact]
    public void Message_Stack_ShowCase_Configuration_Row_Uses_One_Vertical_Center_Line()
    {
        AvaloniaTestApp.EnsureInitialized();
        var page = new MessageShowCase();

        ShowInWindow(page, window =>
        {
            var stackItem = page.GetVisualDescendants()
                                .OfType<ShowCaseItem>()
                                .Single(item => item.SourceKey == "message-stack");
            stackItem.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var enabledLabel = stackItem.GetVisualDescendants()
                                        .OfType<AvaloniaTextBlock>()
                                        .Single(control => control.Text == "Enabled:");
            var thresholdLabel = stackItem.GetVisualDescendants()
                                          .OfType<AvaloniaTextBlock>()
                                          .Single(control => control.Text == "Threshold:");
            var enabledSwitch = stackItem.GetVisualDescendants()
                                         .OfType<DesktopToggleSwitch>()
                                         .Single(control => control.Name == "StackEnabledSwitch");
            var thresholdInput = stackItem.GetVisualDescendants()
                                          .OfType<DesktopNumericUpDown>()
                                          .Single(control => control.Name == "StackThresholdInput");

            var expectedCenterY = GetCenterY(thresholdInput, stackItem);
            GetCenterY(enabledLabel, stackItem).ShouldBe(expectedCenterY, 0.5);
            GetCenterY(enabledSwitch, stackItem).ShouldBe(expectedCenterY, 0.5);
            GetCenterY(thresholdLabel, stackItem).ShouldBe(expectedCenterY, 0.5);
        });
    }

    [Fact]
    public void Message_ShowCase_Semantic_Parts_Follow_The_Two_Owner_Pattern()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Feedback/Message/Views/MessageShowCase.axaml");
        var codeBehind = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Feedback/Message/Views/MessageShowCase.axaml.cs");
        var semanticStylesResource = ExtractStylesResource(source, "MessageSemanticStyleStyles");

        source.ShouldContain("GalleryShowCaseHost.SemanticPartsContentTemplate");
        // 对齐上游 message#semantic-dom：单个预览承载两个 owner，不再拆成两个 Preview。
        CountOccurrences(source, "<gallery:SemanticPartPreview ").ShouldBe(1);
        source.ShouldContain("<gallery:SemanticPartPreview Name=\"MessageSemanticPreview\"");
        source.ShouldContain("<gallery:SemanticPartPreviewOwner Owner=\"{Binding #MessageManagerSemanticOwner}\"");
        source.ShouldContain("<gallery:SemanticPartPreviewOwner Owner=\"{Binding #MessageCardSemanticOwner}\"");
        source.ShouldContain("OwnerType=\"{x:Type atom:WindowMessageManager}\"");
        source.ShouldContain("OwnerType=\"{x:Type atom:MessageCard}\"");

        // 模板根必须是普通 Panel，滚动由宿主自身的 ScrollViewer 负责。Message 的两个 owner 都不是
        // 需要局部层宿主的遮罩类弹层控件（对照 Drawer 的 ScrollContentPresenter 舞台先例），在模板根
        // 再套一层 ScrollContentPresenter 只会形成嵌套滚动容器：呈现器未附加到视觉树时不会实现子
        // 节点，宿主曾因此把它误判为空模板并在切换到 Semantic Parts 时抛异常。
        source.ShouldNotContain("<ScrollContentPresenter");
        source.ShouldContain("<Panel MinHeight=\"320\"");

        // manager 有两个 public 构造（含无参），因此预览可声明式实例化；manager 没有声明式消息入口，
        // 示例数据由 owner 的 Loaded 事件调用 Show() 播种。
        source.ShouldContain("<atom:WindowMessageManager Name=\"MessageManagerSemanticOwner\"");
        source.ShouldContain("Loaded=\"HandleSemanticPreviewOwnerLoaded\"");
        codeBehind.ShouldContain("HandleSemanticPreviewOwnerLoaded");
        // 舞台根是 Panel/StackPanel 而非 owner 类型，必须显式绑定 SemanticOwner 才能做归属校验。
        source.ShouldContain("SemanticOwner=\"{Binding #MessageSemanticAnchor}\"");

        // 合并后的面板按 owner 顺序列出全部 6 个 Part；MessageCard 的 root 与 manager 的 root 以 OwnerType 消歧。
        var descriptions = Regex.Matches(source, "SemanticPartDescription Path=\"([^\"]+)\"")
                                .Select(static match => match.Groups[1].Value)
                                .ToArray();
        descriptions.ShouldBe(["root", "wrapper", "icon", "title", "root", "listContent"]);
        CountOccurrences(source, "OwnerType=\"{x:Type atom:MessageCard}\"").ShouldBeGreaterThanOrEqualTo(4);
        CountOccurrences(source, "OwnerType=\"{x:Type atom:WindowMessageManager}\"").ShouldBeGreaterThanOrEqualTo(3);

        // SemanticStyles 示例的样式以 Styles 资源声明（仍是生成的专用 Style 类），因为消息弹在
        // 窗口反馈层、不在页面视觉树内，无法用条目内联 Style 命中；资源由 code-behind 挂到 manager。
        semanticStylesResource.ShouldContain("atom:MessageCardIconStyle");
        semanticStylesResource.ShouldContain("atom:MessageCardTitleStyle");
        semanticStylesResource.ShouldContain("x:SetterTargetType=\"atom:IconPresenter\"");
        semanticStylesResource.ShouldContain("x:SetterTargetType=\"SelectableTextBlock\"");
        // 部件样式必须是声明式专用 Style，不得以事件处理器做代码回退。
        semanticStylesResource.ShouldNotContain(".Loaded=");
        semanticStylesResource.ShouldNotContain(".Unloaded=");
        semanticStylesResource.ShouldNotContain("/template/ .semantic-");
        codeBehind.ShouldContain("MessageSemanticStyleStyles");
        // 示例条目本身只保留触发按钮。
        var semanticStylesItem = ExtractShowCaseItem(source, "SemanticPartStyleTitle");
        semanticStylesItem.ShouldContain("HandleShowObjectStyleMessage");
        semanticStylesItem.ShouldContain("HandleShowFunctionStyleMessage");

        // 语义预览不得用代码回退定制部件：不得按 Name 查找模板节点后设置部件属性，也不得注册
        // AdditionalRoots（manager 内联在舞台视觉树内，无需跨根注册）。
        source.ShouldNotContain("/template/ .semantic-");
        codeBehind.ShouldNotContain("AdditionalRoots");
        codeBehind.ShouldNotContain("semantic-wrapper");
        codeBehind.ShouldNotContain("semantic-icon");
        codeBehind.ShouldNotContain("semantic-title");
        codeBehind.ShouldNotContain("semantic-list-content");
    }

    [Fact]
    public void Message_Semantic_Style_Example_Matches_Upstream_Object_And_Function_Styles()
    {
        // 严格对齐上游 components/message/demo/style-class.tsx：
        //  - Object style：root 绿底 / 2px 绿边 / 圆角 16 / 硬阴影 4px 4px 0 #d9f7be，icon 与 title 绿字
        //  - Function style：函数式按 type 分支，error 时整卡转红（浅红底 / 红边 / 红字 / 红色硬阴影）
        //  - 两条消息由按钮触发，经 manager 的 per-message classes 钩子染色（上游 styles 的等价物）
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Feedback/Message/Views/MessageShowCase.axaml");
        var codeBehind = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Feedback/Message/Views/MessageShowCase.axaml.cs");
        var item = ExtractShowCaseItem(source, "SemanticPartStyleTitle");
        var itemStyles = ExtractStylesResource(source, "MessageSemanticStyleStyles");

        // 对象式与函数式两套 owner 作用域 class。
        itemStyles.ShouldContain("atom|MessageCard.semantic-object-style-demo");
        itemStyles.ShouldContain("atom|MessageCard.semantic-function-style-demo");

        // Object style 精确值（上游 defaultStyles）。
        var objectStyle = ExtractStyleBlock(itemStyles, "semantic-object-style-demo\"");
        objectStyle.ShouldContain("Value=\"#F6FFED\"");
        objectStyle.ShouldContain("Value=\"#95DE64\"");
        objectStyle.ShouldContain("Value=\"2\"");
        objectStyle.ShouldContain("Value=\"16\"");
        objectStyle.ShouldContain("Value=\"4 4 0 #D9F7BE\"");
        objectStyle.ShouldContain("Value=\"#237804\"");
        objectStyle.ShouldContain("Value=\"SemiBold\"");

        // Function style：默认分支返回与 object styles 同款绿色，:error 分支整卡转红（上游 stylesFn）。
        var functionBase = ExtractStyleBlock(itemStyles, "semantic-function-style-demo\"");
        functionBase.ShouldContain("Value=\"#F6FFED\"");
        functionBase.ShouldContain("Value=\"#95DE64\"");
        functionBase.ShouldContain("Value=\"#237804\"");

        var functionError = ExtractStyleBlock(itemStyles, "semantic-function-style-demo:error");
        functionError.ShouldContain("Value=\"#FFF2F0\"");
        functionError.ShouldContain("Value=\"#FFCCC7\"");
        functionError.ShouldContain("Value=\"4 4 0 #FFCCC7\"");
        functionError.ShouldContain("Value=\"#CF1322\"");
        itemStyles.ShouldContain("MessageCard.semantic-function-style-demo:error");

        // root 表面定制必须落在 owner 的 StyledProperty 上（不是模板节点），这也是控件侧补透传的原因。
        itemStyles.ShouldContain("Property=\"Background\"");
        itemStyles.ShouldContain("Property=\"BorderBrush\"");
        itemStyles.ShouldContain("Property=\"BorderThickness\"");
        itemStyles.ShouldContain("Property=\"CornerRadius\"");
        itemStyles.ShouldContain("Property=\"BoxShadow\"");

        // 按钮触发：标签用上游原文，经 manager 的 classes 参数给弹出消息染色。
        item.ShouldContain("MessageShowCaseLangResource P2ContentObjectStyle");
        item.ShouldContain("MessageShowCaseLangResource P2ContentFunctionStyle");
        item.ShouldContain("Click=\"HandleShowObjectStyleMessage\"");
        item.ShouldContain("Click=\"HandleShowFunctionStyleMessage\"");
        codeBehind.ShouldContain("HandleShowObjectStyleMessage");
        codeBehind.ShouldContain("HandleShowFunctionStyleMessage");
        // per-message 样式钩子是 Show(IMessage, string[]? classes)，不能在代码里改部件属性。
        codeBehind.ShouldContain("semantic-object-style-demo");
        codeBehind.ShouldContain("semantic-function-style-demo");
        // 上游 demo 用 messageApi.open 的默认 duration = 3 秒；样式消息必须给有限时长，不能常驻堆积
        //（语义预览舞台的常驻播种消息是另一条有意为之的路径，不走这里）。
        var styledMessageMethod = ExtractMethodBody(codeBehind, "ShowStyledMessage");
        styledMessageMethod.ShouldContain("TimeSpan.FromSeconds(3)");
        styledMessageMethod.ShouldNotContain("TimeSpan.Zero");

        // 版本徽标必须读取 DisplayVersion，禁止硬编码。
    }

    // selectorSuffix 必须精确到闭合引号或伪类，避免匹配到同名前缀的其它样式块
    //（例如 semantic-function-style-demo 会先命中基础块而不是 :error 块）。
    private static string ExtractStyleBlock(string source, string selectorSuffix)
    {
        var marker = $"Selector=\"atom|MessageCard.{selectorSuffix}";
        var start  = source.IndexOf(marker, StringComparison.Ordinal);
        start.ShouldBeGreaterThanOrEqualTo(0);
        var end = source.IndexOf("</Style>", start, StringComparison.Ordinal);
        end.ShouldBeGreaterThan(start);
        return source[start..end];
    }

    [Fact]
    public void Message_Semantic_Style_Buttons_Pop_At_The_Window_Feedback_Layer()
    {
        // 输出行为对齐上游 style-class 示例：消息弹在视口顶部浮层（AtomUI 的 WindowFeedbackLayer），
        // 而不是页面内的局部容器。同时锁定 object / function 两套样式真的作用到卡片。
        // 反馈层在页面视觉树之外，页面内声明的 Style 命中不到它弹出的卡片——实测证明，所以语义样式
        // 以 Styles 资源声明后由 manager 自身携带（见 MessageShowCase.axaml + code-behind）。
        AvaloniaTestApp.EnsureInitialized();

        GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled = true;
        try
        {
            var page = new MessageShowCase
            {
                DataContext = new MessageViewModel(new TestScreen())
            };

            ShowInWindow(page, 1280, 900, () =>
            {
                ClickButton(page, Lang(MessageShowCaseLangResourceKind.P2ContentObjectStyle, "Object style"));
                Dispatcher.UIThread.RunJobs();

                // 卡片必须落在窗口反馈层里，且不是页面视觉子树的一部分。
                var card = FindLayerCard(page, "semantic-object-style-demo");
                var frame = card.GetVisualDescendants()
                                .OfType<Border>()
                                .Single(static border => border.Name == "PART_Frame");
                                BrushColor(frame.Background).ShouldBe(Color.Parse("#F6FFED"));
                BrushColor(frame.BorderBrush).ShouldBe(Color.Parse("#95DE64"));
                frame.BorderThickness.ShouldBe(new Thickness(2));
                frame.CornerRadius.ShouldBe(new CornerRadius(16));
                frame.BoxShadow.ShouldBe(BoxShadows.Parse("4 4 0 #D9F7BE"));

                ClickButton(page, Lang(MessageShowCaseLangResourceKind.P2ContentFunctionStyle, "Function style"));
                Dispatcher.UIThread.RunJobs();

                // function styles 的 error 分支：整卡转红（上游 stylesFn 在 type === 'error' 时替换这四个值）。
                var errorCard = FindLayerCard(page, "semantic-function-style-demo");
                var errorFrame = errorCard.GetVisualDescendants()
                                          .OfType<Border>()
                                          .Single(static border => border.Name == "PART_Frame");
                BrushColor(errorFrame.Background).ShouldBe(Color.Parse("#FFF2F0"));
                BrushColor(errorFrame.BorderBrush).ShouldBe(Color.Parse("#FFCCC7"));
                errorFrame.BoxShadow.ShouldBe(BoxShadows.Parse("4 4 0 #FFCCC7"));
            });
        }
        finally
        {
            GalleryShowCaseRuntimeOptions.ResetDeferredLoadingDisabledOverride();
        }
    }

    // 从窗口反馈层取卡片：manager 由宿主构造进 WindowFeedbackLayer，卡片不在页面子树内，
    // 因此以 TopLevel 为根枚举并按语义样式 class 精确匹配。
    private static MessageCard FindLayerCard(Control page, string styleClass)
    {
        var topLevel = TopLevel.GetTopLevel(page).ShouldNotBeNull();
        // 反馈层弹出：卡片不得出现在页面子树内。
        page.GetVisualDescendants().OfType<MessageCard>().ShouldBeEmpty();
        var manager = topLevel.GetVisualDescendants()
                              .OfType<WindowMessageManager>()
                              .Single(m => m.GetVisualParent() is not null &&
                                           m.GetVisualParent()!.GetType().Name.Contains("FeedbackLayer"));
        manager.ShouldNotBeNull();
        return manager.GetVisualDescendants()
                      .OfType<MessageCard>()
                      .Single(card => card.Classes.Contains(styleClass));
    }

    private static string ExtractMethodBody(string source, string methodName)
    {
        var marker = $"private void {methodName}(";
        var start  = source.IndexOf(marker, StringComparison.Ordinal);
        start.ShouldBeGreaterThanOrEqualTo(0);
        var open  = source.IndexOf('{', start);
        var depth = 0;
        for (var i = open; i < source.Length; i++)
        {
            if (source[i] == '{') depth++;
            else if (source[i] == '}')
            {
                depth--;
                if (depth == 0) return source[open..(i + 1)];
            }
        }
        throw new InvalidOperationException($"Unbalanced braces for '{methodName}'.");
    }

    private static string ExtractStylesResource(string source, string key)
    {
        var marker = $"<Styles x:Key=\"{key}\">";
        var start  = source.IndexOf(marker, StringComparison.Ordinal);
        start.ShouldBeGreaterThanOrEqualTo(0);
        var end = source.IndexOf("</Styles>", start, StringComparison.Ordinal);
        end.ShouldBeGreaterThan(start);
        return source[start..end];
    }

    private static string Lang(MessageShowCaseLangResourceKind kind, string fallback)
    {
        return GalleryLocalization.Get(kind, fallback);
    }

    private static Color BrushColor(IBrush? brush)
    {
        // 主题 Token 产出的是 ImmutableSolidColorBrush，按颜色比较而不是按刷子实例相等。
        var solid = brush.ShouldBeAssignableTo<ISolidColorBrush>();
        solid.ShouldNotBeNull();
        return solid!.Color;
    }

    private static void ClickButton(Control root, string content)
    {
        var button = root.GetVisualDescendants()
                         .OfType<Avalonia.Controls.Button>()
                         .Single(button => Equals(button.Content, content));
        button.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
    }

    [Fact]
    public void Message_Semantic_Previews_Materialize_When_The_Semantic_Tab_Is_Selected()
    {
        // 端到端回归：切到 Semantic Parts 时宿主必须能解析出两个 Preview。
        // 曾经此处因模板根是 ScrollContentPresenter、呈现器未附加时不实现子节点而抛
        // InvalidOperationException（"must build a Control containing at least one
        // SemanticPartPreview"），静态标记断言无法覆盖这条运行时路径。
        AvaloniaTestApp.EnsureInitialized();

        var page = new MessageShowCase
        {
            DataContext = new MessageViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldBeEmpty();

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            for (var i = 0; i < 3; i++)
            {
                Dispatcher.UIThread.RunJobs();
            }

            host.SemanticPartsContent.ShouldNotBeNull();
            var preview = host.GetVisualDescendants().OfType<SemanticPartPreview>().Single();
            preview.Title.ShouldBe("Message");
            preview.IsEffectivelyVisible.ShouldBeTrue();

            // 单个预览同时实例化两个 owner：无宿主 manager 与两张常驻卡片。
            host.GetVisualDescendants().OfType<WindowMessageManager>().ShouldNotBeEmpty();
            host.GetVisualDescendants().OfType<MessageCard>().Count().ShouldBeGreaterThanOrEqualTo(2);
        });
    }

    private static void ShowInWindow(Control content, double width, double height, Action assertion)
    {
        var visualLayerManager = new VisualLayerManager
        {
            EnableAdornerLayer = true,
            Child = content
        };
        var window = new AvaloniaWindow
        {
            Content = visualLayerManager,
            Width   = width,
            Height  = height
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

    private static string ExtractShowCaseItem(string source, string titleResourceName)
    {
        var titleMarker = $"MessageShowCaseLangResource {titleResourceName}";
        var titleIndex  = source.IndexOf(titleMarker, StringComparison.Ordinal);
        titleIndex.ShouldBeGreaterThanOrEqualTo(0);

        var itemStart = source.LastIndexOf("<gallery:ShowCaseItem", titleIndex, StringComparison.Ordinal);
        itemStart.ShouldBeGreaterThanOrEqualTo(0);

        var itemEnd = source.IndexOf("</gallery:ShowCaseItem>", titleIndex, StringComparison.Ordinal);
        itemEnd.ShouldBeGreaterThan(titleIndex);

        return source[itemStart..(itemEnd + "</gallery:ShowCaseItem>".Length)];
    }

    private static string ExtractMessageExampleItems(string source)
    {
        const string firstItemMarker  = "<gallery:ShowCaseItem";
        const string panelCloseMarker = "</gallery:ShowCasePanel>";

        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        var panelCloseStart = source.IndexOf(panelCloseMarker, firstItemStart, StringComparison.Ordinal);
        panelCloseStart.ShouldBeGreaterThan(firstItemStart);

        return source[firstItemStart..panelCloseStart];
    }

    private static string NormalizeMarkup(string source)
    {
        return ShowCaseSnapshotMarkup.Normalize(source);
    }

    private static string ComputeSha256(string source)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(source));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string ReadSnapshotHash(string source)
    {
        return source
            .Split('\n')
            .First(line => line.StartsWith("sha256:", StringComparison.Ordinal))
            .Split(':', 2)[1]
            .Trim();
    }

    private static int ReadSnapshotCount(string source)
    {
        return int.Parse(source
            .Split('\n')
            .First(line => line.StartsWith("count:", StringComparison.Ordinal))
            .Split(':', 2)[1]
            .Trim());
    }

    private static int CountShowCaseItemElements(string source)
    {
        return Regex.Matches(source, @"<gallery:ShowCaseItem(\s|>)", RegexOptions.CultureInvariant).Count;
    }

    private static int CountOccurrences(string source, string value)
    {
        var count      = 0;
        var startIndex = 0;
        while (true)
        {
            var matchIndex = source.IndexOf(value, startIndex, StringComparison.Ordinal);
            if (matchIndex < 0)
            {
                return count;
            }

            count++;
            startIndex = matchIndex + value.Length;
        }
    }

    private static double GetCenterY(Control control, Visual relativeTo)
    {
        var center = control.TranslatePoint(
            new Point(control.Bounds.Width / 2, control.Bounds.Height / 2),
            relativeTo);
        center.ShouldNotBeNull();
        return center.Value.Y;
    }

    private static void RaiseClick(DesktopButton button)
    {
        button.RaiseEvent(new RoutedEventArgs(DesktopButton.ClickEvent, button));
        Dispatcher.UIThread.RunJobs();
    }

    private static void ShowInWindow(Control content, Action<AvaloniaWindow> assertion)
    {
        var visualLayerManager = new VisualLayerManager
        {
            EnableAdornerLayer = true,
            Child              = content
        };
        var window = new AvaloniaWindow
        {
            Width   = 1000,
            Height  = 900,
            Content = visualLayerManager
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();

        try
        {
            assertion(window);
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

    private sealed class TestScreen : IScreen
    {
        public RoutingState Router { get; } = new();
    }
}
