Ext.define('AM.controller.ConfigurationController', {
    extend: 'Ext.app.Controller',
    views: [
      'spreadsheet.SpreadsheetSource'
    , 'spreadsheet.SpreadsheetConfigPanel'
    , 'common.PropertyPanel'
    , 'common.ContentPanel'
    , 'common.CenterPanel'
    ],
    stores: [],
    models: [
      'SpreadsheetModel'
    ],
    refs: [
      {
          ref: 'dirTree',
          selector: 'viewport > directorypanel > directorytree'
      },
      {
          ref: 'mainContent',
          selector: 'viewport > centerpanel > contentpanel'
      }
    ],
    init: function () {
        this.control({
            'button[action=configureendpoint]': {
                click: this.onConfigureEndpoint
            },
            'button[action=uploadspreadsheet]': {
                click: this.onUploadspreadsheet
            },
            'button[action=savespreadsheet]': {
                click: this.onSaveSpreadsheet
            },
            'button[action=reloadspreadsheet]': {
                click: this.onReloadSpreadsheet
            }
        })
    },
    onSaveSpreadsheet: function () {
        var tree = this.getDirTree(),
        node = tree.getSelectedNode();
        var contextName = node.data.record.context;
        var datalayer = node.data.record.Assembly;
        var endpointName = node.data.record.endpoint;
        Ext.Ajax.request({
            url: 'spreadsheet/configure',    // where you wanna post
            method: 'POST',
            success: function (f, a) {

            },   // function called on success
            failure: function (f, a) {

            },
            params: {
                context: contextName,
                endpoint: endpointName,
                DataLayer: datalayer
            }
        });
    },
    onUploadspreadsheet: function (panel) {
        var tree = this.getDirTree(),
        node = tree.getSelectedNode();
        var contextName = node.data.record.context;
        var datalayer = node.data.record.DataLayer;
        var endpoint = node.data.record.endpoint;
        var that = this;
        var sourceconf = {
            width: 450,
            title: 'Upload ' + contextName + '-' + endpoint,
            context: contextName,
            endpoint: endpoint,
            DataLayer: datalayer,
            method: 'POST',
            url: 'spreadsheet/upload'
        },
        form = Ext.widget('spreadsheetsource', sourceconf);
        form.show();
    },

    onConfigureEndpoint: function () {
        var tree = this.getDirTree(),
        node = tree.getSelectedNode();
        switch (node.data.record.DataLayer) {
//            case 'NHibernateLibrary':
//                alert('NHibernate Library');
//                break;
            case 'SpreadsheetDataLayer':
                var content = this.getMainContent();
                var contextName = node.data.record.context;
                var datalayer = node.data.record.DataLayer;
                var endpoint = node.data.record.endpoint;
                var conf =
                {
                    context: contextName,
                    endpoint: endpoint,
                    datalayer: datalayer,
                    url: 'spreadsheet/configure'
                };
                var sctree = Ext.widget('spreadsheetconfig', conf);
                var scprop = Ext.widget('propertypanel', { title: 'Details', region: 'east', width: 350, height: 150, split: true, collapsible: true });
                var panconf = {
                    id: 'tab-c.' + contextName + '.' + endpoint,
                    title: 'Spreadsheet Configuration - ' + contextName + '.' + endpoint,
                    height: 300,
                    minSize: 250,
                    layout: {
                        type: 'border',
                        padding: 2
                    },
                    split: true,
                    closable: true,
                    iconCls: 'tabsMapping',
                    items: []
                },
                scpanel = Ext.widget('panel', panconf);
                scpanel.items.add(sctree);
                scpanel.items.add(scprop);
                sctree.on('beforeitemexpand', function () {
                    content.getEl().mask('Loading...');
                }, this);

                sctree.on('load', function () {
                    content.getEl().unmask();
                }, this);

                sctree.on('itemexpand', function () {
                    content.getEl().unmask();
                }, this);

                sctree.on('itemclick', function (view, model, n, index) {
                    var obj = model.store.getAt(index).data;
                    if (obj.record != null && obj.record != "") {
                        scprop.setSource(obj.record);
                    }
                }, this);

                var exist = content.items.map[panconf.id];
                if (exist == null) {
                    content.add(scpanel).show();
                } else {
                    exist.show();
                }

                tree.applicationMenu.hide();
                break;
        }
    }
});