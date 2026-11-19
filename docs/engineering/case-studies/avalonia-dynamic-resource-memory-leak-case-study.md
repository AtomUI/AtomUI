# Avalonia DynamicResource 内存泄露案例

本文记录 2026-06 Gallery 内存泄露排查与修复过程，并沉淀为 AtomUI 控件库处理 `DynamicResource` 生命周期问题的标准案例。

核心结论：当一个非 Visual 的 `AvaloniaObject` 使用 `DynamicResource`，但自身不是 `IResourceHost` 时，Avalonia 会从 XAML anchor 查找资源宿主。Gallery 场景里这个宿主经常落到 `Application`，随后 `Application.ResourcesChanged` 会强引用 `DynamicResourceExpression`，再通过 binding/value store 链路把目标对象和整棵 ShowCase 页面留住。

## 适用范围

这份文档适用于下面几类对象：

- 继承 `AvaloniaObject`，但不是 `StyledElement` / `Control` / `Visual` 的数据型控件对象。
- 暴露 `AvaloniaProperty`，并允许在 AXAML 中通过 `{DynamicResource ...}`、`TokenResourceBinder.CreateGlobalTokenBinding(...)` 或类似机制绑定资源。
- 对象生命周期由另一个控件管理，例如 `DataGridColumn` 由 `DataGrid` 管理，`NavMenuNode` 由 `NavMenu` 或 `NavMenuItem` 容器管理。
- 对象能反向引用真实控件、容器、Header、Popup、Flyout 或 Gallery ShowCase。

本案例里的具体对象：

- `DataGridColumn`
- `DataGridColumnGroupItem`
- `NavMenuNode`
- `Flyout` 的全局 token 资源绑定

## 强边界

这次事故已经进入全局资源生命周期规范，见 `.agents/skills/atomui-resource-lifecycle/SKILL.md` 的 `DynamicResource Scoped Host Rule`。

写代码时必须遵守下面的硬规则：

- 非 Visual `AvaloniaObject` 只要承载 `DynamicResource` / token-resource binding，就必须实现 scoped `IResourceHost` / `IThemeVariantHost`，或者提供显式 attach/release token。
- 新增或改造 owner-managed 非 Visual `AvaloniaObject` 时，默认按 [Scoped Resource Host 开发规范](../development/scoped-resource-host.md) 管理生命周期，并由 [Scoped Resource Host Generator](../../modules/generator/scoped-resource-host-generator.md) 生成样板代码。
- scoped resource host 必须先查 owner control，再 fallback 到 `Application.Current`。
- owner change、container clear、detach、unregister 必须能释放 owner 订阅。
- 构造函数里默认禁止创建 global token binding；需要首次使用时创建，并在 close/detach/unregister/dispose 时释放。
- 不允许用清空 Gallery DataContext、手动清路由、改成静态资源值来掩盖泄露。
- 没有 WeakReference 生命周期测试的修复不算完成。

这不是建议，是 merge 阻断条件。任何违反这条边界的实现，即使短期功能正常，也必须重写或补齐生命周期。

## 事故现象

用户在 Gallery 中通过 F5 随机打开 ShowCase，用于测试内存泄露。理论预期是同一时刻只有当前路由页的一个 ShowCase 存活。

实际现象：

- 随机导航一段时间后，RSS 快速上升到 600 MB、700 MB，继续操作后可到 1 GB 以上。
- 采集 dump 后发现多个旧 ShowCase 仍然存活。
- 典型残留对象包括多个 `DataGridShowCase`、`MenuShowCase`、大量 `ShowCaseItem` 和大量 `DynamicResourceExpression`。
- 用户停止在某个页面时，历史页面没有被释放，说明不是单纯的“当前页面比较重”。

一次修复前的典型对象计数：

```text
DataGridShowCase              4
MenuShowCase                  4
FlexPanelShowCase             1
ShowCaseItem                159
DynamicResourceExpression 141,023
DataGridTextColumn          588
DataGridColumnGroupItem      16
NavMenuNode                 480
GC Heap                 ~465 MB
```

这个数据不符合 Gallery 的路由策略。即使当前页面是重页面，也不应该同时保留多个旧 ShowCase。

## 排查方法

### 进程采样

使用 RSS 只判断趋势，不直接作为泄露结论：

```bash
ps -o pid=,etime=,rss=,vsz=,state=,command= -p <pid>
```

长期采样建议写入 tsv：

```bash
log=/private/tmp/atomui-memory-monitor/gallery-<pid>-rss.tsv
printf 'timestamp\tpid\tetime\trss_kb\tvsz_kb\tstate\tcommand\n' > "$log"

for i in {1..180}; do
  ts=$(date '+%Y-%m-%d %H:%M:%S')
  ps -o pid=,etime=,rss=,vsz=,state=,command= -p <pid> |
    awk -v ts="$ts" '{pid=$1; etime=$2; rss=$3; vsz=$4; state=$5; $1=$2=$3=$4=$5=""; sub(/^ +/,"",$0); printf "%s\t%s\t%s\t%s\t%s\t%s\t%s\n", ts,pid,etime,rss,vsz,state,$0}' >> "$log"
  sleep 10
done
```

RSS 的判断规则：

- 持续单调上升且不回落，才是泄露强信号。
- 重页面打开时的短峰值不是泄露证据。
- macOS / .NET runtime 不一定马上把托管堆回收后的内存还给系统，所以 RSS 必须结合 `gcdump` 看。

### 托管堆快照

使用 `gcdump` 查看存活对象数量：

```bash
dotnet-gcdump collect -p <pid> -o /private/tmp/atomui-memory-monitor/gallery-<pid>-end.gcdump
dotnet-gcdump report /private/tmp/atomui-memory-monitor/gallery-<pid>-end.gcdump
```

关键过滤：

```bash
dotnet-gcdump report /private/tmp/atomui-memory-monitor/gallery-<pid>-end.gcdump |
  rg "GC Heap bytes|GC Heap objects|AtomUIGallery\.ShowCases\.[^.]+\.[A-Za-z0-9]+ShowCase\s|AtomUIGallery\.Controls\.ShowCaseItem|DynamicResourceExpression|DataGridColumnGroupItem|NavMenuNode|DataGridTextColumn"
```

判断标准：

- ShowCase 存活数必须是 1。
- 允许 `ShowCaseItem` 数量随当前页面不同而变化。
- 允许 `DynamicResourceExpression` 数量随当前页面复杂度变化，但不应带着旧页面线性累积。

### 完整根链

当 `gcdump` 证明对象残留后，用 heap dump 和 `gcroot` 定位强引用链。

本案例里的典型根链：

```text
GalleryApplication
  DynamicResourceExpression
    ValueStore
      DataGridTextColumn / DataGridColumnGroupItem / NavMenuNode
        DataGridColumnHeader / NavMenuItem / DataGrid
          ShowCasePanel
            ShowCaseItem
              DataGridShowCase / MenuShowCase
```

这个根链说明根对象不是 Gallery 路由缓存，而是 `Application` 级资源订阅。

## Avalonia 资源绑定机制

`DynamicResourceExpression` 启动时会寻找资源宿主。这里记录的是 AtomUI 生命周期设计需要满足的行为约束，
不把某个框架版本的源码路径或行号作为长期依据。

关键行为：

1. 先看 target 是否实现 `IResourceHost`。
2. 如果 target 不是 `IResourceHost`，再从 `_anchor` 或 `IResourceProvider.Owner` 找宿主。
3. 找到宿主后订阅 `ResourcesChanged`。
4. 如果宿主也是 `IThemeVariantHost`，还会订阅 `ActualThemeVariantChanged`。
5. 只有 binding 停止时才会 unsubscribe。

这对 Visual 控件通常不是问题，因为控件本身有树生命周期，资源宿主也是局部的。但对非 Visual `AvaloniaObject`，如果 target 不是 `IResourceHost`，宿主经常退化到 `Application`。一旦 `Application` 订阅链持有 binding expression，target 就不会释放。

## 根因

### DataGridColumn

`DataGridColumn` 是非 Visual 的 `AvaloniaObject`，但 Header、CellTheme 等属性可以使用动态资源。修复前它不是资源宿主，动态资源在 Gallery 中会订阅到 `Application`。

泄露后果：

- `Application.ResourcesChanged` 持有 `DynamicResourceExpression`。
- `DynamicResourceExpression` 关联列对象的 value store。
- 列对象持有 header cell、owning grid 或其他 DataGrid 相关对象。
- DataGrid 位于 ShowCase 页面内，旧 ShowCase 被整棵保留。

### DataGridColumnGroupItem

`DataGridColumnGroupItem` 与 `DataGridColumn` 问题相同。它也是非 Visual 的 `AvaloniaObject`，Header 可动态资源绑定，并通过 `OwningGrid` 与 DataGrid 生命周期关联。

修复 `DataGridColumn` 后，dump 中仍能看到 `DataGridShowCase` 残留，根链落在 `DataGridColumnGroupItem`，说明同类非 Visual 对象必须成组审计。

### NavMenuNode

`NavMenuNode` 是菜单数据节点，Header、Icon、HeaderTemplate、IsEnabled 等属性会绑定到 `NavMenuItem`。修复前它不是资源宿主，动态资源订阅到 `Application` 后，会通过 binding observer、`NavMenuItem` 和视觉树把 `MenuShowCase` 留住。

`NavMenuNode` 的特殊点是：它没有固定的 `OwningMenu` 字段，节点由容器准备和清理过程动态使用。因此不能像 DataGridColumn 一样只靠属性 setter，而要在 `PrepareContainerForItemOverride` 和 `ClearContainerForItemOverride` 对称挂载/释放资源宿主。

### Flyout

`Flyout` 的问题不是 ShowCase 级 `DynamicResource` anchor，而是构造函数里创建了全局 token binding。Flyout 可以由按钮、SplitButton、FlyoutHost 等对象临时注册和释放，如果全局资源 binding 没有显式释放，也会把对象链条延长。

修复方向是把全局 token binding 从构造时创建改为打开时创建，关闭或解除注册时释放。

## 修复范式

### 原则 1：非 Visual AvaloniaObject 使用 Source Generator 生成资源宿主

当 owner-managed 非 Visual 对象会承载动态资源时，默认遵循 [Scoped Resource Host 开发规范](../development/scoped-resource-host.md)，并使用 [Scoped Resource Host Generator](../../modules/generator/scoped-resource-host-generator.md)：

```csharp
[GenerateScopedResourceHost]
public partial class DescriptionItem : AvaloniaObject
{
    // 业务 Avalonia 属性保留在主文件
}
```

生成器负责让目标类型实现：

```csharp
IResourceHost, IThemeVariantHost
```

资源查找顺序：

1. 先查自己的 owner control，例如 `OwningGrid`、`NavMenu`。
2. 再 fallback 到 `Application.Current`。

这样可以保留全局资源 fallback，同时让动态资源表达式优先订阅当前控件树的宿主，而不是直接订阅 `Application`。业务对象主文件不再手写 `_resourceHost`、事件转发、attach count 和 owner subscribe/unsubscribe 代码。

### 原则 2：owner 变化必须对称 unsubscribe

历史手写修复中的 DataGrid 类对象使用下面的标准形态。新增同类对象不再复制该样板代码，应由 generator 生成等价逻辑。

```csharp
private DataGrid? _owningGrid;
private DataGrid? _subscribedResourceHostGrid;

protected internal DataGrid? OwningGrid
{
    get => _owningGrid;
    internal set
    {
        if (ReferenceEquals(_owningGrid, value))
        {
            return;
        }

        UnregisterOwningGridResourceHost();
        _owningGrid = value;
        RegisterOwningGridResourceHost(_owningGrid);
    }
}
```

订阅和释放必须成对：

```csharp
_subscribedResourceHostGrid.ResourcesChanged += HandleOwningGridResourcesChanged;
_subscribedResourceHostGrid.ActualThemeVariantChanged += HandleOwningGridActualThemeVariantChanged;

_subscribedResourceHostGrid.ResourcesChanged -= HandleOwningGridResourcesChanged;
_subscribedResourceHostGrid.ActualThemeVariantChanged -= HandleOwningGridActualThemeVariantChanged;
```

本次实现位置：

- `src/AtomUI.Desktop.Controls.DataGrid/Column/DataGridColumn.cs`
- `src/AtomUI.Desktop.Controls.DataGrid/Column/DataGridColumnGroupItem.cs`

### 原则 3：容器驱动型数据节点使用 attach token

`NavMenuNode` 没有天然 owner setter，资源宿主由菜单容器准备时确定。标准形态：

```csharp
internal IDisposable AttachResourceHost(IResourceHost resourceHost)
{
    if (ReferenceEquals(_resourceHost, resourceHost))
    {
        _resourceHostAttachmentCount++;
    }
    else
    {
        DetachCurrentResourceHost();
        _resourceHost                = resourceHost;
        _resourceHostAttachmentCount = 1;
        RegisterResourceHost(resourceHost);
    }

    return Disposable.Create((Node: this, ResourceHost: resourceHost), state =>
    {
        state.Node.DetachResourceHost(state.ResourceHost);
    });
}
```

容器准备时挂载：

```csharp
if (menuNode is NavMenuNode navMenuNode)
{
    nodeBindingDisposables.Add(navMenuNode.AttachResourceHost(this));
}
```

容器清理时复用已有的 `ClearNodeBindingDisposables()`，避免新增另一套生命周期。

本次实现位置：

- `src/AtomUI.Desktop.Controls/NavMenu/NavMenuNode.cs`
- `src/AtomUI.Desktop.Controls/NavMenu/NavMenu.cs`
- `src/AtomUI.Desktop.Controls/NavMenu/NavMenuItem.cs`

### 原则 4：全局资源绑定不要放在构造函数里

构造函数创建全局 token binding 的风险是：对象还没有明确生命周期边界，释放路径容易遗漏。

Flyout 的修复方式：

- `EnsurePlacementTokenBindings()`：首次打开时创建全局资源 binding。
- `ReleasePlacementTokenBindings()`：关闭、解除注册、宿主释放时统一释放。
- `DropdownButton`、`SplitButton`、`FlyoutHost` 在 unregister 时调用 release，并在需要时先 hide。

本次实现位置：

- `src/AtomUI.Desktop.Controls/Flyouts/Flyout.cs`
- `src/AtomUI.Desktop.Controls/Flyouts/FlyoutHost.cs`
- `src/AtomUI.Desktop.Controls/DropdownButton/DropdownButton.cs`
- `src/AtomUI.Desktop.Controls/SplitButton/SplitButton.cs`

## 测试范式

### 弱引用测试

每个修复对象都要有一个“动态资源不会 root 住未引用对象”的测试。

测试结构：

1. 给 `Application.Current.Resources` 写入唯一 key。
2. 创建目标对象。
3. 用反射模拟 XAML `DynamicResourceExtension` 的 anchor 形态。
4. 读取初始值，确认 binding 生效。
5. 返回 `WeakReference`，让局部强引用离开作用域。
6. 多次强制 GC。
7. 断言 `WeakReference.IsAlive == false`。

DataGrid 测试位置：

- `tests/AtomUI.Desktop.Controls.DataGrid.Tests/Lifecycle/DataGridColumnResourceLifetimeTests.cs`

覆盖：

- `DataGridColumn` 不被 `Application.ResourcesChanged` root。
- `DataGridColumnGroupItem` 不被 `Application.ResourcesChanged` root。
- `DataGridColumn` 挂到 DataGrid 后仍能响应 Application 资源变化。

NavMenu 测试位置：

- `tests/AtomUI.Desktop.Controls.Tests/NavMenu/NavMenuNodeResourceLifetimeTests.cs`

覆盖：

- `NavMenuNode` 不被 `Application.ResourcesChanged` root。
- `NavMenuNode` 挂到 `NavMenu` 后优先使用 menu 自己的资源。

Flyout 测试位置：

- `tests/AtomUI.Desktop.Controls.Tests/Flyouts/FlyoutResourceBindingLifetimeTests.cs`

覆盖：

- Flyout 的全局资源 binding 有明确释放路径。

### 为什么测试要模拟 XAML anchor

直接 new 对象再设置属性，不能复现 XAML 中 `{DynamicResource ...}` 的 anchor 选择。测试必须模拟 `DynamicResourceExtension` 的 `_anchor`，否则可能测不到 `Application` 被订阅的真实泄露路径。

## 验收数据

修复后，用 F5 随机导航约 18 分钟，采样停止后抓取起始和结束 `gcdump`。

RSS 分桶：

```text
00-05min avg 547.1MB max 719.5MB
05-10min avg 571.5MB max 722.2MB
10-15min avg 570.4MB max 711.9MB
15-18min avg 580.8MB max 669.7MB
```

对象计数：

| 指标 | 起始 | 结束 | 结论 |
| --- | ---: | ---: | --- |
| 存活 ShowCase | 1 个 `NotificationShowCase` | 1 个 `FlexPanelShowCase` | 符合预期 |
| `ShowCaseItem` | 6 | 11 | 当前页差异 |
| `DynamicResourceExpression` | 3,835 | 9,126 | 当前页更重，但没有旧页累计 |
| GC Heap | 25.4 MB | 54.5 MB | 增长可解释 |
| RSS 最后样本 | 约 282 MB 起步 | 551.5 MB | 峰值后可回落 |

结论：修复后没有复现“多个旧 ShowCase 被保留”的严重泄露。RSS 仍有页面加载峰值，但托管堆对象没有显示旧 ShowCase 累积。

## Review 清单

新增或修改以下类型时，必须检查这份清单。

### 非 Visual AvaloniaObject

- 是否继承 `AvaloniaObject`，但不是 `Control` / `StyledElement`？
- 是否暴露可被 DynamicResource 或 token binding 绑定的 `AvaloniaProperty`？
- 是否有 owner control，例如 `OwningGrid`、`OwnerMenu`、`Host`？
- 如果有，是否实现 `IResourceHost` 和 `IThemeVariantHost`？
- `TryGetResource` 是否先查 owner，再 fallback 到 Application？
- owner 变化时是否先 unsubscribe 旧 owner，再 subscribe 新 owner？
- 是否转发 `ResourcesChanged` 和 `ActualThemeVariantChanged`？
- detach/remove/clear 路径是否能触发释放？

### 容器准备/清理

- `PrepareContainerForItemOverride` 里新增的 binding/subscription 是否加入已有 `CompositeDisposable`？
- `ClearContainerForItemOverride` 是否释放同一个 disposable？
- 如果同一个节点可被多个容器短暂使用，是否有 attachment count 或等价保护？
- 是否避免把数据节点永久挂到最后一次容器？

### 全局资源绑定

- 是否在构造函数里创建了 global token binding？
- 如果是，是否能改为首次使用时创建？
- 是否存在关闭、detach、unregister、re-template 时的释放路径？
- `Hide()` 和 release 的顺序是否明确？

### 测试

- 是否有 WeakReference + GC 的生命周期测试？
- 测试是否模拟 XAML `DynamicResource` anchor，而不是只测手动赋值？
- 是否覆盖资源值更新，避免修复泄露时破坏主题/资源响应？
- 是否跑过相关控件测试和 Gallery 测试？

## 常见错误

不要这样修：

- 只在 ShowCase 里手动清空 DataContext。这样只能掩盖某个页面，不能修复控件库根因。
- 在 Gallery 路由层强制缓存或强制释放页面。根链来自 `Application.ResourcesChanged`，路由层不是根因。
- 删除动态资源或改成静态值。这样会破坏主题、语言或 token 更新能力。
- 只给当前发现的类打补丁，不审计同类型非 Visual 对象。`DataGridColumn` 修完后，`DataGridColumnGroupItem` 仍然能保留旧页面。
- 只看 RSS 下结论。RSS 会受 runtime、native resource、OS 内存策略和当前页面复杂度影响。

## 标准修复步骤

1. 用 F5 或真实用户操作复现内存增长。
2. 用 `gcdump` 证明旧 ShowCase 或目标对象确实残留。
3. 用 `createdump --withheap` 和 `gcroot` 找到强引用链。
4. 如果根链经过 `DynamicResourceExpression`，检查 target 是否为非 Visual `AvaloniaObject` 且不是 `IResourceHost`。
5. 为目标对象补 WeakReference 红测。
6. 对 owner-managed 非 Visual `AvaloniaObject`，优先用 `GenerateScopedResourceHost` 生成资源宿主代理；只有不适用时才手写显式 attach/release 生命周期，并说明原因。
7. 补资源更新测试，确保主题和资源响应不退化。
8. 跑相关控件测试、Gallery 测试和 `git diff --check`。
9. 再跑一轮 F5 随机导航，确认 ShowCase 存活数回到 1。
10. 删除临时 dump/gcdump，避免把大文件留在临时目录。

## 本次验证命令

```bash
dotnet test tests/AtomUI.Desktop.Controls.DataGrid.Tests/AtomUI.Desktop.Controls.DataGrid.Tests.csproj
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj
dotnet build controlgallery/AtomUIGallery.Desktop/AtomUIGallery.Desktop.csproj -c Debug --framework net10.0
dotnet test tests/AtomUIGallery.Tests/AtomUIGallery.Tests.csproj
git diff --check
```

这些验证只能说明当前修复范围通过。后续新增类似非 Visual 对象时，仍要按本文 Review 清单重新审查生命周期。
