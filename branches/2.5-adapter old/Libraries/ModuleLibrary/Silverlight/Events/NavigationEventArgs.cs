using System;
using System.Collections.Generic;
using System.Windows.Controls;

using org.iringtools.informationmodel.usercontrols;

namespace org.iringtools.informationmodel.events
{
    public enum DetailType
    {
        NotDefined, //default
        DataSource, // from upper left view
        InformationModel, // from information model
        Mapping, // Mapping region
        Pinned // from pinned region
    }

  /// <summary>
  /// Navigation Event Arguments
  /// </summary>
  public class NavigationEventArgs : EventArgs
  {

    public DetailType DetailProcess { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationEventArgs"/> class.
    /// </summary>
    public NavigationEventArgs()
    {
    }

    private string _callingProcess;
    /// <summary>
    /// Gets the calling process so that processes that use this
    /// event can distinguish between process (if required).
    /// </summary>
    /// <value>The calling process.</value>
    public string CallingProcess
    {
      get
      {
        // Default to Sender if null
        if (_callingProcess == null)
          if (Sender != null)
            _callingProcess = Sender.GetType().FullName;
        
        return _callingProcess ?? "NotAssigned";
      }
    }


    /// <summary>
    /// Gets or sets the sender.
    /// </summary>
    /// <value>The sender.</value>
    public object Sender { get; set; }

    /// <summary>
    /// Gets or sets the selected node.
    /// </summary>
    /// <value>The selected node.</value>
    public CustomTreeItem SelectedNode { get; set; }



    /// <summary>
    /// Gets or sets a value indicating whether this instance is init.
    /// </summary>
    /// <value><c>true</c> if this instance is init; otherwise, <c>false</c>.</value>
    public bool IsInit { get; set; }
  }
}
