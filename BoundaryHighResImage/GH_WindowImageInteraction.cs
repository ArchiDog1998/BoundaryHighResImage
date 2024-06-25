using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI.Canvas.Interaction;
using Grasshopper.Kernel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            using SolidBrush solidBrush = new (Color.FromArgb(150, Color.LightSkyBlue));
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

        Task.Run(() => Capture(m_selbox));
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

        var bitmap = canvas.Invoke(() => canvas.GenerateHiResImageTile(viewport, Data.CanvasColor));

        if (Data.Save)
        {
            var dialog = new SaveFileDialog()
            {
                Title = "Saving Path",
                Filter = "image files (*.png)|*.png",
            };

            if (dialog.ShowDialog() != DialogResult.OK) return;

            bitmap.Save(dialog.FileName);
        }
        else
        {
            using var ms = new MemoryStream();

            bitmap.Save(ms, ImageFormat.Png);

            IDataObject dataObj = new DataObject();
            dataObj.SetData("PNG", ms);
            dataObj.SetData(DataFormats.Dib, Clipboard.GetData(DataFormats.Dib));
            dataObj.SetData("Format17", Clipboard.GetData("Format17"));
            dataObj.SetData("Bitmap", Clipboard.GetData("Bitmap"));

            canvas.Invoke(() => Clipboard.SetDataObject(dataObj, true));
        }
    }
}
