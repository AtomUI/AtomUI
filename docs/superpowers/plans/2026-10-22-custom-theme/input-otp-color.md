# CT-13 OTP 分隔符默认颜色修复

用户2026-10-24明确允许修复默认分隔符颜色。批准的原型以OtpLineEdit有效ColorText作为普通颜色、ColorTextDisabled作为禁用颜色；原16控件明暗默认变化2560／2960像素。本次可改变该默认颜色，不更改字形位置、字体合同、cell状态配色、文本／输入、尺寸、分隔符内容或动效。

最终颜色在OtpLineEdit ControlTheme声明，由既有前景继承传给separator。保留根本地Foreground与专用Semantic Part Style的定制优先级；示例／测试对separator Part的定制使用生成的OtpLineEditSeparatorStyle在AXAML声明，禁止查找节点后赋样式。

先以实际普通／禁用、明暗、控件Global配置／清除、透明值、根本地值和专用Style建立失败／正对照；保存当前产品原版图。改后核对批准的默认像素、实际颜色与输入、cell几何／文字／默认配色，证明正常motion和其他视觉维度保持。专用Style既验证声明形态，也验证真实命中；当前Mac AOT单独执行。清理临时用例、驱动、原型／媒体与源备份后，聚焦OTP模块、NET8、正式文档及作者自审。不代表完整CT-13完成。
