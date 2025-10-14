using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using DocToolkit;

namespace CraftSynth.ImageEditor
{
    /// <summary>
    /// Working area.
    /// Handles mouse input and draws graphics objects.
    /// Migrated from .NET Framework 4.8 to .NET 9 with SkiaSharp replacing System.Drawing
    /// </summary>
    internal partial class DrawArea : SKControl, IDisposable
    {
        #region Constructor, Dispose
        public DrawArea()
        {
            // create list of Layers, with one default active visible layer
            _layers = new Layers();
            _layers.CreateNewLayer("Default");
            _panning = false;
            _panX = 0;
            _panY = 0;
            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.DrawArea_MouseWheel);
            
            // Initialize SkiaSharp rendering
            this.PaintSurface += DrawArea_PaintSurface;
            
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
        }

        // Public implementation of Dispose pattern callable by consumers. 
        public new void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Flag: Has Dispose already been called? 
        bool _disposed = false;

        // Protected implementation of Dispose pattern. 
        protected override void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    // Free any managed objects here. 
                    //
                    if (this._currentBrush != null)
                    {
                        this._currentBrush.Dispose();
                    }
                    if (this.CurrentPen != null)
                    {
                        this._currentPen.Dispose();
                    }
                    if (this._layers != null)
                    {
                        this._layers.Dispose();
                    }
                    if (this.tools != null)
                    {
                        foreach (Tool tool in tools)
                        {
                            if (tool != null)
                            {
                                tool.Dispose();
                            }
                        }
                    }
                    if (this.undoManager != null)
                    {
                        this.undoManager.Dispose();
                    }

                    if (components != null)
                    {
                        components.Dispose();
                    }

                    // Free any unmanaged objects here. 
                }

                this._disposed = true;
            }
            base.Dispose(disposing);
        }

        ~DrawArea()
        {
            this.Dispose(false);
        }
        #endregion Constructor, Dispose

        #region Enumerations
        public enum DrawToolType
        {
            Pointer,
            Rectangle,
            Ellipse,
            Line,
            PolyLine,
            Polygon,
            Text,
            Image,
            Connector,
            NumberOfDrawTools
        };
        #endregion Enumerations

        #region Members
        private float _zoom = 1.0f;
        private float _rotation = 0f;
        private int _panX = 0;
        private int _panY;
        private int _originalPanY;
        private bool _panning = false;
        private SKPoint lastPoint;
        private SKColor _lineColor = SKColors.Red;
        private SKColor _fillColor = SKColors.White;
        private bool _drawFilled = false;
        private int _lineWidth = 5;
        private SKStrokeCap _endCap = SKStrokeCap.Round;
        private SKPaint _currentPen;
        private DrawingPens.PenType _penType;
        private SKPaint _currentBrush;
        private FillBrushes.BrushType _brushType;

        // Define the Layers collection
        private Layers _layers;

        private DrawToolType activeTool; // active drawing tool
        private Tool[] tools; // array of tools

        // Information about owner form
        private MainForm owner;
        private DocManager docManager;

        // group selection rectangle
        private SKRect netRectangle;
        private bool drawNetRectangle = false;

        private MainForm myparent;

        public MainForm MyParent
        {
            get { return myparent; }
            set { myparent = value; }
        }

        private UndoManager undoManager;
        #endregion Members

        #region Properties
        /// <summary>
        /// Allow tools and objects to see the type of brush set
        /// </summary>
        public FillBrushes.BrushType BrushType
        {
            get { return _brushType; }
            set { _brushType = value; }
        }

        public SKPaint CurrentBrush
        {
            get { return _currentBrush; }
            set { _currentBrush = value; }
        }

        /// <summary>
        /// Allow tools and objects to see the type of pen set
        /// </summary>
        public DrawingPens.PenType PenType
        {
            get { return _penType; }
            set { _penType = value; }
        }

        /// <summary>
        /// Arrow or Rounded.
        /// </summary>
        public SKStrokeCap EndCap
        {
            get { return _endCap; }
            set { _endCap = value; }
        }

        /// <summary>
        /// Current Drawing Pen
        /// </summary>
        public SKPaint CurrentPen
        {
            get { return _currentPen; }
            set { _currentPen = value; }
        }

        /// <summary>
        /// Current Line Width
        /// </summary>
        public int LineWidth
        {
            get { return _lineWidth; }
            set { _lineWidth = value; }
        }

        /// <summary>
        /// Flag determines if objects will be drawn filled or not
        /// </summary>
        public bool DrawFilled
        {
            get { return _drawFilled; }
            set { _drawFilled = value; }
        }

        /// <summary>
        /// Color to draw filled objects with
        /// </summary>
        public SKColor FillColor
        {
            get { return _fillColor; }
            set { _fillColor = value; }
        }

        /// <summary>
        /// Color for drawing lines
        /// </summary>
        public SKColor LineColor
        {
            get { return _lineColor; }
            set { _lineColor = value; }
        }

        /// <summary>
        /// Original Y position - used when panning
        /// </summary>
        public int OriginalPanY
        {
            get { return _originalPanY; }
            set { _originalPanY = value; }
        }

        /// <summary>
        /// Flag is true if panning active
        /// </summary>
        public bool Panning
        {
            get { return _panning; }
            set { _panning = value; }
        }

        /// <summary>
        /// Current pan offset along X-axis
        /// </summary>
        public int PanX
        {
            get { return _panX; }
            set { _panX = value; }
        }

        /// <summary>
        /// Current pan offset along Y-axis
        /// </summary>
        public int PanY
        {
            get { return _panY; }
            set { _panY = value; }
        }

        /// <summary>
        /// Degrees of rotation of the drawing
        /// </summary>
        public float Rotation
        {
            get { return _rotation; }
            set { _rotation = value; }
        }

        /// <summary>
        /// Current Zoom factor
        /// </summary>
        public float Zoom
        {
            get { return _zoom; }
            set { _zoom = value; }
        }

        /// <summary>
        /// Group selection rectangle. Used for drawing.
        /// </summary>
        public SKRect NetRectangle
        {
            get { return netRectangle; }
            set { netRectangle = value; }
        }

        /// <summary>
        /// Flag is set to true if group selection rectangle should be drawn.
        /// </summary>
        public bool DrawNetRectangle
        {
            get { return drawNetRectangle; }
            set { drawNetRectangle = value; }
        }

        /// <summary>
        /// Reference to the owner form
        /// </summary>
        public MainForm Owner
        {
            get { return owner; }
            set { owner = value; }
        }

        /// <summary>
        /// Reference to DocManager
        /// </summary>
        public DocManager DocManager
        {
            get { return docManager; }
            set { docManager = value; }
        }

        /// <summary>
        /// Active drawing tool.
        /// </summary>
        public DrawToolType ActiveTool
        {
            get { return activeTool; }
            set { activeTool = value; }
        }

        /// <summary>
        /// List of Layers in the drawing
        /// </summary>
        public Layers TheLayers
        {
            get { return _layers; }
            set { _layers = value; }
        }

        /// <summary>
        /// Return True if Undo operation is possible
        /// </summary>
        public bool CanUndo
        {
            get
            {
                if (undoManager != null)
                {
                    return undoManager.CanUndo;
                }

                return false;
            }
        }

        /// <summary>
        /// Return True if Redo operation is possible
        /// </summary>
        public bool CanRedo
        {
            get
            {
                if (undoManager != null)
                {
                    return undoManager.CanRedo;
                }

                return false;
            }
        }

        public SKRect GetBounds()
        {
            float furthestLeft = float.MaxValue;
            float furthestTop = float.MaxValue;
            float furthestRight = float.MinValue;
            float furthestBottom = float.MinValue;
            SKRect rect;
            
            if (_layers != null)
            {
                int lc = _layers.Count;
                for (int i = 0; i < lc; i++)
                {
                    // Console.WriteLine(String.Format("Layer {0} is Visible: {1}", i.ToString(), _layers[i].IsVisible.ToString()));
                    if (_layers[i].IsVisible)
                    {
                        if (_layers[i].Graphics != null)
                        {
                            for (int ig = 0; ig < _layers[i].Graphics.Count; ig++)
                            {
                                rect = _layers[i].Graphics[ig].GetBoundsSkia();
                                furthestLeft = Math.Min(furthestLeft, rect.Left);
                                furthestTop = Math.Min(furthestTop, rect.Top); // Fixed bug from original (was rect.Left)
                                furthestRight = Math.Max(furthestRight, rect.Right);
                                furthestBottom = Math.Max(furthestBottom, rect.Bottom);
                            }
                        }
                    }
                }
            }
            rect = new SKRect(furthestLeft, furthestTop, furthestRight, furthestBottom);
            return rect;
        }

        internal void ReplaceInitialImage(SKBitmap image)
        {
            ((CraftSynth.ImageEditor.DrawImage)(this._layers[0].Graphics[this._layers[0].Graphics.Count - 1])).TheImage = image;
            //  this.AddCommandToHistory(new CommandAdd(this.TheLayers[0].Graphics[this._layers[0].Graphics.Count - 1]));
            this.Invalidate();
        }

        internal KeyValuePair<int, DrawImage>? GetInitialImageGraphic()
        {
            for (int i = this._layers[0].Graphics.Count - 1; i >= 0; i--)
            {
                if (this._layers[0].Graphics[i] is DrawImage && (this._layers[0].Graphics[i] as DrawImage).IsInitialImage)
                {
                    return new KeyValuePair<int, DrawImage>(i, this._layers[0].Graphics[i] as DrawImage);
                }
            }
            return null;
        }

        internal void DeselectAll()
        {
            ActiveTool = DrawToolType.Pointer;
            int al = this.TheLayers.ActiveLayerIndex;
            this.TheLayers[al].Graphics.UnselectAll();
            this.Invalidate();
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Draw graphic objects and group selection rectangle (optionally) using SkiaSharp
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DrawArea_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            var info = e.Info;
            
            // Clear the canvas
            canvas.Clear(SKColors.White);
            
            // Save the current state
            canvas.Save();
            
            // Apply transformations: translate to center, rotate, translate back with pan, then scale
            canvas.Translate(info.Width / 2f, info.Height / 2f);
            canvas.RotateDegrees(_rotation);
            canvas.Translate(-info.Width / 2f + _panX, -info.Height / 2f + _panY);
            canvas.Scale(_zoom, _zoom);

            // Draw objects on each layer, in succession so we get the correct layering. Only draw layers that are visible
            if (_layers != null)
            {
                int lc = _layers.Count;
                for (int i = 0; i < lc; i++)
                {
                    Console.WriteLine(String.Format("Layer {0} is Visible: {1}", i.ToString(), _layers[i].IsVisible.ToString()));
                    if (_layers[i].IsVisible)
                    {
                        if (_layers[i].Graphics != null)
                            _layers[i].Graphics.DrawSkia(canvas);
                    }
                }
            }

            DrawNetSelectionSkia(canvas);

            // Restore the canvas state
            canvas.Restore();
        }

        /// <summary>
        /// Back Track the Mouse to return accurate coordinates regardless of zoom or pan effects.
        /// Courtesy of BobPowell.net adapted for SkiaSharp
        /// </summary>
        /// <param name="p">Point to backtrack</param>
        /// <returns>Backtracked point</returns>
        public SKPoint BackTrackMouse(System.Drawing.Point p)
        {
            // Convert to SKPoint first
            var skPoint = new SKPoint(p.X, p.Y);
            
            // Create inverse transformation matrix
            var matrix = SKMatrix.CreateIdentity();
            
            // Apply transformations in reverse order
            SKMatrix.PostConcat(ref matrix, SKMatrix.CreateScale(1f / _zoom, 1f / _zoom));
            SKMatrix.PostConcat(ref matrix, SKMatrix.CreateTranslation(-ClientSize.Width / 2f - _panX, -ClientSize.Height / 2f - _panY));
            SKMatrix.PostConcat(ref matrix, SKMatrix.CreateRotationDegrees(-_rotation));
            SKMatrix.PostConcat(ref matrix, SKMatrix.CreateTranslation(ClientSize.Width / 2f, ClientSize.Height / 2f));
            
            // Transform the point
            return matrix.MapPoint(skPoint);
        }

        /// <summary>
        /// Mouse down.
        /// Left button down event is passed to active tool.
        /// Right button down event is handled in this class.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            lastPoint = BackTrackMouse(e.Location);
            if (e.Button == MouseButtons.Left)
                tools[(int)activeTool].OnMouseDown(this, e);
            else if (e.Button == MouseButtons.Right)
            {
                if (_panning)
                    _panning = false;
                if (activeTool == DrawToolType.PolyLine || activeTool == DrawToolType.Connector)
                    tools[(int)activeTool].OnMouseDown(this, e);
                ActiveTool = DrawToolType.Pointer;
                OnContextMenu(e);
            }
            
            base.OnMouseDown(e);
        }

        /// <summary>
        /// Mouse move.
        /// Moving without button pressed or with left button pressed
        /// is passed to active tool.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            SKPoint curLoc = BackTrackMouse(e.Location);
            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.None)
            {
                if (e.Button == MouseButtons.Left && _panning)
                {
                    if (curLoc.X != lastPoint.X)
                        _panX += (int)(curLoc.X - lastPoint.X);
                    if (curLoc.Y != lastPoint.Y)
                        _panY += (int)(curLoc.Y - lastPoint.Y);
                    Invalidate();
                }
                else
                    tools[(int)activeTool].OnMouseMove(this, e);
            }
            else
                Cursor = Cursors.Default;
                
            lastPoint = BackTrackMouse(e.Location);
            
            base.OnMouseMove(e);
        }

        /// <summary>
        /// Mouse up event.
        /// Left button up event is passed to active tool.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                tools[(int)activeTool].OnMouseUp(this, e);
                if (activeTool != DrawToolType.Pointer && activeTool != DrawToolType.Text && activeTool != DrawToolType.Image)
                {
                    int al = this.TheLayers.ActiveLayerIndex;
                    this.AddCommandToHistory(new CommandAdd(this.TheLayers[al].Graphics[0]));
                }

                if (this.PanX != 0 || this.PanY != 0)
                {
                    this.myparent.ManualScroll(true, -(int)Math.Round(this.PanX * this._zoom));
                    this.myparent.ManualScroll(false, -(int)Math.Round(this.PanY * this._zoom));
                    this.PanX = 0;
                    this.PanY = 0;
                    this.Invalidate();
                    this.myparent.pnlDrawArea.Invalidate();
                }

                this.ActiveTool = DrawArea.DrawToolType.Pointer;//Selected tool is automatically dropped after drawing item
            }
            
            base.OnMouseUp(e);
        }

        public void CutObject()
        {
            MessageBox.Show("Cut (from drawarea)");
        }

        private void DrawArea_MouseWheel(object sender, MouseEventArgs e)
        {
        }

        #endregion

        #region Other Functions
        /// <summary>
        /// Initialization
        /// </summary>
        /// <param name="owner">Reference to the owner form</param>
        /// <param name="docManager">Reference to Document manager</param>
        public void Initialize(MainForm owner, DocManager docManager, SKBitmap initialImage, string initialImageAsFilePath, byte[] initialImageAsPngBytes)
        {
            // Keep reference to owner form
            Owner = owner;
            DocManager = docManager;

            // set default tool
            activeTool = DrawToolType.Pointer;

            // Create undo manager
            undoManager = new UndoManager(_layers);

            // create array of drawing tools
            tools = new Tool[(int)DrawToolType.NumberOfDrawTools];
            tools[(int)DrawToolType.Pointer] = new ToolPointer();
            tools[(int)DrawToolType.Rectangle] = new ToolRectangle();
            tools[(int)DrawToolType.Ellipse] = new ToolEllipse();
            tools[(int)DrawToolType.Line] = new ToolLine();
            tools[(int)DrawToolType.PolyLine] = new ToolPolyLine();
            tools[(int)DrawToolType.Polygon] = new ToolPolygon();
            tools[(int)DrawToolType.Text] = new ToolText();
            tools[(int)DrawToolType.Image] = new ToolImage();
            tools[(int)DrawToolType.Connector] = new ToolConnector();

            LineColor = SKColors.Red;
            FillColor = SKColors.White;
            LineWidth = 5;

            LoadInitialImage(initialImage, initialImageAsFilePath, initialImageAsPngBytes, null);
        }

        public void LoadInitialImage(SKBitmap initialImage, string initialImageAsFilePath, byte[] initialImageAsPngBytes, DrawImage paradigm)
        {
            if (initialImage != null)
            {
                ((ToolImage)tools[(int)DrawToolType.Image]).InsertImage(this, initialImage, true, true, paradigm);
            }

            if (initialImageAsFilePath != null)
            {
                ((ToolImage)tools[(int)DrawToolType.Image]).InsertImage(this, initialImageAsFilePath, true, true, paradigm);
            }

            if (initialImageAsPngBytes != null)
            {
                ((ToolImage)tools[(int)DrawToolType.Image]).InsertImage(this, initialImageAsPngBytes, true, true, paradigm);
            }
            this.Invalidate();
        }

        /// <summary>
        /// Add command to history.
        /// </summary>
        public void AddCommandToHistory(Command command)
        {
            undoManager.AddCommandToHistory(command);
        }

        /// <summary>
        /// Clear Undo history.
        /// </summary>
        public void ClearHistory()
        {
            undoManager.ClearHistory();
        }

        /// <summary>
        /// Undo
        /// </summary>
        public void Undo()
        {
            undoManager.Undo();
            Refresh();
        }

        /// <summary>
        /// Redo
        /// </summary>
        public void Redo()
        {
            undoManager.Redo();
            Refresh();
        }

        /// <summary>
        ///  Draw group selection rectangle using SkiaSharp
        /// </summary>
        /// <param name="canvas"></param>
        public void DrawNetSelectionSkia(SKCanvas canvas)
        {
            if (!DrawNetRectangle)
                return;

            using (var paint = new SKPaint())
            {
                paint.Style = SKPaintStyle.Stroke;
                paint.Color = SKColors.Black;
                paint.StrokeWidth = 1;
                paint.PathEffect = SKPathEffect.CreateDash(new float[] { 5, 5 }, 0);
                canvas.DrawRect(NetRectangle, paint);
            }
        }

        /// <summary>
        /// Right-click handler
        /// </summary>
        /// <param name="e"></param>
        private void OnContextMenu(MouseEventArgs e)
        {
            // Change current selection if necessary
            SKPoint point = BackTrackMouse(new System.Drawing.Point(e.X, e.Y));
            System.Drawing.Point menuPoint = new System.Drawing.Point(e.X, e.Y);
            int al = _layers.ActiveLayerIndex;
            int n = _layers[al].Graphics.Count;
            DrawObject o = null;

            for (int i = 0; i < n; i++)
            {
                if (_layers[al].Graphics[i].HitTestSkia(point) == 0)
                {
                    o = _layers[al].Graphics[i];
                    break;
                }
            }

            if (o != null)
            {
                if (!o.Selected)
                    _layers[al].Graphics.UnselectAll();

                // Select clicked object
                o.Selected = true;
            }
            else
            {
                _layers[al].Graphics.UnselectAll();
            }

            Refresh();
            Owner.ctxtMenu.Show(this, menuPoint);
        }

        #region Helper Methods for System.Drawing compatibility

        /// <summary>
        /// Convert SKColor to System.Drawing.Color for backwards compatibility
        /// </summary>
        /// <param name="skColor"></param>
        /// <returns></returns>
        public static System.Drawing.Color SKColorToColor(SKColor skColor)
        {
            return System.Drawing.Color.FromArgb(skColor.Alpha, skColor.Red, skColor.Green, skColor.Blue);
        }

        /// <summary>
        /// Convert System.Drawing.Color to SKColor
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static SKColor ColorToSKColor(System.Drawing.Color color)
        {
            return new SKColor(color.R, color.G, color.B, color.A);
        }

        /// <summary>
        /// Convert SKRect to System.Drawing.Rectangle for backwards compatibility
        /// </summary>
        /// <param name="skRect"></param>
        /// <returns></returns>
        public static System.Drawing.Rectangle SKRectToRectangle(SKRect skRect)
        {
            return new System.Drawing.Rectangle(
                (int)Math.Floor(skRect.Left),
                (int)Math.Floor(skRect.Top),
                (int)Math.Ceiling(skRect.Width),
                (int)Math.Ceiling(skRect.Height)
            );
        }

        /// <summary>
        /// Convert System.Drawing.Rectangle to SKRect
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static SKRect RectangleToSKRect(System.Drawing.Rectangle rect)
        {
            return new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }

        /// <summary>
        /// Convert SKPoint to System.Drawing.Point for backwards compatibility  
        /// </summary>
        /// <param name="skPoint"></param>
        /// <returns></returns>
        public static System.Drawing.Point SKPointToPoint(SKPoint skPoint)
        {
            return new System.Drawing.Point((int)Math.Round(skPoint.X), (int)Math.Round(skPoint.Y));
        }

        /// <summary>
        /// Convert System.Drawing.Point to SKPoint
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static SKPoint PointToSKPoint(System.Drawing.Point point)
        {
            return new SKPoint(point.X, point.Y);
        }

        #endregion

        #endregion
    }
}