# AtomUI.Controls.Themes.PrimeNG

Status: implemented and validated locally; not published to NuGet.

The PrimeNG values of the controls shared by every idiom (`AtomUI.Controls`). The idiom value packages add them (`AddDesktopControls()` calls `AddCommonControls()`); applications do not reference this package directly.

```csharp
language.AddCommonControls()
```

## Target levels

Spec 11.1: **A** (对标) — PrimeUIX has the component; its values follow the `@primeuix/themes` 3.1.1 standard preset and
are accepted visually against primeng.dev (PrimeNG 22.1.5, 16px root). **B** (协调) — PrimeUIX has no such component;
its values are composed from the PrimeNG global and language tokens and its key states are reviewed by hand.

| Control token | PrimeUIX 3.1.1 component | Level | Values |
|---|---|---|---|
| `AvatarToken` | `avatar` | A | Ant Design formulas over the PrimeNG global tokens, then the preset control rules (`PrimeNGAvatarTokens`) |
| `IconToken` | none | B | Ant Design formulas over the PrimeNG global tokens |

## Unsupported

The PrimeNG mapping tables, the AtomUI deviations and the unsupported list are in the
[language package README](../AtomUI.DesignLanguages.PrimeNG/README.md#unsupported-and-known-differences).
