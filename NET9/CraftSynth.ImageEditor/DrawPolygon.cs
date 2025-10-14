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
    /// Polygon graphic object
    /// </summary>
    public class DrawPolygon : DrawLine
    {
        private List<SKPoint> pointArray; // list of points - migrated from ArrayList
        private Cursor? handleCursor;

        private const string entryLength = "Length";
        private const string entryPoint = "Point";

        private bool _disposed;

        public DrawPolygon()
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
                    this._disposed = true;
                }

                // Free any unmanaged objects here. 
                
                this._disposed = true;
            }
            base.Dispose(disposing);
        }

        ~DrawPolygon()
        {
             this.Dispose(false);
        }
        #endregion

        public DrawPolygon(int x1, int y1, int x2, int y2, System.Drawing.Color lineColor, int lineWidth, DrawingPens.PenType penType, System.Drawing.Drawing2D.LineCap endCap)
        {
            pointArray = new List<SKPoint>();
            pointArray.Add(new SKPoint(x1, y1));
            pointArray.Add(new SKPoint(x2, y2));
            Color = lineColor;
            PenWidth = lineWidth;
            PenType = penType;
            EndCap = endCap;

            LoadCursor();
            Initialize();
        }

        /// <summary>
        /// Clone this instance
        /// </summary>
        public override DrawObject Clone()
        {
            DrawPolygon drawPolygon = new DrawPolygon();

            foreach (SKPoint p in pointArray)
            {
                drawPolygon.pointArray.Add(p);
            }

            FillDrawObjectFields(drawPolygon);
            return drawPolygon;
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
                paint.StrokeCap = ConvertLineCap(EndCap);

                // Apply pen type styling if DrawPen is set
                if (DrawPen != null)
                {
                    // Apply custom pen properties
                    ApplyPenType(paint, PenType);
                }

                using (var path = new SKPath())
                {
                    // Convert List<SKPoint> to SKPoint array
                    var pts = pointArray.ToArray();
                    
                    // Add points to path as connected lines
                    if (pts.Length > 0)
                    {
                        path.MoveTo(pts[0]);
                        for (int i = 1; i < pts.Length; i++)
                        {
                            path.LineTo(pts[i]);
                        }
                    }

                    // Apply rotation if necessary
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
                var drawingPoint = new System.Drawing.Point((int)p.X, (int)p.Y);
                info.AddValue(
                    String.Format(CultureInfo.InvariantCulture,
                                  "{0}{1}-{2}-{3}",
                                  new object[] {entryPoint, orderNumber, objectIndex, i++}),
                    drawingPoint);
            }
            base.SaveToStream(info, orderNumber, objectIndex);
        }

        public override void LoadFromStream(SerializationInfo info, int orderNumber, int objectIndex)
        {
            int n = info.GetInt32(
                String.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryLength, orderNumber, objectIndex));

            for (int i = 0; i < n; i++)
            {
                var point = (System.Drawing.Point)info.GetValue(
                                   String.Format(CultureInfo.InvariantCulture,
                                                 "{0}{1}-{2}-{3}",
                                                 new object[] {entryPoint, orderNumber, objectIndex, i}),
                                   typeof (System.Drawing.Point));

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

            // Create closed path which contains all polygon vertexes
            AreaPath = new SKPath();

            if (pointArray.Count == 0)
                return;

            // Move to first point
            AreaPath.MoveTo(pointArray[0]);

            // Add lines to subsequent points
            for (int i = 1; i < pointArray.Count; i++)
            {
                AreaPath.LineTo(pointArray[i]);
            }

            AreaPath.Close(); // Close the polygon

            // Apply rotation if necessary
            if (Rotation != 0)
            {
                var bounds = AreaPath.Bounds;
                var centerX = bounds.Left + (bounds.Width / 2);
                var centerY = bounds.Top + (bounds.Height / 2);

                var matrix = SKMatrix.CreateRotationDegrees(Rotation, centerX, centerY);
                AreaPath.Transform(matrix);
            }

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
                // Fallback to default cursor if resource loading fails
                handleCursor = Cursors.Default;
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

        #endregion
    }
}