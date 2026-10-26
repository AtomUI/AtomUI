# CT14 N3-B：Breadcrumb 默认函数迁移

沿用[容器 N3](container-consumers.md)及[既定 V2 默认求值设计](own-defaults.md)，在当前 checkout／feature/custom-theme 单 agent 执行。当前基线为 ff0d3d572；不创建分支或 worktree，不提交或调度新的 Actions。本片为已有 Global-only 迁移方案的机械接入，不增加新能力或改变可观察合同。

## 合同与范围

仅改 BreadcrumbToken.cs 的八项默认声明，接入 DependencyDefaultsV2／Revision1／IControlTokenDefaults。ItemColor、LinkColor、SeparatorColor 读取 ColorTextDescription；LastItemColor、LinkHoverColor 读取 ColorText；LinkHoverBgColor 读取 ColorBgTextHover；SeparatorMargin=(UniformlyMarginXXS,0)；BreadcrumbItemContentPadding 四边读取 UniformlyPaddingXXS。没有 Own→Own 边，保留名称、类型、key、枚举数值、slot、资源投射及所有明确 Own／0／撤回行为。

六项消费者在原 ControlTheme，两个 separator 字段由 BreadcrumbSeparatorManager 给运行时兄弟 presenter 绑定。保持条目、分隔符 N−1、公开导航、hover、尺寸、布局、默认动效、动态集合及生命周期；不改 manager、模板、绑定或 Part。API 等级 L0，无公开签名／默认值／事件／交互／视觉变化。正式 Token 文档中的 IconSize 不是该 Own 字段，须纠正其与 Shared 图标尺寸的边界。

## 执行与出口

- [x] 复用原 Breadcrumb 精确模块作基线；在所属 Temporary/CT14BreadcrumbDefaults 中记录明暗八项实际资源、类型、key 数值及消费者。V2 合同先失败，原消费兼容用例通过，二者分开报告。
- [x] 仅迁移八个静态默认函数；同一临时合同通过，默认与原版逐项相等；Global、Control Global、八项明确 Own、0、撤回、真实 hover 与同实例重挂保持。
- [x] 实际产品 Window／明暗同历史默认、hover、配置、恢复作自然 A/A、1 DIP 检错及新旧比较；媒体置于仓库外。Mac managed／NativeAOT 检查实际资源及保持状态，不扩充为原生最终像素结论。
- [x] 删除临时源、外部工程、驱动和媒体；最终只跑 Breadcrumb 精确 filter、NET8、Token 文件成员审计。同步正式 Token／changelog、CSV、任务状态和作者另步审查；Git 另行取得具体快照批准。

N2 其他 Global／字体、完整 Breadcrumb 状态／平台、N4 与 CT14 仍开放。任何新发现的行为变化先另行审查，不以本片扩大 NavMenu 首次展开待批方案。

本机机制切片结果见[八项默认迁移结果](container-breadcrumb-defaults-results.md)；上述勾选不代表完整家族验收或 Git 授权。
