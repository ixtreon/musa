using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Musa.Properties;

namespace Musa
{
    public partial class MainForm : Form
    {
        static readonly string autorunFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "toDo.url");
        
        bool hiddenAtStartup = false;

        Point notifyIconLocation;

        private void updateNotifyIconLocation()
        {
            const int notifyIconRadius = 22;
            var cursor = Cursor.Position.Add(-8, -8);
            if (notifyIconLocation == null || notifyIconLocation.DistanceTo(cursor) > notifyIconRadius)
                notifyIconLocation = cursor;
        }

        private void updateFormLocation()
        {
            var dispRect = Screen.FromControl(this).WorkingArea;
            var rawWindowRect = new Rectangle(notifyIconLocation, Size);
            var windowRect = rawWindowRect.ConstrainTo(dispRect);
            this.Location = windowRect.Location;
        }

        public MainForm()
        {
            InitializeComponent();

            this.runOnStartupToolStripMenuItem.Checked = File.Exists(autorunFile);

            this.Size = new Size(200, 160);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            notifyIcon.Dispose();
            base.OnClosing(e);
        }

      

        private void btnAddTask_Paint(object sender, PaintEventArgs e)
        {
            const int offset = 4;
            var g = e.Graphics;
            var sh = btnAddTask.Height - 2 * offset;
            var p = new Pen(Color.FromArgb(72, 72, 72), 2f);
            g.DrawLine(p, btnAddTask.Width / 2, offset, btnAddTask.Width / 2, offset + sh);
            g.DrawLine(p, (btnAddTask.Width - sh) / 2, btnAddTask.Height / 2, (btnAddTask.Width + sh) / 2, btnAddTask.Height / 2);
        }

        private void btnAddTask_Click(object sender, EventArgs e)
        {
            tasksPanel.CreateNewTask();
        }

        private async void MainForm_Deactivate(object sender, EventArgs e)
        {
            //a hacky workaround to make clicking the icon hide the window
            //makes sure the MouseDown event is already processed 
            //and the form isn't shown right after we hide it
            while(MouseButtons == System.Windows.Forms.MouseButtons.Left)
                await System.Threading.Tasks.Task.Delay(100);

            this.Hide();
            tasksPanel.Collapse();
        }

        private void notifyIcon_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Left)
                return;

            updateNotifyIconLocation();
            updateFormLocation();

            if (this.Visible)
            {
                Hide();
                tasksPanel.Collapse();
            }
            else
            {
                tasksPanel.SortTasks();
                Show();
                NativeMethods.SetForegroundWindow(Handle.ToInt32());
            }
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            Application.Exit();
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            //hide the form when it is first shown
            if (!hiddenAtStartup)
            {
                hiddenAtStartup = true;
                Hide();
                return;
            }

        }

        private void tasksPanel_Layout(object sender, LayoutEventArgs e)
        {
            updateFormLocation();
        }

        private void runOnStartupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (File.Exists(autorunFile))
            {
                File.Delete(autorunFile);
                runOnStartupToolStripMenuItem.Checked = false;
            }
            else
            {
                Ext.CreateShortcut(autorunFile);
                runOnStartupToolStripMenuItem.Checked = true;
            }

            Settings.Default.Save();
        }
    }
}
