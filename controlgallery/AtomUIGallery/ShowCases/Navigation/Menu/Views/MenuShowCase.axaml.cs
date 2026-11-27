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
    // Semantic previews that host popup parts (vertical NavMenu and plain Menu): the popup content lives
    // outside the visual root, so its root must be registered explicitly.
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

    // The NavMenu submenu popup of the semantic preview is created by the menu item template and rendered
    // outside the owner's visual root. TemplateExtensions.GetTemplateDescendants only walks down template
    // descendants whose TemplatedParent is the owner and stops at the runtime-generated menu item containers
    // (TemplatedParent is null), so the highlight session cannot discover that popup by itself. Following the
    // AdditionalRoots pattern also used by DropdownButton and InfoFlyout, the Child of each open popup is
    // registered with the preview explicitly.
    //
    // Keeping the popup open is the job of the public NavMenu pin property (the preview declares
    // IsPopupPinnedOpen="True" in AXAML): it makes NavMenu open and pin the first expandable item and blocks
    // ordinary closing. This code only registers the popup root with the preview once the popup is open, and
    // registers it again after the popup closes for lifetime reasons, such as the preview scrolling out of
    // the viewport.
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
        // NavMenu opens the popup asynchronously after the container is created, later than this callback, and in
        // overlay mode the popup content hangs in the overlay layer, so it cannot be inferred reliably from the
        // owner subtree. The additional root is registered on layout passes once the popup is really open. The
        // callback only registers idempotently and never writes control state, so it cannot loop with opening and
        // closing.
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

    // When the preview scrolls out of the viewport the popup closes for placement-target lifetime reasons;
    // the popup root is registered again when it scrolls back.
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

            // No rescan is needed while a registered popup is still open; this early return keeps the LayoutUpdated
            // handler light.
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
            // The semantic fixture mirrors openKeys and selectedKeys of the upstream semantic example: the submenu is
            // expanded and its first item selected by default, so the subMenu.* and popup.root parts are instantiated
            // in both the preview and the Examples.
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
