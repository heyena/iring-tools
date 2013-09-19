Ext.ns('org.iringtools.apps.xmgr');

Ext.Ajax.on('requestexception', function(conn, response, options) {
	if (response.status == 0 || response.status == 408) {
		location.reload(true);
	}
});

function copyToClipboard(celldata) {
	/*
	 * if (window.clipboardData) // Internet Explorer
	 * window.clipboardData.setData ("Text", celldata);
	 */
	window.prompt("Copy to clipboard: Ctrl+C, Enter", celldata);
}

function storeSort(field, dir) {
	if (field == '&nbsp;')
		return;

	var limit = this.lastOptions.params.limit;

	this.lastOptions.params = {
		start : 0,
		limit : limit
	};

	if (dir == undefined) {
		if (this.sortInfo && this.sortInfo.direction == 'ASC')
			dir = 'DESC';
		else
			dir = 'ASC';
	}

	this.sortInfo = {
		field : field,
		direction : dir
	};

	this.reload();
}

function createGridStore(container, url) {
	var store = new Ext.data.Store({
		proxy : new Ext.data.HttpProxy({
			url : url,
			timeout : 1800000
		// 30 min
		}),
		reader : new Ext.data.DynamicGridReader({}),
		remoteSort : true,
		listeners : {
			exception : function(proxy, type, action, request, response) {
				if (!(response.status == 0 || response.status == 408)) {
					container.getEl().unmask();
					var message = 'Request URL: /' + request.url
							+ '.\n\nError description: '
							+ response.responseText;
					showDialog(500, 240, 'Error', message, Ext.Msg.OK, null);
				}
			}
		}
	});

	store.sort = store.sort.createInterceptor(storeSort);

	return store;
}

function createGridPane(store, pageSize, viewConfig, withResizer) {
	var filters = new Ext.ux.grid.GridFilters({
		remotesort : true,
		local : false,
		encode : true,
		filters : store.reader.filters
	});

	var plugins = [ filters ];

	if (withResizer) {
		var pagingResizer = new Ext.ux.plugin.PagingToolbarResizer({
			displayText : 'Page Size',
			options : [ 25, 50, 75, 100 ],
			prependCombo : true
		});

		plugins.push(pagingResizer);
	}

	var colModel = new Ext.grid.DynamicColumnModel(store);
	// var cellModel = new Ext.grid.CellSelectionModel({ singleSelect: true });
	var selModel = new Ext.grid.RowSelectionModel({
		singleSelect : true
	});
	var pagingToolbar = new Ext.PagingToolbar({
		store : store,
		pageSize : pageSize,
		displayInfo : true,
		autoScroll : true,
		plugins : plugins
	});

	var gridPane = new Ext.grid.GridPanel({
		identifier : store.reader.identifier,
		description : store.reader.description,
		layout : 'fit',
		minColumnWidth : 80,
		val : null,
		loadMask : true,
		store : store,
		stripeRows : true,
		viewConfig : viewConfig,
		cm : colModel,
		selModel : selModel,
		enableColLock : false,
		plugins : [ filters ],
		bbar : pagingToolbar,

		listeners : {
			cellclick : function(ts, td, cellIndex, record, tr, rowIndex, e,
					eOpts) {
				val = record.target.innerText;
			},
			/*
			 * celldblclick: function(ts, td, cellIndex, record, tr, rowIndex,
			 * e, eOpts ){ val = record.target.innerText; copyToClipboard(val); },
			 */
			beforeedit : function(e) {
				e.cancel = true;
			},
			keydown : function(evnt) {
				var keyPressed = evnt.getKey();
				if (evnt.ctrlKey) {
					/*
					 * After trial and error, the ctrl+c combination seems to be
					 * code 67
					 */
					if (67 == 67)// if (keyPressed == 67)
					{
						// var celldata =
						// gridPane.getSelectionModel().events.beforecellselect.obj.selection.record.data.value;
						copyToClipboard(val);

					}
				}
			}
		}
	});

	return gridPane;
}

function createXlogsPane(context, xlogsContainer, xlabel) {
	var xlogsUrl = 'xlogs' + context + '&xlabel=' + xlabel;
	var xlogsStore = createGridStore(xlogsContainer, xlogsUrl);

	xlogsStore
			.on(
					'load',
					function() {
						var xlogsPane = new Ext.grid.GridPanel(
								{
									id : 'xlogs-' + xlabel,
									store : xlogsStore,
									cellValue : '',
									stripeRows : true,
									loadMask : true,
									cm : new Ext.grid.DynamicColumnModel(
											xlogsStore),
									selModel : new Ext.grid.CellSelectionModel(
											{
												singleSelect : true
											}),
									enableColLock : true,
									tbar : new Ext.Toolbar(
											{
												items : [
														{
															xtype : 'tbspacer',
															width : 4
														},
														{
															xtype : 'label',
															html : '<span style="font-weight:bold">Exchange Results</span>'
														},
														{
															xtype : 'tbspacer',
															width : 4
														},
														{
															xtype : 'button',
															icon : 'resources/images/16x16/view-refresh.png',
															tooltip : 'Refresh',
															handler : function() {
																xlogsStore
																		.load();
															}
														} ]
											}),
									listeners : {
										beforeedit : function(e) {
											e.cancel = true;
										},
										cellclick : function(ts, td, cellIndex,
												record, tr, rowIndex, e, eOpts) {
											cellValue = record.target.innerText;
										},
										keydown : function(evnt) {
											var keyPressed = evnt.getKey();
											if (evnt.ctrlKey) {
												/*
												 * After trial and error, the
												 * ctrl+c combination seems to
												 * be code 67
												 */
												if (67 == 67)// if
												// (keyPressed
												// == 67)
												{
													// var celldata =
													// Ext.getCmp('property-pane').getSelectionModel().events.beforecellselect.obj.selection.record.data.value;
													copyToClipboard(cellValue);
												}
											}
										}
									}
								});

						if (xlogsContainer.items.length == 0) {
							xlogsContainer.add(xlogsPane);
							xlogsContainer.doLayout();
						} else {
							xlogsContainer.add(xlogsPane);
						}

						xlogsContainer.expand(false);
					});

	xlogsStore.load();
}

function createPageXlogs(scope, xid, xlabel, xFormattedTime, xtime, poolSize,
		itemCount) {
	var paneTitle = xlabel + ' (' + xFormattedTime + ')';
	var tab = Ext.getCmp('content-pane').getItem(paneTitle);

	if (tab != null) {
		tab.show();
	} else {
		var contentPane = Ext.getCmp('content-pane');
		contentPane.getEl().mask("Loading...", "x-mask-loading");

		var url = 'pageXlogs' + '?scope=' + scope + '&xid=' + xid + '&xtime='
				+ xtime + '&itemCount=' + itemCount;
		var store = createGridStore(contentPane, url);

		store.on('load', function() {
			var gridPane = createGridPane(store, poolSize, {
				forceFit : true
			}, false);

			var xlogsPagePane = new Ext.Panel({
				id : paneTitle,
				layout : 'fit',
				title : paneTitle,
				border : false,
				closable : true,
				items : [ gridPane ]
			});

			Ext.getCmp('content-pane').add(xlogsPagePane).show();
			Ext.getCmp('content-pane').getEl().unmask();
		});

		store.load({
			params : {
				start : 0,
				limit : poolSize
			}
		});
	}
}

function loadPageDto(type, action, context, label) {
	var tab = Ext.getCmp('content-pane').getItem('tab-' + label);

	if (tab != null) {
		tab.show();
	} else {
		var contentPane = Ext.getCmp('content-pane');
		contentPane.getEl().mask("Loading...", "x-mask-loading");

		var url = action + context;
		var store = createGridStore(contentPane, url);
		var pageSize = 25;

		store
				.on(
						'load',
						function() {
							if (Ext.getCmp('content-pane').getItem(
									'tab-' + label) == null) {
								var dtoBcPane = new Ext.Container(
										{
											id : 'bc-' + label,
											cls : 'bc-container',
											padding : '5',
											items : [ {
												xtype : 'box',
												autoEl : {
													tag : 'span',
													html : '<a class="breadcrumb" href="#" onclick="navigate(0)">'
															+ store.reader.description
															+ '</a>'
												}
											} ]
										});

								var dtoNavPane = new Ext.Panel({
									id : 'nav-' + label,
									region : 'north',
									layout : 'hbox',
									height : 26,
									items : [ dtoBcPane ]
								});

								if (type == 'exchange') {
									var dtoToolbar = new Ext.Toolbar(
											{
												cls : 'nav-toolbar',
												width : 80,
												items : [
														{
															id : 'tb-exchange',
															xtype : 'button',
															tooltip : 'Send data to target endpoint',
															icon : 'resources/images/16x16/exchange-send.png',
															handler : function() {
																var xidIndex = context
																		.indexOf('&xid=');
																var scope = context
																		.substring(
																				7,
																				xidIndex);
																var xid = context
																		.substring(xidIndex + 5);
																var msg = 'Are you sure you want to exchange data \r\n['
																		+ label
																		+ ']?';
																var processUserResponse = submitExchange
																		.createDelegate([
																				label,
																				scope,
																				xid,
																				true ]);
																showDialog(
																		460,
																		125,
																		'Exchange Confirmation',
																		msg,
																		Ext.Msg.OKCANCEL,
																		processUserResponse);
															}
														},
														{
															xtype : 'tbspacer',
															width : 4
														},
														{
															id : 'tb-xlog',
															xtype : 'button',
															tooltip : 'exchange results',
															icon : 'resources/images/16x16/history.png',
															handler : function() {
																ShowExchangeHistory()
															}
														},
														{
															xtype : 'tbspacer',
															width : 4
														},
														{
															id : 'tb-dup',
															xtype : 'button',
															tooltip : 'Show Pre-Exchange Summary Data',
															icon : 'resources/images/16x16/file-table.png',
															handler : function() {
																var contentPanel = Ext
																		.getCmp('content-pane');
																var dtoTab = contentPanel
																		.getActiveTab();

																contentPane
																		.getEl()
																		.mask(
																				"Loading...",
																				"x-mask-loading");

																Ext.Ajax
																		.request({
																			url : 'sdata'
																					+ context,
																			timeout : 120000,
																			success : function(
																					response) {
																				Ext
																						.getCmp(
																								'content-pane')
																						.getEl()
																						.unmask();
																				var summary = Ext
																						.decode(response.responseText);

																				var win = new Ext.Window(
																						{
																							title : 'Pre-Exchange Summary',
																							closable : true,
																							resizable : false,
																							modal : true,
																							layout : 'column',
																							width : 500,
																							defaults : {
																								border : false
																							},
																							items : [
																									{
																										xtype : 'panel',
																										columnWidth : .35,
																										frame : false,
																										defaults : {
																											height : 24,
																											border : false,
																											style : 'font-weight:bold;'
																										},
																										items : [
																												{
																													html : 'Sender Application:'
																												},
																												{
																													html : 'Sender Graph:'
																												},
																												{
																													html : 'Sender Endpoint:'
																												},
																												{
																													html : 'Receiver Application:'
																												},
																												{
																													html : 'Receiver Graph:'
																												},
																												{
																													html : 'Receiver Endpoint:'
																												},
																												{
																													html : 'Pool Size:'
																												},
																												{
																													html : 'Total Count:'
																												},
																												{
																													html : 'Adding Count:'
																												},
																												{
																													html : 'Changing Count:'
																												},
																												{
																													html : 'Deleting Count:'
																												},
																												{
																													html : 'Synchronizing Count:'
																												} ]
																									},
																									{
																										xtype : 'panel',
																										columnWidth : .65,
																										frame : false,
																										defaults : {
																											height : 24,
																											border : false
																										},
																										items : [
																												{
																													html : summary['SenderApplication']
																												},
																												{
																													html : summary['SenderGraph']
																												},
																												{
																													html : summary['SenderURI']
																												},
																												{
																													html : summary['ReceiverApplication']
																												},
																												{
																													html : summary['ReceiverGraph']
																												},
																												{
																													html : summary['ReceiverURI']
																												},
																												{
																													html : summary['PoolSize']
																												},
																												{
																													html : summary['TotalCount']
																												},
																												{
																													html : summary['AddingCount']
																												},
																												{
																													html : summary['ChangingCount']
																												},
																												{
																													html : summary['DeletingCount']
																												},
																												{
																													html : summary['SynchronizingCount']
																												} ]
																									} ]
																						});

																				win
																						.show();
																			},
																			failure : function(
																					response,
																					request) {
																				Ext
																						.getCmp(
																								'content-pane')
																						.getEl()
																						.unmask();
																				var message = 'Error getting exchange summary: '
																						+ response.statusText;
																				showDialog(
																						400,
																						100,
																						'Error',
																						message,
																						Ext.Msg.OK,
																						null);
																			}
																		});
															}
														} ]
											});

									dtoNavPane.insert(0, dtoToolbar);
								}

								var dtoContentPane = new Ext.Panel({
									id : 'dto-' + label,
									region : 'center',
									layout : 'card',
									border : false,
									activeItem : 0,
									items : [ createGridPane(store, pageSize, {
										forceFit : false
									}, true) ],
									listeners : {
										afterlayout : function(pane) {
											Ext.getCmp('content-pane').getEl()
													.unmask();
										}
									}
								});

								var dtoTab = new Ext.Panel({
									id : 'tab-' + label,
									title : label,
									type : type,
									context : context,
									layout : 'border',
									closable : true,
									items : [ dtoNavPane, dtoContentPane ],
									listeners : {
										close : function(panel) {
											Ext.Ajax.request({
												url : 'reset?dtoContext='
														+ escape(panel.context
																.substring(1))
											});
										}
									}
								});

								if (type == 'exchange') {
									var xlogsContainer = new Ext.Panel({
										id : 'xlogs-container-' + label,
										region : 'south',
										layout : 'fit',
										border : false,
										height : 294,
										split : true,
										collapsed : true
									});

									dtoTab.add(xlogsContainer);
								}

								Ext.getCmp('content-pane').add(dtoTab).show();
							}
						});

		store.load({
			params : {
				start : 0,
				limit : pageSize
			}
		});
	}
}

function loadRelatedItem(type, context, individual, classId, className) {
	var url = context + '&individual=' + individual + '&classId=' + classId;

	if (type == 'app') {
		url = 'radata' + url;
	} else {
		url = 'rxdata' + url;
	}

	var contentPane = Ext.getCmp('content-pane');
	contentPane.getEl().mask("Loading...", "x-mask-loading");

	var store = createGridStore(contentPane, url);
	var pageSize = 25;

	store
			.on(
					'load',
					function() {
						var dtoTab = Ext.getCmp('content-pane').getActiveTab();
						var label = dtoTab.id.substring(4);
						var dtoBcPane = dtoTab.items.map['nav-' + label].items.map['bc-'
								+ label];

						// remove old bc and content pane on refresh
						var lastBcItem = dtoBcPane.items.items[dtoBcPane.items.length - 1].autoEl.html;
						if (removeHTMLTag(lastBcItem) == className) {
							navigate(dtoBcPane.items.length - 3);
						}

						var dtoContentPane = dtoTab.items.map['dto-' + label];
						var bcItemIndex = dtoBcPane.items.length + 1;

						dtoBcPane
								.add(
										{
											xtype : 'box',
											autoEl : {
												tag : 'img',
												src : 'resources/images/breadcrumb.png'
											},
											cls : 'breadcrumb-img'
										},
										{
											xtype : 'box',
											autoEl : {
												tag : 'span',
												html : '<a class="breadcrumb" href="#" onclick="navigate('
														+ bcItemIndex
														+ ')">'
														+ className + '</a>'
											}
										});

						dtoBcPane.doLayout();
						dtoContentPane.add(createGridPane(store, pageSize, {
							forceFit : false
						}, true));
						dtoContentPane.getLayout().setActiveItem(
								dtoContentPane.items.length - 1);
					});

	store.load({
		params : {
			start : 0,
			limit : pageSize
		}
	});
}

function removeHTMLTag(htmlText) {
	if (htmlText)
		return htmlText.replace(/<\/?[^>]+(>|$)/g, '');

	return '';
}

function findChangedValue(htmlText) {
	if (htmlText) {
		var value = htmlText.replace(/<\/?[^>]+(>|$)/g, '');
		var index = value.indexOf('->');
		if (index == -1) {
			return '';
		} else
			return value;
	} else
		return '';
}

function FindTransferType(htmlText) {
	if (htmlText) {
		// var value = htmlText.replace(/<\/?[^>]+(>|$)/g, '');
		var splits = htmlText.split('/');
		var resultType = splits[2].split('.');
		var transferType = resultType[0];
		return transferType;
	}
}

function navigate(bcItemIndex) {
	var dtoTab = Ext.getCmp('content-pane').getActiveTab();
	var label = dtoTab.id.substring(4);
	var dtoBcPane = dtoTab.items.map['nav-' + label].items.map['bc-' + label];
	var dtoContentPane = dtoTab.items.map['dto-' + label];

	// remove items on the right from nav pane
	while (bcItemIndex < dtoBcPane.items.items.length - 1) {
		dtoBcPane.items.items[bcItemIndex + 1].destroy();
	}

	// remove items on the right from dto content pane
	var contentItemIndex = bcItemIndex / 2;
	while (contentItemIndex < dtoContentPane.items.items.length - 1) {
		dtoContentPane.items.items[contentItemIndex + 1].destroy();
	}
	dtoContentPane.getLayout().setActiveItem(contentItemIndex);
}

function showChangedItemsInfo() {
	var dtoTab = Ext.getCmp('content-pane').getActiveTab();
	var label = dtoTab.id.substring(4);
	// var dtoBcPane = dtoTab.items.map['nav-' + label].items.map['bc-' +
	// label];
	var dtoContentPane = dtoTab.items.map['dto-' + label];
	var dtoGrid = dtoContentPane.getLayout().activeItem;

	var rowData = dtoGrid.selModel.selections.map[dtoGrid.selModel.last].data;
	delete rowData['&nbsp;']; // remove info field
	var tansferType = {};
	var parsedRowData = {};
	for ( var colData in rowData) {
		if (colData == 'Transfer Type') {
			tansferType = FindTransferType(rowData[colData]);
		}
	}
	if (tansferType == 'change') {
		for ( var colData in rowData) {
			var value = findChangedValue(rowData[colData]);
			if (value != "")
				parsedRowData[colData] = value;
		}

		var propertyGrid = new Ext.grid.PropertyGrid({
			region : 'center',
			title : 'Properties of Changed Fields',
			split : true,
			stripeRows : true,
			autoScroll : true,
			source : parsedRowData,
			listeners : {
				beforeedit : function(e) {
					e.cancel = true;
				}
			}
		});

		var win = new Ext.Window({
			closable : true,
			resizable : true,
			// id: 'newwin-' + node.id,
			modal : true,
			// autoHeight:true,
			layout : 'fit',
			shadow : false,
			title : 'Transfer type of selected row is "'
					+ tansferType.toUpperCase() + '"',
			// iconCls: 'tabsApplication',
			height : 300,
			width : 600,
			plain : true,
			items : [ propertyGrid ]
		/*
		 * listeners: { beforelayout: function (pane) { //alert('before
		 * layout..'); Ext.getBody().unmask(); } }
		 */
		});
		win.show();
		dtoContentPane.add(win);
	} else {
		alert("Selected row is '" + tansferType.toUpperCase() + "'");
		dtoContentPane.add(alert);
	}
}

function showIndividualInfo(individual, classIdentifier, relatedClasses) {
	var dtoTab = Ext.getCmp('content-pane').getActiveTab();
	var label = dtoTab.id.substring(4);
	var dtoBcPane = dtoTab.items.map['nav-' + label].items.map['bc-' + label];
	var dtoContentPane = dtoTab.items.map['dto-' + label];
	var dtoGrid = dtoContentPane.getLayout().activeItem;

	var classItemPane = new Ext.Container(
			{
				region : 'north',
				layout : 'fit',
				height : 44,
				cls : 'class-badge',
				html : '<div style="width:50px;float:left"><img style="margin:2px 5px 2px 5px" src="resources/images/class-badge-large.png"/></div>'
						+ '<div style="width:100%;height:100%"><table style="height:100%"><tr><td>'
						+ dtoGrid.description
						+ ' ('
						+ classIdentifier
						+ ')</td></tr></table></div>'
			});

	var rowData = dtoGrid.selModel.selections.map[dtoGrid.selModel.last].data;
	delete rowData['&nbsp;']; // remove info field
	var colModelConfig = dtoGrid.selModel.grid.colModel.config;
	var parsedRowData = {};
	
	for ( var colData in rowData)
	{
		var propertyName = colData;
		//replace dataIndex with column headeR
		var len = colModelConfig.length;
		for (var i=0;i<len;i++)	{
			if (colModelConfig[i].dataIndex === colData){
				propertyName = colModelConfig[i].header;
				break;
			}
		}
		parsedRowData[propertyName] = removeHTMLTag(rowData[colData]);
	}
	var propertyGrid = new Ext.grid.PropertyGrid(
			{
				region : 'center',
				title : 'Properties',
				split : true,
				stripeRows : true,
				autoScroll : true,
				source : parsedRowData,
				listeners : {
					beforeedit : function(e) {
						e.cancel = true;
					},
					click : function() {
						// alert('clicked...');
					},
					keydown : function(evnt) {
						// alert('keydown...');
						var keyPressed = evnt.getKey();
						if (evnt.ctrlKey) {
							/*
							 * After trial and error, the ctrl+c combination
							 * seems to be code 67
							 */
							if (67 == 67)// if (keyPressed == 67)
							{
								var celldata = Ext.getCmp('property-pane')
										.getSelectionModel().events.beforecellselect.obj.selection.record.data.value;
								copyToClipboard(celldata);

							}
						}
					}
				}

			});

	var relatedItemPane = new Ext.Panel({
		title : 'Related Items',
		region : 'east',
		layout : 'vbox',
		boxMinWidth : 100,
		width : 300,
		padding : '4',
		split : true,
		autoScroll : true
	});

	for ( var i = 0; i < relatedClasses.length; i++) {
		var dtoTabType = dtoTab.type;
		var dtoTabContext = dtoTab.context;
		var dtoIdentifier = individual;
		var relatedClassId = relatedClasses[i].id;
		var relatedClassName = relatedClasses[i].name;

		relatedItemPane
				.add({
					xtype : 'box',
					autoEl : {
						tag : 'div',
						html : '<a class="breadcrumb" href="#" onclick="loadRelatedItem(\''
								+ dtoTabType
								+ '\',\''
								+ dtoTabContext
								+ '\',\''
								+ dtoIdentifier
								+ '\',\''
								+ relatedClassId
								+ '\',\''
								+ relatedClassName
								+ '\')">' + relatedClassName + '</a>'
					}
				});
	}

	var individualInfoPane = new Ext.Panel({
		autoWidth : true,
		layout : 'border',
		border : false,
		items : [ classItemPane, propertyGrid, relatedItemPane ]
	});

	var bcItemIndex = dtoBcPane.items.length + 1;

	dtoBcPane.add({
		xtype : 'box',
		autoEl : {
			tag : 'img',
			src : 'resources/images/breadcrumb.png'
		},
		cls : 'breadcrumb-img'
	}, {
		xtype : 'box',
		autoEl : {
			tag : 'span',
			html : '<a class="breadcrumb" href="#" onclick="navigate('
					+ bcItemIndex + ')">' + classIdentifier + '</a>'
		}
	});
	dtoBcPane.doLayout();

	dtoContentPane.add(individualInfoPane);
	dtoContentPane.getLayout().setActiveItem(dtoContentPane.items.length - 1);
}

function getFilters() {
	var dtoTab = Ext.getCmp('content-pane').getActiveTab();
	var label = dtoTab.id.substring(4);
	var dtoContentPane = dtoTab.items.map['dto-' + label];

	var gridFilters = new Array();

	for ( var i = 0; i < dtoContentPane.items.length; i = i + 2) {
		var gridFilter = dtoContentPane.items.items[i].plugins[0];
		var filterData = gridFilter.getFilterData();

		if (filterData.length > 0) {
			var filterQuery = gridFilter.buildQuery(filterData);
			gridFilters.push(filterQuery.filter);
		}
	}

	return gridFilters;
}

function submitExchange(userResponse) {
    var exchange = this[0];
    var scope = this[1];
    var xid = this[2];
    var reviewed = this[3];
    var exchtab = Ext.getCmp('content-pane').getItem('tab-' + exchange);

    var getProgressBoo = true;
    var runner = new Ext.util.TaskRunner();
    var task = {
        run: function () {
            if (getProgressBoo) {
                Ext.Ajax.request({
                    url: 'exchangeRequestStatus?scope=' + scope + '&xid=' + xid,
                    timeout: 5000,
                    method: 'GET',
                    success: function (response, options) {
                        var obj = Ext.decode(response.responseText);
                        var percentComplete = obj.percentComplete;
                        var message = obj.message;
                        Ext.MessageBox.updateProgress(percentComplete / 100, percentComplete + '% ' + message);
                        if (obj.state === 'COMPLETED' || obj.state === 'ERROR') {
                            getProgressBoo = false;
                            Ext.MessageBox.hide();
                        }
                    }
                });
            } else {
                // stop the TaskRunner when the 'getProgressBoo' is false
                runner.stop(task);
            }
        },
        interval: 1000 // monitor the progress every 1000 milliseconds
    };

	

	if (userResponse == 'ok') {
	    if (exchtab) {
	        Ext.MessageBox.show({
	            title: 'Exchange Status',
	            msg: 'Processing items...',
	            progressText: 'Initializing...',
	            width: 300,
	            progress: true,
	            closable: false
	        });
		}

		Ext.Ajax.request({
		    url: 'xsubmit?scope=' + scope + '&xid=' + xid + '&reviewed='
					+ reviewed,
		    timeout: 86400000, // 24 hours
		    success: function (response, request) {
		        if (exchtab) {
		            exchtab.getEl().unmask();
		        }

		        var responseText = Ext.decode(response.responseText);
		        var message = 'Data exchange [' + exchange + ']: '
						+ responseText;
		        /*
		        if (message.length < 300)
		        showDialog(460, 125, 'Exchange Result', message,
		        Ext.Msg.OK, null);
		        else
		        showDialog(660, 300, 'Exchange Result', message,
		        Ext.Msg.OK, null);
		        */
		        // start the TaskRunner
		      
		        runner.start(task);
		    },
		    failure: function (response, request) {
		        // ignore timeout error from proxy server
		        if (response.responseText.indexOf('Error Code 1460') != -1) {
		            if (exchtab) {
		                exchtab.getEl().unmask();
		            }
		            getProgressBoo = false;
		            var title = 'Exchange Error (' + response.status + ')';
		            var message = 'Error while exchanging [' + exchange + '].';

		            var responseText = Ext.decode(response.responseText);

		            if (responseText)
		                message += responseText;

		            showDialog(660, 300, title, message, Ext.Msg.OK, null);
		        }
		    }
		});
	}
}

function showDialog(width, height, title, message, buttons, callback) {
	var style = 'style="margin:0;padding:0;width:' + width + 'px;height:'
			+ height + 'px;border:1px solid #aaa;overflow:auto"';

	Ext.Msg.show({
		title : title,
		msg : '<textarea ' + style + ' readonly="yes">' + message
				+ '</textarea>',
		buttons : buttons,
		fn : callback
	});
}

function onTreeItemContextMenu(node, e) {
	var obj = node;
	var directoryTree = Ext.getCmp('directory-tree');
	var x = e.browserEvent.clientX;
	var y = e.browserEvent.clientY;

	if (node != null) {// var obj = node;
		if ((obj !== null)) {
			if (obj.parentNode !== null) {
				var dataTypeNode = obj.parentNode.text;
				if (obj.parentNode.parentNode !== null) {
					var dataExchangeNode = obj.parentNode.parentNode.text;
					if (dataExchangeNode !== null
							&& dataExchangeNode === 'Data Exchanges') {

						// var dataExchangeMenu =
						// Ext.widget('dataexchangemenu');
						commodityMenu.showAt([ x, y ]);
						e.stopEvent();
					}
				}

				if (dataTypeNode !== null && dataTypeNode === 'Data Exchanges') {

					// if (node.isSelected()) {

					// var obj = node.attributes;

					// if (obj.type == "ExchangeNode") {
					newExchangemenu.showAt([ x, y ]);
					e.stopEvent();
					// this.MenuClick(obj);
					// }
				} else if (obj.parentNode.text === 'Directory') {
					editDeleteScopeMenu.showAt([ x, y ]);
					e.stopEvent();
				} else if ((obj.text === 'Application Data')) {
					newappmenu.showAt([ x, y ]);
					e.stopEvent();
				} else if ((obj.parentNode.text === 'Application Data')) {
					applicationMenu.showAt([ x, y ]);
					e.stopEvent();
				} else if ((obj.parentNode.parentNode.text === 'Application Data')) {
					graphSubMenu.showAt([ x, y ]);
					e.stopEvent();

				} else if ((obj.text === 'Data Exchanges')) {
					newCommoditymenu.showAt([ x, y ]);
					e.stopEvent();
				}
			} else if (obj.text === 'Directory') {
				newScopemenu.showAt([ x, y ]);
				e.stopEvent();
			}

		}
	}
	directoryTree.getSelectionModel().select(node);
}

function buildEditDeleteSubMenu() {
	return [ {
		xtype : 'menuitem',
		handler : function(node, event) {
			newScope(node, event);
			editDeleteScope(node, event);
		},
		text : 'Edit Scope',
		icon : 'resources/images/16x16/document-properties.png'
		
	}, {
		xtype : 'menuitem',
		handler : function(node, event) {
			deleteScope(node, event);
		},
		text : 'Delete Scope',
		icon : 'resources/images/16x16/edit-delete.png'
	} ];
}

function buildNewCommodityMenu() {
	return [ {
		xtype : 'menuitem',
		handler : function(node, event) {

			newCommodity(node, event);
			var view = Ext.getCmp('newCommWin');
			view.show();
		},
		text : 'New Commodity'
	} ];
}

function deleteApp(node, event) {
	var node = Ext.getCmp('directory-tree').getSelectionModel()
			.getSelectedNode();
	var appName = node.text;
	var scope = node.parentNode.parentNode.text;

	var style = 'style="margin:0;padding:0;width:' + 400 + 'px;height:' + 60
			+ 'px;border:1px solid #aaa;overflow:auto"';
	Ext.Msg.show({
		title : 'Delete Application',
		msg : '<textarea ' + style + ' readonly="yes">'
				+ 'Are you sure to delete ' + appName + ' application?'
				+ '</textarea>',
		buttons : Ext.MessageBox.YESNO,
		modal : true,
		// icon: Ext.Msg.QUESTION,
		// buttons: Ext.MessageBox.OKCANCEL,
		// inputField: new IMS.form.DateField(),
		fn : function(buttonId, text) {
			// if (buttonId == 'ok')
			if (buttonId == 'yes') {
				var contentPanel = Ext.getCmp('content-pane');
				contentPanel.getEl().mask("Loading...", "x-mask-loading");
				Ext.Ajax.request({
					url : 'deleteApplication?' + '&scope =' + scope
							+ '&appName =' + appName,
					method : 'POST',
					timeout : 120000,
					success : function(response, request) {
						contentPanel.getEl().unmask();
						refresh();
					},
					failure : function(response, request) {
						contentPanel.getEl().unmask();
						Ext.Msg.show({
							title : 'Delete Application failed ',
							msg : '<textarea ' + style + ' readonly="yes">'
									+ 'Error: ' + response.reponseText
									+ '</textarea>',
							buttons : Ext.MessageBox.OKCANCEL
						});
					}
				});
			}
		}
	});
}

function deleteGraph(node, event) {
	var node = Ext.getCmp('directory-tree').getSelectionModel()
			.getSelectedNode();
	var name = node.text;
	var appName = node.parentNode.text;
	var scope = node.parentNode.parentNode.parentNode.text;

	var style = 'style="margin:0;padding:0;width:' + 400 + 'px;height:' + 60
			+ 'px;border:1px solid #aaa;overflow:auto"';
	Ext.Msg.show({
		title : 'Delete Graph',
		msg : '<textarea ' + style + ' readonly="yes">'
				+ 'Are you sure to delete ' + name + ' graph?' + '</textarea>',
		buttons : Ext.MessageBox.YESNO,
		modal : true,
		// icon: Ext.Msg.QUESTION,
		// buttons: Ext.MessageBox.OKCANCEL,
		// inputField: new IMS.form.DateField(),
		fn : function(buttonId, text) {
			// if (buttonId == 'ok')
			if (buttonId == 'yes') {
				var contentPanel = Ext.getCmp('content-pane');
				contentPanel.getEl().mask("Loading...", "x-mask-loading");
				Ext.Ajax.request({
					url : 'deleteGraph?' + '&name =' + name + '&scope ='
							+ scope + '&appName =' + appName,
					method : 'POST',
					timeout : 120000,
					success : function(response, request) {
						contentPanel.getEl().unmask();
						refresh();
					},
					failure : function(response, request) {
						contentPanel.getEl().unmask();
						Ext.Msg.show({
							title : 'Delete Graph failed ',
							msg : '<textarea ' + style + ' readonly="yes">'
									+ 'Error: ' + response.reponseText
									+ '</textarea>',
							buttons : Ext.MessageBox.OKCANCEL
						});
					}
				});
			}
		}
	});
}

function deleteCommodity(node, event) {
	var node = Ext.getCmp('directory-tree').getSelectionModel()
			.getSelectedNode();
	var commName = node.text;
	var scope = node.parentNode.parentNode.text;

	var style = 'style="margin:0;padding:0;width:' + 400 + 'px;height:' + 60
			+ 'px;border:1px solid #aaa;overflow:auto"';
	Ext.Msg.show({
		title : 'Delete Commodity',
		msg : '<textarea ' + style + ' readonly="yes">'
				+ 'Are you sure to delete ' + commName + ' commodity?'
				+ '</textarea>',
		buttons : Ext.MessageBox.YESNO,
		modal : true,
		// icon: Ext.Msg.QUESTION,
		// buttons: Ext.MessageBox.OKCANCEL,
		// inputField: new IMS.form.DateField(),
		fn : function(buttonId, text) {
			// if (buttonId == 'ok')
			if (buttonId == 'yes') {
				var contentPanel = Ext.getCmp('content-pane');
				contentPanel.getEl().mask("Loading...", "x-mask-loading");
				Ext.Ajax.request({
					url : 'deleteCommodity?' + '&commName =' + commName
							+ '&scope =' + scope,
					method : 'POST',
					timeout : 120000,
					success : function(response, request) {
						contentPanel.getEl().unmask();
						refresh();
					},
					failure : function(response, request) {
						contentPanel.getEl().unmask();
						Ext.Msg.show({
							title : 'Delete Commodity failed ',
							msg : '<textarea ' + style + ' readonly="yes">'
									+ 'Error: ' + response.reponseText
									+ '</textarea>',
							buttons : Ext.MessageBox.OKCANCEL
						});
					}
				});
			}
		}
	});
}

function deleteConfig() {
	var node = Ext.getCmp('directory-tree').getSelectionModel()
			.getSelectedNode();
	var exchangeConfigName = node.text;
	var commName = node.parentNode.text;
	var scope = node.parentNode.parentNode.parentNode.text;
	var xid = node.attributes.properties['Id'];

	var style = 'style="margin:0;padding:0;width:' + 400 + 'px;height:' + 60
			+ 'px;border:1px solid #aaa;overflow:auto"';
	Ext.Msg.show({
		title : 'Delete Exchange',
		msg : '<textarea ' + style + ' readonly="yes">'
				+ 'Are you sure to delete ' + '"' + exchangeConfigName + '"'
				+ ' exchange?' + '</textarea>',
		buttons : Ext.MessageBox.YESNO,
		modal : true,
		// icon: Ext.Msg.QUESTION,
		// buttons: Ext.MessageBox.OKCANCEL,
		// inputField: new IMS.form.DateField(),
		fn : function(buttonId, text) {
			// if (buttonId == 'ok')
			if (buttonId == 'yes') {
				var contentPanel = Ext.getCmp('content-pane');
				contentPanel.getEl().mask("Loading...", "x-mask-loading");
				Ext.Ajax.request({
					url : 'deleteExchangeConfig?' + '&scope =' + scope
							+ '&commName =' + commName + '&name ='
							+ exchangeConfigName + '&xid =' + xid,
					method : 'POST',
					timeout : 120000,
					success : function(response, request) {
						contentPanel.getEl().unmask();
						refresh();
					},
					failure : function(response, request) {
						contentPanel.getEl().unmask();
						Ext.Msg.show({
							title : 'Delete Exchange failed ',
							msg : '<textarea ' + style + ' readonly="yes">'
									+ 'Error: ' + response.reponseText
									+ '</textarea>',
							buttons : Ext.MessageBox.OKCANCEL
						});
					}
				});
			}
		}
	});
}

function buildNewExchangeMenu() {
	return [ {
		xtype : 'menuitem',
		handler : function(item, event) {
			newExchangeConfig();
			Ext.getCmp('sourceScopeCombolist').setDisabled(true);
			Ext.getCmp('sourceAppCombolist').setDisabled(true);
			Ext.getCmp('graphsourceName').setDisabled(true);
			
			Ext.getCmp('targetScopeCombolist').setDisabled(true);
			Ext.getCmp('targetAppCombolist').setDisabled(true);
			Ext.getCmp('targetGraph').setDisabled(true);
			
			var view = Ext.getCmp('newExchangeConfigWin');
			view.show();
		},
		text : 'New Exchange',
		icon : 'resources/images/16x16/document-new.png'
	}, {
		xtype : 'menuitem',
		handler : function(item, event) {
			newCommodity();
			editCommodity();
		},
		text : 'Edit Commodity',
		icon : 'resources/images/16x16/document-properties.png'
	}, {
		xtype : 'menuitem',
		handler : function(item, event) {
			deleteCommodity();
		},
		text : 'Delete commodity',
		icon : 'resources/images/16x16/edit-delete.png'
	} ];
}

function newExchangeConfig() {
	
	var ScopeNameStore = null;
	var AppNameStore = null;
	var newExchConfig = new Ext.FormPanel(
			{
				id : 'newExchConfig',
				frame : false,
				border : false,
				bodyStyle : 'padding:15px 15px 5px 15px',
				labelWidth : 100,
				defaults : {
					width : 390,
					allowBlank : false
				},
				defaultType : 'textfield',
				items : [
						{
							fieldLabel : 'Name',
							name : 'name'
						},
						{
							fieldLabel : 'Description',
							name : 'description',
							allowBlank : true
						},
						{
							xtype : 'fieldset',
							anchor : '100%',
							defaults : {
								width : 360,
								allowBlank : false
							},
							defaultType : 'textfield',
							title : 'Source Config',
							items : [
									{
										xtype : 'textfield',
										fieldLabel : 'Base URI',
										allowBlank : false,
										name : 'sourceUri',
										regex : /^(http[s]?:\/\/){0,1}(www\.){0,1}[a-zA-Z0-9\.\-]+\:[a-zA-Z0-9\/]{2,5}[\/]{0,1}/,
										regexText : 'Invalid URL Format (eg: http://localhost/services/dxfr)',
										vtypeText : 'Invalid URL Format (eg: http://localhost/services/dxfr)',
										listeners : {
											specialkey : function(f, e) {
												if((e.getKey() == e.TAB) || (e.getKey() == e.ENTER)) {
													testUriAndGetScopeName("SourceExchange");

												}
											},
											 render: function(c) {
												 c.getEl().on('keyup', function() {
													 Ext.getCmp('sourceScopeCombolist').clearValue();						
													 Ext.getCmp('sourceScopeCombolist').setDisabled(true);
													 Ext.getCmp('sourceAppCombolist').clearValue();							
													 Ext.getCmp('sourceAppCombolist').setDisabled(true);
													 Ext.getCmp('graphsourceName').setDisabled(true);
												      }, c);
											}
										}
									}, {
										xtype : 'combo',
										fieldLabel : 'Context',
										name : 'sourceScopeName',
										id: 'sourceScopeCombolist',
										allowBlank : false,
										store : ScopeNameStore,
										valueField : 'value',
										displayField : 'name',
										forceSelection : true,
										typeAhead : true,
										triggerAction : 'all',
										listeners : {
											scope : this,
											select : function() {
							//		Ext.getCmp('sourceAppCombolist').clearValue();
												getAppNames("SourceExchange");		
											},
											change:function(id,newvalue, oldvalue)
											{
												 Ext.getCmp('sourceAppCombolist').clearValue();
												 //getAppNames("app");
												Ext.getCmp('graphsourceName').setValue("");
											}
										}
									}, {
										xtype : 'combo',
										fieldLabel : 'Application',
										name : 'sourceAppName',
										id: 'sourceAppCombolist',
										store : AppNameStore,
										valueField : 'value',
										displayField : 'name',
										forceSelection : true,
										typeAhead : true,
										allowBlank : false,
										triggerAction : 'all',
										listeners : {
											scope : this,
											select : function() {									
												var appName = Ext.getCmp('sourceAppCombolist').getValue();
												var obj = Ext.getCmp('newExchConfig');
												var form = obj.getForm();
												form.findField('sourceGraphName').setDisabled(false);
											}}
									}, {
										fieldLabel : 'Graph',
										name : 'sourceGraphName',
										id:'graphsourceName'
									} ],
							buttons : [ {
								text : 'Test',
								handler : function(button, event) {
									SourceUri();
								}
							} ]
						}, {
							xtype : 'fieldset',
							anchor : '100%',
							title : 'Target Config',
							defaults : {
								width : 360,
								allowBlank : false
							},
							defaultType : 'textfield',
							items : [ {
								xtype : 'textfield',
								fieldLabel : 'Base URI',
								allowBlank : false,
								name : 'targetUri',
								regex : /^(http[s]?:\/\/){0,1}(www\.){0,1}[a-zA-Z0-9\.\-]+\:[a-zA-Z0-9\/]{2,5}[\/]{0,1}/,
								regexText : 'Invalid URL Format (eg: http://localhost/services/dxfr)',
								vtypeText : 'Invalid URL Format (eg: http://localhost/services/dxfr)',
								listeners : {
									specialkey : function(f, e) {
										if (e.getKey() == e.TAB) {
											testUriAndGetScopeName("TargetExchange");

										}
									},render: function(c) {
										 c.getEl().on('keyup', function() {
											 Ext.getCmp('targetScopeCombolist').clearValue();						
											 Ext.getCmp('targetScopeCombolist').setDisabled(true);
											 Ext.getCmp('targetAppCombolist').clearValue();							
											 Ext.getCmp('targetAppCombolist').setDisabled(true);
											 Ext.getCmp('targetGraph').setDisabled(true);
										      }, c);
									}
								}
							}, {
								    xtype : 'combo',
									fieldLabel : 'Context',
									name : 'targetScopeName',
									id: 'targetScopeCombolist',
									allowBlank : false,
									store : ScopeNameStore,
									valueField : 'value',
									displayField : 'name',
									forceSelection : true,
									typeAhead : true,
									triggerAction : 'all',
									listeners : {
										scope : this,
										select : function() {
								//	Ext.getCmp('targetAppCombolist').clearValue();
											getAppNames("TargetExchange");		
										},
										change:function(id,newvalue, oldvalue)
										{
											 Ext.getCmp('targetAppCombolist').clearValue();
											 //getAppNames("app");
											Ext.getCmp('targetGraph').setValue("");
										}
									}
								},  {
								    xtype : 'combo',
									fieldLabel : 'Application',
									name : 'targetAppName',
									id: 'targetAppCombolist',
									store : AppNameStore,
									valueField : 'value',
									displayField : 'name',
									forceSelection : true,
									typeAhead : true,
									allowBlank : false,
									triggerAction : 'all',
									listeners : {
										scope : this,
										select : function() {									
											var appName = Ext.getCmp('sourceAppCombolist').getValue();
											var obj = Ext.getCmp('newExchConfig');
											var form = obj.getForm();
											form.findField('targetGraphName').setDisabled(false);
										}}
								},  {
								fieldLabel : 'Graph',
								name : 'targetGraphName',
								id : 'targetGraph'
							} ],
							buttons : [ {
								text : 'Test',
								handler : function(button, event) {
									TargetUri();
								}
							} ]
						}, {
							xtype : 'hidden',
							fieldLabel : 'Label',
							name : 'oldConfigName'
						}, {
							xtype : 'hidden',
							fieldLabel : 'Label',
							name : 'oldCommName'
						}, {
							xtype : 'hidden',
							fieldLabel : 'Label',
							name : 'oldScope'
						} ],
				buttons : [ {
					text : 'Save',
					handler : function(button, event) {
						saveExchangeConfig();
				//newExchangeConfigWin.close();
					}
				}, {
					text : 'Cancel',
					handler : function(button, event) {
						newExchangeConfigWin.close();
					}
				} ]
			});

	var newExchangeConfigWin = new Ext.Window({
		id : 'newExchangeConfigWin',
		title : 'New Exchange',
		width : 540,
		height : 516,
		modal : true,
		resizable : false,
		items : [ newExchConfig ]
	});
}

function SourceUri() {
	var obj = Ext.getCmp('newExchConfig');
	var form = obj.getForm();
	var source = form.findField('sourceUri').getValue();
	var scope = form.findField('sourceScopeName').getValue();
	var app = form.findField('sourceAppName').getValue();
	var graph = form.findField('sourceGraphName').getValue();
	var sourceUri = source + "/" + scope + "/" + app + "/" + graph
			+ "/manifest";
	var style = 'style="margin:0;padding:0;width:' + 400 + 'px;height:' + 160
			+ 'px;border:1px solid #aaa;overflow:auto"';
	Ext.Ajax.request({
		url : 'SourcetestUri?' + '&sourceUri =' + sourceUri + '&graphName =' + graph, 
		method : 'POST',
		timeout : 120000,
		success : function(response, request) {
			var result = Ext.decode(response.responseText);

			Ext.Msg.show({
				title : 'Result ',
				msg : '<textarea ' + style + ' readonly="yes">'
						+ result.message + '</textarea>',
				buttons : Ext.MessageBox.OK
			});

		},
		failure : function(response, request) {
			Ext.Msg.show({
				title : 'Result ',
				msg : '<textarea ' + style + ' readonly="yes">'
						+ "failed to connect to the specified Url"
						+ '</textarea>',
				buttons : Ext.MessageBox.OK
			});
		}
	});
}

/*
 * function testBaseUri() { var obj = Ext.getCmp('newAppForm'); var form =
 * obj.getForm();
 * 
 * var baseUri = form.findField('baseUri').getValue(); var style =
 * 'style="margin:0;padding:0;width:' + 400 + 'px;height:' + 160 +
 * 'px;border:1px solid #aaa;overflow:auto"'; Ext.Ajax.request({ url :
 * 'BasetestUri?' + '&sourceUri =' + baseUri, method : 'POST', timeout : 120000,
 * success : function(response, request) { var result =
 * Ext.decode(response.responseText); if(result.success === false) {
 * Ext.Msg.show({ title : 'Result ', msg : '<textarea ' + style + '
 * readonly="yes">' + "Could not connect to the URL " + result.message + '</textarea>',
 * buttons : Ext.MessageBox.OK }); } if(result.success === true) {
 * Ext.Msg.show({ title : 'Result ', msg : '<textarea ' + style + '
 * readonly="yes">' + result.message + '</textarea>', buttons :
 * Ext.MessageBox.OK }); var obj = Ext.getCmp('newAppForm'); var form =
 * obj.getForm(); form.findField('appScope').setDisabled(false); } }, failure :
 * function(response, request) { Ext.Msg.show({ title : 'Result ', msg : '<textarea ' +
 * style + ' readonly="yes">' + "failed to connect to the specified Url" + '</textarea>',
 * buttons : Ext.MessageBox.OK }); } }); }
 */

function TargetUri() {
	var obj = Ext.getCmp('newExchConfig');
	var form = obj.getForm();
	var target = form.findField('targetUri').getValue();
	var scope = form.findField('targetScopeName').getValue();
	var app = form.findField('targetAppName').getValue();
	var graph = form.findField('targetGraphName').getValue();
	var targetUri = target + "/" + scope + "/" + app + "/" + graph
			+ "/manifest";
	var style = 'style="margin:0;padding:0;width:' + 400 + 'px;height:' + 160
			+ 'px;border:1px solid #aaa;overflow:auto"';
	Ext.Ajax.request({
		url : 'testTargetUrl?' + '&targetUri =' + targetUri+ '&graphName =' + graph,
		method : 'POST',
		timeout : 120000,
		success : function(response, request) {
			var result = Ext.decode(response.responseText);

			Ext.Msg.show({
				title : 'Result ',
				msg : '<textarea ' + style + ' readonly="yes">'
						+ result.message + '</textarea>',
				buttons : Ext.MessageBox.OK
			});
		},
		failure : function(response, request) {
			// alert("failed to connect to the specified Url");
			Ext.Msg.show({
				title : 'Result ',
				msg : '<textarea ' + style + ' readonly="yes">'
						+ "failed to connect to the specified Url"
						+ '</textarea>',
				buttons : Ext.MessageBox.OK
			});
		}
	});

}

function saveExchangeConfig() {
	var node = Ext.getCmp('directory-tree').getSelectionModel()
			.getSelectedNode();
	var obj = Ext.getCmp('newExchConfig').getForm();
	var form = obj.getValues(true);
	var scope = node.parentNode.parentNode.text;
	var commodity = node.text;
	var xid = node.attributes.properties['Id'];
	var oldscope = obj.findField('oldScope').getValue();
	

	//Validating
	var form1 = obj;
	var targetUri = form1.findField('targetUri').getValue();
	var targetScopeName = form1.findField('targetScopeName').getValue();
	var targetAppName = form1.findField('targetAppName').getValue();
	var targetGraphName = form1.findField('targetGraphName').getValue();
	var sourceUri = form1.findField('sourceUri').getValue();
	var sourceScopeName = form1.findField('sourceScopeName').getValue();
	var sourceAppName = form1.findField('sourceAppName').getValue();
	var sourceGraphName = form1.findField('sourceGraphName').getValue();
	var name = form1.findField('name').getValue();

	if(((targetAppName !== "") && (targetAppName !== null)) && ((targetScopeName !== "") && (targetScopeName !== null)) &&  ((targetUri !== "") && (targetUri !== null)) && ((targetGraphName !== "") && (targetGraphName !== null)) &&((sourceAppName !== "") && (sourceAppName !== null)) && ((sourceScopeName !== "") && (sourceScopeName !== null)) &&  ((sourceUri !== "") && (sourceUri !== null)) && ((sourceGraphName !== "") && (sourceGraphName !== null)) && ((name !== "") && (name !== null)))                                             
	{

	Ext.Ajax.request({
		url : 'newExchange?' + form + '&scope =' + scope + '&commodity ='
				+ commodity + '&xid =' + xid,
		method : 'POST',
		timeout : 120000,
		success : function(response, request) {
			refresh();
			Ext.getCmp('newExchangeConfigWin').close();
		},
		failure : function(response, request) {
			alert("save failed");
		}
	});
	}
	else{    
	    if((targetAppName === ""))
	    {
	        form1.findField('targetAppName').markInvalid('App Name cannot be Empty');   
	    }

	    if((targetScopeName === ""))
	    {
	        form1.findField('targetScopeName').markInvalid('Scope Name cannot be Empty');   
	    }
	    if((targetUri === ""))
	    {
	        form1.findField('targetUri').markInvalid('Scope Uri cannot be Empty');   
	    }
	    if((targetGraphName === ""))
	    {
	        form1.findField('targetGraphName').markInvalid('Graph Name cannot be Empty');   
	    }
	    if((sourceAppName === ""))
	    {
	        form1.findField('sourceAppName').markInvalid('App Name cannot be Empty');   
	    }

	    if((sourceScopeName === ""))
	    {
	        form1.findField('sourceScopeName').markInvalid('Scope Name cannot be Empty');   
	    }
	    if((sourceUri === ""))
	    {
	        form1.findField('sourceUri').markInvalid('Scope Uri cannot be Empty');   
	    }
	    if((sourceGraphName === ""))
	    {
	        form1.findField('sourceGraphName').markInvalid('Graph Name cannot be Empty');   
	    }
	    if(name === "")
	    {
	        form1.findField('name').markInvalid('Name cannot be Empty');   
	    }
	}
}

function buildNewScopeMenu() {
	return {
		xtype : 'menuitem',
		handler : function(item, event) {
			newScope();

			var view = Ext.getCmp('newScopeWin');
			var value = Ext.getCmp('newScopeForm').getForm().findField(
					"oldScope").setValue('null');
			view.show();
		},
		text : 'New Scope',
		icon: 'resources/images/16x16/document-new.png'
	};
}

function buildNewApplicationMenu() {
	return {
		xtype : 'menuitem',
		handler : function(item, event) {
			newApp();
			var view = Ext.getCmp('newAppWin');
			var obj = Ext.getCmp('newAppForm');
			var form = obj.getForm();
			form.findField('appScope').setDisabled(true);
			// form.findField('scopeDisplayName').setDisabled(true);
			form.findField('appName').setDisabled(true);
			form.findField('displayName').setDisabled(true);
			form.findField('appDesc').setDisabled(true);
			view.show();
		},
		text : 'New Application',
		icon : 'resources/images/16x16/document-new.png'
	};
}

function buildGraphSubMenu() {
	return [ {
		xtype : 'menuitem',
		text : 'Edit Graph',
		icon : 'resources/images/16x16/document-properties.png',
		handler : function() {
			newGraph();
			editGraph();
		}
	}, {
		xtype : 'menuitem',
		text : 'Delete Graph',
		icon : 'resources/images/16x16/edit-delete.png',
		handler : function(item, event) {
			deleteGraph();
		}
	}, {
		xtype : 'menuseparator'
	}, {
		xtype : 'menuitem',
		text : 'Refresh Cache',
		icon : 'resources/images/16x16/view-refresh.png',
		handler : onRefreshCache,
		scope : this
	}, {
		xtype : 'menuitem',
		text : 'Import Cache',
		icon : 'resources/images/import.jpg',
		handler : onImportCache,
		scope : this
	} ];
}

function editCommodity() {
	var centerPanel = Ext.getCmp('content-pane');
	centerPanel.getEl().mask("Loading...", "x-mask-loading");
	var node = Ext.getCmp('directory-tree').getSelectionModel()
			.getSelectedNode();
	var scope = node.parentNode.parentNode.text;
	var commNameValue = node.text;

	var view = Ext.getCmp('newCommWin');
	view.setTitle("Edit Commodity");
	var obj = Ext.getCmp('newCommForm');
	var form = obj.getForm();

	Ext.Ajax.request({
		url : 'getComm?' + '&scope =' + scope + '&commName =' + commNameValue,
		method : 'POST',
		timeout : 120000,
		success : function(response, request) {
			var comDetails = Ext.decode(response.responseText);
			form.setValues({
				commName : comDetails.name,
				oldCommName : commNameValue,
				oldScope : scope
			});
			centerPanel.getEl().unmask();
			view.show();
		},
		failure : function(response, request) {
			centerPanel.getEl().unmask();
			alert("Error fetching data to fill form");
		}
	});
}

function editGraph() {
	var centerPanel = Ext.getCmp('content-pane');
	centerPanel.getEl().mask("Loading...", "x-mask-loading");
	var node = Ext.getCmp('directory-tree').getSelectionModel()
			.getSelectedNode();
	var scope = node.parentNode.parentNode.parentNode.text;
	var graphValue = node.text;
	var appNameValue = node.parentNode.text;

	var view = Ext.getCmp('newGraphWin');
	view.setTitle("Edit Graph");
	var obj = Ext.getCmp('newGraphForm');
	var form = obj.getForm();

	Ext.Ajax.request({
		url : 'getGraph?' + '&scope =' + scope + '&appName =' + appNameValue
				+ '&name =' + graphValue,
		method : 'POST',
		timeout : 120000,
		success : function(response, request) {
			var graph = Ext.decode(response.responseText);
			form.setValues({
				name : graph.name,
				description : graph.description,
				oldAppName : appNameValue,
				oldScope : scope,
				oldGraphName : graphValue
			/* CommName: graph.commodity */
			});
			centerPanel.getEl().unmask();
			view.show();
		},
		failure : function(response, request) {
			centerPanel.getEl().unmask();
			alert("Error fetching data to fill form");
		}
	});

}

function newGraph() {
	var newGraphForm = new Ext.FormPanel({
		id : 'newGraphForm',
		border : false,
		layout : 'form',
		frame : false,
		bodyStyle : 'padding:15px',
		labelwidth : 75,
		items : [ {
			xtype : 'textfield',
			width : 240,
			fieldLabel : 'Graph Name',
			name : 'name'
		}, {
			xtype : 'textfield',
			width : 240,
			fieldLabel : 'Description',
			name : 'description'
		},
		/*
		 * { xtype: 'textfield', anchor: '95%', fieldLabel: 'Commodity', name:
		 * 'CommName' },
		 */
		{
			xtype : 'hidden',
			anchor : '100%',
			fieldLabel : 'Label',
			name : 'oldScope'
		}, {
			xtype : 'hidden',
			anchor : '100%',
			fieldLabel : 'Label',
			name : 'oldAppName'
		}, {
			xtype : 'hidden',
			anchor : '100%',
			fieldLabel : 'Label',
			name : 'oldGraphName'
		} ],
		buttons : [ {
			text : 'Save',
			handler : function(node, button, event) {
				saveGraph();
				newGraphWin.close();
			}
		// margin: 10,
		}, {
			text : 'Cancel',
			handler : function(button, event) {
				newGraphWin.close();
			}
		} ]
	});

	var newGraphWin = new Ext.Window({
		id : 'newGraphWin',
		resizable : false,
		height : 150,
		width : 417,
		layout : {
			type : 'fit'
		},
		title : 'New Graph',
		modal : true,
		items : [ newGraphForm ]

	});
}

function saveGraph() {
	var me = this;
	var obj = Ext.getCmp('newGraphForm').getForm();
	var form = obj.getValues(true);
	var directoryTree = Ext.getCmp('directory-tree');
	var node = directoryTree.getSelectionModel().getSelectedNode();
	var scope = node.parentNode.parentNode.text;
	var appName = node.text;
	var graph = obj.findField('name').getValue();
	var oldapp = obj.findField('oldAppName').getValue();
	var oldScope = obj.findField('oldScope').getValue();

	Ext.Ajax.request({
		url : 'newGraph?' + form + '&scope =' + scope + '&appName =' + appName,
		method : 'POST',
		timeout : 120000,
		success : function(response, request) {
			var result = Ext.decode(response.responseText);
			if (result === 'ERROR') {
				if ((oldScope == '') && (oldapp == '')) {
					oldScope = scope;
					oldapp = appName;
				}
				var message = 'Graph name ' + '"' + graph + '"'
						+ ' already exists in scope ' + '["' + oldScope + '.'
						+ oldapp + '"]' + '.';

				showDialog(400, 90, 'Error', message, Ext.Msg.OK, null);

			}
			refresh();
		},
		failure : function(response, request) {
			alert("save failed");
		}
	});
}

function buildApplicationSubMenu() {
	return [ {
		xtype : 'menuitem',
		handler : function() {
			newGraph();
			var view = Ext.getCmp('newGraphWin');
			view.show();
		},
		text : 'Add Graph',
		icon : 'resources/images/16x16/document-new.png'
	}, {
		xtype : 'menuseparator'
	}, {
		xtype : 'menuitem',
		handler : function() {
			newApp();
			var obj = Ext.getCmp('newAppForm');
			var form = obj.getForm();
			// form.findField('scopeDisplayName').setDisabled(true);
			editApplication();
		},
		text : 'Edit Application',
		icon : 'resources/images/16x16/document-properties.png'
	}, {
		xtype : 'menuitem',
		handler : function() {
			deleteApp();
		},
		text : 'Delete Application',
		icon : 'resources/images/16x16/edit-delete.png'
	} ];
}

function onRefreshCache(btn, ev) {
	var node = Ext.getCmp('directory-tree').getSelectionModel()
			.getSelectedNode();
	Ext.getCmp('content-pane').getEl().mask("Processing...", "x-mask-loading");

	Ext.Ajax
			.request({
				url : 'refreshCache',
				method : 'POST',
				timeout : 3600000,
				params : {
					'dxfrUri' : node.attributes.properties['Base URI'],
					'scope' : node.attributes.properties['Context'],
					'app' : node.parentNode.text,
					'graph' : node.attributes.properties['Name']
				},
				success : function(response, request) {
					Ext.getCmp('content-pane').getEl().unmask();

					var responseObj = Ext.decode(response.responseText);

					if (responseObj.level == 'SUCCESS') {
						showDialog(450, 100, 'Refresh Cache Result',
								'Cache refreshed successfully.', Ext.Msg.OK,
								null);
					} else {
						showDialog(500, 160, 'Refresh Cache Error',
								responseObj.messages.items.join('\n'),
								Ext.Msg.OK, null);
					}
				},
				failure : function(response, request) {
					Ext.getCmp('content-pane').getEl().unmask();

					if (request.response.status == 200) {
						var responseObj = Ext
								.decode(request.response.responseText);

						if (responseObj.level == 'SUCCESS') {
							showDialog(450, 100, 'Refresh Cache Result',
									'Cache refreshed successfully.',
									Ext.Msg.OK, null);
						} else {
							showDialog(500, 160, 'Refresh Cache Error',
									responseObj.messages.items.join('\n'),
									Ext.Msg.OK, null);
						}
					} else {
						var errMsg = 'Failure Type: ' + request.failureType
								+ '. Status text: '
								+ request.response.statusText + '.';
						showDialog(500, 160, 'Refresh Cache Error', errMsg,
								Ext.Msg.OK, null);
					}
				}
			});
}

function onImportCache(btn, ev) {
	var node = Ext.getCmp('directory-tree').getSelectionModel()
			.getSelectedNode();

	var importCacheForm = new Ext.FormPanel({
		url : 'importCache',
		timeout : 1200000,
		method : 'POST',
		frame : false,
		border : false,
		bodyStyle : 'padding:20px 5px 20px 5px',
		items : [ {
			fieldLabel : 'Cache URI',
			name : 'cacheUri',
			xtype : 'textfield',
			width : 360,
			allowBlank : false
		}, {
			name : 'dxfrUri',
			xtype : 'hidden',
			value : node.attributes.properties['Base URI']
		}, {
			name : 'scope',
			xtype : 'hidden',
			value : node.attributes.properties['Context']
		}, {
			name : 'app',
			xtype : 'hidden',
			value : node.parentNode.text
		}, {
			name : 'graph',
			xtype : 'hidden',
			value : node.attributes.properties['Name']
		} ]
	});

	var win = new Ext.Window(
			{
				title : 'Import Cache',
				width : 500,
				modal : true,
				closable : true,
				resizable : false,
				items : [ importCacheForm ],
				buttons : [
						{
							text : 'Submit',
							handler : function() {
								importCacheForm.getForm().getEl().mask(
										"Processing...", "x-mask-loading");

								importCacheForm
										.getForm()
										.submit(
												{
													success : function(
															response, request) {
														importCacheForm
																.getForm()
																.getEl()
																.unmask();

														var responseObj = Ext
																.decode(response.responseText);

														if (responseObj.level == 'SUCCESS') {
															win.close();
															showDialog(
																	450,
																	100,
																	'Import Cache Result',
																	'Cache imported successfully.',
																	Ext.Msg.OK,
																	null);
														} else {
															showDialog(
																	500,
																	160,
																	'Import Cache Error',
																	responseObj.messages.items
																			.join('\n'),
																	Ext.Msg.OK,
																	null);
														}
													},
													failure : function(
															response, request) {
														importCacheForm
																.getForm()
																.getEl()
																.unmask();

														if (request.response.status == 200) {
															var responseObj = Ext
																	.decode(request.response.responseText);

															if (responseObj.level == 'SUCCESS') {
																win.close();
																showDialog(
																		450,
																		100,
																		'Import Cache Result',
																		'Cache imported successfully.',
																		Ext.Msg.OK,
																		null);
															} else {
																showDialog(
																		500,
																		160,
																		'Import Cache Error',
																		responseObj.messages.items
																				.join('\n'),
																		Ext.Msg.OK,
																		null);
															}
														} else {
															var errMsg = 'Failure Type: '
																	+ request.failureType
																	+ '. Status text: '
																	+ request.response.statusText
																	+ '.';
															showDialog(
																	500,
																	160,
																	'Import Cache Error',
																	errMsg,
																	Ext.Msg.OK,
																	null);
														}
													}
												});
							}
						}, {
							text : 'Close',
							handler : function() {
								win.close();
							}
						} ]
			});

	win.show(this);
}

function buildManifestMenu(scope, xid, isDeleted) {
	if (isDeleted === 'false') {
		return [ {
			xtype : 'menuitem',
			handler : function() {
				deleteTemplate(scope, xid);
			},
			text : 'Exclude Template',
			icon : 'resources/images/list-remove.png'
		} ];
	} else {
		return [ {
			xtype : 'menuitem',
			handler : function() {
				includeTemplate(scope, xid);
			},
			text : 'Include Template',
			icon : 'resources/images/list-add.png'
		} ];
	}
}

function editApplication() {
	var centerPanel = Ext.getCmp('content-pane');
	centerPanel.getEl().mask("Loading...", "x-mask-loading");
	var node = Ext.getCmp('directory-tree').getSelectionModel()
			.getSelectedNode();

	var scope = node.parentNode.parentNode.text;
	var appNameValue = node.text;

	var view = Ext.getCmp('newAppWin');
	view.setTitle("Edit Application");

	Ext.Ajax.request({
		url : 'getApplication?' + '&scope =' + scope + '&appName ='
				+ appNameValue,
		method : 'POST',
		timeout : 120000,
		success : function(response, request) {
			var application = Ext.decode(response.responseText);
			/*
			 * form.setValues({ appName : application.name,
			 * appDisplayName:application.appDisplayName, appDesc :
			 * application.description, appScope : application.context,
			 * scopeDisplayName : application.scopeDisplayName, baseUri :
			 * application.baseUri, oldAppName : appNameValue, oldScope : scope
			 * });
			 */
			var obj = Ext.getCmp('newAppForm');
			var form = obj.getForm();
			form.findField('baseUri').setValue(application.baseUri);
			ScopeNameStore = new Ext.data.ArrayStore({

				url : 'getAMScope?' + '&sourceUri =' + application.baseUri,
				autoDestroy : true,
				autoLoad : false,
				fields : [ {
					name : 'value'
				}, {
					name : 'name'
				} ]

			});
			Ext.getCmp('scopeCombolist').bindStore(ScopeNameStore);
			// form.findField('appScope').setValue(application.context);
			Ext.getCmp('scopeCombolist').setValue(application.context);
			// form.findField('scopeDisplayName').setValue(application.scopeDisplayName);
			AppNameStore = new Ext.data.ArrayStore({

				url : 'getAppNames?' + '&appScope =' + application.context
						+ '&sourceUri =' + application.baseUri,
				autoDestroy : true,
				autoLoad : false,
				fields : [ {
					name : 'value'
				}, {
					name : 'name'
				} ]

			});
			Ext.getCmp('appCombolist').bindStore(AppNameStore);
			// form.findField('appName').setValue(application.name);
			Ext.getCmp('appCombolist').setValue(application.name);
			form.findField('displayName').setValue(application.displayName);
			form.findField('appDesc').setValue(application.description);
			form.findField('oldAppName').setValue(application.displayName);
			form.findField('oldScope').setValue(scope);
			centerPanel.getEl().unmask();
			view.show();
		},
		failure : function(response, request) {
			centerPanel.getEl().unmask();
			alert("Error fetching data to fill form");
		}
	});
}

function buildCommoditySubMenu() {
	return [
			{
				xtype : 'menuitem',
				handler : function() {
					newExchangeConfig();
					editExchangeConfig();
				},
				text : 'Edit Exchange',
				icon : 'resources/images/16x16/document-properties.png'
			},
			{
				xtype : 'menuitem',
				handler : function() {
					deleteConfig();
				},
				text : 'Delete Exchange',
				icon : 'resources/images/16x16/edit-delete.png'
			},			 
			{
				xtype : 'menuitem',
				handler : function() {
					var node = Ext.getCmp('directory-tree').getSelectionModel()
							.getSelectedNode();
					var scope = node.parentNode.parentNode.parentNode.text;
					var xid = node.attributes.properties['Id'];

					var url = 'getColumnNames?' + '&scope =' + scope + '&xid='
							+ xid;

					applyDataFilter(url);
					commodityMenu.setVisible(false);

					var view = Ext.getCmp('applyDataFilterWin');
					view.show();
					editDataFilter();
				},
				text : 'Configure Exchange Filter',
				icon : 'resources/images/16x16/cog_edit.png'
			},{
				xtype : 'menuitem',
				handler : function() {
					ConfigureManifest();
				},
				text : 'Modify Exchange',
				icon : 'resources/images/16x16/document-properties.png'
			} ,{
				xtype: 'menuitem',
				handler : function() {
					ShowExchangeHistory();
				},
              icon:  'resources/images/16x16/history.png',
                text: 'Show History'
			}
			];
}
function ShowExchangeHistory()
{
	var me = this;
	var contentPanel =  Ext.getCmp('content-pane');
	var activeTab = contentPanel.getActiveTab();
	var directoryTree  = Ext.getCmp('directory-tree');
	var node =  directoryTree.getSelectionModel().getSelectedNode();
	var scope = node.parentNode.parentNode.parentNode.text;
	var xid = node.attributes.properties.Id;
	var xlabel = node.text;
	
	var reviewed = true;
	var xlogsContainer;
	var existingXlogCont;
	if(activeTab === undefined || (activeTab === null))
	{
		existingXlogCont = contentPanel.items.map['xlogpanel-'];
	}else{
		existingXlogCont = contentPanel.items.map['xlogpanel-'+activeTab.title];	
	}

	
	if (existingXlogCont == undefined || existingXlogCont.items.length == 0) {	
		  var url = 'xlogs?&scope=' +scope+ '&xid='+xid +'&xlabel=' + xlabel;
		  var store = createGridStore(contentPanel, url);

			store.on('load', function() {
				/*	var gridPane = createGridPane(store, {
					forceFit : true
				}, true);*/
				var colModel = new Ext.grid.DynamicColumnModel(store);
				var selModel = new Ext.grid.RowSelectionModel({
					singleSelect : true
				});
				var gridPane = new Ext.grid.GridPanel({
					identifier : store.reader.identifier,
					description : store.reader.description,
					layout : 'fit',
					minColumnWidth : 80,
					val : null,
					loadMask : true,
					store : store,
					cm : colModel,
					selModel : selModel,
					stripeRows : true,
					enableColLock : false,
				});
				  var xloggridpanel = new Ext.Panel({
						 split: true,
						 layout: {
						        type: 'fit'
						    },
						 id: 'XlogGridPanel',
			            enableColumnHide: false,
			            items: gridPane,
						viewConfig: {
			           enableTextSelection: true
			       },
					});
				  var xlogpanel = new Ext.Panel({
						 region: 'south',
						    layout: {
						        type: 'fit'
						    },
						    header: false,
						    id: 'xlogpanel',
						    items: [xloggridpanel],
						    dockedItems: [
						                  {
						                      xtype: 'toolbar',
						                      dock: 'top',
						                      items: [
						                          {
						                              xtype: 'label',
						                              height: 13,
						                              width: 85,
						                              text: 'Exchange Results'
						                          },
						                          {
						                              xtype: 'button',
						                              action: 'refreshHistory',
						                              icon: 'resources/images/16x16/view-refresh.png',
						                              text: '',
						                              tooltip: 'Refresh'
						                          }
						                      ]
						                  }
						              ]
					});
				  xlogpanel.closable = true;
			     //   xlogPanel.id = 'xlogpanel-'+activeTab.title;
			        var label = '(Exchange Result)'+'('+scope +')' +xlabel;
			        xlogpanel.setTitle(label);
			        xlogpanel.add(xloggridpanel);
			        var myTab = contentPanel.add(xlogpanel);
			        contentPanel.setActiveTab(myTab);
			});
				
			store.load({});
			store.on('exception',function( store, records, options ){
				 var msg = 'No History found for this Exchange';
				    showDialog(400, 200, 'Error', msg, Ext.Msg.OK, null);
			},this);
	 
	
	} 
	else {
	    existingXlogCont.show();
	
	}

}

function ConfigureManifest() {
	var node = Ext.getCmp('directory-tree').getSelectionModel()
			.getSelectedNode();
	var scope = node.parentNode.parentNode.parentNode.text;
	var xid = node.attributes.properties['Id'];

	// var label = "MappingTestTabPanel";
	var label = 'Manifest(' + scope + ')' + node.text;

	var tab = Ext.getCmp('content-pane').getItem('tab-' + label);

	if (tab != null) {
		tab.show();
	} else {
		// var contentPane = Ext.getCmp('content-pane');
		// contentPane.getEl().mask("Loading...", "x-mask-loading");
		var manifestTreePaneId = 'crossedManifest-tree(' + scope + ')' + xid;

		var manifestTreePane = new Ext.tree.TreePanel({
			// id : 'crossedManifest-tree',
			id : manifestTreePaneId,
			region : 'center',
			dataUrl : 'crossedManifest?scope=' + scope + '&xid=' + xid,
			width : 800,
			lines : true,
			autoScroll : true,
			border : false,
			animate : true,
			enableDD : false,
			containerScroll : true,
			rootVisible : false,
			tbar : new Ext.Toolbar({
				items : [ {
					// id : 'Reload-manifest-button',
					xtype : 'button',
					icon : 'resources/images/16x16/view-refresh.png',
					text : 'Reload',
					handler : function() {
						reloadManifest(scope, xid);
					}
				}, {
					// id : 'Reset-manifest-button',
					xtype : 'button',
					icon : 'resources/images/16x16/reset-icon.png',
					text : 'Reset',
					handler : function() {
						resetManifest(scope, xid);
					}
				}, {
					// id : 'Save-manifest-button',
					xtype : 'button',
					icon : 'resources/images/16x16/save-icon.png',
					text : 'Save',
					handler : function() {
						saveManifest(scope, xid);
					}
				} ]
			}),
			root : {
				nodeType : 'async',
				text : 'Mapping',
				icon : 'resources/images/directory.png'
			},
			listeners : {
				click : function(node, event) {
				},
				dblclick : function(node, event) {
				},
				contextmenu : function(node, event) {
					onCrossedManifestTreeItemContextMenu(node, event, scope,
							xid);
				},
				keydown : function(evnt) {
				}
			}

		});

		manifestTreePane.loader.on('loadexception', function(treePanel, node,
				response) {
			if (!(response.status == 0 || response.status == 408)) {
				// container.getEl().unmask();
				var message = 'Request URL: /' + treePanel.dataUrl
						+ '.\n\nError description: ' + response.responseText;
				showDialog(500, 240, 'Error', message, Ext.Msg.OK, null);
			}
		});

		if (Ext.getCmp('content-pane').getItem('tab-' + label) == null) {
			var mappingTab = new Ext.Panel({
				id : 'tab-' + label,
				title : label,
				// type : type,
				// context : context,
				layout : 'border',
				closable : true,
				items : [ manifestTreePane ],
				listeners : {}
			});

			Ext.getCmp('content-pane').add(mappingTab).show();
		}

	}
}

function onCrossedManifestTreeItemContextMenu(node, event, scope, xid) {
	// if (node.isSelected()) {

	// var scope = directoryNode.parentNode.parentNode.parentNode.text;
	// var xid = directoryNode.attributes.properties['Id'];

	var manifestTreePaneId = 'crossedManifest-tree(' + scope + ')' + xid;
	var manifestTree = Ext.getCmp(manifestTreePaneId);
	// var manifestTree = Ext.getCmp('crossedManifest-tree')

	var x = event.browserEvent.clientX;
	var y = event.browserEvent.clientY;

	var obj = node.attributes;
	var isDeleted = node.attributes.properties['IsDeleted'];

	// alert(obj.type);
	if (obj.type == "TemplateNode") {
		var isParentDeleted = node.parentNode.attributes.properties['IsDeleted'];
		if (isParentDeleted == 'false') {
			manifestMenu = new Ext.menu.Menu();
			manifestMenu.add(this.buildManifestMenu(scope, xid, isDeleted));
			manifestMenu.showAt([ x, y ]);
			event.stopEvent();
		}
	}
	manifestTree.getSelectionModel().select(node);

}

function resetManifest(scope, xid) {
	var centerPanel = Ext.getCmp('content-pane');
	centerPanel.getEl().mask("Loading...", "x-mask-loading");
	Ext.Ajax.request({
		url : 'resetCrossedManifest?' + '&scope =' + scope + '&xid =' + xid,
		method : 'POST',
		timeout : 120000,
		success : function(response, request) {
			var application = Ext.decode(response.responseText);
			centerPanel.getEl().unmask();
			refreshManifestTree(scope, xid);
		},
		failure : function(response, request) {
			centerPanel.getEl().unmask();
			var message = 'Error in rest manifest: ' + response.statusText
					+ '.\n\nError description: ' + response.responseText;
			showDialog(500, 240, 'Error', message, Ext.Msg.OK, null);
		}
	});
}

function reloadManifest(scope, xid) {
	var centerPanel = Ext.getCmp('content-pane');
	centerPanel.getEl().mask("Loading...", "x-mask-loading");
	Ext.Ajax.request({
		url : 'reloadCrossedManifest?' + '&scope =' + scope + '&xid =' + xid,
		method : 'POST',
		timeout : 120000,
		success : function(response, request) {
			var application = Ext.decode(response.responseText);
			centerPanel.getEl().unmask();
			refreshManifestTree(scope, xid);
		},
		failure : function(response, request) {
			centerPanel.getEl().unmask();
			var message = 'Error in reload manifest: ' + response.statusText
					+ '.\n\nError description: ' + response.responseText;
			showDialog(500, 240, 'Error', message, Ext.Msg.OK, null);
		}
	});
}

function saveManifest(scope, xid) {
	var centerPanel = Ext.getCmp('content-pane');
	centerPanel.getEl().mask("Loading...", "x-mask-loading");
	Ext.Ajax.request({
		url : 'saveCrossedManifest?' + '&scope =' + scope + '&xid =' + xid,
		method : 'POST',
		timeout : 120000,
		success : function(response, request) {
			var application = Ext.decode(response.responseText);
			centerPanel.getEl().unmask();
			showDialog(400, 90, 'Success', 'Manifest saved successfully.',
					Ext.Msg.OK, null);
		},
		failure : function(response, request) {
			centerPanel.getEl().unmask();
			var message = 'Error in save manifest: ' + response.statusText
					+ '.\n\nError description: ' + response.responseText;
			showDialog(500, 240, 'Error', message, Ext.Msg.OK, null);
		}
	});
}

function deleteTemplate(scope, xid) {
	var centerPanel = Ext.getCmp('content-pane');
	centerPanel.getEl().mask("Loading...", "x-mask-loading");

	// var directorynode =
	// Ext.getCmp('directory-tree').getSelectionModel().getSelectedNode();

	// var scope = directorynode.parentNode.parentNode.parentNode.text;
	// var xid = directorynode.attributes.properties['Id'];

	var manifestTreePaneId = 'crossedManifest-tree(' + scope + ')' + xid;

	var node = Ext.getCmp(manifestTreePaneId).getSelectionModel()
			.getSelectedNode();

	var parentClassId = node.parentNode.attributes.properties['Id'];
	var parentClassIndex = node.parentNode.attributes.properties['ClassIndex'];
	var parentClassPath = node.parentNode.attributes.properties['Path'];
	var templateId = node.attributes.properties['Id'];
	var templateIndex = node.attributes.properties['TemplateIndex'];

	Ext.Ajax.request({
		url : 'deleteTemplate?' + '&scope =' + scope + '&xid =' + xid,
		method : 'POST',
		params : {
			parentClassId : parentClassId,
			parentClassIndex : parentClassIndex,
			parentClassPath : parentClassPath,
			templateId : templateId,
			templateIndex : templateIndex
		},
		timeout : 120000,
		success : function(response, request) {
			var application = Ext.decode(response.responseText);
			centerPanel.getEl().unmask();
			refreshManifestTree(scope, xid);
		},
		failure : function(response, request) {
			centerPanel.getEl().unmask();
			var message = 'Error in delete template: ' + response.statusText
					+ '.\n\nError description: ' + response.responseText;
			showDialog(500, 240, 'Error', message, Ext.Msg.OK, null);
		}
	});
}

function includeTemplate(scope, xid) {
	var centerPanel = Ext.getCmp('content-pane');
	centerPanel.getEl().mask("Loading...", "x-mask-loading");
	var manifestTreePaneId = 'crossedManifest-tree(' + scope + ')' + xid;
	var node = Ext.getCmp(manifestTreePaneId).getSelectionModel()
			.getSelectedNode();

	var parentClassId = node.parentNode.attributes.properties['Id'];
	var parentClassIndex = node.parentNode.attributes.properties['ClassIndex'];
	var parentClassPath = node.parentNode.attributes.properties['Path'];
	var templateId = node.attributes.properties['Id'];
	var templateIndex = node.attributes.properties['TemplateIndex'];

	Ext.Ajax.request({
		url : 'includeTemplate?' + '&scope =' + scope + '&xid =' + xid,
		method : 'POST',
		params : {
			parentClassId : parentClassId,
			parentClassIndex : parentClassIndex,
			parentClassPath : parentClassPath,
			templateId : templateId,
			templateIndex : templateIndex
		},
		timeout : 120000,
		success : function(response, request) {
			var application = Ext.decode(response.responseText);
			centerPanel.getEl().unmask();
			refreshManifestTree(scope, xid);
		},
		failure : function(response, request) {
			centerPanel.getEl().unmask();
			var message = 'Error in include template: ' + response.statusText
					+ '.\n\nError description: ' + response.responseText;
			showDialog(500, 240, 'Error', message, Ext.Msg.OK, null);
		}
	});
}

function editExchangeConfig() {
	var centerPanel = Ext.getCmp('content-pane');
	centerPanel.getEl().mask("Loading...", "x-mask-loading");
	var node = Ext.getCmp('directory-tree').getSelectionModel()
			.getSelectedNode();
	var scope = node.parentNode.parentNode.parentNode.text;
	var commodity = node.parentNode.text;
	var commConfigName = node.text;

	var view = Ext.getCmp('newExchangeConfigWin');
	view.setTitle("Edit Exchange");

	Ext.Ajax.request({
		url : 'getExchange',
		params : {
			scope : scope,
			commName : commodity,
			name : commConfigName
		},
		method : 'POST',
		timeout : 120000,
		success : function(response, request) {
			var form = Ext.decode(response.responseText);
			var obj = Ext.getCmp('newExchConfig');
			var formdata = obj.getForm();
			formdata.findField('name').setValue(form.name);
			formdata.findField('description').setValue(form.description);
			formdata.findField('sourceUri').setValue(form.sourceUri);
			//testUriAndGetScopeName("SourceExchange");
            ScopeNameStore = new Ext.data.ArrayStore({

				url : 'getAMScope?' + '&sourceUri =' + form.sourceUri,
				autoDestroy : true,
				autoLoad : false,
				fields : [ {
					name : 'value'
				}, {
					name : 'name'
				} ]

			});
            Ext.getCmp('sourceScopeCombolist').bindStore(ScopeNameStore);
			Ext.getCmp('sourceScopeCombolist').setValue(form.sourceScope);
		//	getAppNames("SourceExchange");
        AppNameStore = new Ext.data.ArrayStore({

				url : 'getAppNames?' + '&appScope =' + form.sourceScope
						+ '&sourceUri =' + form.sourceUri,
				autoDestroy : true,
				autoLoad : false,
				fields : [ {
					name : 'value'
				}, {
					name : 'name'
				} ]

			});
			Ext.getCmp('sourceAppCombolist').bindStore(AppNameStore);
			Ext.getCmp('sourceAppCombolist').setValue(form.sourceApp);
			formdata.findField('sourceGraphName').setValue(form.sourceGraph);
			
			formdata.findField('targetUri').setValue(form.targetUri);
		//	testUriAndGetScopeName("TargetExchange");
         ScopeNameStore = new Ext.data.ArrayStore({

				url : 'getAMScope?' + '&sourceUri =' + form.targetUri,
				autoDestroy : true,
				autoLoad : false,
				fields : [ {
					name : 'value'
				}, {
					name : 'name'
				} ]

			});
            Ext.getCmp('targetScopeCombolist').bindStore(ScopeNameStore);
			Ext.getCmp('targetScopeCombolist').setValue(form.targetScope);
		//	getAppNames("TargetExchange");
         AppNameStore = new Ext.data.ArrayStore({

				url : 'getAppNames?' + '&appScope =' + form.targetScope
						+ '&sourceUri =' + form.targetUri,
				autoDestroy : true,
				autoLoad : false,
				fields : [ {
					name : 'value'
				}, {
					name : 'name'
				} ]

			});
			Ext.getCmp('targetAppCombolist').bindStore(AppNameStore);
			Ext.getCmp('targetAppCombolist').setValue(form.targetApp);
			formdata.findField('targetGraphName').setValue(form.targetGraph);
			formdata.findField('oldConfigName').setValue(commConfigName);
			formdata.findField('oldCommName').setValue(commodity);
			formdata.findField('oldScope').setValue(scope);
			centerPanel.getEl().unmask();
			view.show();
		},
		failure : function(response, request) {
			centerPanel.getEl().unmask();
			alert("Error fetching data to fill form");
		}
	});

}

function deleteScope(node, event) {
	var me = this;
	var node = Ext.getCmp('directory-tree').getSelectionModel()
			.getSelectedNode();
	var scope = node.text;

	var style = 'style="margin:0;padding:0;width:' + 400 + 'px;height:' + 60
			+ 'px;border:1px solid #aaa;overflow:auto"';
	Ext.Msg.show({
		title : 'Delete Scope',
		msg : '<textarea ' + style + ' readonly="yes">'
				+ 'Are you sure to delete ' + scope + ' context?'
				+ '</textarea>',
		buttons : Ext.MessageBox.YESNO,
		animEl : 'elId',
		// icon: Ext.Msg.QUESTION,
		// modal: true,
		// width:1000,
		// height:800,
		// multiline: 200,
		// buttons: Ext.Msg.YESNOCANCEL,
		// inputField: new IMS.form.DateField(),
		fn : function(buttonId, text) {
			// if (buttonId == 'ok'){
			if (buttonId == 'yes') {
				var contentPanel = Ext.getCmp('content-pane');
				contentPanel.getEl().mask("Loading...", "x-mask-loading");
				Ext.Ajax.request({
					url : 'deleteScope?' + '&scope =' + scope,
					method : 'POST',
					timeout : 120000,
					success : function(response, request) {
						contentPanel.getEl().unmask();
						// Ext.Msg.alert('"'+ scope + '"' +' Context is
						// deleted');
						refresh();
					},
					failure : function(response, request) {
						contentPanel.getEl().unmask();
						Ext.Msg.show({
							title : 'Delete Scope failed ',
							msg : '<textarea ' + style + ' readonly="yes">'
									+ 'Error: ' + response.reponseText
									+ '</textarea>',
							buttons : Ext.MessageBox.OKCANCEL
						});
					}
				});
			}
		}
	});
}

function editDeleteScope() {
	var me = this;
	var centerPanel = Ext.getCmp('content-pane');
	centerPanel.getEl().mask("Loading...", "x-mask-loading");
	var node = Ext.getCmp('directory-tree').getSelectionModel()
			.getSelectedNode();
	var scopevalue = node.text;

	var view = Ext.getCmp('newScopeWin');
	view.setTitle("Edit Scope");
	var obj = Ext.getCmp('newScopeForm');
	var form = obj.getForm();
	Ext.Ajax.request({
		url : 'getScope?' + '&scope =' + scopevalue,
		method : 'POST',
		timeout : 120000,
		success : function(response, request) {
			// alert("saved successfuly");
			var newScopeAdd = Ext.decode(response.responseText);

			form.setValues({
				scope : newScopeAdd.name,
				oldScope : scopevalue
			});
			centerPanel.getEl().unmask();
			view.show();
		},
		failure : function(response, request) {
			centerPanel.getEl().unmask();
			alert("Error fetching data to fill form");
		}
	});
}

function newScope() {
	var newScopeForm = new Ext.FormPanel({
		id : 'newScopeForm',
		border : false,
		layout : 'form',
		frame : false,
		bodyStyle : 'padding:15px',
		labelwidth : 75,
		items : [ {
			xtype : 'textfield',
			fieldLabel : 'Scope Name',
			width : 240,
			name : 'scope'
		}, {
			xtype : 'hidden',
			fieldLabel : 'Label',
			name : 'oldScope'
		} ],
		buttons : [ {
			text : 'Save',
			handler : function(node, button, event) {
				saveScope(node, button, event);
				newScopeWin.close();
			}
		}, {
			text : 'Cancel',
			handler : function(button, event) {
				newScopeWin.close();
			}
		} ]
	});

	var newScopeWin = new Ext.Window({
		id : 'newScopeWin',
		resizible : false,
		layout : {
			type : 'fit'
		},
		height : 130,
		width : 400,
		title : 'New Scope',
		modal : true,
		items : [ newScopeForm ]

	});

}

function saveScope(node, button, event) {
	var me = this;
	var obj = Ext.getCmp('newScopeForm').getForm();
	var form = obj.getValues(true);
	var directoryTree = Ext.getCmp('directory-tree');
	var node = directoryTree.getSelectionModel().getSelectedNode();

	var scope = obj.findField('scope').getValue();
	// newScope();
	Ext.Ajax.request({
		url : 'newScope?' + form,
		method : 'POST',
		timeout : 120000,
		success : function(response, request) {
			var result = Ext.decode(response.responseText);
			if (result === 'ERROR') {
				var message = 'Scope name ' + '"' + scope + '"'
						+ ' already exists' + '.';
				+response.responseText;
				showDialog(400, 90, 'Error', message, Ext.Msg.OK, null);

			}
			refresh();
		},
		failure : function(response, request) {
			alert("save failed");
		}
	});
}

function newCommodity() {
	var newCommForm = new Ext.FormPanel({
		id : 'newCommForm',
		border : false,
		bodyPadding : 15,
		layout : 'form',
		frame : false,
		bodyStyle : 'padding:15px',
		labelwidth : 75,
		items : [ {
			xtype : 'textfield',
			width : 240,
			fieldLabel : 'Commodity Name',
			name : 'commName'
		}, {
			xtype : 'hidden',
			fieldLabel : 'Label',
			name : 'oldCommName'
		}, {
			xtype : 'hidden',
			fieldLabel : 'Label',
			name : 'oldScope'
		} ],
		buttons : [ {
			text : 'Save',
			handler : function(node, button, event) {
				saveComm(node, button, event);
				newCommWin.close();
			}
		}, {
			text : 'Cancel',
			handler : function(button, event) {
				newCommWin.close();
			}
		} ]
	});

	var newCommWin = new Ext.Window({
		id : 'newCommWin',
		resizable : false,
		layout : {
			type : 'fit'
		},
		height : 130,
		width : 400,
		title : 'New Commodity',
		modal : true,
		items : [ newCommForm ]
	});
}

function saveComm(node, button, event) {
	var me = this;
	var obj = Ext.getCmp('newCommForm').getForm();
	var form = obj.getValues(true);
	var directoryTree = Ext.getCmp('directory-tree');
	var node = directoryTree.getSelectionModel().getSelectedNode();
	var scope = node.parentNode.text;
	var commodity = obj.findField('commName').getValue();

	Ext.Ajax.request({
		url : 'newComm?' + form + '&scope=' + scope,
		// method : 'POST',
		timeout : 120000,
		success : function(response, request) {
			var result = Ext.decode(response.responseText);
			if (result === 'ERROR') {
				var message = 'Commodity name ' + '"' + commodity + '"'
						+ ' already exists in scope ' + '"' + scope + '"' + '.';
				+response.responseText;
				showDialog(400, 90, 'Error', message, Ext.Msg.OK, null);

			}
			refresh();
		},
		failure : function(response, request) {
			alert("save failed");
		}
	});
}

function getAppNames(name) {
	var baseUri;
	var scopeName;
	if(name === "app"){
		var obj = Ext.getCmp('newAppForm');
		var form = obj.getForm();		
		baseUri = form.findField('baseUri').getValue();
		scopeName = Ext.getCmp('scopeCombolist').getValue();
	form.findField('appName').setDisabled(false);
	}else{
		var obj = Ext.getCmp('newExchConfig');
		var form = obj.getForm();
		if(name === "SourceExchange")
		{
			baseUri =	form.findField('sourceUri').getValue();
			scopeName = Ext.getCmp('sourceScopeCombolist').getValue();
			form.findField('sourceAppName').setDisabled(false);
		
		}else{
			baseUri =	form.findField('targetUri').getValue();
			scopeName = Ext.getCmp('targetScopeCombolist').getValue();
			form.findField('targetAppName').setDisabled(false);
		}
		}
	AppNameStore = new Ext.data.ArrayStore({

		url : 'getAppNames?' + '&appScope =' + scopeName + '&sourceUri ='
				+ baseUri,
		autoDestroy : true,
		autoLoad : false,
		fields : [ {
			name : 'value'
		}, {
			name : 'name'
		} ]

	});
	if (name === "app") {
		Ext.getCmp('appCombolist').bindStore(AppNameStore);
	} else if( name === "SourceExchange"){
		Ext.getCmp('sourceAppCombolist').bindStore(AppNameStore);

	}else if( name === "TargetExchange"){
		Ext.getCmp('targetAppCombolist').bindStore(AppNameStore);
	}

}

function testUriAndGetScopeName(name) {
	var baseUri
	if(name === "app"){
	var obj = Ext.getCmp('newAppForm');
	var form = obj.getForm();
	baseUri = form.findField('baseUri').getValue();
		form.findField('appScope').setDisabled(false);
	}
	else {
		var obj = Ext.getCmp('newExchConfig');
		var form = obj.getForm();
		if(name === "SourceExchange")
		{
			baseUri =	form.findField('sourceUri').getValue();
			form.findField('sourceScopeName').setDisabled(false);
		
		}else{
			baseUri =	form.findField('targetUri').getValue();
			form.findField('targetScopeName').setDisabled(false);
		}
		}
	var style = 'style="margin:0;padding:0;width:' + 400 + 'px;height:' + 160
			+ 'px;border:1px solid #aaa;overflow:auto"';
	Ext.Ajax.request({
		url : 'BasetestUri?' + '&sourceUri =' + baseUri,
		method : 'POST',
		timeout : 120000,
		success : function(response, request) {
			var result = Ext.decode(response.responseText);
			if (result.success === false) {
				if (name === "app") {
				
					Ext.getCmp('scopeCombolist').setDisabled(true);
				}  else if( name === "SourceExchange"){
					
					Ext.getCmp('sourceScopeCombolist').setDisabled(true);
				}else if (name === "TargetExchange")
					{
					Ext.getCmp('targetScopeCombolist').setDisabled(true);
					}
				Ext.Msg.show({
					title : 'Result ',
					msg : '<textarea ' + style + ' readonly="yes">'
							+ "Could not connect to the URL " + result.message
							+ '</textarea>',
					buttons : Ext.MessageBox.OK
				});
			}
			if (result.success === true) {			
				
				
				ScopeNameStore = new Ext.data.ArrayStore({

					url : 'getAMScope?' + '&sourceUri =' + baseUri,
					autoDestroy : true,
					autoLoad : false,
					fields : [ {
						name : 'value'
					}, {
						name : 'name'
					} ]

				});
				if (name === "app") {
					Ext.getCmp('scopeCombolist').bindStore(ScopeNameStore);
					Ext.getCmp('scopeCombolist').setDisabled(false);
				} else if( name === "SourceExchange"){
					Ext.getCmp('sourceScopeCombolist').bindStore(ScopeNameStore);
					Ext.getCmp('sourceScopeCombolist').setDisabled(false);
				}else if (name === "TargetExchange")
					{
					Ext.getCmp('targetScopeCombolist').bindStore(ScopeNameStore);
					Ext.getCmp('targetScopeCombolist').setDisabled(false);
					}

			}

		},
		failure : function(response, request) {
			Ext.Msg.show({
				title : 'Result ',
				msg : '<textarea ' + style + ' readonly="yes">'
						+ "failed to connect to the specified Url"
						+ '</textarea>',
				buttons : Ext.MessageBox.OK
			});
		}
	});
}

function newApp() {
	var node = Ext.getCmp('directory-tree').getSelectionModel()
			.getSelectedNode();
	var scope = node.parentNode.text;
	var ScopeNameStore = null;
	var AppNameStore = null;

	var scopeCombo = new Ext.form.ComboBox({
		fieldLabel : 'Context',
		name : 'appScope',
		id : 'scopeCombolist',
		allowBlank : false,
		store : ScopeNameStore,
		valueField : 'value',
		displayField : 'name',
		forceSelection : true,
		typeAhead : true,
		triggerAction : 'all',
		listeners : {
			scope : this,
			select : function() {
		//Ext.getCmp('appCombolist').clearValue();
				getAppNames("app");
			//Ext.getCmp('dispalyField').clearValue();
			//Ext.getCmp('AppDescription').clearValue();
				
			},
	change:function(id,newvalue, oldvalue)
	{
		 Ext.getCmp('appCombolist').clearValue();
		 //getAppNames("app");
		Ext.getCmp('dispalyField').setValue("");
		Ext.getCmp('AppDescription').setValue("");
	}
			
		}
	});

	var newAppForm = new Ext.FormPanel(
			{
				id : 'newAppForm',
				frame : false,
				border : false,
				bodyStyle : 'padding:15px',
				labelWidth : 110,
				defaults : {
					width : 300,
					allowBlank : false
				},
				// defaultType : 'textfield',
				items : [
						{

							xtype : 'textfield',
							fieldLabel : 'BaseUrl',
							allowBlank : false,
							name : 'baseUri',
							regex : /^(http[s]?:\/\/){0,1}(www\.){0,1}[a-zA-Z0-9\.\-]+\:[a-zA-Z0-9\/]{2,5}[\/]{0,1}/,
							regexText : 'Invalid URL Format (eg: http://localhost/services/dxfr)',
							vtypeText : 'Invalid URL Format (eg: http://localhost/services/dxfr)',
							listeners : {
								specialkey : function(f, e) {
									if((e.getKey() == e.TAB) || (e.getKey() == e.ENTER)) {
										testUriAndGetScopeName("app");
										// Ext.getCmp('scopeCombolist').bindStore(ScopeNameStore);
									}
									
								},
						 render: function(c) {
							 c.getEl().on('keyup', function() {
								 Ext.getCmp('scopeCombolist').clearValue();						
								 Ext.getCmp('scopeCombolist').setDisabled(true);
								 Ext.getCmp('appCombolist').clearValue();							
								 Ext.getCmp('appCombolist').setDisabled(true);
							      }, c);
						}
							}
						},

						scopeCombo,
						/*
						 * { xtype : 'textfield', fieldLabel : 'Scope Display
						 * Name', width :200, name : 'scopeDisplayName',
						 * allowBlank : false, value : scope },
						 */{
							xtype : 'combo',
							fieldLabel : 'Application',
							name : 'appName',
							id : 'appCombolist',
							store : AppNameStore,
							valueField : 'value',
							displayField : 'name',
							forceSelection : true,
							typeAhead : true,
							allowBlank : false,
							triggerAction : 'all',
							listeners : {
								scope : this,
								select : function() {
									var appName = Ext.getCmp('appCombolist')
											.getValue();
									var obj = Ext.getCmp('newAppForm');
									var form = obj.getForm();
									form.findField('displayName')
											.setDisabled(false);
									form.findField('appDesc')
											.setDisabled(false);
									form.findField('displayName').setValue(
											appName);
								}
								 
							}
						}, {
							xtype : 'textfield',
							fieldLabel : 'Display Name',
							id : "dispalyField",
							allowBlank : false,
							name : 'displayName'
						}, {
							xtype : 'textfield',
							fieldLabel : 'Description',
							id: "AppDescription",
							name : 'appDesc',
							allowBlank : true
						},

						{
							xtype : 'hidden',
							fieldLabel : 'Label',
							name : 'oldAppName'
						}, {
							xtype : 'hidden',
							fieldLabel : 'Label',
							name : 'oldScope'
						} ],
				buttons : [ {
					text : 'Save',
					handler : function(node, button, event) {
						saveApp(node, button, event);
						// newAppWin.close();
					}
				}, {
					text : 'Cancel',
					handler : function(button, event) {
						newAppWin.close();
					}
				} ]
			});

	var newAppWin = new Ext.Window({
		id : 'newAppWin',
		title : 'New Application',
		width : 500,
		height : 226,
		resizable : false,
		closable : true,
		modal : true,
		items : [ newAppForm ]
	});
}

function saveApp(node, button, event) {
	var me = this;
	var obj = Ext.getCmp('newAppForm').getForm();
	var form = obj.getValues(true);
	var directoryTree = Ext.getCmp('directory-tree');
	var node = directoryTree.getSelectionModel().getSelectedNode();
	var scope = node.parentNode.text;
	var application = obj.findField('appName').getValue();
	var oldScope = obj.findField('oldScope').getValue();

	var obj = Ext.getCmp('newAppForm');
	var form1 = obj.getForm();
	var baseUri = form1.findField('baseUri').getValue();
	var scopeName = form1.findField('scopeCombolist').getValue();
	// var displayScope = form.findField('scopeDisplayName').getValue();
	var appName = form1.findField('appCombolist').getValue();
	var appDisplay = form1.findField('displayName').getValue();

	if (((baseUri !== "") && (baseUri !== null))
			&& ((scopeName !== "") && (scopeName !== null))
			&& ((appName !== "") && (appName !== null) && (appDisplay !== "") && (appDisplay !== null))) {
		Ext.Ajax.request({
			url : 'newApplication?' + form + '&scope =' + scope,
			method : 'POST',
			timeout : 120000,
			success : function(response, request) {
				var result = Ext.decode(response.responseText);
				if (result === 'ERROR') {
					if (oldScope == '') {
						oldScope = scope;
					}
					var message = 'Application name ' + '"' + application + '"'
							+ ' already exists in scope ' + '"' + oldScope + '"'
							+ '.';
					+response.responseText;
					showDialog(400, 90, 'Error', message, Ext.Msg.OK, null);

				}
				Ext.getCmp('newAppWin').close();
				refresh();
			},
			failure : function(response, request) {
				alert("save failed");
			}
		});
	} else {
		if (baseUri === "") {
			form1.findField('baseUri').markInvalid('Base Uri cannot be Empty');
		}
		if (scopeName === "") {
			form1.findField('scopeCombolist').markInvalid(
					'Scope cannot be Empty');

		}
		if (appName === "") {
			form1.findField('appCombolist').markInvalid(
					'App Name cannot be Empty');
		}
		if (appDisplay === "") {
			form1.findField('displayName').markInvalid(
					'Dispaly App Name cannot be Empty');
		}
	}
}

function editDataFilter() {
	var centerPanel = Ext.getCmp('content-pane');
	centerPanel.getEl().mask("Loading...", "x-mask-loading");
	var node = Ext.getCmp('directory-tree').getSelectionModel()
			.getSelectedNode();
	var scope = node.parentNode.parentNode.parentNode.text;
	var commodity = node.parentNode.text;
	var commConfigName = node.text;
	var xid = node.attributes.properties['Id'];

	var view = Ext.getCmp('applyDataFilterWin');
	var obj = Ext.getCmp('dataFilterForm');
	var formdata = obj.getForm();

	/*
	 * var propetyName = Ext.getCmp('propertyName_1'); var propetyNameStore =
	 * propetyName.getStore(); var url = 'getColumnNames?' + '&scope =' +scope+
	 * '&xid='+xid; propetyNameStore.getProxy().url = url;
	 * propetyNameStore.load({ callback: function (records, response) { //
	 * Ext.getCmp('propertyName_1').setValues(response.response.responseText); }
	 * 
	 * });
	 */

	Ext.Ajax
			.request({
				url : 'getDataFilter',
				params : {
					scope : scope,
					commName : commodity,
					xid : xid
				},
				method : 'POST',
				timeout : 120000,
				success : function(response, request) {
					var dfList = Ext.decode(response.responseText);
					if (dfList !== null) {
						for ( var i = 0; i < dfList.length; i++) {
							var open = dfList[i][0];
							var log = dfList[i][1];
							var rel = dfList[i][2];
							var prop = dfList[i][3];
							var value = dfList[i][4];
							var close = dfList[i][5];

							var relValue;

							if (rel == "0") {
								relValue = "EqualTo";
							} else if (rel == "1") {
								relValue = "NotEqualTo";
							} else if (rel == "2") {
								relValue = "GreaterThan";
							} else if (rel == "3") {
								relValue = "LessThan";
							} else if (rel == "4") {
								relValue = "Contains";
							} else if (rel == "5") {
								relValue = "StartsWith";
							} else if (rel == "6") {
								relValue = "EndsWith";
							} else if (rel == "7") {
								relValue = "GreaterThanOrEqual";
							} else if (rel == "8") {
								relValue = "LesserThanOrEqual";
							}

							var logValue;
							if (log == "0") {
								logValue = "AND";
							} else if (log == "1") {
								logValue = "OR";
							}

							if (i + 1 >= 2) {
								var add = Ext.getCmp('save_0');
								add.handler.call(add);
							}
							Ext.getCmp('openCount_' + (i + 1)).setValue(open);
							Ext.getCmp('propertyName_' + (i + 1))
									.setValue(prop);
							var columnName = Ext.getCmp(
									'propertyName_' + (i + 1)).getValue();
							RelationalStore = new Ext.data.ArrayStore({
								autoDestroy : true,
								autoLoad : true,
								url : 'getRelational?' + '&name=' + columnName,
								fields : [ {
									name : 'name'
								}, {
									name : 'value'
								} ]

							});
							RelationalStore.on('load', function(store, records,
									options) {
							}, this);
							Ext.getCmp('relationalOperator_' + (i + 1))
									.bindStore(RelationalStore);

							Ext.getCmp('relationalOperator_' + (i + 1))
									.setValue(relValue);
							// Ext.getCmp('relationalOperator_'+ (i +
							// 1)).select(rel, true);
							Ext.getCmp('value_' + (i + 1)).setValue(value);
							Ext.getCmp('logicalOperator_' + (i + 1)).setValue(
									logValue);
							Ext.getCmp('closeGroup_' + (i + 1)).setValue(close);

						}
					}
					centerPanel.getEl().unmask();
					view.show();
				},
				failure : function(response, request) {
					centerPanel.getEl().unmask();
					Ext.Msg.alert("Error fetching data to fill form");
				}
			});
	Ext.Ajax.request({
		url : 'getDataFilterOE',
		params : {
			scope : scope,
			commName : commodity,
			xid : xid
		},
		method : 'POST',
		timeout : 120000,
		success : function(response, request) {
			var dfList = Ext.decode(response.responseText);

			if (dfList !== null) {
				for ( var i = 0; i < dfList.length; i++) {

					var prop = dfList[i][0];
					var sort = dfList[i][1];
					var sortValue;
					/*
					 * if(sort == "0") { sortValue = "Asc"; }else if(sort ==
					 * "1") { sortValue = "Desc"; }
					 */

					if (i + 1 >= 2) {
						var add = Ext.getCmp('OEAdd_0');
						add.handler.call(add);
					}
					Ext.getCmp('OESortOrder_' + (i + 1)).setValue(sort);
					Ext.getCmp('OEProName_' + (i + 1)).setValue(prop);

				}

			}
			centerPanel.getEl().unmask();
			view.show();
		},
		failure : function(response, request) {
			centerPanel.getEl().unmask();
			Ext.Msg.alert("Error fetching Filters");
		}
	});
	if (dfList !== null) {
		centerPanel.getEl().unmask();
		view.show();
	}

}

function applyDataFilter(url) {

	var urlWithTransferType = url + '&sort=' + "false";

	var ColumnNameStore = new Ext.data.ArrayStore({

		url : urlWithTransferType,
		autoDestroy : true,
		autoLoad : true,
		fields : [ {
			name : 'name'
		}, {
			name : 'value'
		} ]

	});

	// ColumnNameStore.load();
	ColumnNameStore.on('load', function(store, records, options) {
	}, this);

	var RelationalStore = new Ext.data.ArrayStore({
		autoDestroy : true,
		autoLoad : true,
		url : 'getRelational?' + '&name=' + "allOpr",
		fields : [ {
			name : 'name'
		}, {
			name : 'value'
		} ]

	});
	RelationalStore.on('load', function(store, records, options) {
	}, this);

	var LogicalStore = new Ext.data.ArrayStore({
		autoDestroy : true,
		autoLoad : true,
		url : 'getLogical',
		fields : [ {
			name : 'name'
		}, {
			name : 'value'
		} ]

	});

	var sortStore = new Ext.data.ArrayStore({
		autoDestroy : true,
		autoLoad : true,
		url : 'getSortOrder',
		fields : [ {
			name : 'name'
		}, {
			name : 'value'
		} ]

	});
	var sortColumnNames = url + '&sort=' + "true";
	var ColumnNameStoreOE = new Ext.data.ArrayStore({

		url : sortColumnNames,
		autoDestroy : true,
		autoLoad : true,
		fields : [ {
			name : 'name'
		}, {
			name : 'value'
		} ]

	});

	ColumnNameStoreOE.on('load', function(store, records, options) {
	}, this);

	var applyDataFilterForm = new Ext.FormPanel(
			{
				id : 'dataFilterForm',
				height : 343,
				margin : 1,
				width : 700,
				// resizable: false,
				// bodyBorder: false,
				// bodyPadding: 15,
				layout : 'form',
				frame : true,
				// bodyStyle:'padding:5px 5px 0',
				items : [
						{
							xtype : 'fieldset',
							height : 190,
							id : 'Expressions',
							margin : 2,
							width : 654,
							autoScroll : true,
							layout : {
								type : 'auto'
							},
							title : 'Expressions',
							items : [
									{
										xtype : 'container',
										height : 40,
										id : 'dataFilter_0',
										width : 630,
										layout : {
											align : 'stretch',
											type : 'hbox'
										},
										defaults : {

										// 5 margin: 6,
										},
										items : [
												{
													xtype : 'displayfield',
													flex : 1.3,
													value : '<b>Open Group Count</b>'
												},
												{
													xtype : 'displayfield',
													flex : 2.5,
													value : '<b>Property Name</b>'
												},
												{
													xtype : 'displayfield',
													flex : 3,
													value : '<b>Relational Operator</b>'
												},
												{
													xtype : 'displayfield',
													flex : 1.1,
													name : 'ValueLabel',
													value : '<b>Value</b>'
												},
												{
													xtype : 'displayfield',
													flex : 1.1,
													value : '<b>Logical Operator</b>'
												},
												{
													xtype : 'displayfield',
													flex : 1.3,
													value : '<b>Close Group Count </b>'
												},
												{
													xtype : 'button',
													handler : function(button,
															event) {
														var fieldSet = Ext
																.getCmp('Expressions');

														var rowCount = fieldSet.items.length;
														var rowID = 'dataFilter'
																+ '_'
																+ rowCount;
														var rowCmp = Ext
																.getCmp(rowID);
														while (rowCmp) {
															rowCount = rowCount + 1;
															rowID = 'dataFilter'
																	+ '_'
																	+ rowCount;
															// rowCmp = Ext
															// .getCmp(rowID);
														}

														var ColumnNameStore = new Ext.data.ArrayStore(
																{
																	url : urlWithTransferType,
																	autoDestroy : true,
																	autoLoad : true,
																	fields : [
																			{
																				name : 'name'
																			},
																			{
																				name : 'value'
																			} ]

																});

														ColumnNameStore
																.on(
																		'load',
																		function(
																				store,
																				records,
																				options) {
																		}, this);

														var RelationalStore = new Ext.data.ArrayStore(
																{
																	autoDestroy : true,
																	autoLoad : true,
																	url : 'getRelational?'
																			+ '&name='
																			+ "allOpr",
																	fields : [
																			{
																				name : 'name'
																			},
																			{
																				name : 'value'
																			} ]

																});

														var LogicalStore = new Ext.data.ArrayStore(
																{
																	autoDestroy : true,
																	autoLoad : true,
																	url : 'getLogical',
																	fields : [
																			{
																				name : 'name'
																			},
																			{
																				name : 'value'
																			} ]

																});
														var newrow = {
															xtype : 'container',
															id : 'dataFilter_'
																	+ rowCount,
															width : 630,
															layout : {
																// align:
																// 'stretch',
																type : 'hbox'
															},
															items : [
																	{
																		xtype : 'textfield',
																		id : 'openCount_'
																				+ rowCount,
																		width : 50,
																		flex : 1.3,
																		name : 'openGroupCount_'
																				+ rowCount
																	},
																	{
																		xtype : 'combo',
																		id : 'propertyName_'
																				+ rowCount,
																		name : 'propertyName_'
																				+ rowCount,
																		valueField : 'value',
																		width : 150,
																		forceSelection : true,
																		typeAhead : true,
																		triggerAction : 'all',
																		lazyRender : true,
																		store : ColumnNameStore,
																		displayField : 'name',
																		listWidth : 400,
																		listeners : {
																			scope : this,
																			select : function() {
																				var columnName = Ext
																						.getCmp(
																								'propertyName_'
																										+ rowCount)
																						.getValue();
																				RelationalStore = new Ext.data.ArrayStore(
																						{
																							autoDestroy : true,
																							autoLoad : true,
																							url : 'getRelational?'
																									+ '&name='
																									+ columnName,
																							fields : [
																									{
																										name : 'name'
																									},
																									{
																										name : 'value'
																									} ]

																						});
																				RelationalStore
																						.on(
																								'load',
																								function(
																										store,
																										records,
																										options) {
																								},
																								this);
																				Ext
																						.getCmp(
																								'relationalOperator_'
																										+ rowCount)
																						.bindStore(
																								RelationalStore);

																			}
																		}
																	// valueField:
																	// 'value',
																	},

																	{
																		xtype : 'combo',
																		id : 'relationalOperator_'
																				+ rowCount,
																		width : 130,
																		name : 'relationalOperator_'
																				+ rowCount,
																		editable : false,
																		flex : 1.8,
																		margin : 5,
																		/*
																		 * store: [ [
																		 * '0',
																		 * 'EqualTo' ], [
																		 * '1',
																		 * 'NotEqualTo' ], [
																		 * '2',
																		 * 'GreaterThan' ], [
																		 * '3',
																		 * 'LessThan' ] ]
																		 */
																		store : RelationalStore,
																		valueField : 'name',
																		forceSelection : true,
																		typeAhead : true,
																		triggerAction : 'all',
																		lazyRender : true,
																		displayField : 'value'

																	},
																	{
																		xtype : 'textfield',
																		id : 'value_'
																				+ rowCount,
																		width : 90,
																		name : 'value_'
																				+ rowCount,
																		flex : 1
																	// margins:
																	// '5',
																	},
																	{
																		xtype : 'combo',
																		id : 'logicalOperator_'
																				+ rowCount,
																		width : 100,
																		flex : 1,
																		// margins:
																		// '5',
																		name : 'logicalOperator_'
																				+ rowCount,
																		editable : false,
																		store : LogicalStore,
																		valueField : 'name',
																		forceSelection : true,
																		typeAhead : true,
																		triggerAction : 'all',
																		lazyRender : true,
																		displayField : 'value'
																	},
																	{
																		xtype : 'textfield',
																		id : 'closeGroup_'
																				+ rowCount,
																		flex : 1.3,
																		width : 50,
																		name : 'closeGroupCount_'
																				+ rowCount
																	},
																	{
																		xtype : 'button',
																		handler : function(
																				button,
																				event) {
																			var fieldSet = Ext
																					.getCmp('Expressions');

																			// container.el.mask('Deleting
																			// Expression
																			// ....');
																			var rowCount = fieldSet.items.length;
																			var container = button
																					.findParentByType(
																							'container')
																					.getId();

																			var curID = parseInt(container
																					.charAt(container.length - 1));
																			var arrOpen = [];
																			var arrProName = [];
																			var arrRelaOper = [];
																			var arrValue = [];
																			var arrLogOper = [];
																			var arrClose = [];
																			// var
																			// arrDel
																			// =[];

																			for ( var i = curID; i <= rowCount - 1; i++) {
																				arrOpen[i] = Ext
																						.getCmp(
																								'openCount_'
																										+ i)
																						.getValue();
																				arrProName[i] = Ext
																						.getCmp(
																								'propertyName_'
																										+ i)
																						.getValue();

																				arrRelaOper[i] = Ext
																						.getCmp(
																								'relationalOperator_'
																										+ i)
																						.getValue();

																				/*
																				 * var
																				 * relopr =
																				 * Ext
																				 * .getCmp(
																				 * 'relationalOperator_' +
																				 * i)
																				 * .getValue();
																				 * if(relopr ==
																				 * "0") {
																				 * arrRelaOper[i] =
																				 * "EqualTo"; }
																				 * else
																				 * if(relopr ==
																				 * "1") {
																				 * arrRelaOper[i] =
																				 * "NotEqualTo"; }
																				 * else
																				 * if(relopr ==
																				 * "2") {
																				 * arrRelaOper[i] =
																				 * "GreaterThan";
																				 * }else
																				 * if(relopr ==
																				 * "3") {
																				 * arrRelaOper[i] =
																				 * "LessThan"; }
																				 */

																				arrValue[i] = Ext
																						.getCmp(
																								'value_'
																										+ i)
																						.getValue();

																				arrLogOper[i] = Ext
																						.getCmp(
																								'logicalOperator_'
																										+ i)
																						.getValue();

																				/*
																				 * var
																				 * log =
																				 * Ext
																				 * .getCmp(
																				 * 'logicalOperator_' +
																				 * i)
																				 * .getValue();
																				 * if(log ==
																				 * "0") {
																				 * arrLogOper[i] =
																				 * "AND"; }
																				 * else
																				 * if(log ==
																				 * "1") {
																				 * arrLogOper[i] =
																				 * "OR"; }
																				 */
																				arrClose[i] = Ext
																						.getCmp(
																								'closeGroup_'
																										+ i)
																						.getValue();
																				// arrDel[i]
																				// =
																				// Ext.getCmp('delete_'+i).getValue();
																				Ext
																						.getCmp(
																								'dataFilter_'
																										+ i)
																						.destroy();

																			}
																			// var
																			// config
																			// =
																			// Ext.apply({},
																			// container.initialConfig.items[1]);

																			for ( var j = curID + 1; j <= rowCount - 1; j++) {

																				var add = Ext
																						.getCmp('save_0');

																				add.handler
																						.call(add);

																				Ext
																						.getCmp(
																								'openCount_'
																										+ (j - 1))
																						.setValue(
																								arrOpen[j]);

																				Ext
																						.getCmp(
																								'propertyName_'
																										+ (j - 1))
																						.setValue(
																								arrProName[j]);
																				Ext
																						.getCmp(
																								'relationalOperator_'
																										+ (j - 1))
																						.setValue(
																								arrRelaOper[j]);
																				Ext
																						.getCmp(
																								'value_'
																										+ (j - 1))
																						.setValue(
																								arrValue[j]);
																				Ext
																						.getCmp(
																								'logicalOperator_'
																										+ (j - 1))
																						.setValue(
																								arrLogOper[j]);
																				Ext
																						.getCmp(
																								'closeGroup_'
																										+ (j - 1))
																						.setValue(
																								arrClose[j]);
																				// Ext.getCmp('delete_'+(j-1)).setValue(arrDel[j]);
																			}
																			// container.el.unmask();

																		},
																		// flex:
																		// 0.12,
																		// margin:
																		// '0,0,0,1000',
																		border : false,
																		id : 'delete_'
																				+ rowCount,
																		// text:'Delete',
																		margins : '0, 20,0,0',
																		icon : 'resources/images/16x16/delete-icon.png'
																	} ]
														};

														fieldSet.add(newrow);

														fieldSet.doLayout();
													},
													// flex: 0.3,
													border : false,
													frame : true,
													id : 'save_0',
													height : 10,
													margins : '0, 20,0,0',
													icon : 'resources/images/16x16/add.png'
												} ]
									},
									{
										xtype : 'container',
										id : 'dataFilter_1',
										// height: 32,
										width : 630,
										layout : {
											// align: 'stretch',
											type : 'hbox'
										},
										defaults : {
										// flex: 1 ,
										// labelAlign: 'top' ,
										// margin: 5,
										},
										items : [
												{
													xtype : 'textfield',
													id : 'openCount_1',
													width : 50,
													flex : 1.3,
													// margin: 5,
													// height: 32,
													name : 'openGroupCount_1'
												},
												// cmbColumnNames,
												{
													xtype : 'combo',
													// flex: 1,
													// margins: '5',
													id : 'propertyName_1',
													name : 'propertyName_1',
													valueField : 'value',
													width : 150,// 250,
													forceSelection : true,
													typeAhead : true,
													triggerAction : 'all',
													lazyRender : true,
													// mode: 'remote',
													store : ColumnNameStore,
													displayField : 'name',
													listWidth : 400,
													listeners : {
														scope : this,
														select : function() {
															var columnName = Ext
																	.getCmp(
																			'propertyName_1')
																	.getValue();
															RelationalStore = new Ext.data.ArrayStore(
																	{
																		autoDestroy : true,
																		autoLoad : true,
																		url : 'getRelational?'
																				+ '&name='
																				+ columnName,
																		fields : [
																				{
																					name : 'name'
																				},
																				{
																					name : 'value'
																				} ]

																	});
															RelationalStore
																	.on(
																			'load',
																			function(
																					store,
																					records,
																					options) {
																			},
																			this);
															Ext
																	.getCmp(
																			'relationalOperator_1')
																	.bindStore(
																			RelationalStore);

														}
													}
												// valueField: 'value',
												},
												{
													xtype : 'combo',
													id : 'relationalOperator_1',
													width : 130,
													name : 'relationalOperator_1',
													editable : false,
													flex : 1.8,
													margin : 5,
													/*
													 * store: [ [ '0', 'EqualTo' ], [
													 * '1', 'NotEqualTo' ], [
													 * '2', 'GreaterThan' ], [
													 * '3', 'LessThan' ] ]
													 */
													store : RelationalStore,
													valueField : 'name',
													forceSelection : true,
													typeAhead : true,
													triggerAction : 'all',
													lazyRender : true,
													displayField : 'value'

												},
												{
													xtype : 'textfield',
													id : 'value_1',
													width : 90,
													name : 'value_1',
													flex : 1
												// margins: '5',
												},
												{
													xtype : 'combo',
													id : 'logicalOperator_1',
													width : 100,
													flex : 1,
													// margins: '5',
													name : 'logicalOperator_1',
													editable : false,
													store : LogicalStore,
													valueField : 'name',
													forceSelection : true,
													typeAhead : true,
													triggerAction : 'all',
													lazyRender : true,
													displayField : 'value'
												},
												{
													xtype : 'textfield',
													id : 'closeGroup_1',
													flex : 1.3,
													width : 50,
													name : 'closeGroupCount_1'
												},
												{
													xtype : 'button',
													handler : function(button,
															event) {
														var fieldSet = Ext
																.getCmp('Expressions');

														// container.el.mask('Deleting
														// Expression ....');
														var rowCount = fieldSet.items.length;
														var container = button
																.findParentByType(
																		'container')
																.getId();
														/*
														 * button.up(
														 * 'container')
														 * .getId();
														 */
														var curID = parseInt(container
																.charAt(container.length - 1));
														var arrOpen = [];
														var arrProName = [];
														var arrRelaOper = [];
														var arrValue = [];
														var arrLogOper = [];
														var arrClose = [];
														// var arrDel =[];
														for ( var i = curID; i <= rowCount - 1; i++) {
															arrOpen[i] = Ext
																	.getCmp(
																			'openCount_'
																					+ i)
																	.getValue();
															arrProName[i] = Ext
																	.getCmp(
																			'propertyName_'
																					+ i)
																	.getValue();
															arrRelaOper[i] = Ext
																	.getCmp(
																			'relationalOperator_'
																					+ i)
																	.getValue();

															/*
															 * var relopr = Ext
															 * .getCmp(
															 * 'relationalOperator_' +
															 * i) .getValue();
															 * if(relopr == "0") {
															 * arrRelaOper[i] =
															 * "EqualTo"; } else
															 * if(relopr == "1") {
															 * arrRelaOper[i] =
															 * "NotEqualTo"; }
															 * else if(relopr ==
															 * "2") {
															 * arrRelaOper[i] =
															 * "GreaterThan";
															 * }else if(relopr ==
															 * "3") {
															 * arrRelaOper[i] =
															 * "LessThan"; }
															 */

															arrValue[i] = Ext
																	.getCmp(
																			'value_'
																					+ i)
																	.getValue();
															arrLogOper[i] = Ext
																	.getCmp(
																			'logicalOperator_'
																					+ i)
																	.getValue();

															/*
															 * var log = Ext
															 * .getCmp(
															 * 'logicalOperator_' +
															 * i) .getValue();
															 * if(log == "0") {
															 * arrLogOper[i] =
															 * "AND"; } else
															 * if(log == "1") {
															 * arrLogOper[i] =
															 * "OR"; }
															 */
															arrClose[i] = Ext
																	.getCmp(
																			'closeGroup_'
																					+ i)
																	.getValue();
															// arrDel[i] =
															// Ext.getCmp('delete_'+i).getValue();
															Ext
																	.getCmp(
																			'dataFilter_'
																					+ i)
																	.destroy();

														}
														// var config =
														// Ext.apply({},
														// container.initialConfig.items[1]);

														for ( var j = curID + 1; j <= rowCount - 1; j++) {

															var add = Ext
																	.getCmp('save_0');

															add.handler
																	.call(add);

															Ext
																	.getCmp(
																			'openCount_'
																					+ (j - 1))
																	.setValue(
																			arrOpen[j]);

															Ext
																	.getCmp(
																			'propertyName_'
																					+ (j - 1))
																	.setValue(
																			arrProName[j]);
															Ext
																	.getCmp(
																			'relationalOperator_'
																					+ (j - 1))
																	.setValue(
																			arrRelaOper[j]);

															Ext
																	.getCmp(
																			'value_'
																					+ (j - 1))
																	.setValue(
																			arrValue[j]);
															Ext
																	.getCmp(
																			'logicalOperator_'
																					+ (j - 1))
																	.setValue(
																			arrLogOper[j]);
															Ext
																	.getCmp(
																			'closeGroup_'
																					+ (j - 1))
																	.setValue(
																			arrClose[j]);
															// Ext.getCmp('delete_'+(j-1)).setValue(arrDel[j]);
														}
														// container.el.unmask();

													},
													// flex: 0.12,
													// margin: '0,0,0,1000',
													border : false,
													id : 'delete_1',
													// text:'Delete',
													margins : '0, 20,0,0',
													icon : 'resources/images/16x16/delete-icon.png',
													tooltip : 'remove Expression'
												} ]
									} ]
						},
						{
							xtype : 'fieldset',
							height : 128,
							id : 'OExpress',
							margin : 5,
							width : 654,
							autoScroll : true,
							layout : {
								type : 'auto'
							},
							title : 'Order Expressions',
							items : [
									{
										xtype : 'container',
										id : 'OE_0',
										height : 40,
										width : 630,
										layout : {
											align : 'stretch',
											type : 'hbox'
										},

										items : [
												{
													xtype : 'displayfield',
													// flex: 6.5,
													width : 350,
													margin : 5,
													// labelStyle: 'center',
													value : '<b>Property Name </b>'
												},
												{
													xtype : 'displayfield',
													// flex: 2,
													width : 170,
													margins : '0, 10,0,0',
													value : '<b>Sort Order </b>'
												},
												{
													xtype : 'button',
													handler : function(button,
															event) {
														var fieldSet = this
																.findParentByType('fieldset');
														var rowCount = fieldSet.items.length;
														var rowID = 'OE_'
																+ rowCount;
														var rowCmp = Ext
																.getCmp(rowID);
														while (rowCmp) {
															rowCount = rowCount + 1;
															rowID = 'dataFilter'
																	+ '_'
																	+ rowCount;
															// rowCmp = Ext
															// .getCmp(rowID);
														}

														var ColumnNameStoreOE = new Ext.data.ArrayStore(
																{
																	url : sortColumnNames,
																	autoDestroy : true,
																	autoLoad : true,
																	fields : [
																			{
																				name : 'name'
																			},
																			{
																				name : 'value'
																			} ]

																});

														ColumnNameStoreOE
																.on(
																		'load',
																		function(
																				store,
																				records,
																				options) {
																		}, this);

														var sortStore = new Ext.data.ArrayStore(
																{
																	autoDestroy : true,
																	autoLoad : true,
																	url : 'getSortOrder',
																	fields : [
																			{
																				name : 'name'
																			},
																			{
																				name : 'value'
																			} ]

																});

														var newrow = {
															xtype : 'container',
															// height: 21,
															width : 630,
															id : 'OE_'
																	+ rowCount,
															layout : {
																// align:
																// 'stretch',
																type : 'hbox'
															},
															defaults : {

															// margin: 5,
															},
															items : [
																	{
																		xtype : 'combo',
																		id : 'OEProName_'
																				+ rowCount,
																		width : 290,
																		name : 'OEProName_'
																				+ rowCount,
																		valueField : 'value',
																		forceSelection : true,
																		typeAhead : true,
																		triggerAction : 'all',
																		lazyRender : true,
																		store : ColumnNameStoreOE,
																		listWidth : 400,
																		displayField : 'name'
																	// valueField:
																	// 'value',
																	}, /*
																		 * {
																		 * xtype:
																		 * 'textfield',
																		 * id:
																		 * 'OEProName_'+rowCount,
																		 * width:
																		 * 290,
																		 * name:
																		 * 'OEProName_'+rowCount },
																		 */

																	{
																		xtype : 'combo',
																		// flex:
																		// 1.2,
																		margins : '0, 50,0,0',
																		height : 15,
																		width : 150,
																		id : 'OESortOrder_'
																				+ rowCount,
																		name : 'OESortOrder_'
																				+ rowCount,
																		editable : false,
																		store : sortStore,
																		valueField : 'name',
																		forceSelection : true,
																		typeAhead : true,
																		triggerAction : 'all',
																		lazyRender : true,
																		displayField : 'value'
																	},
																	{
																		xtype : 'button',
																		handler : function(
																				button,
																				event) {
																			var container = this
																					.findParentByType('fieldset');
																			// container.el.mask('Deleting
																			// Order
																			// Expression
																			// ....');
																			var rowCount = container.items.length;
																			var container = button
																					.findParentByType(
																							'container')
																					.getId();
																			var curID = parseInt(container
																					.charAt(container.length - 1));
																			var arrSortOrder = [];
																			var arrProName = [];

																			for ( var i = curID; i <= rowCount - 1; i++) {

																				var sort = Ext
																						.getCmp(
																								'OESortOrder_'
																										+ i)
																						.getValue();

																				arrProName[i] = Ext
																						.getCmp(
																								'OEProName_'
																										+ i)
																						.getValue();
																				if (sort == "0") {
																					arrSortOrder[i] = "Asc";
																				} else if (sort == "1") {
																					arrSortOrder[i] = "Desc";
																				}
																				Ext
																						.getCmp(
																								'OE_'
																										+ i)
																						.destroy();
																			}

																			for ( var j = curID + 1; j <= rowCount - 1; j++) {

																				var add = Ext
																						.getCmp('OEAdd_0');
																				add.handler
																						.call(add);
																				Ext
																						.getCmp(
																								'OESortOrder_'
																										+ (j - 1))
																						.setValue(
																								arrSortOrder[j]);
																				Ext
																						.getCmp(
																								'OEProName_'
																										+ (j - 1))
																						.setValue(
																								arrProName[j]);

																				// Ext.getCmp('delete_'+(j-1)).setValue(arrDel[j]);
																			}
																			// container.el.unmask();

																		},
																		// flex:
																		// 0.22,
																		// flex:
																		// 1,
																		border : false,
																		id : 'OEDelete_'
																				+ rowCount,
																		// margin:
																		// 10,
																		icon : 'resources/images/16x16/delete-icon.png'
																	} ]
														};

														fieldSet.add(newrow);
														fieldSet.doLayout();

													},
													// flex: 0.17,
													// margins: '5',
													border : false,
													// height: 22,
													id : 'OEAdd_0',
													// width: 26,
													icon : 'resources/images/16x16/add.png'
												} ]
									},
									{
										xtype : 'container',
										// height: 21,
										width : 630,
										id : 'OE_1',
										layout : {
											// align: 'stretch',
											type : 'hbox'
										},
										defaults : {

										// margin: 5,
										},
										items : [
												{
													xtype : 'combo',
													id : 'OEProName_1',
													width : 290,
													name : 'OEProName_1',
													valueField : 'value',
													forceSelection : true,
													typeAhead : true,
													triggerAction : 'all',
													lazyRender : true,
													store : ColumnNameStoreOE,
													listWidth : 400,
													displayField : 'name'
												// valueField: 'value',
												},/*
													 * { xtype: 'textfield', id:
													 * 'OEProName_1', width:
													 * 290, name: 'OEProName_1' },
													 */
												{
													xtype : 'combo',
													// flex: 1.2,
													margins : '0, 50,0,0',
													height : 15,
													width : 150,
													id : 'OESortOrder_1',
													name : 'OESortOrder_1',
													editable : false,
													store : sortStore,
													valueField : 'name',
													forceSelection : true,
													typeAhead : true,
													triggerAction : 'all',
													lazyRender : true,
													displayField : 'value'
												},
												{
													xtype : 'button',
													handler : function(button,
															event) {
														var container = this
																.findParentByType('fieldset');
														// container.el.mask('Deleting
														// Order Expression
														// ....');
														var rowCount = container.items.length;
														var container = button
																.findParentByType(
																		'container')
																.getId();
														var curID = parseInt(container
																.charAt(container.length - 1));
														var arrSortOrder = [];
														var arrProName = [];

														for ( var i = curID; i <= rowCount - 1; i++) {

															var sort = Ext
																	.getCmp(
																			'OESortOrder_'
																					+ i)
																	.getValue();

															arrProName[i] = Ext
																	.getCmp(
																			'OEProName_'
																					+ i)
																	.getValue();
															if (sort == "0") {
																arrSortOrder[i] = "Asc";
															} else if (sort == "1") {
																arrSortOrder[i] = "Desc";
															}
															Ext.getCmp(
																	'OE_' + i)
																	.destroy();
														}

														for ( var j = curID + 1; j <= rowCount - 1; j++) {

															var add = Ext
																	.getCmp('OEAdd_0');
															add.handler
																	.call(add);
															Ext
																	.getCmp(
																			'OESortOrder_'
																					+ (j - 1))
																	.setValue(
																			arrSortOrder[j]);
															Ext
																	.getCmp(
																			'OEProName_'
																					+ (j - 1))
																	.setValue(
																			arrProName[j]);

															// Ext.getCmp('delete_'+(j-1)).setValue(arrDel[j]);
														}
														// container.el.unmask();

													},
													// flex: 0.22,
													// flex: 1,
													border : false,
													id : 'OEDelete_1',
													// margin: 10,
													icon : 'resources/images/16x16/delete-icon.png'
												} ]
									} ]
						} ],
				buttons : [ {
					text : 'Save',
					handler : function(node, button, event) {
						saveDataFilter(node, button, event);
						applyDataFilterWin.close();
					}
				// margin: 10,
				}, {
					text : 'Cancel',
					handler : function(button, event) {
						applyDataFilterWin.close();
					}
				} ]
			});

	var applyDataFilterWin = new Ext.Window({
		id : 'applyDataFilterWin',
		// resizable: false,
		layout : {
			type : 'fit'
		},
		closable : true,
		title : 'Apply Data Filter',
		modal : true,
		items : [ applyDataFilterForm ]
	});
}

function saveDataFilter() {
	var node = Ext.getCmp('directory-tree').getSelectionModel()
			.getSelectedNode();
	var obj = Ext.getCmp('dataFilterForm').getForm();
	var form = obj.getValues(true);
	var scope = node.parentNode.parentNode.parentNode.text;
	var commodity = node.parentNode.text;
	var commConfigName = node.text;
	var xid = node.attributes.properties['Id'];

	var express = Ext.getCmp('Expressions');
	// var container = this.up('fieldset');
	var config = Ext.apply({}, express.initialConfig.items[1]);
	var ExpressCount = express.items.length;
	var arrOpen = [];
	var arrProName = [];
	var arrRelaOper = [];
	var arrValue = [];
	var arrLogOper = [];
	var arrClose = [];

	for ( var i = 1; i <= ExpressCount - 1; i++) {

		arrOpen[i] = Ext.getCmp('openCount_' + i).getValue();
		arrProName[i] = Ext.getCmp('propertyName_' + i).getValue();
		arrRelaOper[i] = Ext.getCmp('relationalOperator_' + i).getValue();

		/*
		 * if(rel == "0") { arrRelaOper[i] = "EqualTo"; } else if(rel == "1") {
		 * arrRelaOper[i] = "NotEqualTo"; } else if(rel == "2") { arrRelaOper[i] =
		 * "GreaterThan"; }else if(rel == "3") { arrRelaOper[i] = "LessThan"; }
		 */

		var value = Ext.getCmp('value_' + i).getValue();
		if (value != '') {
			arrValue[i] = Ext.getCmp('value_' + i).getValue();
		} else {
			arrValue[i] = '';
		}
		arrLogOper[i] = Ext.getCmp('logicalOperator_' + i).getValue();
		/*
		 * if(log == "0") { arrLogOper[i] = "AND"; } else if(log == "1") {
		 * arrLogOper[i] = "OR"; }
		 */
		arrClose[i] = Ext.getCmp('closeGroup_' + i).getValue();

		/*
		 * var arrOpen = Ext.getCmp('openCount_'+i).getValue(); var arrProName =
		 * Ext.getCmp('propertyName_'+i).getValue(); var arrRelaOper =
		 * Ext.getCmp('relationalOperator_'+i).getValue(); var arrValue =
		 * Ext.getCmp('value_'+i).getValue(); var arrLogOper =
		 * Ext.getCmp('logicalOperator_'+i).getValue(); var arrClose =
		 * Ext.getCmp('closeGroup_'+i).getValue();
		 */

	}

	var oExpress = Ext.getCmp('OExpress');
	// var container = this.up('fieldset');
	var config = Ext.apply({}, oExpress.initialConfig.items[1]);
	var OExpressCount = oExpress.items.length;

	var arrSortOrder = [];
	var arrProNameOE = [];

	for ( var i = 1; i <= OExpressCount - 1; i++) {
		// var arrSortOrder = Ext.getCmp('OESortOrder_'+i).getValue();
		// var arrProName = Ext.getCmp('OEProName_'+i).getValue();
		arrSortOrder[i] = Ext.getCmp('OESortOrder_' + i).getValue();
		/*
		 * if(sort == "0") { arrSortOrder[i] = "Asc"; }else if(sort == "1") {
		 * arrSortOrder[i] = "Desc"; }
		 */
		arrProNameOE[i] = Ext.getCmp('OEProName_' + i).getValue();

	}
	var context = '?scope=' + scope + '&xid=' + xid;

	Ext.Ajax.request({
		url : 'dataFilter',
		params : {
			expressCountOE : OExpressCount,
			expressCount : ExpressCount,
			openGroup : arrOpen,
			propertyNameOE : arrProNameOE,
			sortOrder : arrSortOrder,
			propertyName : arrProName,
			relationalOper : arrRelaOper,
			value : arrValue,
			logicalOper : arrLogOper,
			closeGroup : arrClose,
			scope : scope,
			commName : commodity,
			xid : xid
		},
		method : 'POST',
		timeout : 120000,
		success : function(response, request) {
			// alert("saved successfuly");
			Ext.Ajax.request({
				url : 'reset?dtoContext=' + escape(context.substring(1)),
				method : 'POST'

			});
			button.up('.window').close();
			// me.onRefreshTree();
		},
		failure : function(response, request) {
			Ext.Msg.alert("save failed");
		}
	});
}

function refresh() {
	var directoryTree = Ext.getCmp('directory-tree');
	var contentPane = Ext.getCmp('content-pane');

	// clear dto tabs
	while (contentPane.items.length > 0) {
		contentPane.items.items[0].destroy();
	}

	// clear property grid
	Ext.getCmp('property-pane').setSource({});

	// disable toolbar buttons
	Ext.getCmp('exchange-button').disable();
	Ext.getCmp('xlogs-button').disable();

	// reload tree
	directoryTree.getLoader().load(directoryTree.root);
	directoryTree.getRootNode().expand(false);
}

function refreshManifestTree(scope, xid) {

	var manifestTreePaneId = 'crossedManifest-tree(' + scope + ')' + xid;
	var manifestTree = Ext.getCmp(manifestTreePaneId);

	// reload tree
	manifestTree.getLoader().load(manifestTree.root);
	manifestTree.getRootNode().expand(false);
}

Ext
		.onReady(function() {

			Ext.Ajax.timeout = 180000;
			Ext.state.Manager.setProvider(new Ext.state.CookieProvider());

			applicationMenu = new Ext.menu.Menu();
			this.applicationMenu.add(this.buildApplicationSubMenu());

			commodityMenu = new Ext.menu.Menu();
			this.commodityMenu.add(this.buildCommoditySubMenu());

			editDeleteScopeMenu = new Ext.menu.Menu();
			this.editDeleteScopeMenu.add(this.buildEditDeleteSubMenu());

			graphSubMenu = new Ext.menu.Menu();
			this.graphSubMenu.add(this.buildGraphSubMenu());

			newappmenu = new Ext.menu.Menu();
			this.newappmenu.add(this.buildNewApplicationMenu());

			newCommoditymenu = new Ext.menu.Menu();
			this.newCommoditymenu.add(this.buildNewCommodityMenu());

			newExchangemenu = new Ext.menu.Menu();
			this.newExchangemenu.add(this.buildNewExchangeMenu());

			newScopemenu = new Ext.menu.Menu();
			this.newScopemenu.add(this.buildNewScopeMenu());

			// manifestMenu = new Ext.menu.Menu();
			// this.manifestMenu.add(this.buildManifestMenu());

			// exchangeMenu.add(this.buildExchangeMenu());
			Ext.QuickTips.init();
			/*
			 * this.control({
			 * 
			 * "treepanel": { itemclick: this.onTreeItemClick, itemdblclick:
			 * this.onTreeItemDblClick, itemcontextmenu:
			 * this.onTreeItemContextMenu }});
			 */

			Ext.get('about-link').on('click', function() {
				var win = new Ext.Window({
					title : 'About Exchange Manager',
					bodyStyle : 'background-color:white;padding:5px',
					width : 700,
					height : 500,
					closable : true,
					resizable : false,
					autoScroll : true,
					buttons : [ {
						text : 'Close',
						handler : function() {
							Ext.getBody().unmask();
							win.close();
						}
					} ],
					autoLoad : 'about-exchange-manager.jsp',
					listeners : {
						close : {
							fn : function() {
								Ext.getBody().unmask();
							}
						}
					}
				});

				Ext.getBody().mask();
				win.show();
			});

			var headerPane = new Ext.BoxComponent({
				region : 'north',
				height : 55,
				contentEl : 'header'
			});

			var directoryTreePane = new Ext.tree.TreePanel(
					{
						id : 'directory-tree',
						region : 'center',
						dataUrl : 'directory',
						width : 800,
						lines : true,
						autoScroll : true,
						border : false,
						animate : true,
						enableDD : false,
						containerScroll : true,
						rootVisible : true,
						tbar : new Ext.Toolbar(
								{
									items : [
											{
												id : 'refresh-button',
												xtype : 'button',
												icon : 'resources/images/16x16/view-refresh.png',
												text : 'Refresh',
												handler : function() {
													refresh();
												}
											},
											{
												id : 'exchange-button',
												xtype : 'button',
												icon : 'resources/images/16x16/exchange-send.png',
												text : 'Exchange',
												disabled : true,
												handler : function() {
													var node = Ext
															.getCmp(
																	'directory-tree')
															.getSelectionModel()
															.getSelectedNode();
													var scope = node.parentNode.parentNode.parentNode.attributes['text'];
													var exchange = node.attributes["text"];
													var xid = node.attributes.properties['Id'];
													var reviewed = (node.reviewed != undefined);
													var msg = 'Are you sure you want exchange data \r\n['
															+ exchange + ']?';
													var processUserResponse = submitExchange
															.createDelegate([
																	exchange,
																	scope, xid,
																	reviewed ]);
													showDialog(
															460,
															125,
															'Exchange Confirmation',
															msg,
															Ext.Msg.OKCANCEL,
															processUserResponse);
												}
											},
											{
												// TODO: TBD
												id : 'xlogs-button',
												xtype : 'button',
												icon : 'resources/images/16x16/history.png',
												text : 'History',
												disabled : true,
												hidden : true,
												handler : function() {
													alert('Show exchange log');
												}
											} ]
								}),
						root : {
							nodeType : 'async',
							text : 'Directory',
							icon : 'resources/images/directory.png'
						},
						listeners : {
							click : function(node, event) {
								if (node.attributes != null) {
									if (node.attributes.properties != null) {
										Ext.getCmp('property-pane').setSource(
												node.attributes.properties);
									}
								}
								try {
									if (node.parentNode != null) {
										if (node.parentNode.parentNode != null) {
											var dataTypeNode = node.parentNode.parentNode;

											if (dataTypeNode != null
													&& dataTypeNode.attributes['text'] == 'Data Exchanges') {
												Ext.getCmp('exchange-button')
														.enable();
												Ext.getCmp('xlogs-button')
														.enable();
											} else {
												Ext.getCmp('exchange-button')
														.disable();
												Ext.getCmp('xlogs-button')
														.disable();
											}
										}
									}

								} catch (err) {
								}
							},
							dblclick : function(node, event) {
								var properties = node.attributes.properties;
								Ext.getCmp('property-pane').setSource(
										node.attributes.properties);

								try {
									var dataTypeNode = node.parentNode.parentNode;

									if (dataTypeNode != null) {
										if (dataTypeNode.attributes['text'] == 'Application Data') {
											var graphNode = node.parentNode;
											var scope = properties['Context'];
											var app = graphNode.attributes.properties['Internal Name']; 
											var graph = node.attributes['text'];
											var baseUri = properties['Base URI'];
											var label = scope + '.' + app + '.'
													+ graph;
											var context = '?baseUri=' + baseUri
													+ '&scope=' + scope
													+ '&app=' + app + '&graph='
													+ graph;

											loadPageDto('app', 'adata',
													context, label);
										} else if (dataTypeNode.attributes['text'] == 'Data Exchanges') {
											var scope = dataTypeNode.parentNode.attributes['text'];
											var exchangeId = properties['Id'];
											var context = '?scope=' + scope
													+ '&xid=' + exchangeId;
											var label = '(' + scope + ')'
													+ node.text;
											node.reviewed = true;
											loadPageDto('exchange', 'xdata',
													context, label);
										}
									}
								} catch (err) {
								}
							},
							contextmenu : function(node, event) {
								onTreeItemContextMenu(node, event);
							},
							keydown : function(evnt) {
								// alert('keydown...');
								var keyPressed = evnt.getKey();
								if (evnt.ctrlKey) {
									/*
									 * After trial and error, the ctrl+c
									 * combination seems to be code 67
									 */
									if (67 == 67)// if (keyPressed == 67)
									{
										var celldata = Ext.getCmp(
												'property-pane')
												.getSelectionModel().events.beforecellselect.obj.selection.record.data.value;
										copyToClipboard(celldata);
									}
								}
							}
						}
					});

			var propertyPane = new Ext.grid.PropertyGrid(
					{
						id : 'property-pane',
						title : 'Details',
						region : 'south',
						height : 250,
						layout : 'fit',
						collapsible : true,
						stripeRows : true,
						autoScroll : true,
						border : false,
						split : true,
						source : {},
						listeners : {
							beforeedit : function(e) {
								e.cancel = true;
							},
							click : function() {
								// alert('clicked...');
							},
							keydown : function(evnt) {
								var keyPressed = evnt.getKey();
								if (evnt.ctrlKey) {
									/*
									 * After trial and error, the ctrl+c
									 * combination seems to be code 67
									 */
									if (67 == 67)// if (keyPressed == 67)
									{
										var celldata = Ext.getCmp(
												'property-pane')
												.getSelectionModel().events.beforecellselect.obj.selection.record.data.value;
										copyToClipboard(celldata);

									}
								}
							}
						}
					});

			var directoryPane = new Ext.Panel({
				region : 'west',
				id : 'west-panel',
				title : 'Directory',
				frame : false,
				border : false,
				split : true,
				width : 260,
				minSize : 175,
				maxSize : 400,
				collapsible : true,
				// margins: '0 0 0 4',
				layout : 'border',
				items : [ directoryTreePane, propertyPane ]
			});

			var contentPane = new Ext.TabPanel({
				id : 'content-pane',
				region : 'center',
				deferredRender : false,
				enableTabScroll : true,
				border : true,
				activeItem : 0
			});

			var viewport = new Ext.Viewport({
				layout : 'border',
				items : [ headerPane, directoryPane, contentPane ]
			});

			directoryTreePane.getRootNode().expand(false);
		});
