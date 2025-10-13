using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;

namespace CraftSynth.ImageEditor.Gui
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.imageEditor1.ParentForm = this;

            // Load configuration from appsettings.json
            try
            {
                var config = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: true)
                    .Build();
                var zoomOnWheel = config["ImageEditor:ZoomOnMouseWheel"];
                if (bool.TryParse(zoomOnWheel, out var zoom))
                {
                    imageEditor1.ZoomOnMouseWheel = zoom;
                }
            }
            catch (Exception)
            {
                // Ignore config errors, keep defaults
            }
        }

        private void tsmiImport_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == this.openFileDialog1.ShowDialog(this))
            {
                using var bmp = CraftSynth.ImageEditor.ImageSharpHelper.LoadBitmapFromFile(this.openFileDialog1.FileName);
                this.imageEditor1.ReplaceInitialImage((Bitmap)bmp.Clone(), false, true);
            }
        }

        private void tsmiExport_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == this.saveFileDialog1.ShowDialog(this))
            {
                // Export via ImageSharp encoders based on extension
                using var img = (Bitmap)this.imageEditor1.ExportToImage();
                CraftSynth.ImageEditor.ImageSharpHelper.SaveBitmapToFile(img, this.saveFileDialog1.FileName);
            }
        }

        private void tsmiExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void tsmiAbout_Click(object sender, EventArgs e)
        {
            Process process = new Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.FileName = "http://www.f4cio.com/Image-Editor-Control-For-WinForms";
            process.Start();
        }
    }
}
