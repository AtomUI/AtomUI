# CT19 C7：色谱请求顺序与范围映射结果（验收中）

本记录只覆盖C7，不表示CT19完整家族或CT25平台最终像素验收完成。继续既有feature/custom-theme及checkout；产品候选尚未获Git提交授权，不混入已授权第十二批平台提交b8325380f671。

## 产品范围与前后证据

公开ColorPickerView的受限饱和度25→60，受控地让旧请求晚完成：旧版实际红色位图左上由正确#FF6666回退#FFBFBF；独立Spectrum当前420×160被旧300×160覆盖。修正最初不触发内部Bounds的无效外层尺寸fixture后，两项真实反例均失败。范围归一、单像素轴及共享键盘增量helper继续补到26项；最终26项8.0734秒通过。

候选用UI线程拥有的请求代次约束完成顺序，完整范围的同步fast路径同样使旧请求过期。只有最新请求可分配当前位图；分量、尺寸和归一范围元数据均来自同一启动快照。min>=max沿用既有“min为唯一值”约定，零跨度选择环百分比取0；单像素轴不再计算0/0；共享增量helper在倒置／等值范围直接返回该分量min，保留另外两分量。没有新增公开API、Token、Part、绑定或宿主回调。

纯成员布局单独保存，99成员token、imports及header相同，静态初始化次序不变；24项布局问题归零，未因1755行拆文件。之后若申请Git，纯布局与行为必须分开提交。

## 生命周期与清理后功能出口

fast替代pending、重模板、实际框架scale1→2及同View迁往scale2宿主的位图／map均通过。请求完成但未确认渲染时一次GC探针仍观察保留；等所有实际位图任务与两个渲染帧回执后，26项通过。没有为这次GC现象新增产品生命周期代码，尚不能据此归因框架内部保留机制。

所有临时功能测试、产品worker门闩及任务观测已删除。清理后Desktop精确23项5.3630秒通过，NET8 Release零警告／错误，2文件成员审计零问题。最终候选SHA在SDD中逐项核对；一次Subscribe参数与Task discard冲突造成的清理编译准备错误已修正并保留日志，不计产品反例。

## 自驱动宿主与验证准备限制

驱动与所有媒体、工程和编译产物都在系统临时目录；只读观测实际矩形位图、尺寸、选择环、公开树和输入事件，不写内部节点样式。自然Headless/Skia合成帧与独立实例的直接Skia绘制分别记录，不能互换基线；不缩放、对齐、遮罩或使用通用像素容差。

准备阶段两组证据不能当正式亮／暗验收：基础Avalonia Window的继承缺失；随后产品Window只设置RequestedThemeVariant，实际AtomUI主题仍为Light。看图发现并撤回覆盖结论后，正式驱动先调用ThemeManager.ApplyThemeAsync，断言事务成功、CurrentTheme.Appearance、Window.ActualThemeVariant及实际背景。前两组42／70帧已查看、保留数值／SHA／失败记录并审计清理。第二组18对直接A/A／恢复零差异只代表当时实际Light输入；不改标为Dark通过。

原生初探还保留一次渐变stop命中期待失败与两次方向键收到但无颜色通知的期待失败。现有AbstractColorSlider／ColorSlider没有键盘处理，Spectrum默认也不可聚焦，不能将它们称为C7原生键盘覆盖；helper边界只有临时功能验证证据，该既有可达性缺口另列C8。

## 正式验收进度

有效亮／暗自然Headless的308项、独立直接Skia的206项候选检查通过，旧版同轨迹分别50／33项失败，140帧均已取得。原生扩展到每阶段实际范围点击后，四项选择环期待因渐变活动stop未重选、前提颜色仍为旧编辑值而失败；保留该轮，通过真实第二stop→第一stop切换建立红色输入前提后，Mac managed明暗184项通过。一次在前轮构建完成前运行的重复准备也保留为overlap-preparation，不当成最终源身份。

NativeAOT首轮184项有两次初始stop命中失败；为输入增加同一目标／同一点的2秒有界实际命中就绪前置条件，超时仍失败，不改节点／坐标／产品代码。最后managed与NativeAOT各184项通过，实际NSWindow、scale2、28次范围中心指针及相应事件／独立颜色预期均通过；两项键盘仅证明现有事件到达但未处理。最终两后端的命中就绪等待均未触发，因此不能据这一轮通过证明早先两次失败的内部原因或声称修了原生冷启动问题。原始失败保留，结论只覆盖所记就绪条件下的C7输入及范围合同。

临时驱动、外部工程和434个专用文件已删除，SHA／编译／运行及警告日志保留；NativeAOT使用当前实际Mac，保留临时驱动未执行的反射诊断分支警告及macOS12链接目标／Homebrew26库警告，不声称旧系统或原生最终像素兼容。临时红版worker埋点备份也已删除，保留的两文件原始／候选与纯布局文本仅供分离Git审查。清理后Release精确23项约2秒、NET8零警告／错误52.41秒及2文件成员审计零问题通过。

用户已明确批准本次修复与下表41对限定合成差异；不沿用或扩大C1–C6批准，不代表Git授权或完整CT19完成。原始比较和失败计数保持，正式比较器没有添加容差。

## Exact comparison record (approved within the recorded limits)

Host: Mac arm64, Headless/Skia, scale1, 800x620. Two public Views, same 7-stage history, independent main/AA/1DIP roles. Real theme transactions and effective appearance were asserted. Natural compositor and independent direct Skia records are separate.

140 frames: candidate natural 308 checks and direct 206 checks passed; original natural 50/308 and direct 33/206 checks failed. These original failures remain product failures, including non-finite selectors and stale bitmaps. All 28 one-DIP controls were detected. Candidate direct AA14 and reset4 were exactly zero; original direct AA6/14 differ due to stale-generation output, original reset4 were zero. Candidate public-tree AA14 and bitmap groups10 agree exactly.

| Stage | Natural Light | Natural Dark | Direct Light | Direct Dark |
| --- | ---: | ---: | ---: | ---: |
| default | 0 | 8 | 0 | 0 |
| limited | 3 | 41443 | 41435 | 0 |
| collapsed-saturation | 128 | 132 | 40597 | 40597 |
| collapsed-value | 41755 | 83322 | 41752 | 83314 |
| collapsed-hue | 0 | 8 | 0 | 0 |
| rapid | 3 | 40320 | 40312 | 40312 |
| reset | 3 | 8 | 0 | 0 |

Every value above is the original full-frame exact RGBA count. Larger original/candidate changes reflect stale-generation pixels or corrected collapsed-axis selectors. The original is not deterministic in affected async stages: six independent direct AA pairs also show the defect. Candidate outputs match independent coordinate/color expectations; direct original/candidate variation does not replace natural results.

### Bounded thumb compositor differences approved by the user

The following 41 pairs contain 3-8 differing thumb-edge pixels, max channel delta16. Each row lists its own inclusive region and numerical bounds; applicability is limited to the exact recorded host/configuration/history/pair. The full-frame comparator remains unchanged and retains all failures. Some rows also contain larger Spectrum product differences, recorded separately in Full; that larger count is not accepted as environment noise. No diagnosis redraw, mask, tolerance, or source change was applied to product code.

The candidate public trees and source bitmap hashes match. Independent candidate direct AA14/reset4 are zero. These observations isolate the listed differences to compositor output for these records; an internal cache cause has not been proven. They do not authorize another host, a new history, native final pixels, or an entire family. Exact per-pixel before/after RGBA values remain in the local text record visual-thumb-differences.json.

| # | Before / After | Full | Thumb | Inclusive bbox | Max channel |
| --- | --- | ---: | ---: | --- | ---: |
| 1 | old-light-main-default.png / old-light-aa-default.png | 3 | 3 | [359, 239, 579, 256] | 16 |
| 2 | old-light-main-limited.png / old-light-aa-limited.png | 7 | 7 | [27, 219, 579, 256] | 16 |
| 3 | old-light-main-collapsed-saturation.png / old-light-aa-collapsed-saturation.png | 40522 | 3 | [359, 239, 579, 256] | 16 |
| 4 | old-light-main-collapsed-value.png / old-light-aa-collapsed-value.png | 83272 | 6 | [27, 219, 579, 256] | 16 |
| 5 | old-light-main-collapsed-hue.png / old-light-aa-collapsed-hue.png | 6 | 6 | [27, 219, 579, 256] | 16 |
| 6 | old-light-main-rapid.png / old-light-aa-rapid.png | 6 | 6 | [27, 219, 579, 256] | 16 |
| 7 | old-light-main-reset.png / old-light-aa-reset.png | 3 | 3 | [359, 239, 579, 256] | 16 |
| 8 | old-light-main-default.png / old-light-main-reset.png | 3 | 3 | [27, 219, 247, 236] | 16 |
| 9 | old-light-aa-default.png / old-light-aa-reset.png | 3 | 3 | [27, 219, 247, 236] | 16 |
| 10 | old-light-dip-default.png / old-light-dip-reset.png | 3 | 3 | [28, 219, 248, 236] | 16 |
| 11 | old-dark-main-collapsed-value.png / old-dark-aa-collapsed-value.png | 4 | 4 | [27, 219, 254, 237] | 14 |
| 12 | old-dark-main-collapsed-hue.png / old-dark-aa-collapsed-hue.png | 4 | 4 | [27, 219, 254, 237] | 14 |
| 13 | old-dark-main-default.png / old-dark-main-reset.png | 4 | 4 | [27, 219, 254, 237] | 14 |
| 14 | old-dark-aa-default.png / old-dark-aa-reset.png | 4 | 4 | [27, 219, 254, 237] | 14 |
| 15 | new-light-main-default.png / new-light-aa-default.png | 6 | 6 | [27, 219, 579, 256] | 16 |
| 16 | new-light-main-limited.png / new-light-aa-limited.png | 6 | 6 | [27, 219, 579, 256] | 16 |
| 17 | new-light-main-collapsed-saturation.png / new-light-aa-collapsed-saturation.png | 3 | 3 | [359, 239, 579, 256] | 16 |
| 18 | new-light-main-collapsed-value.png / new-light-aa-collapsed-value.png | 3 | 3 | [359, 239, 579, 256] | 16 |
| 19 | new-light-main-collapsed-hue.png / new-light-aa-collapsed-hue.png | 3 | 3 | [359, 239, 579, 256] | 16 |
| 20 | new-light-main-rapid.png / new-light-aa-rapid.png | 6 | 6 | [27, 219, 579, 256] | 16 |
| 21 | new-light-main-reset.png / new-light-aa-reset.png | 3 | 3 | [359, 239, 579, 256] | 16 |
| 22 | new-light-aa-default.png / new-light-aa-reset.png | 3 | 3 | [27, 219, 247, 236] | 16 |
| 23 | new-dark-main-default.png / new-dark-aa-default.png | 8 | 8 | [27, 219, 586, 257] | 14 |
| 24 | new-dark-main-limited.png / new-dark-aa-limited.png | 4 | 4 | [359, 239, 586, 257] | 14 |
| 25 | new-dark-main-collapsed-saturation.png / new-dark-aa-collapsed-saturation.png | 4 | 4 | [359, 239, 586, 257] | 14 |
| 26 | new-dark-main-collapsed-value.png / new-dark-aa-collapsed-value.png | 4 | 4 | [359, 239, 586, 257] | 14 |
| 27 | new-dark-main-collapsed-hue.png / new-dark-aa-collapsed-hue.png | 8 | 8 | [27, 219, 586, 257] | 14 |
| 28 | new-dark-main-rapid.png / new-dark-aa-rapid.png | 8 | 8 | [27, 219, 586, 257] | 14 |
| 29 | new-dark-main-reset.png / new-dark-aa-reset.png | 4 | 4 | [359, 239, 586, 257] | 14 |
| 30 | new-dark-main-default.png / new-dark-main-reset.png | 4 | 4 | [27, 219, 254, 237] | 14 |
| 31 | old-light-main-limited.png / new-light-main-limited.png | 3 | 3 | [243, 236, 247, 245] | 1 |
| 32 | old-light-main-collapsed-value.png / new-light-main-collapsed-value.png | 41755 | 3 | [27, 219, 247, 236] | 16 |
| 33 | old-light-main-rapid.png / new-light-main-rapid.png | 3 | 3 | [27, 219, 247, 236] | 16 |
| 34 | old-light-main-reset.png / new-light-main-reset.png | 3 | 3 | [27, 219, 247, 236] | 16 |
| 35 | old-dark-main-default.png / new-dark-main-default.png | 8 | 8 | [27, 219, 586, 257] | 14 |
| 36 | old-dark-main-limited.png / new-dark-main-limited.png | 41443 | 8 | [27, 219, 586, 257] | 14 |
| 37 | old-dark-main-collapsed-saturation.png / new-dark-main-collapsed-saturation.png | 132 | 4 | [359, 239, 586, 257] | 14 |
| 38 | old-dark-main-collapsed-value.png / new-dark-main-collapsed-value.png | 83322 | 8 | [27, 219, 586, 257] | 14 |
| 39 | old-dark-main-collapsed-hue.png / new-dark-main-collapsed-hue.png | 8 | 8 | [27, 219, 586, 257] | 14 |
| 40 | old-dark-main-rapid.png / new-dark-main-rapid.png | 40320 | 8 | [27, 219, 586, 257] | 14 |
| 41 | old-dark-main-reset.png / new-dark-main-reset.png | 8 | 8 | [27, 219, 586, 257] | 14 |

Visual review: effective Dark default has dark window/input surfaces and readable text. Light and Dark collapsed-value comparisons show the selector move from an invalid/top position to the bottom boundary and the stale gradient bitmap replaced by the current V60 bitmap. Dark rapid comparison shows the gradient panel now consumes final S60 instead of the older request. Input rows, gradient stops, both sliders, dimensions and unrelated content remain visible. The user explicitly approved the repair and all 41 table-limited pairs. Native final pixels remain pending.

## 批准、清理与切片出口

用户明确答复“允许修复并接受表内限定差异”，只覆盖本表Host／阶段／坐标／每行数值，不扩大到原生最终像素、其他平台、Git或完整家族。批准后正式媒体运行审计无发现，145个运行文件（含140帧、4张差异图及管理文件）已删除；前两组准备媒体亦已清理。数值、原始失败及SHA保留，未向比较器添加容差。最终clean源SHA与26项候选保持一致，NET8、聚焦23项及成员审计通过；C7研发／验证切片完成并待Git审查，CT19保持进行中。
