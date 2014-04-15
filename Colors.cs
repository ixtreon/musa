using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDo
{
    static class Colors
    {
        public static Color ForeColor = Color.Black;

        /// <summary>
        /// Tintintintintintint!
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Color Tint(this Color c)
        {
            return c.Tint(c, 10);
        }

        public static Color Tint(this Color a, Color b, int ratio)
        {
            ratio = Math.Min(100, Math.Max(0, ratio));
            return Color.FromArgb(a.R * (100 - ratio) / 100 + b.R * ratio / 100,
                a.G * (100 - ratio) / 100 + b.G * ratio / 100,
                a.B * (100 - ratio) / 100 + b.B * ratio / 100);
        }
    }
}
