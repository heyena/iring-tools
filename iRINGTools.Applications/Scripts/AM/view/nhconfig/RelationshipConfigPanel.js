Ext.define('AM.view.nhconfig.RelationshipConfigPanel', {
    extend: 'Ext.form.Panel',
    alias: 'widget.relationshipconfigpanel',

    bodyStyle: 'background:#fff;padding:10px',
    title: 'Configure Relationship',
    autoScroll: true,

    objectNode: null,
    record: null,

    initComponent: function () {
        var me = this;

        Ext.applyIf(me, {
            items: [
            {
                xtype: 'fieldset',
                title: '<b>General</b>',
                padding: 10,
                defaults: {
                    xtype: 'combobox',
                    queryMode: 'local',
                    displayField: 'name',
                    valueField: 'name',
                    anchor: '100%',
                    labelWidth: 120,
                    allowBlank: false
                },
                items: [
                {
                    xtype: 'textfield',
                    fieldLabel: 'Relationship Name',
                    name: 'name',
                    readOnly: true,
                    allowBlank: true
                },
                {
                    fieldLabel: 'Relationship Type',
                    name: 'type',
                    store: Ext.create('Ext.data.Store', {
                        fields: ['name'],
                        data: [
                            { name: 'OneToOne' },
                            { name: 'OneToMany' }
                        ]
                    })
                },
                {
                    xtype: 'textfield',
                    fieldLabel: 'Source Object',
                    name: 'sourceObject',
                    readOnly: true
                },
                {
                    fieldLabel: 'Related Object',
                    name: 'relatedObject',
                    store: Ext.create('Ext.data.Store', {
                        fields: ['name'],
                        data: [{ name: ''}]
                    }),
                    listeners: {
                        change: me.onRelatedObjectChange,
                        scope: me
                    }
                }]
            },
            {
                xtype: 'fieldset',
                title: '<b>Property Relations</b>',
                padding: 10,
                items: [
                {
                    xtype: 'fieldcontainer',
                    layout: {
                        type: 'hbox'
                    },
                    defaults: {
                        xtype: 'combobox',
                        queryMode: 'local',
                        displayField: 'name',
                        valueField: 'name'
                    },
                    items: [
                    {
                        name: 'sourceProperties',
                        store: Ext.create('Ext.data.Store', {
                            fields: ['name'],
                            data: [{ name: ''}]
                        }),
                        emptyText: 'Select Source Property',
                        flex: 1
                    },
                    {
                        xtype: 'tbspacer',
                        width: 4
                    },
                    {
                        name: 'relatedProperties',
                        emptyText: 'Select Related Property',
                        store: Ext.create('Ext.data.Store', {
                            fields: ['name'],
                            data: [{ name: ''}]
                        }),
                        flex: 1
                    },
                    {
                        xtype: 'tbspacer',
                        width: 4
                    },
                    {
                        xtype: 'button',
                        width: 22,
                        iconCls: 'am-list-add',
                        scope: me,
                        handler: me.addPropertyMap
                    }]
                },
                {
                    xtype: 'fieldcontainer',
                    items: [{
                        xtype: 'grid',
                        forceFit: true,
                        store: {
                            fields: ['dataPropertyName', 'relatedPropertyName']
                        },
                        columns: [{
                            text: 'Source Property',
                            dataIndex: 'dataPropertyName'
                        }, {
                            text: 'Related Property',
                            dataIndex: 'relatedPropertyName'
                        }, {
                            xtype: 'actioncolumn',
                            text: 'Action',
                            align: 'center',
                            width: 30,
                            items: [{
                                iconCls: 'am-list-remove',
                                handler: me.removePropertyMap
                            }]
                        }]
                    }]
                }]
            }],
            dockedItems: [{
                xtype: 'toolbar',
                dock: 'top',
                layout: {
                    padding: 2,
                    type: 'hbox'
                },
                items: [
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

    setRecord: function (record, objectNode) {
        this.record = record;
        this.objectNode = objectNode;
        this.loadValues();
    },

    loadValues: function () {
        var me = this;
        var form = me.getForm();

        if (me.record != null) {
            if (me.objectNode != null) {
                //
                // init related objects combobox
                //
                var objectsNode = me.objectNode.parentNode;
                var objects = [];

                Ext.each(objectsNode.childNodes, function (node) {
                    objects.push({ name: node.data.text });
                });

                form.findField('relatedObject').getStore().loadData(objects);

                //
                // init source properties combobox
                //
                var keyNodes = me.objectNode.findChild('text', 'Keys').childNodes;
                var propNodes = me.objectNode.findChild('text', 'Properties').childNodes;
                var sourceProps = [];

                Ext.each(keyNodes.concat(propNodes), function (childNode) {
                    sourceProps.push({ name: childNode.raw.properties.propertyName });
                });

                form.findField('sourceProperties').getStore().loadData(sourceProps);
            }

            //
            // populate property links
            //
            var data = [];

            Ext.each(me.record.propertyMaps, function (map) {
                if (typeof map.dataPropertyName !== 'undefined' &&
                        typeof map.relatedPropertyName !== 'undefined') {
                    data.push({
                        dataPropertyName: map.dataPropertyName,
                        relatedPropertyName: map.relatedPropertyName
                    });
                }
            });

            var store = me.down('grid').getStore();
            store.clearData();
            store.loadData(data);

            //
            // set form values
            //
            form.setValues(me.record);
        }
    },

    onRelatedObjectChange: function (cbx, newValue, oldValue, eOpts) {
        var me = this;

        if (me.record != null && me.objectNode != null) {
            Ext.each(me.objectNode.parentNode.childNodes, function (node) {
                if (node.raw.properties['objectName'] == newValue) {
                    var keyNodes = node.findChild('text', 'Keys').childNodes;
                    var propNodes = node.findChild('text', 'Properties').childNodes;
                    var relatedProps = [];

                    Ext.each(keyNodes.concat(propNodes), function (childNode) {
                        relatedProps.push({ name: childNode.raw.properties.propertyName });
                    });

                    me.getForm().findField('relatedProperties').getStore().loadData(relatedProps);
                    return;
                }
            });
        }
    },

    addPropertyMap: function (button, e) {
        var me = this;
        var form = me.getForm();

        var dataPropertyName = form.findField('sourceProperties').getValue();
        var relatedPropertyName = form.findField('relatedProperties').getValue();

        if (dataPropertyName != null && relatedPropertyName != null) {
            var newRecord = {
                dataPropertyName: dataPropertyName,
                relatedPropertyName: relatedPropertyName
            };
            var store = me.down('grid').getStore();
            store.add(newRecord);
        }
    },

    removePropertyMap: function (dataview, rowIndex, colIndex, action, e, record, eOpts) {
        dataview.getStore().remove(record);
    }
});