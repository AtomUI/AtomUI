# ColorPicker Semantic Part 改造 · 真机视觉验收步骤

> 状态：**待视觉验收**。改造代码、测试与文档已完成（见
> `docs/superpowers/plans/2026-09-03-colorpicker-semantic-part.md` 与
> `docs/superpowers/specs/2026-09-03-colorpicker-semantic-design.md` §8 As-built 修订）。
> 按全局视觉验收约束：截图/录屏由用户按本文步骤回传，未回传前不宣称通过；图像不入库，本文件只保留文字证据。
>
> ## 验收结论记录（文字证据）

| 日期 | 证据 | 结论 | 修复记录 |
| --- | --- | --- | --- |
| 2026-09-04 | 用户回传截图：弹层箭头破碎/丢失（Semantic Parts 预览与受控模式示例两处） | **发现真机回归：`PART_Popup` 直接 Child 被包裹 Border 顶替，破坏 `Popup.cs:621` 的 `Child is IArrowAwareShadowMaskInfoProvider` 约定，箭头/阴影遮罩与定位偏移全部失效**（headless 测试未覆盖此共享原语约定，为盲区） | `semantic-popup-root` Border 下沉为 `ArrowDecoratedBox` 内容层（面板根容器，箭头在 Border 之外），直接 Child 恢复；新增 `Popup_Direct_Child_Remains_The_Arrow_Decorated_Box` Theory 回归测试（先红后绿）；复审通过（fix accepted）。**箭头与定位待用户按步骤 3.2/2.2-2.4 回传截图复核** |
| 2026-09-04 | 用户回传截图：① Semantic Parts 页签钉住弹层向上翻转，盖住页面上方 Examples/Semantic Parts 页签；② 钉住弹层 light-dismiss 遮罩拦截整个窗口点击 | **发现两处真机问题：钉住弹层 placement 错误翻转 + light-dismiss 遮罩未按钉住先例抑制**（`ApplyPopupTriggerSettings` 默认 Click 时把 `OverlayDismissEventPassThrough` 覆写为 false，XAML 补属性无效；钉住打开在 `OnAttachedToVisualTree` 过早执行，placement 依据未就绪的锚点/尺寸计算） | 钉住预览抑制 light-dismiss 遮罩：镜像 `AbstractSelect` 钉住先例，`OnApplyTemplate` 在钉住打开前抑制 `_popup.IsLightDismissEnabled=false`、取消钉住恢复模板默认值（运行时行为测试先红后绿）；钉住打开从 attach 时移至 `OnApplyTemplate` 尾部（relay 绑定与订阅接好之后，属性变更路径不变）；Gallery SemanticPartPreview 的 PreviewContent 外包 `Panel MinHeight="600"` 保证弹层向下展开。**弹层方向与遮罩穿透待用户按步骤 1.1 回传截图复核** |
| 2026-09-04 | 用户反馈（第二次，随 2026-09-04 复核回传）：① Semantic Parts 页签钉住弹层仍向上翻转，盖住页面上方页签；② 遮罩仍拦截窗口交互 | **定位真根因：两模板 `PART_Popup` 的 `IsOpen="{TemplateBinding IsPickerOpen, Mode=TwoWay}"` 在模板充气阶段即求值——Gallery 预览 XAML 声明 `IsPopupPinnedOpen="True"` 使 `IsPickerOpen` 在模板应用前已为 true，弹层在锚点未布局、light-dismiss 抑制未执行时被打开（placement 翻转 + 遮罩创建）**；AutoComplete 无此绑定、弹层由 `OnApplyTemplate` 尾部代码驱动，故其预览正常 | 修复 = 移除两模板 IsOpen 绑定 + 弹层改为代码驱动（`IsPickerOpen` 变更 true → presenter 就绪后 `_popup.IsOpen=true`，false → 关闭；`PopupClosed` 对 light dismiss/Esc 等外部关闭回写 `IsPickerOpen=false`；`OnApplyTemplate` 尾部按 `AbstractAutoComplete` `OpeningDropDown` 思路对「宿主已开而弹层未开」补开；`_popupLifecycleCloseDepth`/`CoerceIsPickerOpen`/`ClosePickerForLifecycle` 语义不变）；新增 `Picker_Popup_Templates_Must_Not_Bind_IsOpen` 模板回归测试（先红后绿）。**弹层方向与遮罩穿透待用户按步骤 1.1 回传截图复核** |
| 2026-09-04 | 用户录屏与真机复核：第一次进入 Semantic Parts 时 ColorPicker 弹层不可见，切走再进入第二次正常；最终修复后用户确认“解决了” | **确认共享 Popup 开启动画的生命周期竞态：`Opened` 早于懒加载 host 中的 `PopupMotionActor` attach，actor 预置 `Opacity=0` 后错过唯一的开启动画入口，故物理 Popup 已打开但视觉透明** | actor 晚就绪时复用统一开启动画路径，关闭时清理打开周期；恢复 ColorPicker 原有 pinned coercion、移除调查阶段的 `_ignorePropertyChange` 补偿；新增 late-actor 单元测试、首次真实页签点击与 pinned hide/show 回归。首次切换已由用户在桌面 Gallery 真机确认正常。 |
| （待用户回传后填写） | | | |

## 背景

本次 ColorPicker 语义部件改造涉及四处可能影响渲染的变更，需要重点确认：

1. `ColorPickerTheme.axaml` 与 `GradientColorPickerTheme.axaml`：触发区 marker（`semantic-body` /
   `semantic-description`）为 inert class；`PART_Popup` 直接子节点 `ColorPickerPopupRootFrame`
   （`Border` 子类，向内容层 `ArrowDecoratedBox` 转发 `IArrowAwareShadowMaskInfoProvider`）承载
   `semantic-popup-root` marker，语义边框贴合弹层外沿（对齐 antd popover root）——**唯一可能引入
   布局漂移的点**，需确认弹层外观、箭头与定位无变化（边框位置修正见结论表 2026-09-04 记录）。
2. `ColorBlockTheme.axaml`：`PART_ColorPreview` 新增 inert marker `semantic-content`，理论零视觉影响。
3. `AbstractColorPicker.IsPopupPinnedOpen` 由 internal 提升为 public：纯可见性变更，无行为变化
   （置 true 钉住并自动打开弹层）。
4. `ColorPickerShowCase.axaml` 宿主从 `GalleryStickyTabsHost` 迁移到 `GalleryShowCaseHost` 并新增
   Semantic Parts 页签与「自定义语义结构的样式」示例（示例内容为新增，既有 12 个 demo 未改动）。

ColorPicker / GradientColorPicker 公开的语义部件：`root`、`body`、`content`、`description`、
`popup.root`（与 Ant Design 官方语义 API 对齐）。

## 操作路径与判定标准

准备：运行 AtomUIGallery（`controlgallery/AtomUIGallery.Desktop`），进入 Data Entry（数据录入）→
ColorPicker 页面。主题浅色（默认）。

### 步骤 1：Semantic Parts 页签

| # | 操作 | 预期现象 | 截图 |
| --- | --- | --- | --- |
| 1.1 | 进入 "Semantic Parts" 页签（首次进入才加载） | 出现 ColorPicker Preview，取色器为 #1677ff、显示文本、弹层钉住常开（含箭头与完整面板：色谱、滑杆、HEX 输入）；Part 列表显示 5 个 Part 与本地化职责描述 | 1 张全景 |
| 1.2 | Hover/Pin `root` | 高亮罩住整个触发器（含边框与文本） | 1 张 |
| 1.3 | Hover/Pin `body` | 高亮罩住触发器内色块容器（#1677ff 色块整体） | 1 张 |
| 1.4 | Hover/Pin `content` | 高亮罩住色块内部颜色填充区域（比 body 小一圈） | 1 张 |
| 1.5 | Hover/Pin `description` | 高亮紧贴触发器文本（#1677FF 字样），不覆盖色块 | 1 张 |
| 1.6 | Hover/Pin `popup.root` | 高亮罩住弹层根容器（含箭头区域的整个浮层） | 1 张 |

### 步骤 2：自定义语义结构的样式示例（复刻 antd style-class）

| # | 操作 | 预期现象 | 截图 |
| --- | --- | --- | --- |
| 2.1 | 找到 "自定义语义结构的样式" 示例（Examples 面板最后一个 item） | 两个取色器：上 #1677FF（Middle）、下 #722ED1（Large，整体更大），均无下拉箭头，圆角为默认 BorderRadius | 1 张（未展开态） |
| 2.2 | 点击上方取色器展开弹层 | 弹层四周出现 **白色 1px 边框**（#FFFFFF，在阴影与浅色背景下呈亮边）；箭头不显示；面板功能正常 | 1 张（展开态） |
| 2.3 | 关闭后点击下方取色器展开弹层 | 弹层四周出现 **紫色 1px 边框**（#722ED1）；其余同上 | 1 张（展开态） |
| 2.4 | 对照同页其他取色器（如 Basic 示例）展开弹层 | 未样式化的弹层无边框、有箭头——确认语义样式未泄漏到其他实例 | 1 张 |

### 步骤 3：基线回归抽查

| # | 操作 | 预期现象 | 截图 |
| --- | --- | --- | --- |
| 3.1 | Basic / Disabled / ClearColor 三个示例逐一看触发器外观 | 与 6.1.7 基线一致（禁用态透明度、清除斜线红杠不变） | 1 张全景 |
| 3.2 | Basic 取色器展开弹层再关闭 | 弹层外观、箭头、定位、开合动画与基线一致（弹层包裹 Border 不得引入漂移）；**GradientColorPicker 示例（LineGradient / ValueBinding）同样抽查展开一次** | 1 张 |

## 判定标准（量化）

- 1.1 弹层钉住：页面无遮罩阻挡、弹层持续显示不自动关闭。
- 2.2/2.3 边框：像素取样边框色分别 ≈ #FFFFFF / #722ED1，宽度 1 逻辑 px。
- 2.1 圆角：与同页未样式化 Middle/Large 取色器目测一致（BorderRadius token）。
- 3.x 回归：与 6.1.7 基线对比无布局漂移（弹层位置、尺寸、箭头方向）。

## 已知边界

- headless 测试（`ColorPickerSemanticPartTests` 6/6、Gallery 556/556）只证明契约与 Selector 命中，
  不证明真机渲染正确；本文步骤是唯一视觉证据来源。
- 验收截图不入库；本文件仅记录文字结论与问题清单。
