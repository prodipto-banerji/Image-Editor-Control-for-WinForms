using System;
using System.Collections;
using System.Globalization;
using System.Runtime.Serialization;
using SkiaSharp;

namespace CraftSynth.ImageEditor
{
    /// <summary>
    /// Polygon graphic object (SkiaSharp)
    /// </summary>
    public class DrawPolygon : DrawLine
    {
        private ArrayList pointArray = new ArrayList();
        private const string entryLength = "Length";
        private const string entryPoint = "Point";
        private bool _disposed;

        public DrawPolygon()
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
            }
            base.Dispose(disposing);
        }

        ~DrawPolygon() { Dispose(false); }

        public DrawPolygon(float x1, float y1, float x2, float y2, SKColor lineColor, float lineWidth, DrawingPens.PenType penType, SKStrokeCap endCap)
        {
            pointArray.Add(new SKPoint(x1, y1));
            pointArray.Add(new SKPoint(x2, y2));
            Color = lineColor;
            PenWidth = lineWidth;
            PenType = penType;
            EndCap = endCap;
        }

        public override DrawObject Clone()
        {
            var d = new DrawPolygon();
            d.pointArray = (ArrayList)pointArray.Clone();
            FillDrawObjectFields(d);
            return d;
        }

        public void AddPoint(SKPoint p) => pointArray.Add(p);

        public override void Draw(SKCanvas canvas)
        {
            using var stroke = new SKPaint { Color = Color, StrokeWidth = PenWidth, Style = SKPaintStyle.Stroke, IsAntialias = true, StrokeCap = EndCap };
            DrawingPens.ConfigurePaint(stroke, PenType);
            using var path = new SKPath();
            if (pointArray.Count > 0)
            {
                var p0 = (SKPoint)pointArray[0];
                path.MoveTo(p0);
                for (int i = 1; i < pointArray.Count; i++) path.LineTo((SKPoint)pointArray[i]);
                path.Close();
            }
            if (Rotation != 0)
            {
                var b = path.Bounds; var cx = b.MidX; var cy = b.MidY;
                canvas.Save(); canvas.RotateDegrees(Rotation, cx, cy); canvas.DrawPath(path, stroke); canvas.Restore();
            }
            else
            {
                canvas.DrawPath(path, stroke);
            }
        }

        public override int HandleCount => pointArray.Count;

        public override SKPoint GetHandle(int handleNumber)
        {
            if (handleNumber < 1) handleNumber = 1;
            if (handleNumber > pointArray.Count) handleNumber = pointArray.Count;
            return (SKPoint)pointArray[handleNumber - 1];
        }

        public override void MoveHandleTo(SKPoint point, int handleNumber)
        {
            if (handleNumber < 1) handleNumber = 1;
            if (handleNumber > pointArray.Count) handleNumber = pointArray.Count;
            pointArray[handleNumber - 1] = point;
            Dirty = true;
        }

        public override void Move(float dx, float dy)
        {
            for (int i = 0; i < pointArray.Count; i++)
            {
                var p = (SKPoint)pointArray[i];
                pointArray[i] = new SKPoint(p.X + dx, p.Y + dy);
            }
            Dirty = true;
        }

        public override void SaveToStream(SerializationInfo info, int orderNumber, int objectIndex)
        {
            info.AddValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryLength, orderNumber, objectIndex), pointArray.Count);
            int i = 0;
            foreach (SKPoint p in pointArray)
            {
                info.AddValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}-{3}", entryPoint, orderNumber, objectIndex, i++), p);
            }
            base.SaveToStream(info, orderNumber, objectIndex);
        }

        public override void LoadFromStream(SerializationInfo info, int orderNumber, int objectIndex)
        {
            int n = info.GetInt32(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryLength, orderNumber, objectIndex));
            pointArray.Clear();
            for (int i = 0; i < n; i++)
            {
                var p = (SKPoint)info.GetValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}-{3}", entryPoint, orderNumber, objectIndex, i), typeof(SKPoint));
                pointArray.Add(p);
            }
            base.LoadFromStream(info, orderNumber, objectIndex);
        }

        protected override bool PointInObject(SKPoint point)
        {
            using var path = new SKPath();
            if (pointArray.Count > 0)
            {
                var p0 = (SKPoint)pointArray[0];
                path.MoveTo(p0);
                for (int i = 1; i < pointArray.Count; i++) path.LineTo((SKPoint)pointArray[i]);
                path.Close();
            }
            return path.Contains(point.X, point.Y);
        }

        public override SKRect GetBounds() => new SKPath(pointArray.Count > 0 ? (SKPoint[])pointArray.ToArray(typeof(SKPoint)) : Array.Empty<SKPoint>()).Bounds;
        public override bool IntersectsWith(SKRect rect) => GetBounds().IntersectsWith(rect);
    }
}
