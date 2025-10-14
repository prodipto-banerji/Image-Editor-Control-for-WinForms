using System;
using System.Windows.Forms;
using SkiaSharp;

namespace CraftSynth.ImageEditor
{
    /// <summary>
    /// Text tool for creating text objects in the drawing area
    /// </summary>
    internal class ToolText : ToolObject
    {
        private static string? _lastText;
        private static SKTypeface? _lastTypeface;
        private static float _lastTextSize = 12f;
        private static SKFontStyle _lastFontStyle = SKFontStyle.Normal;

        public ToolText()
        {
            Cursor = new Cursor(GetType(), "TextTool.cur");
        }

        public override void OnMouseDown(DrawArea drawArea, MouseEventArgs e)
        {
            TextDialog td = new TextDialog();
            td.TopLevel = true;
            td.TopMost = true;
            td.TheColor = drawArea.LineColor;
            td.TheText = _lastText ?? "";
            
            // Convert SkiaSharp font properties to System.Drawing.Font for dialog compatibility
            System.Drawing.Font dialogFont;
            if (_lastTypeface != null)
            {
                var fontFamily = System.Drawing.FontFamily.GenericSansSerif;
                var fontSize = _lastTextSize;
                var fontStyle = ConvertSKFontStyleToDrawingFontStyle(_lastFontStyle);
                dialogFont = new System.Drawing.Font(fontFamily, fontSize, fontStyle);
            }
            else
            {
                dialogFont = new System.Drawing.Font(System.Drawing.FontFamily.GenericSansSerif, 12, System.Drawing.FontStyle.Regular);
            }
            
            td.TheFont = dialogFont;
            td.Zoom = drawArea.Zoom;
            td.StartPosition = FormStartPosition.Manual;
            
            System.Drawing.Point pnlLocationOnScreen = drawArea.MyParent.pnlDrawArea.PointToScreen(new System.Drawing.Point(0, 0));
            System.Drawing.Point pp = e.Location;
            pp = new System.Drawing.Point(
                pnlLocationOnScreen.X + pp.X // hit point on screen
                - 18 - SystemInformation.Border3DSize.Width - SystemInformation.SizingBorderWidth // -text box location
                + drawArea.Left, // +scroll amount
                pnlLocationOnScreen.Y + pp.Y // hit point on screen
                - 18 - SystemInformation.Border3DSize.Height - SystemInformation.SizingBorderWidth - SystemInformation.CaptionHeight // -text box location
                + drawArea.Top // +scroll amount
            );
            td.Location = pp;
            
            if (td.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(td.TheText))
            {
                _lastText = td.TheText;
                
                // Convert System.Drawing.Font to SkiaSharp font properties
                var drawingFont = td.TheFont;
                _lastTypeface = ConvertToSKTypeface(drawingFont);
                _lastTextSize = drawingFont.Size;
                _lastFontStyle = ConvertDrawingFontStyleToSKFontStyle(drawingFont.Style);
                
                string t = td.TheText;
                System.Drawing.Color c = td.TheColor;
                
                System.Drawing.Point p = drawArea.MyParent.PointToClient(td.Location);
                p = new System.Drawing.Point(p.X + 17 - drawArea.Left, p.Y + 15 - drawArea.Top);
                p = drawArea.BackTrackMouse(p);
                
                // Create DrawText with SkiaSharp font properties
                AddNewObject(drawArea, new DrawText(p.X, p.Y, t, _lastTypeface, _lastTextSize, _lastFontStyle, c));

                int al = drawArea.TheLayers.ActiveLayerIndex;
                drawArea.AddCommandToHistory(new CommandAdd(drawArea.TheLayers[al].Graphics[0]));

                drawArea.ActiveTool = DrawArea.DrawToolType.Pointer;
            }
            
            // Dispose the temporary font
            dialogFont.Dispose();
        }

        public override void OnMouseMove(DrawArea drawArea, MouseEventArgs e)
        {
            drawArea.Cursor = Cursor;
            if (e.Button == MouseButtons.Left)
            {
                System.Drawing.Point point = drawArea.BackTrackMouse(new System.Drawing.Point(e.X, e.Y));
                int al = drawArea.TheLayers.ActiveLayerIndex;
                drawArea.TheLayers[al].Graphics[0].MoveHandleTo(point, 5);
                drawArea.Refresh();
            }
        }

        #region Helper Methods

        /// <summary>
        /// Convert System.Drawing.Font to SKTypeface
        /// </summary>
        private static SKTypeface ConvertToSKTypeface(System.Drawing.Font font)
        {
            var fontWeight = font.Bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal;
            var fontSlant = font.Italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright;
            var fontWidth = SKFontStyleWidth.Normal;
            
            var skFontStyle = new SKFontStyle(fontWeight, fontWidth, fontSlant);
            
            // Try to match the font family name
            var typeface = SKTypeface.FromFamilyName(font.FontFamily.Name, skFontStyle);
            
            // Fallback to default if font not found
            return typeface ?? SKTypeface.Default;
        }

        /// <summary>
        /// Convert System.Drawing.FontStyle to SKFontStyle
        /// </summary>
        private static SKFontStyle ConvertDrawingFontStyleToSKFontStyle(System.Drawing.FontStyle fontStyle)
        {
            var weight = (fontStyle & System.Drawing.FontStyle.Bold) != 0 ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal;
            var slant = (fontStyle & System.Drawing.FontStyle.Italic) != 0 ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright;
            var width = SKFontStyleWidth.Normal;
            
            return new SKFontStyle(weight, width, slant);
        }

        /// <summary>
        /// Convert SKFontStyle to System.Drawing.FontStyle for dialog compatibility
        /// </summary>
        private static System.Drawing.FontStyle ConvertSKFontStyleToDrawingFontStyle(SKFontStyle skFontStyle)
        {
            var style = System.Drawing.FontStyle.Regular;
            
            if (skFontStyle.Weight >= SKFontStyleWeight.SemiBold)
                style |= System.Drawing.FontStyle.Bold;
                
            if (skFontStyle.Slant != SKFontStyleSlant.Upright)
                style |= System.Drawing.FontStyle.Italic;
                
            return style;
        }

        #endregion

        #region Destruction
        private bool _disposed = false;

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Free any managed objects here
                    _lastTypeface?.Dispose();
                    _lastTypeface = null;
                }

                // Free any unmanaged objects here

                _disposed = true;
            }
            base.Dispose(disposing);
        }

        ~ToolText()
        {
            Dispose(false);
        }
        #endregion
    }
}