using AtomUI.Theme;
using AtomUI.Theme.Schema;
using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AtomUIButton = AtomUI.Desktop.Controls.Button;
using AtomUIAutoComplete = AtomUI.Desktop.Controls.AutoComplete;

namespace AtomUI.Toolkits.GalleryBase.Tests.Controls;

public class SemanticPartTargetResolverTests
{
    public SemanticPartTargetResolverTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Resolves_Button_Root_Icon_And_Content_Within_The_Owner_Template()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(AtomUIButton), out var descriptor).ShouldBeTrue();
        var button = new AtomUIButton
        {
            Content   = "Semantic Button",
            IsLoading = true
        };

        ShowInWindow(button, () =>
        {
            var root = Resolve(button, descriptor, "root", registry);
            root.TotalMatchCount.ShouldBe(1);
            root.Targets.ShouldBe([button]);

            var icon = Resolve(button, descriptor, "icon", registry);
            icon.TotalMatchCount.ShouldBe(1);
            icon.Targets.Single().Classes.ShouldContain("semantic-icon");
            icon.Targets.Single().TemplatedParent.ShouldBe(button);

            var content = Resolve(button, descriptor, "content", registry);
            content.TotalMatchCount.ShouldBe(1);
            content.Targets.Single().ShouldBeOfType<ContentPresenter>();
            content.Targets.Single().TemplatedParent.ShouldBe(button);
        });
    }

    [Fact]
    public void Static_Resolution_Does_Not_Enter_A_Nested_Semantic_Control_Template()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(AtomUIButton), out var descriptor).ShouldBeTrue();
        var nestedButton = new AtomUIButton
        {
            Content   = "Nested",
            IsLoading = true
        };
        var owner = new AtomUIButton
        {
            Content   = nestedButton,
            IsLoading = true
        };

        ShowInWindow(owner, () =>
        {
            var result = Resolve(owner, descriptor, "icon", registry);

            result.TotalMatchCount.ShouldBe(1);
            result.Targets.Single().TemplatedParent.ShouldBe(owner);
        });
    }

    [Fact]
    public void Resolves_AutoComplete_Cross_Nested_Owner_Parts_Inside_The_Embedded_Input_Template()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(AtomUIAutoComplete), out var descriptor).ShouldBeTrue();
        var autoComplete = new AtomUIAutoComplete
        {
            Width           = 320,
            PlaceholderText = "input here",
            IsMotionEnabled = false
        };

        ShowInWindow(autoComplete, () =>
        {
            // content 锚定在内嵌输入控件（LineEdit）模板的值区域，属跨嵌套 owner 部件。
            var content = Resolve(autoComplete, descriptor, "content", registry);
            content.TotalMatchCount.ShouldBe(1);
            content.Targets.Single().Classes.ShouldContain("semantic-content");

            var placeholder = Resolve(autoComplete, descriptor, "placeholder", registry);
            placeholder.TotalMatchCount.ShouldBe(1);
            placeholder.Targets.Single().ShouldBeAssignableTo<TextBlock>();
            placeholder.Targets.Single().Classes.ShouldContain("semantic-placeholder");
        });
    }

    [Fact]
    public void Resolves_AutoComplete_Popup_Parts_When_DropDown_Is_Forced_Open()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(AtomUIAutoComplete), out var descriptor).ShouldBeTrue();

        // headless 平台无法创建真实弹层宿主，这里在独立可视根中模拟弹层内容树，
        // 验证 popup.* 部件经 CrossVisualRoot + additionalRoots 的解析路径；
        // 真实弹层打开（IsDropDownOpen 强制打开）由 Gallery 真机走查覆盖。
        var autoComplete = new AtomUIAutoComplete
        {
            Width               = 320,
            IsMotionEnabled     = false,
            MinimumPrefixLength = 0
        };
        var popupRootFrame = new Border
        {
            Width   = 200,
            Height  = 80,
            Classes = { "semantic-popup-root" }
        };
        var host = new Grid
        {
            Children = { autoComplete, popupRootFrame }
        };

        ShowInWindow(host, () =>
        {
            var popupRoot = Resolve(autoComplete, descriptor, "popup.root", registry, popupRootFrame);
            popupRoot.TotalMatchCount.ShouldBe(1);
            popupRoot.Targets.Single().ShouldBeOfType<Border>();
            popupRoot.Targets.Single().Classes.ShouldContain("semantic-popup-root");
        });
    }

    [Fact]
    public void Resolves_AutoComplete_Prefix_Part_From_The_Embedded_Input_Template()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(AtomUIAutoComplete), out var descriptor).ShouldBeTrue();
        var autoComplete = new AtomUIAutoComplete
        {
            Width            = 320,
            ContentLeftAddOn = "$",
            IsMotionEnabled  = false
        };

        ShowInWindow(autoComplete, () =>
        {
            var prefix = Resolve(autoComplete, descriptor, "prefix", registry);
            prefix.TotalMatchCount.ShouldBe(1);
            prefix.Targets.Single().ShouldBeAssignableTo<ContentPresenter>();
            prefix.Targets.Single().Classes.ShouldContain("semantic-prefix");
        });
    }

    [Fact]
    public void Runtime_Created_Resolution_Stops_Before_Another_Registered_Owner()
    {
        var directTarget = Marker();
        var nestedTarget = Marker();
        var nestedOwner = new StackPanel
        {
            Width    = 80,
            Height   = 40,
            Children = { nestedTarget }
        };
        var owner = new Grid
        {
            Width    = 180,
            Height   = 100,
            Children = { directTarget, nestedOwner }
        };
        var ownerDescriptor = RuntimeDescriptor(typeof(Grid), "RuntimeGrid");
        var registry = new SemanticPartRegistry([
            ownerDescriptor,
            RootOnlyDescriptor(typeof(StackPanel), "NestedStackPanel")
        ]);

        ShowInWindow(owner, () =>
        {
            var result = Resolve(owner, ownerDescriptor, "item", registry);

            result.TotalMatchCount.ShouldBe(1);
            result.Targets.ShouldBe([directTarget]);
        });
    }

    [Fact]
    public void Runtime_Created_Resolution_Matches_A_Nested_Registered_Owner_Carrying_The_Part_Marker()
    {
        var nestedOwner = new StackPanel
        {
            Width  = 80,
            Height = 40
        };
        nestedOwner.Classes.Add("semantic-item");
        var owner = new Grid
        {
            Width    = 180,
            Height   = 100,
            Children = { nestedOwner }
        };
        var ownerDescriptor = RuntimeDescriptor(typeof(Grid), "RuntimeGrid");
        var registry = new SemanticPartRegistry([
            ownerDescriptor,
            RootOnlyDescriptor(typeof(StackPanel), "NestedStackPanel")
        ]);

        ShowInWindow(owner, () =>
        {
            var result = Resolve(owner, ownerDescriptor, "item", registry);

            result.TotalMatchCount.ShouldBe(1);
            result.Targets.ShouldBe([nestedOwner]);
        });
    }

    [Fact]
    public void Runtime_Created_Resolution_Matches_The_Owner_When_It_Carries_The_Part_Marker()
    {
        var owner = new Grid
        {
            Width  = 180,
            Height = 100
        };
        owner.Classes.Add("semantic-item");
        var ownerDescriptor = RuntimeDescriptor(typeof(Grid), "RuntimeGrid");
        var registry = new SemanticPartRegistry([ownerDescriptor]);

        ShowInWindow(owner, () =>
        {
            var result = Resolve(owner, ownerDescriptor, "item", registry);

            result.TotalMatchCount.ShouldBe(1);
            result.Targets.ShouldBe([owner]);
        });
    }

    [Fact]
    public void Runtime_Created_Resolution_Matches_The_Owner_And_Nested_Owner_When_Both_Carry_The_Marker()
    {
        var nestedOwner = new StackPanel
        {
            Width  = 80,
            Height = 40
        };
        nestedOwner.Classes.Add("semantic-item");
        var owner = new Grid
        {
            Width    = 180,
            Height   = 100,
            Children = { nestedOwner }
        };
        owner.Classes.Add("semantic-item");
        var ownerDescriptor = RuntimeDescriptor(typeof(Grid), "RuntimeGrid");
        var registry = new SemanticPartRegistry([
            ownerDescriptor,
            RootOnlyDescriptor(typeof(StackPanel), "NestedStackPanel")
        ]);

        ShowInWindow(owner, () =>
        {
            var result = Resolve(owner, ownerDescriptor, "item", registry);

            result.TotalMatchCount.ShouldBe(2);
            result.Targets.ShouldBe([owner, nestedOwner]);
        });
    }

    [Fact]
    public void Resolution_Reports_All_Visible_Matches_But_Returns_Only_The_Budget()
    {
        var owner = new Grid
        {
            Width  = 400,
            Height = 400
        };
        for (var index = 0; index < 40; index++)
        {
            owner.Children.Add(Marker());
        }

        var descriptor = RuntimeDescriptor(typeof(Grid), "BudgetGrid");
        var registry = new SemanticPartRegistry([descriptor]);

        ShowInWindow(owner, () =>
        {
            var part = descriptor.Parts.Single(static candidate => candidate.Path == "item");
            var result = SemanticPartTargetResolver.Resolve(owner, part, registry, targetBudget: 32);

            result.TotalMatchCount.ShouldBe(40);
            result.Targets.Count.ShouldBe(32);
            result.IsTruncated.ShouldBeTrue();
        });
    }

    [Fact]
    public void Resolves_Root_To_The_Popup_Root_Marker_When_The_Owner_Has_No_Locatable_Surface()
    {
        // 弹层承载型控件（如 Tour）：owner 布局尺寸为零，root 没有可定位的宿主表面，
        // 回退到 "popup.root" 部件的标记节点（弹层卡片根）。
        var owner = new Grid
        {
            Width  = 180,
            Height = 0
        };
        var popupRootFrame = new Border
        {
            Width   = 200,
            Height  = 80,
            Classes = { "semantic-popup-root" }
        };
        var host = new Grid
        {
            Children = { owner, popupRootFrame }
        };
        var descriptor = PopupCarrierDescriptor(typeof(Grid), "PopupCarrierGrid");
        var registry = new SemanticPartRegistry([descriptor]);

        ShowInWindow(host, () =>
        {
            var root = Resolve(owner, descriptor, "root", registry, popupRootFrame);

            root.TotalMatchCount.ShouldBe(1);
            root.Targets.Single().ShouldBe(popupRootFrame);
        });
    }

    [Fact]
    public void Resolves_Root_To_The_Owner_Itself_When_The_Owner_Is_Locatable()
    {
        var owner = new Grid
        {
            Width  = 180,
            Height = 100
        };
        var popupRootFrame = new Border
        {
            Width   = 200,
            Height  = 80,
            Classes = { "semantic-popup-root" }
        };
        var host = new Grid
        {
            Children = { owner, popupRootFrame }
        };
        var descriptor = PopupCarrierDescriptor(typeof(Grid), "PopupCarrierGrid");
        var registry = new SemanticPartRegistry([descriptor]);

        ShowInWindow(host, () =>
        {
            var root = Resolve(owner, descriptor, "root", registry, popupRootFrame);

            root.TotalMatchCount.ShouldBe(1);
            root.Targets.ShouldBe([owner]);
        });
    }

    private static SemanticPartTargetResolution Resolve(
        Control owner,
        ControlSemanticDescriptor descriptor,
        string path,
        SemanticPartRegistry registry,
        Visual? additionalRoot = null)
    {
        var part = descriptor.Parts.Single(candidate => candidate.Path == path);
        return SemanticPartTargetResolver.Resolve(
            owner,
            part,
            registry,
            additionalRoot is null ? null : [additionalRoot]);
    }

    private static Border Marker()
    {
        var target = new Border
        {
            Width  = 20,
            Height = 20
        };
        target.Classes.Add("semantic-item");
        return target;
    }

    private static ControlSemanticDescriptor RuntimeDescriptor(Type controlType, string id)
    {
        return new ControlSemanticDescriptor(
            controlType,
            new ControlTokenIdentity("GalleryTests", id),
            [
                Root(controlType),
                new SemanticPartDescriptor(
                    "item",
                    "item",
                    "semantic-item",
                    typeof(Control),
                    SemanticPartCardinality.Multiple,
                    SemanticPartCustomization.Selector,
                    null,
                    false,
                    null,
                    true,
                    "> .semantic-item")
            ]);
    }

    private static ControlSemanticDescriptor RootOnlyDescriptor(Type controlType, string id)
    {
        return new ControlSemanticDescriptor(
            controlType,
            new ControlTokenIdentity("GalleryTests", id),
            [Root(controlType)]);
    }

    /// <summary>
    /// 弹层承载型控件的 descriptor 形态：root 之外声明 "popup.root" 部件，
    /// 供 root 回退解析（owner 布局尺寸为零时定位弹层根）测试复用。
    /// </summary>
    private static ControlSemanticDescriptor PopupCarrierDescriptor(Type controlType, string id)
    {
        return new ControlSemanticDescriptor(
            controlType,
            new ControlTokenIdentity("GalleryTests", id),
            [
                Root(controlType),
                new SemanticPartDescriptor(
                    "popup.root",
                    "popup.root",
                    "semantic-popup-root",
                    typeof(Control),
                    SemanticPartCardinality.Single,
                    SemanticPartCustomization.Selector,
                    null,
                    true,
                    null,
                    true,
                    "/template/ .semantic-popup-root")
            ]);
    }

    private static SemanticPartDescriptor Root(Type controlType)
    {
        return new SemanticPartDescriptor(
            "root",
            "root",
            null,
            controlType,
            SemanticPartCardinality.Single,
            SemanticPartCustomization.Root,
            null,
            false,
            null,
            false);
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new Window
        {
            Width   = 500,
            Height  = 500,
            Content = content
        };

        try
        {
            window.Show();
            content.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();
            assertion();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
