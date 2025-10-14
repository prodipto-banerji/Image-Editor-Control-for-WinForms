using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Windows.Forms;
using SkiaSharp;

namespace CraftSynth.ImageEditor
{
    /// <summary>
    /// Ellipse graphic object
    /// </summary>
    [Serializable]
    public class DrawEllipse : DrawRectangle
    {
        private bool _disposed = false;

        public DrawEllipse()
        {
            SetRectangle(0, 0, 1, 1);
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
                    // Base class handles most disposables
                }

                // Free any unmanaged objects here.

                this._disposed = true;
            }
            base.Dispose(disposing);
        }

        ~DrawEllipse()
        {
            this.Dispose(false);
        }
        #endregion

        public DrawEllipse(int x, int y, int width, int height, System.Drawing.Color lineColor, System.Drawing.Color fillColor, bool filled, int lineWidth, DrawingPens.PenType penType, System.Drawing.Drawing2D.LineCap endCap)
        {
            Rectangle = new System.Drawing.Rectangle(x, y, width, height);
            Center = new System.Drawing.Point(x + (width / 2), y + (height / 2));
            TipText = String.Format("Ellipse Center @ {0}, {1}", Center.X, Center.Y);
            Color = lineColor;
            FillColor = fillColor;
            Filled = filled;
            PenWidth = lineWidth;
            PenType = penType;
            EndCap = endCap;
            ZOrder = 0;
            Initialize();
        }

        /// <summary>
        /// Clone this instance
        /// </summary>
        public override DrawObject Clone()
        {
            DrawEllipse drawEllipse = new DrawEllipse();
            drawEllipse.Rectangle = Rectangle;

            FillDrawObjectFields(drawEllipse);
            return drawEllipse;
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
        /// Draw ellipse using SkiaSharp canvas
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

                // Apply pen type styling if DrawPen is set
                if (DrawPen != null)
                {
                    // Apply custom pen properties
                    ApplyPenType(paint, PenType);
                }
                else
                {
                    // Apply pen type directly
                    ApplyPenType(paint, PenType);
                }

                // Apply end cap styling
                paint.StrokeCap = ConvertLineCap(EndCap);

                // Create the ellipse path
                using (var path = new SKPath())
                {
                    var normalizedRect = GetNormalizedRectangleSK(Rectangle);
                    var skRect = new SKRect(normalizedRect.Left, normalizedRect.Top, normalizedRect.Right, normalizedRect.Bottom);
                    path.AddOval(skRect);

                    // Apply rotation if necessary
                    if (Rotation != 0)
                    {
                        var bounds = path.Bounds;
                        var centerX = bounds.Left + (bounds.Width / 2);
                        var centerY = bounds.Top + (bounds.Height / 2);

                        canvas.Save();
                        canvas.RotateDegrees(Rotation, centerX, centerY);

                        // Draw filled ellipse first if needed
                        if (Filled)
                        {
                            using (var fillPaint = new SKPaint())
                            {
                                fillPaint.IsAntialias = true;
                                fillPaint.Color = ColorToSKColor(FillColor);
                                fillPaint.Style = SKPaintStyle.Fill;
                                canvas.DrawPath(path, fillPaint);
                            }
                        }

                        // Draw the outline
                        canvas.DrawPath(path, paint);
                        canvas.Restore();
                    }
                    else
                    {
                        // Draw filled ellipse first if needed
                        if (Filled)
                        {
                            using (var fillPaint = new SKPaint())
                            {
                                fillPaint.IsAntialias = true;
                                fillPaint.Color = ColorToSKColor(FillColor);
                                fillPaint.Style = SKPaintStyle.Fill;
                                canvas.DrawPath(path, fillPaint);
                            }
                        }

                        // Draw the outline
                        canvas.DrawPath(path, paint);
                    }
                }
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
            switch (penType)
            {
                case DrawingPens.PenType.Solid:
                    // Default solid line - no dash effect needed
                    break;
                case DrawingPens.PenType.Dash:
                    paint.PathEffect = SKPathEffect.CreateDash(new float[] { 10, 5 }, 0);
                    break;
                case DrawingPens.PenType.Dash_Dot:
                    paint.PathEffect = SKPathEffect.CreateDash(new float[] { 10, 5, 2, 5 }, 0);
                    break;
                case DrawingPens.PenType.Dot:
                    paint.PathEffect = SKPathEffect.CreateDash(new float[] { 2, 2 }, 0);
                    break;
                case DrawingPens.PenType.DoubleLine:
                    // SkiaSharp doesn't have direct equivalent of CompoundArray
                    // We can simulate by drawing two concentric ellipses
                    // This would need to be handled in the drawing code
                    // For now, we'll just use a solid line
                    break;
                default:
                    break;
            }

            // Set line join and stroke caps
            paint.StrokeJoin = SKStrokeJoin.Round;
        }

        /// <summary>
        /// Get normalized rectangle as SKRect
        /// </summary>
        private SKRect GetNormalizedRectangleSK(System.Drawing.Rectangle rect)
        {
            float left = Math.Min(rect.Left, rect.Right);
            float top = Math.Min(rect.Top, rect.Bottom);
            float right = Math.Max(rect.Left, rect.Right);
            float bottom = Math.Max(rect.Top, rect.Bottom);

            // Ensure we have positive width and height
            if (right == left) right = left + 1;
            if (bottom == top) bottom = top + 1;

            return new SKRect(left, top, right, bottom);
        }

        #endregion
    }
}