using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;
using System.Windows.Forms;
using SkiaSharp;

namespace CraftSynth.ImageEditor
{
    /// <summary>
    /// Base class for all draw objects
    /// </summary>
    [Serializable]
    public abstract class DrawObject : IComparable, IDisposable
    {
        #region Members
        // Object properties
        private bool selected;
        private System.Drawing.Color color;
        private System.Drawing.Color fillColor;
        private bool filled;
        private int penWidth;
        private SKPaint? drawPaint;
        private SKPaint? drawBrush;
        private DrawingPens.PenType _penType;
        private System.Drawing.Drawing2D.LineCap _endCap;
        private FillBrushes.BrushType _brushType;
        private string? tipText;

        // Last used property values (may be kept in the Registry)
        private static System.Drawing.Color lastUsedColor = System.Drawing.Color.Black;
        private static int lastUsedPenWidth = 1;

        // Entry names for serialization
        private const string entryColor = "Color";
        private const string entryPenWidth = "PenWidth";
        private const string entryPen = "DrawPen";
        private const string entryBrush = "DrawBrush";
        private const string entryFillColor = "FillColor";
        private const string entryFilled = "Filled";
        private const string entryZOrder = "ZOrder";
        private const string entryRotation = "Rotation";
        private const string entryTipText = "TipText";

        private bool dirty;
        private int _id;
        private int _zOrder;
        private int _rotation = 0;
        private System.Drawing.Point _center;

        private bool _disposed;
        #endregion Members

        #region Properties
        /// <summary>
        /// Center of the object being drawn.
        /// </summary>
        public System.Drawing.Point Center
        {
            get { return _center; }
            set { _center = value; }
        }

        /// <summary>
        /// Rotation of the object in degrees. Negative is Left, Positive is Right.
        /// </summary>
        public int Rotation
        {
            get { return _rotation; }
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

        /// <summary>
        /// ZOrder is the order the objects will be drawn in - lower the ZOrder, the closer the to top the object is.
        /// </summary>
        public int ZOrder
        {
            get { return _zOrder; }
            set { _zOrder = value; }
        }

        /// <summary>
        /// Object ID used for Undo Redo functions
        /// </summary>
        public int ID
        {
            get { return _id; }
            set { _id = value; }
        }

        /// <summary>
        /// Set to true whenever the object changes
        /// </summary>
        public bool Dirty
        {
            get { return dirty; }
            set { dirty = value; }
        }

        /// <summary>
        /// Draw object filled?
        /// </summary>
        public bool Filled
        {
            get { return filled; }
            set { filled = value; }
        }

        /// <summary>
        /// Selection flag
        /// </summary>
        public bool Selected
        {
            get { return selected; }
            set { selected = value; }
        }

        /// <summary>
        /// Fill Color
        /// </summary>
        public System.Drawing.Color FillColor
        {
            get { return fillColor; }
            set { fillColor = value; }
        }

        /// <summary>
        /// Border (line) Color
        /// </summary>
        public System.Drawing.Color Color
        {
            get { return color; }
            set { color = value; }
        }

        /// <summary>
        /// Pen width
        /// </summary>
        public int PenWidth
        {
            get { return penWidth; }
            set { penWidth = value; }
        }

        public FillBrushes.BrushType BrushType
        {
            get { return _brushType; }
            set { _brushType = value; }
        }

        /// <summary>
        /// SKPaint used to fill object
        /// </summary>
        public SKPaint? DrawBrush
        {
            get { return drawBrush; }
            set { drawBrush = value; }
        }

        public DrawingPens.PenType PenType
        {
            get { return _penType; }
            set { _penType = value; }
        }

        public System.Drawing.Drawing2D.LineCap EndCap
        {
            get { return _endCap; }
            set { _endCap = value; }
        }

        /// <summary>
        /// SKPaint used to draw object outline
        /// </summary>
        public SKPaint? DrawPaint
        {
            get { return drawPaint; }
            set { drawPaint = value; }
        }

        /// <summary>
        /// Number of handles
        /// </summary>
        public virtual int HandleCount
        {
            get { return 0; }
        }

        /// <summary>
        /// Number of Connection Points
        /// </summary>
        public virtual int ConnectionCount
        {
            get { return 0; }
        }

        /// <summary>
        /// Last used color
        /// </summary>
        public static System.Drawing.Color LastUsedColor
        {
            get { return lastUsedColor; }
            set { lastUsedColor = value; }
        }

        /// <summary>
        /// Last used pen width
        /// </summary>
        public static int LastUsedPenWidth
        {
            get { return lastUsedPenWidth; }
            set { lastUsedPenWidth = value; }
        }

        /// <summary>
        /// Text to display when mouse is over an object
        /// </summary>
        public string? TipText
        {
            get { return tipText; }
            set { tipText = value; }
        }

        #endregion Properties

        #region Constructor

        protected DrawObject()
        {
            // ReSharper disable DoNotCallOverridableMethodsInConstructor
            ID = GetHashCode();
            // ReSharper restore DoNotCallOverridableMethodsInConstructor
        }
        #endregion

        #region Virtual Functions
        /// <summary>
        /// Clone this instance.
        /// </summary>
        public abstract DrawObject Clone();

        /// <summary>
        /// Draw object using SkiaSharp canvas
        /// </summary>
        /// <param name="canvas">SKCanvas to draw on</param>
        public virtual void Draw(SKCanvas canvas)
        {
        }

        /// <summary>
        /// Draw object using legacy Graphics (for compatibility)
        /// </summary>
        /// <param name="g">Graphics object will be drawn on</param>
        [Obsolete("Use Draw(SKCanvas canvas) instead")]
        public virtual void Draw(System.Drawing.Graphics g)
        {
            // Legacy method for backward compatibility
            // Derived classes should override Draw(SKCanvas) instead
        }

        #region Selection handle methods
        /// <summary>
        /// Get handle point by 1-based number
        /// </summary>
        /// <param name="handleNumber">1-based handle number to return</param>
        /// <returns>Point where handle is located, if found</returns>
        public virtual System.Drawing.Point GetHandle(int handleNumber)
        {
            return new System.Drawing.Point(0, 0);
        }

        /// <summary>
        /// Get handle rectangle by 1-based number
        /// </summary>
        /// <param name="handleNumber"></param>
        /// <returns>Rectangle structure to draw the handle</returns>
        public virtual System.Drawing.Rectangle GetHandleRectangle(int handleNumber)
        {
            System.Drawing.Point point = GetHandle(handleNumber);
            // Take into account width of pen
            return new System.Drawing.Rectangle(point.X - (penWidth + 3), point.Y - (penWidth + 3), 7 + penWidth, 7 + penWidth);
        }

        /// <summary>
        /// Draw tracker for selected object using SkiaSharp
        /// </summary>
        /// <param name="canvas">SKCanvas to draw on</param>
        public virtual void DrawTracker(SKCanvas canvas)
        {
            if (!Selected)
                return;

            using (var paint = new SKPaint())
            {
                paint.Color = SKColors.Black;
                paint.Style = SKPaintStyle.Fill;
                paint.IsAntialias = true;

                for (int i = 1; i <= HandleCount; i++)
                {
                    var handleRect = GetHandleRectangle(i);
                    var skRect = new SKRect(handleRect.Left, handleRect.Top, handleRect.Right, handleRect.Bottom);
                    canvas.DrawRect(skRect, paint);
                }
            }
        }

        /// <summary>
        /// Draw tracker for selected object using legacy Graphics (for compatibility)
        /// </summary>
        /// <param name="g">Graphics to draw on</param>
        [Obsolete("Use DrawTracker(SKCanvas canvas) instead")]
        public virtual void DrawTracker(System.Drawing.Graphics g)
        {
            if (!Selected)
                return;
            
            using (var brush = new System.Drawing.SolidBrush(System.Drawing.Color.Black))
            {
                for (int i = 1; i <= HandleCount; i++)
                {
                    g.FillRectangle(brush, GetHandleRectangle(i));
                }
            }
        }
        #endregion Selection handle methods

        #region Connection Point methods
        /// <summary>
        /// Get connection point by 0-based number
        /// </summary>
        /// <param name="connectionNumber">0-based connection number to return</param>
        /// <returns>Point where connection is located, if found</returns>
        public virtual System.Drawing.Point GetConnection(int connectionNumber)
        {
            return new System.Drawing.Point(0, 0);
        }

        /// <summary>
        /// Get connectionPoint rectangle that defines the ellipse for the requested connection
        /// </summary>
        /// <param name="connectionNumber">0-based connection number</param>
        /// <returns>Rectangle structure to draw the connection</returns>
        public virtual System.Drawing.Rectangle GetConnectionEllipse(int connectionNumber)
        {
            System.Drawing.Point p = GetConnection(connectionNumber);
            // Take into account width of pen
            return new System.Drawing.Rectangle(p.X - (penWidth + 3), p.Y - (penWidth + 3), 7 + penWidth, 7 + penWidth);
        }

        /// <summary>
        /// Draw connection point using SkiaSharp
        /// </summary>
        /// <param name="canvas">SKCanvas to draw on</param>
        /// <param name="connectionNumber">Connection number to draw</param>
        public virtual void DrawConnection(SKCanvas canvas, int connectionNumber)
        {
            var connectionRect = GetConnectionEllipse(connectionNumber);
            var skRect = new SKRect(connectionRect.Left, connectionRect.Top, connectionRect.Right, connectionRect.Bottom);

            using (var fillPaint = new SKPaint())
            using (var strokePaint = new SKPaint())
            {
                fillPaint.Color = SKColors.Red;
                fillPaint.Style = SKPaintStyle.Fill;
                fillPaint.IsAntialias = true;

                strokePaint.Color = SKColors.Red;
                strokePaint.Style = SKPaintStyle.Stroke;
                strokePaint.StrokeWidth = 1.0f;
                strokePaint.IsAntialias = true;

                canvas.DrawOval(skRect, strokePaint);
                canvas.DrawOval(skRect, fillPaint);
            }
        }

        /// <summary>
        /// Draw connection point using legacy Graphics (for compatibility)
        /// </summary>
        /// <param name="g">Graphics to draw on</param>
        /// <param name="connectionNumber">Connection number to draw</param>
        [Obsolete("Use DrawConnection(SKCanvas canvas, int connectionNumber) instead")]
        public virtual void DrawConnection(System.Drawing.Graphics g, int connectionNumber)
        {
            using (var b = new System.Drawing.SolidBrush(System.Drawing.Color.Red))
            using (var p = new System.Drawing.Pen(System.Drawing.Color.Red, -1.0f))
            {
                g.DrawEllipse(p, GetConnectionEllipse(connectionNumber));
                g.FillEllipse(b, GetConnectionEllipse(connectionNumber));
            }
        }

        /// <summary>
        /// Draws the ellipse for the connection handles on the object using SkiaSharp
        /// </summary>
        /// <param name="canvas">SKCanvas to draw on</param>
        public virtual void DrawConnections(SKCanvas canvas)
        {
            if (!Selected)
                return;

            using (var fillPaint = new SKPaint())
            using (var strokePaint = new SKPaint())
            {
                fillPaint.Color = SKColors.White;
                fillPaint.Style = SKPaintStyle.Fill;
                fillPaint.IsAntialias = true;

                strokePaint.Color = SKColors.Black;
                strokePaint.Style = SKPaintStyle.Stroke;
                strokePaint.StrokeWidth = 1.0f;
                strokePaint.IsAntialias = true;

                for (int i = 0; i < ConnectionCount; i++)
                {
                    var connectionRect = GetConnectionEllipse(i);
                    var skRect = new SKRect(connectionRect.Left, connectionRect.Top, connectionRect.Right, connectionRect.Bottom);
                    
                    canvas.DrawOval(skRect, strokePaint);
                    canvas.DrawOval(skRect, fillPaint);
                }
            }
        }

        /// <summary>
        /// Draws the ellipse for the connection handles on the object using legacy Graphics (for compatibility)
        /// </summary>
        /// <param name="g">Graphics to draw on</param>
        [Obsolete("Use DrawConnections(SKCanvas canvas) instead")]
        public virtual void DrawConnections(System.Drawing.Graphics g)
        {
            if (!Selected)
                return;
            
            using (var b = new System.Drawing.SolidBrush(System.Drawing.Color.White))
            using (var p = new System.Drawing.Pen(System.Drawing.Color.Black, -1.0f))
            {
                for (int i = 0; i < ConnectionCount; i++)
                {
                    g.DrawEllipse(p, GetConnectionEllipse(i));
                    g.FillEllipse(b, GetConnectionEllipse(i));
                }
            }
        }
        #endregion Connection Point methods

        /// <summary>
        /// Hit test to determine if object is hit.
        /// </summary>
        /// <param name="point">Point to test</param>
        /// <returns>			(-1)		no hit
        ///						(0)		hit anywhere
        ///						(1 to n)	handle number</returns>
        public virtual int HitTest(System.Drawing.Point point)
        {
            return -1;
        }

        /// <summary>
        /// Test whether point is inside of the object
        /// </summary>
        /// <param name="point">Point to test</param>
        /// <returns>true if in object, false if not</returns>
        protected virtual bool PointInObject(System.Drawing.Point point)
        {
            return false;
        }

        public abstract System.Drawing.Rectangle GetBounds(System.Drawing.Graphics g);

        /// <summary>
        /// Get cursor for the handle
        /// </summary>
        /// <param name="handleNumber">handle number to return cursor for</param>
        /// <returns>Cursor object</returns>
        public virtual Cursor GetHandleCursor(int handleNumber)
        {
            return Cursors.Default;
        }

        /// <summary>
        /// Test whether object intersects with rectangle
        /// </summary>
        /// <param name="rectangle">Rectangle structure to test</param>
        /// <returns>true if intersect, false if not</returns>
        public virtual bool IntersectsWith(System.Drawing.Rectangle rectangle)
        {
            return false;
        }

        /// <summary>
        /// Move object
        /// </summary>
        /// <param name="deltaX">Distance along X-axis: (+)=Right, (-)=Left</param>
        /// <param name="deltaY">Distance along Y axis: (+)=Down, (-)=Up</param>
        public virtual void Move(int deltaX, int deltaY)
        {
        }

        /// <summary>
        /// Move handle to the point
        /// </summary>
        /// <param name="point">Point to Move Handle to</param>
        /// <param name="handleNumber">Handle number to move</param>
        public virtual void MoveHandleTo(System.Drawing.Point point, int handleNumber)
        {
        }

        /// <summary>
        /// Dump (for debugging)
        /// </summary>
        public virtual void Dump()
        {
            Trace.WriteLine("");
            Trace.WriteLine(GetType().Name);
            Trace.WriteLine("Selected = " + selected.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Normalize object.
        /// Call this function in the end of object resizing.
        /// </summary>
        public virtual void Normalize()
        {
        }

        // Public implementation of Dispose pattern callable by consumers. 
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);       
        }

        // Protected implementation of Dispose pattern. 
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    // Free any managed objects here. 
                    //
                    if (this.drawPaint != null)
                    {
                        this.drawPaint.Dispose();
                    }
                    if (this.drawBrush != null)
                    {
                        this.drawBrush.Dispose();
                    }
                }

                // Free any unmanaged objects here. 
                //
                
                this._disposed = true;
            }
        }

        ~DrawObject()
        {
             this.Dispose(false);
        }

        #region Save / Load methods
        /// <summary>
        /// Save object to serialization stream
        /// </summary>
        /// <param name="info">The data being written to disk</param>
        /// <param name="orderNumber">Index of the Layer being saved</param>
        /// <param name="objectIndex">Index of the object on the Layer</param>
        public virtual void SaveToStream(SerializationInfo info, int orderNumber, int objectIndex)
        {
            info.AddValue(
                String.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryColor, orderNumber, objectIndex),
                Color.ToArgb());

            info.AddValue(
                String.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryPenWidth, orderNumber, objectIndex),
                PenWidth);

            info.AddValue(
                string.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryPen, orderNumber, objectIndex),
                PenType);

            info.AddValue(
                string.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryBrush, orderNumber, objectIndex),
                BrushType);

            info.AddValue(
                String.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryFillColor, orderNumber, objectIndex),
                FillColor.ToArgb());

            info.AddValue(
                String.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryFilled, orderNumber, objectIndex),
                Filled);

            info.AddValue(
                String.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryZOrder, orderNumber, objectIndex),
                ZOrder);

            info.AddValue(
                String.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryRotation, orderNumber, objectIndex),
                Rotation);

            info.AddValue(
                String.Format(CultureInfo.InvariantCulture, 
                              "{0}{1}-{2}",
                              entryTipText, orderNumber, objectIndex),
                tipText ?? string.Empty);
        }

        /// <summary>
        /// Load object from serialization stream
        /// </summary>
        /// <param name="info">Data from disk to parse into an object</param>
        /// <param name="orderNumber">Index of the layer object resides on</param>
        /// <param name="objectData">Index of the object on the layer</param>
        public virtual void LoadFromStream(SerializationInfo info, int orderNumber, int objectData)
        {
            int n = info.GetInt32(
                String.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryColor, orderNumber, objectData));

            Color = System.Drawing.Color.FromArgb(n);

            PenWidth = info.GetInt32(
                String.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryPenWidth, orderNumber, objectData));

            PenType = (DrawingPens.PenType)info.GetValue(
                                            String.Format(CultureInfo.InvariantCulture,
                                                          "{0}{1}-{2}",
                                                          entryPen, orderNumber, objectData),
                                            typeof(DrawingPens.PenType))!;

            BrushType = (FillBrushes.BrushType)info.GetValue(
                                                string.Format(CultureInfo.InvariantCulture,
                                                              "{0}{1}-{2}",
                                                              entryBrush, orderNumber, objectData),
                                                typeof(FillBrushes.BrushType))!;

            n = info.GetInt32(
                String.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryFillColor, orderNumber, objectData));

            FillColor = System.Drawing.Color.FromArgb(n);

            Filled = info.GetBoolean(
                String.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryFilled, orderNumber, objectData));

            ZOrder = info.GetInt32(
                String.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryZOrder, orderNumber, objectData));

            Rotation = info.GetInt32(
                String.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryRotation, orderNumber, objectData));

            tipText = info.GetString(String.Format(CultureInfo.InvariantCulture,
                              "{0}{1}-{2}",
                              entryTipText, orderNumber, objectData));

            // Set the Brush, if defined
            if (BrushType != FillBrushes.BrushType.NoBrush)
                DrawBrush = FillBrushes.SetCurrentBrush(BrushType);
        }
        #endregion Save/Load methods
        #endregion Virtual Functions

        #region Other functions
        /// <summary>
        /// Initialization
        /// </summary>
        protected void Initialize()
        {
        }

        /// <summary>
        /// Copy fields from this instance to cloned instance drawObject.
        /// Called from Clone functions of derived classes.
        /// </summary>
        /// <param name="drawObject">Object being cloned</param>
        protected void FillDrawObjectFields(DrawObject drawObject)
        {
            drawObject.selected = selected;
            drawObject.color = color;
            drawObject.penWidth = penWidth;
            drawObject._endCap = _endCap;
            drawObject.ID = ID;
            drawObject._brushType = _brushType;
            drawObject._penType = _penType;
            drawObject.drawBrush = drawBrush;
            drawObject.drawPaint = drawPaint;
            drawObject.filled = filled;
            drawObject.fillColor = fillColor;
            drawObject._rotation = _rotation;
            drawObject._center = _center;
            drawObject.tipText = tipText;
        }
        #endregion Other functions

        #region IComparable Members
        /// <summary>
        /// Returns (-1), (0), (+1) to represent the relative Z-order of the object being compared with this object
        /// </summary>
        /// <param name="obj">DrawObject that is compared to this object</param>
        /// <returns>	(-1)	if the object is less (further back) than this object.
        ///				(0)	if the object is equal to this object (same level graphically).
        ///				(1)	if the object is greater (closer to the front) than this object.</returns>
        public int CompareTo(object? obj)
        {
            DrawObject? d = obj as DrawObject;
            int x = 0;
            if (d != null)
                if (d.ZOrder == ZOrder)
                    x = 0;
                else if (d.ZOrder > ZOrder)
                    x = -1;
                else
                    x = 1;

            return x;
        }
        #endregion IComparable Members

        #region Helper Methods for SkiaSharp Conversion

        /// <summary>
        /// Convert System.Drawing.Color to SKColor
        /// </summary>
        /// <param name="color">System.Drawing.Color to convert</param>
        /// <returns>SKColor equivalent</returns>
        protected static SKColor ColorToSKColor(System.Drawing.Color color)
        {
            return new SKColor(color.R, color.G, color.B, color.A);
        }

        /// <summary>
        /// Convert System.Drawing.Drawing2D.LineCap to SKStrokeCap
        /// </summary>
        /// <param name="lineCap">System.Drawing.Drawing2D.LineCap to convert</param>
        /// <returns>SKStrokeCap equivalent</returns>
        protected static SKStrokeCap ConvertLineCap(System.Drawing.Drawing2D.LineCap lineCap)
        {
            switch (lineCap)
            {
                case System.Drawing.Drawing2D.LineCap.Round:
                    return SKStrokeCap.Round;
                case System.Drawing.Drawing2D.LineCap.Square:
                    return SKStrokeCap.Square;
                case System.Drawing.Drawing2D.LineCap.Flat:
                default:
                    return SKStrokeCap.Butt;
            }
        }

        /// <summary>
        /// Apply pen type styling to SKPaint
        /// </summary>
        /// <param name="paint">SKPaint to modify</param>
        /// <param name="penType">Pen type to apply</param>
        protected static void ApplyPenType(SKPaint paint, DrawingPens.PenType penType)
        {
            switch (penType)
            {
                case DrawingPens.PenType.Solid:
                    // No dash effect needed (default)
                    break;
                case DrawingPens.PenType.Dash:
                    paint.PathEffect = SKPathEffect.CreateDash(new float[] { 10, 5 }, 0);
                    break;
                case DrawingPens.PenType.Dot:
                    paint.PathEffect = SKPathEffect.CreateDash(new float[] { 2, 2 }, 0);
                    break;
                case DrawingPens.PenType.Dash_Dot:
                    paint.PathEffect = SKPathEffect.CreateDash(new float[] { 10, 5, 2, 5 }, 0);
                    break;
                case DrawingPens.PenType.DoubleLine:
                    // For double line, we'll need to draw two lines - this would be handled in derived classes
                    break;
            }
        }

        /// <summary>
        /// Rotate a point around a center point
        /// </summary>
        /// <param name="point">Point to rotate</param>
        /// <param name="centerX">Center X coordinate</param>
        /// <param name="centerY">Center Y coordinate</param>
        /// <param name="angleDegrees">Angle in degrees</param>
        /// <returns>Rotated point</returns>
        protected static SKPoint RotatePoint(SKPoint point, float centerX, float centerY, float angleDegrees)
        {
            float angleRadians = angleDegrees * (float)Math.PI / 180f;
            float cos = (float)Math.Cos(angleRadians);
            float sin = (float)Math.Sin(angleRadians);

            float dx = point.X - centerX;
            float dy = point.Y - centerY;

            return new SKPoint(
                centerX + (dx * cos - dy * sin),
                centerY + (dx * sin + dy * cos)
            );
        }

        /// <summary>
        /// Get normalized rectangle from coordinates
        /// </summary>
        /// <param name="x1">First X coordinate</param>
        /// <param name="y1">First Y coordinate</param>
        /// <param name="x2">Second X coordinate</param>
        /// <param name="y2">Second Y coordinate</param>
        /// <returns>Normalized SKRect</returns>
        protected static SKRect GetNormalizedRectangle(float x1, float y1, float x2, float y2)
        {
            if (x2 < x1)
            {
                float tmp = x2;
                x2 = x1;
                x1 = tmp;
            }

            if (y2 < y1)
            {
                float tmp = y2;
                y2 = y1;
                y1 = tmp;
            }
            return new SKRect(x1, y1, x2, y2);
        }

        /// <summary>
        /// Get normalized rectangle from System.Drawing.Rectangle
        /// </summary>
        /// <param name="rect">System.Drawing.Rectangle to normalize</param>
        /// <returns>Normalized SKRect</returns>
        protected static SKRect GetNormalizedRectangle(System.Drawing.Rectangle rect)
        {
            return GetNormalizedRectangle(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }

        #endregion Helper Methods for SkiaSharp Conversion
    }
}