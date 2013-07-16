Ext.ns('org.iringtools.apps.commons');

/*Ext.data.Connection.prototype.handleFailure = 
  Ext.data.Connection.prototype.handleFailure.createInterceptor(
    function(response, e) {
      Ext.getBody().unmask();       
      var message = 'Request URL: /' + response.argument.options.url + 
      	'.\n\nError description: ' + response.responseText;
      showDialog(500, 240, 'Error', message, Ext.Msg.OK, null);
    }
  );*/

Ext.data.DynamicGridReader = Ext.extend(Ext.data.JsonReader, {
  constructor: function(config) {
    Ext.data.DynamicGridReader.superclass.constructor.call(this, config, []);
  },
  
  readRecords: function(store) {
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
  constructor: function(store) {
    var recordType = store.reader.recordType;
    var fields = recordType.prototype.fields;
    var columns = [];
    
    for (var i = 0; i < fields.keys.length; i++) {
      var dataIndex = fields.keys[i];
      var field = recordType.getField(dataIndex);
      var dataType = field.type.type;
      var renderer = 'auto';    
      var align = 'left';
      
      if (dataType != null)
        dataType = dataType.toLowerCase();
        
      if (dataType == 'date') {
        renderer = Ext.util.Format.dateRenderer('Y-m-d');
      }
      else if (dataType != 'string') {
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

// capture DOM events including the ones that are not handled by extjs components
Ext.DomObserver = Ext.extend(Object, {
  constructor: function(config) {
    this.listeners = config.listeners ? config.listeners : config;
  },

  // component passes itself into plugin's init method
  init: function(c) {
    var p, l = this.listeners;
    for (p in l) {
      if (Ext.isFunction(l[p])) {
        l[p] = this.createHandler(l[p], c);
      } 
      else {
        l[p].fn = this.createHandler(l[p].fn, c);
      }
    }
  
    // add the listeners to the element immediately following the render call
    c.render = c.render.createSequence(function() {
      var e = c.getEl();
      if (e) {
        e.on(l);
      }
    });
  },

  createHandler: function(fn, c) {
    return function(e) {
      fn.call(this, e, c);
    };
  }
});
