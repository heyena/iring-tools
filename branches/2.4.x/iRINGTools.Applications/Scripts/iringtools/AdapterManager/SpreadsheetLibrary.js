Ext.ns('AdapterManager');

/**
* @class AdapterManager.SpreadsheetSourcePanel
* @extends FormPanel
* @author by Gert Jansen van Rensburg
*/
AdapterManager.SpreadsheetSourcePanel = Ext.extend(Ext.FormPanel, {

  fileUpload: true,
  labelWidth: 150, // label settings here cascade unless    
  method: 'POST',
  bodyStyle: 'padding:5 5 5 5',

  border: false, // removing the border of the form

  defaultType: 'textfield',
  buttonAlign: 'left', // buttons aligned to the left            
  autoDestroy: false,

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
      uploaded: true,
      downloaded: true
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
          {
            xtype: 'button',
            text: 'Upload',
            scope: this,
            handler: this.onUpload,
            icon: 'Content/img/16x16/document-up.png'
          }

    //{ xtype: 'button', text: 'Cancel', scope: this, handler: this.onReset }
        ]

    this.items = [
            { xtype: 'hidden', name: 'Scope', value: this.Scope },
            { xtype: 'hidden', name: 'Application', value: this.Application },
            { xtype: 'hidden', name: 'DataLayer', value: this.datalayer },
            {
              xtype: 'fileuploadfield',
              name: 'SourceFile',
              emptyText: 'Select a Spreadsheet',
              fieldLabel: 'Spreadsheet Source',
              width: 232,
              buttonText: null,
              buttonCfg: {
                iconCls: 'upload-icon'
              }
            },
           { xtype: 'checkbox', name: 'Generate', fieldLabel: 'Generate Configuration', checked: true }
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

  onReset: function () {
    this.getForm().reset();
    this.fireEvent('Cancel', this);
  }

});

/**
* @class AdapterManager.SpreadsheetWorksheetSelectionPanel
* @extends FormPanel
* @author by Gert Jansen van Rensburg
*/
AdapterManager.SpreadsheetWorksheetSelection = Ext.extend(Ext.FormPanel, {
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
  autoDestroy: false,

  scope: null,
  application: null,
  dataLayer: null,

  btnNext: null,
  btnPrev: null,

  /**
  * initComponent
  * @protected
  */
  initComponent: function () {

    this.addEvents({
      uploaded: true
    });

    var scope = "";
    var wizard = this;
    if (this.scope != null) {
      scope = this.scope.Name;
    }

    var application = "";
    var assembly = "";

    if (this.application != null) {
      application = this.application;
      DataLayer = this.datalayer;
    }

    this.bbar = [
      '->',
      {
        xtype: 'button',
        text: 'Upload',
        scope: this, handler:
          this.onUpload,
        icon: 'Content/img/16x16/document-up.png'
      },
      { xtype: 'button', text: 'Cancel', scope: this }
    ]

    this.items = [
      { xtype: 'hidden', name: 'Scope', value: scope },
      { xtype: 'hidden', name: 'Application', value: application },
      { xtype: 'hidden', name: 'DataLayer', value: this.dataLayer }
    ];

    // super
    AdapterManager.SpreadsheetWorksheetSelection.superclass.initComponent.call(this);
  },

  onUpload: function () {
    that = this;

    this.getForm().submit({
      waitMsg: 'Uploading file...',
      url: this.url,
      success: function (f, a) {
        that.fireEvent('Uploaded', that, f.items.items[3].value);
      },
      failure: function (f, a) {
        Ext.Msg.alert('Warning', 'Error uploading file "' + f.items.items[3].value + '"!');
      }
    });
  }

});

/**
* @class AdapterManager.SpreadsheetLibraryPanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
AdapterManager.SpreadsheetLibraryPanel = Ext.extend(Ext.Panel, {
  title: 'SpreadsheetDataLayer',
  width: 120,

  collapseMode: 'mini',
  //collapsible: true,
  //collapsed: false,
  closable: true,

  layout: 'border',
  border: true,
  split: true,

  scope: null,
  application: null,
  configurationPanel: null,
  // propertyPanel: null,
  // tablesConfigPanel: null,
  url: null,

  btnNext: null,
  btnPrev: null,


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
    }

    var application = "";
    var dataLayer = "";

    if (this.application != null) {
      application = this.application.Name;
      dataLayer = this.application.DataLayer;
    }

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
        ];

    // super
    AdapterManager.SpreadsheetLibraryPanel.superclass.initComponent.call(this);
  },

  buildToolbar: function () {
    return [
      {
        xtype: 'button',
        text: 'Reload',
        handler: this.onReload,
        icon: 'Content/img/16x16/view-refresh.png',
        scope: this
      }, {
        xtype: 'button',
        text: 'Save',
        handler: this.onSave,
        icon: 'Content/img/16x16/document-save.png',
        scope: this
      }, {
        xtype: 'button',
        text: 'Upload',
        handler: this.onUpload,
        icon: 'Content/img/16x16/document-up.png',
        scope: this
      }, {
        xtype: 'button',
        text: 'Download',
        icon: 'Content/img/16x16/document-down.png',
        scope: this,
        handler: function (button) {
          var that = this;
          var downloadUrl = 'spreadsheet/export';
          var scopeName = this.scope;
          var appName = this.application;
          Ext.Ajax.request({
            url: 'spreadsheet/export',    // where you wanna post
            method: 'POST',
            success: function (response, request) {
              var htmlString = '<form action= ' + downloadUrl + ' target=\"_blank\" method=\"post\" style=\"display:none\">' +
                '<input type=\"text\" name=\"scope\" value=' + scopeName +
                '></input><input type=\"text\" name=\"application\" value=' + appName +
                '></input><input type=\"submit\"></input></form>'
              button.el.insertHtml(
                  'beforeBegin',
                  htmlString
              ).submit();
            },   // function called on success
            failure: function (response, request) {
              showDialog(500, 150, 'Error', 'The file does not exist. Need to upload a spreadsheet first.', Ext.Msg.OK, null);
            },
            params: {
              scope: scopeName,
              application: appName
            }
          })
        }        
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
      width: 420,
      // layout: 'fit',
      //height: 300,
      autoScroll: true,
      title: "Upload Spreadsheet",
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
    var that = this;
    Ext.Ajax.request({
      url: 'spreadsheet/configure',    // where you wanna post
      method: 'POST',
      success: function (response, request) {

        /*need the service send error message { success = false } + msg
        var rtext = response.responseText;
        var error = 'SUCCESS = FALSE';
        var index = rtext.toUpperCase().indexOf(error);
        if (index == -1) {
        showDialog(400, 100, 'Saving Result', 'Configuration has been saved successfully.', Ext.Msg.OK, null);
        that.configurationPanel.root.reload();
        }
        else {
        var msg = rtext.substring(index + error.length + 2, rtext.length - 1);
        showDialog(400, 100, 'An error has occurred while saving the configuration.', msg, Ext.Msg.OK, null);
        }*/

        // that.configurationPanel.root.reload();
        that.fireEvent('Save', that);
        showDialog(400, 100, 'Saving Result', 'Configuration has been saved successfully.', Ext.Msg.OK, null);
      },   // function called on success
      failure: function (response, request) {
        showDialog(660, 300, 'Saving Result', 'An error has occurred while saving the configuration.', Ext.Msg.OK, null);
      },
      params: {
        Scope: this.scope,
        Application: this.application,
        DataLayer: this.datalayer
      }
    });

  },

  showContextMenu: function (node, event) {

    //  if (node.isSelected()) { 
    var x = event.browserEvent.clientX;
    var y = event.browserEvent.clientY;

    var obj = node.attributes;

    if (obj.type == "ExcelWorkbookNode") {
      this.workbookMenu.showAt([x, y]);
    } else if (obj.type == "ExcelWorksheetNode") {
      this.worksheetMenu.showAt([x, y]);
    } else if (obj.type == "ExcelColumnNode") {
      this.columnMenu.showAt([x, y]);
    }
    //}
  },

  onUpdate: function (panel) {
    var that = this;
    Ext.Ajax.request({
      url: 'spreadsheet/updateconfiguration',    // where you wanna post
      method: 'POST',
      success: function (response, request) {

        /* need the service send error message { success = false } + msg
        var rtext = response.responseText;
        var error = 'SUCCESS = FALSE';
        var index = rtext.toUpperCase().indexOf(error);
        if (index == -1) {
        showDialog(400, 100, 'Saving Result', 'Configuration has been updated successfully.', Ext.Msg.OK, null);
        that.configurationPanel.root.reload();
        }
        else {
        var msg = rtext.substring(index + error.length + 2, rtext.length - 1);
        showDialog(400, 100, 'An error has occurred while updating the configuration.', msg, Ext.Msg.OK, null);
        }*/

        that.configurationPanel.root.reload();
        showDialog(400, 100, 'Saving Result', 'Configuration has been updated successfully.', Ext.Msg.OK, null);
      },   // function called on success
      failure: function (response, request) {
        showDialog(660, 300, 'Saving Result', 'An error has occurred updating update the configuration.', Ext.Msg.OK, null);
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
