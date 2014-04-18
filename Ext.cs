using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Musa
{
    static class Ext
    {
        public static readonly bool DesignTime = (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime);

        /// <summary>
        /// Returns the euclidean distance from this point to another. 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double DistanceTo(this Point a, Point b)
        {
            var dx = a.X - b.X;
            var dy = a.Y - b.Y;
            var dsq = dx * dx + dy * dy;
            var d = Math.Sqrt(dsq);
            return d;
        }

        public static Point Add(this Point p, int dx, int dy)
        {
            return new Point(p.X + dx, p.Y + dy);
        }

        public static Point Divide(this Point p, int divisor)
        {
            return new Point(p.X / divisor, p.Y / divisor);
        }

        public static PointF Mult(this PointF p, Size mult)
        {
            return new PointF(p.X * mult.Width, p.Y * mult.Height);
        }

        public static Point ToPoint(this PointF p)
        {
            return new Point((int)p.X, (int)p.Y);
        }


        public static Rectangle ConstrainTo(this Rectangle a, Rectangle b)
        {
            a.X = Math.Min(b.Right - a.Width, Math.Max(b.Left, a.X));
            a.Y = Math.Min(b.Bottom - a.Height, Math.Max(b.Top, a.Y));
            return a;
        }

        /// <summary>
        /// Returns whether the mouse pointer is above the specified control
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public static bool IsMouseOver(this Control control)
        {
            return !DesignTime
                && control.Visible
                && control.ClientRectangle != null
                && control.ClientRectangle.Contains(control.PointToClient(Cursor.Position));
        }

        /// <summary>
        /// Creates a shortcut of the current executable at the specified location. 
        /// </summary>
        /// <param name="shortcutPath">The path to the newly created shortcut</param>
        public static void CreateShortcut(string shortcutPath)
        {
            CreateShortcut(System.Reflection.Assembly.GetExecutingAssembly().Location, shortcutPath);
        }

        /// <summary>
        /// Creates a shortcut of the specified executable at the specified location. 
        /// </summary>
        /// <param name="exePath">The path to the executable</param>
        /// <param name="shortcutPath">The path to the newly created shortcut</param>
        public static void CreateShortcut(string exePath, string shortcutPath)
        {
            using (StreamWriter writer = new StreamWriter(shortcutPath))
            {
                string app = exePath;
                writer.WriteLine("[InternetShortcut]");
                writer.WriteLine("URL=file:///" + app);
                writer.WriteLine("IconIndex=0");
                string icon = app.Replace('\\', '/');
                writer.WriteLine("IconFile=" + icon);
                writer.Flush();
            }
        }

        public static IEnumerable<T> GetControls<T>(this System.Windows.Forms.Control.ControlCollection cc, bool visibleOnly)
            where T : Control
        {
            return cc.Cast<Control>()
                    .Where(c => (c is T) && c.Visible == true)
                    .Cast<T>();
        }
    }
}
