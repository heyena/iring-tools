
Ext.define('AM.view.search.SearchPanel', {
    extend: 'Ext.panel.Panel',
    alias: 'widget.searchpanel',
    title: 'Reference Data Search',
    layout: 'border',
    margins: '0 0 5 0',
    collapsible: true,
    height: 250,
    split: true,
    store: null,
    items: [
    { xtype: 'propertypanel', region: 'east', width: 350 },
    { xtype: 'contentpanel', region: 'center', id: 'searchcontent' }
  ],
    initComponent: function () {
        var me = this;

        this.store = Ext.create('Ext.data.ArrayStore', {
            fields: ['name', 'value'],
            data: [['50',50],['100',100],['200',200], ['400',400]]
        });

        this.tbar = [
            {
                xtype: 'textfield',
                width: 200,
                name: 'referencesearch',
                id: 'referencesearch',
                scope: this,
                listeners: {

                    specialkey: function (f, e) {
                        if (e.getKey() == e.ENTER) {
                            me.onSearch();
                        }
                    }
                }
            },
        {
            xtype: "button",
            text: 'Search',
            icon: 'Content/img/16x16/document-properties.png',
            //handler: this.onSearch,
            scope: this,
            style: {
                marginLeft: '5px'
            },
            action: 'search'

        }, {
            xtype: 'checkboxfield',
            // boxLabel:'Reset',
            autoShow: true,
            name: 'reset',
            id: 'reset',
            style: {
                marginLeft: '5px',
                marginBottom: '6px'
            }
        },
        {
            xtype: 'label',
            text: 'Reset',
            style: {
                marginRight: '5px'
            }
        },
        '-',
        {
            xtype: 'combo',
            fieldLabel: 'Limit',
            labelWidth: 40,
            width: 90,
            store: this.store,
            queryMode: 'local',
            displayField: 'name',
            valueField: 'value',
            value: 50
        }
        ],

        this.callParent(arguments);
    },

    onSearch: function (arguments) {
        var btn = this.dockedItems.items[1].items.items[1];
        btn.fireEvent('click', btn)
    }
});