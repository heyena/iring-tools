﻿Ext.ns('AdapterManager');

AdapterManager.CacheInfoPanel = Ext.extend(Ext.Window, {
  layout: 'fit',
  width: 440,
  height: 235,
  closable: true,
  resizable: false,
  modal: true,
  form: null,
  node: null,
  scope: null,
  app: null,

  initComponent: function () {
    this.scope = this.node.parentNode.parentNode.attributes.property['Internal Name'];
    this.app = this.node.parentNode.attributes.property['Internal Name'];

    this.form = new Ext.FormPanel({
      method: 'POST',
      bodyStyle: 'padding:10px',
      border: false,
      frame: false,
      defaults: {
        xtype: 'textfield',
        labelWidth: 100,
        width: 295,
        msgTarget: 'side',
        readOnly: true
      },

      items: [
        { name: 'importURI', fieldLabel: 'Import URI' },
        { name: 'timeout', fieldLabel: 'Timeout' },
        { name: 'lastUpdate', fieldLabel: 'Last Update', xtype: 'textarea', height: 90, autoScroll: true }
      ],

      buttons: [{
        xtype: "tbbutton",
        id: 'RefreshCacheBtn',
        text: 'Refresh',
        disabled: false,
        handler: this.onRefreshCache,
        scope: this
      }, {
        xtype: "tbbutton",
        id: 'ImportCacheBtn',
        text: 'Import',
        disabled: false,
        handler: this.onImportCache,
        scope: this
      }, {
        xtype: "tbbutton",
        text: 'Cancel',
        handler: this.onCancel,
        scope: this
      }]
    });

    this.items = [
      this.form
    ];

    AdapterManager.CacheInfoPanel.superclass.initComponent.call(this);
  },

  display: function () {
    Ext.Ajax.request({
      url: 'AdapterManager/CacheInfo',
      method: 'POST',
      params: {
        'scope': this.scope,
        'app': this.app,
        'panelId': this.id
      },
      success: function (response, request) {
        var cacheInfo = Ext.decode(response.responseText);
        var panel = Ext.getCmp(request.params.panelId);
        var form = panel.form.getForm();

        if (cacheInfo.importURI == null) {
          Ext.getCmp('ImportCacheBtn').setDisabled(true);
        }
        else {
          form.findField('importURI').setValue(cacheInfo.importURI);
        }

        if (cacheInfo.timeout == null || cacheInfo.timeout === 0) {
          form.findField('timeout').setValue(3600000);
        }
        else {
          form.findField('timeout').setValue(cacheInfo.timeout);
        }

        if (cacheInfo.cacheEntries != null) {
          var lastUpdates = '';

          for (var i = 0; i < cacheInfo.cacheEntries.length; i++) {
            if (i > 0) {
              lastUpdates += '\n';
            }

            var entry = cacheInfo.cacheEntries[i];
            lastUpdates += entry.objectName + ' (' + entry.lastUpdate + ')';
          }

          form.findField('lastUpdate').setValue(lastUpdates);
        }

        panel.show();
      },
      failure: function (response, request) {
        var message = 'Error getting cache information!';
        showDialog(400, 100, 'Error', message, Ext.Msg.OK, null);
      }
    });
  },

  onRefreshCache: function (btn, ev) {
    var timeout = this.form.form.findField('timeout').getValue();

    this.form.setDisabled(true);
    this.form.getEl().mask('Processing cache refresh...', 'x-mask-loading');

    Ext.Ajax.request({
      url: 'AdapterManager/RefreshCache',
      method: 'POST',
      timeout: timeout,
      params: {
        'scope': this.scope,
        'app': this.app,
        'timeout': timeout,
        'panelId': this.id
      },
      success: function (response, request) {
        var panel = Ext.getCmp(request.params.panelId);
        panel.node.parentNode.reload();
        panel.form.setDisabled(false);
        panel.form.getEl().unmask();

        var responseObj = Ext.decode(response.responseText);

        if (responseObj.Level == 0) {
          panel.close();
          showDialog(450, 100, 'Refresh Cache Result', 'Cache refreshed successfully.', Ext.Msg.OK, null);
        }
        else {
          showDialog(500, 160, 'Refresh Cache Error', responseObj.Messages.join('\n'), Ext.Msg.OK, null);
        }
      },
      failure: function (response, request) {
        var panel = Ext.getCmp(request.params.panelId);
        panel.node.parentNode.reload();
        panel.form.setDisabled(false);
        panel.form.getEl().unmask();

        var responseObj = Ext.decode(response.responseText);
        showDialog(500, 160, 'Refresh Cache Error', responseObj.Messages.join('\n'), Ext.Msg.OK, null);
      }
    });
  },

  onImportCache: function (btn, ev) {
    var form = this.form.form;
    var timeout = form.findField('timeout').getValue();
    var importURI = form.findField('importURI').getValue();

    this.form.setDisabled(true);
    this.form.getEl().mask('Processing cache import...', 'x-mask-loading');

    Ext.Ajax.request({
      url: 'AdapterManager/ImportCache',
      method: 'POST',
      timeout: timeout,
      params: {
        'scope': this.scope,
        'app': this.app,
        'timeout': timeout,
        'importURI': importURI,
        'panelId': this.id
      },
      success: function (response, request) {
        var panel = Ext.getCmp(request.params.panelId);
        panel.node.parentNode.reload();
        panel.form.setDisabled(false);
        panel.form.getEl().unmask();

        var responseObj = Ext.decode(response.responseText);

        if (responseObj.Level == 0) {
          panel.close();
          showDialog(450, 100, 'Import Cache Result', 'Cache imported successfully.', Ext.Msg.OK, null);
        }
        else {
          showDialog(500, 160, 'Import Cache Error', responseObj.Messages.join('\n'), Ext.Msg.OK, null);
        }
      },
      failure: function (response, request) {
        var panel = Ext.getCmp(request.params.panelId);
        panel.node.parentNode.reload();
        panel.form.setDisabled(false);
        panel.form.getEl().unmask();

        var responseObj = Ext.decode(response.responseText);
        showDialog(500, 160, 'Import Cache Error', responseObj.Messages.join('\n'), Ext.Msg.OK, null);
      }
    });
  },

  onCancel: function () {
    this.close();
  }
});
