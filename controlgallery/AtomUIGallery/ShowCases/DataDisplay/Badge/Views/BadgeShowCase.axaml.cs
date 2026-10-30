using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUIGallery.ShowCases.Badge;

public partial class BadgeShowCase : GalleryReactiveUserControl<BadgeViewModel>
{
    public const string LanguageId = nameof(BadgeShowCase);

    public const string CountIndicatorCodeSnippet =
        """
        <Styles xmlns="https://github.com/avaloniaui"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                xmlns:atom="https://atomui.net">
          <Style Selector="atom|CountBadge">
            <atom:CountBadgeIndicatorStyle x:SetterTargetType="Control">
              <Setter Property="Opacity" Value="0.85" />
            </atom:CountBadgeIndicatorStyle>
          </Style>
        </Styles>
        """;

    public const string DotIndicatorCodeSnippet =
        """
        <Styles xmlns="https://github.com/avaloniaui"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                xmlns:atom="https://atomui.net">
          <Style Selector="atom|DotBadge">
            <atom:DotBadgeIndicatorStyle x:SetterTargetType="Control">
              <Setter Property="Opacity" Value="0.85" />
            </atom:DotBadgeIndicatorStyle>
          </Style>
        </Styles>
        """;

    public const string RibbonIndicatorCodeSnippet =
        """
        <Styles xmlns="https://github.com/avaloniaui"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                xmlns:atom="https://atomui.net">
          <Style Selector="atom|RibbonBadge">
            <atom:RibbonBadgeIndicatorStyle x:SetterTargetType="Control">
              <Setter Property="Opacity" Value="0.85" />
            </atom:RibbonBadgeIndicatorStyle>
          </Style>
        </Styles>
        """;

    public const string RibbonContentCodeSnippet =
        """
        <Styles xmlns="https://github.com/avaloniaui"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                xmlns:atom="https://atomui.net">
          <Style Selector="atom|RibbonBadge">
            <atom:RibbonBadgeContentStyle x:SetterTargetType="TextBlock">
              <Setter Property="FontWeight" Value="SemiBold" />
            </atom:RibbonBadgeContentStyle>
          </Style>
        </Styles>
        """;

    public BadgeShowCase()
    {
        InitializeComponent();
    }

    public void HandleCrossRootSemanticPreviewLoaded(object? sender, RoutedEventArgs args)
    {
        if (sender is not SemanticPartPreview preview)
        {
            return;
        }

        Dispatcher.UIThread.Post(
            () => RegisterRuntimeAdorner(preview),
            DispatcherPriority.Loaded);
    }

    public void HandleCrossRootSemanticPreviewUnloaded(object? sender, RoutedEventArgs args)
    {
        if (sender is SemanticPartPreview preview)
        {
            preview.AdditionalRoots.Clear();
        }
    }

    private static void RegisterRuntimeAdorner(SemanticPartPreview preview)
    {
        preview.AdditionalRoots.Clear();
        if (!preview.IsAttachedToVisualTree() || preview.PreviewContent is not Control owner)
        {
            return;
        }

        var adornerLayer = AdornerLayer.GetAdornerLayer(owner);
        var runtimeAdorner = adornerLayer?.Children.FirstOrDefault(child =>
            ReferenceEquals(AdornerLayer.GetAdornedElement(child), owner));
        if (runtimeAdorner is not null)
        {
            preview.AdditionalRoots.Add(runtimeAdorner);
        }
    }
}
