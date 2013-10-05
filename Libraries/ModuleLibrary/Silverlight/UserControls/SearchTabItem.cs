using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Microsoft.Practices.Unity;
using PrismContrib.Errors;
using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Composite.Events;

using org.iringtools.modulelibrary.events;
using org.iringtools.modulelibrary.extensions;
using org.iringtools.modulelibrary.layerdal;

using org.iringtools.informationmodel.events;
using org.iringtools.informationmodel.types;

using org.iringtools.utility;
using org.iringtools.library;

namespace org.iringtools.informationmodel.usercontrols
{
  public class SearchTabItem : CustomTabItem
  {

    private bool _canExecute;

    public override event EventHandler CanExecuteChanged;

    public SearchTabItem()
    {
    }

    public override bool CanExecute(object parameter)
    {
      bool canExecute = (parameter is CompletedEventArgs);

      try
      {
        if (canExecute != _canExecute)
        {
          _canExecute = canExecute;

          if (CanExecuteChanged != null)
          {
            CanExecuteChanged(this, new EventArgs());
          }
        }
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }

      return _canExecute;
    }

    public override void Execute(object parameter)
    {
      try
      {
        CompletedEventArgs = (CompletedEventArgs)parameter;

        if (CompletedEventArgs.Data.GetType().ToString().Contains("Entity"))
        {
          List<Entity> entities = (List<Entity>)CompletedEventArgs.Data;

          // Add first level Search result nodes
          foreach (Entity entity in entities)
            ContentTree.Items.Add(AddTreeItem(entity.Label, entity));
        }
        else
        {
          RefDataEntities entities = (RefDataEntities)CompletedEventArgs.Data;

          List<string> labels = new List<string>();

          // Add first level Search result nodes
          foreach (KeyValuePair<string, Entity> keyValuePair in entities.Entities)
          {
            int count = 0;
            if (keyValuePair.Value != null)
            {
              if (keyValuePair.Value.Repository.ToUpper() != "REFERENCEDATA")
              {
                var dups = from label in labels
                           where label.ToUpper() == keyValuePair.Value.Label.ToUpper()
                           select label;
                count = dups.Count();
                labels.Add(keyValuePair.Value.Label.ToUpper());
              }

              ContentTree.Items.Add(AddTreeItem(keyValuePair.Key, keyValuePair.Value, count > 0, count));
            }
            else
              Error.SetError(new Exception("Search returned no results......" + Environment.NewLine + "Please try again"));
          }
        }
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
    }
  }
}
