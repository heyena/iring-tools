Ext.ns('ExchangeManager');
/**
 * @class ExchangeManager.NavigationPanel
 * @extends Panel
 * @author by Gert Jansen van Rensburg
 */
ExchangeManager.NavigationPanel = Ext
		.extend(
				Ext.Panel,
				{
					title : 'NavigationPanel',
					layout : 'card',
					activeItem : 0,
					configData : null,
					dataGrid : null,
					url : null,

									
					
					/**
					 * initComponent
					 * 
					 * @protected
					 */
					initComponent : function() {
						this.addEvents({
							next : true,
							prev : true
						});

						// var rowData = eval(this.configData.rowData);
						var fieldList = eval(this.configData.headerLists);
						var headerList = eval(this.configData.columnDatas);
						var classObjName = this.configData.classObjName;
						var filterSet = eval(this.configData.filterSets);
						var pageSize = parseInt(this.configData.pageSize);

						alert("fieldList :" + fieldList);
						// var sortBy = this.configData.sortBy;
						// var sortOrder = this.configData.sortOrder;

						/*
						 * var filters = new Ext.ux.grid.GridFilters({ // encode
						 * and local configuration options defined previously
						 * for easier reuse encode: true, // json encode the
						 * filter query remotesort: true, // json encode the
						 * filter query local: false, // defaults to false
						 * (remote filtering) filters: filterSet });
						 */

						/*
						 * var relatedClassArr = new Array();
						 * 
						 * if (this.configData.relatedClasses != undefined) {
						 * for(var i=0; i <
						 * this.configData.relatedClasses.length; i++) { var key =
						 * this.configData.relatedClasses[i].identifier; var
						 * text = this.configData.relatedClasses[i].text;
						 * relatedClassArr[i] = text; } }
						 */

						// build the header first
						// send the request to generate the arraystore
						var proxy = new Ext.data.HttpProxy({
							api : {
								read : new Ext.data.Connection({
									url : this.url,
									method : 'POST',
									timeout : 120000000000000
								}),
								create : null,
								update : null,
								destroy : null
							}
						});

						var reader = new Ext.data.JsonReader({
							totalProperty : 'total',
							successProperty : 'success',
							root : 'data',
							fields : fieldList
						});

						var store = new Ext.data.Store({
							autoLoad : true,
							proxy : proxy,
							remoteSort : true,
							reader : reader,
							/*
							 * sortInfo : { field : sortBy, direction :
							 * sortOrder },
							 */
							autoLoad : {
								params : {
									start : 0,
									limit : pageSize
								// identifier:identifier,
								// refClassIdentifier:refClassIdentifier

								}
							},
							baseParams : {
							/*
							 * 'identifier':identifier,
							 * 'refClassIdentifier':refClassIdentifier
							 */
							}
						});

						this.dataGrid = new Ext.grid.GridPanel({
							store : store,
							columns : headerList,
							stripeRows : true,
							loadMask : true,
							//plugins: [filters],
							layout : 'fit',
							frame : true,
							autoSizeColumns : true,
							autoSizeGrid : true,
							AllowScroll : true,
							minColumnWidth : 100,
							columnLines : true,
							classObjName : classObjName,
							enableColumnMove : false,
							bbar : new Ext.PagingToolbar({
								pageSize : pageSize,
								store : store,
								displayInfo : true,
								//plugins: [filters],
								autoScroll : true
							})
						});

						
						
						
						
						
						
						
						
						
						
						
						
						this.dataGrid.on('beforerender',
								function() {
									var colmodel = this.getColumnModel();
									for ( var i = 0; i < colmodel
											.getColumnCount(); i++) {
										colmodel.setRenderer(i, function(val) {
											switch (val.toLowerCase()) {
											case "add":
												spanColor = 'red';
												break;
											case "change":
												spanColor = 'blue';
												break;
											case "delete":
												spanColor = 'green';
												break;
											case "sync":
												spanColor = 'black';
												break;
											default:
												spanColor = 'black';
											}
											return '<span style="color:'
													+ spanColor + ';">' + val
													+ '</span>';
										});
									}
								});

						this.dataGrid
								.on(
										'cellclick',
										function(grid, rowIndex, columnIndex, e) {
											var cm = this.colModel;
											var record = grid.getStore().getAt(
													rowIndex); // Get the
											// Record
											var fieldName = grid
													.getColumnModel()
													.getDataIndex(columnIndex); // Get
											// field
											// name
											if (fieldName == 'Identifier'
													&& record.get(fieldName) != '') {
												var IdentificationByTag_value = record
														.get(fieldName);
												var transferType_value = record
														.get('TransferType');
												var rowDataArr = [];
												for ( var i = 3; i < cm
														.getColumnCount(); i++) {
													fieldHeader = grid
															.getColumnModel()
															.getColumnHeader(i); // Get
													// field
													// name
													fieldValue = record
															.get(grid
																	.getColumnModel()
																	.getDataIndex(
																			i));
													tempArr = Array(
															fieldHeader,
															fieldValue);
													rowDataArr.push(tempArr)
												}
												var filedsVal_ = '[{"name":"Property"},{"name":"Value"}]';
												var columnsData_ = '[{"id":"Property","header":"Property","width":144,"sortable":"true","dataIndex":"Property"},{"id":"Value","header":"Value","width":144,"sortable":"true","dataIndex":"Value"}]';
												var prowData = eval(rowDataArr);
												var pfiledsVal = eval(filedsVal_);
												var pColumnData = eval(columnsData_);
												// create the data store
												var pStore = new Ext.data.ArrayStore(
														{
															fields : pfiledsVal
														});
												pStore.loadData(prowData);

												// create the Grid
												var grid_class_properties = new Ext.grid.GridPanel(
														{
															store : pStore,
															columns : pColumnData,
															stripeRows : true,
															loadMask : true,
															height : 460,
															autoSizeColumns : true,
															autoSizeGrid : true,
															AllowScroll : true,
															minColumnWidth : 100,
															columnLines : true,
															enableColumnMove : false
														});

												// get the centerPanel x,y
												// coordinates, used to set the
												// position of Indvidual
												// Class(PopUp window)
												// var strPositon =
												// (Ext.getCmp('centerPanel').getPosition()).toString();
												// var arrPositon =
												// strPositon.split(",");

												var myWin = new Ext.Window(
														{
															title : 'Indvidual Class[ '
																	+ IdentificationByTag_value
																	+ ' ]',
															id : 'indvidual-class',
															closable : true,
															x : 10,
															y : parseInt(10) + 25,

															// width:Ext.getCmp('centerPanel').getInnerWidth()-2,
															// height:Ext.getCmp('centerPanel').getInnerHeight(),
															layout : 'border',
															listeners : {
																beforerender : {
																	fn : function() {
																		Ext
																				.getBody()
																				.mask();
																	}
																},
																close : {
																	fn : function() {
																		Ext
																				.getBody()
																				.unmask();
																	}
																}
															},
															items : [
																	{
																		id : 'identifier-class-detail',
																		region : 'north',
																		split : true,
																		height : 100,
																		html : 'Class Detail'
																	},
																	{
																		id : 'identifier-class-properties',
																		title : 'Properties',
																		region : 'west',
																		split : true,
																		margins : '0 1 3 3',
																		width : 250,
																		height : 900,
																		minSize : 100,
																		items : [ grid_class_properties ]
																	},
																	{
																		title : 'Related Items',
																		layout : 'accordion',
																		split : true,
																		width : 220,
																		region : 'center',
																		margins : '0 3 3 0',
																		layoutConfig : {
																			animate : true,
																			fill : false
																		},
																		html : relatedClassArr[rowIndex]
																	} ]
														});
												myWin.show();
												Ext
														.get('identifier-class-detail').dom.innerHTML = '<div style="float:left; width:110px;"><img src="resources/images/class-badge.png"/></div><div style="padding-top:20px;" id="identifier"><b>'
														+ IdentificationByTag_value
														+ '</b><br/>'
														+ grid.classObjName
														+ '<br/>Transfer Type : '
														+ transferType_value
														+ '</div>'
												// this window should add into
												// the crums
											}
										});

						this.items = [this.dataGrid];

						this.tbar = this.buildToolbar();

						// super
						ExchangeManager.NavigationPanel.superclass.initComponent
								.call(this);
					},

					buildToolbar : function() {
						return [ {
							id : "card-1",
							xtype : "tbbutton",
							tooltip : 'Crum 1',
							text : '1...',
							disabled : false,
							handler : this.onOpen,
							scope : this
						} ];
					},

					onOpen : function(btn, ev) {
						var l = this.getLayout();
						var i = l.activeItem.id.split('card-')[1];
						var next = parseInt(i, 10) + 1;
						l.setActiveItem(next);
						var t = this.getTopToolbar();

						t.add([ {
							id : "card-btn-" + i,
							xtype : "tbbutton",
							tooltip : 'Crum ' + i,
							text : i + '...',
							disabled : false,
							handler : this.onOpen,
							scope : this
						} ]);

						t.doLayout();
						this.fireEvent('next', this, i);
					}

				});