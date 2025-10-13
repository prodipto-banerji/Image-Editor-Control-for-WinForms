using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace CraftSynth.ImageEditor
{
    /// <summary>
    /// Image tool (NET9 version using ImageSharp helper for decoding/encoding)
    /// </summary>
    internal class ToolImage : ToolObject
    {
        public ToolImage()
        {
            Cursor = new Cursor(GetType(), "Rectangle.cur");
        }

        public override void OnMouseDown(DrawArea drawArea, MouseEventArgs e)
        {
            Point p = drawArea.BackTrackMouse(new Point(e.X, e.Y));
            AddNewObject(drawArea, new DrawImage(p.X, p.Y, false));
        }

        public override void OnMouseMove(DrawArea drawArea, MouseEventArgs e)
        {
            drawArea.Cursor = Cursor;

            if (e.Button == MouseButtons.Left)
            {
                Point point = drawArea.BackTrackMouse(new Point(e.X, e.Y));
                int al = drawArea.TheLayers.ActiveLayerIndex;
                drawArea.TheLayers[al].Graphics[0].MoveHandleTo(point, 5);
                drawArea.Refresh();
            }
        }

        public override void OnMouseUp(DrawArea drawArea, MouseEventArgs e)
        {
            using OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "Select an Image to insert into map",
                Filter = "Bitmap (*.bmp)|*.bmp|JPEG (*.jpg)|*.jpg|PNG (*.png)|*.png|GIF (*.gif)|*.gif|Icon (*.ico)|*.ico|All files|*.*",
                FilterIndex = 6,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
            };
            int al = drawArea.TheLayers.ActiveLayerIndex;
            while (true)
            {
                var dlgResult = ofd.ShowDialog();
                if (dlgResult != DialogResult.OK)
                {
                    drawArea.TheLayers[al].Graphics.RemoveAt(0);
                    break;
                }
                else
                {
                    try
                    {
                        using var bmp = ImageSharpHelper.LoadBitmapFromFile(ofd.FileName);
                        ((DrawImage)drawArea.TheLayers[al].Graphics[0]).TheImage = (Bitmap)bmp.Clone();
                        drawArea.AddCommandToHistory(new CommandAdd(drawArea.TheLayers[al].Graphics[0]));
                        break;
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Can not load file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            base.OnMouseUp(drawArea, e);
        }

        #region initial image loading
        public void InsertImage(DrawArea drawArea, string filePath, bool moveToBack, bool isInitialImage, DrawImage paradigm)
        {
            using var bmp = ImageSharpHelper.LoadBitmapFromFile(filePath);
            byte[] bytes;
            using (MemoryStream ms = new MemoryStream())
            {
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                bytes = ms.ToArray();
            }
            InsertImage(drawArea, bytes, moveToBack, isInitialImage, paradigm);
        }

        public static byte[] StreamToBytes(Stream input, bool streamDoesntChange = true)
        {
            using (input)
            {
                input.Seek(0, SeekOrigin.Begin);
                byte[] buffer = streamDoesntChange ? new byte[input.Length] : new byte[16 * 1024];
                using MemoryStream ms = new MemoryStream();
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public void InsertImage(DrawArea drawArea, byte[] bytes, bool moveToBack, bool isInitialImage, DrawImage paradigm)
        {
            using var bmp = ImageSharpHelper.LoadBitmapFromBytes(bytes);
            InsertImage(drawArea, (Image)bmp, moveToBack, isInitialImage, paradigm);
        }

        public void InsertImage(DrawArea drawArea, Image image, bool moveToBack, bool isInitialImage, DrawImage paradigm)
        {
            if (paradigm == null)
            {
                paradigm = new DrawImage(0, 0, isInitialImage);
            }
            AddNewObject(drawArea, paradigm);
            int al = drawArea.TheLayers.ActiveLayerIndex;
            drawArea.TheLayers[al].Graphics[0].MoveHandleTo(new Point(image.Width, image.Height), 5);
            ((DrawImage)drawArea.TheLayers[al].Graphics[0]).TheImage = (Bitmap)image;
            drawArea.AddCommandToHistory(new CommandAdd(drawArea.TheLayers[al].Graphics[0]));
            if (moveToBack)
            {
                drawArea.TheLayers[al].Graphics.MoveSelectionToBack();
            }
            drawArea.TheLayers[al].Graphics.UnselectAll();
        }
        #endregion
    }
}
