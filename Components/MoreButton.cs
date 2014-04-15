using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ToDo;

namespace ToDo.Components
{
    class MoreButton : TaskControl
    {
        static readonly float[] textX = new[] { 0.3f, 0.5f, 0.7f };
        const float textY = 0.7f;
        const float textS = 1.5f;

        
        public int BoxSize { get; set; }

        public MoreButton()
        {
            BoxSize = 20;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
            if (this.Focused)
                ControlPaint.DrawFocusRectangle(e.Graphics, new Rectangle(Point.Empty, this.Size));
            var pos = ((Point)this.Size).Add(-BoxSize, -BoxSize).Divide(2);

            //e.Graphics.FillRectangle(new SolidBrush(BackColor.TintMouseOver()), pos.X, pos.Y, BoxSize, BoxSize);

            //text
            for (int i = 0; i < textX.Length; i++)
                e.Graphics.FillEllipse(new SolidBrush(ForeColor),
                    pos.X + textX[i] * BoxSize - textS,
                    pos.Y + textY * BoxSize - textS,
                    textS * 2, textS * 2);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                OnClick(new EventArgs());
                return;
            }
            base.OnKeyDown(e);
        }
    }
}
