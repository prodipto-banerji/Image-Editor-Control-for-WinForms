using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Windows.Forms;
using SkiaSharp;

namespace CraftSynth.ImageEditor
{
    /// <summary>
    /// Text graphic object for .NET 9 using SkiaSharp
    /// </summary>
    public class DrawText : DrawObject
    {
        private SKRect rectangle;
        private string _theText;
        private SKTypeface _typeface;
        private float _fontSize;
        private SKFontStyle _fontStyle;
        private bool _disposed;

        protected string TheText
        {
            get { return _theText; }
            set
            {
                _theText = value;
                TipText = value;
            }
        }

        public SKTypeface TheTypeface
        {
            get { return _typeface; }
            set { _typeface = value; }
        }

        public float TheFontSize
        {
            get { return _fontSize; }
            set { _fontSize = value; }
        }

        public SKFontStyle TheFontStyle
        {
            get { return _fontStyle; }
            set { _fontStyle = value; }
        }

        private const string entryRectangle = "Rect";
        private const string entryText = "Text";
        private const string entryFontName = "FontName";
        private const string entryFontBold = "FontBold";
        private const string entryFontItalic = "FontItalic";
        private const string entryFontSize = "FontSize";
        private const string entryFontStrikeout = "FontStrikeout";
        private const string entryFontUnderline = "FontUnderline";

        protected SKRect Rectangle
        {
            get { return rectangle; }
            set { rectangle = value; }
        }

        public DrawText()
        {
            _theText = "";
            _typeface = SKTypeface.Default;
            _fontSize = 12.0f;
            _fontStyle = SKFontStyle.Normal;
            rectangle = new SKRect(0, 0, 1, 1);
            Initialize();
        }

        /// <summary>
        /// Clone this instance
        /// </summary>
        public override DrawObject Clone()
        {
            DrawText drawText = new DrawText();

            drawText._typeface = _typeface;
            drawText._fontSize = _fontSize;
            drawText._fontStyle = _fontStyle;
            drawText._theText = _theText;
            drawText.rectangle = rectangle;

            FillDrawObjectFields(drawText);
            return drawText;
        }

        #region Destruction
        protected override void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    // SkiaSharp typefaces are managed by the library and don't need explicit disposal
                    // unless they are custom created typefaces
                }

                this._disposed = true;
            }
            base.Dispose(disposing);
        }

        ~DrawText()
        {
            this.Dispose(false);
        }
        #endregion

        public DrawText(int x, int y, string textToDraw, System.Drawing.Font textFont, System.Drawing.Color textColor)
        {
            rectangle = new SKRect(x, y, x + 100, y + 20); // Initial size, will be adjusted
            _theText = textToDraw;
            
            // Convert System.Drawing.Font to SkiaSharp equivalents
            _typeface = SKTypeface.FromFamilyName(textFont.FontFamily.Name, ConvertFontStyle(textFont.Style));
            _fontSize = textFont.Size;
            _fontStyle = ConvertFontStyle(textFont.Style);
            
            Color = textColor;
            Initialize();
        }

        /// <summary>
        /// Draw text using SkiaSharp
        /// </summary>
        /// <param name="g">Graphics object (for compatibility, but not used)</param>
        public override void Draw(System.Drawing.Graphics g)
        {
            // This method signature is kept for base class compatibility
            // In a real implementation, you would want to change the base class to use SKCanvas
            // For now, this is a placeholder that would need canvas to be passed differently
        }

        /// <summary>
        /// Draw text using SkiaSharp canvas
        /// </summary>
        /// <param name="canvas">SKCanvas to draw on</param>
        public void DrawSkia(SKCanvas canvas)
        {
            if (string.IsNullOrEmpty(_theText))
                return;

            using (var paint = new SKPaint())
            {
                paint.IsAntialias = true;
                paint.Color = ColorToSKColor(Color);
                paint.Typeface = _typeface;
                
                // Apply font size workaround similar to original (adding 7 to match dialog)
                paint.TextSize = _fontSize + 7;
                paint.Style = SKPaintStyle.Fill;

                // Measure text to update rectangle size
                var textBounds = new SKRect();
                paint.MeasureText(_theText, ref textBounds);
                
                // Update rectangle size based on text measurements
                rectangle = new SKRect(rectangle.Left, rectangle.Top, 
                                     rectangle.Left + textBounds.Width, 
                                     rectangle.Top + textBounds.Height);

                // Create text path for outline and fill
                using (var textPath = paint.GetTextPath(_theText, rectangle.Left, rectangle.Top + textBounds.Height))
                {
                    // Apply rotation if necessary
                    if (Rotation != 0)
                    {
                        var bounds = textPath.Bounds;
                        var centerX = bounds.Left + (bounds.Width / 2);
                        var centerY = bounds.Top + (bounds.Height / 2);

                        canvas.Save();
                        canvas.RotateDegrees(Rotation, centerX, centerY);
                    }

                    // Draw text outline
                    paint.Style = SKPaintStyle.Stroke;
                    paint.StrokeWidth = 1;
                    canvas.DrawPath(textPath, paint);

                    // Fill text
                    paint.Style = SKPaintStyle.Fill;
                    canvas.DrawPath(textPath, paint);

                    if (Rotation != 0)
                    {
                        canvas.Restore();
                    }
                }
            }
        }

        /// <summary>
        /// Get number of handles
        /// </summary>
        public override int HandleCount
        {
            get { return 8; }
        }

        /// <summary>
        /// Get handle point by 1-based number
        /// </summary>
        /// <param name="handleNumber"></param>
        /// <returns></returns>
        public override System.Drawing.Point GetHandle(int handleNumber)
        {
            int x, y;
            float xCenter = rectangle.Left + rectangle.Width / 2;
            float yCenter = rectangle.Top + rectangle.Height / 2;

            switch (handleNumber)
            {
                case 1:
                    x = (int)rectangle.Left;
                    y = (int)rectangle.Top;
                    break;
                case 2:
                    x = (int)xCenter;
                    y = (int)rectangle.Top;
                    break;
                case 3:
                    x = (int)rectangle.Right;
                    y = (int)rectangle.Top;
                    break;
                case 4:
                    x = (int)rectangle.Right;
                    y = (int)yCenter;
                    break;
                case 5:
                    x = (int)rectangle.Right;
                    y = (int)rectangle.Bottom;
                    break;
                case 6:
                    x = (int)xCenter;
                    y = (int)rectangle.Bottom;
                    break;
                case 7:
                    x = (int)rectangle.Left;
                    y = (int)rectangle.Bottom;
                    break;
                case 8:
                    x = (int)rectangle.Left;
                    y = (int)yCenter;
                    break;
                default:
                    x = (int)rectangle.Left;
                    y = (int)rectangle.Top;
                    break;
            }

            return new System.Drawing.Point(x, y);
        }

        /// <summary>
        /// Hit test.
        /// Return value: -1 - no hit
        ///                0 - hit anywhere
        ///                > 1 - handle number
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public override int HitTest(System.Drawing.Point point)
        {
            if (Selected)
            {
                for (int i = 1; i <= HandleCount; i++)
                {
                    if (GetHandleRectangle(i).Contains(point))
                        return i;
                }
            }

            if (PointInObject(point))
                return 0;

            return -1;
        }

        protected override bool PointInObject(System.Drawing.Point point)
        {
            var skRect = new SKRectI((int)rectangle.Left, (int)rectangle.Top, 
                                   (int)rectangle.Right, (int)rectangle.Bottom);
            return skRect.Contains(point.X, point.Y);
        }

        public override System.Drawing.Rectangle GetBounds(System.Drawing.Graphics g)
        {
            return new System.Drawing.Rectangle((int)rectangle.Left, (int)rectangle.Top, 
                                              (int)rectangle.Width, (int)rectangle.Height);
        }

        /// <summary>
        /// Get cursor for the handle
        /// </summary>
        /// <param name="handleNumber"></param>
        /// <returns></returns>
        public override Cursor GetHandleCursor(int handleNumber)
        {
            return Cursors.Default;
        }

        /// <summary>
        /// Move handle to new point (resizing)
        /// </summary>
        /// <param name="point"></param>
        /// <param name="handleNumber"></param>
        public override void MoveHandleTo(System.Drawing.Point point, int handleNumber)
        {
            // Resizing for text objects is typically disabled in text editors
            // as text size should be controlled by font size
            // This is left as a placeholder for potential future implementation
        }

        public override bool IntersectsWith(System.Drawing.Rectangle rectangle)
        {
            var skRect = new SKRectI(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
            var thisRect = new SKRectI((int)this.rectangle.Left, (int)this.rectangle.Top,
                                     (int)this.rectangle.Right, (int)this.rectangle.Bottom);
            return SKRectI.Intersect(thisRect, skRect) != SKRectI.Empty;
        }

        /// <summary>
        /// Move object
        /// </summary>
        /// <param name="deltaX"></param>
        /// <param name="deltaY"></param>
        public override void Move(int deltaX, int deltaY)
        {
            rectangle = new SKRect(rectangle.Left + deltaX, rectangle.Top + deltaY,
                                 rectangle.Right + deltaX, rectangle.Bottom + deltaY);
            Dirty = true;
        }

        public override void Dump()
        {
            // For debugging - implementation left as placeholder
        }

        /// <summary>
        /// Normalize rectangle
        /// </summary>
        public override void Normalize()
        {
            // Text objects typically don't need normalization
            // as their bounds are determined by the text content and font
        }

        /// <summary>
        /// Save object to serialization stream
        /// </summary>
        /// <param name="info"></param>
        /// <param name="orderNumber">Index of the Layer being saved</param>
        /// <param name="objectIndex">Index of this object in the Layer</param>
        public override void SaveToStream(SerializationInfo info, int orderNumber, int objectIndex)
        {
            // Convert SKRect to System.Drawing.Rectangle for serialization compatibility
            var rect = new System.Drawing.Rectangle((int)rectangle.Left, (int)rectangle.Top,
                                                   (int)rectangle.Width, (int)rectangle.Height);
            
            info.AddValue(
                String.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryRectangle, orderNumber, objectIndex),
                rect);
            
            info.AddValue(
                String.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryText, orderNumber, objectIndex),
                _theText);
            
            info.AddValue(
                String.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryFontName, orderNumber, objectIndex),
                _typeface?.FamilyName ?? SKTypeface.Default.FamilyName);
            
            info.AddValue(
                String.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryFontBold, orderNumber, objectIndex),
                _fontStyle.Weight >= SKFontStyleWeight.Bold.Value);
            
            info.AddValue(
                String.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryFontItalic, orderNumber, objectIndex),
                _fontStyle.Slant != SKFontStyleSlant.Upright);
            
            info.AddValue(
                String.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryFontSize, orderNumber, objectIndex),
                _fontSize);
            
            // SkiaSharp doesn't have direct strikeout/underline in font style
            // These would be handled through text decorations in drawing
            info.AddValue(
                String.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryFontStrikeout, orderNumber, objectIndex),
                false);
            
            info.AddValue(
                String.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryFontUnderline, orderNumber, objectIndex),
                false);

            base.SaveToStream(info, orderNumber, objectIndex);
        }

        /// <summary>
        /// Load object from serialization stream
        /// </summary>
        /// <param name="info"></param>
        /// <param name="orderNumber"></param>
        /// <param name="objectIndex"></param>
        public override void LoadFromStream(SerializationInfo info, int orderNumber, int objectIndex)
        {
            var rect = (System.Drawing.Rectangle)info.GetValue(
                                    String.Format(CultureInfo.InvariantCulture,
                                                  "{0}{1}-{2}",
                                                  entryRectangle, orderNumber, objectIndex),
                                    typeof(System.Drawing.Rectangle));

            rectangle = new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom);

            _theText = info.GetString(
                String.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryText, orderNumber, objectIndex));

            string familyName = info.GetString(
                String.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryFontName, orderNumber, objectIndex));

            bool bold = info.GetBoolean(
                String.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryFontBold, orderNumber, objectIndex));

            bool italic = info.GetBoolean(
                String.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryFontItalic, orderNumber, objectIndex));

            _fontSize = (float)info.GetValue(
                                String.Format(CultureInfo.InvariantCulture,
                                              "{0}{1}-{2}",
                                              entryFontSize, orderNumber, objectIndex),
                                typeof(float));

            // Note: strikeout and underline are loaded but not used in SkiaSharp font style
            // They would need to be handled through text decorations during drawing
            bool strikeout = info.GetBoolean(
                String.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryFontStrikeout, orderNumber, objectIndex));

            bool underline = info.GetBoolean(
                String.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryFontUnderline, orderNumber, objectIndex));

            // Create SkiaSharp font style
            var weight = bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal;
            var slant = italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright;
            _fontStyle = new SKFontStyle(weight, SKFontStyleWidth.Normal, slant);
            _typeface = SKTypeface.FromFamilyName(familyName, _fontStyle);

            base.LoadFromStream(info, orderNumber, objectIndex);
        }

        #region Helper Functions

        /// <summary>
        /// Convert System.Drawing.Color to SKColor
        /// </summary>
        private SKColor ColorToSKColor(System.Drawing.Color color)
        {
            return new SKColor(color.R, color.G, color.B, color.A);
        }

        /// <summary>
        /// Convert System.Drawing.FontStyle to SKFontStyle
        /// </summary>
        private SKFontStyle ConvertFontStyle(System.Drawing.FontStyle fontStyle)
        {
            var weight = fontStyle.HasFlag(System.Drawing.FontStyle.Bold) ? 
                        SKFontStyleWeight.Bold : SKFontStyleWeight.Normal;
            var slant = fontStyle.HasFlag(System.Drawing.FontStyle.Italic) ? 
                       SKFontStyleSlant.Italic : SKFontStyleSlant.Upright;
            
            return new SKFontStyle(weight, SKFontStyleWidth.Normal, slant);
        }

        /// <summary>
        /// Get normalized rectangle from SkiaSharp rectangle
        /// </summary>
        public static SKRect GetNormalizedRectangle(float x1, float y1, float x2, float y2)
        {
            if (x2 < x1)
            {
                (x1, x2) = (x2, x1);
            }

            if (y2 < y1)
            {
                (y1, y2) = (y2, y1);
            }

            return new SKRect(x1, y1, x2, y2);
        }

        public static SKRect GetNormalizedRectangle(SKPoint p1, SKPoint p2)
        {
            return GetNormalizedRectangle(p1.X, p1.Y, p2.X, p2.Y);
        }

        public static SKRect GetNormalizedRectangle(SKRect r)
        {
            return GetNormalizedRectangle(r.Left, r.Top, r.Right, r.Bottom);
        }

        #endregion
    }
}