using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using ToDo.Properties;

namespace ToDo
{
    class TaskEntryPanel : FlowLayoutPanel
    {
        public readonly TaskEntry EmptyTaskEntry;
        

        //private members
        private TaskEntry lastExpandedEntry = null;

        private ToolTip tTip = new ToolTip()
        {
            AutomaticDelay = 250
        };

        public Settings DefaultSettings
        {
            get
            {
                return Settings.Default;
            }
        }

        //constructor
        public TaskEntryPanel()
        {
            InitializeComponent();

            EmptyTaskEntry = addTaskEntry(Task.Empty, false);

            OnResize(new EventArgs());

            if (Ext.DesignTime)    //VS WinForms designer support
                return;

            DefaultSettings.LoadTasks();

            for (int i = 0; i < DefaultSettings.Tasks.Count; i++)
            {
                var taskData = DefaultSettings.Tasks[i];
                addTaskEntry(new Task(taskData), false);
            }
        }
        
        
        //event overrides
        protected override void OnParentChanged(EventArgs e)
        {
            OnResize(new EventArgs());
            base.OnParentChanged(e);
        }

        protected override void OnResize(EventArgs eventargs)
        {
            foreach (Control c in this.Controls)
                c.Width = Width;

            base.OnResize(eventargs);
        }
        
        protected override void OnMouseClick(MouseEventArgs e)
        {
            this.Focus();
            base.OnMouseClick(e);
        }


        //public methods
        public void Collapse()
        {
            foreach (TaskEntry t in this.Controls)
                t.Expanded = false;
        }

        /// <summary>
        /// Adds a new, empty task to the list.
        /// </summary>
        public void CreateNewTask()
        {
            Task t = new Task();

            DefaultSettings.Tasks.Add(t.Data);
            DefaultSettings.Save();

            addTaskEntry(t, true);
        }

        public void SortTasks()
        {
            //sort saved tasks
            DefaultSettings.Tasks.Sort(TaskData.DefaultComparer);

            //sort entries
            var sortedEntries = this.Controls
                .Cast<Control>()
                .Where(c => c is TaskEntry && !c.Equals(EmptyTaskEntry))
                .Cast<TaskEntry>()
                .OrderBy(te => te.Task.Data, TaskData.DefaultComparer);
            foreach(var e in sortedEntries)
                e.SendToBack();

            //sanity check
            System.Diagnostics.Debug.Assert(sortedEntries.Zip(DefaultSettings.Tasks, (te, td) => (te.Task.Data.Equals(td))).All(b => b));

            Invalidate(true);
        }


        private void te_OnExpanded(TaskEntry obj)
        {
            if (lastExpandedEntry != null && lastExpandedEntry != obj)
                lastExpandedEntry.Expanded = false;
            lastExpandedEntry = obj;
        }
        
        private void task_OnSomethingChanged(Task t)
        {
            t.Entry.Invalidate(true);
            DefaultSettings.Save();
        }


        private TaskEntry addTaskEntry(Task t, bool rename)
        {
            var te = new TaskEntry(t)
            {
                Width = this.Width,
                Top = (this.Controls.Count - 2) * 20,
                Expanded = false
            };
            te.SetTooltip(tTip);
            te.Remove += removeTask;
            te.Expand += te_OnExpanded;
            t.SomethingChanged += task_OnSomethingChanged;

            if (t != Task.Empty && rename)
                te.Expanded = true;

            if (EmptyTaskEntry != null)
                EmptyTaskEntry.Visible = false;

            this.Controls.Add(te);
            PerformLayout();

            return te;
        }

        private void removeTask(TaskEntry te)
        {
            this.SelectNextControl(te, true, true, false, false);
            this.Controls.Remove(te);
            DefaultSettings.Tasks.Remove(te.Task.Data);

            te.Task.SomethingChanged -= task_OnSomethingChanged;
            te.Dispose();

            DefaultSettings.Save();
            PerformLayout();
            EmptyTaskEntry.Visible = (DefaultSettings.Tasks.Count == 0);
        }
        

        //designer code
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // TaskEntryPanel
            // 
            this.AutoScroll = true;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.MaximumSize = new System.Drawing.Size(400, 0);
            this.Size = new System.Drawing.Size(200, 0);
            this.ResumeLayout(false);

        }
    }
}
