Ext.ns('AdapterManager');
/**
* @class AdapterManager.ScopePanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
AdapterManager.FileDownloadGrid = Ext.extend(Ext.Panel, {
    layout: 'fit',
    border: false,
    frame: false,
    split: true,
    from: null,
	grid:null,
    record: null,
    url: null,

    /**
    * initComponent
    * @protected
    */
    initComponent: function () {
		//var scope = 'test';
		//var application = 'DEF';
		//var file = 'abc';
		var vall = 'Download';
         if (this.record != undefined) {
            var scope = this.record.id.split('/')[0];
            var application = this.record.id.split('/')[1];
            //var file = 'colval.xlsx';
        }
        this.addEvents({
            close: true,
            save: true,
            reset: true
        });
		 rt = Ext.data.Record.create([
				{name: 'File'}
				//{name: 'abc'}
			]);
			var store = new Ext.data.Store({
				   autoLoad:true, 
				   proxy: new Ext.data.HttpProxy({
					url: 'File/GetFiles',
					timeout: 1800000  // 30 minutes
				   }),
                   baseParams: {	       
                        'scope': scope,  
                        'application': application
                    },
					autoDestroy: true,
					 reader: new Ext.data.JsonReader(
							{								
							},
							rt // recordType
						)
					
					
				});
			/*var filters = new Ext.ux.grid.GridFilters({
				remotesort: true,
				local: false,
				encode: true,
				filters: store.reader.filters
			});

			var pagingResizer = new Ext.ux.plugin.PagingToolbarResizer({
				displayText: 'Page Size',
				options: [25, 50, 100, 200, 500],
				prependCombo: true
			});
			var pagingToolbar = new Ext.MyPagingToolbar({
				store: store,
				pageSize: 25,//pageSize,
				displayInfo: true,
				autoScroll: true,
				plugins: [filters, pagingResizer]
			});*/
			
			this.grid = new Ext.grid.GridPanel({
				store:store,
				//bbar: pagingToolbar,
				//plugins: [filters],
				colModel: new Ext.grid.ColumnModel({
					defaults: {
						width: 120,
						sortable: true
					},
					columns: [
						{ 
							header: 'File',
							dataIndex: 'File',
							flex:1
						},
						 {
							header: '',
							menuDisabled: true,
							dataIndex: 'Download',
							tooltip: 'Download file',
							width: 45,
							align:'center',
							renderer: function (val, meta, record) {
								return '<a style="color: #0276FD" href="./File/Export?scope=' + scope + '&application=' + application + '&file=' + record.data.File + ' "target="_blank">' + vall + '</a>';
							}
						}
					]
				}),
				viewConfig: {
					forceFit: true
				},
			sm: new Ext.grid.RowSelectionModel({singleSelect:true}),
			width: 600,
			height: 300
			//frame: true,
			//title: 'Download File',
			//iconCls: 'icon-grid'
	});
        this.items = [
  		this.grid
		];
        // super
        AdapterManager.FileDownloadGrid.superclass.initComponent.call(this);
    }
});







  