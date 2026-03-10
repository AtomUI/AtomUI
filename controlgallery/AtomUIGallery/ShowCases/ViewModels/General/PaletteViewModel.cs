using System.Collections.ObjectModel;
using AtomUI.Controls;
using AtomUI.Theme.Palette;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class PaletteMetaItem
{
    public string Title;
    public string Desc;
    public PresetPrimaryColor PresetPrimaryColor;

    public PaletteMetaItem(string title, string desc, PresetPrimaryColor presetPrimaryColor)
    {
        Title              = title;
        Desc               = desc;
        PresetPrimaryColor = presetPrimaryColor;
    }
}

public class PresetPaletteInfoPair
{
    PaletteMetaItem Left { get; set; }
    PaletteMetaItem Right { get; set; }

    public PresetPaletteInfoPair(PaletteMetaItem left, PaletteMetaItem right)
    {
        Left  = left;
        Right = right;
    }
}

public class PaletteViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "Palette";

    public IScreen HostScreen { get; }

    public string? UrlPathSegment { get; } = ID.ToString();

    public ObservableCollection<PresetPaletteInfoPair> PresetPaletteInfos { get; set; }
    
    public PaletteViewModel(IScreen screen)
    {
        HostScreen         = screen;
        PresetPaletteInfos = new ObservableCollection<PresetPaletteInfoPair>();
        PresetPaletteInfos.Add(new PresetPaletteInfoPair(new("Dust Red / 薄暮", "斗志、奔放", PresetPrimaryColor.Red),
            new("Volcano / 火山", "醒目、澎湃", PresetPrimaryColor.Volcano)));
        PresetPaletteInfos.Add(new PresetPaletteInfoPair(new("Sunset Orange / 日暮", "温暖、欢快", PresetPrimaryColor.Orange),
            new("Calendula Gold / 金盏花", "活力、积极", PresetPrimaryColor.Gold)));
        PresetPaletteInfos.Add(new PresetPaletteInfoPair(new("Sunrise Yellow / 日出", "出生、阳光", PresetPrimaryColor.Yellow),
            new("Lime / 青柠", "自然、生机", PresetPrimaryColor.Lime)));
        PresetPaletteInfos.Add(new PresetPaletteInfoPair(new("Polar Green / 极光绿", "健康、创新", PresetPrimaryColor.Green),
            new("Cyan / 明青", "希望、坚强", PresetPrimaryColor.Cyan)));
        PresetPaletteInfos.Add(new PresetPaletteInfoPair(
            new("Daybreak Blue / 拂晓蓝", "包容、科技、普惠", PresetPrimaryColor.Blue),
            new("Geek Blue / 极客蓝", "探索、钻研", PresetPrimaryColor.GeekBlue)));
        PresetPaletteInfos.Add(new PresetPaletteInfoPair(new("Golden Purple / 酱紫", "优雅、浪漫", PresetPrimaryColor.Purple),
            new("Magenta / 法式洋红", "明快、感性", PresetPrimaryColor.Magenta)));
    }
}