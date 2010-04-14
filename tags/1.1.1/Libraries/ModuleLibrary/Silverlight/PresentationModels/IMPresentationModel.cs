﻿using System;
using System.Net;
using PrismContrib.Base;
using OntologyService.Interface.Entities;
using System.Collections.Generic;
using OntologyService.Interface.PresentationModels;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using org.ids_adi.iring.referenceData;
using org.ids_adi.iring;
using ModuleLibrary.Entities;
using ModuleLibrary.Types;
using org.ids_adi.qmxf;
using InformationModel.UserControls;
using ModuleLibrary.UserControls;
using org.iringtools.library;


namespace OntologyService.Interface
{
  /// <summary>
  /// This presentation model has cross-cutting concerns and will be used
  /// by multiple applications/module views
  /// </summary>
  public class IMPresentationModel : PresentationModelBase, IIMPresentationModel
  {

    public TreeView MappingTree { get; set; }



    #region SelectedDataObject 
    private DataObjectItem _selectedDataObject;
    /// <summary>
    /// Currently Selected Data object (DataObjectItem)
    /// </summary>
    public DataObjectItem SelectedDataObject
    {
      get { return _selectedDataObject; }
      set
      {
        _selectedDataObject = value;
        OnPropertyChanged("SelectedDataObject");
      }
    }
    
    #endregion

    private DataProperty _selectedDataSource;
    public DataProperty SelectedDataSource
    {
      get { return _selectedDataSource; }
      set
      {
        _selectedDataSource = value;
        OnPropertyChanged("SelectedDataSource");

        SelectedDataSourceDataTypeField = value.dataType;
        SelectedDataSourceIsPropertyKey = value.isPropertyKey;
        SelectedDataSourceIsRequiredField = value.isRequired;
        SelectedDataSourcePropertyName = value.propertyName;
      }
    }

      
    #region Snapshop of Selected Data Source
    private string _selectedDataSourceDataTypeField;
    public string SelectedDataSourceDataTypeField
    {
      get { return _selectedDataSourceDataTypeField; }
      set
      {
        _selectedDataSourceDataTypeField = value;
        OnPropertyChanged("SelectedDataSourceDataTypeField");
      }
    }
    private string _selectedDataSourcePropertyName;
    public string SelectedDataSourcePropertyName
    {
      get { return _selectedDataSourcePropertyName; }
      set
      {
        _selectedDataSourcePropertyName = value;
        OnPropertyChanged("SelectedDataSourcePropertyName");
      }
    }
    private bool _selectedDataSourceIsPropertyKey;
    public bool SelectedDataSourceIsPropertyKey
    {
      get { return _selectedDataSourceIsPropertyKey; }
      set
      {
        _selectedDataSourceIsPropertyKey = value;
        OnPropertyChanged("SelectedDataSourceIsPropertyKey");
      }
    }
    private bool _selectedDataSourceIsRequiredField;
    public bool SelectedDataSourceIsRequiredField
    {
      get { return _selectedDataSourceIsRequiredField; }
      set
      {
        _selectedDataSourceIsRequiredField = value;
        OnPropertyChanged("SelectedDataSourceIsRequiredField");
      }
    }
    private CustomTreeItem _selectedTreeItem;
    public CustomTreeItem SelectedTreeItem
    {
      get { return _selectedTreeItem; }
      set
      {
        _selectedTreeItem = value;

        Entity entity = _selectedTreeItem.Entity;

        if (entity != null)
        {
          SelectedIMLabel = entity.label;
          SelectedIMRepository = entity.repository;
          SelectedIMUri = entity.uri;
          SelectedInformationModelNode = entity;
        }
        OnPropertyChanged("SelectedTreeItem");
      }
    }
    #endregion

    private MappingItem _selectedMappingItem;
    public MappingItem SelectedMappingItem
    {
        get { return _selectedMappingItem; }
        set
        {
            _selectedMappingItem = value;
            OnPropertyChanged("SelectedMappingItem");

            SelectedNodeType = value.NodeType;
            SelectedGraphMap = value.GraphMap;
            SelectedClassMap = value.ClassMap;
            SelectedTemplateMap = value.TemplateMap;
            SelectedRoleMap = value.RoleMap;
        }
    }

    #region Snapshop of Selected Mapping Item
    private NodeType _selectedNodeType;
    public NodeType SelectedNodeType
    {
        get { return _selectedNodeType; }
        set
        {
            _selectedNodeType = value;
            OnPropertyChanged("SelectedNodeType");
        }
    }

    private GraphMap _selectedGraphMap;
    public GraphMap SelectedGraphMap
    {
        get { return _selectedGraphMap; }
        set
        {
            _selectedGraphMap = value;
            OnPropertyChanged("SelectedGraphMap");
        }
    }

    private ClassMap _selectedClassMap;
    public ClassMap SelectedClassMap
    {
        get { return _selectedClassMap; }
        set
        {
            _selectedClassMap = value;
            OnPropertyChanged("SelectedClassMap");
        }
    }

    private TemplateMap _selectedTemplateMap;
    public TemplateMap SelectedTemplateMap
    {
        get { return _selectedTemplateMap; }
        set
        {
            _selectedTemplateMap = value;
            OnPropertyChanged("SelectedTemplateMap");
        }
    }

    private RoleMap _selectedRoleMap;
    public RoleMap SelectedRoleMap
    {
        get { return _selectedRoleMap; }
        set
        {
            _selectedRoleMap = value;
            OnPropertyChanged("SelectedRoleMap");
        }
    }

    #endregion

    private Entity _selectedInformationModelNode;
    public Entity SelectedInformationModelNode
    {
      get { return _selectedInformationModelNode; }
      set
      {
        _selectedInformationModelNode = value;
        OnPropertyChanged("SelectedInformationModelNode");

        // Snapshot of individual fields (in case node tab is
        // closed and reference is lost)
        this.SelectedIMLabel = value.label;
        this.SelectedIMUri = value.uri;
        this.SelectedIMRepository = value.repository;
      }
    }

    #region Snapshot of SelectedInformationModelNode information
    private string _selectedIMlabel;
    public string SelectedIMLabel
    {
      get { return _selectedIMlabel; }
      set
      {
        _selectedIMlabel = value;
        OnPropertyChanged("SelectedIMLabel");
      }
    }
    private string _selectedIMrepository;
    public string SelectedIMRepository
    {
      get { return _selectedIMrepository; }
      set
      {
        _selectedIMrepository = value;
        OnPropertyChanged("SelectedIMRepository");
      }
    }
    private string _selectedIMuri;
    public string SelectedIMUri
    {
      get { return _selectedIMuri; }
      set
      {
        _selectedIMuri = value;
        OnPropertyChanged("SelectedIMUri");
      }
    }
    #endregion

    #region Information Model additional properties
    private bool _selectedIsMappable;
    public bool SelectedIsMappable
    {
        get { return _selectedIsMappable; }
        set
        {
            _selectedIsMappable = value;
            OnPropertyChanged("SelectedIsMappable");
        }
    }

    private QMXF _selectedQMXF;
    public QMXF SelectedQMXF
    {
        get { return _selectedQMXF; }
        set
        {
            _selectedQMXF = value;
            OnPropertyChanged("SelectedQMXF");
        }
    }

    private Entity _selectedEntity;
    public Entity SelectedEntity
    {
        get { return _selectedEntity; }
        set
        {
            _selectedEntity = value;
            OnPropertyChanged("SelectedEntity");
        }
    }

    private Dictionary<string, QMXF> _classesHistory;
    public Dictionary<string, QMXF> ClassesHistory
    {
        get { return _classesHistory; }
        set
        {
            _classesHistory = value;
            OnPropertyChanged("ClassesHistory");
        }
    }

    private Dictionary<string, QMXF> _templatesHistory;
    public Dictionary<string, QMXF> TemplatesHistory
    {
        get { return _templatesHistory; }
        set
        {
            _templatesHistory = value;
            OnPropertyChanged("TemplatesHistory");
        }
    }
    #endregion

    private ObservableCollection<KeyValuePair<string, string>> _detailProperties;
    public ObservableCollection<KeyValuePair<string, string>> DetailProperties
    {
      get
      {
        if (_detailProperties == null)
          _detailProperties = new ObservableCollection<KeyValuePair<string, string>>();
        return _detailProperties;

      }
      set
      {
        _detailProperties = value;
        OnPropertyChanged("DetailProperties");
      }
    }
  }
}