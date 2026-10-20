using System.Collections;
using System.Diagnostics;
using System.Reflection;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUI.Toolkits.GalleryBase.SourceCode;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaEdit;
using AvaloniaEdit.Editing;
using AvaloniaEdit.TextMate;
using Shouldly;
using TextMateSharp.Registry;
using TextMateSharp.Grammars;
using Xunit;
using AtomUIContextMenu = AtomUI.Desktop.Controls.ContextMenu;
using AtomUIMenuItem = AtomUI.Desktop.Controls.MenuItem;
using DesktopTabControl = AtomUI.Desktop.Controls.TabControl;
using DesktopTabItem = AtomUI.Desktop.Controls.TabItem;

namespace AtomUI.Toolkits.GalleryBase.Tests.SourceCode;

public class GalleryCodeViewerRuntimeTests
{
    static GalleryCodeViewerRuntimeTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void GalleryCodeViewer_Disables_Editor_Scroll_Chaining()
    {
        var viewer = new GalleryCodeViewer
        {
            CodeText = string.Join("\n", Enumerable.Range(1, 100).Select(index => $"Line {index}")),
            Language = "csharp"
        };

        ShowInWindow(viewer, editor =>
        {
            var scrollViewer = GetEditorScrollViewer(editor);

            Avalonia.Controls.ScrollViewer.GetIsScrollChainingEnabled(scrollViewer).ShouldBeFalse();
        });
    }

    [Fact]
    public void GalleryCodeViewer_Opens_Find_SearchPanel_Without_Avalonia_Default_ToggleButton_Theme()
    {
        var viewer = new GalleryCodeViewer
        {
            CodeText = "public class Demo { }",
            Language = "csharp"
        };

        ShowInWindow(viewer, editor =>
        {
            Should.NotThrow(() => OpenSearchPanel(editor, ApplicationCommands.Find));

            editor.SearchPanel.IsOpened.ShouldBeTrue();
            editor.SearchPanel.GetVisualDescendants()
                  .OfType<AtomUI.Desktop.Controls.LineEdit>()
                  .ShouldNotBeEmpty();
            var searchInput = editor.SearchPanel.GetVisualDescendants()
                                    .OfType<AtomUI.Desktop.Controls.LineEdit>()
                                    .First();

            searchInput.SetCurrentValue(Avalonia.Controls.TextBox.TextProperty, "Demo");
            Dispatcher.UIThread.RunJobs();
            editor.SearchPanel.SearchPattern.ShouldBe("Demo");

            editor.SearchPanel.SearchPattern = "class";
            Dispatcher.UIThread.RunJobs();
            searchInput.Text.ShouldBe("class");

            editor.SearchPanel.GetVisualDescendants()
                  .OfType<AtomUI.Desktop.Controls.IconButton>()
                  .ShouldNotBeEmpty();
            var searchPanelButtons = editor.SearchPanel.GetVisualDescendants()
                                           .OfType<Control>()
                                           .Where(control => control.Classes.Contains("search-panel-icon-button"))
                                           .ToArray();
            searchPanelButtons.ShouldNotBeEmpty();
            foreach (var button in searchPanelButtons)
            {
                ToolTip.GetTip(button).ShouldBeNull();
            }

            var optionButtons = editor.SearchPanel.GetVisualDescendants()
                                      .OfType<AtomUI.Desktop.Controls.ToggleIconButton>()
                                      .Where(button => button.Classes.Contains("search-option"))
                                      .ToArray();
            optionButtons.Length.ShouldBe(3);

            optionButtons[0].IsChecked = true;
            Dispatcher.UIThread.RunJobs();
            editor.SearchPanel.MatchCase.ShouldBeTrue();

            editor.SearchPanel.WholeWords = true;
            editor.SearchPanel.UseRegex    = true;
            Dispatcher.UIThread.RunJobs();
            optionButtons[1].IsChecked.ShouldBe(true);
            optionButtons[2].IsChecked.ShouldBe(true);
        });
    }

    [Fact]
    public void GalleryCodeViewer_SearchPanel_Search_Input_Uses_Normal_LineEdit_Middle_Height()
    {
        var normalInput = new AtomUI.Desktop.Controls.LineEdit
        {
            Width    = 265,
            SizeType = CustomizableSizeType.Middle,
            Text     = "Normal"
        };
        var viewer = new GalleryCodeViewer
        {
            CodeText = "public class Demo { }",
            Language = "csharp",
            Height   = 400
        };
        var host = new StackPanel
        {
            Children =
            {
                normalInput,
                viewer
            }
        };

        ShowInWindow(viewer, editor =>
        {
            OpenSearchPanel(editor, ApplicationCommands.Find);
            Dispatcher.UIThread.RunJobs();

            var searchInput = editor.SearchPanel.GetVisualDescendants()
                                    .OfType<AtomUI.Desktop.Controls.LineEdit>()
                                    .Single(input => input.Name == "PART_searchTextBox");

            normalInput.Bounds.Height.ShouldBeGreaterThan(0);
            searchInput.Bounds.Height.ShouldBe(normalInput.Bounds.Height, 0.5);
        }, host);
    }

    [Fact]
    public void GalleryCodeViewer_Opens_Replace_SearchPanel_Without_Avalonia_Default_ToggleButton_Theme()
    {
        var viewer = new GalleryCodeViewer
        {
            CodeText = "public class Demo { }",
            Language = "csharp"
        };

        ShowInWindow(viewer, editor =>
        {
            Should.NotThrow(() => OpenSearchPanel(editor, ApplicationCommands.Replace));

            editor.SearchPanel.IsOpened.ShouldBeTrue();
            editor.SearchPanel.GetVisualDescendants()
                  .OfType<AtomUI.Desktop.Controls.LineEdit>()
                  .ShouldNotBeEmpty();
        });
    }

    [Fact]
    public void GalleryCodeViewer_Updates_TextMate_Theme_When_ThemeVariant_Changes()
    {
        var viewer = new GalleryCodeViewer
        {
            CodeText = "<WrapPanel><Button Content=\"Demo\" /></WrapPanel>",
            Language = "axaml"
        };
        var themeScope = new ThemeVariantScope
        {
            RequestedThemeVariant = ThemeVariant.Light,
            Child                 = viewer
        };

        ShowInWindow(viewer, _ =>
        {
            GetTextMateThemeColor(viewer, "editor.background").ShouldBe("#FFFFFF", StringCompareShould.IgnoreCase);

            themeScope.RequestedThemeVariant = ThemeVariant.Dark;
            Dispatcher.UIThread.RunJobs();

            GetTextMateThemeColor(viewer, "editor.background")
                .ToUpperInvariant()
                .ShouldNotBe("#FFFFFF");
        }, themeScope);
    }

    [Fact]
    public void GalleryCodeViewer_Horizontal_ScrollBar_Starts_After_Line_Number_Gutter()
    {
        var viewer = new GalleryCodeViewer
        {
            CodeText = string.Join(
                "\n",
                Enumerable.Range(1, 40).Select(index => $"{index}: {new string('x', 220)}")),
            Language = "csharp"
        };

        ShowInWindow(viewer, editor =>
        {
            var lineNumberMargin = editor.GetVisualDescendants()
                                         .OfType<LineNumberMargin>()
                                         .Single();
            var horizontalScrollBar = editor.GetVisualDescendants()
                                            .OfType<ScrollBar>()
                                            .Single(scrollBar => scrollBar.Orientation == Orientation.Horizontal);

            horizontalScrollBar.IsVisible.ShouldBeTrue();
            lineNumberMargin.Bounds.Width.ShouldBeGreaterThan(0);

            var lineNumberRightPoint = lineNumberMargin.TranslatePoint(
                new Point(lineNumberMargin.Bounds.Width, 0),
                editor);
            var scrollBarLeftPoint = horizontalScrollBar.TranslatePoint(new Point(), editor);

            lineNumberRightPoint.ShouldNotBeNull();
            scrollBarLeftPoint.ShouldNotBeNull();
            scrollBarLeftPoint.Value.X.ShouldBeGreaterThanOrEqualTo(lineNumberRightPoint.Value.X - 0.5);
        });
    }

    [Fact]
    public void GalleryCodeViewer_ScrollBar_Inset_Converges_While_Horizontally_Scrolling()
    {
        var viewer = new GalleryCodeViewer
        {
            CodeText = string.Join(
                "\n",
                Enumerable.Range(1, 40).Select(index => $"{index}: {new string('x', 220)}")),
            Language = "csharp"
        };

        ShowInWindow(viewer, editor =>
        {
            var horizontalScrollBar = editor.GetVisualDescendants()
                                            .OfType<ScrollBar>()
                                            .Single(scrollBar => scrollBar.Orientation == Orientation.Horizontal);
            var scrollViewer = GetEditorScrollViewer(editor);

            // Drive the editor through a range of horizontal scroll offsets, forcing a layout
            // pass at each step. This reproduces the auto-scroll-while-selecting feedback loop:
            // the inset must settle to a stable value and not keep mutating the margin (which
            // previously re-triggered layout forever and hung the UI thread).
            double MeasureInsetAfterScroll(double offset)
            {
                scrollViewer.Offset = new Vector(offset, scrollViewer.Offset.Y);
                editor.InvalidateMeasure();
                editor.InvalidateArrange();
                Dispatcher.UIThread.RunJobs();
                return horizontalScrollBar.Margin.Left;
            }

            foreach (var offset in new[] { 0d, 50d, 120d, 200d, 320d })
            {
                MeasureInsetAfterScroll(offset);
            }

            // After settling at a fixed offset, repeated layout passes must not change the inset.
            var settled = MeasureInsetAfterScroll(320d);
            for (var i = 0; i < 5; i++)
            {
                MeasureInsetAfterScroll(320d).ShouldBe(settled, 0.5);
            }

            // The inset stays pinned to the (non-scrolling) gutter width, independent of offset.
            settled.ShouldBeGreaterThan(0);
        });
    }

    [Fact]
    public void GalleryCodeViewer_Max_Horizontal_Scroll_Keeps_Text_Clear_Of_Vertical_ScrollBar()
    {
        var longLine = "public class MessageViewModel : ReactiveObject, IRoutableViewModel, IActivatableViewModel";
        var viewer = new GalleryCodeViewer
        {
            CodeText = string.Join(
                "\n",
                new[] { longLine }.Concat(Enumerable.Range(1, 60).Select(index => $"Line {index}"))),
            Language = "csharp"
        };

        ShowInWindow(viewer, editor =>
        {
            var scrollViewer = GetEditorScrollViewer(editor);
            var verticalScrollBar = editor.GetVisualDescendants()
                                          .OfType<ScrollBar>()
                                          .Single(scrollBar => scrollBar.Orientation == Orientation.Vertical);

            verticalScrollBar.IsVisible.ShouldBeTrue();
            editor.TextArea.TextView.EnsureVisualLines();

            scrollViewer.Offset = new Vector(scrollViewer.Extent.Width, scrollViewer.Offset.Y);
            editor.InvalidateMeasure();
            editor.InvalidateArrange();
            Dispatcher.UIThread.RunJobs();
            editor.TextArea.TextView.EnsureVisualLines();

            var visualLine = editor.TextArea.TextView.VisualLines
                                   .Single(line => line.FirstDocumentLine.LineNumber == 1);
            var longestTextLineWidth = visualLine.TextLines.Max(line => line.WidthIncludingTrailingWhitespace);
            var textRight = editor.TextArea.TextView.TranslatePoint(
                                new Point(longestTextLineWidth - editor.TextArea.TextView.HorizontalOffset, 0),
                                editor)
                            .ShouldNotBeNull()
                            .X;
            var scrollBarLeft = verticalScrollBar.TranslatePoint(new Point(), editor)
                                                 .ShouldNotBeNull()
                                                 .X;

            textRight.ShouldBeLessThanOrEqualTo(scrollBarLeft - 1);
        });
    }

    [Fact]
    public void GalleryCodeViewer_Selection_Drag_AutoScroll_Does_Not_Hang()
    {
        var viewer = new GalleryCodeViewer
        {
            CodeText = string.Join(
                "\n",
                Enumerable.Range(1, 40).Select(index => $"{index}: {new string('x', 260)}")),
            Language = "csharp"
        };
        var window = new Window
        {
            Width   = 640,
            Height  = 480,
            Content = viewer
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            viewer.ApplyTemplate();

            var editor = viewer.GetVisualDescendants()
                               .OfType<TextEditor>()
                               .Single();
            editor.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();

            var textView = editor.TextArea.TextView;
            var startPoint = textView.TranslatePoint(new Point(80, 22), window).ShouldNotBeNull();
            var dragPoint = textView.TranslatePoint(new Point(textView.Bounds.Width + 160, 82), window)
                                    .ShouldNotBeNull();

            window.MouseMove(startPoint);
            window.MouseDown(startPoint, MouseButton.Left);
            window.MouseMove(dragPoint);

            for (var i = 0; i < 8; i++)
            {
                Dispatcher.UIThread.RunJobs();
            }

            window.MouseUp(dragPoint, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            editor.HorizontalOffset.ShouldBeGreaterThan(0);
            editor.TextArea.Selection.IsEmpty.ShouldBeFalse();
            editor.TextArea.TextView.LineTransformers
                  .OfType<TextMateColoringTransformer>()
                  .ShouldNotBeEmpty();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
            viewer.Dispose();
        }
    }

    [Fact]
    public void GalleryCodeViewer_Selection_Drag_To_Right_Edge_Does_Not_Rebound_On_Mouse_Up()
    {
        var viewer = new GalleryCodeViewer
        {
            CodeText = string.Join(
                "\n",
                Enumerable.Range(1, 70).Select(index =>
                    $"{index}: <atom:TextBlock Text=\"{{Binding Text}}\" Foreground=\"{{Binding Foreground}}\" FontSize=\"18\" FontWeight=\"Bold\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" />")),
            Language = "axaml"
        };
        var window = new Window
        {
            Width   = 640,
            Height  = 480,
            Content = viewer
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            viewer.ApplyTemplate();

            var editor = viewer.GetVisualDescendants()
                               .OfType<TextEditor>()
                               .Single();
            editor.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();

            var textView = editor.TextArea.TextView;
            var startPoint = textView.TranslatePoint(new Point(120, 28), window)
                                     .ShouldNotBeNull();
            var dragPoint = textView.TranslatePoint(new Point(textView.Bounds.Width + 240, 120), window)
                                    .ShouldNotBeNull();

            window.MouseMove(startPoint);
            window.MouseDown(startPoint, MouseButton.Left);
            window.MouseMove(dragPoint);

            for (var i = 0; i < 16; i++)
            {
                Dispatcher.UIThread.RunJobs();
            }

            var offsetBeforeMouseUp = editor.HorizontalOffset;
            window.MouseUp(dragPoint, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            var offsetAfterMouseUp = editor.HorizontalOffset;

            offsetBeforeMouseUp.ShouldBeGreaterThan(0);
            offsetAfterMouseUp.ShouldBeGreaterThanOrEqualTo(offsetBeforeMouseUp - 0.5);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
            viewer.Dispose();
        }
    }

    [Fact]
    public void GalleryCodeViewer_Keeps_TextMate_When_Click_Does_Not_Start_Selection_Drag()
    {
        var viewer = new GalleryCodeViewer
        {
            CodeText = string.Join(
                "\n",
                Enumerable.Repeat(
                    "    <atom:TextBlock Foreground=\"{atom:SharedTokenResource ColorTextTertiary}\" Text=\"{Binding Description}\" />",
                    40)),
            Language = "axaml"
        };
        var window = new Window
        {
            Width   = 640,
            Height  = 480,
            Content = viewer
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            viewer.ApplyTemplate();

            var editor = viewer.GetVisualDescendants()
                               .OfType<TextEditor>()
                               .Single();
            editor.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();

            editor.TextArea.TextView.LineTransformers
                  .OfType<TextMateColoringTransformer>()
                  .ShouldNotBeEmpty();

            var startPoint = editor.TextArea.TextView.TranslatePoint(new Point(80, 22), window)
                                   .ShouldNotBeNull();

            window.MouseMove(startPoint);
            window.MouseDown(startPoint, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            editor.TextArea.TextView.LineTransformers
                  .OfType<TextMateColoringTransformer>()
                  .ShouldNotBeEmpty();

            window.MouseUp(startPoint, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            editor.TextArea.TextView.LineTransformers
                  .OfType<TextMateColoringTransformer>()
                  .ShouldNotBeEmpty();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
            viewer.Dispose();
        }
    }

    [Fact]
    public void GalleryCodeViewer_Keeps_TextMate_During_Pointer_Selection_Drag()
    {
        var viewer = new GalleryCodeViewer
        {
            CodeText = string.Join(
                "\n",
                Enumerable.Repeat(
                    "    <atom:TextBlock Foreground=\"{atom:SharedTokenResource ColorTextTertiary}\" Text=\"{Binding Description}\" />",
                    40)),
            Language = "axaml"
        };
        var window = new Window
        {
            Width   = 640,
            Height  = 480,
            Content = viewer
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            viewer.ApplyTemplate();

            var editor = viewer.GetVisualDescendants()
                               .OfType<TextEditor>()
                               .Single();
            editor.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();

            editor.TextArea.TextView.LineTransformers
                  .OfType<TextMateColoringTransformer>()
                  .ShouldNotBeEmpty();

            var textView = editor.TextArea.TextView;
            var startPoint = textView.TranslatePoint(new Point(80, 22), window)
                                     .ShouldNotBeNull();
            var dragPoint = textView.TranslatePoint(new Point(180, 52), window)
                                    .ShouldNotBeNull();

            window.MouseMove(startPoint);
            window.MouseDown(startPoint, MouseButton.Left);
            window.MouseMove(dragPoint);
            Dispatcher.UIThread.RunJobs();

            editor.TextArea.TextView.LineTransformers
                  .OfType<TextMateColoringTransformer>()
                  .ShouldNotBeEmpty();

            window.MouseUp(dragPoint, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            editor.TextArea.TextView.LineTransformers
                  .OfType<TextMateColoringTransformer>()
                  .ShouldNotBeEmpty();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
            viewer.Dispose();
        }
    }

    [Fact]
    public void GalleryCodeViewer_Uses_Dark_Syntax_Theme_When_ThemeManager_Appearance_Is_Dark()
    {
        var application = Application.Current!;
        var themeManager = application.GetThemeManager().ShouldNotBeNull();
        var previousAlgorithms = CaptureCurrentAlgorithms(themeManager);

        try
        {
            SetDarkAppearance(themeManager, true);

            var viewer = new GalleryCodeViewer
            {
                CodeText = "<StackPanel Margin=\"20\" Spacing=\"12\" />",
                Language = "axaml"
            };

            ShowInWindow(viewer, _ =>
            {
                GetCurrentSyntaxTheme(viewer).ShouldBe(ThemeName.DarkPlus);
                GetAxamlTagNameColor(viewer).ShouldBe("#569CD6", StringCompareShould.IgnoreCase);
            });
        }
        finally
        {
            ApplyAlgorithms(themeManager, previousAlgorithms);
        }
    }

    [Fact]
    public void GalleryCodeViewer_Axaml_Syntax_Foregrounds_Are_Applied_To_Visible_Elements()
    {
        var viewer = new GalleryCodeViewer
        {
            CodeText = CreateSplashAxamlSnippet(),
            Language = "axaml"
        };

        ShowInWindow(viewer, editor =>
        {
            editor.TextArea.TextView.EnsureVisualLines();
            Dispatcher.UIThread.RunJobs();
            editor.TextArea.TextView.EnsureVisualLines();

            var tagNameColor = Color.Parse(GetAxamlTokenColor(viewer, "StackPanel").ShouldNotBeNull());
            var splashColor = GetVisibleTextForegroundColor(editor, "Splash");
            splashColor.ShouldBe(
                tagNameColor,
                $"Scopes: {string.Join(" | ", GetLineTokenScopes(viewer, 0))}; {GetTextMateDiagnostics(viewer, 0)}");
            GetVisibleTextForegroundColor(editor, "Width")
                .ShouldBe(Color.Parse(GetAxamlTokenColor(viewer, "Margin").ShouldNotBeNull()));
            GetVisibleTextForegroundColor(editor, "420")
                .ShouldBe(Color.Parse(GetAxamlTokenColor(viewer, "20").ShouldNotBeNull()));
            GetVisibleTextForegroundColor(editor, "StackPanel")
                .ShouldBe(Color.Parse(GetAxamlTokenColor(viewer, "StackPanel").ShouldNotBeNull()));
            GetVisibleTextForegroundColor(editor, "Orientation")
                .ShouldBe(Color.Parse(GetAxamlTokenColor(viewer, "Margin").ShouldNotBeNull()));
        });
    }

    [Fact]
    public void GalleryCodeViewer_Updates_TextMate_Theme_When_AtomUI_Dark_Mode_Changes()
    {
        var application = Application.Current!;
        var themeManager = application.GetThemeManager().ShouldNotBeNull();
        var previousAlgorithms = CaptureCurrentAlgorithms(themeManager);

        try
        {
            SetDarkAppearance(themeManager, false);

            var viewer = new GalleryCodeViewer
            {
                CodeText = "<StackPanel Margin=\"20\" Spacing=\"12\" />",
                Language = "axaml"
            };

            ShowInWindow(viewer, _ =>
            {
                GetCurrentSyntaxTheme(viewer).ShouldBe(ThemeName.LightPlus);

                SetDarkAppearance(themeManager, true);

                GetCurrentSyntaxTheme(viewer).ShouldBe(ThemeName.DarkPlus);
                GetAxamlTagNameColor(viewer).ShouldBe("#569CD6", StringCompareShould.IgnoreCase);

                SetDarkAppearance(themeManager, false);

                GetCurrentSyntaxTheme(viewer).ShouldBe(ThemeName.LightPlus);
            });
        }
        finally
        {
            ApplyAlgorithms(themeManager, previousAlgorithms);
        }
    }

    [Fact]
    public void Disposed_Detached_GalleryCodeViewer_Is_Not_Retained_By_Theme_Subscriptions()
    {
        var viewer = AttachAndDetachTemporaryViewer();

        for (var attempt = 0; attempt < 3; attempt++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            Dispatcher.UIThread.RunJobs();
        }

        viewer.IsAlive.ShouldBeFalse();
    }

    [Fact]
    public void GalleryCodeViewer_Line_Number_Text_Keeps_Distance_From_Separator()
    {
        var viewer = new GalleryCodeViewer
        {
            CodeText = string.Join("\n", Enumerable.Range(1, 8).Select(index => $"Line {index}")),
            Language = "text"
        };

        ShowInWindow(viewer, editor =>
        {
            var lineNumberMargin = editor.GetVisualDescendants()
                                         .OfType<LineNumberMargin>()
                                         .Single();
            var separator = editor.TextArea.LeftMargins.Single(DottedLineMargin.IsDottedLineMargin);

            var lineNumberRightPoint = lineNumberMargin.TranslatePoint(
                new Point(lineNumberMargin.Bounds.Width, 0),
                editor);
            var separatorLeftPoint = separator.TranslatePoint(new Point(), editor);

            lineNumberRightPoint.ShouldNotBeNull();
            separatorLeftPoint.ShouldNotBeNull();
            (separatorLeftPoint.Value.X - lineNumberRightPoint.Value.X).ShouldBeGreaterThanOrEqualTo(8);
        });
    }

    [Fact]
    public void GalleryCodeViewer_Uses_AtomUI_ContextMenu_For_Copying_Selected_Code()
    {
        var viewer = new GalleryCodeViewer
        {
            CodeText = "public class Demo { }",
            Language = "csharp"
        };

        ShowInWindow(viewer, editor =>
        {
            var contextMenu = editor.ContextMenu.ShouldBeOfType<AtomUIContextMenu>();
            var copyItem = contextMenu.Items
                                      .OfType<AtomUIMenuItem>()
                                      .Single();

            copyItem.Header.ShouldBe("Copy");
            copyItem.IsEnabled.ShouldBeFalse();

            editor.Select(0, 6);
            Dispatcher.UIThread.RunJobs();

            copyItem.IsEnabled.ShouldBeTrue();
        });
    }

    [Fact]
    public async Task GalleryCodeViewer_ContextMenu_Copy_Writes_Selected_Code_To_Clipboard()
    {
        var viewer = new GalleryCodeViewer
        {
            CodeText = "public class Demo { }",
            Language = "csharp"
        };

        await ShowInWindowAsync(viewer, async editor =>
        {
            editor.Select(0, 6);
            Dispatcher.UIThread.RunJobs();

            var copyItem = editor.ContextMenu
                                 .ShouldBeOfType<AtomUIContextMenu>()
                                 .Items
                                 .OfType<AtomUIMenuItem>()
                                 .Single();

            copyItem.RaiseEvent(new RoutedEventArgs(AtomUIMenuItem.ClickEvent, copyItem));

            await WaitForClipboardTextAsync(editor, "public");
        });
    }

    [Fact]
    public void DrawerContent_Renders_Tab_Header_Per_Snippet()
    {
        var group = new ShowCaseCodeSnippetGroup(
            "Basic",
            new[]
            {
                CreateSnippet("AXAML", "axaml", "<Button Content=\"Primary\" />"),
                CreateSnippet("Code-behind", "csharp", "private void HandleClick() { }"),
                CreateSnippet("ViewModel", "csharp", "public string SelectedValue { get; set; }")
            });

        ShowDrawerContentInWindow(group, content =>
        {
            var tabControl = content.GetVisualDescendants()
                                    .OfType<DesktopTabControl>()
                                    .Single();

            tabControl.Items
                      .OfType<DesktopTabItem>()
                      .Select(static tabItem => tabItem.Header)
                      .ShouldBe(new object?[] { "AXAML", "Code-behind", "ViewModel" });
        });
    }

    [Fact]
    public void DrawerContent_Creates_Viewers_Lazily_Per_Selected_Tab()
    {
        var group = new ShowCaseCodeSnippetGroup(
            "Basic",
            new[]
            {
                CreateSnippet("AXAML", "axaml", "<Button Content=\"Primary\" />"),
                CreateSnippet("Code-behind", "csharp", "private void HandleClick() { }"),
                CreateSnippet("ViewModel", "csharp", "public string SelectedValue { get; set; }")
            });

        ShowDrawerContentInWindow(group, content =>
        {
            var tabControl = content.GetVisualDescendants()
                                    .OfType<DesktopTabControl>()
                                    .Single();
            var tabItems = tabControl.Items.OfType<DesktopTabItem>().ToArray();

            // Only the initially selected tab materializes a viewer.
            tabItems[0].Content.ShouldBeOfType<GalleryCodeViewer>();
            tabItems[1].Content.ShouldBeNull();
            tabItems[2].Content.ShouldBeNull();

            tabControl.SelectedIndex = 2;
            Dispatcher.UIThread.RunJobs();

            // Selecting another tab materializes its viewer; the untouched tab stays empty.
            tabItems[2].Content.ShouldBeOfType<GalleryCodeViewer>();
            tabItems[1].Content.ShouldBeNull();
        });
    }

    [Fact]
    public void DrawerContent_Materialized_Viewer_Has_Syntax_Tokens_For_Selected_Snippet()
    {
        var group = new ShowCaseCodeSnippetGroup(
            "Basic",
            new[]
            {
                CreateSnippet("AXAML", "axaml", "<StackPanel Margin=\"20\"><Button Content=\"Demo\" /></StackPanel>"),
                CreateSnippet("Code-behind", "csharp", "public sealed class DemoView { }")
            });

        ShowDrawerContentInWindow(group, content =>
        {
            var tabControl = content.GetVisualDescendants()
                                    .OfType<DesktopTabControl>()
                                    .Single();
            var tabItems = tabControl.Items.OfType<DesktopTabItem>().ToArray();

            var initialViewer = tabItems[0].Content.ShouldBeOfType<GalleryCodeViewer>();
            var initialScopes = WaitForLineTokenScopes(
                initialViewer,
                0,
                scope => scope.Contains("meta.tag.xml", StringComparison.Ordinal));
            initialScopes.Any(scope => scope.Contains("meta.tag.xml", StringComparison.Ordinal))
                         .ShouldBeTrue(
                             $"Scopes: {string.Join(" | ", initialScopes)}; " +
                             $"{GetTextMateDiagnostics(initialViewer, 0)}; " +
                             GetVisibleTokenizationState(initialViewer));

            tabControl.SelectedIndex = 1;
            Dispatcher.UIThread.RunJobs();

            var selectedViewer = tabItems[1].Content.ShouldBeOfType<GalleryCodeViewer>();
            var selectedScopes = WaitForLineTokenScopes(
                selectedViewer,
                0,
                scope => scope.Contains("storage.modifier", StringComparison.Ordinal) ||
                         scope.Contains("keyword", StringComparison.Ordinal));
            selectedScopes.Any(scope => scope.Contains("storage.modifier", StringComparison.Ordinal) ||
                                        scope.Contains("keyword", StringComparison.Ordinal))
                          .ShouldBeTrue($"Scopes: {string.Join(" | ", selectedScopes)}");
        });
    }

    [Fact]
    public void DrawerContent_Materialized_Viewer_Applies_Syntax_Foregrounds_For_Selected_Snippet()
    {
        var group = new ShowCaseCodeSnippetGroup(
            "Logo, content and footer",
            new[]
            {
                CreateSnippet("AXAML", "axaml", CreateSplashAxamlSnippet()),
                CreateSnippet("ViewModel", "csharp", "public sealed class SplashViewModel { }")
            });

        ShowDrawerContentInWindow(group, content =>
        {
            var tabControl = content.GetVisualDescendants()
                                    .OfType<DesktopTabControl>()
                                    .Single();
            var tabItems = tabControl.Items.OfType<DesktopTabItem>().ToArray();
            var viewer = tabItems[0].Content.ShouldBeOfType<GalleryCodeViewer>();
            var editor = viewer.GetVisualDescendants()
                               .OfType<TextEditor>()
                               .Single();

            editor.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();
            editor.TextArea.TextView.EnsureVisualLines();
            Dispatcher.UIThread.RunJobs();
            editor.TextArea.TextView.EnsureVisualLines();

            var tagNameColor = Color.Parse(GetAxamlTokenColor(viewer, "StackPanel").ShouldNotBeNull());
            var splashColor = GetVisibleTextForegroundColor(editor, "Splash");
            splashColor.ShouldBe(
                tagNameColor,
                $"Scopes: {string.Join(" | ", GetLineTokenScopes(viewer, 0))}; {GetTextMateDiagnostics(viewer, 0)}");
            GetVisibleTextForegroundColor(editor, "Width")
                .ShouldBe(Color.Parse(GetAxamlTokenColor(viewer, "Margin").ShouldNotBeNull()));
            GetVisibleTextForegroundColor(editor, "420")
                .ShouldBe(Color.Parse(GetAxamlTokenColor(viewer, "20").ShouldNotBeNull()));
            GetVisibleTextForegroundColor(editor, "StackPanel")
                .ShouldBe(Color.Parse(GetAxamlTokenColor(viewer, "StackPanel").ShouldNotBeNull()));
            GetVisibleTextForegroundColor(editor, "Orientation")
                .ShouldBe(Color.Parse(GetAxamlTokenColor(viewer, "Margin").ShouldNotBeNull()));
        });
    }

    [Fact]
    public void DrawerContent_Tokenizes_Lines_That_Enter_Viewport_After_First_Layout()
    {
        var group = new ShowCaseCodeSnippetGroup(
            "Logo, content and footer",
            new[]
            {
                CreateSnippet("AXAML", "axaml", CreateSplashAxamlSnippet())
            });
        var content = new GalleryShowCaseCodeDrawerContent(
            group,
            new ShowCaseCodeSnippetKey("Demo", "ExamplesContent", 0));
        var window = new Window
        {
            Width = 640,
            Height = 180,
            Content = content
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            content.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();

            var viewer = content.GetVisualDescendants()
                                .OfType<GalleryCodeViewer>()
                                .Single();
            var editor = viewer.GetVisualDescendants()
                               .OfType<TextEditor>()
                               .Single();
            editor.ApplyTemplate();
            editor.TextArea.TextView.EnsureVisualLines();
            Dispatcher.UIThread.RunJobs();

            ClearLineTokens(viewer, 11);

            window.Height = 720;
            editor.InvalidateMeasure();
            editor.InvalidateArrange();
            Dispatcher.UIThread.RunJobs();
            editor.TextArea.TextView.EnsureVisualLines();
            Dispatcher.UIThread.RunJobs();
            editor.TextArea.TextView.EnsureVisualLines();

            GetVisibleTextForegroundColor(editor, "StackPanel")
                .ShouldBe(Color.Parse(GetAxamlTokenColor(viewer, "StackPanel").ShouldNotBeNull()));
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
            content.Dispose();
        }
    }

    [Fact]
    public void GalleryCodeViewer_Does_Not_Immediately_Requeue_Visible_Tokenization_While_Grammar_Is_Compiling()
    {
        var viewer = new GalleryCodeViewer
        {
            CodeText = CreateSplashAxamlSnippet(),
            Language = "axaml"
        };

        try
        {
            var installation = GetTextMateInstallation(viewer);
            var compilingGrammar = new AlwaysCompilingGrammar();
            installation.GetType()
                        .GetMethod("SetGrammarInternal", BindingFlags.Instance | BindingFlags.NonPublic)!
                        .Invoke(installation, new object[] { compilingGrammar });
            ClearLineTokens(viewer, 11);

            installation.GetType()
                        .GetField("_pendingVisibleStartLine", BindingFlags.Instance | BindingFlags.NonPublic)!
                        .SetValue(installation, 11);
            installation.GetType()
                        .GetField("_pendingVisibleEndLine", BindingFlags.Instance | BindingFlags.NonPublic)!
                        .SetValue(installation, 11);
            installation.GetType()
                        .GetField("_isVisibleLineTokenizationQueued", BindingFlags.Instance | BindingFlags.NonPublic)!
                        .SetValue(installation, true);

            installation.GetType()
                        .GetMethod("ProcessVisibleLineTokenization", BindingFlags.Instance | BindingFlags.NonPublic)!
                        .Invoke(installation, Array.Empty<object>());

            installation.GetType()
                        .GetField("_isVisibleLineTokenizationQueued", BindingFlags.Instance | BindingFlags.NonPublic)!
                        .GetValue(installation)
                        .ShouldBe(false);
            installation.GetType()
                        .GetField("_pendingVisibleStartLine", BindingFlags.Instance | BindingFlags.NonPublic)!
                        .GetValue(installation)
                        .ShouldBe(11);
            installation.GetType()
                        .GetField("_pendingVisibleEndLine", BindingFlags.Instance | BindingFlags.NonPublic)!
                        .GetValue(installation)
                        .ShouldBe(11);
        }
        finally
        {
            viewer.Dispose();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void GalleryCodeViewer_Retries_Pending_Axaml_Tokenization_After_Grammar_Compilation_Finishes()
    {
        var viewer = new GalleryCodeViewer
        {
            CodeText = "<atom:DatePicker PlaceholderText=\"{gallery:DatePickerShowCaseLangResource P2PlaceholderTextSelectDate}\"/>",
            Language = "axaml"
        };

        ShowInWindow(viewer, editor =>
        {
            editor.TextArea.TextView.EnsureVisualLines();
            Dispatcher.UIThread.RunJobs();

            var installation = GetTextMateInstallation(viewer);
            var registryOptions = new RegistryOptions(GetCurrentSyntaxTheme(viewer));
            var registry = new Registry(registryOptions);
            var grammar = new ControlledCompilingGrammar(
                registry.LoadGrammar(registryOptions.GetScopeByExtension(".xml")));
            installation.GetType()
                        .GetMethod("SetGrammarInternal", BindingFlags.Instance | BindingFlags.NonPublic)!
                        .Invoke(installation, new object[] { grammar });
            ClearLineTokens(viewer, 0);
            IsLineInvalid(viewer, 0).ShouldBeTrue();

            installation.GetType()
                        .GetMethod("RequestVisibleLineTokenization", BindingFlags.Instance | BindingFlags.NonPublic)!
                        .Invoke(installation, Array.Empty<object>());
            Dispatcher.UIThread.RunJobs();

            grammar.IsCompiling = false;
            for (var i = 0; i < 20; i++)
            {
                Thread.Sleep(10);
                Dispatcher.UIThread.RunJobs();
                Dispatcher.UIThread.RunJobs(DispatcherPriority.SystemIdle);
                editor.TextArea.TextView.EnsureVisualLines();
                if (!IsLineInvalid(viewer, 0))
                {
                    var scrollViewer = GetEditorScrollViewer(editor);
                    scrollViewer.Offset = new Vector(120, scrollViewer.Offset.Y);
                    editor.InvalidateMeasure();
                    editor.InvalidateArrange();
                    Dispatcher.UIThread.RunJobs();
                    editor.TextArea.TextView.EnsureVisualLines();

                    var expectedAttributeColor = Color.Parse(GetAxamlTokenColor(viewer, "Margin").ShouldNotBeNull());
                    GetVisibleTextForegroundColor(editor, "PlaceholderText")
                        .ShouldBe(expectedAttributeColor);
                    return;
                }
            }

            IsLineInvalid(viewer, 0).ShouldBeFalse(
                $"Scopes: {string.Join(" | ", GetLineTokenScopes(viewer, 0))}; {GetTextMateDiagnostics(viewer, 0)}; {GetVisibleTokenizationState(viewer)}");
        });
    }

    [Fact]
    public void DrawerContent_Shows_Placeholder_When_No_Snippets_Available()
    {
        var key = new ShowCaseCodeSnippetKey(
            "AtomUIGallery.ShowCases.General.Button.Views.ButtonShowCase",
            "ExamplesContent",
            0);

        ShowDrawerContentInWindow(null, key, content =>
        {
            content.GetVisualDescendants()
                   .OfType<DesktopTabControl>()
                   .ShouldBeEmpty();

            content.GetVisualDescendants()
                   .OfType<TextBlock>()
                   .Select(static textBlock => textBlock.Text)
                   .ShouldContain(text => text != null && text.Contains("No source snippet found"));
        });
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static WeakReference AttachAndDetachTemporaryViewer()
    {
        var viewer = new GalleryCodeViewer
        {
            CodeText = "<Button Content=\"Detached\" />",
            Language = "axaml"
        };
        var window = new Avalonia.Controls.Window
        {
            Content = viewer
        };
        window.Show();
        window.Content = null;
        window.Close();
        viewer.Dispose();
        Dispatcher.UIThread.RunJobs();
        return new WeakReference(viewer);
    }

    private static ThemeAlgorithm[] CaptureCurrentAlgorithms(IThemeManager themeManager)
    {
        return themeManager.CurrentTheme?.Algorithms.ToArray() ?? [ThemeAlgorithm.Default];
    }

    private static void SetDarkAppearance(IThemeManager themeManager, bool isDark)
    {
        var algorithms = CaptureCurrentAlgorithms(themeManager)
                         .Where(static algorithm => algorithm != ThemeAlgorithm.Dark)
                         .ToList();
        if (isDark)
        {
            algorithms.Add(ThemeAlgorithm.Dark);
        }

        ApplyAlgorithms(themeManager, algorithms);
    }

    private static void ApplyAlgorithms(IThemeManager themeManager, IEnumerable<ThemeAlgorithm> algorithms)
    {
        var config = new ThemeConfigBuilder()
                     .WithAlgorithms(algorithms.ToArray())
                     .Build();
        var result = themeManager.ApplyThemeAsync(
                                     new ThemeRequest(
                                         themeManager.CurrentTheme?.ThemeId ?? IThemeManager.DEFAULT_THEME_ID,
                                         config,
                                         ThemeTransitionReason.UserRequest))
                                 .GetAwaiter()
                                 .GetResult();

        (result.Status is ThemeTransitionStatus.Committed or ThemeTransitionStatus.NoOp)
            .ShouldBeTrue(
                $"Theme transition failed with status '{result.Status}': " +
                string.Join(" ", result.Diagnostics.Select(static diagnostic => diagnostic.Message)));
        Dispatcher.UIThread.RunJobs();
    }

    private static ShowCaseCodeSnippet CreateSnippet(string tabTitle, string language, string text)
    {
        return new ShowCaseCodeSnippet(tabTitle, language, text, SourceFilePath: null, StartLine: 1, EndLine: 1);
    }

    private static string CreateSplashAxamlSnippet()
    {
        return string.Join(
            "\n",
            "<atom:Splash Width=\"420\"",
            "             MinHeight=\"300\"",
            "             Logo=\"{Binding ComposedLogo}\"",
            "             Title=\"AtomUI Gallery\"",
            "             Subtitle=\"{gallery:SplashShowCaseLangResource P2SubtitleDesktopBoot}\"",
            "             Message=\"{gallery:SplashShowCaseLangResource P2MessageLoadingModules}\"",
            "             Detail=\"{gallery:SplashShowCaseLangResource P2DetailProgress}\"",
            "             Progress=\"{Binding ProgressValue}\"",
            "             IsIndeterminate=\"False\"",
            "             Footer=\"{Binding ComposedFooter}\"",
            "             HorizontalAlignment=\"Left\">",
            "    <StackPanel Orientation=\"Horizontal\"",
            "                Spacing=\"8\"",
            "                HorizontalAlignment=\"Center\">",
            "        <atom:Tag Text=\"{gallery:SplashShowCaseLangResource P2ContentModuleCore}\"",
            "                  TagColor=\"success\" />",
            "        <atom:Tag Text=\"{gallery:SplashShowCaseLangResource P2ContentModuleTheme}\"",
            "                  TagColor=\"processing\" />",
            "        <atom:Tag Text=\"{gallery:SplashShowCaseLangResource P2ContentModuleGallery}\"",
            "                  TagColor=\"warning\" />",
            "    </StackPanel>",
            "    <atom:Splash.LogoTemplate>",
            "        <DataTemplate x:DataType=\"vm:SplashLogoInfo\">",
            "            <Border Width=\"56\"",
            "                    Height=\"56\"",
            "                    CornerRadius=\"18\"",
            "                    Background=\"{Binding Background}\">",
            "                <atom:TextBlock Text=\"{Binding Text}\"",
            "                                Foreground=\"{Binding Foreground}\"",
            "                                FontSize=\"18\"",
            "                                FontWeight=\"Bold\"",
            "                                HorizontalAlignment=\"Center\"",
            "                                VerticalAlignment=\"Center\" />",
            "            </Border>",
            "        </DataTemplate>",
            "    </atom:Splash.LogoTemplate>",
            "    <atom:Splash.FooterTemplate>",
            "        <DataTemplate x:DataType=\"vm:SplashFooterInfo\">",
            "            <StackPanel Orientation=\"Horizontal\"",
            "                        Spacing=\"8\"",
            "                        VerticalAlignment=\"Center\">",
            "                <atom:Tag Text=\"{Binding Version}\"",
            "                          TagColor=\"geekblue\" />",
            "                <atom:TextBlock Text=\"{Binding Description}\"",
            "                                Foreground=\"{atom:SharedTokenResource ColorTextTertiary}\"",
            "                                VerticalAlignment=\"Center\" />",
            "            </StackPanel>",
            "        </DataTemplate>",
            "    </atom:Splash.FooterTemplate>",
            "</atom:Splash>");
    }

    private static void ShowDrawerContentInWindow(ShowCaseCodeSnippetGroup group,
                                                  Action<GalleryShowCaseCodeDrawerContent> verify)
    {
        var key = new ShowCaseCodeSnippetKey("Demo", "ExamplesContent", 0);
        ShowDrawerContentInWindow(group, key, verify);
    }

    private static void ShowDrawerContentInWindow(ShowCaseCodeSnippetGroup? group,
                                                  ShowCaseCodeSnippetKey key,
                                                  Action<GalleryShowCaseCodeDrawerContent> verify)
    {
        var content = new GalleryShowCaseCodeDrawerContent(group, key);
        var window = new Window
        {
            Width = 640,
            Height = 480,
            Content = content
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            content.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();
            verify(content);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
            content.Dispose();
        }
    }

    private static void ShowInWindow(GalleryCodeViewer viewer, Action<TextEditor> verify, Control? content = null)
    {
        var window = new Window
        {
            Width = 640,
            Height = 480,
            Content = content ?? viewer
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            viewer.ApplyTemplate();

            var editor = viewer.GetVisualDescendants()
                               .OfType<TextEditor>()
                               .Single();
            editor.ApplyTemplate();

            Dispatcher.UIThread.RunJobs();
            verify(editor);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
            viewer.Dispose();
        }
    }

    private static async Task ShowInWindowAsync(GalleryCodeViewer viewer,
                                                Func<TextEditor, Task> verify,
                                                Control? content = null)
    {
        var window = new Window
        {
            Width = 640,
            Height = 480,
            Content = content ?? viewer
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            viewer.ApplyTemplate();

            var editor = viewer.GetVisualDescendants()
                               .OfType<TextEditor>()
                               .Single();
            editor.ApplyTemplate();

            Dispatcher.UIThread.RunJobs();
            await verify(editor);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
            viewer.Dispose();
        }
    }

    private static async Task WaitForClipboardTextAsync(Control control, string expectedText)
    {
        var clipboard = TopLevel.GetTopLevel(control)?.Clipboard;
        clipboard.ShouldNotBeNull();

        string? actualText = null;
        for (var i = 0; i < 20; i++)
        {
            Dispatcher.UIThread.RunJobs();
            using var dataTransfer = await clipboard!.TryGetInProcessDataAsync();
            actualText = dataTransfer is null
                ? null
                : await dataTransfer.TryGetValueAsync(DataFormat.Text);
            if (actualText == expectedText)
            {
                return;
            }

            await Task.Delay(10);
        }

        actualText.ShouldBe(expectedText);
    }

    private static void OpenSearchPanel(TextEditor editor, RoutedCommand command)
    {
        command.Execute(null, editor.TextArea);
        Dispatcher.UIThread.RunJobs();
        editor.SearchPanel.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();
    }

    private static string GetTextMateThemeColor(GalleryCodeViewer viewer, string colorKey)
    {
        var installationField = typeof(GalleryCodeViewer).GetField(
            "_textMateInstallation",
            BindingFlags.Instance | BindingFlags.NonPublic);
        installationField.ShouldNotBeNull();

        var installation = installationField.GetValue(viewer);
        installation.ShouldNotBeNull();

        var arguments = new object?[] { colorKey, null };
        var result = installation.GetType()
                                 .GetMethod("TryGetThemeColor")!
                                 .Invoke(installation, arguments);
        result.ShouldBe(true);

        return arguments[1].ShouldBeOfType<string>();
    }

    private static ThemeName GetCurrentSyntaxTheme(GalleryCodeViewer viewer)
    {
        var syntaxThemeField = typeof(GalleryCodeViewer).GetField(
            "_currentSyntaxTheme",
            BindingFlags.Instance | BindingFlags.NonPublic);
        syntaxThemeField.ShouldNotBeNull();

        return syntaxThemeField.GetValue(viewer).ShouldBeOfType<ThemeName>();
    }

    private static Avalonia.Controls.ScrollViewer GetEditorScrollViewer(TextEditor editor)
    {
        var scrollViewerProperty = typeof(TextEditor).GetProperty(
            "ScrollViewer",
            BindingFlags.Instance | BindingFlags.NonPublic);
        scrollViewerProperty.ShouldNotBeNull();

        return scrollViewerProperty.GetValue(editor).ShouldBeOfType<Avalonia.Controls.ScrollViewer>();
    }

    private static string? GetAxamlTagNameColor(GalleryCodeViewer viewer)
    {
        return GetAxamlTokenColor(viewer, "StackPanel");
    }

    private static string? GetAxamlTokenColor(GalleryCodeViewer viewer, string tokenText)
    {
        const string lineText = "<StackPanel Margin=\"20\" Spacing=\"12\">";

        var registryOptions = new RegistryOptions(GetCurrentSyntaxTheme(viewer));
        var registry = new Registry(registryOptions);
        var grammar = registry.LoadGrammar(registryOptions.GetScopeByExtension(".xml"));
        var theme = registry.GetTheme();
        var tokens = grammar.TokenizeLine(lineText).Tokens;

        var token = tokens.Single(token => lineText.Substring(token.StartIndex, token.Length) == tokenText);
        var foregroundColorId = 0;
        foreach (var rule in theme.Match(token.Scopes))
        {
            if (foregroundColorId == 0 && rule.foreground > 0)
            {
                foregroundColorId = rule.foreground;
            }
        }

        return foregroundColorId > 0 ? theme.GetColor(foregroundColorId) : null;
    }

    private static Color GetVisibleTextForegroundColor(TextEditor editor, string text)
    {
        var textView = editor.TextArea.TextView;
        textView.EnsureVisualLines();
        var document = editor.Document.ShouldNotBeNull();
        var visibleText = new List<string>();
        foreach (var visualLine in textView.VisualLines)
        {
            var lineStartOffset = visualLine.FirstDocumentLine.Offset;
            foreach (var element in visualLine.Elements)
            {
                if (element.DocumentLength <= 0)
                {
                    continue;
                }

                var elementText = document.GetText(lineStartOffset + element.RelativeTextOffset, element.DocumentLength);
                visibleText.Add(elementText);
                if (!elementText.Contains(text, StringComparison.Ordinal))
                {
                    continue;
                }

                return element.TextRunProperties
                              .ForegroundBrush
                              .ShouldBeAssignableTo<ISolidColorBrush>()
                              .Color;
            }
        }

        throw new InvalidOperationException($"Visible text '{text}' was not found. Elements: {string.Join("|", visibleText)}");
    }

    private static string[] WaitForLineTokenScopes(GalleryCodeViewer viewer,
                                                   int lineIndex,
                                                   Func<string, bool> expectedScope)
    {
        string[] scopes = [];
        var timeout = TimeSpan.FromSeconds(5);
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < timeout)
        {
            viewer.ApplyTemplate();
            var editor = viewer.GetVisualDescendants()
                               .OfType<TextEditor>()
                               .SingleOrDefault();
            if (editor is not null)
            {
                editor.ApplyTemplate();
                editor.TextArea.TextView.EnsureVisualLines();
            }

            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs(DispatcherPriority.Background);
            Dispatcher.UIThread.RunJobs(DispatcherPriority.SystemIdle);

            scopes = GetLineTokenScopes(viewer, lineIndex);
            if (scopes.Any(expectedScope))
            {
                return scopes;
            }

            Thread.Sleep(10);
        }

        return scopes;
    }

    private static string[] GetLineTokenScopes(GalleryCodeViewer viewer, int lineIndex)
    {
        var installation = GetTextMateInstallation(viewer);
        var tmModel = GetTextMateModel(installation);

        var tokens = tmModel.GetType()
                            .GetMethod("GetLineTokens")!
                            .Invoke(tmModel, new object[] { lineIndex });
        if (tokens is null)
        {
            return [];
        }

        return tokens.ShouldBeAssignableTo<IEnumerable>()
                     .Cast<object>()
                     .SelectMany(token =>
                     {
                         var scopesProperty = token.GetType().GetProperty("Scopes");
                         scopesProperty.ShouldNotBeNull();
                         return scopesProperty.GetValue(token)
                                              .ShouldNotBeNull()
                                              .ShouldBeAssignableTo<IEnumerable>()
                                              .Cast<string>();
                     })
                     .ToArray();
    }

    private static bool IsLineInvalid(GalleryCodeViewer viewer, int lineIndex)
    {
        var installation = GetTextMateInstallation(viewer);
        var tmModel = GetTextMateModel(installation);
        return tmModel.GetType()
                      .GetMethod("IsLineInvalid")!
                      .Invoke(tmModel, new object[] { lineIndex })
                      .ShouldBeOfType<bool>();
    }

    private static void ClearLineTokens(GalleryCodeViewer viewer, int lineIndex)
    {
        var installation = GetTextMateInstallation(viewer);
        var tmModel = GetTextMateModel(installation);

        var lines = tmModel.GetType()
                           .GetMethod("GetLines")!
                           .Invoke(tmModel, Array.Empty<object>());
        lines.ShouldNotBeNull();

        var modelLine = lines.GetType()
                             .GetMethod("Get")!
                             .Invoke(lines, new object[] { lineIndex });
        modelLine.ShouldNotBeNull();

        modelLine.GetType()
                 .GetProperty("Tokens")!
                 .SetValue(modelLine, null);
        modelLine.GetType()
                 .GetProperty("IsInvalid")!
                 .SetValue(modelLine, true);
    }

    private static object GetTextMateInstallation(GalleryCodeViewer viewer)
    {
        var installationField = typeof(GalleryCodeViewer).GetField(
            "_textMateInstallation",
            BindingFlags.Instance | BindingFlags.NonPublic);
        installationField.ShouldNotBeNull();

        var installation = installationField.GetValue(viewer);
        installation.ShouldNotBeNull();
        return installation;
    }

    private static object GetTextMateModel(object installation)
    {
        var tmModelField = installation.GetType()
                                       .GetField("_tmModel", BindingFlags.Instance | BindingFlags.NonPublic);
        tmModelField.ShouldNotBeNull();

        var tmModel = tmModelField.GetValue(installation);
        tmModel.ShouldNotBeNull();
        return tmModel;
    }

    private static string GetTextMateDiagnostics(GalleryCodeViewer viewer, int lineIndex)
    {
        var editor = viewer.GetVisualDescendants()
                           .OfType<TextEditor>()
                           .SingleOrDefault();
        var installationField = typeof(GalleryCodeViewer).GetField(
            "_textMateInstallation",
            BindingFlags.Instance | BindingFlags.NonPublic);
        var installation = installationField?.GetValue(viewer);
        var grammarField = installation?.GetType()
                                        .GetField("_grammar", BindingFlags.Instance | BindingFlags.NonPublic);
        var tmModelField = installation?.GetType()
                                       .GetField("_tmModel", BindingFlags.Instance | BindingFlags.NonPublic);
        var grammar = grammarField?.GetValue(installation);
        var tmModel = tmModelField?.GetValue(installation);
        var tmGrammar = tmModel?.GetType()
                                .GetMethod("GetGrammar")
                                ?.Invoke(tmModel, Array.Empty<object>());
        var installationGrammar = grammar as IGrammar;
        var modelGrammar = tmGrammar as IGrammar;
        var isStopped = tmModel?.GetType()
                               .GetProperty("IsStopped")
                               ?.GetValue(tmModel);
        var isInvalid = tmModel?.GetType()
                               .GetMethod("IsLineInvalid")
                               ?.Invoke(tmModel, new object[] { lineIndex });
        var text = editor?.Text ?? viewer.CodeText ?? string.Empty;
        return string.Join(
            "; ",
            $"Language={viewer.Language}",
            $"TextLength={text.Length}",
            $"TextPrefix={text[..Math.Min(text.Length, 40)]}",
            $"InstallationGrammar={installationGrammar?.GetScopeName()}",
            $"InstallationGrammarIsCompiling={installationGrammar?.IsCompiling}",
            $"TMGrammar={modelGrammar?.GetScopeName()}",
            $"TMGrammarIsCompiling={modelGrammar?.IsCompiling}",
            $"IsStopped={isStopped}",
            $"IsLineInvalid={isInvalid}");
    }

    private static string GetVisibleTokenizationState(GalleryCodeViewer viewer)
    {
        var installation = GetTextMateInstallation(viewer);
        var type = installation.GetType();
        return string.Join(
            "; ",
            $"PendingStart={type.GetField("_pendingVisibleStartLine", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(installation)}",
            $"PendingEnd={type.GetField("_pendingVisibleEndLine", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(installation)}",
            $"Queued={type.GetField("_isVisibleLineTokenizationQueued", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(installation)}",
            $"Retry={type.GetField("_visibleLineTokenizationRetryCancellation", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(installation) is not null}");
    }

    private sealed class AlwaysCompilingGrammar : IGrammar
    {
        public bool IsCompiling => true;

        public string GetName()
        {
            return "Always compiling";
        }

        public string GetScopeName()
        {
            return "source.always-compiling";
        }

        public ICollection<string> GetFileTypes()
        {
            return Array.Empty<string>();
        }

        public ITokenizeLineResult TokenizeLine(LineText lineText)
        {
            throw new InvalidOperationException("The compiling grammar must not be tokenized.");
        }

        public ITokenizeLineResult TokenizeLine(LineText lineText, IStateStack prevState, TimeSpan timeLimit)
        {
            throw new InvalidOperationException("The compiling grammar must not be tokenized.");
        }

        public ITokenizeLineResult2 TokenizeLine2(LineText lineText)
        {
            throw new InvalidOperationException("The compiling grammar must not be tokenized.");
        }

        public ITokenizeLineResult2 TokenizeLine2(LineText lineText, IStateStack prevState, TimeSpan timeLimit)
        {
            throw new InvalidOperationException("The compiling grammar must not be tokenized.");
        }
    }

    private sealed class ControlledCompilingGrammar : IGrammar
    {
        private readonly IGrammar _innerGrammar;

        public ControlledCompilingGrammar(IGrammar innerGrammar)
        {
            _innerGrammar = innerGrammar;
        }

        public bool IsCompiling { get; set; } = true;

        public string GetName()
        {
            return _innerGrammar.GetName();
        }

        public string GetScopeName()
        {
            return _innerGrammar.GetScopeName();
        }

        public ICollection<string> GetFileTypes()
        {
            return _innerGrammar.GetFileTypes();
        }

        public ITokenizeLineResult TokenizeLine(LineText lineText)
        {
            return _innerGrammar.TokenizeLine(lineText);
        }

        public ITokenizeLineResult TokenizeLine(LineText lineText, IStateStack prevState, TimeSpan timeLimit)
        {
            return _innerGrammar.TokenizeLine(lineText, prevState, timeLimit);
        }

        public ITokenizeLineResult2 TokenizeLine2(LineText lineText)
        {
            return _innerGrammar.TokenizeLine2(lineText);
        }

        public ITokenizeLineResult2 TokenizeLine2(LineText lineText, IStateStack prevState, TimeSpan timeLimit)
        {
            return _innerGrammar.TokenizeLine2(lineText, prevState, timeLimit);
        }
    }
}
