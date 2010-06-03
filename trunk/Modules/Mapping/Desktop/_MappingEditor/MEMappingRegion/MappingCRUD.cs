using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using org.iringtools.informationmodel.events;
using PrismContrib.Errors;

using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Unity;

using InformationModel.Events;

using org.iringtools.modulelibrary.events;
using org.iringtools.modulelibrary.extensions;
using org.iringtools.modulelibrary.entities;
using org.iringtools.modulelibrary.layerdal;
using org.iringtools.modulelibrary.types;

using org.iringtools.ontologyservice.presentation;
using org.iringtools.ontologyservice.presentation.presentationmodels;

using org.ids_adi.iring;
using org.ids_adi.iring.referenceData;
using org.ids_adi.qmxf;


using org.iringtools.informationmodel.usercontrols;
using org.iringtools.library;
using org.iringtools.utility;
using System.Text.RegularExpressions;

namespace org.iringtools.modules.memappingregion
{
    public class MappingCRUD
    {
        private IIMPresentationModel model = null;
        private IMappingView view = null;
        private IAdapter adapterProxy = null;
        private IReferenceData referenceProxy = null;
        private IEventAggregator aggregator = null;
        private string projectName = null;
        private string applicationName = null;

        public MappingPresenter Presenter { get; set; }
        public TreeView tvwMapping { get; set; }
        public Mapping mapping { get; set; }

        [Dependency]
        public IError Error { get; set; }

        [Dependency]
        public ILoggerFacade Logger { get; set; }

        public MappingCRUD(IMappingView view, IIMPresentationModel model,
            IAdapter adapterProxy,
            IReferenceData referenceProxy,
            IEventAggregator aggregator)
        {
            this.model = model;
            this.view = view;
            this.adapterProxy = adapterProxy;
            this.referenceProxy = referenceProxy;
            this.aggregator = aggregator;
            aggregator.GetEvent<SpinnerEvent>().Subscribe(SpinnerEventHandler);
            aggregator.GetEvent<SelectionEvent>().Subscribe(SelectionEventHandler);
        }

        public void SelectionEventHandler(SelectionEventArgs e)
        {
            this.projectName = e.SelectedProject;
            this.applicationName = e.SelectedApplication;
        }

        public void SpinnerEventHandler(SpinnerEventArgs e)
        {
            switch (e.Active)
            {
                case SpinnerEventType.Started:
                    this.tvwMapping.IsEnabled = false;

                    break;

                case SpinnerEventType.Stopped:
                    this.tvwMapping.IsEnabled = true;
                    this.tvwMapping.IsEnabled = true;

                    break;

                default:
                    break;
            }
        }

        public void btnAddGraph_Click(object sender, RoutedEventArgs e)
        {
            TextBox txtLabel = sender as TextBox;
            string graphName = txtLabel.Text;

            if (!Regex.IsMatch(graphName, @"^[A-Za-z_]+\w*$"))
            {
                MessageBox.Show("Graph name is invalid", "ADD GRAPH", MessageBoxButton.OK);
            }
            else if (model.SelectedIMUri == null ||
                SPARQLExtensions.GetObjectTypeFromUri(model.SelectedIMUri) != SPARQLPrefix.ObjectType.Class)
            {
                MessageBox.Show("Please select a valid class", "ADD GRAPH", MessageBoxButton.OK);
            }
            else if (model.SelectedDataObject == null || model.SelectedDataObject.DataProperty == null)
            {
                MessageBox.Show("Please select an identifier", "ADD GRAPH", MessageBoxButton.OK);
            }
            else
            {
                GraphMap graphMap = new GraphMap();
                graphMap.name = graphName;
                //TODO: graphMap.GraphUri to be constructed here
                //graphMap.uri = SPARQLExtensions.GetIdWithAliasFromUri(model.SelectedIMUri);

                if (graphMap.dataObjectMaps == null)
                    graphMap.dataObjectMaps = new List<DataObjectMap>();

                DataObjectMap dataObjectMap = new DataObjectMap
                {
                    name = model.SelectedDataObject.DataObject.objectName,
                    inFilter = "",
                    outFilter = ""

                };
                ClassMap classMap = new ClassMap
                    {
                        name = model.SelectedIMLabel,
                        classId = SPARQLExtensions.GetIdWithAliasFromUri(model.SelectedIMUri),
                        identifierValue = "",
                        identifierDelimeter = ""
                    };
                classMap.identifiers.Add(model.SelectedDataObject.DataProperty.propertyName);

                graphMap.AddClassMap(null, classMap);

                graphMap.dataObjectMaps.Add(dataObjectMap);
                mapping.graphMaps.Add(graphMap);
                tvwMapping.Items.Add(Presenter.AddNode(graphMap.name, graphMap, null));
            }
        }

        private string AdjustTemplateName(MappingItem mappingItem, string templateName)
        {
            string tplName = utility.Utility.NameSafe(templateName);

            if (mappingItem.ClassMap != null && mappingItem.TemplateMap != null)
            {
                int lastSeqNum = 0;

                // find last sequence number of template names that match the new template name
                TemplateMap templateMap = mappingItem.TemplateMap;
                {
                    if (templateMap.name.StartsWith(tplName))
                    {
                        if (templateMap.name.Length == tplName.Length)
                        {
                            lastSeqNum = 1;
                        }
                        else
                        {
                            int seqNum;
                            if (int.TryParse(templateMap.name.Substring(tplName.Length), out seqNum))
                            {
                                if (seqNum >= lastSeqNum)
                                    lastSeqNum = seqNum + 1;
                            }
                        }
                    }
                }

                if (lastSeqNum != 0)
                    tplName += lastSeqNum;
            }

            return tplName;
        }

        public void btnAddTemplate_Click(object sender, RoutedEventArgs e)
        {

            string classId = string.Empty;
            if (model.SelectedIMUri == null ||
                SPARQLExtensions.GetObjectTypeFromUri(model.SelectedIMUri) != SPARQLPrefix.ObjectType.Template)
            {
                MessageBox.Show("Please select a valid template", "ADD TEMPLATE", MessageBoxButton.OK);
                return;
            }
            else if (model.SelectedMappingItem == null || (
                model.SelectedMappingItem.NodeType != NodeType.GraphMap &&
                model.SelectedMappingItem.NodeType != NodeType.ClassMap))
            {
                MessageBox.Show("Please select a graph map or class map", "ADD TEMPLATE", MessageBoxButton.OK);
                return;
            }
            else
            {
                if (model.SelectedMappingItem.NodeType == NodeType.GraphMap)
                    classId = model.SelectedGraphMap.classTemplateListMaps.Keys.FirstOrDefault().classId;
                
                else if (model.SelectedMappingItem.NodeType == NodeType.ClassMap)
                    classId = model.SelectedClassMap.classId;

                MappingItem mappingItem = model.SelectedMappingItem;

                TextBox txtLabel = sender as TextBox;
                TemplateMap templateMap = new TemplateMap();

                templateMap.name = AdjustTemplateName(mappingItem, model.SelectedIMLabel);
                templateMap.templateId = SPARQLExtensions.GetIdWithAliasFromUri(model.SelectedIMUri);

                TemplateTreeItem node = (TemplateTreeItem)model.SelectedTreeItem;

                TemplateDefinition templateDefinition = node.TemplateDefinition;
                TemplateQualification templateQualification = node.TemplateQualification;

                object template = null;
                if (templateDefinition != null)
                {
                    template = templateDefinition;
                }
                else
                {
                    template = templateQualification;
                }

                //bool isClassRoleFound = GetClassRole(classId, template, templateMap);

                //if (!isClassRoleFound)
                //{
                //if (model.ClassesHistory != null)
                // {
                //    foreach (KeyValuePair<string, QMXF> keyValuePair in model.ClassesHistory)
                //    {
                //      QMXF classQmxf = keyValuePair.Value;
                //      if (classId == keyValuePair.Key)
                //      {
                //        List<Specialization> specializations = classQmxf.classDefinitions.FirstOrDefault().specialization;
                //        foreach (Specialization specialization in specializations)
                //        {
                //          isClassRoleFound = GetClassRole(specialization.reference, template, templateMap);
                //          if (isClassRoleFound) break;
                //        }
                //      }
                //    }
                //  }
                //}
                //if (!isClassRoleFound)
                //{
                //    QMXF classQmxf = referenceProxy.GetClass(classId, this);
                //    foreach (ClassDefinition classDefinition in classQmxf.classDefinitions)
                //    {
                //        foreach (Specialization specialization in classDefinition.specialization)
                //        {
                //            isClassRoleFound = GetClassRole(specialization.reference, template, templateMap);
                //            if (isClassRoleFound) break;
                //        }
                //    }
                //}

                GetRoleMaps(classId, template, templateMap);
                //GetTemplateType(template, templateMap);

                if (mappingItem.TemplateMap == null)
                    mappingItem.TemplateMap = new TemplateMap();
                mappingItem.TemplateMap = templateMap;

                model.SelectedGraphMap.AddTemplateMap(model.SelectedClassMap, templateMap);
                Presenter.PopulateTemplateMap(mappingItem, templateMap);
            }
        }


        private string HandleSpecialCharacters(string inputString)
        {
            string pattern = "[^a-zA-Z]";
            string outputString =
            System.Text.RegularExpressions.Regex.Replace(inputString, pattern, "");
            return outputString;
        }

        //private bool GetClassRole(string classId, object template, TemplateMap currentTemplateMap)
        //{
        //  bool isClassRoleFound = false;

        //  if (template is TemplateDefinition)
        //  {
        //    TemplateDefinition templateDefinition = (TemplateDefinition)template;
        //    List<RoleDefinition> roleDefinitions = templateDefinition.roleDefinition;

        //    foreach (RoleDefinition roleDefinition in roleDefinitions)
        //    {
        //      string range = roleDefinition.range.GetIdWithAliasFromUri();
        //      if (range == classId)
        //      {
        //        RoleMap rm = currentTemplateMap.roleMaps.Where(c=>c.type == RoleType.ClassRole).First();
        //        string test    = roleDefinition.identifier.GetIdWithAliasFromUri();
        //        isClassRoleFound = true;
        //        break;
        //      }
        //    }
        //  }

        //  if (template is TemplateQualification)
        //  {
        //    TemplateQualification templateQualification = (TemplateQualification)template;

        //    foreach (RoleQualification roleQualification in templateQualification.roleQualification)
        //    {
        //      string range = roleQualification.range.GetIdWithAliasFromUri();
        //      if (range == classId)
        //      {
        //       // currentTemplateMap.classRole = roleQualification.qualifies.GetIdWithAliasFromUri();
        //        isClassRoleFound = true;
        //        break;
        //      }
        //    }
        //  }

        //      return isClassRoleFound;
        //}

        private void GetRoleMaps(string classId, object template, TemplateMap currentTemplateMap)
        {
            if (currentTemplateMap.roleMaps == null)
                currentTemplateMap.roleMaps = new List<RoleMap>();
            //TODO: need to refactor TemplateDefinition still
            if (template is TemplateDefinition)
            {
                TemplateDefinition templateDefinition = (TemplateDefinition)template;
                List<RoleDefinition> roleDefinitions = templateDefinition.roleDefinition;

                foreach (RoleDefinition roleDefinition in roleDefinitions)
                {
                    string range = roleDefinition.range.GetIdWithAliasFromUri();

                    if (range != String.Empty && range != classId)
                    {
                        RoleMap roleMap = new RoleMap
                        {
                            name = utility.Utility.NameSafe(roleDefinition.name.FirstOrDefault().value),
                            dataType = range,
                            propertyName = "",
                            roleId = roleDefinition.identifier.GetIdWithAliasFromUri()
                        };

                        currentTemplateMap.roleMaps.Add(roleMap);
                    }

                    if (range == String.Empty && range != classId)
                    {
                        RoleMap roleMap = new RoleMap
                        {
                            name = utility.Utility.NameSafe(roleDefinition.name.FirstOrDefault().value),
                            dataType = range,
                            propertyName = "",
                            roleId = roleDefinition.identifier.GetIdWithAliasFromUri()
                        };

                        currentTemplateMap.roleMaps.Add(roleMap);
                    }
                }
            }
            if (template is TemplateQualification)
            {
                TemplateQualification templateQualification = (TemplateQualification)template;
                List<RoleQualification> roleQualifications = templateQualification.roleQualification;

                foreach (RoleQualification roleQualification in roleQualifications)
                {
                    string range = roleQualification.range.GetIdWithAliasFromUri();
                    RoleMap roleMap = new RoleMap();

                    roleMap.name = utility.Utility.NameSafe(roleQualification.name.FirstOrDefault().value);
                    roleMap.roleId = roleQualification.qualifies.GetIdWithAliasFromUri();

                    if (roleQualification.value != null)  // fixed role
                    {
                        if (!String.IsNullOrEmpty(roleQualification.value.reference))//&& roleMap.type == RoleType.Reference)  // fixed role is a reference
                        {
                            roleMap.type = RoleType.Reference;
                            roleMap.value = roleQualification.value.reference.GetIdWithAliasFromUri();
                        }
                        else if (!String.IsNullOrEmpty(roleQualification.value.text))  // fixed role is a literal
                        {
                            roleMap.type = RoleType.FixedValue;
                            roleMap.value = roleQualification.value.text;
                            roleMap.dataType = roleQualification.value.As;
                        }

                        currentTemplateMap.roleMaps.Add(roleMap);
                    }
                    else if (range.Contains("xsd:"))// != classId)  // property role
                    {
                        roleMap.type = RoleType.Property;
                        roleMap.dataType = range;
                        roleMap.propertyName = String.Empty;
                        currentTemplateMap.roleMaps.Add(roleMap);
                    }
                    else if (range.Contains("rdl:"))
                    {
                        roleMap.type = RoleType.ClassRole;

                        roleMap.dataType = range;
                        currentTemplateMap.roleMaps.Add(roleMap);
                    }
                }
            }
        }

        private void GetTemplateType(object template, TemplateMap currentTemplateMap)
        {
            bool isMappable = false;
            if (template is TemplateDefinition)
            {
                TemplateDefinition templateDefinition = (TemplateDefinition)template;

                List<RoleDefinition> roleDefinitions = templateDefinition.roleDefinition;
                foreach (RoleDefinition roleDefinition in roleDefinitions)
                {
                    isMappable = roleDefinition.range.IsMappable();
                    if (isMappable)
                        break;
                }
            }
            if (template is TemplateQualification)
            {
                TemplateQualification templateQualification = (TemplateQualification)template;

                List<RoleQualification> roleQulalifications = templateQualification.roleQualification;
                foreach (RoleQualification roleQualification in roleQulalifications)
                {
                    isMappable = roleQualification.range.IsMappable();
                    if (isMappable)
                        break;
                }
            }

            //if (isMappable)
            //  currentTemplateMap.type = TemplateType.Property;
            //else
            //  currentTemplateMap.type = TemplateType.Relationship;
        }

        public void btnMap_Click(object sender, RoutedEventArgs e)
        {
            if (model.SelectedRoleMap == null)
            {
                MessageBox.Show("Please select a role map", "MAP ROLE", MessageBoxButton.OK);
                return;
            }

            MappingItem mappingItem = model.SelectedMappingItem;

            RoleMap roleMap = model.SelectedRoleMap;
            if (roleMap.type == RoleType.Property)
            {
                if (roleMap.dataType == null && !roleMap.dataType.StartsWith("xsd:"))
                {
                    if (model.SelectedDataObject == null || model.SelectedDataObject.DataProperty == null)
                    {
                        MessageBox.Show("Please select a property to map", "MAP ROLE", MessageBoxButton.OK);
                        return;
                    }

                    roleMap.dataType = utility.Utility.CSharpTypeToXsdType(model.SelectedDataObject.DataProperty.dataType.ToString());
                    roleMap.propertyName = model.SelectedDataObject.DataProperty.propertyName;
                    roleMap.valueList = String.Empty;
                }
                else if (model.SelectedTreeItem == null || !(model.SelectedTreeItem is ClassTreeItem))
                {
                    MessageBox.Show("Please select a class to map", "MAP ROLE", MessageBoxButton.OK);
                    return;
                }
                else if (roleMap.dataType != null || roleMap.dataType.StartsWith("xsd:"))
                {
                    roleMap.propertyName = model.SelectedDataObject.DataProperty.propertyName;
                }
                else
                {
                    ClassDefinition selectedClass = ((ClassTreeItem)model.SelectedTreeItem).ClassDefinition;
                    //roleMap.dataType = String.Empty;
                    roleMap.propertyName = String.Empty;
                    roleMap.dataType = selectedClass.identifier.GetIdWithAliasFromUri();
                    roleMap.valueList = String.Empty;
                   
                }

                model.SelectedMappingItem.itemTextBlock.Text = model.SelectedMappingItem.itemTextBlock.Text.Replace(Presenter.unmappedToken, "");
                model.DetailProperties.Clear();
                Presenter.RefreshRoleMap(roleMap);
            }
            else
            {
                if (model.SelectedTreeItem == null || !(model.SelectedTreeItem is ClassTreeItem))
                {
                    MessageBox.Show("Please select a class to map", "MAP ROLE", MessageBoxButton.OK);
                    return;
                }

                ClassDefinition classDefinition = ((ClassTreeItem)model.SelectedTreeItem).ClassDefinition;
                roleMap = model.SelectedRoleMap;
                TextBox txtLabel = sender as TextBox;

                ClassMap classMap = new ClassMap
                {
                    name = utility.Utility.NameSafe(txtLabel.Text),
                    classId = classDefinition.identifier.GetIdWithAliasFromUri()
                };
                classMap.identifiers = new List<string>();
                classMap.identifiers.Add(model.SelectedDataObject.DataProperty.propertyName);

                model.SelectedGraphMap.AddClassMap(roleMap, classMap);

                roleMap.classMap = classMap;

                model.SelectedMappingItem.itemTextBlock.Text = model.SelectedMappingItem.itemTextBlock.Text.Replace(Presenter.unmappedToken, "");

                Presenter.PopulateRoleNode(mappingItem, roleMap);
            }

            aggregator.GetEvent<NavigationEvent>().Publish(new NavigationEventArgs
            {
                DetailProcess = DetailType.Mapping,
                SelectedNode = mappingItem,
                Sender = this
            });
            // }
        }

        public void btnMakeClassRole_Click(object sender, RoutedEventArgs e)
        {
            MappingItem mappingItem = model.SelectedMappingItem;

            if (mappingItem == null)
                return;

            if (mappingItem.NodeType == NodeType.RoleMap)
            {
                string classRoleId = mappingItem.RoleMap.roleId;
                MappingItem templateMapTreeItem = (MappingItem)mappingItem.Parent;

                // Set classRole for templateMap
                //templateMapTreeItem.TemplateMap.classRole = classRoleId;

                // Remove roleMap from mapping
                templateMapTreeItem.TemplateMap.roleMaps.Remove(mappingItem.RoleMap);

                // Remove roleMap from treeview
                templateMapTreeItem.Items.Remove(mappingItem);

                // Refresh templateMap in detail pane
                model.DetailProperties.Clear();
                Presenter.RefreshTemplateMap(templateMapTreeItem.TemplateMap);
            }
            else
            {
                MessageBox.Show("Please select a role", "MAP ROLE", MessageBoxButton.OK);
            }
        }

        public void btnAddValueList(object sender, RoutedEventArgs e)
        {
            MappingItem mappingItem = model.SelectedMappingItem;
            string valuelist = Presenter.selectedValueList;
            if (valuelist == "<No ValueList>")
                valuelist = "";

            if (mappingItem == null)
                return;

            if (mappingItem.NodeType == NodeType.RoleMap)
            {
                if (model.SelectedDataObject == null || model.SelectedDataObject.DataProperty == null)
                {
                    MessageBox.Show("Please select a property to map", "MAP ROLE", MessageBoxButton.OK);
                }
                else if (Presenter.selectedValueList == null)
                {
                    MessageBox.Show("Please select a value list", "MAP ROLE", MessageBoxButton.OK);
                }
                else// if (mappingItem.TemplateMap.type == TemplateType.Property )
                {
                    RoleMap roleMap = mappingItem.RoleMap;

                    if (roleMap.dataType != null || roleMap.dataType != "")
                        roleMap.valueList = valuelist;

                    if (roleMap.type == RoleType.Reference)// == null || roleMap.reference == "")
                    {
                        // roleMap.propertyName = model.SelectedDataObject.DataProperty.propertyName;

                        model.SelectedMappingItem.itemTextBlock.Text = model.SelectedMappingItem.itemTextBlock.Text.Replace(Presenter.unmappedToken, "");

                        // Refresh roleMap in detail pane
                        model.DetailProperties.Clear();
                        Presenter.RefreshRoleMap(roleMap);
                    }
                    else
                    //else if (mappingItem.TemplateMap.type == TemplateType.Relationship)
                    {
                        roleMap = mappingItem.RoleMap;
                        roleMap.valueList = valuelist;
                        roleMap.propertyName = string.Empty;// model.SelectedMappingItem.itemTextBlock.Text.Replace(Presenter.unmappedToken, "");
                        model.DetailProperties.Clear();
                        Presenter.RefreshRoleMap(roleMap);
                        // }
                        // }
                        //else
                        //{
                        //    MessageBox.Show("Please select a role", "MAP ROLE", MessageBoxButton.OK);
                        //}
                    }
                }
            }
        }

        public void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            MappingItem mappingItem = model.SelectedMappingItem;

            if (mappingItem == null)
                return;

            switch (mappingItem.NodeType)
            {
                case NodeType.GraphMap:
                    {
                        TreeView parent = (TreeView)mappingItem.Parent;

                        mapping.graphMaps.Remove(mappingItem.GraphMap);
                        tvwMapping.Items.Remove(mappingItem);
                        break;
                    }
                case NodeType.TemplateMap:
                    {
                        MappingItem parent = (MappingItem)mappingItem.Parent;
                        TemplateMap templateMap = (TemplateMap)mappingItem.Tag;
                        // mappingItem.ClassMap.templateMaps.Remove(mappingItem.TemplateMap);

                        RoleMap classRole = templateMap.roleMaps.Where(c => c.classMap != null).FirstOrDefault();
                        GraphMap graphMap = mappingItem.GraphMap;
                        var pair = graphMap.GetClassTemplateListMap(classRole.classMap.classId);
                        foreach (var template in pair.Value)
                        {
                            graphMap.DeleteTemplateMap(classRole.classMap.classId, template.templateId);
                        }
                        model.SelectedTemplateMap.roleMaps.Clear();
                        model.SelectedGraphMap.DeleteTemplateMap(parent.ClassMap.classId, templateMap.templateId);

                        parent.Items.Remove(mappingItem);

                        if (parent.Items.Count == 0)
                            parent.IsExpanded = false;
                        break;
                    }
                case NodeType.RoleMap:
                    {
                        RoleMap roleMap = mappingItem.RoleMap;

                        // clear relationship to classmap
                        mappingItem.Items.Clear();
                        mappingItem.IsExpanded = false;
                        roleMap.classMap = null;

                        // clear the rolemap
                       // roleMap.dataType = String.Empty;
                       // roleMap.propertyName = String.Empty;
                        //roleMap.value = String.Empty;
                        // roleMap.reference = String.Empty;
                       // roleMap.valueList = String.Empty;

                        if (!mappingItem.itemTextBlock.Text.EndsWith(Presenter.unmappedToken))
                        {
                            mappingItem.itemTextBlock.Text += Presenter.unmappedToken;
                        }

                        break;
                    }
                case NodeType.ClassMap:
                    {
                        MappingItem parent = (MappingItem)mappingItem.Parent;
                        var pair = mappingItem.GraphMap.GetClassTemplateListMap(mappingItem.ClassMap.classId);
                        for (int i = 0; i < pair.Value.Count; i++ )
                        {
                            TemplateMap tm = pair.Value[i];
                            tm.roleMaps.Clear();
                            mappingItem.GraphMap.DeleteTemplateMap(mappingItem.ClassMap.classId, tm.templateId);
                        }
                        mappingItem.GraphMap.DeleteClassMap(mappingItem.ClassMap.classId);
                        parent.Items.Clear();
                        parent.IsExpanded = false;
                        mappingItem.ClassMap = null;

                        // clear the parent rolemap
                        mappingItem.RoleMap.propertyName = String.Empty;

                        if (!parent.itemTextBlock.Text.EndsWith(Presenter.unmappedToken))
                        {
                            parent.itemTextBlock.Text += Presenter.unmappedToken;
                        }

                        break;
                    }
            }

            aggregator.GetEvent<NavigationEvent>().Publish(new NavigationEventArgs
            {
                DetailProcess = DetailType.Mapping,
                SelectedNode = mappingItem,
                Sender = this
            });
        }

        public void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Response response = adapterProxy.UpdateMapping(projectName, applicationName, mapping);
        }
    }
}


