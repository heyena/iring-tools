
Ext.define('AM.view.override.nhibernate.MultiSelectComponentGrid', {
  override: 'AM.view.nhibernate.MultiSelectComponentGrid',
  requires:[
    'Ext.form.Panel',
    'Ext.ux.form.MultiSelect',
    'Ext.ux.form.ItemSelector'
	],
  //itemId:'multiselectcomponentgrid',
initComponent: function() {
    var me = this;
    var ds = Ext.create('Ext.data.ArrayStore', {
    data: [
      ['1', 'One'], ['2', 'Two'], ['3', 'Three'], ['4', 'Four'], ['5', 'Five']
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
			xtype: 'label',
			text: 'Available Namespaces',
			style:'font-weight:bold;font-size:11px;color:#888888;'
			//margin: '0 0 0 10'
		},
		{
			xtype: 'label',
			text: 'Selected Namespaces',
			margin: '0 0 0 110',
			style:'font-weight:bold;font-size:11px;color:#888888;'
		},
        {
            xtype: 'itemselector',
            name: 'itemselector',
            anchor: '100%',
            //fieldLabel: 'ItemSelector',
            //imagePath: '../ux/images/',
            store: ds,
			height:356,
            //displayField: 'text',
            //valueField: 'value',
            //value: ['3', '4', '6'],
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