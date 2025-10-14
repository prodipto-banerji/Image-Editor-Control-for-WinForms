using System;
using System.Windows.Forms;
using SkiaSharp;

namespace CraftSynth.ImageEditor
{
    /// <summary>
    /// Polygon tool - draws freehand polygons
    /// </summary>
    internal class ToolPolygon : ToolObject
    {
        private int lastX;
        private int lastY;
        private DrawPolygon? newPolygon;
        private int minDistance = 15 * 15;
        private bool _disposed = false;

        public ToolPolygon()
        {
            Cursor = new Cursor(GetType(), "Pencil.cur");
        }

        /// <summary>
        /// Left mouse button is pressed
        /// </summary>
        /// <param name="drawArea"></param>
        /// <param name="e"></param>
        public override void OnMouseDown(DrawArea drawArea, MouseEventArgs e)
        {
            // Create new polygon, add it to the list
            // and keep reference to it
            var screenPoint = new System.Drawing.Point(e.X, e.Y);
            var canvasPoint = drawArea.BackTrackMouse(screenPoint);
            
            // Convert System.Drawing.Point to SKPoint for the polygon
            var skPoint = new SKPoint(canvasPoint.X, canvasPoint.Y);
            var skEndPoint = new SKPoint(canvasPoint.X + 1, canvasPoint.Y + 1);
            
            newPolygon = new DrawPolygon(
                skPoint, 
                skEndPoint, 
                drawArea.LineColor, 
                drawArea.LineWidth, 
                drawArea.PenType, 
                drawArea.EndCap
            );
            
            // Set the minimum distance variable according to current zoom level.
            minDistance = Convert.ToInt32((15 * drawArea.Zoom) * (15 * drawArea.Zoom));

            AddNewObject(drawArea, newPolygon);
            lastX = e.X;
            lastY = e.Y;
        }

        /// <summary>
        /// Mouse move - resize new polygon
        /// </summary>
        /// <param name="drawArea"></param>
        /// <param name="e"></param>
        public override void OnMouseMove(DrawArea drawArea, MouseEventArgs e)
        {
            drawArea.Cursor = Cursor;

            if (e.Button != MouseButtons.Left)
                return;

            if (newPolygon == null)
                return; // precaution

            var screenPoint = new System.Drawing.Point(e.X, e.Y);
            var canvasPoint = drawArea.BackTrackMouse(screenPoint);
            var skPoint = new SKPoint(canvasPoint.X, canvasPoint.Y);
            
            int distance = (e.X - lastX) * (e.X - lastX) + (e.Y - lastY) * (e.Y - lastY);

            if (distance < minDistance)
            {
                // Distance between last two points is less than minimum -
                // move last point
                newPolygon.MoveHandleTo(skPoint, newPolygon.HandleCount);
            }
            else
            {
                // Add new point
                newPolygon.AddPoint(skPoint);
                lastX = e.X;
                lastY = e.Y;
            }
            drawArea.Refresh();
        }

        public override void OnMouseUp(DrawArea drawArea, MouseEventArgs e)
        {
            newPolygon = null;
            base.OnMouseUp(drawArea, e);
        }

        #region Destruction
        protected override void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    // Free any managed objects here. 
                    if (this.newPolygon != null)
                    {
                        this.newPolygon.Dispose();
                        this.newPolygon = null;
                    }
                }

                // Free any unmanaged objects here. 
                
                this._disposed = true;
            }
            base.Dispose(disposing);
        }

        ~ToolPolygon()
        {
            this.Dispose(false);
        }
        #endregion
    }
}