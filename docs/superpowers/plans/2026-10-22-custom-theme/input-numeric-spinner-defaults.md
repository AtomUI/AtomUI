# CT13 / CT15 NU1：ButtonSpinner 八项默认与有效Own边

主agent、现有目录/分支，承接CT05默认协议与输入选择计划。产品只修改ButtonSpinnerToken.cs，不改输入转换、模板、尺寸或绘制逻辑。ControlWidth已独立核对为无内置消费的Legacy兼容资源，保留90默认及配置/资源读写；Shared入口留给后续实际消费者核定，不因默认协议通过标为已接通。

8字段保持名称/类型/顺序/key/ID/slot与默认公式：ControlWidth=90、HandleWidth=ControlHeightSM、HandleIconSize=FontSize/2、HandleActiveBg=ColorFillAlter、HandleBg=ColorBgContainer、HandleHoverColor=ColorPrimary、HandleBorderColor=ColorBorder。FilledHandleBg按ColorUtils.OnBackground(Global.ColorFillSecondary, 有效Own.HandleBg)求值，保留唯一已有Own依赖，不新增字段或clamp。

- [x] NU1-R：先实际V1协议反例和有效Own断链；固定Control Global.ColorFillSecondary=#80000000与显式HandleBg=#c86432，独立期望Filled=#ff643219；另证透明HandleBg→#80000000。验证真实Filled ButtonSpinnerHandle.Background，而非只看资源。
- [x] NU1-B：原代码保存明暗16条默认/type/key/ID/slot、真实独立ButtonSpinner与NumericUpDown两模式及三档/Custom的基线；保留inline/float/左右手柄、数值/焦点/输入与原尺寸。默认与已有配置/清除同完整轨迹，AA/1DIP/复位。
- [x] NU1-I：sealed partial + DependencyDefaultsV2 rev1/IControlTokenDefaults，8静态默认函数；Filled函数声明HandleBg输入并接收有效Color参数，移除旧空ctor/whole evaluator。没有consumer或ControlWidth行为改动。
- [x] NU1-V：全部默认/显式Own/透明/0/清除/显式Filled优先；实际handle与公开输入、独立owner/作用域；实际Mac managed/AOT与NET8、单Token成员审计。默认旧新0；有效Own修复所致Filled区域变化按本包精确范围单独审查，不沿用CB4批准。
- [x] NU1-C：记录、正式文档与CSV同步；临时源/工程/媒体清理，清理后精确ButtonSpinner/Numeric模块验证；作者另步自审，完整CT13/CT15/CT20/CT25继续开放，不提交。

本机分包已完成，精确18背景变化及N01-N14已独立批准，详见[结果](input-numeric-spinner-defaults-results.md)。完整家族及CT25保持开放。
