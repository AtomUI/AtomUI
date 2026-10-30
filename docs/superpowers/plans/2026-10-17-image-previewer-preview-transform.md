# ImagePreviewer Preview Transform Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 重构 ImagePreviewer，使基础适配、缩放、拖拽、旋转、翻转、reset、切图和解码尺寸升级遵守最新预览变换设计，并让 Dialog/OverlayHost 行为一致。

**Architecture:** 以 ImageViewer 为唯一变换 owner，新增内部纯值变换/边界计算模型；宿主只 relay 属性和操作请求。图片基础尺寸同时受视口宽度和视口高度 70% 约束，用户 Scale 独立于基础适配比例。Full 解码目标由基础尺寸、Scale 和 RenderScaling 共同决定。

**Tech Stack:** C# 12、Avalonia 12、xUnit/Shouldly、Avalonia Headless。

**Spec:** `docs/controls/desktop/data-display/image-previewer/preview-transform-design.md`

## Global Constraints

- 保持既有 `ImageMinScale`、`ImageMaxScale`、`ImageScaleStep` 和 `IsImageMovable` 名称及默认值。
- 新增 `IsWheelZoomEnabled=true` 时必须同步 owner、Dialog、OverlayHost、viewer、主题绑定和测试。
- Dialog/OverlayHost 不得保留重复的缩放、旋转、拖拽或边界算法。
- 基础适配使用视口宽度和视口高度 70% 的双轴约束；`Scale=1` 表示基础适配尺寸。
- 临时验证代码默认放 `tests/AtomUI.Desktop.Controls.Tests/Temporary/Issue497/`，验证结束删除。
- 最终只运行 ImagePreviewer 所属 Desktop Controls focused 验证，不运行全量回归。

## Review Focus

- 极宽和极高图片在打开、resize、DPI 改变后仍完整位于 viewer 视口内。
- `ImageMinScale` 动态改变后按钮能力、当前比例和滚轮行为立即同步。
- 鼠标中心缩放调整 X/Y 后，指针下图像位置稳定且边界不泄漏。
- 旋转 90/270 度后边界钳制使用交换后的包围盒。
- Full 解码结果升级或迟到提交不能改变当前变换状态、覆盖新 generation 或释放仍在显示的旧 lease。

## Tasks

### Task 1: 变换纯值模型与失败测试

Files:
- Create: `src/AtomUI.Desktop.Controls/ImagePreviewer/ImagePreviewTransform.cs`
- Create: `tests/AtomUI.Desktop.Controls.Tests/Temporary/Issue497/ImagePreviewTransformTests.cs`

- [x] 定义 `ImagePreviewTransformState`、基础适配计算、缩放比例、指针中心缩放和边界修正的内部纯值 API。
- [x] 先写并运行失败测试，覆盖双轴 fit、极端宽高比、按钮/滚轮边界、旋转包围盒和小图回中。
- [x] 实现最小纯值模型并运行该临时测试。
- [x] 纯值模型与 viewer/宿主分阶段接入。

### Task 2: ImageViewer 接入统一变换状态

Files:
- Modify: `src/AtomUI.Desktop.Controls/ImagePreviewer/ImageViewer.cs`
- Modify: `src/AtomUI.Desktop.Controls/ImagePreviewer/Themes/ImageViewerTheme.axaml`
- Modify: `tests/AtomUI.Desktop.Controls.Tests/ImagePreviewer/ImageViewerVisualStateTests.cs`
- Create/Modify: `tests/AtomUI.Desktop.Controls.Tests/Temporary/Issue497/ImagePreviewerViewerTransformTests.cs`

- [x] 写失败 Headless 测试，证明极宽/极高图片基础尺寸完整适配，并验证 Scale=1、reset、resize 和滚轮开关。
- [x] 将 ImageViewer 的布局改为基础尺寸与用户 RenderTransform 分离。
- [x] 将 pointer wheel、drag、rotate/flip/reset 操作归一到统一变换模型，并保留现有 routed request 兼容入口。
- [x] 新增 `IsWheelZoomEnabled` owner 属性和 viewer relay，修正按钮能力在比例边界变化时立即刷新。
- [x] 运行 viewer focused tests 和临时红绿测试。

### Task 3: Dialog/OverlayHost 去重与宿主一致性

Files:
- Modify: `src/AtomUI.Desktop.Controls/ImagePreviewer/ImagePreviewerDialog.cs`
- Modify: `src/AtomUI.Desktop.Controls/ImagePreviewer/ImagePreviewerOverlayHost.cs`
- Modify: `src/AtomUI.Desktop.Controls/ImagePreviewer/ImagePreviewBaseToolbar.cs`
- Modify: `src/AtomUI.Desktop.Controls/ImagePreviewer/Themes/ImagePreviewFloatToolbarTheme.axaml`
- Modify: `src/AtomUI.Desktop.Controls/ImagePreviewer/Themes/ImagePreviewToolbarTheme.axaml`
- Modify: `tests/AtomUI.Desktop.Controls.Tests/ImagePreviewer/ImagePreviewerTitleTests.cs`
- Create/Modify: `tests/AtomUI.Desktop.Controls.Tests/Temporary/Issue497/ImagePreviewerHostTransformTests.cs`

- [x] 写失败测试，验证 native dialog 与 overlay 对同一操作得到同一变换状态。
- [x] 删除宿主内重复的 HandleScale/Rotate/Flip/fit 计算，仅保留 host lifecycle、current navigation 和 event forwarding。
- [x] 把 fit toggle 对齐为统一 reset/base-fit 语义，保持 Semantic Part 和模板部件稳定。
- [x] 保留既有标题栏、关闭、导航和 resize motion 生命周期，切图按批准设计统一重置。
- [x] 运行 ImagePreviewer focused tests。

### Task 4: 解码尺寸升级与生命周期验证

Files:
- Modify: `src/AtomUI.Desktop.Controls/ImagePreviewer/AbstractImagePreviewer.cs`
- Modify: `src/AtomUI.Desktop.Controls/ImagePreviewer/ImagePreviewEntry.cs`
- Modify: `src/AtomUI.Desktop.Controls/ImagePreviewer/ImagePreviewerDialog.cs`
- Modify: `src/AtomUI.Desktop.Controls/ImagePreviewer/ImagePreviewerOverlayHost.cs`
- Modify: `tests/AtomUI.Desktop.Controls.Tests/ImagePreviewer/ImagePreviewerImageLoadingTests.cs`
- Create/Modify: `tests/AtomUI.Desktop.Controls.Tests/Temporary/Issue497/ImagePreviewerDecodeUpgradeTests.cs`

- [x] 临时失败用例验证 native 自身视口/DPI/Scale 解码目标，并验证旧 lease/generation 保护。
- [x] 让 previewer 公开当前显示所需的 decode target 给 entry 请求路径，按 16 px bucket 幂等升级。
- [x] 在 resize、DPI、scale 变化时请求更合适 bucket，保留当前 transform state 和旧图直到新结果提交。
- [x] 真实 PNG 输入验证统一 ImageLoader 的 DimensionLimitExceeded，未绕过安全预算。
- [x] 加载集成与临时完成用例通过，临时文件已删除。

### Task 5: 清理、文档同步与最终验证

Files:
- Modify: `docs/controls/desktop/data-display/image-previewer/overview.md`
- Modify: `docs/controls/desktop/data-display/image-previewer/implementation.md`
- Modify: `docs/controls/desktop/data-display/image-previewer/preview-transform-design.md`
- Modify: `docs/controls/desktop/data-display/image-previewer/changelog.md`
- Delete: `tests/AtomUI.Desktop.Controls.Tests/Temporary/Issue497/` 专用临时测试及辅助资源

- [x] 删除已完成验证的临时测试和专用产物。
- [x] 检查文档与最终源码契约、主题部件和测试命名一致。
- [x] 运行 `git diff --check`。
- [x] 使用 Desktop Controls ImagePreviewer focused test filter 完成 focused 验证。
- [x] 记录测试处置、清理结果和残余平台/AOT风险；不创建 commit。


## 2026-10-17 接续验收记录

- 续接原聊天后重新核对设计，保留工作区原有 Modal 与其他模块改动；不提交。
- 根因补全：真实 scene 视口、原始尺寸元数据、指针缩放、Canvas 平移、旋转/翻转锚点、Avalonia wheel detent、捏合/双击、reset 与 re-template 生命周期。
- 第一组临时验证 20/20，通过后已删除；其中纯值/Headless/实际图片加载验证均通过真实测试项目发现和执行。
- 独立复核提出 5 项重要边界，6 个失败用例分别复现：current reload 降采样、reset 拖拽未结束、旧图目标跨项提交、re-template 平移丢失、非法 minScale 能力不一致。修正后 6/6，随后删除。
- 最终模块验证：Desktop ImagePreviewer 命名空间 118/118，Gallery 页面与 Catalog 契约 6/6。无净新增常驻测试。
- 决策：现有失败提交契约优先，升级期间保留旧 lease，Failed 提交清旧图并显示错误；无旧帧和失败态并存。
- 决策：按批准设计，切图与 reset 清空所有变换；宿主不再保留标题栏专属旋转/翻转策略。
- 发布检查：最终源码 Desktop NativeAOT osx-arm64 发布成功，脚本输出 NativeAOT output validation passed；arm64 原生进程启动 5 秒无异常并主动结束。ReactiveUI 依赖存在 IL2104/IL3053 聚合警告，本机 Brotli 产物有 deployment-target 链接警告；没有新增本模块 warning。
- Browser managed Release 裁剪发布成功，命令限定 `AtomUITargetFrameworks=net10.0` 以避免传递 restore 请求本机缺失的 wasm-tools-net8；未安装或改动工具链。
- 真实 Chrome 运行最终 Browser Gallery：打开本地 Basic Usage 图片，验证 70% 高度适配、滚轮放大、缩小至 0.1、reset；关闭滚轮后图片区域像素完全一致，工具栏和双击仍可放大；浏览器与本地服务器已关闭。
- `git diff --check` 与文档相对链接检查通过；未运行全量回归、未创建 commit。
