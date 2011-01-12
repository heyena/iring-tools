/*
exchnageData_rows.json
appData_rows_json.json
 */
Ext
	.onReady(function() {
	Ext.BLANK_IMAGE_URL = 'resources/images/s.gif';
	Ext.QuickTips.init();

	Ext.state.Manager.setProvider(new Ext.state.CookieProvider());

	var directoryPanel = new ExchangeManager.DirectoryPanel({
		id : 'navigation-panel',
		region : 'west',
		collapsible : false,
		collapsed : false,
		border : true,
		split : true,
		width : 250,
		minSize : 175,
		maxSize : 500,
		url : 'directory'

	});

	var contentPanel = new Ext.TabPanel({
		region : 'center',
		id : 'content-panel',
		xtype : 'tabpanel',
		margins : '0 5 0 0',
		enableTabScroll : true
	});

	directoryPanel
		.on('exchange', function(panel, node, exchangeURI, tablabel) {
			// alert("exchangeURI /" + exchangeURI)
			Ext.Ajax.request({
				url : exchangeURI,
				method : 'GET',
				params : {},
				success : function(result, request) {
					var jsonData = Ext.util.JSON.decode(result.responseText);

					if (eval(jsonData.success) == false) {
						Ext.MessageBox.show({
							title : '<font color=red></font>',
							msg : 'Data is synchronized and no exchange happened',
							buttons : Ext.MessageBox.OK,
							icon : Ext.MessageBox.INFO
						});
												} else if (eval(jsonData.success) == true) {
													// alert(result.responseText);
													// open the new result tab
													// and refresh the actual
													// tab
													//var rowData = eval(jsonData.data);
													// var filedsVal =
													// eval(jsonData.headersList);
													
													var store = new Ext.data.JsonStore({
														data : jsonData.data,
														fields : [
														    'Identifier',
															'Message' ]
															
													});
													//autoLoad: true
													//store.loadData(rowData);

													var label = tablabel;
													// var columnData =
													// eval(jsonData.columnsData);
													var grid = new Ext.grid.GridPanel(
															{
																store : store,
																columns : [
																		{
																			header : 'Identifier',
																			width: 100,
																			dataIndex : 'Identifier',
																			sortable: true
																		},
																		{
																			header : 'Message',
																			width: 150,
																			dataIndex : 'Message',
																			sortable: true
																		} ],

																stripeRows : true,
																id : 'exchangeResultGrid_'
																		+ label,
																loadMask : true,
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
													 * After exchnage the Result
													 * Grid displayed in a new
													 * Window starts
													 */
													var strPositon = (Ext
															.getCmp('content-panel')
															.getPosition())
															.toString();
													var arrPositon = strPositon
															.split(",");

													var myResultWin = new Ext.Window(
															{
																title : 'Exchange Result ( '
																		+ label
																		+ ' )',
																id : 'label_'
																		+ label,
																x : arrPositon[0],
																y : parseInt(arrPositon[1]) + 25,

																closable : true,
																width : Ext
																		.getCmp(
																				'content-panel')
																		.getInnerWidth() - 2,
																height : Ext
																		.getCmp(
																				'content-panel')
																		.getInnerHeight(),
																forceFit : true,
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
																			directoryPanel
																					.openTab(
																							directoryPanel
																									.getSelectedNode(),
																							'true');
																		}
																	}
																},
																items : [ {
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
																} ]
															});

													myResultWin.show();
													/*
													 * After exchnage the Result
													 * Grid displayed in a new
													 * Window ends
													 */

													/*
													 * if
													 * (Ext.getCmp('content-panel').findById('tabResult-'+label)){
													 * //alert('aleready
													 * exists')
													 * Ext.getCmp('content-panel').remove(Ext.getCmp('content-panel').findById('tabResult-'+label));
													 * Ext.getCmp('content-panel').add(
													 * Ext.apply(grid,{
													 * id:'tabResult-'+label,
													 * title: label+'(Result)',
													 * closable:true })).show();
													 * }else {
													 * Ext.getCmp('content-panel').add(
													 * Ext.apply(grid,{
													 * id:'tabResult-'+label,
													 * title: label+'(Result)',
													 * closable:true })).show(); }
													 */
													// directoryPanel.openTab(directoryPanel.getSelectedNode(),'true');
												}
											}
										});
							});

			directoryPanel
					.on(
							'history',
							function(panel, node, exchangeURI, scopeId, uid) {
								// alert("History exchangeURI: /" + exchangeURI)
								Ext.Ajax
										.request({
											url : exchangeURI,
											method : 'GET',
											params : {},
											success : function(result, request) {
												var jsonData = Ext.util.JSON
														.decode(result.responseText);
												if (eval(jsonData.success) == false) {
													alert("Fail to get the Json Response after submission: "
															+ jsonData.response);
												} else if (eval(jsonData.success) == true) {
													alert('History Response:'
															+ result.responseText);
												}
											}
										});
							});

			directoryPanel
					.on(
							'open',
							function(panel, node, label, url, reload) {
								if ((contentPanel.get('tab_' + label) == undefined)
										|| (reload == 'true')) {
									// contentPanel
									// var w =
									// Ext.getCmp(contentPanel).getActiveTab();
									contentPanel
											.getEl()
											.mask(
													'<span><img src="resources/js/ext-js/resources/images/default/grid/loading.gif"/> Loading.....</span>');
									var dataTypeNode = node.parentNode.parentNode;
									var obj = node.attributes;
									var item = obj['properties'];
									var scopeId = dataTypeNode.parentNode.attributes['text'];
									var nodeType = obj['iconCls'];
									var nodeText = obj['text'];
									var uid = item[0].value;

									var parentName = node.parentNode.text;
									var pageURL = null;

									Ext.Ajax
											.request({
												url : url,
												method : 'GET',
												params : {},
												success : function(result,
														request) {
													contentPanel.getEl()
															.unmask();
													if ((nodeType == 'exchange' && uid != '')) {
														pageURL = 'exchDataRows?scopeName='
																+ scopeId
																+ '&idName='
																+ uid;
														// pageURL
														// ="exchnageData_rows.json";

													} else if (nodeType == 'graph') {
														var appName = parentName;
														pageURL = 'appDataRows?scopeName='
																+ scopeId
																+ '&appName='
																+ appName
																+ '&graphName='
																+ nodeText;
														// pageURL
														// ="appData_rows_json.json";
														// alert("Application
														// DataRows URI:
														// "+pageURL);
													}

													var responseData = Ext.util.JSON
															.decode(result.responseText);

													// alert(pageURL)
													if (eval(responseData.success) == false) {

														Ext.MessageBox
																.show({
																	title : '<font color=red>Error</font>',
																	msg : 'No Exchange Results found for:<br/>'
																			+ label,
																	buttons : Ext.MessageBox.OK,
																	icon : Ext.MessageBox.ERROR
																});

														return false;

													} else {

														var newTab = new ExchangeManager.NavigationPanel(
																{
																	title : label,
																	id : 'tab_'
																			+ label,
																	configData : responseData,
																	url : pageURL,
																	closable : true,
																	nodeDisplay : "...",
																	scopeName : scopeId,
																	idName : uid,
																	appName : appName,
																	graphName : nodeText,
																	nodeType : nodeType
																});

														contentPanel
																.add(newTab);
														contentPanel
																.activate(newTab);

														newTab
																.on(
																		'beforeclose',
																		function(
																				newTab) {
																			var deleteReqURL = null;
																			if ((nodeType == 'exchange' && uid != '')) {
																				// deleteReqURL
																				// =
																				// 'dataObjects/deleteDataObjects/'+nodeType+'/'+scopeId+'/'+uid
																				deleteReqURL = 'cleanExchDataRows?scopeName='
																					+ scopeId
																					+ '&idName='
																					+ uid;
																				
																			} else if (nodeType == 'graph') {
																				// deleteReqURL
																				// =
																				// 'dataObjects/deleteGraphObjects/'+nodeType+'/'+scopeId+'/'+node.parentNode.text+'/'+nodeText
																				deleteReqURL = 'cleanAppDataRows?scopeName='
																					+ scopeId
																					+ '&appName='
																					+ appName
																					+ '&graphName='
																					+ nodeText;
																			}
																			if (deleteReqURL != null) {
																				Ext.Ajax
																						.request({
																							url : deleteReqURL,
																							method : 'GET',
																							params : {},
																							success : function(
																									result,
																									request) {
																								// console.log('delete
																								// response
																								// for
																								// '
																								// +url+'
																								// : '+
																								// eval(Ext.util.JSON.decode(result.responseText).success));
																							}
																						});
																			}
																		});
													}
												},
												failure : function(result,
														request) {
													alert('error: '
															+ result.responseText);
												}
											});
								} else {
									contentPanel.setActiveTab('tab_' + label);
								}
							});

			var viewport = new Ext.Viewport({
				layout : 'border',
				renderTo : Ext.getBody(),
				items : [ {
					region : 'north',
					// baseCls : 'x-plain',
					height : 65, // give north and south regions a height
					margins : '-10 5 0 0',
					contentEl : 'header'
				}, directoryPanel, contentPanel ]
			});

		});