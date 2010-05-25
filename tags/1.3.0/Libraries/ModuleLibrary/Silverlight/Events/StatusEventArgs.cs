using System;
using org.iringtools.modulelibrary.types;

namespace org.iringtools.modulelibrary.events
{
  public class StatusEventArgs : EventArgs
  {
    // Status Bar properties follow:
    //----------------------------------

    /// <summary>
    /// Gets or sets the status panel to display the message in (Left, Center or Right)
    /// </summary>
    /// <value>The status.</value>
    public StatusType StatusPanel { get; set; }

    /// <summary>
    /// Gets or sets the message.
    /// </summary>
    /// <value>The message.</value>
    public string Message { get; set; }


    // Progress bar properties follow:
    //---------------------------------

    /// <summary>
    /// PROGRESS BAR: Gets or sets the total.
    /// </summary>
    /// <value>The total.</value>
    public int Total { get; set; }
    
    /// <summary>
    /// PROGRESS BAR: Gets or sets the current.
    /// </summary>
    /// <value>The current.</value>
    public int Current { get; set; }

    /// <summary>
    /// Gets or sets the is reset.
    /// </summary>
    /// <value>The is reset.</value>
    public int IsReset { get; set; }


  }
}
