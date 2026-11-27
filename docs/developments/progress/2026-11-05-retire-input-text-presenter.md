# Input presenter retirement execution ledger

Plan: [2026-11-05-retire-input-text-presenter.md](../plans/2026-11-05-retire-input-text-presenter.md). Approved source audit is the design; the user explicitly authorized implementation. No additional approval gate.

## Contract and scope

L0 internal cleanup. Public/protected API, Avalonia properties/events, interfaces, PART names, pseudo-classes, token keys and Gallery usage stay unchanged. The internal OtpTextBox PART type widens to its already supported TextPresenter contract. Existing field ownership and detach/retemplate cleanup remain identical. Six template nodes retain every attribute and binding.

Existing feature checkout reused per desktop workspace guidance. Pre-existing menu/popup fixes and unrelated records preserved; baseline diff captured outside the repository. No new subscription, binding, timer, lazy object, cache, visual node, helper or runtime guard is introduced. This is source-proven compatibility retirement; no page loading or frame-time improvement is claimed.

## Validation and disposition

- Before implementation: real compiled AXAML fixture realized six template paths (nine presenters including four OTP cells) in Light and Dark. Saved 144 text-run/metric/caret/pixel snapshots. Positive selected ASCII substring `elec` asserted for red/white foreground, reversed ranges and hidden selection; null brush, password/reveal, mixed script/combining/emoji, preedit and template/detach cycles exercised.
- Red migration gates: native presenter requirement failed 1/1 against the old nine subclass instances; existing three Semantic Part marker tests failed 3/3 against old template node names. These are retirement requirements, not a claim of a previously failing user-visible selection bug.
- Baseline OtpTextBox member audit found four missing internal-property region entries. Added the region without changing field initialization or lifecycle order.
- New validation fixture is temporary under Rendering.Tests/Temporary/RetireInputPresenter and will be deleted with raw snapshots/logs. Existing permanent contract test count remains unchanged.
- Light/Dark fixture declares LineEditInputStyle, SearchEditInputStyle and TextAreaTextareaStyle in AXAML, asserts all three hit before and after rebuilding templates, and verifies detach/reattach presenter count. No code-behind customization fallback.
- Green: 3/3 temporary Skia tests, including exact 144-state comparison and native-type requirement. Fresh source-snapshot counters verify all four non-null-selection-foreground property changes invoke full invalidation twice in the old shim vs once natively. This is callback-count reduction, not proof of twice the shaping work or a time/CPU percentage.
- Existing Semantic Part classes: 19/19 pass, 0 skipped. Actual input verification source compiled through temporary symlink entry: selection output contract passes 1/1. Full performance tool build fails on seven pre-existing errors (missing Program theme/registration imports; removed TabControlScrollViewer/TabStripScrollViewer references), not in changed input verification code. No blanket tool-build success claim.
- OtpTextBox member-layout audit: 1 file, 1 control, 0 remaining findings. Added only an internal-property region.
- All new temporary test sources, symlinks and AXAML fixture removed before final module verification. Permanent test count delta: 0.
- Protected AbstractTextInput.InputTextPresenter name retained; its existing return type is TextPresenter and changing the name would alter the API.

## Final review and rulings

One fresh final read-only review by gpt-6-astra (high), per executing-plans: no Critical / Important / Minor findings. Reviewer checked exact Avalonia tag blobs, six nodes, AXAML generated Styles, OTP ownership, null foreground, tool output assertions, API and AOT type references. No additional review pass.

Final: Ruling: Unrelated performance tool Program/TabControl build repairs remain outside this approved retirement — no causal dependency on presenter removal; current edited input source is compiled and executed independently. Report the whole-tool build failure explicitly.

Final: Ruling: Existing Menu/Popup opacity changes and other clock/platform hooks remain under the preceding audit — no new mutation or certification in this retirement. Three theme diff hash remains ec7f48c5f94919a8a24977f6d510d930b58ca0af26899de5086bac7b9826aa37.

Native IME interaction and visible caret blinking are not certified by headless snapshots. No claim about page loading speed, steady-state frame pacing or wall-clock performance.

## Final cleaned-tree verification

- Linux Gallery NativeAOT publish completed successfully using SDK 10.0.400 and temporary 10.0.14 runtime assembled from the already restored official runtime package; ILC/runtime assets both use 10.0.14. Full Gallery and all six compiled template paths linked. Four existing ReactiveUI / ReactiveUI.Avalonia IL2104/IL3053 summary warnings remain, no new suppression.
- Published x86-64 ELF: 95,204,576 bytes; SHA-256 `11cc8c8b7aa483bc1e82823f7d2d13cfb8c434dd7d0c3128428f4c62a745ae26`. Forced `--windowing-platform=wayland` startup survived 12 seconds, `/proc/<pid>/exe` matched the new exact publish path, no CoreCLR loaded, stdout/stderr empty. Process explicitly terminated. This is native startup evidence, not native IME or all-page interaction acceptance.
- Cleaned-tree focused command: `python3 scripts/verification/test.py run --scope iterate --path src/AtomUI.Desktop.Controls/Input --path src/AtomUI.Desktop.Controls/ComboBox --path src/AtomUI.Desktop.Controls/OtpLineEdit --include-test 'tests/AtomUI.Desktop.Controls.Tests/AutoComplete/*.cs' --include-test 'tests/AtomUI.Desktop.Controls.Tests/Mentions/*.cs' --include-test tests/AtomUIGallery.Tests/ShowCases/LineEditShowCasePageTests.cs --include-test tests/AtomUIGallery.Tests/ShowCases/ComboBoxShowCasePageTests.cs --include-test tests/AtomUIGallery.Tests/ShowCases/ComboBoxSemanticPartHighlightTests.cs`.
- Results: Desktop 40 classes **374/374**; Generator 2 classes **64/64**; Gallery 3 classes **8/9**, one failure: `ComboBoxSemanticPartHighlightTests.ComboBox_Semantic_Preview_Cards_Highlight_On_Hover_While_Pinned`, root-card highlight count 0 at line 203. Runner reports **failed**, not a green receipt. Isolated current-tree repeat fails identically.
- Direct DataGrid consumer: exact `FullyQualifiedName~DataGridAutoCompleteTemplateFocusTests` **0/4**, all four focus assertions fail, matching the earlier upgrade baseline.
- Fresh same-version baseline constructed outside the checkout from HEAD `00197a821` plus the task-start diff (preserves prior Menu/Popup fixes; contains the original presenter class/templates). Both exact diagnostic filters rebuilt and executed there: the identical Gallery failure **0/1** and identical DataGrid failures **0/4** reproduce with matching assertions/locations. Checked failure names programmatically. None of these five failures is introduced by this retirement; they remain unresolved existing Linux/headless issues, not skipped or weakened.
- Permanent focused selection including the direct DataGrid diagnosis totals **451 tests: 446 passed, 5 baseline failures, 0 skipped**. Temporary 144-state evidence and 19 semantic tests overlap other checks and are not added to this count. No all-green or full-regression claim.
- Rendering.Tests rebuilt after removing temporary sources to clear fixture/legacy-probe IL from the test assembly: succeeds, one existing xUnit1051 warning, zero errors. Desktop.Tests was rebuilt by the final iterate after temporary link/helper removal.
- Baseline and current byte comparisons confirm all seven pre-existing Menu/Popup theme/document files unchanged by this task. No further reviewer needed: the reviewer found no production issue, and diagnostic failures are baseline-identical.

## Benefits, limits and cleanup

| Metric | Baseline | Optimized | Formula | Improvement | Conclusion |
| --- | --- | --- | --- | --- | --- |
| Full invalidation callbacks per affected selection-property change (non-null selection foreground) | 2 callbacks | 1 callback | (2-1)/2 | 50% fewer callbacks | Removes duplicate compatibility work; does not quantify shaping cost, frame time or page speed |
| Internal compatibility subclasses maintained | 1 class | 0 classes | (1-0)/1 | 100% removed | All six template paths use the same native presenter contract |

Production complexity: one 21-line class removed; no new production methods, fields, bindings, timers or lifecycle branches. OTP adds an internal-property region and widens three internal type references. Tool code replaces an implementation counter with a single output helper and state assertions; 16-line counter helper removed. AXAML nodes stay declarative. No cold-load benchmark or wall-clock speedup claim.

Native IME, visibly blinking caret, Windows/macOS interaction and whole performance-tool execution remain unverified in this retirement. NativeAOT startup and headless rendering cannot stand in for those interactions. No source generators, reflection hooks, clocks or platform implementations changed.

All new temporary tests, linked source entries, AXAML fixture, baseline source/build tree, runtime copy, AOT publish, review package, snapshots and raw logs are deleted after recording these outcomes. No task process remains, no net-new permanent tests, no git commit. Final audit/diff checks recorded below.

Final checks: `scripts/verification/test.py audit` and `git diff --check` pass. Temporary source/artifact cleanup completed; no commit created.

## 编译阻断的后续修复

用户随后授权修复性能工具编译错误，修复与验证记录见[性能工具构建修复](2026-11-05-performance-tool-build-repair.md)。此前的构建失败描述保留为本轮退役时的历史状态；完整 Input/TabItem 状态断言的旧契约问题另行列出，不与编译成功混同。
