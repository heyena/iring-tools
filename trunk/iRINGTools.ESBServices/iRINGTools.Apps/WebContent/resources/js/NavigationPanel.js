/*
exchangeData_relations.json xchangeDataRelated_URI
exchangeDataRelatedRows?scopeName=12345_000&idName=1&id=66015-O
exchangesubgrid_grid.json relatedDataGrid_URI
relatedDataGrid?scopeName=12345_000&idName=1&id=66015-O&classId=rdl:R3847624234
exchangesubgrid_rows.json relatedDataRows_URI
relatedDataRows?scopeName=12345_000&idName=1&id=66015-O&classId=rdl:R3847624234
 */

Ext.ns('ExchangeManager');
/**
 * @class ExchangeManager.NavigationPanel
 * @extends Panel
 * @author by Gert Jansen van Rensburg
 */
ExchangeManager.NavigationPanel = Ext.extend(Ext.TabPanel, {
	title : 'NavigationPanel',
	activeItem : 0,
	configData : null,
	dataGrid : null,
	url : null,
	enableTabScroll : true,
	identifier : 0,
	refClassIdentifier : 0,
	nodeDisplay : null,
	scopeName : null,
	idName : null,
	appName : null,
	graphName : null,
	nodeType : null,
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

		// var rowData = eval(this.configData.Data);
		var fieldList = eval(this.configData.headerLists);
		var headerList = eval(this.configData.columnData);
		var classObjName = this.configData.classObjName;

		if (classObjName == null) {
			this.nodeDisplay = "Related Items";
		} else {
			this.nodeDisplay = classObjName;
		}

		var filterSet = eval(this.configData.filterSets);
		var pageSize = parseInt(this.configData.pageSize);
		var sortBy = this.configData.sortBy;
		// var sortBy = 'IdentificationByTag';
		// alert('sortBy /' + sortBy);
		var sortOrder = this.configData.sortOrder;
		// var sortOrder = 'DESC';
		// alert('sortOrder /' + sortOrder);
		var filters = new Ext.ux.grid.GridFilters({
			// encode and local configuration options defined
			// previously for easier reuse
			encode : true, // json encode the filter query
			remotesort : true, // json encode the filter query
			local : false, // defaults to false (remote
			// filtering)
			filters : filterSet
		});

		// build the header first
		// send the request to generate the arraystore
		// alert(this.url)
		var proxy = new Ext.data.HttpProxy({
			api : {
				read : new Ext.data.Connection({
					url : this.url,
					method : 'GET',
					timeout : 120000
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
			sortInfo : {
				field : sortBy,
				direction : sortOrder
			},
			autoLoad : {
				params : {
					start : 0,
					limit : pageSize,
					identifier : this.identifier,
					refClassIdentifier : this.refClassIdentifier
				}
			},
			baseParams : {
				'identifier' : this.identifier,
				'refClassIdentifier' : this.refClassIdentifier
			}
		});

		this.dataGrid = new Ext.grid.GridPanel({
			store : store,
			columns : headerList,
			stripeRows : true,
			loadMask : true,
			plugins : [ filters ],
			layout : 'fit',
			frame : true,
			autoSizeColumns : true,
			autoSizeGrid : true,
			AllowScroll : true,
			height : 250,
			minColumnWidth : 100,
			columnLines : true,
			classObjName : classObjName,
			enableColumnMove : false,
			bbar : new Ext.PagingToolbar({
				pageSize : pageSize,
				store : store,
				displayInfo : true,
				autoScroll : true,
				plugins : [ filters ]
			})
		});

		if (this.nodeType == "exchange") {
			this.tbar = this.buildToolbar();
		}

		this.dataGrid.on('beforerender', this.beforeRender,
				this);
		this.dataGrid.on('cellclick', this.onCellClick, this);
		// alert(this.dataGrid.classObjName)
		// for related items we don't require the bbar
		if (this.identifier != 0
				&& this.refClassIdentifier != 0) {
			// ***** this.dataGrid.getBottomToolbar().hide();
			// this.dataGrid.classObjName=this.identifier;
		}
		this.items = [ {
			title : this.nodeDisplay,
			// items : [ this.dataGrid ],
			items : [ this.buildContetpanel() ],
			layout : 'fit'
		} ];

		// this.tbar = this.buildToolbar();

		// super
		ExchangeManager.NavigationPanel.superclass.initComponent.call(this);
	},
	buildContetpanel : function() {
		// alert(this.idName)
		var Contetpanel = new Ext.Panel({
			layout : 'border',
			split : true,
			autoScroll : true,
			containerScroll : true,
			items : [ {
				region : 'center',
				layout : 'fit',
				height : '700',
				boxMinHeight : '300',
				bodyBorder : false,
				border : false,
				items : [ this.dataGrid ]
			} ]
		});

		if (this.nodeType == "exchange") {
			var history_panel = new Ext.Panel({
				id : 'hst-' + this.scopeName + '_' + this.idName,
				title : 'History',
				split : true,
				layout : 'fit',
				collapsed : true,
				// collapsible : true,
				region : 'south',
				height : '250',
				AllowScroll : true,
				forceFit : true,
				tbar : new Ext.Toolbar({
					xtype : "toolbar",
					items : [ {
						xtype : "tbbutton",
						text : 'Close Exchange Logs',
						icon : 'resources/images/16x16/document-new.png',
						tooltip : 'Show Grid',
						disabled : false,
						handler : function() {
							history_panel
									.collapse();
						}
					}]
				})
			});

			Contetpanel.add(history_panel);

		}
		return Contetpanel;

	},
	buildToolbar : function() {
		var np = this;
		var tbar = new Ext.Toolbar({
			xtype : "toolbar",
			items : [ {
				xtype : "tbbutton",
				id : 'gridHistory',
				text : 'Open Exchange Logs',
				icon : 'resources/images/16x16/edit-find.png',
				tooltip : 'Open History',
				disabled : false,
				handler : function() {
					exchangeHistory(np.scopeName,
							np.idName, np);
				}
			} ]
		});
		return tbar;
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
	openHistoryWin : function(hstGridPanel, rowIndex,
			columnIndex, e) {
		var cm = hstGridPanel.getColumnModel();
		var record = hstGridPanel.getStore().getAt(rowIndex); // Get
		// the
		// Record
		var str_history_header = '';
		hstID = record.get(cm.getDataIndex(0));

		for ( var i = 0; i < cm.getColumnCount(); i++) {
			fieldHeader = cm.getColumnHeader(i); // Get field
			// name
			fieldValue = record.get(cm.getDataIndex(i));

			if (fieldHeader != 'hstID') {
				str_history_header = str_history_header + '<b>'
						+ fieldHeader + ' : </b>' + fieldValue
						+ '<br/>';
			}
		}
		// showHistoryPopup(hstID,historyCacheKey);
		// alert(hstID+' '+str_history_header);

		var historyDetailUri = 'exchangeHistoryDetail?scopeName='
				+ this.scopeName
				+ '&idName='
				+ this.idName
				+ '&historyId=' + rowIndex;
		// alert(historyDetailUri)
		// var historyDetailUri ='exchangehistory_detail.json';

		Ext.Ajax.request({
			url : historyDetailUri,
			method : 'GET',
			success : function(result, request) {
				var responseTxt = result.responseText;
				var jsonData = Ext.util.JSON
						.decode(result.responseText);

				if (eval(jsonData.success) == false) {
					Ext.MessageBox
							.show({
								title : '<font color=red>Error</font>',
								msg : 'No History Result found for this<br/>',
								buttons : Ext.MessageBox.OK,
								icon : Ext.MessageBox.ERROR
							});
					return false;
				} else {
					var rowData = eval(jsonData.rowData);
					var fieldList = eval(jsonData.headerLists);
					var columnData = eval(jsonData.columnData);

					// shared reader
					var gp_reader = new Ext.data.ArrayReader({}, fieldList);
					var gp_store = new Ext.data.GroupingStore({
						reader : gp_reader,
						data : rowData,
						// sortInfo:{field:
						// 'Identifier',
						// direction: "ASC"},
						groupField : 'Identifier'
					});

					// create the Grid
					var statusListGridPanel = new Ext.grid.GridPanel({
						store : gp_store,
						columns : columnData,
						stripeRows : true,
						layout : 'fit',
						autoSizeColumns : true,
						autoSizeGrid : true,
						AllowScroll : true,
						minColumnWidth : 100,
						columnLines : true,
						enableColumnMove : false,
						// view: new
						// Ext.grid.GroupingView()
						view : new Ext.grid.GroupingView(
								{
									forceFit : true,
									groupTextTpl : '{text} ({[values.rs.length]} {[values.rs.length > 1 ? "Items" : "Item"]})'
								})
					});

					// get the centerPanel x,y
					// coordinates, used to set the
					// position of Indvidual Class(PopUp
					// window)
					var strPositon = (Ext.getCmp('content-panel').getPosition()).toString();
					var arrPositon = strPositon.split(",");
					
					statuslistPanel = new Ext.Panel({
						title : 'Status List',
						region : 'center',
						split : true,
						margins : '0 1 3 3',
						width : 250,
						height : 400,
						layout : 'fit'
					});

					statuslistPanel.add(statusListGridPanel);
					var hstPopup = new Ext.Window({
						title : 'History Detail',
						id : 'history-popup',
						closable : true,
						x : arrPositon[0],
						y : parseInt(arrPositon[1]) + 25,
						width : Ext
								.getCmp(
										'content-panel')
								.getInnerWidth() - 2,
						height : Ext
								.getCmp(
										'content-panel')
								.getInnerHeight(),
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
									id : 'history-header',
									region : 'north',
									split : true,
									frame : true,
									height : 80,
									html : 'Class Detail'
								},
								statuslistPanel ]
					});
					hstPopup.show();
					Ext.get('history-header').dom.innerHTML = '<div style="padding:10 5 0 10">'
							+ str_history_header
							+ "</div>";

				}// end of else
			},
			failure : function(result, request) {
				alert(result.responseText);
			}
		});
	},
	onCellClick : function(grid, rowIndex, columnIndex, e) {
		// alert('scope id passed from exchangemanager:
		// '+this.scopeName)
		// alert(this.ActiveTab)
		// alert(Window.obj.Commodity)
		// alert(Window.ExchangeManager.DirectoryPanel.Commodity)
		var cm = grid.getColumnModel();
		var record = grid.getStore().getAt(rowIndex); // Get
		// the
		// Record
		var fieldName = cm.getDataIndex(columnIndex); // Get
		// field
		// name
		/* Related Items and new classwindow code starts */
		if (fieldName == 'Identifier'
				&& record.get(fieldName) != '') {
			grid
					.getEl()
					.mask(
							'<span><img src="resources/js/ext-js/resources/images/default/grid/loading.gif"/> Loading.....</span>');
			var IdentificationByTag_value = record
					.get(fieldName);
			// alert('204--IdentificationByTag_value /'+
			// IdentificationByTag_value)
			var transferType_value = record.get('TransferType');

			var rowDataArr = [];
			var abc = cm.getColumnCount();
			for ( var i = 0; i < cm.getColumnCount(); i++) {
				fieldHeader = grid.getColumnModel()
						.getColumnHeader(i); // Get field
				// name
				fieldValue = record.get(grid.getColumnModel()
						.getDataIndex(i));
				tempArr = Array(fieldHeader, this
						.removeHTMLTags(fieldValue));
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
						autoSizeColumns : true,
						autoSizeGrid : true,
						AllowScroll : true,
						columnLines : true,
						enableColumnMove : false
					});

			/*
			 * alert('scope id passed from exchangemanager:
			 * '+this.scopeName) alert('exchangeid passed from
			 * exchange manager: : '+this.idName); alert('get
			 * from the Grid on
			 * wThanags(IdentificationByTag_value));
			 */

			if (this.nodeType == "exchange") {

				var xchangeDataRelated_URI = 'exchangeDataRelatedRows?scopeName='
						+ this.scopeName
						+ '&idName='
						+ this.idName
						+ '&id='
						+ this
								.removeHTMLTags(IdentificationByTag_value);
			} else {
				var xchangeDataRelated_URI = 'appDataRelations?scopeName='
						+ this.scopeName
						+ '&appName='
						+ this.appName
						+ '&graphName='
						+ this.graphName
						+ '&id='
						+ this
								.removeHTMLTags(IdentificationByTag_value);

			}

			// alert(xchangeDataRelated_URI)
			// exchangeDataRelatedRows?scopeName=54321_000&idName=2&id=90004-SC
			// exchangeDataRelatedRows?scopeName=12345_000&idName=1&id=90003-SL
			// exchangeDataRelatedRows?scopeName=12345_000&idName=4&id=90012-O

			// var
			// xchangeDataRelated_URI="exchangeData_relations.json";

			/* startttttttttt */
			var navPanel = this;
			Ext.Ajax.request({
    		url : xchangeDataRelated_URI,
    		method : 'GET',
    		params : {},
    		success : function(result, request) {
    			responseData = Ext.util.JSON
    					.decode(result.responseText);
    
    			if (eval(responseData.success) == true) {
    				var store = new Ext.data.JsonStore(
    						{
    							data : responseData.data,
    							// url:"exchangeData_relations.json",
    							fields : [ 'id',
    									'label' ]
    						});
    
    				var listView = new Ext.list.ListView(
    						{
    							store : store,
    							hideHeaders : true,
    							singleSelect : true,
    							emptyText : 'No Items to display',
    							reserveScrollOffset : true,
    							columns : [
    									{
    										header : 'Class Name',
    										dataIndex : 'label'
    									},
    									{
    										header : 'id',
    										hidden : true
    									} ]
    						});
    
    				listView
    						.on(
    								'click',
    								function(
    										dataView,
    										index,
    										node, e) {
    									// ****
    									// alert(this.configData.relatedClasses[rowIndex].identifier)
    									var dtoIdentifier = navPanel.removeHTMLTags(IdentificationByTag_value);
    									var refClassIdentifier = dataView.store.data.items[index].data.reference;
    									var classId = dataView.store.data.items[index].data.id;
    									var relatedClassName = dataView.store.data.items[index].data.name;
    									var scopeId = dataView.store.data.items[index].data.scopeId;
    									var exchangeId = dataView.store.data.items[index].data.exchangeID;
    
    									var ntyp = navPanel.nodeType;
    									if (navPanel.nodeType == "exchange") {
    										var relatedDataGrid_URI = 'relatedDataGrid?scopeName='
    												+ navPanel.scopeName
    												+ '&idName='
    												+ navPanel.idName
    												+ '&id='
    												+ dtoIdentifier
    												+ '&classId='
    												+ classId;
    									} else {
    										// relatedAppDataGrid?scopeName=12345_000&appName=abc&
    										// graphName=lines&id=90002-RV&classId=rdl:R49658319833
    
    										var relatedDataGrid_URI = 'relatedAppDataGrid?scopeName='
    												+ navPanel.scopeName
    												+ '&appName='
    												+ navPanel.appName
    												+ '&graphName='
    												+ navPanel.graphName
    												+ '&id='
    												+ dtoIdentifier
    												+ '&classId='
    												+ classId;
    									}
    									
    									if (navPanel.nodeType == "exchange") {
    										var relatedDataRows_URI = 'relatedDataRows?scopeName='
    												+ navPanel.scopeName
    												+ '&idName='
    												+ navPanel.idName
    												+ '&id='
    												+ dtoIdentifier
    												+ '&classId='
    												+ classId;
    									} 
    									else {    
    										var relatedDataRows_URI = 'relatedAppDataRows?scopeName='
    												+ navPanel.scopeName
    												+ '&appName='
    												+ navPanel.appName
    												+ '&graphName='
    												+ navPanel.graphName
    												+ '&id='
    												+ dtoIdentifier
    												+ '&classId='
    												+ classId;
    
    									}
    
    									Ext.Ajax
    											.request({
    												url : relatedDataGrid_URI,
    												method : 'GET',
    												params : {},
    												success : function(
    														result,
    														request) {
    
    													var responseData = Ext.util.JSON.decode(result.responseText);
    													var pageURL = relatedDataRows_URI;
    
    													var newTab = new ExchangeManager.NavigationPanel({
    														title : navPanel.removeHTMLTags(dataView.store.data.items[index].data.label),
    														id : 'tab_' + navPanel.removeHTMLTags(IdentificationByTag_value),
    														scopeName : navPanel.scopeName,
    														appName : navPanel.appName,
    														graphName : navPanel.graphName,
    														configData : responseData,
    														url : pageURL,
    														closable : false,
    														identifier : dtoIdentifier,
    														refClassIdentifier : refClassIdentifier
    													});
    
    													var topNavPanel = Ext.getCmp('content-panel').getActiveTab();
    													var displayTab = topNavPanel.getItem(newTab.id);
    													
    													if (displayTab == undefined) {
    														topNavPanel.add(newTab).show();
    													} 
    													else {
    														displayTab.show();
    													}
    												}
    											});
    								});

								var classPanel = new Ext.Panel(
										{
											autoWidth : true,
											forceFit : true,
											layout : 'border',
											defaults : {
												collapsible : false,
												split : true
											},
											items : [
													{
														height : 50,
														region : 'north',
														collapsible : false,
														split : true,
														html : '<div style="background-color:#eee; float:left; width:60px"><img src="resources/images/class-badge.png" style="margin:2 4 4 4; height:46px"/></div><div style="background-color:#eee; width:100%; height:100%; padding-top:10px;"><b>'
																+ navPanel
																		.removeHTMLTags(IdentificationByTag_value)
																+ '</b><br/>'
																+ grid.classObjName
																+ '</div>'
													},
													{
														title : 'Properties',
														region : 'west',
														split : true,
														margins : '0 1 3 3',
														width : 300,
														layout : 'fit',
														items : [ grid_class_properties ]
													},
													{
														title : 'Related Items',
														collapsible : false,
														split : true,
														region : 'center',
														margins : '0 3 3 0',
														layoutConfig : {
															animate : true,
															fill : false
														},
														items : listView
													// html:'aaaaaaaaaaaa'
													} ]
										});

								// alert('541--IdentificationByTag_value
								// /'+
								// navPanel.removeHTMLTags(IdentificationByTag_value))
								var newTab = {
									layout : 'fit',
									title : navPanel
											.removeHTMLTags(IdentificationByTag_value),
									id : this.title
											+ '_'
											+ navPanel
													.removeHTMLTags(IdentificationByTag_value),
									items : [ classPanel ],
									closable : false
								};

								navPanel.dataGrid.getEl().unmask();

								var topNavPanel = Ext.getCmp('content-panel').getActiveTab();
                var displayTab = topNavPanel.getItem(newTab.id);
                
                if (displayTab == undefined) {
                  topNavPanel.add(newTab).show();
                } 
                else {
                  displayTab.show();
                }
							} 
    			    else if (eval(responseData.success) == false) {

								Ext.MessageBox
										.show({
											title : '<font color=red></font>',
											msg : 'No Exchange Results found for:<br/>'
													+ label,
											buttons : Ext.MessageBox.OK,
											icon : Ext.MessageBox.INFO
										});
								return false;
							}
						}
					});
		}/* Related Items and new classwindow code ends */
	},
	/*
	 * buildToolbar : function() { return [ { id : "card-1",
	 * xtype : "tbbutton", tooltip : 'Crum 1', text : '1...',
	 * disabled : false, handler : this.onOpen, scope : this } ] },
	 */
	removeHTMLTags : function(strInputCode) {
		/*
		 * This line is optional, it replaces escaped brackets
		 * with real ones, i.e. < is replaced with < and > is
		 * replaced with >
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
	},

	listeners : {
		'tabchange' : closeChildTabs
	}

});

function exchangeHistory(scopeName, idName, np) {
	// np.
	var historyURI = 'exchangeHistory?scopeName=' + scopeName + '&idName='
			+ idName;
	// var historyURI=
	// 'exchangeHistory.json?scopeName='+scopeName+'&idName='+idName;
	Ext.Ajax.request({
		url : historyURI,
		method : 'GET',
		success : function(result, request) {
			var jsonData = Ext.util.JSON.decode(result.responseText);
			if (eval(jsonData.success) == false) {

				Ext.MessageBox.show({
					title : '<font color=red></font>',
					msg : 'No History Result found for this Exchange',
					buttons : Ext.MessageBox.OK,
					icon : Ext.MessageBox.INFO
				});

				return false;
			} else {
				var rowData = eval(jsonData.rowData);
				var fieldList = eval(jsonData.headerLists);
				var columnData = eval(jsonData.columnData);

				// set the ArrayStore to use in Grid
				var hstStore = new Ext.data.ArrayStore({
					fields : fieldList
				});
				hstStore.loadData(rowData);

				// id of the HistoryPanel
				// 'hst-'+scopeName+'_'+idName;
				var hstId = 'hst-' + scopeName + '_' + idName;

				if (Ext.getCmp('hstGrid' + scopeName + '_' + idName)) {
					// Ext.getCmp(hstId).expand();
					// Ext.getCmp(hstId).getEl().mask('Loading...');
					Ext.getCmp('hstGrid' + scopeName + '_' + idName).destroy();
				}

				// create the Grid
				var hstGridPanel = new Ext.grid.GridPanel({
					store : hstStore,
					id : 'hstGrid' + scopeName + '_' + idName,
					columns : columnData,
					stripeRows : true,
					loadMask : true,
					autoSizeColumns : true,
					autoSizeGrid : true,
					AllowScroll : true,
					minColumnWidth : 100,
					columnLines : true,
					enableColumnMove : true
				});
				hstGridPanel.on('cellclick', np.openHistoryWin, np);

				// add the GridPanel to HistoryPanel
				Ext.getCmp(hstId).add(hstGridPanel);
				Ext.getCmp(hstId).doLayout();

				if (Ext.getCmp(hstId).collapsed == true) {
					Ext.getCmp(hstId).expand();
				}
				// Ext.getCmp(hstId).getEl().unmask();
			}
		},
		failure : function(result, request) {
			alert(result.responseText);
		}
	});
}

function closeChildTabs(tp, newTab) {
	var len = tp.items.length;
	if (len <= 1)
		return;

	var tab = tp.items.items;
	var len = tab.length;

	var found = 0;

	for ( var i = 0; i < tab.length; i++) {
		tb = tab[i];
		if (found) {
			tb.destroy();
			i--;
		} else if (tb == newTab) {
			found = 1;
		}
	}
}
