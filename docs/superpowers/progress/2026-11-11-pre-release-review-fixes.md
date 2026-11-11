# 发版前评审问题修复记录

2026-11-11，分支 `bugfix/6.2.14-pre-release-review`（基于 `release/6.0` 及当时未提交的工作区）。逐项处理
[发版前改动评审记录](2026-11-11-pre-release-review.md) 中的缺陷：先评估，再用临时用例复现，修复后转绿。临时用例已全部删除，
永久测试净新增 0。

## 已修复

| 问题 | 结论 | 修复 | 证据 |
| --- | --- | --- | --- |
| ProgressBar 换模板重复挂载内容 | 复现 | `OnApplyTemplate` 先释放旧呈现器的内容与模板 | 修复前 `InvalidOperationException`，修复后新呈现器为唯一视觉父级 |
| ProgressBar 清除旧模板可见性值 | 确认 | 以可释放的 Animation 优先级隐藏标签与图标，清除内容或换模板时释放；共享层保留代码隐藏（Mobile 模板不依赖 Desktop 主题） | 自定义模板的本地 `IsVisible=false` 与绑定在套模板、设清内容后保留；内置 labelinner 竖排隐藏正确。首版改为主题伪类的方案使共享层 `ProgressInfoTests` 4 项失败，已放弃 |
| ProgressBar 自定义信息区尺寸过期 | 复现（线形、Steps、Circle） | internal `ProgressInfoPresenter` 在期望尺寸变化时使宿主失效；线形/Steps 测量时刷新信息区 | 修复前信息区停在旧宽度 20，修复后 50 且轨道不重叠，Circle 重新居中 |
| CountBadge 初始隐藏漏挂目标 | 复现；DotBadge 同构 | 目标挂载/替换独立于指示器可见性 | 两种初始隐藏路径与 DotBadge A/B：去掉修复即失败 |
| TreeView 扩展选区发出中间空值 | 复现 | 只同步差量，`AvaloniaList` 批量增删 | 事件序列由清空+逐项变为 `(1, 0)` / `(0, 1)` |
| MarqueeLabel 悬停时无法恢复 | 复现（3 个场景） | 启动统一检查溢出、挂树、门控与悬停暂停 | 恢复、重排保持暂停、延时保持暂停均转绿 |
| AsyncImage 自适应布局停在 Idle | 复现 | 自动尺寸零轴按位置判定；尺寸变化重判；检查有效可见性；`IsLazy`+`Idle` 显示占位 | 只设宽度的懒加载图片加载；占位撑开 24 高；显式零尺寸仍不加载 |
| Cover 解码欠采样 | 真实 Skia 复现 32×15 | 按单轴解码前检查 Skia 截断后的另一轴，不足则换轴 | Skia 探针 `DecodeToHeight(16)` 得 32×16；轴选择表驱动用例 |
| Avatar 累积属性帧 | 复现 | 每个派生值持有一个可替换句柄，条件退出时释放；避免首次 Size 重复配置 | 50 次改尺寸后帧数不再增长 |
| TreeSelect 截获用户输入框按键 | 复现 | 隧道转发只接收自身 `SelectFilterTextBox` | 附加区输入框 Enter 不再提交；过滤输入仍可导航提交 |
| Skeleton 微光覆盖自定义边框 | 复现 | 微光块按 `BorderThickness` 收缩到内缘 | 块从 (0,0,200,16) 变为 (2,2,196,12) |
| Empty 新入口缺少 Desktop 接线 | 确认 `Image` 设后空白 | Desktop 模板渲染 `Image`（对齐 antd `Empty.image`），同高容器，隐藏 SVG | 设置 `Image` 后可见，清除后 SVG 恢复 |
| Watermark 共享字形重入 | 复现订阅泄漏 | owner/前景版本校验，外层不覆盖内层句柄 | A/B：去掉修复后离树仍跟随主题 |
| Watermark 离树前景约定 | 文档与实现不一致 | 保留实现（离树保留最后主题色），修正文档 | — |
| Spin 自定义动画重入 | 复现 start,start | 启动期间的重启/停止延后重放 | 启停保持配对 |
| Card 清空模板保留 LoadingHost | 复现 | 以 `TemplatedParent` 判断 host 是否仍在模板内 | 清空模板后不再向旧 host 加指示器 |
| Space 首项超宽产生空行 | 复现（水平、垂直） | 只断开已有项的行 | A/B：去掉修复后首项被推下 |
| Dialog 右键激活与焦点分离 | 复现 | 抬升后焦点仍在该 Dialog 外时交给其 Surface；保留“Escape 由最顶层处理”的既有契约 | A/B：去掉修复后焦点停在 B，Escape 作用于 A |

## 评估后未修改

- **TreeView 选中背景遮住活动项提示**：属于新视觉设计；NavMenu 采用同样的“选中时不显示活动态”模式，需要跨控件统一决策并做真实窗口视觉验收。
- **ComboBox 清除动作缺少键盘入口**：Ant Design 的清除图标同样不可聚焦，属于功能增强而非回归。
- **ComboBox 显式 `VirtualizingStackPanel` 绕过离场保护**：按源码确认存在。Avalonia 的自动定位为私有逻辑，控件级只能临时改写公开的 `AutoScrollToSelectedItem`，会干扰调用方绑定；记为已知边界。
- **Spin/MarqueeLabel 全局运动 Token**：Ant Design 的旋转动画不依赖 motion Token，现状一致，无需修改，已写入 Spin 变更记录。
- **Empty 相同值预设写入推迟冲突校验**：已记录的边界，维持。
- **Empty 对象描述的样式入口**：需要新增 Semantic Part 契约，不在缺陷修复范围。
- **兼容性变化**（Skeleton 基类迁移、`SpaceItemsAlignment` 类型身份、`Empty.Description`/`Result.Icon` 签名）：属于发版兼容策略决策，未改动。
- **性能、测试与文档改进项**：不属于缺陷，未处理。
