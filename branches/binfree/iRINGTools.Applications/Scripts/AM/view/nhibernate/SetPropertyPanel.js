Ext.define('AM.view.nhibernate.SetPropertyPanel', {
    extend: 'Ext.form.Panel',
    alias: 'widget.setproperty',
    name: 'dataProperty',
	border: false,
	autoScroll: true,
	monitorValid: true,
	labelWidth: 130,
	bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
	defaults: { anchor: '100%', xtype: 'textfield', allowBlank: false },

	initComponent: function () {
        this.items = [{
			    xtype: 'label',
			    fieldLabel: 'Data Properties',
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
		    }];
		    this.treeNode = node,
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
					    var form = dataPropertyFormPanel.getForm();
					    if (form.treeNode)
						    applyProperty(form);
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
					    var form = dataPropertyFormPanel.getForm();
					    setDataPropertyFields(form, node.attributes.properties);
				    }
			    }]
		    });	    
		var form = dataPropertyFormPanel.getForm();
		setDataPropertyFields(form, node.attributes.properties);
		editPane.add(dataPropertyFormPanel);
		var panelIndex = editPane.items.indexOf(dataPropertyFormPanel);
		editPane.getLayout().setActiveItem(panelIndex);
	}
});

function setDataPropertyFields(form, properties) {
	if (form && properties) {
		form.findField('columnName').setValue(properties.columnName);
		form.findField('propertyName').setValue(properties.propertyName);
		form.findField('dataType').setValue(properties.dataType);
		form.findField('dataLength').setValue(properties.dataLength);

		if (properties.nullable)
			if (properties.nullable.toString().toLowerCase() == 'true') {
				form.findField('isNullable').setValue(true);
			}
			else {
				form.findField('isNullable').setValue(false);
			}
		else
			form.findField('isNullable').setValue(false);

		if (properties.showOnIndex.toString().toLowerCase() == 'true') {
			form.findField('showOnIndex').setValue(true);
		}
		else {
			form.findField('showOnIndex').setValue(false);
		}
		form.findField('numberOfDecimals').setValue(properties.numberOfDecimals);
	}
}

