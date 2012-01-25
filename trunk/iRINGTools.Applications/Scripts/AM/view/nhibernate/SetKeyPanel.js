Ext.define('AM.view.nhibernate.SetKeyPanel', {
    extend: 'Ext.form.Panel',
    alias: 'widget.setkey',
    border: false,
    name: 'keyProperty',
    autoScroll: true,
    monitorValid: true,
    labelWidth: 130,
    bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
    defaults: { anchor: '100%', xtype: 'textfield', allowBlank: false },

    initComponent: function () {
        this.items = [{
            xtype: 'label',
            fieldLabel: 'Key Properties',
            labelSeparator: '',
            itemCls: 'form-title'
        }, {
            name: 'columnName',
            fieldLabel: 'Column Name',
            disabled: true
        }, {
            name: 'propertyName',
            fieldLabel: 'Property Name'
        }, {
            name: 'dataType',
            xtype: 'combo',
            fieldLabel: 'Data Type',
            store: dataTypes,
            mode: 'local',
            editable: false,
            triggerAction: 'all',
            displayField: 'text',
            valueField: 'value',
            selectOnFocus: true,
            disabled: true
        }, {
            xtype: 'numberfield',
            name: 'dataLength',
            fieldLabel: 'Data Length'
        }, {
            xtype: 'checkbox',
            name: 'isNullable',
            fieldLabel: 'Nullable',
            disabled: true
        }, {
            xtype: 'checkbox',
            name: 'showOnIndex',
            fieldLabel: 'Show on Index'
        }, {
            xtype: 'numberfield',
            name: 'numberOfDecimals',
            fieldLabel: 'Number of Decimals'
        }, {
            xtype: 'combo',
            hiddenName: 'keyType',
            fieldLabel: 'Key Type',
            store: [[1, 'assigned'], [0, 'unassigned']],
            mode: 'local',
            editable: false,
            triggerAction: 'all',
            displayField: 'text',
            valueField: 'value',
            value: properties.keyType
        }];
        this.treeNode = node;
        this.tbar = new Ext.Toolbar({
            items: [{
                xtype: 'tbspacer',
                width: 4
            }, {
                xtype: 'tbbutton',
                icon: 'Content/img/16x16/apply.png',
                text: 'Apply',
                tooltip: 'Apply the current changes to the data objects tree',
                handler: function (f) {
                    var form = keyPropertyFormPanel.getForm();
                    applyProperty(form);
                    var treeNodeProps = form.treeNode.attributes.properties;
                    treeNodeProps['keyType'] = form.findField('keyType').getValue();
                }
            }, {
                xtype: 'tbspacer',
                width: 4
            }, {
                xtype: 'tbbutton',
                icon: 'Content/img/16x16/edit-clear.png',
                text: 'Reset',
                tooltip: 'Reset to the latest applied changes',
                handler: function (f) {
                    var form = keyPropertyFormPanel.getForm();
                    setDataPropertyFields(form, properties);
                    form.findField('isNullable').disable();
                }
            }]
        });


        var form = keyPropertyFormPanel.getForm();
        setDataPropertyFields(form, properties);
        editPane.add(keyPropertyFormPanel);
        var panelIndex = editPane.items.indexOf(keyPropertyFormPanel);
        editPane.getLayout().setActiveItem(panelIndex);
    }
});

