using System.IO;
using System.Reflection;
using System.Windows.Forms;
using SkiaSharp;

namespace CraftSynth.ImageEditor
{
    /// <summary>
    /// Line tool
    /// </summary>
    internal class ToolLine : ToolObject
    {
        public ToolLine()
        {
            Cursor = LoadCursorFromResource("Line.cur");
        }

        public override void OnMouseDown(DrawArea drawArea, MouseEventArgs e)
        {
            var point = drawArea.BackTrackMouse(new System.Drawing.Point(e.X, e.Y));
            AddNewObject(drawArea, new DrawLine(point.X, point.Y, point.X + 1, point.Y + 1, 
                drawArea.LineColor, drawArea.LineWidth, drawArea.PenType, drawArea.EndCap));
        }

        public override void OnMouseMove(DrawArea drawArea, MouseEventArgs e)
        {
            drawArea.Cursor = Cursor;

            if (e.Button == MouseButtons.Left)
            {
                var point = drawArea.BackTrackMouse(new System.Drawing.Point(e.X, e.Y));
                int al = drawArea.TheLayers.ActiveLayerIndex;
                drawArea.TheLayers[al].Graphics[0].MoveHandleTo(point, 2);
                drawArea.Refresh();
            }
        }

        /// <summary>
        /// Load cursor from embedded resource file
        /// </summary>
        /// <param name="cursorFileName">The cursor file name</param>
        /// <returns>Cursor object or default cursor if failed</returns>
        private static Cursor LoadCursorFromResource(string cursorFileName)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = $"CraftSynth.ImageEditor.{cursorFileName}";
                
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        return new Cursor(stream);
                    }
                }
                
                // Fallback: try to load from file system if embedded resource fails
                var assemblyLocation = Path.GetDirectoryName(assembly.Location);
                var cursorPath = Path.Combine(assemblyLocation ?? "", cursorFileName);
                
                if (File.Exists(cursorPath))
                {
                    return new Cursor(cursorPath);
                }
            }
            catch
            {
                // If all else fails, return default cursor
            }
            
            return Cursors.Cross; // Default cursor for line tool
        }
    }
}