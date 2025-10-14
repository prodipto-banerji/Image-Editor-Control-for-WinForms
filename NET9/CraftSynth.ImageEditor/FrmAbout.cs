using System;
using System.Windows.Forms;

namespace CraftSynth.ImageEditor
{
    internal partial class FrmAbout : Form
    {
        private bool _disposed = false;

        public FrmAbout()
        {
            InitializeComponent();
        }

        #region Destruction
        protected override void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    // Free any managed objects here.
                    // For this simple dialog, the designer-generated components
                    // are handled by the base class disposal
                }

                // Free any unmanaged objects here.
                this._disposed = true;
            }
            base.Dispose(disposing);
        }

        ~FrmAbout()
        {
            this.Dispose(false);
        }
        #endregion

        private void FrmAbout_Load(object sender, EventArgs e)
        {
            Text = "About " + Application.ProductName;

            lblText.Text = "Program: " + Application.ProductName + "\n" +
                          "Version: " + Application.ProductVersion + "\n\n" +
                          "Migrated to .NET 9 with SkiaSharp\n" +
                          "Cross-platform image editing control";
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}