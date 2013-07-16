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

        this.addEvents({
            close: true,
            save: true,
            reset: true
        });
			this.grid = new Ext.grid.GridPanel({
				store:new Ext.data.Store({
					autoDestroy: true,
					//reader: reader,
					fields: [
						{name:'file'}
					],
					data: [
								{
								  file: 'Text/MNO',
								  
								},
								{
								  file: 'Text/MNO',
								  
								},
								{
								  file: 'Text/MNO',
								  
								},
								{
								  file: 'Text/MNO',
								  
								}
						]
				}),
				colModel: new Ext.grid.ColumnModel({
					defaults: {
						width: 120,
						sortable: true
					},
					columns: [
						{ 
							header: 'File',
							dataIndex: 'file',
							flex:1
						},
						{
							xtype: 'actioncolumn',
							width: 40,
							menuDisabled: true,
							items: [
								{
									icon   : 'Content/img/16x16/document-down.png',                // Use a URL in the icon config
									tooltip: 'Download file',
									handler: function(grid, rowIndex, colIndex) {
										alert('hi...');
									}
								}
							]
                        }
					]
				}),
				viewConfig: {
					forceFit: true
				},
			sm: new Ext.grid.RowSelectionModel({singleSelect:true}),
			width: 600,
			height: 300,
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







  