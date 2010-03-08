
using System;
using System.Windows;

namespace org.iringtools.informationmodel.events
{
  public class TreeViewEventArgs : EventArgs
  {
#if SILVERLIGHT
    /// <summary>
    /// Gets or sets the routed event.
    /// </summary>
    /// <value>The routed event.</value>
    public RoutedPropertyChangedEventArgs<object> RoutedEvent {get; set; }
#endif
    /// <summary>
    /// Gets or sets the sender.
    /// </summary>
    /// <value>The sender.</value>
    public object Sender { get; set; }

    /// <summary>
    /// Gets or sets the header.
    /// </summary>
    /// <value>The header.</value>
    public string Header { get; set; }
  }
}
