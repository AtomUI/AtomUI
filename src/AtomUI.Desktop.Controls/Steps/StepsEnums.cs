namespace AtomUI.Desktop.Controls;

public enum StepsStatus
{
    Wait,
    Process,
    Finish,
    Error
}

public enum StepsType
{
    Default,
    Dot,
    OutlineDot,
    Navigation,
    Inline,
    Panel
}

public enum StepsPanelVariant
{
    Filled,
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
