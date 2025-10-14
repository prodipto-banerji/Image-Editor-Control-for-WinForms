using System;
using System.Globalization;
using System.Runtime.Serialization;
using SkiaSharp;

namespace CraftSynth.ImageEditor
{
    /// <summary>
    /// Rectangle graphic object (SkiaSharp)
    /// </summary>
    [Serializable]
    public class DrawRectangle : DrawObject
    {
        private SKRect rectangle;
        private const string entryRectangle = "Rect";

        protected SKRect Rectangle
        {
            get => rectangle;
            set => rectangle = value;
        }

        public override DrawObject Clone()
        {
            var d = new DrawRectangle { rectangle = rectangle };
            FillDrawObjectFields(d);
            return d;
        }

        public DrawRectangle()
        {
            SetRectangle(0, 0, 1, 1);
        }

        public DrawRectangle(float x, float y, float width, float height, SKColor lineColor, SKColor fillColor, bool filled, float lineWidth, DrawingPens.PenType penType, SKStrokeCap endCap)
        {
            rectangle = new SKRect(x, y, x + width, y + height);
            Center = new SKPoint(x + (width / 2f), y + (height / 2f));
            TipText = string.Format(CultureInfo.InvariantCulture, "Rectangle Center @ {0}, {1}", Center.X, Center.Y);
            Color = lineColor;
            FillColor = fillColor;
            Filled = filled;
            PenWidth = lineWidth;
            EndCap = endCap;
            PenType = penType;
        }

        public override void Draw(SKCanvas canvas)
        {
            using var stroke = new SKPaint { Color = Color, StrokeWidth = PenWidth, Style = SKPaintStyle.Stroke, IsAntialias = true, StrokeCap = EndCap };
            DrawingPens.ConfigurePaint(stroke, PenType);
            using var fill = new SKPaint { Color = FillColor, Style = SKPaintStyle.Fill, IsAntialias = true };

            var r = NormalizeRect(Rectangle);
            if (Rotation != 0)
            {
                var cx = r.MidX; var cy = r.MidY;
                canvas.Save();
                canvas.RotateDegrees(Rotation, cx, cy);
                if (Filled) canvas.DrawRect(r, fill);
                canvas.DrawRect(r, stroke);
                canvas.Restore();
            }
            else
            {
                if (Filled) canvas.DrawRect(r, fill);
                canvas.DrawRect(r, stroke);
            }
        }

        protected void SetRectangle(float x, float y, float width, float height)
        {
            rectangle = new SKRect(x, y, x + width, y + height);
        }

        public override int HandleCount => 8;
        public override int ConnectionCount => HandleCount;

        public override SKPoint GetConnection(int connectionNumber) => GetHandle(connectionNumber);

        public override SKPoint GetHandle(int handleNumber)
        {
            var r = Rectangle;
            float xCenter = r.MidX;
            float yCenter = r.MidY;
            float x = r.Left;
            float y = r.Top;
            switch (handleNumber)
            {
                case 1: x = r.Left; y = r.Top; break;
                case 2: x = xCenter; y = r.Top; break;
                case 3: x = r.Right; y = r.Top; break;
                case 4: x = r.Right; y = yCenter; break;
                case 5: x = r.Right; y = r.Bottom; break;
                case 6: x = xCenter; y = r.Bottom; break;
                case 7: x = r.Left; y = r.Bottom; break;
                case 8: x = r.Left; y = yCenter; break;
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
            return NormalizeRect(Rectangle).Contains(point.X, point.Y);
        }

        public override SKRect GetBounds()
        {
            return NormalizeRect(Rectangle);
        }

        public override bool IntersectsWith(SKRect rect)
        {
            var r = NormalizeRect(Rectangle);
            return r.IntersectsWith(rect);
        }

        public override void MoveHandleTo(SKPoint point, int handleNumber)
        {
            float left = Rectangle.Left;
            float top = Rectangle.Top;
            float right = Rectangle.Right;
            float bottom = Rectangle.Bottom;

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
        }

        public override void Move(float deltaX, float deltaY)
        {
            var r = Rectangle;
            r.Offset(deltaX, deltaY);
            Rectangle = r;
            Dirty = true;
        }

        public override void SaveToStream(SerializationInfo info, int orderNumber, int objectIndex)
        {
            info.AddValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryRectangle, orderNumber, objectIndex), rectangle);
            base.SaveToStream(info, orderNumber, objectIndex);
        }

        public override void LoadFromStream(SerializationInfo info, int orderNumber, int objectIndex)
        {
            rectangle = (SKRect)info.GetValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryRectangle, orderNumber, objectIndex), typeof(SKRect));
            base.LoadFromStream(info, orderNumber, objectIndex);
        }
    }
}
