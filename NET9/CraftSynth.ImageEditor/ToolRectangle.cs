using System.Windows.Forms;
using SkiaSharp;

namespace CraftSynth.ImageEditor
{
    /// <summary>
    /// Rectangle tool for .NET 9 with SkiaSharp
    /// </summary>
    internal class ToolRectangle : ToolObject
    {
        public ToolRectangle()
        {
            Cursor = new Cursor(GetType(), "Rectangle.cur");
        }

        /// <summary>
        /// Left mouse button is pressed - create new rectangle
        /// </summary>
        /// <param name="drawArea">Drawing area</param>
        /// <param name="e">Mouse event arguments</param>
        public override void OnMouseDown(DrawArea drawArea, MouseEventArgs e)
        {
            // Convert screen coordinates to canvas coordinates
            var screenPoint = new System.Drawing.Point(e.X, e.Y);
            var canvasPoint = drawArea.BackTrackMouse(screenPoint);
            
            // Create new rectangle with current drawing settings
            var newRectangle = new DrawRectangle(
                canvasPoint.X, 
                canvasPoint.Y, 
                1, 
                1, 
                drawArea.LineColor, 
                drawArea.FillColor, 
                drawArea.DrawFilled, 
                drawArea.LineWidth, 
                drawArea.PenType, 
                drawArea.EndCap
            );

            // Add the new rectangle to the active layer
            AddNewObject(drawArea, newRectangle);
        }

        /// <summary>
        /// Mouse is moved - resize rectangle during creation
        /// </summary>
        /// <param name="drawArea">Drawing area</param>
        /// <param name="e">Mouse event arguments</param>
        public override void OnMouseMove(DrawArea drawArea, MouseEventArgs e)
        {
            // Set the appropriate cursor
            drawArea.Cursor = Cursor;
            
            // Get active layer index
            int activeLayerIndex = drawArea.TheLayers.ActiveLayerIndex;
            
            // If left mouse button is pressed, resize the rectangle
            if (e.Button == MouseButtons.Left)
            {
                // Convert screen coordinates to canvas coordinates
                var screenPoint = new System.Drawing.Point(e.X, e.Y);
                var canvasPoint = drawArea.BackTrackMouse(screenPoint);
                
                // Move handle 5 (bottom-right corner) to current mouse position
                // This resizes the rectangle as the user drags
                var graphics = drawArea.TheLayers[activeLayerIndex].Graphics;
                if (graphics.Count > 0)
                {
                    graphics[0].MoveHandleTo(canvasPoint, 5);
                    drawArea.Refresh();
                }
            }
        }
    }
}