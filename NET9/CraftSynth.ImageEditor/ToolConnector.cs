using System;
using System.Windows.Forms;
using SkiaSharp;

namespace CraftSynth.ImageEditor
{
    /// <summary>
    /// Connector tool (a Connector is a series of connected straight lines where each line is drawn individually and at least one of the ends is anchored to another object)
    /// </summary>
    internal class ToolConnector : ToolObject
    {
        public ToolConnector()
        {
            Cursor = new Cursor(GetType(), "Pencil.cur");
        }

        private DrawConnector newConnector;
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
                newConnector = null;
            }
            else
            {
                var skPoint = drawArea.BackTrackMouse(new SKPoint(e.X, e.Y));
                int objectID = -1;
                skPoint = TestForConnection(drawArea, skPoint, out objectID);

                if (_drawingInProcess == false)
                {
                    newConnector = new DrawConnector(
                        (int)skPoint.X, 
                        (int)skPoint.Y, 
                        (int)skPoint.X + 1, 
                        (int)skPoint.Y + 1, 
                        drawArea.LineColor, 
                        drawArea.LineWidth, 
                        drawArea.PenType, 
                        drawArea.EndCap);
                    
                    newConnector.EndPoint = new SKPoint(skPoint.X + 1, skPoint.Y + 1);
                    
                    if (objectID > -1)
                    {
                        newConnector.StartIsAnchored = true;
                        newConnector.StartObjectId = objectID;
                    }
                    
                    AddNewObject(drawArea, newConnector);
                    _drawingInProcess = true;
                }
                else
                {
                    // Drawing is in process, so simply add a new point
                    newConnector.AddPoint(skPoint);
                    newConnector.EndPoint = skPoint;
                    
                    if (objectID > -1)
                    {
                        newConnector.EndIsAnchored = true;
                        newConnector.EndObjectId = objectID;
                        _drawingInProcess = false;
                    }
                }
            }
        }

        private static SKPoint TestForConnection(DrawArea drawArea, SKPoint p, out int objectID)
        {
            // Determine if within 5 pixels of a connection point
            // Step 1: see if a 5 x 5 rectangle centered on the mouse cursor intersects with an object
            // Step 2: If it does, then see if there is a connection point within the rectangle
            // Step 3: If there is, move the point to the connection point, record the object's id in the connector
            //
            objectID = -1;
            var testRectangle = new SKRect(p.X - 2, p.Y - 2, p.X + 3, p.Y + 3);
            int al = drawArea.TheLayers.ActiveLayerIndex;
            bool connectionHere = false;
            var h = new SKPoint(-1, -1);
            GraphicsList gl = drawArea.TheLayers[al].Graphics;
            
            for (int i = 1; i < gl.Count; i++)
            {
                if (gl[i].IntersectsWith(testRectangle))
                {
                    DrawObject obj = (DrawObject)gl[i];
                    for (int j = 1; j < obj.HandleCount + 1; j++)
                    {
                        h = obj.GetHandle(j);
                        if (testRectangle.Contains(h.X, h.Y))
                        {
                            connectionHere = true;
                            p = h;
                            objectID = obj.ID;
                            // obj.DrawConnection(drawArea., j);
                            break;
                        }
                    }
                }
                if (connectionHere)
                    break;
            }
            return p;
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

            if (newConnector == null)
                return; // precaution

            var skPoint = drawArea.BackTrackMouse(new SKPoint(e.X, e.Y));
            int objectID;
            skPoint = TestForConnection(drawArea, skPoint, out objectID);
            
            // move last point
            newConnector.MoveHandleTo(skPoint, newConnector.HandleCount);
            drawArea.Refresh();
            
            if (objectID > -1)
            {
                newConnector.EndIsAnchored = true;
                newConnector.EndObjectId = objectID;
                _drawingInProcess = false;
            }
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
                    if (this.newConnector != null)
                    {
                        this.newConnector.Dispose();
                    }
                }

                // Free any unmanaged objects here. 
                
                this._disposed = true;
            }
            base.Dispose(disposing);
        }

        ~ToolConnector()
        {
            this.Dispose(false);
        }
        #endregion
    }
}