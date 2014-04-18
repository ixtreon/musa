using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Musa.Properties;

namespace Musa
{
    public class TaskEntry : UserControl
    {
        public const int Offset = 4;

        //Designer variables
        private ToolTip toolTip;
        private ContextMenuStrip contextMenu;
        private System.ComponentModel.IContainer components;
        private ToolStripMenuItem tsMore;
        private ToolStripMenuItem tsDelete;
        private TextBox txtDescription;
        private DateTimePicker dueDate;
        private TextBox txtTitle;
        private Components.MoreButton btnMore;
        private Components.CheckButton chkCompleted;
        private Components.ToggleEndDateButton btnExpire;

        //member variable(s)
        private bool _expanded;


        //public fields
        /// <summary>
        /// Gets the underlying Task object for this TaskEntry
        /// </summary>
        public readonly Task Task;

        /// <summary>
        /// Gets whether the TaskEntry or any of its child controls are currently focused. 
        /// </summary>
        public new bool Focused { get; protected set; }

        /// <summary>
        /// Gets the position of the text when the task is hidden
        /// </summary>
        public PointF TextPosition { get; protected set; }
        
        /// <summary>
        /// Gets or sets whether the control is expanded. 
        /// </summary>
        public bool Expanded
        {
            get
            {
                return _expanded;
            }
            set
            {
                if (_expanded != value)
                {
                    _expanded = value;

                    //update UI
                    this.Height = (value ? MaximumSize : MinimumSize).Height;
                    this.txtTitle.Visible = value;
                    this.txtDescription.Visible = value;
                    this.btnExpire.Visible = value;
                    this.dueDate.Visible = value && Task.ExpirationDate.HasValue;

                    if (value)
                    {
                        //update UI from task talues
                        txtTitle.Text = Task.Text;
                        txtDescription.Text = Task.Description;
                        btnExpire.SetChecked(Task.ExpirationDate.HasValue, false);
                        if (Task.ExpirationDate.HasValue)
                            dueDate.Value = Task.ExpirationDate.Value;
                        this.txtTitle.Focus();
                        Expand(this);
                    }
                    else
                    {
                        //update task values from UI
                        Task.Data.Text = txtTitle.Text;
                        Task.Data.Description = txtDescription.Text;
                        Task.Data.ExpirationDate = btnExpire.Checked ? (DateTime?)dueDate.Value : null;
                        Task.OnSomethingChanged();
                    }
                }
            }
        }

        public override Color BackColor
        {
            get
            {
                if (Task == null || Task.IsEmpty)
                    return Color.Yellow.Tint(Color.White, 50);

                //tint from yellow to red depending on time left
                Color c = Colors.PendingTask
                    .Tint(Colors.ExpiredTask, 100 - (int)(100 * Task.RemainingTime.TotalHours / Task.TotalTime.TotalHours));

                //fuck that and override with green if it's completed
                if (Task.Completed)
                    c = Colors.CompletedTask;

                //mouse hover maybe?
                var mouseOver = this.IsMouseOver();
                if ((mouseOver || Focused))
                {
                    c = c.Tint();
                }

                return c;
            }
        }

        //events
        /// <summary>
        /// The event raised when a TaskEntry is to be removed from its list
        /// </summary>
        public event Action<TaskEntry> Remove = (te) => { };

        /// <summary>
        /// The event raised after a TaskEntry is expanded. 
        /// </summary>
        public event Action<TaskEntry> Expand = (te) => { };

        
        //constructor(s)
        public TaskEntry(Task task)
        {
            InitializeComponent();
            this.Height = this.MinimumSize.Height;
            Task = task;

            chkCompleted.Task = task;

            btnExpire.Task = task;

            task.Entry = this;

            if (task.IsEmpty)
            {
                chkCompleted.Visible = false;
                btnMore.Visible = false;
            }

            //makes child controls transparent for mousemove
            foreach (Control c in Controls)
            {
                c.MouseEnter += (o, e) => { OnMouseEnter(e); };
                c.MouseLeave += (o, e) => { OnMouseLeave(e); };
                if (true)
                {
                    c.GotFocus += c_GotFocus;
                    c.LostFocus += c_LostFocus;
                }
            }
            txtTitle.KeyDown += (o, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                    Expanded = false;
            };

            chkCompleted.KeyDown += (o, e) =>
            {
                if (e.KeyCode == Keys.Delete)
                    Remove(this);
                else if (e.KeyCode == Keys.Enter)
                    Expanded = !Expanded;
                else if (e.KeyCode == Keys.Down && Parent is TaskEntryPanel)
                    ((TaskEntryPanel)Parent).SelectNextTaskEntry(this, true, true);
                else if (e.KeyCode == Keys.Up && Parent is TaskEntryPanel)
                    ((TaskEntryPanel)Parent).SelectNextTaskEntry(this, false, true);
            };
        }

        //public methods
        /// <summary>
        /// Sets the tooltip for this TaskEntry. 
        /// </summary>
        public void SetTooltip(ToolTip toolTip)
        {
            toolTip.SetToolTip(btnExpire, "Toggle expiration date");
            toolTip.SetToolTip(btnMore, "More");
            toolTip.SetToolTip(txtTitle, "Title");
            toolTip.SetToolTip(txtDescription, "Description");

            toolTip.Popup += toolTip_Popup;
        }


        //event handlers
        void c_LostFocus(object sender, EventArgs e)
        {
            Focused = false;
            Invalidate(true);
        }

        void c_GotFocus(object sender, EventArgs e)
        {
            Focused = true;
            Invalidate(true);
        }
        
        void btnExpire_CheckedChanged()
        {
            if (btnExpire.Checked)
            {
                txtTitle.Width -= (dueDate.Width + Offset);
                if (Task.Data.ExpirationDate.HasValue)
                    dueDate.Value = Task.Data.ExpirationDate.Value;
                else
                    Task.Data.ExpirationDate = dueDate.Value = DateTime.Today.AddDays(Settings.Default.defaultDuration);
            }
            else
            {
                txtTitle.Width += (dueDate.Width + Offset);
                Task.Data.ExpirationDate = null;
            }
            dueDate.Visible = Expanded && btnExpire.Checked;
            Invalidate(true);
        }
        
        private void btnMore_Click(object sender, EventArgs e)
        {
            Expanded = !_expanded;
        }

        private void tsDelete_Click(object sender, EventArgs e)
        {
            Remove(this);
        }

        void toolTip_Popup(object sender, PopupEventArgs e)
        {
            //if (e.AssociatedControl == this && !toolTipPopup)
            //{
            //    toolTipPopup = true;
            //    if (string.IsNullOrEmpty(Task.Description))
            //    {
            //        toolTip.SetToolTip(this, string.Empty);
            //    }
            //    else
            //    {
            //        toolTip.ToolTipTitle = Task.Text;
            //        toolTip.SetToolTip(this, Task.Description);
            //    }
            //    toolTipPopup = false;
            //}
        }


        //event overrides
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            //background
            g.Clear(BackColor);

            var txt = _expanded ? string.Empty : Task.Text;
            var txtSize = g.MeasureString(txt, Font);
            if (!chkCompleted.Visible)
                TextPosition = new PointF(Offset, Offset);
            else
                TextPosition = chkCompleted.Location.Add(chkCompleted.Width + Offset, Offset);
            var txtBrush = Task.IsEmpty ? Brushes.Black : new SolidBrush(Color.FromArgb(75, 75, 75));

            //text
            g.DrawString(txt, Font, txtBrush, new RectangleF(TextPosition, txtSize));
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            Invalidate(true);
            base.OnMouseEnter(e);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
                Expanded = true;

            base.OnMouseDoubleClick(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            Invalidate(true);
            base.OnMouseLeave(e);
        }

        public override string ToString()
        {
            return Task.Text;
        }

        //designer code
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsMore = new System.Windows.Forms.ToolStripMenuItem();
            this.tsDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.dueDate = new System.Windows.Forms.DateTimePicker();
            this.txtTitle = new System.Windows.Forms.TextBox();
            this.btnExpire = new Musa.Components.ToggleEndDateButton();
            this.chkCompleted = new Musa.Components.CheckButton();
            this.btnMore = new Musa.Components.MoreButton();
            this.contextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenu
            // 
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsMore,
            this.tsDelete});
            this.contextMenu.Name = "contextMenu";
            this.contextMenu.Size = new System.Drawing.Size(112, 48);
            // 
            // tsMore
            // 
            this.tsMore.Name = "tsMore";
            this.tsMore.Size = new System.Drawing.Size(111, 22);
            this.tsMore.Text = "More...";
            // 
            // tsDelete
            // 
            this.tsDelete.Name = "tsDelete";
            this.tsDelete.Size = new System.Drawing.Size(111, 22);
            this.tsDelete.Text = "Delete";
            this.tsDelete.Click += new System.EventHandler(this.tsDelete_Click);
            // 
            // txtDescription
            // 
            this.txtDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDescription.Location = new System.Drawing.Point(22, 24);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(332, 74);
            this.txtDescription.TabIndex = 4;
            this.txtDescription.Visible = false;
            // 
            // dueDate
            // 
            this.dueDate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.dueDate.DropDownAlign = System.Windows.Forms.LeftRightAlignment.Right;
            this.dueDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dueDate.Location = new System.Drawing.Point(251, 2);
            this.dueDate.Name = "dueDate";
            this.dueDate.Size = new System.Drawing.Size(103, 20);
            this.dueDate.TabIndex = 3;
            this.dueDate.Visible = false;
            // 
            // txtTitle
            // 
            this.txtTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTitle.Location = new System.Drawing.Point(22, 2);
            this.txtTitle.Name = "txtTitle";
            this.txtTitle.Size = new System.Drawing.Size(332, 20);
            this.txtTitle.TabIndex = 2;
            this.txtTitle.Visible = false;
            // 
            // btnExpire
            // 
            this.btnExpire.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExpire.Checked = false;
            this.btnExpire.Location = new System.Drawing.Point(364, 20);
            this.btnExpire.Margin = new System.Windows.Forms.Padding(0);
            this.btnExpire.Name = "btnExpire";
            this.btnExpire.Size = new System.Drawing.Size(20, 20);
            this.btnExpire.TabIndex = 6;
            this.btnExpire.Task = null;
            this.btnExpire.Visible = false;
            this.btnExpire.CheckedChanged += new System.Action(this.btnExpire_CheckedChanged);
            // 
            // chkCompleted
            // 
            this.chkCompleted.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.chkCompleted.BoxSize = new System.Drawing.Size(14, 14);
            this.chkCompleted.Location = new System.Drawing.Point(0, 0);
            this.chkCompleted.Margin = new System.Windows.Forms.Padding(0);
            this.chkCompleted.Name = "chkCompleted";
            this.chkCompleted.Size = new System.Drawing.Size(20, 100);
            this.chkCompleted.TabIndex = 1;
            this.chkCompleted.Task = null;
            // 
            // btnMore
            // 
            this.btnMore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMore.BoxSize = 20;
            this.btnMore.Location = new System.Drawing.Point(364, 0);
            this.btnMore.Margin = new System.Windows.Forms.Padding(0);
            this.btnMore.Name = "btnMore";
            this.btnMore.Size = new System.Drawing.Size(20, 20);
            this.btnMore.TabIndex = 5;
            this.btnMore.TabStop = false;
            this.btnMore.Task = null;
            this.btnMore.Click += new System.EventHandler(this.btnMore_Click);
            // 
            // TaskEntry
            // 
            this.ContextMenuStrip = this.contextMenu;
            this.Controls.Add(this.btnExpire);
            this.Controls.Add(this.chkCompleted);
            this.Controls.Add(this.btnMore);
            this.Controls.Add(this.txtTitle);
            this.Controls.Add(this.dueDate);
            this.Controls.Add(this.txtDescription);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.MaximumSize = new System.Drawing.Size(5000, 100);
            this.MinimumSize = new System.Drawing.Size(48, 20);
            this.Name = "TaskEntry";
            this.Size = new System.Drawing.Size(384, 100);
            this.contextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
