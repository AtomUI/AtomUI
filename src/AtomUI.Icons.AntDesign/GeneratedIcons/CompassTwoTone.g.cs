// This code is auto generated. Do not modify.
// Copyright (c) Qinware Technologies Co., Ltd. 2019-2025. All rights reserved.
// Copyright (c) Ant Design Authors 2025 https://github.com/ant-design/ant-design

using Avalonia;
using System;
using Avalonia.Media;
using AtomUI.Controls;
using AtomUI.Media;
namespace AtomUI.Icons.AntDesign;

public class CompassTwoTone : AntDesignIcon
{
    public CompassTwoTone()
    {
        IconTheme = IconThemeType.TwoTone;
        ViewBox = new Rect(0, 0, 1024, 1024);
    }

    private static readonly DrawingInstruction[] StaticInstructions = [
        new PathDrawingInstruction()
        {
            Data = StreamGeometry.Parse("M512 140c-205.4 0-372 166.6-372 372s166.6 372 372 372 372-166.6 372-372-166.6-372-372-372zM327.6 701.7c-2 .9-4.4 0-5.3-2.1-.4-1-.4-2.2 0-3.2L421 470.9 553.1 603l-225.5 98.7zm375.1-375.1L604 552.1 471.9 420l225.5-98.7c2-.9 4.4 0 5.3 2.1.4 1 .4 2.1 0 3.2z"),
            FillBrush = IconBrushType.Fill,
        }
,         new PathDrawingInstruction()
        {
            Data = StreamGeometry.Parse("M322.3 696.4c-.4 1-.4 2.2 0 3.2.9 2.1 3.3 3 5.3 2.1L553.1 603 421 470.9l-98.7 225.5zm375.1-375.1L471.9 420 604 552.1l98.7-225.5c.4-1.1.4-2.2 0-3.2-.9-2.1-3.3-3-5.3-2.1z"),
            FillBrush = IconBrushType.Stroke,
        }
,         new PathDrawingInstruction()
        {
            Data = StreamGeometry.Parse("M512 64C264.6 64 64 264.6 64 512s200.6 448 448 448 448-200.6 448-448S759.4 64 512 64zm0 820c-205.4 0-372-166.6-372-372s166.6-372 372-372 372 166.6 372 372-166.6 372-372 372z"),
            FillBrush = IconBrushType.Stroke,
        }
    ];

    protected override IList<DrawingInstruction> DrawingInstructions => StaticInstructions;
}

