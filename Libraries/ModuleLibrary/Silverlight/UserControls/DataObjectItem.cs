
using Microsoft.Practices.Composite.Logging;
using System.Windows;
using System.Windows.Controls;
using org.iringtools.informationmodel.usercontrols;
using System;
using org.iringtools.library;


namespace org.iringtools.modulelibrary.usercontrols
{
  public class DataObjectItem : CustomTreeItem
  {

    public org.iringtools.library.DataObject DataObject { get; set; }
    public DataProperty DataProperty { get; set; }
    public string ParentObjectName { get; set; }
    public string RelationshipName { get; set; }

    public bool IsRoot()
    {
      return (Parent is TreeView);
      
    }
    /// <summary>
    /// Gets the parent.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetParent<T>()
    {
      try
      {
        return (T)Parent;
      }
      catch
      {
        return default(T);
      }
    }


    public new object Parent { get; set; }

    public override void nodeSelectedHandler(object sender, RoutedEventArgs e)
    {
    }
  }
}
