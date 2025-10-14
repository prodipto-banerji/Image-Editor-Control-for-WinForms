using System;
using System.ComponentModel;
using System.Windows.Forms;
using SkiaSharp;

namespace CraftSynth.ImageEditor
{
    partial class PropertiesDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new Label();
            this.lblColor = new Label();
            this.btnSelectColor = new Button();
            this.label2 = new Label();
            this.lblPenWidth = new Label();
            this.cmbPenWidth = new ComboBox();
            this.btnOK = new Button();
            this.btnCancel = new Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Color";
            // 
            // lblColor
            // 
            this.lblColor.BorderStyle = BorderStyle.Fixed3D;
            this.lblColor.Location = new System.Drawing.Point(90, 21);
            this.lblColor.Name = "lblColor";
            this.lblColor.Size = new System.Drawing.Size(72, 24);
            this.lblColor.TabIndex = 1;
            this.lblColor.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnSelectColor
            // 
            this.btnSelectColor.Location = new System.Drawing.Point(195, 19);
            this.btnSelectColor.Name = "btnSelectColor";
            this.btnSelectColor.Size = new System.Drawing.Size(33, 27);
            this.btnSelectColor.TabIndex = 2;
            this.btnSelectColor.Text = "...";
            this.btnSelectColor.UseVisualStyleBackColor = true;
            this.btnSelectColor.Click += new EventHandler(this.btnSelectColor_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(22, 66);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Pen Width";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblPenWidth
            // 
            this.lblPenWidth.BorderStyle = BorderStyle.Fixed3D;
            this.lblPenWidth.Location = new System.Drawing.Point(90, 60);
            this.lblPenWidth.Name = "lblPenWidth";
            this.lblPenWidth.Size = new System.Drawing.Size(72, 24);
            this.lblPenWidth.TabIndex = 4;
            this.lblPenWidth.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cmbPenWidth
            // 
            this.cmbPenWidth.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbPenWidth.FormattingEnabled = true;
            this.cmbPenWidth.Location = new System.Drawing.Point(195, 61);
            this.cmbPenWidth.Name = "cmbPenWidth";
            this.cmbPenWidth.Size = new System.Drawing.Size(71, 21);
            this.cmbPenWidth.TabIndex = 5;
            this.cmbPenWidth.SelectedIndexChanged += new EventHandler(this.cmbPenWidth_SelectedIndexChanged);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(39, 114);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(80, 27);
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(175, 114);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 27);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // PropertiesDialog
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(290, 156);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.cmbPenWidth);
            this.Controls.Add(this.lblPenWidth);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnSelectColor);
            this.Controls.Add(this.lblColor);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PropertiesDialog";
            this.ShowIcon = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Properties";
            this.Load += new EventHandler(this.PropertiesDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private Label label1;
        private Label lblColor;
        private Button btnSelectColor;
        private Label label2;
        private Label lblPenWidth;
        private ComboBox cmbPenWidth;
        private Button btnOK;
        private Button btnCancel;
    }
}