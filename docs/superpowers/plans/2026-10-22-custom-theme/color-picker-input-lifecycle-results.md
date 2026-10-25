# CT19 C6：输入框重挂同步审查

用户已明确批准C6同步修复及表内59对限定合成差异。本切片功能、生命周期、聚焦模块与所列视觉审查已通过；不表示CT19或完整ColorPicker家族完成，不包含Git提交授权。当前分支feature/custom-theme、现有checkout，无新分支／worktree／子agent或NuGet发布。

## 行为与实现

旧版公开ColorPickerView离树改Blue后，色谱与公共输入已是Blue，但HEX仍显示White；HSV／RGB同样遗漏更新。离树改变格式也留下旧选择器／字段。候选只记录离树期间发生过的权威颜色／格式变化，回树时同步当前值；没有外部变化则保留草稿、校验和光标，A→B→A仍覆盖旧草稿。格式选择改为ControlTheme的编译SelectedValueBinding和双向TemplateBinding，原SelectedIndex映射与SelectionChanged订阅已移除。没有新增公开API、Own、资源编号或忽略PropertyChanged的标志。

两产品文件SHA256：ColorPickerInput.cs为ab2c9d86fb09930f933e139b0de2822a1aaa5e134ae7603ac82d6ecb3e79e738；Input.axaml为a4061aecd87a81ac2a7e47120f9383947afc48236ceebbf8ea79f625ef7598e2。旧基线为9e085594030faf734958e535516b0f249f303a5d。布局、色谱、Alpha、调色板、确认区及模板结构保留。

## 功能、生命周期与平台

原版18项5通过13失败，候选18通过；补充重模板旧selector解绑和整图GC后20项全部通过8.9373秒。覆盖三格式、确认两态、草稿／数值／光标保留、外部往返覆盖、绑定持续生效、重复挂载事件次数及真实Headless原始点击／文字。view／input／selector／host四WeakReference释放。

Mac NSWindow、scale2、明暗三格式，managed／NativeAOT各308项通过。挂载后等待两个实际渲染帧再发原始输入；输入事件报告使用公开View的ValueChanged合同。没有扩大为Windows、Linux或原生最终截图通过；AOT链接的宿主目标OS警告保留。原生临时工程420文件已删除。

临时测试及回放驱动共6文件已删除，常驻测试净新增0。清理后Desktop的ColorPickerPinnedOpen／SemanticPart／TimePickerDisplayTime精确23项5.1405秒通过；NET8 Release ColorPicker含依赖编译零警告／错误32.89秒；Roslyn成员审计files1／controls1／issues0。未运行本地change或全量。批准后，正式／直接诊断／普通输入三个SVA run均完成Git二进制audit及cleanup，媒体已删除，帧SHA、数值和逐对限定保留；审计无新增二进制。

## 视觉执行与证据边界

正式采样为Mac arm64 Headless/Skia、Avalonia12.1.4、scale1、800×620、View宽300，保留默认动效。先以显式Black→White建立非空White，分别执行：确认模式的invalid草稿→无改值重挂→离树Blue→回树→附着White恢复→离树格式改变；普通模式的White→离树Blue→回树→White恢复。每条轨迹使用独立main／A/A／1DIP实例，全图同坐标比较，无裁剪、对齐、遮罩或通用容差。

原同步驱动只RunJobs与render tick，会让确认按钮过渡停在中间色；1500ms诊断已复现。单变量改为正常DispatcherFrame泵送后，约204–210ms达到200ms过渡终值，300–1500ms真实帧SHA稳定。正式采样使用650ms同轨迹泵送，未关闭动画。失效采样的媒体及590MB临时公开属性dump已清理，原始失败计数、全部差异、源码／日志SHA、每帧属性／几何摘要及Bitmap SHA保留。

正式确认216帧及普通108帧合计324帧，108组1DIP正对照全部检出；108组A/A有93组为0、15组边缘1–5像素，36组恢复的2–22像素及旧新恢复中的限定差异逐对列在下表。普通Light HEX Blue整图342像素，输入文字340，另2点位于Alpha滑轨边缘；这2点与原版A/A相同坐标和变化，完整342仍保留，不从比较器扣除。

另有独立288帧诊断：直接Skia的72组A/A、24组恢复全部0；36组旧新目标／非目标预期全部符合，非目标为0。492／625／654节点的公开属性图在72组A/A中仅自动Radio分组名不同，几何、矩阵、字体、画刷与Bitmap SHA相同。恢复并非全部原始属性相同：透明背景null变显式Transparent、部分HSV最小值spinner状态，以及格式原生绑定的SelectedValue形态差异均完整记录；没有据此删除compositor差异，也未证明其内部缓存是唯一原因。既有C1–C5批准不扩大到本次场景。

下表期望变化限输入文字、格式选择显示及外部覆盖无效草稿后恢复可确认状态。截图已实际查看普通旧White／新Blue、确认明色旧invalid／新Blue及暗色RGB／格式场景。

## 期望变化原始计数

| Scene | Theme | Format | Full changed pixels | Full diff bbox (exclusive end) |
| --- | --- | --- | ---: | --- |
| confirmation/blue | light | Hex | 1862 | [314, 224, 503, 276] |
| confirmation/format | light | Hex | 540 | [258, 224, 441, 236] |
| confirmation/blue | light | Hsva | 1934 | [307, 226, 503, 276] |
| confirmation/format | light | Hsva | 431 | [258, 224, 355, 235] |
| confirmation/blue | light | Rgba | 1846 | [309, 226, 503, 276] |
| confirmation/format | light | Rgba | 620 | [249, 220, 449, 242] |
| confirmation/blue | dark | Hex | 1796 | [314, 224, 503, 276] |
| confirmation/format | dark | Hex | 423 | [307, 227, 441, 236] |
| confirmation/blue | dark | Hsva | 1869 | [307, 226, 503, 276] |
| confirmation/format | dark | Hsva | 314 | [307, 225, 355, 235] |
| confirmation/blue | dark | Rgba | 1781 | [309, 226, 503, 276] |
| confirmation/format | dark | Rgba | 397 | [298, 220, 449, 242] |
| plain/blue | light | Hex | 342 | [314, 199, 457, 235] |
| plain/blue | light | Hsva | 311 | [307, 227, 388, 236] |
| plain/blue | light | Rgba | 266 | [309, 227, 380, 236] |
| plain/blue | dark | Hex | 340 | [314, 225, 358, 235] |
| plain/blue | dark | Hsva | 311 | [307, 227, 388, 236] |
| plain/blue | dark | Rgba | 266 | [309, 227, 380, 236] |

## Approved limited compositor differences

Only the exact recorded host, configuration, histories, pairs, individual regions and bounds below are approved. Every row retains its original full changed-pixel count; no tolerance is added to the comparator. Coordinates are inclusive. Hue thumb union: [234,175,252,189]; Alpha thumb union: [455,195,469,206]. No other pixel region is covered. Source pixel RGBA pairs and per-frame digests remain in the local text record. This review does not cover Git commits, native final pixels or a complete control family.

| Suite / check | Before -> after | Count | Hue: count / bbox / max channel | Alpha: count / bbox / max channel |
| --- | --- | ---: | --- | --- |
| confirmation/aa | old-light-Hex-main-default -> old-light-Hex-aa-default | 1 | 0 | 1 / [460, 195, 460, 195] / 1 |
| confirmation/aa | old-light-Hex-main-draft -> old-light-Hex-aa-draft | 1 | 0 | 1 / [460, 195, 460, 195] / 1 |
| confirmation/restore | old-light-Hex-main-draft -> old-light-Hex-main-draft-return | 6 | 4 / [247, 179, 247, 184] / 1 | 2 / [456, 195, 460, 199] / 1 |
| confirmation/restore | old-light-Hex-main-default -> old-light-Hex-main-reset | 3 | 2 / [234, 175, 238, 179] / 2 | 1 / [460, 195, 460, 195] / 1 |
| confirmation/restore | old-light-Hsva-main-draft -> old-light-Hsva-main-draft-return | 6 | 4 / [247, 179, 247, 184] / 1 | 2 / [456, 195, 460, 199] / 2 |
| confirmation/restore | old-light-Hsva-main-default -> old-light-Hsva-main-reset | 2 | 2 / [234, 175, 238, 179] / 2 | 0 |
| confirmation/restore | old-light-Rgba-main-draft -> old-light-Rgba-main-draft-return | 6 | 4 / [247, 179, 247, 184] / 1 | 2 / [456, 195, 460, 199] / 2 |
| confirmation/restore | old-light-Rgba-main-default -> old-light-Rgba-main-reset | 2 | 2 / [234, 175, 238, 179] / 2 | 0 |
| confirmation/aa | old-dark-Hex-main-default -> old-dark-Hex-aa-default | 3 | 2 / [240, 178, 240, 185] / 14 | 1 / [460, 196, 460, 196] / 1 |
| confirmation/aa | old-dark-Hex-main-draft -> old-dark-Hex-aa-draft | 3 | 2 / [240, 178, 240, 185] / 14 | 1 / [460, 196, 460, 196] / 1 |
| confirmation/aa | old-dark-Hex-main-reset -> old-dark-Hex-aa-reset | 3 | 2 / [240, 178, 240, 185] / 14 | 1 / [460, 196, 460, 196] / 1 |
| confirmation/restore | old-dark-Hex-main-draft -> old-dark-Hex-main-draft-return | 22 | 11 / [236, 177, 247, 186] / 14 | 11 / [456, 196, 469, 206] / 1 |
| confirmation/restore | old-dark-Hex-main-default -> old-dark-Hex-main-reset | 14 | 13 / [234, 176, 244, 188] / 14 | 1 / [460, 196, 460, 196] / 1 |
| confirmation/restore | old-dark-Hsva-main-draft -> old-dark-Hsva-main-draft-return | 20 | 9 / [236, 177, 247, 186] / 1 | 11 / [456, 196, 469, 206] / 2 |
| confirmation/restore | old-dark-Hsva-main-default -> old-dark-Hsva-main-reset | 11 | 11 / [234, 176, 244, 188] / 2 | 0 |
| confirmation/restore | old-dark-Rgba-main-draft -> old-dark-Rgba-main-draft-return | 20 | 9 / [236, 177, 247, 186] / 1 | 11 / [456, 196, 469, 206] / 2 |
| confirmation/restore | old-dark-Rgba-main-default -> old-dark-Rgba-main-reset | 11 | 11 / [234, 176, 244, 188] / 2 | 0 |
| confirmation/aa | new-light-Hex-main-default -> new-light-Hex-aa-default | 1 | 0 | 1 / [460, 195, 460, 195] / 1 |
| confirmation/aa | new-light-Hex-main-draft -> new-light-Hex-aa-draft | 1 | 0 | 1 / [460, 195, 460, 195] / 1 |
| confirmation/restore | new-light-Hex-main-draft -> new-light-Hex-main-draft-return | 6 | 4 / [247, 179, 247, 184] / 1 | 2 / [456, 195, 460, 199] / 1 |
| confirmation/restore | new-light-Hex-main-default -> new-light-Hex-main-reset | 2 | 2 / [234, 175, 238, 179] / 2 | 0 |
| confirmation/restore | new-light-Hsva-main-draft -> new-light-Hsva-main-draft-return | 6 | 4 / [247, 179, 247, 184] / 1 | 2 / [456, 195, 460, 199] / 2 |
| confirmation/restore | new-light-Hsva-main-default -> new-light-Hsva-main-reset | 2 | 2 / [234, 175, 238, 179] / 2 | 0 |
| confirmation/restore | new-light-Rgba-main-draft -> new-light-Rgba-main-draft-return | 6 | 4 / [247, 179, 247, 184] / 1 | 2 / [456, 195, 460, 199] / 2 |
| confirmation/restore | new-light-Rgba-main-default -> new-light-Rgba-main-reset | 2 | 2 / [234, 175, 238, 179] / 2 | 0 |
| confirmation/aa | new-dark-Hex-main-default -> new-dark-Hex-aa-default | 3 | 2 / [240, 178, 240, 185] / 14 | 1 / [460, 196, 460, 196] / 1 |
| confirmation/aa | new-dark-Hex-main-draft -> new-dark-Hex-aa-draft | 3 | 2 / [240, 178, 240, 185] / 14 | 1 / [460, 196, 460, 196] / 1 |
| confirmation/aa | new-dark-Hex-main-reset -> new-dark-Hex-aa-reset | 5 | 2 / [240, 178, 240, 185] / 14 | 3 / [456, 199, 457, 202] / 1 |
| confirmation/restore | new-dark-Hex-main-draft -> new-dark-Hex-main-draft-return | 22 | 11 / [236, 177, 247, 186] / 14 | 11 / [456, 196, 469, 206] / 1 |
| confirmation/restore | new-dark-Hex-main-default -> new-dark-Hex-main-reset | 11 | 11 / [234, 176, 244, 188] / 2 | 0 |
| confirmation/restore | new-dark-Hsva-main-draft -> new-dark-Hsva-main-draft-return | 20 | 9 / [236, 177, 247, 186] / 1 | 11 / [456, 196, 469, 206] / 2 |
| confirmation/restore | new-dark-Hsva-main-default -> new-dark-Hsva-main-reset | 11 | 11 / [234, 176, 244, 188] / 2 | 0 |
| confirmation/restore | new-dark-Rgba-main-draft -> new-dark-Rgba-main-draft-return | 20 | 9 / [236, 177, 247, 186] / 1 | 11 / [456, 196, 469, 206] / 2 |
| confirmation/restore | new-dark-Rgba-main-default -> new-dark-Rgba-main-reset | 11 | 11 / [234, 176, 244, 188] / 2 | 0 |
| confirmation/before-after | old-light-Hex-main-reset -> new-light-Hex-main-reset | 1 | 0 | 1 / [460, 195, 460, 195] / 1 |
| confirmation/before-after | old-dark-Hex-main-reset -> new-dark-Hex-main-reset | 3 | 2 / [240, 178, 240, 185] / 14 | 1 / [460, 196, 460, 196] / 1 |
| plain/aa | old-light-Hex-main-default -> old-light-Hex-aa-default | 1 | 0 | 1 / [460, 195, 460, 195] / 1 |
| plain/aa | old-light-Hex-main-blue -> old-light-Hex-aa-blue | 2 | 0 | 2 / [456, 199, 456, 204] / 1 |
| plain/restore | old-light-Hex-main-default -> old-light-Hex-main-reset | 2 | 2 / [234, 175, 238, 179] / 2 | 0 |
| plain/restore | old-light-Hsva-main-default -> old-light-Hsva-main-reset | 3 | 2 / [234, 175, 238, 179] / 2 | 1 / [460, 195, 460, 195] / 1 |
| plain/restore | old-light-Rgba-main-default -> old-light-Rgba-main-reset | 3 | 2 / [234, 175, 238, 179] / 2 | 1 / [460, 195, 460, 195] / 1 |
| plain/restore | old-dark-Hex-main-default -> old-dark-Hex-main-reset | 14 | 13 / [234, 176, 244, 188] / 14 | 1 / [460, 196, 460, 196] / 1 |
| plain/restore | old-dark-Hsva-main-default -> old-dark-Hsva-main-reset | 14 | 13 / [234, 176, 244, 188] / 14 | 1 / [460, 196, 460, 196] / 1 |
| plain/restore | old-dark-Rgba-main-default -> old-dark-Rgba-main-reset | 14 | 13 / [234, 176, 244, 188] / 14 | 1 / [460, 196, 460, 196] / 1 |
| plain/aa | new-light-Hex-main-default -> new-light-Hex-aa-default | 1 | 0 | 1 / [460, 195, 460, 195] / 1 |
| plain/aa | new-light-Hex-main-reset -> new-light-Hex-aa-reset | 1 | 0 | 1 / [460, 195, 460, 195] / 1 |
| plain/restore | new-light-Hex-main-default -> new-light-Hex-main-reset | 2 | 2 / [234, 175, 238, 179] / 2 | 0 |
| plain/restore | new-light-Hsva-main-default -> new-light-Hsva-main-reset | 2 | 2 / [234, 175, 238, 179] / 2 | 0 |
| plain/restore | new-light-Rgba-main-default -> new-light-Rgba-main-reset | 2 | 2 / [234, 175, 238, 179] / 2 | 0 |
| plain/aa | new-dark-Hex-main-reset -> new-dark-Hex-aa-reset | 3 | 2 / [240, 178, 240, 185] / 14 | 1 / [460, 196, 460, 196] / 1 |
| plain/restore | new-dark-Hex-main-default -> new-dark-Hex-main-reset | 11 | 11 / [234, 176, 244, 188] / 2 | 0 |
| plain/restore | new-dark-Hsva-main-default -> new-dark-Hsva-main-reset | 11 | 11 / [234, 176, 244, 188] / 2 | 0 |
| plain/restore | new-dark-Rgba-main-default -> new-dark-Rgba-main-reset | 11 | 11 / [234, 176, 244, 188] / 2 | 0 |
| plain/mixed-target-and-noise | old-light-Hex-main-blue -> new-light-Hex-main-blue | 2 | 0 | 2 / [456, 199, 456, 204] / 1 |
| plain/before-after | old-light-Hsva-main-reset -> new-light-Hsva-main-reset | 1 | 0 | 1 / [460, 195, 460, 195] / 1 |
| plain/before-after | old-light-Rgba-main-reset -> new-light-Rgba-main-reset | 1 | 0 | 1 / [460, 195, 460, 195] / 1 |
| plain/before-after | old-dark-Hex-main-reset -> new-dark-Hex-main-reset | 3 | 2 / [240, 178, 240, 185] / 14 | 1 / [460, 196, 460, 196] / 1 |
| plain/before-after | old-dark-Hsva-main-reset -> new-dark-Hsva-main-reset | 3 | 2 / [240, 178, 240, 185] / 14 | 1 / [460, 196, 460, 196] / 1 |
| plain/before-after | old-dark-Rgba-main-reset -> new-dark-Rgba-main-reset | 3 | 2 / [240, 178, 240, 185] / 14 | 1 / [460, 196, 460, 196] / 1 |

The mixed plain Light HEX Blue row lists only the two diagnosed edge pixels, while its full-frame count stays 342: (456,199) RGBA(7,7,248,255)->(6,6,247,255), (456,204) RGBA(7,7,248,255)->(6,6,248,255). The 340 input-text pixels remain part of the expected change above.

Review authority: [visual verification](../../../engineering/development/theming/verification.md), lines 47 and 50. No automatic baseline replacement or blanket approval of other platforms/scenes.
