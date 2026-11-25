# PrimeNG FloatButton 阴影对齐官方：修复记录

日期：2026-11-17。分支 `fix/primeng-floatbutton-shadow`，worktree `/Users/chinboy/Projects/dotnet/AtomUIV6-fix-primeng-floatbutton-shadow`（用户当日在对话中授权创建）。基线 `release/6.0` @ `2af948083`。用户随后指令：PrimeNG 后续优化都在该分支与 worktree 进行（本文件同时承载第 2、3 项工作）。

## 用户反馈

PrimeNG Aura 下 FloatButton 阴影与官方不符（Gallery FloatButton 页 Basic 示例的白色圆钮带一块本不该有的阴影，用户截图红框标注）。

## 根因

FloatButton 族的 `BoxShadow` 在 AXAML 中直接绑定全局 `BoxShadowsSecondary`（官方语义 = `overlay.select/popover/navigation` 面板级阴影 md 双层），既没有控件自有 token 槽位，`AtomUI.Themes.PrimeNG` 也没有 FloatButton 的 token 覆盖。而官方 `@primeuix/themes` 3.1.1（npm 发布包 `dist/{aura,lara,nora}`）中**按钮族一律无 resting 阴影**：`button` 全部 severity 无阴影（仅 raised 变体有 `raisedShadow`），`speeddial`（FAB 对应物）没有任何 shadow token；阴影只属于 overlay.{select,popover,modal,navigation}、card、message、toast。

## 官方对比矩阵（@primeuix/themes 3.1.1 逐字核对）

| 表面 | 官方值 | 修复前 AtomUI PrimeNG | 修复后 |
|---|---|---|---|
| select/popover/菜单/tooltip 面板 | `0 4px 6px -1px rgba(0,0,0,.1), 0 2px 4px -2px rgba(0,0,0,.1)` (md) | 同值（全局 Secondary） | 不变 ✅ |
| dialog/drawer 模态 | `0 20px 25px -5px .1, 0 8px 10px -6px .1` (xl) | 同值（全局 BoxShadows） | 不变 ✅ |
| Card | `0 1px 3px 0 .1, 0 1px 2px -1px .1` (sm) | 同值（RestingShadows） | 不变 ✅ |
| FloatButton/BackTop/Group 容器 | 无（button/speeddial 无阴影） | md（错） | 无 ✅ |
| Message 卡 | 按严重级别 `0px 4px 8px 0px` 彩色 4% 阴影 | xl 黑（全局 BoxShadows） | **未修，待另立任务** |
| Notification 卡 | toast：normal=md，severity=彩色 `0px 4px 8px 0px` 4% | xl 黑（全局 BoxShadows） | **未修，待另立任务** |

Message/Notification 的官方阴影按 severity 着色（info=blue.500、success=green.500、warn=yellow.500、error=red.500、secondary=surface.500、contrast=surface.950，经 `color-mix(..., transparent 96%)` ≈ 4% 不透明度），需要 per-severity token 管线，超出本次修复范围，建议另立任务。

## 修复内容

1. `FloatButtonToken` 新增自有槽位 `FloatButtonShadows`（类型 `BoxShadows`），Ant 基线默认值 = `EffectiveGlobalToken.BoxShadowsSecondary`——Ant Design 渲染逐值不变。
2. 四处 ControlTheme Setter 改绑 `{atom:FloatButtonTokenResource FloatButtonShadows}`（绑定位置保持在 ControlTheme，符合主题绑定优先约束）：
   - `AbstractFloatButtonTheme.axaml`（独立态 + 嵌入圆钮两处；覆盖 FloatButton 与 BackTopFloatButton）
   - `FloatButtonGroupTheme.axaml`（方形容器）
   - `FloatButtonGroupHostTheme.axaml`（宿主方形容器）
3. 新增 `Algorithms/Controls/PrimeNGFloatButtonTokens.cs`，六个算法（Aura/Aura.Dark/Lara/Lara.Dark/Nora/Nora.Dark）`EvaluateOwn` 写 `FloatButtonShadows = default`。
4. README「Aura control stages」表补 FloatButton 行。

## 验证（TDD）

- 临时测试 `tests/AtomUI.Themes.Tests/Temporary/PrimengFloatButtonShadow/`：三组断言先全部验证 RED（Ant 基线缺槽位、12 个 PrimeNG profile 阴影非空、四个主题文件仍绑 `BoxShadowsSecondary`），实现后 14/14 GREEN；随后按测试生命周期规范删除。
- `tests/AtomUI.Themes.Tests` 全项目 337/337 通过（含既有 PrimeNG 契约测试）。
- 聚焦验证：`test.py run --scope iterate --path src/AtomUI.Themes.PrimeNG` 与 `--path src/AtomUI.Desktop.Controls/FloatButton`，结果见交付说明。

## 台账（执行期裁定）

- Task 1/2 边界缺陷：生成器 ATOMUIGEN042 要求 own token 必须被 AXAML 消费，属性与改绑无法分两步编译。裁定：先写全 RED 断言（含绑定边界源码检查）再一并实现，TDD 顺序不变。
- 计划中的 commit 步骤按仓库提交审查强约束暂停：改动留在 worktree 待用户审查，提交信息草案已备。
- 效果契约测试更新：`ButtonFamilyEffectTests.FloatButton` 的 `s_floatButtonOwnerReads` 删去 `BoxShadowsSecondary` 一行（共享主题不再读该全局 token）；`FloatButtonShadows` 的渲染效果覆盖由消费图自动生成的 own-token 理论行承担（harness 按归属身份校验，跨身份运行会被 EnsureMatches 拒绝，BackTop/Group/Host 无法挂同一 own token 的跨身份行）。

## 终审与收尾

- 终审（独立子代理，2026-11-17）：七项重点全核对通过，结论「可交付」。唯一 Important 项为控件文档未按 token.md §5 自身规则同步，已补：`docs/controls/desktop/general/float-button/token.md`（分类表 + §3 阴影来源段落）与 `changelog.md`（2026-11-17 条目）。Minor 项（PrimeNG 无阴影契约无常驻测试锁定）按测试净零政策接受，记录知悉。
- 预置失败（与本次改动无关，在干净 release/6.0 上原样复现）：`AtomUI.Desktop.Controls.Rendering.Tests` 中 `NavigationFamilyEffectTests` 的 Menu/NavMenu/MenuFlyoutPresenter 共 8 行（`BoxShadowsSecondary`/`MenuPopupShadows`/`MenuPopupBorderColor`/`ColorBorderSecondary`）与 `StepsProgressFamilyEffectTests.ProgressBar_Token_Has_Effect(InnerLabelEmptyOpacity)`。
- 最终验证：`AtomUI.Themes.Tests` 323/323；`AtomUI.Desktop.Controls.Tests --filter ~FloatButton` 57/57；`ButtonFamilyTokenIsolation` 5/5；Rendering.Tests `ButtonFamilyEffectTests` 490/490；`git diff --check` 干净。`test.py run --scope iterate` 对主题包模块被预置策略缺口拦截（干净主线同样被拦），已按 AGENTS 回退到精确 filter 验证。

---

## 第 2 项：默认浮动按钮不可见回归（用户 2026-11-17 反馈，方案 A 已获批准）

### 反馈与根因

第 1 项把浮动按钮阴影置空后，用户真机看到 Basic 示例的白色圆钮在浅色卡片上"完全看不到圆的轮廓"。
根因不是阴影本身，而是**漏配的事实**：官方 PrimeNG 不存在"白底、无描边、无阴影"的按钮变体——官方按钮只有实心填充
与 outlined/text 两类形态，中性按钮 = `button.secondary`（`surface.100` 亮 / `surface.800` 暗，无阴影无描边）。
Ant Design 的白底 FAB 靠阴影浮起，阴影一去就与白卡片融为一体。

### 方案（已获用户批准）

默认（Default）类型的表面与前景改由 `FloatButtonToken` 自有槽位表达，Ant 值不变、PrimeNG 取官方 secondary：

| 槽位 | Ant 默认 | PrimeNG（亮 / 暗） | 官方来源 |
|---|---|---|---|
| `DefaultBg` | `ColorBgElevated` | surface 100 / 800 | `button.secondary.background` |
| `DefaultHoverBg` | `ColorBgTextHover` | surface 200 / 700 | `button.secondary.hoverBackground` |
| `DefaultActiveBg` | 透明（Ant 按下无反应） | surface 300 / 600 | `button.secondary.activeBackground` |
| `DefaultIconColor` | `ColorText` | surface 600 / 300 | `button.secondary.color` |

### 实现中的两处根因修正（非方案变更）

1. **死读清理**：基座 `<Setter Property="Foreground" Value="{atom:OwnerTokenResource ColorText}" />` 被 `ButtonType` 的
   Default / Primary 两个分支完全遮蔽，属死读；`AbstractFloatButtonTheme` 已删除该 setter，效果契约的漂移表同步移除
   `ColorBgElevated` / `ColorBgTextHover` / `ColorText`。
2. **Ant 按下回归自查**：首版把 `DefaultActiveBg` 的 Ant 默认写成 hover 值，会让 Ant 白钮按下出现底色（原本无反应），
   已改为透明。

### 家族身份边界（已知限制，非本次引入）

FloatButton 族只有 `FloatButtonToken` 一个真实 token 类，BackTop / Group / Host 主题经 `FloatButtonTokenResource`
跨身份读该组槽位（既有设计，`FloatButtonSize` / `FloatOffsetX` 早已如此）。效果测试的 override 按身份下发，因此
跨身份读无法在 BackTop 侧被扰动验证——`ButtonFamilyEffectTests` 的漂移表只覆盖 `OwnerTokenResource` 读，这一点保持原样。

## 第 3 项：Message / Notification 卡阴影对齐官方

- `MessageCardToken`：`Information` / `Success` / `Warning` / `Error` / `Loading`Shadows 五个 severity 槽位；
  `MessageType` 每个取值都有样式块，基础的 `BoxShadows` 读会被完全遮蔽，故不设无 severity 兜底槽并移除基础 setter。
- `NotificationCardToken`：`RestingShadows`（`NotificationType.Default`）+ 四个 severity 槽位。
- PrimeNG 值（官方 `@primeuix/themes` 3.1.1 `message` / `toast`，明暗同值）：
  severity = `0px 4px 8px 0px` 的 blue / green / yellow / red 500 档 @4%（alpha `0x0A`）；
  `RestingShadows` = `overlay.popover.shadow`（Aura / Nora 双层 md，Lara `0 1 3 0`）；Message 的 `Loading` 取 neutral。
  官方 `secondary` / `contrast`（surface.500 / 950）在 AtomUI 枚举中无对应项，不映射。
- `FeedbackFamilyEffectTests` 的 severity 映射扩展：阴影槽位按各自的 severity 安排渲染状态（`RestingShadows` 用
  `NotificationType.Default`）。
- 已知差异：`WindowMessageManager` 折叠背板仍读 `BoxShadowsTertiary`（Ant 装饰结构，不参与官方 message/toast 映射）。

## 本轮回看

- 临时测试：`Temporary/PrimengFloatButtonSurface/`（14 项）与 `Temporary/PrimengMessageShadow/`（14 项），先 RED 后 GREEN，
  验证后删除，全局测试净增 0。
- 效果契约：`ButtonFamilyEffectTests` 491/491、`FeedbackFamilyEffectTests` 270/270。
- 聚焦回归：`AtomUI.Themes.Tests` 全项目通过；`AtomUI.Desktop.Controls.Tests` 的 FloatButton / Message / Notification 通过。

### 第 2 项补充：方形胶囊容器填充（用户 2026-11-17 第二轮反馈）

用户反馈 FloatButton Group 的方形胶囊容器"背景色被搞没有了"。同一根因的漏配：容器填充仍读全局 `ColorBgElevated`
（PrimeNG 下为白色），而第 2 项已把容器的阴影去掉，白底在白卡片上不可见。

修法沿用方案 A：`FloatButtonItemsControlTheme` 的 `[Shape=Square]` 填充改绑 `FloatButtonTokenResource DefaultBg`
（Ant 仍等于 `ColorBgElevated`，渲染不变）；`ButtonFamilyEffectTests` 的 `FloatButtonGroup` owner 读表同步移除
`ColorBgElevated`。分隔线仍用 `ColorSplit`（官方没有合体容器，无对应取值，保持不变）。

临时测试：`Temporary/PrimengFloatButtonGroupSurface/`（6 项）先 RED 后 GREEN，验证后删除。

---

## 第 4 项：SplitButton 实心形态接缝竖线（用户 2026-11-17 第三轮反馈）

### 证据

官方 `@primeuix/styles@3.1.1`（`dist/splitbutton`，三预设相同）根本不画接缝：

```css
.p-splitbutton-button.p-button { border-inline-end: 0 none; }
.p-splitbutton-button.p-button:not(:disabled):hover, ...:active { border-inline-end: 0 none; }
```

拼接处由右半档自己的 `button.<severity>.borderColor` 承担，filled 下该值等于填充色 → 无可见线。
`@primeuix/themes@3.1.1` 的 `splitbutton` 预设也确实只有 `root`（`borderRadius` / `raisedShadow` / `roundedBorderRadius`），
没有任何 separator token。

AtomUI 由 `SplitButton.Render` 在两支按钮的间隙自绘 `SplitSeparatorBrush`；主题对 `IsPrimaryButtonType` 取
`ColorPrimaryHover` / `ColorErrorHover`（注释写明是 Ant 的 solid 组合规则），因此缝比填充浅一档，成为截图里那条亮竖线。

### 修法

1. `SplitButtonToken` 新增 `SolidSeparatorColor` / `DangerSeparatorColor`，Ant 默认 = 全局 `ColorPrimaryHover` /
   `ColorErrorHover`（渲染不变）。
2. `SplitButtonTheme.axaml` 两个 Setter 改绑自有槽位。
3. 新增 `PrimeNGSplitButtonTokens`（六算法）：两槽位取 `ColorPrimary` / `ColorError`，缝色 = 填充色 → 不可见。
   **不置 null**：null 会让页底色透过 1px 间隙，反而出现白线；取同色可保持几何不变（无位移、无重排）。
4. 效果契约：`ButtonFamilyEffectTests.SplitButtonSetup` 新增两个新槽位 case；`ColorPrimaryHover` / `ColorErrorHover`
   因 `SplitButtonToken` 仍以代码读取（Ant 默认值来源）而保留为全局行，共用同一 primary/danger 布置。
5. Outline（Default）形态原本就无接缝线，未改动。

### 已知限制

hover 时主按钮底色变为 hover 档（如 primary.600），1px 缝仍取填充色（500），会短暂出现一道极细的边界——
官方在该处本就存在「hover 填充 vs 右侧按钮边框」的自然分界，故不额外处理。

临时测试：`Temporary/PrimengSplitButtonSeparator/`（14 项）先 RED 后 GREEN，验证后删除。

---

## 第 5 项：ButtonSpinner / NumericUpDown 手柄（用户 2026-11-17 第四轮反馈，全部拍板项均做）

### 证据

官方 `@primeuix/themes@3.1.1` `inputnumber.button`：`background: transparent`、`hoverBackground: surface.100/800`、
`activeBackground: surface.200/700`、`color: surface.400`、`hoverColor: surface.500/300`、`activeColor: surface.600/200`、
`borderColor: form.field.border.color`、`width: 2.25rem`。官方 `@primeuix/styles` 的 stacked 变体：
`.p-inputnumber-button { border: 0 none }`（无分隔线），按钮组 `inset 1px / height: calc(100% - 2px)`（填色不压边框）。

### 两个叠加缺陷

1. **几何**：手柄内两个 IconButton 铺满，按下/悬停底色盖住输入框 1px 边框；`SpinnerHandleClip` 的 `ClipToBounds`
   只按矩形裁剪（Avalonia 不按 CornerRadius 裁剪），圆角被切平。像素级放大可确认（灰色块溢过边框、边线中断）。
2. **颜色**：PrimeNG 无 ButtonSpinner 覆盖，沿用 Ant（按下 `ColorFillAlter`、图标 `ColorPrimary`/`ColorPrimaryActive`、
   分隔线 `ColorBorder`、宽度 `ControlHeightSM`）。

### 修法（用户批准：几何 + 颜色 + NumericUpDown + 宽度全部做）

- 几何（所有语言，属缺陷修复）：`ButtonSpinnerHandle.HandleContentMargin` 按 `SpinnerBorderThickness` 在**外侧**内缩
  （内侧保持齐平以贴住分隔线），模板 `PART_ButtonsLayout` 的 Margin 绑定它。
- 槽位（`AbstractButtonSpinnerToken`，ButtonSpinner 与 NumericUpDown 共享）：新增 `HandleHoverBg`（Ant 默认透明）、
  `HandleIconColor`（Ant 默认全局 `ColorIcon`）、`HandlePressedColor`（Ant 默认 `ColorPrimaryActive`，原为主题常量）；
  两个宿主主题改绑。
- 新增 `PrimeNGButtonSpinnerTokens` / `PrimeNGNumericUpDownTokens`（各六算法，共用一个 static 映射）：上述官方取值 +
  分隔线透明 + `HandleWidth 36`。
- 效果契约：`InputFamilyEffectTests` 增补 `HandleHoverBg`（:pointerover）、`HandlePressedColor`（:pressed）、
  `HandleIconColor`/`ColorIcon`（静息，比较范围取手柄本体）。

### 验证

临时测试：`Temporary/PrimengButtonSpinnerHandle/`（14 项，token 取值）+ `Temporary/PrimengButtonSpinnerGeometry/`
（2 项，左右两种位置的 Margin 内缩），先 RED 后 GREEN，验证后删除。

### 第 5 项补充：图标尺寸与禁用态（用户 2026-11-17 第五轮反馈）

- **图标偏小**：官方 `@primeuix/styles` `.p-inputnumber-button .p-icon { font-size/width/height: form.field.font.size }`
  = 0.875rem（14）；Ant Design 的 `handleFontSize` 是 `fontSize / 2`（14 → 7）。AtomUI 原先只用 Ant 值，PrimeNG 下只有
  官方的一半。修法：`PrimeNGSpinnerHandle.Apply` 写 `HandleIconSize = 14`，Ant 保持 `fontSize / 2`（与 antd 完全一致）。
  已知差异：官方 sm/lg 分别 0.75rem / 1rem，AtomUI 手柄图标不随 SizeType 变化，暂未拆分。
- **禁用态隐藏手柄**：`ButtonSpinnerDecoratedBox.UpdateHandleVisualState` 在 `!IsEffectivelyEnabled` 时把
  `HandleOpacity` 置 0，整块淡出。改为保持 `IsShowHandle ? 1 : 0`（Ant Design 禁用时 handler 仍可见并变灰），
  禁用外观由手柄 `:disabled` 样式承担。常驻契约测试
  `NumericUpDownHandleTests.Disabled_NumericUpDown_Hides_Floatable_Spinner_Handle` 按新契约更名为
  `..._Keeps_The_Floatable_Spinner_Handle` 并断言 opacity 为 1；效果测试里 NumericUpDown 的手柄可见性前置断言同步改为恒为 1。

临时测试：`Temporary/PrimengSpinnerIconSize/`（5 项）、`Temporary/PrimengSpinnerDisabledHandle/`（2 项）先 RED 后 GREEN，验证后删除。

### 第 5 项补充二：图标尺寸回调

真机看后用户判断 14 偏大，按产品口径缩小 1/4 → `HandleIconSize = 10.5`（官方 `form.field.font.size` 14 的 3/4，
仍大于 Ant 的 `fontSize / 2`）。README 与两个控件 changelog 同步注明这是有意的产品偏差。

再一轮真机反馈：10.5 仍偏大，再缩 1/4 → `HandleIconSize = 7.875`（14 → 10.5 → 7.875，连续两次四分之一）。

---

## 第 6 项：ComboBox 下拉间距（用户 2026-11-17 第六轮反馈）

### 诊断（先量再说）

用渲染测试 fixture（`VisualLayerManager` + `ScopeAwareOverlayLayerPanel`）实测打开状态：

| 项 | 官方 PrimeNG | 修复前 AtomUI | 修复后 |
|---|---|---|---|
| 输入框底边 → 面板顶边 | `anchor.gutter` 2 | 2（已经正确） | 2 |
| 面板顶边 → 首项 | `list.padding` 4 | 4（正确） | 4 |
| 选项行高 | 14×1.5 + 2×4 = **29** | **35**（= `ControlHeight`） | **29** |
| 选项横向内边距 | 0.625rem = 10 | 7（`UniformlyPaddingSM`） | 10 |

所以红框看到的「空带偏大」根因是**行高比官方高 6px**，导致首项文字在行内垂直居中后整体下移 3px，整列也更松散；
外间距本身是对的。

### 修法

- `ComboBoxToken` 新增自有槽位 `ItemHeight`（Ant 默认 = 全局 `ControlHeight`），`ComboBoxTheme` 的 `ItemHeight`
  setter 由直读 `ControlHeight` 改绑该槽位。
- 新增 `PrimeNGComboBoxTokens`（六个算法）：`ItemHeight = 29`、`ItemPadding = (10, 0)`（官方固定 rem 值；
  PrimeUIX 的列表行不随控件尺寸变化）。
- 效果契约 `SelectFamilyEffectTests` 增加 `ItemHeight` 的布置（打开下拉、6 项），验证行高在列表项上可见。

临时测试：`Temporary/PrimengComboBoxItemHeight/`（5 项）先 RED 后 GREEN；诊断用的 `ComboGapDiag` fixture 测量完即删除。
