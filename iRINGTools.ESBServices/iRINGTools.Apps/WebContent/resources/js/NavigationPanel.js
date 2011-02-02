/*
exchangeData_relations.json xchangeDataRelated_URI
exchangeDataRelatedRows?scopeName=12345_000&idName=1&id=66015-O
exchangesubgrid_grid.json relatedDataGrid_URI
relatedDataGrid?scopeName=12345_000&idName=1&id=66015-O&classId=rdl:R3847624234
exchangesubgrid_rows.json relatedDataRows_URI
relatedDataRows?scopeName=12345_000&idName=1&id=66015-O&classId=rdl:R3847624234
*/
Ext.ns('ExchangeManager');


var hash = function(obj){	
	return obj.id; 	
};  
var globalHistoryPanel = {}; 	
var exTopNavPanel = {};
var appTopNavPanel = {};
var exSubNavPanel = {};
var appSubNavPanel = {};
var exClassPanel = {};
var appClassPanel = {};
var exListView = {};
var appListView = {};
	
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
	firstTabId : null,
	classId : null,
	dtoIdentifier : null,
	key : null,
	node : null,
	
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
		var classObjectName = this.configData.classObjName;

		if (classObjectName == null) {
			this.nodeDisplay = "Related Items";
		} else {
			this.nodeDisplay = classObjectName;
		}
		
		
		var filterSet = eval(this.configData.filterSets);
		//var pageSize = parseInt(this.configData.pageSize);
		var pageSize = 25;
		var sortBy = this.configData.sortBy;
		
		
		var sortOrder = this.configData.sortOrder;
		
		
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
					limit : pageSize/*,
					identifier : this.identifier,
					refClassIdentifier : this.refClassIdentifier*/
				}
			}/*,
			baseParams : {
				'identifier' : this.identifier,
				'refClassIdentifier' : this.refClassIdentifier
			}*/
		});

		this.dataGrid = new Ext.grid.GridPanel({
			store : store,
			columns : headerList,
			
			stripeRows : true,
			selModel: new Ext.grid.RowSelectionModel({ singleSelect: true }),

			//loadMask : true,
			plugins : [ filters ],
			layout : 'fit',
			frame : true,
			autoSizeColumns : true,
			autoSizeGrid : true,
			AllowScroll : true,
			height : 250,
			minColumnWidth : 100,
			columnLines : true,
			classObjName : classObjectName,
			enableColumnMove : false,
			bbar : new Ext.PagingToolbar({
				pageSize : pageSize,
				store : store,
				displayInfo : true,
				autoScroll : true,
				plugins : [
					filters,
					new Ext.ux.plugin.PagingToolbarResizer(
					{
						displayText : 'Page Size',
						options : [ 25, 50, 100,
							200, 500 ],
						prependCombo : true
				})]
			})
		});

		
		
		
		this.dataGrid.on('beforerender', this.beforeRender,
				this);
		this.dataGrid.on('cellclick', this.onCellClick, this);
		
		// for related items we don't require the bbar
		if (this.identifier != 0
				&& this.refClassIdentifier != 0) {
			// ***** this.dataGrid.getBottomToolbar().hide();
			// this.dataGrid.classObjName=this.identifier;
		}
		
		if (this.id != this.firstTabId) {
			var thiscontentPanel = this.buildContetpanel(false);
			
		}else 
			var thiscontentPanel = this.buildContetpanel(true);
		
		this.items = [ {
			title : this.nodeDisplay,
			// items : [ this.dataGrid ],
			items : [ thiscontentPanel ],
			layout : 'fit'
		} ];

		if (this.nodeType == "exchange" && this.id == this.firstTabId) {
			buildToolbar(this);
		}
		
        var hisotyrPanelkey = hash(this);
       
        if (hisotyrPanelkey == this.firstTabId) {
        	var rootThisTab = this;
	        rootThisTab.key = this.key + this.nodeDisplay;
        }
        
        if (hisotyrPanelkey == this.firstTabId && this.nodeType == 'exchange') {
        	var hstId = 'hst-' + this.scopeName + '_' + this.idName;
            var historyPanel = Ext.getCmp(hstId);
        	globalHistoryPanel[hisotyrPanelkey] = historyPanel;
        	
        }
        if (globalHistoryPanel[hisotyrPanelkey] != undefined)
        	globalHistoryPanel[hisotyrPanelkey].collapsed = 'true';
        	//historyPanel.collapsed = true;
		
         
		ExchangeManager.NavigationPanel.superclass.initComponent.call(this);
		
		
		
	},
	
	buildContetpanel : function(ifBuildHistoryPanel) {
		
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
						tooltip : 'Close Exchange Logs',
						disabled : false,
						handler : function() {
							history_panel
									.collapse();
						}
					}]
				})
			});

			if (ifBuildHistoryPanel == true)
				Contetpanel.add(history_panel);

		}
		return Contetpanel;

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
		

		var historyDetailUri = 'exchangeHistoryDetail?scopeName='
				+ this.scopeName
				+ '&idName='
				+ this.idName
				+ '&historyId=' + rowIndex;
		
		// var historyDetailUri ='exchangehistory_detail.json';

		Ext.Ajax.request({
			url : historyDetailUri,
			method : 'GET',
			success : function(result, request) {
				var responseTxt = result.responseText;
				var jsonData = Ext.util.JSON.decode(result.responseText);

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
						width : Ext.getCmp('content-panel').getInnerWidth() - 2,
						height : Ext.getCmp('content-panel').getInnerHeight(),
						layout : 'border',
						listeners : {
							beforerender : {
								fn : function() {
									//Ext.getBody().mask();
								}
							},
							close : {
								fn : function() {
									//Ext.getBody().unmask();
								}
							}
						},
						items : [{
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
				app.setAlert('false', 'History Details', result.responseText);
			}
		});
	},
	onCellClick : function(grid, rowIndex, columnIndex, e) {
		
		
        
        var contentPan = Ext.getCmp('content-panel');
        var topNavPan = contentPan.getActiveTab();
       

        var key = hash(topNavPan);
        if (key == topNavPan.firstTabId && topNavPan.nodeType == 'graph')
        	appTopNavPanel[key] = topNavPan;
        if (key == topNavPan.firstTabId && topNavPan.nodeType == 'exchange') {
        	
        	exTopNavPanel[key] = topNavPan;
        }
      
		
		
		var cm = grid.getColumnModel();
		var record = grid.getStore().getAt(rowIndex); 
		var fieldName = cm.getDataIndex(columnIndex); 
		if (fieldName == 'Identifier' && record.get(fieldName) != '') {
			grid.getEl().mask('<span><img src="resources/js/ext-js/resources/images/default/grid/loading.gif"/> Loading.....</span>');
			var IdentificationByTag_value = record.get(fieldName);	
			var identityNoHtml = removeHTMLTags(IdentificationByTag_value);
			
			var transferType_value = record.get('TransferType');

			var rowDataArr = [];
			var abc = cm.getColumnCount();
			for ( var i = 0; i < cm.getColumnCount(); i++) {
				fieldHeader = grid.getColumnModel().getColumnHeader(i); 
				fieldValue = record.get(grid.getColumnModel().getDataIndex(i));
				tempArr = Array(fieldHeader, removeHTMLTags(fieldValue));
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
			var grid_class_properties = new Ext.grid.GridPanel({
				store : pStore,
				columns : pColumnData,
				stripeRows : true,
				autoSizeColumns : true,
				autoSizeGrid : true,
				AllowScroll : true,
				columnLines : true,
				enableColumnMove : false
			});

			
			if (this.firstTabId == this.id) {
				if (this.nodeType == "exchange") {
					var xchangeDataRelated_URI = 'exchangeDataRelatedRows?scopeName='
						+ this.scopeName
						+ '&idName='
						+ this.idName
						+ '&id='
						+ identityNoHtml;
				} 
				else {
					var xchangeDataRelated_URI = 'appDataRelations?scopeName='
						+ this.scopeName
						+ '&appName='
						+ this.appName
						+ '&graphName='
						+ this.graphName
						+ '&id='
						+ identityNoHtml;

				}
			} else {
				if (this.nodeType == "exchange") {
					var xchangeDataRelated_URI = 'relatedDataRelationsRows?scopeName='
						+ this.scopeName
						+ '&idName='
						+ this.idName
						+ '&id='
						+ this.dtoIdentifier
						+ '&classId='
						+ this.classId
						+ '&relatedId='
						+ identityNoHtml;
				} 
				else {
					var xchangeDataRelated_URI = 'relatedAppDataRelationsRows?scopeName='
						+ this.scopeName
						+ '&appName='
						+ this.appName
						+ '&graphName='
						+ this.graphName
						+ '&id='
						+ this.dtoIdentifier
						+ '&classId='
						+ this.classId
						+ '&relatedId='
						+ identityNoHtml;
				}
			}

			var navPanel = this;
			
			Ext.Ajax.request({
    		url : xchangeDataRelated_URI,
    		method : 'GET',
    		params : {},
    		success : function(result, request) {
    			responseData = Ext.util.JSON.decode(result.responseText);
    
    			if (eval(responseData.success) == true) {
    				var store = new Ext.data.JsonStore({
							data : responseData.data,
							fields : [ 'id', 'label' ]
						});
    
    				var contentPanel0 = Ext.getCmp('content-panel');
			        var topNavPanel0 = contentPanel0.getActiveTab();
			        var thisTab = topNavPanel0.getActiveTab();
			        var key0 = thisTab.key;
			        
			        if (key0 == undefined)
			        	key0 = topNavPanel0.key;
			       
			        var keys0 = key0 + identityNoHtml;
			        var nodeType0 = topNavPanel0.nodeType;
			        
			        if (nodeType0 == 'exchange') {
			        	exListView[keys0] = new Ext.list.ListView({
							store : store,
							hideHeaders : true,
							singleSelect : true,
							emptyText : 'No Items to display',
							reserveScrollOffset : true,
							columns : [{
								header : 'Class Name',
								dataIndex : 'label'
							},
							{
								header : 'id',
								hidden : true
							}]
						});
			        	exListView[keys0].on(
								'click', 
								clickListViewItem, this
								
							);
			        } else if (nodeType0 == 'graph') {
			        	appListView[keys0] = new Ext.list.ListView({
							store : store,
							hideHeaders : true,
							singleSelect : true,
							emptyText : 'No Items to display',
							reserveScrollOffset : true,
							columns : [{
								header : 'Class Name',
								dataIndex : 'label'
							},
							{
								header : 'id',
								hidden : true
							}]
						});
			        	appListView[keys0].on(
								'click', 
								clickListViewItem, this
								
							);
			        }	        
    				
    				var thisGrid = grid;
    				var classObjectName = thisGrid.classObjName;
    				
    				var contentPanel2 = Ext.getCmp('content-panel');
			        var topNavPanel2 = contentPanel2.getActiveTab();
			        var thisTab = topNavPanel2.getActiveTab();
			        var topNavPanelId2 = topNavPanel2.id;
			        if (thisTab.key != undefined)
			        	var key2 = thisTab.key;
			        else
			        	var key2 = topNavPanel2.key;
    				var nodeType2 = topNavPanel2.nodeType;
			        var tabId2 = identityNoHtml;
			        var title2 = identityNoHtml;
			        var keys2 = key2 + tabId2;
			        
			        if (nodeType2 == 'exchange') {
			        	exClassPanel[keys2] = new Ext.Panel({
			        		autoWidth : true,
			        		layout : 'border',
			        		items : [{
			        			height : 50,
			        			region : 'north',
			        			html : '<div style="background-color:#eee;"><div style=" float:left; width:60px"><img src="resources/images/class-badge.png" style="margin:2 4 4 4; height:46px"/></div><div style="background-color:#eee; width:100%; height:100%; padding-top:10px;"><b>'
  									+ title2
  									+ '</b><br>'
  									+ grid.classObjName
  									+ '<br>&nbsp;</div></div>'
			        		},
			        		{
			        			title : 'Properties',
			        			region : 'west',
			        			split : true,
			        			width : 300,
			        			layout : 'fit',
			        			items : [ grid_class_properties ]
			        		},
			        		{
			        			title : 'Related Items',
			        			split : true,
			        			region : 'center',
			        			layout : 'fit',
			        			items : exListView[keys2]
			        		}]
			        	});

			        	exSubNavPanel[keys2] = {
			        		id : tabId2,
			        		title : title2,
			        		layout : 'fit',
			        		key : keys2,
			        		forceFit : true,
			        		items : [ exClassPanel[keys2] ],
			        		closable : false,
			        		listeners : {
			        			'tabchange' : closeChildTabs
			        		}
			        	};
			        	
			        	var thisTopNavPanel2 = exTopNavPanel[topNavPanelId2];
			        	
			        } else if (nodeType2 == 'graph') {
			        	appClassPanel[keys2] = new Ext.Panel({
			        		autoWidth : true,
			        		layout : 'border',
			        		items : [{
			        			height : 50,
			        			region : 'north',
			        			html : '<div style="background-color:#eee;"><div style="float:left; width:60px"><img src="resources/images/class-badge.png" style="margin:2 4 4 4; height:46px"/></div><div style="background-color:#eee; width:100%; height:100%; padding-top:10px;"><b>'
  									+ title2
  									+ '</b><br>'
  									+ grid.classObjName
  									+ '<br>&nbsp;</div></div>'
			        		},
			        		{
			        			title : 'Properties',
			        			region : 'west',
			        			split : true,
			        			width : 300,
			        			layout : 'fit',
			        			items : [ grid_class_properties ]
			        		},
			        		{
			        			title : 'Related Items',
			        			split : true,
			        			region : 'center',
			        			layout : 'fit',
			        			items : appListView[keys2]
			        		}]
			        	});

			        	appSubNavPanel[keys2] = {
			        		id : tabId2,
			        		title : title2,
			        		layout : 'fit',
			        		key : keys2,
			        		forceFit : true,
			        		items : [ appClassPanel[keys2] ],
			        		closable : false,
			        		listeners : {
			        			'tabchange' : closeChildTabs
			        		}
			        	};
			        	
			        	var thisTopNavPanel2 = appTopNavPanel[topNavPanelId2]; 
			        	
			        }
  
			        if (thisTab.dataGrid == undefined)
			        	thisTopNavPanel2.dataGrid.getEl().unmask();
			        else
			        	thisTab.dataGrid.getEl().unmask();
  					
  					var displayTab2 = thisTopNavPanel2.getItem(tabId2);
            
  					if (nodeType2 == "exchange")
  						hideToolBarButton(topNavPanel2);
  					  					
  					if (displayTab2 == undefined) {						
  						if (nodeType2 == 'exchange')
  							thisTopNavPanel2.add(exSubNavPanel[keys2]).show();
  						else if (nodeType2 == 'graph')
  							thisTopNavPanel2.add(appSubNavPanel[keys2]).show();
  					} 
  					else {
  						displayTab2.show();
  					}
  				} 
    			else if (eval(responseData.success) == false) {
  					Ext.MessageBox.show({
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

function buildToolbar(np) {
	np.tbar = new Ext.Toolbar({
		xtype : "toolbar",
		items : [{
			xtype : "tbbutton",
			id : 'gridHistory',
			text : 'Open Exchange Logs',
			icon : 'resources/images/16x16/edit-find.png',
			tooltip : 'Open Exchange Logs',
			disabled : false,
			handler : function() {
				exchangeHistory(np.scopeName, np.idName, np);
			}
		},{
			xtype : "tbbutton",
			id : 'gridExchange',
			text : 'Exchange',
			icon : 'resources/images/16x16/go-send.png',
			tooltip : 'Exchange',
			disabled : false,
			handler : function() {
				showExchangeResponseWindow(np.scopeName, np.idName, np);
			}
		}]
	});
	
}

/*
 * buildToolbar : function() { return [ { id : "card-1", xtype : "tbbutton",
 * tooltip : 'Crum 1', text : '1...', disabled : false, handler : this.onOpen,
 * scope : this } ] },
 */
function removeHTMLTags(strInputCode) {
	/*
	 * This line is optional, it replaces escaped brackets
	 * with real ones, i.e. < is replaced with < and > is
	 * replaced with >
	 */
	strInputCode = strInputCode.replace(/&(lt|gt);/g,
		function(strMatch, p1) {
			return (p1 == "lt") ? "<" : ">";
		});

	var strTagStrippedText = strInputCode.replace(/<\/?[^>]+(>|$)/g, "");

	return strTagStrippedText;
}

function exchangeHistory(scopeName, idName, np) {
	var historyURI = 'exchangeHistory?scopeName=' + scopeName + '&idName=' + idName;
	
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
			} 
			else {
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
					//loadMask : true,
					autoSizeColumns : true,
					autoSizeGrid : true,
					AllowScroll : true,
					minColumnWidth : 100,
					columnLines : true,
					enableColumnMove : true
				});
				hstGridPanel.on('cellclick', np.openHistoryWin, np);

				// add the GridPanel to HistoryPanel
				//var historyPanel = Ext.getCmp(hstId);
				
				
		        
		        var contentPanel3 = Ext.getCmp('content-panel');
		        var topNavPanel3 = contentPanel3.getActiveTab();
		        var key3 = topNavPanel3.id;
		        
				var historyPanel = globalHistoryPanel[key3];
				
				if (historyPanel != undefined && hstGridPanel != undefined) {
					historyPanel.add(hstGridPanel);
					historyPanel.doLayout();

					if (historyPanel.collapsed == true || historyPanel.collapsed == 'true') {
						historyPanel.expand();
						historyPanel.collapsed = false;
					}
					
					
					
				}
				// Ext.getCmp(hstId).getEl().unmask();
			}
		},
		failure : function(result, request) {			
			app.setAlert(false, 'History List', result.responseText);
		}
	});
}

function clickListViewItem(dataView, index, node, e) {
	
	var refClassIdentifier = dataView.store.data.items[index].data.reference;
	var classId = dataView.store.data.items[index].data.id;
	var relatedClassName = dataView.store.data.items[index].data.name;
	var scopeId = dataView.store.data.items[index].data.scopeId;
	var exchangeId = dataView.store.data.items[index].data.exchangeID;
		
	var contentPanel0 = Ext.getCmp('content-panel');
    var topNavPanel0 = contentPanel0.getActiveTab();
    var thisTab = topNavPanel0.getActiveTab();
    
    var dtoIdentifier = thisTab.title;
	var ntyp = topNavPanel0.nodeType;
		
	if (ntyp == "exchange") {
		var relatedDataGrid_URI = 'relatedDataGrid?scopeName='
				+ topNavPanel0.scopeName
				+ '&idName='
				+ topNavPanel0.idName
				+ '&id='
				+ dtoIdentifier
				+ '&classId='
				+ classId;
	} 
	else {
		var relatedDataGrid_URI = 'relatedAppDataGrid?scopeName='
				+ topNavPanel0.scopeName
				+ '&appName='
				+ topNavPanel0.appName
				+ '&graphName='
				+ topNavPanel0.graphName
				+ '&id='
				+ dtoIdentifier
				+ '&classId='
				+ classId;
	}
		
	if (ntyp == "exchange") {
		var relatedDataRows_URI = 'relatedDataRows?scopeName='
			+ topNavPanel0.scopeName
			+ '&idName='
			+ topNavPanel0.idName
			+ '&id='
			+ dtoIdentifier
			+ '&classId='
			+ classId;
	} 
	else {    
		var relatedDataRows_URI = 'relatedAppDataRows?scopeName='
			+ topNavPanel0.scopeName
			+ '&appName='
			+ topNavPanel0.appName
			+ '&graphName='
			+ topNavPanel0.graphName
			+ '&id='
			+ dtoIdentifier
			+ '&classId='
			+ classId;
	}

	topNavPanel0.getEl().mask('<span><img src="resources/js/ext-js/resources/images/default/grid/loading.gif"/> Loading.....</span>');
	Ext.Ajax.request({
		url : relatedDataGrid_URI,
		method : 'GET',
		params : {},									
		success : function(result, request) {
			topNavPanel0.getEl().unmask();
			var responseData = Ext.util.JSON.decode(result.responseText);
			var pageURL = relatedDataRows_URI;
			var tabTitle1 = removeHTMLTags(dataView.store.data.items[index].data.label);
			var contentPanel1 = Ext.getCmp('content-panel');
	        var topNavPanel1 = contentPanel1.getActiveTab();
	        var thisTab1 = topNavPanel1.getActiveTab();
	        var topNavPanelId1 = topNavPanel1.id;
	        var key1 = thisTab1.key;
	        var tabId1 = tabTitle1;
	        var keys1 = key1 + tabId1;
	        var nodeType1 = topNavPanel1.nodeType;
		        
	        if (nodeType1 == 'exchange') {
	        	exSubNavPanel[keys1] = new ExchangeManager.NavigationPanel({
	        		title : tabTitle1,
	        		id : tabId1,
	        		scopeName : topNavPanel1.scopeName,								        						        		
	        		configData : responseData,
	        		classObjName : topNavPanel1.classObjName,
	        		idName : topNavPanel1.idName,
	        		nodeType : nodeType1,
	        		url : pageURL,
	        		closable : false,        		
	        		identifier : dtoIdentifier,
	        		refClassIdentifier : refClassIdentifier,
	        		firstTabId : topNavPanel1.firstTabId,
	        		classId : classId,
	        		dtoIdentifier : dtoIdentifier,
	        		key : keys1,
	        		listeners : {
	        			'tabchange' : closeChildTabs
	        		}
	        	});
		        	
		        var thisTopNavPanel1 = exTopNavPanel[topNavPanelId1];
		        	
		    } else if (nodeType1 == 'graph') {
	        	appSubNavPanel[keys1] = new ExchangeManager.NavigationPanel({
	        		title : tabTitle1,
		       		id : tabId1,
		       		scopeName : topNavPanel1.scopeName,
		       		appName : topNavPanel1.appName,
		       		graphName : topNavPanel1.graphName,
	        		configData : responseData,
	        		classObjName : topNavPanel1.classObjName,
		       		idName : topNavPanel1.idName,
		       		nodeType : nodeType1,
		       		url : pageURL,
		       		closable : false,
		       		identifier : dtoIdentifier,
		       		refClassIdentifier : refClassIdentifier,
		       		firstTabId : topNavPanel1.firstTabId,
		       		classId : classId,
		       		dtoIdentifier : dtoIdentifier,
		       		key : keys1,
		       		listeners : {
		       			'tabchange' : closeChildTabs
		       		}
		       	});
		        	
		       	var thisTopNavPanel1 = appTopNavPanel[topNavPanelId1];
		        	
   	        }
		        
			var displayTab1 = thisTopNavPanel1.getItem(tabId1);
				
			if (displayTab1 == undefined) {
				if (nodeType1 == 'exchange')
					thisTopNavPanel1.add(exSubNavPanel[keys1]).show();
				else if (nodeType1 == 'graph')
					thisTopNavPanel1.add(appSubNavPanel[keys1]).show();
			} 
			else {
				displayTab1.show();
			}
		},
		failure: function(result, request) {     
			thisTopNavPanel1.getEl().unmask();
		  app.setAlert(false, 'Related Items', result.responseText);
		}
	});
}


function hideToolBarButton(topNavPanel) {
	var thisbar = topNavPanel.tbar;	
	thisbar.setDisplayed(false);
}

function closeChildTabs(tp, activeTab) {
	var len = tp.items.length;
	
	if (len <= 1)
		return;


	
    
    var contPan = Ext.getCmp('content-panel');
    var topNPan = contPan.getActiveTab();
    var k = topNPan.id;

    var nodeType = topNPan.nodeType;
    
    var tab = tp.items.items;	
	var found = 0;
	var destroyTab;
	var keys;
	
   
    var thisTopNavPan = exTopNavPanel[k];
    
    
    for ( var i = 0; i < tab.length; i++) {
    	tb = tab[i];
    	if (found) {
    		destroyTab = thisTopNavPan.getItem(tb.id);
    		//theTab.hideTabStripItem();
    		//thisTopNavPan.hideTabStripItem(tb.id);
    		//this.dom.style.display = 'none';
    		//tb.hide();
    		destroyTab.destroy();
    		//i--;
    	} 
    	else if (tb == activeTab) {
    		found = 1;
    	}
    }
    
   
	
	if (tab.length == 1 ) {
		
		//reloadPanel();
		
		if (nodeType == 'exchange')			
			thisTopNavPan.tbar.setDisplayed(true);
		
	}
	
}

/*
 * Ext.getCmp(tabpanel_id).hideTabStripItem(tabitem_id);
 * Ext.getCmp('slip_box_1').hide();
 * 
 * Ext.getCmp('slip_box_1').hide();
    slip_box.getTabEl('slip_box_1').dom.style.display = 'none';
 * 
 * 
 * function reloadPanel() {
	 var contentP = Ext.getCmp('content-panel');
     var topNavP = contentP.getActiveTab();
     if (topNavP != undefined){
   	  contentP.getItem(topNavP.id).destroy();
   	  directoryPanel.openTab(directoryPanel.getSelectedNode(), 'true');
     }
}*/

function showExchangeResponseWindow(scopeName, idName, np) {
    
	var exchangeURI='exchangeResponse?scopeName='+scopeName+'&idName='+idName+'&hasReviewed=true';
	  
	Ext.Ajax.request( {
      url : exchangeURI,
      method : 'GET',
      params : {},
      success : function(result, request) {
        var jsonData = Ext.util.JSON.decode(result.responseText);

        if (eval(jsonData.success) == false) {
          Ext.MessageBox.show( {
            title : '<font color=red></font>',
            msg : 'Data is synchronized and no exchange happened',
            buttons : Ext.MessageBox.OK,
            icon : Ext.MessageBox.INFO
          });
        }
        else if (eval(jsonData.success) == true) {
          var store = new Ext.data.JsonStore( {
            data : jsonData.data,
            fields : [ 'Identifier', 'Message' ]

          });
          // autoLoad: true
          // store.loadData(rowData);

          var label = np.title;
          // var columnData =
          // eval(jsonData.columnsData);
          var grid = new Ext.grid.GridPanel( {
            store : store,
            columns : [{
              header : 'Identifier',
              width : 80,
              dataIndex : 'Identifier',
              sortable : true
            },{
              header : 'Message',
              width : 300,
              dataIndex : 'Message',
              sortable : true
            }],

            stripeRows : true,
            id : 'exchangeResultGrid_' + label,
            //loadMask : true,
            layout : 'fit',
            frame : true,
            autoSizeColumns : true,
            autoSizeGrid : true,
            AllowScroll : true,
            minColumnWidth : 100,
            columnLines : true,
            autoWidth : true,
            enableColumnMove : false
          });

          /*
           * After exchnage the Result Grid displayed in a new Window starts
           */
          var strPositon = (Ext.getCmp('content-panel').getPosition()).toString();
          var arrPositon = strPositon.split(",");
          
          var myResultWin = new Ext.Window({
            title : 'Exchange Result ( ' + label + ' )',
            id : 'label_' + label,
            x : arrPositon[0],
            y : parseInt(arrPositon[1]) + 25,

            closable : true,
            width : Ext.getCmp('content-panel').getInnerWidth() - 2,
            height : Ext.getCmp('content-panel').getInnerHeight(),
            forceFit : true,
            layout : 'border',
            listeners : {
             beforerender : {
                fn : function() {
                  Ext.getBody().mask();
                }
              },
              close : {
                fn : function() {
                  Ext.getBody().unmask();
                  var contentP = Ext.getCmp('content-panel');
                  var topNavP = contentP.getActiveTab();
                  if (topNavP != undefined){                	  
                	  directoryPanel.openTab(np.node, 'true');
                  }
                 }
              }
            },
            items : [{
              region : 'center',
              layout : 'fit',
              collapsible : false,
              margins : '0 3 3 0',
              layoutConfig : {
                animate : true,
                fill : false
              },
              split : true,
              items : grid
            }]
          });

          myResultWin.show();
        }
      }
    });
  }
