using SimpleGrasshopper.Attributes;
using System;
using System.ComponentModel;
using System.Drawing;

namespace BoundaryHighResImage;

internal static partial class Data
{
    [ToolButton("icon24.png")]
    [Setting, Config("Boundary Hi-Res Image")]
    private static readonly bool _enable = true;

    [Range(1, 5, 1)]
    [Setting, Config("Zoom Factor")]
    private static readonly float _ZoomFactor = 2;

    [Setting, Config("Canvas Color")]
    private static readonly Color _CanvasColor = Color.Transparent;

    [Setting, Config("Save")]
    private static readonly bool _Save = false;

    [Setting, Config("OutSideWire Wires")]
    private static readonly OutsideWireType _WireType = OutsideWireType.KeepIt;

    [Setting, Config("AddRhinoView")]
    private static readonly bool _RhinoView = false;

    [Setting, Config("Ratio", parent: "AddRhinoView")]
    private static readonly Ratio _Ratio = Ratio.R16_9;

    [Setting, Config("Transparent Background", parent: "AddRhinoView")]
    private static readonly bool _TrasnparentBg = true;

    [Setting, Config("Draw Grid Axes", parent: "AddRhinoView")]
    private static readonly bool _DrawGridAxes = false;

    [Setting, Config("Draw Grid", parent: "AddRhinoView")]
    private static readonly bool _DrawGrid = false;

    [Setting, Config("Draw Axes", parent: "AddRhinoView")]
    private static readonly bool _DrawAxes = false;
}

internal enum OutsideWireType : byte
{
    KeepIt,
    Hide,
    Remove,
}

internal enum Ratio : byte
{
    [Description("16:9")]
    R16_9,

    [Description("4:3")]
    R4_3,

    [Description("1:1")]
    R1_1,
}
