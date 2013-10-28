
Ext.define('AM.view.override.nhibernate.MultiSelectComponentGrid', {
  override: 'AM.view.nhibernate.MultiSelectComponentGrid',
  requires:[
    'Ext.form.Panel',
    'Ext.ux.form.MultiSelect',
    'Ext.ux.form.ItemSelector'
	],
  
initComponent: function() {
    var me = this;
    var ds = Ext.create('Ext.data.ArrayStore', {
    data: [[]],
    fields: ['value','text'],
    sortInfo: {
    field: 'value',
    direction: 'ASC'
  	}
   });

    Ext.applyIf(me, {
     items: [
              {
                  xtype: 'itemselector',
                  name: 'itemselector',
                  anchor: '100%',
                  store: ds,
                  allowBlank: false,
                	fromTitle: 'Available',
            			toTitle: 'Selected',
                  msgTarget: 'side'
              }
      ]
    });

    me.callOverridden(arguments);
  },
});