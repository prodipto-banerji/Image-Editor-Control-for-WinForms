using System.Windows.Forms;
using SkiaSharp;

namespace CraftSynth.ImageEditor
{
    /// <summary>
    /// Ellipse tool
    /// </summary>
    internal class ToolEllipse : ToolRectangle
    {
        public ToolEllipse()
        {
            Cursor = new Cursor(GetType(), "Ellipse.cur");
        }

        public override void OnMouseDown(DrawArea drawArea, MouseEventArgs e)
        {
            SKPoint p = drawArea.BackTrackMouse(new SKPoint(e.X, e.Y));
            AddNewObject(drawArea, new DrawEllipse((int)p.X, (int)p.Y, 1, 1, drawArea.LineColor, drawArea.FillColor, drawArea.DrawFilled, drawArea.LineWidth, drawArea.PenType, drawArea.EndCap));
        }
    }
}