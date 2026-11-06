# Windows package specialist

SelectedRecipe.cs is linked only into generated, on-demand Windows package
consumers. The runner reuses the existing Shared, Resources and ThirdParty
fixtures and normal release package inventory. It executes seven distinct cold
routes: net8 full trim, NativeAOT scanner/no-scanner, net10 full trim/NativeAOT,
and net10 full trim/NativeAOT consuming separately built net8-only packages.

The selected Complete recipe deliberately avoids exporting the entire catalog;
otherwise the validation helper itself roots unused controls. It checks JSON,
actual recipe application and retained input/selection in the native window.
Each route must emit the exact ordered 35-check contract and its single approved
runtime marker; a matching count alone cannot satisfy the protocol.
The managed legacy bridge also restores an actual unsupported runtime pack in
separate outputs and must reject it before producing a delivery receipt or EXE.
Final managed metadata/native maps and immutable output proofs independently
check removal and consumption.

This is a specialist protocol, not an additional global xUnit suite. Independent
value is compiler/SDK/package delivery that unit tests cannot establish.
Cost is tens of minutes, dominated by normal packaging and seven publishes.
All feeds, caches, generated projects and binaries belong to the runner.
