using System.Net;
using System;
using PrismContrib.Base;
using System.Collections.ObjectModel;
using org.iringtools.modulelibrary.types;
using org.iringtools.modulelibrary.entities;
using org.ids_adi.qmxf;
using System.Collections.Generic;
using org.iringtools.informationmodel.usercontrols;
using System.Windows.Controls;
using org.iringtools.modulelibrary.usercontrols;
using org.iringtools.library;
using org.iringtools.mapping;

namespace org.iringtools.ontologyservice.presentation.presentationmodels
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
    bool SelectedIsMapped { get; set; }
    QMXF SelectedQMXF { get; set; }
    Dictionary<string, QMXF> ClassesHistory { get; set; }
    Dictionary<string, QMXF> TemplatesHistory { get; set; }
    Dictionary<string, string> IdLabelDictionary { get; }
  }
}
