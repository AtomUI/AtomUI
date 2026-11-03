# CT15 ImagePreviewer 限定视觉审查（2026-10-31）

状态：工程、临时清理、清理后117项/NET8验收完成；表内104对已获用户限定批准。 Only the following 104 intentional old/new pairs are proposed. No family/task closure or Git authorization.

Native frames 1344x896; direct Overlay 900x650. Four algorithms, full matching history, normal motion, 750ms + 12 equal frames / 6s bound. Same-coordinate complete RGBA; no mask/crop/align/tolerance. Values are changed pixels / max channel difference.

| 宿主 / owner / 明确有意变化 | Light | Dark | Compact | DarkCompact |
| --- | --- | --- | --- | --- |
| native / single / 专用Style生效 | 1150464/max196 | 1150464/max214 | 1150464/max196 | 1150464/max214 |
| native / group / 专用Style生效 | 1150464/max196 | 1150464/max214 | 1150464/max196 | 1150464/max214 |
| native / single / 14Own含切换67 | 1150464/max202 | 1150464/max239 | 1150464/max202 | 1150464/max246 |
| native / group / 14Own含切换67 | 1150464/max202 | 1150464/max239 | 1150464/max202 | 1150464/max246 |
| overlay / single / 14Own含切换67 | 36927/max198 | 36927/max207 | 29237/max198 | 29237/max206 |
| overlay / group / 14Own含切换67 | 36927/max198 | 36927/max207 | 29237/max198 | 29237/max206 |
| native / single / 显式切换/图标0 | 1150464/max193 | 1150464/max194 | 1150464/max193 | 1150464/max194 |
| native / group / 显式切换/图标0 | 1150464/max193 | 1150464/max194 | 1150464/max193 | 1150464/max194 |
| overlay / single / 显式切换/图标0 | 22505/max194 | 22505/max202 | 15874/max194 | 15874/max202 |
| overlay / group / 显式切换/图标0 | 22505/max194 | 22505/max202 | 15874/max194 | 15874/max202 |
| native / single / 公开Global字体/颜色/间距 | 1151685/max199 | 1151678/max205 | 1151685/max207 | 1151678/max247 |
| native / group / 公开Global字体/颜色/间距 | 1151685/max199 | 1151678/max214 | 1151685/max207 | 1151678/max247 |
| overlay / single / 公开Global字体/颜色/间距 | 12718/max198 | 12734/max205 | 11035/max199 | 11027/max206 |
| overlay / group / 公开Global字体/颜色/间距 | 12703/max198 | 12719/max205 | 11021/max199 | 11013/max206 |
| native / single / 原生Courier17优先 | 1150464/max199 | 1150464/max205 | 1150464/max207 | 1150464/max247 |
| native / group / 原生Courier17优先 | 1150464/max199 | 1150464/max214 | 1150464/max207 | 1150464/max247 |
| overlay / single / 原生Courier17优先 | 12614/max198 | 12630/max205 | 10768/max199 | 10758/max206 |
| overlay / group / 原生Courier17优先 | 12599/max198 | 12615/max205 | 10753/max199 | 10743/max206 |
| native / single / 清原生回Global | 1151685/max199 | 1151678/max205 | 1151685/max207 | 1151678/max247 |
| native / group / 清原生回Global | 1151685/max199 | 1151678/max214 | 1151685/max207 | 1151678/max247 |
| overlay / single / 清原生回Global | 12718/max198 | 12734/max205 | 11035/max199 | 11027/max206 |
| overlay / group / 清原生回Global | 12703/max198 | 12719/max205 | 11021/max199 | 11013/max206 |
| native / single / 失败正文/图标Global | 1153126/max169 | 7957/max237 | 1153127/max169 | 8009/max255 |
| native / group / 失败正文/图标Global | 1153100/max169 | 7956/max237 | 1153099/max169 | 8008/max255 |
| overlay / single / 失败正文/图标Global | 16109/max161 | 15697/max226 | 14426/max161 | 13943/max227 |
| overlay / group / 失败正文/图标Global | 16089/max161 | 15678/max226 | 14408/max161 | 13924/max227 |

## 具体限定范围

The native style repair makes the existing generated popup root/body/footer/actions styles enter the independent window. The fixture uses body #FFCE27A9, actions Padding17,9, root Opacity.93 and footer Opacity.8; image Opacity.8 / cover radius11 retain their existing routes, and Overlay mask #800080FF remains optional. The public root uses its normal properties; seven other parts use dedicated AXAML styles only. No code-behind part assignments. Eight descriptors, routes and contract types are unchanged; close is not a published part.

Own recipe: operation size31 (0 in zero phase), optional switch67 (0), operation #DEB887 / hover #00FFFF / disabled #799999, nav #66123456 / hover #99345678, toolbar23,11 / index7,3, mask #70123456, cover137, window minima511x211 and title #234567. Own clear restores the original auto/values; 56 four-algorithm defaults, types, keys and slots remain exact. Global recipe is Menlo23, solid text #D6ABFF, tertiary #FFA940, MarginSM11,13 and MarginXL17,19. Native recipe is Courier17 and clear restores current Global. TitleFontSize retains its separate WindowTitleBar role.

The original two borrowed images, pending/failed third item, titles, viewport, automatic native size, source/lease/borrowed semantics and current keyboard/scale behaviour are retained. Plain scenes remove both owners user classes before attachment; all 16 unconfigured defaults compare zero. The table does not claim general contrast/brand acceptance.

## 排除的失败和边界

- Old/new 448 pairs: 173 zero, 275 nonzero. Only 104 intentional pairs above are proposed; the other 171 nonzero pairs (107 static + 64 timed loading) remain unapproved.
- JIT/AOT 448 pairs: 355 zero, 93 nonzero = 29 static + all 64 timed loading. Every proposed pair above is JIT/AOT zero.
- All three versions have 32/32 same-instance restore/reopen pairs zero; independent A/A and 1-DIP restoration zero, detection four times408/max155 per version. This does not erase other intermediate nonzero frames.
- The 29 static cross-runtime differences include reset/retained/next/business frames, 4 to111 pixels/max1 to14. Inspected diff bounds include native reset (29,451)-(1330,859) at thin navigation/footer contours, and compact Overlay next (29,315)-(31,335), four/max1. Cause is not closed or assigned to CT25/FW1; no tolerance or exemption.
- The 64 loading samples use the same requested 0/64/128/256ms sequence with real timers; actual capture times are recorded. Every version has 48 positive inter-frame motion comparisons. Cross-runtime/source trajectories have not passed exact phase equality. Motion was not disabled.
- Actual OS window/chrome/input, Browser public Overlay opening path, full hover/wheel/drag, natural extremes and complete ImagePreviewer/CT15 remain open. Direct Overlay is a scripted internal host with real model/loader and current-index projection, not a Browser claim.

See [results](image-previewer-consumers-results.md) and [author review](image-previewer-consumers-review.md).


限定批准登记：2026-10-31，用户明确回复“批准表内104对，其余未通过与完整CT15保留”（request_user_input_async / call_5593304c893e46a296348b6914a561c3 / item 0）。仅ImagePreviewer表内104对明确变化获批准；其余171对旧新非零、29静态/64持续态跨运行非零、OS/Browser/输入/极值及完整ImagePreviewer/CT15继续保留，不授权提交。
