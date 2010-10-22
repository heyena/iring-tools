Ext.ns('iIRNGTools', 'iIRNGTools.AdapterManager');
/**
* @class iIRNGTools.AdapterManager.NavigationPanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
iIRNGTools.AdapterManager.NavigationPanel = Ext.extend(Ext.Panel, {
  title: 'Directory',
  width: 220,

  collapseMode: 'mini',
  collapsible: true,
  collapsed: false,

  layout: 'border',
  border: true,
  split: true,

  navigationUrl: null,
  navigationPanel: null,

  /**
  * initComponent
  * @protected
  */
  initComponent: function () {

    this.addEvents({
      create: true,
      update: true,
      configure: true,
      mapping: true,
      exchange: true
    });

    this.tbar = this.buildToolbar();

    this.navigationPanel = new Ext.tree.TreePanel({
      region: 'center',
      collapseMode: 'mini',
      height: 200,
      layout: 'fit',
      border: false,

      rootVisible: true,
      lines: true,
      //singleExpand: true,
      useArrows: true,

      loader: new Ext.tree.TreeLoader({
        dataUrl: this.navigationUrl
      }),

      root: {
        nodeType: 'async',
        text: 'World',
        expanded: true,
        draggable: false,
        icon: 'Content/img/internet-web-browser.png',
        id: 'src'
      }

      //root: new Ext.tree.AsyncTreeNode({})

    });

    this.items = [
      this.navigationPanel
    ];

    // super
    iIRNGTools.AdapterManager.NavigationPanel.superclass.initComponent.call(this);
  },

  getScope: function () {
    var scope = '';
    var node = this.navigationPanel.getSelectionModel().getSelectedNode();
    scope = node.attributes.Scope.Name;
    return scope;
  },

  getApplication: function () {
    var application = '';
    var node = this.navigationPanel.getSelectionModel().getSelectedNode();
    if(node.allowChildren && node.attributes._application != null) {
      application = node.attributes._application.Name;
    }
    return application;
  },

  getGraph: function () {
    var graph = '';
    var node = this.navigationPanel.getSelectionModel().getSelectedNode()
    if(node.isLeaf()) {
      graph = node.attributes.GraphName;
    }
    return graph;
  },

  buildToolbar: function () {
    return [
      {
        tooltip: 'Add',
        handler: this.onCreate,
        icon: 'Content/img/list-add.png',
        scope: this
      },
      {
        tooltip: 'Remove',
        handler: this.onUpdate,
        icon: 'Content/img/list-remove.png',
        scope: this
      },
      {
        text: 'Edit',
        handler: this.onConfigure,
        icon: 'Content/img/document-properties.png',
        scope: this
      },
      {
        text: 'Mapping',
        handler: this.onMapping,
        icon: 'Content/img/file-mapping.png',
        scope: this
      },
      {
        text: 'Exchange',
        handler: this.onExchange,
        //icon: 'Content/img/file-mapping.png',
        scope: this
      }
    ]
  },

  onCreate: function (btn, ev) {
    this.fireEvent('create', this);
  },

  onUpdate: function (btn, ev) {
    this.fireEvent('update', this);
  },

  onConfigure: function (btn, ev) {
    this.fireEvent('configure', this, this.getScope(), this.getApplication(), this.getGraph());
  },

  onMapping: function (btn, ev) {
    this.fireEvent('mapping', this, this.getScope(), this.getApplication(), this.getGraph());
  },

  onExchange: function (btn, ev) {
    this.fireEvent('exchange', this, this.getScope(), this.getApplication(), this.getGraph());
  }

});