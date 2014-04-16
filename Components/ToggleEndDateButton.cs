using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Musa.Properties;

namespace Musa.Components
{
    partial class ToggleEndDateButton : TaskControl
    {
        const int hourglassStiffness = 65;


        public readonly Size HourglassSize = new Size(16, 16);

        private bool _checked = false;
        public bool Checked
        {
            get
            {
                return _checked;
            }
            set
            {
                if (_checked != value)
                {
                    _checked = value;
                    CheckedChanged();
                }
            }
        }


        public Point HourglassPosition
        {
            get { return ((Point)Size - HourglassSize).Divide(2); }
        }

        public event Action CheckedChanged = () => { };

        public ToggleEndDateButton()
        {
            InitializeComponent();
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
            if (this.Focused)
                ControlPaint.DrawFocusRectangle(e.Graphics, new Rectangle(Point.Empty, this.Size));

            if (Checked)
                e.Graphics.DrawImage(Resources.clock_black, HourglassPosition);
            else
                e.Graphics.DrawImage(Resources.clock_gray, HourglassPosition);
        }

        protected override void OnTaskDataChanged()
        {
            this.Checked = Task.ExpirationDate.HasValue;

            base.OnTaskDataChanged();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (Task == null)
                return;

            Checked = !Checked;

            base.OnMouseClick(e);
        }

        internal void SetChecked(bool chkd, bool raiseEvent)
        {
            _checked = chkd;
            if (raiseEvent)
                CheckedChanged();
        }
    }
}
