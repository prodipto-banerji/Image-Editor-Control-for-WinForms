using System;
using System.Windows.Forms;
using Microsoft.Win32;
using SkiaSharp;

namespace CraftSynth.ImageEditor.DocToolkit
{
    /// <summary>
    /// Class allows to keep last window state in Registry
    /// and restore it when form is loaded.
    /// 
    /// Source: Saving and Restoring the Location, Size and 
    ///         Windows State of a .NET Form
    ///         By Joel Matthias 
    ///         
    ///  Downloaded from http://www.codeproject.com
    ///  
    ///  Using:
    ///  1. Add class member to the owner form:
    ///  
    ///  private PersistWindowState persistState;
    ///  
    ///  2. Create it in the form constructor:
    ///  
    ///  persistState = new PersistWindowState("Software\\MyCompany\\MyProgram", this);
    ///  
    /// </summary>
    public class PersistWindowState : IDisposable
    {
        #region Members

        private Form? ownerForm;          // reference to owner form
        private string registryPath;       // path in Registry where state information is kept

        // Form state parameters:
        private int normalLeft;
        private int normalTop;
        private int normalWidth;
        private int normalHeight;

        // FormWindowState is enumeration from System.Windows.Forms Namespace
        // Contains 3 members: Maximized, Minimized and Normal.
        private FormWindowState windowState = FormWindowState.Normal;

        // if allowSaveMinimized is true, form closed in minimal state
        // is loaded next time in minimal state.
        private bool allowSaveMinimized = false;

        private bool _disposed = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Initialization
        /// </summary>
        /// <param name="path">Registry path where window state will be stored</param>
        /// <param name="owner">Owner form to track state for</param>
        public PersistWindowState(string path, Form owner)
        {
            ArgumentNullException.ThrowIfNull(owner);

            if (string.IsNullOrWhiteSpace(path))
            {
                registryPath = "Software\\Unknown";
            }
            else
            {
                registryPath = path;
            }

            if (!registryPath.EndsWith("\\"))
                registryPath += "\\";

            registryPath += "MainForm";

            ownerForm = owner;

            // subscribe to parent form's events
            ownerForm.FormClosing += OnClosing;
            ownerForm.Resize += OnResize;
            ownerForm.Move += OnMove;
            ownerForm.Load += OnLoad;

            // get initial width and height in case form is never resized
            normalWidth = ownerForm.Width;
            normalHeight = ownerForm.Height;
        }

        #endregion

        #region Properties

        /// <summary>
        /// AllowSaveMinimized property (default value false) 
        /// </summary>
        public bool AllowSaveMinimized
        {
            get => allowSaveMinimized;
            set => allowSaveMinimized = value;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Parent form is resized.
        /// Keep current size.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void OnResize(object? sender, EventArgs e)
        {
            if (ownerForm == null || _disposed) return;

            // save width and height
            if (ownerForm.WindowState == FormWindowState.Normal)
            {
                normalWidth = ownerForm.Width;
                normalHeight = ownerForm.Height;
            }
        }

        /// <summary>
        /// Parent form is moved.
        /// Keep current window position.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void OnMove(object? sender, EventArgs e)
        {
            if (ownerForm == null || _disposed) return;

            // save position
            if (ownerForm.WindowState == FormWindowState.Normal)
            {
                normalLeft = ownerForm.Left;
                normalTop = ownerForm.Top;
            }

            // save state
            windowState = ownerForm.WindowState;
        }

        /// <summary>
        /// Parent form is closing.
        /// Keep last state in Registry.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void OnClosing(object? sender, FormClosingEventArgs e)
        {
            if (ownerForm == null || _disposed) return;

            try
            {
                // save position, size and state
                using (RegistryKey? key = Registry.CurrentUser.CreateSubKey(registryPath))
                {
                    if (key != null)
                    {
                        key.SetValue("Left", normalLeft);
                        key.SetValue("Top", normalTop);
                        key.SetValue("Width", normalWidth);
                        key.SetValue("Height", normalHeight);

                        // check if we are allowed to save the state as minimized (not normally)
                        var stateToSave = windowState;
                        if (!allowSaveMinimized && windowState == FormWindowState.Minimized)
                        {
                            stateToSave = FormWindowState.Normal;
                        }

                        key.SetValue("WindowState", (int)stateToSave);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception if needed, but don't prevent form closing
                System.Diagnostics.Debug.WriteLine($"Error saving window state: {ex.Message}");
            }
        }

        /// <summary>
        /// Parent form is loaded.
        /// Read last state from Registry and set it to form.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void OnLoad(object? sender, EventArgs e)
        {
            if (ownerForm == null || _disposed) return;

            try
            {
                // attempt to read state from registry
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(registryPath))
                {
                    if (key != null)
                    {
                        int left = (int)(key.GetValue("Left") ?? ownerForm.Left);
                        int top = (int)(key.GetValue("Top") ?? ownerForm.Top);
                        int width = (int)(key.GetValue("Width") ?? ownerForm.Width);
                        int height = (int)(key.GetValue("Height") ?? ownerForm.Height);
                        FormWindowState restoredWindowState = (FormWindowState)(key.GetValue("WindowState") ?? (int)ownerForm.WindowState);

                        // Validate the restored values to ensure they're reasonable
                        if (IsValidPosition(left, top, width, height))
                        {
                            // Using SKPointI for cross-platform compatibility (though System.Drawing.Point still works in WinForms)
                            // We'll use regular Point for Windows Forms compatibility but keep SkiaSharp reference for consistency
                            ownerForm.Location = new System.Drawing.Point(left, top);
                            ownerForm.Size = new System.Drawing.Size(width, height);
                            ownerForm.WindowState = restoredWindowState;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception if needed, but don't prevent form loading
                System.Diagnostics.Debug.WriteLine($"Error restoring window state: {ex.Message}");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Validates that the position and size are reasonable for the current screen configuration
        /// </summary>
        /// <param name="left">Left position</param>
        /// <param name="top">Top position</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <returns>True if the position is valid</returns>
        private static bool IsValidPosition(int left, int top, int width, int height)
        {
            // Ensure minimum size
            if (width < 100 || height < 50) return false;

            // Ensure the window is at least partially visible on screen
            var workingArea = Screen.GetWorkingArea(new System.Drawing.Point(left, top));
            return left < workingArea.Right && 
                   top < workingArea.Bottom && 
                   left + width > workingArea.Left && 
                   top + height > workingArea.Top;
        }

        /// <summary>
        /// Convert System.Drawing.Point to SKPointI for cross-platform operations if needed
        /// </summary>
        /// <param name="point">System.Drawing.Point</param>
        /// <returns>SKPointI equivalent</returns>
        public static SKPointI ToSKPoint(System.Drawing.Point point)
        {
            return new SKPointI(point.X, point.Y);
        }

        /// <summary>
        /// Convert SKPointI to System.Drawing.Point for Windows Forms operations
        /// </summary>
        /// <param name="point">SKPointI</param>
        /// <returns>System.Drawing.Point equivalent</returns>
        public static System.Drawing.Point FromSKPoint(SKPointI point)
        {
            return new System.Drawing.Point(point.X, point.Y);
        }

        /// <summary>
        /// Convert System.Drawing.Size to SKSizeI for cross-platform operations if needed
        /// </summary>
        /// <param name="size">System.Drawing.Size</param>
        /// <returns>SKSizeI equivalent</returns>
        public static SKSizeI ToSKSize(System.Drawing.Size size)
        {
            return new SKSizeI(size.Width, size.Height);
        }

        /// <summary>
        /// Convert SKSizeI to System.Drawing.Size for Windows Forms operations
        /// </summary>
        /// <param name="size">SKSizeI</param>
        /// <returns>System.Drawing.Size equivalent</returns>
        public static System.Drawing.Size FromSKSize(SKSizeI size)
        {
            return new System.Drawing.Size(size.Width, size.Height);
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        /// Dispose of resources and unsubscribe from events
        /// </summary>
        /// <param name="disposing">True if disposing managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Unsubscribe from events to prevent memory leaks
                    if (ownerForm != null)
                    {
                        try
                        {
                            ownerForm.FormClosing -= OnClosing;
                            ownerForm.Resize -= OnResize;
                            ownerForm.Move -= OnMove;
                            ownerForm.Load -= OnLoad;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error unsubscribing from events: {ex.Message}");
                        }
                        ownerForm = null;
                    }
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Public dispose method
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~PersistWindowState()
        {
            Dispose(disposing: false);
        }

        #endregion
    }
}