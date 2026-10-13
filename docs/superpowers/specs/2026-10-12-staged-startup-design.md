# AtomUI 两阶段启动设计（Staged Startup）

> 历史设计记录。算法复核后的目标边界见[分级启动演化与提交边界](../../architecture/foundations/staged-startup-evolution.md)，
> 后续任务见[优化计划](../plans/2026-10-12-startup-algorithm-optimization.md)。本页的时序保证、复用与提交假设
> 不能替代上述修订设计；历史耗时不作为当前工作树的验收结果。

日期：2026-10-12
状态：设计对话已获用户批准（含控件粒度分期方向与三条硬约束），spec 待用户审阅
上游材料：`docs/superpowers/progress/2026-10-11-macos-startup-remaining-hotspots.md`；
热点 C spec/提交（4fab9a043）；路线 A Gallery 实测；UseAtomUI 全分段探针数据（2026-10-12）

## 目标与硬约束

**目标**：AtomUI 应用（含 Gallery）主窗体最大程度不被初始化阻塞——**完整 AtomUI chrome 首帧**
（用户无感知，无视觉过渡跳变）在 JIT ~0.9s / AOT ~0.3s 出现，窗口内显示业务可自定义的加载态；
完整初始化在窗口首帧提交并完成原生激活后立即进行。方案通用：框架包与三方包同一机制。

**硬约束（一等验收条件，各配锁定测试）：**

1. **正确性不损失**：任何查询/资源解析结果值不变（差分机械锁定）；无视觉闪烁。
2. **架构不补丁式 + 通用**：两阶段与控件粒度分期是框架一等架构概念——单一编排点、显式契约、
   声明式分期；禁止调用点散点 defer；任何控件包（框架/三方）可用同一机制。
3. **AOT 性能与裁剪正确性不受影响**：生成器演进为**契约兼容的增量变更**（老生成器包二进制兼容回退）；
   net8 ILLink/ILC8 模板门、裁剪 CandidateKeys 分期等价、双 AOT 发布全部门禁。

**决策点（已确认）**：分期 **opt-in 起步**（Gallery 首个采用者，稳定后议转默认）；事件形态
`IAtomUIStartupEvents`（挂 ApplicationScope，普通 C# 事件）；分期粒度 = **控件（fragment）级**，
包无 Stage 元数据时回退整包粒度。

## 实测基线（2026-10-12，Release JIT 终端，中位数）

UseAtomUI 同步路径 **579ms**；空窗出现（路线 A 后）1.37s。关键基石：**热点 C 使主题重编译仅
~43ms（全局 only）**，"registry 演进 + 受控重编译"因此成为可行原语。Desktop 包 Collect 141.6ms
为几百个生成 fragment 的执行+JIT——控件粒度分期将其在 Stage 1 压缩至 chrome 闭包子集。

## 模型定义

```
进程启动 → Stage 1【窗口阶段】→ 主窗体出现(完整 chrome + 加载态) → Stage 2【完备阶段】自动启动 → 完成
           chrome 闭包最小集      事件: StageOneCompleted              事件: StageTwoCompleted
```

- **Stage 1 = 渲染完整 AtomUI 主窗体骨架的最小集**：chrome 闭包注册（Window→TitleBar→窗口按钮/
  图标等 fragment 子集，~15-25ms）+ 主题目录 + 初始主题应用（全局 token）+ 框架公共语言构建 +
  关键初始化器（animator/MediaBreakpoint）+ 关键字体 + registry 构建。实测构成 ≈ **~215ms**。
- **Stage 2 = 框架完备 + 业务初始化**，于启动 TopLevel 首个 `RequestAnimationFrame` 回调后的
  Dispatcher Background 空隙自动启动。该调度本身不能保证主窗体首帧已提交、已呈现或原生窗口已激活；
  就绪策略按修订后的正式架构实施。两段：
  - **框架段**：按声明序执行 `builder.Deferred(...)` 注册块——包粒度（Gallery/业务包整包）或
    控件粒度（Desktop 包的 chrome 闭包外 fragments）→ registry v2 → 增量主题重编译（~43ms）→
    原子合并重发布；
  - **业务段**：应用订阅 `StageTwoCompleted` 构建真实 shell/导航/页面。

## 分期粒度契约

### 控件粒度（chrome 闭包）

- 生成器为每个控件包产出 **fragment 级 Stage 元数据**：关键集（chrome 闭包）fragment 标注为
  Stage 1，其余默认 Stage 2；包级 `Collect` 分区为关键集先行、余量延迟两段收集。
- **chrome 闭包集合**以实证法确定（headless 渲染迭代：过滤至骨架窗无缺失资源警告），结果固化为
  生成器内置框架包关键集；三方包可经包清单声明自己的 critical 集。
- **未声明 Stage 元数据的包**（老生成器产出）：回退整包粒度分期（`Deferred` 块内整包注册），
  二进制与行为兼容。

### Builder API

```csharp
public interface IAtomUIBuilder
{
    // 现有成员全部不变；默认仍为 Stage 1（不 opt-in 行为零变化）
    IAtomUIBuilder Deferred(Action registration);   // Stage 2 注册块，按声明序执行
}
```

- `Deferred` 块内可调用控件包、本地化和 Theme Definition Resolver 注册 API，语义与现行一致，
  仅执行时机后移，并与 `Ensure` 幂等语义自然组合。`AddInitializer` 具有不可回滚的任意副作用，
  必须留在 Stage 1；Deferred 块内新增 initializer 在发布前失败。
- 分期实现只存 `Action` 委托，Stage 2 窗口内静态直接调用——无反射、无表达式树、无动态装配。

### 冻结契约演进

1. Stage 1 冻结（Build 后，现行行为）；
2. **Stage 2 注册窗口**：ApplicationScope 执行 Deferred 块期间受控重开 staging（仅接受块内调用，声明序原子）；
3. 窗口关闭后永久冻结（窗口外晚期注册抛异常——现行行为保持）。

### Registry 演进与增量重编译

Deferred 块全部执行后：registry v2（同一构建路径 + 增补注册集）→ 以当前主题定义重应用（~43ms，
全局 only，控件快照零物化——热点 C 契约）→ 原子合并重发布。

- **值等价保证**：同一主题定义/配置 + 注册集超集 → 全局 token/资源值不变（差分锁定）；新控件快照
  首查编译。
- **重发布防闪烁/防风暴**：值相等 → DynamicResource 重估无视觉变化；dispatcher 合并单次广播
  （热点 B 教训：广播不允许同步级联嵌套）。
- 快照缓存：registry revision 变化 → 旧 entry 自然失效（LRU 既有行为）。

## 事件契约（IAtomUIStartupEvents）

挂 ApplicationScope，UI 线程派发，时序确定：

| 事件 | 时点 | 业务用途 |
|---|---|---|
| `StageOneCompleted` | Stage 1 完成（UseAtomUI 返回即已保证） | 应用 Show 主窗体（加载态内容） |
| `StageTwoStarted` | Stage 2 编排启动 | 可选感知 |
| `StageTwoCompleted` | 注册 + registry v2 + 主题/本地化原子提交完成 | 业务构建真实 shell/页面、撤加载态 |
| `StartupProgress(StartupStage, string)` | Stage 2 内部里程碑（可选粒度） | 加载组件进度展示 |
| `StageTwoFailed(Exception)` | 框架段失败 | 应用存活（Stage 1 状态完好），业务可展示错误态 |

非 staged 模式不派发任何事件——零执行路径。
事件处理器逐个隔离；提交后的 `StageTwoCompleted` 业务异常只记录 warning，不得反向触发
`StageTwoFailed` 或改变框架已提交状态。

## Loading 组件模式

- 事件是契约，组件是可替换实现：业务开发者订阅事件自定义任意加载组件。
- Gallery 路线 A 实现重构为官方参照（事件订阅 + 本地化占位 + 错误态）；框架级
  `StartupLoadingIndicator` 控件后置为独立小任务（控件注册流程重，契约价值在事件）。

## 失败语义

- Stage 1 失败：现行行为（抛出，原子回滚）。
- Stage 2 失败：Deferred 块或重编译抛异常 → 派发 `StageTwoFailed`，停止后续块，窗口关闭，
  应用以 Stage 1 状态继续运行；框架记录诊断。

## 生成器契约兼容演进（原"零改动"约束的升级）

- **增量式**：fragment Stage 元数据与分区收集为新增发射形态；`Register/Ensure` 既有签名与调用
  形态保持，老生成器产出的包（无元数据）走整包回退——二进制兼容。
- **模板门**：net8 ILLink/ILC8 绑定的纯调用模板契约以扩展模板测试锁定（F1 方法论延续）；
  分区发射必须通过既有全部生成器契约测试。
- **裁剪等价**：CandidateKeys → fragment 映射在分期下等价（Stage 1 执行关键集 keys，Stage 2 执行
  余量 keys，合集与非分期一致）。
- **未 opt-in 零执行路径**：不调用 `Deferred` 的应用（含 AOT）行为与现状逐项一致；新增代码静态
  体积（KB 级）进 AOT 体积棘轮门。

## 兼容性

- **opt-in**：不调用 `Deferred` 的应用行为零变化（含 AOT）。
- 老生成器包：整包粒度回退，无需重编。
- Gallery 为首个采用者；稳定后议转默认（另行评审）。

## 收益指标与验收线

| 指标 | 现状 | 预期 | 依据 |
|---|---:|---:|---|
| UseAtomUI 窗口前路径 | 579ms | **~215ms（−63%）** | 🔴分段实测构成（chrome 闭包 ~20 替代整包 174） |
| 空窗出现（Release JIT 终端，完整 chrome） | 1.37s | **验收线 ≤0.95s，目标 ~0.9s** | 🔴248+215+ctor/show 实测段 🔵微调 |
| 空窗出现（Debug JIT） | 1.50s | ~1.1s | 🔵 |
| 空窗出现（AOT 发布版） | 全就绪 ~1s（用户实测） | **验收线 ≤0.4s，目标 ~0.3s** | AOT 消除 Collect/pre-init/ctor JIT；实施首任务实测基线棘轮收紧（只紧不松） |
| 全就绪总时长（JIT） | 1.75s | ~1.5-1.7s（持平略优） | Stage 2 与渲染暖机并行 |
| 非 staged 应用启动 | — | 无回归（交错采样容差内） | 🔴对照门 |

不确定性：±20-30ms。未达验收线 → 回退分析根因，不降线。

## 测试矩阵

1. **差分**：staged vs 非 staged，同一真实注册表最终全局资源值/快照结构相等（热点 C 方法论）。
2. **无闪烁**：值相等重发布的视觉不变性（headless 属性断言 + 渲染快照）。
3. **事件时序**：StageOne < StageTwoStarted < StageTwoCompleted 顺序、UI 线程、非 staged 零派发。
4. **冻结契约**：窗口外注册抛异常；Deferred 按声明序；与 Ensure 幂等组合。
5. **失败注入**：Deferred 抛异常 → StageTwoFailed、应用存活、Stage 1 主题完好、后续块停止、窗口关闭。
6. **chrome 闭包**：Stage 1 骨架窗渲染无缺失资源警告（headless 迭代实证，闭包集合锁定为测试）。
7. **生成器门**：分区发射模板契约测试（net8 后端绑定形态）；老生成器形态包回退整包粒度测试。
8. **裁剪等价**：CandidateKeys 分期/非分期合集一致；IsTrimmed 两模式注册集合一致。
9. **AOT 门**：Gallery staged + 非 staged 应用双发布（osx-arm64）+ 冒烟；体积棘轮归因；
   非 staged 启动性能无回归。
10. **本地化差分**：Stage 2 追加 catalog 后既有键值不变（含 FlowDirection）。
11. **收益验收**：交错采样中位数满足验收线（JIT ≤0.95s 与 AOT ≤0.4s 双线，AOT 按首任务基线棘轮）。

## 改动文件清单（预估）

- 生成器：fragment Stage 元数据发射 + 分区收集 + 框架包 chrome 闭包关键集
- 框架：`AtomUIBuilder`（Deferred 捕获）、`ApplicationExtensions`（staged 分支）、`ApplicationScope`
  （编排 + 事件 + 注册窗口）、`ThemeManager`（staging 追加 + registry v2 + 增量重编译 + 合并重发布）、
  `ControlRegistrationRuntime`（分区收集协作）、`IAtomUIStartupEvents` + 事件参数
- 本地化：`LocalizationBuilder` 受控重开 + `LocalizationHost` 增量重建
- Gallery：`GalleryApplication` 分期声明、`WorkspaceWindow` 事件模式重构、删除临时手工编排
- 文档：启动模式指南、runtime.md §16、progress 回填
- 探针：保留至收益验收后删除

## 非目标

- 不改变任何查询结果、缓存键、公开注册 API 签名；
- 不做全局 token 惰性化（全局急切提交是 Stage 1 正确性前提）；
- 不默认启用分期；框架级 StartupLoadingIndicator 控件后置独立任务。

## 风险与回退

- chrome 闭包集合低估（Stage 1 缺资源）→ 闭包迭代实证 + 测试锁定；发现即扩集。
- 生成器分区发射与 net8 后端不兼容 → 回退整包粒度（spec 已含回退路径，验收线随之后退至
  整包版本：JIT ≤1.05s / AOT ≤0.45s——**回退需显式决策，不静默降线**）。
- Stage 2 重编译广播风暴：dispatcher 合并 + 值相等 → 测试矩阵 2 锁定。
- 回退：staged 为独立 opt-in 路径，单提交 revert 恢复。
