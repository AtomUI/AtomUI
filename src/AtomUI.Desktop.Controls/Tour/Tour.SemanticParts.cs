using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Shapes;
using AvaloniaButton = Avalonia.Controls.Button;
using AvaloniaBorder = Avalonia.Controls.Border;

namespace AtomUI.Desktop.Controls;

// The surface of the tour popup: the arrow-decorated box that holds the steps view and points at the target of
// the current step. It is shown only while the tour is open, in the separate visual root of the popup.
// TourPopupRootStyle applies to the box.
// Every part of Tour is declared as cross visual root: the popup content lives in that popup root, and the mask
// is the TourLayer shared through the visual layer manager of the TopLevel. The markers of cover, close, header,
// title, description, footer, actions, indicators and indicator are carried by the templates of TourStep and
// TourStepsView and by the dots that DefaultTourIndicator creates in code, so those routes leave the template of
// Tour, and CrossNestedOwners lets the marker validation look for them there.
[SemanticPart(
    "popup.root",
    SelectorClass = "semantic-popup-root",
    SelectorRoute = "/template/ .semantic-popup-root",
    CrossVisualRoot = true,
    ContractType = typeof(ArrowDecoratedBox),
    Since = "6.2.0")]
// The mask that dims the page and leaves the region around the target clear. It is the TourLayer shared by the
// tours of a TopLevel, created at runtime outside every template; the marker class is added in code. The route
// is a plain descendant selector: the open Tour sets itself as the logical parent of the layer, so the marker is
// matched down the logical tree. The layer is not part of the visual tree of the Tour.
// TourPopupMaskStyle applies to the layer.
[SemanticPart(
    "popup.mask",
    SelectorClass = "semantic-popup-mask",
    SelectorRoute = ">> .semantic-popup-mask",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Optional,
    ContractType = typeof(Control),
    Since = "6.2.0")]
// The content frame of the popup surface: the border in the template of the arrow-decorated box that paints the
// background, border and corner radius of the popup and holds the steps view. The marker belongs to the template
// of that box, so the route crosses into its nested template. TourPopupSectionStyle applies to the frame.
[SemanticPart(
    "popup.section",
    SelectorClass = "semantic-container",
    SelectorRoute = "/template/ .semantic-popup-root /template/ .semantic-container",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(AvaloniaBorder),
    Since = "6.2.0")]
// The cover content of a step, shown below the header and above the description. The marker is in the template
// of TourStep, so there is one presenter per step and only the one of the active step is visible; it presents
// nothing when the step has no cover. TourPopupCoverStyle applies to the presenter.
[SemanticPart(
    "popup.cover",
    SelectorClass = "semantic-popup-cover",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-cover",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Optional,
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The close button at the right end of the header of a step. The marker is in the template of TourStep, so there
// is one button per step and only the one of the active step is visible. TourPopupCloseStyle applies to the
// button.
[SemanticPart(
    "popup.close",
    SelectorClass = "semantic-popup-close",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-close",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(AvaloniaButton),
    Since = "6.2.0")]
// The header bar of a step, docked at the top; it holds the title and the close button. The marker is in the
// template of TourStep, so there is one bar per step and only the one of the active step is visible.
// TourPopupHeaderStyle applies to the bar.
[SemanticPart(
    "popup.header",
    SelectorClass = "semantic-popup-header",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-header",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(AvaloniaBorder),
    Since = "6.2.0")]
// The title of a step, shown in the header next to the close button. The marker is in the template of TourStep,
// so there is one presenter per step and only the one of the active step is visible; it presents nothing when
// the step has no title. TourPopupTitleStyle applies to the presenter.
[SemanticPart(
    "popup.title",
    SelectorClass = "semantic-popup-title",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-title",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Optional,
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The description of a step, shown below the cover. The marker is in the template of TourStep, so there is one
// presenter per step and only the one of the active step is visible; it presents nothing when the step has no
// description. TourPopupDescriptionStyle applies to the presenter.
[SemanticPart(
    "popup.description",
    SelectorClass = "semantic-popup-description",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-description",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Optional,
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The footer bar of the tour popup, docked at the bottom of the steps view; it holds the step indicator and the
// action buttons. The marker is in the template of TourStepsView. TourPopupFooterStyle applies to the bar.
[SemanticPart(
    "popup.footer",
    SelectorClass = "semantic-popup-footer",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-footer",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(AvaloniaBorder),
    Since = "6.2.0")]
// The row of action buttons at the right end of the footer: the previous, next and finish buttons, preceded by
// the custom actions of the tour. The marker is in the template of TourStepsView.
// TourPopupActionsStyle applies to the row, not to the individual buttons.
[SemanticPart(
    "popup.actions",
    SelectorClass = "semantic-popup-actions",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-actions",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(StackPanel),
    Since = "6.2.0")]
// The step indicator at the left end of the footer. The marker is in the template of TourStepsView; the presenter
// shows the control set as Indicator of the tour and presents nothing when no indicator is set.
// TourPopupIndicatorsStyle applies to the presenter, not to the indicator control.
[SemanticPart(
    "popup.indicators",
    SelectorClass = "semantic-popup-indicators",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-indicators",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Optional,
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// One dot of the default step indicator. The dots are created in code, one per step, so the part repeats once per
// step; the dot of the active step also carries the class active. TourPopupIndicatorStyle applies to every dot.
[SemanticPart(
    "popup.indicator",
    SelectorClass = "semantic-popup-indicator",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-indicator",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Multiple,
    ContractType = typeof(Ellipse),
    Since = "6.2.0")]
public partial class Tour
{
}
