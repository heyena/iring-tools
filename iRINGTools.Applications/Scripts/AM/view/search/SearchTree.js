Ext.define('AM.view.search.SearchTree', {
    extend: 'Ext.tree.Panel',
    alias: 'widget.searchtree',
    scroll: 'both',
    style: 'padding-left:5px;',
    border: false,
    closable: true,
    rootVisible: false,
    store: 'SearchStore',
    containerScroll: true,
    animate: true,
    lines: true,
    viewConfig: {
        plugins: {
            ptype: 'treeviewdragdrop',
            dragGroup: 'refdataGroup',
            enableDrop: false
        }
    },
    initComponent: function () {
        this.callParent(arguments);
    }
});