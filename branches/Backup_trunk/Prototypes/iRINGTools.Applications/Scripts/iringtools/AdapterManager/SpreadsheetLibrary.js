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
          { xtype: 'button', text: 'Cancel', scope: this }
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
      { xtype: 'button', text: 'Upload', scope: this, handler: this.onUpload },
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
  title: 'SpreadsheetLibrary',
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
  propertyPanel: null,
  tablesConfigPanel: null,
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

    this.workbookMenu = new Ext.menu.Menu();
    this.workbookMenu.add(this.buildWorkbookMenu());

    this.worksheetMenu = new Ext.menu.Menu();
    this.worksheetMenu.add(this.buildWorksheetMenu());

    this.columnMenu = new Ext.menu.Menu();
    this.columnMenu.add(this.buildColumnMenu());

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

    this.configurationPanel.on('contextmenu', this.showContextMenu, this);
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

  buildWorkbookMenu: function () {
    return [
            {
              text: 'Add Worksheet(s)',
              handler: this.onAddWorkSheets,
              icon: 'Content/img/16x16/document-new.png',
              scope: this
            },
            {
              text: 'Reload Worksheets',
              handler: this.onReloadNode,
              icon: 'Content/img/16x16/view-refresh.png',
              scope: this
            }
        ]
  },

  buildWorksheetMenu: function () {
    return [
            {
              text: 'Edit Worksheet',
              handler: this.onEditWorksheet,
              icon: 'Content/img/16x16/document-open.png',
              scope: this
            },
            {
              text: 'Remove Worksheet',
              handler: this.onRemoveWorksheet,
              icon: 'Content/img/16x16/edit-delete.png',
              scope: this
            },
            {
              xtype: 'menuseparator'
            },
            {
              text: 'Add Worksheet(s)',
              handler: this.onAddWorksheets,
              icon: 'Content/img/16x16/document-new.png',
              scope: this
            },
            {
              text: 'Add Column(s)',
              handler: this.onAddColumns,
              icon: 'Content/img/16x16/document-new.png',
              scope: this
            }
        ]
  },

  buildColumnMenu: function () {
    return [
            {
              text: 'Rename Column',
              handler: this.onRenameColumn,
              icon: 'Content/img/16x16/document-open.png',
              scope: this
            },
            {
              text: 'Remove Column',
              handler: this.onRemoveColumn,
              icon: 'Content/img/16x16/edit-delete.png',
              scope: this
            },
            {
              xtype: 'menuseparator'
            },
            {
              text: 'Edit Key(s)',
              handler: this.onEditKeys,
              icon: 'Content/img/16x16/document-new.png',
              scope: this
            },
             {
               text: 'Add Column(s)',
               handler: this.onAddColumns,
               icon: 'Content/img/16x16/document-new.png',
               scope: this
             }
        ]
  },

  onAddWorksheets: function (node) {

  },

  onAddColumns: function (node) {
  },

  ononEditKeys: function (node) {
  },

  onEditWorksheet: function (node) {
  },

  onRemoveWorksheet: function (node) {
  },

  onRenameColumn: function (node) {
  },

  onRemoveColumn: function (node) {
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
  },
  onClick: function (node) {
    try {
      //  this.propertyPanel.setSource(node.attributes.record);

      var obj = node.attributes
      var form = null;

      if (this.tablesConfigPanel.items.getCount() > 0 && this.tablesConfigPanel.items.items[0].form.isDirty() == true) {
        this.onUpdate(this);
      }
      //                var that = this;
      //                
      //                Ext.Msg.show({
      //                    msg: 'Would you like to update the changes?',
      //                    buttons: Ext.Msg.YESNO,
      //                    icon: Ext.Msg.QUESTION, //'profile', // &lt;- customized icon
      //                    fn: function (action) {
      //                        if (action == 'yes') {
      //                            that.onUpdate(that);
      //                        }
      //                        else if (action == 'no') {
      //                            reviewed = false;
      //                        }
      //                    }
      //                });

      //            }

      if (obj.type == "ExcelWorksheetNode") {
        form = new Ext.FormPanel({
          labelWidth: 80, // label settings here cascade unless
          url: null,
          method: 'POST',
          bodyStyle: 'padding:5px 5px 0',
          autoScroll: true,
          border: false, // removing the border of the form

          frame: true,
          closable: true,
          defaults: {
            width: 150,
            msgTarget: 'side',
            xtype: 'textfield'
          },
          items: [{ xtype: 'hidden', name: 'Scope', value: this.scope },
                            { xtype: 'hidden', name: 'Application', value: this.application },
                            { xtype: 'hidden', name: 'DataLayer', value: this.dataLayer },
                            { xtype: 'hidden', name: 'Name', value: node.attributes.record.Name },
                            { fieldLabel: 'DataIdx', allowBlank: false, disabled: true, value: node.attributes.record.DataIdx },
                            { fieldLabel: 'HeaderIdx', allowBlank: false, disabled: true, value: node.attributes.record.HeaderIdx },
                            { fieldLabel: 'Identifier', allowBlank: false, disabled: true, value: node.attributes.record.Identifier },
                            { fieldLabel: 'Name', allowBlank: false, disabled: true, value: node.attributes.record.Name },
                            { fieldLabel: 'Label', name: 'Label', allowBlank: false, value: node.attributes.record.Label
                            }],
          buttons: [{
            text: 'Save',
            formBind: true,
            handler: this.onUpdate,
            scope: this
          }, { text: 'Reset',
            formBind: true,
            handler: this.onReset,
            scope: this
          }],
          buttonAlign: 'left', // buttons aligned to the left            
          autoDestroy: true
        });
      } else if (obj.type == "ExcelColumnNode") {
        form = new Ext.FormPanel({
          labelWidth: 50, // label settings here cascade unless
          url: null,
          method: 'POST',
          bodyStyle: 'padding:5px 5px 0',
          border: false, // removing the border of the form
          autoScroll: true,

          frame: true,
          closable: true,
          defaults: {
            width: 150,
            msgTarget: 'side',
            xtype: 'textfield'
          },
          items: [{ xtype: 'hidden', name: 'Scope', value: this.scope },
                            { xtype: 'hidden', name: 'Application', value: this.application },
                            { xtype: 'hidden', name: 'DataLayer', value: this.dataLayer },
                            { xtype: 'hidden', name: 'Name', value: node.attributes.record.Name },
                            { fieldLabel: 'Datatype', allowBlank: false, disabled: true, value: node.attributes.record.Datatype },
                            { fieldLabel: 'Index', allowBlank: false, disabled: true, value: node.attributes.record.Index },
                            { fieldLabel: 'Name', allowBlank: false, disabled: true, value: node.attributes.record.Name },
                            { fieldLabel: 'Label', name: 'Label', allowBlank: false, value: node.attributes.record.Label
                            }],
          buttons: [{
            text: 'Save',
            formBind: true,
            handler: this.onUpdate,
            scope: this
          }, { text: 'Reset',
            formBind: true,
            handler: this.onReset,
            scope: this
          }],
          buttonAlign: 'left', // buttons aligned to the left            
          autoDestroy: false
        });
      } else if (obj.type == "Identifier") {
        var availItems = new Array();
        var selectedItems = new Array();
        var itemSelector = new Array();

        itemSelector = node.parentNode.attributes.record.Columns;

        for (var i = 0; i < itemSelector.length; i++) {
          if (itemSelector[i].Name == node.parentNode.attributes.record.Identifier) {
            selectedItems.push([itemSelector[i].Name, itemSelector[i].Label]);
          }
          else {
            availItems.push([itemSelector[i].Name, itemSelector[i].Label]);
          }
        }

        form = new Ext.FormPanel({
          url: null,
          method: 'POST',
          bodyStyle: 'padding:5px 5px 0',
          border: false, // removing the border of the form
          autoScroll: true,

          frame: true,
          closable: true,
          items: [{
            xtype: 'itemselector',
            name: 'keySelector',
            fieldLabel: 'Select Keys',
            imagePath: 'scripts/ext-3.3.1/examples/ux/images/',
            multiselects: [{
              width: 150,
              height: 300,
              store: availItems,
              displayField: 'keyName',
              valueField: 'keyValue'
            }, {
              width: 150,
              height: 300,
              store: selectedItems,
              displayField: 'keyName',
              valueField: 'keyValue'
            }]
          }],
          buttons: [{
            text: 'Save',
            formBind: true,
            //  handler: this.onUpdate,
            scope: this
          }, { text: 'Reset',
            formBind: true,
            handler: this.onReset,
            scope: this
          }],
          buttonAlign: 'left', // buttons aligned to the left            
          autoDestroy: false
        });

      } else if (obj.type == "columns") {
        var availItems = new Array();
        var selectedItems = new Array();
        var itemSelector = new Array();

        itemSelector = node.parentNode.attributes.record.Columns;
        for (var i = 0; i < itemSelector.length; i++) {
          if (itemSelector[i].Name == node.parentNode.attributes.record.Name) {
            selectedItems.push([itemSelector[i].Name, itemSelector[i].Label]);
          }
          else {
            availItems.push([itemSelector[i].Name, itemSelector[i].Label]);
          }
        }
        form = new Ext.FormPanel({
          url: null,
          method: 'POST',
          bodyStyle: 'padding:5px 5px 0',
          border: false, // removing the border of the form
          autoScroll: true,

          frame: true,
          closable: true,
          items: [{
            xtype: 'itemselector',
            name: 'columnSelector',
            fieldLabel: 'Select Columns',
            imagePath: 'scripts/ext-3.3.1/examples/ux/images/',
            multiselects: [{
              width: 150,
              height: 300,
              store: availItems,
              displayField: 'columnName',
              valueField: 'keyValue'
            }, {
              width: 150,
              height: 300,
              store: selectedItems,
              displayField: 'coloumnName',
              valueField: 'columnValue'
            }]
          }],
          buttons: [{
            text: 'Save',
            formBind: true,
            //  handler: this.onUpdate,
            scope: this
          }, { text: 'Reset',
            formBind: true,
            handler: this.onReset,
            scope: this
          }],
          buttonAlign: 'left', // buttons aligned to the left            
          autoDestroy: false
        });
      }

      this.tablesConfigPanel.removeAll();
      this.tablesConfigPanel.add(form);
      this.tablesConfigPanel.events.bodyresize.fire();

    } catch (e) {
      // alert(e);
    }
  }

});
