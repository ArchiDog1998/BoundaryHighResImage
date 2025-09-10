using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Grasshopper.GUI.Canvas;
using HarmonyLib;

namespace BoundaryHighResImage.Patches;

[HarmonyPatch(typeof(GH_Painter), "GenerateWirePen")]
internal class PainterPatch
{
    internal static LockType Type = LockType.None;

    public enum LockType : byte
    {
        None,
        A,
        B,
    }

    private static void Postfix(PointF a, PointF b, ref Pen __result)
    {
        if (Data.WireRange is 0) return;
        switch (Type)
        {
            case LockType.None:
                break;
            case LockType.A:
                __result.Brush = CreateBrush(a);
                break;
            case LockType.B:
                __result.Brush = CreateBrush(b);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static PathGradientBrush CreateBrush(PointF location)
    {
        var radius = Data.WireRange;
        var path = new GraphicsPath();
        path.AddEllipse(location.X - radius, location.Y - radius, radius * 2, radius * 2);

        var brush = new PathGradientBrush(path);
        brush.CenterPoint = location;
        brush.CenterColor = GH_Skin.wire_default;
        brush.SurroundColors = [Color.Transparent];

        return brush;
    }
}