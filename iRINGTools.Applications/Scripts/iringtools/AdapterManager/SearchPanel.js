Ext.ns('AdapterManager');
/**
* @class AdapterManager.SearchPanel
* @extends TreePanel
* @author by Gert Jansen van Rensburg
*/

//image path
var IMG_CLASS = 'Content/img/class.png';
var IMG_TEMPLATE = 'Content/img/template.png';
//renderer function
function renderIcon(value, p, record) {
    var label = null;

    if (record.data.Uri.indexOf("tpl") != -1) {
        label = '<img src="' + IMG_TEMPLATE + '" align="top"> ' + value;
    } else {
        label = '<img src="' + IMG_CLASS + '" align="top"> ' + value;
    }
    return label;
}

AdapterManager.SearchPanel = Ext.extend(Ext.Panel, {
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
            root: 'items',
            totalProperty: 'total',
            idProperty: 'label',
            fields: [
              { name: 'Uri', allowBlank: false },
              { name: 'Label', allowBlank: false },
              { name: 'Repository', allowBlank: false }
            ],
            proxy: new Ext.data.HttpProxy({
                url: this.searchUrl,
                timeout: 12000
            }),
            baseParams: { limit: this.limit }
        });

        this.searchExpander = new AdapterManager.AjaxRowExpander({
            tpl: new Ext.Template(
            '<p><b>Uri:</b> {Uri}</p><br>',
            '<p><b>Description:</b> {Desc}</p>'
          )
        },'');

        this.searchExpander.on('beforeexpand', function (record, body, rowIndex) {
            alert(record);
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
        { header: "Label", width: 400, sortable: true, dataIndex: 'Label', renderer: renderIcon },
        { header: "Repository", width: 150, sortable: true, dataIndex: 'Repository' },
        { header: "Uri", width: 400, sortable: true, dataIndex: 'Uri', hidden: true }
      ])
        });

        // super
        AdapterManager.SearchPanel.superclass.initComponent.call(this);
    },

    load: function () {
        this.searchStore.load({ params: { start: 0, limit: this.limit} });
        return;
    }

});