using SkiaSharp;

namespace CraftSynth.ImageEditor
{
    public static class FillBrushes
    {
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

        /// <summary>
        /// Returns an SKPaint configured for filling with the specified brush type.
        /// Callers are responsible for disposing the returned SKPaint.
        /// </summary>
        public static SKPaint SetCurrentBrush(BrushType bType)
        {
            switch (bType)
            {
                case BrushType.Brown:
                    return new SKPaint { Color = new SKColor(165, 42, 42, 255), Style = SKPaintStyle.Fill, IsAntialias = true };
                case BrushType.Aqua:
                    return new SKPaint { Color = SKColors.Aqua, Style = SKPaintStyle.Fill, IsAntialias = true };
                case BrushType.GrayDivot:
                    // Emulate hatch using a bitmap shader pattern
                    return CreateHatchPaint(SKColors.Gray, SKColors.Gainsboro, 6, HatchPattern.Divot);
                case BrushType.RedDiag:
                    return CreateHatchPaint(SKColors.Red, SKColors.Yellow, 6, HatchPattern.ForwardDiagonal);
                case BrushType.ConfettiGreen:
                    return CreateHatchPaint(SKColors.Green, SKColors.White, 6, HatchPattern.LargeConfetti);
                case BrushType.NoBrush:
                default:
                    return new SKPaint { Color = SKColors.Transparent, Style = SKPaintStyle.Fill };
            }
        }

        private enum HatchPattern { Divot, ForwardDiagonal, LargeConfetti }

        private static SKPaint CreateHatchPaint(SKColor fg, SKColor bg, int size, HatchPattern pattern)
        {
            using var surface = SKSurface.Create(new SKImageInfo(size, size));
            var canvas = surface.Canvas;
            canvas.Clear(bg);
            using var pen = new SKPaint { Color = fg, Style = SKPaintStyle.Stroke, StrokeWidth = 1, IsAntialias = true };
            using var fill = new SKPaint { Color = fg, Style = SKPaintStyle.Fill, IsAntialias = true };

            switch (pattern)
            {
                case HatchPattern.ForwardDiagonal:
                    canvas.DrawLine(0, size, size, 0, pen);
                    break;
                case HatchPattern.Divot:
                    canvas.DrawCircle(size / 2f, size / 2f, size / 4f, pen);
                    break;
                case HatchPattern.LargeConfetti:
                    canvas.DrawCircle(size * 0.25f, size * 0.25f, size * 0.15f, fill);
                    canvas.DrawCircle(size * 0.7f, size * 0.6f, size * 0.2f, fill);
                    break;
            }

            using var img = surface.Snapshot();
            using var shader = img.ToShader(SKShaderTileMode.Repeat, SKShaderTileMode.Repeat);
            return new SKPaint { Shader = shader, Style = SKPaintStyle.Fill, IsAntialias = true };
        }
    }
}
