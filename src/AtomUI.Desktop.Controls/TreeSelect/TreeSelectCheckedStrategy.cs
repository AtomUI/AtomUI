namespace AtomUI.Desktop.Controls;

[Flags]
public enum TreeSelectCheckedStrategy
{
    ShowParent = 0x1,
    ShowChild = 0x2,
    All = ShowParent | ShowChild,
}