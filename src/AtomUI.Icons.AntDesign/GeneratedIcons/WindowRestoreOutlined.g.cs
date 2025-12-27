// This code is auto generated. Do not modify.
// Copyright (c) Qinware Technologies Co., Ltd. 2019-2025. All rights reserved.
// Copyright (c) Ant Design Authors 2025 https://github.com/ant-design/ant-design

using Avalonia;
using System;
using Avalonia.Media;
using AtomUI.Controls;
using AtomUI.Media;
namespace AtomUI.Icons.AntDesign;

public class WindowRestoreOutlined : AntDesignIcon
{
    public WindowRestoreOutlined()
    {
        IconTheme = IconThemeType.Outlined;
        ViewBox = new Rect(0, 0, 24, 24);
    }

    private static readonly DrawingInstruction[] StaticInstructions = [
        new PathDrawingInstruction()
        {
            Data = StreamGeometry.Parse("M17.4,6.6C17,6.2,16.5,6,16,6H4C3.5,6,3,6.2,2.6,6.6S2,7.5,2,8v12c0,0.5,0.2,1,0.6,1.4S3.5,22,4,22h12 c0.5,0,1-0.2,1.4-0.6S18,20.5,18,20V8C18,7.5,17.8,7,17.4,6.6z M16,18H4V8h12V18z"),
            FillBrush = IconBrushType.Stroke,
        }
,         new PathDrawingInstruction()
        {
            Data = StreamGeometry.Parse("M20,2c0.6,0,1,0.2,1.4,0.6S22,3.4,22,4v14h-2V4H6V2H20z"),
            FillBrush = IconBrushType.Stroke,
        }
,         new PathDrawingInstruction()
        {
            Data = StreamGeometry.Parse("M16,20V8V20z"),
            FillBrush = IconBrushType.Stroke,
        }
    ];

    protected override IList<DrawingInstruction> DrawingInstructions => StaticInstructions;
}

