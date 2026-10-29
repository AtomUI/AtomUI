# CT13 S5：Select两历史资源分类与文档校正

本包是源码合同分类/文档修正，不增加内置消费者或视觉行为，不关闭完整Select家族。

`rg -n 'SelectAffixPadding|FixedItemMargin' src tests controlgallery`仅命中SelectToken两个声明和各自默认函数/attribute，未找到内置C#/AXAML/Gallery消费；自动schema/资源导出不是布局投射。原字段也没有独立公开实例属性或已实现alias，不能因为其名称而给prefix/suffix或tag补造新效果。

Ruling：按既有ControlWidth等历史兼容策略将两者归Legacy/CompatibilityRetained——保留名称、Thickness类型、默认公式、key/ID/slot和config/resource读写，外部专用Style仍可显式使用；当前无自动内置布局效果。若判断有误，代价是调用者可能期待内置调节，正式文档因此明确这一边界，后续实际功能需独立合同与视觉审查，而不是悄悄映射到公开Padding或标签margin。

SelectAffixPadding默认有效Global.PaddingXXS；FixedItemMargin默认各边Floor(UniformlyPaddingXXS/2)。没有改公式、求值顺序、修订、alias或消费者；C#只添加XML说明。逐行移除XML注释后原/新源码严格相等，声明、attribute和可执行内容不变；临时对照文本已删除，没有新增测试/驱动/媒体/原生工程。

正式token文档将两者移出有效内置入口列表，单列历史兼容资源；将不存在的Own Padding/PaddingSM/PaddingLG改为真实SingleModePadding三档，并按实际SelectAddOnDecoratedBox主题定位。单选内缩、MultiModePadding/PrefixIndent、有效Global.SpacingXXS等既有行为保持。

源码分类已回填两字段记录，未把它们标成Active消费者或完整家族通过。成员审查1文件/0控件/0问题（Token数据类不作控件结构计数）；已有SelectTokenTests精确1PASS、0Skip、19ms，git diff --check通过。无临时测试/媒体残留，也不重复运行无代码变化的视觉/原生矩阵，不借S4批准，不含Git。

```bash
rg -n 'SelectAffixPadding|FixedItemMargin' src tests controlgallery
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~AtomUI.Desktop.Controls.Tests.SelectControl.SelectTokenTests
```
