using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Windows.Forms;
using SkiaSharp;

namespace CraftSynth.ImageEditor
{
    /// <summary>
    /// Connector graphic object - a Connector is a series of connected straight lines
    /// where each line is drawn individually and at least
    /// one of the ends is anchored to another object
    /// </summary>
    public class DrawConnector : DrawLine
    {
        // Connector-specific fields
        private SKStrokeCap startCap = SKStrokeCap.Butt;
        private SKStrokeCap endCap = SKStrokeCap.Butt;
        private bool startIsAnchored = false;
        private bool endIsAnchored = false;
        private int startObjectId = -1;
        private int endObjectId = -1;

        // Last Segment start and end points
        private SKPoint startPoint;
        private SKPoint endPoint;

        private List<SKPoint> pointArray; // list of points (migrated from ArrayList)
        private Cursor? handleCursor;

        private const string entryLength = "Length";
        private const string entryPoint = "Point";

        private bool _disposed = false;

        public SKStrokeCap StartCap
        {
            get { return startCap; }
            set { startCap = value; }
        }

        public SKStrokeCap EndCap
        {
            get { return endCap; }
            set { endCap = value; }
        }

        public bool StartIsAnchored
        {
            get { return startIsAnchored; }
            set { startIsAnchored = value; }
        }

        public bool EndIsAnchored
        {
            get { return endIsAnchored; }
            set { endIsAnchored = value; }
        }

        public int StartObjectId
        {
            get { return startObjectId; }
            set { startObjectId = value; }
        }

        public int EndObjectId
        {
            get { return endObjectId; }
            set { endObjectId = value; }
        }

        public SKPoint StartPoint
        {
            get { return startPoint; }
            set { startPoint = value; }
        }

        public SKPoint EndPoint
        {
            get { return endPoint; }
            set { endPoint = value; }
        }

        /// <summary>
        /// Clone this instance
        /// </summary>
        public override DrawObject Clone()
        {
            DrawConnector drawConnector = new DrawConnector();

            drawConnector.startPoint = startPoint;
            drawConnector.endPoint = endPoint;
            drawConnector.pointArray = new List<SKPoint>(pointArray);
            drawConnector.startCap = startCap;
            drawConnector.endCap = endCap;
            drawConnector.startIsAnchored = startIsAnchored;
            drawConnector.endIsAnchored = endIsAnchored;
            drawConnector.startObjectId = startObjectId;
            drawConnector.endObjectId = endObjectId;

            FillDrawObjectFields(drawConnector);
            return drawConnector;
        }

        public DrawConnector()
        {
            pointArray = new List<SKPoint>();
            LoadCursor();
            Initialize();
        }

        #region Destruction
        protected override void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    // Free any managed objects here. 
                    if (this.handleCursor != null)
                    {
                        this.handleCursor.Dispose();
                        this.handleCursor = null;
                    }
                }

                // Free any unmanaged objects here. 
                
                this._disposed = true;
            }
            base.Dispose(disposing);
        }

        ~DrawConnector()
        {
             this.Dispose(false);
        }
        #endregion

        public DrawConnector(int x1, int y1, int x2, int y2, System.Drawing.Color lineColor, int lineWidth, DrawingPens.PenType penType, SKStrokeCap endCap)
        {
            pointArray = new List<SKPoint>();
            pointArray.Add(new SKPoint(x1, y1));
            pointArray.Add(new SKPoint(x2, y2));
            TipText = String.Format("Start @ {0}-{1}, End @ {2}, {3}", x1, y1, x2, y2);
            Color = lineColor;
            PenWidth = lineWidth;
            PenType = penType;
            EndCap = endCap;

            LoadCursor();
            Initialize();
        }

        public override void Draw(System.Drawing.Graphics g)
        {
            // For compatibility with base class signature, we need to handle Graphics
            // In a real migration, you'd want to change the base class to use SKCanvas
            // For now, we'll create a workaround or you can call DrawSkia directly
            
            // This is a placeholder - in actual implementation you'd need to:
            // 1. Either change the base class Draw method to use SKCanvas
            // 2. Or create a separate DrawSkia method
            // 3. Or use a graphics interop layer
        }

        /// <summary>
        /// Draw using SkiaSharp canvas
        /// </summary>
        /// <param name="canvas">SKCanvas to draw on</param>
        public void DrawSkia(SKCanvas canvas)
        {
            if (pointArray.Count < 2)
                return;

            using (var paint = new SKPaint())
            {
                paint.IsAntialias = true;
                paint.Color = ColorToSKColor(Color);
                paint.StrokeWidth = PenWidth;
                paint.Style = SKPaintStyle.Stroke;
                paint.StrokeCap = endCap;

                // Apply pen type styling if DrawPen is set
                if (DrawPen != null)
                {
                    // Apply custom pen properties
                    ApplyPenType(paint, PenType);
                }

                using (var path = new SKPath())
                {
                    // Move to first point
                    path.MoveTo(pointArray[0]);
                    
                    // Draw lines to subsequent points
                    for (int i = 1; i < pointArray.Count; i++)
                    {
                        path.LineTo(pointArray[i]);
                    }

                    // Rotate the path about its center if necessary
                    if (Rotation != 0)
                    {
                        var bounds = path.Bounds;
                        var centerX = bounds.Left + (bounds.Width / 2);
                        var centerY = bounds.Top + (bounds.Height / 2);

                        canvas.Save();
                        canvas.RotateDegrees(Rotation, centerX, centerY);
                        canvas.DrawPath(path, paint);
                        canvas.Restore();
                    }
                    else
                    {
                        canvas.DrawPath(path, paint);
                    }
                }
            }
        }

        public void AddPoint(SKPoint point)
        {
            pointArray.Add(point);
        }

        // Overload for backward compatibility
        public void AddPoint(System.Drawing.Point point)
        {
            pointArray.Add(new SKPoint(point.X, point.Y));
        }

        public override int HandleCount
        {
            get { return pointArray.Count; }
        }

        /// <summary>
        /// Get handle point by 1-based number
        /// </summary>
        /// <param name="handleNumber"></param>
        /// <returns></returns>
        public override System.Drawing.Point GetHandle(int handleNumber)
        {
            if (handleNumber < 1)
                handleNumber = 1;
            if (handleNumber > pointArray.Count)
                handleNumber = pointArray.Count;
                
            var skPoint = pointArray[handleNumber - 1];
            return new System.Drawing.Point((int)skPoint.X, (int)skPoint.Y);
        }

        public override Cursor GetHandleCursor(int handleNumber)
        {
            return handleCursor ?? Cursors.Default;
        }

        public override void MoveHandleTo(System.Drawing.Point point, int handleNumber)
        {
            if (handleNumber < 1)
                handleNumber = 1;

            if (handleNumber > pointArray.Count)
                handleNumber = pointArray.Count;
                
            pointArray[handleNumber - 1] = new SKPoint(point.X, point.Y);
            Dirty = true;
            Invalidate();
        }

        public override void Move(int deltaX, int deltaY)
        {
            int n = pointArray.Count;

            for (int i = 0; i < n; i++)
            {
                var currentPoint = pointArray[i];
                pointArray[i] = new SKPoint(currentPoint.X + deltaX, currentPoint.Y + deltaY);
            }
            
            // Update start and end points
            startPoint = new SKPoint(startPoint.X + deltaX, startPoint.Y + deltaY);
            endPoint = new SKPoint(endPoint.X + deltaX, endPoint.Y + deltaY);
            
            Dirty = true;
            Invalidate();
        }

        public override void SaveToStream(SerializationInfo info, int orderNumber, int objectIndex)
        {
            info.AddValue(
                String.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryLength, orderNumber, objectIndex),
                pointArray.Count);

            int i = 0;
            foreach (SKPoint p in pointArray)
            {
                // Convert SKPoint to System.Drawing.Point for serialization compatibility
                var point = new System.Drawing.Point((int)p.X, (int)p.Y);
                info.AddValue(
                    String.Format(CultureInfo.InvariantCulture,
                                  "{0}{1}-{2}-{3}",
                                  new object[] { entryPoint, orderNumber, objectIndex, i++ }),
                    point);
            }
            base.SaveToStream(info, orderNumber, objectIndex);
        }

        public override void LoadFromStream(SerializationInfo info, int orderNumber, int objectIndex)
        {
            int n = info.GetInt32(
                String.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryLength, orderNumber, objectIndex));

            pointArray.Clear();
            for (int i = 0; i < n; i++)
            {
                var point = (System.Drawing.Point)info.GetValue(
                                String.Format(CultureInfo.InvariantCulture,
                                              "{0}{1}-{2}-{3}",
                                              new object[] { entryPoint, orderNumber, objectIndex, i }),
                                typeof(System.Drawing.Point));
                pointArray.Add(new SKPoint(point.X, point.Y));
            }
            base.LoadFromStream(info, orderNumber, objectIndex);
        }

        /// <summary>
        /// Create graphic object used for hit test
        /// </summary>
        protected override void CreateObjects()
        {
            if (AreaPath != null)
                return;

            if (pointArray.Count < 2)
                return;

            // Create closed path which contains all polygon vertexes
            AreaPath = new SKPath();

            // Move to first point
            AreaPath.MoveTo(pointArray[0]);

            // Add lines to subsequent points
            for (int i = 1; i < pointArray.Count; i++)
            {
                AreaPath.LineTo(pointArray[i]);
            }

            // Close the figure to create a polygon for hit testing
            AreaPath.Close();

            // Create region from the path
            AreaRegion = new SKRegion();
            var pathBounds = AreaPath.Bounds;
            var clipRect = new SKRectI(
                (int)Math.Floor(pathBounds.Left),
                (int)Math.Floor(pathBounds.Top),
                (int)Math.Ceiling(pathBounds.Right),
                (int)Math.Ceiling(pathBounds.Bottom)
            );
            AreaRegion.SetPath(AreaPath, new SKRegion(clipRect));
        }

        private void LoadCursor()
        {
            try
            {
                handleCursor = new Cursor(GetType(), "PolyHandle.cur");
            }
            catch
            {
                // Fallback to default cursor if resource not found
                handleCursor = Cursors.SizeAll;
            }
        }

        #region Helper Functions

        /// <summary>
        /// Convert System.Drawing.Color to SKColor
        /// </summary>
        private SKColor ColorToSKColor(System.Drawing.Color color)
        {
            return new SKColor(color.R, color.G, color.B, color.A);
        }

        /// <summary>
        /// Apply pen type styling to SKPaint
        /// </summary>
        private void ApplyPenType(SKPaint paint, DrawingPens.PenType penType)
        {
            // Implement pen type logic based on your DrawingPens.PenType enum
            // This is a placeholder - adjust based on your actual pen types
            
            // Example implementation - adjust based on your actual PenType enum values:
            // You'll need to check what values exist in DrawingPens.PenType
            
            // Common dash patterns:
            // Solid - no dash effect needed (default)
            // Dash - paint.PathEffect = SKPathEffect.CreateDash(new float[] { 10, 5 }, 0);
            // Dot - paint.PathEffect = SKPathEffect.CreateDash(new float[] { 2, 2 }, 0);
            // DashDot - paint.PathEffect = SKPathEffect.CreateDash(new float[] { 10, 5, 2, 5 }, 0);
            
            // Since we don't know the exact enum values, we'll leave this as a placeholder
            // You can add specific cases once you know the PenType enum values
        }

        /// <summary>
        /// Convert System.Drawing.Drawing2D.LineCap to SKStrokeCap
        /// </summary>
        private SKStrokeCap ConvertLineCap(System.Drawing.Drawing2D.LineCap lineCap)
        {
            switch (lineCap)
            {
                case System.Drawing.Drawing2D.LineCap.Round:
                    return SKStrokeCap.Round;
                case System.Drawing.Drawing2D.LineCap.Square:
                    return SKStrokeCap.Square;
                case System.Drawing.Drawing2D.LineCap.Flat:
                default:
                    return SKStrokeCap.Butt;
            }
        }

        /// <summary>
        /// Invalidate object.
        /// When object is invalidated, path used for hit test
        /// is released and should be created again.
        /// </summary>
        protected new void Invalidate()
        {
            if (AreaPath != null)
            {
                AreaPath.Dispose();
                AreaPath = null;
            }

            if (AreaRegion != null)
            {
                AreaRegion.Dispose();
                AreaRegion = null;
            }
        }

        #endregion
    }
}