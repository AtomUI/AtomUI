using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUI.Localization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Presenters;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIButton = AtomUI.Desktop.Controls.Button;
using AtomUIDescriptions = AtomUI.Desktop.Controls.Descriptions;
using AtomUISteps = AtomUI.Desktop.Controls.Steps;
using AtomUIToolTip = AtomUI.Desktop.Controls.ToolTip;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUI.Toolkits.GalleryBase.Tests.Controls;

public class SemanticPartPreviewTests
{
    public SemanticPartPreviewTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void LineEdit_Resolves_Highlight_Targets_For_Every_Declared_Part()
    {
        var lineEdit = new AtomUI.Desktop.Controls.LineEdit
        {
            Width             = 420,
            Text              = "atomui",
            InnerLeftContent  = "https://",
            InnerRightContent = ".com",
            IsAllowClear      = true,
            IsShowCount       = true,
            MaxLength         = 20
        };
        var preview = CreatePreviewCore(lineEdit, typeof(AtomUI.Desktop.Controls.LineEdit));
        foreach (var path in new[] { "root", "prefix", "input", "suffix", "clear", "count" })
        {
            preview.PartDescriptions.Add(new SemanticPartDescription
            {
                Path        = path,
                Description = path + " part."
            });
        }

        var window = new AtomUIWindow { Width = 640, Height = 240, Content = preview };
        window.Show();
        preview.ActivatePreview();
        Dispatcher.UIThread.RunJobs();

        foreach (var item in preview.Items)
        {
            preview.SetHoveredPart(item, true);
            Dispatcher.UIThread.RunJobs();
            var session = preview.ActiveHighlightSession.ShouldNotBeNull();
            session.TotalMatchCount.ShouldBe(1,
                $"part '{item.Name}' must resolve exactly one highlight target inside the preview owner.");
            session.HighlightedTargetCount.ShouldBe(1);
            session.IsTruncated.ShouldBeFalse();
            preview.SetHoveredPart(item, false);
            Dispatcher.UIThread.RunJobs();
            (preview.ActiveHighlightSession?.HighlightedTargetCount ?? 0).ShouldBe(0);
        }
        window.Close();
    }

    [Fact]
    public void Activate_Builds_Descriptor_Items_Without_Creating_A_Highlight_Session()
    {
        var button = new AtomUIButton
        {
            Content   = "Semantic Button",
            IsLoading = true
        };
        var preview = CreatePreview(button);

        preview.Items.ShouldBeEmpty();
        preview.ActiveHighlightSession.ShouldBeNull();
        preview.CodeViewer.ShouldBeNull();

        using var context = ShowInWindow(preview);
        preview.ActivatePreview();

        preview.Items.Select(static item => item.Path)
               .ShouldBe(["root", "icon", "content"]);
        preview.ActiveHighlightSession.ShouldBeNull();
        preview.CodeViewer.ShouldBeNull();
        AdornerLayer.GetAdornerLayer(button).ShouldNotBeNull()
                    .Children.OfType<SemanticPartAdorner>().ShouldBeEmpty();
    }

    [Fact]
    public void Activate_Uses_Explicit_Description_Order_Then_Appends_Undescribed_Parts()
    {
        var button = new AtomUIButton
        {
            Content   = "Semantic Button",
            IsLoading = true
        };
        var preview = new SemanticPartPreview
        {
            PreviewContent    = button,
            SemanticOwnerType = typeof(AtomUIButton)
        };
        preview.PartDescriptions.Add(new SemanticPartDescription
        {
            Path        = "root",
            Description = "The Button root."
        });
        preview.PartDescriptions.Add(new SemanticPartDescription
        {
            Path        = "icon",
            Description = "The icon region."
        });

        using var context = ShowInWindow(preview);
        preview.ActivatePreview();

        preview.Items.Select(static item => item.Path)
               .ShouldBe(["root", "icon", "content"]);
    }

    [Fact]
    public void Title_Renders_Inside_The_Preview_Stage_Only_When_Provided()
    {
        var preview = CreatePreview(new AtomUIButton
        {
            Content = "Semantic Button"
        });
        preview.Title = "TabItem";

        using var context = ShowInWindow(preview);
        preview.ActivatePreview();

        var title = preview.GetVisualDescendants()
                           .OfType<TextBlock>()
                           .Single(static textBlock => textBlock.Name == "PART_Title");
        title.Text.ShouldBe("TabItem");
        title.IsVisible.ShouldBeTrue();
        title.FindAncestorOfType<Border>()!.Name.ShouldBe("PART_PreviewStage");

        preview.Title = null;
        Dispatcher.UIThread.RunJobs();

        title.IsVisible.ShouldBeFalse();
    }

    [Fact]
    public void Title_Does_Not_Change_The_Parts_Pane_Viewport()
    {
        var preview = CreatePreview(new AtomUIButton
        {
            Content = "Semantic Button"
        });

        using var context = ShowInWindow(preview);
        preview.ActivatePreview();
        Dispatcher.UIThread.RunJobs();

        var pane = preview.GetVisualDescendants()
                          .OfType<Border>()
                          .Single(static border => border.Name == "PART_PartsPane");
        var scroller = pane.GetVisualDescendants()
                           .OfType<AtomUI.Desktop.Controls.ScrollViewer>()
                           .Single();
        var paneBoundsWithoutTitle = pane.Bounds;
        var viewportWithoutTitle = scroller.Viewport;

        preview.Title = "Button";
        Dispatcher.UIThread.RunJobs();

        pane.Bounds.ShouldBe(paneBoundsWithoutTitle);
        scroller.Viewport.ShouldBe(viewportWithoutTitle);
    }

    [Fact]
    public void Effective_Part_Uses_Pin_Over_Hover_And_Deactivate_Releases_The_Session()
    {
        var button = new AtomUIButton
        {
            Content   = "Semantic Button",
            IsLoading = true
        };
        var preview = CreatePreview(button);

        using var context = ShowInWindow(preview);
        preview.ActivatePreview();
        var root = preview.Items.Single(static item => item.Path == "root");
        var content = preview.Items.Single(static item => item.Path == "content");
        var icon = preview.Items.Single(static item => item.Path == "icon");

        root.StyleType.ShouldBe("-");
        content.StyleType.ShouldBe("AtomUI.Theme.Styling.ButtonContentStyle");
        icon.StyleType.ShouldBe("AtomUI.Theme.Styling.ButtonIconStyle");

        preview.SetHoveredPart(icon, true);
        Dispatcher.UIThread.RunJobs();
        preview.EffectivePart.ShouldBe(icon);
        preview.ActiveHighlightSession.ShouldNotBeNull();

        preview.TogglePinnedPart(content);
        Dispatcher.UIThread.RunJobs();
        preview.EffectivePart.ShouldBe(content);
        content.IsPinned.ShouldBeTrue();

        preview.SetHoveredPart(root, true);
        Dispatcher.UIThread.RunJobs();
        preview.EffectivePart.ShouldBe(content);

        preview.DeactivatePreview();
        Dispatcher.UIThread.RunJobs();
        preview.ActiveHighlightSession.ShouldBeNull();
        AdornerLayer.GetAdornerLayer(button).ShouldNotBeNull()
                    .Children.OfType<SemanticPartAdorner>().ShouldBeEmpty();
    }

    [Fact]
    public void Info_Lazily_Creates_The_Code_Viewer_Without_Target_Resolution()
    {
        var button = new AtomUIButton
        {
            Content = "Semantic Button"
        };
        var preview = CreatePreview(button);

        using var context = ShowInWindow(preview);
        preview.ActivatePreview();
        var content = preview.Items.Single(static item => item.Path == "content");
        preview.CodeViewer.ShouldBeNull();

        preview.ShowPartInfo(content);

        preview.CodeViewer.ShouldNotBeNull();
        var code = preview.CodeViewer.CodeText.ShouldNotBeNull();
        code.ShouldContain("xmlns:atom=\"https://atomui.net\"");
        code.ShouldContain("<Style Selector=\"atom|Button\">");
        code.ShouldContain("<atom:ButtonContentStyle x:SetterTargetType=\"ContentPresenter\">");
        code.ShouldContain("<Setter Property=\"Opacity\" Value=\"1\" />");
        code.ShouldNotContain("Selector=\"atom|Button /template/ .semantic-content\"");
        code.ShouldNotContain("x:SetterTargetType=\"contract:ContentPresenter\"");
        code.ShouldNotContain("xmlns:owner=");
        code.ShouldNotContain(":is(");

        preview.ShowPartInfo(preview.Items.Single(static item => item.Path == "root"));
        var rootCode = preview.CodeViewer.CodeText.ShouldNotBeNull();
        rootCode.ShouldContain("Selector=\"atom|Button\"");
        rootCode.ShouldContain("x:SetterTargetType=\"atom:Button\"");
        preview.ActiveHighlightSession.ShouldBeNull();
    }

    [Fact]
    public void Runtime_Created_Part_Code_Uses_The_Descriptor_Owner_Relative_Selector_Route()
    {
        var descriptions = new AtomUIDescriptions();
        descriptions.Items.Add(new AtomUI.Desktop.Controls.DescriptionItem
        {
            Label = "Product",
            Content = "Cloud Database"
        });
        var preview = new SemanticPartPreview
        {
            PreviewContent = descriptions,
            SemanticOwnerType = typeof(AtomUIDescriptions)
        };

        using var context = ShowInWindow(preview);
        preview.ActivatePreview();
        preview.ShowPartInfo(preview.Items.Single(static item => item.Path == "label"));

        var code = preview.CodeViewer.ShouldNotBeNull().CodeText.ShouldNotBeNull();
        code.ShouldContain("<Style Selector=\"atom|Descriptions\">");
        code.ShouldContain("<atom:DescriptionsLabelStyle x:SetterTargetType=\"ContentPresenter\">");
        code.ShouldNotContain("Selector=\"atom|Descriptions /template/ .semantic-scope-items > .semantic-scope-item /template/ .semantic-label\"");
        code.ShouldNotContain("x:SetterTargetType=\"contract:ContentPresenter\"");
        code.ShouldNotContain(":is(");
    }

    [Fact]
    public void Template_Uses_A_Single_Flat_Inspection_Panel()
    {
        var preview = CreatePreview(new AtomUIButton
        {
            Content = "Semantic Button"
        });

        using var context = ShowInWindow(preview);
        preview.ActivatePreview();
        preview.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var frame = preview.GetVisualDescendants()
                           .OfType<Border>()
                           .Single(static border => border.Name == "PART_InspectionPanel");
        frame.VerticalAlignment.ShouldBe(VerticalAlignment.Top);
        frame.BorderThickness.ShouldBe(new Thickness(1));
        frame.CornerRadius.ShouldBe(default);

        var layout = preview.GetVisualDescendants()
                            .OfType<SemanticPartPreviewLayoutPanel>()
                            .Single(static panel => panel.Name == "PART_Layout");
        layout.Spacing.ShouldBe(0);
        layout.Children.Count.ShouldBe(2);

        var previewStage = layout.Children[0].ShouldBeOfType<Border>();
        previewStage.Name.ShouldBe("PART_PreviewStage");
        previewStage.Background.ShouldNotBeNull();
        previewStage.BorderThickness.ShouldBe(default);
        previewStage.CornerRadius.ShouldBe(default);
        var previewStageLayout = previewStage.Child.ShouldBeOfType<Grid>();
        var previewTitle = previewStageLayout.Children[0].ShouldBeOfType<TextBlock>();
        previewTitle.Name.ShouldBe("PART_Title");
        var previewPresenter = previewStageLayout.Children[1].ShouldBeOfType<ContentPresenter>();
        previewPresenter.HorizontalAlignment.ShouldBe(HorizontalAlignment.Stretch);
        previewPresenter.HorizontalContentAlignment.ShouldBe(HorizontalAlignment.Stretch);
        previewPresenter.VerticalAlignment.ShouldBe(VerticalAlignment.Stretch);
        previewPresenter.VerticalContentAlignment.ShouldBe(preview.PreviewContentAlignment);
        preview.PreviewContentAlignment.ShouldBe(VerticalAlignment.Center);

        var partsPane = layout.Children[1].ShouldBeOfType<Border>();
        partsPane.Name.ShouldBe("PART_PartsPane");
        partsPane.ClipToBounds.ShouldBeTrue();
        partsPane.BorderThickness.ShouldBe(new Thickness(1, 0, 0, 0));
        partsPane.CornerRadius.ShouldBe(default);

        var rows = preview.GetVisualDescendants()
                          .OfType<SemanticPartPreviewItemControl>()
                          .ToArray();
        rows.Length.ShouldBe(3);
        preview.GetVisualDescendants()
               .OfType<AtomUI.Desktop.Controls.Tag>()
               .ShouldBeEmpty();
        preview.Items.Select(static item => item.Since)
               .ShouldAllBe(static since => since == "6.2.0");
        foreach (var row in rows)
        {
            var rowBorder = row.GetVisualDescendants()
                               .OfType<Border>()
                               .Single(static border => border.Name == "PART_Row");
            rowBorder.BorderThickness.ShouldBe(new Thickness(0, 0, 0, 1));
            rowBorder.CornerRadius.ShouldBe(default);
            AtomUIToolTip.GetTip(rowBorder).ShouldBeNull();
        }

        var persistentTexts = rows.SelectMany(static row => row.GetVisualDescendants().OfType<TextBlock>())
                                  .Select(static textBlock => textBlock.Text)
                                  .Where(static text => text is not null)
                                  .ToArray();
        persistentTexts.ShouldNotContain(".semantic-content");
        persistentTexts.ShouldNotContain(nameof(ContentPresenter));
    }

    [Fact]
    public void Info_Lazily_Separates_Technical_Metadata_From_The_Main_Row()
    {
        var preview = CreatePreview(new AtomUIButton
        {
            Content = "Semantic Button"
        });

        using var context = ShowInWindow(preview);
        preview.ActivatePreview();
        var content = preview.Items.Single(static item => item.Path == "content");

        preview.ShowPartInfo(content);
        Dispatcher.UIThread.RunJobs();

        var infoContent = preview.InfoContent.ShouldNotBeNull();
        infoContent.ShouldNotBeOfType<GalleryCodeViewer>();
        var metadataValues = infoContent.GetVisualDescendants()
                                        .OfType<TextBlock>()
                                        .Select(static textBlock => textBlock.Text)
                                        .Where(static text => text is not null)
                                        .ToArray();
        metadataValues.ShouldContain(content.Selector);
        metadataValues.ShouldContain(content.SelectorRoute);
        metadataValues.ShouldContain(content.ContractType);
        metadataValues.ShouldContain(content.StyleType);
        metadataValues.ShouldContain(content.Cardinality);
        metadataValues.ShouldContain(content.Customization);
        var metadataGrid = infoContent.GetVisualDescendants()
                                      .OfType<Grid>()
                                      .Single();
        metadataGrid.Children
                    .OfType<TextBlock>()
                    .ShouldAllBe(static textBlock =>
                        textBlock.VerticalAlignment == VerticalAlignment.Center);
        infoContent.GetVisualDescendants()
                   .OfType<GalleryCodeViewer>()
                   .ShouldBeEmpty();
        preview.CodeViewer.ShouldNotBeNull();
        preview.ActiveHighlightSession.ShouldBeNull();
    }

    [Fact]
    public void Part_Details_And_Code_Are_Hosted_Side_By_Side_Below_The_Preview()
    {
        var preview = CreatePreview(new AtomUIButton
        {
            Content = "Semantic Button"
        });

        using var context = ShowInWindow(preview);
        preview.ActivatePreview();
        preview.ShowPartInfo(preview.Items.Single(static item => item.Path == "content"));
        preview.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var frame = preview.GetVisualDescendants()
                           .OfType<Border>()
                           .Single(static border => border.Name == "PART_InspectionPanel");
        var rootLayout = frame.Child.ShouldBeOfType<Grid>();
        rootLayout.RowDefinitions.Count.ShouldBe(2);

        var title = frame.GetVisualDescendants()
                         .OfType<TextBlock>()
                         .Single(static textBlock => textBlock.Name == "PART_Title");
        title.IsVisible.ShouldBeFalse();
        title.FindAncestorOfType<Border>()!.Name.ShouldBe("PART_PreviewStage");

        var layout = rootLayout.Children
                               .OfType<SemanticPartPreviewLayoutPanel>()
                               .Single(static panel => panel.Name == "PART_Layout");
        var detailsPane = rootLayout.Children
                                    .OfType<Border>()
                                    .Single(static border => border.Name == "PART_DetailsPane");
        var infoPane = detailsPane.GetVisualDescendants()
                                  .OfType<Border>()
                                  .Single(static border => border.Name == "PART_InfoPane");
        var infoPresenter = infoPane.GetVisualDescendants()
                                    .OfType<ContentPresenter>()
                                    .Single(static presenter => presenter.Name == "PART_InfoContent");
        var codePresenter = detailsPane.GetVisualDescendants()
                                       .OfType<ContentPresenter>()
                                       .Single(static presenter => presenter.Name == "PART_CodeContent");

        Grid.GetRow(layout).ShouldBe(0);
        Grid.GetRow(detailsPane).ShouldBe(1);
        detailsPane.Bounds.Width.ShouldBe(layout.Bounds.Width, 0.01);
        infoPane.Width.ShouldBe(340);
        DockPanel.GetDock(infoPane).ShouldBe(Dock.Left);
        infoPane.BorderThickness.ShouldBe(new Thickness(0, 0, 1, 0));
        infoPresenter.Bounds.Width.ShouldBe(
            infoPane.Bounds.Width - infoPane.BorderThickness.Left - infoPane.BorderThickness.Right,
            0.01);
        codePresenter.Bounds.Width.ShouldBe(
            detailsPane.Bounds.Width - infoPane.Bounds.Width,
            0.01);
        codePresenter.Bounds.Width.ShouldBeGreaterThan(infoPresenter.Bounds.Width);
        infoPresenter.GetVisualDescendants()
                     .OfType<GalleryCodeViewer>()
                     .ShouldBeEmpty();
        codePresenter.GetVisualDescendants()
                     .OfType<GalleryCodeViewer>()
                     .ShouldHaveSingleItem();
    }

    [Fact]
    public void Activate_Rejects_A_Description_For_An_Unknown_Part_Path()
    {
        var preview = CreatePreview(new AtomUIButton { Content = "Semantic Button" });
        preview.PartDescriptions.Add(new SemanticPartDescription
        {
            Path        = "missing",
            Description = "Unknown"
        });

        Should.Throw<InvalidOperationException>(() => preview.ActivatePreview())
              .Message.ShouldContain("missing");
    }

    [Fact]
    public void Activate_Uses_The_Current_Language_For_Fallback_Descriptions_Without_Row_Status_Tooltips()
    {
        var languageManager = Application.Current.ShouldNotBeNull()
                                         .GetLanguageManager().ShouldNotBeNull();
        var button = new AtomUIButton { Content = "Semantic Button" };
        var preview = new SemanticPartPreview
        {
            PreviewContent    = button,
            SemanticOwnerType = typeof(AtomUIButton)
        };

        try
        {
            languageManager.ChangeLanguage(LanguageTags.ZhCN);
            using var context = ShowInWindow(preview);
            preview.ActivatePreview();

            var root = preview.Items.Single(static item => item.Path == "root");
            root.Description.ShouldBe("控件根区域。");

            preview.SetHoveredPart(root, true);
            Dispatcher.UIThread.RunJobs();

            var rootRow = preview.GetVisualDescendants()
                                 .OfType<SemanticPartPreviewItemControl>()
                                 .Single(row => ReferenceEquals(row.Item, root));
            var rowBorder = rootRow.GetVisualDescendants()
                                   .OfType<Border>()
                                   .Single(static border => border.Name == "PART_Row");
            AtomUIToolTip.GetTip(rowBorder).ShouldBeNull();
        }
        finally
        {
            languageManager.ChangeLanguage(LanguageTags.EnUS);
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Multiple_Owner_Instances_Highlight_All_Instances_Of_The_Selected_Part()
    {
        var buttonA = new AtomUIButton
        {
            Content = "Semantic A"
        };
        var buttonB = new AtomUIButton
        {
            Content = "Semantic B"
        };
        var panel = new StackPanel
        {
            Children =
            {
                buttonA,
                buttonB
            }
        };
        var preview = new SemanticPartPreview
        {
            PreviewContent    = panel,
            SemanticOwner     = buttonA,
            SemanticOwnerType = typeof(AtomUIButton)
        };
        preview.PartDescriptions.Add(new SemanticPartDescription
        {
            Path        = "root",
            Description = "The Button root."
        });
        preview.PartDescriptions.Add(new SemanticPartDescription
        {
            Path        = "icon",
            Description = "The icon region."
        });
        preview.PartDescriptions.Add(new SemanticPartDescription
        {
            Path        = "content",
            Description = "The content region."
        });

        using var context = ShowInWindow(preview);
        preview.ActivatePreview();
        var content = preview.Items.Single(static item => item.Path == "content");
        preview.SetHoveredPart(content, true);
        Dispatcher.UIThread.RunJobs();

        var session = preview.ActiveHighlightSession.ShouldNotBeNull();
        session.TotalMatchCount.ShouldBe(2);
        session.HighlightedTargetCount.ShouldBe(2);
    }

    [Fact]
    public void Measure_Uses_The_Available_Height_Or_Default_Cap_For_The_Parts_Pane()
    {
        var preview = new SemanticPartPreview
        {
            PreviewContent = new AtomUISteps(),
            SemanticOwnerType = typeof(AtomUISteps)
        };

        using var context = ShowInWindow(preview);
        preview.ActivatePreview();
        Dispatcher.UIThread.RunJobs();

        var layout = preview.GetVisualDescendants()
                            .OfType<SemanticPartPreviewLayoutPanel>()
                            .Single(static panel => panel.Name == "PART_Layout");
        var pane = layout.Children[1];

        layout.Measure(new Size(900, 500));
        pane.DesiredSize.Height.ShouldBeGreaterThan(410);
        pane.DesiredSize.Height.ShouldBeLessThanOrEqualTo(500);

        layout.Measure(new Size(900, double.PositiveInfinity));
        pane.DesiredSize.Height.ShouldBe(layout.PaneMaxHeight, 0.01);

        layout.Measure(new Size(600, 500));
        var previewStage = layout.Children[0];
        pane.DesiredSize.Height.ShouldBeLessThan(300);
        pane.DesiredSize.Height.ShouldBe(
            Math.Max(0, 500 - previewStage.DesiredSize.Height - layout.Spacing), 0.01);
    }

    private static SemanticPartPreview CreatePreview(Avalonia.Controls.Control content, Type ownerType)
    {
        return CreatePreviewCore(content, ownerType);
    }

    [Fact]
    public void Multi_Owner_Preview_Merges_Part_Lists_In_Declaration_Order()
    {
        // Message 场景：单个预览同时承载列表宿主与消息卡片两个 owner。合并后 Part 列表顺序 = owner
        // 声明顺序，重名路径（两侧都有 root）靠 OwnerType 消歧，且高亮按 Part 所属 owner 解析。
        var manager = new AtomUI.Desktop.Controls.WindowMessageManager(null) { IsMotionEnabled = false };
        var card = new AtomUI.Desktop.Controls.MessageCard
        {
            MessageType     = AtomUI.Desktop.Controls.MessageType.Success,
            Message         = "Merged",
            IsMotionEnabled = false
        };
        // 与页面一致：manager 拉伸铺满舞台，卡片叠在舞台内居中；垂直 StackPanel 会把
        // 拉伸的 manager 压成 0 高，导致 root 目标不可解析。
        var stage = new Panel
        {
            Children =
            {
                manager,
                new StackPanel
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment   = VerticalAlignment.Center,
                    Children            = { card }
                }
            }
        };
        var preview = new SemanticPartPreview { PreviewContent = stage };
        preview.SemanticOwners.Add(new SemanticPartPreviewOwner
        {
            Owner     = manager,
            OwnerType = typeof(AtomUI.Desktop.Controls.WindowMessageManager)
        });
        preview.SemanticOwners.Add(new SemanticPartPreviewOwner
        {
            Owner     = card,
            OwnerType = typeof(AtomUI.Desktop.Controls.MessageCard)
        });
        preview.PartDescriptions.Add(new SemanticPartDescription
        {
            OwnerType = typeof(AtomUI.Desktop.Controls.WindowMessageManager),
            Path      = "root",
            Description = "manager root"
        });
        preview.PartDescriptions.Add(new SemanticPartDescription
        {
            OwnerType = typeof(AtomUI.Desktop.Controls.WindowMessageManager),
            Path      = "listContent",
            Description = "list content"
        });
        preview.PartDescriptions.Add(new SemanticPartDescription
        {
            OwnerType = typeof(AtomUI.Desktop.Controls.MessageCard),
            Path      = "root",
            Description = "card root"
        });
        preview.PartDescriptions.Add(new SemanticPartDescription
        {
            OwnerType = typeof(AtomUI.Desktop.Controls.MessageCard),
            Path      = "icon",
            Description = "card icon"
        });

        var window = new AtomUIWindow { Width = 640, Height = 400, Content = preview };
        window.Show();
        preview.ActivatePreview();
        Dispatcher.UIThread.RunJobs();

        // manager 声明在前 -> 它的 Part 先入列表。
        preview.Items.Select(static item => (item.OwnerType.Name, item.Path)).ShouldBe(
        [
            ("WindowMessageManager", "root"),
            ("WindowMessageManager", "listContent"),
            ("MessageCard", "root"),
            ("MessageCard", "icon"),
            ("MessageCard", "title"),
            ("MessageCard", "wrapper")
        ]);
        // 重名 root 的描述按 owner 取到各自条目。
        preview.Items.Where(static item => item.Path == "root")
               .Select(static item => item.Description)
               .ShouldBe(["manager root", "card root"]);
        preview.Items.ShouldAllBe(static item => item.IsOwnerLabelVisible);

        // 高亮按 Part 所属 owner 解析：manager 的 root 命中 manager 自身，card 的 icon 命中卡片模板图标。
        var managerRoot = preview.Items.Single(static item =>
            item.OwnerType.Name == "WindowMessageManager" && item.Path == "root");
        preview.TogglePinnedPart(managerRoot);
        Dispatcher.UIThread.RunJobs();
        preview.ActiveHighlightSession.ShouldNotBeNull().TotalMatchCount.ShouldBe(1);

        var cardIcon = preview.Items.Single(static item =>
            item.OwnerType.Name == "MessageCard" && item.Path == "icon");
        preview.TogglePinnedPart(cardIcon);
        Dispatcher.UIThread.RunJobs();
        preview.ActiveHighlightSession.ShouldNotBeNull().TotalMatchCount.ShouldBe(1);

        window.Close();
    }

    [Fact]
    public void Multi_Owner_Description_Without_OwnerType_Is_Rejected()
    {
        var manager = new AtomUI.Desktop.Controls.WindowMessageManager(null);
        var card = new AtomUI.Desktop.Controls.MessageCard();
        var preview = new SemanticPartPreview { PreviewContent = new StackPanel { Children = { manager, card } } };
        preview.SemanticOwners.Add(new SemanticPartPreviewOwner
        {
            Owner = manager, OwnerType = typeof(AtomUI.Desktop.Controls.WindowMessageManager)
        });
        preview.SemanticOwners.Add(new SemanticPartPreviewOwner
        {
            Owner = card, OwnerType = typeof(AtomUI.Desktop.Controls.MessageCard)
        });
        preview.PartDescriptions.Add(new SemanticPartDescription { Path = "root" });

        var window = new AtomUIWindow { Width = 640, Height = 400, Content = preview };
        window.Show();
        var exception = Should.Throw<InvalidOperationException>(() => preview.ActivatePreview());
        exception.Message.ShouldContain("OwnerType");
        window.Close();
    }

    private static SemanticPartPreview CreatePreviewCore(Avalonia.Controls.Control content, Type ownerType)
    {
        var preview = new SemanticPartPreview
        {
            PreviewContent    = content,
            SemanticOwnerType = ownerType
        };
        return preview;
    }

    private static SemanticPartPreview CreatePreview(AtomUIButton button)
    {
        var preview = CreatePreviewCore(button, typeof(AtomUIButton));
        preview.PartDescriptions.Add(new SemanticPartDescription
        {
            Path        = "root",
            Description = "The Button root."
        });
        preview.PartDescriptions.Add(new SemanticPartDescription
        {
            Path        = "icon",
            Description = "The icon region."
        });
        preview.PartDescriptions.Add(new SemanticPartDescription
        {
            Path        = "content",
            Description = "The content region."
        });
        return preview;
    }

    private static WindowContext ShowInWindow(SemanticPartPreview preview)
    {
        var window = new AtomUIWindow
        {
            Width   = 1000,
            Height  = 700,
            Content = preview
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();
        return new WindowContext(window);
    }

    private sealed class WindowContext : IDisposable
    {
        private readonly AtomUIWindow _window;

        public WindowContext(AtomUIWindow window)
        {
            _window = window;
        }

        public void Dispose()
        {
            _window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
