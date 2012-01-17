Ext.define('AM.view.spreadsheet.SpreadsheetConfigPanel', {
    extend: 'Ext.tree.Panel',
    alias: 'widget.spreadsheetconfig',
    title: 'Spreadsheet Config',
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

        //    this.treeLoader.on("beforeload", function (treeLoader, node) {
        //      treeLoader.baseParams.type = node.attributes.type;
        //    }, this);

        //    this.rootNode = new Ext.tree.AsyncTreeNode({
        //      id: 'root',
        //      text: 'Workbook',
        //      expanded: true,
        //      draggable: false,
        //      icon: 'Content/img/excel.png',
        //      type: 'ExcelWorkbookNode'
        //    });

        this.configurationPanel = Ext.create('Ext.tree.Panel', {
            layout: 'fit',
            region: 'west',
            border: false,
            split: false,
            lines: true,
            expandAll: true,
            rootVisible: true,
            autoScroll: true,
            width: 500,
            store: null
        });

        this.configurationPanel.on('click', this.onClick, this);

        this.propertyPanel = new Ext.grid.property.Grid({
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
            source: {},
            listeners: {
                beforeedit: function (e) {
                    e.cancel = true;
                },
                afteredit: function (e) {
                    e.grid.getSelectionModel().selections.items[0].data.value = e.originalValue;
                    e.record.data.value = e.originalValue;
                    e.value = e.originalValue;
                    e.grid.getView().refresh();
                }
            }
        });

        this.tablesConfigPanel = new Ext.panel.Panel({
            layout: 'fit',
            region: 'center',
            minWidth: 10,
            frame: false,
            border: false,
            autoScroll: true
        });

        this.items = [
            this.configurationPanel,
            this.tablesConfigPanel
        ];

        // super
        this.callParent(arguments);
    },

    buildToolbar: function () {
        return [
      {
          text: 'Reload',
          //handler: this.onReload,
          icon: 'Content/img/16x16/view-refresh.png',
          scope: this,
          action: 'reloadspreadsheet'
      },
      {
          text: 'Save',
          //handler: this.onSave,
          icon: 'Content/img/16x16/document-save.png',
          scope: this,
          action: 'savespreadsheet'
      },
      {
          text: 'Upload',
          //handler: this.onUpload,
          //icon: 'Content/img/list-remove.png',
          scope: this,
          action: 'uploadspreadsheet'
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
            url: 'spreadsheet/configure',  
            method: 'POST',
            success: function (f, a) {

            },   
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
            url: 'spreadsheet/updateconfiguration',   
            method: 'POST',
            success: function (f, a) {
                that.configurationPanel.root.reload();
            },    
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