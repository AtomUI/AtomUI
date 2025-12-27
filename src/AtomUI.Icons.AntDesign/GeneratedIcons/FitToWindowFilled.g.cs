// This code is auto generated. Do not modify.
// Copyright (c) Qinware Technologies Co., Ltd. 2019-2025. All rights reserved.
// Copyright (c) Ant Design Authors 2025 https://github.com/ant-design/ant-design

using Avalonia;
using System;
using Avalonia.Media;
using AtomUI.Controls;
using AtomUI.Media;
namespace AtomUI.Icons.AntDesign;

public class FitToWindowFilled : AntDesignIcon
{
    public FitToWindowFilled()
    {
        IconTheme = IconThemeType.Filled;
        ViewBox = new Rect(0, 0, 1024, 1024);
    }

    private static readonly DrawingInstruction[] StaticInstructions = [
        new PathDrawingInstruction()
        {
            Data = StreamGeometry.Parse("M896 128a42.7 42.7 0 0 1 42.7 42.7v682.7a42.7 42.7 0 0 1-42.7 42.7H128a42.7 42.7 0 0 1-42.7-42.7V170.7A42.7 42.7 0 0 1 128 128h768zM768 512h-85.3v128h-128v85.3H768V512zM469.3 298.7H256V512h85.3V384h128v-85.3z"),
            FillBrush = IconBrushType.Fill,
        }
    ];

    protected override IList<DrawingInstruction> DrawingInstructions => StaticInstructions;
}

