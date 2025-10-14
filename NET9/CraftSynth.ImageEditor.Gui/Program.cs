using System;
using System.Windows.Forms;

namespace CraftSynth.ImageEditor.Gui
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Configure the application for .NET 9 Windows Forms
            ApplicationConfiguration.Initialize();
            
            // Run the application with the main form
            Application.Run(new Form1());
        }
    }
}