using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Musa
{
    static class NativeMethods
    {
        /// <summary>
        /// Brings the window with the specified handle to the top. 
        /// </summary>
        /// <param name="hWnd">The handle of the window to pop</param>
        /// <returns></returns>
        [DllImport("User32.dll")]
        public static extern Int32 SetForegroundWindow(int hWnd);
    }
}
