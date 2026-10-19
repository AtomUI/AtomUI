using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUIGallery.Localization;
using AtomUIGallery.ShowCases.Notification;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using AvaloniaButton = Avalonia.Controls.Button;
using AvaloniaWindow = Avalonia.Controls.Window;
using AvaloniaTextBlock = Avalonia.Controls.TextBlock;
using DesktopButton = AtomUI.Desktop.Controls.Button;
using DesktopNumericUpDown = AtomUI.Desktop.Controls.NumericUpDown;
using DesktopToggleSwitch = AtomUI.Desktop.Controls.ToggleSwitch;
using NotificationCard = AtomUI.Desktop.Controls.NotificationCard;
using NotificationViewModel = AtomUIGallery.ShowCases.Notification.NotificationViewModel;
using WindowNotificationManager = AtomUI.Desktop.Controls.WindowNotificationManager;

namespace AtomUIGallery.Tests.ShowCases;

public class NotificationShowCasePageTests
{

    [Fact]
    public void Notification_Stack_ShowCase_Isolates_Configuration_Lifetime_And_Destroy_Scope()
    {
        AvaloniaTestApp.EnsureInitialized();
        var page = new NotificationShowCase();

        ShowInWindow(page, window =>
        {
            var items = page.GetVisualDescendants().OfType<ShowCaseItem>().ToArray();
            var stackItem = items.Single(item => item.SourceKey == "notification-stack");
            stackItem.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var enabledSwitch = stackItem.GetVisualDescendants()
                                         .OfType<DesktopToggleSwitch>()
                                         .Single(control => control.Name == "NotificationStackEnabledSwitch");
            var thresholdInput = stackItem.GetVisualDescendants()
                                          .OfType<DesktopNumericUpDown>()
                                          .Single(control => control.Name == "NotificationStackThresholdInput");
            var openButton = stackItem.GetVisualDescendants()
                                      .OfType<DesktopButton>()
                                      .Single(control => control.Name == "OpenStackNotificationButton");
            var destroyButton = stackItem.GetVisualDescendants()
                                         .OfType<DesktopButton>()
                                         .Single(control => control.Name == "DestroyStackNotificationsButton");

            enabledSwitch.IsChecked.ShouldBe(true);
            thresholdInput.Value.ShouldBe(3m);
            thresholdInput.Minimum.ShouldBe(1m);
            thresholdInput.Maximum.ShouldBe(10m);
            thresholdInput.IsEnabled.ShouldBeTrue();

            RaiseClick(destroyButton);
            window.GetVisualDescendants().OfType<WindowNotificationManager>().ShouldBeEmpty();

            RaiseClick(openButton);
            RaiseClick(openButton);
            window.UpdateLayout();

            var stackManager = window.GetVisualDescendants().OfType<WindowNotificationManager>().Single();
            stackManager.IsStackEnabled.ShouldBeTrue();
            stackManager.StackThreshold.ShouldBe(3);
            stackManager.GetVisualDescendants()
                        .OfType<NotificationCard>()
                        .Select(card => card.Content)
                        .ShouldBe([
                            "Notification 1: This is a stacked notification.",
                            "Notification 2: This is a deliberately longer stacked notification used to verify variable-height cards."
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

            var managers = window.GetVisualDescendants().OfType<WindowNotificationManager>().ToArray();
            managers.Length.ShouldBe(2);
            var defaultManager = managers.Single(manager => !ReferenceEquals(manager, stackManager));
            defaultManager.GetVisualDescendants().OfType<NotificationCard>().Count().ShouldBe(1);

            stackManager.IsMotionEnabled = false;
            RaiseClick(destroyButton);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            stackManager.GetVisualDescendants().OfType<NotificationCard>().ShouldBeEmpty();
            defaultManager.GetVisualDescendants().OfType<NotificationCard>().Count().ShouldBe(1);

            RaiseClick(openButton);
            window.UpdateLayout();
            stackManager.GetVisualDescendants()
                        .OfType<NotificationCard>()
                        .Single()
                        .Content.ShouldBe("Notification 3: This is a stacked notification.");

            window.Content = null;
            Dispatcher.UIThread.RunJobs();
            foreach (var manager in managers)
            {
                manager.GetVisualParent().ShouldBeNull();
            }
        });
    }

    [Fact]
    public void Notification_Actions_ShowCase_Button_Displays_Card_With_Action_Buttons()
    {
        AvaloniaTestApp.EnsureInitialized();
        var page = new NotificationShowCase
        {
            DataContext = new NotificationViewModel(new TestScreen())
        };

        ShowInWindow(page, window =>
        {
            var actionsTitle = Lang(NotificationShowCaseLangResourceKind.ActionsTitle, "Notification with actions");
            var actionsItem = page.GetVisualDescendants()
                                  .OfType<ShowCaseItem>()
                                  .Single(item => Equals(item.Title, actionsTitle));
            actionsItem.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var openButton = actionsItem.GetVisualDescendants().OfType<DesktopButton>().Single();
            RaiseClick(openButton);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var manager = window.GetVisualDescendants().OfType<WindowNotificationManager>().Single();
            var cards = manager.GetVisualDescendants().OfType<NotificationCard>().ToArray();
            cards.Length.ShouldBe(1, "the Actions example must show a notification when its button is clicked");
            var card = cards[0];
            card.Actions.ShouldNotBeNull();
            var actionButtons = card.GetVisualDescendants().OfType<DesktopButton>().ToArray();
            actionButtons.Select(button => button.Content).ShouldBe([
                Lang(NotificationShowCaseLangResourceKind.P2ContentDestroyAll, "Destroy All"),
                Lang(NotificationShowCaseLangResourceKind.P2ContentConfirm, "Confirm")
            ]);
            actionButtons.ShouldAllBe(button => button.IsEffectivelyVisible);
        });
    }

    [Fact]
    public void Notification_ShowCase_Semantic_Parts_Follow_The_Two_Owner_Pattern()
    {
        var source     = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Feedback/Notification/Views/NotificationShowCase.axaml");
        var codeBehind = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Feedback/Notification/Views/NotificationShowCase.axaml.cs");
        var semanticStylesResource = ExtractStylesResource(source, "NotificationSemanticStyleStyles");

        source.ShouldContain("GalleryShowCaseHost.SemanticPartsContentTemplate");
        // 单个预览承载两个 owner，不再拆成两个 Preview。
        CountOccurrences(source, "<gallery:SemanticPartPreview ").ShouldBe(1);
        source.ShouldContain("<gallery:SemanticPartPreview Name=\"NotificationSemanticPreview\"");
        source.ShouldContain("<gallery:SemanticPartPreviewOwner Owner=\"{Binding #NotificationCardSemanticOwner}\"");
        source.ShouldContain("OwnerType=\"{x:Type atom:NotificationCard}\"");
        source.ShouldContain("<gallery:SemanticPartPreviewOwner Owner=\"{Binding #NotificationManagerSemanticOwner}\"");
        source.ShouldContain("OwnerType=\"{x:Type atom:WindowNotificationManager}\"");
        // 模板根必须是普通 Panel；两个 owner 都用声明式实例（卡片有无参公开构造）。
        source.ShouldNotContain("<ScrollContentPresenter");
        source.ShouldContain("<Panel MinHeight=\"320\"");
        source.ShouldContain("<atom:WindowNotificationManager Name=\"NotificationManagerSemanticOwner\"");
        source.ShouldContain("<atom:NotificationCard Name=\"NotificationCardSemanticOwner\"");
        source.ShouldContain("SemanticOwner=\"{Binding #NotificationSemanticAnchor}\"");

        // 面板按 owner 顺序列出全部 11 个 Part（卡片 9 + manager 2）。
        var descriptions = Regex.Matches(source, "SemanticPartDescription Path=\"([^\"]+)\"")
                                .Select(static match => match.Groups[1].Value)
                                .ToArray();
        descriptions.ShouldBe(
        [
            "root", "wrapper", "icon", "section", "title", "description", "close", "actions", "progress",
            "root", "listContent"
        ]);
        CountOccurrences(source, "OwnerType=\"{x:Type atom:NotificationCard}\"").ShouldBeGreaterThanOrEqualTo(10);
        CountOccurrences(source, "OwnerType=\"{x:Type atom:WindowNotificationManager}\"").ShouldBeGreaterThanOrEqualTo(3);

        // 语义样式以 Styles 资源声明（生成的专用 Style 类）；反馈层在页面视觉树之外，无法用条目内联 Style
        // 命中，资源由 code-behind 挂到 manager。部件样式必须是声明式专用 Style，不得以事件处理器做代码回退。
        semanticStylesResource.ShouldContain("atom:NotificationCardIconStyle");
        semanticStylesResource.ShouldContain("atom:NotificationCardTitleStyle");
        semanticStylesResource.ShouldContain("atom:NotificationCardDescriptionStyle");
        semanticStylesResource.ShouldContain("x:SetterTargetType=\"atom:IconPresenter\"");
        semanticStylesResource.ShouldContain("x:SetterTargetType=\"ContentPresenter\"");
        semanticStylesResource.ShouldNotContain(".Loaded=");
        semanticStylesResource.ShouldNotContain(".Unloaded=");
        semanticStylesResource.ShouldNotContain("/template/ .semantic-");
        source.ShouldNotContain("/template/ .semantic-");
        codeBehind.ShouldContain("NotificationSemanticStyleStyles");
        // 语义预览不得用代码回退定制部件：不得按 Name 查找模板节点后设置部件属性，也不得注册 AdditionalRoots。
        codeBehind.ShouldNotContain("AdditionalRoots");
        codeBehind.ShouldNotContain("semantic-wrapper");
        codeBehind.ShouldNotContain("semantic-section");
        codeBehind.ShouldNotContain("semantic-close");
    }

    [Fact]
    public void Notification_Semantic_Style_Example_Matches_Upstream_Default_And_Error_Styles()
    {
        //  - default：卡片绿底 / 2px 绿边 / 圆角 16 / 硬阴影 4px 4px 0 #d9f7be，icon / title / description 绿字
        //  - error：整卡转红（浅红底 / 红边 / 红字 / 红色硬阴影）
        var source     = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Feedback/Notification/Views/NotificationShowCase.axaml");
        var codeBehind = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Feedback/Notification/Views/NotificationShowCase.axaml.cs");
        var itemStyles = ExtractStylesResource(source, "NotificationSemanticStyleStyles");

        var defaultStyle = ExtractStyleBlock(itemStyles, "semantic-default-style-demo\"");
        defaultStyle.ShouldContain("Value=\"#F6FFED\"");
        defaultStyle.ShouldContain("Value=\"#95DE64\"");
        defaultStyle.ShouldContain("Value=\"2\"");
        defaultStyle.ShouldContain("Value=\"16\"");
        defaultStyle.ShouldContain("Value=\"4 4 0 #D9F7BE\"");
        defaultStyle.ShouldContain("Value=\"#237804\"");
        defaultStyle.ShouldContain("Value=\"SemiBold\"");
        defaultStyle.ShouldContain("Value=\"#3F6600\"");

        var errorStyle = ExtractStyleBlock(itemStyles, "semantic-error-style-demo:error");
        errorStyle.ShouldContain("Value=\"#FFF2F0\"");
        errorStyle.ShouldContain("Value=\"#FFCCC7\"");
        errorStyle.ShouldContain("Value=\"4 4 0 #FFCCC7\"");
        errorStyle.ShouldContain("Value=\"#CF1322\"");
        errorStyle.ShouldContain("Value=\"#5C0011\"");

        // root 表面定制必须落在 owner 的 StyledProperty 上（不是模板节点）。
        itemStyles.ShouldContain("Property=\"Background\"");
        itemStyles.ShouldContain("Property=\"BorderBrush\"");
        itemStyles.ShouldContain("Property=\"BorderThickness\"");
        itemStyles.ShouldContain("Property=\"CornerRadius\"");
        itemStyles.ShouldContain("Property=\"BoxShadow\"");

        codeBehind.ShouldContain("semantic-default-style-demo");
        codeBehind.ShouldContain("semantic-error-style-demo");
        ExtractMethodBody(codeBehind, "ShowStyledNotification").ShouldContain("TimeSpan.FromSeconds(3)");
    }

    [Fact]
    public void Notification_Semantic_Previews_Materialize_When_The_Semantic_Tab_Is_Selected()
    {
        // 端到端回归：切到 Semantic Parts 时宿主必须能解析出 Preview，并实例化两个 owner。
        AvaloniaTestApp.EnsureInitialized();

        var page = new NotificationShowCase
        {
            DataContext = new NotificationViewModel(new TestScreen())
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
            preview.Title.ShouldBe("Notification");
            preview.IsEffectivelyVisible.ShouldBeTrue();

            // 单个预览同时实例化两个 owner：无宿主 manager 与播种在锚点上的常驻卡片。
            host.GetVisualDescendants().OfType<WindowNotificationManager>().ShouldNotBeEmpty();
            var card = host.GetVisualDescendants().OfType<NotificationCard>().Single();
            card.GetVisualDescendants()
                .OfType<Control>()
                .Count(static control => control.Classes.Contains("semantic-wrapper"))
                .ShouldBe(1);
        });
    }

    [Fact]
    public void Notification_Stack_ShowCase_Localization_Is_Complete()
    {
        foreach (var language in new[] { "en-US", "zh-CN", "zh-TW", "pt-BR" })
        {
            var localization = XliffTestDocument.Read(
                $"controlgallery/AtomUIGallery/ShowCases/Feedback/Notification/Localization/{language}.xlf");

            foreach (var key in new[]
                     {
                         "StackTitle",
                         "StackDescription",
                         "StackEnabledLabel",
                         "StackThresholdLabel",
                         "P2ContentDestroyAll",
                         "P2NotificationStackedTitleFormat",
                         "P2NotificationStackedFormat",
                         "P2NotificationLongStackedFormat"
                     })
            {
                localization.ContainsKey(key).ShouldBeTrue($"Missing {key} in {language}.");
            }
        }
    }

    [Fact]
    public void Notification_Stack_ShowCase_Configuration_Row_Uses_One_Vertical_Center_Line()
    {
        AvaloniaTestApp.EnsureInitialized();
        var page = new NotificationShowCase();

        ShowInWindow(page, window =>
        {
            var stackItem = page.GetVisualDescendants()
                                .OfType<ShowCaseItem>()
                                .Single(item => item.SourceKey == "notification-stack");
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
                                         .Single(control => control.Name == "NotificationStackEnabledSwitch");
            var thresholdInput = stackItem.GetVisualDescendants()
                                          .OfType<DesktopNumericUpDown>()
                                          .Single(control => control.Name == "NotificationStackThresholdInput");

            var expectedCenterY = GetCenterY(thresholdInput, stackItem);
            GetCenterY(enabledLabel, stackItem).ShouldBe(expectedCenterY, 0.5);
            GetCenterY(enabledSwitch, stackItem).ShouldBe(expectedCenterY, 0.5);
            GetCenterY(thresholdLabel, stackItem).ShouldBe(expectedCenterY, 0.5);
        });
    }

    [Fact]
    public void Notification_Semantic_Style_Buttons_Produce_Styled_Cards()
    {
        // 点击示例按钮后弹出的卡片必须真的带上 default（浅绿）与 error（红）语义样式。
        // 仅断言 AXAML 文本无法覆盖资源解析与样式挂载这条运行时路径。
        AvaloniaTestApp.EnsureInitialized();

        GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled = true;
        try
        {
            var page = new NotificationShowCase
            {
                DataContext = new NotificationViewModel(new TestScreen())
            };

            ShowInWindow(page, 1280, 900, () =>
            {
                ClickButton(page, Lang(NotificationShowCaseLangResourceKind.P2ContentDefaultNotification, "Default Notification"));
                Dispatcher.UIThread.RunJobs();

                var card = FindLayerCard(page, "semantic-default-style-demo");
                var frame = FindFrame(card);
                BrushColor(frame.Background).ShouldBe(Color.Parse("#F6FFED"));
                BrushColor(frame.BorderBrush).ShouldBe(Color.Parse("#95DE64"));
                frame.BorderThickness.ShouldBe(new Thickness(2));
                frame.CornerRadius.ShouldBe(new CornerRadius(16));
                frame.BoxShadow.ShouldBe(BoxShadows.Parse("4 4 0 #D9F7BE"));
                BrushColor(FindSemanticControl<AvaloniaTextBlock>(card, "semantic-title").Foreground)
                    .ShouldBe(Color.Parse("#237804"));
                BrushColor(FindSemanticControl<ContentPresenter>(card, "semantic-description").Foreground)
                    .ShouldBe(Color.Parse("#3F6600"));
                // icon 部件也必须按 owner-scoped 语义样式染色，而不是回落到主题的状态色。
                var defaultIcon = FindSemanticControl<IconPresenter>(card, "semantic-icon").IconBrush.ShouldNotBeNull();
                BrushColor(defaultIcon).ShouldBe(Color.Parse("#237804"));

                ClickButton(page, Lang(NotificationShowCaseLangResourceKind.P2ContentErrorNotification, "Error Notification"));
                Dispatcher.UIThread.RunJobs();

                var errorCard = FindLayerCard(page, "semantic-error-style-demo");
                var errorFrame = FindFrame(errorCard);
                BrushColor(errorFrame.Background).ShouldBe(Color.Parse("#FFF2F0"));
                BrushColor(errorFrame.BorderBrush).ShouldBe(Color.Parse("#FFCCC7"));
                errorFrame.BoxShadow.ShouldBe(BoxShadows.Parse("4 4 0 #FFCCC7"));
                BrushColor(FindSemanticControl<AvaloniaTextBlock>(errorCard, "semantic-title").Foreground)
                    .ShouldBe(Color.Parse("#CF1322"));
                BrushColor(FindSemanticControl<ContentPresenter>(errorCard, "semantic-description").Foreground)
                    .ShouldBe(Color.Parse("#5C0011"));
                BrushColor(FindSemanticControl<IconPresenter>(errorCard, "semantic-icon").IconBrush)
                    .ShouldBe(Color.Parse("#CF1322"));
            });
        }
        finally
        {
            GalleryShowCaseRuntimeOptions.ResetDeferredLoadingDisabledOverride();
        }
    }

    private static NotificationCard FindLayerCard(Control page, string styleClass)
    {
        var topLevel = TopLevel.GetTopLevel(page).ShouldNotBeNull();
        // 反馈层弹出：卡片不得出现在页面子树内。
        page.GetVisualDescendants().OfType<NotificationCard>().ShouldBeEmpty();
        var manager = topLevel.GetVisualDescendants()
                              .OfType<WindowNotificationManager>()
                              .Single(m => m.GetVisualParent() is not null &&
                                           m.GetVisualParent()!.GetType().Name.Contains("FeedbackLayer"));
        return manager.GetVisualDescendants()
                      .OfType<NotificationCard>()
                      .Single(card => card.Classes.Contains(styleClass));
    }

    private static Border FindFrame(NotificationCard card)
    {
        return card.GetVisualDescendants().OfType<Border>().Single(static border => border.Name == "Frame");
    }

    private static T FindSemanticControl<T>(Control owner, string marker)
        where T : Control
    {
        return owner.GetVisualDescendants()
                    .OfType<T>()
                    .Single(control => control.Classes.Contains(marker));
    }

    private static Color BrushColor(IBrush? brush)
    {
        var solid = brush.ShouldBeAssignableTo<ISolidColorBrush>();
        solid.ShouldNotBeNull();
        return solid!.Color;
    }

    private static void ClickButton(Control root, string content)
    {
        var button = root.GetVisualDescendants()
                         .OfType<AvaloniaButton>()
                         .Single(candidate => Equals(candidate.Content, content));
        button.RaiseEvent(new RoutedEventArgs(AvaloniaButton.ClickEvent));
    }

    private static string Lang(NotificationShowCaseLangResourceKind kind, string fallback)
    {
        return GalleryLocalization.Get(kind, fallback);
    }

    private static string ExtractNotificationExampleItems(string source)
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

    // selectorSuffix 必须精确到闭合引号或伪类，避免匹配到同名前缀的其它样式块。
    private static string ExtractStyleBlock(string source, string selectorSuffix)
    {
        var marker = $"Selector=\"atom|NotificationCard.{selectorSuffix}";
        var start  = source.IndexOf(marker, StringComparison.Ordinal);
        start.ShouldBeGreaterThanOrEqualTo(0);
        var end = source.IndexOf("</Style>", start, StringComparison.Ordinal);
        end.ShouldBeGreaterThan(start);
        return source[start..end];
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

    private static void ShowInWindow(Control content, Action<AvaloniaWindow> assertion)
    {
        var visualLayerManager = new VisualLayerManager
        {
            EnableAdornerLayer = true,
            Child = content
        };
        var window = new AvaloniaWindow
        {
            Width = 1000,
            Height = 900,
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
