using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI.Canvas.Interaction;
using Grasshopper.Kernel;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json.Bson;
using Rhino;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BoundaryHighResImage;
internal class GH_WindowImageInteraction : GH_AbstractInteraction
{
    private Rectangle m_selbox;
    public GH_WindowImageInteraction(GH_Canvas canvas, GH_CanvasMouseEvent mEvent)
        : base(canvas, mEvent)
    {
        m_canvas.CanvasPaintBackground += Canvas_PaintBackGround;
    }

    public override void Destroy()
    {
        base.Canvas.ShowMRUPanels();
        m_canvas.CanvasPaintBackground -= Canvas_PaintBackGround;
        base.Destroy();
    }

    private void Canvas_PaintBackGround(GH_Canvas sender)
    {
        if (m_selbox.Width != 0 || m_selbox.Height != 0)
        {
            SolidBrush solidBrush = new SolidBrush(Color.FromArgb(150, Color.BlueViolet));
            sender.Graphics.FillRectangle(solidBrush, m_selbox);
            solidBrush.Dispose();
        }
    }

    public override GH_ObjectResponse RespondToMouseMove(GH_Canvas sender, GH_CanvasMouseEvent e)
    {
        base.RespondToMouseMove(sender, e);
        if (!m_active)
        {
            return GH_ObjectResponse.Ignore;
        }
        base.Canvas.HideMRUPanels();
        m_selbox = Rectangle.Union(new Rectangle(GH_Convert.ToPoint(m_canvas_mousedown), new Size(0, 0)), new Rectangle(GH_Convert.ToPoint(e.CanvasLocation), new Size(0, 0)));
        sender.Refresh();
        return GH_ObjectResponse.Handled;
    }

    public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
    {
        if (!m_active)
        {
            return GH_ObjectResponse.Release;
        }
        if (!sender.IsDocument)
        {
            return GH_ObjectResponse.Release;
        }

        Capture();
        return GH_ObjectResponse.Release;
    }

    private void Capture()
    {
        var zoom = Data.ZoomFactor;
        var viewport = new GH_Viewport
        {
            Width = (int)(m_selbox.Width * zoom),
            Height = (int)(m_selbox.Height * zoom),
            Zoom = zoom,
            Tx = -(int)(m_selbox.X * zoom),
            Ty = -(int)(m_selbox.Y * zoom)
        };
        viewport.ComputeProjection();
        var bitmap = Canvas.GenerateHiResImageTile(viewport, Data.CanvasColor);

        if (Data.Save)
        {
            var dialog = new SaveFileDialog()
            {
                Title = "Saving Path",
                Filter = "image files (*.png)|*.png",
            };

            if (dialog.ShowDialog() != DialogResult.OK) return;

            Task.Run(() => bitmap.Save(dialog.FileName));
        }
        else
        {
            Clipboard.SetImage(bitmap);
        }
    }
}
