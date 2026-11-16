using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

// The selected item's text of the list and tree controls: PrimeUIX highlight.color
// (PrimeNGAppearanceRules.HighlightColor), drawn on the selected background ControlItemBgActive (highlight.background).
// Aura and Lara: primary.700 / white at 87% on the pale highlight; Nora and noir: the contrast colour on the solid
// primary. Select sets its OptionSelectedColor in PrimeNGSelectTokens.

/// <summary>
/// Cascader under the PrimeNG presets: the selected item's text <c>OptionSelectedColor</c> is PrimeUIX <c>highlight.color</c>.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGCascaderHighlightTokens : IControlTokenAlgorithm<CascaderToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, CascaderToken token)
    {
        token.OptionSelectedColor = PrimeNGPresets.Rules(context, effectiveGlobal).HighlightColor(effectiveGlobal);
    }
}

/// <summary>
/// ComboBox under the PrimeNG presets: the selected item's text <c>ItemSelectedColor</c> is PrimeUIX <c>highlight.color</c>.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGComboBoxHighlightTokens : IControlTokenAlgorithm<ComboBoxToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, ComboBoxToken token)
    {
        token.ItemSelectedColor = PrimeNGPresets.Rules(context, effectiveGlobal).HighlightColor(effectiveGlobal);
    }
}

/// <summary>
/// ListBox under the PrimeNG presets: the selected item's text <c>ItemSelectedColor</c> is PrimeUIX <c>highlight.color</c>.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGListBoxHighlightTokens : IControlTokenAlgorithm<ListBoxToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, ListBoxToken token)
    {
        token.ItemSelectedColor = PrimeNGPresets.Rules(context, effectiveGlobal).HighlightColor(effectiveGlobal);
    }
}

/// <summary>
/// ListView under the PrimeNG presets: the selected item's text <c>ItemSelectedColor</c> is PrimeUIX <c>highlight.color</c>.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGListViewHighlightTokens : IControlTokenAlgorithm<ListViewToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, ListViewToken token)
    {
        token.ItemSelectedColor = PrimeNGPresets.Rules(context, effectiveGlobal).HighlightColor(effectiveGlobal);
    }
}

/// <summary>
/// TransferListView under the PrimeNG presets: the selected item's text <c>ItemSelectedColor</c> is PrimeUIX <c>highlight.color</c>.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGTransferListViewHighlightTokens : IControlTokenAlgorithm<TransferListViewToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, TransferListViewToken token)
    {
        token.ItemSelectedColor = PrimeNGPresets.Rules(context, effectiveGlobal).HighlightColor(effectiveGlobal);
    }
}

/// <summary>
/// TreeView under the PrimeNG presets: the selected item's text <c>NodeSelectedColor</c> is PrimeUIX <c>highlight.color</c>.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGTreeViewHighlightTokens : IControlTokenAlgorithm<TreeViewToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, TreeViewToken token)
    {
        token.NodeSelectedColor = PrimeNGPresets.Rules(context, effectiveGlobal).HighlightColor(effectiveGlobal);
    }
}

/// <summary>
/// TransferTreeView under the PrimeNG presets: the selected item's text <c>NodeSelectedColor</c> is PrimeUIX <c>highlight.color</c>.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGTransferTreeViewHighlightTokens : IControlTokenAlgorithm<TransferTreeViewToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, TransferTreeViewToken token)
    {
        token.NodeSelectedColor = PrimeNGPresets.Rules(context, effectiveGlobal).HighlightColor(effectiveGlobal);
    }
}

/// <summary>
/// TreeSelect under the PrimeNG presets: the selected item's text <c>NodeSelectedColor</c> is PrimeUIX <c>highlight.color</c>.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGTreeSelectHighlightTokens : IControlTokenAlgorithm<TreeSelectToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, TreeSelectToken token)
    {
        token.NodeSelectedColor = PrimeNGPresets.Rules(context, effectiveGlobal).HighlightColor(effectiveGlobal);
    }
}
