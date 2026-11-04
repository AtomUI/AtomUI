# 内置 Motion 轨迹与组件规格

本文拥有[Motion 系统](overview.md)内置轨迹、曲线控制点与组件选择的规格。公共定义与组合见
[定义与扩展](definitions-and-extension.md)，弹层配置与几何算法见[弹层组件动效](popup-motion.md)，
具体控件的业务与模板合同继续由控件文档拥有。设计与源码的差异统一由[验证状态](verification.md#弹层组件动效接入状态)记录。

## 基础参数

Fast/Mid/Slow 使用有效主题值；默认分别为 100/200/300ms。显式参数与组件覆盖服从定义页的优先级，不能改写共享定义。
未单独指定的基础 Fade、Slide、Move、Zoom 使用 Mid；big-fast 通过 Fast 参数表达，不构成另一种缩放算法。
透明度端点按下表指定；CSS 未声明的端点承接当前有效值，不能无条件当作固定 1。中断衔接另按播放协议处理。
位移百分比以目标的相应参考宽/高解析，变换顺序和原点属于轨迹的一部分。

## 曲线控制点

名称仅用于定位对应的内置参数，不声明同名全局 Token 已存在。自定义 Easing 可覆盖默认曲线。

| 曲线 | cubic-bezier(x1, y1, x2, y2) |
| --- | --- |
| OutCirc | (0.08, 0.82, 0.17, 1) |
| InOutCirc | (0.78, 0.14, 0.15, 0.86) |
| Out | (0.215, 0.61, 0.355, 1) |
| InOut | (0.645, 0.045, 0.355, 1) |
| OutBack | (0.12, 0.4, 0.29, 1.46) |
| InBack | (0.71, -0.46, 0.88, 0.6) |
| InQuint | (0.755, 0.05, 0.855, 0.06) |
| OutQuint | (0.23, 1, 0.32, 1) |
| CSS ease | (0.25, 0.1, 0.25, 1) |

linear 不进行曲线变换。求值按 x 反解参数，再取 y；超调输出与属性合法范围分别处理。

## 家族轨迹

| 家族 | 进入端点 | 离开端点 | 默认曲线 |
| --- | --- | --- | --- |
| Fade | opacity 0→1 | opacity 1→0 | linear / linear |
| 普通 Zoom | scale .2→1，opacity 0→1 | scale 1→.2，opacity→0 | OutCirc / InOutCirc |
| Big/方向 Zoom | scale .8→1，opacity 0→有效终点 | scale 1→.8，opacity→0 | OutCirc / InOutCirc |
| Slide | 对应轴 scale .8→1，opacity 0→1 | 对应轴 1→.8，opacity 1→0 | OutQuint / InQuint |
| Move | 对应轴 ±100%→0，opacity 0→1 | 0→对应轴 ±100%，opacity 1→0 | OutCirc / InOutCirc |
| Expand/Collapse | 0/当前占位→自然占位，opacity→1 | 当前占位→0，opacity→0 | InOut / InOut |

Move 只有两个端点，不加入 80% 中段、方向专属透明度或单方向 scale。
Expand/Collapse 表达自然内容视口，不能将其实现为缩放文字或一般矩阵布局。

## 方向与原点

原点使用目标本地参考区域的比例坐标。

| 方向 | Slide 轴/原点 | Zoom 原点 | Move 位移端 |
| --- | --- | --- | --- |
| Up | Y，(0,0) | (.5,0) | Y=-目标高度 |
| Down | Y，(1,1) | (.5,1) | Y=+目标高度 |
| Left | X，(0,0) | (0,.5) | X=-目标宽度 |
| Right | X，(1,0) | (1,.5) | X=+目标宽度 |

普通/Big Zoom 默认以中心为原点，组件显式计算的原点优先。Move 原点为 (0,0)。
内容视口的 Up/Left 尾边锚定与缩放原点不同，按[布局合同](targets-and-layout.md#内容几何)处理。

## 组件选择

| 组件或场景 | 规格 |
| --- | --- |
| Modal/Dialog 表面 | 普通 Zoom；默认入场 Slow、离场 Mid，原点来自本次有效打开位置；无有效位置时使用中心原点 |
| Drawer 面板 | 对应边 ±100%↔0、opacity .7↔1、CSS ease、Slow；不套用基础 Move 透明度 |
| Drawer 遮罩 | opacity 0↔1、CSS ease、Slow，与面板共享启动基准 |
| Inline 子菜单/内容 collapse | 自然高度与 opacity、InOut；参考默认 Mid，组件若保留其他时长必须明确记录 |

Modal 表面与遮罩、Drawer 表面与遮罩是不同目标，不因为同一业务动作而争用一个目标的请求槽位。
组件现有 Slow/其它曲线与参考规格不同时，校准属于可感知行为变化，须单列验收，不能称为保持外观的纯性能优化。
减少动画策略和 IsMotionEnabled 改变时由当前 owner 收敛状态，不把 watchdog 时间误当视觉时长。

### 弹层组件选择

| 组件或场景 | 对应语义 | 默认规格 |
| --- | --- | --- |
| ComboBox、Select、AutoComplete 家族、Cascader 根弹层、TreeSelect 根弹层 | Select/AutoComplete/Cascader/TreeSelect | Slide，Mid；实际下方 SlideUp，上方 SlideDown |
| DatePicker、RangeDatePicker、TimePicker、RangeTimePicker | 单值/范围 Picker | Slide，Mid；根据实际上下方切换，范围箭头随表面同步 |
| DropdownButton、SplitButton 的菜单路径、MenuFlyout/ContextMenu 根弹层、DataGrid 筛选 | Dropdown；ContextMenu 对应 contextMenu 触发方式 | 四方向 Slide，Mid；实际下/上/右/左分别为 SlideUp/Down/Left/Right |
| Menu/NavMenu 水平顶层菜单 | Menu horizontal | 固定 SlideUp，Mid；与选择器自动翻转策略分开 |
| 独立 Menu 次级、NavMenu Vertical/Horizontal 次级与 InlineCollapsed 弹层 | Menu other/vertical | ZoomBig，Mid；LTR 按 placement 选角点，RTL 最终原点右上 |
| Dropdown、MenuFlyout、ContextMenu 内嵌子菜单 | Dropdown 内嵌 Menu | ZoomBig，Mid；固定左上原点，包含 RTL 和翻转 |
| ToolTip、OverflowTip、折叠菜单 Tooltip | Tooltip | ZoomBig，Fast；原点来自 alignment 与箭头 Token 偏移 |
| 内容 Flyout、FlyoutHost/InfoFlyout、PopupConfirm、ColorPicker | Popover/Popconfirm/ColorPicker | ZoomBig，Mid；原点来自 alignment 与箭头 Token 偏移 |
| 通用 Popup | 无特定组件语义 | 默认无动效；调用方直接设置 OpenMotion/CloseMotion |
| 通用 TreeViewFlyout | AtomUI 扩展 | 采用 Popover 规格；委托入口继承实际 Flyout 的规格 |

表内 Slide 入场使用 OutQuint、离场使用 InQuint；ZoomBig 入场使用 OutCirc、离场使用 InOutCirc。
默认入/离场时长均为对应 Fast 或 Mid，不统一缩短 leave；FillMode 为 Both。Hover/focus 延迟与 motionDeadline 不作为视觉时长。
Inline 自然高度展开仍按上表内容 collapse 规格管理，不迁到 PopupSurface。

表内均为未覆盖时的组件默认，由[族基类或独立配置入口](popup-motion.md#2-基类属性默认装配与所有权)上的 OpenMotion/CloseMotion
按主题优先级提供。组件语义不注册为 Popup 上的预设枚举属性；用户显式定义和 null 不受默认计算回写。
Menu 的模式、层级和样式来源先决定默认，不能仅按当前放置方向推断。具体角点、alignment 原点和阴影坐标转换由
[弹层几何算法](popup-motion.md#4-实际方向与原点)统一拥有，单个控件不复制同一算法。

## 来源与更新

可追溯的官方源码：[曲线与时长种子](https://github.com/ant-design/ant-design/blob/984592267de88fbf9cdaecc96a56629e721c709d/components/theme/themes/seed.ts)、
[Fade](https://github.com/ant-design/ant-design/blob/984592267de88fbf9cdaecc96a56629e721c709d/components/style/motion/fade.ts)、
[Slide](https://github.com/ant-design/ant-design/blob/984592267de88fbf9cdaecc96a56629e721c709d/components/style/motion/slide.ts)、
[Move](https://github.com/ant-design/ant-design/blob/984592267de88fbf9cdaecc96a56629e721c709d/components/style/motion/move.ts)、
[Zoom](https://github.com/ant-design/ant-design/blob/984592267de88fbf9cdaecc96a56629e721c709d/components/style/motion/zoom.ts)、
[内容展开](https://github.com/ant-design/ant-design/blob/984592267de88fbf9cdaecc96a56629e721c709d/components/_util/motion.ts)、
[Drawer](https://github.com/ant-design/ant-design/blob/984592267de88fbf9cdaecc96a56629e721c709d/components/drawer/style/motion.ts)、
[Modal](https://github.com/ant-design/ant-design/blob/984592267de88fbf9cdaecc96a56629e721c709d/components/modal/style/index.ts)。

上游更新不会静默改变当前库。更新时同时复核参数、消费者选择和中间轨迹样本，再修改本页及对应验证。

弹层组件选择的官方参考源码：[Select 与翻转](https://github.com/ant-design/ant-design/blob/211c6ed29b0d2abd18485e293681f8a4e415240b/components/select/style/dropdown.ts)、
[Dropdown 方向选择](https://github.com/ant-design/ant-design/blob/211c6ed29b0d2abd18485e293681f8a4e415240b/components/dropdown/dropdown.tsx)、
[Dropdown 子菜单原点](https://github.com/ant-design/ant-design/blob/211c6ed29b0d2abd18485e293681f8a4e415240b/components/dropdown/style/index.ts)、
[Menu 模式](https://github.com/ant-design/ant-design/blob/211c6ed29b0d2abd18485e293681f8a4e415240b/components/menu/menu.tsx)、
[Menu placement 原点](https://github.com/ant-design/ant-design/blob/211c6ed29b0d2abd18485e293681f8a4e415240b/components/menu/style/index.ts)、
[Menu RTL 覆盖](https://github.com/ant-design/ant-design/blob/211c6ed29b0d2abd18485e293681f8a4e415240b/components/menu/style/rtl.ts)、
[Tooltip Fast](https://github.com/ant-design/ant-design/blob/211c6ed29b0d2abd18485e293681f8a4e415240b/components/tooltip/index.tsx)、
[Popover ZoomBig](https://github.com/ant-design/ant-design/blob/211c6ed29b0d2abd18485e293681f8a4e415240b/components/popover/index.tsx)、
[边对齐箭头偏移](https://github.com/ant-design/ant-design/blob/211c6ed29b0d2abd18485e293681f8a4e415240b/components/style/placementArrow.ts)。

组合复用关系的源码依据：[AutoComplete 复用 Select](https://github.com/ant-design/ant-design/blob/211c6ed29b0d2abd18485e293681f8a4e415240b/components/auto-complete/AutoComplete.tsx)、
[Cascader](https://github.com/ant-design/ant-design/blob/211c6ed29b0d2abd18485e293681f8a4e415240b/components/cascader/index.tsx)、
[TreeSelect](https://github.com/ant-design/ant-design/blob/211c6ed29b0d2abd18485e293681f8a4e415240b/components/tree-select/index.tsx)、
[单值 Picker](https://github.com/ant-design/ant-design/blob/211c6ed29b0d2abd18485e293681f8a4e415240b/components/date-picker/generatePicker/generateSinglePicker.tsx)、
[范围 Picker](https://github.com/ant-design/ant-design/blob/211c6ed29b0d2abd18485e293681f8a4e415240b/components/date-picker/generatePicker/generateRangePicker.tsx)、
[Picker 翻转](https://github.com/ant-design/ant-design/blob/211c6ed29b0d2abd18485e293681f8a4e415240b/components/date-picker/style/index.ts)、
[TimePicker 复用](https://github.com/ant-design/ant-design/blob/211c6ed29b0d2abd18485e293681f8a4e415240b/components/time-picker/index.tsx)、
[Popconfirm](https://github.com/ant-design/ant-design/blob/211c6ed29b0d2abd18485e293681f8a4e415240b/components/popconfirm/index.tsx)、
[ColorPicker](https://github.com/ant-design/ant-design/blob/211c6ed29b0d2abd18485e293681f8a4e415240b/components/color-picker/ColorPicker.tsx)、
[Table 筛选 Dropdown](https://github.com/ant-design/ant-design/blob/211c6ed29b0d2abd18485e293681f8a4e415240b/components/table/hooks/useFilter/FilterDropdown.tsx)。
菜单层级与 alignment 的辅助来源：[rc-menu 层级模式](https://github.com/react-component/menu/blob/5e2a3e21a39af9ea4a1ec33926f1e1ffb11474a4/src/SubMenu/index.tsx)、
[rc-trigger 对齐坐标](https://github.com/react-component/trigger/blob/6e7b38514578d8c259eb42f2b79d69817c66b41e/src/hooks/useAlign.ts)。
