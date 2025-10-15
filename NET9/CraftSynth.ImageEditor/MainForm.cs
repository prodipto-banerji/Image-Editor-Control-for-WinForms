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

 

 

 

 

 

 

 

 

 

 

 

 

namespace CraftSynth.ImageEditor

{

    partial class MainForm

    {

        private System.ComponentModel.IContainer components = null;

 

        private System.Windows.Forms.ToolStrip toolStrip1;

        private System.Windows.Forms.ToolStripButton toolStripButtonPointer;

        private System.Windows.Forms.ToolStripButton tsbPanMode;

        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;

        private System.Windows.Forms.ToolStripButton toolStripButtonUndo;

        private System.Windows.Forms.ToolStripButton toolStripButtonRedo;

        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;

        private System.Windows.Forms.ToolStripButton toolStripButtonPencil;

        private System.Windows.Forms.ToolStripButton toolStripButtonArrow;

        private System.Windows.Forms.ToolStripButton toolStripButtonLine;

        private System.Windows.Forms.ToolStripButton toolStripButtonRectangle;

        private System.Windows.Forms.ToolStripButton tsbFilledRectangle;

        private System.Windows.Forms.ToolStripButton toolStripButtonEllipse;

        private System.Windows.Forms.ToolStripButton tsbFilledEllipse;

        private System.Windows.Forms.ToolStripButton tsbText;

        private System.Windows.Forms.ToolStripButton tsbImage;

        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;

        private System.Windows.Forms.ToolStripButton tsbLineColor;

        private System.Windows.Forms.ToolStripButton tsbFillColor;

        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;

        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButtonPenType;

        private System.Windows.Forms.ToolStripMenuItem solidToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem dottedToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem dashedToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem dotDashedtoolStripMenuItem7;

        private System.Windows.Forms.ToolStripMenuItem doubleLineToolStripMenuItem8;

        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButtonLineThickness;

        private System.Windows.Forms.ToolStripMenuItem thinnestToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem thinToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem mediumToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem thickToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem thickestToolStripMenuItem;

        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;

        private System.Windows.Forms.ToolStripButton toolStripButtonAbout;

        private System.Windows.Forms.ToolStripButton tsbZoomIn;

        private System.Windows.Forms.ToolStripButton tsbZoomReset;

        private System.Windows.Forms.ToolStripButton tsbZoomOut;

        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;

        private System.Windows.Forms.ToolStripButton tsbRotateLeft;

        private System.Windows.Forms.ToolStripButton tsbRotateRight;

        private System.Windows.Forms.ContextMenuStrip ctxtMenu;

        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem8;

        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;

        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem7;

        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;

        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem10;

        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem11;

        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;

        private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem2;

        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem13;

        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem6;

        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;

        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;

        private System.Windows.Forms.ToolStripMenuItem recentFilesToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem unselectAllToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem deleteAllToolStripMenuItem;

        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;

        private System.Windows.Forms.ToolStripMenuItem moveToFrontToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem moveToBackToolStripMenuItem;

        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;

        private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;

        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;

        private System.Windows.Forms.ToolStripMenuItem propertiesToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem drawToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem pointerToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem rectangleToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem ellipseToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem lineToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem pencilToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;

        private CustomPanel pnlDrawArea;

        private System.Windows.Forms.ColorDialog dlgColor;

 

        private void InitializeComponent()

        {

            this.components = new System.ComponentModel.Container();

            var resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));

            this.toolStrip1 = new System.Windows.Forms.ToolStrip();

            this.toolStripButtonPointer = new System.Windows.Forms.ToolStripButton();

            this.tsbPanMode = new System.Windows.Forms.ToolStripButton();

            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();

            this.toolStripButtonUndo = new System.Windows.Forms.ToolStripButton();

            this.toolStripButtonRedo = new System.Windows.Forms.ToolStripButton();

            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();

            this.toolStripButtonPencil = new System.Windows.Forms.ToolStripButton();

            this.toolStripButtonArrow = new System.Windows.Forms.ToolStripButton();

            this.toolStripButtonLine = new System.Windows.Forms.ToolStripButton();

            this.toolStripButtonRectangle = new System.Windows.Forms.ToolStripButton();

            this.tsbFilledRectangle = new System.Windows.Forms.ToolStripButton();

            this.toolStripButtonEllipse = new System.Windows.Forms.ToolStripButton();

            this.tsbFilledEllipse = new System.Windows.Forms.ToolStripButton();

            this.tsbText = new System.Windows.Forms.ToolStripButton();

            this.tsbImage = new System.Windows.Forms.ToolStripButton();

            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();

            this.tsbLineColor = new System.Windows.Forms.ToolStripButton();

            this.tsbFillColor = new System.Windows.Forms.ToolStripButton();

            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();

            this.toolStripDropDownButtonPenType = new System.Windows.Forms.ToolStripDropDownButton();

            this.solidToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.dottedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.dashedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.dotDashedtoolStripMenuItem7 = new System.Windows.Forms.ToolStripMenuItem();

            this.doubleLineToolStripMenuItem8 = new System.Windows.Forms.ToolStripMenuItem();

            this.toolStripDropDownButtonLineThickness = new System.Windows.Forms.ToolStripDropDownButton();

            this.thinnestToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.thinToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.mediumToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.thickToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.thickestToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();

            this.toolStripButtonAbout = new System.Windows.Forms.ToolStripButton();

            this.tsbZoomIn = new System.Windows.Forms.ToolStripButton();

            this.tsbZoomReset = new System.Windows.Forms.ToolStripButton();

            this.tsbZoomOut = new System.Windows.Forms.ToolStripButton();

            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();

            this.tsbRotateLeft = new System.Windows.Forms.ToolStripButton();

            this.tsbRotateRight = new System.Windows.Forms.ToolStripButton();

            this.ctxtMenu = new System.Windows.Forms.ContextMenuStrip(this.components);

            this.toolStripMenuItem8 = new System.Windows.Forms.ToolStripMenuItem();

            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();

            this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripMenuItem();

            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();

            this.toolStripMenuItem10 = new System.Windows.Forms.ToolStripMenuItem();

            this.toolStripMenuItem11 = new System.Windows.Forms.ToolStripMenuItem();

            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();

            this.undoToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();

            this.toolStripMenuItem13 = new System.Windows.Forms.ToolStripMenuItem();

            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripSeparator();

            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();

            this.recentFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.unselectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.deleteAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();

            this.moveToFrontToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.moveToBackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();

            this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();

            this.propertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.drawToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.pointerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.rectangleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.ellipseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.lineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.pencilToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.pnlDrawArea = new CraftSynth.ImageEditor.CustomPanel();

 

            this.toolStrip1.SuspendLayout();

            this.ctxtMenu.SuspendLayout();

            this.SuspendLayout();

 

            // toolStrip1

            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;

            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);

            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {

                this.toolStripButtonPointer,

                this.tsbPanMode,

                this.toolStripSeparator5,

                this.toolStripButtonUndo,

                this.toolStripButtonRedo,

                this.toolStripSeparator3,

                this.toolStripButtonPencil,

                this.toolStripButtonArrow,

                this.toolStripButtonLine,

                this.toolStripButtonRectangle,

                this.tsbFilledRectangle,

                this.toolStripButtonEllipse,

                this.tsbFilledEllipse,

                this.tsbText,

                this.tsbImage,

                this.toolStripSeparator2,

                this.tsbLineColor,

                this.tsbFillColor,

                this.toolStripSeparator6,

                this.toolStripDropDownButtonPenType,

                this.toolStripDropDownButtonLineThickness,

                this.toolStripSeparator1,

                this.toolStripButtonAbout,

                this.tsbZoomIn,

                this.tsbZoomReset,

                this.tsbZoomOut,

                this.toolStripSeparator4,

                this.tsbRotateLeft,

                this.tsbRotateRight

            });

            this.toolStrip1.Location = new System.Drawing.Point(0, 0);

            this.toolStrip1.Name = "toolStrip1";

            this.toolStrip1.Size = new System.Drawing.Size(900, 30);

            this.toolStrip1.TabIndex = 1;

            this.toolStrip1.Text = "toolStrip1";

 

            // individual buttons (icons/resources preserved as in your current Designer)

            this.toolStripButtonPointer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;

            this.toolStripButtonPointer.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonPointer.Image")));

            this.toolStripButtonPointer.ImageTransparentColor = System.Drawing.Color.Silver;

            this.toolStripButtonPointer.Click += new System.EventHandler(this.toolStripButtonPointer_Click);

 

            this.tsbPanMode.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;

            this.tsbPanMode.Image = ((System.Drawing.Image)(resources.GetObject("tsbPanMode.Image")));

            this.tsbPanMode.ImageTransparentColor = System.Drawing.Color.Magenta;

            this.tsbPanMode.Click += new System.EventHandler(this.tsbPanMode_Click);

 

            this.toolStripButtonUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;

            this.toolStripButtonUndo.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonUndo.Image")));

            this.toolStripButtonUndo.ImageTransparentColor = System.Drawing.Color.Silver;

            this.toolStripButtonUndo.Click += new System.EventHandler(this.toolStripButtonUndo_Click);

 

            this.toolStripButtonRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;

            this.toolStripButtonRedo.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonRedo.Image")));

            this.toolStripButtonRedo.ImageTransparentColor = System.Drawing.Color.Silver;

            this.toolStripButtonRedo.Click += new System.EventHandler(this.toolStripButtonRedo_Click);

 

            this.toolStripButtonPencil.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;

            this.toolStripButtonPencil.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonPencil.Image")));

            this.toolStripButtonPencil.ImageTransparentColor = System.Drawing.Color.Silver;

            this.toolStripButtonPencil.Click += new System.EventHandler(this.toolStripButtonPencil_Click);

 

            this.toolStripButtonArrow.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;

            this.toolStripButtonArrow.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonArrow.Image")));

            this.toolStripButtonArrow.ImageTransparentColor = System.Drawing.Color.Silver;

            this.toolStripButtonArrow.Click += new System.EventHandler(this.toolStripButtonArrow_Click);

 

            this.toolStripButtonLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;

            this.toolStripButtonLine.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonLine.Image")));

            this.toolStripButtonLine.ImageTransparentColor = System.Drawing.Color.Silver;

            this.toolStripButtonLine.Click += new System.EventHandler(this.toolStripButtonLine_Click);

 

            this.toolStripButtonRectangle.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;

            this.toolStripButtonRectangle.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonRectangle.Image")));

            this.toolStripButtonRectangle.ImageTransparentColor = System.Drawing.Color.Silver;

            this.toolStripButtonRectangle.Click += new System.EventHandler(this.toolStripButtonRectangle_Click);

 

            this.tsbFilledRectangle.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;

            this.tsbFilledRectangle.Image = ((System.Drawing.Image)(resources.GetObject("tsbFilledRectangle.Image")));

            this.tsbFilledRectangle.ImageTransparentColor = System.Drawing.Color.Magenta;

 

            this.toolStripButtonEllipse.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;

            this.toolStripButtonEllipse.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonEllipse.Image")));

            this.toolStripButtonEllipse.ImageTransparentColor = System.Drawing.Color.Silver;

            this.toolStripButtonEllipse.Click += new System.EventHandler(this.toolStripButtonEllipse_Click);

 

            this.tsbFilledEllipse.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;

            this.tsbFilledEllipse.Image = ((System.Drawing.Image)(resources.GetObject("tsbFilledEllipse.Image")));

            this.tsbFilledEllipse.ImageTransparentColor = System.Drawing.Color.Magenta;

 

            this.tsbText.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;

            this.tsbText.Image = ((System.Drawing.Image)(resources.GetObject("tsbText.Image")));

            this.tsbText.ImageTransparentColor = System.Drawing.Color.Magenta;

 

            this.tsbImage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;

            this.tsbImage.Image = ((System.Drawing.Image)(resources.GetObject("tsbImage.Image")));

            this.tsbImage.ImageTransparentColor = System.Drawing.Color.Magenta;

 

            this.tsbLineColor.AutoSize = false;

            this.tsbLineColor.BackColor = System.Drawing.Color.Black;

            this.tsbLineColor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.None;

            this.tsbLineColor.Click += new System.EventHandler(this.tsbSelectLineColor_Click);

 

            this.tsbFillColor.AutoSize = false;

            this.tsbFillColor.BackColor = System.Drawing.Color.White;

            this.tsbFillColor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.None;

            this.tsbFillColor.Click += new System.EventHandler(this.tsbSelectFillColor_Click);

 

            this.toolStripDropDownButtonPenType.AutoSize = false;

            this.toolStripDropDownButtonPenType.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;

            this.toolStripDropDownButtonPenType.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {

                this.solidToolStripMenuItem,

                this.dottedToolStripMenuItem,

                this.dashedToolStripMenuItem,

                this.dotDashedtoolStripMenuItem7,

                this.doubleLineToolStripMenuItem8

            });

 

            this.solidToolStripMenuItem.Click += new System.EventHandler(this.solidToolStripMenuItem_Click);

            this.dottedToolStripMenuItem.Click += new System.EventHandler(this.dottedToolStripMenuItem_Click);

            this.dashedToolStripMenuItem.Click += new System.EventHandler(this.dashedToolStripMenuItem_Click);

            this.dotDashedtoolStripMenuItem7.Click += new System.EventHandler(this.dotDashedtoolStripMenuItem7_Click);

            this.doubleLineToolStripMenuItem8.Visible = false;

            this.doubleLineToolStripMenuItem8.Click += new System.EventHandler(this.doubleLineToolStripMenuItem8_Click);

 

            this.toolStripDropDownButtonLineThickness.AutoSize = false;

            this.toolStripDropDownButtonLineThickness.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;

            this.toolStripDropDownButtonLineThickness.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {

                this.thinnestToolStripMenuItem,

                this.thinToolStripMenuItem,

                this.mediumToolStripMenuItem,

                this.thickToolStripMenuItem,

                this.thickestToolStripMenuItem

            });

            this.thinnestToolStripMenuItem.Click += new System.EventHandler(this.tsbLineThinnest_Click);

            this.thinToolStripMenuItem.Click += new System.EventHandler(this.tsbLineThin_Click);

            this.mediumToolStripMenuItem.Click += new System.EventHandler(this.tsbThickLine_Click);

            this.thickToolStripMenuItem.Click += new System.EventHandler(this.tsbThickerLine_Click);

            this.thickestToolStripMenuItem.Click += new System.EventHandler(this.tsbThickestLine_Click);

 

            this.toolStripButtonAbout.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;

            this.toolStripButtonAbout.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;

            this.toolStripButtonAbout.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonAbout.Image")));

            this.toolStripButtonAbout.ImageTransparentColor = System.Drawing.Color.Silver;

            this.toolStripButtonAbout.Visible = false;

            this.toolStripButtonAbout.Click += new System.EventHandler(this.toolStripButtonAbout_Click);

 

            this.tsbZoomIn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;

            this.tsbZoomIn.Image = ((System.Drawing.Image)(resources.GetObject("tsbZoomIn.Image")));

            this.tsbZoomIn.Click += new System.EventHandler(this.tsbZoomIn_Click);

 

            this.tsbZoomReset.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;

            this.tsbZoomReset.Image = ((System.Drawing.Image)(resources.GetObject("tsbZoomReset.Image")));

            this.tsbZoomReset.Click += new System.EventHandler(this.tsbZoomReset_Click);

 

            this.tsbZoomOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;

            this.tsbZoomOut.Image = ((System.Drawing.Image)(resources.GetObject("tsbZoomOut.Image")));

            this.tsbZoomOut.Click += new System.EventHandler(this.tsbZoomOut_Click);

 

            this.tsbRotateLeft.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;

            this.tsbRotateLeft.Image = ((System.Drawing.Image)(resources.GetObject("tsbRotateLeft.Image")));

            this.tsbRotateLeft.Click += new System.EventHandler(this.tsbRotateLeft_Click);

 

            this.tsbRotateRight.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;

            this.tsbRotateRight.Image = ((System.Drawing.Image)(resources.GetObject("tsbRotateRight.Image")));

            this.tsbRotateRight.Click += new System.EventHandler(this.tsbRotateRight_Click);

 

            // Context menu (kept for parity; wire items as needed)

            this.ctxtMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {

                this.toolStripMenuItem8,

                this.toolStripMenuItem2,

                this.toolStripMenuItem7,

                this.toolStripSeparator7,

                this.toolStripMenuItem10,

                this.toolStripMenuItem11,

                this.toolStripSeparator8,

                this.undoToolStripMenuItem2,

                this.toolStripMenuItem13,

                this.toolStripMenuItem6,

                this.fileToolStripMenuItem,

                this.editToolStripMenuItem,

                this.drawToolStripMenuItem,

                this.helpToolStripMenuItem

            });

 

            // Drawing host

            this.pnlDrawArea = new CraftSynth.ImageEditor.CustomPanel

            {

                Dock = System.Windows.Forms.DockStyle.Fill,

                BackColor = System.Drawing.Color.White

            };

 

            // MainForm (UserControl)

            this.Controls.Add(this.pnlDrawArea);

            this.Controls.Add(this.toolStrip1);

            this.Name = "MainForm";

            this.Size = new System.Drawing.Size(900, 600);

 

            // Hook lifecycle events

            this.Load += new System.EventHandler(this.MainForm_Load);

            this.Resize += new System.EventHandler(this.MainForm_Resize);

 

            this.toolStrip1.ResumeLayout(false);

            this.toolStrip1.PerformLayout();

            this.ctxtMenu.ResumeLayout(false);

            this.ResumeLayout(false);

            this.PerformLayout();

        }

    }

}
