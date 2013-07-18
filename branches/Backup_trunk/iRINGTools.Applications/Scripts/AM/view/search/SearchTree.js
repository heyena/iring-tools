Ext.define('AM.view.search.SearchTree', {
    extend: 'Ext.tree.Panel',
    alias: 'widget.searchtree',
    scroll: 'both',
    style: 'padding-left:5px;',
    border: false,
    closable: true,
    rootVisible: false,
    containerScroll: true,
    animate: true,
    selModel: Ext.create('Ext.selection.RowModel', { singleSelect: true }),
    lines: true,
    store: Ext.create('Ext.data.TreeStore', {
        model: 'AM.model.SearchModel',
        autoLoad: false,
        storeId: this.id,
        clearOnLoad: true,
        root: {
            expanded: true
        },
        proxy: {
            url: 'refdata/getnode',
            type: 'ajax',
            timeout: 600000,
            actionMethods: { read: 'POST' },
            extraParams: {
                id: null,
                type: null,
                query: null,
                reset: null,
                limit: null,
                start: 0
            },
            reader: { type: 'json' }
        }
    }),
    viewConfig: {
        plugins: {
            ptype: 'treeviewdragdrop',
            dragGroup: 'refdataGroup',
            enableDrop: false,
            dragText: '{0}',
            dragField: 'text'
        }
    },
    initComponent: function () {
        var me = this;
        me.callParent(arguments);
    }
});