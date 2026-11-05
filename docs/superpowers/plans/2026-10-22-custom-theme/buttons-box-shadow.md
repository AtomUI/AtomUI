# Button.BoxShadow Implementation Plan

> 使用superpowers:executing-plans，由主agent在既有feature/custom-theme执行；用户禁止子agent、新分支/worktree及自行提交。

**Goal:** 关闭V11的预设色阴影定制缺口，保持133Own及未定制默认视觉。
**Architecture:** Border.BoxShadowProperty.AddOwner<Button>；ControlTheme编译绑定VariantShadow至根属性，再投影至既有阴影层。保持原生优先级，不引入代码绑定、手工存在位或新Part。
**Tech Stack:** Avalonia12.1.4、NET10精确filter/Skia、Release NET8、当前Mac NativeAOT。
**Spec:** [已选合同](../../specs/2026-10-22-custom-theme/button-box-shadow.md)。

## Review Focus

- 未配置和显式空阴影必须区分，ClearValue恢复当前主题而非初始缓存。
- Dropdown独立Own/Global不借Button配置；继承Button主题的派生类保持原生属性效果。
- Style/local不得被颜色、variant、loading或scope更新压住；只读探针不能强改目标值。
- 所有Button模板与Dropdown独立主题实际阴影层均接入；不新增布局、命中或公开Part。
- 前后默认像素比较完整同历史，原生有限采样/CT25历史失败不得混称全平台通过。

## Task 1：证据与最小实现

- [x] 保存被改产品文件的开工preimage/hash，保护CT11输入。
- [x] 临时运行测试确认Button/Dropdown缺少可设置的注册BoxShadow属性，取得有效RED；捕获默认Skia基线。
- [x] Button.cs公共属性region增加注册及wrapper；ButtonTheme与DropdownButtonBaseTheme声明根默认与阴影层绑定。
- [x] 同断言GREEN，补实际绘制层优先级/空值/当前Own/preset恢复、模板、继承派生与GC。
- [x] 触及Button.cs全部代码绑定审查及完整成员布局audit；不为长度拆文件，纯重排与行为分开核对。

## Task 2：公开合同与验收

- [x] 同步Button/Dropdown正式overview/implementation/token/semantic-root说明和changelog，Gallery既有Root样式例；当前无独立API/Token表，不添加shadow Part。
- [x] 编译AXAML根Style实际命中；明暗默认同历史0、1DIP检错/恢复、定制与清除，必要明确视觉审查。
- [x] 当前Mac managed/AOT实际根属性与绘制层检查；默认Native产物证据匹配本次代码，保留有限采样范围。
- [x] 删除本包临时源、preimage/驱动/产物/媒体后精确受影响模块、NET8、链接/diff/成员audit及作者另步自审。
- [x] 回填V11、CT12及两对细线处置；未获范围核定不关闭CT12，不自行提交。

2026-10-30接手工程收尾完成：当前产物1236/1236、52原生帧及12跨运行0；临时清理与最后189项/NET8/成员通过，见[最终工程结果](buttons-box-shadow-finish-results.md)。V11不再待实现；唯一视觉批准见[汇总表](buttons-box-shadow-final-visual-review.md)，无其他待开发项。不提交。

最终完成：2026-10-30，用户已明确回复“批准本表限定范围，保留 CT25”（request_user_input_async / call_a2fe796a8df0481aadd7e4393060b87e / item 0）。公开BoxShadow工程、默认原生验证、清理后189项及限定视觉批准已全部完成，CT12在既定批准范围完成并关闭。原始24/max1、两对3968/max2及其他既定历史仍按CT25保留，不设通用容差，不扩大其他平台/背景/恢复模式，无Git授权。 依据[最后结果](buttons-box-shadow-finish-results.md)与[批准表](buttons-box-shadow-final-visual-review.md)。
