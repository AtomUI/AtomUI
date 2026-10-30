# AtomUI Framework – GitHub Copilot Instructions

**Always reuse existing code - no redundancy!**

> **Scope**: AtomUI repository — for developing AtomUI itself, a cross-platform UI control library that faithfully reproduces all Ant Design 5.0 components on .NET / Avalonia. The architecture is designed for multi-platform delivery: Desktop (Windows/macOS/Linux) today, Mobile (iOS/Android) in the future.
>
> **Goal**: Enforce AtomUI architecture best practices (layered control library, platform-agnostic base controls, Design Token system, theme styling), maintain backward compatibility, ensure extensibility, and strictly follow Ant Design 5.0 design language.

---

## Global Defaults

- Follow existing patterns in this repository first. Before generating new code, search for similar implementations and mirror their structure, naming, and conventions.
- Before creating any utility, helper, or converter, search the codebase for existing implementations.
- Prefer minimal, focused diffs. Avoid drive-by refactors and formatting churn.
- Preserve public APIs. Avoid breaking changes unless explicitly requested and justified.
- Keep layers clean. Do not introduce forbidden dependencies between packages.
- All user-facing strings must be localized through the `LanguageProvider` system; no hardcoded English text in controls.

---

## Tech Stack

- **Runtime**: .NET 10 for development and production via `$(AtomUITargetFrameworks)`
- **UI Framework**: Avalonia v11 (currently 11.3.x)
- **Language**: C# (latest version, nullable enabled, implicit usings)
- **Reactive**: ReactiveUI.Avalonia + System.Reactive
- **Icons**: Ant Design icon set (auto-generated via `AtomUI.Icons.AntDesign.Generator`)
- **Fonts**: Alibaba Sans (bundled via `AtomUI.Fonts.AlibabaSans`)
- **Code Generation**: Custom Roslyn Source Generators (`AtomUI.Generator`)
- **Package Management**: Central Package Management (`Directory.Packages.props`)
- **Build**: MSBuild with shared props (`build/` directory)
- **Tests**: xUnit v3, NSubstitute, Shouldly
- **Design Language**: Ant Design 5.0 specification

---

## Anti-Redundancy Rules

- If a utility, converter, or helper already exists in `AtomUI.Core`, `AtomUI.Controls.Shared`, or `AtomUI.Controls`, import it — do NOT create a duplicate.
- If a shared interface (e.g., `ISizeTypeAware`, `IInputControlStatusAware`, `IWaveSpiritAwareControl`) already exists, implement it — do NOT define a new one.
- Reuse existing `StyledProperty` definitions from shared abstract property classes (e.g., `SizeTypeControlProperty.SizeTypeProperty.AddOwner<T>()`).
- Before creating any pseudo-class string, check `StdPseudoClass` constants first.
- Token values must be derived from `SharedToken` (the global `DesignToken`) whenever possible; do NOT hardcode colors, sizes, or spacing.

---

## Project Architecture (Layering)

The architecture is designed around a **three-tier cross-platform control model**:

1. **Foundation Layer** — Platform-independent infrastructure (`AtomUI.Core`, `AtomUI.Controls.Shared`).
2. **Base Control Layer** — Device-agnostic abstract controls (`AtomUI.Controls`) that define shared behavior, properties, pseudo-classes, and logic. These serve as base classes for all platform-specific control implementations.
3. **Platform Control Layer** — Platform-specific concrete controls that inherit from the base layer:
   - `AtomUI.Desktop.Controls` → Desktop (Windows/macOS/Linux) — **current focus**
   - `AtomUI.Mobile.Controls` → Mobile (iOS/Android) — **planned future**

This layering ensures maximum code reuse: shared behavior lives in `AtomUI.Controls`, while platform-specific theming, interaction patterns, and native integration live in the platform layer.

```
┌─────────────────────────────────────────────────────────────────────┐
│  Platform Control Layer (platform-specific concrete controls)       │
│  ┌──────────────────────────┐  ┌──────────────────────────┐        │
│  │ AtomUI.Desktop.Controls  │  │ AtomUI.Mobile.Controls   │        │
│  │ (Desktop: Win/Mac/Linux) │  │ (Mobile: iOS/Android)    │        │
│  │ + .ColorPicker, .DataGrid│  │ (planned future)         │        │
│  └────────────┬─────────────┘  └────────────┬─────────────┘        │
├───────────────┼─────────────────────────────┼──────────────────────┤
│  Base Control Layer (device-agnostic abstract/base controls)       │
│  ┌────────────┴─────────────────────────────┴─────────────┐        │
│  │                   AtomUI.Controls                       │        │
│  │  AbstractTag, AbstractAvatar, AbstractIconButton, etc.  │        │
│  └────────────┬────────────────────────────────────────────┘        │
├───────────────┼────────────────────────────────────────────────────┤
│  Foundation Layer (core infrastructure, no UI controls)             │
│  ┌────────────┴────────────┐  ┌────────────────────────┐           │
│  │      AtomUI.Core        │  │  AtomUI.Controls.Shared │           │
│  │  Theme, Tokens, Motion, │  │  Interfaces, Enums,     │           │
│  │  Animations, Utilities  │  │  Converters, WaveSpirit  │          │
│  └────────────┬────────────┘  └────────────────────────┘           │
│  ┌────────────┴────────────┐                                        │
│  │     AtomUI.Native       │                                        │
│  │  Platform native interop│                                        │
│  └─────────────────────────┘                                        │
├─────────────────────────────────────────────────────────────────────┤
│  Shared Resources                                                   │
│  AtomUI.Icons.Shared → AtomUI.Icons.AntDesign                      │
│  AtomUI.Fonts.AlibabaSans                                           │
│  AtomUI.Generator (Roslyn Source Generators, netstandard2.0)        │
└─────────────────────────────────────────────────────────────────────┘
```

### Project Descriptions

| Project | Role |
|---|---|
| `AtomUI.Native` | Platform-specific native interop |
| `AtomUI.Core` | Core infrastructure: Theme system, Token system, Animations, MotionScene, Utilities |
| `AtomUI.Controls.Shared` | Shared interfaces, enums, converters, WaveSpirit, MediaQuery — **no UI controls**; the most fundamental building blocks for control development |
| `AtomUI.Controls` | **Device-agnostic base controls** (e.g., `AbstractTag`, `AbstractAvatar`, `AbstractIconButton`). Defines shared behavior, properties, and pseudo-classes that Desktop and future Mobile controls inherit from |
| `AtomUI.Desktop.Controls` | **Desktop-specific concrete controls** (e.g., `Tag`, `Avatar`, `Button`). Contains all Ant Design 5.0 component implementations for Desktop platforms |
| `AtomUI.Desktop.Controls.ColorPicker` | Desktop ColorPicker extension |
| `AtomUI.Desktop.Controls.DataGrid` | Desktop DataGrid extension |
| `AtomUI.Icons.Shared` | Icon infrastructure interfaces |
| `AtomUI.Icons.AntDesign` | Auto-generated Ant Design icon set |
| `AtomUI.Fonts.AlibabaSans` | Bundled font resources |
| `AtomUI.Generator` | Roslyn Source Generators (TokenResourceKey, etc.) — targets `netstandard2.0`, consumed as Analyzer only |

### Dependency Direction

```
AtomUI.Desktop.Controls.* → AtomUI.Desktop.Controls ─┐
(future) AtomUI.Mobile.Controls ─────────────────────┤
                                                      ├→ AtomUI.Controls → AtomUI.Core → AtomUI.Native
                                                      │  AtomUI.Controls → AtomUI.Controls.Shared → AtomUI.Core
                                                      │  AtomUI.Controls → AtomUI.Icons.AntDesign → AtomUI.Icons.Shared
                                                      │  AtomUI.Controls → AtomUI.Fonts.AlibabaSans
```

### Dependency Rules (STRICT)

- `AtomUI.Core` MUST NOT depend on `AtomUI.Controls`, `AtomUI.Desktop.Controls`, or any platform control package.
- `AtomUI.Controls.Shared` MUST NOT depend on `AtomUI.Controls` or any platform control package.
- `AtomUI.Controls` MUST NOT depend on `AtomUI.Desktop.Controls` or any platform control package.
- `AtomUI.Desktop.Controls` MUST NOT depend on `AtomUI.Mobile.Controls` (and vice versa). Platform layers are **sibling peers**, never cross-dependent.
- `AtomUI.Generator` targets `netstandard2.0` and is consumed as an Analyzer only.

### InternalsVisibleTo

- `AtomUI.Core` exposes internals to: `AtomUI.Controls`, `AtomUI.Controls.Shared`, `AtomUI.Desktop.Controls`, `AtomUI.Desktop.Controls.DataGrid`, `AtomUI.Desktop.Controls.ColorPicker`.
- `AtomUI.Controls` exposes internals to: `AtomUI.Desktop.Controls`, `AtomUI.Desktop.Controls.DataGrid`, `AtomUI.Desktop.Controls.ColorPicker`.
- `AtomUI.Desktop.Controls` exposes internals to: `AtomUI.Desktop.Controls.DataGrid`, `AtomUI.Desktop.Controls.ColorPicker`.

---

## Control Structure Pattern

### Cross-Platform Control Inheritance Model

Controls follow a two-level inheritance pattern to maximize code reuse across platforms:

```
AtomUI.Controls (base layer, device-agnostic)     AtomUI.Desktop.Controls (platform layer)
─────────────────────────────────────────────      ──────────────────────────────────────────
AbstractTag (behavior, properties, logic)    ───►  Tag : AbstractTag (+ desktop token scope)
AbstractAvatar (behavior, properties, logic) ───►  Avatar : AbstractAvatar (+ desktop token scope)
AbstractIconButton (behavior, properties)    ───►  IconButton : AbstractIconButton (+ desktop token scope)
```

**Rules for deciding where code belongs:**

| Code belongs in… | When… |
|---|---|
| `AtomUI.Controls` (Abstract base) | Shared behavior, properties, pseudo-classes, converters, and logic that are identical across Desktop and Mobile |
| `AtomUI.Desktop.Controls` (Concrete) | Desktop-specific token registration, desktop-specific themes, desktop-only interaction patterns, or controls that have no mobile counterpart |

When creating a new control, always ask: *"Would a future Mobile version of this control share this behavior?"* If yes → put it in the base class in `AtomUI.Controls`.

### Standard Control Folder Structure

AtomUI controls are split across two layers. The pattern below is the **universal standard** for all platform control projects (`AtomUI.Desktop.Controls` today, `AtomUI.Mobile.Controls` in the future).

#### Platform Control Layer (`AtomUI.Desktop.Controls`, etc.)

Each platform-specific control follows this folder structure:

```
ControlName/
├── ControlName.cs            # Concrete control class (public, inherits from Abstract base in AtomUI.Controls)
├── ControlNameToken.cs       # Platform-specific Design Token definition (internal, [ControlDesignToken])
├── ControlNamePseudoClass.cs # Platform-specific pseudo-class constants (if needed)
├── Themes/
│   ├── ControlNameTheme.axaml    # XAML ControlTheme definition
│   ├── ControlNameTheme.cs       # Code-behind (extends ControlTheme)
│   └── ControlNameThemes.axaml   # ResourceDictionary registering theme by x:Key="{x:Type}"
```

#### Base Control Layer (`AtomUI.Controls`)

Each device-agnostic base control follows this folder structure:

```
ControlName/
├── AbstractControlName.cs        # Abstract base class (behavior, properties, shared logic)
├── ControlNameEnums.cs           # Shared enums (if any)
├── ControlNamePseudoClass.cs     # Shared pseudo-class constants (cross-platform)
```

**Key rule**: Base controls in `AtomUI.Controls` do NOT define tokens or themes — those are platform-specific and belong in the platform control layer (e.g., `AtomUI.Desktop.Controls`).

### Control Class Conventions

- **Namespaces**:
  - `AtomUI.Controls` or `AtomUI.Controls.Commons` — base abstract controls.
  - `AtomUI.Desktop.Controls` — desktop concrete controls.
  - `AtomUI.Desktop.Controls.Themes` — theme code-behind.
  - *(future)* `AtomUI.Mobile.Controls` — mobile concrete controls.
- **Inheritance**: Desktop concrete controls MUST inherit from their abstract base when one exists (e.g., `Tag : AbstractTag`, `Avatar : AbstractAvatar`). The concrete constructor registers the token scope:
  ```csharp
  public class Tag : AbstractTag
  {
      public Tag()
      {
          this.RegisterTokenResourceScope(TagToken.ScopeProvider);
      }
  }
  ```
- Public `StyledProperty<T>` fields follow Avalonia conventions: `public static readonly StyledProperty<T> XxxProperty`.
- For shared properties, use `AddOwner<T>()` pattern (e.g., `SizeTypeControlProperty.SizeTypeProperty.AddOwner<Button>()`).
- Decorate controls with `[PseudoClasses(...)]` attribute listing all custom pseudo-classes.
- Implement relevant shared interfaces: `ISizeTypeAware`, `IWaveSpiritAwareControl`, `ICompactSpaceAware`, `IFormItemAware`, `IMotionAwareControl`, etc.
- When a control is a Desktop specialization of a base control, inherit from the base (e.g., `Tag : AbstractTag`).

### Control Aliasing

When wrapping an Avalonia built-in control, use a `using` alias to avoid name conflicts:

```csharp
using AvaloniaButton = Avalonia.Controls.Button;

public class Button : AvaloniaButton, ISizeTypeAware { ... }
```

---

## Design Token System

The Design Token system is a **faithful C# implementation** of the [Ant Design 5.0 token architecture](https://ant.design/docs/react/customize-theme). All theme infrastructure lives in `src/AtomUI.Core/Theme/`.

### Token Lifecycle (Three-Layer Derivation)

Ant Design 5.0 decomposes Design Tokens into three layers with a strict derivation chain:

```
Seed Token  ──(Algorithm)──►  Map Token  ──(Alias calculation)──►  Alias Token
  (origin)                    (gradient)                           (semantic)
```

1. **Seed Token** — The origin of all design intent. Changing a Seed Token (e.g., `ColorPrimary`) triggers the algorithm to automatically derive a full set of gradient tokens.
   - Defined in `DesignToken.Seed.cs`, marked with `[DesignTokenKind(DesignTokenKind.Seed)]`.
   - Examples: `ColorPrimary`, `ColorSuccess`, `ColorWarning`, `ColorError`, `ColorInfo`, `FontSize`, `BorderRadius`, `SizeUnit`, `SizeStep`, `ControlHeight`, `LineWidth`, `EnableMotion`.

2. **Map Token** — Gradient variables derived from Seed Tokens by theme algorithms. These form the systematic color palette, size ladder, and font scale.
   - Defined in `DesignToken.ColorPrimaryMap.cs`, `DesignToken.ColorMap.cs`, `DesignToken.FontMap.cs`, `DesignToken.SizeMap.cs`, `DesignToken.HeightMap.cs`, `DesignToken.StyleMap.cs`, etc.
   - Marked with `[DesignTokenKind(DesignTokenKind.Map)]`.
   - Examples: `ColorPrimaryBg` (1号色), `ColorPrimaryBgHover` (2号色), `ColorPrimaryBorder` (3号色), `ColorPrimaryHover` (5号色), `ColorPrimaryActive` (7号色), `FontSizeSM`, `FontSizeLG`, `SizeSM`, `SizeLG`, `ControlHeightSM`, `ControlHeightLG`.

3. **Alias Token** — Semantic tokens used to control common component styles in batches. Essentially Map Token aliases or specially processed Map Tokens.
   - Defined in `DesignToken.Alias.cs`, marked with `[DesignTokenKind(DesignTokenKind.Alias)]`.
   - Computed in `DesignToken.CalculateAliasTokenValues()`.
   - Examples: `ColorTextPlaceholder`, `ColorTextDisabled`, `ColorBgContainerDisabled`, `ColorSplit`, `PaddingContentHorizontal`, `ControlItemBgHover`.

4. **Component Token** (`AbstractControlDesignToken` subclasses) — Per-component tokens scoped by component ID. These derive all values from the global `SharedToken` (the `DesignToken` instance) and can override individual shared tokens per component.

### Theme Algorithms

Algorithms expand Seed Tokens into Map Tokens (color palette generation, size ladder calculation, etc.):

| Algorithm | Class | Purpose |
|---|---|---|
| `ThemeAlgorithm.Default` | `DefaultThemeVariantCalculator` | Light theme (default) |
| `ThemeAlgorithm.Dark` | `DarkThemeVariantCalculator` | Dark theme |
| `ThemeAlgorithm.Compact` | `CompactThemeVariantCalculator` | Compact layout (smaller sizes/spacing) |

Algorithms can be **composed** — e.g., Dark + Compact produces a dark compact theme. The `ThemeDefinition` stores a set of `ThemeAlgorithm` values.

### Color Palette Generation

Each Seed color (e.g., `ColorPrimary = #1677ff`) is expanded into a 10-color gradient palette via `PaletteGenerator.GeneratePalette()`. The palette maps to numbered positions (1号色 through 10号色) used across the Map Token layer.

### Component Token Conventions

```csharp
[ControlDesignToken]
internal class ButtonToken : AbstractControlDesignToken
{
    public const string ID = "Button";
    public static readonly ControlTokenResourceScopeProvider ScopeProvider = new(ID);

    /// <summary>
    /// 主要按钮文本颜色
    /// </summary>
    public Color PrimaryColor { get; set; }

    /// <summary>
    /// 按钮内间距
    /// </summary>
    public Thickness Padding { get; set; }

    public ButtonToken() : base(ID) { }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        // Derive values from SharedToken — NEVER hardcode
        PrimaryColor = SharedToken.ColorTextLightSolid;
        Padding = new Thickness(SharedToken.PaddingContentHorizontal - SharedToken.LineWidth, ...);
    }

    protected override Type GetTokenKindType() => typeof(ButtonTokenKind);
}
```

**Rules:**
- Token class MUST be `internal` and decorated with `[ControlDesignToken]`.
- Token class MUST have a `const string ID` and a `static readonly ControlTokenResourceScopeProvider ScopeProvider`.
- Constructor calls `base(ID)`.
- Override `CalculateTokenValues(bool isDarkMode)` to compute token values from `SharedToken`.
- All color, spacing, size, and font values MUST be derived from `SharedToken` properties — do NOT hardcode magic numbers.
- Token properties SHOULD have XML doc comments describing their purpose.
- Override `GetTokenKindType()` to return the generated `TokenKind` enum type.

### Token Resource Access in AXAML

- Shared (global) tokens: `{atom:SharedTokenResource ColorPrimary}`
- Control-scoped tokens: `{atom:TokenResource ButtonPrimaryColor}` (auto-generated resource keys)

---

## Theme & Styling

### ControlTheme (AXAML)

```xml
<ControlTheme xmlns="https://github.com/avaloniaui"
              xmlns:atom="https://atomui.net"
              xmlns:antdicons="https://atomui.net/icons/antdesign"
              x:Class="AtomUI.Desktop.Controls.Themes.ButtonTheme"
              TargetType="atom:Button">
    <!-- Template and style selectors -->
</ControlTheme>
```

**Conventions:**
- Use `atom:` XML namespace prefix for AtomUI controls.
- Use `antdicons:` for Ant Design icons.
- Reference template parts via `x:Static` constants from `ThemeConstants` classes.
- Use `{atom:SharedTokenResource ...}` and `{atom:TokenResource ...}` markup extensions to bind token values.
- NEVER hardcode colors, sizes, or brushes in AXAML — always use token resources.
- Use `^:is(atom|ControlName)` selector syntax for style inheritance.

### Theme Registration

Each control package provides a `ControlThemesProvider` (e.g., `DesktopControlThemesProvider`) and a corresponding `ThemeManagerBuilderExtensions` with `Use*Controls()` extension method:

```csharp
public static IThemeManagerBuilder UseDesktopControls(this IThemeManagerBuilder builder) { ... }
```

### Theme Variants

- **Default** (light), **Dark**, **Compact** — implemented as `IThemeVariantCalculator`.
- Dark mode is handled via `isDarkMode` parameter in `CalculateTokenValues`.

---

## Pseudo-Classes

- Reuse standard pseudo-classes from `StdPseudoClass` (`:disabled`, `:focus`, `:pointerover`, `:pressed`, `:checked`, etc.).
- For control-specific pseudo-classes, define a static class `ControlNamePseudoClass` with `public const string` fields.
- Always use `[PseudoClasses(...)]` attribute on the control class.

---

## Motion & Animation

- Predefined motions in `AtomUI.Core/MotionScene/`: `FadeMotions`, `SlideMotions`, `ZoomMotions`, `CollapseMotions`, `MoveMotions`.
- Use `IMotionActor` / `BaseMotionActor` for animation orchestration.
- Controls support `IsMotionEnabled` property via `IMotionAwareControl` interface.
- WaveSpirit click-ripple effect via `IWaveSpiritAwareControl` and `WaveSpiritDecorator`.

---

## Localization

- Define language resources per package using `[LanguageProvider(LanguageCode.xx_XX, LangId)]` attribute.
- Language resource classes contain `public const string` fields.
- Override `GetResourceKindType()` to return the language resource kind enum.
- Currently supported: `en_US`, `zh_CN`.
- All user-visible text must go through the localization system.

---

## Icons

- Ant Design icons are auto-generated into `AtomUI.Icons.AntDesign/GeneratedIcons/`.
- Use them in AXAML: `<antdicons:LoadingOutlined />`, `<antdicons:CloseOutlined />`, etc.
- Use `PathIcon` for icon properties on controls.
- Do NOT manually create icon geometry — use the generated icon classes.

---

## Source Generators

- `AtomUI.Generator` provides Roslyn source generators (e.g., `TokenResourceKeyGenerator`).
- Generated files output to `GeneratedFiles/` directory (excluded from compilation via `<Compile Remove="...">`).
- Do NOT manually edit files in `GeneratedFiles/` directories.

---

## Shared Interfaces & Properties

### Key Interfaces (in `AtomUI.Controls.Shared`)

| Interface | Purpose |
|---|---|
| `ISizeTypeAware` | Controls with `SizeType` (Small/Middle/Large) |
| `IInputControlStatusAware` | Input controls with validation `Status` |
| `IInputControlStyleVariantAware` | Controls with style variants (Outlined/Filled/Borderless) |
| `IWaveSpiritAwareControl` | Controls with click-ripple wave effect |
| `IFormItemAware` | Controls that participate in Form validation |
| `IMotionAwareControl` | Controls with animation toggle |
| `ICompactSpaceAware` | Controls aware of compact spacing mode |
| `IMediaBreakAware` | Controls aware of responsive breakpoints |
| `ITreeNode` | Tree-structured controls |

### Shared Property Pattern

Shared properties use an abstract holder class + `AddOwner`:

```csharp
// Definition (in Controls.Shared):
public abstract class SizeTypeControlProperty
{
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        AvaloniaProperty.Register<StyledElement, SizeType>("SizeType", SizeType.Middle);
}

// Usage (in control):
public static readonly StyledProperty<SizeType> SizeTypeProperty =
    SizeTypeControlProperty.SizeTypeProperty.AddOwner<Button>();
```

---

## Code Quality & Style

- **Nullable**: Always enabled (`<Nullable>enable</Nullable>`). Treat nullable warnings as errors (`<WarningsAsErrors>Nullable</WarningsAsErrors>`).
- **File-scoped namespaces**: Use `namespace X;` (not block-scoped).
- **XML doc comments**: Required for all public API members and token properties.
- **Naming**: PascalCase for properties, methods, types; `I` prefix for interfaces; `Abstract` prefix for abstract base classes.
- **Constants**: Use `public const string` for pseudo-class names, template part names, token IDs.
- **Avoid `any`/`object`**: Use strongly-typed APIs.
- **Access modifiers**: Token classes are `internal`; control classes are `public`; theme code-behind classes match their visibility needs.
- **Virtual members**: Consider `protected virtual` for extensibility-critical members.

---

## Backward Compatibility

- Do NOT remove or rename public API members without a deprecation cycle.
- Use `[Obsolete("Message. Use X instead.")]` with clear migration guidance before removal.
- Add new optional parameters with defaults; do not change existing method signatures.
- When adding new `StyledProperty` to existing controls, always provide sensible default values.

---

## Build & Development

- **Solution**: `AtomUI.slnx`
- **Build**: `dotnet build` (Debug and Release target `net10.0`)
- **Test**: `dotnet test` (xUnit v3 test projects under `tests/`)
- **Gallery App**: `controlgallery/AtomUIGallery.Desktop/` — run for visual testing of all controls
- **Publish**: `scripts/PublishToLocalSources.ps1`
- Package versions managed centrally in `Directory.Packages.props` and `build/Version.props`.

---

## Control Gallery (ShowCase)

- Located in `controlgallery/AtomUIGallery/ShowCases/`.
- Each showcase demonstrates a specific control's usage.
- When adding a new control, add a corresponding showcase for visual validation.

---

## Summary Checklist for New Controls

1. Create folder under `AtomUI.Desktop.Controls/ControlName/`.
2. Define `ControlNameToken.cs` — inherit `AbstractControlDesignToken`, derive all values from `SharedToken`.
3. Define `ControlName.cs` — implement relevant shared interfaces, register token scope.
4. Create `Themes/ControlNameTheme.axaml` — use token resources, never hardcode values.
5. Create `Themes/ControlNameTheme.cs` — code-behind extending `ControlTheme`.
6. Create `Themes/ControlNameThemes.axaml` — register with `x:Key="{x:Type atom:ControlName}"`.
7. Create `Themes/ControlNameThemeConstants.cs` — template part name constants.
8. Add pseudo-class constants if needed (`ControlNamePseudoClass.cs`).
9. Add localization strings if the control has user-visible text.
10. Add a ShowCase in the gallery app.
11. Verify the control follows Ant Design 5.0 specification.

