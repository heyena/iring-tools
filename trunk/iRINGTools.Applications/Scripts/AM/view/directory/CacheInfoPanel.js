
Ext.define('AM.view.directory.CacheInfoPanel', {
    extend: 'Ext.form.Panel',
    alias: 'widget.cacheinfopanel',
    height: 250,
    width: 390,
    bodyPadding: 10,
    bodyStyle: 'padding:10px 5px 0',
    //url: 'directory/Scope',
    //cacheConnStrTpl: 'Data Source={hostname\\dbInstance};Initial Catalog={dbName};User ID={userId};Password={password}',

    initComponent: function () {
        var me = this;
        me.initialConfig = Ext.apply({
            //url: 'directory/Scope'
        }, me.initialConfig);

        Ext.applyIf(me, {
            defaults: {
                anchor: '100%',
                msgTarget: 'side'
            },
            dockedItems: [
        {
            xtype: 'toolbar',
            dock: 'bottom',
            items: [
            {
                xtype: 'tbfill',
                height: 24
            }, 
			{
                xtype: 'tbbutton',
                itemId: 'RefreshCacheBtn',
                text: 'Refresh',
                disabled: false
                //handler: this.onRefreshCache,
                //scope: this
            }, 
			{
                xtype: 'tbspacer',
                width: 5
            }, 
			{
                xtype: 'tbbutton',
                itemId: 'ImportCacheBtn',
                text: 'Import',
                disabled: false
                //handler: this.onImportCache,
                //scope: this
            },
			{
                xtype: 'tbspacer',
                width: 5
            }, 
			{
                xtype: 'tbbutton',
                text: 'Cancel'
                //handler: this.onCancel,
                //scope: this
            }
          ]
        }
      ],
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
			height: 90,
			autoScroll: true
        }
         ]
        });
        me.callParent(arguments);
    }
});