using System;
using System.Windows.Controls;

namespace org.iringtools.modulelibrary.events
{
  /// <summary>
  /// Event Argument for Button Event
  /// </summary>
  public class ButtonEventArgs : EventArgs
  {
    /// <summary>
    /// Gets or sets the button clicked.
    /// </summary>
    /// <value>The button clicked.</value>
    public Button ButtonClicked { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ButtonEventArgs"/> class.
    /// </summary>
    public ButtonEventArgs() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ButtonEventArgs"/> class.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="buttonClicked">The button clicked.</param>
    public ButtonEventArgs(object sender, Button buttonClicked)
    {
      Sender = sender;
      ButtonClicked = buttonClicked;
    }

    /// <summary>
    /// Gets the tag
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetTag<T>()
    {
      return (T)ButtonClicked.Tag;
    }

    /// <summary>
    /// Gets unique name for the control.
    /// </summary>
    /// <returns></returns>
    public string GetUniqueName()
    {
      return string.Format("{0}:{1}", Sender??"NotDefined", Name);
    }

    /// <summary>
    /// Gets or sets the sender (button names could be the
    /// same across different senders).
    /// </summary>
    /// <value>The sender.</value>
    public object Sender { get; set; }

    /// <summary>
    /// Gets the name of the button control.
    /// </summary>
    /// <value>The name.</value>
    public object Name
    {
      get
      {
        return ButtonClicked.Name;
      }
    }
  }
}
