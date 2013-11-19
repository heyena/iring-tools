Ext.define('AM.controller.Spreadsheet', {
  extend: 'Ext.app.Controller',

  models: [
    'SpreadsheetModel'
  ],
  stores: [
    'SpreadsheetStore'
  ],
  views: [
    'spreadsheet.SpreadsheetTree',
    'spreadsheet.SpreadsheetWindow',
    'spreadsheet.SpreadsheetForm',
    'spreadsheet.SpreadsheetPanel',
    'spreadsheet.SourceWindow'
  ],

  refs: [
    {
      ref: 'dirTree',
      selector: 'viewport > directorypanel > directorytree',
      xtype: 'directorytree'
    },
    {
      ref: 'mainContent',
      selector: 'viewport > centerpanel > contentpanel',
      xtype: 'contentpanel'
    },
    {
      ref: 'spreadsheetPanel',
      selector: 'spreadsheetpanel'
    },
    {
      //autoCreate: true,
      ref: 'spreadsheetSourceWindow',
      selector: 'spreadsheetsourcewindow',
      xtype: 'spreadsheetsourcewindow'
    }
  ],

  onOpenUploadForm: function(button, e, eOpts) {
    var me = this;
    var win = me.getSpreadsheetSourceWindow();
    var panel = button.up('spreadsheetpanel');
    var title = 'Upload Spreadsheet for ' + panel.context + '.' + panel.endpoint;

    win.setTitle(title);

    var form = win.down('spreadsheetsourceform');

    form.getForm().findField('context').setValue(panel.context);
    form.getForm().findField('endpoint').setValue(panel.endpoint);
    form.getForm().findField('datalayer').setValue(panel.datalayer);

    win.show();
  },

  onUploadSpreadsheet: function(button, e, eOpts) {
    var me = this;
    var win = me.getSpreadsheetSourceWindow();
    var form = win.down('spreadsheetsourceform');
    var pan = me.getSpreadsheetPanel();
    var tree = pan.down('spreadsheettree');
    form.getForm().submit({
      waitMsg: 'Uploading file...',
      success: function (f, a) {
        win.close();
        tree.onReload();
      },
      failure: function (f, a) {
        Ext.Msg.alert('Warning', 'Error uploading file "' + f.items[3] + '"!');
      }

    });
  },

  onSaveSpreadsheet: function(button, e, eOpts) {
    var me = this;
    var pan = me.getSpreadsheetPanel();
    var tree = pan.down('spreadsheettree');
    var context = pan.context;
    var endpoint = pan.endpoint;
    var datalayer = pan.datalayer;

    Ext.Ajax.request({
      url: 'Spreadsheet/Configure',    // where you wanna post
      method: 'POST',
      params: {
        'context': context,
        'endpoint': endpoint,
        'DataLayer': datalayer
      },
      success: function (f, a) {
        Ext.widget('messagepanel', { title: 'Saving Result', msg: 'Configuration has been saved successfully.'});
		//showDialog(350, 80, 'Saving Result', 'Configuration has been saved successfully.', Ext.Msg.OK, null);          
        tree.onReload();
      },   // function called on success
      failure: function (f, a) {

      }
    });
  },

  init: function(application) {
    var me = this;
    me.application.addEvents('configspreadsheet');
    Ext.QuickTips.init();

    this.control({
      "button[action=openuploadform]": {
        click: this.onOpenUploadForm
      },
      "spreadsheetsourcewindow button[action=uploadspreadsheet]": {
        click: this.onUploadSpreadsheet
      },
      "spreadsheetpanel button[action=savespreadsheet]": {
        click: this.onSaveSpreadsheet
      }
    });

    application.on({
      configspreadsheet: {
        fn: this.onConfigSpreadsheet,
        scope: this
      }
    });
  },

  onConfigSpreadsheet: function() {
    var me = this;
    var dirTree = me.getDirTree(),
      dirNode = dirTree.getSelectedNode(),
      content = me.getMainContent();

    var contextName = dirNode.data.record.ContextName;
    var datalayer = dirNode.data.record.DataLayer;
    var endpoint = dirNode.data.record.Endpoint;
    var title = 'Spreadsheet Configuration - ' + contextName + '.' + endpoint;

    var scPanel = content.down('spreadsheetpanel[title=' + title+']');

    if(!scPanel) {
      scPanel = Ext.widget('spreadsheetpanel', {
        'title': title,
        context : contextName,
        datalayer : datalayer,
        endpoint : endpoint
      });
      var scTree = scPanel.down('spreadsheettree');

      var treeStore = scTree.getStore();
      var scProp = scPanel.down('propertypanel');
      treeStore.on('beforeload', function (store, operation, eopts) {
        var params = store.getProxy().extraParams;
        params.context = contextName;
        params.endpoint = endpoint;
      }, me);

      scTree.on('beforeitemexpand', function () {
        content.getEl().mask('Loading...');
      }, me);

      scTree.on('load', function () {
        content.getEl().unmask();
      }, me);

      scTree.on('itemexpand', function () {
        content.getEl().unmask();
      }, me);

      scTree.on('itemclick', function (view, model, n, index) {
        var obj = model.store.getAt(index).data;
        if (obj.record !== null && obj.record !== "") {
          scProp.setSource(obj.record);
        }
      }, me);

      scPanel.on('save', function () {
        dirtree.onReload(node);
      }, me);

      content.add(scPanel);
      treeStore.load();
    }

    content.setActiveTab(scPanel);
  }

});
