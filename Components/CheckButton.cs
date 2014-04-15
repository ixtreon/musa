using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ToDo.Components
{
    class CheckButton : TaskControl
    {
        public const int Offset = TaskEntry.Offset;
        static readonly PointF[] TickLocation = new[] 
            {
                new PointF(0.25f, 0.30f),
                new PointF(0.50f, 0.80f),
                new PointF(1.13f, -0.17f),
            };


        public Size BoxSize { get; set; }

        public static Pen CheckBoxPen
        {
            get { return new Pen(Colors.ForeColor, 1.5f); }
        }

        public Rectangle CheckBoxRect
        {
            get
            {
                return new Rectangle(((Point)Size - BoxSize).Divide(2), BoxSize);
            }
        }


        public CheckButton()
        {
            BoxSize = new Size(16, 16);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            e.Graphics.Clear(BackColor);

            if (this.Focused)
                ControlPaint.DrawFocusRectangle(e.Graphics, new Rectangle(CheckBoxRect.Location.Add(-1, -1), new Size(CheckBoxRect.Width + 3, CheckBoxRect.Height + 3)));

            e.Graphics.DrawRectangle(CheckBoxPen, CheckBoxRect);

            if (Task == null)
                return;

            if (Task.Completed)
            {
                var pts = TickLocation.Select(pf => pf.Mult(CheckBoxRect.Size).ToPoint() + (Size)CheckBoxRect.Location).ToArray();
                var pen = new Pen(Color.DarkSlateBlue.Tint(ForeColor, 75), 2f);
                for (int i = 0; i < pts.Length - 1; i++)
                    e.Graphics.DrawLine(pen, pts[i], pts[i + 1]);
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            Task.Completed = !Task.Completed;
            base.OnMouseClick(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Task.Completed = !Task.Completed;
                return;
            }
            base.OnKeyDown(e);
        }
    }
}
