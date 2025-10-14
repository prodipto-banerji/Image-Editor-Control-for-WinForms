using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;
using System.Windows.Forms;
using SkiaSharp;

namespace CraftSynth.ImageEditor
{
    /// <summary>
    /// Image graphic object migrated to .NET 9 with SkiaSharp
    /// </summary>
    public class DrawImage : DrawObject
    {
        private SKRectI rectangle;
        private SKBitmap? _image;
        // this holds the original image unscaled
        private SKBitmap? _originalImage;
        public bool IsInitialImage;

        public SKBitmap? TheImage
        {
            get { return _image; }
            set
            {
                _originalImage?.Dispose();
                _originalImage = value?.Copy();
                ResizeImage(rectangle.Width, rectangle.Height);
            }
        }

        private const string entryRectangle = "Rect";
        private const string entryImage = "Image";
        private const string entryImageOriginal = "OriginalImage";

        private bool _disposed = false;

        /// <summary>
        /// Clone this instance
        /// </summary>
        public override DrawObject Clone()
        {
            DrawImage drawImage = new DrawImage();
            drawImage._image = _image?.Copy();
            drawImage._originalImage = _originalImage?.Copy();
            drawImage.rectangle = rectangle;
            drawImage.IsInitialImage = IsInitialImage;

            FillDrawObjectFields(drawImage);
            return drawImage;
        }

        protected SKRectI Rectangle
        {
            get { return rectangle; }
            set { rectangle = value; }
        }

        #region Destruction
        protected override void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    // Free any managed objects here. 
                    _originalImage?.Dispose();
                    _image?.Dispose();
                }

                // Free any unmanaged objects here. 
                
                this._disposed = true;
            }
            base.Dispose(disposing);
        }

        ~DrawImage()
        {
             this.Dispose(false);
        }
        #endregion

        public DrawImage()
        {
            SetRectangle(0, 0, 1, 1);
            Initialize();
        }

        public DrawImage(int x, int y, bool isInitialImage)
        {
            rectangle = new SKRectI(x, y, x + 1, y + 1);
            this.IsInitialImage = isInitialImage;
            Initialize();
        }

        public DrawImage(int x, int y, SKBitmap image)
        {
            rectangle = new SKRectI(x, y, x + image.Width, y + image.Height);
            _image = image.Copy();
            _originalImage = image.Copy();
            Center = new System.Drawing.Point(x + (image.Width / 2), y + (image.Height / 2));
            TipText = String.Format("Image Center @ {0}, {1}", Center.X, Center.Y);
            Initialize();
        }

        /// <summary>
        /// Draw image using SkiaSharp
        /// </summary>
        /// <param name="canvas">SKCanvas to draw on</param>
        public void DrawSkia(SKCanvas canvas)
        {
            canvas.Save();

            // Apply rotation if needed
            if (Rotation != 0)
            {
                var centerX = rectangle.Left + (rectangle.Width / 2f);
                var centerY = rectangle.Top + (rectangle.Height / 2f);
                canvas.RotateDegrees(Rotation, centerX, centerY);
            }

            if (_image == null)
            {
                // Draw placeholder rectangle
                using (var paint = new SKPaint())
                {
                    paint.Color = SKColors.Black;
                    paint.Style = SKPaintStyle.Stroke;
                    paint.StrokeWidth = 1;
                    paint.IsAntialias = true;
                    
                    canvas.DrawRect(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height, paint);
                }
            }
            else
            {
                // Draw the image
                var destRect = new SKRect(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
                canvas.DrawBitmap(_image, destRect);
            }

            canvas.Restore();
        }

        /// <summary>
        /// Legacy Draw method for compatibility with base class
        /// In a complete migration, you would change the base class to use SKCanvas
        /// </summary>
        /// <param name="g">Graphics object (legacy)</param>
        public override void Draw(System.Drawing.Graphics g)
        {
            // For now, we need to maintain compatibility with the base class
            // In a real migration, you'd want to change DrawObject.Draw to use SKCanvas
            
            // This is a placeholder implementation
            // You could implement GDI+ to SkiaSharp interop here if needed
            // Or preferably, migrate the entire rendering pipeline to use SkiaSharp
            
            // For demonstration, we'll draw a placeholder rectangle using GDI+
            if (_image == null)
            {
                using (var pen = new System.Drawing.Pen(System.Drawing.Color.Black, 1f))
                {
                    var gdiRect = new System.Drawing.Rectangle(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height);
                    
                    // Apply rotation if needed
                    if (Rotation != 0)
                    {
                        var transform = g.Transform;
                        var matrix = transform.Clone();
                        matrix.RotateAt(Rotation, 
                            new System.Drawing.PointF(rectangle.Left + (rectangle.Width / 2f), rectangle.Top + (rectangle.Height / 2f)), 
                            System.Drawing.Drawing2D.MatrixOrder.Append);
                        g.Transform = matrix;
                        g.DrawRectangle(pen, gdiRect);
                        g.Transform = transform;
                    }
                    else
                    {
                        g.DrawRectangle(pen, gdiRect);
                    }
                }
            }
            else
            {
                // Convert SKBitmap to System.Drawing.Bitmap for legacy compatibility
                using (var gdiImage = SKBitmapToGdiBitmap(_image))
                {
                    var gdiRect = new System.Drawing.Rectangle(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height);
                    
                    if (Rotation != 0)
                    {
                        var transform = g.Transform;
                        var matrix = transform.Clone();
                        matrix.RotateAt(Rotation, 
                            new System.Drawing.PointF(rectangle.Left + (rectangle.Width / 2f), rectangle.Top + (rectangle.Height / 2f)), 
                            System.Drawing.Drawing2D.MatrixOrder.Append);
                        g.Transform = matrix;
                        g.DrawImage(gdiImage, gdiRect);
                        g.Transform = transform;
                    }
                    else
                    {
                        g.DrawImage(gdiImage, gdiRect);
                    }
                }
            }
        }

        protected void SetRectangle(int x, int y, int width, int height)
        {
            rectangle = new SKRectI(x, y, x + width, y + height);
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
            int x, y, xCenter, yCenter;

            xCenter = rectangle.Left + rectangle.Width / 2;
            yCenter = rectangle.Top + rectangle.Height / 2;
            x = rectangle.Left;
            y = rectangle.Top;

            switch (handleNumber)
            {
                case 1:
                    x = rectangle.Left;
                    y = rectangle.Top;
                    break;
                case 2:
                    x = xCenter;
                    y = rectangle.Top;
                    break;
                case 3:
                    x = rectangle.Right;
                    y = rectangle.Top;
                    break;
                case 4:
                    x = rectangle.Right;
                    y = yCenter;
                    break;
                case 5:
                    x = rectangle.Right;
                    y = rectangle.Bottom;
                    break;
                case 6:
                    x = xCenter;
                    y = rectangle.Bottom;
                    break;
                case 7:
                    x = rectangle.Left;
                    y = rectangle.Bottom;
                    break;
                case 8:
                    x = rectangle.Left;
                    y = yCenter;
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
            var skRect = new SKRect(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
            return skRect.Contains(point.X, point.Y);
        }

        public override System.Drawing.Rectangle GetBounds(System.Drawing.Graphics g)
        {
            return new System.Drawing.Rectangle(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height);
        }

        /// <summary>
        /// Get cursor for the handle
        /// </summary>
        /// <param name="handleNumber"></param>
        /// <returns></returns>
        public override Cursor GetHandleCursor(int handleNumber)
        {
            switch (handleNumber)
            {
                case 1:
                    return Cursors.SizeNWSE;
                case 2:
                    return Cursors.SizeNS;
                case 3:
                    return Cursors.SizeNESW;
                case 4:
                    return Cursors.SizeWE;
                case 5:
                    return Cursors.SizeNWSE;
                case 6:
                    return Cursors.SizeNS;
                case 7:
                    return Cursors.SizeNESW;
                case 8:
                    return Cursors.SizeWE;
                default:
                    return Cursors.Default;
            }
        }

        /// <summary>
        /// Move handle to new point (resizing)
        /// </summary>
        /// <param name="point"></param>
        /// <param name="handleNumber"></param>
        public override void MoveHandleTo(System.Drawing.Point point, int handleNumber)
        {
            int left = Rectangle.Left;
            int top = Rectangle.Top;
            int right = Rectangle.Right;
            int bottom = Rectangle.Bottom;

            switch (handleNumber)
            {
                case 1:
                    left = point.X;
                    top = point.Y;
                    break;
                case 2:
                    top = point.Y;
                    break;
                case 3:
                    right = point.X;
                    top = point.Y;
                    break;
                case 4:
                    right = point.X;
                    break;
                case 5:
                    right = point.X;
                    bottom = point.Y;
                    break;
                case 6:
                    bottom = point.Y;
                    break;
                case 7:
                    left = point.X;
                    bottom = point.Y;
                    break;
                case 8:
                    left = point.X;
                    break;
            }
            Dirty = true;
            SetRectangle(left, top, right - left, bottom - top);
            ResizeImage(rectangle.Width, rectangle.Height);
        }

        protected void ResizeImage(int width, int height)
        {
            if (_originalImage != null && width > 0 && height > 0)
            {
                _image?.Dispose();
                
                // Create a new bitmap with the desired size
                _image = new SKBitmap(width, height);
                
                // Draw the original image scaled to the new size
                using (var canvas = new SKCanvas(_image))
                {
                    canvas.Clear(SKColors.Transparent);
                    
                    var destRect = new SKRect(0, 0, width, height);
                    var srcRect = new SKRect(0, 0, _originalImage.Width, _originalImage.Height);
                    
                    using (var paint = new SKPaint())
                    {
                        paint.IsAntialias = true;
                        paint.FilterQuality = SKFilterQuality.High;
                        canvas.DrawBitmap(_originalImage, srcRect, destRect, paint);
                    }
                }
            }
        }

        public override bool IntersectsWith(System.Drawing.Rectangle rectangle)
        {
            var gdiRect = new System.Drawing.Rectangle(this.rectangle.Left, this.rectangle.Top, this.rectangle.Width, this.rectangle.Height);
            return gdiRect.IntersectsWith(rectangle);
        }

        /// <summary>
        /// Move object
        /// </summary>
        /// <param name="deltaX"></param>
        /// <param name="deltaY"></param>
        public override void Move(int deltaX, int deltaY)
        {
            rectangle = new SKRectI(
                rectangle.Left + deltaX, 
                rectangle.Top + deltaY, 
                rectangle.Right + deltaX, 
                rectangle.Bottom + deltaY);
            Dirty = true;
        }

        public override void Dump()
        {
            base.Dump();

            Trace.WriteLine("rectangle.Left = " + rectangle.Left.ToString(CultureInfo.InvariantCulture));
            Trace.WriteLine("rectangle.Top = " + rectangle.Top.ToString(CultureInfo.InvariantCulture));
            Trace.WriteLine("rectangle.Width = " + rectangle.Width.ToString(CultureInfo.InvariantCulture));
            Trace.WriteLine("rectangle.Height = " + rectangle.Height.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Normalize rectangle
        /// </summary>
        public override void Normalize()
        {
            rectangle = GetNormalizedRectangle(rectangle);
        }

        /// <summary>
        /// Save object to serialization stream
        /// </summary>
        /// <param name="info"></param>
        /// <param name="orderNumber"></param>
        /// <param name="objectIndex"></param>
        public override void SaveToStream(SerializationInfo info, int orderNumber, int objectIndex)
        {
            // Convert SKRectI to System.Drawing.Rectangle for serialization compatibility
            var gdiRect = new System.Drawing.Rectangle(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height);
            
            info.AddValue(
                String.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryRectangle, orderNumber, objectIndex),
                gdiRect);

            // Convert SKBitmap to byte array for serialization
            byte[]? imageBytes = null;
            byte[]? originalImageBytes = null;

            if (_image != null)
            {
                imageBytes = SKBitmapToBytes(_image);
            }

            if (_originalImage != null)
            {
                originalImageBytes = SKBitmapToBytes(_originalImage);
            }

            info.AddValue(
                String.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryImage, orderNumber, objectIndex),
                imageBytes);

            info.AddValue(
                String.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryImageOriginal, orderNumber, objectIndex),
                originalImageBytes);

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
            var gdiRect = (System.Drawing.Rectangle)info.GetValue(
                                    String.Format(CultureInfo.InvariantCulture,
                                                  "{0}{1}-{2}",
                                                  entryRectangle, orderNumber, objectIndex),
                                    typeof(System.Drawing.Rectangle))!;

            rectangle = new SKRectI(gdiRect.Left, gdiRect.Top, gdiRect.Right, gdiRect.Bottom);

            var imageBytes = (byte[]?)info.GetValue(
                                String.Format(CultureInfo.InvariantCulture,
                                              "{0}{1}-{2}",
                                              entryImage, orderNumber, objectIndex),
                                typeof(byte[]));

            var originalImageBytes = (byte[]?)info.GetValue(
                                        String.Format(CultureInfo.InvariantCulture,
                                                      "{0}{1}-{2}",
                                                      entryImageOriginal, orderNumber, objectIndex),
                                        typeof(byte[]));

            if (imageBytes != null)
            {
                _image = BytesToSKBitmap(imageBytes);
            }

            if (originalImageBytes != null)
            {
                _originalImage = BytesToSKBitmap(originalImageBytes);
            }

            base.LoadFromStream(info, orderNumber, objectIndex);
        }

        #region Helper Functions
        public static SKRectI GetNormalizedRectangle(int x1, int y1, int x2, int y2)
        {
            if (x2 < x1)
            {
                int tmp = x2;
                x2 = x1;
                x1 = tmp;
            }

            if (y2 < y1)
            {
                int tmp = y2;
                y2 = y1;
                y1 = tmp;
            }
            return new SKRectI(x1, y1, x2, y2);
        }

        public static SKRectI GetNormalizedRectangle(System.Drawing.Point p1, System.Drawing.Point p2)
        {
            return GetNormalizedRectangle(p1.X, p1.Y, p2.X, p2.Y);
        }

        public static SKRectI GetNormalizedRectangle(SKRectI r)
        {
            return GetNormalizedRectangle(r.Left, r.Top, r.Right, r.Bottom);
        }

        /// <summary>
        /// Convert SKBitmap to byte array for serialization
        /// </summary>
        private byte[]? SKBitmapToBytes(SKBitmap bitmap)
        {
            if (bitmap == null) return null;

            using (var image = SKImage.FromBitmap(bitmap))
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            {
                return data.ToArray();
            }
        }

        /// <summary>
        /// Convert byte array to SKBitmap for deserialization
        /// </summary>
        private SKBitmap? BytesToSKBitmap(byte[] bytes)
        {
            if (bytes == null) return null;

            using (var stream = new System.IO.MemoryStream(bytes))
            {
                return SKBitmap.Decode(stream);
            }
        }

        /// <summary>
        /// Convert SKBitmap to System.Drawing.Bitmap for legacy compatibility
        /// This is a temporary helper for the transition period
        /// </summary>
        private System.Drawing.Bitmap? SKBitmapToGdiBitmap(SKBitmap skBitmap)
        {
            if (skBitmap == null) return null;

            // Convert SKBitmap to byte array
            byte[] bytes = SKBitmapToBytes(skBitmap)!;
            
            // Create GDI+ bitmap from byte array
            using (var stream = new System.IO.MemoryStream(bytes))
            {
                return new System.Drawing.Bitmap(stream);
            }
        }

        /// <summary>
        /// Convert System.Drawing.Bitmap to SKBitmap
        /// </summary>
        public static SKBitmap? GdiBitmapToSKBitmap(System.Drawing.Bitmap gdiBitmap)
        {
            if (gdiBitmap == null) return null;

            using (var stream = new System.IO.MemoryStream())
            {
                gdiBitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Position = 0;
                return SKBitmap.Decode(stream);
            }
        }

        /// <summary>
        /// Create an SKBitmap from a file path
        /// </summary>
        public static SKBitmap? LoadFromFile(string filePath)
        {
            try
            {
                return SKBitmap.Decode(filePath);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Create an SKBitmap from a stream
        /// </summary>
        public static SKBitmap? LoadFromStream(System.IO.Stream stream)
        {
            try
            {
                return SKBitmap.Decode(stream);
            }
            catch
            {
                return null;
            }
        }
        #endregion Helper Functions
    }
}