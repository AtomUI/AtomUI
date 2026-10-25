# CT19 C1：ColorPicker Own 默认依赖

沿用已批准的 Own V2 设计、CT19 总任务及既有迁移方式。修改 ColorPickerToken 的默认求值，不改变24个字段、类型、名称、顺序、资源编号或缺省数值；暂不扩展两个未核定消费者的字段语义。B08颜色源码和冻结30文件保持字节不变。

## 契约与选择

四条现行公式必须读有效 Own：PresetColorSize → CheckedMarkSize；HandlerSizeSM → SliderSize；HandlerSizeSM → SliderThumbSize；SliderTrackSize → PreviewSize。后两项同时读取当前 Control Effective Global 的 LineWidth／UniformlyMarginSM。明确下游、0值和恢复保持既有优先级。

使用 ControlDesignToken(DependencyDefaultsV2, Revision=1)、IControlTokenDefaults 与24个静态默认函数。属性保留原位置，默认函数接在现有属性后；文件不到400行，不另拆partial文件。拒绝在模板重复计算、在控件补监听或修改全局Compiler特判。

## 执行

1. 在 Temporary/CT19ColorDefaults 通过真实Startup注册及ThemeConfigProvider编译复现四依赖边；分别检查两个Handler下游，0输入、下游显式覆盖和恢复。先记录原版有效失败。
2. 在六类 Global／appearance 配置记录24字段值、类型、slot、enum资源ID与实际资源投射；迁移后逐项精确比较。实现仅修改 ColorPickerToken.cs；不改B08两文件或冻结表。
3. 实际消费者的明暗默认、Own依赖、明确下游及恢复，使用真实Skia同轨迹、A/A和1DIP。先区分现有消费者和未消费字段；有意修复的已有输入表现单独审查。当前Mac managed／NativeAOT实际调用生成默认计划与资源读取。
4. 更新现有token／implementation／changelog及本结果，不在冻结总表中伪造家族完成。删除临时验证、专用工程和媒体（待审查媒体明确保留），清理后运行ColorPicker精确模块检查和NET8。作者审查；不自动提交或发布。

## 仍开放

此切片不表示所有消费者、Global、尺寸、输入或家族完成。ColorPickerAlphaInputWidth／ColorPickerInputNumberHandleWidth的消费结论、公开旧SetPresetColor委托形状，以及其余CT19合同继续单独处理。CT24维持暂缓。
