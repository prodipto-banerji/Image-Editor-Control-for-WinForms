using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;
using System.Windows.Forms;
using SkiaSharp;

namespace CraftSynth.ImageEditor
{
    /// <summary>
    /// Rectangle graphic object
    /// </summary>
    [Serializable]
    public class DrawRectangle : DrawObject
    {
        private SKRect rectangle;

        private const string entryRectangle = "Rect";

        protected SKRect Rectangle
        {
            get { return rectangle; }
            set { rectangle = value; }
        }

        /// <summary>
        /// Clone this instance
        /// </summary>
        public override DrawObject Clone()
        {
            DrawRectangle drawRectangle = new DrawRectangle();
            drawRectangle.rectangle = rectangle;

            FillDrawObjectFields(drawRectangle);
            return drawRectangle;
        }

        public DrawRectangle()
        {
            SetRectangle(0, 0, 1, 1);
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
                }

                // Free any unmanaged objects here. 
                
                this._disposed = true;
            }
            base.Dispose(disposing);
        }

        ~DrawRectangle()
        {
             this.Dispose(false);
        }
        #endregion

        public DrawRectangle(int x, int y, int width, int height, System.Drawing.Color lineColor, System.Drawing.Color fillColor, bool filled, int lineWidth, DrawingPens.PenType penType, System.Drawing.Drawing2D.LineCap endCap)
        {
            Center = new System.Drawing.Point(x + (width / 2), y + (height / 2));
            rectangle = SKRect.Create(x, y, width, height);
            Color = lineColor;
            FillColor = fillColor;
            Filled = filled;
            PenWidth = lineWidth;
            EndCap = endCap;
            PenType = penType;
            TipText = String.Format("Rectangle Center @ {0}, {1}", Center.X, Center.Y);
        }

        /// <summary>
        /// Draw rectangle using SkiaSharp
        /// </summary>
        /// <param name="canvas">SKCanvas to draw on</param>
        public void DrawSkia(SKCanvas canvas)
        {
            using (var paint = new SKPaint())
            {
                paint.IsAntialias = true;
                paint.Color = ColorToSKColor(Color);
                paint.StrokeWidth = PenWidth;
                paint.StrokeCap = ConvertLineCap(EndCap);

                // Apply pen type styling
                ApplyPenType(paint, PenType);

                // Create the path for the rectangle
                using (var path = new SKPath())
                {
                    var normalizedRect = GetNormalizedRectangle(Rectangle);
                    path.AddRect(normalizedRect);

                    // Apply rotation if necessary
                    if (Rotation != 0)
                    {
                        var bounds = path.Bounds;
                        var centerX = bounds.Left + (bounds.Width / 2);
                        var centerY = bounds.Top + (bounds.Height / 2);

                        canvas.Save();
                        canvas.RotateDegrees(Rotation, centerX, centerY);
                        
                        // Fill first if needed
                        if (Filled)
                        {
                            paint.Style = SKPaintStyle.Fill;
                            paint.Color = ColorToSKColor(FillColor);
                            canvas.DrawPath(path, paint);
                        }

                        // Then draw the outline
                        paint.Style = SKPaintStyle.Stroke;
                        paint.Color = ColorToSKColor(Color);
                        canvas.DrawPath(path, paint);
                        
                        canvas.Restore();
                    }
                    else
                    {
                        // Fill first if needed
                        if (Filled)
                        {
                            paint.Style = SKPaintStyle.Fill;
                            paint.Color = ColorToSKColor(FillColor);
                            canvas.DrawPath(path, paint);
                        }

                        // Then draw the outline
                        paint.Style = SKPaintStyle.Stroke;
                        paint.Color = ColorToSKColor(Color);
                        canvas.DrawPath(path, paint);
                    }
                }
            }
        }

        /// <summary>
        /// Draw rectangle - legacy compatibility method
        /// </summary>
        /// <param name="g">System.Drawing.Graphics object</param>
        public override void Draw(System.Drawing.Graphics g)
        {
            // For compatibility with existing code that uses System.Drawing.Graphics
            // In a complete migration, this would be replaced with DrawSkia calls
            // This is a placeholder that would need a graphics conversion layer
            
            // Convert Graphics to SKCanvas would require additional infrastructure
            // For now, we maintain the method signature for compatibility
            throw new NotImplementedException("Use DrawSkia method with SKCanvas instead of System.Drawing.Graphics");
        }

        protected void SetRectangle(int x, int y, int width, int height)
        {
            rectangle = SKRect.Create(x, y, width, height);
        }

        /// <summary>
        /// Get number of handles
        /// </summary>
        public override int HandleCount
        {
            get { return 8; }
        }

        /// <summary>
        /// Get number of connection points
        /// </summary>
        public override int ConnectionCount
        {
            get { return HandleCount; }
        }

        public override System.Drawing.Point GetConnection(int connectionNumber)
        {
            return GetHandle(connectionNumber);
        }

        /// <summary>
        /// Get handle point by 1-based number
        /// </summary>
        /// <param name="handleNumber"></param>
        /// <returns></returns>
        public override System.Drawing.Point GetHandle(int handleNumber)
        {
            int x, y, xCenter, yCenter;

            xCenter = (int)(rectangle.Left + rectangle.Width / 2);
            yCenter = (int)(rectangle.Top + rectangle.Height / 2);
            x = (int)rectangle.Left;
            y = (int)rectangle.Top;

            switch (handleNumber)
            {
                case 1:
                    x = (int)rectangle.Left;
                    y = (int)rectangle.Top;
                    break;
                case 2:
                    x = xCenter;
                    y = (int)rectangle.Top;
                    break;
                case 3:
                    x = (int)rectangle.Right;
                    y = (int)rectangle.Top;
                    break;
                case 4:
                    x = (int)rectangle.Right;
                    y = yCenter;
                    break;
                case 5:
                    x = (int)rectangle.Right;
                    y = (int)rectangle.Bottom;
                    break;
                case 6:
                    x = xCenter;
                    y = (int)rectangle.Bottom;
                    break;
                case 7:
                    x = (int)rectangle.Left;
                    y = (int)rectangle.Bottom;
                    break;
                case 8:
                    x = (int)rectangle.Left;
                    y = yCenter;
                    break;
            }
            return new System.Drawing.Point(x, y);
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
                    if (GetHandleRectangle(i).Contains(point))
                        return i;
                }
            }

            if (PointInObject(point))
                return 0;
            return -1;
        }

        protected override bool PointInObject(System.Drawing.Point point)
        {
            var skPoint = new SKPoint(point.X, point.Y);
            return rectangle.Contains(skPoint);
        }

        public override System.Drawing.Rectangle GetBounds(System.Drawing.Graphics g)
        {
            return new System.Drawing.Rectangle(
                (int)Math.Floor(rectangle.Left),
                (int)Math.Floor(rectangle.Top),
                (int)Math.Ceiling(rectangle.Width),
                (int)Math.Ceiling(rectangle.Height)
            );
        }

        /// <summary>
        /// Get cursor for the handle
        /// </summary>
        /// <param name="handleNumber"></param>
        /// <returns></returns>
        public override Cursor GetHandleCursor(int handleNumber)
        {
            switch (handleNumber)
            {
                case 1:
                    return Cursors.SizeNWSE;
                case 2:
                    return Cursors.SizeNS;
                case 3:
                    return Cursors.SizeNESW;
                case 4:
                    return Cursors.SizeWE;
                case 5:
                    return Cursors.SizeNWSE;
                case 6:
                    return Cursors.SizeNS;
                case 7:
                    return Cursors.SizeNESW;
                case 8:
                    return Cursors.SizeWE;
                default:
                    return Cursors.Default;
            }
        }

        /// <summary>
        /// Move handle to new point (resizing)
        /// </summary>
        /// <param name="point"></param>
        /// <param name="handleNumber"></param>
        public override void MoveHandleTo(System.Drawing.Point point, int handleNumber)
        {
            float left = Rectangle.Left;
            float top = Rectangle.Top;
            float right = Rectangle.Right;
            float bottom = Rectangle.Bottom;

            switch (handleNumber)
            {
                case 1:
                    left = point.X;
                    top = point.Y;
                    break;
                case 2:
                    top = point.Y;
                    break;
                case 3:
                    right = point.X;
                    top = point.Y;
                    break;
                case 4:
                    right = point.X;
                    break;
                case 5:
                    right = point.X;
                    bottom = point.Y;
                    break;
                case 6:
                    bottom = point.Y;
                    break;
                case 7:
                    left = point.X;
                    bottom = point.Y;
                    break;
                case 8:
                    left = point.X;
                    break;
            }
            Dirty = true;
            SetRectangle((int)left, (int)top, (int)(right - left), (int)(bottom - top));
        }

        public override bool IntersectsWith(System.Drawing.Rectangle rectangle)
        {
            var skRect = new SKRect(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
            return SKRect.Intersect(this.Rectangle, skRect);
        }

        /// <summary>
        /// Move object
        /// </summary>
        /// <param name="deltaX"></param>
        /// <param name="deltaY"></param>
        public override void Move(int deltaX, int deltaY)
        {
            rectangle = rectangle.Offset(deltaX, deltaY);
            Dirty = true;
        }

        public override void Dump()
        {
            base.Dump();

            Trace.WriteLine("rectangle.Left = " + rectangle.Left.ToString(CultureInfo.InvariantCulture));
            Trace.WriteLine("rectangle.Top = " + rectangle.Top.ToString(CultureInfo.InvariantCulture));
            Trace.WriteLine("rectangle.Width = " + rectangle.Width.ToString(CultureInfo.InvariantCulture));
            Trace.WriteLine("rectangle.Height = " + rectangle.Height.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Normalize rectangle
        /// </summary>
        public override void Normalize()
        {
            rectangle = GetNormalizedRectangle(rectangle);
        }

        /// <summary>
        /// Save object to serialization stream
        /// </summary>
        /// <param name="info">Contains all data being written to disk</param>
        /// <param name="orderNumber">Index of the Layer being saved</param>
        /// <param name="objectIndex">Index of the drawing object in the Layer</param>
        public override void SaveToStream(SerializationInfo info, int orderNumber, int objectIndex)
        {
            // Convert SKRect to System.Drawing.Rectangle for serialization compatibility
            var systemRect = new System.Drawing.Rectangle(
                (int)rectangle.Left,
                (int)rectangle.Top,
                (int)rectangle.Width,
                (int)rectangle.Height
            );

            info.AddValue(
                String.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryRectangle, orderNumber, objectIndex),
                systemRect);

            base.SaveToStream(info, orderNumber, objectIndex);
        }

        /// <summary>
        /// Load object from serialization stream
        /// </summary>
        /// <param name="info"></param>
        /// <param name="orderNumber"></param>
        /// <param name="objectIndex"></param>
        public override void LoadFromStream(SerializationInfo info, int orderNumber, int objectIndex)
        {
            var systemRect = (System.Drawing.Rectangle)info.GetValue(
                                    String.Format(CultureInfo.InvariantCulture,
                                                  "{0}{1}-{2}",
                                                  entryRectangle, orderNumber, objectIndex),
                                    typeof(System.Drawing.Rectangle));

            // Convert System.Drawing.Rectangle to SKRect
            rectangle = new SKRect(systemRect.X, systemRect.Y, systemRect.Right, systemRect.Bottom);

            base.LoadFromStream(info, orderNumber, objectIndex);
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

        // Legacy overloads for backward compatibility
        public static System.Drawing.Rectangle GetNormalizedRectangle(int x1, int y1, int x2, int y2)
        {
            if (x2 < x1)
            {
                int tmp = x2;
                x2 = x1;
                x1 = tmp;
            }

            if (y2 < y1)
            {
                int tmp = y2;
                y2 = y1;
                y1 = tmp;
            }
            return new System.Drawing.Rectangle(x1, y1, x2 - x1, y2 - y1);
        }

        public static System.Drawing.Rectangle GetNormalizedRectangle(System.Drawing.Point p1, System.Drawing.Point p2)
        {
            return GetNormalizedRectangle(p1.X, p1.Y, p2.X, p2.Y);
        }

        public static System.Drawing.Rectangle GetNormalizedRectangle(System.Drawing.Rectangle r)
        {
            return GetNormalizedRectangle(r.X, r.Y, r.X + r.Width, r.Y + r.Height);
        }
        #endregion Helper Functions

        #region SkiaSharp Helper Methods

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
            // Apply custom pen properties based on pen type
            // This implementation should be customized based on your DrawingPens.PenType enum values
            
            switch (penType)
            {
                case DrawingPens.PenType.Generic:
                    // Solid line - no dash effect needed
                    break;
                // Add more cases as needed based on your actual PenType enum values
                // Example patterns:
                // case DrawingPens.PenType.Dashed:
                //     paint.PathEffect = SKPathEffect.CreateDash(new float[] { 10, 5 }, 0);
                //     break;
                // case DrawingPens.PenType.Dotted:
                //     paint.PathEffect = SKPathEffect.CreateDash(new float[] { 2, 2 }, 0);
                //     break;
                // case DrawingPens.PenType.DashDot:
                //     paint.PathEffect = SKPathEffect.CreateDash(new float[] { 10, 5, 2, 5 }, 0);
                //     break;
                default:
                    // Default to solid line
                    break;
            }

            // Apply DrawPen properties if available
            if (DrawPen != null)
            {
                // Custom pen properties can be applied here
                // This would require additional conversion logic from System.Drawing.Pen to SKPaint
            }
        }

        #endregion
    }
}