# CT19 C8 ColorSpectrum Components Implementation Plan

> For agentic workers: use `superpowers:executing-plans` inline. 用户要求由主 agent 独立完成，不委派 agent；既有 checkout 满足研发位置约束。

**Goal:** 使两类公开 View 的六种色谱配置与第三滑块一致，同时保持当前默认画面。

**Architecture:** 默认元数据采用 SV；两套 ControlTheme 用 TemplateBinding 连接 Spectrum，再用模板内显式 CompiledBinding 连接已有 ThirdComponent。除非临时反例证明必要，不改色谱算法、滑块逻辑或生命周期。

**Tech Stack:** Avalonia AXAML compiled bindings、.NET 10 Headless/Skia 临时检查、.NET 8 Release、真实 Mac managed／NativeAOT。

**Spec:** [批准的短设计](color-spectrum-components.md)

## Global Constraints

- 不创建分支／worktree，不自动提交，不发布 NuGet；第十四轮 CI 使用已批准的 3e7efb341 提交，C8 不混入该轮。
- 新验证源写在 `tests/AtomUI.Desktop.Controls.Tests/Temporary/CT19SpectrumComponents/`，证明 before/after 后删除；临时驱动和媒体在外部系统临时目录，审查后清理。
- 不改键盘合同，不给测试直接设置语义节点外观；保留原几何、动效、滑轨和全部输入区。
- 清理后只运行 ColorPicker 精确模块检查和 NET8 Release；不自行执行本地全量／change scope。

## Review Focus

1. 默认枚举与默认像素不一致：任务 1 的默认值 RED、任务 3 的旧新默认与恢复整图比较。
2. 六模式或其中一种 View 漏接线：任务 1、2 的双 View × 六模式及剩余分量矩阵。
3. 模式切换使 RangeBase 强制值反向改写 HSV：任务 2 的完整 HSV／Alpha、事件计数、往返和实际指针检查。
4. 编译绑定未命名命中或依赖反射：任务 2 的真实模板检查与任务 3 NativeAOT 构建／运行。
5. 模板替换残留旧订阅或新部件失联：任务 2 的模板重建、离树修改／重挂、旧节点释放检查。

## Task 1 — 建立临时 RED

- [x] 读取两套模板、AbstractColorPickerView、ColorSlider 与现有 ColorPicker fixture；保存修改前 SHA。
- [x] 临时检查两种 View 默认 SV；六种显式模式命中实际 Spectrum 和第三滑块，并保持当前颜色。
- [x] 运行 `dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj -c Release --no-restore --filter FullyQualifiedName~CT19SpectrumComponents`，分别记录编译准备错误和有效断言失败。

## Task 2 — 最小接线与邻接合同

- [x] 修改 `ColorView/AbstractColorPickerView.cs` 的默认值。
- [x] 修改 `Themes/ColorView/{ColorPickerViewTheme,GradientColorPickerViewTheme}.axaml`：Components TemplateBinding、第三滑块显式 CompiledBinding。
- [x] 同一 RED 转 GREEN；扩大六模式往返、完整 HSV／Alpha、实际指针、模板重建和离树恢复检查。若失败，先定位单变量原因，不添加兜底同步旗标。
- [x] 检查绑定生命周期和实际编译产物；审查无新增 API／Token／PART／反射绑定。

## Task 3 — 可复现视觉与真实宿主

- [x] 写明同轨迹验收合同后，使用产品 Window 和真实 ThemeManager 明暗配置；默认、六模式、恢复，独立 A/A 与 1 DIP 对照，精确整图比较。
- [x] 保留既有布局和输入，真实框架指针验证各轴及剩余滑块；Mac managed／NativeAOT 验证同合同。
- [x] 对明确预期视觉变化提供可审查证据；任何新合成噪声逐项定位并审查，不能继承旧表容差。

## Task 4 — 清理、文档与最终模块验证

- [x] 更新 `docs/controls/desktop/data-entry/color-picker/{implementation,changelog}.md`、C8 结果与 tasks 状态，明确默认值兼容变化和键盘边界。
- [x] 删除临时测试／驱动／工程及专属产物；媒体按审查状态清理，保留文本、SHA和真实失败计数。
- [x] 清理后执行 `dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj -c Release --no-restore --filter FullyQualifiedName~ColorPicker`；执行 ColorPicker 项目 NET8 Release 构建、成员审计和 `git diff --check`。
- [x] 作者按五项 Review Focus 自审。仅报告 C8 实证出口；完整 CT19／CT25 继续，Git 另需具体快照批准。

计划自审：短设计每项均有实现位置和检查；两个模板共享现有属性合同，无新增抽象或公开类型。默认变更已获用户批准，不重复询问；本计划不扩大该批准到 Git 或新的视觉差异。

当前出口：功能源已删除，清理后27项、NET8 Release零警告和成员审计通过；30对自然合成限定差异已提交用户审查，外部驱动／工程及媒体暂留供该审查和可能的补充定位。未提交Git，不关闭完整家族。

审查后出口：用户已接受限定30对；242份媒体及433个外部驱动文件已清理并通过媒体审计。冻结第十五批已获批提交393d718b3并推送，实际跨平台验证运行40188740837；不关闭完整家族。
