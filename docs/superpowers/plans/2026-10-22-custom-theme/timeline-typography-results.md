# CT-15 P2 Timeline 排版与 Global 消费结果

2026-10-24，在现有 feature/custom-theme worktree、P1 清理后源码上继续，单 agent、不创建分支/worktree、不提交、不用 CUA 或全量测试。P1 结果见 [部件与默认输入](timeline-consumers-results.md)，整体状态见 [tasks](tasks.md)。

## 实现边界

- Timeline／TimelineItem 分别使用本 identity 的 FontFamily、FontSize、ColorText 明确存在位与有效值；缺省保留真实 InheritanceParent 字体和前景。只读内部接口桥接受保护父对象，最终 Setter 均在 AXAML。没有新 public 属性、Own、ID、字体预设或时钟。
- Core 的字号选择新增无预设的三输入形态，原有 8／9／12 输入保持；新增有限 IBrush 继承源和三输入前景选择，复用已有订阅生命周期，不反射。
- 根已有 Background／BorderBrush 改读 Timeline Global，section IndicatorSpacing 读公开 TimelineItem Global；不自动增加边框或圆角。Item 的节点掩膜仍消费自身 ColorBgContainer，root 背景不会复制到 Item。仅改 root 背景的反例中可见掩膜异色，配方若需一致，应使用共同 Global 或分别配置两个 owner。
- 明确 public Item 本地字体／前景和生成的 TimelineItemTitleStyle／TimelineItemContentStyle 保持原生优先级；FontWeight／FontStyle 原生继承保持。未配置时旧暗色文字仍为黑色，本包不自动改变该默认外观，不宣称修复暗色可读性。

## 失败与消费者检查

临时测试位于 Temporary/CT15Typography，AXAML 只使用生成专用 Style，检查关键 FontFamily／FontSize／Foreground Setter 及不存在 Name／Loaded／Unloaded 回退；Part 查找只读。真实公开控件的有效 RED 为 **18 项：7 FAIL／11 正对照 PASS，956ms**，覆盖字体、文字色、配置清除、重挂、公开 owner 独立性、frame 和 section spacing。

最初透明色断言将 Colors.Transparent（ARGB00FFFFFF）当作明确 #00000000，两个 fixture 误判在有效 RED 前已修正；缺少 TimelineMode using、候选局部变量重名 CS0136 也不记作产品失败。增强实际几何断言后，隐藏 pending dot 的 Bounds 正确为0，按可见形态断言；一项既有源码契约从 Shared 更新为 TimelineItemTokenResource，不降低检查。初次合跑 **136 PASS，3s**；补充测量／滚动 extent 两项后，最终 **138 PASS，3s**（20 临时 + 52 Timeline + 66 OTP），无新增永久 case。

11 Own 在两方向 × 三模式实际覆盖：dot 12+2 的14方盒／半径7／描边2、rail宽3及Lime、icon24与轴槽24、73末项、普通7／pending19的padding，以及 Start9／End11／Middle13 margin。两方向同时检查所有数值0、Thickness0与精确 #00000000，未替换为默认；缺省清除恢复。另验证长寿命 parent/provider 保留时，移除的 root／item／title／content 四个 WeakReference 释放。

## 渲染与配置恢复

真实 Skia、UseHeadlessDrawing=false、正常 motion，1400×760／scale1；6根／22项，两方向三模式、pending/reverse与真实 Action 点击。root Height=300 保留，原生父字体分别为 $Default12/Black、Arial19/Navy、Courier17/Brown。A→B→A 不重建22项，点击到达一次。pending 使用产品支持的静态 PathIcon 分支，默认循环已在 P1 单独观察。

| 对照 | 实际结果 |
| --- | --- |
| 同版本 A/A及所有恢复 | 0 RGBA像素差异 |
| 仅移1 DIP检错 | Light28,951／Dark28,959像素；恢复0 |
| 默认、原有 public 字体21/Arial/Orange、专用 title27/content29 Style、恢复和输入 | 20张旧新均0 |
| 新 Global 配置（字体Menlo23、文字#801080、框体、间距11） | 两主题各780,551像素改变；实际根／项／frame／spacing均有属性断言 |
| 新 Global 加public Item本地字体 | Light778,134／Dark778,580像素；本地21/Arial/Orange优先 |

已查看旧 public 定制及新暗色配置关键画面。没有遮罩、容差、移动对齐或移除尺寸／pending／内容来通过比较。大字在原宽度内自然换行，不声明任意字号下无裁剪。新配置的变化与原有配置的兼容对照分别记录。

## Padding 测量残留的根因修复

追加 Own 全字段置零再清除时，最初 JIT／AOT 的亮暗结果彼此完全一致，却各有866像素没有回到原画面。差异只在 Vertical Start 的中间图标项与后续项：Padding 已恢复20，DesiredHeight仍为19（原39），后一项 Y=58（原78）。这不能按原生噪声或容差放过。

独立 public Padding 反例为1 FAIL／432ms，Padding清零后实际39而期望19。固定root高度45的另一个反例确认旧 extent仍58，原生应夹到viewport45（内容38）。确切旧Common源码重放两项 **2 FAIL，459ms**。单变量 Skia 探针只重新测量那个图标项，866像素立即变0，22项完整几何文本逐字恢复，证明缺失测量失效声明。

AbstractTimelineItem 在自己的 MeasureOverride 消费 Padding，现将 PaddingProperty 加入既有 AffectsMeasure 声明；不在资源回调手动重排或重建页面。五个公共常量的纯布局移动有逐块SHA与非空白行证明，访问级别／名字／值不变。最终driver删除手动失效探针，仅通过正常配置更新，亮暗／JIT／AOT四次 Own 恢复均0像素。固定45高度的extent与可见滚动条清零／恢复检查通过，root bounds及viewport高度保持。

## NativeAOT、清理与最终验证

当前 Mac osx-arm64 NativeAOT实际启动，**32张与JIT完整RGBA均0**（24张排版／Global＋8张全部Own／0／清除），11Own实际dot／icon／rail／模式margin／pending／末项断言通过。最终源码改变后重新publish并执行；无IL警告，五条已知Homebrew dylib最低OS linker警告单列。Own额外矩阵证明JIT/AOT及清除恢复，不冒充其旧新全字段像素前证。原有20张默认／公开定制兼容对照在测量修复后仍全部0。

已删除20项临时case所在2文件、外部driver428文件、6份确切源码备份所在JSON及118份媒体／几何临时文本／manifest；evidence cleanup的Git媒体审计无发现。保留的几何文本只有描述控件状态的数值证据，没有图像编码。净增永久case为0，仅更新一项既有源码契约的owner资源断言。

清理后Core显式资源13项 **PASS，167ms**；Desktop Timeline／OTP **118 PASS，3s**。NET8 Desktop（覆盖Core／Common）Release **23.05s，0警告／0错误**；6文件／2控件成员审计0问题。文档链接、目录键集和最终diff检查见本包文本记录；仅作者自审，不冒充独立审查。上述仍为当前Mac Headless/Skia＋NativeAOT，不代表原生最终显示面或其他OS。

## 剩余边界

P2覆盖不等于完整 Timeline 家族：Compact／RTL、全部业务和持续动效、极端字体／几何、原生最终显示面及其他平台仍待后续；CT15其他展示／反馈控件继续。作者自审为当前单 agent 的审查范围，不冒充独立审查。
