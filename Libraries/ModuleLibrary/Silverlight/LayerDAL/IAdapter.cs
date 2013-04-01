using System;
using System.Collections.Generic;

/// <summary>
/// Adapter Service WCF Interface
/// </summary>
public interface IAdapter : org.iringtools.library.IAdapter
{
  /// <summary>
  /// Occurs when [on data arrived].
  /// </summary>
  event EventHandler<EventArgs> OnDataArrived;

  /// <summary>
  /// Refreshes all.
  /// </summary>
  /// <param name="userState">State of the user.</param>
  /// <returns></returns>
  org.iringtools.library.Response RefreshAll(object userState);

  /// <summary>
  /// Generates the specified user state.
  /// </summary>
  /// <param name="userState">State of the user.</param>
  /// <returns></returns>
  org.iringtools.library.Response Generate(object userState);

  /// <summary>
  /// Gets the unit test string.
  /// </summary>
  /// <param name="valueToReturn">The value to return.</param>
  void GetUnitTestString(string valueToReturn);

  /// <summary>
  /// 
  /// </summary>
  /// <returns></returns>
  org.iringtools.library.Response GetScopes();

  /// <summary>
  /// 
  /// </summary>
  /// <param name="mapping"></param>
  /// <returns></returns>
  org.iringtools.library.Response UpdateMapping(org.iringtools.mapping.Mapping mapping);

  string GetBaseAddress { get; }

  string GetAdapterServiceUri { get; }
}
