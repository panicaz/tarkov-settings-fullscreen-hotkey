using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using tarkov_settings.GPU;

namespace tarkov_settings
{
    static class Program
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern uint RegisterWindowMessage(string message);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private static readonly IntPtr HWND_BROADCAST = new IntPtr(0xFFFF);
        internal static readonly uint WM_SHOWME = RegisterWindowMessage("TarkovSettings_ShowMe");

        private static MainForm mForm;

        [STAThread]
        static void Main()
        {
            const string mutexName = "TarkovSettings_SingleInstance";

            using (var mutex = new Mutex(true, mutexName, out bool isNewInstance))
            {
                if (!isNewInstance)
                {
                    PostMessage(HWND_BROADCAST, WM_SHOWME, IntPtr.Zero, IntPtr.Zero);
                    return;
                }

                IGPU gpu = null;
                try
                {
                    gpu = GPUDevice.Instance;
                    if (gpu.Vendor == GPUVendor.AMD)
                    {
                        MessageBox.Show(
                            "AMD Device Detected - Saturation is not supported yet.",
                            "Warning",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );
                    }
                }
                catch (NotImplementedException)
                {
                    MessageBox.Show(
                        "Intel/Nvidia Optimus/Etc Device Detected - Will be supported soon",
                        "Nvidia GPU is not found!",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    Thread.Sleep(1000);
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                mForm = new MainForm();
                mForm.Init();
                Application.Run(mForm);

                // Unload NvAPI dll after Application.Exit()
                gpu?.Close();
            }
        }
    }
}