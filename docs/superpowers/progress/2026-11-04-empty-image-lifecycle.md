# Empty 图片生命周期修复记录

计划：[2026-11-04-empty-image-lifecycle](../plans/2026-11-04-empty-image-lifecycle.md)。

## 范围与依据

- 用户授权：按已更新 Empty 文档实施修复并完成对应模块验证；不包含创建分支/worktree、提交或全量回归。
- 原始触发：同一控件实例经 `Content=null` 后设回，预设和描述恢复而 SVG Source 为空、Bounds 为 0×0。
- 既有评估：Empty Default/Simple、ListBox、ListView、TreeView、CascaderView、DataGrid 共 7 场景已证实同源；本次恢复临时驱动取得修复前后证据。
- 源码审计：图片输入和 `_svg` 归 `AbstractEmpty`，颜色绑定在 ControlTheme，无 C# relay binding；无需 API/Token/模板变化或新增订阅。
- 执行调整：用户已明确开始实施，直接执行已批准方案；沿用当前 checkout，不运行技能默认的分支/提交/隐藏目录脚本。

## 状态

- 失败复现：完成，21 项中 18 失败、3 通过，7 个原始场景全部失败。
- 共享基类修复：完成，生产改动仅 `AbstractEmpty.cs`。
- UX/来源/主题边界：7 个原始场景与 15 个边界场景通过，Skia 截图已查看。
- 清理与最终模块验证：完成，模块 23/23、DataGrid 消费检查 3/3 通过，均无失败或跳过。
- 独立审查：完成，Critical 0、Important 0、Minor 0。

## 验收范围

- AC-01 原始 7 场景：相同窗口、尺寸、空数据源与控件实例，Content 摘挂后图案、描述、Footer、布局及模板目标身份保持。
- AC-02 直接风险：资源暂缺保留结果、恢复后正常；自定义 Source/Path、离树变更和主动清空按当前输入同步。
- AC-03 提交与模板：相同输出不产生 Source=null 中间态；重套模板后新 part 正确，旧 part 不再被更新。
- AC-04 渲染：真实 Skia 同场景前后采样与查看，图片显示和布局与数值结论一致；Headless/Skia不等同于 Windows 原生后端证据。

## 执行证据

- 红：计划临时命令，修复前原始源码，21 项发现、18 失败、3 通过，7.5033 秒。Source 由 753（Simple）或 2066（Default）变为 null，图形 Bounds 归零；图像与文字布局发生差异。幂等检查观察到 Source 变化 2 次；离树来源冲突未被校验。
- 首轮绿：相同驱动，21 项中 20 通过；7 个原始场景 Source、Bounds、part 身份和 RGBA 前后比较通过。剩余主题场景只改 Avalonia RequestedThemeVariant，未切换 AtomUI 颜色算法，按下条修正驱动。
- 执行调整：主题验收改为真实 `IThemeManager.ApplyThemeAsync` + Dark 算法，完成后恢复默认主题；不改产品主题实现。原失败证据仍由同一摘挂轨迹的 7 场景承担。
- 边界绿：同一临时命令增加 filter `FullyQualifiedName!~Original_Same_Instance_Reattach`，15/15 通过，2.9175 秒；包含真实 ComboBox 弹层关闭后重开，不禁用动效。
- 成员布局：`python3 .agents/skills/atomui-control-optimization/scripts/check-member-layout.py --path src/AtomUI.Controls/Empty/AbstractEmpty.cs`，修改前后均为 1 文件、1 控件、0 问题。无 API/模板/绑定/字段新增；继承生命周期 override 复用私有同步方法。
- 原始场景最终绿：计划临时命令加 `--no-build`、filter `FullyQualifiedName~Original_Same_Instance_Reattach`，7/7 通过，6.2770 秒。每项重复摘挂两次，Source 长度保持 753/2066、Bounds 与 part 身份保持。
- Skia 验收：420×260 实际帧，7 个场景的修复后首次帧及重挂帧，相对修复前正常首次帧的 RGBA 差异均为 0；修复前摘挂帧差异分别为 3079（Default）、4478（Simple/DataGrid）、1058（ListBox/ListView/TreeView）、1863（CascaderView）。逐一查看修复后截图，插画、描述、列表表面与尺寸保持。A/A 为零差异，独立 1 DIP 间距扰动被检出，恢复后回到零差异。未缩放、裁剪或遮罩对照区域。
- AOT/生命周期判断：仅现有属性读取、字符串计算及继承生命周期调用；未引入反射、动态绑定、注册或资源订阅变化。模块计划无专项义务，不额外发布 NativeAOT；Windows 原生后端未在本机验证。

## 清理与最终验证

- 测试处置：本次 22 项临时检查（7 个原始场景 + 15 个边界）均删除，不新增常驻用例；现有 Empty Semantic Part、来源切换、默认空态及消费契约测试继续保留。临时驱动、反射观测 helper 与 SVG 输入全部清理。
- 清理：DataGrid 测试宿主逐字节还原；媒体清理返回 removed=true、files_removed=30、Git audit clean、findings=[]。本任务没有长运行进程。本次仅删除 `Temporary/Issue529`，不改其他任务原有的 Temporary 内容。
- 最终 Empty 模块：`python3 scripts/verification/test.py run --scope iterate --path src/AtomUI.Controls/Empty --path src/AtomUI.Desktop.Controls/Empty`，清理后发现并通过 4 类、23/23 项，测试 4.335 秒，总计 27.902 秒，状态 `iteration-passed`，无 gap 或剩余专项义务。范围为 EmptySemanticPartTests、EmptyIndicatorContractTests、AutoCompleteEmptyIndicatorTests、SelectEmptyIndicatorTests。
- 最终 DataGrid 消费检查：`dotnet test tests/AtomUI.Desktop.Controls.DataGrid.Tests/AtomUI.Desktop.Controls.DataGrid.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~DataGridEmptyTitleFocusTests`，还原宿主并重新编译后 3/3 通过，0 失败、0 跳过，测试约 1 秒。
- 文档与差异：正式文档 27 处相对链接有效；`git diff --check` 通过。仅本次生产文件 `AbstractEmpty.cs` 有代码变更，无公开属性、Token、AXAML 或 Gallery 用法变更；未创建分支/worktree、未提交。
- 结论范围：原始同实例摘挂、已列来源/资源/模板边界及 Headless/Skia 验收完成。未运行全量回归、Windows 原生宿主或 NativeAOT 发布；这些不在本次模块修复的验证范围。

## 独立审查裁定

- Ruling: 保持原有五个内部颜色属性的实色画刷契约，不扩展为渐变画刷；该强转在原实现已存在，本次不扩大输入类型。若该内部契约另有变化，应单独评估。
- Ruling: 相同结果不重写 renderer 符合文档和真实像素证据；本次原始摘挂不需要强制重解析，相反做法会恢复重复重置成本。
- Ruling: 首次挂树即执行来源校验符合批准的同步入口；无 part 时停止投影，由模板接入完成绘制，不放宽来源互斥约束。
- Ruling: 保持模板清除至新模板接入之间原有的 `_svg` 引用方式，不加入额外生命周期状态；新模板接入后替换引用，验证旧 part 不再更新。没有新增持有链。
