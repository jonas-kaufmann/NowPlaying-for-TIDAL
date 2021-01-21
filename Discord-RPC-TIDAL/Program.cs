using discord_rpc_tidal.UI;
using System.Windows.Forms;

namespace discord_rpc_tidal
{
    static class Program
    {
        private static readonly MyNotifyIcon MyNotifyIcon = new MyNotifyIcon();
        private static readonly MyListener MyListener = new MyListener();

        static void Main()
        {
            MyNotifyIcon.PropertyChanged += MyNotifyIcon_PropertyChanged;
            MyNotifyIcon_PropertyChanged(null, new System.ComponentModel.PropertyChangedEventArgs(nameof(MyNotifyIcon.Active))); // start Listener if in status 'active'

            Application.Run();
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
