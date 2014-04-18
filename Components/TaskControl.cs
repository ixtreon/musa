using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Musa.Components
{
    public class TaskControl : UserControl
    {
        private Task task;

        public Task Task
        {
            get { return task; }
            set
            {
                if (task != value)
                {
                    task = value;
                    OnTaskDataChanged();
                }
            }
        }

        public override Color BackColor
        {
            get
            {
                var c = Parent == null ? base.BackColor : Parent.BackColor;
                if (MouseOver)
                    return c.Tint();
                return c;
            }
        }

        protected event Action<TaskControl> TaskDataChanged = (c) => { };

        protected bool MouseOver = false;

        protected virtual void OnTaskDataChanged()
        {
            TaskDataChanged(this);
        }

        protected override bool IsInputKey(Keys keyData)
        {
            return base.IsInputKey(keyData)
                || keyData == Keys.Down
                || keyData == Keys.Up;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            MouseOver = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            MouseOver = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            Invalidate();
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            Invalidate();
            base.OnLostFocus(e);
        }
    }
}
