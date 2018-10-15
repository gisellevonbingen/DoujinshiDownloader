using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Giselle.DoujinshiDownloader.Forms.Utils
{
    public static class ControlUtils
    {
        public static void InvokeIfNeed(Control control, Action action)
        {
            if (control.InvokeRequired == true)
            {
                control.Invoke(action);
            }
            else
            {
                action();
            }

        }

        public static void InvokeIfNeed<E1>(Control control, Action<E1> action, E1 e1)
        {
            if (control.InvokeRequired == true)
            {
                control.Invoke(action, e1);
            }
            else
            {
                action(e1);
            }

        }

        public static void InvokeIfNeed<E1, E2>(Control control, Action<E1, E2> action, E1 e1, E2 e2)
        {
            if (control.InvokeRequired == true)
            {
                control.Invoke(action, e1, e2);
            }
            else
            {
                action(e1, e2);
            }

        }

    }

}
