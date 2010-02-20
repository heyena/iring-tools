using System;
using System.Collections.Generic;
using System.Linq;
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
using org.iringtools.modulelibrary.types;

using org.iringtools.informationmodel.events;
using org.iringtools.informationmodel.types;

using org.ids_adi.iring.referenceData;
using org.iringtools.utility;
using org.ids_adi.qmxf;
using org.iringtools.library;

using org.iringtools.ontologyservice.presentation.presentationmodels;

namespace org.iringtools.informationmodel.usercontrols
{
  public class RoleTreeItem : CustomTreeItem
  {
    private bool _hasExecuted = false;
    public RoleDefinition RoleDefinition { get; set; }
    public RoleQualification RoleQualification { get; set; }

    public override bool CanExecute(object parameter)
    {
      return !_hasExecuted;
    }

    public override void Execute(object parameter)
    {
      CompletedEventArgs = (CompletedEventArgs)parameter;

      if (CompletedEventArgs.CheckForType(CompletedEventType.GetClass))
      {
        QMXF qmxf = (QMXF)CompletedEventArgs.Data;
        ClassDefinition classDefinition = qmxf.classDefinitions.FirstOrDefault();

        if (classDefinition != null)
        {
          Entity entity = new Entity()
          {
            label = classDefinition.name[0].value,
            repository = "UnKnown",
            uri = classDefinition.identifier
          };

          ClassTreeItem item = (ClassTreeItem)AddTreeItem(entity.label, entity);
          item.ProcessClassDefinition(classDefinition);
          this.Items.Add(item);
          _hasExecuted = true;
        }

        //if (this.IsSelectionActive)
        //{
          UpdateModel(qmxf);
        //}
      }
      else if (CompletedEventArgs.CheckForType(CompletedEventType.GetClassLabel))
      {
        DisplayAndSaveLabel(CompletedEventArgs.Data);
      }
    }

    public override void nodeMouseLeftButtonUpHandler(object sender, MouseButtonEventArgs e)
    {
        if (!_hasExecuted && RoleQualification != null)
        {
            if (RoleQualification.range != null &&
                RoleQualification.range != String.Empty &&
                !RoleQualification.range.IsMappable())
            {
                ReferenceDataService.GetClass(RoleQualification.range.GetIdFromUri(), this);
            }
        }
        else if (!_hasExecuted && RoleDefinition != null)
        {
            if (RoleDefinition.range != null &&
                    RoleDefinition.range != string.Empty &&
                    !RoleDefinition.range.IsMappable())
            {
                ReferenceDataService.GetClass(RoleDefinition.range.GetIdFromUri(), this);
            }
        }

      FillRoleDetailView();
      e.Handled = true;
    }

    private void FillRoleDetailView()
    {
      PresentationModel.SelectedTreeItem = this;
      PresentationModel.DetailProperties.Clear();

      if (Tag == null)
        return;

      if (Tag is Entity)
      {
        Entity entity = (Entity)Tag;
        KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>("Label", entity.label);
        PresentationModel.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Uri", entity.uri);
        PresentationModel.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Repository", entity.repository);
        PresentationModel.DetailProperties.Add(keyValuePair);
      }
      else if (Tag is RoleDefinition)
      {
        RoleDefinition roleDefinition = (RoleDefinition)Tag;

        KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>("QMXF Type", "Role Definition");
        PresentationModel.DetailProperties.Add(keyValuePair);

        keyValuePair = new KeyValuePair<string, string>("Name", (roleDefinition.name.FirstOrDefault() != null ? roleDefinition.name.FirstOrDefault().value : string.Empty));
        PresentationModel.DetailProperties.Add(keyValuePair);

        keyValuePair = new KeyValuePair<string, string>("Identifier", (roleDefinition.identifier != null ? roleDefinition.identifier.ToString() : string.Empty));
        PresentationModel.DetailProperties.Add(keyValuePair);

        keyValuePair = new KeyValuePair<string, string>("Description", roleDefinition.description.value);
        PresentationModel.DetailProperties.Add(keyValuePair);

        GetClassLabel("Range", roleDefinition.range);

        keyValuePair = new KeyValuePair<string, string>("Inverse Minimum", roleDefinition.inverseMinimum);
        PresentationModel.DetailProperties.Add(keyValuePair);

        keyValuePair = new KeyValuePair<string, string>("Inverse Maximum", roleDefinition.inverseMaximum);
        PresentationModel.DetailProperties.Add(keyValuePair);

        keyValuePair = new KeyValuePair<string, string>("Minimum", roleDefinition.minimum);
        PresentationModel.DetailProperties.Add(keyValuePair);

        keyValuePair = new KeyValuePair<string, string>("Maximum", roleDefinition.inverseMaximum);
        PresentationModel.DetailProperties.Add(keyValuePair);
        
      }
      else if (Tag is RoleQualification)
      {
        RoleQualification roleQualification = (RoleQualification)Tag;

        KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>("QMXF Type", "Role Qualification");
        PresentationModel.DetailProperties.Add(keyValuePair);

        keyValuePair = new KeyValuePair<string, string>("Name", (roleQualification.name.FirstOrDefault() != null ? roleQualification.name.FirstOrDefault().value : string.Empty));
        PresentationModel.DetailProperties.Add(keyValuePair);

        keyValuePair = new KeyValuePair<string, string>("Description", (roleQualification.description.FirstOrDefault() != null ? roleQualification.description.FirstOrDefault().value : string.Empty));
        PresentationModel.DetailProperties.Add(keyValuePair);

        GetClassLabel("Qualifies", roleQualification.qualifies);

        string value = String.Empty;
        if (roleQualification.value != null)
        {
          string text = roleQualification.value.text;
          string reference = roleQualification.value.reference;
          if (text != null)
            value = roleQualification.value.text;
          if (reference != null)
            value = roleQualification.value.reference;
        }
        if(!string.IsNullOrEmpty(value)) //This is due to data error??
          GetClassLabel("Value", value);

        if (!string.IsNullOrEmpty(roleQualification.range))
          GetClassLabel("Range", roleQualification.range);

        keyValuePair = new KeyValuePair<string, string>("Inverse Minimum", roleQualification.inverseMinimum);
        PresentationModel.DetailProperties.Add(keyValuePair);

        keyValuePair = new KeyValuePair<string, string>("Inverse Maximum", roleQualification.inverseMaximum);
        PresentationModel.DetailProperties.Add(keyValuePair);

        keyValuePair = new KeyValuePair<string, string>("Minimum", roleQualification.minimum);
        PresentationModel.DetailProperties.Add(keyValuePair);

        keyValuePair = new KeyValuePair<string, string>("Maximum", roleQualification.maximum);
        PresentationModel.DetailProperties.Add(keyValuePair);
      }
    }
  }
}
