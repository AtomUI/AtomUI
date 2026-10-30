using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AvaloniaTextBox = Avalonia.Controls.TextBox;
using AtomUIScrollViewer = AtomUI.Desktop.Controls.ScrollViewer;
using AtomUITextArea = AtomUI.Desktop.Controls.TextArea;
using AtomUITextBox = AtomUI.Desktop.Controls.TextBox;

namespace AtomUI.Desktop.Controls.Tests.Tooltip;

public class OverflowTipTests
{
    static OverflowTipTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void OverflowTip_Default_Placement_Is_Top_Edge_Aligned_Left()
    {
        var textBlock = new TextBlock();

        OverflowTip.GetPlacement(textBlock).ShouldBe(PlacementMode.TopEdgeAlignedLeft);
    }

    [Fact]
    public void TextBlock_Uses_ToolTip_Only_When_Text_Overflows()
    {
        const string value = "AtomUI overflow tooltip should show this complete text";
        var textBlock = new TextBlock
        {
            Width        = 80,
            Text         = value,
            TextTrimming = Avalonia.Media.TextTrimming.CharacterEllipsis
        };

        OverflowTip.SetIsEnabled(textBlock, true);
        OverflowTip.SetShowDelay(textBlock, 1500);

        ShowInWindow(textBlock, () =>
        {
            ToolTip.GetTip(textBlock).ShouldBe(value);
            ToolTip.GetShowDelay(textBlock).ShouldBe(1500);

            textBlock.Width = 1000;
            Dispatcher.UIThread.RunJobs();

            ToolTip.GetTip(textBlock).ShouldBeNull();
        });
    }

    [Fact]
    public void AtomUI_TextBox_Uses_Visible_Text_Viewport_For_Overflow()
    {
        const string value = "AtomUI text input overflow must use the visible text viewport";
        var textBox = new AtomUITextBox
        {
            Width = 100,
            Text  = value
        };

        OverflowTip.SetIsEnabled(textBox, true);

        ShowInWindow(textBox, () => ToolTip.GetTip(textBox).ShouldBe(value));
    }

    [Fact]
    public void AtomUI_TextArea_Publishes_Visible_Text_Viewport_Width()
    {
        var textArea = new AtomUITextArea
        {
            Width = 160,
            Text  = "AtomUI text area viewport"
        };

        ShowInWindow(textArea, () =>
        {
            var scrollViewer = textArea.GetVisualDescendants()
                                       .OfType<AtomUIScrollViewer>()
                                       .Single(item => item.Name == "PART_ScrollViewer");
            var textPresenter = textArea.GetVisualDescendants()
                                        .OfType<TextPresenter>()
                                        .Single(item => item.Name == "PART_TextPresenter");

            scrollViewer.Padding = new Thickness(3, 0, 7, 0);
            textPresenter.Margin = new Thickness(5, 0, 11, 0);
            Dispatcher.UIThread.RunJobs();

            var viewportWidth = TextViewportMetrics.GetViewportWidth(textArea);
            var expectedWidth = scrollViewer.Viewport.Width -
                                scrollViewer.Padding.Left - scrollViewer.Padding.Right -
                                textPresenter.Margin.Left - textPresenter.Margin.Right;

            viewportWidth.ShouldNotBeNull();
            viewportWidth.Value.ShouldBe(expectedWidth, 0.001);
        });
    }

    [Fact]
    public void AtomUI_TextBox_Reevaluates_When_Viewport_Padding_Changes_Without_Owner_Resize()
    {
        const string value = "AtomUI";
        var textBox = new AtomUITextBox
        {
            Width = 160,
            Text  = value
        };

        OverflowTip.SetIsEnabled(textBox, true);

        ShowInWindow(textBox, () =>
        {
            ToolTip.GetTip(textBox).ShouldBeNull();
            var ownerBounds = textBox.Bounds;
            var scrollViewer = textBox.GetVisualDescendants()
                                      .OfType<AtomUIScrollViewer>()
                                      .Single(item => item.Name == "ScrollViewer");
            var viewportWidth = scrollViewer.Viewport.Width;

            scrollViewer.Padding = new Thickness(0, 0, viewportWidth - 12, 0);
            Dispatcher.UIThread.RunJobs();

            textBox.Bounds.ShouldBe(ownerBounds);
            ToolTip.GetTip(textBox).ShouldBe(value);
        });
    }

    [Fact]
    public void AtomUI_TextBox_Releases_Previous_Viewport_Source_On_Template_Reapply()
    {
        var textBox = new AtomUITextBox
        {
            Width = 160,
            Text  = "AtomUI"
        };

        OverflowTip.SetIsEnabled(textBox, true);

        ShowInWindow(textBox, () =>
        {
            var oldScrollViewer = textBox.GetVisualDescendants()
                                             .OfType<AtomUIScrollViewer>()
                                             .Single(item => item.Name == "ScrollViewer");
            var template = textBox.Template;
            template.ShouldNotBeNull();

            textBox.Template = null;
            Dispatcher.UIThread.RunJobs();
            textBox.Template = template;
            Dispatcher.UIThread.RunJobs();

            var newScrollViewer = textBox.GetVisualDescendants()
                                             .OfType<AtomUIScrollViewer>()
                                             .Single(item => item.Name == "ScrollViewer");
            ReferenceEquals(oldScrollViewer, newScrollViewer).ShouldBeFalse();
            var currentWidth = TextViewportMetrics.GetViewportWidth(textBox);

            oldScrollViewer.Padding = new Thickness(0, 0, oldScrollViewer.Viewport.Width - 4, 0);
            Dispatcher.UIThread.RunJobs();

            TextViewportMetrics.GetViewportWidth(textBox).ShouldBe(currentWidth);
            ToolTip.GetTip(textBox).ShouldBeNull();
        });
    }

    [Fact]
    public void AtomUI_TextArea_Releases_Previous_Viewport_Source_On_Template_Reapply()
    {
        var textArea = new AtomUITextArea
        {
            Width = 160,
            Text  = "AtomUI"
        };

        ShowInWindow(textArea, () =>
        {
            var oldScrollViewer = textArea.GetVisualDescendants()
                                          .OfType<AtomUIScrollViewer>()
                                          .Single(item => item.Name == "PART_ScrollViewer");
            var template = textArea.Template;
            template.ShouldNotBeNull();

            textArea.Template = null;
            Dispatcher.UIThread.RunJobs();
            textArea.Template = template;
            Dispatcher.UIThread.RunJobs();

            var newScrollViewer = textArea.GetVisualDescendants()
                                          .OfType<AtomUIScrollViewer>()
                                          .Single(item => item.Name == "PART_ScrollViewer");
            ReferenceEquals(oldScrollViewer, newScrollViewer).ShouldBeFalse();
            var currentWidth = TextViewportMetrics.GetViewportWidth(textArea);

            oldScrollViewer.Padding = new Thickness(0, 0, oldScrollViewer.Viewport.Width - 4, 0);
            Dispatcher.UIThread.RunJobs();

            TextViewportMetrics.GetViewportWidth(textArea).ShouldBe(currentWidth);
        });
    }

    [Fact]
    public void Avalonia_TextBox_Uses_Geometry_Fallback_When_No_Viewport_Metric_Is_Published()
    {
        const string value = "Third-party Avalonia TextBox fallback must remain available";
        var textBox = new AvaloniaTextBox
        {
            Width = 80,
            Text  = value
        };

        OverflowTip.SetIsEnabled(textBox, true);

        ShowInWindow(textBox, () =>
        {
            TextViewportMetrics.GetViewportWidth(textBox).ShouldBeNull();
            ToolTip.GetTip(textBox).ShouldBe(value);
        });
    }

    [Fact]
    public void OverflowTip_Does_Not_Query_Input_Template_Internals()
    {
        var source = ReadRepoFile("src/AtomUI.Desktop.Controls/Tooltip/OverflowTip.cs");

        source.ShouldNotContain("GetVisualDescendants()");
        source.ShouldNotContain("PART_TextPresenter");
    }

    [Fact]
    public void User_Defined_ToolTip_Is_Not_Overwritten_Or_Cleared()
    {
        const string value = "AtomUI overflow tooltip should not replace manual tooltip";
        var textBlock = new TextBlock
        {
            Width        = 60,
            Text         = value,
            TextTrimming = Avalonia.Media.TextTrimming.CharacterEllipsis
        };

        ToolTip.SetTip(textBlock, "manual tip");
        OverflowTip.SetIsEnabled(textBlock, true);
        OverflowTip.SetShowDelay(textBlock, 1600);

        ShowInWindow(textBlock, () =>
        {
            ToolTip.GetTip(textBlock).ShouldBe("manual tip");

            textBlock.Width = 1000;
            Dispatcher.UIThread.RunJobs();

            ToolTip.GetTip(textBlock).ShouldBe("manual tip");
        });
    }

    [Fact]
    public void ContentPresenter_Uses_Clipping_Ancestor_Width_When_Own_Bounds_Are_Wider()
    {
        const string value = "Selected content can be wider than the clipped input surface";
        var presenter = new ContentPresenter
        {
            Width               = 420,
            Content             = value,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
        };
        var panel = new Panel
        {
            Width        = 90,
            ClipToBounds = true
        };
        panel.Children.Add(presenter);

        OverflowTip.SetIsEnabled(presenter, true);
        OverflowTip.SetText(presenter, value);

        ShowInWindow(panel, () => ToolTip.GetTip(presenter).ShouldBe(value));
    }

    [Fact]
    public void PlacementTarget_Offsets_TopLeft_Tip_To_Target_Left_Edge()
    {
        const string value = "Jiangsu/Nanjing/Zhong Hua Men";
        var textBlock = new TextBlock
        {
            Width        = 80,
            Margin       = new Thickness(20, 0, 0, 0),
            Text         = value,
            TextTrimming = Avalonia.Media.TextTrimming.CharacterEllipsis
        };
        var target = new Panel
        {
            Width  = 240,
            Height = 48
        };
        target.Children.Add(textBlock);

        OverflowTip.SetIsEnabled(textBlock, true);
        OverflowTip.SetText(textBlock, value);
        OverflowTip.SetPlacement(textBlock, PlacementMode.TopEdgeAlignedLeft);

        var setPlacementTarget = typeof(OverflowTip).GetMethod(
            "SetPlacementTarget",
            BindingFlags.Public | BindingFlags.Static);
        setPlacementTarget.ShouldNotBeNull();
        setPlacementTarget!.Invoke(null, [textBlock, target]);

        ShowInWindow(target, () =>
        {
            ToolTip.GetTip(textBlock).ShouldBe(value);
            ToolTip.GetHorizontalOffset(textBlock).ShouldBe(-textBlock.Bounds.X, 0.5);
        });
    }

    [Fact]
    public void Select_Multiple_Tag_Receives_OverflowTip_Settings_From_Owner()
    {
        var option = new SelectOption
        {
            Header  = "Long selected option label",
            Content = "long"
        };
        var select = new Select
        {
            Width              = 140,
            Mode               = SelectMode.Multiple,
            IsFilterEnabled    = false,
            SelectedOptions    = [option],
            IsShowOverflowTip  = true,
            OverflowTipDelay   = 1700
        };
        var placementProperty = select.GetType().GetProperty("OverflowTipPlacement");
        placementProperty.ShouldNotBeNull();
        placementProperty.SetValue(select, PlacementMode.BottomEdgeAlignedRight);

        ShowInWindow(select, () =>
        {
            var tag = select.GetVisualDescendants()
                            .OfType<SelectTag>()
                            .Single(item => item.Text == "Long selected option label");

            OverflowTip.GetIsEnabled(tag).ShouldBeTrue();
            OverflowTip.GetText(tag).ShouldBe(tag.Text);
            OverflowTip.GetShowDelay(tag).ShouldBe(1700);
            OverflowTip.GetPlacement(tag).ShouldBe(PlacementMode.BottomEdgeAlignedRight);
        });
    }

    [Fact]
    public void Target_Controls_Expose_Public_OverflowTip_Properties()
    {
        var abstractSelect = ReadRepoFile("src/AtomUI.Desktop.Controls/Select/AbstractSelect.cs");
        var comboBox       = ReadRepoFile("src/AtomUI.Desktop.Controls/ComboBox/ComboBox.cs");

        abstractSelect.ShouldContain("IsShowOverflowTipProperty");
        abstractSelect.ShouldContain("OverflowTipDelayProperty");
        abstractSelect.ShouldContain("OverflowTipPlacementProperty");
        comboBox.ShouldContain("IsShowOverflowTipProperty");
        comboBox.ShouldContain("OverflowTipDelayProperty");
        comboBox.ShouldContain("OverflowTipPlacementProperty");
    }

    [Fact]
    public void Target_Control_Themes_Attach_OverflowTip_To_Display_Nodes()
    {
        var select     = ReadRepoFile("src/AtomUI.Desktop.Controls/Select/Themes/SelectTheme.axaml");
        var cascader   = ReadRepoFile("src/AtomUI.Desktop.Controls/Cascader/Themes/CascaderTheme.axaml");
        var treeSelect = ReadRepoFile("src/AtomUI.Desktop.Controls/TreeSelect/Themes/TreeSelectTheme.axaml");
        var comboBox   = ReadRepoFile("src/AtomUI.Desktop.Controls/ComboBox/Themes/ComboBoxTheme.axaml");

        select.ShouldContain("atom:OverflowTip.IsEnabled=\"{TemplateBinding IsShowOverflowTip}\"");
        select.ShouldContain("atom:OverflowTip.ShowDelay=\"{TemplateBinding OverflowTipDelay}\"");
        select.ShouldContain("atom:OverflowTip.Placement=\"{TemplateBinding OverflowTipPlacement}\"");
        select.ShouldContain("atom:OverflowTip.Text=\"{Binding Text, RelativeSource={RelativeSource Self}}\"");
        select.ShouldContain("IsShowOverflowTip=\"{TemplateBinding IsShowOverflowTip}\"");
        select.ShouldContain("OverflowTipDelay=\"{TemplateBinding OverflowTipDelay}\"");
        select.ShouldContain("OverflowTipPlacement=\"{TemplateBinding OverflowTipPlacement}\"");
        select.ShouldContain("atom:OverflowTip.PlacementTarget=\"{Binding $parent[atom:SelectAddOnDecoratedBox]}\"");

        cascader.ShouldContain("atom:OverflowTip.IsEnabled=\"{TemplateBinding IsShowOverflowTip}\"");
        cascader.ShouldContain("atom:OverflowTip.Text=\"{TemplateBinding SelectedOptionPath}\"");
        cascader.ShouldContain("atom:OverflowTip.Placement=\"{TemplateBinding OverflowTipPlacement}\"");
        cascader.ShouldContain("IsShowOverflowTip=\"{TemplateBinding IsShowOverflowTip}\"");
        cascader.ShouldContain("OverflowTipDelay=\"{TemplateBinding OverflowTipDelay}\"");
        cascader.ShouldContain("OverflowTipPlacement=\"{TemplateBinding OverflowTipPlacement}\"");
        cascader.ShouldContain("atom:OverflowTip.PlacementTarget=\"{Binding $parent[atom:CascaderAddOnDecoratedBox]}\"");

        treeSelect.ShouldContain("atom:OverflowTip.IsEnabled=\"{TemplateBinding IsShowOverflowTip}\"");
        treeSelect.ShouldContain("atom:OverflowTip.Text=\"{Binding Text, RelativeSource={RelativeSource Self}}\"");
        treeSelect.ShouldContain("atom:OverflowTip.Placement=\"{TemplateBinding OverflowTipPlacement}\"");
        treeSelect.ShouldContain("IsShowOverflowTip=\"{TemplateBinding IsShowOverflowTip}\"");
        treeSelect.ShouldContain("OverflowTipDelay=\"{TemplateBinding OverflowTipDelay}\"");
        treeSelect.ShouldContain("OverflowTipPlacement=\"{TemplateBinding OverflowTipPlacement}\"");
        treeSelect.ShouldContain("atom:OverflowTip.PlacementTarget=\"{Binding $parent[atom:TreeSelectAddOnDecoratedBox]}\"");

        comboBox.ShouldContain("atom:OverflowTip.IsEnabled=\"{TemplateBinding IsShowOverflowTip}\"");
        comboBox.ShouldContain("atom:OverflowTip.Text=\"{TemplateBinding SelectionBoxItem}\"");
        comboBox.ShouldContain("atom:OverflowTip.ShowDelay=\"{TemplateBinding OverflowTipDelay}\"");
        comboBox.ShouldContain("atom:OverflowTip.Placement=\"{TemplateBinding OverflowTipPlacement}\"");
        comboBox.ShouldContain("atom:OverflowTip.PlacementTarget=\"{Binding $parent[atom:AddOnDecoratedBox]}\"");
    }

    private static string ReadRepoFile(string relativePath)
    {
        return File.ReadAllText(Path.Combine(GetRepositoryRoot(), relativePath));
    }

    private static string GetRepositoryRoot()
    {
        var directory = AppContext.BaseDirectory;
        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory, "AtomUI.slnx")))
            {
                return directory;
            }

            directory = Directory.GetParent(directory)?.FullName;
        }

        throw new DirectoryNotFoundException("Unable to locate repository root.");
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 420,
            Height  = 320,
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
}
