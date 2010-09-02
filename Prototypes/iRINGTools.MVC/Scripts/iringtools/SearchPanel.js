Ext.ns('iIRNGTools', 'iIRNGTools.AdapterManager');
/**
* @class iIRNGTools.AdapterManager.SearchPanel
* @extends TreePanel
* @author by Gert Jansen van Rensburg
*/
iIRNGTools.AdapterManager.SearchPanel = Ext.extend(Ext.Panel, {
  title: 'Reference Data Search',
  collapsible: true,
  collapsed: false,
  split: true,
  layout: 'fit',
  border: true,

  searchStore: null,
  searchExpander: null,
  searchColumnmodel: null,
  searchUrl: null,
  limit: 100,

  /**
  * initComponent
  * @protected
  */
  initComponent: function () {

    this.searchStore = new Ext.data.Store({
      proxy: new Ext.data.HttpProxy({
        url: this.searchUrl
      }),
      reader: new Ext.data.JsonReader({
        root: 'Entities',
        totalProperty: 'Total',
        id: 'label'
      },
      [
        { name: 'uri', allowBlank: false },
        { name: 'label', allowBlank: false },
        { name: 'repository', allowBlank: false }
      ]),
      baseParams: { limit: this.limit }
    });

    this.searchExpander = new Ext.ux.grid.RowExpander({
      tpl: new Ext.Template(
        '<p><b>Uri:</b> {uri}</p><br>',
        '<p><b>Description:</b> {desc}</p>'
      )
    });

    this.tbar = [
      'Search: ', ' ',
      new Ext.ux.form.SearchField({
        store: this.searchStore,
        width: 320
      }, ' ', { text: '' })
    ];

    this.bbar = new Ext.PagingToolbar({
      store: this.searchStore,
      pageSize: this.limit,
      displayInfo: true,
      displayMsg: 'Results {0} - {1} of {2}',
      emptyMsg: "No results to display"
    });

    this.items = new Ext.grid.GridPanel({
      store: this.searchStore,
      plugins: this.searchExpander,
      cm: new Ext.grid.ColumnModel([
        this.searchExpander,
        { header: "Label", width: 400, sortable: true, dataIndex: 'label' },
        { header: "Repository", width: 150, sortable: true, dataIndex: 'repository' },
        { header: "Uri", width: 400, sortable: true, dataIndex: 'uri', hidden: true }
      ])
    });

    // super
    iIRNGTools.AdapterManager.SearchPanel.superclass.initComponent.call(this);
  },

  load: function () {
    this.searchStore.load({ params: { start: 0, limit: this.limit} });
    return;
  }

});