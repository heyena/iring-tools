﻿Ext.define('AM.view.sqlconfig.SqlTableSelectionPanel', {
    extend: 'Ext.form.Panel',
    alias: 'widget.sqltableselectionpanel',

    bodyStyle: 'background:#fff;padding:10px',
    title: 'Select Tables',
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
                itemId: 'tableselector',
                name: 'selectedTables',
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

    setRecord: function (record) {
        this.record = record;
        this.loadValues();
    },

    loadValues: function () {
        var me = this;

        if (me.record != null) {
            var selector = me.down('#tableselector');
            var itemList = me.record.raw.properties.tableNames;

            var availItems = [];
            Ext.each(itemList, function (item) {
                availItems.push({ name: item });
            });

            selector.store.loadData(availItems);
            selector.reset();

            var selectedItems = [];
            Ext.each(me.record.childNodes, function (child) {
                selectedItems.push(child.data.text);
            });

            selector.setValue(selectedItems);
        }
    }
});