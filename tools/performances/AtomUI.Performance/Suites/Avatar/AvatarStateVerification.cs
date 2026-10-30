using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using AtomUI.Icons.AntDesign;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace AtomUI.Performance;

internal static partial class Program
{
    private static bool RunAvatarStateVerification()
    {
        var failures = new List<string>();
        VerifyAvatarStaticPresentersUseVisibility(failures);
        VerifyAvatarContentTypeSwitching(failures);
        VerifyAvatarGroupFoldLifecycle(failures);

        if (failures.Count == 0)
        {
            Console.WriteLine("Avatar state verification passed.");
            return true;
        }

        Console.Error.WriteLine("Avatar state verification failed:");
        foreach (var failure in failures)
        {
            Console.Error.WriteLine($"- {failure}");
        }
        return false;
    }

    private static void VerifyAvatarStaticPresentersUseVisibility(ICollection<string> failures)
    {
        VerifyAvatarPresenterVisibility(
            new Avatar { Icon = new UserOutlined() },
            "Icon Avatar",
            "IconPresenter",
            failures);
        VerifyAvatarPresenterVisibility(
            new Avatar { Text = "U" },
            "Text Avatar",
            "PART_TextPresenter",
            failures);
        VerifyAvatarPresenterVisibility(
            new Avatar { Source = CreateAvatarFileSource() },
            "Svg Avatar",
            "ImagePresenter",
            failures);
        VerifyAvatarPresenterVisibility(
            new Avatar { Source = AvatarBitmapSource.Value },
            "Bitmap Avatar",
            "ImagePresenter",
            failures);
    }

    private static void VerifyAvatarPresenterVisibility(Avatar avatar,
                                                        string label,
                                                        string visiblePresenterName,
                                                        ICollection<string> failures)
    {
        using var realized = RealizeControl(avatar);
        WaitUntilAvatarImageTerminal(avatar, realized.Window);
        ExpectAvatarPresenterVisibility(avatar, label, visiblePresenterName, failures);
    }

    private static void WaitUntilAvatarImageTerminal(Avatar avatar, Avalonia.Controls.Window window)
    {
        if (avatar.Source is null)
        {
            return;
        }
        WaitUntil(() => avatar.LoadState is ImageLoadState.Loaded or ImageLoadState.Failed, "avatar image terminal state");
        RefreshLayout(window);
    }

    private static void VerifyAvatarContentTypeSwitching(ICollection<string> failures)
    {
        var avatar = new Avatar
        {
            Icon = new UserOutlined()
        };

        using var realized = RealizeControl(avatar);
        var firstIconPresenter = FindVisualByName<IconPresenter>(avatar, "IconPresenter");
        Expect(firstIconPresenter != null,
            "Avatar should start with IconPresenter for Icon content.",
            failures);
        ExpectAvatarPresenterVisibility(avatar, "Initial Icon Avatar", "IconPresenter", failures);

        avatar.SetCurrentValue(Avatar.TextProperty, "USER");
        RefreshLayout(realized.Window);
        Expect(firstIconPresenter?.GetVisualParent() != null && !firstIconPresenter.IsVisible,
            "Avatar should keep static IconPresenter hidden when switching to Text.",
            failures);
        ExpectAvatarPresenterVisibility(avatar, "Avatar after switching to Text", "PART_TextPresenter", failures);

        var textPresenter = FindVisualByName<Avalonia.Controls.TextBlock>(avatar, "PART_TextPresenter");
        avatar.SetCurrentValue(AbstractAvatar.SourceProperty, CreateAvatarFileSource());
        WaitUntilAvatarImageTerminal(avatar, realized.Window);
        Expect(textPresenter?.GetVisualParent() != null && !textPresenter.IsVisible,
            "Avatar should keep static TextBlock hidden when switching to image content.",
            failures);
        ExpectAvatarPresenterVisibility(avatar, "Avatar after switching to image source", "ImagePresenter", failures);

        var imagePresenter = FindVisualByName<Avalonia.Controls.Image>(avatar, "ImagePresenter");
        avatar.SetCurrentValue(AbstractAvatar.SourceProperty, AvatarBitmapSource.Value);
        WaitUntilAvatarImageTerminal(avatar, realized.Window);
        Expect(imagePresenter != null && imagePresenter.IsVisible,
            "Avatar should keep ImagePresenter visible when switching between image sources.",
            failures);
        ExpectAvatarPresenterVisibility(avatar, "Avatar after switching to bytes source", "ImagePresenter", failures);
    }

    private static void VerifyAvatarGroupFoldLifecycle(ICollection<string> failures)
    {
        var noFoldGroup = CreateAvatarGroup(maxDisplayCount: null);
        using (var realized = RealizeControl(noFoldGroup))
        {
            Expect(CountVisualByTypeName(noFoldGroup, "FlyoutHost") == 0,
                "AvatarGroup without folding should not create FlyoutHost visuals.",
                failures);
            Expect(GetPrivateField(noFoldGroup, "AtomUI.Desktop.Controls.AvatarGroup", "_foldCountFlyout") == null,
                "AvatarGroup without folding should keep _foldCountFlyout null.",
                failures);
            Expect(CountVisualByTypeName(noFoldGroup, "Avatar") == 4,
                "AvatarGroup without folding should show all four avatars.",
                failures);
        }

        var foldGroup = CreateAvatarGroup(maxDisplayCount: 2);
        using (var realized = RealizeControl(foldGroup))
        {
            Expect(CountVisualByTypeName(foldGroup, "FlyoutHost") == 1,
                "Folded AvatarGroup should create one FlyoutHost.",
                failures);
            Expect(CountVisualByTypeName(foldGroup, "Avatar") == 3,
                "Folded AvatarGroup should show two children plus one fold avatar.",
                failures);

            foldGroup.SetCurrentValue(AvatarGroup.MaxDisplayCountProperty, null);
            RefreshLayout(realized.Window);
            Expect(CountVisualByTypeName(foldGroup, "FlyoutHost") == 0,
                "AvatarGroup should remove FlyoutHost when folding is disabled.",
                failures);
            Expect(GetPrivateField(foldGroup, "AtomUI.Desktop.Controls.AvatarGroup", "_foldCountFlyout") == null,
                "AvatarGroup should release _foldCountFlyout when folding is disabled.",
                failures);
            Expect(CountVisualByTypeName(foldGroup, "Avatar") == 4,
                "AvatarGroup should restore all child avatars when folding is disabled.",
                failures);

            foldGroup.SetCurrentValue(AvatarGroup.MaxDisplayCountProperty, 2);
            RefreshLayout(realized.Window);
            Expect(CountVisualByTypeName(foldGroup, "FlyoutHost") == 1,
                "AvatarGroup should recreate FlyoutHost when folding is re-enabled.",
                failures);
            Expect(CountVisualByTypeName(foldGroup, "Avatar") == 3,
                "AvatarGroup should fold back to visible children plus fold avatar.",
                failures);
        }

        Expect(GetPrivateField(foldGroup, "AtomUI.Desktop.Controls.AvatarGroup", "_foldCountFlyout") == null,
            "Detached folded AvatarGroup should release _foldCountFlyout.",
            failures);
    }

    private static void ExpectAvatarPresenterVisibility(Avatar avatar,
                                                        string label,
                                                        string visiblePresenterName,
                                                        ICollection<string> failures)
    {
        var presenters = new[]
        {
            FindVisualByName<Control>(avatar, "IconPresenter"),
            FindVisualByName<Control>(avatar, "ImagePresenter"),
            FindVisualByName<Control>(avatar, "PART_TextPresenter")
        };

        Expect(presenters.All(presenter => presenter != null),
            $"{label} should keep all static presenters in the template.",
            failures);
        foreach (var presenter in presenters.OfType<Control>())
        {
            var shouldBeVisible = presenter.Name == visiblePresenterName;
            Expect(presenter.IsVisible == shouldBeVisible,
                $"{label} presenter {presenter.Name} visibility should be {shouldBeVisible}.",
                failures);
        }
    }

    private static int CountVisualByTypeName(Control root, string typeName)
    {
        return root.GetSelfAndVisualDescendants()
                   .OfType<Control>()
                   .Count(control => control.GetType().Name == typeName);
    }
}
