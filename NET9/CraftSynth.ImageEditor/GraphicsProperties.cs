using SkiaSharp;

namespace CraftSynth.ImageEditor
{
    /// <summary>
    /// Helper class used to show properties
    /// for one or more graphic objects
    /// </summary>
    internal class GraphicsProperties
    {
        private SKColor? color;
        private int? penWidth;

        public GraphicsProperties()
        {
            color = null;
            penWidth = null;
        }

        public SKColor? Color
        {
            get { return color; }
            set { color = value; }
        }

        public int? PenWidth
        {
            get { return penWidth; }
            set { penWidth = value; }
        }

        /// <summary>
        /// Convert System.Drawing.Color to SKColor for backward compatibility
        /// </summary>
        /// <param name="drawingColor">System.Drawing.Color to convert</param>
        /// <returns>Equivalent SKColor</returns>
        public static SKColor ConvertFromDrawingColor(System.Drawing.Color drawingColor)
        {
            return new SKColor(drawingColor.R, drawingColor.G, drawingColor.B, drawingColor.A);
        }

        /// <summary>
        /// Convert SKColor to System.Drawing.Color for backward compatibility
        /// (used where System.Drawing.Color is still needed for Windows Forms controls)
        /// </summary>
        /// <param name="skColor">SKColor to convert</param>
        /// <returns>Equivalent System.Drawing.Color</returns>
        public static System.Drawing.Color ConvertToDrawingColor(SKColor skColor)
        {
            return System.Drawing.Color.FromArgb(skColor.Alpha, skColor.Red, skColor.Green, skColor.Blue);
        }

        /// <summary>
        /// Set color from System.Drawing.Color for compatibility
        /// </summary>
        /// <param name="drawingColor">System.Drawing.Color to set</param>
        public void SetColorFromDrawing(System.Drawing.Color drawingColor)
        {
            color = ConvertFromDrawingColor(drawingColor);
        }

        /// <summary>
        /// Get color as System.Drawing.Color for compatibility with Windows Forms controls
        /// </summary>
        /// <returns>System.Drawing.Color or null if not set</returns>
        public System.Drawing.Color? GetColorAsDrawing()
        {
            if (color.HasValue)
            {
                return ConvertToDrawingColor(color.Value);
            }
            return null;
        }
    }
}