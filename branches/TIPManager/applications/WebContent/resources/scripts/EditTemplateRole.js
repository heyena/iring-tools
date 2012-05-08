Ext.ns('FederationManager');
/**
 * @class FederationManager.EditTemplateRole
 * @extends Panel
 * @author Rashmi Shukla
 */
FederationManager.EditTemplateRole = Ext.extend(Ext.Panel, {
	title : null,
	data_form : null,
	configData : null,
	formData : null,
	url : null,
	label : null,

	initComponent : function() {

		this.addEvents({
			close : true,
			save : true,
			reset : true,
			validate : true,
			tabChange : true
		});
		/*
		 * this.formData= [{xtype: 'fieldset', title: 'Role/Value', items:[{
		 * xtype: 'radiogroup', items: [{ boxLabel: 'Range', name: 'rangeValue',
		 * checked: true }, { boxLabel: 'Value', name: 'rangeValue' } ]} ]
		 *  }] ;
		 */
		this.formData = creatRadioField(this.formData, 'abc');
		this.data_form = new Ext.FormPanel({
			id : 'data-form',
			url : this.url,
			method : 'POST',
			// bodyStyle:'padding:10px 5px 0',
			autoScroll : true,
			border : false,
			frame : true,
			// width: 1000,
			closable : true,
			defaults : {
				msgTarget : 'under'
			},
			defaultType : 'textfield',
			items : this.formData,
			buttonAlign : 'left',
			autoDestroy : false
		});
		
		this.items = [ this.data_form ];

		this.on('close', this.onCloseTab, this);

		// super
		FederationManager.EditTemplateRole.superclass.initComponent.call(this);
	}
});
ContentData = Ext.extend(Ext.Panel, {
	value : null,
	rangeType : null,

	constructor : function(config) {
		ContentData.superclass.constructor.call(this);
		Ext.apply(this, config);

		this.bodyStyle = 'background:#eee';

		this.rangeRadioGrp = new Ext.form.RadioGroup({
			columns : 1,
			items : [ {
				name : 'rangeVal',
				inputValue : 0,
				style : 'margin-top: 4px'
			}, {
				name : 'rangeVal',
				inputValue : 1,
				style : 'margin-top: 4px'
			} ]
		});
		this.valueRadioGrp = new Ext.form.RadioGroup({
			columns : 1,
			items : [ {
				name : 'valueRadio',
				inputValue : 0,
				style : 'margin-top: 4px'
			}, {
				name : 'valueRadio',
				inputValue : 1,
				style : 'margin-top: 4px'
			} ]
		});
		var that = this;
		var rd_dataType = new Ext.data.JsonReader({}, ['e']);
		var dataTypeStore = new Ext.data.Store({
		    proxy: new Ext.data.HttpProxy({
				url: 'getDataType'
		    }),
		    reader: rd_dataType,
			remoteSort: false
		});
		this.range = new Ext.form.ComboBox({
			fieldLabel: 'Range',
			store: dataTypeStore,
			valueField: 'e',
			displayField: 'e',
			hiddenName: 'active_id',
			mode: 'remote',
			minChars : 0
		});
		
		this.reference = new Ext.form.TextField({
			disabled : true,
			allowBlank : false,
			fieldLabel : 'Reference',
			value : this.value,
			name : 'fieldReference',
			listeners : {
				'change' : function(field, newValue, oldValue) {
					that.value = newValue.toUpperCase();
				}
			}
		});
		this.literal = new Ext.form.TextField({
			disabled : true,
			allowBlank : false,
			fieldLabel : 'Literal',
			value : this.value,
			name : 'fieldLiteral',
			listeners : {
				'change' : function(field, newValue, oldValue) {
					that.value = newValue.toUpperCase();
				}
			}
		});
		this.dataType = new Ext.form.TextField({
			disabled : true,
			allowBlank : false,
			fieldLabel : 'Data Type',
			value : this.value,
			name : 'fieldDataType',
			listeners : {
				'change' : function(field, newValue, oldValue) {
					that.value = newValue.toUpperCase();
				}
			}
		});
		this.restrictions = {name:'restrictions',xtype:'textarea',width : 400, height:50};
		this.restrictionValue = new Ext.form.TextField({
			allowBlank : false,
			fieldLabel : 'Value',
			name : 'restrictionValue'
			});
		this.restrictionType = new Ext.form.ComboBox({
			fieldLabel: 'Restriction Type',
			store: dataTypeStore,
			valueField: 'e',
			displayField: 'e',
			hiddenName: 'active_id',
			mode: 'remote',
			minChars : 0
		});
		
		if (this.rangeType != '') {
			if (this.rangeType.toUpperCase() == 'RANGE') {
				this.range.disabled = false;
				this.reference.disabled = true;
				this.literal.disabled = true;
				this.dataType.disabled = true;
				this.rangeRadioGrp.items[0].checked = true;
				this.valueRadioGrp.disabled = true;
				
				
			} else {
				this.range.disabled = true;
				this.range.value = '';
				this.reference.disabled = false;
				this.literal.disabled = false;
				this.dataType.disabled = false;
				this.valueRadioGrp.disabled = false;
				this.rangeRadioGrp.items[1].checked = true;
			}
		}

		//this.layout = 'column';
		this.border = false;
		this.frame = false;

		this.add([ {
  			xtype: 'fieldset', title:'Role/Value', layout:'column',
  			items:[{
  						width : 40,
						layout : 'form',
						columnWidth : 0.1,
						labelWidth : 0.1,
						items : this.rangeRadioGrp,
						border : false,
						frame : false,
						bodyStyle : 'background:#eee'
					}, {
						columnWidth : 0.9,
						layout : 'form',
						labelWidth : 70,
						defaults : {
							anchor : '100%',
							allowBlank : false
						},
						items : [ this.range, {xtype:'label', fieldLabel:'Value'} ],
						border : false,
						frame : false,
						bodyStyle : 'background:#eee'
					}, {
						width : 40,
						layout : 'form',
						columnWidth : 0.1,
						labelWidth : 0.1,
						items : this.valueRadioGrp,
						border : false,
						frame : false,
						bodyStyle : 'background:#eee',
						style : 'padding-left:10px'
					}, {
						columnWidth : 0.9,
						layout : 'form',
						labelWidth : 60,
						defaults : {
							anchor : '100%',
							allowBlank : false
						},
						items : [ this.reference, this.literal ],
						border : false,
						frame : false,
						bodyStyle : 'background:#eee'
					},
					{
						layout : 'form',
						columnWidth : 1,
						labelWidth : 60,
						defaults : {
							anchor : '100%',
							allowBlank : false
						},
						items : [ this.dataType ],
						style : 'padding-left:50px',
						border : false,
						frame : false,
						bodyStyle : 'background:#eee'
					}]
		},{xtype: 'fieldset', title:'Restrictions',layout:'column',
  			items:[{
					layout : 'form',
					labelWidth : 60,
					defaults : {
						anchor : '100%',
						allowBlank : false
					},
					border : false,
					frame : false,
					bodyStyle : 'background:#eee',			
					columnWidth : 1,
					items : [ this.restrictions]
				},
				{
					layout : 'form',
					labelWidth : 60,
					defaults : {
						anchor : '100%',
						allowBlank : false
					},
					border : false,
					frame : false,
					bodyStyle : 'background:#eee',	
					columnWidth : 1,
					items : [this.restrictionType]
				},
				{
					layout : 'form',
					labelWidth : 60,
					defaults : {
						anchor : '100%',
						allowBlank : false
					},
					border : false,
					frame : false,
					bodyStyle : 'background:#eee',	
					columnWidth : 1,
					items : [ this.restrictionValue]
				}
 			       ]}]);
		this.subscribeEvents();
	},
	subscribeEvents : function() {
		this.rangeRadioGrp.on('change', this.toggleRangeRadio, this);
		this.valueRadioGrp.on('change', this.toggleValueRadio, this);
	},
	toggleRangeRadio : function(e, changed) {
		if (changed) {
			var value = this.rangeRadioGrp.getValue().inputValue;
			if (value == 0) {
				this.range.enable();
				this.range.focus();
				this.reference.disable();
				this.literal.disable();
				this.dataType.disable();
				this.valueRadioGrp.items.items[0].setValue(false);
				this.valueRadioGrp.items.items[1].setValue(false);
				this.valueRadioGrp.disable();
				this.rangeType = 'RANGE';
			} else {
				this.range.clearInvalid();
				this.range.disable();
				this.reference.enable();
				this.reference.focus();
				this.valueRadioGrp.enable();
				this.valueRadioGrp.items.items[0].setValue(true);
				this.valueRadioGrp.items.items[1].setValue(false);
				this.rangeType = 'VALUE';
			}
		}
	},
	toggleValueRadio : function(e, changed) {
		if (changed) {
			var value = this.valueRadioGrp.getValue().inputValue;
			if (value == 0) {
				this.reference.enable();
				this.literal.disable();
				this.dataType.disable();
			} else {
				this.reference.disable();
				this.literal.enable();
				this.dataType.enable();
			}
		}
	}
});

Ext.reg('contentData', ContentData);
function creatRadioField(formData, id) {

	var contentData = new ContentData({
		id : id + 'contentData',
		value : 0,
		rangeType : 'RANGE'
	});
	formData = contentData;
	return formData;
}