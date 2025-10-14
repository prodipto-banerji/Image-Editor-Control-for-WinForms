using System;
using System.Globalization;
using System.Runtime.Serialization;
using SkiaSharp;

namespace CraftSynth.ImageEditor
{
    /// <summary>
    /// Text graphic object (SkiaSharp)
    /// </summary>
    public class DrawText : DrawObject
    {
        private SKRect rectangle;
        private string _theText = string.Empty;
        private float _fontSize = 16f;
        private string _fontFamily = "Arial";
        private bool _bold, _italic, _strikeout, _underline;
        private bool _disposed;

        private const string entryRectangle = "Rect";
        private const string entryText = "Text";
        private const string entryFontName = "FontName";
        private const string entryFontBold = "FontBold";
        private const string entryFontItalic = "FontItalic";
        private const string entryFontSize = "FontSize";
        private const string entryFontStrikeout = "FontStrikeout";
        private const string entryFontUnderline = "FontUnderline";

        protected string TheText
        {
            get => _theText;
            set { _theText = value ?? string.Empty; TipText = _theText; }
        }

        public float FontSize { get => _fontSize; set => _fontSize = value; }
        public string FontFamily { get => _fontFamily; set => _fontFamily = value; }
        public bool Bold { get => _bold; set => _bold = value; }
        public bool Italic { get => _italic; set => _italic = value; }
        public bool Strikeout { get => _strikeout; set => _strikeout = value; }
        public bool Underline { get => _underline; set => _underline = value; }

        protected SKRect Rectangle
        {
            get => rectangle;
            set => rectangle = value;
        }

        public DrawText() { }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
            }
            base.Dispose(disposing);
        }

        ~DrawText() { Dispose(false); }

        public DrawText(float x, float y, string textToDraw, string fontFamily, float fontSize, SKColor textColor)
        {
            rectangle = new SKRect(x, y, x + 1, y + 1);
            _theText = textToDraw;
            _fontFamily = fontFamily;
            _fontSize = fontSize;
            Color = textColor;
        }

        public override DrawObject Clone()
        {
            var d = new DrawText();
            d._fontFamily = _fontFamily; d._fontSize = _fontSize; d._theText = _theText;
            d.rectangle = rectangle; d._bold = _bold; d._italic = _italic; d._strikeout = _strikeout; d._underline = _underline;
            FillDrawObjectFields(d);
            return d;
        }

        public override void Draw(SKCanvas canvas)
        {
            // SkiaSharp fonts use typeface
            using var tf = SKTypeface.FromFamilyName(_fontFamily, (_bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal), SKFontStyleWidth.Normal, _italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright);
            using var paint = new SKPaint { Color = Color, Typeface = tf, TextSize = _fontSize + 7, IsAntialias = true };

            // Underline/Strikeout cannot be directly set; draw line decorations if requested
            var x = rectangle.Left; var y = rectangle.Top + paint.TextSize;
            if (Rotation != 0)
            {
                var b = new SKRect();
                paint.MeasureText(_theText, ref b);
                var cx = rectangle.Left + b.Width / 2f; var cy = rectangle.Top + b.Height / 2f;
                canvas.Save(); canvas.RotateDegrees(Rotation, cx, cy);
                canvas.DrawText(_theText, x, y, paint);
                DrawTextDecorations(canvas, x, y, b, paint);
                canvas.Restore();
            }
            else
            {
                canvas.DrawText(_theText, x, y, paint);
                var b = new SKRect(); paint.MeasureText(_theText, ref b);
                DrawTextDecorations(canvas, x, y, b, paint);
            }

            // Update rectangle size based on measured text
            var bounds = new SKRect();
            paint.MeasureText(_theText, ref bounds);
            rectangle.Right = rectangle.Left + bounds.Width;
            rectangle.Bottom = rectangle.Top + bounds.Height + paint.TextSize * 0.2f;
        }

        private void DrawTextDecorations(SKCanvas canvas, float x, float baselineY, SKRect measured, SKPaint paint)
        {
            using var deco = new SKPaint { Color = paint.Color, StrokeWidth = Math.Max(1, paint.TextSize / 16f), Style = SKPaintStyle.Stroke, IsAntialias = true };
            if (_underline)
            {
                float underlineY = baselineY + deco.StrokeWidth;
                canvas.DrawLine(x, underlineY, x + measured.Width, underlineY, deco);
            }
            if (_strikeout)
            {
                float strikeY = baselineY - paint.TextSize * 0.5f;
                canvas.DrawLine(x, strikeY, x + measured.Width, strikeY, deco);
            }
        }

        public override int HandleCount => 8;

        public override SKPoint GetHandle(int handleNumber)
        {
            float xCenter = rectangle.MidX; float yCenter = rectangle.MidY; float x = rectangle.Left; float y = rectangle.Top;
            return handleNumber switch
            {
                1 => new SKPoint(rectangle.Left, rectangle.Top),
                2 => new SKPoint(xCenter, rectangle.Top),
                3 => new SKPoint(rectangle.Right, rectangle.Top),
                4 => new SKPoint(rectangle.Right, yCenter),
                5 => new SKPoint(rectangle.Right, rectangle.Bottom),
                6 => new SKPoint(xCenter, rectangle.Bottom),
                7 => new SKPoint(rectangle.Left, rectangle.Bottom),
                8 => new SKPoint(rectangle.Left, yCenter),
                _ => new SKPoint(x, y)
            };
        }

        public override int HitTest(SKPoint point)
        {
            if (Selected)
            {
                for (int i = 1; i <= HandleCount; i++) if (GetHandleRectangle(i).Contains(point)) return i;
            }
            return PointInObject(point) ? 0 : -1;
        }

        protected override bool PointInObject(SKPoint point) => rectangle.Contains(point.X, point.Y);
        public override SKRect GetBounds() => rectangle;
        public override bool IntersectsWith(SKRect rect) => rectangle.IntersectsWith(rect);

        public override void Move(float dx, float dy)
        {
            rectangle.Offset(dx, dy);
            Dirty = true;
        }

        public override void SaveToStream(SerializationInfo info, int orderNumber, int objectIndex)
        {
            info.AddValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryRectangle, orderNumber, objectIndex), rectangle);
            info.AddValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryText, orderNumber, objectIndex), _theText);
            info.AddValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryFontName, orderNumber, objectIndex), _fontFamily);
            info.AddValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryFontBold, orderNumber, objectIndex), _bold);
            info.AddValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryFontItalic, orderNumber, objectIndex), _italic);
            info.AddValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryFontSize, orderNumber, objectIndex), _fontSize);
            info.AddValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryFontStrikeout, orderNumber, objectIndex), _strikeout);
            info.AddValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryFontUnderline, orderNumber, objectIndex), _underline);
            base.SaveToStream(info, orderNumber, objectIndex);
        }

        public override void LoadFromStream(SerializationInfo info, int orderNumber, int objectIndex)
        {
            rectangle = (SKRect)info.GetValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryRectangle, orderNumber, objectIndex), typeof(SKRect));
            _theText = info.GetString(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryText, orderNumber, objectIndex));
            _fontFamily = info.GetString(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryFontName, orderNumber, objectIndex));
            _bold = info.GetBoolean(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryFontBold, orderNumber, objectIndex));
            _italic = info.GetBoolean(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryFontItalic, orderNumber, objectIndex));
            _fontSize = info.GetSingle(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryFontSize, orderNumber, objectIndex));
            _strikeout = info.GetBoolean(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryFontStrikeout, orderNumber, objectIndex));
            _underline = info.GetBoolean(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryFontUnderline, orderNumber, objectIndex));
            base.LoadFromStream(info, orderNumber, objectIndex);
        }
    }
}
