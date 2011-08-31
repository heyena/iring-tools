Ext.ns('AdapterManager');

/**
* @class AdapterManager.SpreadsheetSourcePanel
* @extends FormPanel
* @author by Gert Jansen van Rensburg
*/
AdapterManager.SpreadsheetSourcePanel = Ext.extend(Ext.FormPanel, {
  width: 120,
  layout: 'fit',

  frame: true,
  border: true,

  fileUpload: true,
  labelWidth: 150, // label settings here cascade unless    
  method: 'POST',
  bodyStyle: 'padding:10px 5px 0',

  border: false, // removing the border of the form
  defaults: {
    width: 330,
    msgTarget: 'side'
  },
  defaultType: 'textfield',
  buttonAlign: 'left', // buttons aligned to the left            
  autoDestroy: true,

  scope: null,
  application: null,
  dataLayer: null,
  assembly: null,

  /**
  * initComponent
  * @protected
  */
  initComponent: function () {

    this.addEvents({
      uploaded: true
    });

    var scope = "";

    if (this.scope != null) {
      scope = this.scope;
    }

    var application = "";
    var dataLayer = "";
    var assembly = "";

    if (this.application != null) {
      application = this.application
      dataLayer = this.dataLayer;
    }

    this.bbar = [
          '->',
          { xtype: 'button', text: 'Upload', scope: this, handler: this.onUpload },
          { xtype: 'button', text: 'Cancel', scope: this, handler: this.onCancel }
        ]

    this.items = [
            { xtype: 'hidden', name: 'Scope', value: this.Scope },
            { xtype: 'hidden', name: 'Application', value: this.Application },
            { xtype: 'hidden', name: 'DataLayer', value: this.datalayer },
            {
              xtype: 'fileuploadfield',
              name: 'SourceFile',
              emptyText: 'Select an Spreadsheet Source File',
              fieldLabel: 'Spreadsheet Source File',
              buttonText: '',
              buttonCfg: {
                iconCls: 'upload-icon'
              }
            },
            { xtype: 'checkbox', name: 'Generate', boxLabel: 'Generate Configuration' }
        ];

    // super
    AdapterManager.SpreadsheetSourcePanel.superclass.initComponent.call(this);
  },

  onUpload: function () {
    that = this;
    this.getForm().submit({
      waitMsg: 'Uploading file...',
      url: this.url,
      method: 'POST',
      success: function (f, a) {
        that.fireEvent('Uploaded', that, f.items.items[3].value);
      },
      failure: function (f, a) {
        Ext.Msg.alert('Warning', 'Error uploading file "' + f.items.items[3].value + '"!');
      }
    });
  },
  onCancel: function (b) {
    this.getForm().reset();
    this.findParentByType('panel').hide();
  }
});


/**
* @class AdapterManager.SpreadsheetLibraryPanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
AdapterManager.SpreadsheetLibraryPanel = Ext.extend(Ext.Panel, {
  title: 'SpreadsheetLibrary',
  width: 120,
  collapseMode: 'mini',
  closable: true,
  layout: 'fit',
  border: true,
  split: true,
  scope: null,
  application: null,
  configurationPanel: null,
  propertyPanel: null,
  tablesConfigPanel: null,
  url: null,

  /**
  * initComponent
  * @protected
  */
  initComponent: function () {
    this.addEvents({
      save: true,
      reset: true,
      validate: true,
      refresh: true,
      selectionchange: true
    });
    var scope = "";
    if (this.scope != null) {
      scope = this.scope.Name;
    };

    var application = "";
    var dataLayer = "";

    if (this.application != null) {
      application = this.application.Name;
      dataLayer = this.application.DataLayer;
    };

    this.tbar = this.buildToolbar();

    this.treeLoader = new Ext.tree.TreeLoader({
      baseParams: {
        scope: this.scope,
        application: this.application,
        type: null
      },
      url: 'spreadsheet/getnode'

    });

    this.treeLoader.on("beforeload", function (treeLoader, node) {
      treeLoader.baseParams.type = node.attributes.type;
    }, this);

    this.rootNode = new Ext.tree.AsyncTreeNode({
      id: 'root',
      text: 'Workbook',
      expanded: true,
      draggable: false,
      icon: 'Content/img/excel.png',
      type: 'ExcelWorkbookNode'
    });

    this.configurationPanel = new Ext.tree.TreePanel({
      layout: 'fit',
      region: 'west',
      border: false,
      split: false,
      lines: true,
      expandAll: true,
      rootVisible: true,
      autoScroll: true,
      width: 500,
      loader: this.treeLoader,
      root: this.rootNode
    });

    this.configurationPanel.on('click', this.onClick, this);

    this.propertyPanel = new Ext.grid.PropertyGrid({
      title: 'Details',
      region: 'east',
      width: 200,
      split: true,
      collapseMode: 'mini',
      stripeRows: true,
      collapsible: true,
      autoScroll: true,
      border: false,
      frame: false,
      height: 150,
      selModel: new Ext.grid.RowSelectionModel({ singleSelect: true }),
      // bodyStyle: 'padding-bottom:15px;background:#eee;',
      source: {},
      listeners: {
        beforeedit: function (e) {
          e.cancel = true;
        },
        // to copy but not edit content of property grid
        afteredit: function (e) {
          e.grid.getSelectionModel().selections.items[0].data.value = e.originalValue;
          e.record.data.value = e.originalValue;
          e.value = e.originalValue;
          e.grid.getView().refresh();
        }
      }
    });

    //--------------
    this.tablesConfigPanel = new Ext.Panel({
      layout: 'fit',
      region: 'center',
      minWidth: 10,
      frame: false,
      border: false,
      autoScroll: true
    });
    //--------------------

    this.items = [
            this.configurationPanel,
            this.tablesConfigPanel
    // this.propertyPanel
        ];

    // super
    AdapterManager.SpreadsheetLibraryPanel.superclass.initComponent.call(this);
  },

  buildToolbar: function () {
    return [
      {
        text: 'Reload',
        handler: this.onReload,
        icon: 'Content/img/16x16/view-refresh.png',
        scope: this
      },
      {
        text: 'Save',
        handler: this.onSave,
        icon: 'Content/img/16x16/document-save.png',
        scope: this
      },
      {
        text: 'Upload',
        handler: this.onUpload,
        //icon: 'Content/img/list-remove.png',
        scope: this
      }
    ]
  },

  onReload: function () {
    this.configurationPanel.root.reload();
  },

  onReloadNode: function (node) {
    node.reload();
  },

  onUpload: function (panel) {
    var that = this;
    var form = new AdapterManager.SpreadsheetSourcePanel({
      Scope: that.scope,
      Application: that.application,
      DataLayer: that.datalayer,
      method: 'POST',
      url: 'spreadsheet/upload'
    });

    var newWin = new Ext.Window({
      width: 400,
      layout: 'fit',
      height: 300,
      autoScroll: true,
      modal: true,
      items: form
    });

    form.on('uploaded', function () {
      newWin.close();
      this.configurationPanel.root.reload();
    }, this);

    newWin.show();
  },


  onSave: function (panel) {

    Ext.Ajax.request({
      url: 'spreadsheet/configure',    // where you wanna post
      method: 'POST',
      success: function (f, a) {

      },   // function called on success
      failure: function (f, a) {

      },
      params: {
        Scope: this.scope,
        Application: this.application,
        DataLayer: this.datalayer
      }
    });
  },

  onUpdate: function (panel) {
    var that = this;
    Ext.Ajax.request({
      url: 'spreadsheet/updateconfiguration',    // where you wanna post
      method: 'POST',
      success: function (f, a) {
        that.configurationPanel.root.reload();
      },   // function called on success
      failure: function (f, a) {

      },
      params: {
        Scope: this.scope.Name,
        Application: this.application.Name,
        DataLayer: this.application.Assembly,
        Label: this.tablesConfigPanel.items.items[0].getForm().getFieldValues().Label,
        Name: this.tablesConfigPanel.items.items[0].getForm().getFieldValues().Name
      }
    });
  },

  onReset: function (panel) {
    this.tablesConfigPanel.items.items[0].getForm().reset();
  }
});
