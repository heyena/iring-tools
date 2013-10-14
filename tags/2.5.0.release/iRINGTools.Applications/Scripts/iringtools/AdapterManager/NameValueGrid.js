Ext.ns('AdapterManager');

AdapterManager.NameValueGrid = Ext.extend(Ext.Panel, {
    layout: 'fit',
    store: null,
    autoScroll: true,
    source: null,

    initComponent: function () {
        this.store = new Ext.data.ArrayStore({
            fields: [
               { name: 'name' }, { name: 'value' }
            ]
        });

        this.setSource(this.source);

        var grid = new Ext.grid.EditorGridPanel({
            stripeRows: true,
            clicksToEdit: 1,
            frame: false,
            border: false,
            store: this.store,
            colModel: new Ext.grid.ColumnModel({
                defaults: {
                    sortable: true,
                    align: 'left'
                },
                columns: [{
                    header: 'Name',
                    dataIndex: 'name'
                }, {
                    header: 'Value',
                    dataIndex: 'value',
                    editor: new Ext.form.TextField({
                        readOnly: true
                    })
                }]
            }),

            viewConfig: {
                forceFit: true
            }
        });

        this.items = [grid];

        AdapterManager.NameValueGrid.superclass.initComponent.call(this);
    },

    setSource: function (nameValuePairs) {
        var data = [];

        for (var name in nameValuePairs) {
            var value = nameValuePairs[name];

            if (typeof(value) != 'object')
                data.push([name, value]);
        }

        this.store.loadData(data);
    }
});
