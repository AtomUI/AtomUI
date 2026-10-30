using AtomUI.Controls;
using AtomUI.Desktop.Controls.Primitives.Themes;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Input;

public class AddOnContentTemplateTests
{
    static AddOnContentTemplateTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void TemplateOnly_AddOnDecoratedBox_Slots_Create_Visible_Children()
    {
        var decoratedBox = new AddOnDecoratedBox
        {
            Width                       = 320,
            Content                     = new Border { Width = 120, Height = 32 },
            LeftAddOnTemplate           = CreateTemplate("left"),
            RightAddOnTemplate          = CreateTemplate("right"),
            ContentLeftAddOnTemplate    = CreateTemplate("content-left"),
            ContentRightAddOnTemplate   = CreateTemplate("content-right")
        };

        ShowInWindow(decoratedBox, () =>
        {
            AssertPresenterChild(decoratedBox, AddOnDecoratedBoxThemeConstants.LeftAddOnPresenterPart, "left");
            AssertPresenterChild(decoratedBox, AddOnDecoratedBoxThemeConstants.RightAddOnPresenterPart, "right");
            AssertPresenterChild(decoratedBox, AddOnDecoratedBoxThemeConstants.ContentLeftAddOnPart, "content-left");
            AssertPresenterChild(decoratedBox, AddOnDecoratedBoxThemeConstants.ContentRightAddOnPart, "content-right");

            FindNamedControl(decoratedBox, AddOnDecoratedBoxThemeConstants.LeftAddOnPart).IsVisible.ShouldBeTrue();
            FindNamedControl(decoratedBox, AddOnDecoratedBoxThemeConstants.RightAddOnPart).IsVisible.ShouldBeTrue();
        });
    }

    [Fact]
    public void LineEdit_TemplateOnly_InnerRightContent_Creates_ToolTip_Host()
    {
        var lineEdit = new LineEdit
        {
            Width                     = 240,
            InnerRightContentTemplate = CreateTemplate("line-edit-right", "line edit tip")
        };

        ShowInWindow(lineEdit, () =>
        {
            var child = AssertPresenterChild(lineEdit, "InnerRightContentPresenter", "line-edit-right");
            ToolTip.GetTip(child).ShouldBe("line edit tip");
        });
    }

    [Fact]
    public void LineEdit_TemplateOnly_InnerLeftContent_Creates_Prefix_Content()
    {
        var lineEdit = new LineEdit
        {
            Width                    = 240,
            InnerLeftContentTemplate = CreateTemplate("line-edit-left")
        };

        ShowInWindow(lineEdit, () =>
        {
            var prefix = lineEdit.GetVisualDescendants()
                                 .OfType<ContentPresenter>()
                                 .Single(static presenter =>
                                     presenter.Classes.Contains("semantic-prefix"));
            prefix.IsVisible.ShouldBeTrue();
            prefix.Child.ShouldNotBeNull();
            prefix.Child!.Tag.ShouldBe("line-edit-left");
        });
    }

    [Fact]
    public void TextArea_InnerRightContent_And_Template_Are_Visible()
    {
        var directContent = new Border { Tag = "direct-right" };
        var textArea = new TextArea
        {
            Width                     = 240,
            Height                    = 100,
            InnerRightContent         = directContent,
            InnerRightContentTemplate = CreateTemplate("template-right")
        };

        ShowInWindow(textArea, () =>
        {
            var presenter = FindPresenter(textArea, "PART_InnerRightContentPresenter");
            presenter.IsVisible.ShouldBeTrue();
            presenter.Content.ShouldBeSameAs(directContent);
            presenter.ContentTemplate.ShouldBeSameAs(textArea.InnerRightContentTemplate);
        });
    }

    [Fact]
    public void ButtonSpinner_TemplateOnly_InnerContent_Creates_Both_Children()
    {
        var spinner = new ButtonSpinner
        {
            Width                     = 240,
            Content                   = "value",
            InnerLeftContentTemplate  = CreateTemplate("spinner-left"),
            InnerRightContentTemplate = CreateTemplate("spinner-right")
        };

        ShowInWindow(spinner, () =>
        {
            AssertPresenterChild(spinner, AddOnDecoratedBoxThemeConstants.ContentLeftAddOnPart, "spinner-left");
            AssertPresenterChild(spinner, AddOnDecoratedBoxThemeConstants.ContentRightAddOnPart, "spinner-right");
        });
    }

    [Fact]
    public void DatePicker_TemplateOnly_LeftAddOn_Uses_LeftAddOnTemplate()
    {
        var datePicker = new DatePicker
        {
            Width             = 240,
            LeftAddOnTemplate = CreateTemplate("date-picker-left")
        };

        ShowInWindow(datePicker, () =>
        {
            AssertPresenterChild(datePicker, AddOnDecoratedBoxThemeConstants.LeftAddOnPresenterPart,
                "date-picker-left");
            FindNamedControl(datePicker, AddOnDecoratedBoxThemeConstants.LeftAddOnPart).IsVisible.ShouldBeTrue();
        });
    }

    [Fact]
    public void SearchEdit_TemplateOnly_InnerContent_Creates_Both_Children()
    {
        var searchEdit = new SearchEdit
        {
            Width                     = 240,
            InnerLeftContentTemplate  = CreateTemplate("search-left"),
            InnerRightContentTemplate = CreateTemplate("search-right")
        };

        ShowInWindow(searchEdit, () =>
        {
            AssertMarkerVisible(searchEdit, "search-left");
            AssertMarkerVisible(searchEdit, "search-right");
        });
    }

    [Fact]
    public void TemplateOnly_Content_Works_Across_AddOnDecoratedBox_Consumers()
    {
        var controls = new Control[]
        {
            new AtomUI.Desktop.Controls.ComboBox
            {
                ContentRightAddOnTemplate = CreateTemplate("combo-box-right")
            },
            new Select
            {
                ContentRightAddOnTemplate = CreateTemplate("select-right")
            },
            new TreeSelect
            {
                ContentRightAddOnTemplate = CreateTemplate("tree-select-right")
            },
            new AtomUI.Desktop.Controls.Cascader
            {
                ContentRightAddOnTemplate = CreateTemplate("cascader-right")
            },
            new DatePicker
            {
                ContentRightAddOnTemplate = CreateTemplate("date-picker-right")
            },
            new AtomUI.Desktop.Controls.NumericUpDown
            {
                InnerRightContentTemplate = CreateTemplate("numeric-up-down-right")
            }
        };

        foreach (var control in controls)
        {
            control.Width = 240;
            ShowInWindow(control, () => AssertMarkerVisible(control, GetExpectedMarker(control)));
        }
    }

    [Fact]
    public void TemplateOnly_Content_Works_Through_AutoComplete_And_Mentions_Wrappers()
    {
        var controls = new Control[]
        {
            new AtomUI.Desktop.Controls.AutoComplete
            {
                ContentLeftAddOnTemplate  = CreateTemplate("auto-complete-left"),
                ContentRightAddOnTemplate = CreateTemplate("auto-complete-right")
            },
            new AutoCompleteSearchEdit
            {
                ContentLeftAddOnTemplate  = CreateTemplate("auto-search-left"),
                ContentRightAddOnTemplate = CreateTemplate("auto-search-right")
            },
            new AutoCompleteTextArea
            {
                ContentLeftAddOnTemplate  = CreateTemplate("auto-text-area-left"),
                ContentRightAddOnTemplate = CreateTemplate("auto-text-area-right")
            },
            new Mentions
            {
                ContentLeftAddOnTemplate  = CreateTemplate("mentions-left"),
                ContentRightAddOnTemplate = CreateTemplate("mentions-right")
            }
        };

        var expectedMarkers = new[]
        {
            new[] { "auto-complete-left", "auto-complete-right" },
            new[] { "auto-search-left", "auto-search-right" },
            new[] { "auto-text-area-left", "auto-text-area-right" },
            new[] { "mentions-left", "mentions-right" }
        };

        for (var index = 0; index < controls.Length; index++)
        {
            var control = controls[index];
            control.Width = 240;
            ShowInWindow(control, () =>
            {
                foreach (var marker in expectedMarkers[index])
                {
                    AssertMarkerVisible(control, marker);
                }
            });
        }
    }

    [Fact]
    public void Runtime_Template_Changes_Update_Child_Visibility_And_CornerRadius()
    {
        var decoratedBox = new AddOnDecoratedBox
        {
            Width             = 240,
            CornerRadius      = new Avalonia.CornerRadius(8),
            StyleVariant      = InputControlStyleVariant.Outlined,
            Content           = new Border { Width = 120, Height = 32 },
            LeftAddOnTemplate = CreateTemplate("runtime-first")
        };

        ShowInWindow(decoratedBox, () =>
        {
            AssertPresenterChild(decoratedBox, AddOnDecoratedBoxThemeConstants.LeftAddOnPresenterPart,
                "runtime-first");
            decoratedBox.InnerBoxCornerRadius.TopLeft.ShouldBe(0);
            decoratedBox.InnerBoxCornerRadius.BottomLeft.ShouldBe(0);

            decoratedBox.LeftAddOnTemplate = CreateTemplate("runtime-second");
            Dispatcher.UIThread.RunJobs();
            AssertPresenterChild(decoratedBox, AddOnDecoratedBoxThemeConstants.LeftAddOnPresenterPart,
                "runtime-second");

            decoratedBox.LeftAddOnTemplate = null;
            Dispatcher.UIThread.RunJobs();

            var presenter = FindPresenter(decoratedBox,
                AddOnDecoratedBoxThemeConstants.LeftAddOnPresenterPart);
            presenter.Child.ShouldBeNull();
            presenter.IsVisible.ShouldBeFalse();
            FindNamedControl(decoratedBox, AddOnDecoratedBoxThemeConstants.LeftAddOnPart)
                .IsVisible.ShouldBeFalse();
            decoratedBox.InnerBoxCornerRadius.TopLeft.ShouldBe(8);
            decoratedBox.InnerBoxCornerRadius.BottomLeft.ShouldBe(8);
        });
    }

    private static IDataTemplate CreateTemplate(string marker, string? toolTip = null)
    {
        return new FuncDataTemplate(_ => true, (_, _) =>
        {
            var child = new Border
            {
                Width  = 16,
                Height = 16,
                Tag    = marker
            };
            if (toolTip is not null)
            {
                ToolTip.SetTip(child, toolTip);
            }
            return child;
        });
    }

    private static Control AssertPresenterChild(Control owner, string presenterName, string marker)
    {
        var presenter = FindPresenter(owner, presenterName);
        presenter.IsVisible.ShouldBeTrue();
        presenter.Child.ShouldNotBeNull();
        presenter.Child!.Tag.ShouldBe(marker);
        return presenter.Child;
    }

    private static void AssertMarkerVisible(Control owner, string marker)
    {
        owner.GetVisualDescendants()
             .OfType<Control>()
             .Single(control => Equals(control.Tag, marker))
             .IsVisible.ShouldBeTrue();
    }

    private static string GetExpectedMarker(Control control)
    {
        return control switch
        {
            AtomUI.Desktop.Controls.ComboBox => "combo-box-right",
            Select                           => "select-right",
            TreeSelect                       => "tree-select-right",
            AtomUI.Desktop.Controls.Cascader => "cascader-right",
            DatePicker                       => "date-picker-right",
            AtomUI.Desktop.Controls.NumericUpDown => "numeric-up-down-right",
            _ => throw new ArgumentOutOfRangeException(nameof(control))
        };
    }

    private static ContentPresenter FindPresenter(Control owner, string name)
    {
        return owner.GetVisualDescendants()
                    .OfType<ContentPresenter>()
                    .Single(item => item.Name == name);
    }

    private static Control FindNamedControl(Control owner, string name)
    {
        return owner.GetVisualDescendants()
                    .OfType<Control>()
                    .Single(item => item.Name == name);
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 420,
            Height  = 180,
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
