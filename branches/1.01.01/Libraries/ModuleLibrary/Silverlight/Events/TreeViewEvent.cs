using Microsoft.Practices.Composite.Presentation.Events;

#if SILVERLIGHT
using System.Windows.Controls;
#endif

namespace InformationModel.Events
{
  public class TreeViewEvent : CompositePresentationEvent<TreeViewEventArgs>
  {
    public TreeViewEventType EventType { get; set; }

#if SILVERLIGHT
    public TreeView TreeView { get; set; }
#endif    

  }
}
