#region Using directives

using System;
using System.Windows.Forms;

#endregion

namespace CraftSynth.ImageEditor.DocToolkit
{
    /// <summary>
    /// DragDropManager class allows to open files dropped from 
    /// Windows Explorer in Windows Form application.
    /// 
    /// Using:
    /// 1) Write function which opens file selected from MRU:
    ///
    /// private void dragDropManager_FileDroppedEvent(object sender, FileDroppedEventArgs e)
    /// {
    ///    // open file(s) from e.FileArray:
    ///    // e.FileArray.GetValue(0).ToString() ...
    /// }
    ///     
    /// 2) Add member of this class to the parent form:
    ///  
    /// private DragDropManager dragDropManager;
    /// 
    /// 3) Create class instance in parent form initialization code:
    ///  
    /// dragDropManager = new DragDropManager(this);
    /// dragDropManager.FileDroppedEvent += dragDropManager_FileDroppedEvent; 
    /// 
    /// </summary>
    public class DragDropManager
    {
        private readonly Form _frmOwner;          // reference to owner form

        // Event raised when drops file(s) to the form
        public event FileDroppedEventHandler? FileDroppedEvent;

        public DragDropManager(Form owner)
        {
            _frmOwner = owner ?? throw new ArgumentNullException(nameof(owner));

            // ensure that parent form allows dropping
            _frmOwner.AllowDrop = true;

            // subscribe to parent form's drag-drop events
            _frmOwner.DragEnter += OnDragEnter;
            _frmOwner.DragDrop += OnDragDrop;
        }

        /// <summary>
        /// Handle parent form DragEnter event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDragEnter(object? sender, DragEventArgs e)
        {
            // If file is dragged, show cursor "Drop allowed"
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        /// <summary>
        /// Handle parent form DragDrop event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDragDrop(object? sender, DragEventArgs e)
        {
            try
            {
                // When file(s) are dragged from Explorer to the form, IDataObject
                // contains array of file names. If one file is dragged,
                // array contains one element.
                if (e.Data?.GetData(DataFormats.FileDrop) is Array fileArray && fileArray.Length > 0)
                {
                    FileDroppedEvent?.Invoke(this, new FileDroppedEventArgs(fileArray));
                    _frmOwner.Activate();        // in the case Explorer overlaps parent form
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                // In .NET 9, we should handle exceptions more gracefully
                System.Diagnostics.Debug.WriteLine($"Error handling drag-drop: {ex.Message}");
                
                // You might want to show a message to the user or log to a proper logging system
                // For now, we'll just debug output to prevent crashes
            }
        }

        /// <summary>
        /// Dispose resources and unsubscribe from events
        /// </summary>
        public void Dispose()
        {
            if (_frmOwner != null)
            {
                _frmOwner.DragEnter -= OnDragEnter;
                _frmOwner.DragDrop -= OnDragDrop;
            }
        }
    }

    public delegate void FileDroppedEventHandler(object sender, FileDroppedEventArgs e);

    public class FileDroppedEventArgs : EventArgs
    {
        public Array FileArray { get; }

        public FileDroppedEventArgs(Array array)
        {
            FileArray = array ?? throw new ArgumentNullException(nameof(array));
        }

        /// <summary>
        /// Get file paths as string array for easier access
        /// </summary>
        public string[] GetFilePathsAsStringArray()
        {
            if (FileArray is string[] stringArray)
                return stringArray;

            var result = new string[FileArray.Length];
            for (int i = 0; i < FileArray.Length; i++)
            {
                result[i] = FileArray.GetValue(i)?.ToString() ?? string.Empty;
            }
            return result;
        }

        /// <summary>
        /// Get the first file path (convenience method for single file drops)
        /// </summary>
        public string GetFirstFilePath()
        {
            if (FileArray.Length > 0)
                return FileArray.GetValue(0)?.ToString() ?? string.Empty;
            return string.Empty;
        }
    }
}