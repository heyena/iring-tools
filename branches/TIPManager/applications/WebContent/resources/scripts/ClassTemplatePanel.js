Ext.ns('FederationManager');
/**
 * @class FederationManager.ClassTemplatePanel
 * @extends Panel
 * @author Rashmi Shukla
 */
FederationManager.ClassTemplatePanel = Ext
		.extend(
				Ext.Panel,
				{
					// title: 'ClassTemplatePanel',
					title : null,
					data_form : null,
					configData : null,
					formData : null,
					url : null,
					nId : null,
					label : null,
					parentNode : null,
					node : null,
					repositoryCombo : null,
					name : null,
					desc : null,
					parentTemplate : null,
					statusAuth : null,
					statusClass : null,
					statusFrom : null,
					statusTo : null,
					entityType : null,
					specStore : [],
					repoStore : [],
					classStore : [],
					roleStore : [],

					/**
					 * initComponent
					 * 
					 * @protected
					 */

					initComponent : function() {

						this.addEvents({
							close : true,
							save : true,
							reset : true,
							validate : true,
							tabChange : true
						});
						if (this.parentNode == null) {
							this.label = 'New';
							if (this.configData == 'class') {
								this.url = 'postClass';
							} else {
								this.url = 'postTemplate';
							}

						} else {
							this.label = 'Edit';
							if (this.configData == 'class') {
								this.node = this.parentNode.childNodes[0];
								this.name = this.node.attributes.record.Name;
								this.desc = this.node.attributes.record.Description;
								this.statusAuth = this.node.attributes.record["Status Authority"];
								this.statusClass = this.node.attributes.record["Status Class"];
								this.statusFrom = this.node.attributes.record["Status From"];
								this.statusTo = this.node.attributes.record["Status To"];
								this.entityType = this.node.attributes.record["Entity Type"];
								this.specStore = this
										.createStore(
												this.parentNode.childNodes[3].attributes.children,
												0);
								this.classStore = this
										.createStore(
												this.parentNode.childNodes[1].attributes.children,
												0);
								this.url = 'postClass';
							} else {
								this.name = this.parentNode.attributes.record.label;
								this.node = this.parentNode.childNodes[0];
								this.parentNode.childNodes.splice(0, 1);
								this.roleStore = this.createStore(
										this.parentNode.childNodes, 1);
								this.parentNode.childNodes.splice(0, 0,
										this.node);
								this.parentTemplate = this.node.attributes.record["Parent Template"];
								this.statusAuth = this.node.attributes.record["Authority"];
								this.statusClass = this.node.attributes.record["Class"];
								this.url = 'postTemplate';

							}
						}

						this.repoStore = this
								.createRepoStore(Ext.getCmp('federation-tree')
										.getRootNode().childNodes[2].childNodes);

						repositoryCombo = new Ext.form.ComboBox(
								{
									fieldLabel : 'Target Repo',
									boxMaxWidth : 250,
									width : 250,
									forceSelection : true,
									typeAhead : true,
									triggerAction : 'all',
									lazyRender : true,
									store : this.repoStore,
									displayField : 'name',
									valueField : '',
									hiddenName : 'targetRepo',
									listeners : {
										select : {
											fn : function(combo, value) {
												if (combo.lastSelectionText
														.indexOf("[Read Only]") != -1) {
													Ext.Msg
															.alert('Warning',
																	'This Repository is Read-Only and cannot be selected!');
													combo.setValue("");
												}
											}
										}
									}
								});

						if (this.configData == 'class') {
							var that = this;
							this.tbar = this.buildToolbar();

							this.formData = [
									{
										fieldLabel : 'Name',
										name : 'name',
										xtype : 'textfield',
										enableKeyEvents : true,
										listeners : {
											change : function(f, newval, old) {
												Ext.getCmp(that.id).setTitle(
														that.label + ': {'
																+ newval + '}');
											},
											keyup : function(f, evt) {
												Ext.getCmp(that.id).setTitle(
														that.label + ': {'
																+ f.getValue()
																+ '}');
											}
										},
										width : 250,
										value : this.name
									},
									{
										name : 'description',
										fieldLabel : 'Description',
										xtype : 'textarea',
										width : 250,
										height : 205,
										value : this.desc
									},
									{
										fieldLabel : 'Authority',
										name : 'authority',
										xtype : 'textfield',
										disabled : true,
										width : 250,
										value : this.statusAuth
									},
									{
										fieldLabel : 'Recorded',
										name : 'recorded',
										xtype : 'textfield',
										disabled : true,
										width : 250,
										value : this.statusClass
									},
									{
										fieldLabel : 'Date From',
										name : 'dateFrom',
										xtype : 'textfield',
										disabled : true,
										width : 250,
										value : this.statusFrom
									},
									{
										fieldLabel : 'Date To',
										name : 'dateTo',
										xtype : 'textfield',
										disabled : true,
										width : 250,
										value : this.statusTo
									},
									{
										fieldLabel : 'Entity Type',
										name : 'entityType',
										xtype : 'textfield',
										width : 250,
										value : this.entityType
									},
									{
										fieldLabel : 'Specialization',
										name : 'specialization',
										width : 250,
										xtype : 'multiselect',
										store : new Ext.data.SimpleStore({
											fields : [ 'rid', 'value' ],
											data : this.specStore
										}),
										displayField : 'value',
										valueField : 'rid',
										id : 'spec' + that.id
									}, 
									{
										xtype : 'panel',
										layout : 'column',
										border : true,
										width : 240,
										style : {
											marginBottom : '10px',
											marginLeft : '110px',
											marginRight : '5px',
											marginTop : '2px'
										},
										items : [
												{
													xtype : "button",
													columnWidth : .5,
													text : 'Add',
													// icon:'resources/images/16x16/Add-icon.png',
													tooltip : 'Add Specialization',
													handler : function() {
														this
																.onStoreDtlsAdd(
																		this.specStore,
																		'spec'
																				+ that.id,
																		'ClassNode',
																		0);
													},

													scope : this
												},
												{
													xtype : "button",
													columnWidth : .5,
													text : 'Remove',
													// icon:'resources/images/16x16/Remove-icon.png',
													tooltip : 'Remove Specialization',
													handler : function() {
														this
																.onStoreDtlsRemove(
																		this.specStore,
																		'spec'
																				+ that.id);
													},
													scope : this
												} ]
									},									
									{
										name : 'classification',
										fieldLabel : 'Classification',
										xtype : 'multiselect',
										width : 250,
										store : new Ext.data.SimpleStore({
											fields : [ 'rid', 'value' ],
											data : this.specStore
										}),
										displayField : 'value',
										valueField : 'rid',
										id : 'class' + that.id
									},
									{
										xtype : 'panel',
										layout : 'column',
										border : true,
										width : 240,
										style : {
											marginBottom : '10px',
											marginLeft : '110px',
											marginRight : '5px',
											marginTop : '2px'
										},
										items : [
												{
													xtype : "button",
													columnWidth : .5,
													text : 'Add',
													// icon :
													// 'resources/images/16x16/Add-icon.png',
													tooltip : 'Add Classification',
													handler : function() {
														this
																.onStoreDtlsAdd(
																		this.classStore,
																		'class'
																				+ that.id,
																		'ClassNode',
																		0);
													},
													scope : this
												},
												{
													xtype : "button",
													columnWidth : .5,
													text : 'Remove',
													// icon :
													// 'resources/images/16x16/Remove-icon.png',
													tooltip : 'Remove Classification',
													handler : function() {
														this
																.onStoreDtlsRemove(
																		this.classStore,
																		'class'
																				+ that.id);
													},
													scope : this
												} ]
									}, repositoryCombo ];
							this.formData.push({
								xtype : 'hidden',
								name : 'classId',
								value : that.id
							});

						} else {
							var that = this;
							this.formData = [
									{
										xtype : 'radiogroup',
										fieldLabel : 'Template Type',
										width : 300,
										items : [ {
											boxLabel : 'Base Template',
											name : 'tempType',
											checked : true
										}, {
											boxLabel : 'Specialized Template',
											name : 'tempType'
										} ]
									},
									{
										fieldLabel : 'Name',
										name : 'name',
										xtype : 'textfield',
										width : 300,
										value : this.name,
										enableKeyEvents : true,
										listeners : {
											change : function(f, newval, old) {
												Ext.getCmp(that.id).setTitle(
														that.label + ': {'
																+ newval + '}');
											},
											keyup : function(f, evt) {
												Ext.getCmp(that.id).setTitle(
														that.label + ': {'
																+ f.getValue()
																+ '}');
											}
										}
									},
									{
										fieldLabel : 'Parent Template',
										name : 'parentTemplate',
										xtype : 'textfield',
										width : 300,
										value : this.parentTemplate
									},
									{
										fieldLabel : 'Description',
										name : 'description',
										xtype : 'textarea',
										width : 300
									},

									{
										fieldLabel : 'Authority',
										name : 'authority',
										xtype : 'textfield',
										width : 300,
										disabled : true,
										value : this.statusAuth
									},
									{
										fieldLabel : 'Recorded',
										name : 'recorded',
										xtype : 'textfield',
										width : 300,
										disabled : true,
										value : this.statusClass
									},
									{
										fieldLabel : 'Date From',
										name : 'dateFrom',
										xtype : 'datefield',
										disabled : true,
										width : 300
									},
									{
										fieldLabel : 'Date To',
										name : 'dateTo',
										xtype : 'datefield',
										disabled : true,
										width : 300
									},
									{
										fieldLabel : 'Role Definition',

										name : 'roleDefinition',
										xtype : 'multiselect',
										width : 300,
										store : this.roleStore,
										id : 'role'
												+ that.id,
										listeners : {
											click : function(
													object,event) {
												var rec = object.store.query(
																this.valueField,
																this.getValue()).itemAt(0);
												Ext.getCmp('roleId'+ that.id).setValue(rec.json[2]);
												Ext.getCmp('roleName'+ that.id).setValue(rec.json[1]);
												Ext.getCmp('EditRole'+ that.id).enable();
											}
										}

									},
									{
										xtype : 'panel',
										layout : 'column',
										border : true,
										width : 300,
										style : {
											marginBottom : '5px',
											marginLeft : '105px',
											marginRight : '5px',
											marginTop : '5px'
										},
										items : [
												{
													columnWidth : .25,
													xtype : "button",
													text : 'Edit',
													disabled : true,
													id : 'EditRole'
															+ that.id,
													tooltip : 'Edit',
													handler : function() {
														that.onTempRoleEdit(that.id);
													}
												},
												{
													columnWidth : .25,
													xtype : "button",
													text : 'Add',
													handler : function() {
														that.onStoreDtlsAdd(
															that.roleStore,'role'+ that.id,'RoleNode',1);
														},
													tooltip : 'Add'
													
												},
												{
													columnWidth : .25,
													xtype : "button",
													text : 'Remove',
													handler : function() {
														that.onStoreDtlsRemove(that.roleStore,'role'+ that.id);
														},
													tooltip : 'Remove'
												},
												{
													columnWidth : .25,
													xtype : "tbbutton",
													text : 'Apply',
													tooltip : 'Apply',
													handler : this.onSave,
													scope : this
												} 
												
												
												]
									}, {
										fieldLabel : 'Id',
										name : 'roleId'+ that.id,
										id : 'roleId'+ that.id,
										readOnly : true,
										xtype : 'textfield',
										width : 300
									},
									{
										fieldLabel : 'Name',
										name : 'roleName'+ that.id,
										id : 'roleName'+ that.id,
										readOnly : true,
										xtype : 'textfield',
										width : 300
									},
									{
										fieldLabel : 'Description',
										name : 'roleDescription'+ that.id,
										id : 'roleDescription'+ that.id,
										readOnly : true,
										xtype : 'textfield',
										width : 300
									}, repositoryCombo ];

							this.formData.push({
								xtype : 'hidden',
								name : 'tempId',
								value : that.id
							});
						}
						this.data_form = new Ext.FormPanel({
							labelWidth : 100, // label settings here cascade
							// unless
							id : 'data-form',
							url : this.url,
							method : 'POST',
							bodyStyle : 'padding:10px 5px 0',
							autoScroll : true,
							border : false, // removing the border of the form
							frame : true,
							// layout:'fit',
							width : 1000,
							closable : true,
							defaults : {
								msgTarget : 'under'
							},
							defaultType : 'textfield',

							items : this.formData, // binding with the fields
							// list
							buttonAlign : 'left', // buttons aligned to the
							// left
							autoDestroy : false

						});

						this.items = [ this.data_form ];

						this.on('close', this.onCloseTab, this);

						// super
						FederationManager.ClassTemplatePanel.superclass.initComponent
								.call(this);
					},
					buildToolbar : function() {
						return [ {
							xtype : "tbbutton",
							text : 'Apply',
							icon : 'resources/images/16x16/apply-icon.png',
							tooltip : 'Apply',
							disabled : false,
							handler : this.onSave,
							scope : this
						}, {
							xtype : "tbbutton",
							text : 'Reset',
							icon : 'resources/images/16x16/edit-clear.png',
							tooltip : 'Reset',
							disabled : false,
							handler : this.onReset,
							scope : this
						}, {
							xtype : "tbbutton",
							text : 'Save',
							icon : 'resources/images/16x16/document-save.png',
							tooltip : 'Save',
							disabled : false,
							handler : this.onSave,
							scope : this
						} ];
					},

					createStore : function(obj, roleStore) {
						var storeData = new Array();
						for ( var i = 0; i < obj.length; i++) {
							var nodeId, data;
							if (roleStore == 1) {
								nodeId = obj[i].attributes.identifier;
								var identifierVal = obj[i].attributes.record.Identifier;
								data = [ nodeId, obj[i].text, identifierVal ];
							} else {
								nodeId = obj[i].identifier;
								data = [ nodeId, obj[i].text ];
							}
							storeData.push(data);
						}
						return storeData;

					},
					createRepoStore : function(obj) {
						var storeData = new Array();
						storeData.push([ "", "Select a Repository" ]);
						for ( var i = 0; i < obj.length; i++) {
							var properties = obj[i].attributes.properties;
							var repoId = properties.Id;
							var repoText = properties.Name;
							var readOnly = 0;
							for ( var j in properties) {
								if (j == 'Read Only' && properties[j] == 'true') {
									readOnly = 1;
									break;
								}
							}
							if (readOnly == 1) {
								repoText = repoText + ' [Read Only]';
							}
							var data = [ properties.Name, repoText ];
							storeData.push(data);
						}
						return storeData;

					},
					onCloseTab : function(node) {
						// check number of tabs in panel to make disabled the
						// centerPanel if its the last tab has been closed.
						if ((Ext.getCmp('contentPanel').items.length) == 1) {
							Ext.getCmp('contentPanel').disable();
						}

					},
					onTempRoleEdit : function(id) {
						this.fireEvent('EditTempRole', id);

					},
					onStoreDtlsAdd : function(store, id, type, roleStore) {
						var selectedNode = Ext.getCmp('search-panel')
								.getSelectedNode();
						var identifierVal;
						if (selectedNode.attributes.type == type) {
							if(roleStore == 1){
								identifierVal = selectedNode.attributes.record.Identifier;
							}
							var nId = selectedNode.attributes.identifier;
							var isPresent = 0;
							for ( var i = 0; i < store.length; i++) {
								if (store[i][0] == nId) {
									isPresent = 1;
									break;
								}
							}
							if (isPresent == 0) {
								var text = selectedNode.text
										.substring(
												0,
												(selectedNode.text
														.lastIndexOf("(") != -1) ? selectedNode.text
														.lastIndexOf("(") - 1
														: selectedNode.text.length);
								var data = (roleStore == 1)?[ nId, text,identifierVal ]:[ nId, text ];
								store.push(data);
							} else if (isPresent == 1) {
								Ext.Msg.alert('Warning',
										'Selected element already exists!');
							}

							var multiselect = Ext.getCmp(id);
							if (multiselect.store.data) {
								multiselect.reset();
								multiselect.store.removeAll();
							}
							multiselect.store.loadData(store);
							multiselect.store.commitChanges();
						} else {
							Ext.Msg.alert('Warning', 'Cannot add '
									+ selectedNode.attributes.type.substring(0,
											(selectedNode.attributes.type
													.lastIndexOf("Node")))
									+ '. Please select a '
									+ type.substring(0, (type
											.lastIndexOf("Node"))) + '!');
						}
					},

					onStoreDtlsRemove : function(store, id) {
						var localStore = store;
						if (Ext.getCmp(id).getValue() != "") {
							for ( var i = 0; i < localStore.length; i++) {
								if (Ext.getCmp(id).getValue().indexOf(
										localStore[i][0]) != -1) {
									store.remove(localStore[i]);
								}
							}
							Ext.getCmp(id).store.loadData(store);
						} else {
							Ext.Msg.alert('Warning',
									"Please select a value to be removed");
							return false;
						}
						if(id.indexOf("role") != -1) {
							var roleId =id.substring(id.indexOf('role')+4,id.length);
							Ext.getCmp('roleId'+ roleId).setValue("");
							Ext.getCmp('roleName'+ roleId).setValue("");
							Ext.getCmp('EditRole'+ roleId).disable();
						}
						
					},

					onReset : function() {
						this.data_form.getForm().reset();
					},

					onTextChange : function(value) {
						this.title = value;
					},
					onSave : function() {
						if (this.configData == 'class') {
							this.data_form.getForm()
									.findField('specialization').selectAll();
							this.data_form.getForm()
									.findField('classification').selectAll();
						} else {
							this.data_form.getForm()
									.findField('roleDefinition').selectAll();
						}
						if (this.data_form.getForm().findField('targetRepo')
								.getValue() == "") {
							Ext.Msg.alert('Warning',
									"Please select a repository");
							return false;
						}
						this.data_form.getForm().submit(
								{
									waitMsg : 'Saving Data...',
									success : function(f, a) {
										Ext.Msg.alert('Success',
												'Changes saved successfully!');
									},
									failure : function(f, a) {
										Ext.Msg.alert('Warning',
												'Error saving changes!');
									}
								});

					}
				});
Ext.override(Ext.ux.form.MultiSelect, {
	selectAll : function() {
		var ids = this.store.collect(this.valueField);
		this.setValue(ids);
	}
});