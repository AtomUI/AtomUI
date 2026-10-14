# 控件注册分期与按需创建

状态：BootstrapRegistration 已实现；测量、测试及发布验证范围见[实施记录](../../superpowers/progress/2026-10-14-bootstrap-registration.md)。

## 1. 目标与决策

普通 Rider / dotnet run 与发布应用都需要缩短首帧前的注册工作。完整注册基线把普通 JIT 首 Render 从
849.92 ms 增至 986.47 ms（同一临时骨架程序、中位数）；Ready 基本相当。两项 cache getter 实验均未消除这项成本。
采用源码 `[BootstrapRegistration]` 声明首阶段需要的少量控件/资源，让源生成器输出阶段事实。
应用继续使用 `UseDesktopControls()` 和既有 `Deferred`，无需枚举或手工控件名称配置。

主题字典与控件快照仍按实际使用物化；Token 派生算法仍在运行时执行。本模块只调节注册片段的执行阶段。

## 2. 分期契约

- `AtomUI.Registration.BootstrapRegistrationAttribute` 标注本包控件或导出主题的资源类，非继承、无参数。没有标记的包维持完整注册；有标记的包仅把对应片段作为 bootstrap。无贡献的标记是构建错误，避免静默无效。
- 复用现有资产关系补齐同包 Token owner、语义 owner 及不可分割字典的导出目标。C# 创建的 chrome helper 必须就地标记；不分析应用调用图。跨包 owner 仍由实际 registry 验证。
- 隐藏生成 ABI `ControlRegistrationFragmentAttribute.IsBootstrap` 为 virtual bool，默认 true，保持既有片段行为；支持分期的生成片段覆写为编译期常量。net10 非裁剪路径统一经过 `AddFragment`，net8 的 `CollectFull` 精确 IL 模板和 TypeMap 条件不变。标记不增加裁剪根。
- 初始 collection 在片段 Add 前保留非 bootstrap 片段；不会触碰其 descriptor 或资源工厂。完成初始 collection 时发布不可变包快照，pending 只属于该包。
- 通过 `Build(staged: false)` 同步补齐；staged 则在 Stage 2 成功封存前补齐。补齐复制已选元数据并执行一次 pending 片段，替换原包槽位，不追加第二份包或 segment 注册表。package/provider/commit ordinal 不变，服务与 complete callback 不重跑。
- partial 包的 SchemaFingerprint 为 null；仅完整非裁剪包使用原生成指纹，裁剪路径继续按实际贡献计算。
- 同一 merge 路径按旧/新不可变包快照的差集注入新增 descriptor；资源按现有完整排序键归并，并沿用原子 theme/language evolution。补齐失败使 builder 永久失败；取消及所有终态清空 pending，旧可见主题保持有效。
- 只允许在初始配置保留 pending；第二阶段新注册的包完整收集。晚期写入依然拒绝。

## 3. 维护与复杂度

标记写在声明旁，使用真实 Symbol 身份，支持 public/internal/nested 类。纯 Avalonia 主题目标通过本地资源类声明，
不修改第三方类型。修改 chrome 的 C# 组合关系时检查相应内部控件；已有资产 owner/export 关系在生成期闭合，
以队列与已访问集合保证每个片段/资产最多处理一次。此闭合只选择阶段，不新增片段或 linker 保留边。

无独立主题、Token 或 Semantic 内容的帮助类型不需要标记；错误标记由 ATOMUIREG010 阻止。
默认普通包完整注册；支持分期的包必须提供完整可用的 bootstrap 表面，不能在 Stage 1 使用未收集控件。
`Deferred` 是应用承诺延后业务内容的边界；初始化成功、失败、取消沿用 IAtomUIStartup。

## 4. 资源与生命周期

partial 和 full 共用既有不可变 descriptor，provider 与 package ordinal 稳定；两阶段按同一
(Phase, Order, AssetId) 顺序挂载，已物化资源不重新构造。跨包重复、owner 身份与 schema 验证保持原强度。
未知资源键保持有序回退，不能因静态索引缺失而被排除。

pending collection 只被配置 builder 持有，成功补齐前取走 continuation；失败禁止再次使用 builder，
关闭注册窗口清空所有 continuation。ApplicationScope 的各终态清除 hooks/actions，不建立新的静态缓存或 owner 订阅。
准备阶段不发布有效资源，仍由 StageTwoEvolutionPlan 原子交换主题与语言；关闭/取消后禁止提交。

## 5. 验证要求

- 临时测试证明非 bootstrap 工厂未执行，以及同步/分期终态等价、完整指纹恢复、重复片段去重、服务仅一次。
- 覆盖资源类、第三方 StyledElement、多目标资产、Token owner、无效标记和非继承语义。
- net8 固定 collector 模板、net10 TypeMap、NativeAOT 与 Browser 选中片段不增加裁剪根。
- 同程序普通 JIT 对比同步段、首 Render、Ready；渲染回调不是物理显示器 presentation 证据。
- macOS 红绿灯、应用名、主菜单与主题切换需真实窗口验证。仅构建或测试通过不能替代 UX 证据。
