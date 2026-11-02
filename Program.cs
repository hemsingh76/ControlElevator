using System;
using System.Windows.Forms;
using ElevatorSimulation.UI;

namespace ElevatorSimulation
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var system = new ElevatorSystem();   // builds and connects everything
            Application.Run(new MainForm(system));
        }
    }
}
