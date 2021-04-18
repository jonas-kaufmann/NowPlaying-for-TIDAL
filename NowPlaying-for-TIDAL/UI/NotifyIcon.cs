using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using nowplaying_for_tidal.Data;

namespace nowplaying_for_tidal.UI
{
    public class MyNotifyIcon : IDisposable, INotifyPropertyChanged
    {
        #region observable properties

        private bool active;

        public bool Active
        {
            get => active;
            set
            {
                if (value == active) return;
                active = value;
                ToggleActiveItem.Checked = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Active)));
            }
        }

        #endregion


        private readonly NotifyIcon NotifyIcon = new NotifyIcon();
        private readonly ToolStripMenuItem ToggleActiveItem;

        public event PropertyChangedEventHandler PropertyChanged;


        public MyNotifyIcon()
        {
            NotifyIcon.Text = AppDomain.CurrentDomain.FriendlyName;
            NotifyIcon.Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);

            // add items
            var cms = new ContextMenuStrip();


            ToggleActiveItem = new ToolStripMenuItem(StatusText)
            {
                CheckOnClick = true,
                Checked = Active
            };
            ToggleActiveItem.CheckedChanged += (sender, args) => Active = ToggleActiveItem.Checked;
            cms.Items.Add(ToggleActiveItem);

            var configItem = cms.Items.Add(ConfigText);
            configItem.Click += (sender, args) => Process.Start("explorer.exe", $"/select,\"{AppConfig.ConfigPath}\"");

            // show app version
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            if (version != null)
            {
                var versionString = $"{version.Major}.{version.Minor}.{version.Build}";

                cms.Items.Add(new ToolStripSeparator());
                cms.Items.Add(new ToolStripLabel(versionString) {Enabled = false});
            }

            var exitItem = cms.Items.Add(ExitText);
            exitItem.Click += (sender, args) => Application.Exit();

            NotifyIcon.ContextMenuStrip = cms;

            NotifyIcon.Visible = true;
        }

        public void Dispose()
        {
            NotifyIcon.Dispose();
        }

        #region string resources

        private const string StatusText = "Active";
        private const string ConfigText = "Show Config";
        private const string ExitText = "Exit";

        #endregion
    }
}