using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace discord_rpc_tidal.UI
{
    public class MyNotifyIcon : IDisposable, INotifyPropertyChanged
    {
        #region observable properties

        private bool active = true;
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
            ToggleActiveItem.CheckedChanged += ToggleActive_CheckedChanged;
            cms.Items.Add(ToggleActiveItem);

            ExitItem = cms.Items.Add(ExitText);
            ExitItem.Click += Exit_Click;

            NotifyIcon.ContextMenuStrip = cms;

            NotifyIcon.Visible = true;
        }

        private void ToggleActive_CheckedChanged(object sender, EventArgs e)
        {
            Active = ToggleActiveItem.Checked;
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        public void Dispose()
        {
            NotifyIcon.Dispose();
        }

        #region string resources

        private const string StatusText = "Active";
        private const string ExitText = "Exit";

        #endregion
    }
}
