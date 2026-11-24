## AtomUI

AtomUI is an Ant Design 6 component system for Avalonia/.NET desktop applications. It brings Ant Design's enterprise
interaction patterns, visual language, design tokens and theme customization model to native cross-platform apps on
Windows, macOS and Linux.

### Highlights

- Ant Design 6 style desktop controls for layout, navigation, data entry, feedback and data display.
- Token-driven themes built on Avalonia styles, resources and design-token generation.
- Native desktop integration for window chrome, popups, overlays and platform-specific interaction details.
- A production Gallery application for exploring controls, examples and documentation.
- Source generators for theme tokens, localization and control infrastructure.

### Package Areas

- `AtomUI.Desktop.Controls`: the main desktop control library.
- `AtomUI.Desktop.Controls.DataGrid`: advanced table, filtering, sorting and row-detail scenarios.
- `AtomUI.Desktop.Controls.ColorPicker`: desktop color picking controls and supporting primitives.
- `AtomUI.Desktop.Controls.Extras`: stable supplemental controls beyond the Ant Design core set.
- `AtomUI.Icons.AntDesign`: Ant Design icon assets for AtomUI controls and applications.

### Design languages

Enable a design language and register its value packages for the controls used by your application. For desktop
Ant Design, install `AtomUI.Desktop.Controls.Themes.AntDesign` and configure
`UseAntDesign(language => language.AddDesktopControls())` alongside `UseDesktopControls()`.
ColorPicker and DataGrid each have an additional Ant Design value package.
Shadcn and PrimeNG follow the same package structure.

[Migration guide for 6.2.15 users](https://github.com/AtomUI/AtomUI/blob/v6.3.0/docs/releases/6.3.0-api-changes.md)

### Requirements

- .NET 10.
- Avalonia 12.1.x runtime.
- Windows, macOS or Linux desktop applications.

### Links

- Repository: https://github.com/AtomUI/AtomUI
- Issues: https://github.com/AtomUI/AtomUI/issues
- Samples: https://github.com/AtomUI/AtomUI.Samples

### License

Projects using AtomUI OSS must comply with LGPL v3. Commercial applications, including internal company software,
personal commercial products and outsourced projects, may use AtomUI for free when linking to the published binaries.
If you customize AtomUI from source code, you must either open source the modified code under the license terms or
purchase a commercial license.
