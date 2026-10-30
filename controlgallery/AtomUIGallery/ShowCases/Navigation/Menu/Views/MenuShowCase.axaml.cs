using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using AtomUI.Controls.Primitives;
using AtomUI.Desktop.Controls;
using AtomUI.Icons.AntDesign;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaPopup = Avalonia.Controls.Primitives.Popup;

namespace AtomUIGallery.ShowCases.Menu;

public partial class MenuShowCase : GalleryReactiveUserControl<MenuViewModel>
{
    public const string LanguageId = nameof(MenuShowCase);

    private NavMenuNode? _navMenuDefaultSelectedItem;
    private readonly List<AvaloniaPopup> _trackedSemanticPreviewPopups = [];
    // 承载弹层部件的语义预览（Vertical NavMenu 与普通 Menu）：弹层内容跨视觉根，需要显式注册根。
    private readonly List<SemanticPartPreview> _popupHostPreviews = [];

    public MenuShowCase()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            RefreshCurrentViewModelData();

            var languageManager = GalleryLocalization.GetLanguageManager();
            if (languageManager != null)
            {
                EventHandler<LanguageChangedEventArgs> handler = (_, _) => RefreshCurrentViewModelData();
                languageManager.LanguageChanged += handler;
                Disposable.Create(() => languageManager.LanguageChanged -= handler)
                          .DisposeWith(disposables);
            }

            Disposable.Create(ClearCurrentViewModelData).DisposeWith(disposables);
        });
    }

    // 语义预览的 NavMenu 子菜单弹层由菜单项模板创建、跨视觉根渲染。TemplateExtensions.GetTemplateDescendants
    // 只沿着 TemplatedParent 为 owner 的模板后代下钻，遇到运行时生成的菜单项容器（TemplatedParent 为 null）
    // 就停止，因此高亮会话无法自动发现该 Popup；这里按仓库既定的 AdditionalRoots 模式（同 DropdownButton /
    // InfoFlyout）把已打开弹层的 Child 显式注册给 Preview。
    //
    // 弹层是否保持打开由 NavMenu 的公开钉住属性负责（预览在 AXAML 里声明 IsPopupPinnedOpen="True"），
    // 该属性会让 NavMenu 打开并钉住首个可展开项、同时拦截普通关闭；这里只负责在弹层打开后把弹层根
    // 注册给预览，并在弹层因生命周期原因（如预览滚出可视区）关闭后重新注册。
    private void HandleVerticalSemanticPreviewLoaded(object? sender, RoutedEventArgs args)
    {
        TrackPopupHostPreview(sender);
    }

    private void HandleMenuSemanticPreviewLoaded(object? sender, RoutedEventArgs args)
    {
        TrackPopupHostPreview(sender);
    }

    private void TrackPopupHostPreview(object? sender)
    {
        if (sender is not SemanticPartPreview preview)
        {
            return;
        }

        if (!_popupHostPreviews.Contains(preview))
        {
            _popupHostPreviews.Add(preview);
        }
        // 弹层由 NavMenu 在容器生成后按钉住请求异步打开，打开时机晚于本回调，且 overlay 模式下
        // 弹层内容挂在 overlay 层，无法从 owner 子树稳定推断。这里跟随布局在弹层真正打开后补注册
        // 附加根；回调只做幂等注册、不写控件状态，因此不会与打开/关闭形成循环。
        preview.LayoutUpdated += HandleVerticalSemanticPreviewLayoutUpdated;
        RegisterSemanticPreviewPopupRoots();
    }

    private void HandleVerticalSemanticPreviewUnloaded(object? sender, RoutedEventArgs args)
    {
        UntrackPopupHostPreview(sender);
    }

    private void HandleMenuSemanticPreviewUnloaded(object? sender, RoutedEventArgs args)
    {
        UntrackPopupHostPreview(sender);
    }

    private void UntrackPopupHostPreview(object? sender)
    {
        if (sender is not SemanticPartPreview preview)
        {
            return;
        }

        preview.LayoutUpdated -= HandleVerticalSemanticPreviewLayoutUpdated;
        preview.AdditionalRoots.Clear();
        _popupHostPreviews.Remove(preview);
        if (_popupHostPreviews.Count > 0)
        {
            return;
        }

        foreach (var popup in _trackedSemanticPreviewPopups)
        {
            popup.Opened -= HandleSemanticPreviewPopupOpened;
            popup.Closed -= HandleSemanticPreviewPopupClosed;
        }

        _trackedSemanticPreviewPopups.Clear();
    }

    private void HandleVerticalSemanticPreviewLayoutUpdated(object? sender, EventArgs args)
    {
        RegisterSemanticPreviewPopupRoots();
    }

    private void HandleSemanticPreviewPopupOpened(object? sender, EventArgs args)
    {
        RegisterSemanticPreviewPopupRoots();
    }

    // 预览滚出可视区时 Popup 会按放置目标有效性规则做生命周期关闭，滚动回来后再重新注册弹层根。
    private void HandleSemanticPreviewPopupClosed(object? sender, EventArgs args)
    {
        Dispatcher.UIThread.Post(RegisterSemanticPreviewPopupRoots, DispatcherPriority.Loaded);
    }

    private void RegisterSemanticPreviewPopupRoots()
    {
        foreach (var preview in _popupHostPreviews)
        {
            if (preview.PreviewContent is not { } content)
            {
                continue;
            }

            // 已注册的弹层仍处于打开状态时无需重扫；这条早退让 LayoutUpdated 上的调用保持轻量。
            if (preview.AdditionalRoots.Count > 0 &&
                _trackedSemanticPreviewPopups.Any(static popup => popup.IsOpen))
            {
                continue;
            }

            foreach (var popup in content.GetVisualDescendants().OfType<AvaloniaPopup>())
            {
                if (popup.IsOpen && popup.Child is { } child)
                {
                    TrackSemanticPreviewPopup(popup);
                    if (!preview.AdditionalRoots.Contains(child))
                    {
                        preview.AdditionalRoots.Add(child);
                    }
                }
            }
        }
    }

    private void TrackSemanticPreviewPopup(AvaloniaPopup popup)
    {
        if (_trackedSemanticPreviewPopups.Contains(popup))
        {
            return;
        }

        popup.Opened += HandleSemanticPreviewPopupOpened;
        popup.Closed += HandleSemanticPreviewPopupClosed;
        _trackedSemanticPreviewPopups.Add(popup);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        RefreshCurrentViewModelData();
    }

    public void HandleChangeModeCheckChanged(object? sender, RoutedEventArgs? args)
    {
        if (DataContext is MenuViewModel viewModel)
        {
            viewModel.HandleChangeModeCheckChanged(sender, args);
        }
    }

    public void HandleChangeStyleCheckChanged(object? sender, RoutedEventArgs? args)
    {
        if (DataContext is MenuViewModel viewModel)
        {
            viewModel.HandleChangeStyleCheckChanged(sender, args);
        }
    }

    public void HandleToggleInlineCollapsedClick(object? sender, RoutedEventArgs? args)
    {
        if (DataContext is MenuViewModel viewModel)
        {
            viewModel.HandleToggleInlineCollapsedClick(sender, args);
        }
    }

    public void HandleToggleStructuredNavMenuCollapsedClick(object? sender, RoutedEventArgs? args)
    {
        if (DataContext is MenuViewModel viewModel)
        {
            viewModel.HandleToggleStructuredNavMenuCollapsedClick(sender, args);
        }
    }

    private void RefreshCurrentViewModelData()
    {
        if (DataContext is MenuViewModel viewModel)
        {
            viewModel.DefaultOpenPaths =
            [
                new TreeNodePath("/3/SubGroup2")
            ];
            viewModel.DefaultSelectedPath = new TreeNodePath("/3/SubGroup1/Option1");
            viewModel.IsInlineCollapsed             = false;
            viewModel.IsStructuredNavMenuCollapsed  = false;
            viewModel.InlineCollapsedOpenPaths =
            [
                new TreeNodePath("/NavigationOne")
            ];
            viewModel.InlineCollapsedSelectedPath = new TreeNodePath("/Option1");
            // 语义夹具对齐上游语义示例的 openKeys / selectedKeys：默认展开子菜单并选中首项，
            // 这样 subMenu.* 与 popup.root 部件在预览和 Examples 示例里都是已实例化状态。
            viewModel.SemanticPreviewOpenPaths    = [new TreeNodePath("/sub-menu")];
            viewModel.SemanticPreviewSelectedPath = new TreeNodePath("/nav-one");
            RefreshMenuSources(viewModel);
        }
    }

    private void ClearCurrentViewModelData()
    {
        if (DataContext is not MenuViewModel viewModel)
        {
            return;
        }

        viewModel.MenuItems                   = null;
        viewModel.InlineNavMenuNodes          = null;
        viewModel.ItemsSourceDemoNavMenuNodes = null;
        viewModel.MenuFlyoutItems             = null;
        viewModel.ContextMenuItems            = null;
        viewModel.DefaultOpenPaths            = null;
        viewModel.DefaultSelectedPath         = null;
        viewModel.IsInlineCollapsed           = false;
        viewModel.IsStructuredNavMenuCollapsed = false;
        viewModel.InlineCollapsedOpenPaths    = null;
        viewModel.InlineCollapsedSelectedPath = null;
        viewModel.SemanticPreviewOpenPaths    = null;
        viewModel.SemanticPreviewSelectedPath = null;
        viewModel.DefaultSelectedNode         = null;
    }

    private void RefreshMenuSources(MenuViewModel viewModel)
    {
        InitInlineNavMenuNodes(viewModel);
        InitMenuTreeNodes(viewModel);
        InitContextMenuItems(viewModel);
        InitMenuFlyoutMenuItems(viewModel);
    }

    private static string Lang(MenuShowCaseLangResourceKind resourceKind, string fallback)
    {
        return GalleryLocalization.Get(resourceKind, fallback);
    }

    private static string DisplayLang(MenuShowCaseLangResourceKind resourceKind, string fallback)
    {
        return Lang(resourceKind, fallback).Replace("_", string.Empty, StringComparison.Ordinal);
    }

    private void InitContextMenuItems(MenuViewModel viewModel)
    {
        viewModel.ContextMenuItems = new List<IMenuItemData>
        {
            new MenuItemData
            {
                Header       = Lang(MenuShowCaseLangResourceKind.P2HeaderCut, "Cut"),
                Icon         = new ScissorOutlined(),
                InputGesture = KeyGesture.Parse("Ctrl+X"),
            },
            new MenuItemData
            {
                Header       = Lang(MenuShowCaseLangResourceKind.P2HeaderCopy, "Copy"),
                Icon         = new CopyOutlined(),
                InputGesture = KeyGesture.Parse("Ctrl+C"),
            },
            new MenuItemData
            {
                Header       = Lang(MenuShowCaseLangResourceKind.P2HeaderDelete, "Delete"),
                Icon         = new DeleteOutlined(),
                InputGesture = KeyGesture.Parse("Ctrl+D"),
            },
            new MenuItemData
            {
                Header = Lang(MenuShowCaseLangResourceKind.P2HeaderPaste, "Paste"),
                Children =
                [
                    new MenuItemData
                    {
                        Header       = Lang(MenuShowCaseLangResourceKind.P2HeaderPaste, "Paste"),
                        Icon         = new FileDoneOutlined(),
                        InputGesture = KeyGesture.Parse("Ctrl+P")
                    },
                    new MenuItemData
                    {
                        Header       = Lang(MenuShowCaseLangResourceKind.P2HeaderPasteFromHistory, "Paste from History"),
                        InputGesture = KeyGesture.Parse("Ctrl+Shift+V")
                    }
                ]
            }
        };
    }

    private void InitMenuTreeNodes(MenuViewModel viewModel)
    {
        viewModel.MenuItems = new List<IMenuItemData>
        {
            new MenuItemData
            {
                Header = DisplayLang(MenuShowCaseLangResourceKind.P2HeaderFile, "File"),
                Children =
                [
                    new MenuItemData
                    {
                        Header       = Lang(MenuShowCaseLangResourceKind.P2HeaderNewTextFile, "New Text File"),
                        InputGesture = KeyGesture.Parse("Ctrl+N")
                    },
                    new MenuItemData
                    {
                        Header       = Lang(MenuShowCaseLangResourceKind.P2HeaderNewFile, "New File"),
                        InputGesture = KeyGesture.Parse("Ctrl+Alt+N")
                    },
                    new MenuItemData
                    {
                        Header       = Lang(MenuShowCaseLangResourceKind.P2HeaderNewWindow, "New Window"),
                        InputGesture = KeyGesture.Parse("Ctrl+Shift+N")
                    }
                ]
            },
            new MenuItemData
            {
                Header = DisplayLang(MenuShowCaseLangResourceKind.P2HeaderEdit, "Edit"),
                Children =
                [
                    new MenuItemData
                    {
                        Header       = Lang(MenuShowCaseLangResourceKind.P2HeaderUndo, "Undo"),
                        InputGesture = KeyGesture.Parse("Ctrl+Shift+Z")
                    },
                    new MenuSeparatorData(),
                    new MenuItemData
                    {
                        Header       = Lang(MenuShowCaseLangResourceKind.P2HeaderCut, "Cut"),
                        InputGesture = KeyGesture.Parse("Ctrl+X")
                    }
                ]
            },
            new MenuItemData
            {
                Header    = Lang(MenuShowCaseLangResourceKind.P2HeaderDisabledItem, "Disabled Item"),
                IsEnabled = false
            }
        };
    }

    private void InitInlineNavMenuNodes(MenuViewModel viewModel)
    {
        viewModel.InlineNavMenuNodes          = BuildNavMenuNodes(out _);
        viewModel.ItemsSourceDemoNavMenuNodes = BuildNavMenuNodes(out _navMenuDefaultSelectedItem);
        viewModel.DefaultSelectedNode         = _navMenuDefaultSelectedItem;
    }

    private static List<INavMenuNode> BuildNavMenuNodes(out NavMenuNode defaultSelected)
    {
        defaultSelected = new NavMenuNode
        {
            Header  = Lang(MenuShowCaseLangResourceKind.P2HeaderOptionN4, "Option 4"),
            ItemKey = "Option4",
            Icon    = new TwitterOutlined()
        };
        return new List<INavMenuNode>
        {
            new NavMenuNode
            {
                Header  = Lang(MenuShowCaseLangResourceKind.P2HeaderNavigationOne, "Navigation One"),
                Icon    = new MailOutlined(),
                ItemKey = "1"
            },
            new NavMenuNode
            {
                Header  = Lang(MenuShowCaseLangResourceKind.P2HeaderNavigationTwo, "Navigation Two"),
                Icon    = new AppstoreOutlined(),
                ItemKey = "2"
            },
            new NavMenuNode
            {
                Header  = Lang(MenuShowCaseLangResourceKind.P2HeaderNavigationThreeSubmenu, "Navigation Three - Submenu"),
                Icon    = new SettingOutlined(),
                ItemKey = "3",
                Children =
                [
                    new NavMenuNode
                    {
                        Header  = Lang(MenuShowCaseLangResourceKind.P2HeaderItemN1, "Item 1"),
                        ItemKey = "SubGroup1",
                        Children =
                        [
                            new NavMenuNode
                            {
                                Header  = Lang(MenuShowCaseLangResourceKind.P2HeaderOptionN1, "Option 1"),
                                ItemKey = "Option1"
                            },
                            new NavMenuNode
                            {
                                Header  = Lang(MenuShowCaseLangResourceKind.P2HeaderOptionN2, "Option 2"),
                                ItemKey = "Option2"
                            }
                        ]
                    },
                    new NavMenuNode
                    {
                        Header  = Lang(MenuShowCaseLangResourceKind.P2HeaderItemN2, "Item 2"),
                        ItemKey = "SubGroup2",
                        Children =
                        [
                            new NavMenuNode
                            {
                                Header  = Lang(MenuShowCaseLangResourceKind.P2HeaderOptionN3, "Option 3"),
                                ItemKey = "Option3"
                            },
                            defaultSelected
                        ]
                    }
                ]
            },
            new NavMenuNode
            {
                Header  = Lang(MenuShowCaseLangResourceKind.P2HeaderNavigationFour, "Navigation Four"),
                ItemKey = "4"
            }
        };
    }

    private void InitMenuFlyoutMenuItems(MenuViewModel viewModel)
    {
        viewModel.MenuFlyoutItems = new List<IMenuItemData>
        {
            new MenuItemData
            {
                Header       = Lang(MenuShowCaseLangResourceKind.P2HeaderCut, "Cut"),
                InputGesture = KeyGesture.Parse("Ctrl+X"),
                Icon         = new ScissorOutlined(),
            },
            new MenuItemData
            {
                Header       = Lang(MenuShowCaseLangResourceKind.P2HeaderCopy, "Copy"),
                InputGesture = KeyGesture.Parse("Ctrl+C"),
                Icon         = new CopyOutlined(),
            },
            new MenuItemData
            {
                Header       = Lang(MenuShowCaseLangResourceKind.P2HeaderDelete, "Delete"),
                InputGesture = KeyGesture.Parse("Ctrl+D"),
                Icon         = new DeleteOutlined(),
            },
            new MenuItemData
            {
                Header = Lang(MenuShowCaseLangResourceKind.P2HeaderPaste, "Paste"),
                Children =
                [
                    new MenuItemData
                    {
                        Header       = Lang(MenuShowCaseLangResourceKind.P2HeaderPaste, "Paste"),
                        InputGesture = KeyGesture.Parse("Ctrl+P"),
                        Icon         = new FileDoneOutlined(),
                    },
                    new MenuSeparatorData(),
                    new MenuItemData
                    {
                        Header       = Lang(MenuShowCaseLangResourceKind.P2HeaderPasteFromHistory, "Paste from History"),
                        InputGesture = KeyGesture.Parse("Ctrl+Shift+V"),
                    }
                ]
            }
        };
    }
}
