namespace AtomUI.Desktop.Controls;

/// <summary>
/// Visual form of the steps of a <see cref="Steps" /> control, chosen with <see cref="Steps.Type" />.
/// </summary>
public enum StepsType
{
    /// <summary>
    /// Standard step: an indicator with the step number or the custom icon before or above the title, and a
    /// connector line to the next step.
    /// </summary>
    Default,
    /// <summary>
    /// Step with a small solid dot in place of the numbered indicator.
    /// </summary>
    Dot,
    /// <summary>
    /// Step with a hollow dot, drawn as an outline, in place of the numbered indicator.
    /// </summary>
    OutlineDot,
    /// <summary>
    /// Navigation step: an arrow replaces the connector to the next step, and a bar marks the current step.
    /// </summary>
    Navigation,
    /// <summary>
    /// Compact step with a small dot indicator and a small title. The description is not shown in the step but
    /// in a tooltip.
    /// </summary>
    Inline,
    /// <summary>
    /// Steps as adjoining arrow-shaped panels: the panel itself shows the status, without a separate
    /// indicator or connector. <see cref="Steps.PanelVariant" /> selects their fill style.
    /// </summary>
    Panel
}

/// <summary>
/// Fill style of the step panels in the <see cref="StepsType.Panel" /> form, chosen with
/// <see cref="Steps.PanelVariant" />.
/// </summary>
public enum StepsPanelVariant
{
    /// <summary>
    /// Each panel is filled with the color of its status; the current panel uses the active status colors.
    /// </summary>
    Filled,
    /// <summary>
    /// Each panel keeps the container background and is outlined in the color of its status; the current panel
    /// has a tinted background.
    /// </summary>
    Outlined
}

internal enum StepsItemLayoutRole
{
    ItemWrapper,
    Indicator,
    Section,
    Header,
    SubHeader,
    Connector,
    Content,
    NavigationArrow,
    PanelArrow,
    NavigationActiveIndicator
}
