using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WindowsForms;

namespace CraftSynth.ImageEditor
{
    /// <summary>
    /// SkiaSharp-based MainForm control hosting a canvas and tools.
    /// </summary>
    public class MainForm : UserControl
    {
        private readonly SKControl _skiaView;
        private Layers _layers;

        public MainForm()
        {
            _layers = new Layers();
            _layers.CreateNewLayer("Default");

            _skiaView = new SKControl { Dock = DockStyle.Fill }; // WindowsForms view; for cross-platform use Views.Desktop
            _skiaView.PaintSurface += SkiaView_PaintSurface;
            Controls.Add(_skiaView);

            LineColor = SKColors.Red;
            FillColor = SKColors.White;
            LineWidth = 5f;
            PenType = DrawingPens.PenType.Solid;
        }

        private void SkiaView_PaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.White);
            _layers.Draw(canvas);
        }

        public Layers TheLayers
        {
            get => _layers;
            set => _layers = value;
        }

        public SKColor LineColor { get; set; }
        public SKColor FillColor { get; set; }
        public float LineWidth { get; set; }
        public DrawingPens.PenType PenType { get; set; }

        public void RefreshCanvas() => _skiaView.Invalidate();

        public void ExportToFile(string filePath)
        {
            var width = Math.Max(1, _skiaView.Width);
            var height = Math.Max(1, _skiaView.Height);
            using var bmp = new SKBitmap(width, height);
            using var surface = SKSurface.Create(new SKImageInfo(width, height));
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.White);
            _layers.Draw(canvas);
            using var image = surface.Snapshot();
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            SKEncodedImageFormat fmt = SKEncodedImageFormat.Png;
            if (ext == ".jpg" || ext == ".jpeg") fmt = SKEncodedImageFormat.Jpeg;
            else if (ext == ".gif") fmt = SKEncodedImageFormat.Gif;
            else if (ext == ".bmp") fmt = SKEncodedImageFormat.Bmp;
            using var data = image.Encode(fmt, 90);
            using var fs = File.Open(filePath, FileMode.Create, FileAccess.Write);
            data.SaveTo(fs);
        }

        public void ReplaceInitialImage(SKBitmap image, bool preserveSize, bool addNewIfNotFound)
        {
            var initial = GetInitialImageGraphic();
            if (initial != null)
            {
                var pair = initial.Value;
                if (!preserveSize)
                {
                    pair.Value.rectangle.Right = pair.Value.rectangle.Left + image.Width;
                    pair.Value.rectangle.Bottom = pair.Value.rectangle.Top + image.Height;
                }
                pair.Value.TheImage = image;
                RefreshCanvas();
            }
            else if (addNewIfNotFound)
            {
                InsertInitialImage(image);
                RefreshCanvas();
            }
        }

        private void InsertInitialImage(SKBitmap image)
        {
            var di = new DrawImage(0, 0, image) { IsInitialImage = true };
            _layers[0].Graphics.AddAsInitialGraphic(di);
        }

        public KeyValuePair<int, DrawImage>? GetInitialImageGraphic()
        {
            for (int i = _layers[0].Graphics.Count - 1; i >= 0; i--)
            {
                if (_layers[0].Graphics[i] is DrawImage di && di.IsInitialImage)
                {
                    return new KeyValuePair<int, DrawImage>(i, di);
                }
            }
            return null;
        }
    }
}
