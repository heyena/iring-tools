Ext.ns('iIRNGTools', 'iIRNGTools.AdapterManager');
/**
* @class iIRNGTools.AdapterManager.SearchPanel
* @extends TreePanel
* @author by Gert Jansen van Rensburg
*/
//image path
var IMG_CLASS = 'Content/img/class.png';
var IMG_TEMPLATE = 'Content/img/template.png';
//renderer function
function renderIcon(value, p, record) {
    var label = null;

    if (record.data.uri.indexOf("tpl") != -1) {
      label = '<img src="' + IMG_TEMPLATE + '" align="top"> ' + value;
    } else {
      label = '<img src="' + IMG_CLASS + '" align="top"> ' + value;
    }
    return label;
}


iIRNGTools.AdapterManager.SearchPanel = Ext.extend(Ext.Panel, {
  title: 'Reference Data Search',

  layout: 'fit',
  border: true,
  split: true,

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

    this.searchStore = new Ext.data.JsonStore({
      root: 'Items',
      totalProperty: 'Total',
      idProperty: 'label',
      fields: [
        { name: 'uri', allowBlank: false },
        { name: 'label', allowBlank: false },
        { name: 'repository', allowBlank: false }
      ],
      proxy: new Ext.data.HttpProxy({
          url: this.searchUrl,
          timeout: 12000
      }),
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
      border: false,
      store: this.searchStore,
      plugins: this.searchExpander,
      cm: new Ext.grid.ColumnModel([
        this.searchExpander,
        { header: "Label", width: 400, sortable: true, dataIndex: 'label', renderer: renderIcon },
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