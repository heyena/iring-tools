﻿Ext.ns('AdapterManager');
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
      activeItem: 0
    });


    this.items = [this.refClassTabPanel, this.propertyPanel];

    // super
    AdapterManager.SearchPanel.superclass.initComponent.call(this);
  },
  buildToolbar: function () {
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
                         var query = Ext.get('referencesearch').getValue();
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
  onSearch: function () {

    var treeLoader = new Ext.tree.TreeLoader({
      requestMethod: 'POST',
      url: this.searchUrl,
      baseParams: {
        type: null,
        query: searchText,
        limit: this.limit,
        start: 0
      }
    });

    this.treeLoader.on("beforeload", function (treeLoader, node) {
      treeLoader.baseParams.type = node.attributes.type;
      treeLoader.baseParams.query = Ext.get('referencesearch').getValue();
      treeLoader.baseParams.limit = this.limit;
      treeLoader.baseParams.start = 0;
    }, this);

    var tree = new Ext.tree.TreePanel({
      title: searchText,
      useArrows: true,
      animate: true,
      lines: false,
      id: 'tab_' + searchText,
      autoScroll: true,
      style: 'padding-left:5px;',
      border: false,
      closable: true,
      rootVisible: false,
      loader: treeLoader,
      root: {
        nodeType: 'async',
        qtipCfg: 'Aswini',
        draggable: false,
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