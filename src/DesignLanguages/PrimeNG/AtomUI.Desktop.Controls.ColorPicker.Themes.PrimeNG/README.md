# AtomUI.Desktop.Controls.ColorPicker.Themes.PrimeNG

Status: implemented and validated locally; not published to NuGet.

The PrimeNG values of the desktop colour picker (`AtomUI.Desktop.Controls.ColorPicker`).

```csharp
builder.UseDesktopColorPicker();
builder.UsePrimeNG(language => language.AddDesktopColorPicker());
```

## Target levels

Spec 11.1: **A** (对标) — PrimeUIX has the component; its values follow the `@primeuix/themes` 3.1.1 standard preset and
are accepted visually against primeng.dev (PrimeNG 22.1.5, 16px root). **B** (协调) — PrimeUIX has no such component;
its values are composed from the PrimeNG global and language tokens and its key states are reviewed by hand.

| Control token | PrimeUIX 3.1.1 component | Level | Values |
|---|---|---|---|
| `ColorPickerToken` | `colorpicker` | A | Ant Design formulas over the PrimeNG global tokens |
| `ColorPickerViewToken` | `colorpicker (panel)` | A | Ant Design formulas over the PrimeNG global tokens |
| `GradientColorPickerToken` | none | B | Ant Design formulas over the PrimeNG global tokens |
| `GradientColorPickerViewToken` | none | B | Ant Design formulas over the PrimeNG global tokens |

## Unsupported

The PrimeNG mapping tables, the AtomUI deviations and the unsupported list are in the
[language package README](../AtomUI.DesignLanguages.PrimeNG/README.md#unsupported-and-known-differences).
