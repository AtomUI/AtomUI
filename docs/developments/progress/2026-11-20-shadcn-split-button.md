# Shadcn SplitButton 配色与接缝

## 上游核对

官方 Base UI 和 Radix 的 Split 示例在两枚 secondary 按钮之间显式插入 `ButtonGroupSeparator`。Nova、Vega、Maia、Lyra、Mira、Luma、Sera、Rhea 八个样式的分隔线均使用 `bg-input`，竖线宽 1 CSS px、距上下各 1 px。Dropdown Menu 示例由两枚 outline 按钮组成，共用第一枚的右边框。以上与旧有主题对齐基准提交一致。 [官方分隔线规则](https://ui.shadcn.com/docs/components/base/button-group#separator) · [Split 源码](https://github.com/shadcn-ui/ui/blob/f50887ae68d0fdcbfbe86b57be08af94a4c88a68/apps/v4/examples/base/button-group-split.tsx#L9-L17) · [Dropdown 源码](https://github.com/shadcn-ui/ui/blob/f50887ae68d0fdcbfbe86b57be08af94a4c88a68/apps/v4/examples/base/button-group-dropdown.tsx#L25-L35)

AtomUI 的实心 Primary 参考该结构，竖线用两段前景派生的低透明度颜色适配主色填充。Danger 用低透明度危险色。Default 使用两段原有描边。

## 复现与修正

实际 Gallery Luma 浅色页面的 Danger Primary 为 119.5 × 36 DIP，两个内容面显示 `#FD6C6A`，图标与文字 `#E7000B`，对比度 1.71:1。暗色面为 `#EC6C6A`，文字 `#FF6467`，对比度 1.05:1。两个子表面实际存放半透明色，旧 SplitButton 将不透明分隔色绘满了整个外轮廓。两层外圆角边缘产生浅色轮廓。

Shadcn 的透明危险色先解决完整表面绘制，分隔色移至实际模板槽内，取消从整块底色露出竖线的渲染方式。危险色填充用边框外沿作为绘制范围，可覆盖分隔槽两侧原来呈透明的半像素。两段只使用约束在实际高度一半之内的外侧圆角。主按钮按压的 1 DIP 反馈作用于文字与图标，触发器展开时使用悬停配色。危险红前景分别按浅深背景微调，以改善有色悬停面上的可读性。

## 验收

- 八个风格的浅色、深色和 Compact 共 32 个配方通过实际模板检查。覆盖内缩接缝、共享外圆角、半透明危险色底、浅深色静止和悬停对比。另有一项真实路由按压检查通过。
- Luma 2× 原生浅色静止 `#FDE5E6`／`#B40009`，对比度 5.95:1。悬停及按住的半段为 `#FACCCE`，对比度 4.96:1。展开实际 Cut／Copy／Delete 菜单后，只有触发段使用更深填充。两段的外框按住时保持稳定，文字下移 1 DIP。静止深色 `#3B1C1D`／`#FF8385` 的对比度为 6.45:1。
- 两端有效垂直圆角为 18 DIP。浅色内缩细线为 `#F8B3B5`，左右紧邻像素均与填充 `#FDE5E6` 相同。深色细线为 `#662C2E`，两侧均为 `#3B1C1D`。普通浅色 Primary 填充 `#171717`，竖线 `#404040`。Default 的描边接缝对照通过。原生校准同状态、1 DIP 扰动、复位差值分别为 0／2016／0。
- Gallery 构建成功。清理临时用例后，Shadcn 主题 164 项、控件与键盘 38 项、渲染 13 项、Gallery 3 项定向测试通过。改动文件的布局审查只有本次变更前已有的成员排序与接口分区项。
