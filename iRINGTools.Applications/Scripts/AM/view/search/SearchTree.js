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
    initComponent: function () {
        Ext.apply(this, {
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
                    timeout: 120000,
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
                    enableDrop: false
                },
                store: this.store// make sure the tree.View uses same store
            }
        });
        this.callParent(arguments);
    }
});