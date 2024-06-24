using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI.Canvas.Interaction;
using HarmonyLib;
using System.Windows.Forms;

namespace BoundaryHighResImage;

[HarmonyPatch(typeof(GH_Canvas))]
internal class CanvasPatch
{
    [HarmonyPatch("MouseDown_DefaultBehaviour")]
    static void Postfix(GH_Canvas __instance, GH_CanvasMouseEvent e)
    {
        if (__instance.ActiveInteraction is not GH_WindowSelectInteraction) return;
        if (Control.ModifierKeys != Keys.Control) return;
        __instance.ActiveInteraction = new GH_WindowImageInteraction(__instance, e);
    }
}
