using System;
using System.Collections.Generic;

namespace AtomUI.Desktop.Controls;

public class SplitterResizeEventArgs : EventArgs
{
    public SplitterResizeEventArgs(int handleIndex, IReadOnlyList<double> sizes)
    {
        HandleIndex = handleIndex;
        Sizes = sizes;
    }

    public int HandleIndex { get; }
    public IReadOnlyList<double> Sizes { get; }
}
