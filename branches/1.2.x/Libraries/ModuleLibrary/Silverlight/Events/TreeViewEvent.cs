using Microsoft.Practices.Composite.Presentation.Events;

#if SILVERLIGHT
using System.Windows.Controls;
#endif

namespace org.iringtools.informationmodel.events
{
  public class TreeViewEvent : CompositePresentationEvent<TreeViewEventArgs>
  {
    public TreeViewEventType EventType { get; set; }

#if SILVERLIGHT
    public TreeView TreeView { get; set; }
#endif    

  }
}
