using System;
using SkiaSharp;

namespace CraftSynth.ImageEditor
{
    public class DrawingPens
    {
        #region Enumerations
        public enum PenType
        {
            Solid,
            Dash,
            Dash_Dot,
            Dot,
            DoubleLine
        } 
        #endregion Enumerations

        public static string GetPenTypeAsString(PenType penType)
        {
            switch (penType)
            {
                case PenType.Solid:
                    return "___";
                case PenType.Dash:
                    return "- - -";
                case PenType.Dash_Dot:
                    return "- . -";
                case PenType.Dot:
                    return ". . .";
                case PenType.DoubleLine:
                    return "===";
                default:
                    throw new ArgumentOutOfRangeException(nameof(penType));
            }
        }

        /// <summary>
        /// Configure an SKPaint object based on the pen type requested
        /// </summary>
        /// <param name="paint">SKPaint object to configure</param>
        /// <param name="penType">Type of pen from the PenType enumeration</param>
        /// <param name="endCap">End cap style for the pen</param>
        public static void SetCurrentPen(SKPaint paint, PenType penType, SKStrokeCap endCap)
        {
            if (paint == null)
                throw new ArgumentNullException(nameof(paint));

            // Set basic paint properties
            paint.Style = SKPaintStyle.Stroke;
            paint.IsAntialias = true;
            paint.StrokeJoin = SKStrokeJoin.Round;
            paint.StrokeCap = endCap;

            // Clear any existing path effect
            paint.PathEffect?.Dispose();
            paint.PathEffect = null;

            switch (penType)
            {
                case PenType.Solid:
                    // No additional configuration needed for solid lines
                    break;
                case PenType.Dash:
                    paint.PathEffect = SKPathEffect.CreateDash(new float[] { 10, 5 }, 0);
                    break;
                case PenType.Dash_Dot:
                    paint.PathEffect = SKPathEffect.CreateDash(new float[] { 10, 5, 2, 5 }, 0);
                    break;
                case PenType.Dot:
                    paint.PathEffect = SKPathEffect.CreateDash(new float[] { 2, 3 }, 0);
                    break;
                case PenType.DoubleLine:
                    // For double line effect, we'll use a custom approach
                    // This creates a compound stroke effect similar to System.Drawing.Pen.CompoundArray
                    // Note: SkiaSharp doesn't have direct compound array support, so we simulate it
                    // by drawing multiple strokes with different widths
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(penType));
            }
        }

        /// <summary>
        /// Create a configured SKPaint object based on pen parameters
        /// </summary>
        /// <param name="color">Color of the pen</param>
        /// <param name="width">Width of the pen stroke</param>
        /// <param name="penType">Type of pen pattern</param>
        /// <param name="endCap">End cap style</param>
        /// <returns>Configured SKPaint object</returns>
        public static SKPaint CreatePen(SKColor color, float width, PenType penType, SKStrokeCap endCap)
        {
            var paint = new SKPaint
            {
                Color = color,
                StrokeWidth = width
            };

            SetCurrentPen(paint, penType, endCap);
            return paint;
        }

        /// <summary>
        /// Create a configured SKPaint object from System.Drawing.Color
        /// </summary>
        /// <param name="color">System.Drawing.Color to convert</param>
        /// <param name="width">Width of the pen stroke</param>
        /// <param name="penType">Type of pen pattern</param>
        /// <param name="endCap">End cap style</param>
        /// <returns>Configured SKPaint object</returns>
        public static SKPaint CreatePen(System.Drawing.Color color, float width, PenType penType, SKStrokeCap endCap)
        {
            var skColor = new SKColor(color.R, color.G, color.B, color.A);
            return CreatePen(skColor, width, penType, endCap);
        }

        /// <summary>
        /// Convert System.Drawing.Drawing2D.LineCap to SKStrokeCap
        /// </summary>
        /// <param name="lineCap">System.Drawing LineCap to convert</param>
        /// <returns>Equivalent SKStrokeCap</returns>
        public static SKStrokeCap ConvertLineCap(System.Drawing.Drawing2D.LineCap lineCap)
        {
            return lineCap switch
            {
                System.Drawing.Drawing2D.LineCap.Round => SKStrokeCap.Round,
                System.Drawing.Drawing2D.LineCap.Square => SKStrokeCap.Square,
                System.Drawing.Drawing2D.LineCap.Flat => SKStrokeCap.Butt,
                _ => SKStrokeCap.Butt
            };
        }

        /// <summary>
        /// Draw a double line effect using SkiaSharp
        /// This method provides the double line functionality that was achieved 
        /// with CompoundArray in System.Drawing
        /// </summary>
        /// <param name="canvas">SKCanvas to draw on</param>
        /// <param name="path">SKPath to stroke</param>
        /// <param name="paint">Base paint configuration</param>
        public static void DrawDoubleLine(SKCanvas canvas, SKPath path, SKPaint paint)
        {
            if (canvas == null || path == null || paint == null)
                return;

            var originalWidth = paint.StrokeWidth;
            var originalColor = paint.Color;

            using (var outerPaint = new SKPaint(paint))
            using (var innerPaint = new SKPaint(paint))
            {
                // Draw outer stroke (thicker)
                outerPaint.StrokeWidth = originalWidth;
                outerPaint.Color = originalColor;
                canvas.DrawPath(path, outerPaint);

                // Draw inner stroke (thinner, creating the double line effect)
                innerPaint.StrokeWidth = originalWidth * 0.3f; // 30% of original width
                innerPaint.Color = SKColors.White; // or background color
                canvas.DrawPath(path, innerPaint);

                // Draw the actual line strokes
                var lineWidth = originalWidth * 0.15f; // 15% for each line
                paint.StrokeWidth = lineWidth;
                
                // Create offset paths for the two lines
                using (var leftPath = paint.GetFillPath(path))
                using (var rightPath = paint.GetFillPath(path))
                {
                    if (leftPath != null && rightPath != null)
                    {
                        // Transform paths to create parallel lines
                        var offset = originalWidth * 0.25f;
                        var leftMatrix = SKMatrix.CreateTranslation(-offset, 0);
                        var rightMatrix = SKMatrix.CreateTranslation(offset, 0);
                        
                        leftPath.Transform(leftMatrix);
                        rightPath.Transform(rightMatrix);
                        
                        paint.Color = originalColor;
                        canvas.DrawPath(leftPath, paint);
                        canvas.DrawPath(rightPath, paint);
                    }
                }
            }

            // Restore original paint properties
            paint.StrokeWidth = originalWidth;
            paint.Color = originalColor;
        }

        /// <summary>
        /// Helper method to convert System.Drawing.Color to SKColor
        /// </summary>
        /// <param name="color">System.Drawing.Color to convert</param>
        /// <returns>Equivalent SKColor</returns>
        public static SKColor ToSKColor(System.Drawing.Color color)
        {
            return new SKColor(color.R, color.G, color.B, color.A);
        }

        /// <summary>
        /// Helper method to convert SKColor to System.Drawing.Color
        /// </summary>
        /// <param name="color">SKColor to convert</param>
        /// <returns>Equivalent System.Drawing.Color</returns>
        public static System.Drawing.Color ToDrawingColor(SKColor color)
        {
            return System.Drawing.Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
        }
    }
}