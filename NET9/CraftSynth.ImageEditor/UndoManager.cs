using System;
using System.Collections.Generic;

/// Undo-Redo code is written using the article:
/// http://www.codeproject.com/cs/design/commandpatterndemo.asp
//  The Command Pattern and MVC Architecture
//  By David Veeneman.
namespace CraftSynth.ImageEditor
{
    /// <summary>
    /// Class is responsible for executing Undo - Redo operations
    /// Migrated to .NET 9 with improved disposal patterns and nullable annotations
    /// </summary>
    internal class UndoManager : IDisposable
    {
        #region Class Members
        private readonly Layers layers;
        private List<Command> historyList;
        private int nextUndo;
        private bool _disposed = false;
        #endregion  Class Members

        #region Constructor
        public UndoManager(Layers layerList)
        {
            layers = layerList ?? throw new ArgumentNullException(nameof(layerList));
            ClearHistory();
        }
        #endregion Constructor

        #region Destruction
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);       
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    // Free any managed objects here
                    if (this.historyList != null)
                    {
                        foreach (Command command in this.historyList)
                        {
                            command?.Dispose();
                        }
                        this.historyList.Clear();
                        this.historyList = null!;
                    }
                    
                    // Note: We don't dispose layers here as they are owned by the DrawArea
                    // The layers reference is readonly and managed by the parent DrawArea
                }

                // Free any unmanaged objects here
                this._disposed = true;
            }
        }

        ~UndoManager()
        {
             this.Dispose(false);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Return true if Undo operation is available
        /// </summary>
        public bool CanUndo
        {
            get
            {
                // If the NextUndo pointer is -1, no commands to undo
                if (nextUndo < 0 || nextUndo > historyList.Count - 1) // precaution
                {
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Return true if Redo operation is available
        /// </summary>
        public bool CanRedo
        {
            get
            {
                // If the NextUndo pointer points to the last item, no commands to redo
                if (nextUndo == historyList.Count - 1)
                {
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Gets the current history count for debugging purposes
        /// </summary>
        public int HistoryCount => historyList?.Count ?? 0;

        /// <summary>
        /// Gets the current undo position for debugging purposes
        /// </summary>
        public int CurrentUndoPosition => nextUndo;
        #endregion Properties

        #region Public Functions
        /// <summary>
        /// Clear History
        /// </summary>
        public void ClearHistory()
        {
            ObjectDisposedException.ThrowIfDisposed(_disposed, this);

            if (this.historyList != null)
            {
                foreach (Command command in historyList)
                {
                    command?.Dispose();
                }
                this.historyList.Clear();
            }
            else
            {
                historyList = new List<Command>();
            }
            
            nextUndo = -1;
        }

        /// <summary>
        /// Add new command to history.
        /// Called by client after executing some action.
        /// </summary>
        /// <param name="command">Command to add to history</param>
        /// <exception cref="ArgumentNullException">Thrown when command is null</exception>
        /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed</exception>
        public void AddCommandToHistory(Command command)
        {
            ObjectDisposedException.ThrowIfDisposed(_disposed, this);
            ArgumentNullException.ThrowIfNull(command);

            // Purge history list
            TrimHistoryList();

            // Add command and increment undo counter
            historyList.Add(command);
            nextUndo++;
        }

        /// <summary>
        /// Undo the last executed command
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed</exception>
        public void Undo()
        {
            ObjectDisposedException.ThrowIfDisposed(_disposed, this);

            if (!CanUndo)
            {
                return;
            }

            try
            {
                // Get the Command object to be undone
                Command command = historyList[nextUndo];

                // Execute the Command object's undo method
                command.Undo(layers);

                // Move the pointer up one item
                nextUndo--;
            }
            catch (Exception ex)
            {
                // Log error and reset to a safe state
                System.Diagnostics.Debug.WriteLine($"Error during undo operation: {ex.Message}");
                // Could add more sophisticated error handling here
                throw;
            }
        }

        /// <summary>
        /// Redo the next available command
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed</exception>
        public void Redo()
        {
            ObjectDisposedException.ThrowIfDisposed(_disposed, this);

            if (!CanRedo)
            {
                return;
            }

            try
            {
                // Get the Command object to redo
                int itemToRedo = nextUndo + 1;
                Command command = historyList[itemToRedo];

                // Execute the Command object
                command.Redo(layers);

                // Move the undo pointer down one item
                nextUndo++;
            }
            catch (Exception ex)
            {
                // Log error and reset to a safe state
                System.Diagnostics.Debug.WriteLine($"Error during redo operation: {ex.Message}");
                // Could add more sophisticated error handling here
                throw;
            }
        }

        /// <summary>
        /// Gets information about the command at the specified history index
        /// </summary>
        /// <param name="historyIndex">The index in the history list</param>
        /// <returns>String description of the command, or null if index is invalid</returns>
        public string? GetCommandDescription(int historyIndex)
        {
            ObjectDisposedException.ThrowIfDisposed(_disposed, this);

            if (historyIndex < 0 || historyIndex >= historyList.Count)
            {
                return null;
            }

            return historyList[historyIndex]?.ToString() ?? "Unknown Command";
        }

        /// <summary>
        /// Limits the history size to prevent excessive memory usage
        /// </summary>
        /// <param name="maxHistorySize">Maximum number of commands to keep in history</param>
        public void LimitHistorySize(int maxHistorySize = 50)
        {
            ObjectDisposedException.ThrowIfDisposed(_disposed, this);

            if (maxHistorySize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxHistorySize), "History size must be greater than 0");
            }

            while (historyList.Count > maxHistorySize)
            {
                // Remove and dispose the oldest command
                var oldestCommand = historyList[0];
                historyList.RemoveAt(0);
                oldestCommand?.Dispose();

                // Adjust the nextUndo pointer
                if (nextUndo >= 0)
                {
                    nextUndo--;
                }
            }
        }
        #endregion Public Functions

        #region Private Functions
        private void TrimHistoryList()
        {
            // We can redo any undone command until we execute a new 
            // command. The new command takes us off in a new direction,
            // which means we can no longer redo previously undone actions. 
            // So, we purge all undone commands from the history list.

            // Exit if no items in History list
            if (historyList.Count == 0)
            {
                return;
            }

            // Exit if NextUndo points to last item on the list
            if (nextUndo == historyList.Count - 1)
            {
                return;
            }

            // Purge all items below the NextUndo pointer
            for (int i = historyList.Count - 1; i > nextUndo; i--)
            {
                // Dispose the command before removing it
                historyList[i]?.Dispose();
                historyList.RemoveAt(i);
            }
        }
        #endregion
    }
}