// This code is auto generated. Do not modify.
// Copyright (c) Qinware Technologies Co., Ltd. 2019-2025. All rights reserved.
// Copyright (c) Ant Design Authors 2025 https://github.com/ant-design/ant-design

using Avalonia;
using System;
using Avalonia.Media;
using AtomUI.Controls;
using AtomUI.Media;
namespace AtomUI.Icons.AntDesign;

public class HarmonyOSOutlined : AntDesignIcon
{
    public HarmonyOSOutlined()
    {
        IconTheme = IconThemeType.Outlined;
        ViewBox = new Rect(0, 0, 1024, 1024);
    }

    private static readonly DrawingInstruction[] StaticInstructions = [
        new PathDrawingInstruction()
        {
            Data = StreamGeometry.Parse("M377.5 0C585.987 0 755 169.013 755 377.5S585.987 755 377.5 755 0 585.987 0 377.5 169.013 0 377.5 0m0 64C204.359 64 64 204.359 64 377.5S204.359 691 377.5 691 691 550.641 691 377.5 550.641 64 377.5 64"),
            FillBrush = IconBrushType.Stroke,
            Transform = TransformParser.Parse("translate(134 65)").Value
        }
,         new PathDrawingInstruction()
        {
            Data = StreamGeometry.Parse("M611 824 611 896 144 896 144 824z"),
            FillBrush = IconBrushType.Stroke,
            Transform = TransformParser.Parse("translate(134 65)").Value
        }
    ];

    protected override IList<DrawingInstruction> DrawingInstructions => StaticInstructions;
}

