using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ManaSpline
{
    static class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AttachConsole(int dwProcessId);

        private const int ATTACH_PARENT_PROCESS = -1;

        [STAThread]
        static void Main()
        {
            AttachConsole(ATTACH_PARENT_PROCESS);

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new ManaForm());
        }
    }
}