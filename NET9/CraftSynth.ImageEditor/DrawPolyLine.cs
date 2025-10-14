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
    /// PolyLine graphic object - a PolyLine is a series of connected lines
    /// </summary>
    public class DrawPolyLine : DrawLine
    {
        // Last Segment start and end points
        private SKPoint startPoint;
        private SKPoint endPoint;

        private List<SKPoint> pointArray; // list of points
        private Cursor? handleCursor;

        private const string entryLength = "Length";
        private const string entryPoint = "Point";

        private bool _disposed;

        /// <summary>
        /// Graphic objects for hit test
        /// </summary>
        private SKPath? areaPath = null;
        private SKPaint? areaPaint = null;
        private SKRegion? areaRegion = null;

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
            DrawPolyLine drawPolyLine = new DrawPolyLine();

            drawPolyLine.startPoint = startPoint;
            drawPolyLine.endPoint = endPoint;
            drawPolyLine.pointArray = new List<SKPoint>(pointArray);

            FillDrawObjectFields(drawPolyLine);
            return drawPolyLine;
        }

        public DrawPolyLine()
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
                    }
                    if (this.areaPath != null)
                    {
                        this.areaPath.Dispose();
                    }
                    if (this.areaPaint != null)
                    {
                        this.areaPaint.Dispose();
                    }
                    if (this.areaRegion != null)
                    {
                        this.areaRegion.Dispose();
                    }
                }

                // Free any unmanaged objects here.
                
                this._disposed = true;
            }
            base.Dispose(disposing);
        }

        ~DrawPolyLine()
        {
            this.Dispose(false);
        }
        #endregion

        public DrawPolyLine(int x1, int y1, int x2, int y2, System.Drawing.Color lineColor, int lineWidth, DrawingPens.PenType penType)
        {
            pointArray = new List<SKPoint>();
            pointArray.Add(new SKPoint(x1, y1));
            pointArray.Add(new SKPoint(x2, y2));
            Color = lineColor;
            PenWidth = lineWidth;
            PenType = penType;

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
                paint.StrokeCap = ConvertLineCap(EndCap);

                // Apply pen type styling if DrawPen is set
                if (DrawPen != null)
                {
                    // Apply custom pen properties
                    ApplyPenType(paint, PenType);
                }

                using (var path = new SKPath())
                {
                    // Create path from point array
                    bool first = true;
                    foreach (var point in pointArray)
                    {
                        if (first)
                        {
                            path.MoveTo(point);
                            first = false;
                        }
                        else
                        {
                            path.LineTo(point);
                        }
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

            // Apply rotation if needed
            if (Rotation != 0)
            {
                using (var path = new SKPath())
                {
                    foreach (var point in pointArray)
                    {
                        if (path.IsEmpty)
                            path.MoveTo(point);
                        else
                            path.LineTo(point);
                    }

                    var bounds = path.Bounds;
                    var centerX = bounds.Left + (bounds.Width / 2);
                    var centerY = bounds.Top + (bounds.Height / 2);

                    skPoint = RotatePoint(skPoint, centerX, centerY, Rotation);
                }
            }

            return new System.Drawing.Point((int)skPoint.X, (int)skPoint.Y);
        }

        public override Cursor GetHandleCursor(int handleNumber)
        {
            return handleCursor ?? Cursors.SizeAll;
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
            for (int i = 0; i < pointArray.Count; i++)
            {
                var point = pointArray[i];
                pointArray[i] = new SKPoint(point.X + deltaX, point.Y + deltaY);
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
                                  new object[] { entryPoint, orderNumber, objectIndex, i++ }),
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
        /// Hit test.
        /// Return value: -1 - no hit
        ///                0 - hit anywhere
        ///                > 1 - handle number
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public override int HitTest(System.Drawing.Point point)
        {
            if (Selected)
            {
                for (int i = 1; i <= HandleCount; i++)
                {
                    var handleRect = GetHandleRectangle(i);
                    if (handleRect.Contains(point))
                        return i;
                }
            }

            // OK, so the point is not on a selection handle, is it anywhere else on the polyline?
            if (PointInObject(point))
                return 0;

            return -1;
        }

        protected override bool PointInObject(System.Drawing.Point point)
        {
            CreateObjects();
            return AreaRegion?.Contains((int)point.X, (int)point.Y) ?? false;
        }

        public override System.Drawing.Rectangle GetBounds(System.Drawing.Graphics g)
        {
            CreateObjects();
            if (AreaRegion == null)
                return System.Drawing.Rectangle.Empty;

            var bounds = AreaRegion.Bounds;
            var normalizedBounds = GetNormalizedRectangle(bounds);
            
            return new System.Drawing.Rectangle(
                (int)Math.Floor(normalizedBounds.Left),
                (int)Math.Floor(normalizedBounds.Top),
                (int)Math.Ceiling(normalizedBounds.Width),
                (int)Math.Ceiling(normalizedBounds.Height)
            );
        }

        public override bool IntersectsWith(System.Drawing.Rectangle rectangle)
        {
            CreateObjects();
            if (AreaRegion == null)
                return false;

            var skRect = new SKRectI(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
            return AreaRegion.Intersects(skRect);
        }

        /// <summary>
        /// Invalidate object.
        /// When object is invalidated, path used for hit test
        /// is released and should be created again.
        /// </summary>
        protected void Invalidate()
        {
            if (AreaPath != null)
            {
                AreaPath.Dispose();
                AreaPath = null;
            }

            if (AreaPaint != null)
            {
                AreaPaint.Dispose();
                AreaPaint = null;
            }

            if (AreaRegion != null)
            {
                AreaRegion.Dispose();
                AreaRegion = null;
            }
        }

        /// <summary>
        /// Create graphic object used for hit test
        /// </summary>
        protected virtual void CreateObjects()
        {
            if (AreaPath != null)
                return;

            if (pointArray.Count < 2)
                return;

            // Create path which contains wide polyline for easy mouse selection
            AreaPath = new SKPath();
            
            // Take into account the width of the pen used to draw the actual object
            float strokeWidth = PenWidth < 7 ? 7 : PenWidth;
            
            AreaPaint = new SKPaint
            {
                Color = SKColors.Black,
                StrokeWidth = strokeWidth,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            };

            // Create the polyline path
            bool first = true;
            foreach (var point in pointArray)
            {
                if (first)
                {
                    AreaPath.MoveTo(point);
                    first = false;
                }
                else
                {
                    AreaPath.LineTo(point);
                }
            }

            // Create a stroked path for hit testing
            SKPath? strokedPath = AreaPaint.GetFillPath(AreaPath);
            if (strokedPath != null)
            {
                AreaPath.Dispose();
                AreaPath = new SKPath(strokedPath);
                strokedPath.Dispose();
            }

            // Rotate the path about its center if necessary
            if (Rotation != 0)
            {
                var bounds = AreaPath.Bounds;
                var centerX = bounds.Left + (bounds.Width / 2);
                var centerY = bounds.Top + (bounds.Height / 2);

                var matrix = SKMatrix.CreateRotationDegrees(Rotation, centerX, centerY);
                AreaPath.Transform(in matrix);
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

        protected SKPath? AreaPath
        {
            get { return areaPath; }
            set { areaPath = value; }
        }

        protected SKPaint? AreaPaint
        {
            get { return areaPaint; }
            set { areaPaint = value; }
        }

        protected SKRegion? AreaRegion
        {
            get { return areaRegion; }
            set { areaRegion = value; }
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
                handleCursor = Cursors.SizeAll;
            }
        }

        #region Helper Functions

        public static SKRect GetNormalizedRectangle(float x1, float y1, float x2, float y2)
        {
            if (x2 < x1)
            {
                float tmp = x2;
                x2 = x1;
                x1 = tmp;
            }

            if (y2 < y1)
            {
                float tmp = y2;
                y2 = y1;
                y1 = tmp;
            }
            return new SKRect(x1, y1, x2, y2);
        }

        public static SKRect GetNormalizedRectangle(SKPoint p1, SKPoint p2)
        {
            return GetNormalizedRectangle(p1.X, p1.Y, p2.X, p2.Y);
        }

        public static SKRect GetNormalizedRectangle(SKRect r)
        {
            return GetNormalizedRectangle(r.Left, r.Top, r.Right, r.Bottom);
        }

        public static SKRect GetNormalizedRectangle(SKRectI r)
        {
            return GetNormalizedRectangle(r.Left, r.Top, r.Right, r.Bottom);
        }

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

        /// <summary>
        /// Rotate a point around a center point
        /// </summary>
        private SKPoint RotatePoint(SKPoint point, float centerX, float centerY, float angleDegrees)
        {
            float angleRadians = angleDegrees * (float)Math.PI / 180f;
            float cos = (float)Math.Cos(angleRadians);
            float sin = (float)Math.Sin(angleRadians);

            float dx = point.X - centerX;
            float dy = point.Y - centerY;

            return new SKPoint(
                centerX + (dx * cos - dy * sin),
                centerY + (dx * sin + dy * cos)
            );
        }

        #endregion
    }
}