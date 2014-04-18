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
using Musa.Properties;

namespace Musa
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
            EmptyTaskEntry.Width = Width;

            if (Ext.DesignTime)    //VS WinForms designer support
                return;

            //Load tasks from storage and add
            //the corresponding TaskEntries
            DefaultSettings.LoadTasks();
            foreach (var td in DefaultSettings.Tasks)
                addTaskEntry(new Task(td), false);
        }
        
        //event overrides
        protected override void OnParentChanged(EventArgs e)
        {
            //needed?
            OnResize(new EventArgs());
            base.OnParentChanged(e);
        }

        protected override void OnResize(EventArgs eventargs)
        {
            //resize all controls to match the container width
            foreach (Control c in this.Controls)
                c.Width = Width;
            base.OnResize(eventargs);
        }


        //public methods

        /// <summary>
        /// Collapses all TaskEntries in this Panel. 
        /// </summary>
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

        /// <summary>
        /// Selects the first visible TaskEntry after (or before) the given child TaskEntry. 
        /// </summary>
        /// <param name="c">The child TaskEntry to start the search from. </param>
        /// <param name="forward">True to select the next control, False to select the previous. </param>
        public bool SelectNextTaskEntry(TaskEntry c, bool forward, bool loop)
        {
            //get matching child controls
            var childControls = Controls.GetControls<TaskEntry>(true);
            if (!forward)
                childControls = childControls.Reverse();

            TaskEntry t = null;

            if (c == childControls.Last())
            {
                if (loop)
                    t = childControls.First();
            }
            else
            {
                var nxtControls = childControls.SkipWhile(ic => ic != c);
                if(nxtControls.Any())
                    t = nxtControls.Skip(1).First();
            }
            if (t == null)
                return false;

            t.Focus();
            return true;
        }

        /// <summary>
        /// Sorts the tasks both in the UI and in the underlying in-memory collection. 
        /// </summary>
        public void SortTasks()
        {
            //sort saved tasks
            DefaultSettings.Tasks.Sort(TaskData.DefaultComparer);

            //sort UI elements
            var sortedEntries = this.Controls
                .Cast<Control>()
                .Where(c => c is TaskEntry && !c.Equals(EmptyTaskEntry))
                .Cast<TaskEntry>()
                .OrderBy(te => te.Task.Data, TaskData.DefaultComparer).ToArray();

            //update the UI
            this.SuspendLayout();
            for (int i = 0; i < sortedEntries.Length; i++)
            {
                var te = sortedEntries[i];
                te.SendToBack();    //sorts the entries for some reason
                te.TabIndex = i;    //update the tab order
            }
            this.ResumeLayout();
            Invalidate(true);

            //check if saved and displayed tasks match?
            System.Diagnostics.Debug.Assert(sortedEntries
                .Zip(DefaultSettings.Tasks, (te, td) => (te.Task.Data.Equals(td)))
                .All(b => b));

        }

        private void te_OnExpanded(TaskEntry obj)
        {
            //collapse the previously expanded TaskEntry
            if (lastExpandedEntry != null && lastExpandedEntry != obj)
                lastExpandedEntry.Expanded = false;
            lastExpandedEntry = obj;
        }
        
        private void task_OnSomethingChanged(Task t)
        {
            //invalidate the entry whenever a task changes
            t.Entry.Invalidate(true);
            DefaultSettings.Save();
        }

        /// <summary>
        /// Adds a new TaskEntry to this Panel setting all necessary events and members. 
        /// Assumes the underlying task is already in the in-memory collection. 
        /// </summary>
        /// <param name="t">The underlying Task for the new TaskEntry. </param>
        /// <param name="expand">Whether to expand the TaskEntry after creation. </param>
        /// <returns>The newly created TaskEntry</returns>
        private TaskEntry addTaskEntry(Task t, bool expand)
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

            if (t != Task.Empty && expand)
                te.Expanded = true;

            if (EmptyTaskEntry != null)
                EmptyTaskEntry.Visible = false;

            this.Controls.Add(te);
            PerformLayout();

            return te;
        }

        /// <summary>
        /// Removes the specified TaskEntry from both the UI controls and the in-memory collection. 
        /// </summary>
        /// <param name="te">The TaskEntry to remove. </param>
        private void removeTask(TaskEntry te)
        {
            if (!this.SelectNextTaskEntry(te, true, false))   //try selecting the next control without looping
                this.SelectNextTaskEntry(te, false, false);    //fail, select prev one!
            this.Controls.Remove(te);
            DefaultSettings.Tasks.Remove(te.Task.Data);

            te.Task.SomethingChanged -= task_OnSomethingChanged;
            te.Dispose();

            if (DefaultSettings.Tasks.Count == 0)
            {
                EmptyTaskEntry.Visible = true;
                EmptyTaskEntry.Width = te.Width;
            }
            PerformLayout();
            
            DefaultSettings.Save();


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
