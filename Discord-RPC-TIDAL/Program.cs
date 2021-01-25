using discord_rpc_tidal.UI;
using System;
using System.Windows.Forms;

namespace discord_rpc_tidal
{
    static class Program
    {
        private static MyNotifyIcon MyNotifyIcon;
        private static MyListener MyListener;

        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            using (MyNotifyIcon = new MyNotifyIcon())
            using (MyListener = new MyListener())
            {
                MyNotifyIcon.PropertyChanged += MyNotifyIcon_PropertyChanged;
                MyNotifyIcon_PropertyChanged(null, new System.ComponentModel.PropertyChangedEventArgs(nameof(MyNotifyIcon.Active))); // start Listener if in status 'active'

                Application.Run();
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.ExceptionObject.ToString(), AppDomain.CurrentDomain.FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static void MyNotifyIcon_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MyNotifyIcon.Active))
            {
                MyListener.Start();
                if (MyNotifyIcon.Active)
                    MyListener.Start();
                else
                    MyListener.Stop();
            }
        }
    }
}
