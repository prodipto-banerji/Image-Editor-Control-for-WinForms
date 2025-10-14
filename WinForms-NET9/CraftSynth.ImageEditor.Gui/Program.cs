using System;
using System.Windows.Forms;
using Microsoft.Extensions.Hosting;

namespace CraftSynth.ImageEditor.Gui
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((ctx, services) => { /* register services if needed */ })
                .Build();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
