Ext.define('AM.view.sqlconfig.SqlKeySelectionPanel', {
    extend: 'Ext.form.Panel',
    alias: 'widget.sqlkeyselectionpanel',
    bodyStyle: 'background:#fff;padding:10px',
    title: 'Select Keys',
    layout: 'fit',
    record: null,
    initComponent: function () {
        var me = this;

        var store = Ext.create('Ext.data.Store', {
            fields: ['name'],
            data: [{ name: ''}]
        });

        Ext.applyIf(me, {
            items: [{
                xtype: 'itemselector',
                itemId: 'keyselector',
                name: 'selectedKeys',
                anchor: '100%',
                imagePath: '../ux/images/',
                store: store,
                displayField: 'name',
                valueField: 'name',
                msgTarget: 'side',
                fromTitle: 'Available',
                toTitle: 'Selected'
            }],
            dockedItems: [
            {
                xtype: 'toolbar',
                height: 32,
                dock: 'top',
                layout: {
                    padding: 2,
                    type: 'hbox'
                },
                items: [
                {
                    xtype: 'tbspacer'
                },
                {
                    xtype: 'button',
                    action: 'apply',
                    iconCls: 'am-apply',
                    text: 'Apply'
                },
                {
                    xtype: 'tbspacer'
                },
                {
                    xtype: 'button',
                    action: 'reset',
                    iconCls: 'am-reset',
                    text: 'Reset',
                    scope: me,
                    handler: me.loadValues
                }
              ]
            }],
            listeners: {
                afterrender: me.loadValues,
                scope: me
            }
        });

        me.callParent(arguments);
    },

    setRecord: function (record, dataview) {
        this.record = record;
        this.dataview = dataview;
        this.loadValues();
    },

    loadValues: function () {
        var me = this;

        if (me.record != null) {
            var selector = me.down('#keyselector');
            var itemList = me.record.parentNode.raw.properties.dataProperties;

            var availItems = [];
            Ext.each(itemList, function (item) {
                availItems.push({ name: item.columnName });
            });

            //hg - get extension data from dataview
            var items = me.dataview.store.data.items;
            Ext.each(items, function (item) {
                if (item.raw.type == 'extension') {
                    var oChildren = item.childNodes;
                    Ext.each(oChildren, function (child) {
                        availItems.push({ name: child.raw.text });
                    });
                }
            });

            selector.store.loadData(availItems);
            selector.reset();

            var selectedItems = [];
            Ext.each(me.record.childNodes, function (child) {
                selectedItems.push(child.raw.properties.columnName);
            });

            selector.setValue(selectedItems);
        }
    }
});