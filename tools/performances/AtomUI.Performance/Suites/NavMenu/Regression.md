# NavMenu Regression Matrix

## Functional matrix

- [ ] Pointer press has no selection, command, or routed-event side effects; a same-item primary-button release commits once.
- [ ] Press-and-hold changes only the background to the exact selected background with the 300ms CSS-ease-equivalent transition while text color and logical selection remain unchanged; no hover/transparent/default background frame appears during the release handoff, and selection commits only on release.
- [ ] Override `Controls[NavMenu].MotionDurationSlow` and verify Base/Inline/Horizontal item header background transitions follow it.
- [ ] Pointer capture stays on the hand-cursor item header for the full hold gesture.
- [ ] Pointer capture loss cancels only when it belongs to the transaction pointer.
- [ ] Pointer-hold and keyboard-active have independent owners and project through separate hover/active visual channels without overwriting each other.
- [ ] Leaf activation order is selected state -> `SelectedItem` -> `NavMenuNodeSelected` -> command -> `NavMenuItemClick`.
- [ ] A synchronous `SelectedItem` redirect suppresses the superseded node's stale selected event and remaining invocation.
- [ ] A redirect from `NavMenuNodeSelected` suppresses the superseded node's command and `NavMenuItemClick`.
- [ ] Inline parent activation toggles the submenu; Default parent activation opens the popup; press alone does neither.
- [ ] Enter/Space leaf activation uses the same ordered commit as pointer release; Default parent Enter retains submenu-navigation behavior.

## Gallery ShowCase scripts

- [ ] NavMenu Gallery: press and hold a leaf -> routed content remains unchanged -> release on the same leaf -> content changes once.
- [ ] NavMenu Gallery: press a leaf -> drag outside -> release -> selection and routed content remain unchanged.
- [ ] NavMenu Gallery: navigate with arrow keys while a pointer transaction is active -> keyboard-active and pointer-hold visuals remain independently correct.

## Lifecycle matrix

- [ ] Replace the interaction handler by changing mode during a pointer transaction -> capture and pointer-hold state are released without activation.
- [ ] Remove or recycle the pressed container -> transaction target, capture-lost subscription, and pointer-hold state are released.
- [ ] Detach and reattach the menu -> no stale pointer or keyboard active state and no duplicate routed-event subscriptions.
