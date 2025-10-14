using System;
using System.IO;
using System.Windows.Forms;
using SkiaSharp;

namespace CraftSynth.ImageEditor
{
    /// <summary>
    /// Image tool for .NET 9 with SkiaSharp support
    /// </summary>
    internal class ToolImage : ToolObject
    {
        public ToolImage()
        {
            Cursor = new Cursor(GetType(), "Rectangle.cur");
        }

        public override void OnMouseDown(DrawArea drawArea, MouseEventArgs e)
        {
            System.Drawing.Point p = drawArea.BackTrackMouse(new System.Drawing.Point(e.X, e.Y));
            AddNewObject(drawArea, new DrawImage(p.X, p.Y, false));
        }

        public override void OnMouseMove(DrawArea drawArea, MouseEventArgs e)
        {
            drawArea.Cursor = Cursor;

            if (e.Button == MouseButtons.Left)
            {
                System.Drawing.Point point = drawArea.BackTrackMouse(new System.Drawing.Point(e.X, e.Y));
                int al = drawArea.TheLayers.ActiveLayerIndex;
                drawArea.TheLayers[al].Graphics[0].MoveHandleTo(point, 5);
                drawArea.Refresh();
            }
        }

        public override void OnMouseUp(DrawArea drawArea, MouseEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select an Image to insert into map";
            ofd.Filter = "Bitmap (*.bmp)|*.bmp|JPEG (*.jpg)|*.jpg|PNG (*.png)|*.png|WebP (*.webp)|*.webp|GIF (*.gif)|*.gif|All files|*.*";
            ofd.FilterIndex = 6;
            ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
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
                        SKBitmap skBitmap = LoadImageFromFile(ofd.FileName);
                        ((DrawImage)drawArea.TheLayers[al].Graphics[0]).TheImage = skBitmap;
                        drawArea.AddCommandToHistory(new CommandAdd(drawArea.TheLayers[al].Graphics[0]));
                        break;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Can not load file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            ofd.Dispose();
            base.OnMouseUp(drawArea, e);
        }

        #region Image Loading Methods

        /// <summary>
        /// Load SKBitmap from file path
        /// </summary>
        /// <param name="filePath">Path to the image file</param>
        /// <returns>SKBitmap loaded from file</returns>
        private SKBitmap LoadImageFromFile(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            {
                return SKBitmap.Decode(stream);
            }
        }

        /// <summary>
        /// Insert image from file path
        /// </summary>
        public void InsertImage(DrawArea drawArea, string filePath, bool moveToBack, bool isInitialImage, DrawImage? paradigm)
        {
            var skBitmap = LoadImageFromFile(filePath);
            byte[] bytes = SkBitmapToBytes(skBitmap);
            InsertImage(drawArea, bytes, moveToBack, isInitialImage, paradigm);
        }

        /// <summary>
        /// Convert SKBitmap to byte array
        /// </summary>
        /// <param name="bitmap">SKBitmap to convert</param>
        /// <returns>Byte array representation of the bitmap</returns>
        private byte[] SkBitmapToBytes(SKBitmap bitmap)
        {
            using (var image = SKImage.FromBitmap(bitmap))
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            {
                return data.ToArray();
            }
        }

        /// <summary>
        /// Convert stream to byte array
        /// Source: http://stackoverflow.com/questions/221925/creating-a-byte-array-from-a-stream
        /// </summary>
        /// <param name="input">Input stream</param>
        /// <param name="streamDoesntChange">Whether stream content changes</param>
        /// <returns>Byte array from stream</returns>
        public static byte[] StreamToBytes(Stream input, bool streamDoesntChange = true)
        {
            using (input)
            {
                input.Seek(0, SeekOrigin.Begin);
                byte[] buffer = streamDoesntChange ? new byte[input.Length] : new byte[16 * 1024];
                using (MemoryStream ms = new MemoryStream())
                {
                    int read;
                    while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// Insert image from byte array
        /// </summary>
        public void InsertImage(DrawArea drawArea, byte[] bytes, bool moveToBack, bool isInitialImage, DrawImage? paradigm)
        {
            SKBitmap skBitmap = LoadImageFromBytes(bytes);
            InsertImage(drawArea, skBitmap, moveToBack, isInitialImage, paradigm);
        }

        /// <summary>
        /// Load SKBitmap from byte array
        /// </summary>
        /// <param name="bytes">Byte array containing image data</param>
        /// <returns>SKBitmap loaded from bytes</returns>
        private SKBitmap LoadImageFromBytes(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                return SKBitmap.Decode(stream);
            }
        }

        /// <summary>
        /// Insert image from System.Drawing.Image (for backward compatibility)
        /// </summary>
        public void InsertImage(DrawArea drawArea, System.Drawing.Image image, bool moveToBack, bool isInitialImage, DrawImage? paradigm)
        {
            // Convert System.Drawing.Image to SKBitmap
            SKBitmap skBitmap = ConvertSystemDrawingImageToSkBitmap(image);
            InsertImage(drawArea, skBitmap, moveToBack, isInitialImage, paradigm);
        }

        /// <summary>
        /// Convert System.Drawing.Image to SKBitmap for backward compatibility
        /// </summary>
        /// <param name="image">System.Drawing.Image to convert</param>
        /// <returns>SKBitmap equivalent</returns>
        private SKBitmap ConvertSystemDrawingImageToSkBitmap(System.Drawing.Image image)
        {
            using (var memoryStream = new MemoryStream())
            {
                // Save System.Drawing.Image to memory stream as PNG
                image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                memoryStream.Seek(0, SeekOrigin.Begin);
                
                // Load SKBitmap from the stream
                return SKBitmap.Decode(memoryStream);
            }
        }

        /// <summary>
        /// Insert SKBitmap image into the drawing area
        /// </summary>
        public void InsertImage(DrawArea drawArea, SKBitmap image, bool moveToBack, bool isInitialImage, DrawImage? paradigm)
        {
            if (paradigm == null)
            {
                paradigm = new DrawImage(0, 0, isInitialImage);
            }

            AddNewObject(drawArea, paradigm);
            int al = drawArea.TheLayers.ActiveLayerIndex;
            drawArea.TheLayers[al].Graphics[0].MoveHandleTo(new System.Drawing.Point(image.Width, image.Height), 5);
            ((DrawImage)drawArea.TheLayers[al].Graphics[0]).TheImage = image;
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