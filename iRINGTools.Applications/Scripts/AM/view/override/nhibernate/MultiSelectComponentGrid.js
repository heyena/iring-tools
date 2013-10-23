
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
        fields: ['value', 'text'],
        data: [[]],
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
            msgTarget: 'side',
            fromTitle: 'Available',
            toTitle: 'Selected'
        }
      ]
    });

    me.callOverridden(arguments);
  },
});