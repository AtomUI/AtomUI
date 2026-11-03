# MarqueeLabel Changelog

本文记录控件设计与实现变化，不代替正式版本发布说明。

## 2026-11-01

- 建立正式控件文档，明确完整文本、视口/裁剪、hover、可见性与无公开 Semantic manifest 的边界。
- 两 Own 迁移至 DependencyDefaultsV2 revision 1，保留200/150与四算法默认/type/key/slot。
- 可声明 Root 绑定迁至 ControlTheme，普通原生 Style 恢复覆盖能力；明确 Global 字体/前景进入实际文字，未配置保持继承。
- 文本/字体宽度变动时重建终点，旧完成回调只清理所属实例，退出取消并释放 CTS；原4倍时长、OnlyIfVisible、绘制与裁剪保持。
- 临时、正常Skia/AOT与清理后证据逐条记入CT15，不据局部分包关闭完整CT15。
