# HyperLinkButton Changelog

## 2026-11-04

- Custom 高度、排版和图标指标改用可由普通 Style 接管的基础默认，保留中档回退与三档预设。

## 2026-10-27

- 修复IsLoading未同步loading伪类，接通现有图标替换、旋转与加载透明度主题规则；保持Click、命令、enabled及导航语义。
- loading旋转由当前owner IsMotionEnabled约束，关闭时保留静态loading图标与透明度。
- 补齐Icon/Content的icon-only资格和IsDanger的danger资格同步，接通既有四档图标指标、内边距及危险状态色；保留disabled优先和原生属性覆盖。
- root背景、描边画刷与线宽进入实际Frame；保留null背景默认与透明命中面，移除子节点强制透明造成的覆盖，禁用Own边框色在请求非零线宽时绘制。

## 2026-10-23

- Own定义采用DependencyDefaultsV2，显式字体和布局行高进入下游默认内边距，保留17个字段、类型、历史名称与缺省数值。
- 建立独立控件文档入口，明确主题identity、导航行为、布局指标及模板责任。
