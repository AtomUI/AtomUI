# 内置 Motion 轨迹与组件规格

本文拥有[Motion 系统](overview.md)内置轨迹、曲线控制点与组件选择的规格。公共定义与组合见
[定义与扩展](definitions-and-extension.md)，具体控件的业务与模板合同继续由控件文档拥有。

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
| Select 默认弹层 | SlideUp；根据实际 placement 和组件规则处理方向，不统一改为 Big Zoom |
| Modal/Dialog 表面 | 普通 Zoom；默认入场 Slow、离场 Mid，原点来自本次有效打开位置；无有效位置时使用中心原点 |
| Drawer 面板 | 对应边 ±100%↔0、opacity .7↔1、CSS ease、Slow；不套用基础 Move 透明度 |
| Drawer 遮罩 | opacity 0↔1、CSS ease、Slow，与面板共享启动基准 |
| Inline 子菜单/内容 collapse | 自然高度与 opacity、InOut；参考默认 Mid，组件若保留其他时长必须明确记录 |

Modal 表面与遮罩、Drawer 表面与遮罩是不同目标，不因为同一业务动作而争用一个目标的请求槽位。
组件现有 Slow/其它曲线与参考规格不同时，校准属于可感知行为变化，须单列验收，不能称为保持外观的纯性能优化。
减少动画策略和 IsMotionEnabled 改变时由当前 owner 收敛状态，不把 watchdog 时间误当视觉时长。

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
