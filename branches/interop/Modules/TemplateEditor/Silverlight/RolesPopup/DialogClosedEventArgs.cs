using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace org.iringtools.modules.templateeditor.rolespopup
{
    public class DialogClosedEventArgs : EventArgs
    {
        public bool? DialogResult { get; set; }

        public DialogClosedEventArgs(bool? result)
        {
            this.DialogResult = result;
        }
    }
}
