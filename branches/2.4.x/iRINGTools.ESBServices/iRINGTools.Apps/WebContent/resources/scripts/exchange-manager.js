Ext.ns('org.iringtools.apps.xmgr');

Ext.Ajax.on('requestexception', function (conn, response, options) {
  if (response.status == 0 || response.status == 408) {
      location.reload(true);
  }
});

function copyToClipboard(celldata) {
  /*
   * if (window.clipboardData) // Internet Explorer window.clipboardData.setData
   * ("Text", celldata);
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
      timeout : 86400000
    // 24 hours
    }),
    reader : new Ext.data.DynamicGridReader({}),
    remoteSort : true,
    listeners : {
      exception : function(proxy, type, action, request, response) {
        if (!(response.status == 0 || response.status == 408)) {
          container.getEl().unmask();
          var message = 'Request URL: /' + request.url + '.\n\nError description: ' + response.responseText;
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
      cellclick : function(ts, td, cellIndex, record, tr, rowIndex, e, eOpts) {
        val = record.target.innerText;
      },
      /*
       * celldblclick: function(ts, td, cellIndex, record, tr, rowIndex, e,
       * eOpts ){ val = record.target.innerText; copyToClipboard(val); },
       */
      beforeedit : function(e) {
        e.cancel = true;
      },
      keydown : function(evnt) {
        var keyPressed = evnt.getKey();
        if (evnt.ctrlKey) {
          /*
           * After trial and error, the ctrl+c combination seems to be code 67
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

  xlogsStore.on('load', function() {
    var xlogsPane = new Ext.grid.GridPanel({
      id : 'xlogs-' + xlabel,
      store : xlogsStore,
      cellValue : '',
      stripeRows : true,
      loadMask : true,
      cm : new Ext.grid.DynamicColumnModel(xlogsStore),
      selModel : new Ext.grid.CellSelectionModel({
        singleSelect : true
      }),
      enableColLock : true,
      tbar : new Ext.Toolbar({
        items : [ {
          xtype : 'tbspacer',
          width : 4
        }, {
          xtype : 'label',
          html : '<span style="font-weight:bold">Exchange Results</span>'
        }, {
          xtype : 'tbspacer',
          width : 4
        }, {
          xtype : 'button',
          icon : 'resources/images/16x16/view-refresh.png',
          tooltip : 'Refresh',
          handler : function() {
            xlogsStore.load();
          }
        } ]
      }),
      listeners : {
        beforeedit : function(e) {
          e.cancel = true;
        },
        cellclick : function(ts, td, cellIndex, record, tr, rowIndex, e, eOpts) {
          cellValue = record.target.innerText;
        },
        keydown : function(evnt) {
          var keyPressed = evnt.getKey();
          if (evnt.ctrlKey) {
            /*
             * After trial and error, the ctrl+c combination seems to be code 67
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

function createPageXlogs(scope, xid, xlabel, xFormattedTime, xtime, poolSize, itemCount) {
  var paneTitle = xlabel + ' (' + xFormattedTime + ')';
  var tab = Ext.getCmp('content-pane').getItem(paneTitle);

  if (tab != null) {
    tab.show();
  } else {
    var contentPane = Ext.getCmp('content-pane');
    contentPane.getEl().mask("Loading...", "x-mask-loading");

    var url = 'pageXlogs' + '?scope=' + scope + '&xid=' + xid + '&xtime=' + xtime + '&itemCount=' + itemCount;
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

    store.on('load', function() {
      if (Ext.getCmp('content-pane').getItem('tab-' + label) == null) {
        var dtoBcPane = new Ext.Container({
          id : 'bc-' + label,
          cls : 'bc-container',
          padding : '5',
          items : [ {
            xtype : 'box',
            autoEl : {
              tag : 'span',
              html : '<a class="breadcrumb" href="#" onclick="navigate(0)">' + store.reader.description + '</a>'
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
          var dtoToolbar = new Ext.Toolbar({
            cls : 'nav-toolbar',
            width : 80,
            items : [ {
              id : 'tb-exchange',
              xtype : 'button',
              tooltip : 'Send data to target endpoint',
              icon : 'resources/images/16x16/exchange-send.png',
              handler : function() {
                var xidIndex = context.indexOf('&xid=');
                var scope = context.substring(7, xidIndex);
                var xid = context.substring(xidIndex + 5);
                var msg = 'Are you sure you want to exchange data \r\n[' + label + ']?';
                var processUserResponse = submitExchange.createDelegate([ label, scope, xid, true ]);
                showDialog(460, 125, 'Exchange Confirmation', msg, Ext.Msg.OKCANCEL, processUserResponse);
              }
            }, {
              xtype : 'tbspacer',
              width : 4
            }, {
              id : 'tb-xlog',
              xtype : 'button',
              tooltip : 'Show/hide exchange results',
              icon : 'resources/images/16x16/history.png',
              handler : function() {
                var dtoTab = Ext.getCmp('content-pane').getActiveTab();
                var xlogsContainer = dtoTab.items.map['xlogs-container-' + label];

                if (xlogsContainer.items.length == 0) {
                  createXlogsPane(context, xlogsContainer, label);
                } else {
                  if (xlogsContainer.collapsed)
                    xlogsContainer.expand(true);
                  else {
                    xlogsContainer.collapse(true);
                  }
                }
              }
            }, {
              xtype : 'tbspacer',
              width : 4
            }, {
              id : 'tb-dup',
              xtype : 'button',
              tooltip : 'Show Pre-Exchange Summary Data',
              icon : 'resources/images/16x16/file-table.png',
              handler : function() {
                var contentPanel = Ext.getCmp('content-pane');
                var dtoTab = contentPanel.getActiveTab();
                
                contentPane.getEl().mask("Loading...", "x-mask-loading");
                
                Ext.Ajax.request({
                  url: 'sdata' + context,
                  timeout: 120000,
                  success: function (response) {
                    Ext.getCmp('content-pane').getEl().unmask();
                    var summary = Ext.decode(response.responseText);
                    
                    var win = new Ext.Window({
                      title : 'Pre-Exchange Summary',
                      closable : true,
                      resizable : false,                      
                      modal : true,
                      layout : 'column',
                      width: 500,
                      defaults: {
                        border: false
                      },  
                      items: [{
                         xtype: 'panel',
                         columnWidth: .35,      
                         frame: false,
                         defaults: {
                           height: 24,
                           border: false,
                           style : 'font-weight:bold;'
                         },                         
                         items: [{
                           html : 'Sender Application:'
                         }, {
                           html : 'Sender Graph:'
                         }, {
                           html : 'Sender Endpoint:'
                         }, {
                           html : 'Receiver Application:'
                         }, {
                           html : 'Receiver Graph:'
                         }, {
                           html : 'Receiver Endpoint:'
                         }, {
                           html : 'Pool Size:'
                         }, {
                           html : 'Total Count:'
                         }, {
                           html : 'Adding Count:'
                         }, {
                           html : 'Changing Count:'
                         }, {
                           html : 'Deleting Count:'
                         }, {
                           html : 'Synchronizing Count:'
                         }]
                      }, {
                        xtype: 'panel',                      
                        columnWidth: .65,
                        frame: false,
                        defaults: {
                          height: 24,
                          border: false
                        },
                        items: [{
                          html : summary['SenderApplication']
                        }, {
                          html : summary['SenderGraph']
                        }, {
                          html : summary['SenderURI']
                        }, {
                          html : summary['ReceiverApplication']
                        }, {
                          html : summary['ReceiverGraph']
                        }, {
                          html : summary['ReceiverURI']
                        }, {
                          html : summary['PoolSize']
                        }, {
                          html : summary['TotalCount']
                        }, {
                          html : summary['AddingCount']
                        }, {
                          html : summary['ChangingCount']
                        }, {
                          html : summary['DeletingCount']
                        }, {
                          html : summary['SynchronizingCount']
                        }]
                      }]
                    });
                    
                    win.show();
                  },
                  failure: function (response, request) {
                    Ext.getCmp('content-pane').getEl().unmask();
                    var message = 'Error getting exchange summary: ' + response.statusText;
                    showDialog(400, 100, 'Error', message, Ext.Msg.OK, null);
                  }
                });
              }
            }]
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
              Ext.getCmp('content-pane').getEl().unmask();
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
                url : 'reset?dtoContext=' + escape(panel.context.substring(1))
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

  store.on('load', function() {
    var dtoTab = Ext.getCmp('content-pane').getActiveTab();
    var label = dtoTab.id.substring(4);
    var dtoBcPane = dtoTab.items.map['nav-' + label].items.map['bc-' + label];

    // remove old bc and content pane on refresh
    var lastBcItem = dtoBcPane.items.items[dtoBcPane.items.length - 1].autoEl.html;
    if (removeHTMLTag(lastBcItem) == className) {
      navigate(dtoBcPane.items.length - 3);
    }

    var dtoContentPane = dtoTab.items.map['dto-' + label];
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
        html : '<a class="breadcrumb" href="#" onclick="navigate(' + bcItemIndex + ')">' + className + '</a>'
      }
    });

    dtoBcPane.doLayout();
    dtoContentPane.add(createGridPane(store, pageSize, {
      forceFit : false
    }, true));
    dtoContentPane.getLayout().setActiveItem(dtoContentPane.items.length - 1);
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
      title : 'Transfer type of selected row is "' + tansferType.toUpperCase() + '"',
      // iconCls: 'tabsApplication',
      height : 300,
      width : 600,
      plain : true,
      items : [ propertyGrid ]
    /*
     * listeners: { beforelayout: function (pane) { //alert('before layout..');
     * Ext.getBody().unmask(); } }
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
            + classIdentifier + ')</td></tr></table></div>'
      });

  var rowData = dtoGrid.selModel.selections.map[dtoGrid.selModel.last].data;
  delete rowData['&nbsp;']; // remove info field

  var parsedRowData = {};
  for ( var colData in rowData)
    parsedRowData[colData] = removeHTMLTag(rowData[colData]);

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
               * After trial and error, the ctrl+c combination seems to be code
               * 67
               */
              if (67 == 67)// if (keyPressed == 67)
              {
                var celldata = Ext.getCmp('property-pane').getSelectionModel().events.beforecellselect.obj.selection.record.data.value;
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

    relatedItemPane.add({
      xtype : 'box',
      autoEl : {
        tag : 'div',
        html : '<a class="breadcrumb" href="#" onclick="loadRelatedItem(\'' + dtoTabType + '\',\'' + dtoTabContext
            + '\',\'' + dtoIdentifier + '\',\'' + relatedClassId + '\',\'' + relatedClassName + '\')">'
            + relatedClassName + '</a>'
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
      html : '<a class="breadcrumb" href="#" onclick="navigate(' + bcItemIndex + ')">' + classIdentifier + '</a>'
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

  if (userResponse == 'ok') {
    if (exchtab) {
      exchtab.getEl().mask('Exchange in progress, please wait ...', 'x-mask-loading');
    }

    Ext.Ajax.request({
      url : 'xsubmit?scope=' + scope + '&xid=' + xid + '&reviewed=' + reviewed,
      timeout : 86400000, // 24 hours
      success : function(response, request) {
        if (exchtab) {
          exchtab.getEl().unmask();
        }

        var responseText = Ext.decode(response.responseText);
        var message = 'Data exchange [' + exchange + ']: ' + responseText;

        if (message.length < 300)
          showDialog(460, 125, 'Exchange Result', message, Ext.Msg.OK, null);
        else
          showDialog(660, 300, 'Exchange Result', message, Ext.Msg.OK, null);
      },
      failure : function(response, request) {
        // ignore timeout error from proxy server
        if (response.responseText.indexOf('Error Code 1460') != -1) {
          if (exchtab) {
            exchtab.getEl().unmask();
          }

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
  var style = 'style="margin:0;padding:0;width:' + width + 'px;height:' + height
      + 'px;border:1px solid #aaa;overflow:auto"';

  Ext.Msg.show({
    title : title,
    msg : '<textarea ' + style + ' readonly="yes">' + message + '</textarea>',
    buttons : buttons,
    fn : callback
  });
}

function onTreeItemContextMenu  (node, e)
{
	  var obj = node;
	  var directoryTree = Ext.getCmp('directory-tree');
var x = e.browserEvent.clientX;
var y = e.browserEvent.clientY;

if(node != null)
	{// var obj = node;
if ((obj !== null))
{
    if(obj.parentNode !== null)
    {
        var dataTypeNode = obj.parentNode.text;
        if( obj.parentNode.parentNode !== null)
        {
            var dataExchangeNode = obj.parentNode.parentNode.text;
            if (dataExchangeNode !== null && dataExchangeNode === 'Data Exchanges'){

             //   var dataExchangeMenu = Ext.widget('dataexchangemenu');
            	commodityMenu.showAt([ x, y ]);
                e.stopEvent();
            } }

            if (dataTypeNode !== null && dataTypeNode === 'Data Exchanges'){ 

                // if (node.isSelected()) {

                //	var obj = node.attributes;

                // if (obj.type == "ExchangeNode") {
            	newExchangemenu.showAt([ x, y ]);
                e.stopEvent();
                // this.MenuClick(obj);
                // }
            }else if(obj.parentNode.text === 'Directory')
            {
            	editDeleteScopeMenu.showAt([ x, y ]);
                e.stopEvent(); 
            } 
            else if((obj.text === 'Application Data'))
            {
            	newappmenu.showAt([ x, y ]);
                e.stopEvent(); 
            }else if((obj.parentNode.text === 'Application Data'))
            {
            	applicationMenu.showAt([ x, y ]);
                e.stopEvent(); 
            }else if((obj.parentNode.parentNode.text === 'Application Data'))
            {
            	graphSubMenu.showAt([ x, y ]);
                e.stopEvent()

            } else if((obj.text === 'Data Exchanges'))
            {
            	newCommoditymenu.showAt([ x, y ]);
                e.stopEvent(); 
            }}
            else if (obj.text === 'Directory')
            {
            	newScopemenu.showAt([ x, y ]);
                e.stopEvent();
            }

        }
	}
directoryTree.getSelectionModel().select(node);
}

function buildEditDeleteSubMenu () 
{
	 return  [
              {
                  xtype: 'menuitem',
                  handler: function(node, event) {
                 	              newScope(node, event);
                 	             editDeleteScope(node, event);
                                  },
                  text: 'Edit Scope'
              },
              {
                  xtype: 'menuitem',
                 handler: function(node, event) {
                	 deleteScope(node, event);
                                  },
                  text: 'Delete Scope'
              }
          ]
}
function buildNewCommodityMenu () 
{
	 return[
            {
                xtype: 'menuitem',
                handler: function(node, event) {
                	        
                	newCommodity(node, event);
                    var view = Ext.getCmp('newCommWin');
                      //var view = Ext.widget('editDir');
                    view.show();
                },
                text: 'New Commodity'
            }
        ]
}
function deleteApp(node, event)
{
	
//var node = Ext.getCmp('directory-tree').getSelectionModel().getSelectedNode();
	var appName = node.text;
	var scope = node.parentNode.parentNode.text;

	Ext.Msg.show({
	    //  title: 'Choose',
	    msg: 'Are you sure to delete '+ appName + ' Application ?',
	    buttons: Ext.MessageBox.YESNOCANCEL,
	    modal: true,
	    // buttons: Ext.MessageBox.OKCANCEL,
	    //inputField: new IMS.form.DateField(),
	    fn: function(buttonId, text) {
	        //  if (buttonId == 'ok')
	        if(buttonId == 'yes'){

	            Ext.Ajax.request({
	                url : 'deleteApplication?'+'&scope =' + scope +'&appName =' + appName,
	                method: 'POST',
	                timeout : 86400000, // 24 hours
	                success : function(response, request) {
	        //          Ext.Msg.alert('"'+ appName + '"' +'  Application is deleted');
	                   refresh();
	                },
	                failure : function(response, request) {
	                    Ext.Msg.alert('Delete Failed');
	                } });  
	            }
	        }
	    });
}

function deleteGraph(node, event)
{
//var node = Ext.getCmp('directory-tree').getSelectionModel().getSelectedNode();
	var name = node.text;
	var appName = node.parentNode.text;
	var scope = node.parentNode.parentNode.parentNode.text;

	Ext.Msg.show({
	    //  title: 'Choose',
	    msg: 'Are you sure to delete '+ name + ' Graph ?',
	    buttons: Ext.MessageBox.YESNOCANCEL,
	    modal: true,
	    // buttons: Ext.MessageBox.OKCANCEL,
	    //inputField: new IMS.form.DateField(),
	    fn: function(buttonId, text) {
	        //  if (buttonId == 'ok')
	        if(buttonId == 'yes'){

	            Ext.Ajax.request({
	                url : 'deleteGraph?'+ '&name ='+ name +'&scope =' + scope +'&appName =' + appName,
	                method: 'POST',
	                timeout : 86400000, // 24 hours
	                success : function(response, request) {
	       //           Ext.Msg.alert('"'+ name + '"' +'  Graph is deleted');
	                    refresh();
	                },
	                failure : function(response, request) {
	                    Ext.Msg.alert('Delete Failed');
	                } });  
	            }
	        }
	    });
}

function deleteCommodity(node, event)
{
//var node = Ext.getCmp('directory-tree').getSelectionModel().getSelectedNode();
	var commName = node.text;
	var scope = node.parentNode.parentNode.text;

	Ext.Msg.show({
	    //  title: 'Choose',
	    msg: 'Are you sure to delete '+ commName + ' Commodity ?',
	    buttons: Ext.MessageBox.YESNOCANCEL,
	    modal: true,
	    // buttons: Ext.MessageBox.OKCANCEL,
	    //inputField: new IMS.form.DateField(),
	    fn: function(buttonId, text) {
	        //  if (buttonId == 'ok')
	        if(buttonId == 'yes'){

	            Ext.Ajax.request({
	                url : 'deleteCommodity?'+ '&commName =' + commName +'&scope =' + scope ,
	                method: 'POST',
	                timeout : 86400000, // 24 hours
	                success : function(response, request) {
	            //      Ext.Msg.alert('"'+ commName + '"' +'  Commodity is deleted');
	                  refresh();
	                },
	                failure : function(response, request) {
	                    Ext.Msg.alert('Delete Failed');
	                } });  
	            }
	        }
	    });
}

function deleteConfig()
{
	 var node = Ext.getCmp('directory-tree').getSelectionModel().getSelectedNode();
	var exchangeConfigName = node.text;
	var commName = node.parentNode.text;
	var scope = node.parentNode.parentNode.parentNode.text;

	Ext.Msg.show({
	    //   title: 'Choose',
	    msg: 'Are you sure to delete '+ '"' +exchangeConfigName+'"' + ' Exchange ?',
	    buttons: Ext.MessageBox.YESNOCANCEL,
	    modal: true,
	    // buttons: Ext.MessageBox.OKCANCEL,
	    //inputField: new IMS.form.DateField(),
	    fn: function(buttonId, text) {
	        //  if (buttonId == 'ok')
	        if(buttonId == 'yes'){

	            Ext.Ajax.request({
	                url : 'deleteExchangeConfig?'+'&scope =' + scope +'&commName =' + commName +'&name =' +exchangeConfigName,
	                method: 'POST',
	                timeout : 86400000, // 24 hours
	                success : function(response, request) {
	  //                Ext.Msg.alert('"'+ exchangeConfigName + '"' +' Exchange is deleted');
	                   refresh();
	                },
	                failure : function(response, request) {
	                    Ext.Msg.alert('Delete Failed');
	                } });  
	            }
	        }
	    });
}
function buildNewExchangeMenu () 
{
	 return[
            {
                xtype: 'menuitem',
                handler: function(item, event) {
                	newexchangeConfig();
                    var view = Ext.getCmp('newExchangeConfigWin');
                    view.show();
                },
                text: 'New Exchange Configuration'
            },
        {
            xtype: 'menuitem',
           handler: function(item, event) {
        	   newCommodity();
        	   editCommodity();
            },
            text: 'Edit Commodity'
        },
        {
            xtype: 'menuitem',
            handler: function(item, event) {
            	deleteCommodity();
            },
            text: 'Delete commodity'
        }
        ]
}

function newexchangeConfig()  
{
	var newExchConfig= new Ext.FormPanel({
		 id:'newExchConfig',
		 height: 596,
		    width: 584,
		    bodyPadding: 10,
		    layout:'form',
		    frame: true,
		    bodyStyle:'padding:5px 5px 0',
		    labelwidth: 75,
		
		    items: [
	                {
	                    xtype: 'textfield',
	                    anchor: '90%',
	                    padding: 0,
	                    fieldLabel: 'Name',
	                    labelWidth: 125,
	                    name: 'name'
	                },
	                {
	                    xtype: 'textfield',
	                    anchor: '90%',
	                    fieldLabel: 'Description',
	                    labelWidth: 125,
	                    name: 'description'
	                },
	                {
	                    xtype: 'fieldset',
	                    height: 212,
	                    width: 553,
	                    title: 'Source Config',
	                    items: [
	                        {
	                            xtype: 'textfield',
	                            anchor: '90%',
	                            fieldLabel: 'Source Uri',
	                            labelWidth: 125,
	                            name: 'sourceUri'
	                        },
	                        {
	                            xtype: 'textfield',
	                            anchor: '90%',
	                            fieldLabel: 'Source Scope Name',
	                            labelWidth: 125,
	                            name: 'sourceScopeName'
	                        },
	                        {
	                            xtype: 'textfield',
	                            anchor: '90%',
	                            fieldLabel: 'Source App Name',
	                            labelWidth: 125,
	                            name: 'sourceAppName'
	                        },
	                        {
	                            xtype: 'textfield',
	                            anchor: '90%',
	                            fieldLabel: 'Source Graph Name',
	                            labelWidth: 125,
	                            name: 'sourceGraphName'
	                        },
	                     
	                    ],
	                    buttons: [{
	                    	 text: 'Test',
	                    	   handler: function(button, event) {
	                    		   SourceUri();

	                            }	                            
	                           }]
	                },
	                {
	                    xtype: 'fieldset',
	                    height: 220,
	                    margin: '',
	                    width: 537,
	                    title: 'Target Config',
	                    items: [
	                        {
	                            xtype: 'textfield',
	                            anchor: '90%',
	                            fieldLabel: 'Target Uri',
	                            labelWidth: 125,
	                            name: 'targetUri'
	                        },
	                        {
	                            xtype: 'textfield',
	                            anchor: '90%',
	                            fieldLabel: 'Target Scope Name',
	                            labelWidth: 125,
	                            name: 'targetScopeName'
	                        },
	                        {
	                            xtype: 'textfield',
	                            anchor: '90%',
	                            fieldLabel: 'Target App Name',
	                            labelWidth: 125,
	                            name: 'targetAppName'
	                        },
	                        {
	                            xtype: 'textfield',
	                            anchor: '90%',
	                            fieldLabel: 'Target Graph Name',
	                            labelWidth: 125,
	                            name: 'targetGraphName'
	                        },
	                        
	                    ],
	                    buttons: [{
	                    	 text: 'Test',
	                    	   handler: function(button, event) {
	                                TargetUri();

	                            }	                            
	                           }]
	                },
	               
	                {
	                    xtype: 'hidden',
	                    anchor: '100%',
	                    fieldLabel: 'Label',
	                    name: 'oldConfigName'
	                },
	                {
	                    xtype: 'hidden',
	                    anchor: '100%',
	                    fieldLabel: 'Label',
	                    name: 'oldCommName'
	                },
	                {
	                    xtype: 'hidden',
	                    anchor: '100%',
	                    fieldLabel: 'Label',
	                    name: 'oldScope'
	                }
	            ],
	            buttons: [{
	                text: 'Save',
	                handler: function(button, event) {
                     saveExchangeConfig();
                 	newExchangeConfigWin.close();
                    }
	              
	            },{
	                text: 'Cancel',
	                handler: function(button, event) {
	                	newExchangeConfigWin.close();
	            }}]
	        });
	var newExchangeConfigWin = new Ext.Window({
		id:'newExchangeConfigWin',
		  height: 515,
		    width: 568,
	    layout: {
	        type: 'fit'
	    },
	    title: 'New Exchange Configration',
	    modal: true,
	    items:[newExchConfig]
	            
	        });
}
	
	function SourceUri()
	{
		var obj = Ext.getCmp('newExchConfig');
		var form = obj.getForm();
	
		//console.log("form ! and..."+form.findField('id').getValue());
		var source = form.findField('sourceUri').getValue();
		var scope = form.findField('sourceScopeName').getValue();
		var app = form.findField('sourceAppName').getValue();
		var sourceUri = source+"/"+scope+"/"+app+"/manifest";
		console.log("1 is " + sourceUri);
		Ext.Ajax.request({
		    url : 'SourcetestUri?' + '&sourceUri =' + sourceUri,
		    method: 'POST',
		    timeout : 86400000, // 24 hours
		    success : function(response, request) {
		        var result = Ext.decode(response.responseText);
		        alert(result);
		    },
		    failure : function(response, request) {
		        alert("failed to connect to the specified Url");
		    }
		});
	}
	
	function TargetUri()
	{
		var obj = Ext.getCmp('newExchConfig');
		var form = obj.getForm();
		var target = form.findField('targetUri').getValue();
		var scope = form.findField('targetScopeName').getValue();
		var app = form.findField('targetAppName').getValue();
		var targetUri =target+"/"+scope+"/"+app+"/manifest";
		console.log("1 is " + targetUri);
		Ext.Ajax.request({
		    url : 'testTargetUrl?' + '&targetUri =' + targetUri,
		    method: 'POST',
		    timeout : 86400000, // 24 hours
		    success : function(response, request) {
		        var result = Ext.decode(response.responseText);
		        alert(result);
		    },
		    failure : function(response, request) {
		        alert("failed to connect to the specified Url");
		    }
		});
		
	}
	

function saveExchangeConfig()
{
	 var node = Ext.getCmp('directory-tree').getSelectionModel().getSelectedNode();
	 var obj = Ext.getCmp('newExchConfig').getForm();
	var form = obj.getValues(true);
	var scope = node.parentNode.parentNode.text;
	var commodity = node.text;
	//var xid = node.attributes.properties['Id'];


	Ext.Ajax.request({
	    //url : 'newExchange?form=' + form,
	    url : 'newExchange?' + form +'&scope ='+ scope + '&commodity ='+ commodity,
	    method: 'POST',
	    timeout : 86400000, // 24 hours
	    success : function(response, request) {
	     refresh();
	    },
	    failure : function(response, request) {
	        alert("save failed");
	    }
	});
}
function buildNewScopeMenu () 
{
	 return {
         xtype: 'menuitem',
         handler: function(item, event) {
        	 newScope();
        	
        	 var view = Ext.getCmp('newScopeWin');
             var value = Ext.getCmp('newScopeForm').getForm().findField("oldScope").setValue('null');
             view.show();
                         },
       // icon: 'resources/images/16x16/add.png',
         text: 'New Scope'
     }
}
function buildNewApplicationMenu () 
{
	 return  {
         xtype: 'menuitem',
         handler: function(item, event) {
        	 newApp();
                             var view = Ext.getCmp('newAppWin');
                       //           var view = Ext.widget('newScopeDir');
           
                             view.show();
                         },
        // icon: 'resources/images/16x16/add.png',
         text: 'New Application'
     }
}
function buildGraphSubMenu () 
{
	 return [
             {
                 xtype: 'menuitem',
                handler: function() {
                    	 newGraph();
                    	 editGraph();
                    },
                 text: 'Edit Graph'
             },
             {
                 xtype: 'menuitem',
                 handler: function(item, event) {
                	 deleteGraph();
                                  },
                 text: 'Delete Graph'
             }
         ]
}
function editCommodity()
{
	var centerPanel = Ext.getCmp('content-pane');
	centerPanel.getEl().mask("Loading...", "x-mask-loading");
	 var node = Ext.getCmp('directory-tree').getSelectionModel().getSelectedNode();
	 var scope = node.parentNode.parentNode.text;
	 var commNameValue = node.text;

	 var view = Ext.getCmp('newCommWin');
	 view.setTitle("Edit Commodity");
	 var obj =Ext.getCmp('newCommForm');
	 var form = obj.getForm();
	 //form.setValues({scope: scopevalue, oldScope: scopevalue});
	 //form.setValues({scope: scopevalue});

	 Ext.Ajax.request({
	     url:'getComm?' + '&scope =' + scope +'&commName =' + commNameValue,
	     method: 'POST',
	     timeout : 86400000, // 24 hours
	     success : function(response, request) {
	         // alert("saved successfuly");
	         // var application =  obj.getForm().setValues(Ext.JSON.decode(response.data));
	         var comDetails = Ext.decode(response.responseText);
	         form.setValues({commName : comDetails.name, oldCommName : commNameValue, oldScope :scope});   
	         centerPanel.getEl().unmask();
	         view.show();
	     },
	     failure : function(response, request) {
	         centerPanel.getEl().unmask();
	         alert("Error fetching data to fill form");
	     }
	 });
}

function editGraph()
{
	var centerPanel = Ext.getCmp('content-pane');
	centerPanel.getEl().mask("Loading...", "x-mask-loading");
	 var node = Ext.getCmp('directory-tree').getSelectionModel().getSelectedNode();
	var scope = node.parentNode.parentNode.parentNode.text;
	var graphValue = node.text;
	var appNameValue = node.parentNode.text;

	var view = Ext.getCmp('newGraphWin');
	view.setTitle("Edit Graph");
	var obj = Ext.getCmp('newGraphForm');
	var form = obj.getForm();

	Ext.Ajax.request({
	    url:'getGraph?' + '&scope =' + scope +'&appName =' + appNameValue + '&name =' + graphValue,
	    method: 'POST',
	    timeout : 86400000, // 24 hours
	    success : function(response, request) {
	        var graph = Ext.decode(response.responseText);
	        form.setValues({name : graph.name, description : graph.description,  CommName : graph.commodity, oldAppName : appNameValue, oldScope :scope, oldGraphName : graphValue});   
	        centerPanel.getEl().unmask();
	        view.show();
	    },
	    failure : function(response, request) {
	        centerPanel.getEl().unmask();
	        alert("Error fetching data to fill form");
	    }
	});

}

function newGraph()
{
	var newGraphForm= new Ext.FormPanel({
	id: 'newGraphForm',
	 height: 120,
	    width: 400,
	    bodyPadding: 10,
	    layout:'form',
	    frame: true,
	    bodyStyle:'padding:5px 5px 0',
	    labelwidth: 75,
	    items: [
                {
                    xtype: 'textfield',
                    anchor: '95%',
                    fieldLabel: 'Graph Name',
                    name: 'name'
                },
                {
                    xtype: 'textfield',
                    anchor: '95%',
                    fieldLabel: 'Description',
                    name: 'description'
                },
                {
                    xtype: 'textfield',
                    anchor: '95%',
                    fieldLabel: 'Commodity',
                    name: 'CommName'
                },
                {
                    xtype: 'hidden',
                    anchor: '100%',
                    fieldLabel: 'Label',
                    name: 'oldScope'
                },
                {
                    xtype: 'hidden',
                    anchor: '100%',
                    fieldLabel: 'Label',
                    name: 'oldAppName'
                },
                {
                    xtype: 'hidden',
                    anchor: '100%',
                    fieldLabel: 'Label',
                    name: 'oldGraphName'
                }
            ],
            buttons: [{
                text: 'Save',
                handler: function(node,button, event){
              	  saveGraph();
              	newGraphWin.close();
                }
               // margin: 10,
            },{
                text: 'Cancel',
                handler: function(button, event) {
                	newGraphWin.close();
            }}]
                });
	
	var newGraphWin = new Ext.Window({
		id:'newGraphWin',
	    height: 120,
	    width: 400,
	    layout: {
	        type: 'fit'
	    },
	    title: 'New Graph',
	    modal: true,
	    items:[newGraphForm]
	            
	        });
       
}

function saveGraph() 
{var me = this;
var obj = Ext.getCmp('newGraphForm').getForm();
var form = obj.getValues(true);
var directoryTree = Ext.getCmp('directory-tree');
var node = directoryTree.getSelectionModel().getSelectedNode();
var scope = node.parentNode.parentNode.text;
var appName = node.text;

Ext.Ajax.request({
    url : 'newGraph?' + form +'&scope =' + scope + '&appName =' + appName,
    method: 'POST',
    timeout : 86400000, // 24 hours
    success : function(response, request) {
       //lert("saved successfuly");
      
    	 refresh();
    },
    failure : function(response, request) {
        alert("save failed");
    }
});
	}

function buildApplicationSubMenu () 
{
	 return  [
                {
                    xtype: 'menuitem',
                    handler: function() {
                    	 newApp();
                    	editApplication();
                    },
                    text: 'Edit Application'
                },
                {
                    xtype: 'menuitem',
                    handler: function() {
                    	 deleteApp();
                    },
                    text: 'Delete Application'
                },
                {
                    xtype: 'menuitem',
                    handler: function() {
                    	 newGraph();
                        var view = Ext.getCmp('newGraphWin');
                        //var view = Ext.widget('editDir');
                        view.show();
                    },
                    text: 'Add New Graph'
                }
            ]
}
function editApplication()
{
	var centerPanel = Ext.getCmp('content-pane');
	centerPanel.getEl().mask("Loading...", "x-mask-loading");
	 var node = Ext.getCmp('directory-tree').getSelectionModel().getSelectedNode();
	
	var scope = node.parentNode.parentNode.text;
	var appNameValue = node.text;

	var view = Ext.getCmp('newAppWin');
	view.setTitle("Edit Application");
	var obj = Ext.getCmp('newAppForm');
	var form = obj.getForm();
	//form.setValues({scope: scopevalue, oldScope: scopevalue});
	//form.setValues({scope: scopevalue});

	Ext.Ajax.request({
	    url:'getApplication?' + '&scope =' + scope +'&appName =' + appNameValue,
	    method: 'POST',
	    timeout : 86400000, // 24 hours
	    success : function(response, request) {
	        // alert("saved successfuly");
	        // var application =  obj.getForm().setValues(Ext.JSON.decode(response.data));
	        var application = Ext.decode(response.responseText);
	        form.setValues({appName : application.name, appDesc : application.description,  baseUri: application.baseUri, oldAppName : appNameValue, oldScope :scope});   
	        centerPanel.getEl().unmask();
	        view.show();
	    },
	    failure : function(response, request) {
	        centerPanel.getEl().unmask();
	        alert("Error fetching data to fill form");
	    }
	});
	}
function buildCommoditySubMenu () 
{
	 return[
     {
         xtype: 'menuitem',
         handler: function() {
        	   newexchangeConfig();
        	   editExchangeConfig();
         },
         text: 'Edit Exchange Configuration'
     },
     {
         xtype: 'menuitem',
         handler: function() {
        	 deleteConfig();
         },
         text: 'Delete Exchange Configuration'
     },
     {
         xtype: 'menuitem',
         action: 'dataFiltersMenuItem',
         text: 'Apply Data Filters'
     }  /*,
   {
         xtype: 'menuitem',
         action: 'exchangereviewandacceptance',
         text: 'Review and Acceptance'
     },
     {
         xtype: 'menuitem',
         action: 'exchange',
         icon: 'resources/images/16x16/exchange-send.png',
         text: 'Execute Exchange'
     },
     {
         xtype: 'menuitem',
         action: 'exchangeHistory',
         icon: 'resources/images/16x16/history.png',
         text: 'Show History'
     },
     {
         xtype: 'menuitem',
         action: 'exchangeSummary',
         icon: 'resources/images/16x16/file-table.png',
         text: 'Show Summary'
     }*/
 ]
}
function editExchangeConfig()
{
	var centerPanel = Ext.getCmp('content-pane');
	centerPanel.getEl().mask("Loading...", "x-mask-loading");
	 var node = Ext.getCmp('directory-tree').getSelectionModel().getSelectedNode();
	 var scope = node.parentNode.parentNode.parentNode.text;
	 var commodity = node.parentNode.text;
	 var commConfigName = node.text;

	 var view = Ext.getCmp('newExchangeConfigWin');
	 view.setTitle("Edit Exchange Configaration");
	 var obj = Ext.getCmp('newExchConfig');
	 var formdata =  obj.getForm();

	 Ext.Ajax.request({
	     url : 'getExchange', //+'&scope ='+ scope + '&commName ='+ commodity +'&name =' + commConfigName,
	     params: { scope : scope, commName : commodity, name : commConfigName },
	     method: 'POST',
	     timeout : 86400000, // 24 hours
	     success : function(response, request) {
	         var form = Ext.decode(response.responseText);
	         formdata.setValues({name : form.name,description : form.description, sourceUri : form.sourceUri, sourceScopeName : form.sourceScope, sourceAppName : form.sourceApp, sourceGraphName : form.sourceGraph , targetUri :form.targetUri, targetScopeName : form.targetScope, targetAppName : form.targetApp, targetGraphName : form.targetGraph, oldConfigName : commConfigName, oldCommName : commodity, oldScope :scope});   
	         centerPanel.getEl().unmask();
	         view.show();
	     },
	     failure : function(response, request) {
	         centerPanel.getEl().unmask();
	         alert("Error fetching data to fill form");
	     }
	 });

}

function deleteScope()
{
	var me = this;
	 var node = Ext.getCmp('directory-tree').getSelectionModel().getSelectedNode();
	var scope = node.text;

	Ext.Msg.show({
	    //   title: 'Choose',
	    msg: 'Are you sure to delete '+ scope + ' context ?',
	    buttons: Ext.MessageBox.YESNOCANCEL,
	    modal: true,
	    // buttons: Ext.MessageBox.OKCANCEL,
	    //inputField: new IMS.form.DateField(),
	    fn: function(buttonId, text) {
	        //  if (buttonId == 'ok')
	        if(buttonId == 'yes'){

	            Ext.Ajax.request({
	                url : 'deleteScope?'+'&scope =' + scope,
	                method: 'POST',
	                timeout : 86400000, // 24 hours
	                success : function(response, request) {
	           //       Ext.Msg.alert('"'+ scope + '"' +' Context is deleted');
	                refresh();
	                },
	                failure : function(response, request) {
	                    Ext.Msg.alert('Delete Failed');
	                } });  
	            }
	        }
	    });
	}
function editDeleteScope()
{var me = this;
var centerPanel = Ext.getCmp('content-pane');
centerPanel.getEl().mask("Loading...", "x-mask-loading");
 var node = Ext.getCmp('directory-tree').getSelectionModel().getSelectedNode();
var scopevalue = node.text;

var view = Ext.getCmp('newScopeWin');
view.setTitle("Edit Scope");
var obj =Ext.getCmp('newScopeForm');
var form = obj.getForm();
Ext.Ajax.request({
    url:'getScope?' + '&scope =' + scopevalue,
    method: 'POST',
    timeout : 86400000, // 24 hours
    success : function(response, request) {
        // alert("saved successfuly");
        var newScopeAdd = Ext.decode(response.responseText);
        
        form.setValues({scope : newScopeAdd.name, oldScope : scopevalue });
        centerPanel.getEl().unmask();
        view.show();
    },
    failure : function(response, request) {
        centerPanel.getEl().unmask();
        alert("Error fetching data to fill form");
    }
});
	}
function newScope()
{
	var newScopeForm = new Ext.FormPanel({
		 id: 'newScopeForm',
		 height: 120,
		    width: 400,
		    bodyPadding: 10,
		    layout:'form',
		    frame: true,
		    bodyStyle:'padding:5px 5px 0',
		    labelwidth: 75,
		    items: [
                    {
                    	 xtype: 'textfield',
                         anchor: '95%',
                         fieldLabel: 'Scope Name',
                         name: 'scope'
                    },
                
                    {
                        xtype: 'hidden',
                        anchor: '100%',
                        fieldLabel: 'Label',
                        name: 'oldScope'
                    }
                ],
                buttons: [{
                    text: 'Save',
                    handler: function(node,button, event){
                  	  saveScope(node,button, event);
                  	  newScopeWin.close();
                    }
                   // margin: 10,
                },{
                    text: 'Cancel',
                    handler: function(button, event) {
                    	newScopeWin.close();
                }}]
                    });
	var newScopeWin = new Ext.Window({
		id:'newScopeWin',
	    height: 120,
	    width: 400,
	    layout: {
	        type: 'fit'
	    },
	    title: 'New Scope',
	    modal: true,
	    items:[newScopeForm]
	            
	        });
	
}
function saveScope(node,button, event){
	var me = this;
	var obj =Ext.getCmp('newScopeForm').getForm();
	var form = obj.getValues(true);
var directoryTree = Ext.getCmp('directory-tree');
var node = directoryTree.getSelectionModel().getSelectedNode();
	console.log("1 is " + form); // console.log("2 is " + form1);
  // newScope();
	Ext.Ajax.request({
	    url : 'newScope?' + form,
	    method: 'POST',
	    timeout : 86400000, // 24 hours
	    success : function(response, request) {
	     // alert("saved successfuly");
	       // newScopeWin.close();
	        //Ext.getCmp('newCommForm').close();
	       //newScopeWin.
	        refresh();
	    },
	    failure : function(response, request) {
	        alert("save failed");
	    }
	});
}

function newCommodity()
{  var newCommForm = new Ext.FormPanel({
	id: 'newCommForm',
    height: 94,
    width: 400,
    bodyPadding: 10,
    layout:'form',
    frame: true,
    bodyStyle:'padding:5px 5px 0',
    labelwidth: 75,
    items: [
            {
                xtype: 'textfield',
                anchor: '100%',
                fieldLabel: 'Commodity Name',
                name: 'commName'
            }
        ],
buttons: [{
    text: 'Save',
    handler: function(node,button, event){
  	  saveComm(node,button, event);
  	  newCommWin.close();
    }
   // margin: 10,
},{
    text: 'Cancel',
    handler: function(button, event) {
    	newCommWin.close();
}}]
    });

var newCommWin = new Ext.Window({
	id:'newCommWin',
    height: 152,
    width: 400,
    layout: {
        type: 'fit'
    },
    title: 'New Commodity',
    modal: true,
    items:[newCommForm]
            
        });

   
}
function saveComm(node,button, event){
	var me = this;
	var obj =Ext.getCmp('newCommForm').getForm();
	var form = obj.getValues(true);
	var directoryTree = Ext.getCmp('directory-tree');
 var node = directoryTree.getSelectionModel().getSelectedNode();
//r node = directoryTree.getSelectedNode();
	var scope = node.parentNode.text;

	Ext.Ajax.request({
	    url : 'newComm?' + form + '&scope=' + scope,
	    method: 'POST',
	    timeout : 86400000, // 24 hours
	    success : function(response, request) {
	    //  alert("saved successfuly");
	      
	       refresh();
	    },
	    failure : function(response, request) {
	        alert("save failed");
	    }
	});
}
function newApp()
{  var newAppForm = new Ext.FormPanel({
	  id: 'newAppForm',
	    height: 120,
	    width: 400,
	    bodyPadding: 10,
	    layout:'form',
	    frame: true,
	    bodyStyle:'padding:5px 5px 0',
	    labelwidth: 75,
	    items: [
              {
                  xtype: 'textfield',
                  anchor: '95%',
                  fieldLabel: 'Application Name',
                  name: 'appName'
              },
              {
                  xtype: 'textfield',
                  anchor: '95%',
                  fieldLabel: 'Description',
                  name: 'appDesc'
              },
              {
                  xtype: 'textfield',
                  anchor: '95%',
                  fieldLabel: 'BaseUrl',
                  name: 'baseUri'
              },
              {
                  xtype: 'hidden',
                  anchor: '100%',
                  fieldLabel: 'Label',
                  name: 'oldAppName'
              },
              {
                  xtype: 'hidden',
                  anchor: '100%',
                  fieldLabel: 'Label',
                  name: 'oldScope'
              },
            /*  {
                  xtype: 'button',
                  handler:  function(){
                	  saveApp(node,button, event);
                  },
                  margin: 15,
                  text: 'Save'
              },*/
          ],
          buttons: [{
              text: 'Save',
              handler: function(node,button, event){
            	  saveApp(node,button, event);
            	  newAppWin.close();
              }
             // margin: 10,
          },{
              text: 'Cancel',
              handler: function(button, event) {
              	newAppWin.close();
          }}]
      });

var newAppWin = new Ext.Window({
	id:'newAppWin',

	    height: 120,
	    width: 400,
	    layout: {
	        type: 'fit'
	    },
	    closable:true,
	    title: 'New Application',
	    modal: true,
	    items: [newAppForm]
          //     xtype: 'newApp'
           
      
});
	}
function saveApp(node,button, event)
{var me = this;
var obj = Ext.getCmp('newAppForm').getForm();
var form = obj.getValues(true);
var directoryTree = Ext.getCmp('directory-tree');
var node = directoryTree.getSelectionModel().getSelectedNode();
var scope = node.parentNode.text;

Ext.Ajax.request({
    url : 'newApplication?' + form +'&scope =' + scope,
    method: 'POST',
    timeout : 86400000, // 24 hours
    success : function(response, request) {
     // alert("saved successfuly");
        
        refresh();
    },
    failure : function(response, request) {
        alert("save failed");
    }
});
	}

function refresh(){
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
Ext
    .onReady(function() {
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
      
      //exchangeMenu.add(this.buildExchangeMenu());
      Ext.QuickTips.init();
    /*  this.control({
         
          "treepanel": {
              itemclick: this.onTreeItemClick,
              itemdblclick: this.onTreeItemDblClick,
              itemcontextmenu: this.onTreeItemContextMenu
          }});*/
          

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
            tbar : new Ext.Toolbar({
              items : [ {
                id : 'refresh-button',
                xtype : 'button',
                icon : 'resources/images/16x16/view-refresh.png',
                text : 'Refresh',
                handler : function() {
                	refresh();
                  
                }
              }, {
                id : 'exchange-button',
                xtype : 'button',
                icon : 'resources/images/16x16/exchange-send.png',
                text : 'Exchange',
                disabled : true,
                handler : function() {
                  var node = Ext.getCmp('directory-tree').getSelectionModel().getSelectedNode();
                  var scope = node.parentNode.parentNode.parentNode.attributes['text'];
                  var exchange = node.attributes["text"];
                  var xid = node.attributes.properties['Id'];
                  var reviewed = (node.reviewed != undefined);
                  var msg = 'Are you sure you want exchange data \r\n[' + exchange + ']?';
                  var processUserResponse = submitExchange.createDelegate([ exchange, scope, xid, reviewed ]);
                  showDialog(460, 125, 'Exchange Confirmation', msg, Ext.Msg.OKCANCEL, processUserResponse);
                }
              }, {
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
              nodeType : 'async', // only load child nodes as
              // needed
              text : 'Directory',
              icon : 'resources/images/directory.png'
            },
            listeners : {
              click : function(node, event) {
            	 if(node.attributes != null)
            		 {
            		 if(node.attributes.properties != null)
            			 {
              Ext.getCmp('property-pane').setSource(node.attributes.properties);
            		 }
            		 }
                try {
                	if(node.parentNode != null)
                		{
                		if(node.parentNode.parentNode != null)
                			{
                			 var dataTypeNode = node.parentNode.parentNode;

                             if (dataTypeNode != null && dataTypeNode.attributes['text'] == 'Data Exchanges') {
                               Ext.getCmp('exchange-button').enable();
                               Ext.getCmp('xlogs-button').enable();
                             } else {
                               Ext.getCmp('exchange-button').disable();
                               Ext.getCmp('xlogs-button').disable();
                             }
                			}
                		}
                 
                } catch (err) {
                }
              },
              dblclick : function(node, event) {
                var properties = node.attributes.properties;
                Ext.getCmp('property-pane').setSource(node.attributes.properties);

                try {
                  var dataTypeNode = node.parentNode.parentNode;

                  if (dataTypeNode != null) {
                    if (dataTypeNode.attributes['text'] == 'Application Data') {
                      var graphNode = node.parentNode;
                      var scope = properties['Context'];
                      var app = graphNode.attributes['text'];
                      var graph = node.attributes['text'];
                      var baseUri = properties['Base URI'];
                      var label = scope + '.' + app + '.' + graph;
                      var context = '?baseUri=' + baseUri + '&scope=' + scope + '&app=' + app + '&graph=' + graph;

                      loadPageDto('app', 'adata', context, label);
                    } else if (dataTypeNode.attributes['text'] == 'Data Exchanges') {
                      var scope = dataTypeNode.parentNode.attributes['text'];
                      var exchangeId = properties['Id'];
                      var context = '?scope=' + scope + '&xid=' + exchangeId;

                      node.reviewed = true;
                      loadPageDto('exchange', 'xdata', context, node.text);
                    }
                  }
                } catch (err) {
                }
              },
              contextmenu:function (node, event) {
            	  onTreeItemContextMenu(node, event);  
              },
              keydown : function(evnt) {
                // alert('keydown...');
                var keyPressed = evnt.getKey();
                if (evnt.ctrlKey) {
                  /*
                   * After trial and error, the ctrl+c combination seems to be
                   * code 67
                   */
                  if (67 == 67)// if (keyPressed == 67)
                  {
                    var celldata = Ext.getCmp('property-pane').getSelectionModel().events.beforecellselect.obj.selection.record.data.value;
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
                   * After trial and error, the ctrl+c combination seems to be
                   * code 67
                   */
                  if (67 == 67)// if (keyPressed == 67)
                  {
                    var celldata = Ext.getCmp('property-pane').getSelectionModel().events.beforecellselect.obj.selection.record.data.value;
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
