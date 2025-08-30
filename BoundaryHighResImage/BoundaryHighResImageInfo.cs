using Grasshopper.GUI;
using Grasshopper.Kernel;
using HarmonyLib;
using SimpleGrasshopper.Util;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace BoundaryHighResImage;
public class BoundaryHighResImageInfo : GH_AssemblyInfo
{
    public override string Name => "Boundary Hi-Res Image";

    //Return a 24x24 pixel bitmap to represent this GHA library.
    public override Bitmap Icon => typeof(BoundaryHighResImageInfo).Assembly.GetBitmap("icon24.png");

    //Return a short string describing the purpose of this GHA library.
    public override string Description => "Create a hi-res image asap. Icon was created by Dorman.";

    public override Guid Id => new ("2c5f0c76-645b-4037-a4cb-79d100f2e4be");

    //Return a string identifying you or your company.
    public override string AuthorName => "秋水";

    //Return a string representing your preferred contact details.
    public override string AuthorContact => "1123993881@qq.com";

    public override string Version => typeof(BoundaryHighResImageInfo).Assembly.GetName().Version?.ToString() ?? "unknown";
}

partial class SimpleAssemblyPriority
{
    protected override int? MenuIndex => 0;

    protected override int InsertIndex => 10;

    protected override void DoWithEditor(GH_DocumentEditor editor)
    {
        var harmony = new Harmony("Grasshopper.BoundaryHighResImage");
        harmony.PatchAll();
        base.DoWithEditor(editor);
    }
}
