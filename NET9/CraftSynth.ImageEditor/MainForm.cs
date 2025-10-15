using System;

using System.ComponentModel;

using System.Diagnostics.CodeAnalysis;

using System.Windows.Forms;

 

// Skia-first export; GDI fallback for legacy DrawArea rendering

using SkiaSharp;

 

namespace CraftSynth.ImageEditor

{

    public partial class MainForm : UserControl

    {

        private DrawArea _drawArea;

        private bool _panMode;

        private bool _zoomOnMouseWheel;

 

        public Form ParentFormWindow { get; private set; }

        public string ArgumentFile { get; set; } = string.Empty;

 

        // Optional inputs at construction time

        public System.Drawing.Image? InitialImage { get; set; }

        public string? InitialImageAsFilePath { get; set; }

        public byte[]? InitialImageAsPngBytes { get; set; }

 

        public bool ZoomOnMouseWheel

        {

            get => _zoomOnMouseWheel;

            set

            {

                if (_zoomOnMouseWheel == value) return;

                _zoomOnMouseWheel = value;

 

                // Rewire wheel handler to panel when toggled on

                if (_zoomOnMouseWheel) this.pnlDrawArea.MouseWheel += MainForm_MouseWheel;

                else this.pnlDrawArea.MouseWheel -= MainForm_MouseWheel;

            }

        }

 

        public MainForm()

        {

            InitializeComponent();

            this.MouseWheel += MainForm_MouseWheel;

        }

 

        public void Initialize(Form parentForm)

        {

            ParentFormWindow = parentForm;

        }

 

        #region Lifecycle / Dispose

 

        private bool _disposed;

 

        protected override void Dispose(bool disposing)

        {

            if (_disposed) return;

 

            if (disposing)

            {

                components?.Dispose();

                InitialImage?.Dispose();

                _drawArea?.Dispose();

            }

 

            _disposed = true;

            base.Dispose(disposing);

        }

 

        #endregion

 

        #region WinForms events

 

        private void MainForm_Load(object? sender, EventArgs e)

        {

            // Create and add DrawArea into the panel host

            _drawArea = new DrawArea

            {

                MyParent = this,

                Owner = this,

                Location = new System.Drawing.Point(0, 0),

                Size = new System.Drawing.Size(10, 10),

                BorderStyle = BorderStyle.None

            };

            this.pnlDrawArea.Controls.Add(_drawArea);

 

            // minimal init consistent with existing DrawArea API (kept from your source)

            _drawArea.Initialize(this, docManager: null, // docManager path removed

                                 initialImage: InitialImage,

                                 initialImageAsFilePath: InitialImageAsFilePath,

                                 initialImageAsPngBytes: InitialImageAsPngBytes);

 

            ResizeDrawArea();

            SetStateOfControls();

 

            Application.Idle += (_, __) =>

            {

                if (IsDisposed) return;

                ResizeDrawArea();

                SetStateOfControls();

            };

 

            if (!string.IsNullOrWhiteSpace(ArgumentFile))

            {

                // no-op if you removed DocManager; hook your file-open flow here

                // OpenDocument(ArgumentFile);

            }

        }

 

        private void MainForm_Resize(object? sender, EventArgs e)

        {

            if (_drawArea is null) return;

            ResizeDrawArea();

        }

 

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)

        {

            // persist UI settings if required

        }

 

        private void MainForm_MouseWheel(object? sender, MouseEventArgs e)

        {

            if (!_zoomOnMouseWheel || _drawArea is null) return;

 

            if (e.Delta > 0) _drawArea.Zoom = Math.Min(_drawArea.Zoom * 1.1f, 8f);

            else _drawArea.Zoom = Math.Max(_drawArea.Zoom / 1.1f, 0.1f);

 

            ResizeDrawArea();

            _drawArea.Invalidate();

        }

 

        #endregion

 

        #region UI State / Layout

 

        public void SetStateOfControls()

        {

            if (_drawArea is null) return;

 

            toolStripButtonPointer.Checked   = !_drawArea.Panning && _drawArea.ActiveTool == DrawArea.DrawToolType.Pointer;

            toolStripButtonRectangle.Checked = _drawArea.ActiveTool == DrawArea.DrawToolType.Rectangle;

            toolStripButtonEllipse.Checked   = _drawArea.ActiveTool == DrawArea.DrawToolType.Ellipse;

            toolStripButtonLine.Checked      = _drawArea.ActiveTool == DrawArea.DrawToolType.Line && _drawArea.EndCap != System.Drawing.Drawing2D.LineCap.ArrowAnchor;

            toolStripButtonArrow.Checked     = _drawArea.ActiveTool == DrawArea.DrawToolType.Line && _drawArea.EndCap == System.Drawing.Drawing2D.LineCap.ArrowAnchor;

            toolStripButtonPencil.Checked    = _drawArea.ActiveTool == DrawArea.DrawToolType.Polygon;

 

            tsbPanMode.Checked = _drawArea.Panning;

            // Line/Fill color buttons reflect DrawArea state (these are decorative in the toolbar)

            tsbLineColor.BackColor = _drawArea.LineColor;

            tsbFillColor.BackColor = _drawArea.FillColor;

 

            // thickness text

            toolStripDropDownButtonLineThickness.Text = _drawArea.LineWidth switch

            {

                -1 => "Thinnest",

                2  => "Thin",

                5  => "Thick",

                10 => "Thicker",

                15 => "Thickest",

                _  => "Thickness"

            };

            toolStripDropDownButtonPenType.Text = DrawingPens.GetPenTypeAsString(_drawArea.PenType);

        }

 

        // Ensure the drawing surface is large enough for the current zoom/bounds

        private void ResizeDrawArea()

        {

            if (_drawArea is null) return;

 

            var bounds = _drawArea.GetBounds(); // uses existing API in DrawArea

            _drawArea.Width  = Math.Max(pnlDrawArea.ClientRectangle.Width,  (int)Math.Round((bounds.Left + bounds.Width  + 10) * _drawArea.Zoom));

            _drawArea.Height = Math.Max(pnlDrawArea.ClientRectangle.Height, (int)Math.Round((bounds.Top  + bounds.Height + 10) * _drawArea.Zoom));

            pnlDrawArea.Invalidate();

        }

 

        #endregion

 

        #region Commands (wired from ToolStrip/Menu)

 

        private void CommandPointer()

        {

            if (_drawArea is null) return;

            _drawArea.ActiveTool = DrawArea.DrawToolType.Pointer;

            _panMode = false;

            _drawArea.Panning = _panMode;

        }

 

        private void CommandRectangle()

        {

            if (_drawArea is null) return;

            _drawArea.ActiveTool = DrawArea.DrawToolType.Rectangle;

            _drawArea.DrawFilled = false;

            _drawArea.EndCap = System.Drawing.Drawing2D.LineCap.Round;

            _panMode = false;

            _drawArea.Panning = _panMode;

        }

 

        private void CommandEllipse()

        {

            if (_drawArea is null) return;

            _drawArea.ActiveTool = DrawArea.DrawToolType.Ellipse;

            _drawArea.DrawFilled = false;

            _drawArea.EndCap = System.Drawing.Drawing2D.LineCap.Round;

            _panMode = false;

            _drawArea.Panning = _panMode;

        }

 

        private void CommandLine()

        {

            if (_drawArea is null) return;

            _drawArea.ActiveTool = DrawArea.DrawToolType.Line;

            _drawArea.EndCap = System.Drawing.Drawing2D.LineCap.Round;

            _panMode = false;

            _drawArea.Panning = _panMode;

        }

 

        private void CommandArrow()

        {

            if (_drawArea is null) return;

            _drawArea.ActiveTool = DrawArea.DrawToolType.Line;

            _drawArea.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;

            _panMode = false;

            _drawArea.Panning = _panMode;

        }

 

        private void CommandPolygon()

        {

            if (_drawArea is null) return;

            _drawArea.ActiveTool = DrawArea.DrawToolType.Polygon;

            _drawArea.EndCap = System.Drawing.Drawing2D.LineCap.Round;

            _panMode = false;

            _drawArea.Panning = _panMode;

        }

 

        private void CommandUndo() => _drawArea?.Undo();

        private void CommandRedo() => _drawArea?.Redo();

 

        private void CommandAbout()

        {

            using var frm = new FrmAbout();

            frm.ShowDialog(this);

        }

 

        #endregion

 

        #region Export helpers (Skia-first)

 

        /// <summary>

        /// Returns the current canvas as a PNG (byte[]) using SkiaSharp first.

        /// Falls back to legacy GDI render if DrawArea hasn’t been ported fully yet.

        /// </summary>

        public byte[] ExportPng()

        {

            // Preferred path: ask DrawArea to render to an SKCanvas if it exposes one

            if (TrySkiaExport(out var pngBytes))

                return pngBytes;

 

            // Fallback: use existing GDI-based drawing the same way your old code did

            using var bmp = new System.Drawing.Bitmap(_drawArea.Width, _drawArea.Height);

            using (var g = System.Drawing.Graphics.FromImage(bmp))

            {

                g.Clear(System.Drawing.Color.White);

                _drawArea.TheLayers.Draw(g);

            }

            using var ms = new System.IO.MemoryStream();

            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

            return ms.ToArray();

        }

 

        private bool TrySkiaExport([NotNullWhen(true)] out byte[]? pngBytes)

        {

            pngBytes = null;

 

            // If you add a DrawArea method like: void Draw(SKCanvas canvas);

            // you can render the exact same scene via Skia here.

            if (_drawArea is null || !_drawArea.TryGetBounds(out var w, out var h))

                return false;

 

            if (w <= 0 || h <= 0) return false;

 

            using var surface = SKSurface.Create(new SKImageInfo(w, h, SKColorType.Bgra8888, SKAlphaType.Premul));

            var canvas = surface.Canvas;

            canvas.Clear(SKColors.White);

 

            // TODO: call a Skia renderer on DrawArea once it exists:

            // _drawArea.Draw(canvas);

 

            // Without a Skia renderer yet, return false so we hit the GDI fallback.

            return false;

        }

 

        #endregion

 

        #region ToolStrip/Menu event handlers (unchanged behaviours)

 

        private void toolStripButtonNew_Click(object sender, EventArgs e)  => Clear(clearHistory: true);

        private void toolStripButtonOpen_Click(object sender, EventArgs e) => CommandOpen();

        private void toolStripButtonSave_Click(object sender, EventArgs e) => CommandSave();

 

        private void toolStripButtonPointer_Click(object sender, EventArgs e)   => CommandPointer();

        private void toolStripButtonRectangle_Click(object sender, EventArgs e) => CommandRectangle();

        private void toolStripButtonEllipse_Click(object sender, EventArgs e)   => CommandEllipse();

        private void toolStripButtonLine_Click(object sender, EventArgs e)      => CommandLine();

        private void toolStripButtonPencil_Click(object sender, EventArgs e)    => CommandPolygon();

        private void toolStripButtonAbout_Click(object sender, EventArgs e)     => CommandAbout();

        private void toolStripButtonUndo_Click(object sender, EventArgs e)      => CommandUndo();

        private void toolStripButtonRedo_Click(object sender, EventArgs e)      => CommandRedo();

 

        private void tsbPanMode_Click(object sender, EventArgs e)

        {

            _panMode = !_panMode;

            if (_drawArea is not null) _drawArea.Panning = _panMode;

            SetStateOfControls();

        }

 

        // Menu items mapped to same commands

        private void newToolStripMenuItem_Click(object sender, EventArgs e)        => Clear(clearHistory: true);

        private void openToolStripMenuItem_Click(object sender, EventArgs e)       => CommandOpen();

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)       => CommandSave();

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)     => CommandSaveAs();

        private void pointerToolStripMenuItem_Click(object sender, EventArgs e)    => CommandPointer();

        private void rectangleToolStripMenuItem_Click(object sender, EventArgs e)  => CommandRectangle();

        private void ellipseToolStripMenuItem_Click(object sender, EventArgs e)    => CommandEllipse();

        private void lineToolStripMenuItem_Click(object sender, EventArgs e)       => CommandLine();

        private void pencilToolStripMenuItem_Click(object sender, EventArgs e)     => CommandPolygon();

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)      => CommandAbout();

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)       => CommandUndo();

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)       => CommandRedo();

 

        #endregion

 

        #region File commands (stubs you can wire to your new persistence)

 

        private void CommandOpen()

        {

            // hook your new open-file provider here (removed DocManager path)

            using var ofd = new OpenFileDialog { Filter = "PNG|*.png|Bitmap|*.bmp|All files|*.*" };

            if (ofd.ShowDialog(this) == DialogResult.OK)

            {

                // TODO: load into _drawArea via your new pipeline

            }

        }

 

        private void CommandSave()   => CommandSaveAs();

        private void CommandSaveAs()

        {

            var bytes = ExportPng();

            using var sfd = new SaveFileDialog { Filter = "PNG|*.png" };

            if (sfd.ShowDialog(this) == DialogResult.OK)

            {

                System.IO.File.WriteAllBytes(sfd.FileName, bytes);

            }

        }

 

        private void Clear(bool clearHistory)

        {

            if (_drawArea is null) return;

            var i = _drawArea.TheLayers.ActiveLayerIndex;

            if (_drawArea.TheLayers[i].Graphics.Clear())

            {

                _drawArea.Refresh();

                if (clearHistory) _drawArea.ClearHistory();

            }

        }

 

        #endregion

    }

}
