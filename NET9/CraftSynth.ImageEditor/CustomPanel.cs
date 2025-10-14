using System.Windows.Forms;
using SkiaSharp;

namespace CraftSynth.ImageEditor
{
    /// <summary>
    /// Source:
    /// https://nickstips.wordpress.com/2010/03/03/c-panel-resets-scroll-position-after-focus-is-lost-and-regained/
    /// 
    /// CustomPanel extends Windows Forms Panel to prevent automatic scrolling when focus changes.
    /// In WinForms, a Panel with AutoScroll calls ScrollToControl to determine the target scroll position 
    /// that brings the active control into view. By overriding ScrollToControl to return 
    /// DisplayRectangle.Location (the current viewport origin), the panel refuses to adjust its scroll offset.
    /// This preserves the user's current scroll position when the panel loses and regains focus, 
    /// avoiding the default behavior that snaps to the focused child control.
    /// 
    /// Migrated to .NET 9 with SkiaSharp compatibility for cross-platform support.
    /// </summary>
    public class CustomPanel : Panel
    {
        /// <summary>
        /// Overrides the default scroll behavior to maintain current scroll position
        /// when the panel loses and regains focus.
        /// </summary>
        /// <param name="activeControl">The control that is receiving focus</param>
        /// <returns>The current display rectangle location to prevent scrolling</returns>
        protected override System.Drawing.Point ScrollToControl(Control activeControl)
        {
            // Returning the current location prevents the panel from
            // scrolling to the active control when the panel loses and regains focus
            return this.DisplayRectangle.Location;
        }

        /// <summary>
        /// Helper method to convert System.Drawing.Point to SKPoint for SkiaSharp operations
        /// </summary>
        /// <param name="point">System.Drawing.Point to convert</param>
        /// <returns>Equivalent SKPoint</returns>
        public static SKPoint ToSKPoint(System.Drawing.Point point)
        {
            return new SKPoint(point.X, point.Y);
        }

        /// <summary>
        /// Helper method to convert SKPoint to System.Drawing.Point for WinForms compatibility
        /// </summary>
        /// <param name="point">SKPoint to convert</param>
        /// <returns>Equivalent System.Drawing.Point</returns>
        public static System.Drawing.Point FromSKPoint(SKPoint point)
        {
            return new System.Drawing.Point((int)point.X, (int)point.Y);
        }

        /// <summary>
        /// Helper method to convert System.Drawing.Rectangle to SKRect for SkiaSharp operations
        /// </summary>
        /// <param name="rectangle">System.Drawing.Rectangle to convert</param>
        /// <returns>Equivalent SKRect</returns>
        public static SKRect ToSKRect(System.Drawing.Rectangle rectangle)
        {
            return new SKRect(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
        }

        /// <summary>
        /// Helper method to convert SKRect to System.Drawing.Rectangle for WinForms compatibility
        /// </summary>
        /// <param name="rect">SKRect to convert</param>
        /// <returns>Equivalent System.Drawing.Rectangle</returns>
        public static System.Drawing.Rectangle FromSKRect(SKRect rect)
        {
            return new System.Drawing.Rectangle(
                (int)rect.Left,
                (int)rect.Top,
                (int)rect.Width,
                (int)rect.Height
            );
        }

        /// <summary>
        /// Helper method to convert System.Drawing.Color to SKColor for SkiaSharp operations
        /// </summary>
        /// <param name="color">System.Drawing.Color to convert</param>
        /// <returns>Equivalent SKColor</returns>
        public static SKColor ToSKColor(System.Drawing.Color color)
        {
            return new SKColor(color.R, color.G, color.B, color.A);
        }

        /// <summary>
        /// Helper method to convert SKColor to System.Drawing.Color for WinForms compatibility
        /// </summary>
        /// <param name="color">SKColor to convert</param>
        /// <returns>Equivalent System.Drawing.Color</returns>
        public static System.Drawing.Color FromSKColor(SKColor color)
        {
            return System.Drawing.Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
        }
    }
}