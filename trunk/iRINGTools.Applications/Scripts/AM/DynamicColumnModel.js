Ext.define('Ext.grid.DynamicColumnModel', {
  extend: 'Ext.grid.header.Container',
  alias: 'widget.dynamiccolumns',
  
  initComponent: function () {
    var config = {
      items: [],
      rowNumberer: false
    };
    //apply this to the config
    Ext.apply(this, config);
    // apply to initial config
    Ext.apply(this.initialConfig, config);
    // call the arguments
    this.callParent(arguments);
  },
  /*
  * When store is loading then reconfigure grid
  */
  storeLoad: function () {
    /*
    * jsond data returned from server has column definitions
    */
    if (typeof (this.store.proxy.reader.jsonData.fields) === 'object') {
      var items = [];
      /*
      * add rownumberer as needed before other columns to display first
      */
      if (this.rowNumberer) {
        items.push(Ext.create('Ext.grid.RowNumberer'));
      }
      /*
      * assign new columns from json columns
      */
      Ext.each(this.store.proxy.reader.jsonData.fields, function (column) {
        items.push(column);
      });
      /*
      * reconfigure grid
      */
      this.reconfigure(this.store, items);
    }
  },
  /*
  * assign event to itself when object is initialising
  */

  onRender: function (ct, position) {
    Ext.ux.grid.DynamicGridPanel.superclass.onRender.call(this, ct, position);
    /*
    * hook the store load event to this
    */
    this.store.on('load', this.storeLoad, this);
  }
});
