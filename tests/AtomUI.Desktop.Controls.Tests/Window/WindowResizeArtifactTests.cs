using System.Runtime.Versioning;
using System.Xml.Linq;
using AtomUI.Controls;
using AtomUI.Media;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Window;

public class WindowResizeArtifactTests
{
    [Fact]
    public void Visual_Layers_Use_One_Window_Frame_Clip_Without_Changing_Their_Coordinate_System()
    {
        var clientSize = new Size(900, 650);
        var shadow     = new Thickness(36, 27, 36, 45);
        var document   = XDocument.Load(GetRepoFile(
            "src/AtomUI.Desktop.Controls/Window/Themes/WindowTheme.axaml"));
        XNamespace av   = "https://github.com/avaloniaui";
        XNamespace atom = "https://atomui.net";

        WindowVisualLayerClip.CalculateClipBounds(clientSize, shadow)
              .ShouldBe(new Rect(36, 27, 828, 578));
        WindowVisualLayerClip.CalculateClipBounds(clientSize, default)
                             .ShouldBe(new Rect(clientSize));

        var layerManagers = document.Descendants(av + "VisualLayerManager")
                                    .Where(element =>
                                        (string?)element.Attribute("Name") == "PART_VisualLayerManager")
                                    .ToList();
        layerManagers.Count.ShouldBe(3);
        layerManagers.Count(manager =>
            manager.Parent?.Name == atom + "WindowVisualLayerClip").ShouldBe(2);
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(1.25)]
    [InlineData(1.5)]
    [InlineData(1.6666666666666667)]
    public void Visual_Layer_Clip_Edges_Are_Aligned_To_Physical_Pixels(double renderScaling)
    {
        var surfaceSize = new Size(641, 480);
        var shadow     = new Thickness(36, 27, 36, 45);
        var rawBounds  = WindowVisualLayerClip.CalculateClipBounds(surfaceSize, shadow);
        var clipBounds = WindowVisualLayerClip.CalculatePixelAlignedClipBounds(
            surfaceSize,
            shadow,
            renderScaling);

        IsPhysicalPixelAligned(clipBounds.Left, renderScaling).ShouldBeTrue();
        IsPhysicalPixelAligned(clipBounds.Top, renderScaling).ShouldBeTrue();
        IsPhysicalPixelAligned(clipBounds.Right, renderScaling).ShouldBeTrue();
        IsPhysicalPixelAligned(clipBounds.Bottom, renderScaling).ShouldBeTrue();
        clipBounds.Right.ShouldBeGreaterThanOrEqualTo(rawBounds.Right);
        clipBounds.Bottom.ShouldBeGreaterThanOrEqualTo(rawBounds.Bottom);
        (clipBounds.Right - rawBounds.Right).ShouldBeGreaterThanOrEqualTo(
            1 / renderScaling - 0.000001);
        (clipBounds.Bottom - rawBounds.Bottom).ShouldBeGreaterThanOrEqualTo(
            1 / renderScaling - 0.000001);
    }

    [Fact]
    public void Visual_Layer_Clip_Preserves_Empty_Bounds_When_Frame_Is_Larger_Than_Surface()
    {
        WindowVisualLayerClip.CalculatePixelAlignedClipBounds(
                new Size(20, 18),
                new Thickness(12, 10, 12, 10),
                1.25)
            .ShouldBe(new Rect(12, 10, 0, 0));
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(1.25)]
    [InlineData(1.6666666666666667)]
    public void Visual_Layer_Clip_Does_Not_Bleed_Past_Surface_Without_Shadow(double renderScaling)
    {
        var surfaceSize = new Size(641.4, 480);

        WindowVisualLayerClip.CalculatePixelAlignedClipBounds(
                surfaceSize,
                default,
                renderScaling)
            .ShouldBe(new Rect(
                0,
                0,
                Avalonia.Layout.LayoutHelper.RoundLayoutValueUp(surfaceSize.Width, renderScaling),
                Avalonia.Layout.LayoutHelper.RoundLayoutValueUp(surfaceSize.Height, renderScaling)));
    }

    [Theory]
    [InlineData(901.81, 480.2, 1.6666666666666667, 902.4, 480.6)]
    [InlineData(901.4, 479.6, 1.5, 902, 480)]
    [InlineData(640, 480, 1, 640, 480)]
    public void Wayland_Overlay_Mask_Covers_The_Complete_Physical_Buffer(
        double logicalWidth,
        double logicalHeight,
        double renderScaling,
        double expectedWidth,
        double expectedHeight)
    {
        var maskSize = WindowVisualLayerClip.CalculateWaylandMaskSize(
            new Size(logicalWidth, logicalHeight),
            renderScaling);
        maskSize.Width.ShouldBe(expectedWidth, 0.000001);
        maskSize.Height.ShouldBe(expectedHeight, 0.000001);
    }

    [Fact]
    public void Linux_Window_Preserves_Shadow_And_Scales_Only_The_Managed_Resize_Grip()
    {
        var tokenSource  = File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Window/WindowToken.cs"));
        var chromeSource = File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Window/Chrome/AbstractLinuxWindowChromeManager.cs"));
        var reflectionSource = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/Window/Utils/WindowDrawnDecorationsReflectionExtensions.cs"));
        var waylandSource = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/Window/Chrome/WaylandWindowChromeManager.cs"));

        tokenSource.ShouldContain("FrameShadows             = EffectiveGlobalToken.BoxShadowsSecondary;");
        tokenSource.ShouldNotContain("ScaleFrameShadows");
        waylandSource.ShouldContain("private const double ManagedResizeGripScale = 1.0 / 3.0;");
        waylandSource.ShouldContain("Window.TryTakeOverManagedResizeGrip(");
        waylandSource.ShouldContain("ManagedResizeGripScale,");
        reflectionSource.ShouldContain("decorations.FrameThickness");
        reflectionSource.ShouldContain("decorations.ShadowThickness");
        reflectionSource.ShouldContain("(frame.Left + shadow.Left) * scale");
        reflectionSource.ShouldContain("(frame.Top + shadow.Top) * scale");
        reflectionSource.ShouldContain("(frame.Right + shadow.Right) * scale");
        reflectionSource.ShouldContain("(frame.Bottom + shadow.Bottom) * scale");
        reflectionSource.ShouldContain("gripThicknessProperty.SetValue(resizeGrips, default(Thickness));");
    }

    [Fact]
    [SupportedOSPlatform("linux")]
    public void Linux_Resize_Minimum_Does_Not_Grow_From_The_Current_TitleBar_Width()
    {
        var minimumSize = AbstractLinuxWindowChromeManager.CalculateResizeMinimumSize(
            new Thickness(36, 27, 36, 45),
            new CornerRadius(12),
            titleBarHeight: 40);

        minimumSize.Width.ShouldBe(98);
        minimumSize.Height.ShouldBe(194);
    }

    [Fact]
    public void Wayland_Resize_Uses_The_Platform_Reported_Size_Unmodified()
    {
        var waylandSource = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/Window/Chrome/WaylandWindowChromeManager.cs"));
        var windowSource = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/Window/Window.cs"));
        var resizerSource = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/Window/Utils/WindowResizer.cs"));

        waylandSource.ShouldNotContain("platformImpl.Resized =");
        waylandSource.ShouldNotContain("CorrectPlatformResize");
        windowSource.ShouldNotContain("NotifyResizeStarted");
        windowSource.ShouldNotContain("NotifyResizeFinished");
        resizerSource.ShouldNotContain("NotifyResizeStarted");
        resizerSource.ShouldContain("TargetWindow.BeginResizeDrag(windowEdge, e);");
    }

    [Theory]
    [SupportedOSPlatform("linux")]
    [InlineData(36, 26.666666666666668, 36, 45.333333333333336, 36, 27, 36, 45)]
    [InlineData(35.5, 26.4, 36.49, 45.5, 36, 26, 36, 46)]
    public void Wayland_Shadow_Extents_Are_Integral_At_The_Protocol_Boundary(
        double left,
        double top,
        double right,
        double bottom,
        double expectedLeft,
        double expectedTop,
        double expectedRight,
        double expectedBottom)
    {
        var normalized = WaylandWindowChromeManager.NormalizeShadowExtents(
            new Thickness(left, top, right, bottom));
        normalized.ShouldBe(new Thickness(expectedLeft, expectedTop, expectedRight, expectedBottom));

        const int surfaceWidth  = 1000;
        const int surfaceHeight = 900;
        var geometryWidth = (int)Math.Ceiling(surfaceWidth - normalized.Right) - (int)normalized.Left;
        var geometryHeight = (int)Math.Ceiling(surfaceHeight - normalized.Bottom) - (int)normalized.Top;

        (geometryWidth + normalized.Left + normalized.Right).ShouldBe(surfaceWidth);
        (geometryHeight + normalized.Top + normalized.Bottom).ShouldBe(surfaceHeight);
    }

    [Fact]
    [SupportedOSPlatform("linux")]
    public void Wayland_Min_Max_Hints_Use_Window_Geometry_Excluding_Shadow()
    {
        var geometryConstraint = WaylandWindowChromeManager.CalculateGeometryConstraint(
            new Size(594, 474),
            new Thickness(36, 27, 36, 45));

        geometryConstraint.ShouldBe(new Size(522, 402));

        WaylandWindowChromeManager.CalculateGeometryConstraint(
                new Size(double.PositiveInfinity, double.PositiveInfinity),
                new Thickness(36, 27, 36, 45))
            .ShouldBe(new Size(double.PositiveInfinity, double.PositiveInfinity));

        WaylandWindowChromeManager.CalculateGeometryConstraint(
                default,
                new Thickness(36, 27, 36, 45))
            .ShouldBe(default);
    }

    [Fact]
    [SupportedOSPlatform("linux")]
    public void Wayland_Shadow_Input_Region_Preserves_Only_The_Managed_Resize_Band()
    {
        var region = WaylandWindowChromeManager.CalculateInputRegion(
            new Avalonia.Size(900, 600),
            new Avalonia.Thickness(24),
            new Avalonia.Thickness(8),
            canResize: true,
            isCsdEnabled: true,
            Avalonia.Controls.WindowState.Normal);

        region.ShouldBe(new Avalonia.PixelRect(16, 16, 868, 568));

        WaylandWindowChromeManager.CalculateInputRegion(
                new Avalonia.Size(900, 600),
                new Avalonia.Thickness(24),
                new Avalonia.Thickness(8),
                canResize: false,
                isCsdEnabled: true,
                Avalonia.Controls.WindowState.Normal)
            .ShouldBe(new Avalonia.PixelRect(24, 24, 852, 552));

        WaylandWindowChromeManager.CalculateInputRegion(
                new Avalonia.Size(900, 600),
                new Avalonia.Thickness(24),
                new Avalonia.Thickness(8),
                canResize: true,
                isCsdEnabled: true,
                Avalonia.Controls.WindowState.Maximized)
            .ShouldBe(new Avalonia.PixelRect(0, 0, 900, 600));

        WaylandWindowChromeManager.CalculateInputRegion(
                new Avalonia.Size(900, 600),
                new Avalonia.Thickness(24),
                new Avalonia.Thickness(8),
                canResize: true,
                isCsdEnabled: false,
                Avalonia.Controls.WindowState.Normal)
            .ShouldBe(new Avalonia.PixelRect(0, 0, 900, 600));
    }

    [Fact]
    public void Wayland_Shadow_Input_Region_Uses_WlSurface_Protocol_And_Tracks_Size()
    {
        var chromeSource = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/Window/Chrome/WaylandWindowChromeManager.cs"));
        var reflectionSource = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Native/Linux/WaylandWindowReflectionExtensions.cs"));
        var nativeSource = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Native/Linux/WaylandWindowUtils.cs"));

        chromeSource.ShouldContain("e.Property == Visual.BoundsProperty");
        chromeSource.ShouldContain("RequestFrameGeometryUpdate();");
        chromeSource.ShouldContain("Window.TrySetWaylandInputRectangle(");
        chromeSource.ShouldContain("shadowThickness.Left - effectiveGrip.Left");
        chromeSource.ShouldContain("Math.Ceiling(insetLeft)");
        chromeSource.ShouldContain("Math.Floor(surfaceWidth - insetRight)");
        chromeSource.ShouldNotContain("RenderScaling");

        reflectionSource.ShouldContain("WaylandWindowUtils.SetInputRectangle");
        nativeSource.ShouldContain("wlCompositor.CreateRegion()");
        nativeSource.ShouldContain("region.Add(x, y, width, height);");
        nativeSource.ShouldContain("wlSurface.SetInputRegion(region);");
        nativeSource.ShouldContain("region.Destroy();");
        nativeSource.IndexOf("SetInputRegion", StringComparison.Ordinal)
                    .ShouldBeLessThan(nativeSource.IndexOf("region.Destroy", StringComparison.Ordinal));
    }

    [Fact]
    public void Wayland_Input_Region_Destroys_The_Protocol_Object_Before_Disposing_The_Local_Proxy()
    {
        var nativeSource = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Native/Linux/WaylandWindowUtils.cs"));

        nativeSource.ShouldContain("region.Destroy();");
        nativeSource.ShouldContain("region.Dispose();");
        nativeSource.IndexOf("region.Destroy();", StringComparison.Ordinal)
                    .ShouldBeLessThan(nativeSource.IndexOf("region.Dispose();", StringComparison.Ordinal));
    }

    [Fact]
    public void Browser_Wasm_Excludes_Wayland_Input_Region_Protocol_Assemblies()
    {
        var browserProject = File.ReadAllText(GetRepoFile(
            "controlgallery/AtomUIGallery.Browser/AtomUIGallery.Browser.csproj"));

        browserProject.ShouldContain("ExcludeDesktopPlatformAssembliesFromBrowserWasm");
        browserProject.ShouldContain("'%(FileName)' == 'Avalonia.Wayland'");
        browserProject.ShouldContain("'%(FileName)' == 'NWayland'");
    }

    [Fact]
    public void Window_Resizer_Hover_Cursors_Preserve_Directional_Semantics()
    {
        var source = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/Window/Themes/WindowResizerTheme.axaml"));
        var builderSource = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/DesktopAppBuilderExtensions.cs"));

        source.ShouldContain("Cursor=\"TopSide\"");
        source.ShouldContain("Cursor=\"BottomSide\"");
        source.ShouldContain("Cursor=\"LeftSide\"");
        source.ShouldContain("Cursor=\"RightSide\"");
        source.ShouldContain("Cursor=\"TopLeftCorner\"");
        source.ShouldContain("Cursor=\"TopRightCorner\"");
        source.ShouldContain("Cursor=\"BottomLeftCorner\"");
        source.ShouldContain("Cursor=\"BottomRightCorner\"");
        builderSource.ShouldNotContain("PreferSymmetricResizeCursorNames");
    }

    [Fact]
    public void Window_Resizer_Compiled_Theme_Uses_The_Expected_Hover_Cursors()
    {
        AvaloniaTestApp.EnsureInitialized();
        var resizer = new WindowResizer();
        Application.Current!.TryFindResource(typeof(WindowResizer), out var resource).ShouldBeTrue();
        resizer.Theme = resource.ShouldBeAssignableTo<ControlTheme>();
        var host = new Avalonia.Controls.Window
        {
            Width = 100,
            Height = 100,
            Content = resizer
        };
        host.Show();
        resizer.ApplyTemplate();

        var grips = resizer.GetVisualDescendants()
                           .OfType<Border>()
                           .Where(border => border.Tag is ResizeHandleLocation)
                           .ToDictionary(
                               border => (ResizeHandleLocation)border.Tag!,
                               border => border.Cursor?.ToString());

        grips[ResizeHandleLocation.North].ShouldBe("TopSide");
        grips[ResizeHandleLocation.South].ShouldBe("BottomSide");
        grips[ResizeHandleLocation.West].ShouldBe("LeftSide");
        grips[ResizeHandleLocation.East].ShouldBe("RightSide");
        grips[ResizeHandleLocation.NorthWest].ShouldBe("TopLeftCorner");
        grips[ResizeHandleLocation.NorthEast].ShouldBe("TopRightCorner");
        grips[ResizeHandleLocation.SouthWest].ShouldBe("BottomLeftCorner");
        grips[ResizeHandleLocation.SouthEast].ShouldBe("BottomRightCorner");
        host.Close();
    }

    [Fact]
    public void Window_Prepares_Linux_Initial_Client_Size_Before_Show()
    {
        var windowSource = File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Window/Window.cs"));
        var chromeSource = File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Window/Chrome/AbstractLinuxWindowChromeManager.cs"));
        var x11Source = File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Window/Chrome/X11WindowChromeManager.cs"));

        windowSource.ShouldContain("public override void Show()");
        windowSource.ShouldContain("_platformChromeManager?.PrepareInitialShowState();");
        windowSource.ShouldContain("base.Show();");
        windowSource.IndexOf("_platformChromeManager?.PrepareInitialShowState();", StringComparison.Ordinal)
                    .ShouldBeLessThan(windowSource.IndexOf("base.Show();", StringComparison.Ordinal));

        chromeSource.ShouldContain("PrepareInitialShowState()");
        x11Source.ShouldContain("SizeToContent != SizeToContent.Manual");
        x11Source.ShouldContain("WindowState is WindowState.Minimized or WindowState.Maximized or WindowState.FullScreen");
        x11Source.ShouldContain("SetPlatformChromeClientSize(clientSize);");
        x11Source.ShouldContain("Window.Width = clientSize.Width;");
        x11Source.ShouldContain("Window.Height = clientSize.Height;");
        x11Source.ShouldContain("TryGetInitialStartupPosition");
        x11Source.ShouldContain("WindowStartupLocation.Manual");
        x11Source.ShouldContain("ConfigureLinuxInitialWindowGeometry");
        chromeSource.ShouldContain("_initialShowStatePrepared");
    }

    [Fact]
    public void Linux_Initial_Geometry_Updates_X11_Size_Hints_Before_Map()
    {
        var extensionSource = File.ReadAllText(GetRepoFile("src/AtomUI.Native/WindowExtensions.cs"));
        var linuxSource     = File.ReadAllText(GetRepoFile("src/AtomUI.Native/Linux/WindowUtils.Linux.cs"));
        var interopSource   = File.ReadAllText(GetRepoFile("src/AtomUI.Native/Linux/WindowUtils.Interop.cs"));

        extensionSource.ShouldContain("ConfigureLinuxInitialWindowGeometry");
        extensionSource.ShouldContain("WindowUtilsLinux.ConfigureInitialWindowGeometry");

        linuxSource.ShouldContain("ConfigureInitialWindowGeometry");
        linuxSource.ShouldContain("XSetWMNormalHints");
        linuxSource.ShouldContain("XMoveResizeWindow");
        linuxSource.IndexOf("XSetWMNormalHints", StringComparison.Ordinal)
                   .ShouldBeLessThan(linuxSource.IndexOf("XMoveResizeWindow", StringComparison.Ordinal));

        interopSource.ShouldContain("USPosition");
        interopSource.ShouldContain("USSize");
        interopSource.ShouldContain("PSize");
    }

    [Fact]
    public void Linux_Client_Drawn_Shadow_Publishes_X11_Csd_Frame_Extents()
    {
        var windowSource    = File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Window/Window.cs"));
        var chromeSource    = File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Window/Chrome/X11WindowChromeManager.cs"));
        var linuxChromeSource = File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Window/Chrome/AbstractLinuxWindowChromeManager.cs"));
        var extensionSource = File.ReadAllText(GetRepoFile("src/AtomUI.Native/WindowExtensions.cs"));
        var linuxSource     = File.ReadAllText(GetRepoFile("src/AtomUI.Native/Linux/WindowUtils.Linux.cs"));
        var interopSource   = File.ReadAllText(GetRepoFile("src/AtomUI.Native/Linux/WindowUtils.Interop.cs"));

        windowSource.ShouldContain("_platformChromeManager = WindowChromeManager.Attach(this);");
        linuxChromeSource.ShouldContain("_window.ScalingChanged += HandleScalingChanged;");
        chromeSource.ShouldContain("ApplyCsdFrameExtents();");
        chromeSource.ShouldContain("Window.WindowState is WindowState.Normal ? Window.FrameShadowThickness : default");
        chromeSource.ShouldContain("Window.SetLinuxX11CsdFrameExtents(frameExtents);");
        windowSource.ShouldNotContain("EnsureMinSizeForDecorations");
        windowSource.IndexOf("_platformChromeManager?.PrepareInitialShowState();", StringComparison.Ordinal)
                    .ShouldBeLessThan(windowSource.IndexOf("base.Show();", StringComparison.Ordinal));

        extensionSource.ShouldContain("SetLinuxX11CsdFrameExtents");
        extensionSource.ShouldContain("handle.HandleDescriptor != \"XID\"");
        extensionSource.ShouldContain("ToPixelMargin(frameExtents.Left, scaling)");
        extensionSource.ShouldContain("ToPixelMargin(frameExtents.Top, scaling)");
        extensionSource.ShouldContain("ToPixelMargin(frameExtents.Right, scaling)");
        extensionSource.ShouldContain("ToPixelMargin(frameExtents.Bottom, scaling)");

        linuxSource.ShouldContain("X11CsdFrameExtentsPropertyName = \"_GTK_FRAME_EXTENTS\"");
        linuxSource.ShouldContain("SetX11CsdFrameExtents");
        linuxSource.ShouldContain("Mutter and KWin");
        linuxSource.ShouldContain("left, right, top, bottom");
        linuxSource.ShouldContain("XInternAtom");
        linuxSource.ShouldContain("XChangeProperty");
        linuxSource.IndexOf("ToCardinal(left)", StringComparison.Ordinal)
                   .ShouldBeLessThan(linuxSource.IndexOf("ToCardinal(right)", StringComparison.Ordinal));
        linuxSource.IndexOf("ToCardinal(right)", StringComparison.Ordinal)
                   .ShouldBeLessThan(linuxSource.IndexOf("ToCardinal(top)", StringComparison.Ordinal));
        linuxSource.IndexOf("ToCardinal(top)", StringComparison.Ordinal)
                   .ShouldBeLessThan(linuxSource.IndexOf("ToCardinal(bottom)", StringComparison.Ordinal));

        interopSource.ShouldContain("PropModeReplace");
        interopSource.ShouldContain("XInternAtom");
        interopSource.ShouldContain("XChangeProperty");
        interopSource.ShouldContain("IntPtr[] data");

        windowSource.ShouldNotContain("GtkFrameExtents");
        chromeSource.ShouldNotContain("GtkFrameExtents");
        extensionSource.ShouldNotContain("GtkFrameExtents");
        linuxSource.ShouldNotContain("GtkFrameExtents");
    }

    [Fact]
    public void Linux_Window_Frame_Geometry_Updates_Are_Coalesced_Before_Render()
    {
        var windowSource = File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Window/Window.cs"));
        var chromeSource = File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Window/Chrome/AbstractLinuxWindowChromeManager.cs"));

        chromeSource.ShouldContain("HandleFrameShadowChanged");
        chromeSource.ShouldContain("ConfigureTitleBarHeightHint");
        chromeSource.ShouldContain("_hasPendingFrameShadowThickness");
        chromeSource.ShouldContain("_hasPendingTitleBarHeightHint");
        chromeSource.ShouldContain("RequestFrameGeometryUpdate");
        chromeSource.ShouldContain("ApplyPendingFrameGeometryUpdate");
        chromeSource.ShouldContain("ApplyPendingFrameGeometryInputs");
        chromeSource.ShouldContain("UpdateFrameGeometry");
        chromeSource.ShouldContain("Avalonia.Threading.DispatcherPriority.Render");

        windowSource.ShouldContain("FrameShadowProperty.Changed.AddClassHandler<Window>((window, args) =>");
        windowSource.ShouldContain("window.HandleFrameShadowPropertyChanged(args.GetNewValue<BoxShadows>())");
        windowSource.ShouldContain("_platformChromeManager.HandleFrameShadowChanged(frameShadow);");
        windowSource.ShouldContain("_platformChromeManager.ConfigureTitleBarHeightHint(e.NewSize.Height);");
        windowSource.ShouldContain("_platformChromeManager?.HandlePropertyChanged(change.Property);");
        chromeSource.ShouldContain("RequestFrameGeometryUpdate();");

        chromeSource.IndexOf("protected void RequestFrameGeometryUpdate()", StringComparison.Ordinal)
                    .ShouldBeLessThan(chromeSource.IndexOf("private void ApplyPendingFrameGeometryUpdate()", StringComparison.Ordinal));
        chromeSource.IndexOf("private void ApplyPendingFrameGeometryUpdate()", StringComparison.Ordinal)
                    .ShouldBeLessThan(chromeSource.IndexOf("private void ApplyPendingFrameGeometryInputs()", StringComparison.Ordinal));
        chromeSource.ShouldContain("ApplyPendingFrameGeometryInputs();\n        UpdateFrameGeometry();");
        chromeSource.IndexOf("Dispatcher.Post(ApplyPendingFrameGeometryUpdate", StringComparison.Ordinal)
                    .ShouldBeLessThan(chromeSource.IndexOf("private void ApplyPendingFrameGeometryUpdate()", StringComparison.Ordinal));

        windowSource.ShouldNotContain("RequestLinuxFrameGeometryUpdate");
        windowSource.ShouldNotContain("ApplyPendingLinuxFrameGeometryUpdate");
        windowSource.ShouldNotContain("UpdateLinuxFrameGeometry");
    }

    [Fact]
    public void Linux_Shadow_Input_Region_Keeps_Only_Ten_Dip_Resize_Band()
    {
        var chromeSource = File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Window/Chrome/X11WindowChromeManager.cs"));
        var nativeInputSource  = File.ReadAllText(GetRepoFile("src/AtomUI.Native/WindowExtensions.cs"));
        var nativeLinuxSource  = File.ReadAllText(GetRepoFile("src/AtomUI.Native/Linux/WindowUtils.Linux.cs"));

        File.Exists(GetRepoFileOrNull("src/AtomUI.Native/Linux/ClickThroughShadowExtensions.cs") ?? string.Empty)
            .ShouldBeFalse();
        chromeSource.ShouldContain("AttachClickThroughShadow();");
        chromeSource.ShouldContain("s_shadowInputRegionAffectsProperties");
        chromeSource.ShouldContain("ShadowInputRegionResizeBand");
        chromeSource.ShouldContain("CanResizeProperty");
        chromeSource.ShouldContain("FrameShadowThicknessProperty");
        chromeSource.ShouldContain("IsCsdEnabledProperty");
        chromeSource.ShouldContain("Window.Opened +=");
        chromeSource.ShouldContain("Window.PropertyChanged +=");
        chromeSource.ShouldContain("Visual.BoundsProperty");
        chromeSource.ShouldContain("AvaloniaWindow.WindowDecorationMarginProperty");
        chromeSource.ShouldContain("TopLevel.TransparencyLevelHintProperty");

        chromeSource.ShouldContain("private const double ShadowInputRegionResizeBand = 10.0;");
        chromeSource.ShouldContain("var effectiveResizeBand = Window.CanResize ? ShadowInputRegionResizeBand : 0;");
        chromeSource.ShouldContain("shadowThickness.Left - effectiveResizeBand");
        chromeSource.ShouldContain("shadowThickness.Top - effectiveResizeBand");
        chromeSource.ShouldContain("Window.SetWindowInputRectangle(x, y, w, h);");
        chromeSource.ShouldContain("Window.ResetWindowInputRegion(fullW, fullH);");
        nativeInputSource.ShouldContain("SetWindowInputRectangle");
        nativeInputSource.ShouldContain("ResetWindowInputRegion");
        nativeLinuxSource.ShouldContain("SetInputRectangle");
    }

    [Fact]
    public void Linux_Input_Region_Does_Not_Use_Whole_Window_Mouse_Ignore_Or_Shape_Queries()
    {
        var extensionSource = File.ReadAllText(GetRepoFile("src/AtomUI.Native/WindowExtensions.cs"));
        var linuxSource = File.ReadAllText(GetRepoFile("src/AtomUI.Native/Linux/WindowUtils.Linux.cs"));
        var interopSource = File.ReadAllText(GetRepoFile("src/AtomUI.Native/Linux/WindowUtils.Interop.cs"));

        foreach (var source in new[] { extensionSource, linuxSource, interopSource })
        {
            source.ShouldNotContain("SetWindowIgnoreMouseEventsLinux");
            source.ShouldNotContain("IsWindowIgnoreMouseEventsLinux");
            source.ShouldNotContain("xcb_get_geometry");
            source.ShouldNotContain("xcb_shape_get_rectangles");
        }
    }

    [Fact]
    public void Linux_NonCsd_Window_Template_Clips_Content_To_Window_CornerRadius()
    {
        var document = XDocument.Load(GetRepoFile("src/AtomUI.Desktop.Controls/Window/Themes/WindowTheme.axaml"));
        XNamespace av = "https://github.com/avaloniaui";
        XNamespace atom = "https://atomui.net";

        var linuxTemplateStyle = document.Descendants(av + "Style")
                                         .Single(element =>
                                             (string?)element.Attribute("Selector") ==
                                             "^[OsType=Linux][IsCsdEnabled=False]" &&
                                             element.Descendants(av + "ControlTemplate").Any());

        var contentClip = linuxTemplateStyle.Descendants(av + "Border")
                                            .Single(element =>
                                                (string?)element.Attribute("Name") == "WindowContentClip");

        contentClip.Attribute("CornerRadius").ShouldNotBeNull().Value.ShouldBe("{TemplateBinding CornerRadius}");
        contentClip.Attribute("ClipToBounds").ShouldNotBeNull().Value.ShouldBe("True");
        contentClip.Attribute("Margin").ShouldNotBeNull().Value.ShouldBe("{TemplateBinding FrameShadowThickness}");
        contentClip.Ancestors(atom + "WindowVisualLayerClip").Count().ShouldBe(1);

        var dockPanel = contentClip.Elements(av + "DockPanel").Single();
        dockPanel.Attribute("LastChildFill").ShouldNotBeNull().Value.ShouldBe("True");
        dockPanel.Attribute("Margin").ShouldBeNull();
    }

    [Fact]
    public void Linux_Csd_Window_Template_Clips_Content_Surface_To_Bottom_Window_CornerRadius()
    {
        var document = XDocument.Load(GetRepoFile("src/AtomUI.Desktop.Controls/Window/Themes/WindowTheme.axaml"));
        XNamespace av = "https://github.com/avaloniaui";
        XNamespace atom = "https://atomui.net";

        document.Descendants()
                .ShouldContain(element =>
                    element.Name.LocalName == "CornerRadiusFilterConverter" &&
                    element.Attributes().Any(attribute =>
                        attribute.Name.LocalName == "Key" &&
                        (string?)attribute == "WindowContentBottomCornerRadiusFilter") &&
                    (string?)element.Attribute("Filter") == "BottomLeft, BottomRight");

        var csdTemplateStyle = document.Descendants(av + "Style")
                                       .Single(element =>
                                           (string?)element.Attribute("Selector") == "^[IsCsdEnabled=True]" &&
                                           element.Descendants(av + "ControlTemplate").Any());

        csdTemplateStyle.Descendants(av + "Border")
                        .ShouldNotContain(element =>
                            (string?)element.Attribute("Name") == "WindowContentClip");

        var layerClip = csdTemplateStyle.Descendants(atom + "WindowVisualLayerClip").Single();
        layerClip.Attribute("ShadowThickness").ShouldNotBeNull().Value.ShouldBe(
            "{TemplateBinding FrameShadowThickness}");
        layerClip.Attribute("CornerRadius").ShouldNotBeNull().Value.ShouldBe(
            "{TemplateBinding CornerRadius}");

        var visualLayerManager = layerClip.Elements(av + "VisualLayerManager")
                                          .Single(element =>
                                              (string?)element.Attribute("Name") == "PART_VisualLayerManager");
        var contentPanel = visualLayerManager.Elements(av + "Panel").Single();

        contentPanel.Attribute("Margin").ShouldNotBeNull().Value.ShouldBe(
            "{Binding $parent[atom:Window].EffectiveContentFrameMargin}");
        contentPanel.Attribute("ClipToBounds").ShouldBeNull();

        var contentFrameLayer = contentPanel.Elements(av + "ContentPresenter")
                                            .Single(element =>
                                                (string?)element.Attribute("Name") == "ContentFrameLayer");

        contentFrameLayer.Attribute("CornerRadius").ShouldNotBeNull().Value.ShouldBe(
            "{TemplateBinding CornerRadius, Converter={StaticResource WindowContentBottomCornerRadiusFilter}}");
        contentFrameLayer.Attribute("ClipToBounds").ShouldNotBeNull().Value.ShouldBe("True");

        var contentFrame = contentPanel.Elements(av + "Border")
                                       .Single(element =>
                                           (string?)element.Attribute("Name") == "ContentFrame");

        contentFrame.Attribute("CornerRadius").ShouldNotBeNull().Value.ShouldBe(
            "{TemplateBinding CornerRadius, Converter={StaticResource WindowContentBottomCornerRadiusFilter}}");
        contentFrame.Attribute("ClipToBounds").ShouldNotBeNull().Value.ShouldBe("True");
    }

    [Theory]
    [InlineData(OsType.Windows)]
    [InlineData(OsType.Linux)]
    [InlineData(OsType.macOS)]
    public void Csd_Window_Hides_The_Managed_TitleBar_Without_Dropping_Full_Window_Decorations(OsType osType)
    {
        AvaloniaTestApp.EnsureInitialized();
        var window = new AtomUI.Desktop.Controls.Window();
        window.SetValue(AtomUI.Desktop.Controls.Window.OsTypeProperty, osType);
        window.IsCsdEnabled = true;
        window.IsTitleBarVisible = false;
        window.PreparePlatformChromeInitialShowLayout();

        window.WindowDecorations.ShouldBe(WindowDecorations.Full);

        window.IsTitleBarVisible = true;
        window.WindowDecorations.ShouldBe(WindowDecorations.Full);

        window.IsTitleBarVisible = false;
        window.WindowDecorations.ShouldBe(WindowDecorations.Full);
    }

    [Fact]
    public void Drawn_Decorations_Combine_Platform_Title_Bar_Capability_With_AtomUI_Visibility()
    {
        var document = XDocument.Load(GetRepoFile(
            "src/AtomUI.Desktop.Controls/Window/Themes/WindowDrawnDecorationsTheme.axaml"));
        XNamespace av = "https://github.com/avaloniaui";

        AssertTitleBarVisibilityContract(document, "PART_TitleBar");
        AssertTitleBarVisibilityContract(document, "PART_TitleBarPresenter");
        AssertTitleBarVisibilityContract(document, "WindowTitleBarShadowBackground");
        document.Descendants(av + "Style")
                .ShouldContain(style =>
                    (string?)style.Attribute("Selector") ==
                    "^:fullscreen /template/ ContentPresenter#PART_TitleBarPresenter");
        document.Descendants(av + "Style")
                .ShouldNotContain(style =>
                    (string?)style.Attribute("Selector") ==
                    "^:fullscreen /template/ Panel#PART_TitleBarPresenter");
    }

    [Fact]
    public void MacOs_NonCsd_Window_Removes_The_Native_TitleBar_When_TitleBar_Visibility_Is_Disabled()
    {
        AvaloniaTestApp.EnsureInitialized();
        var window = new AtomUI.Desktop.Controls.Window();
        window.SetValue(AtomUI.Desktop.Controls.Window.OsTypeProperty, OsType.macOS);
        window.IsCsdEnabled = false;
        window.IsTitleBarVisible = false;
        window.PreparePlatformChromeInitialShowLayout();

        window.WindowDecorations.ShouldBe(WindowDecorations.BorderOnly);

        window.IsTitleBarVisible = true;
        window.WindowDecorations.ShouldBe(WindowDecorations.Full);

        window.IsTitleBarVisible = false;
        window.WindowDecorations.ShouldBe(WindowDecorations.BorderOnly);
    }

    [Fact]
    public void MacOs_Window_Decoration_Changes_Reapply_And_Defer_Traffic_Light_Layout()
    {
        var windowSource = File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Window/Window.cs"));
        var relayoutBlockStart = windowSource.IndexOf(
            "if (change.Property == WindowStateProperty ||",
            StringComparison.Ordinal);
        var relayoutBlockEnd = windowSource.IndexOf(
            "if (change.Property == ExtendClientAreaTitleBarHeightHintProperty ||",
            relayoutBlockStart + 1,
            StringComparison.Ordinal);
        var relayoutBlock = windowSource[relayoutBlockStart..relayoutBlockEnd];

        (relayoutBlock.Split(
                "change.Property == WindowDecorationsProperty",
                StringSplitOptions.None).Length - 1)
            .ShouldBe(2);
        relayoutBlock.ShouldContain("QueueMacOsWindowConfiguration");
        relayoutBlock.ShouldContain("forceFollowUp: true");
        windowSource.ShouldContain("DispatcherPriority.Render");
    }

    [Fact]
    public void MacOs_TitleBar_Height_Changes_Defer_Traffic_Light_Layout_Until_Render()
    {
        var windowSource = File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Window/Window.cs"));
        var relayoutBlockStart = windowSource.IndexOf(
            "if (change.Property == WindowStateProperty ||",
            StringComparison.Ordinal);
        var relayoutBlockEnd = windowSource.IndexOf(
            "if (change.Property == ExtendClientAreaTitleBarHeightHintProperty ||",
            relayoutBlockStart + 1,
            StringComparison.Ordinal);
        var relayoutBlock = windowSource[relayoutBlockStart..relayoutBlockEnd];

        relayoutBlock.ShouldContain("change.Property == ExtendClientAreaTitleBarHeightHintProperty");
        relayoutBlock.ShouldContain("QueueMacOsWindowConfiguration");
        windowSource.ShouldContain("DispatcherPriority.Render");
        windowSource.ShouldContain("ApplyQueuedMacOsWindowConfiguration");
    }

    [Fact]
    public void MacOs_Window_Sharing_Transitions_Recheck_Native_Button_Frame()
    {
        var windowSource = File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Window/Window.cs"));
        var nativeSource = File.ReadAllText(GetRepoFile("src/AtomUI.Native/MacOS/WindowUtils.MacOS.cs"));
        var interopSource = File.ReadAllText(GetRepoFile("src/AtomUI.Native/MacOS/WindowUtils.Interop.cs"));

        windowSource.ShouldNotContain("DispatcherTimer");
        windowSource.ShouldContain("ObserveStandardWindowButtonChanges");
        windowSource.ShouldContain("_macOsWindowButtonObserver");
        windowSource.ShouldContain("StopMacOsWindowButtonObserver");
        nativeSource.ShouldContain("IsStandardWindowButtonsVisible");
        nativeSource.ShouldContain("ObserveStandardWindowButtonChanges");
        nativeSource.ShouldContain("NSNotificationCenter");
        nativeSource.ShouldContain("GetStandardWindowButtonFrame");
        interopSource.ShouldContain("isHidden");
        interopSource.ShouldContain("postsFrameChangedNotifications");
        interopSource.ShouldContain("NSViewFrameDidChangeNotification");
        nativeSource.ShouldContain("observeValueForKeyPath");
        nativeSource.ShouldContain("HiddenKeyPath");
        interopSource.ShouldContain("removeObserver:forKeyPath:");
        windowSource.ShouldContain("_macOsCachedButtonY");
    }

    [Fact]
    public void MacOs_Window_Button_Observer_Releases_All_Native_Resources()
    {
        var nativeSource = File.ReadAllText(GetRepoFile("src/AtomUI.Native/MacOS/WindowUtils.MacOS.cs"));
        var interopSource = File.ReadAllText(GetRepoFile("src/AtomUI.Native/MacOS/WindowUtils.Interop.cs"));

        interopSource.ShouldContain("dlclose");
        nativeSource.ShouldContain("_hiddenKeyPath");
        nativeSource.ShouldNotContain("private static readonly IntPtr HiddenKeyPath");
        nativeSource.ShouldContain("RemoveNativeRegistrations");
        nativeSource.ShouldContain("_keyValueObserversRegistered");
        nativeSource.ShouldContain("_frameNotificationStateCaptured");
        nativeSource.ShouldContain("WindowUtilsInterop.ReleaseSelector");
    }

    [Fact]
    public void MacOs_NonCsd_Window_Preserves_The_Native_Resize_Cursor_Hit_Test_Layer()
    {
        var document = XDocument.Load(GetRepoFile("src/AtomUI.Desktop.Controls/Window/Themes/WindowTheme.axaml"));
        var managerSource = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/Window/Chrome/WindowChromeManager.cs"));
        var macOsManagerSource = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/Window/Chrome/MacOSWindowChromeManager.cs"));
        XNamespace av   = "https://github.com/avaloniaui";
        XNamespace atom = "https://atomui.net";

        var macOsTemplateStyle = document.Descendants(av + "Style")
                                         .Single(element =>
                                             (string?)element.Attribute("Selector") ==
                                             "^[OsType=macOS][IsCsdEnabled=False]" &&
                                             element.Descendants(av + "ControlTemplate").Any());
        macOsTemplateStyle.Descendants(atom + "WindowResizer").ShouldBeEmpty();

        managerSource.ShouldContain("MacOSWindowChromeManager.Attach(window)");
        macOsManagerSource.ShouldContain("public bool UsesCustomResizer => false;");
        macOsManagerSource.ShouldNotContain("Window.TryTakeOverManagedResizeGrip(");
        macOsManagerSource.ShouldNotContain("Window.ConfigureManagedResizeGrip(");
        macOsManagerSource.ShouldContain("_window.SetMacOsResizeIndicatorVisible(false);");
    }

    [Fact]
    public void MacOs_Resize_Cursor_Manager_Does_Not_Replace_Other_Platform_Managers()
    {
        var managerSource = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/Window/Chrome/WindowChromeManager.cs"));

        var linuxBranchStart = managerSource.IndexOf(
            "if (OperatingSystem.IsLinux())",
            StringComparison.Ordinal);
        var windowsBranchStart = managerSource.IndexOf(
            "if (OperatingSystem.IsWindows())",
            linuxBranchStart + 1,
            StringComparison.Ordinal);
        var macOsBranchStart = managerSource.IndexOf(
            "if (OperatingSystem.IsMacOS())",
            windowsBranchStart + 1,
            StringComparison.Ordinal);

        linuxBranchStart.ShouldBeGreaterThanOrEqualTo(0);
        windowsBranchStart.ShouldBeGreaterThan(linuxBranchStart);
        macOsBranchStart.ShouldBeGreaterThan(windowsBranchStart);
        managerSource[linuxBranchStart..windowsBranchStart].ShouldContain(
            "AbstractLinuxWindowChromeManager.Attach(window)");
        managerSource[windowsBranchStart..macOsBranchStart].ShouldContain(
            "WindowsWindowChromeManager.Attach(window)");
        managerSource[macOsBranchStart..].ShouldContain("MacOSWindowChromeManager.Attach(window)");
    }

    [Fact]
    public void MacOs_Window_Chrome_Manager_Preserves_Default_Frame_Geometry_Updates()
    {
        AvaloniaTestApp.EnsureInitialized();
        var window = new AtomUI.Desktop.Controls.Window();
        var manager = MacOSWindowChromeManager.Attach(window);
        var frameShadow = new BoxShadows(new BoxShadow
        {
            OffsetX = 3,
            OffsetY = 4,
            Blur    = 10,
            Spread  = 2
        });

        manager.HandleFrameShadowChanged(frameShadow);
        manager.ConfigureTitleBarHeightHint(42);

        manager.UsesCustomResizer.ShouldBeFalse();
        window.FrameShadowThickness.ShouldBe(frameShadow.Thickness());
        window.ExtendClientAreaTitleBarHeightHint.ShouldBe(42);
    }

    [Fact]
    public void Linux_NonCsd_Window_Hides_Only_The_AtomUI_TitleBar_And_Keeps_Native_Decorations_Disabled()
    {
        var document = XDocument.Load(GetRepoFile("src/AtomUI.Desktop.Controls/Window/Themes/WindowTheme.axaml"));
        XNamespace av = "https://github.com/avaloniaui";

        var linuxTemplateStyle = document.Descendants(av + "Style")
                                         .Single(element =>
                                             (string?)element.Attribute("Selector") ==
                                             "^[OsType=Linux][IsCsdEnabled=False]" &&
                                             element.Descendants(av + "ControlTemplate").Any());
        var titleBarPanel = linuxTemplateStyle.Descendants(av + "Panel")
                                              .Single(element =>
                                                  (string?)element.Attribute("Name") == "TitleBarPanel");
        titleBarPanel.Attribute("IsVisible").ShouldNotBeNull().Value.ShouldBe(
            "{TemplateBinding IsTitleBarVisible}");

        var linuxDecorationStyle = document.Descendants(av + "Style")
                                           .Single(element =>
                                               (string?)element.Attribute("Selector") ==
                                               "^[OsType=Linux][IsCsdEnabled=False]" &&
                                               element.Elements(av + "Setter").Any(setter =>
                                                   (string?)setter.Attribute("Property") ==
                                                   "WindowDecorations"));
        linuxDecorationStyle.Elements(av + "Setter").ShouldContain(setter =>
            (string?)setter.Attribute("Property") == "WindowDecorations" &&
            (string?)setter.Attribute("Value") == "None");
    }

    [Fact]
    public void Wayland_Csd_Drawn_TitleBar_Layers_Clip_To_Top_Window_CornerRadius()
    {
        var document = XDocument.Load(GetRepoFile(
            "src/AtomUI.Desktop.Controls/Window/Themes/WindowDrawnDecorationsTheme.axaml"));
        XNamespace av = "https://github.com/avaloniaui";

        var titleBarFrame = document.Descendants(av + "ContentPresenter")
                                    .Single(element =>
                                        (string?)element.Attribute("Name") == "PART_TitleBar");
        var titleBarContent = document.Descendants(av + "ContentPresenter")
                                      .Single(element =>
                                          (string?)element.Attribute("Name") == "PART_TitleBarPresenter");
        var titleBarClip = titleBarContent.Parent.ShouldNotBeNull();

        titleBarFrame.Attribute("ClipToBounds").ShouldNotBeNull().Value.ShouldBe("True");
        titleBarClip.Name.ShouldBe(av + "Border");
        titleBarClip.Attribute("ClipToBounds").ShouldNotBeNull().Value.ShouldBe("True");
        titleBarClip.Attribute("CornerRadius").ShouldNotBeNull().Value.ShouldBe(
            "{Binding $parent[atom:Window].CornerRadius, Converter={StaticResource TitleBarCornerRadiusFilter}}");
        titleBarContent.Attribute("ClipToBounds").ShouldBeNull();
        titleBarContent.Attribute("CornerRadius").ShouldBeNull();
    }

    [Fact]
    public void Windows_Window_Has_No_NonCsd_Template_And_Uses_Opaque_Csd_Background()
    {
        var document = XDocument.Load(GetRepoFile("src/AtomUI.Desktop.Controls/Window/Themes/WindowTheme.axaml"));
        var decorationsDocument = XDocument.Load(GetRepoFile(
            "src/AtomUI.Desktop.Controls/Window/Themes/WindowDrawnDecorationsTheme.axaml"));
        XNamespace av = "https://github.com/avaloniaui";

        document.Descendants(av + "Style")
                .ShouldNotContain(element =>
                    (string?)element.Attribute("Selector") == "^[OsType=Windows][IsCsdEnabled=False]");

        var windowsStyle = document.Descendants(av + "Style")
                                   .Single(element =>
                                       (string?)element.Attribute("Selector") == "^[OsType=Windows]");

        windowsStyle.Elements(av + "Setter").ShouldContain(setter =>
            (string?)setter.Attribute("Property") == "TransparencyLevelHint" &&
            (string?)setter.Attribute("Value") == "None");

        decorationsDocument.Descendants(av + "Style")
                           .Single(style =>
                               (string?)style.Attribute("Selector") == "^/template/ Border#PART_WindowFrame")
                           .Elements(av + "Setter")
                           .ShouldContain(setter =>
                               (string?)setter.Attribute("Property") == "Background" &&
                               (string?)setter.Attribute("Value") == "{atom:WindowTokenResource DefaultBackground}");
    }

    [Fact]
    public void BuiltIn_TitleBars_Use_One_Full_Frame_LayoutPanel_With_Three_Roles()
    {
        var document = XDocument.Load(GetRepoFile(
            "src/AtomUI.Desktop.Controls/WindowTitleBar/Themes/WindowTitleBarTheme.axaml"));
        XNamespace av   = "https://github.com/avaloniaui";
        XNamespace atom = "https://atomui.net";

        var baseStyle = document.Descendants(av + "Style")
                                .Single(style =>
                                    (string?)style.Attribute("Selector") == "^:is(atom|WindowTitleBar)");
        baseStyle.Elements(av + "Setter").ShouldContain(setter =>
            (string?)setter.Attribute("Property") == "Background" &&
            (string?)setter.Attribute("Value") ==
            "{atom:WindowTitleBarTokenResource ColorBgContainer}");
        baseStyle.Elements(av + "Setter").ShouldNotContain(setter =>
            (string?)setter.Attribute("Property") == "Background" &&
            (string?)setter.Attribute("Value") == "Transparent");

        var templates = document.Descendants(av + "ControlTemplate").ToList();
        templates.Count.ShouldBe(3);

        foreach (var template in templates)
        {
            var frame = template.Elements(av + "Border").Single();
            var layoutPanel = frame.Elements(atom + "WindowTitleBarLayoutPanel").Single();
            var directChildren = layoutPanel.Elements().ToList();

            frame.Attribute("Name")?.Value.ShouldBe("Frame");
            frame.Attribute("Background")?.Value.ShouldBe("{TemplateBinding Background}");
            layoutPanel.Attribute("TitleAlignment")?.Value.ShouldBe("{TemplateBinding TitleAlignment}");
            layoutPanel.Attribute("OsType")?.Value.ShouldBe("{TemplateBinding OsType}");
            layoutPanel.Attribute("NativeChromeInsets")?.Value.ShouldBe("{TemplateBinding NativeChromeInsets}");
            layoutPanel.Attribute("IsCsdEnabled")?.Value.ShouldBe("{TemplateBinding IsCsdEnabled}");
            layoutPanel.Attribute("WindowState")?.Value.ShouldBe("{TemplateBinding HostWindowState}");
            layoutPanel.Attribute("HorizontalSpacing")?.Value.ShouldBe(
                "{atom:WindowTitleBarTokenResource HeaderHorizontalSpacing}");

            directChildren.Count(child =>
                    (string?)child.Attribute(atom + "WindowTitleBarLayoutPanel.Role") == "Leading")
                .ShouldBe(1);
            directChildren.Count(child =>
                    (string?)child.Attribute(atom + "WindowTitleBarLayoutPanel.Role") == "Title")
                .ShouldBe(1);
            directChildren.Count(child =>
                    (string?)child.Attribute(atom + "WindowTitleBarLayoutPanel.Role") == "Trailing")
                .ShouldBe(1);

            layoutPanel.Descendants(atom + "CaptionButtonGroup").Single()
                       .Attribute("Name")?.Value.ShouldBe("PART_CaptionButtonGroup");
        }
    }

    [Fact]
    public void Fullscreen_Title_Hosts_Use_The_Shared_Full_Frame_Layout_With_Managed_Operations()
    {
        var themePaths = new[]
        {
            "src/AtomUI.Desktop.Controls/Window/Themes/WindowDrawnDecorationsTheme.axaml",
            "src/AtomUI.Desktop.Controls/Window/Themes/FullscreenPopoverLayerTheme.axaml"
        };
        XNamespace atom = "https://atomui.net";

        foreach (var themePath in themePaths)
        {
            var document = XDocument.Load(GetRepoFile(themePath));
            var layoutPanel = document.Descendants(atom + "WindowTitleBarLayoutPanel").Single();
            var directChildren = layoutPanel.Elements().ToList();

            layoutPanel.Attribute("TitleAlignment")?.Value.ShouldBe(
                "{Binding $parent[atom:Window].TitleAlignment}");
            layoutPanel.Attribute("OsType")?.Value.ShouldBe("{Binding $parent[atom:Window].OsType}");
            layoutPanel.Attribute("NativeChromeInsets")?.Value.ShouldBe("0");
            layoutPanel.Attribute("WindowState")?.Value.ShouldBe("FullScreen");
            layoutPanel.Attribute("Padding")?.Value.ShouldBe(
                "{atom:WindowTokenResource FullscreenHeaderFramePadding}");
            layoutPanel.Attribute("HorizontalSpacing")?.Value.ShouldBe(
                "{atom:WindowTitleBarTokenResource HeaderHorizontalSpacing}");

            directChildren.Count(child => GetLayoutRole(child) == "Leading").ShouldBe(1);
            directChildren.Count(child => GetLayoutRole(child) == "Title").ShouldBe(1);
            directChildren.Count(child => GetLayoutRole(child) == "Trailing").ShouldBe(1);

            directChildren.Single(child => GetLayoutRole(child) == "Leading")
                          .Descendants()
                          .ShouldBeEmpty();
            var title = directChildren.Single(child => GetLayoutRole(child) == "Title");
            title.Name.LocalName.ShouldBe("DockPanel");
            title.Attribute("LastChildFill")?.Value.ShouldBe("True");
            title.Attribute("HorizontalSpacing")?.Value.ShouldBe(
                "{atom:WindowTitleBarTokenResource LogoAndTitleSpacing}");
            title.Attribute("IsHitTestVisible")?.Value.ShouldBe("False");
            title.Attribute("ClipToBounds")?.Value.ShouldBe("True");
            var logoPresenter = title.Descendants().Single(element =>
                (string?)element.Attribute("Name") == "FullscreenLogoPresenter");
            logoPresenter.Attribute("DockPanel.Dock")?.Value.ShouldBe("Left");
            logoPresenter.Attribute("Content")?.Value.ShouldBe(
                "{Binding $parent[atom:Window].EffectiveFullscreenLogo}");
            logoPresenter.Attribute("ContentTemplate")?.Value.ShouldBe(
                "{Binding $parent[atom:Window].EffectiveFullscreenLogoTemplate}");
            var titleText = title.Descendants().Single(element =>
                (string?)element.Attribute("Name") == "FullscreenTitleText");
            titleText.Attribute("TextWrapping")?.Value.ShouldBe("NoWrap");
            titleText.Attribute("TextTrimming")?.Value.ShouldBe("CharacterEllipsis");

            var trailing = directChildren.Single(child => GetLayoutRole(child) == "Trailing");
            trailing.Descendants(atom + "CaptionButton").Count().ShouldBe(2);
        }
    }

    [Theory]
    [InlineData(40, 80)]
    [InlineData(0, 0)]
    [InlineData(-1, 0)]
    public void Windows_Csd_Minimum_Height_Always_Leaves_A_Nonzero_Content_Surface(
        double titleBarHeight,
        double expectedMinimumHeight)
    {
        AtomUI.Desktop.Controls.Window.CalculateWindowsCsdMinimumHeight(titleBarHeight)
              .ShouldBe(expectedMinimumHeight);
    }

    [Fact]
    public void Hidden_Csd_TitleBar_Removes_Only_The_Drawn_TitleBar_From_Content_Margin()
    {
        var decorationMargin = new Thickness(3, 44, 5, 7);

        AtomUI.Desktop.Controls.Window.CalculateEffectiveContentFrameMargin(
                decorationMargin,
                isCsdEnabled: true,
                isTitleBarVisible: false,
                drawnTitleBarHeight: 40)
            .ShouldBe(new Thickness(3, 4, 5, 7));

        AtomUI.Desktop.Controls.Window.CalculateEffectiveContentFrameMargin(
                decorationMargin,
                isCsdEnabled: true,
                isTitleBarVisible: false,
                drawnTitleBarHeight: 80)
            .ShouldBe(new Thickness(3, 0, 5, 7));
    }

    [Theory]
    [InlineData(false, false, 40)]
    [InlineData(true, true, 40)]
    [InlineData(true, false, 0)]
    [InlineData(true, false, -1)]
    [InlineData(true, false, double.NaN)]
    public void Content_Margin_Remains_Unchanged_Without_A_Valid_Hidden_Csd_TitleBar(
        bool isCsdEnabled,
        bool isTitleBarVisible,
        double drawnTitleBarHeight)
    {
        var decorationMargin = new Thickness(3, 44, 5, 7);

        AtomUI.Desktop.Controls.Window.CalculateEffectiveContentFrameMargin(
                decorationMargin,
                isCsdEnabled,
                isTitleBarVisible,
                drawnTitleBarHeight)
            .ShouldBe(decorationMargin);
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public void Windows_Csd_Window_Rejects_A_Minimum_Height_Below_Its_Safe_Content_Surface()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        AvaloniaTestApp.EnsureInitialized();
        var window = new AtomUI.Desktop.Controls.Window
        {
            TitleBarHeight = 40
        };

        window.MinHeight.ShouldBe(80);

        window.MinHeight = 20;
        window.MinHeight.ShouldBe(80);

        window.MinHeight = 160;
        window.MinHeight.ShouldBe(160);
    }

    [Fact]
    public void Windows_Caption_Buttons_Suppress_Stale_Hover_After_Window_State_Transitions()
    {
        var buttonSource = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/WindowTitleBar/WindowsCaptionButton.cs"));
        var captionThemeSource = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/WindowTitleBar/Themes/CaptionButtonGroupTheme.axaml"));
        var themeSource = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/WindowTitleBar/Themes/WindowsCaptionButtonTheme.axaml"));

        buttonSource.ShouldContain("HostWindowStateProperty.Changed.AddClassHandler<WindowsCaptionButton>");
        buttonSource.ShouldContain("button.InvalidatePointerOverVisualState()");
        captionThemeSource.ShouldContain("HostWindowState=\"{TemplateBinding HostWindowState}\"");
        themeSource.ShouldContain("^[IsPointerOverSuppressed=False]:pointerover");
    }

    [Fact]
    public void AtomUI_Defaults_Use_Ordered_Windows_Composition_Fallbacks_For_Live_Resize()
    {
        var source = File.ReadAllText(GetRepoFile("src/AtomUI.Core/AppBuilderExtensions.cs"));

        source.ShouldContain(".With(new Win32PlatformOptions");
        source.ShouldContain("Win32RenderingMode.AngleEgl");
        source.ShouldContain("Win32RenderingMode.Software");
        source.ShouldContain("Win32CompositionMode.WinUIComposition");
        source.ShouldContain("Win32CompositionMode.DirectComposition");
        source.ShouldContain("Win32CompositionMode.LowLatencyDxgiSwapChain");
        source.ShouldContain("Win32CompositionMode.RedirectionSurface");
        source.IndexOf("Win32CompositionMode.WinUIComposition", StringComparison.Ordinal)
              .ShouldBeLessThan(source.IndexOf("Win32CompositionMode.DirectComposition", StringComparison.Ordinal));
        source.IndexOf("Win32CompositionMode.DirectComposition", StringComparison.Ordinal)
              .ShouldBeLessThan(source.IndexOf("Win32CompositionMode.LowLatencyDxgiSwapChain", StringComparison.Ordinal));
        source.IndexOf("Win32CompositionMode.LowLatencyDxgiSwapChain", StringComparison.Ordinal)
              .ShouldBeLessThan(source.IndexOf("Win32CompositionMode.RedirectionSurface", StringComparison.Ordinal));
        source.ShouldNotContain("WindowsAppBuilderDefaults");
        source.ShouldNotContain("ShouldRenderOnUIThread = true");
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

        throw new FileNotFoundException($"Could not find repository file: {relativePath}");
    }

    private static string? GetRepoFileOrNull(string relativePath)
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

        return null;
    }

    private static string? GetLayoutRole(XElement element)
    {
        return element.Attributes()
                      .SingleOrDefault(attribute =>
                          attribute.Name.LocalName == "WindowTitleBarLayoutPanel.Role")
                      ?.Value;
    }

    private static bool IsPhysicalPixelAligned(double value, double renderScaling)
    {
        var physicalValue = value * renderScaling;
        return Math.Abs(physicalValue - Math.Round(physicalValue)) < 0.000001;
    }

    private static void AssertTitleBarVisibilityContract(XDocument document, string elementName)
    {
        var element = document.Descendants()
                              .Single(candidate =>
                                  (string?)candidate.Attribute("Name") == elementName ||
                                  candidate.Name.LocalName == elementName);
        var multiBinding = element.Elements()
                                  .Single(property => property.Name.LocalName.EndsWith(".IsVisible"))
                                  .Elements()
                                  .Single(binding => binding.Name.LocalName == "MultiBinding");

        multiBinding.Attribute("Converter").ShouldNotBeNull().Value.ShouldBe("{x:Static BoolConverters.And}");
        var bindings = multiBinding.Elements()
                                   .Where(binding => binding.Name.LocalName == "Binding")
                                   .ToArray();
        bindings.ShouldContain(binding =>
            (string?)binding.Attribute("RelativeSource") == "{RelativeSource TemplatedParent}" &&
            (string?)binding.Attribute("Path") == "HasTitleBar");
        bindings.ShouldContain(binding =>
            (string?)binding.Attribute("Path") == "$parent[atom:Window].IsTitleBarVisible");
    }
}
