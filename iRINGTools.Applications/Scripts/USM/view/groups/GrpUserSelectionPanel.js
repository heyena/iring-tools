Ext.define('USM.view.groups.GrpUserSelectionPanel', {
    extend: 'Ext.form.Panel',
    alias: 'widget.grpuserselectionpanel',

    bodyStyle: 'background:#fff;padding:10px',
    width: 600,
    record: null,
    requires: [
           'Ext.ux.form.ItemSelector'
    ],
    initComponent: function () {
        var me = this;
        utilsObj.grpUser = "Group Users";

        var store = Ext.create('Ext.data.Store', {
            fields: ['UserId', 'UserName'],
            data: [{ UserId: '', UserName: 'est'}]
        });

        var gstore = Ext.create('Ext.data.Store', {
            fields: ['GroupId', 'GroupName'],
            data: [{ GroupId: '', GroupName: 'est'}]
        });

        Ext.applyIf(me, {
            items: [{
                xtype: 'panel',
                layout: 'form',
                bodyPadding: 2,
                border: false,
                items: [{
                    xtype: 'combobox',
                    disabled: false,
                    name: 'groupName',
                    allowBlank: false,
                    fieldLabel: 'Group',
                    emptyText: 'Select Group',
                    editable: false,
                    width: 100,
                    displayField: 'GroupName',
                    forceSelection: false,
                    store: 'GroupS',
                    valueField: 'GroupId',
                    listeners: {
                        select: {
                            fn: me.onSelectGroup,
                            scope: me
                        }
                    }
                }]
            },
            {
                xtype: 'panel',
                layout: 'fit',
                bodyPadding: 2,
                border: false,
                items: [{
                    xtype: 'itemselector',
                    itemId: 'userselector',
                    name: 'selectedUsers',
                    anchor: '100%',
                    imagePath: '../ux/images/',
                    store: 'UserS',
                    height: 200,
                    displayField: 'UserFirstName',
                    valueField: 'UserId',
                    msgTarget: 'side',
                    fromTitle: 'Users',
                    toTitle: utilsObj.grpUser
                }]
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

    onSelectGroup: function (combo, records, eOpts) {
        var me = this;
        var grpName = records[0].data.GroupName;
        utilsObj.grpUser = grpName + " Users";
        me.down('itemselector').updateLayout();
        //me.updateLayout();
    },

    loadValues: function () {
        var me = this;

        //        if (me.record != null) {
        //            var selector = me.down('#keyselector');
        //            var itemList = me.record.parentNode.raw.properties.dataProperties;

        //            var availItems = [];
        //            Ext.each(itemList, function (item) {
        //                availItems.push({ name: item.columnName });
        //            });

        //            selector.store.loadData(availItems);
        //            selector.reset();

        //            var selectedItems = [];
        //            Ext.each(me.record.childNodes, function (child) {
        //                selectedItems.push(child.raw.properties.columnName);
        //            });

        //            selector.setValue(selectedItems);
        //        }
    }
});