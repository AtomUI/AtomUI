using AtomUI.Theme.Schema;

namespace AtomUIGallery.Workspace.Theming;

internal sealed record GalleryTokenGuidance(bool IsReadOnly, string Description);

/// <summary>Compatibility annotations only. Token enumeration and values always come from runtime descriptors.</summary>
internal static class GalleryTokenGuidanceCatalog
{
    // Finite compatibility decisions from the reviewed control contracts; this is not a duplicate token schema.
    private static readonly IReadOnlyDictionary<string, string> Inactive = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["AtomUI:Button.ExtraContentItemSpacing"] = "Reserved",
        ["AtomUI:Button.ExtraContentMargin"] = "Reserved",
        ["AtomUI:Button.ExtraContentMarginLG"] = "Reserved",
        ["AtomUI:Button.ExtraContentMarginSM"] = "Reserved",
        ["AtomUI:Button.GroupBorderColor"] = "Legacy",
        ["AtomUI:ButtonSpinner.ControlWidth"] = "Legacy",
        ["AtomUI:Card.ActionsSpacing"] = "Reserved",
        ["AtomUI:Card.CardHeadPadding"] = "Reserved",
        ["AtomUI:Card.CardPaddingBase"] = "Reserved",
        ["AtomUI:Card.TabsMarginBottom"] = "Reserved",
        ["AtomUI:DataGrid.ExpandIconHalfInner"] = "Reserved",
        ["AtomUI:DataGrid.ExpandIconScale"] = "Reserved",
        ["AtomUI:Expander.LeftExpandButtonVMargin"] = "Reserved",
        ["AtomUI:Expander.RightExpandButtonVMargin"] = "Reserved",
        ["AtomUI:FloatButton.CircleBadgeOffset"] = "Reserved",
        ["AtomUI:FloatButton.SquareBadgeOffset"] = "Reserved",
        ["AtomUI:Form.VerticalLabelPadding"] = "Legacy",
        ["AtomUI:GalleryShowCaseHeader.MetadataItemSpacing"] = "Legacy",
        ["AtomUI:GroupBox.OrientationMarginPercent"] = "Reserved",
        ["AtomUI:GroupBox.TextPaddingInline"] = "Reserved",
        ["AtomUI:GroupBox.VerticalMarginInline"] = "Reserved",
        ["AtomUI:Menu.DangerItemColor"] = "Reserved",
        ["AtomUI:Menu.DangerItemHoverColor"] = "Reserved",
        ["AtomUI:Menu.ItemIconMarginInlineEnd"] = "Legacy",
        ["AtomUI:Menu.MenuTearOffHeight"] = "Reserved",
        ["AtomUI:MessageCard.CardHeight"] = "Reserved",
        ["AtomUI:NavMenu.CollapsedWidth"] = "Legacy",
        ["AtomUI:NavMenu.DangerItemActiveBg"] = "Reserved",
        ["AtomUI:NavMenu.DangerItemColor"] = "Reserved",
        ["AtomUI:NavMenu.DangerItemHoverColor"] = "Reserved",
        ["AtomUI:NavMenu.DangerItemSelectedBg"] = "Reserved",
        ["AtomUI:NavMenu.DangerItemSelectedColor"] = "Reserved",
        ["AtomUI:NavMenu.DarkDangerItemActiveBg"] = "Reserved",
        ["AtomUI:NavMenu.DarkDangerItemColor"] = "Reserved",
        ["AtomUI:NavMenu.DarkDangerItemHoverColor"] = "Reserved",
        ["AtomUI:NavMenu.DarkDangerItemSelectedBg"] = "Reserved",
        ["AtomUI:NavMenu.DarkDangerItemSelectedColor"] = "Reserved",
        ["AtomUI:NavMenu.KeyGestureColor"] = "Reserved",
        ["AtomUI:NavMenu.MenuSubMenuBg"] = "Legacy",
        ["AtomUI:Pagination.InputOutlineOffset"] = "Reserved",
        ["AtomUI:Pagination.ItemLinkBg"] = "Reserved",
        ["AtomUI:PopupConfirm.PopupMinHeight"] = "Reserved",
        ["AtomUI:PopupConfirm.PopupMinWidth"] = "Reserved",
        ["AtomUI:ProgressBar.ProgressActiveMotionDuration"] = "Reserved",
        ["AtomUI:ProgressBar.ProgressStepMarginInlineEnd"] = "Reserved",
        ["AtomUI:ProgressBar.ProgressStepMinWidth"] = "Reserved",
        ["AtomUI:RadioButton.RadioBgColor"] = "Legacy",
        ["AtomUI:Select.FixedItemMargin"] = "Legacy",
        ["AtomUI:Select.SelectAffixPadding"] = "Legacy",
        ["AtomUI:Separator.HorizontalWithTextGutterMargin"] = "Reserved",
        ["AtomUI:Slider.MarginPartWithMark"] = "Reserved",
        ["AtomUI:TabControl.HorizontalMargin"] = "Legacy",
        ["AtomUI:ToolTip.MotionDuration"] = "Reserved",
        ["AtomUI:TreeView.DirectoryNodeSelectedBg"] = "Reserved",
        ["AtomUI:TreeView.DirectoryNodeSelectedColor"] = "Reserved",
        ["AtomUI:WindowTitleBar.CloseHoverBackgroundColor"] = "Legacy",
        ["AtomUI:WindowTitleBar.ClosePressedBackgroundColor"] = "Legacy",
        ["AtomUI:WindowTitleBar.ForegroundColor"] = "Legacy",
        ["AtomUI:WindowTitleBar.HoverBackgroundColor"] = "Legacy",
        ["AtomUI:WindowTitleBar.PressedBackgroundColor"] = "Legacy",
    };

    internal static GalleryTokenGuidance Describe(ControlTokenDescriptor? owner, string name, bool isOwn)
    {
        if (owner is null || !isOwn)
        {
            return new GalleryTokenGuidance(false, "Global schema · effect depends on owner, state and local values");
        }
        var alias = owner.OwnTokenAliases.FirstOrDefault(item => item.Name == name);
        if (alias is not null)
        {
            return new GalleryTokenGuidance(false, $"Compatibility alias of {alias.CanonicalName} · conflicting inputs are diagnosed");
        }
        if (Inactive.TryGetValue($"{owner.Identity}.{name}", out var category))
        {
            return new GalleryTokenGuidance(true, category == "Unreviewed"
                ? "Consumer contract unreviewed · retained in full JSON, no visual-effect claim"
                : $"{category} · no built-in visual consumer · retained in full JSON");
        }
        return new GalleryTokenGuidance(false, "Own schema · applicability and units follow the control contract");
    }
}
