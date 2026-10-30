# ColorPicker 语义化改造设计

- 日期：2026-09-03
- 状态：已批准（2026-09-03 会话确认）
- 分支：`feature/semantic-colorpicker`（自 `feature/semantic` @ ee07bbf5c 创建）
- 参考源码：本地 `/Users/chinboy/Projects/dotnet/ReferenceProjects/ant-design`（antd 6.4.5），按 reference-project-source-guidelines 第 2 级 `ReferenceProjects` 命中，未使用 GitHub 来源

## 1. 目标

为 `ColorPicker` 与 `GradientColorPicker` 提供 Semantic Part 公共定制契约，部件名与 antd 6.4.5 官方语义 API
（`ColorPickerSemanticType`，`components/color-picker/interface.ts:57-73`）严格对齐；Gallery 例子精确复刻 antd
对应示例，不自行发挥。

先例：Cascader 语义化改造（commit fd5cbd97a），部件名与 antd `CascaderSemanticType`（继承
`SelectSemanticType`）逐一同名对齐。

## 2. Semantic Part 清单

| Part | antd 元素 | AtomUI 落点 | ContractType | Cardinality | 备注 |
| --- | --- | --- | --- | --- | --- |
| `root` | 触发器容器 | owner 自身 | — | Root | 隐式部件，不标 marker |
| `body` | `-color-block` 色块容器 | 触发模板 `PART_ColorIndicator`（ColorBlock 节点） | `ColorBlock` | Single | 先例：Cascader `item` 用具体 public 类型 `Tag` |
| `content` | `-color-block-inner` 色块颜色元素 | ColorBlock 模板 `PART_ColorPreview` | `Border` | Single | SelectorRoute 跨模板 |
| `description` | `-trigger-text` 触发器文本 | ColorPicker: `PART_ColorText`；GradientColorPicker: `PART_ColorTextPanel` | `TextBlock` / `Panel` | Single | 两控件触发文本区结构不同，各自声明 |
| `popup.root` | 弹层根容器 | View 模板根 `Border #Frame` | `Border` | Single | `CrossVisualRoot=true`、`RuntimeCreated=true`（View 由 `CreatePresenter()` 动态创建） |

- 声明文件：`ColorPicker.SemanticParts.cs`、`GradientColorPicker.SemanticParts.cs`（新文件）。
- `Since = "6.2.0"`。
- 跨模板 route 沿用 Cascader scope 语法；若 `SemanticPartTemplateValidator` 需扩展（跨两个 `/template/`），
  按 Cascader 提交对 validator 的扩展方式处理。

## 3. Theme marker 落点

| 文件 | 节点 | marker |
| --- | --- | --- |
| `Themes/ColorPickerTheme.axaml` | `PART_ColorIndicator` | `semantic-body` |
| `Themes/ColorPickerTheme.axaml` | `PART_ColorText` | `semantic-description` |
| `Themes/GradientColorPickerTheme.axaml` | `PART_ColorIndicator` | `semantic-body` |
| `Themes/GradientColorPickerTheme.axaml` | `PART_ColorTextPanel` | `semantic-description` |
| `Themes/ColorBlockTheme.axaml` | `PART_ColorPreview` | `semantic-content` |
| `Themes/ColorView/ColorPickerViewTheme.axaml` | 根 `Border #Frame` | `semantic-popup-root` |
| `Themes/ColorView/GradientColorPickerViewTheme.axaml` | 根 `Border #Frame` | `semantic-popup-root` |

弹层内部滑杆、色谱、输入区、预设色板不加 semantic class（antd 官方语义 API 无这些部件）。

## 4. Gallery（精确复刻）

1. `GalleryShowCaseHost.SemanticPartsContentTemplate` + `SemanticPartPreview`：预览打开状态 ColorPicker
   （钉住弹层、箭头、showText），五个 `SemanticPartDescription` 文案逐字复刻 antd
   `components/color-picker/demo/_semantic.tsx` 的 cn/en 描述，翻译 zh-TW/pt-BR。
2. 新 showcase item「自定义语义结构的样式」复刻 antd `components/color-picker/demo/style-class.tsx`：
   - 实例一：`DefaultValue=#1677ff`、`IsArrowVisible=False`、root 圆角 = BorderRadius token、
     `popup.root` 白色 1px 边框（#FFFFFF）；
   - 实例二：`DefaultValue=#722ed1`、`SizeType=Large`、`IsArrowVisible=False`、`popup.root` 紫色 1px 边框
     （#722ed1，对应 antd styles 函数 size=large 分支），以两个业务 class 分组实现；
   - 新增 typed part style `ColorPickerPopupRootStyle`（SetterTargetType=Border，静态注册，AOT 安全）。
3. 本地化：`ColorPickerShowCaseLangResourceKind` 新增 `Semantic*Description` ×5 与示例标题/描述 key，
   四语言 xlf 同步。

## 5. 文档

- 新增 `docs/controls/desktop/data-entry/color-picker/semantic-part.md`（Part/Selector/ContractType/
  Cardinality/AtomUI Node/Responsibility/Related API/Related Token/Customization/Stability 十列表 + 定制示例）。
- 更新 color-picker `overview.md`、`implementation.md`、`changelog.md`。
- `docs/architecture/systems/theming/semantic-parts.md` 登记 ColorPicker。
- 视觉验收步骤文档 `docs/superpowers/specs/2026-09-03-colorpicker-semantic-visual-acceptance.md`
  （操作路径、预期现象、判定标准；截图由用户提供，未回传前一律标「待视觉验收」）。

## 6. 测试与验证

- 新增 `ColorPickerSemanticPartTests.cs`：descriptor 完整性（5 部件）、Selector 命中（含 OverlayPopupHost
  弹层路径）、GradientColorPicker 契约、Semantic Style 覆盖 TemplateBinding 优先级边界。
- 同步生成器 manifest 快照、`ColorPickerShowCaseExamples.snapshot`、本地化 baseline
  （`CatalogMemberOrder.baseline`、`GalleryCatalogCoverageTests`）。
- 命令：`dotnet test tests/AtomUI.Desktop.Controls.Tests/... --framework net10.0` 与
  `dotnet test tests/AtomUIGallery.Tests/... --framework net10.0`。
- 真机视觉验收按全局约束执行：agent 产出步骤，用户回传截图，未回传不得宣称通过。

## 7. 范围排除

- 不新增 children 自定义触发器、panelRender、pure-panel API 与 demo。
- 不改 Token；不动现有 12 个 demo 行为。
- 不为 antd 语义清单之外的内部结构（slider/palette/input 等）添加 semantic class。

## 8. As-built 修订（2026-09-03）

实现过程中生成器校验与运行时机制暴露了四处与本设计原文的差异，均已按最小改动落定并通过测试与审查。
原文保留在上文，以本节为准：

1. **`body` ContractType：`ColorBlock` → `Control`**（§2 表）。生成器诊断 ATOMUIGEN022 要求
   ContractType 为公共类型，而 `ColorBlock` 是 internal；为不扩大公共 API 面，公共契约承诺放宽为
   `Control`（selector 身份由 `.semantic-body` class 承载，定制面不变）。
2. **`content` 声明追加 `CrossNestedOwners = true`**（§2 表）。`content` 的 marker 位于 ColorBlock
   自身模板（`ColorBlockTheme.axaml` 的 `PART_ColorPreview`）而非 owner 模板，验证器要求跨嵌套模板
   边界的部件显式声明，与 Cascader `clear`（marker 在共享 `SelectHandle` 模板内）先例一致。
3. **`description` ContractType 经别名 `AvaloniaTextBlock` 声明**。`AtomUI.Desktop.Controls` 命名空间
   存在自有 `TextBlock` 类型遮蔽 Avalonia 同名类型（生成器诊断 ATOMUIGEN026），声明文件以
   `using AvaloniaTextBlock = Avalonia.Controls.TextBlock` 消歧；公共契约仍为 Avalonia `TextBlock`。
4. **`popup.root` 落点改为 owner 模板内的透明 `Border` 包裹层**（§2 表原「View 模板根 `Border #Frame`」
   + `RuntimeCreated=true` 作废）。弹层 View（ColorPickerView/GradientColorPickerView）由
   `CreatePresenter()` 在弹层打开时动态创建，其模板节点的 `TemplatedParent` 链断在 View 上，owner 的
   `/template/` route 与 `>>` 视觉步进在 overlay 弹层（`ShouldUseOverlayPopup=true`）下均不可达动态
   内容。owner 模板内 `PART_Popup` 下的静态包裹 `Border`（包 `ArrowDecoratedBox`）经 TemplatedParent
   传播保持可达，结构上等价 antd 的 popover 根（官方 `popup.root` 即挂在 popover 根上），默认零视觉
   影响；不再声明 `RuntimeCreated`。
   **同日修订（弹层箭头回归修复）**：该 `Border` 的落点从「包裹 `ArrowDecoratedBox`」调整为
   「`ArrowDecoratedBox` 的内容层（面板根容器，箭头在 Border 之外）」。原因：共享 Popup 的箭头布局
   偏移与阴影遮罩机制只识别直接 Child 为 `IArrowAwareShadowMaskInfoProvider`（`Popup.cs:621`），
   `Border` 直接作为 `PART_Popup` 的 Child 会顶替 `ArrowDecoratedBox`，导致箭头渲染与弹层定位失效；
   修正后 `PART_Popup` 的直接 Child 恢复为 `ArrowDecoratedBox`，透明 `Border` 下沉为其内容层，
   TemplatedParent 可达性与零视觉影响的结论不变。
   **二次修订（边框贴合弹层外沿）**：真机复核发现内容层落点使语义边框受 `ArrowDecoratedBox` 的
   `ContentPadding` 挤压、外留一圈白边，与 antd popover root 贴合弹层最外沿的形态不符。最终落点统一为
   「`PART_Popup` 直接子节点 `ColorPickerPopupRootFrame`（`Border` 子类，向内容层 `ArrowDecoratedBox`
   转发 `IArrowAwareShadowMaskInfoProvider`），语义边框贴合弹层外沿（对齐 antd popover root）」：
   `ColorPickerPopupRootFrame` 作为 `PART_Popup` 的直接 Child 满足箭头/阴影机制的接口识别约定，
   箭头与阴影经转发完整保留，语义边框回到弹层外沿。
   **三次修订（移除 IsOpen 模板绑定，借鉴 AutoComplete 代码驱动弹层）**：2026-09-04 第二次真机反馈
   钉住弹层仍向上翻转且遮罩残留，真根因为两模板 `PART_Popup` 的
   `IsOpen="{TemplateBinding IsPickerOpen, Mode=TwoWay}"` 在模板充气阶段即求值——Gallery 预览在
   模板应用前已置位钉住（`IsPickerOpen=true`），弹层在锚点未布局、light-dismiss 抑制未执行的充气
   时刻被打开，placement 翻转且遮罩已创建。修复：两模板删除该绑定，弹层打开/关闭全部由
   `AbstractColorPicker` 代码驱动（属性变更 true → presenter 就绪后 `_popup.IsOpen=true`；false →
   `_popup.IsOpen=false`；`PopupClosed` 对 light dismiss/Esc 等外部关闭回写 `IsPickerOpen=false`；
   `OnApplyTemplate` 尾部按 `AbstractAutoComplete` 的 `OpeningDropDown` 思路对「宿主已开而弹层未开」
   补开），对齐 `AbstractAutoComplete` 的代码驱动先例；新增
   `Picker_Popup_Templates_Must_Not_Bind_IsOpen` 模板回归测试防复发。
4. **最终复审（恢复既有 pinned coercion，删除补偿状态）**：第三次真机反馈最初被误归因于
   `CoerceIsPickerOpen`，并临时改成 `_ignorePropertyChange` 驱动的 `true -> false -> true` 回填。最终的
   动画竞态证据表明该判断不成立：ColorPicker 的业务 open state 原本就应在 pinned 期间保持稳定，只有
   `_popupLifecycleCloseDepth` 范围内允许关闭。最终恢复 `CoerceIsPickerOpen`，删除补偿 flag 和瞬态状态；
   `OnAttachedToVisualTree` 继续负责 detach/re-attach 后恢复业务状态，代码驱动的物理 Popup 打开和
   `OnApplyTemplate` 尾部补开保持不变。
5. **共享 Popup 动画竞态修复**：首次进入 Semantic Parts 时，懒创建的 Popup host 可能先触发
   `Popup.Opened`，随后才挂载 `PopupMotionActor`。actor 在 attach 时为防闪烁先把 `Opacity` 设为 0，旧逻辑
   又只在 `Opened` handler 中启动开启动画，导致弹层物理上 `IsOpen=true`、视觉上却持续透明；第二次进入
   因 host/actor 已就绪而正常。`Popup.NotifyMotionActorReady` 现在在当前打开周期已开始时进入同一
   `StartOpenMotion` 路径；`Closed` 清理打开周期与 actor，dispatcher 回调捕获具体 actor 和 token，避免快速
   开关时读取已替换或清空的字段。新增 late-actor 单元回归和首次真实页签点击的 Gallery 生命周期回归。
6. **执行顺序**：theme marker（原 §3）先于 `SemanticPart` 声明落地——生成器诊断 ATOMUIGEN025 要求
   声明编译期即能在模板中找到 marker，声明先行会导致整个工程编译失败。
7. **新增公共 API**：`AbstractColorPicker.IsPopupPinnedOpen`（含属性字段）由 internal 提升为 public
   （对齐 `AbstractSelect.IsPopupPinnedOpen` 的公共形态；Gallery 语义预览需在无 InternalsVisibleTo 的
   程序集中钉住弹层，且与 antd 的 `open` 类公开 API 对齐）。`IsPickerOpen` 保持 internal。
