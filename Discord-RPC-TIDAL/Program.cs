using discord_rpc_tidal.Logging;
using discord_rpc_tidal.UI;
using Squalr.Engine.Logging;
using System;
using System.Diagnostics;
using System.Windows.Forms;
using discord_rpc_tidal.Data;
using discord_rpc_tidal.Discord;

namespace discord_rpc_tidal
{
    static class Program
    {
        private static MyNotifyIcon MyNotifyIcon;

        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException +=
                CurrentDomain_UnhandledException; // Receive unhandled exceptions
            Logger.Subscribe(new SqualrLogger()); // Receive logs from the Squalr
            Trace.Listeners.Add(new ConsoleTraceListener());

            AppConfig.Load();
            AppConfig.Save();

            AssetManager.Sync();

            using (MyNotifyIcon = new MyNotifyIcon())
            using (var tidalListener = new TidalListener())
            using (var discordRpc = new DiscordRPC(tidalListener))
            {
                MyNotifyIcon.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == nameof(MyNotifyIcon.Active))
                    {
                        if (MyNotifyIcon.Active)
                            tidalListener.Start();
                        else
                            tidalListener.Stop();
                    }
                };

                MyNotifyIcon.Active = true;
                Application.Run();
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Trace.TraceError(e.ExceptionObject.ToString());
            MessageBox.Show(e.ExceptionObject.ToString(), AppDomain.CurrentDomain.FriendlyName, MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}