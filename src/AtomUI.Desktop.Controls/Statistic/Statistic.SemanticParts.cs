using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

// The header block above the value, which hosts the title. It is visible only when Header is not null.
// StatisticHeaderStyle applies to it.
[SemanticPart(
    "header",
    SelectorClass = "semantic-header",
    ContractType = typeof(Border),
    Since = "6.2.0")]
// The title: the presenter of Header and HeaderTemplate inside the header block. It is shown only when
// Header is not null.
// StatisticTitleStyle applies to it.
[SemanticPart(
    "title",
    SelectorClass = "semantic-title",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The value row under the title, which lays out the prefix, the value and the suffix side by side. It is
// the content of the loading skeleton.
// StatisticContentStyle applies to it.
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    ContractType = typeof(StackPanel),
    Since = "6.2.0")]
// The value in the middle of the value row. It presents Content, which holds the formatted number unless a
// Content of your own, such as a StatisticCountUp, is set.
// StatisticValueStyle applies to it.
[SemanticPart(
    "value",
    SelectorClass = "semantic-value",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The prefix before the value in the value row. It presents ValuePrefixAddOn and is visible only when
// ValuePrefixAddOn is not null.
// StatisticPrefixStyle applies to it.
[SemanticPart(
    "prefix",
    SelectorClass = "semantic-prefix",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The suffix after the value in the value row. It presents ValueSuffixAddOn and is visible only when
// ValueSuffixAddOn is not null.
// StatisticSuffixStyle applies to it.
[SemanticPart(
    "suffix",
    SelectorClass = "semantic-suffix",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
public partial class Statistic
{
}
