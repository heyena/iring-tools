Ext.ns('AdapterManager');

function loadAppData(scope, app, graph) {
	var tab = Ext.getCmp('content-panel').getItem('tab-' + scope + '.' + app + '.' + graph);

  if (tab != null) {
    tab.show();
  }
  else {
    Ext.getBody().mask("Loading...", "x-mask-loading");

    var store = createGridStore(scope, app, graph);
		var pageSize = 25;

		store.on('load', function () {
		  if (Ext.getCmp('content-panel').getItem('tab-' + scope + '.' + app + '.' + graph) == null) {
				var dtoBcPane = new Ext.Container({
					id: 'bc-' + scope + '.' + app + '.' + graph,
					cls: 'bc-container',
					padding: '5'
				});			
            
			var dtoContentPane = new Ext.Panel({
				id: 'dto-' + scope + '.' + app + '.' + graph,
				region: 'center',
				layout: 'card',
				border: false,
				activeItem: 0,
				items: [createGridPane(store, pageSize, {forceFit: false})],
				listeners: {
					afterlayout: function(pane){
					  Ext.getBody().unmask();
					}
				}
			});
        
			var dtoTab = new Ext.Panel({
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
	var url = "GridManager/Pages"
	var store = new Ext.data.Store({
	  proxy: new Ext.data.HttpProxy({
	    url: url,
	    timeout: 1800000  // 30 minutes
	  }),
	  baseParams: {
	    scope: scope,
	    app: app,
	    graph: graph
	  },
	  reader: new Ext.data.DynamicGridReader({}),
	  remoteSort: true,
	  listeners: {
	    exception: function (ex, a, b, c, response) {
	      Ext.getBody().unmask();
	      var rtext = response.responseText;
	      if (rtext != null) {
	        var ind = rtext.indexOf('}');
	        var len = rtext.length - ind - 1;
	        var msg = rtext.substring(ind + 1, rtext.length - 1);
	        showDialog(560, 320, 'Error', msg, Ext.Msg.OK, null);
	      }
	    }
	  }
	});

	store.sort = store.sort.createInterceptor(storeSort);
	return store;
}

function createGridPane(store, pageSize, viewConfig) {
	var filters = new Ext.ux.grid.GridFilters({
		remotesort: true,
		local: false,
		encode: true,
		filters: store.reader.filters
	});

	var pagingResizer = new Ext.ux.plugin.PagingToolbarResizer({
		displayText: 'Page Size',
		options: [25, 50, 100, 200, 500],
		prependCombo: true
	});

	var colModel = new Ext.grid.DynamicColumnModel(store);
	var selModel = new Ext.grid.RowSelectionModel({ singleSelect: true });
	var pagingToolbar = new Ext.MyPagingToolbar({
		store: store,
		pageSize: pageSize,
		displayInfo: true,
		autoScroll: true,
		plugins: [filters, pagingResizer]
	});

	var gridPane = new Ext.grid.GridPanel({
		identifier: store.reader.identifier,
		description: store.reader.description,
		layout: 'fit',
		minColumnWidth: 80,
		loadMask: true,
		store: store,
		stripeRows: true,
		viewConfig: viewConfig,
		cm: colModel,
		selModel: selModel,
		enableColLock: false,
		plugins: [filters],
		bbar: pagingToolbar
	});

	return gridPane;
}

Ext.MyPagingToolbar = Ext.extend(Ext.PagingToolbar, {
	constructor: function (config) {
		Ext.MyPagingToolbar.superclass.constructor.call(this, config, []);
	},

	doRefresh: function () {
		var filters = this.ownerCt.filters;
		var hasActiveFilter = false;

		for (var i = 0; i < filters.filters.length; i++) {
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

Ext.data.DynamicGridReader = Ext.extend(Ext.data.JsonReader, {
	constructor: function (config) {
		Ext.data.DynamicGridReader.superclass.constructor.call(this, config, []);
	},

	readRecords: function (store) {
		var fields = store.fields;
		var data = store.data;

		this.identifier = store.identifier;
		this.description = store.description;
		this.recordType = Ext.data.Record.create(fields);

		var records = [];
		for (var i = 0; i < data.length; i++) {
			var values = {};

			for (var j = 0; j < fields.length; j++) {
				values[fields[j].dataIndex] = data[i][j];
			}

			records[i] = new Ext.data.Record(values, i);
		}

		var filters = [];
		for (var i = 0; i < fields.length; i++) {
			if (fields[i].filterable) {
				filters.push({
					type: fields[i].type,
					dataIndex: fields[i].dataIndex
				});
			}
		}
		this.filters = filters;

		return {
			records: records,
			totalRecords: store.total || records.length
		};
	}
});

Ext.grid.DynamicColumnModel = Ext.extend(Ext.grid.ColumnModel, {
	constructor: function (store) {
		var recordType = store.reader.recordType;
		var fields = recordType.prototype.fields;
		var columns = [];

		for (var i = 0; i < fields.keys.length; i++) {
			var dataIndex = fields.keys[i];
			var field = recordType.getField(dataIndex);
			var dataType = field.type.type;
			var renderer = 'auto';
			var keyType = field.keytype
			var align = 'left';

			if (dataType != null)
				dataType = dataType.toLowerCase();

			if (dataType == 'date') {
				renderer = Ext.util.Format.dateRenderer('Y-m-d');
			}
			else if (dataType != 'string' && keyType != 'key') {
				if (dataType == 'double' || dataType == 'float' || dataType == 'decimal') {
					renderer = Ext.util.Format.numberRenderer('0,000.00');
				}
				else {
					renderer = Ext.util.Format.numberRenderer('0,000');
				}
				align = 'right';
			}

			columns[i] = {
				header: field.name,
				width: field.width,
				dataIndex: field.dataIndex,
				sortable: field.sortable,
				renderer: renderer,
				align: align
			};

			if (field.fixed) {
				columns[i].fixed = true;
				columns[i].menuDisabled = true;
			}
		}

		Ext.grid.DynamicColumnModel.superclass.constructor.call(this, columns);
	}
});

  