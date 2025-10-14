using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using SkiaSharp;

namespace CraftSynth.ImageEditor.Gui
{
    public partial class Form1 : Form
    {
        private readonly CraftSynth.ImageEditor.MainForm imageEditor1;

        public Form1()
        {
            InitializeComponent();
            imageEditor1 = new CraftSynth.ImageEditor.MainForm { Dock = DockStyle.Fill };
            Controls.Add(imageEditor1);
        }

        private void tsmiImport_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog { Filter = "Images|*.bmp;*.jpg;*.jpeg;*.png;*.gif" };
            if (ofd.ShowDialog(this) == DialogResult.OK)
            {
                using var data = SKData.Create(ofd.FileName);
                using var img = SKImage.FromEncodedData(data);
                using var bitmap = SKBitmap.FromImage(img);
                imageEditor1.ReplaceInitialImage(bitmap, preserveSize: false, addNewIfNotFound: true);
            }
        }

        private void tsmiExport_Click(object sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog { Filter = "Png|*.png|Jpg|*.jpg|Bmp|*.bmp|Gif|*.gif" };
            if (sfd.ShowDialog(this) == DialogResult.OK)
            {
                imageEditor1.ExportToFile(sfd.FileName);
            }
        }

        private void tsmiExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void tsmiAbout_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = "http://www.f4cio.com/Image-Editor-Control-For-WinForms"
            });
        }
    }
}
