using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;
using SkiaSharp;

namespace CraftSynth.ImageEditor
{
    /// <summary>
    /// Base class for all draw objects using SkiaSharp.
    /// </summary>
    [Serializable]
    public abstract class DrawObject : IComparable, IDisposable
    {
        // Object properties
        private bool selected;
        private SKColor color = SKColors.Black;
        private SKColor fillColor = SKColors.White;
        private bool filled;
        private float penWidth = 1f;
        private DrawingPens.PenType _penType = DrawingPens.PenType.Solid;
        private SKStrokeCap _endCap = SKStrokeCap.Round;
        private FillBrushes.BrushType _brushType = FillBrushes.BrushType.NoBrush;
        private string tipText = string.Empty;

        // Last used property values
        private static SKColor lastUsedColor = SKColors.Black;
        private static int lastUsedPenWidth = 1;

        private bool dirty;
        private int _id;
        private int _zOrder;
        private int _rotation = 0; // degrees
        private SKPoint _center;

        private bool _disposed;

        // Serialization entry names
        private const string entryColor = "Color";
        private const string entryPenWidth = "PenWidth";
        private const string entryPen = "DrawPen";
        private const string entryBrush = "DrawBrush";
        private const string entryFillColor = "FillColor";
        private const string entryFilled = "Filled";
        private const string entryZOrder = "ZOrder";
        private const string entryRotation = "Rotation";
        private const string entryTipText = "TipText";

        protected DrawObject()
        {
            ID = GetHashCode();
        }

        public SKPoint Center
        {
            get => _center;
            set => _center = value;
        }

        /// <summary>
        /// Rotation of the object in degrees. Negative is Left, Positive is Right.
        /// </summary>
        public int Rotation
        {
            get => _rotation;
            set
            {
                if (value > 360)
                    _rotation = value - 360;
                else if (value < -360)
                    _rotation = value + 360;
                else
                    _rotation = value;
            }
        }

        public int ZOrder
        {
            get => _zOrder;
            set => _zOrder = value;
        }

        public int ID
        {
            get => _id;
            set => _id = value;
        }

        public bool Dirty
        {
            get => dirty;
            set => dirty = value;
        }

        public bool Filled
        {
            get => filled;
            set => filled = value;
        }

        public bool Selected
        {
            get => selected;
            set => selected = value;
        }

        public SKColor FillColor
        {
            get => fillColor;
            set => fillColor = value;
        }

        public SKColor Color
        {
            get => color;
            set => color = value;
        }

        public float PenWidth
        {
            get => penWidth;
            set => penWidth = value;
        }

        public FillBrushes.BrushType BrushType
        {
            get => _brushType;
            set => _brushType = value;
        }

        public DrawingPens.PenType PenType
        {
            get => _penType;
            set => _penType = value;
        }

        public SKStrokeCap EndCap
        {
            get => _endCap;
            set => _endCap = value;
        }

        public virtual int HandleCount => 0;
        public virtual int ConnectionCount => 0;

        public static SKColor LastUsedColor
        {
            get => lastUsedColor;
            set => lastUsedColor = value;
        }

        public static int LastUsedPenWidth
        {
            get => lastUsedPenWidth;
            set => lastUsedPenWidth = value;
        }

        public string TipText
        {
            get => tipText;
            set => tipText = value ?? string.Empty;
        }

        public abstract DrawObject Clone();

        /// <summary>
        /// Draw this object using SkiaSharp.
        /// </summary>
        public virtual void Draw(SKCanvas canvas) { }

        /// <summary>
        /// Draw selection handles if selected.
        /// </summary>
        public virtual void DrawTracker(SKCanvas canvas)
        {
            if (!Selected)
                return;
            using var paint = new SKPaint
            {
                Color = SKColors.Black,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };
            for (int i = 1; i <= HandleCount; i++)
            {
                var r = GetHandleRectangle(i);
                canvas.DrawRect(r, paint);
            }
        }

        /// <summary>
        /// Get handle point by 1-based number.
        /// </summary>
        public virtual SKPoint GetHandle(int handleNumber) => new SKPoint(0, 0);

        /// <summary>
        /// Get handle rectangle by 1-based number.
        /// </summary>
        public virtual SKRect GetHandleRectangle(int handleNumber)
        {
            var point = GetHandle(handleNumber);
            float size = 7 + PenWidth;
            return new SKRect(point.X - (PenWidth + 3), point.Y - (PenWidth + 3), point.X - (PenWidth + 3) + size, point.Y - (PenWidth + 3) + size);
        }

        public virtual void DrawConnections(SKCanvas canvas)
        {
            if (!Selected) return;
            using var stroke = new SKPaint { Color = SKColors.Black, Style = SKPaintStyle.Stroke, StrokeWidth = 1, IsAntialias = true };
            using var fill = new SKPaint { Color = SKColors.White, Style = SKPaintStyle.Fill, IsAntialias = true };
            for (int i = 0; i < ConnectionCount; i++)
            {
                var r = GetConnectionEllipse(i);
                canvas.DrawOval(r, stroke);
                canvas.DrawOval(r, fill);
            }
        }

        public virtual SKPoint GetConnection(int connectionNumber) => new SKPoint(0, 0);

        public virtual SKRect GetConnectionEllipse(int connectionNumber)
        {
            var p = GetConnection(connectionNumber);
            float size = 7 + PenWidth;
            return new SKRect(p.X - (PenWidth + 3), p.Y - (PenWidth + 3), p.X - (PenWidth + 3) + size, p.Y - (PenWidth + 3) + size);
        }

        /// <summary>
        /// Hit test to determine if object is hit.
        /// Return (-1) no hit, (0) hit anywhere, (>0) handle number.
        /// </summary>
        public virtual int HitTest(SKPoint point) => -1;

        protected virtual bool PointInObject(SKPoint point) => false;

        public abstract SKRect GetBounds();

        /// <summary>
        /// Test whether object intersects with rectangle.
        /// </summary>
        public virtual bool IntersectsWith(SKRect rect) => false;

        /// <summary>
        /// Move object.
        /// </summary>
        public virtual void Move(float deltaX, float deltaY) { }

        /// <summary>
        /// Move handle to specified point.
        /// </summary>
        public virtual void MoveHandleTo(SKPoint point, int handleNumber) { }

        public virtual void Dump()
        {
            Trace.WriteLine("");
            Trace.WriteLine(GetType().Name);
            Trace.WriteLine("Selected = " + selected.ToString(CultureInfo.InvariantCulture));
        }

        public virtual void Normalize() { }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                // no managed resources currently
                _disposed = true;
            }
        }

        ~DrawObject()
        {
            Dispose(false);
        }

        #region Save / Load methods
        public virtual void SaveToStream(SerializationInfo info, int orderNumber, int objectIndex)
        {
            info.AddValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryColor, orderNumber, objectIndex), Color.ToString());
            info.AddValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryPenWidth, orderNumber, objectIndex), PenWidth);
            info.AddValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryPen, orderNumber, objectIndex), PenType);
            info.AddValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryBrush, orderNumber, objectIndex), BrushType);
            info.AddValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryFillColor, orderNumber, objectIndex), FillColor.ToString());
            info.AddValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryFilled, orderNumber, objectIndex), Filled);
            info.AddValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryZOrder, orderNumber, objectIndex), ZOrder);
            info.AddValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryRotation, orderNumber, objectIndex), Rotation);
            info.AddValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryTipText, orderNumber, objectIndex), tipText);
        }

        public virtual void LoadFromStream(SerializationInfo info, int orderNumber, int objectData)
        {
            var colorStr = info.GetString(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryColor, orderNumber, objectData));
            Color = SKColor.Parse(colorStr);
            PenWidth = info.GetSingle(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryPenWidth, orderNumber, objectData));
            PenType = (DrawingPens.PenType)info.GetValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryPen, orderNumber, objectData), typeof(DrawingPens.PenType));
            BrushType = (FillBrushes.BrushType)info.GetValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryBrush, orderNumber, objectData), typeof(FillBrushes.BrushType));
            var fillStr = info.GetString(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryFillColor, orderNumber, objectData));
            FillColor = SKColor.Parse(fillStr);
            Filled = info.GetBoolean(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryFilled, orderNumber, objectData));
            ZOrder = info.GetInt32(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryZOrder, orderNumber, objectData));
            Rotation = info.GetInt32(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryRotation, orderNumber, objectData));
            tipText = info.GetString(string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", entryTipText, orderNumber, objectData));
        }
        #endregion

        #region Helpers
        protected static SKRect NormalizeRect(SKRect r)
        {
            var left = Math.Min(r.Left, r.Right);
            var right = Math.Max(r.Left, r.Right);
            var top = Math.Min(r.Top, r.Bottom);
            var bottom = Math.Max(r.Top, r.Bottom);
            return new SKRect(left, top, right, bottom);
        }

        protected static SKRect NormalizeRect(float x1, float y1, float x2, float y2)
        {
            return NormalizeRect(new SKRect(x1, y1, x2, y2));
        }
        #endregion
    }
}
