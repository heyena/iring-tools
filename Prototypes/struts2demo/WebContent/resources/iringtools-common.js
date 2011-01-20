Ext.ns('iringtools');

Ext.data.DynamicGridReader = Ext.extend(Ext.data.JsonReader, {
  constructor: function(config) {
    Ext.data.DynamicGridReader.superclass.constructor.call(this, config, []);
  },
  readRecords: function(store) {
    var fields = store.fields;  
    var data = store.data;
    
    this.type = store.type;
    this.recordType = Ext.data.Record.create(fields);
    
    var records = [];    
    for (var i = 0; i < data.length; i++) {
      var values = {};
      for (var j = 0; j < fields.length; j++) {
        values[fields[j].name] = data[i][j];        
      }
      records[i] = new Ext.data.Record(values, i);
    }
    
    return {
      records: records,
      totalRecords: store.total || records.length
    };
  }
});

Ext.grid.DynamicColumnModel = Ext.extend(Ext.grid.ColumnModel, {
  constructor: function(store) {
    var recordType = store.recordType;
    var fields = recordType.prototype.fields;
    var columns = [];
    
    for (var i = 0; i < fields.keys.length; i++) {
      var fieldName = fields.keys[i];
      var field = recordType.getField(fieldName);
      var fieldType = field.type.type;
      var renderer = 'auto';    
      var align = 'left';
      
      if (fieldType != null)
        fieldType = fieldType.toLowerCase();
        
      if (fieldType == 'date') {
        renderer = Ext.util.Format.dateRenderer('mm/dd/YYYY');
      }
      else if (fieldType != 'string') {
        if (fieldType == 'double' || fieldType == 'float' || fieldType == 'decimal') {
          renderer = Ext.util.Format.numberRenderer('0,000.00');
        }
        else {
          renderer = Ext.util.Format.numberRenderer('0,000');
        }
        align = 'right';
      }
      
      columns[i] = {
        header: field.name,
        dataIndex: field.name,
        renderer: renderer,
        align: align,
        sortable: true
      };
      
      if (field.fixed) {
        columns[i].fixed = true;
        columns[i].width = field.width;
      }
    }
    
    Ext.grid.DynamicColumnModel.superclass.constructor.call(this, columns);
  }
});