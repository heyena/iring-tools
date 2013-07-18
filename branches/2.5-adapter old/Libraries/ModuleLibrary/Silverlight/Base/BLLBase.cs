using System;

namespace org.iringtools.modulelibrary.baseclass
{
  public class BLLBase
  {

    /// <summary>
    /// Determines whether [is supported event] [the specified completed event arg].
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="completedEventArg">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    /// <returns>
    /// 	<c>true</c> if [is supported event] [the specified completed event arg]; otherwise, <c>false</c>.
    /// </returns>
    protected bool CheckClassTypeFor<T>(EventArgs completedEventArg)
    {
      return (completedEventArg is T);
    }
  }
}
