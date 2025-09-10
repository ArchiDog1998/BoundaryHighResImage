using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using HarmonyLib;

namespace BoundaryHighResImage.Patches;

[HarmonyPatch(typeof(GH_Attributes<IGH_Param>), "RenderIncomingWires",
    typeof(GH_Painter), typeof(IEnumerable<IGH_Param>), typeof(GH_ParamWireDisplay))]
internal class DrawIncomingPatch
{
    internal static Rectangle? CapturingRange = null;

    private static bool Prefix(GH_Attributes<IGH_Param> __instance, GH_Painter painter, IEnumerable<IGH_Param> sources,
        GH_ParamWireDisplay style)
    {
        if (CapturingRange is not { } rectangle) return true;

        var inputInRange = rectangle.Contains(GH_Convert.ToPoint(__instance.InputGrip));
        var sourcesWithRange = sources.Select(source =>
        {
            var isInRange = rectangle.Contains(GH_Convert.ToPoint(source.Attributes.OutputGrip));
            return (source, isInRange);
        }).ToArray();

        if (inputInRange)
        {
            foreach (var source in sourcesWithRange.Where(i => i.isInRange))
            {
                Draw(source.source, PainterPatch.LockType.None);
            }
        }

        switch (Data.WireType)
        {
            case OutsideWireType.KeepIt:
                if (inputInRange)
                {
                    foreach (var source in sourcesWithRange.Where(i => !i.isInRange))
                    {
                        Draw(source.source, PainterPatch.LockType.A);
                    }
                }
                else
                {
                    foreach (var source in sourcesWithRange.Where(i => i.isInRange))
                    {
                        Draw(source.source, PainterPatch.LockType.B);
                    }
                }

                break;
            case OutsideWireType.Hide:
                if (inputInRange && sourcesWithRange.Any(i => !i.isInRange))
                    DrawRemote(__instance.InputGrip);
                break;
            case OutsideWireType.Remove:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return false;

        void Draw(IGH_Param param, PainterPatch.LockType lockType)
        {
            PainterPatch.Type = lockType;
            try
            {
                var type = style is GH_ParamWireDisplay.faint
                    ? GH_WireType.faint
                    : GH_Painter.DetermineWireType(param.VolatileData);
                painter.DrawConnection(__instance.InputGrip, param.Attributes.OutputGrip, GH_WireDirection.left,
                    GH_WireDirection.right,
                    __instance.Selected, param.Attributes.Selected,
                    CentralSettings.CanvasFancyWires ? type : GH_WireType.generic);
            }
            finally
            {
                PainterPatch.Type = PainterPatch.LockType.None;
            }
        }
    }

    private static void DrawRemote(PointF location)
    {
        var graphic = Instances.ActiveCanvas.Graphics;
        var num = graphic.Transform.Elements[0];
        if (!(num > 0.55f)) return;
        var point = GH_Convert.ToPoint(location);
        var alpha = GH_GraphicsUtil.BlendInteger(0.5, 1.0, 0, 80, num);
        var num2 = 0;
        do
        {
            var num3 = 6 + 3 * num2;
            var rect = new Rectangle(point.X - num3, point.Y - num3, 2 * num3, 2 * num3);
            using var linearGradientBrush = new LinearGradientBrush(rect, Color.FromArgb(0, 0, 0, 0),
                Color.FromArgb(alpha, 0, 0, 0), LinearGradientMode.Vertical);
            linearGradientBrush.WrapMode = WrapMode.TileFlipXY;
            linearGradientBrush.SetSigmaBellShape(0.5f);
            using var pen = new Pen(linearGradientBrush, 1f);
            graphic.DrawEllipse(pen, rect);
            num2++;
        } while (num2 <= 3);
    }
}