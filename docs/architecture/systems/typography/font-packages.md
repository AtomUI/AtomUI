# 字体包与资源注册

本文讲 [架构总览](overview.md) 里的轴 A：字体文件怎么变成 Avalonia 能解析的字族。这条链路与主题状态无关，注册
只影响 `FontManager` 能不能解析某个 `fonts:` 标识，不写入任何 Token。字体包的项目结构与发布边界见
[Fonts 模块](../../../modules/fonts/overview.md)。

## FontCollection 的构成

一个字体包的全部机制就是一个 `EmbeddedFontCollection` 子类，它把两个 URI 绑在一起：

```csharp
public class AlibabaSansFontCollection : EmbeddedFontCollection
{
    public AlibabaSansFontCollection() : base(
        new Uri("fonts:AlibabaSans", UriKind.Absolute),
        new Uri("avares://AtomUI.Fonts.AlibabaSans/Assets", UriKind.Absolute))
    {
    }
}
```

`fonts:AlibabaSans` 是对外的逻辑标识，`FontFamily` 字符串引用的就是它；`avares://.../Assets` 是资源实际位置。
两者之间没有中间层，改任何一个都是破坏性变更。

资源通过通配包含进入程序集：

```xml
<ItemGroup>
    <AvaloniaResource Include="Assets/*" />
</ItemGroup>
```

`EmbeddedFontCollection` 会扫描 `Assets` 下所有字体文件，按字体内部元数据归并字族与字重。这两件事合起来意味着
新增一个字重只需要把文件丢进 `Assets`：不用改 C# 代码，不用维护文件清单，`fonts:` 标识和字族名都不变。

## 两个包的实际内容

| 包 | 集合标识 | 字族名 | 资产 |
|---|---|---|---|
| `AtomUI.Fonts.AlibabaSans` | `fonts:AlibabaSans` | `Alibaba Sans` | `AlibabaSans-Light/Regular/Medium/Bold/Black/Heavy.ttf` |
| `AtomUI.Fonts.AlibabaPuHuiTi` | `fonts:AlibabaPuHuiTi` | `Alibaba PuHuiTi 3.0` | `AlibabaPuHuiTi-3-55-Regular.otf` |

两个包的定位不同。AlibabaSans 提供 6 个字重，是默认字体族回退链的首项，服务拉丁字符；`FontWeightStrong` 之类的
字重 Token 落在它身上才有真实字重资产可用。AlibabaPuHuiTi 只有 Regular 一个字重，且不在默认链里，它存在的理由是
宿主环境缺少中文系统字族时补位——目前只有 Browser Gallery 用到。因为没有 Bold 资产，中文文本的加粗依赖字形合成，
不是真实字重。

## 字族名常量

`AtomUI.Fonts.AlibabaPuHuiTi` 把字族名导出成常量，省掉调用方手写字符串：

```csharp
public static class AlibabaPuHuiTiFontConstants
{
    public const string FamilyName  = "Alibaba PuHuiTi 3.0";
    public const string FontFamily  = "fonts:AlibabaPuHuiTi#Alibaba PuHuiTi 3.0";
}
```

`AtomUI.Fonts.AlibabaSans` 没有对应的常量类，用它的地方只能硬写 `fonts:AlibabaSans#Alibaba Sans`。这是两个包
之间的接口不对称，记在 [verification.md](verification.md) 的已知不一致清单里。

## 注册入口

每个包给出两个注册方法，对应 Avalonia 与 AtomUI 两个配置阶段：

```csharp
// AppBuilder 阶段：WithAlibabaSansFont() 经 ConfigureFonts 使用统一的进程级登记。
AppBuilder.Configure<App>().WithAlibabaSansFont();

// IAtomUIBuilder 阶段：只声明包；OnCommit 经同一进程级登记入口执行。
AppBuilder.Configure<App>().UseAtomUI(builder => builder.UseAlibabaSansFont());
```

两个官方入口共享 `FontCollectionLifetime` 的同一 `FontManager`/URI 登记，不碰 Token，也不设置默认字体族。
同时调用、多个 AtomUI 应用 scope 或重复配置不会替换已有官方字体集合。Avalonia 的 `FontManager` 只提供
按 URI 的公开移除 API，无法确认该 URI 当前仍指向哪个实例；因此应用 scope 释放只放弃租约，不按 URI 删除。
集合由进程级 `FontManager` 持有并在其 Dispose 时释放。宿主后来显式替换同 URI 时，旧 AtomUI 租约的释放也
不会误删新实例；宿主自行直接使用同 URI 的任意第三方集合属于 URI 所有权冲突，应避免。

注册必须在首个视觉帧之前完成，并且不参与主题切换事务。字体集合在 `FontManager` 生命周期内至多按官方 URI
登记一次，应用 scope 的销毁不撤销其他应用仍可能使用的字体。

## 引用一个包不等于注册它

`AtomUI.Controls` 引用了 `AtomUI.Fonts.AlibabaSans`，但控件包里没有任何一处 `AddFontCollection` 调用。项目引用
只保证字体资源随产物分发，注册这件事必须由应用显式做。

这点直接决定了默认字体族能不能命中首项。应用没调 `UseAlibabaSansFont()` 时，`fonts:AlibabaSans` 解析不出来，
Avalonia 静默跳过继续往后回退，全过程没有任何异常或诊断。完整后果见
[font-family-resolution.md](font-family-resolution.md)。

## 两种宿主的实际做法

Desktop 宿主只注册拉丁字族，中文交给系统：

```csharp
.UseAtomUI(builder =>
{
    builder.UseAlibabaSansFont();
    // ...
})
```

Browser 宿主注册两个包，并显式覆盖默认字体族——浏览器里没有可依赖的中文系统字族，不覆盖就会缺字：

```csharp
builder.UseAlibabaSansFont();
builder.UseAlibabaPuHuiTiFont();
builder.WithDefaultFontFamily(FontFamily.Parse(
    $"fonts:AlibabaSans#Alibaba Sans, {AlibabaPuHuiTiFontConstants.FontFamily}, $Default"));
```

覆盖路径之间的优先级见 [font-family-resolution.md](font-family-resolution.md)。按本文描述的结构新建一个字体包，
操作步骤见 [制作 AtomUI 字体包](../../../guides/typography/creating-a-font-package.md)。
