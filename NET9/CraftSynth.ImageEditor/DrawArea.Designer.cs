using System.ComponentModel;
using System.Windows.Forms;
using SkiaSharp;

namespace CraftSynth.ImageEditor
{
    partial class DrawArea
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private IContainer? components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // DrawArea
            // 
            BackColor = System.Drawing.Color.White; // Note: BackColor still uses System.Drawing.Color as it's a WinForms property
            BorderStyle = BorderStyle.FixedSingle;
            Name = "DrawArea";
            Size = new System.Drawing.Size(100, 100); // Note: Size still uses System.Drawing.Size as it's a WinForms property
            Paint += DrawArea_Paint;
            MouseDown += DrawArea_MouseDown;
            MouseMove += DrawArea_MouseMove;
            MouseUp += DrawArea_MouseUp;
            ResumeLayout(false);
        }

        #endregion
    }
}