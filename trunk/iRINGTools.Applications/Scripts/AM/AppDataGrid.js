
function loadAppPageDto(scope, app, graph) {

  Ext.define('dynamicModel', {
    extend: 'Ext.data.Model',
    fields: []	// are defined in the JSON response metaData
  });

  var tab = Ext.getCmp('content-panel').items['tab-' + scope + '.' + app + '.' + graph];

  if (tab != null) {
    tab.show();
  }
  else {
    Ext.getBody().mask("Loading...", "x-mask-loading");

    var store = createGridStore(scope, app, graph);
    var pageSize = 25;
    var grid = createGridPane(store, pageSize, { forceFit: false });

    store.on('load', function () {
      var me = this;
      
      if (Ext.getCmp('content-panel').items.findBy('tab-' + scope + '.' + app + '.' + graph) === null) {
        var dtoBcPane = new Ext.container.Container({
          id: 'bc-' + scope + '.' + app + '.' + graph,
          cls: 'bc-container',
          padding: '5'
        });

        var dtoContentPane = new Ext.panel.Panel({
          id: 'dto-' + scope + '.' + app + '.' + graph,
          region: 'center',
          layout: 'card',
          border: false,
          activeItem: 0,
          items: [grid],
          listeners: {
            afterlayout: function (pane) {
              Ext.getBody().unmask();
            }
          }
        });

        var dtoTab = new Ext.panel.Panel({
          id: 'tab-' + scope + '.' + app + '.' + graph,
          title: scope + '.' + app + '.' + graph,
          type: 'app',
          context: 'adata',
          layout: 'border',
          closable: true,
          items: dtoContentPane
        });

        Ext.getCmp('content-panel').add(dtoTab).show();
      }
    });
    store.load({

      params: {
        start: 0,
        limit: pageSize
      },
      callback: function (recs, options, success) {
        if (success) {
          grid.reconfigure(store, recs[0].store.proxy.reader.jsonData.fields);
          grid.show();
        }
      }
    });
  }
}

function storeSort(field, dir) {
  if (field == '&nbsp;') return;

  var limit = this.lastOptions.params.limit;

  this.lastOptions.params = {
    start: 0,
    limit: limit
  };

  if (dir == undefined) {
    if (this.sortInfo && this.sortInfo.direction == 'ASC')
      dir = 'DESC';
    else
      dir = 'ASC';
  }

  this.sortInfo = {
    field: field,
    direction: dir
  };

  this.reload();
}

function createGridStore(scope, app, graph) {

  var store = new Ext.data.JsonStore({
    pageSize: 25,
    model: 'dynamicModel',
    proxy: {
      type: 'rest',
      url: 'GridManager/Pages',
      actionMethods: { read: 'POST' },
      reader: {root: 'data' },
      extraParams: {
        scope: scope,
        app: app,
        graph: graph
      }
    },
    remoteSort: true,
    listeners: {
      exception: function (ex) {
        Ext.getBody().unmask();
        showDialog(400, 100, 'Error', 'Error loading data at URL: ' + ex.url, Ext.Msg.OK, null);
      }
    }
  });

  //store.sort = store.sort.createInterceptor(storeSort);
  return store;
}

function createGridPane(store, pageSize, viewConfig) {
  var me = this;
  var filters = new Ext.ux.grid.filter.Filter({
    remotesort: true,
    local: false,
    encode: true,
    filters: store.filters
  });

  var pagingResizer = new Ext.ux.plugin.PagingToolbarResizer({
    displayText: 'Page Size',
    options: [25, 50, 100, 200, 500],
    prependCombo: true
  });

  var selModel = new Ext.selection.Model({ singleSelect: true });
  var pagingToolbar = new Ext.GridPagingToolbar({
    store: store,
    pageSize: pageSize,
    displayInfo: true,
    autoScroll: true,
    plugins: [filters, pagingResizer]
  });

  var gridPane = Ext.create('Ext.grid.Panel', {
    id: 'Grid-Panel-'+store.proxy.extraParams['scope']+'.'+store.proxy.extraParams['app']+'.'+store.proxy.extraParams['graph'],
    layout: 'fit',
    minColumnWidth: 80,
    loadMask: true,
    store: store,
    stripeRows: true,
    viewConfig: viewConfig,
    columns: {
      defaults: {
        field: {
          xtype: 'textfield'
        }
      }
    },
    selModel: selModel,
    enableColLock: false,
    plugins: [filters],
    bbar: Ext.create('Ext.PagingToolbar', {
      store: store,
      displayInfo: true,
      displayMsg: 'Displaying Data {0} - {1} of {2}',
      emptyMsg: "No topics to display"
    })
  });

  return gridPane;
}

Ext.define('Ext.GridPagingToolbar', {
  extend: 'Ext.toolbar.Paging',
  constructor: function (config) {
    Ext.GridPagingToolbar.superclass.constructor.call(this, config, []);
  },

  doRefresh: function () {
    var filters = this.ownerCt.store.filters;
    var hasActiveFilter = false;

    for (var i = 0; i < filters.length; i++) {
      if (filters.filters.items[i].active) {
        hasActiveFilter = true;
        break;
      }
    }

    // clear sort info
    this.ownerCt.store.sortInfo = null;

    if (hasActiveFilter)
      filters.clearFilters();  // will trigger store to reload
    else
      this.doLoad(0);
  }
});

