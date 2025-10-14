using System;
using SkiaSharp;

namespace CraftSynth.ImageEditor
{
    public static class DrawingPens
    {
        public enum PenType
        {
            Solid,
            Dash,
            Dash_Dot,
            Dot,
            DoubleLine
        }

        public static string GetPenTypeAsString(PenType penType)
        {
            return penType switch
            {
                PenType.Solid => "___",
                PenType.Dash => "- - -",
                PenType.Dash_Dot => "- . -",
                PenType.Dot => ". . .",
                PenType.DoubleLine => "===",
                _ => throw new ArgumentOutOfRangeException(nameof(penType))
            };
        }

        /// <summary>
        /// Configure the SKPaint based on requested pen type.
        /// </summary>
        public static void ConfigurePaint(SKPaint paint, PenType penType)
        {
            switch (penType)
            {
                case PenType.Solid:
                    paint.PathEffect = null;
                    break;
                case PenType.Dash:
                    paint.PathEffect = SKPathEffect.CreateDash(new float[] { 12, 6 }, 0);
                    break;
                case PenType.Dash_Dot:
                    paint.PathEffect = SKPathEffect.CreateDash(new float[] { 12, 6, 2, 6 }, 0);
                    break;
                case PenType.Dot:
                    paint.PathEffect = SKPathEffect.CreateDash(new float[] { 2, 4 }, 0);
                    break;
                case PenType.DoubleLine:
                    // Emulate a double line by stroking twice with different widths using callers' logic
                    // (callers can draw twice using thicker and thinner strokes)
                    paint.PathEffect = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(penType));
            }
        }
    }
}
