using System;
using System.Net;
using PrismContrib.Base;
using System.Collections.Generic;
using org.iringtools.ontologyservice.presentation.presentationmodels;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using org.iringtools.modulelibrary.entities;
using org.iringtools.modulelibrary.types;
using org.ids_adi.qmxf;
using org.iringtools.informationmodel.usercontrols;
using org.iringtools.modulelibrary.usercontrols;
using org.iringtools.library;
using org.iringtools.mapping;


namespace org.iringtools.ontologyservice.presentation
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

        //SelectedDataSourceDataTypeField = value.dataType;
        //SelectedDataSourceIsPropertyKey = value.isPropertyKey;
        //SelectedDataSourceIsRequiredField = value.isRequired;
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
          SelectedIMLabel = entity.Label;
          SelectedIMRepository = entity.Repository;
          SelectedIMUri = entity.Uri;
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
        this.SelectedIMLabel = value.Label;
        this.SelectedIMUri = value.Uri;
        this.SelectedIMRepository = value.Repository;
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

    private bool _selectedIsMapped;
    public bool SelectedIsMapped
    {
      get { return _selectedIsMapped; }
      set
      {
        _selectedIsMapped = value;
        OnPropertyChanged("SelectedIsMapped");
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

    private Dictionary<string, string> _idLabelDictionary;
    public Dictionary<string, string> IdLabelDictionary
    {
      get
      {
        if (_idLabelDictionary == null)
        {
          _idLabelDictionary = new Dictionary<string, string>();
        }

        return _idLabelDictionary;
      }
    }
  }
}