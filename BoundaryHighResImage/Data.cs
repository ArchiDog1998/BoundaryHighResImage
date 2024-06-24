using SimpleGrasshopper.Attributes;
using System;
using System.Drawing;

namespace BoundaryHighResImage;
internal static partial class Data
{
    [Range(1, 5, 1)]
    [Setting, Config("Zoom Factor")]
    private static readonly float _ZoomFactor = 2;

    [Setting, Config("Canvas Color")]
    private static readonly Color _CanvasColor = Color.Transparent;

    [Setting, Config("Save")]
    private static readonly bool _Save = false;
}
