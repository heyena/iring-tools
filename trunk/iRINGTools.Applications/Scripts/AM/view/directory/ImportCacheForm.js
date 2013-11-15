Ext.define('AM.view.directory.ImportCacheForm', {
    extend: 'Ext.form.Panel',
    alias: 'widget.importcacheform',
    node: null,
    bodyPadding: 10,
    header: false,
    method: 'post',

    initComponent: function () {
        var me = this;

        Ext.applyIf(me, {
            defaults: {
                anchor: '100%',
                msgTarget: 'side',
                readOnly: true
            },
            items: [
            {
                xtype: 'textfield',
                fieldLabel: 'Import URI',
                name: 'importURI'
            },
		    {
		        xtype: 'hiddenfield',
		        name: 'timeout'
		    },
            {
                xtype: 'textfield',
                fieldLabel: 'Timeout',
                name: 'displayTimeout'
            },
            {
                xtype: 'textareafield',
                fieldLabel: 'Last Update',
                name: 'lastUpdate',
                height: 90
            },
            {
                xtype: 'hiddenfield',
                anchor: '100%',
                name: 'nodeid'
            }
          ],
            dockedItems: [
            {
                xtype: 'toolbar',
                dock: 'bottom',
                layout: {
                    pack: 'end',
                    type: 'hbox'
                },
                items: [
                {
                    xtype: 'button',
                    handler: function (button, event) {
                        me.onRefresh();
                    },
                    text: 'Refresh'
                },
                {
                    xtype: 'button',
                    itemId: 'importButton',
                    handler: function (button, event) {
                        me.onImport();
                    },
                    text: 'Import'
                },
			    {
			        xtype: 'button',
			        handler: function (button, event) {
			            me.onClose();
			        },
			        text: 'Cancel'
			    }
              ]
            }
          ]
        });

        me.callParent(arguments);
    },

    display: function () {
        var me = this;
        var scope = me.node.data.id.split('/')[0];
        var app = me.node.data.id.split('/')[1];

        Ext.Ajax.request({
            url: 'AdapterManager/CacheInfo',
            method: 'POST',
            params: {
                'scope': scope,
                'app': app,
                'panelId': me.id
            },
            success: function (response, request) {
                var cacheInfo = Ext.decode(response.responseText);
                var panel = Ext.getCmp(request.params.panelId);
                var form = panel.getForm();

                if (cacheInfo.importURI == null || cacheInfo.importURI == '') {
                    //Ext.getCmp('importButton').setDisabled(true);
                    Ext.ComponentQuery.query('toolbar')[2].getChildItemsToDisable()[1].setDisabled(true);
                }
                else {
                    form.findField('importURI').setValue(cacheInfo.importURI);
                }

                if (cacheInfo.timeout == null || cacheInfo.timeout === 0) {
                    form.findField('timeout').setValue(60);
                    form.findField('displayTimeout').setValue('60 minutes');
                }
                else {
                    form.findField('timeout').setValue(cacheInfo.timeout);
                    form.findField('displayTimeout').setValue(cacheInfo.timeout + ' minutes');
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
            },
            failure: function (response, request) {
                var panel = Ext.getCmp(request.params.panelId);
                panel.close();

                var message = 'Error getting cache information!';
                showDialog(400, 100, 'Error', message, Ext.Msg.OK, null);
            }
        });
    },

    onRefresh: function () {
        var me = this;
        var timeout = me.getForm().findField('timeout').getValue() * 60000;
        me.setDisabled(true);
        me.getEl().mask('Processing cache refresh request...', 'x-mask-loading');
        var scope = me.node.data.id.split('/')[0];
        var app = me.node.data.id.split('/')[1];

        Ext.Ajax.request({
            url: 'AdapterManager/RefreshCache',
            method: 'POST',
            timeout: timeout,
            params: {
                'scope': scope,
                'app': app,
                'timeout': timeout
            },
            success: function (response, request) {
                var responseObj = Ext.decode(response.responseText);

                if (responseObj.Level == 0) {
                    Ext.ComponentQuery.query('directorytree')[0].onReload();
                    me.up('window').close();
                    Ext.example.msg('Notification', 'Cache refreshed successfully!');
                }
                else {
                    showDialog(500, 160, 'Refresh Cache Error', responseObj.Messages.join('\n'), Ext.Msg.OK, null);
                }
            },
            failure: function (response, request) {
                me.setDisabled(false);
                me.getEl().unmask();
                showDialog(500, 160, 'Refresh Cache Error', response.statusText, Ext.Msg.OK, null);
            }
        });
    },

    onImport: function () {
        var me = this;

        var form = me.getForm();
        var timeout = form.findField('timeout').getValue();
        var importURI = form.findField('importURI').getValue();
        var scope = me.node.data.id.split('/')[0];
        var app = me.node.data.id.split('/')[1];

        me.setDisabled(true);
        me.getEl().mask('Processing cache import request...', 'x-mask-loading');

        Ext.Ajax.request({
            url: 'AdapterManager/ImportCache',
            method: 'POST',
            timeout: timeout,
            params: {
                'scope': scope,
                'app': app,
                'timeout': timeout,
                'importURI': importURI
            },
            success: function (response, request) {
                var responseObj = Ext.decode(response.responseText);

                if (responseObj.Level == 0) {
                    Ext.ComponentQuery.query('directorytree')[0].onReload();
                    me.up('window').close();
                    Ext.example.msg('Notification', 'Cache imported successfully!');
                }
                else {
                    showDialog(500, 160, 'Import Cache Error', responseObj.Messages.join('\n'), Ext.Msg.OK, null);
                }
            },
            failure: function (response, request) {
                me.setDisabled(false);
                panel.getEl().unmask();
                showDialog(500, 160, 'Import Cache Error', response.statusText, Ext.Msg.OK, null);
            }
        });
    },

    onClose: function () {
        var me = this;
        var win = me.up('window');
        win.destroy();
    }
});