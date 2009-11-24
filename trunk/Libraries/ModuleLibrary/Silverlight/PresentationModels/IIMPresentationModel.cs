using System.Net;
using System;
using PrismContrib.Base;
using org.ids_adi.iring.referenceData;
using org.ids_adi.iring;
using System.Collections.ObjectModel;
using ModuleLibrary.Types;
using ModuleLibrary.Entities;
using org.ids_adi.qmxf;
using System.Collections.Generic;
using InformationModel.UserControls;
using System.Windows.Controls;
using ModuleLibrary.UserControls;
using org.iringtools.library;

namespace OntologyService.Interface.PresentationModels
{
  public interface IIMPresentationModel : IPresentationModel
  {

    TreeView MappingTree { get; set; }

    ObservableCollection<KeyValuePair<string, string>> DetailProperties { get; set; }

    DataProperty SelectedDataSource { get; set; }
    string SelectedDataSourceDataTypeField { get; set; }
    bool SelectedDataSourceIsPropertyKey { get; set; }
    bool SelectedDataSourceIsRequiredField { get; set; }
    string SelectedDataSourcePropertyName { get; set; }

    
    Entity SelectedInformationModelNode { get; set; }
    string SelectedIMLabel { get; set; }
    string SelectedIMRepository { get; set; }
    string SelectedIMUri { get; set; }

    DataObjectItem SelectedDataObject { get; set; }
    MappingItem SelectedMappingItem { get; set; }
    NodeType SelectedNodeType { get; set; }
    GraphMap SelectedGraphMap { get; set; }
    ClassMap SelectedClassMap { get; set; }
    TemplateMap SelectedTemplateMap { get; set; }
    RoleMap SelectedRoleMap { get; set; }

    CustomTreeItem SelectedTreeItem { get; set; }

    bool SelectedIsMappable { get; set; }
    QMXF SelectedQMXF { get; set; }
    Dictionary<string, QMXF> ClassesHistory { get; set; }
    Dictionary<string, QMXF> TemplatesHistory { get; set; }

  }
}
