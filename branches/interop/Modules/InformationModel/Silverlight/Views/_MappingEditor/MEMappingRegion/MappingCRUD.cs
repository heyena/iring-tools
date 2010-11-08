using System;
using System.Windows;
using System.Windows.Controls;
using InformationModel.Views.MEDataDetailRegion;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Unity;
using OntologyService.Interface.PresentationModels;
using PrismContrib.Errors;
using org.ids_adi.iring;
using org.ids_adi.iring.referenceData;
using InformationModel.Events;
using System.Collections.Generic;
using org.ids_adi.iring.utility;
using System.Linq;
using ModuleLibrary.Extensions;
using ModuleLibrary.Entities;
using ModuleLibrary.Types;
using OntologyService.Interface;
using ModuleLibrary.Events;
using org.ids_adi.qmxf;
using ModuleLibrary.LayerDAL;
using InformationModel.UserControls;

namespace InformationModel.Views._MappingEditor.MEMappingRegion
{
    public class MappingCRUD
    {
        private IIMPresentationModel model = null;
        private IMappingView view = null;
        private IAdapter adapterProxy = null;
        private IReferenceData referenceProxy = null;
        private IEventAggregator aggregator = null;
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
        }

        public void btnAddGraph_Click(object sender, RoutedEventArgs e)
        {
            if(model.SelectedIMUri == null || 
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
                
                TextBox txtLabel = sender as TextBox;
                GraphMap graphMap = new GraphMap();
                graphMap.name = txtLabel.Text;
                graphMap.classId = SPARQLExtensions.GetIdWithAliasFromUri(model.SelectedIMUri);
                graphMap.identifier = model.SelectedDataObject.DataProperty.propertyName;
                if (graphMap.dataObjectMaps == null)
                  graphMap.dataObjectMaps = new List<DataObjectMap>();

                DataObjectMap dataObjectMap = new DataObjectMap
                {
                  name = model.SelectedDataObject.DataObject.objectName,
                  inFilter = "",
                  outFilter = ""
              
                };
                graphMap.dataObjectMaps.Add(dataObjectMap);

                mapping.graphMaps.Add(graphMap);

                tvwMapping.Items.Add(Presenter.AddNode(graphMap.name, graphMap, null));                            
            }
        }

        public void btnAddTemplate_Click(object sender, RoutedEventArgs e)
        {
            if (model.SelectedIMUri == null ||
                SPARQLExtensions.GetObjectTypeFromUri(model.SelectedIMUri) != SPARQLPrefix.ObjectType.Template)
            {
                MessageBox.Show("Please select a valid template", "ADD TEMPLATE", MessageBoxButton.OK);
            }
            else if (model.SelectedMappingItem == null || (
                model.SelectedMappingItem.NodeType != NodeType.GraphMap &&
                model.SelectedMappingItem.NodeType != NodeType.ClassMap)
              )
            {
                MessageBox.Show("Please select a graph map or class map", "ADD TEMPLATE", MessageBoxButton.OK);
            }
            else
            {
                MappingItem mappingItem = model.SelectedMappingItem;
                
                TextBox txtLabel = sender as TextBox;
                TemplateMap templateMap = new TemplateMap();

                templateMap.name = HandleSpecialCharacters(model.SelectedIMLabel);
                templateMap.templateId = SPARQLExtensions.GetIdWithAliasFromUri(model.SelectedIMUri);                
               
                string classId = mappingItem.ClassMap.classId;
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
              
                bool isClassRoleFound = GetClassRole(classId, template, templateMap);

                if (!isClassRoleFound)
                {
                  if (model.ClassesHistory != null)
                  {
                    foreach (KeyValuePair<string, QMXF> keyValuePair in model.ClassesHistory)
                    {
                      QMXF classQmxf = keyValuePair.Value;
                      if (classId == keyValuePair.Key)
                      {
                        List<Specialization> specializations = classQmxf.classDefinitions.FirstOrDefault().specialization;
                        foreach (Specialization specialization in specializations)
                        {
                          isClassRoleFound = GetClassRole(specialization.reference, template, templateMap);
                          if (isClassRoleFound) break;
                        }
                      }
                    }
                  }
                }
                if (!isClassRoleFound)
                {
                    //QMXF classQmxf = referenceProxy.GetClass(classId);
                    //foreach (ClassDefinition classDefinition in classQmxf.classDefinitions)
                    //{
                    //    foreach (Specialization specialization in classDefinition.specialization)
                    //    {
                    //      isClassRoleFound = GetClassRole(specialization.reference, template, templateMap);
                    //        if (isClassRoleFound) break;
                    //    }
                    //}
                }

                GetRoleMaps(classId, template, templateMap);
                GetTemplateType(template, templateMap);

                if (mappingItem.ClassMap.templateMaps == null)
                  mappingItem.ClassMap.templateMaps = new List<TemplateMap>();
                mappingItem.ClassMap.templateMaps.Add(templateMap);

                Presenter.PopulateTemplateMap(mappingItem, templateMap);
                //Presenter.PopulateClassNode(mappingItem, mappingItem.GraphMap);
            }
        }

        private string HandleSpecialCharacters(string inputString)
        {
            string pattern = "[^a-zA-Z]";
            string outputString =
            System.Text.RegularExpressions.Regex.Replace(inputString, pattern, "");
            return outputString;

        }

        private bool GetClassRole(string classId, object template, TemplateMap currentTemplateMap)
        {
            bool isClassRoleFound = false;
            
            if (template is TemplateDefinition)
            {
                TemplateDefinition templateDefinition = (TemplateDefinition)template;
                
                List<RoleDefinition> roleDefinitions = templateDefinition.roleDefinition;
                foreach (RoleDefinition roleDefinition in roleDefinitions)
                {
                    string range = roleDefinition.range.GetIdWithAliasFromUri();
                    if (range == classId)
                    {
                        currentTemplateMap.classRole = roleDefinition.identifier.GetIdWithAliasFromUri();
                        isClassRoleFound = true;
                        break;
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
                if (range == classId)
                {
                  currentTemplateMap.classRole = roleQualification.qualifies.GetIdWithAliasFromUri();
                  isClassRoleFound = true;
                  break;
                } 
              }
            }
            return isClassRoleFound;
        }

        private void GetRoleMaps(string classId, object template, TemplateMap currentTemplateMap)
        {
          if (currentTemplateMap.roleMaps == null)
            currentTemplateMap.roleMaps = new List<RoleMap>();

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
                  name = roleDefinition.name.FirstOrDefault().value,
                  dataType = range,
                  propertyName = "",
                  roleId = roleDefinition.identifier.GetIdWithAliasFromUri(),
                  isMapped = false
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
              if (range != String.Empty && range != classId)
              {
                RoleMap roleMap = new RoleMap
                {
                  name = roleQualification.name.FirstOrDefault().value,
                  dataType = range,
                  propertyName = "",
                  roleId = roleQualification.qualifies.GetIdWithAliasFromUri(),
                  isMapped = false
                };

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

          if (isMappable)
            currentTemplateMap.type = TemplateType.Property;
          else
            currentTemplateMap.type = TemplateType.Relationship;
        }
        
        public void btnData_Click(object sender, RoutedEventArgs e)
        {            

        }  

        public void btnMap_Click(object sender, RoutedEventArgs e)
        {
          if (model.SelectedRoleMap == null)
          {
            MessageBox.Show("Please select a role map", "MAP ROLE", MessageBoxButton.OK);
            return;
          }

          if (model.SelectedDataObject == null || model.SelectedDataObject.DataProperty == null)
          {
            MessageBox.Show("Please select a property to map", "MAP ROLE", MessageBoxButton.OK);
            return;
          }

          MappingItem mappingItem = model.SelectedMappingItem;

          if (mappingItem.TemplateMap.type == TemplateType.Property)
          {
            RoleMap roleMap = model.SelectedRoleMap;
            roleMap.propertyName = model.SelectedDataObject.DataProperty.propertyName;
            roleMap.isMapped = true;
            model.SelectedMappingItem.Header = model.SelectedMappingItem.Header.ToString().Replace(" [UnMapped]", "");
          }
          else
          {
            if (model.SelectedTreeItem == null || !(model.SelectedTreeItem is ClassTreeItem))
            {
              MessageBox.Show("Please select a class to map", "MAP ROLE", MessageBoxButton.OK);
              return;
            }

            ClassDefinition classDefinition = ((ClassTreeItem)model.SelectedTreeItem).ClassDefinition;

            RoleMap roleMap = model.SelectedRoleMap;
            
            TextBox txtLabel = sender as TextBox;
            ClassMap classMap = new ClassMap
            {
              name = txtLabel.Text,
              classId = classDefinition.identifier.GetIdWithAliasFromUri(),
              identifier = model.SelectedDataObject.DataProperty.propertyName,
            };

            roleMap.classMap = classMap;
            roleMap.isMapped = true;
            model.SelectedMappingItem.Header = model.SelectedMappingItem.Header.ToString().Replace(" [UnMapped]", "");

            Presenter.PopulateRoleNode(mappingItem, roleMap);
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
            Response response = adapterProxy.UpdateMapping(mapping);
        }

        public void btnDelete_Click(object sender, RoutedEventArgs e)
        {
          MappingItem mappingItem = model.SelectedMappingItem;

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

                mappingItem.ClassMap.templateMaps.Remove(mappingItem.TemplateMap);
                parent.Items.Remove(mappingItem);

                if (parent.Items.Count == 0)
                  parent.IsExpanded = false;
                break;
              }
            case NodeType.RoleMap:
              {
                //clear any classmaps
                mappingItem.Items.Clear();
                mappingItem.IsExpanded = false;
                mappingItem.RoleMap.classMap = null;
                
                //clear the rolemap
                mappingItem.RoleMap.propertyName = String.Empty;
                mappingItem.RoleMap.isMapped = false;
                mappingItem.Header += " [UnMapped]";
                break;
              }
            case NodeType.ClassMap:
              {
                MappingItem parent = (MappingItem)mappingItem.Parent;

                // clear the classmap
                parent.Items.Clear();
                parent.IsExpanded = false;
                mappingItem.RoleMap.classMap = null;
                
                // clear the parent rolemap
                mappingItem.RoleMap.propertyName = String.Empty;
                mappingItem.RoleMap.isMapped = false;
                parent.Header += " [UnMapped]";
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
        
    }
}


