using Squalr.Engine.Logging;
using System;
using System.Diagnostics;
using System.Windows.Forms;
using nowplaying_for_tidal.Data;
using nowplaying_for_tidal.Discord;
using nowplaying_for_tidal.Logging;
using nowplaying_for_tidal.UI;

namespace nowplaying_for_tidal
{
    static class Program
    {
        private static MyNotifyIcon MyNotifyIcon;

        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException +=
                CurrentDomain_UnhandledException; // Handler for unhandled exception
            Logger.Subscribe(new SqualrLogger()); // Log Squalr
            Trace.Listeners.Add(new ConsoleTraceListener());

            AppConfig.Load();
            AppConfig.Save();

            DiscordAssets.Load();
            AssetManager.Sync();

            using (MyNotifyIcon = new MyNotifyIcon())
            using (var tidalListener = new TidalListener())
            using (var discordRpc = new DiscordRpc(tidalListener))
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