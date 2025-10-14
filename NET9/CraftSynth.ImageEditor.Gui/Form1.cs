using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using SkiaSharp;

namespace CraftSynth.ImageEditor.Gui
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.imageEditor1.ParentForm = this;
        }

        private void tsmiImport_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == this.openFileDialog1.ShowDialog(this))
            {
                // Load image using SkiaSharp instead of System.Drawing.Image
                using var fileStream = File.OpenRead(this.openFileDialog1.FileName);
                using var skBitmap = SKBitmap.Decode(fileStream);
                
                if (skBitmap != null)
                {
                    this.imageEditor1.ReplaceInitialImage(skBitmap, false, true);
                }
                else
                {
                    MessageBox.Show("Unable to load the selected image file.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void tsmiExport_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == this.saveFileDialog1.ShowDialog(this))
            {
                string ext = Path.GetExtension(this.saveFileDialog1.FileName).ToLower().TrimStart('.');
                SKEncodedImageFormat format = SKEncodedImageFormat.Png; // Default to PNG
                
                switch (ext)
                {
                    case "jpg":
                    case "jpeg":
                        format = SKEncodedImageFormat.Jpeg;
                        break;
                    case "png":
                        format = SKEncodedImageFormat.Png;
                        break;
                    case "gif":
                        format = SKEncodedImageFormat.Gif;
                        break;
                    case "bmp":
                        format = SKEncodedImageFormat.Bmp;
                        break;
                    case "webp":
                        format = SKEncodedImageFormat.Webp;
                        break;
                }
                
                try
                {
                    this.imageEditor1.ExportToFile(this.saveFileDialog1.FileName, format);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting image: {ex.Message}", "Export Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void tsmiExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void tsmiAbout_Click(object sender, EventArgs e)
        {
            try
            {
                Process process = new Process();
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.FileName = "http://www.f4cio.com/Image-Editor-Control-For-WinForms";
                process.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to open the URL: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}