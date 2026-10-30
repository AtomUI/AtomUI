using AtomUI.Controls.Primitives;
using AtomUI.Desktop.Controls.Tests.Window;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media.Transformation;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AtomUI.Animations;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Drawer;

public class DrawerInteractionTests
{
    static DrawerInteractionTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Drawer_Content_Popup_Uses_The_Window_TopLevel()
    {
        var comboBox = new AtomUI.Desktop.Controls.ComboBox
        {
            Width = 180,
            IsMotionEnabled = false
        };
        comboBox.Items.Add(new AtomUI.Desktop.Controls.ComboBoxItem { Content = "Alpha" });
        comboBox.Items.Add(new AtomUI.Desktop.Controls.ComboBoxItem { Content = "Beta" });
        var drawer = new AtomUI.Desktop.Controls.Drawer
        {
            Content = comboBox,
            IsMotionEnabled = false,
            Width = 1,
            Height = 1
        };

        var window = CreateAtomWindow(drawer);
        try
        {
            drawer.IsOpen = true;
            Dispatcher.UIThread.RunJobs();

            var container = window.GetVisualDescendants().OfType<DrawerContainer>().Single();
            TopLevel.GetTopLevel(comboBox).ShouldBeSameAs(window);

            comboBox.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);
            Dispatcher.UIThread.RunJobs();

            var popupHost = window.GetVisualDescendants()
                                  .OfType<OverlayPopupHost>()
                                  .Last(host => host.GetLogicalAncestors()
                                                    .OfType<AtomUI.Desktop.Controls.ComboBox>()
                                                    .Contains(comboBox));
            TopLevel.GetTopLevel(popupHost).ShouldBeSameAs(window);

            var secondItem = popupHost.GetVisualDescendants()
                                      .OfType<AtomUI.Desktop.Controls.ComboBoxItem>()
                                      .Single(item => item.Content?.ToString() == "Beta");
            ClickControl(window, secondItem);
            Dispatcher.UIThread.RunJobs();

            comboBox.SelectedIndex.ShouldBe(1);
            comboBox.IsDropDownOpen.ShouldBeFalse();

            drawer.IsOpen = false;
            Dispatcher.UIThread.RunJobs();
            container.Parent.ShouldBeNull();
        }
        finally
        {
            if (drawer.IsOpen)
            {
                drawer.IsOpen = false;
                Dispatcher.UIThread.RunJobs();
            }
            window.Close();
        }
    }

    [Fact]
    public void Drawer_Suppresses_Drawn_Chrome_Only_While_Open()
    {
        var drawer = new AtomUI.Desktop.Controls.Drawer
        {
            IsMotionEnabled = false,
            Width = 1,
            Height = 1
        };
        var window = CreateAtomWindow(drawer);
        try
        {
            window.IsDrawnChromeOverlayVisible.ShouldBeTrue();

            drawer.IsOpen = true;
            Dispatcher.UIThread.RunJobs();
            window.IsDrawnChromeOverlayVisible.ShouldBeFalse();

            drawer.IsOpen = false;
            Dispatcher.UIThread.RunJobs();
            window.IsDrawnChromeOverlayVisible.ShouldBeTrue();
        }
        finally
        {
            if (drawer.IsOpen)
            {
                drawer.IsOpen = false;
                Dispatcher.UIThread.RunJobs();
            }
            window.Close();
        }
    }

    [Fact]
    public void Drawer_Right_Button_Release_Inside_InfoContainer_Does_Not_Close()
    {
        var drawer = new AtomUI.Desktop.Controls.Drawer
        {
            Content              = new TextBlock { Text = "Body" },
            IsCloseOnMaskClick   = true,
            IsMotionEnabled      = false,
            Width                = 1,
            Height               = 1
        };

        var window = CreateWindow(drawer);
        try
        {
            drawer.IsOpen = true;
            Dispatcher.UIThread.RunJobs();

            var layer = ScopeAwareAdornerLayer.GetLayer(drawer);
            layer.ShouldNotBeNull();

            var infoContainer = layer.GetVisualDescendants()
                                     .OfType<DrawerInfoContainer>()
                                     .Single();

            RaisePointerReleased(infoContainer, MouseButton.Right, PointerUpdateKind.RightButtonReleased);
            Dispatcher.UIThread.RunJobs();

            drawer.IsOpen.ShouldBeTrue("right-clicking inside Drawer content must not be treated as a mask click.");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Drawer_Left_Button_Release_On_Mask_Closes_When_CloseOnMaskClick()
    {
        var drawer = new AtomUI.Desktop.Controls.Drawer
        {
            Content            = new TextBlock { Text = "Body" },
            IsCloseOnMaskClick = true,
            IsMotionEnabled    = false,
            Width              = 1,
            Height             = 1
        };

        var window = CreateWindow(drawer);
        try
        {
            drawer.IsOpen = true;
            Dispatcher.UIThread.RunJobs();

            var layer = ScopeAwareAdornerLayer.GetLayer(drawer);
            layer.ShouldNotBeNull();

            var mask = layer.GetVisualDescendants()
                            .OfType<Border>()
                            .Single(border => border.Name == "PART_Mask");

            RaisePointerReleased(mask, MouseButton.Left, PointerUpdateKind.LeftButtonReleased);
            Dispatcher.UIThread.RunJobs();

            drawer.IsOpen.ShouldBeFalse("left-clicking the mask should keep closing the Drawer.");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public async Task Reopening_Without_Motion_Clears_Completed_Close_Transform()
    {
        var drawer = new AtomUI.Desktop.Controls.Drawer
        {
            Content = new TextBlock { Text = "Body" },
            Width   = 1,
            Height  = 1
        };
        drawer.SetValue(AtomUI.Desktop.Controls.Drawer.IsMotionEnabledProperty, false, BindingPriority.Animation);

        var window = CreateWindow(drawer);
        try
        {
            drawer.IsOpen = true;
            Dispatcher.UIThread.RunJobs();

            var layer = ScopeAwareAdornerLayer.GetLayer(drawer);
            layer.ShouldNotBeNull();
            var container = layer.GetVisualDescendants().OfType<DrawerContainer>().Single();
            var motionActor = container.GetVisualDescendants().OfType<Border>().Single(control => control.Name == "PART_Surface");
            (await new MoveRightOutMotion(240, TimeSpan.Zero).PlayAsync(motionActor, TestContext.Current.CancellationToken))
                .IsCompleted.ShouldBeTrue();

            drawer.IsOpen = false;
            Dispatcher.UIThread.RunJobs();
            drawer.IsAttachedToVisualTree().ShouldBeTrue();
            container.GetVisualParent().ShouldBeNull();

            drawer.IsOpen = true;
            Dispatcher.UIThread.RunJobs();

            container.GetVisualParent().ShouldNotBeNull();
            var reopenedMotionActor = container.GetVisualDescendants().OfType<Border>().Single(control => control.Name == "PART_Surface");
            reopenedMotionActor.ShouldBeSameAs(motionActor);
            var matrix = reopenedMotionActor.RenderTransform?.Value ?? Matrix.Identity;
            matrix.M31.ShouldBe(0, 0.001);
            matrix.M32.ShouldBe(0, 0.001);

            var mask = container.GetVisualDescendants().OfType<Border>().Single(control => control.Name == "PART_Mask");
            (await new FadeOutMotion(TimeSpan.Zero).PlayAsync(reopenedMotionActor, TestContext.Current.CancellationToken)).IsCompleted.ShouldBeTrue();
            (await new FadeOutMotion(TimeSpan.Zero).PlayAsync(mask, TestContext.Current.CancellationToken)).IsCompleted.ShouldBeTrue();
            void FailOnce(object? sender, AvaloniaPropertyChangedEventArgs change)
            {
                if (change.Property == Visual.OpacityProperty && reopenedMotionActor.Opacity != 0)
                {
                    reopenedMotionActor.PropertyChanged -= FailOnce;
                    throw new InvalidOperationException("drawer-cleanup-failure");
                }
            }
            reopenedMotionActor.PropertyChanged += FailOnce;
            var failure = Should.Throw<Exception>(container.Release);
            failure.ToString().ShouldContain("drawer-cleanup-failure");
            container.GetVisualParent().ShouldBeNull();
            container.Content.ShouldBeNull();
            container.Drawer.ShouldBeNull();
            mask.Opacity.ShouldBe(1);
            drawer.Title = "late-title";
            container.Title.ShouldBeNull();
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [InlineData(DrawerPlacement.Left, true)]
    [InlineData(DrawerPlacement.Top, true)]
    [InlineData(DrawerPlacement.Right, true)]
    [InlineData(DrawerPlacement.Bottom, true)]
    [InlineData(DrawerPlacement.Left, false)]
    [InlineData(DrawerPlacement.Top, false)]
    [InlineData(DrawerPlacement.Right, false)]
    [InlineData(DrawerPlacement.Bottom, false)]
    public void TopLevel_Drawer_Uses_The_Window_Visible_Frame(
        DrawerPlacement placement,
        bool isCsdEnabled)
    {
        var drawer = new AtomUI.Desktop.Controls.Drawer
        {
            Content         = new TextBlock { Text = "Body" },
            DialogSize      = new Dimension(50, DimensionUnitType.Percentage),
            IsMotionEnabled = false,
            Placement       = placement,
            Width           = 1,
            Height          = 1
        };

        var window = CreateAtomWindow(drawer);
        try
        {
            var shadow = new Thickness(12, 18, 24, 30);
            var cornerRadius = new CornerRadius(9, 11, 13, 15);
            window.FrameShadowThickness = shadow;
            window.CornerRadius         = cornerRadius;
            window.IsCsdEnabled          = isCsdEnabled;
            Dispatcher.UIThread.RunJobs();

            // The visible frame belongs to the client surface and must remain
            // covered by the Drawer mask. Only the transparent shadow buffer
            // is excluded from the Drawer root bounds.
            window.VisibleFrameBorderThickness = new Thickness(3, 5, 7, 9);

            drawer.IsOpen = true;
            Dispatcher.UIThread.RunJobs();

            var layer = ScopeAwareAdornerLayer.GetLayer(drawer);
            layer.ShouldNotBeNull();
            var container = layer.GetVisualDescendants().OfType<DrawerContainer>().Single();

            container.Margin.ShouldBe(shadow);
            container.CornerRadius.ShouldBe(cornerRadius);

            var mask = container.GetVisualDescendants()
                                .OfType<Border>()
                                .Single(border => border.Name == "PART_Mask");
            var maskOrigin = mask.TranslatePoint(default, window).ShouldNotBeNull();
            var visibleFrameBounds = WindowVisualLayerClip.CalculateClipBounds(
                window.Bounds.Size,
                shadow);
            maskOrigin.X.ShouldBe(visibleFrameBounds.X, 0.001);
            maskOrigin.Y.ShouldBe(visibleFrameBounds.Y, 0.001);
            mask.Bounds.Size.ShouldBe(visibleFrameBounds.Size);

            window.CornerRadius = default;
            Dispatcher.UIThread.RunJobs();
            container.CornerRadius.ShouldBe(default);

            var visibleFrameInsets = new Thickness(
                shadow.Left + window.VisibleFrameBorderThickness.Left,
                shadow.Top + window.VisibleFrameBorderThickness.Top,
                shadow.Right + window.VisibleFrameBorderThickness.Right,
                shadow.Bottom + window.VisibleFrameBorderThickness.Bottom);
            var expectedReferenceSize = placement is DrawerPlacement.Top or DrawerPlacement.Bottom
                ? Math.Max(0, window.Bounds.Height - visibleFrameInsets.Top - visibleFrameInsets.Bottom)
                : Math.Max(0, window.Bounds.Width - visibleFrameInsets.Left - visibleFrameInsets.Right);
            drawer.EffectiveDialogSize.ShouldBe(expectedReferenceSize * 0.5, 0.001);

            container.IsMotionEnabled = false;
            drawer.IsOpen = false;
            Dispatcher.UIThread.RunJobs();
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Drawer_Content_Remains_In_The_TopLevel_Visual_Tree_When_Drawn_Decorations_Are_Available(
        bool isCsdEnabled)
    {
        var input = new AtomUI.Desktop.Controls.LineEdit();
        var drawer = new AtomUI.Desktop.Controls.Drawer
        {
            Content         = input,
            IsMotionEnabled = false,
            Width           = 1,
            Height          = 1
        };

        var window = CreateAtomWindow(drawer);
        var drawnHost = new ScopeAwareAdornerLayer
        {
            Name = "PART_DrawerOverlayLayerHost",
            LayerHost = window
        };
        Action? restoreDecorations = null;
        try
        {
            window.IsCsdEnabled = isCsdEnabled;
            restoreDecorations = DrawnDecorationsTestHost.Install(window, drawnHost);
            Dispatcher.UIThread.RunJobs();

            drawer.IsOpen = true;
            Dispatcher.UIThread.RunJobs();

            drawnHost.Children.OfType<DrawerContainer>().ShouldBeEmpty();
            TopLevel.GetTopLevel(input).ShouldBeSameAs(window);

            drawer.IsOpen = false;
            Dispatcher.UIThread.RunJobs();
            drawnHost.Children.OfType<DrawerContainer>().ShouldBeEmpty();
        }
        finally
        {
            if (drawer.IsOpen)
            {
                drawer.IsOpen = false;
                Dispatcher.UIThread.RunJobs();
            }
            restoreDecorations?.Invoke();
            window.Close();
        }
    }

    [Fact]
    public void Drawer_Inside_An_Existing_Scope_Layer_Reuses_The_Containing_Layer()
    {
        var openOn = new Border();
        var drawer = new AtomUI.Desktop.Controls.Drawer
        {
            OpenOn = openOn
        };
        var layer = new ScopeAwareAdornerLayer
        {
            LayerHost = openOn,
            Children = { drawer }
        };

        try
        {
            ScopeAwareAdornerLayer.GetLayer(drawer).ShouldBeSameAs(layer);
        }
        finally
        {
            layer.Children.Remove(drawer);
        }
    }

    [Fact]
    public void Nested_Drawer_Reuses_The_Parent_Drawer_Layer_And_Releases_In_Order()
    {
        var childDrawer = new AtomUI.Desktop.Controls.Drawer
        {
            Content         = new TextBlock { Text = "Child body" },
            IsMotionEnabled = false,
            Width           = 1,
            Height          = 1
        };
        var parentDrawer = new AtomUI.Desktop.Controls.Drawer
        {
            Content = new StackPanel
            {
                Children =
                {
                    new TextBlock { Text = "Parent body" },
                    childDrawer
                }
            },
            IsMotionEnabled = false,
            Width           = 1,
            Height          = 1
        };
        childDrawer.SetValue(
            AtomUI.Desktop.Controls.Drawer.IsMotionEnabledProperty,
            false,
            BindingPriority.Animation);
        parentDrawer.SetValue(
            AtomUI.Desktop.Controls.Drawer.IsMotionEnabledProperty,
            false,
            BindingPriority.Animation);

        var window = CreateAtomWindow(parentDrawer);
        try
        {
            parentDrawer.IsOpen = true;
            Dispatcher.UIThread.RunJobs();

            var layer = ScopeAwareAdornerLayer.GetLayer(parentDrawer).ShouldNotBeNull();
            AtomUI.Desktop.Controls.Drawer.GetDrawer(childDrawer).ShouldBeSameAs(parentDrawer);
            childDrawer.OpenOn.ShouldBeSameAs(window);
            ScopeAwareAdornerLayer.GetLayer(childDrawer).ShouldBeSameAs(layer);

            childDrawer.IsOpen = true;
            Dispatcher.UIThread.RunJobs();
            layer.Children.OfType<DrawerContainer>().Count().ShouldBe(2);

            childDrawer.IsOpen = false;
            Dispatcher.UIThread.RunJobs();
            layer.Children.OfType<DrawerContainer>().Count().ShouldBe(1);

            parentDrawer.IsOpen = false;
            Dispatcher.UIThread.RunJobs();
            layer.Children.OfType<DrawerContainer>().ShouldBeEmpty();
        }
        finally
        {
            if (childDrawer.IsOpen)
            {
                childDrawer.IsOpen = false;
                Dispatcher.UIThread.RunJobs();
            }
            if (parentDrawer.IsOpen)
            {
                parentDrawer.IsOpen = false;
                Dispatcher.UIThread.RunJobs();
            }
            window.Close();
        }
    }

    [Fact]
    public void Open_Drawer_Releases_Window_Chrome_Suppression_When_OpenOn_Changes_To_Local_Control()
    {
        var localTarget = new Border { Width = 240, Height = 180 };
        var drawer = new AtomUI.Desktop.Controls.Drawer
        {
            IsMotionEnabled = false,
            OpenOn = null,
            Width = 1,
            Height = 1
        };
        var root = new Avalonia.Controls.Grid
        {
            Children = { localTarget, drawer }
        };
        var window = CreateAtomWindow(root);
        try
        {
            drawer.OpenOn = window;
            drawer.IsOpen = true;
            Dispatcher.UIThread.RunJobs();

            window.IsDrawnChromeOverlayVisible.ShouldBeFalse();
            var container = window.GetVisualDescendants().OfType<DrawerContainer>().Single();

            drawer.OpenOn = localTarget;
            Dispatcher.UIThread.RunJobs();

            window.IsDrawnChromeOverlayVisible.ShouldBeTrue();
            ScopeAwareAdornerLayer.GetAdornedElement(container).ShouldBeSameAs(localTarget);
        }
        finally
        {
            if (drawer.IsOpen)
            {
                drawer.IsOpen = false;
                Dispatcher.UIThread.RunJobs();
            }
            window.Close();
        }
    }

    [Fact]
    public void Open_Drawer_Migrates_Layer_And_Chrome_Suppression_When_OpenOn_Changes_Window()
    {
        var drawer = new AtomUI.Desktop.Controls.Drawer
        {
            IsMotionEnabled = false,
            Width = 1,
            Height = 1
        };
        var secondTarget = new Border { Width = 240, Height = 180 };
        var firstWindow = CreateAtomWindow(drawer);
        var secondWindow = CreateAtomWindow(secondTarget);
        try
        {
            var firstShadow = new Thickness(4, 5, 6, 7);
            var secondShadow = new Thickness(8, 9, 10, 11);
            firstWindow.FrameShadowThickness = firstShadow;
            secondWindow.FrameShadowThickness = secondShadow;
            drawer.OpenOn = firstWindow;
            drawer.IsOpen = true;
            Dispatcher.UIThread.RunJobs();

            firstWindow.IsDrawnChromeOverlayVisible.ShouldBeFalse();
            secondWindow.IsDrawnChromeOverlayVisible.ShouldBeTrue();
            var firstLayer = ScopeAwareAdornerLayer.GetLayer(firstWindow).ShouldNotBeNull();
            var secondLayer = ScopeAwareAdornerLayer.GetLayer(secondTarget).ShouldNotBeNull();
            var container = firstLayer.Children.OfType<DrawerContainer>().Single();
            container.Margin.ShouldBe(firstShadow);

            drawer.OpenOn = secondWindow;
            Dispatcher.UIThread.RunJobs();

            firstWindow.IsDrawnChromeOverlayVisible.ShouldBeTrue();
            secondWindow.IsDrawnChromeOverlayVisible.ShouldBeFalse();
            firstLayer.Children.ShouldNotContain(container);
            secondLayer.Children.ShouldContain(container);
            ScopeAwareAdornerLayer.GetAdornedElement(container).ShouldBeSameAs(secondWindow);
            container.Margin.ShouldBe(secondShadow);

            firstWindow.FrameShadowThickness = new Thickness(12);
            Dispatcher.UIThread.RunJobs();
            container.Margin.ShouldBe(secondShadow);

            var migratedShadow = new Thickness(13, 14, 15, 16);
            secondWindow.FrameShadowThickness = migratedShadow;
            Dispatcher.UIThread.RunJobs();
            container.Margin.ShouldBe(migratedShadow);

            drawer.IsOpen = false;
            Dispatcher.UIThread.RunJobs();
            secondWindow.IsDrawnChromeOverlayVisible.ShouldBeTrue();
            secondLayer.Children.ShouldNotContain(container);
        }
        finally
        {
            if (drawer.IsOpen)
            {
                drawer.IsOpen = false;
                Dispatcher.UIThread.RunJobs();
            }
            firstWindow.Close();
            secondWindow.Close();
        }
    }

    [Fact]
    public void Nested_Open_Drawers_Release_All_Window_Chrome_Leases_When_Parent_OpenOn_Becomes_Local()
    {
        var localTarget = new Border { Width = 240, Height = 180 };
        var childDrawer = new AtomUI.Desktop.Controls.Drawer
        {
            IsMotionEnabled = false,
            Width = 1,
            Height = 1
        };
        var parentDrawer = new AtomUI.Desktop.Controls.Drawer
        {
            Content = new StackPanel
            {
                Children = { localTarget, childDrawer }
            },
            IsMotionEnabled = false,
            Width = 1,
            Height = 1
        };
        var window = CreateAtomWindow(parentDrawer);
        try
        {
            parentDrawer.IsOpen = true;
            Dispatcher.UIThread.RunJobs();
            childDrawer.IsOpen = true;
            Dispatcher.UIThread.RunJobs();

            window.IsDrawnChromeOverlayVisible.ShouldBeFalse();
            parentDrawer.OpenOn = localTarget;
            Dispatcher.UIThread.RunJobs();

            childDrawer.OpenOn.ShouldBeSameAs(localTarget);
            window.IsDrawnChromeOverlayVisible.ShouldBeTrue();
            var layer = ScopeAwareAdornerLayer.GetLayer(localTarget).ShouldNotBeNull();
            layer.Children.OfType<DrawerContainer>().Count().ShouldBe(2);
            layer.Children.OfType<DrawerContainer>()
                 .ShouldAllBe(container => ReferenceEquals(
                      ScopeAwareAdornerLayer.GetAdornedElement(container),
                      localTarget));
        }
        finally
        {
            if (childDrawer.IsOpen)
            {
                childDrawer.IsOpen = false;
                Dispatcher.UIThread.RunJobs();
            }
            if (parentDrawer.IsOpen)
            {
                parentDrawer.IsOpen = false;
                Dispatcher.UIThread.RunJobs();
            }
            window.Close();
        }
    }

    private static void RaisePointerReleased(Control source, MouseButton button, PointerUpdateKind updateKind)
    {
        source.RaiseEvent(new PointerReleasedEventArgs(
            source,
            new Pointer(Pointer.GetNextFreeId(), PointerType.Mouse, true),
            source,
            default,
            0,
            new PointerPointProperties(RawInputModifiers.None, updateKind),
            KeyModifiers.None,
            button));
    }

    private static void ClickControl(Avalonia.Controls.Window window, Control control)
    {
        control.IsAttachedToVisualTree().ShouldBeTrue();
        control.Bounds.Width.ShouldBeGreaterThan(0);
        control.Bounds.Height.ShouldBeGreaterThan(0);
        var clickPoint = control.TranslatePoint(
            new Point(control.Bounds.Width / 2, control.Bounds.Height / 2),
            window).ShouldNotBeNull();

        var pointer = new Pointer(Pointer.GetNextFreeId(), PointerType.Mouse, true);
        control.RaiseEvent(new PointerPressedEventArgs(
            control,
            pointer,
            window,
            clickPoint,
            0,
            new PointerPointProperties(RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed),
            KeyModifiers.None));
        var releaseTarget = pointer.Captured as InputElement ?? control;
        releaseTarget.RaiseEvent(new PointerReleasedEventArgs(
            releaseTarget,
            pointer,
            window,
            clickPoint,
            1,
            new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased),
            KeyModifiers.None,
            MouseButton.Left));
    }

    private static AvaloniaWindow CreateWindow(Control content)
    {
        var window = new AvaloniaWindow
        {
            Width   = 480,
            Height  = 360,
            Content = content
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static AtomUIWindow CreateAtomWindow(Control content)
    {
        var window = new AtomUIWindow
        {
            Width   = 480,
            Height  = 360,
            Content = content
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

}
