Ext.define('common.QuickFilter', {
	extend: 'Ext.grid.feature.Feature',
    alias: 'feature.quickfilter',

    grid: null,
	win: null,
	
	constructor: function (config) {
		config = config || {};
    },
	
	init: function(grid) {
		var me = this;
		
		me.grid = grid;
		me.grid.filters = [];
		
		me.grid.on('cellcontextmenu', me.addFilter, me);       
        me.grid.store.on('beforeload', me.onBeforeLoad, me);
        
        if (me.grid.bbar != null) {
        	me.grid.bbar.insert(0, {
        		type: 'button',
        		iconCls: 'filter',
        		handler: me.show,
        		scope: me
        	});
        }
        
        var conjStore = Ext.create('Ext.data.Store', {
        	fields: ['name', 'value'],            
            data: [
                {name: 'AND', value: 'And'},
                {name: 'OR', value: 'Or'}
            ]
        });

		var conjCbx = new Ext.form.field.ComboBox({
            typeAhead: true,
            triggerAction: 'all',
            store: conjStore,
            displayField: 'name',
            valueField: 'value'
        });
		
		var opStore = Ext.create('Ext.data.Store', {
        	fields: ['name', 'value'],            
            data: [
                {name: 'EqualTo', value: 'EqualTo'},
                {name: 'StartsWith', value: 'StartsWith'},
                {name: 'EndsWith', value: 'EndsWith'},
                {name: 'Contains', value: 'Contains'},
                {name: 'GreaterThan', value: 'GreaterThan'},
                {name: 'GreaterThanOrEqualTo', value: 'GreaterThanOrEqual'},
                {name: 'LessThan', value: 'LesserThan'},
                {name: 'LessThanOrEqualTo', value: 'LesserThanOrEqual'}
            ]
        });
		
		var opCbx = new Ext.form.field.ComboBox({
            typeAhead: true,
            triggerAction: 'all',
            store: opStore,
            displayField: 'name',
            valueField: 'value'
        });
		
		var cellEditing = new Ext.grid.plugin.CellEditing({
            clicksToEdit: 1
        });
       	
		var panel = Ext.create('Ext.grid.Panel', {
			layout: 'fit',
    		margin: 20,
    	    forceFit: true,
    	    sortableColumns: false,
    	    enableColumnHide: false,
    	    enableColumnMove: false,
    	    plugins: [cellEditing],
            store: {                	
                fields: ['conjunction', 'field', 'operator', 'value'],
                data: me.grid.filters
            },
            columns: [{
                text: 'Conjunction',
                width: 40,
                dataIndex: 'conjunction',
                editor: conjCbx,
                renderer: function (value) {
                    var index = conjStore.findExact('value', value); 
                    if (index != -1) {
                        var rs = conjStore.getAt(index).data; 
                        return rs.name; 
                    }
                }
            }, {
                text: 'Field',
                dataIndex: 'field'
            }, {
                text: 'Operator',
                align: 'center',
                width: 70,
                dataIndex: 'operator',
                editor: opCbx,
                renderer: function (value) {
                    var index = opStore.findExact('value', value); 
                    if (index != -1) {
                        var rs = opStore.getAt(index).data; 
                        return rs.name; 
                    }
                }
            }, {
                text: 'Value',
                width: 60,
                dataIndex: 'value',
                editor: {
                    xtype: 'textfield'
                }
            }, {
                xtype: 'actioncolumn',
                align: 'center',
                width: 20,
                items: [{
                    iconCls: 'item-remove',
                    handler: me.removeFilter,
                    scope: me
                }]
            }],
            selModel: {
                selType: 'cellmodel'
            },
            filters: me.grid.filters
		});
		
		panel.on('edit', function(editor, e) 
		{
		    var me = this;
			me.filters[e.rowIdx][e.column.dataIndex] = e.value;
			me.getStore().reload();
		});
	
		me.win = Ext.create('Ext.window.Window', {
		    title: 'Quick Filter Builder',
		    layout: 'fit',
		    bodyStyle: 'background:#fff',
		    width: 640,
		    closeAction: 'hide',
		    items: [ panel ],
		    dockedItems: [{
              xtype: 'toolbar',
              dock: 'bottom',
              layout: {
                  pack: 'end',
                  type: 'hbox'
              },
              items: [
              {
                  xtype: 'button',
                  handler: function (button, event) {
                	  button.up('window').close();
			      },
                  text: 'OK'
              },
              {
                  xtype: 'button',
                  handler: function (button, event) {
                	  button.up('window').close();
                	  me.reload();
                  },
                  text: 'Apply'
              }]
    	   }]
		});
	},
	
	addFilter: function(view, td, cellIndex, record, tr, rowIndex, e) {
    	var me = this;
    	var field = record.fields.items[cellIndex].name;
    	
    	var columns = view.panel.columnManager.getColumns();
    	var filterable = false;
    	
    	Ext.Array.each(columns, function (column) {
            if (column.dataIndex === field && column.filterable) {
                filterable = true;
                return;
            }
    	});
    	
    	if (filterable) {
	    	var value = record.data[field];
	    	
	    	me.grid.filters.push({
	    		conjunction: (me.grid.filters.length == 0) ? '' : 'And', 
	    		field: field, 
	    		operator: 'EqualTo', 
	    		value: value
	        });
    	}
    	
    	me.show();
    },
	
	removeFilter: function (view, rowIndex, colIndex, action, e, record, eOpts) {
		var me = this;
        
        for (var i in me.grid.filters) {
        	var expr = me.grid.filters[i];
        	if (expr.field == record.data.field &&
        		expr.operation == record.data.operation &&
        		expr.value == record.data.value) {
        		
        		me.grid.filters.splice(i, 1);
        		
        		if (me.grid.filters.length > 0) {
        			me.grid.filters[0].conjunction = null;
        		}
            	
        		break;
        	}
        }

		me.show();
    },
    
    show: function() {
    	var me = this;    	
    	me.win.down('panel').getStore().reload();
    	me.win.show();
    },
    
    onBeforeLoad: function(store, options) {
    	var me = this;
    	var arr = [];

    	options.params = options.params || {};
    	
    	for (var filter in me.grid.filters) {
    		arr.push(me.grid.filters[filter]);
    	}		
    	
		if (arr.length > 0) {
			var filterParam = Ext.JSON.encode(arr);
	    	options.params.filter = filterParam;
		}
		else if (options.params.filter != null) {
			delete options.params.filter;
		}
    },
    
    reload: function() {	
    	this.grid.getStore().loadPage(1);
    }
});