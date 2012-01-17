Ext.define('AM.controller.ConfigurationController', {
    extend: 'Ext.app.Controller',
    views: [
      'spreadsheet.SourcePanel'
    , 'spreadsheet.SpreadsheetConfigPanel'
    , 'spreadsheet.ConfigWindow'
    ],
    stores: [],
    models: [
      'SpreadsheetModel'
    ],
    refs: [
      {
          ref: 'source',
          selector: 'sourcepanel'
      },
      {
          ref: 'spreadsheetconfig',
          selector: 'spreadsheetconfig'
      },
      {
          ref: 'configwindow',
          selector: 'configwin'
      },
      {
            ref: 'dirTree',
            selector: 'viewport > directorypanel > directorytree'
        }
    ],
    init: function () {
        this.control({
            'button[action=uploadspreadsheet]': {
                click: this.onUpload
            },
            'menu button[action=configureendpoint]': {
                click: this.onConfigureEndpoint
            }
        })
    },

    onUpload: function (panel) {
        var that = this;
        var sourceconf = {
            Scope: that.scope,
            Application: that.application,
            DataLayer: that.datalayer,
            method: 'POST',
            url: 'spreadsheet/upload'
        },
        form = Ext.widget('sourcepanel', sourceconf),
        winconf = {
            items: form
        },
        win = Ext.widget('configwin', winconf);

        form.on('uploaded', function () {
            win.close();
            this.configurationPanel.root.reload();
        }, this);

        newWin.show();
    },

    onConfigureEndpoint: function () {
        var tree = this.getDirTree(),
        node = tree.getSelectedNode();
        if (node.data.type === 'ApplicationNode' && node.data.record.DataLayer === 'NHibernateLibrary') {
            alert('NHibernate Library');
        } else if (node.data.type === 'ApplicationNode' && node.data.record.DataLayer === 'SpreadsheetLibrary') {
            alert('Spreadsheet Datalayer');
        }
    }
});