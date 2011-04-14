Ext.ns('AdapterManager');
/**
* @class FederationManager.SearchPanel
* @author by Aswini Nayak
*/
AdapterManager.SearchPanel = Ext.extend(Ext.Panel, {
  title: 'Reference Data Search',
  layout: 'border',
  border: true,
  split: true,
  searchUrl: null,
  limit: 100,
  refClassTabPanel: null,
  propertyPanel: null,
  searchStore: null,
  /**
  * initComponent
  * @protected
  */
  initComponent: function () {
    this.tbar = this.buildToolbar();
    this.propertyPanel = new Ext.grid.PropertyGrid({
      id: 'class-property-panel',
      title: 'Details',
      region: 'east',
      // layout: 'fit',
      stripeRows: true,
      collapsible: true,
      autoScroll: true,
      width: 450,
      split: true,
      bodyBorder: true,
      collapsed: false,
      border: true,
      frame: true,
      source: {},
      listeners: {
        // to disable editable option of the property grid
        beforeedit: function (e) {
          e.cancel = true;
        }
      }
    });

    this.refClassTabPanel = new Ext.TabPanel({
      id: 'content-pane',
      region: 'center',
      deferredRender: false,
      enableTabScroll: true,
      border: true,
      activeItem: 0,
			iconCls: 'tabsClass'
    });


    this.items = [this.refClassTabPanel, this.propertyPanel];

    // super
    AdapterManager.SearchPanel.superclass.initComponent.call(this);
  },
  buildToolbar: function () {
    var that = this;
    return [
                 {
                   xtype: 'textfield',
                   allowBlank: false,
                   blankText: 'This field can not be blank',
                   name: 'referencesearch',
                   id: 'referencesearch',
                   style: {
                     marginLeft: '15px'
                   },
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
            	   xtype: 'checkbox',
            	   boxLabel: 'Reset',
            	   name: 'reset',
            	   style: {
            	     marginRight: '5px',
            	     marginLeft: '3px'
            	   }
            	 },
                {
                  xtype: "tbbutton",
                  text: 'Search',
                  handler: this.onSearch,
                  scope: this,
                  style: {
                    marginLeft: '5px'
                  }

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

  onSearch: function () {

    var searchText = Ext.get('referencesearch').getValue();

    var treeLoader = new Ext.tree.TreeLoader({
      requestMethod: 'POST',
      url: this.searchUrl,
      baseParams: {
        id: null,
        type: null,
        query: searchText,
        limit: this.limit,
        start: 0
      }
    });

    treeLoader.on("beforeload", function (treeLoader, node) {
      treeLoader.baseParams.type = node.attributes.type;
      treeLoader.baseParams.query = searchText;
      treeLoader.baseParams.limit = this.limit;
      treeLoader.baseParams.start = 0;
      if (node.parentNode != undefined)
        treeLoader.baseParams.id = node.parentNode.attributes.identifier;
      if (node.attributes.type == 'TemplateNode')
        treeLoader.baseParams.id = node.attributes.identifier;
    }, this);

    var tree = new Ext.tree.TreePanel({
      title: searchText,
      enableDrag: true,
      ddGroup: 'refdataGroup',      
      animate: true,
      lines: true,
      id: 'tab_' + searchText,
      autoScroll: true,
      style: 'padding-left:5px;',
      border: false,
      closable: true,
      rootVisible: false,
      loader: treeLoader,
      root: {
        nodeType: 'async',
        // draggable: true,
        type: 'SearchNode'
      },
      containerScroll: true
    });

    //	tree.on('beforeexpandnode', this.restrictExpand, this);

    tree.on('beforeload', function (node) {
      Ext.getCmp('content-pane').getEl().mask('Loading...');
    });
    tree.on('load', function (node) {
      Ext.getCmp('content-pane').getEl().unmask();
    });
    tree.getRootNode().expand();
    tree.on('click', this.onClick, this);
    this.refClassTabPanel.add(tree).show();
  },
  onClick: function (node) {
    try {
      this.propertyPanel.setSource(node.attributes.record);
    } catch (e) {
    };
  }

});