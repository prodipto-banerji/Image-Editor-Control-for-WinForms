using SkiaSharp;

namespace CraftSynth.ImageEditor
{
    public class FillBrushes
    {
        #region Enumerations
        public enum BrushType
        {
            Brown,
            Aqua,
            GrayDivot,
            RedDiag,
            ConfettiGreen,
            NoBrush,
            NumberOfBrushes
        }
        #endregion Enumerations

        public static SKPaint SetCurrentBrush(BrushType _bType)
        {
            SKPaint paint = null;
            switch (_bType)
            {
                case BrushType.Aqua:
                    paint = AquaBrush();
                    break;
                case BrushType.Brown:
                    paint = BrownBrush();
                    break;
                case BrushType.ConfettiGreen:
                    paint = ConfettiBrush();
                    break;
                case BrushType.GrayDivot:
                    paint = GrayDivotBrush();
                    break;
                case BrushType.RedDiag:
                    paint = RedDiagBrush();
                    break;
                default:
                    break;
            }
            return paint;
        }

        private static SKPaint BrownBrush()
        {
            return new SKPaint
            {
                Color = SKColors.Brown,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };
        }

        private static SKPaint AquaBrush()
        {
            return new SKPaint
            {
                Color = SKColors.Aqua,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };
        }

        private static SKPaint GrayDivotBrush()
        {
            var paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            // Create a pattern shader for divot effect
            // Using a simple dot pattern as SkiaSharp doesn't have direct HatchBrush equivalent
            using (var surface = SKSurface.Create(new SKImageInfo(8, 8)))
            {
                var canvas = surface.Canvas;
                canvas.Clear(SKColors.Gainsboro);
                
                using (var dotPaint = new SKPaint { Color = SKColors.Gray })
                {
                    canvas.DrawCircle(2, 2, 1, dotPaint);
                    canvas.DrawCircle(6, 6, 1, dotPaint);
                }
                
                var image = surface.Snapshot();
                paint.Shader = SKShader.CreateBitmap(image, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat);
            }

            return paint;
        }

        private static SKPaint RedDiagBrush()
        {
            var paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            // Create a diagonal pattern shader
            using (var surface = SKSurface.Create(new SKImageInfo(8, 8)))
            {
                var canvas = surface.Canvas;
                canvas.Clear(SKColors.Yellow);
                
                using (var linePaint = new SKPaint 
                { 
                    Color = SKColors.Red, 
                    StrokeWidth = 1,
                    Style = SKPaintStyle.Stroke
                })
                {
                    // Draw diagonal lines
                    canvas.DrawLine(0, 0, 8, 8, linePaint);
                    canvas.DrawLine(-4, 0, 4, 8, linePaint);
                    canvas.DrawLine(4, 0, 12, 8, linePaint);
                }
                
                var image = surface.Snapshot();
                paint.Shader = SKShader.CreateBitmap(image, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat);
            }

            return paint;
        }

        private static SKPaint ConfettiBrush()
        {
            var paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            // Create a confetti pattern shader
            using (var surface = SKSurface.Create(new SKImageInfo(16, 16)))
            {
                var canvas = surface.Canvas;
                canvas.Clear(SKColors.White);
                
                using (var confettiPaint = new SKPaint { Color = SKColors.Green })
                {
                    // Draw confetti-like rectangles
                    canvas.DrawRect(2, 3, 3, 2, confettiPaint);
                    canvas.DrawRect(8, 6, 2, 3, confettiPaint);
                    canvas.DrawRect(12, 2, 2, 2, confettiPaint);
                    canvas.DrawRect(5, 10, 3, 2, confettiPaint);
                    canvas.DrawRect(11, 12, 2, 2, confettiPaint);
                    canvas.DrawRect(1, 8, 2, 3, confettiPaint);
                }
                
                var image = surface.Snapshot();
                paint.Shader = SKShader.CreateBitmap(image, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat);
            }

            return paint;
        }

        /// <summary>
        /// Helper method to convert System.Drawing.Color to SKColor for compatibility
        /// </summary>
        /// <param name="color">System.Drawing.Color to convert</param>
        /// <returns>Equivalent SKColor</returns>
        public static SKColor ColorToSKColor(System.Drawing.Color color)
        {
            return new SKColor(color.R, color.G, color.B, color.A);
        }

        /// <summary>
        /// Helper method to create a solid color SKPaint from System.Drawing.Color
        /// </summary>
        /// <param name="color">System.Drawing.Color to use</param>
        /// <returns>SKPaint configured with the specified color</returns>
        public static SKPaint CreateSolidBrush(System.Drawing.Color color)
        {
            return new SKPaint
            {
                Color = ColorToSKColor(color),
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };
        }
    }
}