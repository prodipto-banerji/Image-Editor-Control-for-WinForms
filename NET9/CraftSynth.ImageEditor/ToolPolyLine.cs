using System.Windows.Forms;
using SkiaSharp;

namespace CraftSynth.ImageEditor
{
    /// <summary>
    /// PolyLine tool (a PolyLine is a series of connected straight lines where each line is drawn individually)
    /// </summary>
    internal class ToolPolyLine : ToolObject
    {
        public ToolPolyLine()
        {
            Cursor = new Cursor(GetType(), "Pencil.cur");
        }

        private DrawPolyLine? newPolyLine;
        private bool _drawingInProcess = false; // Set to true when drawing

        /// <summary>
        /// Left mouse button is pressed
        /// </summary>
        /// <param name="drawArea"></param>
        /// <param name="e"></param>
        public override void OnMouseDown(DrawArea drawArea, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                _drawingInProcess = false;
                newPolyLine = null;
            }
            else
            {
                // Convert System.Drawing.Point to SKPoint for SkiaSharp compatibility
                var originalPoint = new System.Drawing.Point(e.X, e.Y);
                var backTrackedPoint = drawArea.BackTrackMouse(originalPoint);
                var skPoint = new SKPoint(backTrackedPoint.X, backTrackedPoint.Y);
                
                if (_drawingInProcess == false)
                {
                    newPolyLine = new DrawPolyLine(
                        (int)skPoint.X, 
                        (int)skPoint.Y, 
                        (int)skPoint.X + 1, 
                        (int)skPoint.Y + 1, 
                        drawArea.LineColor, 
                        drawArea.LineWidth, 
                        drawArea.PenType);
                    
                    newPolyLine.EndPoint = new System.Drawing.Point((int)skPoint.X + 1, (int)skPoint.Y + 1);
                    AddNewObject(drawArea, newPolyLine);
                    _drawingInProcess = true;
                }
                else
                {
                    // Drawing is in process, so simply add a new point
                    newPolyLine.AddPoint(backTrackedPoint);
                    newPolyLine.EndPoint = backTrackedPoint;
                }
            }
        }

        /// <summary>
        /// Mouse move - resize new polyline
        /// </summary>
        /// <param name="drawArea"></param>
        /// <param name="e"></param>
        public override void OnMouseMove(DrawArea drawArea, MouseEventArgs e)
        {
            drawArea.Cursor = Cursor;

            if (e.Button != MouseButtons.Left)
                return;

            if (newPolyLine == null)
                return; // precaution

            var originalPoint = new System.Drawing.Point(e.X, e.Y);
            var point = drawArea.BackTrackMouse(originalPoint);
            
            // Move last point
            newPolyLine.MoveHandleTo(point, newPolyLine.HandleCount);
            drawArea.Refresh();
        }

        #region Destruction
        private bool _disposed = false;

        protected override void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    // Free any managed objects here. 
                    if (this.newPolyLine != null)
                    {
                        this.newPolyLine.Dispose();
                        this.newPolyLine = null;
                    }
                }

                // Free any unmanaged objects here. 
                
                this._disposed = true;
            }
            base.Dispose(disposing);
        }

        ~ToolPolyLine()
        {
            this.Dispose(false);
        }
        #endregion
    }
}