Ext.ns('AdapterManager');
/**
* @class AdapterManager.SearchPanel
* @author by Neha Bhardwaj
*/
Ext.define('AdapterManager.SearchPanel', {
  extend: 'Ext.panel.Panel',
  alias: 'widget.AdapterManager.SearchPanel',

  title: 'Reference Data Search',
  layout: 'border',
  margins: '0 0 20 0',
  split: true,
  searchUrl: null,
  limit: 100,
  refClassTabPanel: null,
  propertyPanel: null,
  searchModel: null,
  searchStore: null,
  ajaxProxy: null,
  contextClassMenu: null,
  /**
  * initComponent
  * @protected
  */
  initComponent: function () {

    this.contextClassMenu = new Ext.menu.Menu();
    this.contextClassMenu.add(this.buildClassContextMenu());

    this.propertyPanel = new Ext.grid.property.Grid({
      title: 'Details',
      region: 'east',
      layout: 'fit',
      viewConfig: { stripeRows: true },
      collapsible: true,
      autoScroll: true,
      width: 350,
      split: true,
      bodyBorder: true,
      collapsed: false,
      border: 0,
      selModel: new Ext.selection.RowModel({ mode: 'SINGLE' }),
      frame: false,
      source: {},
      listeners: {
        beforepropertychange: function (source, recordid, v, oldValue) {
          return false;
        },
        //        // to copy but not edit content of property grid				
        afteredit: function (e) {
          e.grid.getSelectionModel().selections.items[0].data.value = e.originalValue;
          e.record.data.value = e.originalValue;
          e.value = e.originalValue;
          e.grid.getView().refresh();
        }
      }
    });

    this.refClassTabPanel = Ext.create('Ext.tab.Panel', {
      id: 'content-pane',
      deferredRender: false,

      scroll: 'both',
      activeItem: 0,
      iconCls: 'tabsClass'
    });

    this.ajaxProxy = Ext.create('Ext.data.proxy.Ajax', {
      timeout: 120000,
      actionMethods: { read: 'POST' },
      url: this.searchUrl,
      extraParams: {
        id: null,
        type: null,
        query: null,
        reset: null,
        limit: null,
        start: 0
      },
      reader: { type: 'json' }
    });

    if (!Ext.ModelManager.isRegistered('searchmodel')) {
      Ext.define('searchmodel', {
        extend: 'Ext.data.Model',
        fields: [
         { name: 'id', type: 'string' },
         { name: 'hidden', type: 'boolean' },
         { name: 'property', type: 'string' },
         { name: 'identifier', type: 'string' },
         { name: 'text', type: 'string' },
         { name: 'type', type: 'string' },
         { name: 'record', type: 'object' }
       ]
      });
    }

    this.searchStore = Ext.create('Ext.data.TreeStore', {
      model: 'searchmodel',
      autoLoad: false,
      clearOnLoad: true,
      root: {
        rootVisible: false,
        expanded: true
      },
      proxy: this.ajaxProxy
    });

    this.mainPanel = Ext.create('Ext.panel.Panel', {
      region: 'center',
      scroll: 'both',
      layout: 'fit',
      items: [this.refClassTabPanel]

    });

    this.tbar = new Ext.toolbar.Toolbar();
    this.tbar.add(this.buildToolbar());

    this.items = [this.mainPanel, this.propertyPanel];

    // super
    this.callParent(arguments);
  },
  buildToolbar: function () {
    var that = this;
    return [
          {
            xtype: 'textfield',
            width: 300,
            name: 'referencesearch',
            id: 'referencesearch',
            scope: this,
            listeners: {
              specialkey: function (f, e) {
                if (e.getKey() == e.ENTER) {
                  that.onSearch();
                }
              }
            }
          },
        {
          xtype: "button",
          text: 'Search',
          icon: 'Content/img/16x16/document-properties.png',
          handler: this.onSearch,
          scope: this,
          style: {
            marginLeft: '5px'
          }

        }, {
          xtype: 'checkboxfield',
          // boxLabel:'Reset',
          autoShow: true,
          name: 'reset',
          id: 'reset',
          style: {
            marginLeft: '5px',
            marginBottom: '6px'
          }
        },
        {
          xtype: 'label',
          text: 'Reset',
          style: {
            marginRight: '5px'
          }
        }
        ];
  },

  buildClassContextMenu: function () {
    return [{
      text: 'Promote',
      handler: this.onPromote,
      icon: 'Content/img/16x16/promote-icon.png',
      scope: this
    }];
  },

  getActiveTab: function () {
    return this.refClassTabPanel.getActiveTab();
  },

  getSelectedNode: function () {
    var activeTab = this.getActiveTab();
    if (activeTab != undefined)
      return activeTab.getSelectionModel().getSelectedNode();
  },

  showContextMenu: function (node, event) {
    if (node.isSelected()) {
      var x = event.browserEvent.clientX;
      var y = event.browserEvent.clientY;
      if (node.attributes.type == "ClassNode")
        this.contextClassMenu.showAt([x, y]);
    }
  },

  onSelect: function (selModel, model, idx) {
    selectedSearchNode = model.store.getAt(idx);
  },

  onClick: function (view, model, n, idx, e) {
    try {
      var node = model.store.getAt(idx);
      if (node.data.type == "ClassNode" && model.firstChild) {
        this.propertyPanel.setSource(model.firstChild.data.record);
      } else {
        this.propertyPanel.setSource(node.data.record);
      }
    } catch (e) { }
    node.expand();
  },


  onSearch: function () {
    var searchText = Ext.getCmp('referencesearch').getValue();
    Ext.getCmp('content-pane').getEl().mask('Loading...');
    var isreset = Ext.getCmp('reset').checked;

    if (searchText != undefined && searchText != '') {
      this.ajaxProxy.extraParams.query = searchText;
      this.ajaxProxy.extraParams.reset = isreset;
      this.ajaxProxy.extraParams.limit = this.limit;

      this.searchStore.on('beforeload', function (store, action) {
        this.ajaxProxy.extraParams.type = (action.node.data.type == "" ? 'SearchNode' : action.node.data.type);
        if (action.node.parentNode && (action.node.data.identifier == null || action.node.data.identifier == '')) {
          this.ajaxProxy.extraParams.id = action.node.parentNode.data.identifier;
        } else {
          this.ajaxProxy.extraParams.id = action.node.data.identifier;
        }
      }, this);
      this.searchStore.load();
    }

    var tree = Ext.create('Ext.tree.TreePanel', {
      title: searchText,
      animate: true,
      lines: true,
      id: 'tab_' + searchText,
      scroll: 'both',
      style: 'padding-left:5px;',
      border: false,
      closable: true,
      rootVisible: false,
      store: this.searchStore,
      containerScroll: true,
      viewConfig: {
        plugins: {
          ptype: 'treeviewdragdrop',
          dragGroup: 'refdataGroup'
        }
      }
    });

    tree.on('beforeload', function (store, model, a) {
      Ext.getCmp('content-pane').getEl().mask('Loading...');

    });
    tree.on('load', function (store, model, a) {
      Ext.getCmp('content-pane').getEl().unmask();

      if (model.data.type == "ClassNode") {
        try {
          if (model.childNodes.length > 0) {
            this.propertyPanel.setSource(model.childNodes[0].data.record);
          }
          else {
            this.propertyPanel.setSource(model.data.record);
          }
        } catch (e) { }
      }
    }, this);

    tree.on('itemclick', this.onClick, this);
    tree.on('select', this.onSelect, this);
    this.refClassTabPanel.add(tree).show();
    tree.on('contextmenu', this.showContextMenu, this);
  }

});


