using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using AtomUI.Icons.AntDesign;
using Avalonia.Controls;
using Avalonia.Media;

namespace AtomUI.Performance;

internal static partial class Program
{
    private static readonly Lazy<BytesImageSource> AvatarBitmapSource = new(CreateAvatarBytesSource);

    private static IReadOnlyList<PerfScenario> CreateAvatarScenarios()
    {
        return
        [
            new PerfScenario("Avatar.Icon.Middle", _ => CreateIconAvatar()),
            new PerfScenario("Avatar.Icon.Custom64", _ => CreateIconAvatar(64)),
            new PerfScenario("Avatar.Text.Single", _ => new Avatar { Text = "U" }),
            new PerfScenario("Avatar.Text.Long.Custom40", _ => new Avatar { Size = 40, Text = "USER" }),
            new PerfScenario("Avatar.Svg", _ => new Avatar { Source = CreateAvatarFileSource() }),
            new PerfScenario("Avatar.Bitmap", _ => new Avatar { Source = AvatarBitmapSource.Value }),
            new PerfScenario("Avatar.Group.NoFold", _ => CreateAvatarGroup(maxDisplayCount: null)),
            new PerfScenario("Avatar.Group.Fold2", _ => CreateAvatarGroup(maxDisplayCount: 2)),
            new PerfScenario("Avatar.GalleryShape", _ => CreateAvatarGalleryShape())
        ];
    }

    private static Avatar CreateIconAvatar(double size = double.NaN)
    {
        return new Avatar
        {
            Size = size,
            Icon = new UserOutlined()
        };
    }

    private static AvatarGroup CreateAvatarGroup(int? maxDisplayCount)
    {
        var group = new AvatarGroup
        {
            MaxDisplayCount           = maxDisplayCount,
            FoldInfoAvatarForeground  = AvatarBrush("#f56a00"),
            FoldInfoAvatarBackground  = AvatarBrush("#fde3cf")
        };

        group.Children.Add(new Avatar { Source = CreateAvatarFileSource() });
        group.Children.Add(new Avatar { Background = AvatarBrush("#f56a00"), Text = "K" });
        group.Children.Add(new Avatar { Background = AvatarBrush("#87d068"), Icon = new UserOutlined() });
        group.Children.Add(new Avatar { Background = AvatarBrush("#1677ff"), Icon = new AntDesignOutlined() });
        return group;
    }

    private static Control CreateAvatarGalleryShape()
    {
        var root = new StackPanel
        {
            Spacing = 20
        };

        var basic = new StackPanel
        {
            Spacing = 20
        };
        basic.Children.Add(CreateAvatarRow(
            CreateIconAvatar(64),
            new Avatar { SizeType = CustomizableSizeType.Large, Icon = new UserOutlined() },
            new Avatar { Icon = new UserOutlined() },
            new Avatar { SizeType = CustomizableSizeType.Small, Icon = new UserOutlined() },
            CreateIconAvatar(14)));
        basic.Children.Add(CreateAvatarRow(
            new Avatar { Shape = AvatarShape.Square, Size = 64, Icon = new UserOutlined() },
            new Avatar { Shape = AvatarShape.Square, SizeType = CustomizableSizeType.Large, Icon = new UserOutlined() },
            new Avatar { Shape = AvatarShape.Square, Icon = new UserOutlined() },
            new Avatar { Shape = AvatarShape.Square, SizeType = CustomizableSizeType.Small, Icon = new UserOutlined() },
            new Avatar { Shape = AvatarShape.Square, Size = 14, Icon = new UserOutlined() }));
        root.Children.Add(basic);

        root.Children.Add(CreateAvatarRow(
            new Avatar { Icon = new UserOutlined() },
            new Avatar { Text = "U" },
            new Avatar { Size = 40, Text = "USER" },
            new Avatar { Source = CreateAvatarFileSource() },
            new Avatar { Background = AvatarBrush("#fde3cf"), Foreground = AvatarBrush("#f56a00"), Text = "U" },
            new Avatar { Background = AvatarBrush("#87d068"), Icon = new UserOutlined() }));

        root.Children.Add(CreateAvatarRow(
            new Avatar
            {
                Background = AvatarBrush("#f56a00"),
                Gap        = 4,
                SizeType   = CustomizableSizeType.Large,
                Text       = "Edward"
            },
            new AtomUI.Desktop.Controls.Button
            {
                ButtonType = ButtonType.Default,
                SizeType   = CustomizableSizeType.Small,
                Content    = "ChangeUser"
            },
            new AtomUI.Desktop.Controls.Button
            {
                ButtonType = ButtonType.Default,
                SizeType   = CustomizableSizeType.Small,
                Content    = "ChangeGap"
            }));

        root.Children.Add(CreateAvatarGroup(maxDisplayCount: null));
        root.Children.Add(new AtomUI.Desktop.Controls.Separator());
        root.Children.Add(CreateAvatarGroup(maxDisplayCount: 2));
        root.Children.Add(new AtomUI.Desktop.Controls.Separator());
        root.Children.Add(new AvatarGroup
        {
            MaxDisplayCount          = 2,
            SizeType                 = CustomizableSizeType.Large,
            FoldInfoAvatarForeground = AvatarBrush("#f56a00"),
            FoldInfoAvatarBackground = AvatarBrush("#fde3cf"),
            Children =
            {
                new Avatar { Source = CreateAvatarFileSource() },
                new Avatar { Background = AvatarBrush("#f56a00"), Text = "K" },
                new Avatar { Background = AvatarBrush("#87d068"), Icon = new UserOutlined() },
                new Avatar { Background = AvatarBrush("#1677ff"), Icon = new AntDesignOutlined() }
            }
        });
        root.Children.Add(new AtomUI.Desktop.Controls.Separator());
        root.Children.Add(new AvatarGroup
        {
            MaxDisplayCount             = 2,
            SizeType                    = CustomizableSizeType.Large,
            FoldAvatarFlyoutTriggerType = FlyoutTriggerType.Click,
            FoldInfoAvatarForeground    = AvatarBrush("#f56a00"),
            FoldInfoAvatarBackground    = AvatarBrush("#fde3cf"),
            Children =
            {
                new Avatar { Source = AvatarBitmapSource.Value },
                new Avatar { Background = AvatarBrush("#f56a00"), Text = "K" },
                new Avatar { Background = AvatarBrush("#87d068"), Icon = new UserOutlined() },
                new Avatar { Background = AvatarBrush("#1677ff"), Icon = new AntDesignOutlined() }
            }
        });
        root.Children.Add(new AtomUI.Desktop.Controls.Separator());
        root.Children.Add(new AvatarGroup
        {
            Shape = AvatarShape.Square,
            Children =
            {
                new Avatar { Background = AvatarBrush("#fde3cf"), Text = "A" },
                new Avatar { Background = AvatarBrush("#f56a00"), Text = "K" },
                new Avatar { Background = AvatarBrush("#87d068"), Icon = new UserOutlined() },
                new Avatar { Background = AvatarBrush("#1677ff"), Icon = new AntDesignOutlined() }
            }
        });

        return root;
    }

    private static StackPanel CreateAvatarRow(params Control[] controls)
    {
        var row = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing     = 10
        };
        foreach (var control in controls)
        {
            row.Children.Add(control);
        }

        return row;
    }

    private static string GetAvatarSvgPath()
    {
        return Path.GetFullPath("resources/images/readme/AntDesign.svg");
    }

    private static FileImageSource CreateAvatarFileSource()
    {
        return new FileImageSource(GetAvatarSvgPath());
    }

    private static IBrush AvatarBrush(string color)
    {
        return Avalonia.Media.Brush.Parse(color);
    }

    private static BytesImageSource CreateAvatarBytesSource()
    {
        var bytes = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAFgwJ/lw7v9wAAAABJRU5ErkJggg==");
        return new BytesImageSource(bytes);
    }
}
