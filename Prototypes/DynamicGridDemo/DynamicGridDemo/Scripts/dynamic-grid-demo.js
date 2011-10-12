Ext.onReady(function () {
    Ext.QuickTips.init();
    Ext.state.Manager.setProvider(new Ext.state.CookieProvider());

    var baseParams = {
        dataObject: 'lines'
    };

    var pageSize = 25;

    Ext.Ajax.request({
        url: 'DynamicGrid/GridDefinition',
        method: 'POST',
        params: baseParams,
        success: function (response, request) {
            var gridDef = Ext.util.JSON.decode(response.responseText);

            var gridStore = new Ext.data.JsonStore({
                autoDestroy: true,
                proxy: new Ext.data.HttpProxy({
                    url: 'DynamicGrid/GridData',
                    timeout: 120000
                }),
                baseParams: baseParams,
                remoteSort: true, 
                totalProperty: 'total',
                idProperty: gridDef.idProperty,
                fields: gridDef.fields,
                root: 'rows'
            });

            var gridFilters = new Ext.ux.grid.GridFilters({
                remotesort: true,
                local: false,
                encode: true,
                filters: gridDef.filters
            });

            var pagingResizer = new Ext.ux.plugin.PagingToolbarResizer({
                displayText: 'Page Size',
                options: [25, 50, 100, 200, 500],
                prependCombo: true
            });

            var pagingToolbar = new Ext.PagingToolbar({
                store: gridStore,
                pageSize: pageSize,
                displayInfo: true,
                autoScroll: true,
                plugins: [gridFilters, pagingResizer]
            })

            var gridPanel = new Ext.grid.GridPanel({
                store: gridStore,
                columns: gridDef.columns,
                region: 'center',
                minColumnWidth: 80,
                loadMask: true,
                stripeRows: true,
                bbar: pagingToolbar,
                plugins: [gridFilters],
                renderTo: 'grid-div'
            });

            var viewPort = new Ext.Viewport({
                layout: 'border',
                items: [gridPanel]
            });

            gridStore.load({ params: { start: 0, limit: pageSize} })
        },
        failure: function (response, request) {
            alert("Error getting grid definition: " + response.responseText);
        }
    });
});