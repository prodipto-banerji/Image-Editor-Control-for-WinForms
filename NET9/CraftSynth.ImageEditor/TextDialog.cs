using System;
using System.Windows.Forms;
using SkiaSharp;

namespace CraftSynth.ImageEditor
{
    public partial class TextDialog : Form
    {
        public TextDialog()
        {
            InitializeComponent();
        }

        private string _text = string.Empty;

        public string TheText
        {
            get { return _text; }
            set { _text = value ?? string.Empty; }
        }

        private SKTypeface? _typeface;
        private float _fontSize = 12f;
        private SKFontStyle _fontStyle = SKFontStyle.Normal;

        public SKTypeface? TheTypeface
        {
            get { return _typeface; }
            set { _typeface = value; }
        }

        public float TheFontSize
        {
            get { return _fontSize; }
            set { _fontSize = value; }
        }

        public SKFontStyle TheFontStyle
        {
            get { return _fontStyle; }
            set { _fontStyle = value; }
        }

        private SKColor _color = SKColors.Black;

        public SKColor TheColor
        {
            get { return _color; }
            set { _color = value; }
        }

        private float _zoom = 1f;

        public float Zoom
        {
            get { return _zoom; }
            set { _zoom = value; }
        }

        private void TextDialog_Load(object sender, EventArgs e)
        {
            this.Height = this.txtTheText.Height + 100;
            
            // Set the display font based on SkiaSharp properties
            UpdateDisplayFont();
            
            // Set the text color - convert SKColor to System.Drawing.Color for WinForms controls
            this.txtTheText.ForeColor = SKColorToSystemColor(_color);
            this.txtTheText.Text = _text;
            this.txtTheText.SelectAll();
            this.Height = this.txtTheText.Height + 100;
        }

        private void btnFont_Click(object sender, EventArgs e)
        {
            // Convert SkiaSharp font properties to System.Drawing.Font for the FontDialog
            using (var systemFont = SKFontToSystemFont())
            {
                dlgFont.Font = systemFont;
                dlgFont.Color = SKColorToSystemColor(_color);
                dlgFont.AllowSimulations = true;
                dlgFont.AllowVectorFonts = true;
                dlgFont.AllowVerticalFonts = true;
                dlgFont.MaxSize = 200;
                dlgFont.MinSize = 4;
                dlgFont.ShowApply = false;
                dlgFont.ShowColor = true;
                dlgFont.ShowEffects = true;
                
                if (dlgFont.ShowDialog() == DialogResult.OK)
                {
                    // Convert System.Drawing.Font back to SkiaSharp properties
                    SystemFontToSKFont(dlgFont.Font);
                    _color = SystemColorToSKColor(dlgFont.Color);
                    
                    UpdateDisplayFont();
                    txtTheText.ForeColor = SKColorToSystemColor(_color);
                    this.Height = this.txtTheText.Height + 100;
                }
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            _text = txtTheText.Text;
        }

        private void TextDialog_ResizeEnd(object sender, EventArgs e)
        {
            this.Height = this.txtTheText.Height + 100;
        }

        #region Helper Methods

        /// <summary>
        /// Update the display font in the text box based on SkiaSharp properties
        /// </summary>
        private void UpdateDisplayFont()
        {
            try
            {
                using (var systemFont = SKFontToSystemFont())
                {
                    this.txtTheText.Font = new System.Drawing.Font(
                        systemFont.FontFamily, 
                        systemFont.Size * this.Zoom, 
                        systemFont.Style);
                }
            }
            catch
            {
                // Fallback to default font if conversion fails
                this.txtTheText.Font = new System.Drawing.Font(
                    System.Drawing.FontFamily.GenericSansSerif, 
                    12f * this.Zoom, 
                    System.Drawing.FontStyle.Regular);
            }
        }

        /// <summary>
        /// Convert SkiaSharp font properties to System.Drawing.Font
        /// </summary>
        /// <returns>System.Drawing.Font equivalent</returns>
        private System.Drawing.Font SKFontToSystemFont()
        {
            string familyName = _typeface?.FamilyName ?? "Arial";
            float size = _fontSize;
            System.Drawing.FontStyle style = SKFontStyleToSystemFontStyle(_fontStyle);

            try
            {
                return new System.Drawing.Font(familyName, size, style);
            }
            catch
            {
                // Fallback to default font
                return new System.Drawing.Font(System.Drawing.FontFamily.GenericSansSerif, size, style);
            }
        }

        /// <summary>
        /// Convert System.Drawing.Font to SkiaSharp font properties
        /// </summary>
        /// <param name="font">System.Drawing.Font to convert</param>
        private void SystemFontToSKFont(System.Drawing.Font font)
        {
            _typeface?.Dispose();
            _typeface = SKTypeface.FromFamilyName(font.Name);
            _fontSize = font.Size;
            _fontStyle = SystemFontStyleToSKFontStyle(font.Style);
        }

        /// <summary>
        /// Convert SKFontStyle to System.Drawing.FontStyle
        /// </summary>
        /// <param name="skFontStyle">SkiaSharp font style</param>
        /// <returns>System.Drawing.FontStyle equivalent</returns>
        private System.Drawing.FontStyle SKFontStyleToSystemFontStyle(SKFontStyle skFontStyle)
        {
            System.Drawing.FontStyle style = System.Drawing.FontStyle.Regular;

            if (skFontStyle.Weight >= SKFontStyleWeight.SemiBold)
                style |= System.Drawing.FontStyle.Bold;

            if (skFontStyle.Slant != SKFontStyleSlant.Upright)
                style |= System.Drawing.FontStyle.Italic;

            // Note: SKFontStyle doesn't have direct underline/strikeout equivalents
            // These would need to be handled separately in the drawing logic

            return style;
        }

        /// <summary>
        /// Convert System.Drawing.FontStyle to SKFontStyle
        /// </summary>
        /// <param name="systemFontStyle">System.Drawing.FontStyle to convert</param>
        /// <returns>SKFontStyle equivalent</returns>
        private SKFontStyle SystemFontStyleToSKFontStyle(System.Drawing.FontStyle systemFontStyle)
        {
            SKFontStyleWeight weight = systemFontStyle.HasFlag(System.Drawing.FontStyle.Bold) 
                ? SKFontStyleWeight.Bold 
                : SKFontStyleWeight.Normal;

            SKFontStyleSlant slant = systemFontStyle.HasFlag(System.Drawing.FontStyle.Italic) 
                ? SKFontStyleSlant.Italic 
                : SKFontStyleSlant.Upright;

            return new SKFontStyle(weight, SKFontStyleWidth.Normal, slant);
        }

        /// <summary>
        /// Convert SKColor to System.Drawing.Color
        /// </summary>
        /// <param name="skColor">SkiaSharp color</param>
        /// <returns>System.Drawing.Color equivalent</returns>
        private System.Drawing.Color SKColorToSystemColor(SKColor skColor)
        {
            return System.Drawing.Color.FromArgb(skColor.Alpha, skColor.Red, skColor.Green, skColor.Blue);
        }

        /// <summary>
        /// Convert System.Drawing.Color to SKColor
        /// </summary>
        /// <param name="systemColor">System.Drawing.Color</param>
        /// <returns>SKColor equivalent</returns>
        private SKColor SystemColorToSKColor(System.Drawing.Color systemColor)
        {
            return new SKColor(systemColor.R, systemColor.G, systemColor.B, systemColor.A);
        }

        #endregion

        #region IDisposable Implementation

        private bool _disposed = false;

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    _typeface?.Dispose();
                    _typeface = null;
                }

                _disposed = true;
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}