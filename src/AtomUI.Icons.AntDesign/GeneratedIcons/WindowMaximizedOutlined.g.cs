// This code is auto generated. Do not modify.
// Copyright (c) Qinware Technologies Co., Ltd. 2019-2025. All rights reserved.
// Copyright (c) Ant Design Authors 2025 https://github.com/ant-design/ant-design

using Avalonia;
using System;
using Avalonia.Media;
using AtomUI.Controls;
using AtomUI.Media;
namespace AtomUI.Icons.AntDesign;

public class WindowMaximizedOutlined : AntDesignIcon
{
    public WindowMaximizedOutlined()
    {
        IconTheme = IconThemeType.Outlined;
        ViewBox = new Rect(0, 0, 24, 24);
    }

    private static readonly DrawingInstruction[] StaticInstructions = [
        new PathDrawingInstruction()
        {
            Data = StreamGeometry.Parse("M5,21c-0.5,0-1-0.2-1.4-0.6S3,19.5,3,19V5c0-0.5,0.2-1,0.6-1.4S4.5,3,5,3h14c0.6,0,1,0.2,1.4,0.6S21,4.4,21,5  v14c0,0.5-0.2,1-0.6,1.4S19.6,21,19,21H5z M5,19h14V5H5V19z M5,19V5V19z"),
            FillBrush = IconBrushType.Stroke,
        }
    ];

    protected override IList<DrawingInstruction> DrawingInstructions => StaticInstructions;
}

