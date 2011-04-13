Ext.ns('AdapterManager');

AdapterManager.NHibernateConfigWizard = Ext.extend(Ext.Container, {
	scope: null,
	app: null,

	constructor: function (configs) {
		configs = configs || {};

		var wizard = this;
		var scopeName = configs.scope.Name;
		var appName = configs.app.Name;

		var providersStore = new Ext.data.JsonStore({
			autoLoad: true,
			autoDestroy: true,
			url: 'AdapterManager/DataProviders',
			root: 'items',
			idProperty: 'Provider',
			fields: [{
				name: 'Provider'
			}]
		});


		var myData = new Array();

		var selectproperty = '';
		var mapproperty = '';
		var relationConfigPanel = new Ext.FormPanel({
			labelWidth: 160,
			id: 'relation-config-form',
			bodyStyle: 'padding:15px',
			monitorValid: true,
			defaults: { anchor: '40%' },
			items: [{
				xtype: 'label',
				fieldLabel: 'Configure Data Relationship',
				labelSeparator: '',
				anchor: '100%'
			}, {
				xtype: 'textfield',
				name: 'relatedObject',
				fieldLabel: 'Related Object',
				allowBlank: false
			}, {
				xtype: 'textfield',
				name: 'relationType',
				fieldLabel: 'Relationship Type',
				allowBlank: false
			}, {
				layout: 'column',
				border: false,
				id: 'mapping-column',
				defaults: {
					layout: 'form',
					border: false,
					xtype: 'panel',
					bodyStype: 'padding:0 18px 0 0'
				},
				items: [{
					columnWidth: 0.8,
					defaults: { anchor: '100%' },					
					id: 'left-column',
					items: [{
						xtype: 'combo',
						id: 'map-property',
						triggerAction: 'all',
						mode: 'local',
						fieldLabel: 'Property Maps',
						store: ['CompId', 'CompName', 'Location'],
						displayField: 'CompId',
						valueField: 'CompName',
						listeners: { 'select': function (combo, record, index) {
							selectproperty = record.data.field1;
						}
						}
					}, {
						xtype: 'combo',
						id: 'mapped-property',
						triggerAction: 'all',
						mode: 'local',
						store: ['EmpCompId', 'EmpName'],
						displayField: 'EmpCompId',
						valueField: 'EmpName',
						listeners: { 'select': function (combo, record, index) {
							mapproperty = record.data.field1;
						}
						}
					}]
				}, {
					items: [{		
						columnWidth: 1,				
						defaults: {
							anchor: '120%',
							bodyStype: 'paddingLeft: 100px'
						},
						xtype: 'button',
						text: 'Add',
						anchor: '20 0',
						handler: function (button) {
							var arrayData = new Array();
							arrayData.push(selectproperty);
							arrayData.push(mapproperty);							
							var deleteButtonData = "<input type=\"image\" src=\"Content/img/16x16/edit-delete.png\" " + "onClick='javascript:deleteRow(\"" + arrayData + "\")'>";
							arrayData.push(deleteButtonData);
							myData.push(arrayData);
							var tab = Ext.getCmp('content-panel');
							createRelationGrid(myData);
						}
					}]
				}]
			}]
		});

		var dsConfigPanel = new Ext.FormPanel({
			labelWidth: 160,
			bodyStyle: 'padding:15px',
			monitorValid: true,
			defaults: { anchor: '40%' },
			items: [{
				xtype: 'label',
				fieldLabel: 'Configure Data Source',
				labelSeparator: '',
				anchor: '100% -100'
			}, {
				xtype: 'combo',
				fieldLabel: 'Database Provider',
				hiddenName: 'dbProvider',
				allowBlank: false,
				mode: 'local',
				triggerAction: 'all',
				editable: false,
				store: providersStore,
				displayField: 'Provider',
				valueField: 'Provider'
			}, {
				xtype: 'textfield',
				name: 'dbServer',
				fieldLabel: 'Database Server',
				allowBlank: false
			}, {
				xtype: 'textfield',
				name: 'dbInstance',
				fieldLabel: 'Database Instance',
				allowBlank: false
			}, {
				xtype: 'textfield',
				name: 'dbName',
				fieldLabel: 'Database Name',
				allowBlank: false
			}, {
				xtype: 'textfield',
				name: 'dbSchema',
				fieldLabel: 'Schema Name',
				allowBlank: false
			}, {
				xtype: 'textfield',
				name: 'dbUserName',
				fieldLabel: 'User Name',
				allowBlank: false
			}, {
				xtype: 'textfield',
				inputType: 'password',
				name: 'dbPassword',
				fieldLabel: 'Password',
				allowBlank: false
			}],
			buttons: [{
				text: 'Next',
				formBind: true,
				handler: function (button) {
					var form = wizard.getLayout().activeItem;
					var formIndex = wizard.items.indexOf(form);
					var dsConfigForm = dsConfigPanel.getForm();

					form.getForm().submit({
						url: 'AdapterManager/SchemaObjects',
						timeout: 600000,
						params: {
							scope: scopeName,
							app: appName
						},
						success: function (f, a) {
							var schemaObjects = Ext.util.JSON.decode(a.response.responseText);

							if (schemaObjects.items.length > 0) {
								// populate available tables  
								var tableSelector = tablesConfigPanel.getForm().findField('tableSelector');
								var availItems = new Array();

								for (var i = 0; i < schemaObjects.items.length; i++) {
									var schemaObject = schemaObjects.items[i];
									var selected = false;

									for (var j = 0; j < tableSelector.toData.length; j++) {
										if (schemaObject == tableSelector.toData[j][1]) {
											selected = true;
											break;
										}
									}

									if (!selected) {
										availItems[i] = [schemaObject, schemaObject];
									}
								}

								tableSelector.fromData = availItems;
								wizard.getLayout().setActiveItem(formIndex + 1);
							}
						},
						failure: function (f, a) {
							Ext.Msg.show({
								title: 'Error',
								msg: a.response.responseText,
								modal: true,
								icon: Ext.Msg.ERROR,
								buttons: Ext.Msg.OK
							});
						},
						waitMsg: 'Processing ...'
					});
				}
			}, {
				text: 'Cancel',
				handler: function () {
					wizard.destroy();
				}
			}]
		});

		var tablesConfigPanel = new Ext.FormPanel({
			labelWidth: 200,
			bodyStyle: 'padding:15px',
			monitorValid: true,
			items: [{
				xtype: 'itemselector',
				name: 'tableSelector',
				fieldLabel: 'Configure Tables',
				imagePath: 'scripts/ext-3.3.1/ux/multiselect/',
				fromLegend: 'Available',
				toLegend: 'Selected',
				msWidth: 250,
				msHeight: 300,
				dataFields: ['tableName', 'tableValue'],
				displayField: 'tableName',
				valueField: 'tableValue'
			}],
			buttons: [{
				text: 'Prev',
				handler: function () {
					var form = wizard.getLayout().activeItem;
					var formIndex = wizard.items.indexOf(form);
					wizard.getLayout().setActiveItem(formIndex - 1);
				}
			}, {
				text: 'Next',
				formBind: true,
				handler: function () {
					var form = wizard.getLayout().activeItem;
					var formIndex = wizard.items.indexOf(form);
					wizard.getLayout().setActiveItem(formIndex + 1);
				}
			}, {
				text: 'Cancel',
				handler: function () {
					wizard.destroy();
				}
			}]
		});

		var tablesEditingPanel = new Ext.FormPanel({
			labelWidth: 160,
			bodyStyle: 'padding:15px',
			items: [{
				xtype: 'label',
				fieldLabel: 'Configure Database Dictionary',
				labelSeparator: '',
				anchor: '100% -100'
			}, {
				xtype: 'textfield',
				name: 'table',
				fieldLabel: 'table',
				anchor: '40%'
			}],
			buttons: [{
				text: 'Prev',
				handler: function () {
					var form = wizard.getLayout().activeItem;
					var formIndex = wizard.items.indexOf(form);
					wizard.getLayout().setActiveItem(formIndex - 1);
				}
			}, {
				text: 'Next',
				formBind: true,
				handler: function () {
					var form = wizard.getLayout().activeItem;
					var formIndex = wizard.items.indexOf(form);
					wizard.getLayout().setActiveItem(formIndex + 1);
				}
			}, {
				text: 'Cancel',
				handler: function () {
					wizard.destroy();
				}
			}]
		});

		Ext.apply(this, {
			id: 'nh-config-wizard',
			title: 'NHibernate Configuration Wizard',
			closable: true,
			layout: 'card',
			activeItem: 3,
			items: [dsConfigPanel, tablesConfigPanel, tablesEditingPanel, relationConfigPanel]
		});

		Ext.Ajax.request({
			url: 'AdapterManager/DatabaseDictionary',
			method: 'POST',
			params: {
				scope: scopeName,
				app: appName
			},
			success: function (response, request) {
				var dbDict = Ext.util.JSON.decode(response.responseText);

				if (dbDict) {
					// populate data source form
					var dsConfigForm = dsConfigPanel.getForm();
					var connStr = dbDict.ConnectionString;
					var connStrParts = '';
					if (connStr != null)
						connStrParts = connStr.split(';');


					for (var i = 0; i < connStrParts.length; i++) {
						var pair = connStrParts[i].split('=');

						switch (pair[0].toUpperCase()) {
							case 'DATA SOURCE':
								var dsValue = pair[1].split('\\');
								var dbServer = (dsValue[0] == '.' ? 'localhost' : dsValue[0]);
								dsConfigForm.findField('dbServer').setValue(dbServer);
								dsConfigForm.findField('dbInstance').setValue(dsValue[1]);
								break;
							case 'INITIAL CATALOG':
								dsConfigForm.findField('dbName').setValue(pair[1]);
								break;
							case 'USER ID':
								dsConfigForm.findField('dbUserName').setValue(pair[1]);
								break;
							case 'PASSWORD':
								dsConfigForm.findField('dbPassword').setValue(pair[1]);
								break;
						}
					}

					dsConfigForm.findField('dbProvider').setValue(dbDict.Provider);
					dsConfigForm.findField('dbSchema').setValue(dbDict.SchemaName);

					// populate selected tables
					var tableSelector = tablesConfigPanel.getForm().findField('tableSelector');
					var selectedItems = new Array();

					for (var i = 0; i < dbDict.dataObjects.length; i++) {
						var dataObject = dbDict.dataObjects[i];
						selectedItems[i] = [dataObject.tableName, dataObject.tableName];
					}

					tableSelector.toData = selectedItems;
				}
			},
			failure: function (response, request) {
				Ext.Msg.alert('error');
			}
		});

		AdapterManager.NHibernateConfigWizard.superclass.constructor.apply(this, arguments);
	}
});

function createStore(myData) {
	var datastore = new Ext.data.Store({
		proxy: new Ext.data.MemoryProxy(myData),
		reader: new Ext.data.ArrayReader({}, [
			{ name: 'property' },
			{ name: 'relatedProperty' },
			{ name: 'deleteButton' }
		])
	});

	return datastore;
}

function createRelationGrid(mydata) {
	var tab = Ext.getCmp('content-panel');

	var gridtab = tab.items.map['dataRelationGridPane'];
	if (gridtab != null)
		gridtab.destroy();

	var colModel = new Ext.grid.ColumnModel([
		{ id: "property", header: "Property", dataIndex: 'property' },
		{ header: "Related Property", dataIndex: 'relatedProperty' },
		{ dataIndex: 'deleteButton' }
	]);

	var ds = createStore(mydata);

	ds.on('load', function () {
		var dataRelationGridPane = new Ext.grid.GridPanel({
			id: 'dataRelationGridPane',
			layout: 'fit',
			store: ds,
			title: 'Data Relationship',
			stripeRows: true,
			loadMask: true,
			cm: colModel,
			selModel: new Ext.grid.RowSelectionModel({ singleSelect: true }),
			enableColLock: true,
			viewConfig: {
				forceFit: true
			}
		});

		tab.add(dataRelationGridPane);
		tab.doLayout();
	});

	ds.load();
}

function deleteRow(record) {
	var tab = Ext.getCmp('content-panel');
	var gridtab = tab.items.map['dataRelationGridPane'];
	var mydata = gridtab.store.data.items;
	var totaldata = new Array();
	
	var comparestring;
	for (var i = 0; i < mydata.length; i++) {
		var itemdata = new Array();
		comparestring = mydata[i].data.property + ',' + mydata[i].data.relatedProperty;
		if (record != comparestring) {
			itemdata.push(mydata[i].data.property);
			itemdata.push(mydata[i].data.relatedProperty);
			itemdata.push(mydata[i].data.deleteButton);
			totaldata.push(itemdata);
		}
	}
	createRelationGrid(totaldata);
}