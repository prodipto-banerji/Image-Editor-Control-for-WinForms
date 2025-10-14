using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;
using SkiaSharp;

namespace CraftSynth.ImageEditor
{
    /// <summary>
    /// Image graphic object (SkiaSharp)
    /// </summary>
    public class DrawImage : DrawObject
    {
        public SKRect rectangle;
        private SKBitmap? _image;
        private SKBitmap? _originalImage; // holds the original image unscaled
        public bool IsInitialImage;

        public SKBitmap? TheImage
        {
            get => _image;
            set
            {
                _originalImage = value;
                if (value != null)
                {
                    ResizeImage(rectangle.Width, rectangle.Height);
                }
            }
        }

        private const string entryRectangle = "Rect";
        private const string entryImage = "Image";
        private const string entryImageOriginal = "OriginalImage";

        private bool _disposed;

        public override DrawObject Clone()
        {
            var d = new DrawImage
            {
                _image = _image,
                _originalImage = _originalImage,
                rectangle = rectangle,
                IsInitialImage = IsInitialImage
            };
            FillDrawObjectFields(d);
            return d;
        }

        protected SKRect Rectangle
        {
            get => rectangle;
            set => rectangle = value;
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _originalImage?.Dispose();
                    _image?.Dispose();
                }
                _disposed = true;
            }
            base.Dispose(disposing);
        }

        ~DrawImage() { Dispose(false); }

        public DrawImage()
        {
            SetRectangle(0, 0, 1, 1);
        }

        public DrawImage(float x, float y, bool isInitialImage)
        {
            rectangle = new SKRect(x, y, x + 1, y + 1);
            IsInitialImage = isInitialImage;
        }

        public DrawImage(float x, float y, SKBitmap image)
        {
            rectangle.Left = x;
            rectangle.Top = y;
            _image = image.Copy();
            SetRectangle(rectangle.Left, rectangle.Top, image.Width, image.Height);
            Center = new SKPoint(x + (image.Width / 2f), y + (image.Height / 2f));
            TipText = string.Format(CultureInfo.InvariantCulture, "Image Center @ {0}, {1}", Center.X, Center.Y);
        }

        public override void Draw(SKCanvas canvas)
        {
            if (Rotation != 0)
            {
                var r = rectangle;
                var cx = r.MidX; var cy = r.MidY;
                canvas.Save();
                canvas.RotateDegrees(Rotation, cx, cy);
                if (_image == null)
                {
                    using var p = new SKPaint { Color = SKColors.Black, Style = SKPaintStyle.Stroke, StrokeWidth = 1 };
                    canvas.DrawRect(r, p);
                }
                else
                {
                    canvas.DrawBitmap(_image, new SKPoint(rectangle.Left, rectangle.Top));
                }
                canvas.Restore();
            }
            else
            {
                if (_image == null)
                {
                    using var p = new SKPaint { Color = SKColors.Black, Style = SKPaintStyle.Stroke, StrokeWidth = 1 };
                    canvas.DrawRect(rectangle, p);
                }
                else
                {
                    canvas.DrawBitmap(_image, new SKPoint(rectangle.Left, rectangle.Top));
                }
            }
        }

        protected void SetRectangle(float x, float y, float width, float height)
        {
            rectangle = new SKRect(x, y, x + width, y + height);
        }

        public override int HandleCount => 8;

        public override SKPoint GetHandle(int handleNumber)
        {
            float xCenter = rectangle.MidX;
            float yCenter = rectangle.MidY;
            float x = rectangle.Left;
            float y = rectangle.Top;
            switch (handleNumber)
            {
                case 1: x = rectangle.Left; y = rectangle.Top; break;
                case 2: x = xCenter; y = rectangle.Top; break;
                case 3: x = rectangle.Right; y = rectangle.Top; break;
                case 4: x = rectangle.Right; y = yCenter; break;
                case 5: x = rectangle.Right; y = rectangle.Bottom; break;
                case 6: x = xCenter; y = rectangle.Bottom; break;
                case 7: x = rectangle.Left; y = rectangle.Bottom; break;
                case 8: x = rectangle.Left; y = yCenter; break;
            }
            return new SKPoint(x, y);
        }

        public override int HitTest(SKPoint point)
        {
            if (Selected)
            {
                for (int i = 1; i <= HandleCount; i++)
                {
                    if (GetHandleRectangle(i).Contains(point)) return i;
                }
            }
            return PointInObject(point) ? 0 : -1;
        }

        protected override bool PointInObject(SKPoint point)
        {
            return rectangle.Contains(point.X, point.Y);
        }

        public override SKRect GetBounds()
        {
            return rectangle;
        }

        public override bool IntersectsWith(SKRect rect)
        {
            return rectangle.IntersectsWith(rect);
        }

        public override void MoveHandleTo(SKPoint point, int handleNumber)
        {
            float left = rectangle.Left;
            float top = rectangle.Top;
            float right = rectangle.Right;
            float bottom = rectangle.Bottom;

            switch (handleNumber)
            {
                case 1: left = point.X; top = point.Y; break;
                case 2: top = point.Y; break;
                case 3: right = point.X; top = point.Y; break;
                case 4: right = point.X; break;
                case 5: right = point.X; bottom = point.Y; break;
                case 6: bottom = point.Y; break;
                case 7: left = point.X; bottom = point.Y; break;
                case 8: left = point.X; break;
            }
            Dirty = true;
            SetRectangle(left, top, right - left, bottom - top);
            ResizeImage(rectangle.Width, rectangle.Height);
        }

        protected void ResizeImage(float width, float height)
        {
            if (_originalImage != null)
            {
                // Scale original to requested size using SKBitmap resize
                var info = new SKImageInfo((int)Math.Max(1, width), (int)Math.Max(1, height));
                using var scaled = _originalImage.Resize(info, SKFilterQuality.Medium);
                _image?.Dispose();
                _image = scaled?.Copy();
            }
        }

        public override void Move(float deltaX, float deltaY)
        {
            rectangle.Offset(deltaX, deltaY);
            Dirty = true;
        }

        public override void Normalize()
        {
            rectangle = DrawImage.GetNormalizedRectangle(rectangle);
        }

        public override void SaveToStream(SerializationInfo info, int orderNumber, int objectIndex)
        {
            info.AddValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryRectangle, orderNumber, objectIndex), rectangle);
            info.AddValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryImage, orderNumber, objectIndex), _image);
            info.AddValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryImageOriginal, orderNumber, objectIndex), _originalImage);
            base.SaveToStream(info, orderNumber, objectIndex);
        }

        public override void LoadFromStream(SerializationInfo info, int orderNumber, int objectIndex)
        {
            rectangle = (SKRect)info.GetValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryRectangle, orderNumber, objectIndex), typeof(SKRect));
            _image = (SKBitmap?)info.GetValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryImage, orderNumber, objectIndex), typeof(SKBitmap));
            _originalImage = (SKBitmap?)info.GetValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryImageOriginal, orderNumber, objectIndex), typeof(SKBitmap));
            base.LoadFromStream(info, orderNumber, objectIndex);
        }

        #region Helper Functions
        public static SKRect GetNormalizedRectangle(float x1, float y1, float x2, float y2)
        {
            if (x2 < x1) { var tmp = x2; x2 = x1; x1 = tmp; }
            if (y2 < y1) { var tmp = y2; y2 = y1; y1 = tmp; }
            return new SKRect(x1, y1, x2, y2);
        }
        public static SKRect GetNormalizedRectangle(SKPoint p1, SKPoint p2) => GetNormalizedRectangle(p1.X, p1.Y, p2.X, p2.Y);
        public static SKRect GetNormalizedRectangle(SKRect r) => GetNormalizedRectangle(r.Left, r.Top, r.Right, r.Bottom);
        #endregion
    }
}
