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

namespace ModuleLibrary.Events
{
  public class ListBoxEventArgs : EventArgs
  {
    public object Sender { get; set; }
    public ListBox ListBox { get; set; }
 
    public string GetUniqueID
    {
      get {
        if (Sender == null)
          return "NotDefined";
        return Sender.GetType().Name;
      }
    }
  }
}