
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
    data: [
      [123,'One Hundred Twenty Three'],
      ['1', 'One'], ['2', 'Two'], ['3', 'Three'], ['4', 'Four'], ['5', 'Five'],
      ['6', 'Six'], ['7', 'Seven'], ['8', 'Eight'], ['9', 'Nine']
    ],
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
            fieldLabel: 'ItemSelector',
            imagePath: '../ux/images/',
            store: ds,
            displayField: 'text',
            valueField: 'value',
            value: ['3', '4', '6'],

            allowBlank: false,
            // minSelections: 2,
            // maxSelections: 3,
            msgTarget: 'side'
        }
      ]
    });

    me.callOverridden(arguments);
  },
});