using OntologyService.Interface;
using PrismContrib.Base;
using System.ComponentModel;
using OntologyService.Interface.PresentationModels;
using System.Windows.Controls;
using Microsoft.Practices.Composite.Events;
using InformationModel.Events;
using System.Collections.Generic;
using org.ids_adi.iring;
using System.Text;
using ModuleLibrary.Events;
using org.ids_adi.iring.referenceData;
using InformationModel.Entities;
using org.ids_adi.qmxf;
using System.Linq;

namespace InformationModel.Views.ClassDetails
{
  public class DetailPresenter : PresenterBase<IDetailView>
  {
    private IIMPresentationModel model = null;
    private IEventAggregator aggregator = null;


    /// <summary>
    /// Gets the details grid.
    /// </summary>
    /// <value>The grid details.</value>
    private Grid grdDetails { get { return GridCtrl("dg"); } }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClassDetailPresenter"/> class.
    /// </summary>
    /// <param name="view">The view.</param>
    /// <param name="model">The model.</param>
    public DetailPresenter(IDetailView view, IIMPresentationModel model,
      IEventAggregator aggregator)
      : base(view, model)
    {
      this.aggregator = aggregator;
      this.model = model;

      aggregator.GetEvent<NavigationEvent>().Subscribe(NavigationEventHandler);

    }


    public void NavigationEventHandler(NavigationEventArgs e)
    {
      // Conditional Statement based on NavigationEventArgs content

      model.DetailProperties.Clear();
      object tag = e.SelectedNode.Tag;

      //if (tag is DataObject)
      //{
      //  DataObject dataObject = (DataObject)tag;
      //  KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>("DataObject Name", dataObject.objectName);
      //  model.DetailProperties.Add(keyValuePair);

      //}

      //if (tag is DataProperty)
      //{
      //  DataProperty dataProperty = (DataProperty)tag;
      //  KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>("Property Name", dataProperty.propertyName);
      //  model.DetailProperties.Add(keyValuePair);
      //  keyValuePair = new KeyValuePair<string, string>("Datatype", dataProperty.dataType);
      //  model.DetailProperties.Add(keyValuePair);
      //  keyValuePair = new KeyValuePair<string, string>("Is Required", dataProperty.isRequired.ToString());
      //  model.DetailProperties.Add(keyValuePair);
      //  keyValuePair = new KeyValuePair<string, string>("Is Key", dataProperty.isPropertyKey.ToString());
      //  model.DetailProperties.Add(keyValuePair);
      //}

      //if (tag is DataRelationship)
      //{
      //  DataRelationship dataRelationship = (DataRelationship)tag;
      //  KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>("Related Graph", dataRelationship.relatedObject);
      //  model.DetailProperties.Add(keyValuePair);
      //  keyValuePair = new KeyValuePair<string, string>("Graph Property", dataRelationship.graphProperty);
      //  model.DetailProperties.Add(keyValuePair);
      //  keyValuePair = new KeyValuePair<string, string>("Cardinality", dataRelationship.cardinality.ToString());
      //  model.DetailProperties.Add(keyValuePair);
      //}

      if (tag is ClassMap)
      {
        ClassMap classMap = (ClassMap)tag;
        KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>("Class Name", classMap.name);
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Class Id", classMap.classId);
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Identifier", classMap.identifier);
        model.DetailProperties.Add(keyValuePair);
      }
      if (tag is TemplateMap)
      {
        TemplateMap templateMap = (TemplateMap)tag;
        KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>("Template Name", templateMap.name);
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Template Id", templateMap.templateId);
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Class Role", templateMap.classRole);
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Type", templateMap.type.ToString());
        model.DetailProperties.Add(keyValuePair);
      }
      if (tag is RoleMap)
      {
        RoleMap roleMap = (RoleMap)tag;
        KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>("Role Name", roleMap.name);
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Role Id", roleMap.roleId);
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Property Name", roleMap.propertyName);
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Datatype", roleMap.dataType);
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("ValueList", roleMap.valueList);
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Class", roleMap.classMap.ToString());
        model.DetailProperties.Add(keyValuePair);
      }
      if (tag is Entity)
      {
        Entity entity = (Entity)tag;
        KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>("Label", entity.label);
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Uri", entity.uri);
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Repository", entity.repository);
        model.DetailProperties.Add(keyValuePair);
      }

      if (tag is ClassDefinition)
      {
        ClassDefinition classDefinition = (ClassDefinition)tag;
        KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>("QMXF Type", "Class Definition");
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Name", (classDefinition.name.FirstOrDefault() != null ? classDefinition.name.FirstOrDefault().value : string.Empty));
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Identifier", classDefinition.identifier);
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Entity Type", classDefinition.entityType.reference);
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Designation", classDefinition.designation.value);
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Classification Label", (classDefinition.classification.FirstOrDefault() != null ? classDefinition.classification.FirstOrDefault().label : string.Empty));
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Classification Reference", (classDefinition.classification.FirstOrDefault() != null ? classDefinition.classification.FirstOrDefault().reference : string.Empty));
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Description", (classDefinition.description.FirstOrDefault() != null ? classDefinition.description.FirstOrDefault().value : string.Empty));
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Specialization Label", (classDefinition.specialization.FirstOrDefault() != null ? classDefinition.specialization.FirstOrDefault().label : string.Empty));
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Specialization Reference", (classDefinition.specialization.FirstOrDefault() != null ? classDefinition.specialization.FirstOrDefault().reference : string.Empty));
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Status Class", (classDefinition.status.FirstOrDefault() != null ? classDefinition.status.FirstOrDefault().Class : string.Empty));
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Status Authority", (classDefinition.status.FirstOrDefault() != null ? classDefinition.status.FirstOrDefault().authority : string.Empty));
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Status From", (classDefinition.status.FirstOrDefault() != null ? classDefinition.status.FirstOrDefault().from : string.Empty));
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Status To", (classDefinition.status.FirstOrDefault() != null ? classDefinition.status.FirstOrDefault().to : string.Empty));
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Suggested Designation Value", (classDefinition.suggestedDesignation.FirstOrDefault() != null ? classDefinition.suggestedDesignation.FirstOrDefault().value : string.Empty));
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Texual Definition Value", (classDefinition.textualDefinition.FirstOrDefault() != null ? classDefinition.textualDefinition.FirstOrDefault().value : string.Empty));
        model.DetailProperties.Add(keyValuePair);
      }

      if (tag is TemplateDefinition)
      {
        TemplateDefinition templateDefinition = (TemplateDefinition)tag;
        KeyValuePair<string, string> list = new KeyValuePair<string, string>("QMXF Type", "Template Definition");
        model.DetailProperties.Add(list);
        list = new KeyValuePair<string, string>("Name", (templateDefinition.name.FirstOrDefault() != null ? templateDefinition.name.FirstOrDefault().value : string.Empty));
        model.DetailProperties.Add(list);
        list = new KeyValuePair<string, string>("Identifier", templateDefinition.identifier);
        model.DetailProperties.Add(list);
        list = new KeyValuePair<string, string>("Designation", templateDefinition.designation.value);
        model.DetailProperties.Add(list);
        list = new KeyValuePair<string, string>("Description", (templateDefinition.description.FirstOrDefault() != null ? templateDefinition.description.FirstOrDefault().value : string.Empty));
        model.DetailProperties.Add(list);
        list = new KeyValuePair<string, string>("Status Class", (templateDefinition.status.FirstOrDefault() != null ? templateDefinition.status.FirstOrDefault().Class : string.Empty));
        model.DetailProperties.Add(list);
        list = new KeyValuePair<string, string>("Status Authority", (templateDefinition.status.FirstOrDefault() != null ? templateDefinition.status.FirstOrDefault().authority : string.Empty));
        model.DetailProperties.Add(list);
        list = new KeyValuePair<string, string>("Status From", (templateDefinition.status.FirstOrDefault() != null ? templateDefinition.status.FirstOrDefault().from : string.Empty));
        model.DetailProperties.Add(list);
        list = new KeyValuePair<string, string>("Status To", (templateDefinition.status.FirstOrDefault() != null ? templateDefinition.status.FirstOrDefault().to : string.Empty));
        model.DetailProperties.Add(list);
        list = new KeyValuePair<string, string>("Suggested Designation Value", (templateDefinition.suggestedDesignation.FirstOrDefault() != null ? templateDefinition.suggestedDesignation.FirstOrDefault().value : string.Empty));
        model.DetailProperties.Add(list);
        list = new KeyValuePair<string, string>("Texual Definition Value", (templateDefinition.textualDefinition.FirstOrDefault() != null ? templateDefinition.textualDefinition.FirstOrDefault().value : string.Empty));
        model.DetailProperties.Add(list);
      }

      if (tag is TemplateQualification)
      {
        TemplateQualification templateQualification = (TemplateQualification)tag;
        KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>("QMXF Type", "Template Qualification");
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Name", (templateQualification.name.FirstOrDefault() != null ? templateQualification.name.FirstOrDefault().value : string.Empty));
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Identifier", templateQualification.identifier);
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Designation", (templateQualification.designation.FirstOrDefault() != null ? templateQualification.designation.FirstOrDefault().value : string.Empty));
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Description", (templateQualification.description.FirstOrDefault() != null ? templateQualification.description.FirstOrDefault().value : string.Empty));
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Status Class", (templateQualification.status.FirstOrDefault() != null ? templateQualification.status.FirstOrDefault().Class : string.Empty));
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Status Authority", (templateQualification.status.FirstOrDefault() != null ? templateQualification.status.FirstOrDefault().authority : string.Empty));
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Status From", (templateQualification.status.FirstOrDefault() != null ? templateQualification.status.FirstOrDefault().from : string.Empty));
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Status To", (templateQualification.status.FirstOrDefault() != null ? templateQualification.status.FirstOrDefault().to : string.Empty));
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Suggested Designation", (templateQualification.suggestedDesignation.FirstOrDefault() != null ? templateQualification.suggestedDesignation.FirstOrDefault().value : string.Empty));
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Texual Definition", (templateQualification.textualDefinition.FirstOrDefault() != null ? templateQualification.textualDefinition.FirstOrDefault().value : string.Empty));
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Qualifies", templateQualification.qualifies);
        model.DetailProperties.Add(keyValuePair);
      }

      if (tag is RoleDefinition)
      {
        RoleDefinition roleDefinition = (RoleDefinition)tag;
        KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>("QMXF Type", "Role Definition");
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Name", (roleDefinition.name.FirstOrDefault() != null ? roleDefinition.name.FirstOrDefault().value : string.Empty));
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Identifier", roleDefinition.identifier);
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Designation", roleDefinition.designation.value);
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Description", roleDefinition.description.value);
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Inverse Minimum", roleDefinition.inverseMinimum);
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Inverse Maximum", roleDefinition.inverseMaximum);
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Minimum", roleDefinition.minimum);
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Maximum", roleDefinition.inverseMaximum);
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Range", roleDefinition.range);
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Suggested Designation", (roleDefinition.suggestedDesignation.FirstOrDefault() != null ? roleDefinition.suggestedDesignation.FirstOrDefault().value : string.Empty));
        model.DetailProperties.Add(keyValuePair);
      }

      if (tag is RoleQualification)
      {
        RoleQualification roleQualification = (RoleQualification)tag;
        KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>("QMXF Type", "Role Qualification");
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Name", (roleQualification.name.FirstOrDefault() != null ? roleQualification.name.FirstOrDefault().value : string.Empty));
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Value", roleQualification.value.ToString());
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Description", (roleQualification.description.FirstOrDefault() != null ? roleQualification.description.FirstOrDefault().value : string.Empty));
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Inverse Minimum", roleQualification.inverseMinimum);
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Inverse Maximum", roleQualification.inverseMaximum);
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Minimum", roleQualification.minimum);
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Maximum", roleQualification.maximum);
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Range", roleQualification.range);
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Qualifies", roleQualification.qualifies);
        model.DetailProperties.Add(keyValuePair);
      }
    }
  }
}
