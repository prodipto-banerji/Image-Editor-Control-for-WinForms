using System;
using System.Drawing;
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
        /// Configure SkiaSharp paint based on the pen type requested
        /// </summary>
        /// <param name="paint">SKPaint to configure</param>
        /// <param name="penType">Type of pen from the PenType enumeration</param>
        /// <param name="endCap">End cap style</param>
        public static void SetCurrentPaint(SKPaint paint, PenType penType, SKStrokeCap endCap)
        {
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeJoin = SKStrokeJoin.Round;
            paint.StrokeCap = endCap;

            switch (penType)
            {
                case PenType.Solid:
                    paint.PathEffect = null;
                    break;
                case PenType.Dash:
                    paint.PathEffect = SKPathEffect.CreateDash(new float[] { 10, 5 }, 0);
                    break;
                case PenType.Dash_Dot:
                    paint.PathEffect = SKPathEffect.CreateDash(new float[] { 10, 5, 2, 5 }, 0);
                    break;
                case PenType.Dot:
                    paint.PathEffect = SKPathEffect.CreateDash(new float[] { 2, 5 }, 0);
                    break;
                case PenType.DoubleLine:
                    // SkiaSharp doesn't have direct compound array support like GDI+
                    // We'll simulate it by drawing two lines with different stroke widths
                    paint.PathEffect = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(penType));
            }
        }

        /// <summary>
        /// Configure GDI+ pen based on the pen type requested (for compatibility)
        /// </summary>
        /// <param name="pen">Pen to configure</param>
        /// <param name="penType">Type of pen from the PenType enumeration</param>
        /// <param name="endCap">End cap style</param>
        public static void SetCurrentPen(ref Pen pen, PenType penType, System.Drawing.Drawing2D.LineCap endCap)
        {
            switch (penType)
            {
                case PenType.Solid:
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                    break;
                case PenType.Dash:
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    break;
                case PenType.Dash_Dot:
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;
                    break;
                case PenType.Dot:
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                    break;
                case PenType.DoubleLine:
                    pen.CompoundArray = new float[] { 0.0f, 0.1f, 0.2f, 0.3f, 0.7f, 0.8f, 0.9f, 1.0f };
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(penType));
            }
            pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
            pen.EndCap = endCap;
            pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
        }

        /// <summary>
        /// Convert GDI+ LineCap to SkiaSharp SKStrokeCap
        /// </summary>
        /// <param name="lineCap">GDI+ LineCap</param>
        /// <returns>SkiaSharp SKStrokeCap</returns>
        public static SKStrokeCap ToSKStrokeCap(System.Drawing.Drawing2D.LineCap lineCap)
        {
            switch (lineCap)
            {
                case System.Drawing.Drawing2D.LineCap.Flat:
                    return SKStrokeCap.Butt;
                case System.Drawing.Drawing2D.LineCap.Round:
                    return SKStrokeCap.Round;
                case System.Drawing.Drawing2D.LineCap.Square:
                    return SKStrokeCap.Square;
                default:
                    return SKStrokeCap.Round;
            }
        }

        /// <summary>
        /// Convert SkiaSharp SKStrokeCap to GDI+ LineCap
        /// </summary>
        /// <param name="strokeCap">SkiaSharp SKStrokeCap</param>
        /// <returns>GDI+ LineCap</returns>
        public static System.Drawing.Drawing2D.LineCap ToLineCap(SKStrokeCap strokeCap)
        {
            switch (strokeCap)
            {
                case SKStrokeCap.Butt:
                    return System.Drawing.Drawing2D.LineCap.Flat;
                case SKStrokeCap.Round:
                    return System.Drawing.Drawing2D.LineCap.Round;
                case SKStrokeCap.Square:
                    return System.Drawing.Drawing2D.LineCap.Square;
                default:
                    return System.Drawing.Drawing2D.LineCap.Round;
            }
        }
    }
}