using System;
using System.Globalization;
using System.Runtime.Serialization;
using SkiaSharp;

namespace CraftSynth.ImageEditor
{
    /// <summary>
    /// Line graphic object (SkiaSharp)
    /// </summary>
    public class DrawLine : DrawObject
    {
        private SKPoint startPoint;
        private SKPoint endPoint;

        private const string entryStart = "Start";
        private const string entryEnd = "End";

        // Hit-test geometry
        private SKPath? areaPath;
        private SKPaint? areaPaint;
        private SKRegion? areaRegion;

        private bool _disposed;

        public DrawLine()
        {
            startPoint = new SKPoint(0, 0);
            endPoint = new SKPoint(1, 1);
            ZOrder = 0;
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    areaPath?.Dispose();
                    areaPaint?.Dispose();
                    areaRegion?.Dispose();
                }
                _disposed = true;
            }
            base.Dispose(disposing);
        }

        ~DrawLine()
        {
            Dispose(false);
        }

        public DrawLine(float x1, float y1, float x2, float y2, SKColor lineColor, float lineWidth, DrawingPens.PenType penType, SKStrokeCap endCap)
        {
            startPoint = new SKPoint(x1, y1);
            endPoint = new SKPoint(x2, y2);
            Color = lineColor;
            PenWidth = lineWidth;
            PenType = penType;
            EndCap = endCap;
            ZOrder = 0;
            TipText = string.Format(CultureInfo.InvariantCulture, "Line Start @ {0}-{1}, End @ {2}-{3}", x1, y1, x2, y2);
        }

        public override void Draw(SKCanvas canvas)
        {
            using var paint = new SKPaint
            {
                IsAntialias = true,
                Color = Color,
                StrokeWidth = PenWidth,
                Style = SKPaintStyle.Stroke,
                StrokeCap = EndCap
            };
            DrawingPens.ConfigurePaint(paint, PenType);

            using var path = new SKPath();
            path.MoveTo(startPoint);
            path.LineTo(endPoint);

            if (Rotation != 0)
            {
                var bounds = path.Bounds;
                var cx = bounds.MidX;
                var cy = bounds.MidY;
                canvas.Save();
                canvas.RotateDegrees(Rotation, cx, cy);
                canvas.DrawPath(path, paint);
                canvas.Restore();
            }
            else
            {
                canvas.DrawPath(path, paint);
            }
        }

        public override DrawObject Clone()
        {
            var d = new DrawLine
            {
                startPoint = startPoint,
                endPoint = endPoint
            };
            FillDrawObjectFields(d);
            return d;
        }

        public override int HandleCount => 2;

        public override SKPoint GetHandle(int handleNumber)
        {
            // account for rotation
            using var path = new SKPath();
            path.MoveTo(startPoint);
            path.LineTo(endPoint);
            var b = path.Bounds;
            var cx = b.MidX;
            var cy = b.MidY;

            SKPoint s = startPoint;
            SKPoint e = endPoint;
            if (Rotation != 0)
            {
                s = RotatePoint(startPoint, cx, cy, Rotation);
                e = RotatePoint(endPoint, cx, cy, Rotation);
            }
            return handleNumber == 1 ? s : e;
        }

        public override int HitTest(SKPoint point)
        {
            if (Selected)
            {
                for (int i = 1; i <= HandleCount; i++)
                {
                    var r = GetHandleRectangle(i);
                    if (r.Contains(point))
                        return i;
                }
            }
            return PointInObject(point) ? 0 : -1;
        }

        protected override bool PointInObject(SKPoint point)
        {
            CreateObjects();
            return areaRegion?.Contains((int)point.X, (int)point.Y) ?? false;
        }

        public override SKRect GetBounds()
        {
            CreateObjects();
            if (areaRegion == null)
                return SKRect.Empty;
            var b = areaRegion.Bounds;
            return new SKRect(b.Left, b.Top, b.Right, b.Bottom);
        }

        public override bool IntersectsWith(SKRect rect)
        {
            CreateObjects();
            if (areaRegion == null) return false;
            var r = new SKRectI((int)rect.Left, (int)rect.Top, (int)rect.Right, (int)rect.Bottom);
            return areaRegion.Intersects(r);
        }

        public override void MoveHandleTo(SKPoint point, int handleNumber)
        {
            if (handleNumber == 1)
                startPoint = point;
            else
                endPoint = point;
            Dirty = true;
            Invalidate();
        }

        public override void Move(float dx, float dy)
        {
            startPoint = new SKPoint(startPoint.X + dx, startPoint.Y + dy);
            endPoint = new SKPoint(endPoint.X + dx, endPoint.Y + dy);
            Dirty = true;
            Invalidate();
        }

        public override void SaveToStream(SerializationInfo info, int orderNumber, int objectIndex)
        {
            info.AddValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryStart, orderNumber, objectIndex), new SKPointI((int)startPoint.X, (int)startPoint.Y));
            info.AddValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryEnd, orderNumber, objectIndex), new SKPointI((int)endPoint.X, (int)endPoint.Y));
            base.SaveToStream(info, orderNumber, objectIndex);
        }

        public override void LoadFromStream(SerializationInfo info, int orderNumber, int objectIndex)
        {
            var s = (SKPointI)info.GetValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryStart, orderNumber, objectIndex), typeof(SKPointI));
            var e = (SKPointI)info.GetValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryEnd, orderNumber, objectIndex), typeof(SKPointI));
            startPoint = new SKPoint(s.X, s.Y);
            endPoint = new SKPoint(e.X, e.Y);
            base.LoadFromStream(info, orderNumber, objectIndex);
        }

        private void Invalidate()
        {
            if (areaPath != null)
            {
                areaPath.Dispose();
                areaPath = null;
            }
            if (areaPaint != null)
            {
                areaPaint.Dispose();
                areaPaint = null;
            }
            if (areaRegion != null)
            {
                areaRegion.Dispose();
                areaRegion = null;
            }
        }

        protected virtual void CreateObjects()
        {
            if (areaPath != null) return;

            areaPath = new SKPath();
            float strokeWidth = PenWidth < 7 ? 7 : PenWidth;
            areaPaint = new SKPaint
            {
                Color = SKColors.Black,
                StrokeWidth = strokeWidth,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            };

            var endAdj = endPoint;
            if (startPoint.Equals(endPoint))
            {
                endAdj = new SKPoint(endPoint.X + 1, endPoint.Y + 1);
            }

            areaPath.MoveTo(startPoint);
            areaPath.LineTo(endAdj);

            using var stroked = areaPaint.GetFillPath(areaPath);
            if (stroked != null)
            {
                areaPath.Dispose();
                areaPath = new SKPath(stroked);
            }

            if (Rotation != 0)
            {
                var b = areaPath.Bounds;
                var cx = b.MidX;
                var cy = b.MidY;
                var m = SKMatrix.CreateRotationDegrees(Rotation, cx, cy);
                areaPath.Transform(in m);
            }

            areaRegion = new SKRegion();
            var pb = areaPath.Bounds;
            var clip = new SKRectI((int)Math.Floor(pb.Left), (int)Math.Floor(pb.Top), (int)Math.Ceiling(pb.Right), (int)Math.Ceiling(pb.Bottom));
            areaRegion.SetPath(areaPath, new SKRegion(clip));
        }

        private static SKPoint RotatePoint(SKPoint p, float cx, float cy, float angleDeg)
        {
            var rad = angleDeg * (float)Math.PI / 180f;
            var cos = (float)Math.Cos(rad);
            var sin = (float)Math.Sin(rad);
            var dx = p.X - cx;
            var dy = p.Y - cy;
            return new SKPoint(cx + (dx * cos - dy * sin), cy + (dx * sin + dy * cos));
        }
    }
}
