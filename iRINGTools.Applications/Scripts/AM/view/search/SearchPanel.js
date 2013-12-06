Ext.define('AM.view.search.SearchPanel', {
    extend: 'Ext.panel.Panel',
    alias: 'widget.searchpanel',

    requires: [
    'AM.view.common.PropertyPanel',
    'AM.view.search.SearchToolbar',
    'AM.view.common.ContentPanel'
  ],

    layout: 'border',
    collapsible: true,
    title: 'Reference Data Search',

    initComponent: function () {
        var me = this;

        Ext.applyIf(me, {
            items: [
            {
                xtype: 'propertypanel',
                width: 360,
                region: 'east',
                collapseDirection: 'right',
                title: 'Search Details',
                split: true
            },
            {
                xtype: 'contentpanel',
                tabPosition: 'bottom',
                itemId: 'searchcontent',
                region: 'center',
                dockedItems: [
                {
                    xtype: 'searchtoolbar',
                    dock: 'top'
                }
              ]
            }
          ]
        });

        me.callParent(arguments);
    }
});