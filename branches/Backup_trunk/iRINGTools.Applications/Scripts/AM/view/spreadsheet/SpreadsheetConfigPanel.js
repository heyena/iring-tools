Ext.define('AM.view.spreadsheet.SpreadsheetConfigPanel', {
    extend: 'Ext.tree.Panel',
    alias: 'widget.spreadsheetconfig',
    region: 'center',
    collapseMode: 'mini',
    layout: 'fit',
    border: true,
    lines: true,
    scroll: 'both',
    split: true,
    context: null,
    endpoint: null,
    datalayer: null,
    baseurl: null,
    assembly: null,
    configurationPanel: null,
    propertyPanel: null,
    tablesConfigPanel: null,
    url: null,
    store: null,
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
            selectionchange: true,
            downloaded: true
        });
        var context = "";
        if (this.context != null) {
            context = this.context;
        };

        var endpoint = "";
        var dataLayer = "";

        if (this.endpoint != null) {
            endpoint = this.endpoint;
            dataLayer = this.datalayer;
        };

        this.tbar = this.buildToolbar();

        var me = this;
        Ext.apply(me, {
            store: Ext.create('Ext.data.TreeStore', {
                model: 'AM.model.SpreadsheetModel',
                autoLoad: false,
                storeId: me.id,
                clearOnLoad: true,
                root: {
                    expanded: true, 
                    text: 'WorkBook',
                    icon: 'Content/img/excel.png',
                    type: 'ExcelWorkbookNode'
                },
                proxy: {
                  url: 'Spreadsheet/GetNode',
                    type: 'ajax',
                    timeout: 600000,
                    actionMethods: { read: 'POST' },
                    extraParams: {
                        type: 'ExcelWorkbookNode',
                        context: me.context,
                        endpoint: me.endpoint,
                        baseurl: me.baseurl
                    },
                    reader: { type: 'json' }
                }
            })
        });

        this.callParent(arguments);
       
    },

    buildToolbar: function () {
        return [
      {
          text: 'Reload',
          icon: 'Content/img/16x16/view-refresh.png',
          scope: this,
          action: 'reloadspreadsheet'
      },
      {
          text: 'Save',
          icon: 'Content/img/16x16/document-save.png',
          scope: this,
          action: 'savespreadsheet'
      },
      {
          text: 'Upload',
          //icon: 'Content/img/list-remove.png',
          scope: this,
          icon: 'Content/img/16x16/document-up.png',
          action: 'uploadspreadsheet'
      },
      {
          text: 'Download',
          //icon: 'Content/img/list-remove.png',
          scope: this,
          icon: 'Content/img/16x16/document-down.png',
          action: 'downloadspreadsheet'
      }
    ]}
});