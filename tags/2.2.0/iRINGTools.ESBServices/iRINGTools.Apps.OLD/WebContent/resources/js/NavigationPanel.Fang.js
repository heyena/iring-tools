Ext.ns('ExchangeManager');
/**
 * @class ExchangeManager.NavigationPanel
 * @extends Panel
 * @author by Gert Jansen van Rensburg
 */
ExchangeManager.NavigationPanel = Ext
		.extend(
				Ext.TabPanel,
				{
					title : 'NavigationPanel',
					activeItem : 0,
					configData : null,
					dataGrid : null,
					url : null,
					relatedClassArr : null,
					enableTabScroll : true,
					identifier : 0,
					refClassIdentifier : 0,
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

						//var rowData = eval(this.configData.rowData);
						var fieldList = eval(this.configData.headerLists);
						var headerList = eval(this.configData.columnDatas);
						var classObjName = this.configData.classObjName;
						var filterSet = eval(this.configData.filterSets);
						var pageSize = parseInt(this.configData.pageSize);
						
						alert("fieldList :" + fieldList);
						
						//var sortBy = this.configData.sortBy
						//var sortOrder = this.configData.sortOrder
						/*var filters = new Ext.ux.grid.GridFilters({
							// encode and local configuration options defined
							// previously for easier reuse
							encode : true, // json encode the filter query
							remotesort : true, // json encode the filter query
							local : false, // defaults to false (remote
											// filtering)
							filters : filterSet
						});*/

						/*
						 * this.relatedClassArr = new Array(); if
						 * (this.configData.relatedClasses != undefined) {
						 * for(var i=0; i <
						 * this.configData.relatedClasses.length; i++) { var key =
						 * this.configData.relatedClasses[i].identifier; var
						 * text = this.configData.relatedClasses[i].text;
						 * this.relatedClassArr[i] = text; } }
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
								// identifier : this.identifier,
								// refClassIdentifier : this.refClassIdentifier
								}
							},
							baseParams : {
						   /* 
						    *'identifier' : this.identifier,
							*'refClassIdentifier' : this.refClassIdentifier
							*/
							}
						});

						this.dataGrid = new Ext.grid.GridPanel({
							store : store,
							columns : headerList,
							stripeRows : true,
							loadMask : true,
							//plugins : [filters],
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
								//plugins : [filters],
								autoScroll : true
								
							})
						});

						/*
						 * if(!this.dataGrid.getBottomToolbar()){ var ptb = new
						 * Ext.PagingToolbar({ pageSize: pageSize, store: store,
						 * displayInfo: true, autoScroll: true, plugins:
						 * [filters] }); this.dataGrid.add(ptb);
						 * this.dataGrid.doLayout(); }else{ alert(1)
						 * 
						 * this.dataGrid.remove(this.dataGrid.bbar);
						 *  }
						 */

						this.dataGrid.on('beforerender', this.beforeRender,
								this);
						this.dataGrid.on('cellclick', this.onCellClick, this);
						// alert(this.dataGrid.classObjName)
						// for related items we don't require the bbar
						if (this.identifier != 0
								&& this.refClassIdentifier != 0) {
							this.dataGrid.getBottomToolbar().hide();
							// this.dataGrid.classObjName=this.identifier;
						}
						this.items = [ {
							// title:this.dataGrid.classObjName,
							title : 'Detail Grid View',
							items : [ this.dataGrid ],
							layout : 'fit'
						} ];

						 this.tbar = this.buildToolbar();

						// super
						ExchangeManager.NavigationPanel.superclass.initComponent
								.call(this);
					},
					beforeRender : function(grid) {
						var colmodel = grid.getColumnModel();
						for ( var i = 0; i < colmodel.getColumnCount(); i++) {
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
								return '<span style="color:' + spanColor
										+ ';">' + val + '</span>';
							});
						}
					},
					onCellClick : function(grid, rowIndex, columnIndex, e) {
						var cm = grid.getColumnModel();
						var record = grid.getStore().getAt(rowIndex); // Get
																		// the
																		// Record
						var fieldName = grid.getColumnModel().getDataIndex(
								columnIndex); // Get field name
						if (fieldName == 'Identifier'
								&& record.get(fieldName) != '') {
							var IdentificationByTag_value = record
									.get(fieldName);
							var transferType_value = record.get('TransferType');
							var rowDataArr = [];
							for ( var i = 3; i < cm.getColumnCount(); i++) {
								fieldHeader = grid.getColumnModel()
										.getColumnHeader(i); // Get field
																// name
								fieldValue = record.get(grid.getColumnModel()
										.getDataIndex(i));
								tempArr = Array(fieldHeader, fieldValue);
								rowDataArr.push(tempArr);
							}
							var filedsVal_ = '[{"name":"Property"},{"name":"Value"}]';
							var columnsData_ = '[{"id":"Property","header":"Property","width":144,"sortable":"true","dataIndex":"Property"},{"id":"Value","header":"Value","width":144,"sortable":"true","dataIndex":"Value"}]';
							var prowData = eval(rowDataArr);
							var pfiledsVal = eval(filedsVal_);
							var pColumnData = eval(columnsData_);
							// create the data store
							var pStore = new Ext.data.ArrayStore({
								fields : pfiledsVal
							});
							pStore.loadData(prowData);

							// create the Grid
							var grid_class_properties = new Ext.grid.GridPanel(
									{
										store : pStore,
										columns : pColumnData,
										stripeRows : true,
										height : 460,
										autoSizeColumns : true,
										autoSizeGrid : true,
										AllowScroll : true,
										minColumnWidth : 100,
										columnLines : true,
										enableColumnMove : false
									});

							var store = new Ext.data.JsonStore(
									{
										data : eval(this.configData.relatedClasses[rowIndex].label),
										fields : [ 'name', 'reference',
												'identifier', 'scopeId',
												'exchangeID' ]
									});

							var navPanel = this;
							var listView = new Ext.list.ListView({
								store : store,
								hideHeaders : true,
								singleSelect : true,
								emptyText : 'No Items to display',
								reserveScrollOffset : true,
								columns : [ {
									header : 'Class Name',
									dataIndex : 'name'
								}, {
									header : 'reference',
									hidden : true
								}, {
									header : 'scopeId',
									hidden : true
								}, {
									header : 'identifier',
									hidden : true
								}, {
									header : 'exchangeID',
									hidden : true
								} ]
							});

							listView
									.on(
											'click',
											function(dataView, index, node, e) {

												// ****
												// alert(this.configData.relatedClasses[rowIndex].identifier)
												var dtoIdentifier = dataView.store.data.items[index].data.identifier;
												var refClassIdentifier = dataView.store.data.items[index].data.reference;
												var relatedClassName = dataView.store.data.items[index].data.name;
												var scopeId = dataView.store.data.items[index].data.scopeId;
												var exchangeId = dataView.store.data.items[index].data.exchangeID;

												/*
												 * check-tree.js file: function
												 * displayRleatedClassGrid(refClassIdentifier,dtoIdentifier,relatedClassName) {}
												 * built the URI
												 * http://localhost:8888/dataObjects/getRelatedDataObjects/exchanges/12345_000/1/90003-SL/90
												 * Send the request to get the
												 * grid details built the
												 * gridPanel add this gp to new
												 * Panel add this Panel to the
												 * new tab
												 */
												var requestURL = 'dataObjects/getRelatedDataObjects/exchanges/'
														+ scopeId
														+ '/'
														+ exchangeId
														+ '/'
														+ dtoIdentifier
														+ '/'
														+ refClassIdentifier;

												// alert('Get related list
												// '+requestURL)
												Ext.Ajax
														.request({
															url : requestURL,
															method : 'POST',
															params : {},
															success : function(
																	result,
																	request) {
																var responseData = Ext.util.JSON
																		.decode(result.responseText);
																var pageURL = '/dataObjects/getPageData/exchanges/'
																		+ scopeId
																		+ '/'
																		+ exchangeId
																		+ '/'
																		+ dtoIdentifier
																		+ '/'
																		+ refClassIdentifier;
																// alert(refClassIdentifier+'_'+relatedClassName+'
																// '+dtoIdentifier)

																var newTab = new ExchangeManager.NavigationPanel(
																		{
																			title : relatedClassName,
																			id : 'tab_'
																					+ dtoIdentifier,
																			configData : responseData,
																			url : pageURL,
																			closable : true,
																			identifier : dtoIdentifier,
																			refClassIdentifier : refClassIdentifier
																		});

																if (navPanel
																		.get(newTab.id) == undefined) {
																	navPanel
																			.add(newTab);
																	navPanel
																			.setActiveTab(navPanel.items.length - 1);
																} else {
																	navPanel
																			.setActiveTab(navPanel.items.length - 1);
																}
															}
														});

												var newRelatedClassTab = {
													title : relatedClassName,
													// items:[classPanel],
													html : 'aaaaaaaa',
													closable : true
												};
												//navPanel.add(newRelatedClassTab);
											});

							var classPanel = new Ext.Panel(
									{
										autoWidth : true,
										height : 500,
										forceFit : true,
										layout : 'border',
										defaults : {
											collapsible : true,
											split : true
										},
										items : [
												{
													height : 100,
													region : 'north',
													collapsible : false,
													split : true,
													html : '<div style="float:left; width:110px;"><img src="resources/images/class-badge.png"/></div><div style="padding-top:20px;" id="identifier"><b>'
															+ this
																	.removeHTMLTags(IdentificationByTag_value)
															+ '</b><br/>'
															+ grid.classObjName
															+ '<br/>Transfer Type : '
															+ transferType_value
															+ '</div>'
												},
												{
													title : 'Properties',
													region : 'west',
													split : true,
													margins : '0 1 3 3',
													width : 250,
													height : 900,
													minSize : 100,
													items : [ grid_class_properties ]
												}, {
													title : 'Related Items',
													layout : 'fit',
													collapsible : false,
													split : true,
													width : 220,
													region : 'center',
													margins : '0 3 3 0',
													layoutConfig : {
														animate : true,
														fill : false
													},
													items : listView
												} ]
									});

							var newTab = {
								title : IdentificationByTag_value,
								id : this.title + '_'
										+ IdentificationByTag_value,
								items : [ classPanel ],
								closable : true
							};

							if (this.get(newTab.id) == undefined) {
								this.add(newTab);
								this.setActiveTab(this.items.length - 1);
							} else {
								this.setActiveTab(newTab.id);
							}
						}
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
					removeHTMLTags : function(strInputCode) {
						/*
						This line is optional, it replaces escaped brackets with real ones,
						i.e. < is replaced with < and > is replaced with >
						 */
						strInputCode = strInputCode.replace(/&(lt|gt);/g,
								function(strMatch, p1) {
									return (p1 == "lt") ? "<" : ">";
								});

						var strTagStrippedText = strInputCode.replace(
								/<\/?[^>]+(>|$)/g, "");

						return strTagStrippedText;
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