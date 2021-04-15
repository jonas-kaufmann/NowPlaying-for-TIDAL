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
                if (value != active)
                {
                    active = value;
                    ToggleActiveItem.Checked = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Active)));
                }
            }
        }

        #endregion


        private readonly NotifyIcon NotifyIcon = new NotifyIcon();
        private readonly ToolStripMenuItem ToggleActiveItem;
        private readonly ToolStripItem ConfigItem;
        private readonly ToolStripItem ExitItem;

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

            ConfigItem = cms.Items.Add(ConfigText);
            ConfigItem.Click += (sender, args) => Process.Start("explorer.exe", $"/select,\"{AppConfig.ConfigPath}\"");

            ExitItem = cms.Items.Add(ExitText);
            ExitItem.Click += (sender, args) => Application.Exit();

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
