using System;
using SkiaSharp;

namespace CraftSynth.ImageEditor
{
    /// <summary>
    /// Ellipse graphic object (SkiaSharp)
    /// </summary>
    [Serializable]
    public class DrawEllipse : DrawRectangle
    {
        public DrawEllipse()
        {
            SetRectangle(0, 0, 1, 1);
        }

        public DrawEllipse(float x, float y, float width, float height, SKColor lineColor, SKColor fillColor, bool filled, float lineWidth, DrawingPens.PenType penType, SKStrokeCap endCap)
        {
            Rectangle = new SKRect(x, y, x + width, y + height);
            Center = new SKPoint(x + (width / 2f), y + (height / 2f));
            TipText = $"Ellipse Center @ {Center.X}, {Center.Y}";
            Color = lineColor;
            FillColor = fillColor;
            Filled = filled;
            PenWidth = lineWidth;
            PenType = penType;
            EndCap = endCap;
        }

        public override DrawObject Clone()
        {
            var d = new DrawEllipse();
            d.Rectangle = Rectangle;
            FillDrawObjectFields(d);
            return d;
        }

        public override void Draw(SKCanvas canvas)
        {
            using var stroke = new SKPaint { Color = Color, StrokeWidth = PenWidth, Style = SKPaintStyle.Stroke, IsAntialias = true, StrokeCap = EndCap };
            DrawingPens.ConfigurePaint(stroke, PenType);
            using var fill = new SKPaint { Color = FillColor, Style = SKPaintStyle.Fill, IsAntialias = true };
            var r = NormalizeRect(Rectangle);
            if (Rotation != 0)
            {
                var cx = r.MidX; var cy = r.MidY;
                canvas.Save();
                canvas.RotateDegrees(Rotation, cx, cy);
                if (Filled) canvas.DrawOval(r, fill);
                canvas.DrawOval(r, stroke);
                canvas.Restore();
            }
            else
            {
                if (Filled) canvas.DrawOval(r, fill);
                canvas.DrawOval(r, stroke);
            }
        }
    }
}
