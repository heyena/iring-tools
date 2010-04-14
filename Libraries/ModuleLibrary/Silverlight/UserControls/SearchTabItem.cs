using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Microsoft.Practices.Unity;
using PrismContrib.Errors;
using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Composite.Events;

using ModuleLibrary.Events;
using ModuleLibrary.Extensions;
using ModuleLibrary.LayerDAL;

using InformationModel.Events;
using InformationModel.Types;

using org.ids_adi.iring.referenceData;
using org.iringtools.utility;
using org.iringtools.library;

namespace InformationModel.UserControls
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

      if (canExecute !=_canExecute)
      {
        _canExecute = canExecute;
        
        if (CanExecuteChanged != null)
        {
          CanExecuteChanged(this, new EventArgs());
        }
      }

      return _canExecute;
    }

    public override void Execute(object parameter)
    {
      CompletedEventArgs = (CompletedEventArgs)parameter;

      if (CompletedEventArgs.Data.GetType().ToString().Contains("Entity"))
      {
          List<Entity> entities = (List<Entity>)CompletedEventArgs.Data;

          // Add first level Search result nodes
          foreach (Entity entity in entities)
              ContentTree.Items.Add(AddTreeItem(entity.label, entity));
      }
      else
      {
          RefDataEntities entities = (RefDataEntities)CompletedEventArgs.Data;

          // Add first level Search result nodes
          foreach (KeyValuePair<string, Entity> keyValuePair in entities)
          {
              if (keyValuePair.Value != null)
                ContentTree.Items.Add(AddTreeItem(keyValuePair.Key, keyValuePair.Value));
              else
                Error.SetError(new Exception("Search returned no results......"+Environment.NewLine + "Please try again"));
          }
      } 
    }      
  }
}
