
using System;
using System.Collections.Generic;
using org.iringtools.adapter;
using org.iringtools.library;

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
  Response RefreshAll(object userState);

  /// <summary>
  /// Generates the specified user state.
  /// </summary>
  /// <param name="userState">State of the user.</param>
  /// <returns></returns>
  Response Generate(object userState);

  /// <summary>
  /// Gets the unit test string.
  /// </summary>
  /// <param name="valueToReturn">The value to return.</param>
  void GetUnitTestString(string valueToReturn);

  string GetAdapterServiceUri { get; }

}
