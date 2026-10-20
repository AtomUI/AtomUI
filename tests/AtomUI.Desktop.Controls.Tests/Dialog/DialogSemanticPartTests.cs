using System.Xml.Linq;
using AtomUI.Controls.Primitives;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIDialog = AtomUI.Desktop.Controls.Dialog;
using AtomUIMessageBox = AtomUI.Desktop.Controls.MessageBox;

namespace AtomUI.Desktop.Controls.Tests.Dialog;

public class DialogSemanticPartTests
{
    // manifest 顺序：隐式 root 在前，其余按 path 字典序。
    private static readonly string[] ApprovedPartNames =
    [
        "root", "body", "close", "container", "footer", "header", "mask", "title", "wrapper"
    ];

    private const string MaskThemePath =
        "src/AtomUI.Desktop.Controls/Dialog/Themes/OverlayDialogMaskTheme.axaml";

    private const string PresenterThemePath =
        "src/AtomUI.Desktop.Controls/Dialog/Themes/OverlayDialogPresenterTheme.axaml";

    private const string SurfaceThemePath =
        "src/AtomUI.Desktop.Controls/Dialog/Themes/DialogSurfaceTheme.axaml";

    private const string HeaderThemePath =
        "src/AtomUI.Desktop.Controls/Dialog/Themes/OverlayDialogHeaderTheme.axaml";

    static DialogSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_Only_The_Approved_Dialog_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUIDialog), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name).ShouldBe(ApprovedPartNames);

        AssertRoot(descriptor, typeof(AtomUIDialog));
        AssertPart(descriptor, "Dialog", "body", "semantic-body",
            typeof(Border), ">> .semantic-scope-content-layer > .semantic-body");
        AssertPart(descriptor, "Dialog", "close", "semantic-close",
            typeof(Avalonia.Controls.Button), ">> .semantic-scope-content-layer > .semantic-scope-header /template/ .semantic-close", SemanticPartCardinality.Optional);
        AssertPart(descriptor, "Dialog", "container", "semantic-container",
            typeof(Border), ">> .semantic-scope-frame-host > .semantic-container");
        AssertPart(descriptor, "Dialog", "footer", "semantic-footer",
            typeof(Border), ">> .semantic-scope-content-layer > .semantic-footer");
        AssertPart(descriptor, "Dialog", "header", "semantic-header",
            typeof(Border), ">> .semantic-scope-content-layer > .semantic-scope-header /template/ .semantic-header", SemanticPartCardinality.Optional);
        AssertPart(descriptor, "Dialog", "mask", "semantic-mask",
            typeof(Border), ">> .semantic-scope-mask /template/ .semantic-mask", SemanticPartCardinality.Optional);
        AssertPart(descriptor, "Dialog", "title", "semantic-title",
            typeof(Avalonia.Controls.TextBlock), ">> .semantic-scope-content-layer > .semantic-scope-header /template/ .semantic-title", SemanticPartCardinality.Optional);
        AssertPart(descriptor, "Dialog", "wrapper", "semantic-wrapper",
            typeof(Avalonia.Controls.Control), ">> .semantic-scope-presenter > .semantic-wrapper", SemanticPartCardinality.Optional);
    }

    [Fact]
    public void Registered_Descriptor_Exposes_Only_The_Approved_MessageBox_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUIMessageBox), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name).ShouldBe(ApprovedPartNames);

        AssertRoot(descriptor, typeof(AtomUIMessageBox));
        AssertPart(descriptor, "MessageBox", "body", "semantic-body",
            typeof(Border), ">> .semantic-scope-content-layer > .semantic-body");
        AssertPart(descriptor, "MessageBox", "close", "semantic-close",
            typeof(Avalonia.Controls.Button), ">> .semantic-scope-content-layer > .semantic-scope-header /template/ .semantic-close", SemanticPartCardinality.Optional);
        AssertPart(descriptor, "MessageBox", "container", "semantic-container",
            typeof(Border), ">> .semantic-scope-frame-host > .semantic-container");
        AssertPart(descriptor, "MessageBox", "footer", "semantic-footer",
            typeof(Border), ">> .semantic-scope-content-layer > .semantic-footer");
        AssertPart(descriptor, "MessageBox", "header", "semantic-header",
            typeof(Border), ">> .semantic-scope-content-layer > .semantic-scope-header /template/ .semantic-header", SemanticPartCardinality.Optional);
        AssertPart(descriptor, "MessageBox", "mask", "semantic-mask",
            typeof(Border), ">> .semantic-scope-mask /template/ .semantic-mask", SemanticPartCardinality.Optional);
        AssertPart(descriptor, "MessageBox", "title", "semantic-title",
            typeof(Avalonia.Controls.TextBlock), ">> .semantic-scope-content-layer > .semantic-scope-header /template/ .semantic-title", SemanticPartCardinality.Optional);
        AssertPart(descriptor, "MessageBox", "wrapper", "semantic-wrapper",
            typeof(Avalonia.Controls.Control), ">> .semantic-scope-presenter > .semantic-wrapper", SemanticPartCardinality.Optional);
    }

    [Fact]
    public void Host_Themes_Carry_The_Approved_Static_Markers()
    {
        CollectMarkers(XDocument.Load(GetRepoFile(MaskThemePath)))
            .ShouldBe(["semantic-mask:Border"]);
        CollectMarkers(XDocument.Load(GetRepoFile(PresenterThemePath)))
            .ShouldBe([
                "semantic-scope-mask:OverlayDialogMask",
                "semantic-scope-presenter:Panel",
                "semantic-wrapper:MotionActor"
            ]);
        CollectMarkers(XDocument.Load(GetRepoFile(SurfaceThemePath)))
            .ShouldBe([
                "semantic-body:Border",
                "semantic-container:Border",
                "semantic-footer:Border",
                "semantic-scope-content-layer:DockPanel",
                "semantic-scope-frame-host:ShadowsAwareContainer",
                "semantic-scope-header:OverlayDialogHeader"
            ]);
        CollectMarkers(XDocument.Load(GetRepoFile(HeaderThemePath)))
            .ShouldBe(["semantic-close:DialogCaptionButton", "semantic-header:Border", "semantic-title:TextBlock"]);
    }

    [Fact]
    public void Overlay_Dialog_Exposes_All_Part_Markers_On_The_Intended_Nodes()
    {
        RunOnUIThread(() =>
        {
            var placementTarget = CreatePlacementTarget();
            var dialog          = CreateDialog(placementTarget);
            var window          = CreateHost(placementTarget, dialog);
            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                dialog.IsOpen = true;
                PumpUntil(() => window.GetVisualDescendants().OfType<OverlayDialogPresenter>().Any());

                AssertAllPartMarkers(window);
            }
            finally
            {
                CloseWindow(window, dialog);
            }
        });
    }

    [Fact]
    public void Overlay_Presenter_Is_Logically_Parented_To_The_Dialog_While_Open()
    {
        RunOnUIThread(() =>
        {
            var placementTarget = CreatePlacementTarget();
            var dialog          = CreateDialog(placementTarget);
            var window          = CreateHost(placementTarget, dialog);
            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                dialog.IsOpen = true;
                PumpUntil(() => window.GetVisualDescendants().OfType<OverlayDialogPresenter>().Any());

                var presenter = window.GetVisualDescendants().OfType<OverlayDialogPresenter>().Single();
                ((ILogical)presenter).LogicalParent.ShouldBeSameAs(dialog);
            }
            finally
            {
                CloseWindow(window, dialog);
            }
        });
    }

    [Fact]
    public void Dialog_Reports_The_Presenter_As_The_Live_Cross_Root()
    {
        RunOnUIThread(() =>
        {
            var placementTarget = CreatePlacementTarget();
            var dialog          = CreateDialog(placementTarget);
            var window          = CreateHost(placementTarget, dialog);
            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                dialog.IsOpen = true;
                PumpUntil(() => dialog.GetCrossRoots().Count == 1);
                dialog.GetCrossRoots()[0].ShouldBeOfType<OverlayDialogPresenter>();

                dialog.IsOpen = false;
                PumpUntil(() => dialog.GetCrossRoots().Count == 0);
                dialog.GetCrossRoots().ShouldBeEmpty();
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Generated_Semantic_Styles_Hit_Exactly_One_Node_Per_Part_On_Dialog()
    {
        RunOnUIThread(() =>
        {
            var placementTarget = CreatePlacementTarget();
            var dialog          = CreateDialog(placementTarget);
            var window          = CreateHost(placementTarget, dialog);
            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                AssertGeneratedStylesHitEveryPart(window, dialog, typeof(AtomUIDialog));
            }
            finally
            {
                CloseWindow(window, dialog);
            }
        });
    }

    [Fact]
    public void Generated_Semantic_Styles_Hit_Exactly_One_Node_Per_Part_On_MessageBox()
    {
        RunOnUIThread(() =>
        {
            var placementTarget = CreatePlacementTarget();
            var messageBox = new AtomUIMessageBox
            {
                PlacementTarget = placementTarget,
                Title           = "Title",
                Style           = AtomUI.Desktop.Controls.MessageBoxStyle.Confirm,
                StandardButtons = AtomUI.Desktop.Controls.DialogStandardButton.Ok,
                IsMotionEnabled = false,
                HostWidth       = 320,
                HostHeight      = 180
            };
            var window = CreateHost(placementTarget, messageBox);
            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                AssertGeneratedStylesHitEveryPart(window, messageBox, typeof(AtomUIMessageBox));
            }
            finally
            {
                CloseWindow(window, messageBox);
            }
        });
    }

    // 生成 Style 是 owner 级嵌套样式：owner 必须与 placement target 同处可视树，路由再经 owner logical
    // parent 命中 presenter 子树。MessageBox 与 Dialog 共用 presenter/Surface，需分别验证各自的 descriptor
    // 与生成 Style 都能精确命中。
    [Fact]
    public void Window_Host_Reports_The_Native_Window_As_The_Cross_Root()
    {
        RunOnUIThread(() =>
        {
            var placementTarget = CreatePlacementTarget();
            var dialog          = CreateDialog(placementTarget);
            dialog.DialogHostType = AtomUI.Desktop.Controls.DialogHostType.Window;
            dialog.IsModal       = false;
            var window = CreateHost(placementTarget, dialog);
            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                dialog.IsOpen = true;
                PumpUntil(() => dialog.GetCrossRoots().Count == 1);

                // Window 宿主的跨根是独立 TopLevel 的原生窗口（先例：ImagePreviewer native dialog）。
                dialog.GetCrossRoots()[0].ShouldBeAssignableTo<Avalonia.Controls.Window>();

                dialog.IsOpen = false;
                PumpUntil(() => dialog.GetCrossRoots().Count == 0);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Window_Host_Owner_Scoped_Semantic_Styles_Cascade_Via_Logical_Parent()
    {
        RunOnUIThread(() =>
        {
            var registry = Application.Current.ShouldNotBeNull()
                                      .GetThemeManager().ShouldNotBeNull()
                                      .SemanticParts;
            registry.TryGetControl(typeof(AtomUIDialog), out var descriptor).ShouldBeTrue();
            descriptor.ShouldNotBeNull();

            var placementTarget = CreatePlacementTarget();
            var dialog          = CreateDialog(placementTarget);
            dialog.DialogHostType = AtomUI.Desktop.Controls.DialogHostType.Window;
            dialog.IsModal        = false;
            dialog.HostWidth      = 360;
            dialog.HostHeight     = 220;

            dialog.Classes.Add("semantic-owner");
            var ownerStyle = new Avalonia.Styling.Style(
                selector => selector.OfType(typeof(AtomUIDialog)).Class("semantic-owner"));
            foreach (var part in descriptor.Parts.Where(static part => part.StyleType != null))
            {
                var partStyle = (Avalonia.Styling.Style)Activator
                    .CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
                partStyle.Setters.Add(new Avalonia.Styling.Setter(
                    Avalonia.Controls.Control.TagProperty, part.Name));
                ownerStyle.Children.Add(partStyle);
            }

            dialog.Styles.Add(ownerStyle);

            var window = CreateHost(placementTarget, dialog);
            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                dialog.IsOpen = true;
                PumpUntil(() => dialog.GetCrossRoots().Count == 1);

                var dialogWindow = (Avalonia.Controls.Window)dialog.GetCrossRoots()[0];
                Dispatcher.UIThread.RunJobs();
                dialogWindow.UpdateLayout();
                Dispatcher.UIThread.RunJobs();

                // 实证：DialogWindow 经 SetParent 挂在 owner 逻辑树下，owner 实例级 Styles
                // 是否随逻辑祖先链进入独立 TopLevel 的 Surface 子树。
                var tagged = dialogWindow.GetVisualDescendants()
                                         .OfType<Control>()
                                         .Where(static control => control.Tag is string)
                                         .ToArray();
                tagged.Count(control => Equals(control.Tag, "container"))
                    .ShouldBe(1, "owner 作用域生成样式应经逻辑父链命中 Window 宿主的 container");
            }
            finally
            {
                dialog.IsOpen = false;
                PumpUntil(() => dialog.GetCrossRoots().Count == 0);
                window.Close();
            }
        });
    }

    // 嵌套样式经生成 Part Style 命中部件内部元素（footer 内的按钮、body 内的内容文字）：
    // 用户在 DialogFooterStyle/DialogBodyStyle 内声明 `^ DialogButton` / `^ TextBlock` 嵌套 Style 即可
    // 定制按钮与内容文字，这是语义样式 demo 的能力基础。
    [Fact]
    public void Nested_Styles_Inside_Part_Styles_Reach_Footer_Buttons_And_Body_Text()
    {
        RunOnUIThread(() =>
        {
            var registry = Application.Current.ShouldNotBeNull()
                                      .GetThemeManager().ShouldNotBeNull()
                                      .SemanticParts;
        registry.TryGetControl(typeof(AtomUIDialog), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

            var placementTarget = CreatePlacementTarget();
            var dialog = new AtomUIDialog
            {
                PlacementTarget = placementTarget,
                Title           = "Title",
                // 与 Gallery demo 一致使用 atom:TextBlock 作为内容文字。
                Content         = new AtomUI.Desktop.Controls.TextBlock { Text = "Body" },
                IsMotionEnabled = false,
                HostWidth       = 320,
                HostHeight      = 180
            };
            dialog.StandardButtons = AtomUI.Desktop.Controls.DialogStandardButton.Cancel |
                                     AtomUI.Desktop.Controls.DialogStandardButton.Ok;
            var window = CreateHost(placementTarget, dialog);

            dialog.Classes.Add("semantic-owner");
            var ownerStyle = new Avalonia.Styling.Style(
                selector => selector.OfType(typeof(AtomUIDialog)).Class("semantic-owner"));

            var footerStyle = (Avalonia.Styling.Style)Activator
                .CreateInstance(descriptor.Parts.Single(p => p.Name == "footer").StyleType!)!;
            // DialogButton 的 StyleKeyOverride 是 AtomUI Button（OfType 按 StyleKey 精确匹配），
            // 因此按钮要经 atom|Button 选择；正文文字是 atom:TextBlock。
            var footerButtonStyle = new Avalonia.Styling.Style(
                selector => selector.Nesting().Descendant().OfType(typeof(AtomUI.Desktop.Controls.Button)));
            footerButtonStyle.Setters.Add(new Avalonia.Styling.Setter(
                Avalonia.Controls.Control.TagProperty, "footer-button"));
            // 鉴别：先直接挂 ownerStyle（一级嵌套）；若命中再测挂 footerStyle（二级嵌套）。
            ownerStyle.Children.Add(footerButtonStyle);
            ownerStyle.Children.Add(footerStyle);

            var bodyStyle = (Avalonia.Styling.Style)Activator
                .CreateInstance(descriptor.Parts.Single(p => p.Name == "body").StyleType!)!;
            var bodyTextStyle = new Avalonia.Styling.Style(
                selector => selector.Nesting().Descendant().OfType(typeof(AtomUI.Desktop.Controls.TextBlock)));
            bodyTextStyle.Setters.Add(new Avalonia.Styling.Setter(
                Avalonia.Controls.Control.TagProperty, "body-text"));
            // 二级嵌套（生成 Part Style 内再嵌套）不生效：嵌套样式必须直接挂外层 owner Style。
            ownerStyle.Children.Add(bodyTextStyle);
            ownerStyle.Children.Add(bodyStyle);

            dialog.Styles.Add(ownerStyle);

            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                dialog.IsOpen = true;
                PumpUntil(() => window.GetVisualDescendants().OfType<Control>()
                                      .Any(static c => Equals(c.Tag, "container")) ||
                                      window.GetVisualDescendants().OfType<OverlayDialogPresenter>().Any());

                var buttons = window.GetVisualDescendants()
                                    .OfType<AtomUI.Desktop.Controls.DialogButton>()
                                    .ToArray();
                buttons.Length.ShouldBeGreaterThan(1, "标准按钮序列应已生成");
                buttons.Count(b => Equals(b.Tag, "footer-button"))
                    .ShouldBe(buttons.Length, "footer 嵌套样式应命中全部 footer 按钮");

                var bodyText = window.GetVisualDescendants()
                                     .OfType<Avalonia.Controls.TextBlock>()
                                     .Where(t => t.Text is not null)
                                     .ToArray();
                bodyText.Count(t => Equals(t.Tag, "body-text"))
                    .ShouldBeGreaterThanOrEqualTo(1, "body 嵌套样式应命中内容文字");
            }
            finally
            {
                CloseWindow(window, dialog);
            }
        });
    }

    // 真机缺陷回归（2026-09-13）：Window 宿主里 owner 实例级语义样式必须能真正改写
    // container 背景、正文文字与 footer 按钮的属性值，而不是仅命中路由节点。
    // 缺陷链：TopLevel 的 IStyleHost.StylingParent 固定指向 Application（Avalonia 的 PopupRoot
    // 用 `IStyleHost.StylingParent => Parent` 解决弹层同源问题），窗口模板应用时
    // ContentPresenter 还会改写 surface 的继承父，导致窗口模板之后才挂载的内容文字与
    // footer 按钮永远走不到 owner 的样式宿主链；container 背景则因主题写成模板局部值，
    // 局部值优先级压过任何 Style setter。三个断言分别锁定这三条修复。
    [Fact]
    public void Window_Host_Owner_Instance_Styles_Restyle_Container_Content_And_Footer_Buttons()
    {
        RunOnUIThread(() =>
        {
            var registry = Application.Current.ShouldNotBeNull()
                                      .GetThemeManager().ShouldNotBeNull()
                                      .SemanticParts;
            registry.TryGetControl(typeof(AtomUIDialog), out var descriptor).ShouldBeTrue();
            descriptor.ShouldNotBeNull();

            var pink    = new SolidColorBrush(Color.FromRgb(0xff, 0xf0, 0xf6));
            var magenta = new SolidColorBrush(Color.FromRgb(0xeb, 0x2f, 0x96));
            var green   = new SolidColorBrush(Color.FromRgb(0x52, 0xc4, 0x1a));

            var placementTarget = CreatePlacementTarget();
            var dialog = new AtomUIDialog
            {
                PlacementTarget = placementTarget,
                Title = "Title",
                Content = new StackPanel
                {
                    Children = { new AtomUI.Desktop.Controls.TextBlock { Text = "Body" } }
                },
                IsMotionEnabled = false,
                IsModal = false,
                DialogHostType = AtomUI.Desktop.Controls.DialogHostType.Window,
                StandardButtons = AtomUI.Desktop.Controls.DialogStandardButton.Cancel |
                                  AtomUI.Desktop.Controls.DialogStandardButton.Ok,
                HostWidth = 360,
                HostHeight = 220
            };

            dialog.Classes.Add("semantic-owner");
            var ownerStyle = new Avalonia.Styling.Style(
                selector => selector.OfType(typeof(AtomUIDialog)).Class("semantic-owner"));

            var containerStyle = (Avalonia.Styling.Style)Activator
                .CreateInstance(descriptor.Parts.Single(p => p.Name == "container").StyleType!)!;
            containerStyle.Setters.Add(new Avalonia.Styling.Setter(Border.BackgroundProperty, pink));
            var footerStyle = (Avalonia.Styling.Style)Activator
                .CreateInstance(descriptor.Parts.Single(p => p.Name == "footer").StyleType!)!;
            footerStyle.Setters.Add(new Avalonia.Styling.Setter(Border.BackgroundProperty, pink));
            ownerStyle.Children.Add(containerStyle);
            ownerStyle.Children.Add(footerStyle);

            // 与 Gallery demo 相同：正文文字与 footer 按钮经 owner 一级嵌套样式定制。
            var bodyTextStyle = new Avalonia.Styling.Style(
                selector => selector.Nesting().Descendant().OfType(typeof(AtomUI.Desktop.Controls.TextBlock)));
            bodyTextStyle.Setters.Add(new Avalonia.Styling.Setter(
                Avalonia.Controls.Control.TagProperty, "body-text"));
            bodyTextStyle.Setters.Add(new Avalonia.Styling.Setter(
                Avalonia.Controls.TextBlock.ForegroundProperty, magenta));
            bodyTextStyle.Setters.Add(new Avalonia.Styling.Setter(
                Avalonia.Controls.TextBlock.FontWeightProperty, FontWeight.Bold));
            ownerStyle.Children.Add(bodyTextStyle);

            var footerButtonStyle = new Avalonia.Styling.Style(
                selector => selector.Nesting().Descendant().OfType(typeof(AtomUI.Desktop.Controls.Button)));
            footerButtonStyle.Setters.Add(new Avalonia.Styling.Setter(
                Avalonia.Controls.Control.TagProperty, "footer-button"));
            footerButtonStyle.Setters.Add(new Avalonia.Styling.Setter(
                Border.BackgroundProperty, green));
            ownerStyle.Children.Add(footerButtonStyle);

            dialog.Styles.Add(ownerStyle);

            var window = CreateHost(placementTarget, dialog);
            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                dialog.IsOpen = true;
                PumpUntil(() => dialog.GetCrossRoots().Count == 1);

                var dialogWindow = (Avalonia.Controls.Window)dialog.GetCrossRoots()[0];
                Dispatcher.UIThread.RunJobs();
                dialogWindow.UpdateLayout();
                Dispatcher.UIThread.RunJobs();

                var container = dialogWindow.GetVisualDescendants()
                                            .OfType<Border>()
                                            .Single(b => b.Classes.Contains("semantic-container"));
                ((ISolidColorBrush?)container.Background)?.Color.ShouldBe(pink.Color);

                var bodyText = dialogWindow.GetVisualDescendants()
                                           .OfType<AtomUI.Desktop.Controls.TextBlock>()
                                           .Single(t => t.Text == "Body");
                bodyText.Tag.ShouldBe("body-text");
                ((ISolidColorBrush?)bodyText.Foreground)?.Color.ShouldBe(magenta.Color);
                bodyText.FontWeight.ShouldBe(FontWeight.Bold);

                var footerButtons = dialogWindow.GetVisualDescendants()
                                                .OfType<AtomUI.Desktop.Controls.DialogButton>()
                                                .ToArray();
                footerButtons.Length.ShouldBe(2);
                foreach (var button in footerButtons)
                {
                    button.Tag.ShouldBe("footer-button");
                    ((ISolidColorBrush?)button.Background)?.Color.ShouldBe(green.Color);
                }
            }
            finally
            {
                dialog.IsOpen = false;
                PumpUntil(() => dialog.GetCrossRoots().Count == 0);
                window.Close();
            }
        });
    }

    private static void AssertGeneratedStylesHitEveryPart(Visual root, AtomUIDialog owner, Type ownerType)
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(ownerType, out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        owner.Classes.Add("semantic-owner");
        var ownerStyle = new Avalonia.Styling.Style(
            selector => selector.OfType(ownerType).Class("semantic-owner"));
        foreach (var part in descriptor.Parts.Where(static part => part.StyleType != null))
        {
            var partStyle = (Avalonia.Styling.Style)Activator
                .CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
            partStyle.Setters.Add(new Avalonia.Styling.Setter(
                Avalonia.Controls.Control.TagProperty, part.Name));
            ownerStyle.Children.Add(partStyle);
        }

        var partNames = descriptor.Parts
                                  .Select(static part => part.Name)
                                  .Where(static name => name != "root")
                                  .ToArray();
        owner.Styles.Add(ownerStyle);

        owner.IsOpen = true;
        PumpUntil(() => root.GetVisualDescendants().OfType<OverlayDialogPresenter>().Any());
        PumpUntil(() => root.GetVisualDescendants().OfType<Control>()
                              .Any(static control => Equals(control.Tag, "container")));

        var tagged = root.GetVisualDescendants()
                         .OfType<Control>()
                         .Where(static control => control.Tag is string)
                         .ToArray();

        foreach (var name in partNames)
        {
            tagged.Count(control => Equals(control.Tag, name)).ShouldBe(
                1,
                $"{ownerType.Name} part '{name}' must hit exactly one node; hits: " +
                string.Join(
                    " | ",
                    tagged.Where(control => Equals(control.Tag, name))
                          .Select(static control => $"{control.GetType().Name}#{control.Name}")));
        }
    }

    [Fact]
    public void Pinned_Dialog_Ignores_User_Initiated_Close_But_Not_External_Close()
    {
        RunOnUIThread(() =>
        {
            var background = new Border
            {
                Width            = 640,
                Height           = 480,
                Background       = Brushes.Transparent,
                IsHitTestVisible = true
            };
            var root = new ScopeAwareOverlayLayerPanel
            {
                Children = { background }
            };
            var window = new AtomUI.Desktop.Controls.Window
            {
                Width   = 640,
                Height  = 480,
                Content = root
            };
            var dialog = new AtomUIDialog
            {
                IsModal         = true,
                IsMotionEnabled = false,
                HostWidth       = 320,
                HostHeight      = 180
            };
            var presenter = new OverlayDialogPresenter(dialog, background);
            var closeRequestCount = 0;
            presenter.CloseRequested += (_, _) => closeRequestCount++;

            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                WaitWithDispatcherPump(presenter.ShowAsync(CancellationToken.None).AsTask());
                Dispatcher.UIThread.RunJobs();

                dialog.IsPinnedOpen = true;

                var closeButton = presenter.GetVisualDescendants()
                                           .OfType<DialogCaptionButton>()
                                           .Single(static b => b.Name == "PART_CloseButton");
                ControlAutomationPeer.CreatePeerForElement(closeButton)
                                     .ShouldBeAssignableTo<IInvokeProvider>()
                                     .Invoke();
                Dispatcher.UIThread.RunJobs();
                closeRequestCount.ShouldBe(0, "钉住时关闭按钮不得发起关闭请求");

                var mask = presenter.GetVisualDescendants().OfType<OverlayDialogMask>().Single();
                RaisePointerPressed(mask);
                Dispatcher.UIThread.RunJobs();
                closeRequestCount.ShouldBe(0, "钉住时遮罩外点不得发起关闭请求");

                dialog.IsPinnedOpen = false;
                RaisePointerPressed(mask);
                Dispatcher.UIThread.RunJobs();
                closeRequestCount.ShouldBe(1, "取消钉住后遮罩外点必须恢复关闭请求");

                // 外部关闭（IsOpen=false 对应的 presenter 侧路径）不受钉住影响。
                dialog.IsPinnedOpen = true;
                WaitWithDispatcherPump(presenter.CloseAsync().AsTask());
                presenter.GetVisualParent().ShouldBeNull("外部关闭必须仍能卸载 presenter");
            }
            finally
            {
                WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
                window.Close();
            }
        });
    }

    [Fact]
    public void Modeless_Overlay_Does_Not_Materialize_The_Mask()
    {
        RunOnUIThread(() =>
        {
            var placementTarget = CreatePlacementTarget();
            var dialog          = CreateDialog(placementTarget);
            dialog.IsModal = false;
            var window = CreateHost(placementTarget, dialog);
            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                dialog.IsOpen = true;
                PumpUntil(() => window.GetVisualDescendants().OfType<OverlayDialogPresenter>().Any());

                var presenter = window.GetVisualDescendants().OfType<OverlayDialogPresenter>().Single();
                presenter.GetVisualDescendants().OfType<MotionActor>()
                         .Any(static m => m.Name == "PART_MaskMotionActor")
                         .ShouldBeTrue("遮罩宿主仍然存在");
                presenter.GetVisualDescendants().OfType<OverlayDialogMask>()
                         .ShouldBeEmpty("modeless 不物化遮罩节点：mask 的 Optional 同时覆盖 modeless 与 Window 宿主");
            }
            finally
            {
                CloseWindow(window, dialog);
            }
        });
    }

    [Fact]
    public void Hidden_Footer_And_Close_Keep_Their_Markers()
    {
        RunOnUIThread(() =>
        {
            var placementTarget = CreatePlacementTarget();
            var dialog          = CreateDialog(placementTarget);
            dialog.IsFooterVisible = false;
            dialog.IsClosable      = false;
            var window = CreateHost(placementTarget, dialog);
            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                dialog.IsOpen = true;
                PumpUntil(() => window.GetVisualDescendants().OfType<OverlayDialogPresenter>().Any());

                var surface     = window.GetVisualDescendants().OfType<DialogSurface>().Single();
                var footerFrame = surface.GetVisualDescendants().OfType<Border>()
                                         .Single(static b => b.Name == "FooterFrame");
                footerFrame.Classes.ShouldContain("semantic-footer");
                footerFrame.IsVisible.ShouldBeFalse("IsFooterVisible=false 只隐藏节点，Cardinality 保持 Single");

                var closeButton = surface.GetVisualDescendants().OfType<DialogCaptionButton>()
                                         .Single(static b => b.Name == "PART_CloseButton");
                closeButton.Classes.ShouldContain("semantic-close");
                closeButton.IsVisible.ShouldBeFalse("IsClosable=false 只隐藏节点，Cardinality 保持 Single");
            }
            finally
            {
                CloseWindow(window, dialog);
            }
        });
    }

    [Fact]
    public void MessageBox_Overlay_Exposes_The_Same_Part_Markers()
    {
        RunOnUIThread(() =>
        {
            var placementTarget = CreatePlacementTarget();
            var messageBox = new AtomUIMessageBox
            {
                PlacementTarget = placementTarget,
                Title           = "Title",
                IsMotionEnabled = false,
                HostWidth       = 320,
                HostHeight      = 180
            };
            var window = CreateHost(placementTarget, messageBox);
            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                messageBox.IsOpen = true;
                PumpUntil(() => window.GetVisualDescendants().OfType<OverlayDialogPresenter>().Any());

                AssertAllPartMarkers(window);
            }
            finally
            {
                CloseWindow(window, messageBox);
            }
        });
    }

    private static void AssertAllPartMarkers(Visual root)
    {
        var presenter = root.GetVisualDescendants().OfType<OverlayDialogPresenter>().Single();
        var surface   = presenter.GetVisualDescendants().OfType<DialogSurface>().Single();
        var header    = surface.GetVisualDescendants().OfType<OverlayDialogHeader>().Single();
        var mask      = presenter.GetVisualDescendants().OfType<OverlayDialogMask>().Single();

        AssertMarker(
            presenter.GetVisualDescendants().OfType<MotionActor>()
                     .Single(static m => m.Name == "PART_SurfaceMotionActor"),
            "semantic-wrapper");
        AssertMarker(
            mask.GetVisualDescendants().OfType<Border>().Single(static b => b.Name == "Frame"),
            "semantic-mask");
        AssertMarker(
            surface.GetVisualDescendants().OfType<ShadowsAwareContainer>().Single()
                   .GetVisualChildren().OfType<Border>().Single(),
            "semantic-container");
        AssertMarker(
            surface.GetVisualDescendants().OfType<Border>().Single(static b => b.Name == "ContentFrame"),
            "semantic-body");
        AssertMarker(
            surface.GetVisualDescendants().OfType<Border>().Single(static b => b.Name == "FooterFrame"),
            "semantic-footer");
        AssertMarker(
            header.GetVisualDescendants().OfType<Border>().Single(static b => b.Name == "HeaderFrame"),
            "semantic-header");
        AssertMarker(
            header.GetVisualDescendants().OfType<Avalonia.Controls.TextBlock>()
                  .Single(static t => t.Name == "Title"),
            "semantic-title");
        AssertMarker(
            header.GetVisualDescendants().OfType<DialogCaptionButton>()
                  .Single(static b => b.Name == "PART_CloseButton"),
            "semantic-close");
    }

    [Fact]
    public void OverlayScope_Contains_The_Host_And_Mask_Within_The_Scope()
    {
        RunOnUIThread(() =>
        {
            var placementTarget = CreatePlacementTarget();
            var scopeHost = new ScopeAwareOverlayLayerPanel { Name = "ScopeHost" };
            var stage = new Border
            {
                Width  = 300,
                Height = 200,
                Child  = scopeHost
            };
            var dialog = new AtomUIDialog
            {
                PlacementTarget = placementTarget,
                OverlayScope    = scopeHost,
                Title           = "Title",
                Content         = new TextBlock { Text = "Body" },
                IsMotionEnabled = false,
                HostWidth       = 200,
                HostHeight      = 120
            };
            var root = new ScopeAwareOverlayLayerPanel
            {
                Children = { placementTarget, stage, dialog }
            };
            var window = new AtomUI.Desktop.Controls.Window { Width = 640, Height = 480, Content = root };
            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                dialog.IsOpen = true;
                PumpUntil(() => dialog.GetCrossRoots().Count == 1);

                var presenter = dialog.GetCrossRoots()[0].ShouldBeOfType<OverlayDialogPresenter>();
                var layer     = presenter.GetVisualParent().ShouldBeOfType<DialogOverlayLayer>();
                var scopeLayer = layer.GetVisualParent().ShouldBeOfType<ScopeAwareOverlayLayer>();
                scopeLayer.GetVisualParent().ShouldBeSameAs(scopeHost);

                var maskActor = presenter.GetVisualDescendants().OfType<MotionActor>()
                                         .Single(static m => m.Name == "PART_MaskMotionActor");
                PumpUntil(() => maskActor.Bounds.Width > 0);
                maskActor.Bounds.Width.ShouldBe(300, 0.5);
                maskActor.Bounds.Height.ShouldBe(200, 0.5);
                maskActor.Bounds.Width.ShouldBeLessThan(window.Width);
            }
            finally
            {
                CloseWindow(window, dialog);
            }
        });
    }

    [Fact]
    public void Without_OverlayScope_The_Host_Stays_In_The_TopLevel_Overlay_Layer()
    {
        RunOnUIThread(() =>
        {
            var placementTarget = CreatePlacementTarget();
            var dialog          = CreateDialog(placementTarget);
            var window          = CreateHost(placementTarget, dialog);
            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                dialog.IsOpen = true;
                PumpUntil(() => window.GetVisualDescendants().OfType<OverlayDialogPresenter>().Any());

                var presenter = window.GetVisualDescendants().OfType<OverlayDialogPresenter>().Single();
                var layer     = presenter.GetVisualParent().ShouldBeOfType<DialogOverlayLayer>();
                layer.GetVisualParent().ShouldBeOfType<OverlayLayer>();
            }
            finally
            {
                CloseWindow(window, dialog);
            }
        });
    }

    private static Border CreatePlacementTarget()
    {
        return new Border { Width = 100, Height = 40 };
    }

    // Dialog 必须与 placement target 同处一棵可视树：owner 作用域生成的 Semantic Style 需要 owner 元素
    // 命中外层 Style selector 才能激活（Dialog 自身是零尺寸状态持有控件，不参与布局）。
    private static AtomUI.Desktop.Controls.Window CreateHost(Border placementTarget, Control dialog)
    {
        var root = new ScopeAwareOverlayLayerPanel
        {
            Children = { placementTarget, dialog }
        };
        return new AtomUI.Desktop.Controls.Window
        {
            Width = 640,
            Height = 480,
            Content = root
        };
    }

    private static AtomUIDialog CreateDialog(Border placementTarget)
    {
        return new AtomUIDialog
        {
            PlacementTarget = placementTarget,
            Title = "Title",
            Content = new TextBlock { Text = "Body" },
            IsMotionEnabled = false,
            HostWidth = 320,
            HostHeight = 180
        };
    }

    private static void CloseWindow(AtomUI.Desktop.Controls.Window window, AtomUIDialog dialog)
    {
        dialog.IsOpen = false;
        PumpUntil(() => !window.GetVisualDescendants().OfType<OverlayDialogPresenter>().Any());
        window.Close();
    }

    private static void WaitWithDispatcherPump(Task task)
    {
        var timeoutAt = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(5);
        while (!task.IsCompleted && DateTimeOffset.UtcNow < timeoutAt)
        {
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(1);
        }

        task.IsCompleted.ShouldBeTrue();
        task.GetAwaiter().GetResult();
    }

    private static void RaisePointerPressed(InputElement source)
    {
        source.RaiseEvent(new PointerPressedEventArgs(
            source,
            new Pointer(Pointer.GetNextFreeId(), PointerType.Mouse, true),
            source,
            default,
            0,
            new PointerPointProperties(
                RawInputModifiers.LeftMouseButton,
                PointerUpdateKind.LeftButtonPressed),
            KeyModifiers.None));
    }

    private static void AssertMarker(Control node, string marker)
    {
        node.Classes.ShouldContain(marker);
    }

    private static void PumpUntil(Func<bool> condition)
    {
        var timeoutAt = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(5);
        while (!condition() && DateTimeOffset.UtcNow < timeoutAt)
        {
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(1);
        }

        condition().ShouldBeTrue();
    }

    private static void RunOnUIThread(Action action)
    {
        Dispatcher.UIThread.Invoke(action);
    }

    private static string[] CollectMarkers(XDocument document)
    {
        return document.Descendants()
                       .SelectMany(static element => element.Attributes()
                           .Where(static attribute => attribute.Name.LocalName.StartsWith(
                               "Classes.semantic-", StringComparison.Ordinal)))
                       .Select(static attribute =>
                           $"{attribute.Name.LocalName["Classes.".Length..]}:{attribute.Parent!.Name.LocalName}")
                       .OrderBy(static marker => marker, StringComparer.Ordinal)
                       .ToArray();
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
    }

    private static void AssertPart(
        ControlSemanticDescriptor descriptor,
        string ownerPrefix,
        string name,
        string selectorClass,
        Type contractType,
        string selectorRoute,
        SemanticPartCardinality cardinality = SemanticPartCardinality.Single)
    {
        var part = descriptor.Parts.Single(candidate => candidate.Name == name);
        part.Path.ShouldBe(name);
        part.SelectorClass.ShouldBe(selectorClass);
        part.SelectorRoute.ShouldBe(selectorRoute);
        part.ContractType.ShouldBe(contractType);
        part.Cardinality.ShouldBe(cardinality);
        part.Customization.ShouldBe(SemanticPartCustomization.Selector);
        part.Since.ShouldBe("6.2.0");
        part.StyleType.ShouldNotBeNull();
        part.StyleType.Name.ShouldBe($"{ownerPrefix}{Pascal(name)}Style");
    }

    private static string Pascal(string partName)
    {
        return char.ToUpperInvariant(partName[0]) + partName[1..];
    }
}
