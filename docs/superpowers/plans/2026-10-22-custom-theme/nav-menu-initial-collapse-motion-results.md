# NavMenu 首次展开：原始反例与被否决候选

本片继续已批准短设计，不重开任务。当前产品`NavMenu.cs`已恢复到278429b50，净改动为0；首次展开修复没有完成，没有Git提交。

## 原始条件与TDD

Headless/Skia的420×240 Window，Inline、初始折叠、请求Width300、默认折叠48、motion开启。初始化顺序保留先设`IsInlineCollapsed=true`再赋Width300；先赋Width再折叠的对照可以保有旧base输入，不能替代原始反例。

15项临时检查在原始顺序下实际13 FAIL / 2 PASS，0Skip。原首次展开缺少48与300之间的Bounds；先正常展开再折叠的正对照及Auto历史回退通过。失败覆盖Style/Local/绑定/明确0/撤销覆盖、Min/Max及首次展开后的四种取消路径。使用真实Bounds事件保存中间帧，避免只在某次轮询取样。

此前一次初始化顺序不同的运行7 FAIL / 8 PASS，以及Auto被错误期望为填满420而实际自然宽102的探针错误，均保留原日志，不计作该15项的有效原始结果。初次别名编译错误也不计作RED。

命令为：

```bash
ATOMUI_NAV_AUDIT="$PWD/.superpowers/sdd/nav-menu-initial-motion-audit/original-order-preserved" dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj -c Release --framework net10.0 --no-restore --filter FullyQualifiedName~Temporary.CT14NavInitialMotion
```

## 首个候选及否决理由

按原短设计在Width coercion入口保存一个实例double，展开目标使用该输入。实际13 PASS / 2 FAIL，不能接受：

1. Local Width300获胜时，新增低优先级Style Width260也调用coercion。镜像被260覆盖；动效内部目标260，最终Width300。候选引入可见的错误目标与终点跳变。
2. Animation Width360、Local Width300的优先级轨迹最终停在300，期望360；原有绑定/属性优先级未得到完整保障。

被否决产品SHA256为`0dfc38b789e5fa10900b462e36bdadc98cc438eb79f62b5f095f72b1f1e32c2a`，差异补丁SHA256为`c06ed757c404d36ff481a9364b4c7821e82772a8259569cb31cb6978be94f91b`，仅留文本调查记录。没有保留该镜像字段，也没有放宽目标断言。

## 框架最小反例

另用普通Avalonia Border派生类覆盖Width coercion，完全不使用NavMenu：Local300→Animation360，先确认有效Width360；依次把coercion布局约束设为48、80、NaN并调用公开CoerceValue。约束清除后期望360，实际300，独立临时用例1 FAIL / 0 PASS。该运行使用当前解析的Avalonia12.1.4，证明Animation恢复问题不只存在于NavMenu。

本地参考按规定查找，采用`/Users/chinboy/Projects/ReferenceProjects/Avalonia`，commit仍为原短设计记录的8eeda4f6f546165b3f72e63c9f42247abb306905。其EffectiveValue coercion同时处理当前值和base值，能够解释丢失原输入的风险；当前依赖的实际运行反例才是结论依据，不把参考版本当作当前二进制的源码证明。

## 当前出口

- 原“保留最后一次coercion输入”不能证明它是获胜输入；不得直接提交该实现。需要修订取值方案并重新证明Style/Local/绑定/Animation优先级。
- 不读取框架私有`_uncoerced`字段，不改写PropertyStore，不加Animation relay，不通过临时暴露展开Width或延迟刷新规避原始条件。
- 原始15项、候选15项和框架最小反例的失败计数保留，两份临时源码已经删除；不存在常驻测试净新增。恢复HEAD后的原有NavMenu307项通过，不覆盖本次新增反例。
- NavMenu的产品修复、实际明暗动效、原生/AOT及清理后修复模块出口未完成；CT14/CT25保持开放。不能把已撤回候选的13项通过写成完成。

详细数值和短日志在`.superpowers/sdd/nav-menu-initial-motion-audit/`；调查范围不包含升级Avalonia或修改参考项目。
