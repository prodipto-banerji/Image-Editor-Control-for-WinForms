using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Windows.Forms;
using SkiaSharp;

namespace CraftSynth.ImageEditor
{
    /// <summary>
    /// Line graphic object
    /// </summary>
    public class DrawLine : DrawObject
    {
        private SKPoint startPoint;
        private SKPoint endPoint;

        private const string entryStart = "Start";
        private const string entryEnd = "End";

        /// <summary>
        /// Graphic objects for hit test
        /// </summary>
        private SKPath areaPath = null;
        private SKPaint areaPaint = null;
        private SKRegion areaRegion = null;

        private bool _disposed = false;

        public DrawLine()
        {
            startPoint = new SKPoint(0, 0);
            endPoint = new SKPoint(1, 1);
            ZOrder = 0;

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

        ~DrawLine()
        {
            this.Dispose(false);
        }
        #endregion

        public DrawLine(int x1, int y1, int x2, int y2, System.Drawing.Color lineColor, int lineWidth, DrawingPens.PenType penType, System.Drawing.Drawing2D.LineCap endCap)
        {
            startPoint = new SKPoint(x1, y1);
            endPoint = new SKPoint(x2, y2);
            Color = lineColor;
            PenWidth = lineWidth;
            PenType = penType;
            EndCap = endCap;
            ZOrder = 0;
            TipText = String.Format("Line Start @ {0}-{1}, End @ {2}-{3}", x1, y1, x2, y2);

            Initialize();
        }

        public override void Draw(System.Drawing.Graphics g)
        {
            // Legacy method signature maintained for compatibility
            // In a real-world scenario, you would want to change the base class to use SKCanvas
            // For now, this method creates a temporary surface to draw on
            throw new NotSupportedException("Use DrawSkia method instead for SkiaSharp rendering");
        }

        /// <summary>
        /// Draw using SkiaSharp canvas
        /// </summary>
        /// <param name="canvas">SKCanvas to draw on</param>
        public void DrawSkia(SKCanvas canvas)
        {
            using (var paint = new SKPaint())
            {
                paint.IsAntialias = true;
                paint.Color = ColorToSKColor(Color);
                paint.StrokeWidth = PenWidth;
                paint.Style = SKPaintStyle.Stroke;
                paint.StrokeCap = ConvertLineCap(EndCap);

                // Apply pen type styling if DrawPen is set
                ApplyPenType(paint, PenType);

                using (var path = new SKPath())
                {
                    path.MoveTo(startPoint);
                    path.LineTo(endPoint);

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

        /// <summary>
        /// Clone this instance
        /// </summary>
        public override DrawObject Clone()
        {
            DrawLine drawLine = new DrawLine();
            drawLine.startPoint = startPoint;
            drawLine.endPoint = endPoint;

            FillDrawObjectFields(drawLine);
            return drawLine;
        }

        public override int HandleCount
        {
            get { return 2; }
        }

        /// <summary>
        /// Get handle point by 1-based number
        /// </summary>
        /// <param name="handleNumber"></param>
        /// <returns></returns>
        public override System.Drawing.Point GetHandle(int handleNumber)
        {
            using (var path = new SKPath())
            {
                path.MoveTo(startPoint);
                path.LineTo(endPoint);

                var bounds = path.Bounds;
                var centerX = bounds.Left + (bounds.Width / 2);
                var centerY = bounds.Top + (bounds.Height / 2);

                // Apply rotation if needed
                SKPoint start = startPoint;
                SKPoint end = endPoint;

                if (Rotation != 0)
                {
                    start = RotatePoint(startPoint, centerX, centerY, Rotation);
                    end = RotatePoint(endPoint, centerX, centerY, Rotation);
                }

                if (handleNumber == 1)
                    return new System.Drawing.Point((int)start.X, (int)start.Y);
                else
                    return new System.Drawing.Point((int)end.X, (int)end.Y);
            }
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

            // OK, so the point is not on a selection handle, is it anywhere else on the line?
            if (PointInObject(point))
                return 0;

            return -1;
        }

        protected override bool PointInObject(System.Drawing.Point point)
        {
            CreateObjects();
            return AreaRegion.Contains((int)point.X, (int)point.Y);
        }

        public override System.Drawing.Rectangle GetBounds(System.Drawing.Graphics g)
        {
            CreateObjects();
            var bounds = areaRegion.Bounds;
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
            var skRect = new SKRectI(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
            return AreaRegion.Intersects(skRect);
        }

        public override Cursor GetHandleCursor(int handleNumber)
        {
            switch (handleNumber)
            {
                case 1:
                case 2:
                    return Cursors.SizeAll;
                default:
                    return Cursors.Default;
            }
        }

        public override void MoveHandleTo(System.Drawing.Point point, int handleNumber)
        {
            if (handleNumber == 1)
                startPoint = new SKPoint(point.X, point.Y);
            else
                endPoint = new SKPoint(point.X, point.Y);

            Dirty = true;
            Invalidate();
        }

        public override void Move(int deltaX, int deltaY)
        {
            startPoint = new SKPoint(startPoint.X + deltaX, startPoint.Y + deltaY);
            endPoint = new SKPoint(endPoint.X + deltaX, endPoint.Y + deltaY);
            
            Dirty = true;
            Invalidate();
        }

        public override void SaveToStream(SerializationInfo info, int orderNumber, int objectIndex)
        {
            // Convert SKPoint to System.Drawing.Point for serialization compatibility
            info.AddValue(
                String.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryStart, orderNumber, objectIndex),
                new System.Drawing.Point((int)startPoint.X, (int)startPoint.Y));

            info.AddValue(
                String.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryEnd, orderNumber, objectIndex),
                new System.Drawing.Point((int)endPoint.X, (int)endPoint.Y));

            base.SaveToStream(info, orderNumber, objectIndex);
        }

        public override void LoadFromStream(SerializationInfo info, int orderNumber, int objectIndex)
        {
            var start = (System.Drawing.Point)info.GetValue(
                                    String.Format(CultureInfo.InvariantCulture,
                                                  "{0}{1}-{2}",
                                                  entryStart, orderNumber, objectIndex),
                                    typeof(System.Drawing.Point));

            var end = (System.Drawing.Point)info.GetValue(
                                String.Format(CultureInfo.InvariantCulture,
                                              "{0}{1}-{2}",
                                              entryEnd, orderNumber, objectIndex),
                                typeof(System.Drawing.Point));

            startPoint = new SKPoint(start.X, start.Y);
            endPoint = new SKPoint(end.X, end.Y);

            base.LoadFromStream(info, orderNumber, objectIndex);
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
        /// Create graphic objects used for hit test.
        /// </summary>
        protected virtual void CreateObjects()
        {
            if (AreaPath != null)
                return;

            // Create path which contains wide line for easy mouse selection
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

            // Prevent crash when startPoint == endPoint
            SKPoint adjustedEndPoint = endPoint;
            if (startPoint.Equals(endPoint))
            {
                adjustedEndPoint = new SKPoint(endPoint.X + 1, endPoint.Y + 1);
            }

            AreaPath.MoveTo(startPoint);
            AreaPath.LineTo(adjustedEndPoint);

            // Create a stroked path for hit testing
            SKPath strokedPath = AreaPaint.GetFillPath(AreaPath);
            if (strokedPath != null)
            {
                AreaPath.Dispose();
                AreaPath = ClonePath(strokedPath);
                strokedPath.Dispose();
            }

            // Rotate the path about its center if necessary
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

        protected SKPath AreaPath
        {
            get { return areaPath; }
            set { areaPath = value; }
        }

        protected SKPaint AreaPaint
        {
            get { return areaPaint; }
            set { areaPaint = value; }
        }

        protected SKRegion AreaRegion
        {
            get { return areaRegion; }
            set { areaRegion = value; }
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
        /// Clone an SKPath manually since Clone() doesn't exist
        /// </summary>
        private SKPath ClonePath(SKPath source)
        {
            if (source == null)
                return null;

            var clone = new SKPath(source);
            return clone;
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
            switch (penType)
            {
                case DrawingPens.PenType.Solid:
                    // Default - no dash effect needed
                    break;
                case DrawingPens.PenType.Dash:
                    paint.PathEffect = SKPathEffect.CreateDash(new float[] { 10, 5 }, 0);
                    break;
                case DrawingPens.PenType.Dot:
                    paint.PathEffect = SKPathEffect.CreateDash(new float[] { 2, 2 }, 0);
                    break;
                case DrawingPens.PenType.Dash_Dot:
                    paint.PathEffect = SKPathEffect.CreateDash(new float[] { 10, 5, 2, 5 }, 0);
                    break;
                case DrawingPens.PenType.DoubleLine:
                    // SkiaSharp doesn't have direct compound array support like GDI+
                    // We'll implement this by drawing two lines with different stroke widths
                    // This would need to be handled at the drawing level
                    break;
                default:
                    break;
            }
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