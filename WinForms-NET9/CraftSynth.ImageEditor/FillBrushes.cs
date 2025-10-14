using System.Drawing;
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

        /// <summary>
        /// Create SkiaSharp paint for fill based on brush type
        /// </summary>
        /// <param name="brushType">Type of brush</param>
        /// <returns>SKPaint configured for the brush type</returns>
        public static SKPaint? SetCurrentBrush(BrushType brushType)
        {
            switch (brushType)
            {
                case BrushType.Aqua:
                    return AquaBrush();
                case BrushType.Brown:
                    return BrownBrush();
                case BrushType.ConfettiGreen:
                    return ConfettiBrush();
                case BrushType.GrayDivot:
                    return GrayDivotBrush();
                case BrushType.RedDiag:
                    return RedDiagBrush();
                case BrushType.NoBrush:
                default:
                    return null;
            }
        }

        /// <summary>
        /// Create GDI+ brush for compatibility
        /// </summary>
        /// <param name="brushType">Type of brush</param>
        /// <returns>System.Drawing.Brush</returns>
        public static Brush? SetCurrentGdiBrush(BrushType brushType)
        {
            switch (brushType)
            {
                case BrushType.Aqua:
                    return new SolidBrush(Color.Aqua);
                case BrushType.Brown:
                    return new SolidBrush(Color.Brown);
                case BrushType.ConfettiGreen:
                    return new System.Drawing.Drawing2D.HatchBrush(
                        System.Drawing.Drawing2D.HatchStyle.LargeConfetti, 
                        Color.Green, 
                        Color.White);
                case BrushType.GrayDivot:
                    return new System.Drawing.Drawing2D.HatchBrush(
                        System.Drawing.Drawing2D.HatchStyle.Divot, 
                        Color.Gray, 
                        Color.Gainsboro);
                case BrushType.RedDiag:
                    return new System.Drawing.Drawing2D.HatchBrush(
                        System.Drawing.Drawing2D.HatchStyle.ForwardDiagonal, 
                        Color.Red, 
                        Color.Yellow);
                case BrushType.NoBrush:
                default:
                    return null;
            }
        }

        private static SKPaint BrownBrush()
        {
            return new SKPaint
            {
                Color = SKColors.Brown,
                Style = SKPaintStyle.Fill
            };
        }

        private static SKPaint AquaBrush()
        {
            return new SKPaint
            {
                Color = SKColors.Aqua,
                Style = SKPaintStyle.Fill
            };
        }

        private static SKPaint GrayDivotBrush()
        {
            // SkiaSharp doesn't have built-in hatch patterns like GDI+
            // We'll create a simple pattern or use solid color as fallback
            return new SKPaint
            {
                Color = SKColors.Gray,
                Style = SKPaintStyle.Fill
            };
        }

        private static SKPaint RedDiagBrush()
        {
            // Create a diagonal pattern using a shader
            var paint = new SKPaint
            {
                Style = SKPaintStyle.Fill
            };

            // Create a simple diagonal pattern
            using var surface = SKSurface.Create(new SKImageInfo(20, 20));
            using var canvas = surface.Canvas;
            canvas.Clear(SKColors.Yellow);
            
            using var linePaint = new SKPaint
            {
                Color = SKColors.Red,
                StrokeWidth = 2,
                Style = SKPaintStyle.Stroke
            };
            
            // Draw diagonal lines
            for (int i = -20; i < 40; i += 5)
            {
                canvas.DrawLine(i, 0, i + 20, 20, linePaint);
            }

            var image = surface.Snapshot();
            paint.Shader = SKShader.CreateBitmap(image, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat);
            
            return paint;
        }

        private static SKPaint ConfettiBrush()
        {
            // Create a confetti-like pattern
            var paint = new SKPaint
            {
                Style = SKPaintStyle.Fill
            };

            using var surface = SKSurface.Create(new SKImageInfo(30, 30));
            using var canvas = surface.Canvas;
            canvas.Clear(SKColors.White);
            
            using var confettiPaint = new SKPaint
            {
                Color = SKColors.Green,
                Style = SKPaintStyle.Fill
            };
            
            // Draw random small rectangles to simulate confetti
            var random = new System.Random(42); // Fixed seed for consistency
            for (int i = 0; i < 15; i++)
            {
                float x = random.Next(0, 25);
                float y = random.Next(0, 25);
                canvas.DrawRect(x, y, 3, 3, confettiPaint);
            }

            var image = surface.Snapshot();
            paint.Shader = SKShader.CreateBitmap(image, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat);
            
            return paint;
        }
    }
}