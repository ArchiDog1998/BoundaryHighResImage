using System;
using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI.Canvas.Interaction;
using Grasshopper.Kernel;
using Rhino.Display;
using Rhino;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using BoundaryHighResImage.Patches;

namespace BoundaryHighResImage;

internal class GH_WindowImageInteraction : GH_AbstractInteraction
{
    private Rectangle _selectionBox;

    public GH_WindowImageInteraction(GH_Canvas canvas, GH_CanvasMouseEvent mEvent)
        : base(canvas, mEvent)
    {
        m_canvas.CanvasPaintBackground += Canvas_PaintBackGround;
    }

    public override void Destroy()
    {
        Canvas.ShowMRUPanels();
        m_canvas.CanvasPaintBackground -= Canvas_PaintBackGround;
        base.Destroy();
    }

    private void Canvas_PaintBackGround(GH_Canvas sender)
    {
        if (_selectionBox is { Width: 0, Height: 0 }) return;
        using SolidBrush solidBrush = new(Color.FromArgb(150, Color.LightSkyBlue));
        sender.Graphics.FillRectangle(solidBrush, _selectionBox);
    }

    public override GH_ObjectResponse RespondToMouseMove(GH_Canvas sender, GH_CanvasMouseEvent e)
    {
        base.RespondToMouseMove(sender, e);
        if (!m_active)
        {
            return GH_ObjectResponse.Ignore;
        }

        Canvas.HideMRUPanels();
        _selectionBox = Rectangle.Union(new Rectangle(GH_Convert.ToPoint(m_canvas_mousedown), new Size(0, 0)),
            new Rectangle(GH_Convert.ToPoint(e.CanvasLocation), new Size(0, 0)));
        sender.Refresh();
        return GH_ObjectResponse.Handled;
    }

    public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
    {
        if (!m_active || !sender.IsDocument)
        {
            return GH_ObjectResponse.Release;
        }

        Task.Run(() =>
        {
            DrawIncomingPatch.CapturingRange = _selectionBox;
            try
            {
                Capture(_selectionBox);
            }
            finally
            {
                DrawIncomingPatch.CapturingRange = null;
                Instances.ActiveCanvas.Invoke(Instances.ActiveCanvas.Refresh);
            }
        });
        return GH_ObjectResponse.Release;
    }

    private static void Capture(Rectangle rect)
    {
        var canvas = Instances.ActiveCanvas;
        if (canvas == null) return;

        var zoom = Data.ZoomFactor;
        var viewport = new GH_Viewport
        {
            Width = (int)(rect.Width * zoom),
            Height = (int)(rect.Height * zoom),
            Zoom = zoom,
            Tx = -(int)(rect.X * zoom),
            Ty = -(int)(rect.Y * zoom)
        };
        viewport.ComputeProjection();

        var bitmap =
#if NET48
            (Bitmap)
#endif
            canvas.Invoke(() => Capture(canvas, viewport));

        if (Data.Save)
        {
            canvas.Invoke((Action)(() =>
            {
                var dialog = new SaveFileDialog()
                {
                    Title = "Saving Path",
                    Filter = "image files (*.png)|*.png",
                };

                if (dialog.ShowDialog() != DialogResult.OK) return;

                bitmap.Save(dialog.FileName);
            }));
        }
        else
        {
            using var ms = new MemoryStream();

            bitmap.Save(ms, ImageFormat.Png);

            IDataObject dataObj = new DataObject();
            dataObj.SetData("PNG", ms);
            dataObj.SetData(DataFormats.Dib, Clipboard.GetData(DataFormats.Dib));
            dataObj.SetData("Format17", Clipboard.GetData("Format17"));
            dataObj.SetData(DataFormats.Bitmap, true, bitmap);

            canvas.Invoke(() =>
            {
                Clipboard.SetDataObject(dataObj, true);
                Instances.DocumentEditor.SetStatusBarEvent(
                    new GH_RuntimeMessage("Captured the image to the clipboard."));
            });
        }
    }

    private static Bitmap Capture(GH_Canvas canvas, GH_Viewport vp)
    {
        var grasshopper = canvas.GenerateHiResImageTile(vp, Data.CanvasColor);
        if (!Data.RhinoView) return grasshopper;

        var capture = new ViewCapture
        {
            TransparentBackground = Data.TrasnparentBg,
            DrawGridAxes = Data.DrawGridAxes,
            DrawGrid = Data.DrawGrid,
            DrawAxes = Data.DrawAxes
        };

        var view = RhinoDoc.ActiveDoc.Views.ActiveView;

        capture.Height = grasshopper.Height;
        capture.Width = (int)(capture.Height * Data.Ratio switch
        {
            Ratio.R4_3 => 4 / 3f,
            Ratio.R16_9 => 16 / 9f,
            _ => 1,
        });

        var rhino = capture.CaptureToBitmap(view);

        var bitmap = new Bitmap(grasshopper.Width + rhino.Width, grasshopper.Height);
        var g = Graphics.FromImage(bitmap);

        g.DrawImage(grasshopper, new Point(0, 0));
        if (!Data.TrasnparentBg)
        {
            g.FillRectangle(new SolidBrush(Data.CanvasColor),
                new Rectangle(new Point(grasshopper.Width, 0), new Size(rhino.Width, rhino.Height)));
        }

        g.DrawImage(rhino, new Point(grasshopper.Width, 0));

        return bitmap;
    }
}