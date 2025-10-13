using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace CraftSynth.ImageEditor
{
    /// <summary>
    /// Image graphic object (NET9 version using ImageSharp for image manipulation)
    /// </summary>
    public class DrawImage : DrawObject
    {
        public Rectangle rectangle;
        private Bitmap _image;
        private Bitmap _originalImage;
        public bool IsInitialImage;

        public Bitmap TheImage
        {
            get => _image;
            set
            {
                // Keep original reference for future resizing
                _originalImage = value;
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
            var drawImage = new DrawImage
            {
                _image = _image,
                _originalImage = _originalImage,
                rectangle = rectangle,
                IsInitialImage = IsInitialImage
            };

            FillDrawObjectFields(drawImage);
            return drawImage;
        }

        protected Rectangle Rectangle
        {
            get { return rectangle; }
            set { rectangle = value; }
        }

        #region Destruction
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

        ~DrawImage()
        {
            Dispose(false);
        }
        #endregion

        public DrawImage()
        {
            SetRectangle(0, 0, 1, 1);
            Initialize();
        }

        public DrawImage(int x, int y, bool isInitialImage)
        {
            rectangle.X = x;
            rectangle.Y = y;
            rectangle.Width = 1;
            rectangle.Height = 1;
            IsInitialImage = isInitialImage;
            Initialize();
        }

        public DrawImage(int x, int y, Bitmap image)
        {
            rectangle.X = x;
            rectangle.Y = y;
            _image = (Bitmap)image.Clone();
            SetRectangle(rectangle.X, rectangle.Y, image.Width, image.Height);
            Center = new Point(x + (image.Width / 2), y + (image.Height / 2));
            TipText = string.Format(CultureInfo.InvariantCulture, "Image Center @ {0}, {1}", Center.X, Center.Y);
            Initialize();
        }

        /// <summary>
        /// Draw image
        /// </summary>
        public override void Draw(Graphics g)
        {
            // Get existing World transformation
            Matrix mSave = g.Transform;
            if (Rotation != 0)
            {
                Matrix m = mSave.Clone();
                m.RotateAt(Rotation, new PointF(rectangle.Left + (rectangle.Width / 2), rectangle.Top + (rectangle.Height / 2)), MatrixOrder.Append);
                g.Transform = m;
            }
            if (_image == null)
            {
                using Pen p = new Pen(Color.Black, -1f);
                g.DrawRectangle(p, rectangle);
            }
            else
            {
                g.DrawImage(_image, new Point(rectangle.X, rectangle.Y));
            }
            // Restore World transformation
            g.Transform = mSave;
        }

        protected void SetRectangle(int x, int y, int width, int height)
        {
            rectangle.X = x;
            rectangle.Y = y;
            rectangle.Width = width;
            rectangle.Height = height;
        }

        public override int HandleCount => 8;

        public override Point GetHandle(int handleNumber)
        {
            int x, y, xCenter, yCenter;

            xCenter = rectangle.X + rectangle.Width / 2;
            yCenter = rectangle.Y + rectangle.Height / 2;
            x = rectangle.X;
            y = rectangle.Y;

            switch (handleNumber)
            {
                case 1:
                    x = rectangle.X;
                    y = rectangle.Y;
                    break;
                case 2:
                    x = xCenter;
                    y = rectangle.Y;
                    break;
                case 3:
                    x = rectangle.Right;
                    y = rectangle.Y;
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
                    x = rectangle.X;
                    y = rectangle.Bottom;
                    break;
                case 8:
                    x = rectangle.X;
                    y = yCenter;
                    break;
            }
            return new Point(x, y);
        }

        public override int HitTest(Point point)
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

        protected override bool PointInObject(Point point)
        {
            return rectangle.Contains(point);
        }

        public override Rectangle GetBounds(Graphics g)
        {
            return rectangle;
        }

        public override Cursor GetHandleCursor(int handleNumber)
        {
            return handleNumber switch
            {
                1 => Cursors.SizeNWSE,
                2 => Cursors.SizeNS,
                3 => Cursors.SizeNESW,
                4 => Cursors.SizeWE,
                5 => Cursors.SizeNWSE,
                6 => Cursors.SizeNS,
                7 => Cursors.SizeNESW,
                8 => Cursors.SizeWE,
                _ => Cursors.Default,
            };
        }

        public override void MoveHandleTo(Point point, int handleNumber)
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
            if (_originalImage != null)
            {
                // Use ImageSharp for resizing for better quality and cross-platform safety
                var resized = ImageSharpHelper.ResizeBitmap(_originalImage, width, height);
                _image?.Dispose();
                _image = resized;
            }
        }

        public override bool IntersectsWith(Rectangle rectangle)
        {
            return Rectangle.IntersectsWith(rectangle);
        }

        public override void Move(int deltaX, int deltaY)
        {
            rectangle.X += deltaX;
            rectangle.Y += deltaY;
            Dirty = true;
        }

        public override void Dump()
        {
            base.Dump();
            Trace.WriteLine("rectangle.X = " + rectangle.X.ToString(CultureInfo.InvariantCulture));
            Trace.WriteLine("rectangle.Y = " + rectangle.Y.ToString(CultureInfo.InvariantCulture));
            Trace.WriteLine("rectangle.Width = " + rectangle.Width.ToString(CultureInfo.InvariantCulture));
            Trace.WriteLine("rectangle.Height = " + rectangle.Height.ToString(CultureInfo.InvariantCulture));
        }

        public override void Normalize()
        {
            rectangle = DrawRectangle.GetNormalizedRectangle(rectangle);
        }

        public override void SaveToStream(SerializationInfo info, int orderNumber, int objectIndex)
        {
            info.AddValue(
                string.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryRectangle, orderNumber, objectIndex),
                rectangle);
            info.AddValue(
                string.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryImage, orderNumber, objectIndex),
                _image);
            info.AddValue(
                string.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryImageOriginal, orderNumber, objectIndex),
                _originalImage);

            base.SaveToStream(info, orderNumber, objectIndex);
        }

        public override void LoadFromStream(SerializationInfo info, int orderNumber, int objectIndex)
        {
            rectangle = (Rectangle)info.GetValue(
                string.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryRectangle, orderNumber, objectIndex),
                typeof(Rectangle));
            _image = (Bitmap)info.GetValue(
                string.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryImage, orderNumber, objectIndex),
                typeof(Bitmap));
            _originalImage = (Bitmap)info.GetValue(
                string.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryImageOriginal, orderNumber, objectIndex),
                typeof(Bitmap));

            base.LoadFromStream(info, orderNumber, objectIndex);
        }

        #region Helper Functions
        public static Rectangle GetNormalizedRectangle(int x1, int y1, int x2, int y2)
        {
            if (x2 < x1)
            {
                int tmp = x2; x2 = x1; x1 = tmp;
            }
            if (y2 < y1)
            {
                int tmp = y2; y2 = y1; y1 = tmp;
            }
            return new Rectangle(x1, y1, x2 - x1, y2 - y1);
        }

        public static Rectangle GetNormalizedRectangle(Point p1, Point p2)
        {
            return GetNormalizedRectangle(p1.X, p1.Y, p2.X, p2.Y);
        }

        public static Rectangle GetNormalizedRectangle(Rectangle r)
        {
            return GetNormalizedRectangle(r.X, r.Y, r.X + r.Width, r.Y + r.Height);
        }
        #endregion Helper Functions
    }
}
